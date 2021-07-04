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
            var message = "Tokens from type {0} can never be the leading token in a control flow expression";
            switch (expression.First().DetailType)
            {
                case TokenDetailTypes.If:
                    return ParseConditionalInstructionExpression(expression, context);
                case TokenDetailTypes.Return:
                    return ParseReturnExpression(expression, context);
                case TokenDetailTypes.Do:
                    throw new ParseException(expression, string.Format(message, TokenDetailTypes.Do));
                case TokenDetailTypes.Else:
                    throw new ParseException(expression, string.Format(message, TokenDetailTypes.Else));
                case TokenDetailTypes.ElseIf:
                    throw new ParseException(expression, string.Format(message, TokenDetailTypes.ElseIf));
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
                throw new ParseException(expression, "Return operation is invalid.");

            var returnExpression = ParseBracketedExpression(expressionArray[1..^1].ToList(), context);
            if (!context.TryGetVariable(RESULT_VARIABLE_NAME, out var resultVariable))
                throw new InvalidOperationException("Return value parameter has not been defined in Parse function.");

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

            int position = 1;
            var subexpression = ExtractConditionExpression(expression, ref position);
            Expression condition = ParseConditionExpression(subexpression, context);
            Expression conditionBlock =
                ParseControlFlowExecutionBlockExpression(expression, context, ref position);

            // Get the alternate execution block(s)
            // get all else ifs (if they exist)
            List<Expression> alternateConditions =
                new List<Expression>();
            List<Expression> alternateConditionBlocks =
                new List<Expression>();
            if (expression[position].DetailType == TokenDetailTypes.ElseIf)
            {
                while (expression[position].DetailType == TokenDetailTypes.ElseIf)
                {
                    position++;
                    subexpression = ExtractConditionExpression(expression, ref position);
                    var additionalCondition = ParseConditionExpression(subexpression, context);
                    var additionalConditionBlock = ParseControlFlowExecutionBlockExpression(expression, context, ref position);
                    alternateConditions.Add(additionalCondition);
                    alternateConditionBlocks.Add(additionalConditionBlock);
                }
                position++;
            }

            // get the else-block (if it exists)
            Expression elseBlock = null;
            if (expression[position].DetailType == TokenDetailTypes.Else)
            {
                position++;
                elseBlock = ParseControlFlowExecutionBlockExpression(
                    expression, context, ref position);
            }

            // Construct the if-elseif-else statement:
            Expression controlFlowLambda = null;
            if (alternateConditionBlocks.Count > 0 || elseBlock != null)
            {
                if (alternateConditionBlocks.Count > 0)
                {
                    if (alternateConditions.Count != alternateConditionBlocks.Count)
                        throw new ParseException(expression, "More else if Blocks than else if conditions detected.");
                    var finalCondition = alternateConditions.Last();
                    var finalBlock = alternateConditionBlocks.Last();
                    var elseIfBlocks = elseBlock != null
                        ? Expression.IfThenElse(finalCondition, finalBlock, elseBlock)
                        : Expression.IfThen(finalCondition, finalBlock);
                    
                    for (int i = alternateConditionBlocks.Count - 1; i >= 0; i--)
                    {
                        elseIfBlocks = Expression.IfThenElse(alternateConditions[i], alternateConditionBlocks[i],
                            elseIfBlocks);
                    }

                    controlFlowLambda = Expression.IfThenElse(condition, conditionBlock, elseIfBlocks);
                }
                else
                {
                    controlFlowLambda = Expression.IfThenElse(condition, conditionBlock, elseBlock);
                }
            }
            else
            {
                controlFlowLambda = Expression.IfThen(condition, conditionBlock);
            }
            
            return controlFlowLambda;
        }

        private List<IToken> ExtractConditionExpression(List<IToken> expression, ref int position)
        {
            if (expression[position].DetailType == TokenDetailTypes.Do ||
                !expression.Exists(x => x.DetailType == TokenDetailTypes.Do))
                throw new ParseException(expression,
                    $"Control flow operation is missing the condition.");

            var subexpression = new List<IToken>();

            // Get the condition
            while (expression[position].DetailType is not TokenDetailTypes.Do)
            {
                subexpression.Add(expression[position]);
                position++;
                if (position >= expression.Count)
                    throw new ParseException(expression.ToList(),
                        $"Control flow operation is missing the {TokenDetailTypes.Do} token.");
            }

            position++;
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

        private Expression ParseControlFlowExecutionBlockExpression(List<IToken> expression, ParsingContext context,
            ref int position)
        {
            if (expression[position].Type == TokenTypes.Expression &&
                expression[position].DetailType == TokenDetailTypes.BlockExpression)
            {
                var expressionToken = (ExpressionToken) expression[position];
                expressionToken.Value = ParseBlockExpression(expressionToken.Expression, context);
                position++;
                return expressionToken.Value;
            }

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

            position++;
            return ParseStatement(subexpression, context);
        }

        private Expression ParseBlockExpression(List<IToken> expression, ParsingContext context)
        {
            var blockContext = new ParsingContext(context);
            var statements = ParseStatements(expression, blockContext);
            return Expression.Block(blockContext.GetVariables(), statements);
        }
    }
}