using System;

namespace FlKr.ScriptLanguage.Lexing.Tokens
{
    public class Token : BaseToken<string>
    {
        public override string ToString(string format, IFormatProvider formatProvider)
        {
            return this.Value;
        }

        public static Token EndOfLine()
        {
            return new Token()
            {
                Type = TokenTypes.Syntax,
                DetailType = TokenDetailTypes.EndOfLine
            };
        }
    }
}