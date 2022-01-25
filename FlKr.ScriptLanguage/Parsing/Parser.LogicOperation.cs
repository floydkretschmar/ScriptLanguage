using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FlKr.ScriptLanguage.Lexing.Tokens;

namespace FlKr.ScriptLanguage.Parsing;

public partial class Parser
{
    private Expression ParseOrOperationExpression(List<IToken> expression, ParsingContext context, out Type dataType)
    {
        var orConditions = SplitIntoExpressions(
            expression,
            true,
            TokenDetailTypes.Or);

        if (orConditions.Count > 1)
        {
            var orExpression = ParseAndOperationExpression(orConditions.First(), context, out var subDataType);
            if (subDataType != typeof(bool))
                throw new ParseException(orConditions.First(), "Expression is not a valid boolean expression.");

            for (var i = 1; i < orConditions.Count; i++)
            {
                var otherOrExpression = ParseAndOperationExpression(orConditions[i], context, out subDataType);
                if (subDataType != typeof(bool))
                    throw new ParseException(orConditions[i], "Expression is not a valid boolean expression.");

                orExpression = Expression.OrElse(orExpression, otherOrExpression);
            }

            dataType = typeof(bool);
            return orExpression;
        }

        return ParseAndOperationExpression(expression, context, out dataType);
    }

    private Expression ParseAndOperationExpression(List<IToken> expression, ParsingContext context, out Type dataType)
    {
        var andConditions = SplitIntoExpressions(
            expression,
            true,
            TokenDetailTypes.And);

        if (andConditions.Count > 1)
        {
            var andExpression = ParseNotOperationExpression(andConditions.First(), context, out var subDataType);
            if (subDataType != typeof(bool))
                throw new ParseException(andConditions.First(), "Expression is not a valid boolean expression.");

            for (var i = 1; i < andConditions.Count; i++)
            {
                var otherAndExpression = ParseNotOperationExpression(andConditions[i], context, out subDataType);
                if (subDataType != typeof(bool))
                    throw new ParseException(andConditions[i], "Expression is not a valid boolean expression.");

                andExpression = Expression.AndAlso(andExpression, otherAndExpression);
            }

            dataType = typeof(bool);
            return andExpression;
        }

        return ParseNotOperationExpression(expression, context, out dataType);
    }

    private Expression ParseNotOperationExpression(List<IToken> expression, ParsingContext context, out Type dataType)
    {
        if (expression.First().DetailType == TokenDetailTypes.Not)
        {
            if (expression.Count < 2)
                throw new ParseException(expression, "Invalid negation expression");

            var expressionToNegate = ParseNotOperationExpression(
                expression.GetRange(1, expression.Count - 1), context, out dataType);
            if (dataType != typeof(bool))
                throw new ParseException(expression,
                    "Expression is not a valid boolean expression.");
            return Expression.Not(expressionToNegate);
        }

        return ParseEqualsOperationExpression(expression, context, out dataType);
    }

    private Expression ParseEqualsOperationExpression(List<IToken> expression, ParsingContext context,
        out Type dataType)
    {
        return ParseComparisonOperationExpression(
            expression,
            context,
            TokenDetailTypes.Equals,
            Expression.Equal,
            ParseGreaterThanOperationExpression,
            out dataType);
    }

    private Expression ParseGreaterThanOperationExpression(List<IToken> expression, ParsingContext context,
        out Type dataType)
    {
        return ParseComparisonOperationExpression(
            expression,
            context,
            TokenDetailTypes.GreaterThan,
            Expression.GreaterThan,
            ParseGreaterEqualThanOperationExpression,
            out dataType);
    }

    private Expression ParseGreaterEqualThanOperationExpression(List<IToken> expression, ParsingContext context,
        out Type dataType)
    {
        return ParseComparisonOperationExpression(
            expression,
            context,
            TokenDetailTypes.GreaterEqualThan,
            Expression.GreaterThanOrEqual,
            ParseLessThanOperationExpression,
            out dataType);
    }

    private Expression ParseLessThanOperationExpression(List<IToken> expression, ParsingContext context,
        out Type dataType)
    {
        return ParseComparisonOperationExpression(
            expression,
            context,
            TokenDetailTypes.LessThan,
            Expression.LessThan,
            ParseLessEqualThanOperationExpression,
            out dataType);
    }

    private Expression ParseLessEqualThanOperationExpression(List<IToken> expression, ParsingContext context,
        out Type dataType)
    {
        return ParseComparisonOperationExpression(
            expression,
            context,
            TokenDetailTypes.LessEqualThan,
            Expression.LessThanOrEqual,
            ParseAdditionOperationExpression,
            out dataType);
    }

    private Expression ParseComparisonOperationExpression(
        List<IToken> expression,
        ParsingContext context,
        TokenDetailTypes detailType,
        Func<Expression, Expression, BinaryExpression> comparisonExpression,
        ParseOperation parseOperation,
        out Type dataType)
    {
        var expressions = SplitIntoExpressions(
            expression,
            true,
            detailType);

        if (expressions.Count > 2) throw new ParseException(expression, "Invalid comparison expression");

        if (expressions.Count == 2)
        {
            var leftPart = parseOperation(expressions[0], context, out var leftSubType);
            var rightPart = parseOperation(expressions[1], context, out var rightSubType);

            if (leftSubType != rightSubType)
                throw new ParseException(expression, "Data types on both sides of the equation do not align.");

            dataType = typeof(bool);
            return comparisonExpression(leftPart, rightPart);
        }

        return parseOperation(expression, context, out dataType);
    }
}