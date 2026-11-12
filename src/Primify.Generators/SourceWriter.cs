namespace Primify.Generators;

using System.Text;

/// <summary>
/// Indentation-aware writer for generated source. Emission sites call <see cref="WriteLine"/> with
/// unindented text; nesting is expressed with <see cref="OpenBrace"/> and <see cref="CloseBrace"/>,
/// which manage the indentation depth. This replaces StringBuilder plus post-hoc re-indentation, so
/// emitted code never carries manual four-space prefixes and multi-line fragments can't drift.
/// </summary>
internal sealed class SourceWriter
{
    private readonly StringBuilder _builder = new();

    /// <summary>Gets the number of open brace scopes.</summary>
    public int Depth { get; private set; }

    /// <summary>Writes a blank line.</summary>
    public SourceWriter BlankLine()
    {
        _builder.Append('\n');
        return this;
    }

    /// <summary>Pushes the indentation depth by one. Use with <see cref="PopIndent"/> for continuation lines.</summary>
    public SourceWriter PushIndent()
    {
        Depth++;
        return this;
    }

    /// <summary>Pops the indentation depth by one.</summary>
    public SourceWriter PopIndent()
    {
        Depth--;
        return this;
    }

    /// <summary>Writes a single line at the current indentation, or an empty line when no text is given.</summary>
    public SourceWriter WriteLine(string text = "")
    {
        if (text.Length == 0)
        {
            _builder.Append('\n');
            return this;
        }

        _builder.Append(' ', Depth * 4).Append(text).Append('\n');
        return this;
    }

    /// <summary>Writes <c>{</c> and increases the indentation depth.</summary>
    public SourceWriter OpenBrace()
    {
        WriteLine("{");
        return PushIndent();
    }

    /// <summary>Decreases the indentation depth and writes <c>}</c>.</summary>
    public SourceWriter CloseBrace()
    {
        PopIndent();
        WriteLine("}");
        return this;
    }

    /// <summary>Returns the emitted text.</summary>
    public override string ToString() => _builder.ToString();
}
