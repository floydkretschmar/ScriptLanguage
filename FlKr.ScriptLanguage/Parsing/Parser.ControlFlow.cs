using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FlKr.ScriptLanguage.Lexing.Tokens;

namespace FlKr.ScriptLanguage.Parsing
{
    public partial class Parser
    {
        private Expression ParseControlFlowStatement(List<IToken> expression)
        {
            switch (expression.First().DetailType)
            {
                case TokenDetailTypes.If:
                    return ParseConditionalInstructionExpression(expression);
                case TokenDetailTypes.Return:
                    return ParseReturnExpression(expression);
                case TokenDetailTypes.Then:
                    throw new ParseException(expression,
                        $"Tokens from type {nameof(TokenDetailTypes.Then)} can never be the leading token in a control flow expression");
                case TokenDetailTypes.Else:
                    throw new ParseException(expression,
                        $"Tokens from type {nameof(TokenDetailTypes.Else)} can never be the leading token in a control flow expression");
                default:
                    throw new ParseException(
                        $"Token detail type {expression.First().DetailType} not implemented yet for token type {expression.First().Type}");
            }
        }

        private Expression ParseReturnExpression(List<IToken> expression)
        {
            var expressionArray = expression.ToArray();
            if (expression.First().DetailType != TokenDetailTypes.Return)
                throw new ParseException(expression,
                    $"Return operations have to start with the {nameof(TokenDetailTypes.Return)} token.");

            if (expressionArray[^1].DetailType != TokenDetailTypes.EndOfLine)
                throw new ParseException(expression,
                    $"Return operation was not terminated with a {nameof(TokenDetailTypes.EndOfLine)} token.");

            var subexpression = expressionArray[1..^1].ToList();
            if (subexpression.Count < 1)
                throw new ParseException(expression,
                    $"Return operation is invalid.");

            return ParseBracketedExpression(expressionArray[1..^1].ToList());
        }

        private Expression ParseConditionalInstructionExpression(List<IToken> expression)
        {
            if (expression.First().DetailType != TokenDetailTypes.If)
                throw new ParseException(expression,
                    $"Control flow operations have to start with the {nameof(TokenDetailTypes.If)} token.");

            if (expression[1].DetailType == TokenDetailTypes.Then)
                throw new ParseException(expression,
                    $"Control flow operation is missing the condition.");

            var subexpression = new List<IToken>();
            int position = 1;

            // Get the condition
            while (expression[position].DetailType is not TokenDetailTypes.Then)
            {
                subexpression.Add(expression[position]);
                position++;
                if (position >= expression.Count)
                    throw new ParseException(expression,
                        $"Control flow operation is missing the {TokenDetailTypes.Then} token.");
            }

            Expression conditionLambda = ParseConditionExpression(subexpression);

            position++;
            subexpression.Clear();
            // Get the execution block
            while (expression[position].DetailType != TokenDetailTypes.Else &&
                   expression[position].DetailType != TokenDetailTypes.EndOfLine)
            {
                subexpression.Add(expression[position]);
                position++;
                if (position >= expression.Count)
                    throw new ParseException(expression,
                        $"Control flow operation was not terminated correctly in the execution block.");
            }

            Expression blockLambda = ParseBlockExpression(subexpression);

            Expression controlFlowLambda = null;
            // Get the alternate execution block
            if (expression[position].DetailType == TokenDetailTypes.Else)
            {
                position++;
                subexpression.Clear();
                while (expression[position].DetailType != TokenDetailTypes.EndOfLine)
                {
                    subexpression.Add(expression[position]);
                    position++;
                    if (position >= expression.Count)
                        throw new ParseException(expression,
                            $"Control flow operation was not terminated correctly in the alternate execution block.");
                }

                Expression alternateBlockLambda = ParseBlockExpression(subexpression);
                controlFlowLambda = Expression.IfThenElse(conditionLambda, blockLambda, alternateBlockLambda);
            }
            else
            {
                controlFlowLambda = Expression.IfThen(conditionLambda, blockLambda);
            }

            position++;
            if (position >= expression.Count || expression[position].DetailType != TokenDetailTypes.EndOfLine)
                throw new ParseException(expression,
                    $"Control flow operation was not terminated with a {nameof(TokenDetailTypes.EndOfLine)} token.");

            return controlFlowLambda;
        }

        private Expression ParseConditionExpression(List<IToken> expression)
        {
            throw new NotImplementedException();
        }

        private Expression ParseBlockExpression(List<IToken> expression)
        {
            throw new NotImplementedException();
        }
    }
}