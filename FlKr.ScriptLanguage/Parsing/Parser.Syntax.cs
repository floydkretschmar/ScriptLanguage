using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FlKr.ScriptLanguage.Lexing.Tokens;

namespace FlKr.ScriptLanguage.Parsing
{
    public partial class Parser
    {
        private Expression ParseSyntaxExpression(List<IToken> expression)
        {
            switch (expression.First().DetailType)
            {
                case TokenDetailTypes.EndOfLine:
                    throw new ParseException(expression,
                        $"Tokens from type {nameof(TokenDetailTypes.EndOfLine)} can never be the leading token in an expression");
                case TokenDetailTypes.EndOfLineBlock:
                    throw new ParseException(expression,
                        $"Tokens from type {nameof(TokenDetailTypes.EndOfLineBlock)} can never be the leading token in an expression");
                case TokenDetailTypes.RightBracket:
                    throw new ParseException(expression,
                        $"Tokens from type {nameof(TokenDetailTypes.RightBracket)} can never be the leading token in an expression");
                case TokenDetailTypes.LeftBracket:
                case TokenDetailTypes.Expression:
                    return ParseSingleLineExpression(expression);
                default:
                    throw new ParseException(
                        $"Token detail type {expression.First().DetailType} not implemented yet for token type {expression.First().Type}");
            }
        }
        
        private Expression ParseSingleLineExpression(List<IToken> expression)
        {
            var assignmentTokens = expression.Select(token => token.DetailType == TokenDetailTypes.Assignment);
            if (assignmentTokens.Count() > 1)
                throw new ParseException(expression, "Only one assignment per expression is allowed.");
            if (assignmentTokens.Count() == 1)
                return ParseVariableAssignmentExpression(expression);

            return ParseBracketedExpression(expression);
        }

        private Expression ParseBracketedExpression(List<IToken> expression)
        {
            return ParseBracketedExpression(expression, out var dataType);
        }

        private Expression ParseBracketedExpression(List<IToken> expression, out Type dataType)
        {
            var leftBracketCount = expression.Select(t => t.DetailType == TokenDetailTypes.LeftBracket).Count();
            var rightBracketCount = expression.Select(t => t.DetailType == TokenDetailTypes.RightBracket).Count();

            if (leftBracketCount != rightBracketCount)
                throw new ParseException(expression, "Unequal amount of brackets detected in expression.");

            if (leftBracketCount == 0 && rightBracketCount == 0)
                return ParseOrOperationExpression(expression, out dataType);

            var decomposedExpression = new List<IToken>();
            var bracketExpressions = new Stack<List<IToken>>();
            foreach (var token in expression)
            {
                // New bracketed expression starts: Put it on top of the stack
                if (token.DetailType == TokenDetailTypes.LeftBracket)
                {
                    bracketExpressions.Push(new List<IToken>());
                }
                // Current bracketed expression ends: Pop it from stack, convert it to Expression and create "Expression"-Token
                else if (token.DetailType == TokenDetailTypes.RightBracket)
                {
                    var bracketExpression = bracketExpressions.Pop();
                    var bracketExpressionToken = new ExpressionToken()
                    {
                        Type = TokenTypes.Syntax,
                        DetailType = TokenDetailTypes.Expression,
                        Value = ParseOrOperationExpression(bracketExpression, out var type),
                        Expression = bracketExpression,
                        DataType = type
                    };

                    // If bracketed expression was nested: Add it to the parent expression
                    if (bracketExpressions.Count > 0)
                        bracketExpressions.Peek().Add(bracketExpressionToken);
                    // otherwise add it to the top-level expression
                    else
                        decomposedExpression.Add(bracketExpressionToken);
                }
                // Currently there is a bracketed expression being decomposed: add tokens to the topmost expression
                else if (bracketExpressions.Count > 0)
                {
                    bracketExpressions.Peek().Add(token);
                }
                // Currently no bracketed expression detected: Token is part of the top-level expression
                else
                {
                    decomposedExpression.Add(token);
                }
            }

            return ParseOrOperationExpression(decomposedExpression, out dataType);
        }
    }
}