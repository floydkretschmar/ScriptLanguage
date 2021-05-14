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
            var expressions = ParseBlockExpressions(tokens, context);
            List<Expression> expressionLambdas = new List<Expression>();
            
            foreach (var expression in expressions)
            {
                var lambda = ParseStatement(expression, context);
                expressionLambdas.Add(lambda);
            }
            return expressionLambdas;
        }

        private List<List<IToken>> ParseBlockExpressions(List<IToken> tokens, ParsingContext context)
        {
            var beginBlockCount = tokens.Count(t => t.DetailType == TokenDetailTypes.BeginBlock);
            var endBlockCount = tokens.Count(t => t.DetailType == TokenDetailTypes.EndBlock);

            if (beginBlockCount != endBlockCount)
                throw new ParseException(tokens, "Unequal amount of block start and end tokens detected in expression.");

            if (beginBlockCount == 0 && endBlockCount == 0)
                return SplitIntoExpressions(tokens, false, TokenDetailTypes.EndOfLine);

            var decomposedExpression = new List<IToken>();
            var blockExpressions = new Stack<List<IToken>>();
            beginBlockCount = 0;
            endBlockCount = 0;
            foreach (var token in tokens)
            {
                // New bracketed expression starts: Put it on top of the stack
                if (token.DetailType == TokenDetailTypes.BeginBlock)
                {
                    blockExpressions.Push(new List<IToken>());
                }
                // Current bracketed expression ends: Pop it from stack, convert it to Expression and create "Expression"-Token
                else if (token.DetailType == TokenDetailTypes.EndBlock)
                {
                    var blockExpression = blockExpressions.Pop();
                    var blockExpressionToken = new ExpressionToken()
                    {
                        Type = TokenTypes.Syntax,
                        DetailType = TokenDetailTypes.BlockExpression,
                        Value = ParseBlockExpression(blockExpression, context),
                        Expression = blockExpression
                    };

                    // If bracketed expression was nested: Add it to the parent expression
                    if (blockExpressions.Count > 0)
                        blockExpressions.Peek().Add(blockExpressionToken);
                    // otherwise add it to the top-level expression
                    else
                        decomposedExpression.Add(blockExpressionToken);
                }
                // Currently there is a bracketed expression being decomposed: add tokens to the topmost expression
                else if (blockExpressions.Count > 0)
                {
                    blockExpressions.Peek().Add(token);
                }
                // Currently no bracketed expression detected: Token is part of the top-level expression
                else
                {
                    decomposedExpression.Add(token);
                }
            }

            return SplitIntoExpressions(decomposedExpression, false, TokenDetailTypes.EndOfLine);
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