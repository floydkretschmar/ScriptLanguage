using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FlKr.ScriptLanguage.Lexing.Tokens;

namespace FlKr.ScriptLanguage.Parsing
{
    public partial class Parser
    {
        private Expression ParseAdditionOperationExpression(List<IToken> expression, ParsingContext context,
            out Type dataType)
        {
            List<List<IToken>> parts = SplitIntoExpressions(
                expression,
                true,
                TokenDetailTypes.Addition);

            if (parts.Count > 1)
            {
                return ParseMathOperationExpression(
                    parts,
                    context,
                    Expression.Add,
                    ParseSubtractionOperationExpression,
                    out dataType);
            }

            return ParseSubtractionOperationExpression(expression, context, out dataType);
        }

        private Expression ParseSubtractionOperationExpression(List<IToken> expression, ParsingContext context,
            out Type dataType)
        {
            List<List<IToken>> parts = SplitIntoExpressions(
                expression,
                true,
                TokenDetailTypes.Subtraction);

            if (parts.Count > 1)
                return ParseMathOperationExpression(
                    parts,
                    context,
                    Expression.Subtract,
                    ParseMultiplicationOperationExpression,
                    out dataType);

            return ParseMultiplicationOperationExpression(expression, context, out dataType);
        }

        private Expression ParseMultiplicationOperationExpression(List<IToken> expression, ParsingContext context,
            out Type dataType)
        {
            List<List<IToken>> parts = SplitIntoExpressions(
                expression,
                true,
                TokenDetailTypes.Multiplication);

            if (parts.Count > 1)
                return ParseMathOperationExpression(
                    parts,
                    context,
                    Expression.Multiply,
                    ParseDivisionOperationExpression,
                    out dataType);

            return ParseDivisionOperationExpression(expression, context, out dataType);
        }

        private Expression ParseDivisionOperationExpression(List<IToken> expression, ParsingContext context,
            out Type dataType)
        {
            List<List<IToken>> parts = SplitIntoExpressions(
                expression,
                true,
                TokenDetailTypes.Division);

            if (parts.Count > 1)
                return ParseMathOperationExpression(
                    parts,
                    context,
                    Expression.Divide,
                    ParseModuloOperationExpression,
                    out dataType);

            return ParseModuloOperationExpression(expression, context, out dataType);
        }

        private Expression ParseModuloOperationExpression(List<IToken> expression, ParsingContext context,
            out Type dataType)
        {
            List<List<IToken>> parts = SplitIntoExpressions(
                expression,
                true,
                TokenDetailTypes.Modulo);

            if (parts.Count > 1)
                return ParseMathOperationExpression(
                    parts,
                    context,
                    Expression.Modulo,
                    ParseNegativeExpression,
                    out dataType);

            return ParseNegativeExpression(expression, context, out dataType);
        }

        private Expression ParseNegativeExpression(List<IToken> expression, ParsingContext context, out Type dataType)
        {
            if (expression.First().DetailType == TokenDetailTypes.Negative)
            {
                if (expression.Count < 2)
                    throw new ParseException(expression, "Invalid negative numeric expression");

                var expressionToNegate = ParseNegativeExpression(
                    expression.GetRange(1, expression.Count - 1), context, out dataType);
                if (dataType != typeof(double))
                    throw new ParseException(expression,
                        "Expression is not a valid numerical expression.");
                return Expression.Negate(expressionToNegate);
            }

            return ParseExponentiateOperationExpression(expression, context, out dataType);
        }

        private Expression ParseExponentiateOperationExpression(List<IToken> expression, ParsingContext context,
            out Type dataType)
        {
            List<List<IToken>> parts = SplitIntoExpressions(
                expression,
                true,
                TokenDetailTypes.Exponentiation);

            if (parts.Count > 1)
                return ParseMathOperationExpression(
                    parts,
                    context,
                    Expression.Power,
                    ParseValueExpression,
                    out dataType);

            return ParseValueExpression(expression, context, out dataType);
        }

        private Expression ParseMathOperationExpression(
            List<List<IToken>> expressions,
            ParsingContext context,
            Func<Expression, Expression, BinaryExpression> expressionDefinition,
            ParseOperation parseOperation,
            out Type dataType)
        {
            var expression = parseOperation(expressions.First(), context, out dataType);
            if (dataType != typeof(double))
                throw new ParseException(expressions.First(), "Expression is not a valid numerical expression.");

            for (int i = 1; i < expressions.Count; i++)
            {
                var otherExpression = parseOperation(expressions[i], context, out var subDataType);
                if (subDataType != typeof(double))
                    throw new ParseException(expressions.First(),
                        "Expression is not a valid numerical expression.");

                expression = expressionDefinition(expression, otherExpression);
            }

            return expression;
        }
    }
}