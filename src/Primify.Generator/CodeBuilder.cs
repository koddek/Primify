using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Primify.Generator;

internal static class CodeBuilder
{
    public static void AppendFactoryMethodImplementation(WrapperTypeInfo info, StringBuilder sb, string indent1,
        string indent2
    )
    {
        // --- Factory Method ---
        sb.AppendLine($"{indent1}/// <summary>Creates a new instance from a primitive value.</summary>");
        sb.AppendLine($"{indent1}public static {info.TypeName} From({info.PrimitiveTypeName} value)");
        sb.AppendLine($"{indent1}{{");
        sb.AppendLine($"{indent2}var normalized = Normalize(value);");
        sb.AppendLine($"{indent2}Validate(normalized);");
        sb.AppendLine($"{indent2}return new {info.TypeName}(normalized);");
        sb.AppendLine($"{indent1}}}");
        sb.AppendLine();
    }

    public static void AppendValidateMethodImplementation(WrapperTypeInfo info, StringBuilder sb, in string indent1)
    {
        var summaryDocStart = $"{indent1}/// <summary>";
        var summaryDocEnd = $"{indent1}/// </summary>";

        // --- Partial Validate Implementation ---
        sb.AppendLine(summaryDocStart);
        sb.AppendLine(
            $"{indent1}/// Provides a hook for validating the normalized primitive value before construction.");
        sb.AppendLine($"{indent1}/// Implement this partial method in your own code file to apply custom validation.");
        sb.AppendLine($"{indent1}/// Throw an exception (e.g., ArgumentException) if validation fails.");
        sb.AppendLine(summaryDocEnd);
        sb.AppendLine($"{indent1}/// <param name=\"value\">The normalized primitive value.</param>");

        // Only declare the partial method signature, never provide an implementation here
        // This way user can provide implementation in their code
        sb.AppendLine($"{indent1}static partial void Validate({info.PrimitiveTypeName} value);");
        sb.AppendLine();
    }

    public static void AppendToStringImplementation(WrapperTypeInfo info, StringBuilder sb, string indent)
    {
        sb.AppendLine($"{indent}/// <summary>");
        sb.AppendLine($"{indent}/// Returns a string representation of the value.");
        sb.AppendLine($"{indent}/// </summary>");
        sb.AppendLine($"{indent}/// <returns>String representation of the underlying value.</returns>");
        sb.AppendLine($"{indent}public override string ToString() => Value.ToString();");
        sb.AppendLine();
    }

    public static void AppendNormalizeMethodImplementation(WrapperTypeInfo info, StringBuilder sb, in string indent1)
    {
        var summaryDocStart = $"{indent1}/// <summary>";
        var summaryDocEnd = $"{indent1}/// </summary>";
        var msg1 =
            $"{indent1}/// Provides a hook for normalizing the primitive value before validation and construction.";
        var msg2 =
            $"{indent1}/// Implement this partial method in your own code file to apply custom normalization.";

        // --- Partial Normalize Implementation ---
        sb.AppendLine(summaryDocStart);
        sb.AppendLine(msg1);
        sb.AppendLine(msg2);
        sb.AppendLine(summaryDocEnd);
        sb.AppendLine($"{indent1}/// <param name=\"value\">The raw primitive value.</param>");
        sb.AppendLine($"{indent1}/// <returns>The normalized primitive value.</returns>");

        // If user has a partial implementation, just declare the partial method signature
        sb.AppendLine(info.HasNormalizeImplementation
            ? $"{indent1}private static partial {info.PrimitiveTypeName} Normalize({info.PrimitiveTypeName} value);"
            // If no user implementation, provide the default (no-op) implementation without an access modifier
            : $"{indent1}static {info.PrimitiveTypeName} Normalize({info.PrimitiveTypeName} value) => value;");
        sb.AppendLine();
    }

    public static void AppendImplicitConvertersImplementation(WrapperTypeInfo info, StringBuilder sb, string indent1)
    {
        // todo: test if i can create an implicit converter to primitive
        // --- Implicit Conversions ---
        sb.AppendLine(
            $"{indent1}/// <summary>Explicitly converts the wrapper to its primitive value.</summary>");
        sb.AppendLine(
            $"{indent1}public static explicit operator {info.PrimitiveTypeName}({info.TypeName} self) => self.Value;");
        sb.AppendLine();
        sb.AppendLine(
            $"{indent1}/// <summary>Implicitly converts a primitive value to the wrapper type.</summary>");
        sb.AppendLine(
            $"{indent1}public static implicit operator {info.TypeName}({info.PrimitiveTypeName} value) => From(value);");
        sb.AppendLine();
    }

    public static void AppendJsonConverterAttributesPointingToConverters(WrapperTypeInfo info, StringBuilder sb,
        string indent
    )
    {
        // Apply JsonConverter attributes pointing to nested converters
        sb.AppendLine($"{indent}/// <summary>");
        sb.AppendLine($"{indent}/// Represents a value wrapper for <see cref=\"{info.PrimitiveTypeName}\"/>.");
        sb.AppendLine($"{indent}/// Generated by ValueWrapperGenerator.");
        sb.AppendLine($"{indent}/// </summary>");
        sb.AppendLine($"{indent}[JsonConverter(typeof({info.TypeName}.{info.SystemTextConverterName}))]");
        sb.AppendLine(
            $"{indent}[NewtonsoftJson.JsonConverter(typeof({info.TypeName}.{info.NewtonsoftConverterName}))]");
    }

    public static void AppendSystemTextJsonConverterImplementation(WrapperTypeInfo info, StringBuilder sb,
        string indent1, string indent2
    )
    {
        // --- System.Text.Json Converter ---
        sb.AppendLine($"{indent1}/// <summary>Internal System.Text.Json converter.</summary>");
        sb.AppendLine(
            $"{indent1}internal class {info.SystemTextConverterName} : JsonConverter<{info.TypeName}>");
        sb.AppendLine($"{indent1}{{");
        // Return type is non-nullable for structs, nullable for classes
        var readReturnType = info.IsValueType ? info.TypeName : $"{info.TypeName}?";
        sb.AppendLine(
            $"{indent2}public override {readReturnType} Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)");
        sb.AppendLine($"{indent2}{{");
        sb.AppendLine($"{indent2}    if (reader.TokenType == System.Text.Json.JsonTokenType.Null)");
        sb.AppendLine($"{indent2}    {{");
        // Return null for classes, default (which is default struct) for structs
        sb.AppendLine(
            $"{indent2}        return {(info.IsValueType ? $"default({info.TypeName})" : "null")};");
        sb.AppendLine($"{indent2}    }}");
        sb.AppendLine(
            $"{indent2}    var primitiveValue = System.Text.Json.JsonSerializer.Deserialize<{info.PrimitiveTypeName}>(ref reader, options);");
        sb.AppendLine($"{indent2}    // Use factory method instead of constructor for validation");
        sb.AppendLine($"{indent2}    return {info.TypeName}.From(primitiveValue!);");
        sb.AppendLine($"{indent2}}}");
        sb.AppendLine();
        sb.AppendLine(
            $"{indent2}public override void Write(System.Text.Json.Utf8JsonWriter writer, {info.TypeName} value, System.Text.Json.JsonSerializerOptions options)");
        sb.AppendLine($"{indent2}{{");
        // Handle null classes passed to Write (structs cannot be null here)
        if (!info.IsValueType)
        {
            sb.AppendLine($"{indent2}    if (value is null)");
            sb.AppendLine($"{indent2}    {{");
            sb.AppendLine($"{indent2}        writer.WriteNullValue();");
            sb.AppendLine($"{indent2}        return;");
            sb.AppendLine($"{indent2}    }}");
        }

        sb.AppendLine($"{indent2}    // Use implicit conversion to get primitive");
        sb.AppendLine(
            $"{indent2}    System.Text.Json.JsonSerializer.Serialize(writer, ({info.PrimitiveTypeName})value, options);");
        sb.AppendLine($"{indent2}}}");
        sb.AppendLine($"{indent1}}}"); // Close SystemTextJsonConverter
        sb.AppendLine();
    }

    public static void AppendNewtonsoftJsonConverterImplementation(WrapperTypeInfo info, StringBuilder sb,
        string indent1, string indent2
    )
    {
        // --- Newtonsoft.Json Converter ---
        sb.AppendLine($"{indent1}/// <summary>Internal Newtonsoft.Json converter.</summary>");
        if (info.IsValueType)
        {
            // Struct implementation - No nullable parameter types
            sb.AppendLine(
                $"{indent1}internal class {info.NewtonsoftConverterName} : NewtonsoftJson.JsonConverter<{info.TypeName}>");
            sb.AppendLine($"{indent1}{{");
            sb.AppendLine(
                $"{indent2}public override void WriteJson(NewtonsoftJson.JsonWriter writer, {info.TypeName} value, NewtonsoftJson.JsonSerializer serializer)");
            sb.AppendLine($"{indent2}{{");
            sb.AppendLine($"{indent2}    // Structs cannot be null, so no null check needed");
            sb.AppendLine($"{indent2}    // Use implicit conversion to get primitive");
            sb.AppendLine(
                $"{indent2}    serializer.Serialize(writer, ({info.PrimitiveTypeName})value);");
            sb.AppendLine($"{indent2}}}");
            sb.AppendLine();
            sb.AppendLine(
                $"{indent2}public override {info.TypeName} ReadJson(NewtonsoftJson.JsonReader reader, Type objectType, {info.TypeName} existingValue, bool hasExistingValue, NewtonsoftJson.JsonSerializer serializer)");
            sb.AppendLine($"{indent2}{{");
            sb.AppendLine($"{indent2}    if (reader.TokenType == NewtonsoftJson.JsonToken.Null)");
            sb.AppendLine($"{indent2}    {{");
            sb.AppendLine($"{indent2}        // Return default value for struct");
            sb.AppendLine($"{indent2}        return default;");
            sb.AppendLine($"{indent2}    }}");
            sb.AppendLine(
                $"{indent2}    var primitiveValue = serializer.Deserialize<{info.PrimitiveTypeName}>(reader);");
            sb.AppendLine($"{indent2}    // Use factory method for validation");
            sb.AppendLine($"{indent2}    return {info.TypeName}.From(primitiveValue!);");
            sb.AppendLine($"{indent2}}}");
        }
        else
        {
            // Class implementation - With nullable parameter types
            sb.AppendLine(
                $"{indent1}internal class {info.NewtonsoftConverterName} : NewtonsoftJson.JsonConverter<{info.TypeName}>");
            sb.AppendLine($"{indent1}{{");
            sb.AppendLine(
                $"{indent2}public override void WriteJson(NewtonsoftJson.JsonWriter writer, {info.TypeName}? value, NewtonsoftJson.JsonSerializer serializer)");
            sb.AppendLine($"{indent2}{{");
            sb.AppendLine($"{indent2}    if (value is null)");
            sb.AppendLine($"{indent2}    {{");
            sb.AppendLine($"{indent2}        writer.WriteNull();");
            sb.AppendLine($"{indent2}        return;");
            sb.AppendLine($"{indent2}    }}");
            sb.AppendLine($"{indent2}    // Use implicit conversion to get primitive");
            sb.AppendLine(
                $"{indent2}    serializer.Serialize(writer, ({info.PrimitiveTypeName})value);");
            sb.AppendLine($"{indent2}}}");
            sb.AppendLine();
            sb.AppendLine(
                $"{indent2}public override {info.TypeName}? ReadJson(NewtonsoftJson.JsonReader reader, Type objectType, {info.TypeName}? existingValue, bool hasExistingValue, NewtonsoftJson.JsonSerializer serializer)");
            sb.AppendLine($"{indent2}{{");
            sb.AppendLine($"{indent2}    if (reader.TokenType == NewtonsoftJson.JsonToken.Null)");
            sb.AppendLine($"{indent2}    {{");
            sb.AppendLine($"{indent2}        return null;");
            sb.AppendLine($"{indent2}    }}");
            sb.AppendLine(
                $"{indent2}    var primitiveValue = serializer.Deserialize<{info.PrimitiveTypeName}>(reader);");
            sb.AppendLine($"{indent2}    // Use factory method for validation");
            sb.AppendLine($"{indent2}    return {info.TypeName}.From(primitiveValue!);");
            sb.AppendLine($"{indent2}}}");
        }

        sb.AppendLine($"{indent1}}}"); // Close NewtonsoftJsonConverter
    }

    // Generates the central registration class ONLY for BSON mapping
    public static string GenerateLiteDbRegistrationImplementation(List<WrapperTypeInfo> types)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System;");
        sb.AppendLine("using LiteDB;");
        sb.AppendLine("using System.Runtime.CompilerServices;");
        sb.AppendLine();
        // Add using directives for all namespaces containing the types
        foreach (var ns in types.Select(t => t.Namespace).Where(n => !string.IsNullOrEmpty(n)).Distinct())
        {
            sb.AppendLine($"using {ns};");
        }

        sb.AppendLine();
        sb.AppendLine("namespace Primify.Generated");
        sb.AppendLine("{");
        sb.AppendLine(
            "    /// <summary>Provides BSON registration for types generated by ValueWrapperGenerator.</summary>");
        sb.AppendLine("    internal static class PrimifyLiteDbRegistration");
        sb.AppendLine("    {");
        sb.AppendLine("        private static bool _initialized = false;");
        sb.AppendLine();
        sb.AppendLine("        /// <summary>Initializes BSON mappers using BsonMapper.Global.</summary>");
        sb.AppendLine("        [ModuleInitializer]");
        sb.AppendLine("        internal static void InitializeGlobalLiteDbBsonMappers()");
        sb.AppendLine("        {");
        sb.AppendLine("            RegisterLiteDbBsonMappers(BsonMapper.Global);");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        /// <summary>Registers BSON mappers with a specific BsonMapper instance.</summary>");
        sb.AppendLine("        public static void RegisterLiteDbBsonMappers(BsonMapper mapper)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (mapper == BsonMapper.Global && _initialized) return;");
        sb.AppendLine("            if (mapper == null) throw new ArgumentNullException(nameof(mapper));");
        sb.AppendLine();

        foreach (var info in types)
        {
            string deserializeExpression;
            string serializeExpression;
            string fullTypeName = info.PrimitiveTypeSymbol.ToDisplayString();

            // Special handling for different supported types
            switch (fullTypeName)
            {
                case "System.Guid":
                    serializeExpression = $"new BsonValue(({info.PrimitiveTypeName})value)";
                    deserializeExpression = $"{info.FullTypeName}.From(bson.AsGuid)";
                    break;

                case "System.DateTime":
                    serializeExpression = $"new BsonValue(({info.PrimitiveTypeName})value)";
                    deserializeExpression = $"{info.FullTypeName}.From(bson.AsDateTime)";
                    break;

                case "System.DateTimeOffset":
                    serializeExpression = $"new BsonValue(({info.PrimitiveTypeName})value.UtcDateTime)";
                    deserializeExpression =
                        $"{info.FullTypeName}.From(System.DateTimeOffset.FromUnixTimeMilliseconds((long)(bson.AsDateTime.ToUniversalTime() - new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)).TotalMilliseconds))";
                    break;

                case "System.TimeSpan":
                    serializeExpression = $"new BsonValue(({info.PrimitiveTypeName})value.Ticks)";
                    deserializeExpression = $"{info.FullTypeName}.From(System.TimeSpan.FromTicks(bson.AsInt64))";
                    break;

                case "System.DateOnly":
                    serializeExpression =
                        $"new BsonValue(({info.PrimitiveTypeName})value.ToDateTime(System.TimeOnly.MinValue))";
                    deserializeExpression = $"{info.FullTypeName}.From(System.DateOnly.FromDateTime(bson.AsDateTime))";
                    break;

                case "System.TimeOnly":
                    serializeExpression = $"new BsonValue(({info.PrimitiveTypeName})value.Ticks)";
                    deserializeExpression =
                        $"{info.FullTypeName}.From(System.TimeOnly.FromTimeSpan(System.TimeSpan.FromTicks(bson.AsInt64)))";
                    break;

                default:
                    // Standard handling for other types
                    var bsonAccessor = GetBsonAccessor(info.PrimitiveTypeSymbol);
                    serializeExpression = $"new BsonValue(({info.PrimitiveTypeName})value)";
                    deserializeExpression = $"{info.FullTypeName}.From(bson.{bsonAccessor})";

                    if (bsonAccessor == "RawValue")
                    {
                        sb.AppendLine(
                            $"            // Warning: BSON mapping for {info.PrimitiveTypeName} used by {info.FullTypeName} using RawValue.");
                    }

                    break;
            }

            sb.AppendLine($"            mapper.RegisterType<{info.FullTypeName}>(");
            sb.AppendLine($"                serialize: (value) => {serializeExpression},");
            sb.AppendLine($"                deserialize: (bson) => {deserializeExpression});");
        }

        sb.AppendLine();
        sb.AppendLine("            if (mapper == BsonMapper.Global) _initialized = true;");
        sb.AppendLine("        }");
        sb.AppendLine("    }"); // Close class
        sb.AppendLine("}"); // Close namespace

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
            case "System.DateOnly": return "AsDateTime"; // Store as DateTime with 00:00:00 time
            case "System.TimeOnly": return "AsInt64"; // Store as ticks since midnight
        }

        // Then check special types
        switch (primitiveTypeSymbol.SpecialType)
        {
            case SpecialType.System_Boolean: return "AsBoolean";
            case SpecialType.System_Char: return "AsString";
            case SpecialType.System_SByte: return "AsInt32";
            case SpecialType.System_Byte: return "AsInt32";
            case SpecialType.System_Int16: return "AsInt32";
            case SpecialType.System_UInt16: return "AsInt32";
            case SpecialType.System_Int32: return "AsInt32";
            case SpecialType.System_UInt32: return "AsInt32";
            case SpecialType.System_Int64: return "AsInt64";
            case SpecialType.System_UInt64: return "AsDecimal";
            case SpecialType.System_Single: return "AsDecimal";
            case SpecialType.System_Double: return "AsDecimal";
            case SpecialType.System_Decimal: return "AsDecimal";
            case SpecialType.System_String: return "AsString";
            default:
                if (primitiveTypeSymbol.TypeKind == TypeKind.Enum)
                    return "AsInt32";
                return "RawValue"; // Fallback for unsupported types
        }
    }

    // New code for Instance members

    public static void AppendPredefinedInstancesImplementation(WrapperTypeInfo info, StringBuilder sb, string indent)
    {
        if (info.PredefinedInstances == null || info.PredefinedInstances.Count == 0)
            return;

        sb.AppendLine($"{indent}#region Predefined Instances");
        sb.AppendLine();
        sb.AppendLine($"{indent}static {info.TypeName}()");
        sb.AppendLine($"{indent}{{");
        foreach (var instance in info.PredefinedInstances)
        {
            var formattedValue = FormatValue(info.PrimitiveTypeSymbol, instance.Value);
            sb.AppendLine($"{indent}    {instance.PropertyName} = new {info.TypeName}({formattedValue});");
        }

        sb.AppendLine($"{indent}}}");
        sb.AppendLine();
        sb.AppendLine($"{indent}#endregion");
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
}
