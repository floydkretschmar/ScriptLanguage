using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FlKr.ScriptLanguage.Lexing.Tokens;

namespace FlKr.ScriptLanguage.Parsing
{
    delegate Expression ParseOperation(List<IToken> expression, out Type dataType);

    public partial class Parser
    {
        private Expression ParseAdditionOperationExpression(List<IToken> expression)
        {
            return ParseAdditionOperationExpression(expression, out var dataType);
        }

        private Expression ParseAdditionOperationExpression(List<IToken> expression, out Type dataType)
        {
            List<List<IToken>> parts = SplitIntoExpressions(
                expression,
                true,
                TokenDetailTypes.Addition);

            if (parts.Count > 1)
            {
                return ParseMathOperationExpression(
                    parts,
                    Expression.Add,
                    ParseSubtractionOperationExpression,
                    out dataType);
            }

            return ParseSubtractionOperationExpression(expression, out dataType);
        }

        private Expression ParseSubtractionOperationExpression(List<IToken> expression, out Type dataType)
        {
            List<List<IToken>> parts = SplitIntoExpressions(
                expression,
                true,
                TokenDetailTypes.Subtraction);

            if (parts.Count > 1)
                return ParseMathOperationExpression(
                    parts,
                    Expression.Subtract,
                    ParseMultiplicationOperationExpression,
                    out dataType);

            return ParseMultiplicationOperationExpression(expression, out dataType);
        }

        private Expression ParseMultiplicationOperationExpression(List<IToken> expression, out Type dataType)
        {
            List<List<IToken>> parts = SplitIntoExpressions(
                expression,
                true,
                TokenDetailTypes.Multiplication);

            if (parts.Count > 1)
                return ParseMathOperationExpression(
                    parts,
                    Expression.Multiply,
                    ParseDivisionOperationExpression,
                    out dataType);

            return ParseDivisionOperationExpression(expression, out dataType);
        }

        private Expression ParseDivisionOperationExpression(List<IToken> expression, out Type dataType)
        {
            List<List<IToken>> parts = SplitIntoExpressions(
                expression,
                true,
                TokenDetailTypes.Division);

            if (parts.Count > 1)
                return ParseMathOperationExpression(
                    parts,
                    Expression.Divide,
                    ParseNegativeExpression,
                    out dataType);

            return ParseNegativeExpression(expression, out dataType);
        }

        private Expression ParseNegativeExpression(List<IToken> expression, out Type dataType)
        {
            if (expression.First().DetailType == TokenDetailTypes.Negative)
            {
                if (expression.Count < 2)
                    throw new ParseException(expression, "Invalid unary negative expression");

                var expressionToNegate = ParseNegativeExpression(
                        expression.GetRange(1, expression.Count - 1), out dataType);
                if (dataType != typeof(double))
                    throw new ParseException(expression,
                        "Invalid negation: Negated expressions have to return numeric values.");
                return Expression.Negate(expressionToNegate);
            }
            
            if (expression.Count > 1)
                throw new ParseException(expression, "Invalid value expression.");

            return ParseValueExpression(expression[0], out dataType);
        }

        private Expression ParseMathOperationExpression(
            List<List<IToken>> expressions,
            Func<Expression, Expression, BinaryExpression> expressionDefinition,
            ParseOperation parseOperation,
            out Type dataType)
        {
            var expression = parseOperation(expressions.First(), out dataType);
            if (dataType != typeof(double))
                throw new ParseException(expressions.First(), "Expression is not a valid numerical expression.");

            for (int i = 1; i < expressions.Count; i++)
            {
                var otherExpression = parseOperation(expressions[i], out var subDataType);
                if (subDataType != typeof(double))
                    throw new ParseException(expressions.First(),
                        "Expression is not a valid numerical expression.");

                expression = expressionDefinition(expression, otherExpression);
            }

            return expression;
        }
    }
}