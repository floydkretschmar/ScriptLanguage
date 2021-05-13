using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FlKr.ScriptLanguage.Lexing.Tokens;

namespace FlKr.ScriptLanguage.Parsing
{
    public partial class Parser
    {
        private Expression ParseOrOperationExpression(List<IToken> expression, out Type dataType)
        {
            List<List<IToken>> orConditions = SplitIntoExpressions(
                expression,
                true,
                TokenDetailTypes.Or);

            if (orConditions.Count > 1)
            {
                var orExpression = ParseAndOperationExpression(orConditions.First(), out var subDataType);
                if (subDataType != typeof(bool))
                    throw new ParseException(orConditions.First(), "Expression is not a valid boolean expression.");

                for (int i = 1; i < orConditions.Count; i++)
                {
                    var otherOrExpression = ParseAndOperationExpression(orConditions[i], out subDataType);
                    if (subDataType != typeof(bool))
                        throw new ParseException(orConditions[i], "Expression is not a valid boolean expression.");

                    orExpression = Expression.OrElse(orExpression, otherOrExpression);
                }

                dataType = typeof(bool);
                return orExpression;
            }
            else
            {
                return ParseAndOperationExpression(expression, out dataType);
            }
        }

        private Expression ParseAndOperationExpression(List<IToken> expression, out Type dataType)
        {
            List<List<IToken>> andConditions = SplitIntoExpressions(
                expression,
                true,
                TokenDetailTypes.And);

            if (andConditions.Count > 1)
            {
                Expression andExpression = ParseNotOperationExpression(andConditions.First(), out var subDataType);
                if (subDataType != typeof(bool))
                    throw new ParseException(andConditions.First(), "Expression is not a valid boolean expression.");

                for (int i = 1; i < andConditions.Count; i++)
                {
                    var otherAndExpression = ParseNotOperationExpression(andConditions[i], out subDataType);
                    if (subDataType != typeof(bool))
                        throw new ParseException(andConditions[i], "Expression is not a valid boolean expression.");

                    andExpression = Expression.AndAlso(andExpression, otherAndExpression);
                }

                dataType = typeof(bool);
                return andExpression;
            }
            else
            {
                return ParseNotOperationExpression(expression, out dataType);
            }
        }

        private Expression ParseNotOperationExpression(List<IToken> expression, out Type dataType)
        {
            if (expression.First().DetailType == TokenDetailTypes.Not)
            {
                if (expression.Count < 2)
                    throw new ParseException(expression, "Invalid negation in expression");

                var expressionToNegate = ParseNotOperationExpression(
                    expression.GetRange(1, expression.Count - 1), out dataType);
                if (dataType != typeof(bool))
                    throw new ParseException(expression,
                        "Invalid negation: Negated value has to return boolean values.");
                return Expression.Not(expressionToNegate);
            }

            return ParseEqualsOperationExpression(expression, out dataType);
        }

        private Expression ParseEqualsOperationExpression(List<IToken> expression, out Type dataType)
        {
            return ParseComparisonOperationExpression(
                expression, 
                TokenDetailTypes.Equals, 
                Expression.Equal,
                ParseGreaterThanOperationExpression, 
                out dataType);
        }

        private Expression ParseGreaterThanOperationExpression(List<IToken> expression, out Type dataType)
        {
            return ParseComparisonOperationExpression(
                expression, 
                TokenDetailTypes.GreaterThan, 
                Expression.GreaterThan,
                ParseGreaterEqualThanOperationExpression, 
                out dataType);
        }

        private Expression ParseGreaterEqualThanOperationExpression(List<IToken> expression, out Type dataType)
        {
            return ParseComparisonOperationExpression(
                expression, 
                TokenDetailTypes.GreaterEqualThan, 
                Expression.GreaterThanOrEqual,
                ParseLessThanOperationExpression, 
                out dataType);
        }

        private Expression ParseLessThanOperationExpression(List<IToken> expression, out Type dataType)
        {
            return ParseComparisonOperationExpression(
                expression, 
                TokenDetailTypes.LessThan, 
                Expression.LessThan,
                ParseLessEqualThanOperationExpression, 
                out dataType);
        }

        private Expression ParseLessEqualThanOperationExpression(List<IToken> expression, out Type dataType)
        {
            return ParseComparisonOperationExpression(
                expression, 
                TokenDetailTypes.LessEqualThan, 
                Expression.LessThanOrEqual,
                ParseAdditionOperationExpression, 
                out dataType);
        }

        private Expression ParseComparisonOperationExpression(
            List<IToken> expression,
            TokenDetailTypes detailType,
            Func<Expression, Expression, BinaryExpression> comparisonExpression,
            ParseOperation parseOperation,
            out Type dataType)
        {
            List<List<IToken>> expressions = SplitIntoExpressions(
                expression,
                true,
                detailType);
            
            if (expressions.Count > 2)
            {
                throw new ParseException(expression, "Invalid comparison expression");
            }
            else if (expressions.Count == 2)
            {
                var leftPart = parseOperation(expressions[0], out var leftSubType);
                var rightPart = parseOperation(expressions[1], out var rightSubType);

                if (leftSubType != rightSubType)
                    throw new ParseException(expression, "Data types on both sides of the equation do not align.");

                dataType = typeof(bool);
                return comparisonExpression(leftPart, rightPart);
            }
            else
            {
                return parseOperation(expression, out dataType);
            }
        }
    }
}