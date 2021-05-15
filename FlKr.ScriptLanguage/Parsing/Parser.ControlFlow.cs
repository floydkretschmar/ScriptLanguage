using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FlKr.ScriptLanguage.Lexing.Tokens;

namespace FlKr.ScriptLanguage.Parsing
{
    public partial class Parser
    {
        private Expression ParseControlFlowStatement(List<IToken> expression, ParsingContext context)
        {
            switch (expression.First().DetailType)
            {
                case TokenDetailTypes.If:
                    return ParseConditionalInstructionExpression(expression, context);
                case TokenDetailTypes.Return:
                    return ParseReturnExpression(expression, context);
                case TokenDetailTypes.Do:
                    throw new ParseException(expression,
                        $"Tokens from type {nameof(TokenDetailTypes.Do)} can never be the leading token in a control flow expression");
                case TokenDetailTypes.Else:
                    throw new ParseException(expression,
                        $"Tokens from type {nameof(TokenDetailTypes.Else)} can never be the leading token in a control flow expression");
                default:
                    throw new ParseException(
                        $"Token detail type {expression.First().DetailType} not implemented yet for token type {expression.First().Type}");
            }
        }

        private Expression ParseReturnExpression(List<IToken> expression, ParsingContext context)
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

            var returnExpression = ParseBracketedExpression(expressionArray[1..^1].ToList(), context);
            if (!context.TryGetVariable(RESULT_VARIABLE_NAME, out var resultVariable))
                throw new ParseException(expression, "Cannot return value in function without defined return type.");

            return Expression.Block(
                Expression.Assign(resultVariable, returnExpression),
                Expression.Return(context.ReturnTarget)
            );
        }

        private Expression ParseConditionalInstructionExpression(List<IToken> expression, ParsingContext context)
        {
            if (expression.First().DetailType != TokenDetailTypes.If)
                throw new ParseException(expression,
                    $"Control flow operations have to start with the {nameof(TokenDetailTypes.If)} token.");

            var subexpression = ExtractConditionExpression(expression.ToArray()[1..], out var position);

            position++;
            Expression conditionLambda = ParseConditionExpression(subexpression, context);

            position++;
            Expression conditionBlockLambda =
                ParseControlFlowExecutionBlockExpression(expression, context, ref position);

            position++;
            Expression controlFlowLambda = null;
            // Get the alternate execution block(s)
            if (expression[position].DetailType is TokenDetailTypes.Else or TokenDetailTypes.ElseIf)
            {
                // get all else ifs
                List<Expression> alternateConditionBlockLambdas = new List<Expression>();
                while (expression[position].DetailType == TokenDetailTypes.ElseIf)
                {
                    position++;
                    alternateConditionBlockLambdas.Add(ParseControlFlowExecutionBlockExpression(
                        expression, context, ref position));
                }

                // get the else
                position++;
                var alternateBlockLambda = ParseControlFlowExecutionBlockExpression(
                    expression, context, ref position);

                controlFlowLambda = Expression.IfThenElse(conditionLambda, conditionBlockLambda, alternateBlockLambda);
            }
            else
            {
                controlFlowLambda = Expression.IfThen(conditionLambda, conditionBlockLambda);
            }

            return controlFlowLambda;
        }

        private List<IToken> ExtractConditionExpression(IToken[] expression, out int position)
        {
            if (expression[0].DetailType == TokenDetailTypes.Do)
                throw new ParseException(expression.ToList(),
                    $"Control flow operation is missing the condition.");

            var subexpression = new List<IToken>();
            position = 0;

            // Get the condition
            while (expression[position].DetailType is not TokenDetailTypes.Do)
            {
                subexpression.Add(expression[position]);
                position++;
                if (position >= expression.Length)
                    throw new ParseException(expression.ToList(),
                        $"Control flow operation is missing the {TokenDetailTypes.Do} token.");
            }

            return subexpression;
        }

        private Expression ParseConditionExpression(List<IToken> expression, ParsingContext context)
        {
            if (expression.Any(t => t.DetailType == TokenDetailTypes.EndOfLine))
                throw new ParseException(expression, "Conditions cannot contain end of statement tokens.");

            if (expression.Any(t => t.DetailType == TokenDetailTypes.Assignment))
                throw new ParseException(expression, "Conditions cannot contain assignment tokens.");

            switch (expression.First().Type)
            {
                case TokenTypes.ControlFlow:
                    throw new ParseException(expression, "Conditions cannot contain control flow tokens.");
                case TokenTypes.Variable:
                case TokenTypes.Value:
                case TokenTypes.LogicOperation:
                case TokenTypes.MathOperation:
                case TokenTypes.Syntax:
                    var condition = ParseBracketedExpression(expression, context, out var dataType);
                    if (dataType != typeof(bool))
                        throw new ParseException("Conditions is not a valid boolean expression.");
                    return condition;
                default:
                    throw new ParseException($"Token type {expression.First().Type} not implemented yet.");
            }
        }

        private Expression ParseControlFlowExecutionBlockExpression(List<IToken> expression, ParsingContext context, ref int position)
        {
            if (expression[position].DetailType == TokenDetailTypes.BlockExpression)
            {
                var expressionToken = (ExpressionToken)expression[position];
                expressionToken.Value = ParseBlockExpression(expressionToken.Expression, context);
                return expressionToken.Value;
            }
            else
            {
                var subexpression = new List<IToken>();
                // Get the execution block
                while (expression[position].DetailType != TokenDetailTypes.EndOfLine)
                {
                    subexpression.Add(expression[position]);
                    position++;
                    if (position >= expression.Count)
                        throw new ParseException(expression,
                            $"Control flow operation was not terminated correctly in the execution block.");
                }

                subexpression.Add(expression[position]);
                return ParseStatement(subexpression, context);
            }
        }

        private Expression ParseBlockExpression(List<IToken> expression, ParsingContext context)
        {
            var blockContext = new ParsingContext(context);

            if (expression.Last().DetailType == TokenDetailTypes.EndOfLine)
            {
                var statements = ParseStatements(expression, blockContext);
                return Expression.Block(blockContext.GetVariables(), statements);
            }
            else
            {
                var statement = ParseStatement(expression, blockContext);
                return Expression.Block(blockContext.GetVariables(), statement);
            }
        }
    }
}