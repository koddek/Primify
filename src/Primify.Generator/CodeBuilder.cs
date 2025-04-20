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

    public static void AppendBraces(StringBuilder sb, Action innerContent)
    {
        sb.AppendLine($"{Indent}{{");
        innerContent();
        sb.AppendLine($"{Indent}}}");
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
        });
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
        sb.AppendLine($"{Indent}/// Implement this partial method in your own code file to apply custom normalization.");
        AppendParam(sb, "value", "The raw primitive value.");
        AppendReturns(sb, "The normalized primitive value.");
        if (info.HasNormalizeImplementation)
            AppendMethodSignature(sb, $"private static partial {info.PrimitiveTypeName} Normalize({info.PrimitiveTypeName} value);");
        else
            AppendMethodSignature(sb, $"static {info.PrimitiveTypeName} Normalize({info.PrimitiveTypeName} value) => value;");
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
        AppendMethodSignature(sb, $"public static explicit operator {info.PrimitiveTypeName}({info.TypeName} self) => self.Value;");
        AppendNewLine(sb);
        AppendSummary(sb, "Explicitly converts a primitive value to the wrapper type.");
        AppendMethodSignature(sb, $"public static explicit operator {info.TypeName}({info.PrimitiveTypeName} value) => From(value);");
        AppendNewLine(sb);

        // Add these methods to each generated wrapper class
        if (info.PrimitiveTypeName.Contains("DateOnly"))
        {
            sb.AppendLine($"{Indent}// LiteDB serialization support");
            AppendMethodSignature(sb, $"public static implicit operator BsonValue({info.TypeName} value) => new BsonValue(value.Value.DayNumber);");
            AppendMethodSignature(sb, $"public static implicit operator {info.TypeName}(BsonValue bson) => From(DateOnly.FromDayNumber(bson.AsInt32));");
        }
        else
        {
            sb.AppendLine($"{Indent}// LiteDB serialization support");
            AppendMethodSignature(sb, $"public static implicit operator BsonValue({info.TypeName} value) => new BsonValue(value.Value);");
            AppendMethodSignature(sb, $"public static implicit operator {info.TypeName}(BsonValue bson) => From(bson.{GetBsonAccessor(info.PrimitiveTypeSymbol)});");
        }
    }

    public static void AppendJsonConverterAttributes(StringBuilder sb, string typeName)
    {
        sb.AppendLine($"[JsonConverter(typeof({typeName}.SystemTextJsonConverter))]");
        sb.AppendLine($"[NewtonsoftJson.JsonConverter(typeof({typeName}.NewtonsoftJsonConverter))]");
    }

    public static void AppendJsonConverterAttributesPointingToConverters(WrapperTypeInfo info, StringBuilder sb)
    {
        AppendSummary(sb, $"Represents a value wrapper for <see cref=\"{info.PrimitiveTypeName}\"/>.");
        sb.AppendLine($"{Indent}/// Generated by ValueWrapperGenerator.");
    }

    public static void AppendSystemTextJsonConverterImplementation(WrapperTypeInfo info, StringBuilder sb)
    {
        // --- System.Text.Json Converter ---
        AppendSummary(sb, "Internal System.Text.Json converter.");
        AppendMethodSignature(sb, $"internal class {info.SystemTextConverterName} : JsonConverter<{info.TypeName}>");
        AppendBraces(sb, () =>
        {
            // Return type is non-nullable for structs, nullable for classes
            var readReturnType = info.IsValueType ? info.TypeName : $"{info.TypeName}?";
            AppendMethodSignature(sb, $"public override {readReturnType} Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)");
            AppendBraces(sb, () =>
            {
                sb.AppendLine($"{NestedIndent}    if (reader.TokenType == System.Text.Json.JsonTokenType.Null)");
                AppendBraces(sb, () =>
                {
                    // Return null for classes, default (which is default struct) for structs
                    sb.AppendLine($"{NestedIndent}        return {(info.IsValueType ? $"default({info.TypeName})" : "null")};");
                });
                sb.AppendLine($"{NestedIndent}    var primitiveValue = System.Text.Json.JsonSerializer.Deserialize<{info.PrimitiveTypeName}>(ref reader, options);");
                sb.AppendLine($"{NestedIndent}    // Use factory method instead of constructor for validation");
                sb.AppendLine($"{NestedIndent}    return {info.TypeName}.From(primitiveValue!);");
            });
            AppendNewLine(sb);
            AppendMethodSignature(sb, $"public override void Write(System.Text.Json.Utf8JsonWriter writer, {info.TypeName} value, System.Text.Json.JsonSerializerOptions options)");
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
                    });
                }

                sb.AppendLine($"{NestedIndent}    // Use implicit conversion to get primitive");
                sb.AppendLine($"{NestedIndent}    System.Text.Json.JsonSerializer.Serialize(writer, ({info.PrimitiveTypeName})value, options);");
            });
        });
        AppendNewLine(sb);
    }

    public static void AppendNewtonsoftJsonConverterImplementation(WrapperTypeInfo info, StringBuilder sb)
    {
        // --- Newtonsoft.Json Converter ---
        AppendSummary(sb, "Internal Newtonsoft.Json converter.");
        if (info.IsValueType)
        {
            // Struct implementation - No nullable parameter types
            AppendMethodSignature(sb, $"internal class {info.NewtonsoftConverterName} : NewtonsoftJson.JsonConverter<{info.TypeName}>");
            AppendBraces(sb, () =>
            {
                AppendMethodSignature(sb, $"public override void WriteJson(NewtonsoftJson.JsonWriter writer, {info.TypeName} value, NewtonsoftJson.JsonSerializer serializer)");
                AppendBraces(sb, () =>
                {
                    sb.AppendLine($"{NestedIndent}    // Structs cannot be null, so no null check needed");
                    sb.AppendLine($"{NestedIndent}    // Use implicit conversion to get primitive");
                    sb.AppendLine($"{NestedIndent}    serializer.Serialize(writer, ({info.PrimitiveTypeName})value);");
                });
                AppendNewLine(sb);
                AppendMethodSignature(sb, $"public override {info.TypeName} ReadJson(NewtonsoftJson.JsonReader reader, Type objectType, {info.TypeName} existingValue, bool hasExistingValue, NewtonsoftJson.JsonSerializer serializer)");
                AppendBraces(sb, () =>
                {
                    sb.AppendLine($"{NestedIndent}    if (reader.TokenType == NewtonsoftJson.JsonToken.Null)");
                    AppendBraces(sb, () =>
                    {
                        sb.AppendLine($"{NestedIndent}        // Return default value for struct");
                        sb.AppendLine($"{NestedIndent}        return default;");
                    });
                    sb.AppendLine($"{NestedIndent}    var primitiveValue = serializer.Deserialize<{info.PrimitiveTypeName}>(reader);");
                    sb.AppendLine($"{NestedIndent}    // Use factory method for validation");
                    sb.AppendLine($"{NestedIndent}    return {info.TypeName}.From(primitiveValue!);");
                });
            });
        }
        else
        {
            // Class implementation - With nullable parameter types
            AppendMethodSignature(sb, $"internal class {info.NewtonsoftConverterName} : NewtonsoftJson.JsonConverter<{info.TypeName}>");
            AppendBraces(sb, () =>
            {
                AppendMethodSignature(sb, $"public override void WriteJson(NewtonsoftJson.JsonWriter writer, {info.TypeName}? value, NewtonsoftJson.JsonSerializer serializer)");
                AppendBraces(sb, () =>
                {
                    sb.AppendLine($"{NestedIndent}    if (value is null)");
                    AppendBraces(sb, () =>
                    {
                        sb.AppendLine($"{NestedIndent}        writer.WriteNull();");
                        sb.AppendLine($"{NestedIndent}        return;");
                    });
                    sb.AppendLine($"{NestedIndent}    // Use implicit conversion to get primitive");
                    sb.AppendLine($"{NestedIndent}    serializer.Serialize(writer, ({info.PrimitiveTypeName})value);");
                });
                AppendNewLine(sb);
                AppendMethodSignature(sb, $"public override {info.TypeName}? ReadJson(NewtonsoftJson.JsonReader reader, Type objectType, {info.TypeName}? existingValue, bool hasExistingValue, NewtonsoftJson.JsonSerializer serializer)");
                AppendBraces(sb, () =>
                {
                    sb.AppendLine($"{NestedIndent}    if (reader.TokenType == NewtonsoftJson.JsonToken.Null)");
                    AppendBraces(sb, () =>
                    {
                        sb.AppendLine($"{NestedIndent}        return null;");
                    });
                    sb.AppendLine($"{NestedIndent}    var primitiveValue = serializer.Deserialize<{info.PrimitiveTypeName}>(reader);");
                    sb.AppendLine($"{NestedIndent}    // Use factory method for validation");
                    sb.AppendLine($"{NestedIndent}    return {info.TypeName}.From(primitiveValue!);");
                });
            });
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

        foreach (var ns in types.Select(t => t.Namespace).Where(n => !string.IsNullOrEmpty(n)).Distinct())
        {
            sb.AppendLine($"using {ns};");
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
            sb.AppendLine($"                mapper.RegisterType<{info.FullTypeName}>(");
            // Handle DateOnly specially
            if (info.PrimitiveTypeName.Contains("DateOnly"))
            {
                sb.AppendLine($"                    serialize: value => new BsonValue(value.Value.DayNumber),");
                sb.AppendLine($"                    deserialize: bson => {info.FullTypeName}.From(DateOnly.FromDayNumber(bson.AsInt32)));");
            }
            else
            {
                sb.AppendLine($"                    serialize: value => new BsonValue(value.Value),");
                sb.AppendLine($"                    deserialize: bson => {info.FullTypeName}.From(bson.{GetBsonAccessor(info.PrimitiveTypeSymbol)}));");
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

    // Helper to get the LiteDB BsonValue accessor
    private static string GetBsonAccessor(ITypeSymbol primitiveTypeSymbol)
    {
        // Check specific system types first by full name
        var fullTypeName = primitiveTypeSymbol.ToDisplayString();
        switch (fullTypeName)
        {
            case "System.Guid": return "AsGuid";
            case "System.DateTime": return "AsDateTime";
            case "System.DateTimeOffset": return "AsDateTime"; // Store as DateTime
            case "System.TimeSpan": return "AsInt64"; // Store as ticks
            case "System.DateOnly": return "AsInt32"; // Store as day number
            case "System.TimeOnly": return "AsInt64"; // Store as ticks since midnight
        }

        // Then check special types
        switch (primitiveTypeSymbol.SpecialType)
        {
            case SpecialType.System_Boolean: return "AsBoolean";
            case SpecialType.System_Char: return "AsString";
            case SpecialType.System_SByte:
            case SpecialType.System_Byte:
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_Int32:
            case SpecialType.System_UInt32:
                return "AsInt32";
            case SpecialType.System_Int64: return "AsInt64";
            case SpecialType.System_UInt64:
            case SpecialType.System_Single:
            case SpecialType.System_Double:
            case SpecialType.System_Decimal:
                return "AsDecimal";
            case SpecialType.System_String: return "AsString";
            default:
                if (primitiveTypeSymbol.TypeKind == TypeKind.Enum)
                    return "AsInt32";
                return "RawValue"; // Fallback for unsupported types
        }
    }

    public static void AppendPredefinedInstancesImplementation(WrapperTypeInfo info, StringBuilder sb)
    {
        if (info.PredefinedInstances == null || info.PredefinedInstances.Count == 0)
            return;

        sb.AppendLine($"{Indent}#region Predefined Instances");
        sb.AppendLine();

        sb.AppendLine("#pragma warning disable IDE1006");
        // Generate the private static readonly fields
        foreach (var instance in info.PredefinedInstances)
        {
            var fieldName = $"_{char.ToLowerInvariant(instance.PropertyName[0])}{instance.PropertyName.Substring(1)}";
            sb.AppendLine($"{Indent}private static readonly {info.TypeName} {fieldName};");
        }

        sb.AppendLine("#pragma warning restore IDE1006");
        sb.AppendLine();

        // Generate the static constructor to initialize the fields
        sb.AppendLine($"{Indent}static {info.TypeName}()");
        sb.AppendLine($"{Indent}{{");
        foreach (var instance in info.PredefinedInstances)
        {
            var fieldName = $"_{char.ToLowerInvariant(instance.PropertyName[0])}{instance.PropertyName.Substring(1)}";
            var formattedValue = FormatValue(info.PrimitiveTypeSymbol, instance.Value);
            sb.AppendLine($"{NestedIndent}{fieldName} = new {info.TypeName}({formattedValue});");
        }

        sb.AppendLine();
        sb.AppendLine($"{NestedIndent}    // Auto-register with LiteDB");
        sb.AppendLine($"{NestedIndent}    LiteDbMapper.RegisterType();");
        sb.AppendLine($"{Indent}}}");
        sb.AppendLine();

        // Generate readonly properties for each predefined instance
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

        sb.AppendLine($"{Indent}#endregion");
        sb.AppendLine();
    }

    public static void AppendLiteDbRegistration(WrapperTypeInfo info, StringBuilder sb)
    {
        // Add LiteDB registration as a nested class
        sb.AppendLine($"{NestedIndent}/// <summary>Provides LiteDB serialization support.</summary>");
        sb.AppendLine($"{NestedIndent}internal static class LiteDbMapper");
        sb.AppendLine($"{NestedIndent}{{");
        sb.AppendLine($"{DoubleNestedIndent}static LiteDbMapper()");
        sb.AppendLine($"{DoubleNestedIndent}{{");
        sb.AppendLine($"{DoubleNestedIndent}    // Auto-register this type when the mapper is first used");
        sb.AppendLine($"{DoubleNestedIndent}    RegisterType();");
        sb.AppendLine($"{DoubleNestedIndent}}}");
        sb.AppendLine();
        sb.AppendLine($"{DoubleNestedIndent}public static void RegisterType()");
        sb.AppendLine($"{DoubleNestedIndent}{{");
        sb.AppendLine($"{DoubleNestedIndent}    LiteDB.BsonMapper.Global.RegisterType<{info.TypeName}>(");

        // Special handling for DateOnly
        if (info.PrimitiveTypeName.Contains("DateOnly"))
        {
            sb.AppendLine($"{DoubleNestedIndent}        serialize: value => new BsonValue(value.Value.DayNumber),");
            sb.AppendLine($"{DoubleNestedIndent}        deserialize: bson => {info.TypeName}.From(System.DateOnly.FromDayNumber(bson.AsInt32))");
        }
        else
        {
            sb.AppendLine($"{DoubleNestedIndent}        serialize: value => new BsonValue(value.Value),");
            sb.AppendLine($"{DoubleNestedIndent}        deserialize: bson => {info.TypeName}.From(bson.{GetBsonAccessor(info.PrimitiveTypeSymbol)})");
        }

        sb.AppendLine($"{DoubleNestedIndent}    );");
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

    private static string FormatValue(ITypeSymbol typeSymbol, object value)
    {
        if (value == null)
            return "null";

        // Handle specific system types first by full name
        var fullTypeName = typeSymbol.ToDisplayString();
        switch (fullTypeName)
        {
            case "System.Guid":
                // Check if value is already a Guid or if it's a string that can be parsed as a Guid
                if (value is Guid guid)
                    return $"new System.Guid(\"{guid}\")";
                if (value is string guidStr)
                    return $"new System.Guid(\"{guidStr}\")";
                return $"new System.Guid({value})";
            case "System.DateTime":
                if (value is DateTime dt)
                    return $"new System.DateTime({dt.Ticks}L, System.DateTimeKind.{dt.Kind})";
                return $"new System.DateTime({value})";
            case "System.DateTimeOffset":
                if (value is DateTimeOffset dto)
                    return
                        $"new System.DateTimeOffset({dto.Ticks}L, System.TimeSpan.FromMinutes({dto.Offset.TotalMinutes}))";
                return $"new System.DateTimeOffset({value})";
            case "System.TimeSpan":
                if (value is TimeSpan ts)
                    return $"System.TimeSpan.FromTicks({ts.Ticks}L)";
                return $"System.TimeSpan.FromTicks({value}L)";
            case "System.DateOnly":
                // Convert to appropriate constructor format, assuming stored as integer or via "new DateOnly(year, month, day)"
                return $"System.DateOnly.FromDayNumber({value})";
            case "System.TimeOnly":
                // Convert to appropriate constructor format, assuming stored as ticks since midnight
                return $"System.TimeOnly.FromTimeSpan(System.TimeSpan.FromTicks({value}L))";
        }

        // Then check special types
        return typeSymbol.SpecialType switch
        {
            SpecialType.System_String => $"\"{value}\"",
            SpecialType.System_Boolean => value.ToString().ToLowerInvariant(),
            SpecialType.System_Char => $"'{value}'",
            _ => value.ToString()
        };
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
    public static void AppendTypeDeclaration(StringBuilder sb, string typeKindKeyword, string typeName, string sealedModifier)
    {
        var typeDecl = typeKindKeyword.Contains("record")
            ? $"{typeKindKeyword.Replace("record", "partial record")}" : $"partial {typeKindKeyword}";
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
