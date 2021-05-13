using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FlKr.ScriptLanguage.Lexing.Tokens;

namespace FlKr.ScriptLanguage.Parsing
{
    public partial class Parser
    {
        private Dictionary<string, ParameterExpression> _variables;

        public Parser()
        {
            _variables = new Dictionary<string, ParameterExpression>();
        }
        
        private Expression ParseVariableAssignmentExpression(List<IToken> expression)
        {
            if (expression.Count < 3)
                throw new ParseException(expression,
                    $"Variable value assignment has to consist of at least 3 tokens: {nameof(TokenDetailTypes.VariableName)} {nameof(TokenDetailTypes.Assignment)} {nameof(TokenDetailTypes.VariableName)} or {nameof(TokenTypes.Value)}");

            if (expression.First().DetailType != TokenDetailTypes.VariableName)
                throw new ParseException(expression,
                    $"First token of a variable value assignment has to be a {nameof(TokenDetailTypes.VariableName)}");

            if (expression[1].DetailType != TokenDetailTypes.Assignment)
                throw new ParseException(expression,
                    $"Second token of a variable value assignment has to be a {nameof(TokenDetailTypes.Assignment)}");

            var valueExpression = ParseBracketedExpression(expression.ToArray()[2..].ToList(), out var dataType);
            var variableName = ((Token) expression[1]).Value;

            if (!_variables.TryGetValue(variableName, out var parameterExpression))
            {
                parameterExpression = Expression.Variable(dataType);
                _variables.Add(variableName, parameterExpression);
            }

            return Expression.Assign(parameterExpression, valueExpression);
        }
    }
}