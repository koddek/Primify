namespace System.Runtime.CompilerServices
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Polyfill to enable 'init' properties (C# 9.0) in netstandard2.0.
    /// This is required for the library to compile.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit { }
}
