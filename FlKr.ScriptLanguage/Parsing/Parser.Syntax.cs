using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FlKr.ScriptLanguage.Lexing.Tokens;

namespace FlKr.ScriptLanguage.Parsing;

public partial class Parser
{
    private Expression ParseVariableStatement(List<IToken> expression, ParsingContext context)
    {
        var expressionWithoutEndToken = expression.SkipLast(1).ToList();
        var assignmentTokenCount =
            expressionWithoutEndToken.Count(token => token.DetailType == TokenDetailTypes.Assignment);
        if (assignmentTokenCount > 1)
            throw new ParseException(expression, "Only one assignment per expression is allowed.");
        if (assignmentTokenCount < 1)
            throw new ParseException(expression,
                $"Only variable expression from type {nameof(TokenDetailTypes.Assignment)} can be used as statement");
        return ParseVariableAssignmentExpression(expressionWithoutEndToken, context);
    }

    private Expression ParseBracketedExpression(List<IToken> expression, ParsingContext context)
    {
        return ParseBracketedExpression(expression, context, out var dataType);
    }

    private Expression ParseBracketedExpression(List<IToken> expression, ParsingContext context, out Type dataType)
    {
        var leftBracketCount = expression.Count(t => t.DetailType == TokenDetailTypes.LeftBracket);
        var rightBracketCount = expression.Count(t => t.DetailType == TokenDetailTypes.RightBracket);

        if (leftBracketCount != rightBracketCount)
            throw new ParseException(expression, "Unequal amount of brackets detected in expression.");

        if (leftBracketCount == 0 && rightBracketCount == 0)
            return ParseOrOperationExpression(expression, context, out dataType);

        var decomposedExpression = new List<IToken>();
        var bracketExpressions = new Stack<List<IToken>>();
        foreach (var token in expression)
            // New bracketed expression starts: Put it on top of the stack
            if (token.DetailType == TokenDetailTypes.LeftBracket)
            {
                bracketExpressions.Push(new List<IToken>());
            }
            // Current bracketed expression ends: Pop it from stack, convert it to Expression and create "Expression"-Token
            else if (token.DetailType == TokenDetailTypes.RightBracket)
            {
                var bracketExpression = bracketExpressions.Pop();
                var bracketExpressionToken = new ExpressionToken
                {
                    Type = TokenTypes.Expression,
                    DetailType = TokenDetailTypes.BracketedExpression,
                    Value = ParseOrOperationExpression(bracketExpression, context, out var type),
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

        return ParseOrOperationExpression(decomposedExpression, context, out dataType);
    }

    private List<List<IToken>> ParseBlockExpressions(List<IToken> tokens, ParsingContext context)
    {
        var beginBlockCount = tokens.Count(t => t.DetailType == TokenDetailTypes.BeginBlock);
        var endBlockCount = tokens.Count(t => t.DetailType == TokenDetailTypes.EndBlock);

        if (beginBlockCount != endBlockCount)
            throw new ParseException(tokens, "Unequal amount of block start and end tokens detected in expression.");

        if (beginBlockCount == 0 && endBlockCount == 0)
            return SplitIntoEndOfLineExpressions(tokens);

        var decomposedExpression = new List<IToken>();
        var blockExpressions = new Stack<List<IToken>>();
        foreach (var token in tokens)
            // New bracketed expression starts: Put it on top of the stack
            if (token.DetailType == TokenDetailTypes.BeginBlock)
            {
                blockExpressions.Push(new List<IToken>());
            }
            // Current bracketed expression ends: Pop it from stack, convert it to Expression and create "Expression"-Tokenk
            else if (token.DetailType == TokenDetailTypes.EndBlock)
            {
                var blockExpression = blockExpressions.Pop();
                var blockExpressionToken = new ExpressionToken
                {
                    Type = TokenTypes.Expression,
                    DetailType = TokenDetailTypes.BlockExpression,
                    // Dont evaluate the expression here yet, otherwise variables might not be resolved yet
                    // Will be evaluated in ParseStatement
                    Value = null,
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

        var splitExpressions = SplitIntoEndOfLineExpressions(decomposedExpression);
        return splitExpressions;
    }
}