using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FlKr.ScriptLanguage.Lexing.Tokens;

namespace FlKr.ScriptLanguage.Parsing
{
    
    delegate Expression ParseOperation(List<IToken> expression, ParsingContext context, out Type dataType);
    
    public partial class Parser
    {
        private static readonly string RESULT_VARIABLE_NAME = "ergebnis";
        
        public Func<T> Parse<T>(List<IToken> tokens)
        {
            LabelTarget returnTarget = Expression.Label();
            ParsingContext context = new ParsingContext()
            {
                ReturnTarget = returnTarget
            };
            ParameterExpression resultVariable = Expression.Variable(typeof(T));
            context.AddVariable(RESULT_VARIABLE_NAME, resultVariable);
            
            var statements = ParseStatements(tokens, context);
            statements.Add(Expression.Label(returnTarget));
            statements.Add(resultVariable);
            
            var finalExpression = Expression.Block(context.GetVariables(), statements);
            return Expression.Lambda<Func<T>>(finalExpression).Compile();
        }

        private List<List<IToken>> SplitIntoStatements(List<IToken> tokens)
        {
            var expressions = SplitIntoExpressions(tokens, false, TokenDetailTypes.EndOfLine);

            var mergedExpressions = new List<List<IToken>>();
            var mergedExpression = new List<IToken>();
            bool merging = false;
            foreach (var expression in expressions)
            {
                if (expression.First().DetailType == TokenDetailTypes.If)
                {
                    if (merging)
                        throw new ParseException(tokens, "Invalid conditional expression.");
                    mergedExpression = new List<IToken>();
                    mergedExpression.AddRange(expression);
                    merging = true;
                }
                else if (expression.First().DetailType == TokenDetailTypes.EndOfControlFlowOperation)
                {
                    if (!merging)
                        throw new ParseException(tokens, "Invalid conditional expression.");
                    mergedExpression.AddRange(expression);
                    mergedExpressions.Add(mergedExpression);
                    merging = false;
                }
                else if (merging)
                {
                    mergedExpression.AddRange(expression);
                }
                else
                {
                    mergedExpressions.Add(expression);
                }
            }

            return mergedExpressions;
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

        private List<Expression> ParseStatements(List<IToken> tokens, ParsingContext context)
        {
            var expressions = SplitIntoStatements(tokens);
            List<Expression> expressionLambdas = new List<Expression>();
            
            foreach (var expression in expressions)
            {
                var lambda = ParseStatement(expression, context);
                expressionLambdas.Add(lambda);
            }
            return expressionLambdas;
        }

        private Expression ParseStatement(List<IToken> expression, ParsingContext context)
        {
            if (expression.Count < 2)
                throw new ParseException(expression, "Invalid statement.");
            
            if (expression.Last().DetailType != TokenDetailTypes.EndOfLine)
                throw new ParseException(expression, "Invalid end of statement.");

            switch (expression.First().Type)
            {
                case TokenTypes.Variable:
                    return ParseVariableStatement(expression, context);
                case TokenTypes.ControlFlow:
                    return ParseControlFlowStatement(expression, context);
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