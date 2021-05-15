using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FlKr.ScriptLanguage.Lexing.Tokens
{
    public class ExpressionToken : BaseToken<Expression>
    {
        public List<IToken> Expression { get; init; }
        
        public Type DataType { get; init; }
        public override string ToString(string format, IFormatProvider formatProvider)
        {
            return $"{string.Join(" ", this.Expression.Select(x => x.ToString()))}";
        }
    }
}