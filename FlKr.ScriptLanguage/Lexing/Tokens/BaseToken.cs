using System;
using System.Globalization;

namespace FlKr.ScriptLanguage.Lexing.Tokens;

public abstract class BaseToken<TValue> : IToken
{
    public TValue Value { get; set; }

    public TokenTypes Type { get; init; }

    public TokenDetailTypes DetailType { get; init; }

    public abstract string ToString(string format, IFormatProvider formatProvider);

    public override string ToString()
    {
        return ToString(null, CultureInfo.CurrentCulture);
    }
}