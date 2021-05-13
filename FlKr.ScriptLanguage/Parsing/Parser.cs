using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FlKr.ScriptLanguage.Lexing.Tokens;

namespace FlKr.ScriptLanguage.Parsing
{
    delegate Expression ParseOperation(List<IToken> expression, out Type dataType);
    
    public partial class Parser
    {
        public Func<T> Parse<T>(List<IToken> tokens)
        {
            var expressions = SplitIntoExpressions(tokens, false);
            List<Expression> expressionLambdas = new List<Expression>();
            
            foreach (var expression in expressions)
            {
                var lambda = ParseStatement(expression);
                expressionLambdas.Add(lambda);
            }
            var finalExpression = Expression.Block(_variables.Values.ToArray(), expressionLambdas);
            return Expression.Lambda<Func<T>>(finalExpression).Compile();
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

        private Expression ParseStatement(List<IToken> expression)
        {
            if (expression.Count < 2)
                throw new ParseException(expression, "Invalid statement.");
            
            if (expression.Last().DetailType != TokenDetailTypes.EndOfLine &&
                expression.Last().DetailType != TokenDetailTypes.EndOfLineBlock)
                throw new ParseException(expression, "Invalid end of statement.");

            switch (expression.First().Type)
            {
                case TokenTypes.Variable:
                    return ParseVariableStatement(expression);
                case TokenTypes.ControlFlow:
                    return ParseControlFlowStatement(expression);
                case TokenTypes.Value:
                case TokenTypes.LogicOperation:
                case TokenTypes.MathOperation:
                case TokenTypes.Syntax:
                    throw new ParseException(expression,
                        $"Only expression beginning with tokens from type {nameof(TokenTypes.Variable)} or " +
                        $"{nameof(TokenTypes.ControlFlow)} can be used as statements.");
                default:
                    throw new ParseException($"Token type {expression.First().Type} not implemented yet.");
            }
        }
    }
}