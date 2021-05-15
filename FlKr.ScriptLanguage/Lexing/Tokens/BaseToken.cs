using System;

namespace FlKr.ScriptLanguage.Lexing.Tokens
{
    public abstract class BaseToken<TValue> : IToken
    {
        public TValue Value { get; set; }
        
        public TokenTypes Type { get; init; }
        
        public TokenDetailTypes DetailType { get; init; }

        public abstract string ToString(string format, IFormatProvider formatProvider);
        // public string ToString(string format, IFormatProvider formatProvider)
        // {
        //     if (token is Token textToken)
        //         return textToken.Value;
        //     else if (token is ExpressionToken expressionToken)
        //         return $"{string.Join(" ", expressionToken.Expression.Select(x => x.GetValue()))}";
        //     else
        //         return token.ToString();
        // }

        public override string ToString()
        {
            return ToString(null, System.Globalization.CultureInfo.CurrentCulture);
        }
    }
}