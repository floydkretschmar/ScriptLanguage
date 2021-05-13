using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FlKr.ScriptLanguage.Lexing.Tokens;

namespace FlKr.ScriptLanguage.Parsing
{
    public partial class Parser
    {
        public Action Parse(List<IToken> tokens)
        {
            var expressions = SplitIntoExpressions(tokens, false);
            List<Expression> expressionLambdas = new List<Expression>();
            
            foreach (var expression in expressions)
            {
                var lambda = ParseExpression(expression);
                expressionLambdas.Add(lambda);
            }
            Expression finalExpression = Expression.Block(_variables.Values.ToArray(), expressionLambdas);
            return Expression.Lambda<Action>(finalExpression).Compile();
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
            expressions.Add(expression);

            var splitters = new List<TokenDetailTypes>() {splitter};
            splitters.AddRange(types);

            foreach (var token in tokens)
            {
                if (!splitters.Contains(token.DetailType) || !removeSplitter)
                    expressions.Last().Add(token);

                if (splitters.Contains(token.DetailType) && (tokens.IndexOf(token) + 1) < tokens.Count)
                {
                    expressions.Add(new List<IToken>());
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