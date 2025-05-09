using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Primify.Generator;

internal static class CodeBuilder
{
    // --- Constants ---
    public const string SummaryStart = "/// <summary>";
    public const string SummaryEnd = "/// </summary>";
    public const string ParamTag = "/// <param name=\"{0}\">{1}</param>";
    public const string ReturnsTag = "/// <returns>{0}</returns>";

    // --- Indentation Constants ---
    public const string Indent = "    ";
    public const string NestedIndent = Indent + Indent;
    public const string DoubleNestedIndent = NestedIndent + Indent;

    // --- Helper Methods ---
    public static void AppendSummary(StringBuilder sb, string summary)
    {
        sb.AppendLine($"{Indent}{SummaryStart}");
        sb.AppendLine($"{Indent}/// {summary}");
        sb.AppendLine($"{Indent}{SummaryEnd}");
    }

    public static void AppendParam(StringBuilder sb, string paramName, string description)
    {
        sb.AppendLine($"{Indent}{string.Format(ParamTag, paramName, description)}");
    }

    public static void AppendReturns(StringBuilder sb, string description)
    {
        sb.AppendLine($"{Indent}{string.Format(ReturnsTag, description)}");
    }

    public static void AppendMethodSignature(StringBuilder sb, string signature)
    {
        sb.AppendLine($"{Indent}{signature}");
    }

    public static void AppendBraces(StringBuilder sb, Action innerContent, string indent)
    {
        sb.AppendLine($"{indent}{{");
        innerContent();
        sb.AppendLine($"{indent}}}");
    }

    public static void AppendNewLine(StringBuilder sb)
    {
        sb.AppendLine();
    }

    public static void AppendFactoryMethodImplementation(WrapperTypeInfo info, StringBuilder sb)
    {
        AppendSummary(sb, "Creates a new instance from a primitive value.");
        AppendMethodSignature(sb, $"public static {info.TypeName} From({info.PrimitiveTypeName} value)");
        AppendBraces(sb, () =>
        {
            sb.AppendLine($"{NestedIndent}var normalized = Normalize(value);");
            sb.AppendLine($"{NestedIndent}Validate(normalized);");
            sb.AppendLine($"{NestedIndent}return new {info.TypeName}(normalized);");
        }, NestedIndent);
        AppendNewLine(sb);
    }

    public static void AppendValidateMethodImplementation(WrapperTypeInfo info, StringBuilder sb)
    {
        AppendSummary(sb, "Provides a hook for validating the normalized primitive value before construction.");
        sb.AppendLine($"{Indent}/// Implement this partial method in your own code file to apply custom validation.");
        sb.AppendLine($"{Indent}/// Throw an exception (e.g., ArgumentException) if validation fails.");
        AppendParam(sb, "value", "The normalized primitive value.");
        AppendMethodSignature(sb, $"static partial void Validate({info.PrimitiveTypeName} value);");
        AppendNewLine(sb);
    }

    public static void AppendNormalizeMethodImplementation(WrapperTypeInfo info, StringBuilder sb)
    {
        AppendSummary(sb, "Provides a hook for normalizing the primitive value before validation and construction.");
        sb.AppendLine(
            $"{Indent}/// Implement this partial method in your own code file to apply custom normalization.");
        AppendParam(sb, "value", "The raw primitive value.");
        AppendReturns(sb, "The normalized primitive value.");
        AppendMethodSignature(sb,
            info.HasNormalizeImplementation
                ? $"private static partial {info.PrimitiveTypeName} Normalize({info.PrimitiveTypeName} value);"
                : $"static {info.PrimitiveTypeName} Normalize({info.PrimitiveTypeName} value) => value;");
        AppendNewLine(sb);
    }

    public static void AppendToStringImplementation(WrapperTypeInfo info, StringBuilder sb)
    {
        AppendSummary(sb, "Returns a string representation of the value.");
        AppendReturns(sb, "String representation of the underlying value.");
        AppendMethodSignature(sb, "public override string ToString() => Value.ToString();");
        AppendNewLine(sb);
    }

    public static void AppendImplicitExplicitConvertersImplementation(WrapperTypeInfo info, StringBuilder sb)
    {
        AppendSummary(sb, "Explicitly converts the wrapper to its primitive value.");
        AppendMethodSignature(sb,
            $"public static explicit operator {info.PrimitiveTypeName}({info.TypeName} self) => self.Value;");
        AppendNewLine(sb);
        AppendSummary(sb, "Explicitly converts a primitive value to the wrapper type.");
        AppendMethodSignature(sb,
            $"public static explicit operator {info.TypeName}({info.PrimitiveTypeName} value) => From(value);");
        AppendNewLine(sb);

        // Add these methods to each generated wrapper class
        // Special handling for DateOnly needed because it's stored as Int32 (DayNumber)
        if (info.PrimitiveTypeName == "global::System.DateOnly") // Use fully qualified name
        {
            sb.AppendLine($"{Indent}// LiteDB serialization support");
            AppendSummary(sb,
                $"Implicitly converts a <see cref=\"{info.TypeName}\"/> to a <see cref=\"LiteDB.BsonValue\"/> for LiteDB serialization.");
            AppendMethodSignature(sb,
                $"public static implicit operator LiteDB.BsonValue({info.TypeName} value) => new LiteDB.BsonValue(value.Value.DayNumber);");
            AppendSummary(sb,
                $"Implicitly converts a <see cref=\"LiteDB.BsonValue\"/> to a <see cref=\"{info.TypeName}\"/> for LiteDB deserialization.");
            AppendMethodSignature(sb,
                $"public static implicit operator {info.TypeName}(LiteDB.BsonValue bson) => From(global::System.DateOnly.FromDayNumber(bson.AsInt32));"); // Use fully qualified name
        }
        else
        {
            sb.AppendLine($"{Indent}// LiteDB serialization support");
            AppendSummary(sb,
                $"Implicitly converts a <see cref=\"{info.TypeName}\"/> to a <see cref=\"LiteDB.BsonValue\"/> for LiteDB serialization.");
            AppendMethodSignature(sb,
                $"public static implicit operator LiteDB.BsonValue({info.TypeName} value) => new LiteDB.BsonValue(value.Value);");
            AppendSummary(sb,
                $"Implicitly converts a <see cref=\"LiteDB.BsonValue\"/> to a <see cref=\"{info.TypeName}\"/> for LiteDB deserialization.");
            // Use the helper to get the correct expression for converting BsonValue to the primitive type
            var deserializationExpressionForOperator =
                GetDeserializationExpression(info,
                    "bson"); // Get the correct expression (e.g., Convert.ToDouble(bson.AsDecimal))
            AppendMethodSignature(sb,
                $"public static implicit operator {info.TypeName}(LiteDB.BsonValue bson) => From({deserializationExpressionForOperator});"); // Use the expression here
        }

        AppendNewLine(sb);
    }

    public static void AppendJsonConverterAttributes(StringBuilder sb, string typeName)
    {
        sb.AppendLine(
            $"[System.Text.Json.Serialization.JsonConverter(typeof({typeName}.SystemTextJsonConverter))]"); // Fully qualify
        sb.AppendLine($"[NewtonsoftJson.JsonConverter(typeof({typeName}.NewtonsoftJsonConverter))]");
    }

    public static void AppendSystemTextJsonConverterImplementation(WrapperTypeInfo info, StringBuilder sb)
    {
        // --- System.Text.Json Converter ---
        AppendSummary(sb, "Internal System.Text.Json converter.");
        // Fully qualify JsonConverter base type
        AppendMethodSignature(sb,
            $"internal class {info.SystemTextConverterName} : System.Text.Json.Serialization.JsonConverter<{info.TypeName}>");
        AppendBraces(sb, () =>
        {
            // Return type is non-nullable for structs, nullable for classes
            var readReturnType = info.IsValueType ? info.TypeName : $"{info.TypeName}?";
            AppendMethodSignature(sb,
                $"public override {readReturnType} Read(ref System.Text.Json.Utf8JsonReader reader, global::System.Type typeToConvert, System.Text.Json.JsonSerializerOptions options)"); // Fully qualify Type
            AppendBraces(sb, () =>
            {
                sb.AppendLine($"{NestedIndent}    if (reader.TokenType == System.Text.Json.JsonTokenType.Null)");
                AppendBraces(sb, () =>
                {
                    // Return null for classes, default (which is default struct) for structs
                    sb.AppendLine(
                        $"{NestedIndent}        return {(info.IsValueType ? $"default({info.TypeName})" : "null")};");
                }, NestedIndent);
                sb.AppendLine(
                    $"{NestedIndent}    var primitiveValue = System.Text.Json.JsonSerializer.Deserialize<{info.PrimitiveTypeName}>(ref reader, options);");
                sb.AppendLine($"{NestedIndent}    // Use factory method instead of constructor for validation");
                sb.AppendLine($"{NestedIndent}    return {info.TypeName}.From(primitiveValue!);");
            }, NestedIndent);
            AppendNewLine(sb);
            AppendMethodSignature(sb,
                $"public override void Write(System.Text.Json.Utf8JsonWriter writer, {info.TypeName} value, System.Text.Json.JsonSerializerOptions options)");
            AppendBraces(sb, () =>
            {
                // Handle null classes passed to Write (structs cannot be null here)
                if (!info.IsValueType)
                {
                    sb.AppendLine($"{NestedIndent}    if (value is null)");
                    AppendBraces(sb, () =>
                    {
                        sb.AppendLine($"{NestedIndent}        writer.WriteNullValue();");
                        sb.AppendLine($"{NestedIndent}        return;");
                    }, NestedIndent);
                }

                sb.AppendLine($"{NestedIndent}    // Use explicit conversion to get primitive");
                sb.AppendLine(
                    $"{NestedIndent}    System.Text.Json.JsonSerializer.Serialize(writer, ({info.PrimitiveTypeName})value, options);");
            }, NestedIndent);
        }, Indent);
        AppendNewLine(sb);
    }

    public static void AppendNewtonsoftJsonConverterImplementation(WrapperTypeInfo info, StringBuilder sb)
    {
        // --- Newtonsoft.Json Converter ---
        AppendSummary(sb, "Internal Newtonsoft.Json converter.");
        if (info.IsValueType)
        {
            // Struct implementation - No nullable parameter types
            AppendMethodSignature(sb,
                $"internal class {info.NewtonsoftConverterName} : NewtonsoftJson.JsonConverter<{info.TypeName}>");
            AppendBraces(sb, () =>
            {
                AppendMethodSignature(sb,
                    $"public override void WriteJson(NewtonsoftJson.JsonWriter writer, {info.TypeName} value, NewtonsoftJson.JsonSerializer serializer)");
                AppendBraces(sb, () =>
                {
                    sb.AppendLine($"{NestedIndent}    // Structs cannot be null, so no null check needed");
                    sb.AppendLine($"{NestedIndent}    // Use explicit conversion to get primitive");
                    sb.AppendLine($"{NestedIndent}    serializer.Serialize(writer, ({info.PrimitiveTypeName})value);");
                }, NestedIndent);
                AppendNewLine(sb);
                AppendMethodSignature(sb,
                    $"public override {info.TypeName} ReadJson(NewtonsoftJson.JsonReader reader, global::System.Type objectType, {info.TypeName} existingValue, bool hasExistingValue, NewtonsoftJson.JsonSerializer serializer)"); // Fully qualify Type
                AppendBraces(sb, () =>
                {
                    sb.AppendLine($"{NestedIndent}    if (reader.TokenType == NewtonsoftJson.JsonToken.Null)");
                    AppendBraces(sb, () =>
                    {
                        sb.AppendLine($"{NestedIndent}        // Return default value for struct");
                        sb.AppendLine($"{NestedIndent}        return default;");
                    }, NestedIndent);
                    sb.AppendLine(
                        $"{NestedIndent}    var primitiveValue = serializer.Deserialize<{info.PrimitiveTypeName}>(reader);");
                    sb.AppendLine($"{NestedIndent}    // Use factory method for validation");
                    sb.AppendLine($"{NestedIndent}    return {info.TypeName}.From(primitiveValue!);");
                }, NestedIndent);
            }, Indent);
        }
        else
        {
            // Class implementation - With nullable parameter types
            AppendMethodSignature(sb,
                $"internal class {info.NewtonsoftConverterName} : NewtonsoftJson.JsonConverter<{info.TypeName}>");
            AppendBraces(sb, () =>
            {
                AppendMethodSignature(sb,
                    $"public override void WriteJson(NewtonsoftJson.JsonWriter writer, {info.TypeName}? value, NewtonsoftJson.JsonSerializer serializer)");
                AppendBraces(sb, () =>
                {
                    sb.AppendLine($"{NestedIndent}    if (value is null)");
                    AppendBraces(sb, () =>
                    {
                        sb.AppendLine($"{NestedIndent}        writer.WriteNull();");
                        sb.AppendLine($"{NestedIndent}        return;");
                    }, NestedIndent);
                    sb.AppendLine($"{NestedIndent}    // Use explicit conversion to get primitive");
                    sb.AppendLine($"{NestedIndent}    serializer.Serialize(writer, ({info.PrimitiveTypeName})value);");
                }, NestedIndent);
                AppendNewLine(sb);
                AppendMethodSignature(sb,
                    $"public override {info.TypeName}? ReadJson(NewtonsoftJson.JsonReader reader, global::System.Type objectType, {info.TypeName}? existingValue, bool hasExistingValue, NewtonsoftJson.JsonSerializer serializer)"); // Fully qualify Type
                AppendBraces(sb, () =>
                {
                    sb.AppendLine($"{NestedIndent}    if (reader.TokenType == NewtonsoftJson.JsonToken.Null)");
                    AppendBraces(sb, () => { sb.AppendLine($"{NestedIndent}        return null;"); }, NestedIndent);
                    sb.AppendLine(
                        $"{NestedIndent}    var primitiveValue = serializer.Deserialize<{info.PrimitiveTypeName}>(reader);");
                    sb.AppendLine($"{NestedIndent}    // Use factory method for validation");
                    sb.AppendLine($"{NestedIndent}    return {info.TypeName}.From(primitiveValue!);");
                }, NestedIndent);
            }, Indent);
        }

        AppendNewLine(sb);
    }

    // Generates the central registration class ONLY for BSON mapping
    // In your generator's GenerateLiteDbRegistrationImplementation method
    public static string GenerateLiteDbRegistrationImplementation(List<WrapperTypeInfo> types)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System;");
        sb.AppendLine("using LiteDB;");
        sb.AppendLine("using System.Runtime.CompilerServices;");
        sb.AppendLine();

        // Add using statements for all unique namespaces
        foreach (var ns in types.Select(t => t.Namespace).Where(n => !string.IsNullOrEmpty(n)).Distinct())
        {
            sb.AppendLine($"using {ns};");
        }

        // Add global:: prefix for types in the global namespace if any exist
        if (types.Any(t => string.IsNullOrEmpty(t.Namespace)))
        {
            // This might not be strictly necessary depending on how types are resolved,
            // but can prevent ambiguity if types with the same name exist elsewhere.
            // Consider if needed based on potential conflicts.
        }

        sb.AppendLine();
        sb.AppendLine("namespace Primify.Generated");
        sb.AppendLine("{");
        sb.AppendLine("    public static class PrimifyLiteDbRegistration");
        sb.AppendLine("    {");
        sb.AppendLine("        private static readonly object _lock = new object();");
        sb.AppendLine("        private static bool _initialized = false;");
        sb.AppendLine();
        sb.AppendLine("        static PrimifyLiteDbRegistration()");
        sb.AppendLine("        {");
        sb.AppendLine("            // Auto-register when the class is first accessed");
        sb.AppendLine("            Initialize();");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        [ModuleInitializer]");
        sb.AppendLine("        internal static void Initialize()");
        sb.AppendLine("        {");
        sb.AppendLine("            Register(BsonMapper.Global);");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        public static void Register(BsonMapper mapper)");
        sb.AppendLine("        {");
        sb.AppendLine("            lock (_lock)");
        sb.AppendLine("            {");
        sb.AppendLine("                if (_initialized && mapper == BsonMapper.Global) return;");
        sb.AppendLine();
        foreach (var info in types)
        {
            // Use fully qualified type name for RegisterType<T>
            sb.AppendLine($"                mapper.RegisterType<{info.FullTypeName}>(");
            // Handle DateOnly specially
            if (info.PrimitiveTypeName == "global::System.DateOnly") // Use fully qualified name for check
            {
                sb.AppendLine($"                    serialize: value => new BsonValue(value.Value.DayNumber),");
                // Use fully qualified name for From method call
                sb.AppendLine(
                    $"                    deserialize: bson => {info.FullTypeName}.From(global::System.DateOnly.FromDayNumber(bson.AsInt32)));");
            }
            else
            {
                // Use the helper to get the correct expression for converting BsonValue to the primitive type
                var deserializationExpressionForRegistration = GetDeserializationExpression(info, "bson");
                // Use fully qualified name for From method call
                sb.AppendLine($"                    serialize: value => new BsonValue(value.Value),");
                sb.AppendLine(
                    $"                    deserialize: bson => {info.FullTypeName}.From({deserializationExpressionForRegistration}));");
            }
        }

        sb.AppendLine();
        sb.AppendLine("                if (mapper == BsonMapper.Global) _initialized = true;");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();

        return sb.ToString();
    }

    public static void AppendTypeSummary(StringBuilder sb, string summary)
    {
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// {summary}");
        sb.AppendLine("/// </summary>");
    }

    public static void EnsureBlankLine(StringBuilder sb)
    {
        if (sb.Length == 0) return;
        var last = sb.ToString().TrimEnd('\n').Split('\n').LastOrDefault();
        if (!string.IsNullOrWhiteSpace(last)) sb.AppendLine();
    }

    public static void AppendPredefinedInstancesImplementation(WrapperTypeInfo info, StringBuilder sb)
    {
        if (info.PredefinedInstances == null || info.PredefinedInstances.Count == 0)
            return;
        sb.AppendLine("#region Predefined Instances");
        sb.AppendLine();
        sb.AppendLine("#pragma warning disable IDE1006");
        foreach (var instance in info.PredefinedInstances)
        {
            var fieldName = $"_{char.ToLowerInvariant(instance.PropertyName[0])}{instance.PropertyName.Substring(1)}";
            sb.AppendLine($"{Indent}private static readonly {info.TypeName} {fieldName};");
        }

        sb.AppendLine("#pragma warning restore IDE1006");
        sb.AppendLine();
        sb.AppendLine($"{Indent}static {info.TypeName}()");
        sb.AppendLine($"{Indent}{{");
        foreach (var instance in info.PredefinedInstances)
        {
            var fieldName = $"_{char.ToLowerInvariant(instance.PropertyName[0])}{instance.PropertyName.Substring(1)}";
            var formattedValue = FormatValue(info.PrimitiveTypeSymbol, instance.Value);
            sb.AppendLine($"{NestedIndent}{fieldName} = new {info.TypeName}({formattedValue});");
        }

        sb.AppendLine();
        sb.AppendLine($"{NestedIndent}// Auto-register with LiteDB if available");
        sb.AppendLine(
            $"{NestedIndent}#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER // Check if LiteDB might be present");
        sb.AppendLine(
            $"{NestedIndent}try {{ LiteDbMapper.RegisterType(); }} catch {{ /* LiteDB not available or registration failed, ignore */ }}");
        sb.AppendLine($"{NestedIndent}#endif");
        sb.AppendLine($"{Indent}}}");
        sb.AppendLine();
        foreach (var instance in info.PredefinedInstances)
        {
            var fieldName = $"_{char.ToLowerInvariant(instance.PropertyName[0])}{instance.PropertyName.Substring(1)}";
            var propertyName = instance.PropertyName;
            sb.AppendLine($"{Indent}/// <summary>");
            sb.AppendLine($"{Indent}/// Gets the predefined {info.TypeName} instance for '{propertyName}'.");
            sb.AppendLine($"{Indent}/// </summary>");
            sb.AppendLine($"{Indent}[System.Text.Json.Serialization.JsonIgnore]");
            sb.AppendLine($"{Indent}[NewtonsoftJson.JsonIgnore]");
            sb.AppendLine($"{Indent}[LiteDB.BsonIgnore]");
            sb.AppendLine($"{Indent}public static partial {info.TypeName} {propertyName} => {fieldName};");
            sb.AppendLine();
        }

        sb.AppendLine("#endregion");
        sb.AppendLine();
    }

    public static void AppendLiteDbRegistration(WrapperTypeInfo info, StringBuilder sb)
    {
        // Add LiteDB registration as a nested class
        sb.AppendLine($"{NestedIndent}/// <summary>Provides LiteDB serialization support.</summary>");
        sb.AppendLine($"{NestedIndent}internal static class LiteDbMapper");
        sb.AppendLine($"{NestedIndent}{{");
        sb.AppendLine($"{DoubleNestedIndent}private static bool _registered = false;"); // Track registration per type
        sb.AppendLine($"{DoubleNestedIndent}private static readonly object _lock = new object();"); // Lock per type
        sb.AppendLine();
        sb.AppendLine($"{DoubleNestedIndent}static LiteDbMapper()");
        sb.AppendLine($"{DoubleNestedIndent}{{");
        sb.AppendLine($"{DoubleNestedIndent}    // Auto-register this type when the mapper is first used");
        sb.AppendLine($"{DoubleNestedIndent}    RegisterType();");
        sb.AppendLine($"{DoubleNestedIndent}}}");
        sb.AppendLine();
        sb.AppendLine($"{DoubleNestedIndent}public static void RegisterType()");
        sb.AppendLine($"{DoubleNestedIndent}{{");
        sb.AppendLine(
            $"{DoubleNestedIndent}    RegisterType(LiteDB.BsonMapper.Global);"); // Register with global mapper by default
        sb.AppendLine($"{DoubleNestedIndent}}}");
        sb.AppendLine();
        sb.AppendLine(
            $"{DoubleNestedIndent}public static void RegisterType(LiteDB.BsonMapper mapper)"); // Allow specific mapper
        sb.AppendLine($"{DoubleNestedIndent}{{");
        sb.AppendLine($"{DoubleNestedIndent}    lock(_lock)"); // Use the lock
        sb.AppendLine($"{DoubleNestedIndent}    {{");
        sb.AppendLine(
            $"{DoubleNestedIndent}        if (_registered && mapper == LiteDB.BsonMapper.Global) return; // Avoid re-registering globally");
        sb.AppendLine();
        sb.AppendLine(
            $"{DoubleNestedIndent}        mapper.RegisterType<{info.FullTypeName}>("); // Use fully qualified name

        // Special handling for DateOnly
        if (info.PrimitiveTypeName == "global::System.DateOnly") // Use fully qualified name
        {
            sb.AppendLine($"{DoubleNestedIndent}            serialize: value => new BsonValue(value.Value.DayNumber),");
            sb.AppendLine(
                $"{DoubleNestedIndent}            deserialize: bson => {info.FullTypeName}.From(global::System.DateOnly.FromDayNumber(bson.AsInt32))"); // Use fully qualified name
        }
        else
        {
            // Use the helper to get the correct expression for converting BsonValue to the primitive type
            var deserializationExpressionForMapper = GetDeserializationExpression(info, "bson");
            sb.AppendLine($"{DoubleNestedIndent}            serialize: value => new BsonValue(value.Value),");
            sb.AppendLine(
                $"{DoubleNestedIndent}            deserialize: bson => {info.FullTypeName}.From({deserializationExpressionForMapper})"); // Use fully qualified name and helper
        }

        sb.AppendLine($"{DoubleNestedIndent}        );");
        sb.AppendLine();
        sb.AppendLine(
            $"{DoubleNestedIndent}        if (mapper == LiteDB.BsonMapper.Global) _registered = true; // Mark as registered globally");
        sb.AppendLine($"{DoubleNestedIndent}    }}"); // End lock
        sb.AppendLine($"{DoubleNestedIndent}}}");
        sb.AppendLine($"{NestedIndent}}}");
    }

    public static void AppendValuePropertyAndConstructor(WrapperTypeInfo info, StringBuilder sb)
    {
        // Property
        sb.AppendLine($"{Indent}/// <summary>Gets the underlying primitive value.</summary>");
        sb.AppendLine($"{Indent}public {info.PrimitiveTypeName} Value {{ get; }}");
        sb.AppendLine();
        // Private constructor
        sb.AppendLine($"{Indent}private {info.TypeName}({info.PrimitiveTypeName} value)");
        sb.AppendLine($"{Indent}{{");
        sb.AppendLine($"{NestedIndent}Value = value;");
        sb.AppendLine($"{Indent}}}");
        sb.AppendLine();
    }

    private static string GetDeserializationExpression(WrapperTypeInfo info, string bsonVariable)
    {
        var specialType = info.PrimitiveTypeSymbol.SpecialType;
        var primitiveTypeName = info.PrimitiveTypeName;

        switch (specialType)
        {
            case SpecialType.System_Double:
                return $"global::System.Convert.ToDouble({bsonVariable}.AsDecimal)";
            case SpecialType.System_Single:
                return $"global::System.Convert.ToSingle({bsonVariable}.AsDecimal)";
            default:
            {
                switch (primitiveTypeName)
                {
                    case "global::System.TimeOnly":
                        return $"global::System.TimeOnly.FromTimeSpan(global::System.TimeSpan.FromTicks({bsonVariable}.AsInt64))";
                    case "global::System.TimeSpan":
                        return $"global::System.TimeSpan.FromTicks({bsonVariable}.AsInt64)";
                    case "global::System.DateTimeOffset":
                        return $"{bsonVariable}.AsDateTime";
                    default:
                    {
                        var accessor = GetBsonAccessor(info.PrimitiveTypeSymbol);
                        return $"{bsonVariable}.{accessor}";
                    }
                }
            }
        }
    }

    private static string GetBsonAccessor(ITypeSymbol primitiveTypeSymbol)
    {
        var fullTypeName = primitiveTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        var typeAccessors = new Dictionary<string, string>
        {
            ["global::System.Guid"] = "AsGuid",
            ["global::System.DateTime"] = "AsDateTime",
            ["global::System.DateTimeOffset"] = "AsDateTime",
            ["global::System.TimeSpan"] = "AsInt64",
            ["global::System.DateOnly"] = "AsInt32",
            ["global::System.TimeOnly"] = "AsInt64"
        };

        if (typeAccessors.TryGetValue(fullTypeName, out var accessor))
            return accessor;

        var specialTypeAccessors = new Dictionary<SpecialType, string>
        {
            [SpecialType.System_Boolean] = "AsBoolean",
            [SpecialType.System_Char] = "AsString",
            [SpecialType.System_SByte] = "AsInt32",
            [SpecialType.System_Byte] = "AsInt32",
            [SpecialType.System_Int16] = "AsInt32",
            [SpecialType.System_UInt16] = "AsInt32",
            [SpecialType.System_Int32] = "AsInt32",
            [SpecialType.System_UInt32] = "AsInt32",
            [SpecialType.System_Int64] = "AsInt64",
            [SpecialType.System_UInt64] = "AsInt64",
            [SpecialType.System_Single] = "AsDecimal",
            [SpecialType.System_Double] = "AsDecimal",
            [SpecialType.System_Decimal] = "AsDecimal",
            [SpecialType.System_String] = "AsString"
        };

        if (specialTypeAccessors.TryGetValue(primitiveTypeSymbol.SpecialType, out accessor))
            return accessor;

        return primitiveTypeSymbol.TypeKind == TypeKind.Enum ? "AsInt32" : "RawValue";
    }

    private static string FormatValue(ITypeSymbol typeSymbol, object value)
    {
        if (value == null!)
            return "null";

        var fullTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        var typeFormatters = new Dictionary<string, Func<object, string>>
        {
            ["global::System.Guid"] = v => v switch
            {
                Guid guid => $"new global::System.Guid(\"{guid}\")",
                string guidStr when Guid.TryParse(guidStr, out var parsedGuid) =>
                    $"new global::System.Guid(\"{parsedGuid}\")",
                _ => "new global::System.Guid()"
            },
            ["global::System.DateTime"] = v => v switch
            {
                DateTime dt => $"new global::System.DateTime({dt.Ticks}L, global::System.DateTimeKind.{dt.Kind})",
                _ => "default(global::System.DateTime)"
            },
            ["global::System.DateTimeOffset"] = v => v switch
            {
                DateTimeOffset dto =>
                    $"new global::System.DateTimeOffset({dto.Ticks}L, global::System.TimeSpan.FromMinutes({dto.Offset.TotalMinutes}))",
                _ => "default(global::System.DateTimeOffset)"
            },
            ["global::System.TimeSpan"] = v => v switch
            {
                TimeSpan ts => $"global::System.TimeSpan.FromTicks({ts.Ticks}L)",
                long ticks => $"global::System.TimeSpan.FromTicks({ticks}L)",
                _ => "global::System.TimeSpan.Zero"
            },
            ["global::System.DateOnly"] = v => v switch
            {
                int dayNumber => $"global::System.DateOnly.FromDayNumber({dayNumber})",
                string dateStr => $"global::System.DateOnly.Parse(\"{dateStr}\")", // Assume runtime handles parsing
                _ => "default(global::System.DateOnly)"
            },
            ["global::System.TimeOnly"] = v => v switch
            {
                long ticks => $"global::System.TimeOnly.FromTimeSpan(global::System.TimeSpan.FromTicks({ticks}L))",
                string timeStr => $"global::System.TimeOnly.Parse(\"{timeStr}\")", // Assume runtime handles parsing
                _ => "default(global::System.TimeOnly)"
            }
        };

        if (typeFormatters.TryGetValue(fullTypeName, out var formatter))
            return formatter(value);

        var specialTypeFormatters = new Dictionary<SpecialType, Func<object, string>>
        {
            [SpecialType.System_String] = v => $"\"{v}\"",
            [SpecialType.System_Boolean] = v => v.ToString().ToLowerInvariant(),
            [SpecialType.System_Char] = v => $"'{v}'",
            [SpecialType.System_Int64] = v => $"{v}L",
            [SpecialType.System_UInt64] = v => $"{v}UL",
            [SpecialType.System_Single] = v => $"{v}F",
            [SpecialType.System_Double] = v => $"{v}D",
            [SpecialType.System_Decimal] = v => $"{v}M"
        };

        if (specialTypeFormatters.TryGetValue(typeSymbol.SpecialType, out formatter))
            return formatter(value);

        return value.ToString();
    }

    // Helper to append the auto-generated header
    public static void AppendAutoGeneratedHeader(StringBuilder sb)
    {
        sb.AppendLine("// <auto-generated/>");
    }

    // Helper to append nullable enable directive
    public static void AppendNullableEnable(StringBuilder sb)
    {
        sb.AppendLine("#nullable enable");
    }

    // Helper to append using statements
    public static void AppendUsingStatements(StringBuilder sb, params string[] usings)
    {
        foreach (var u in usings)
        {
            sb.AppendLine($"using {u};");
        }
    }

    public static void AppendUsingStatement(StringBuilder sb, string usingNamespace)
    {
        sb.AppendLine($"using {usingNamespace};");
    }

    // Helper for namespace
    public static void AppendNamespace(StringBuilder sb, string ns)
    {
        sb.AppendLine($"namespace {ns}");
        sb.AppendLine("{");
    }

    // Helper for type declaration
    public static void AppendTypeDeclaration(StringBuilder sb, string typeKindKeyword, string typeName,
        string sealedModifier)
    {
        var typeDecl = typeKindKeyword.Contains("record")
            ? $"{typeKindKeyword.Replace("record", "partial record")}"
            : $"partial {typeKindKeyword}";
        sb.AppendLine($"{Indent}{sealedModifier}{typeDecl} {typeName}");
        sb.AppendLine($"{Indent}{{");
    }

    // Helper to close type
    public static void AppendCloseType(StringBuilder sb)
    {
        sb.AppendLine($"{Indent}}}");
    }

    // Helper to close namespace
    public static void AppendCloseNamespace(StringBuilder sb)
    {
        sb.AppendLine("}");
    }
}
