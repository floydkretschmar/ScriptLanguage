using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FlKr.ScriptLanguage.Lexing.Tokens;

namespace FlKr.ScriptLanguage.Parsing
{
    public partial class Parser
    {
        public LambdaExpression Parse(List<IToken> tokens)
        {
            var expressions = SplitIntoExpressions(tokens, false);
            foreach (var expression in expressions)
            {
                var lambda = ParseExpression(expression);
            }

            return null;
        }

        private List<List<IToken>> SplitIntoExpressions(List<IToken> tokens, bool block)
        {
            return SplitIntoExpressions(tokens, false,
                block ? TokenDetailTypes.EndOfLineBlock : TokenDetailTypes.EndOfLine);
        }

        private List<List<IToken>> SplitIntoExpressions(List<IToken> tokens,
            bool removeSplitter,
            TokenDetailTypes splitter,
            params TokenDetailTypes[] types)
        {
            var expressions = new List<List<IToken>>();
            var expression = new List<IToken>();

            var splitters = new List<TokenDetailTypes>() {splitter};
            splitters.AddRange(types);

            foreach (var token in tokens)
            {
                expression.Add(token);

                if (splitters.Contains(token.DetailType))
                {
                    expressions.Add(expression);
                    expression = new List<IToken>();
                }
            }

            return expressions;
        }

        private Expression ParseExpression(List<IToken> expression)
        {
            if (expression.Count < 1)
                throw new ParseException(expression);

            switch (expression.First().Type)
            {
                case TokenTypes.Variable:
                case TokenTypes.Value:
                case TokenTypes.LogicOperation:
                case TokenTypes.MathOperation:
                    return ParseSingleLineExpression(expression);
                case TokenTypes.Syntax:
                    return ParseSyntaxExpression(expression);
                case TokenTypes.ControlFlow:
                    return ParseControlFlowExpression(expression);
                default:
                    throw new ParseException($"Token type {expression.First().Type} not implemented yet.");
            }
        }
    }
}