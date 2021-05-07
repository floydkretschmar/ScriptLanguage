using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FlKr.ScriptLanguage.Base;

namespace FlKr.ScriptLanguage.Parsing
{
    public class Parser
    {
        private Dictionary<string, ParameterExpression> _variables;

        public Parser()
        {
            _variables = new Dictionary<string, ParameterExpression>();
        }

        public LambdaExpression Parse(List<Token> tokens)
        {
            var expressions = SplitIntoExpressions(tokens, false);
            foreach (var expression in expressions)
            {
                var lambda = ParseExpression(expression);
            }
            
            return null;
        }

        private List<List<Token>> SplitIntoExpressions(List<Token> tokens, bool block)
        {
            return SplitIntoExpressions(tokens, false,
                block ? TokenDetailTypes.EndOfLineBlock : TokenDetailTypes.EndOfLine);
        }

        private List<List<Token>> SplitIntoExpressions(List<Token> tokens,
            bool removeSplitter,
            TokenDetailTypes splitter,
            params TokenDetailTypes[] types)
        {
            var expressions = new List<List<Token>>();
            var expression = new List<Token>();

            var splitters = new List<TokenDetailTypes>() {splitter};
            splitters.AddRange(types);

            foreach (var token in tokens)
            {
                expression.Add(token);

                if (splitters.Contains(token.DetailType))
                {
                    expressions.Add(expression);
                    expression = new List<Token>();
                }
            }

            return expressions;
        }

        private Expression ParseExpression(List<Token> expression)
        {
            if (expression.Count < 1)
                throw new ParseException(expression);

            switch (expression.First().Type)
            {
                case TokenTypes.Variable:
                case TokenTypes.Value:
                case TokenTypes.LogicOperation:
                case TokenTypes.MathOperation:
                    return ParseSingleLineExpression(expression);
                case TokenTypes.Syntax:
                    return ParseSyntaxExpression(expression);
                case TokenTypes.ControlFlow:
                    return ParseControlFlowExpression(expression);
                default:
                    throw new ParseException($"Token type {expression.First().Type} not implemented yet.");
            }
        }

        private Expression ParseSingleLineExpression(List<Token> expression)
        {
            var assignmentTokens = expression.Select(token => token.DetailType == TokenDetailTypes.Assignment);
            if (assignmentTokens.Count() > 1)
                throw new ParseException(expression, "Only one assignment per expression is allowed.");
            if (assignmentTokens.Count() == 1)
                return ParseVariableAssignmentExpression(expression);

            return ParseValueExpression(expression);
        }

        private Expression ParseValueExpression(List<Token> expression)
        {
            return ParseValueExpression(expression, out var dataType);
        }

        private Expression ParseValueExpression(List<Token> expression, out Type dataType)
        {
            List<List<Token>> orConditions = SplitIntoExpressions(
                expression,
                true,
                TokenDetailTypes.Or);

            if (orConditions.Count > 1)
            {
                Expression orExpression = ParseAndOperationExpression(orConditions.First());
                for (int i = 1; i < orConditions.Count; i++)
                {
                    orExpression = Expression.OrElse(orExpression, ParseAndOperationExpression(orConditions[i]));
                }

                dataType = typeof(bool);
                return orExpression;
            }
            else
            {
                return ParseAndOperationExpression(expression, out dataType);
            }
        }

        private Expression ParseAndOperationExpression(List<Token> expression)
        {
            return ParseAndOperationExpression(expression, out var dataType);
        }

        private Expression ParseAndOperationExpression(List<Token> expression, out Type dataType)
        {
            List<List<Token>> andConditions = SplitIntoExpressions(
                expression,
                true,
                TokenDetailTypes.And);

            if (andConditions.Count > 1)
            {
                Expression andExpression = ParseEqualsOperationExpression(andConditions.First());
                for (int i = 1; i < andConditions.Count; i++)
                {
                    andExpression = Expression.AndAlso(andExpression, ParseEqualsOperationExpression(andConditions[i]));
                }

                dataType = typeof(bool);
                return andExpression;
            }
            else
            {
                return ParseEqualsOperationExpression(expression, out dataType);
            }
        }

        private Expression ParseEqualsOperationExpression(List<Token> expression)
        {
            return ParseEqualsOperationExpression(expression, out var dataType);
        }

        private Expression ParseEqualsOperationExpression(List<Token> expression, out Type dataType)
        {
            List<List<Token>> equalsConditions = SplitIntoExpressions(
                expression,
                true,
                TokenDetailTypes.And);
            if (equalsConditions.Count > 2)
            {
                throw new ParseException(expression, "Invalid equality expression");
            }
            else if (equalsConditions.Count == 2)
            {
                var equalityParts = new List<Expression>();
                foreach (var equalsCondition in equalsConditions)
                {
                    equalityParts.Add(ParseNotOperationExpression(equalsCondition, expression));
                }

                dataType = typeof(bool);
                return Expression.Equal(equalityParts[0], equalityParts[1]);
            }
            else
            {
                return ParseNotOperationExpression(expression, expression, out dataType);
            }
        }

        private Expression ParseNotOperationExpression(List<Token> expression, List<Token> parentExpression)
        {
            return ParseNotOperationExpression(expression, parentExpression, out var dataType);
        }

        private Expression ParseNotOperationExpression(List<Token> expression, List<Token> parentExpression, out Type dataType)
        {
            if (expression.First().DetailType == TokenDetailTypes.Not)
            {
                if (expression.Count < 2)
                    throw new ParseException(parentExpression, "Invalid negation in equality expression");

                dataType = typeof(bool);
                return Expression.Not(
                    ParseMathOperationExpression(
                        expression.GetRange(1, expression.Count - 1)));
            }

            return ParseMathOperationExpression(expression, out dataType);
        }

        private Expression ParseMathOperationExpression(List<Token> expression)
        {
            throw new NotImplementedException();
        }

        private Expression ParseMathOperationExpression(List<Token> expression, out Type dataType)
        {
            throw new NotImplementedException();
        }
        
        private Expression ParseVariableAssignmentExpression(List<Token> expression)
        {
            if (expression.Count < 3)
                throw new ParseException(expression,
                    $"Variable value assignment has to consist of at least 3 tokens: {nameof(TokenDetailTypes.VariableName)} {nameof(TokenDetailTypes.Assignment)} {nameof(TokenDetailTypes.VariableName)} or {nameof(TokenTypes.Value)}");

            if (expression.First().DetailType != TokenDetailTypes.VariableName)
                throw new ParseException(expression,
                    $"First token of a variable value assignment has to be a {nameof(TokenDetailTypes.VariableName)}");

            if (expression[1].DetailType != TokenDetailTypes.Assignment)
                throw new ParseException(expression,
                    $"Second token of a variable value assignment has to be a {nameof(TokenDetailTypes.Assignment)}");

            var valueExpression = ParseValueExpression(expression.ToArray()[2..].ToList(), out var dataType);
            
            if (!_variables.TryGetValue(expression[1].Value, out var parameterExpression))
            {
                parameterExpression = Expression.Variable(dataType);
                _variables.Add(expression[1].Value, parameterExpression);
            }

            return Expression.Assign(parameterExpression, valueExpression);
        }

        private Expression ParseSyntaxExpression(List<Token> expression)
        {
            switch (expression.First().DetailType)
            {
                case TokenDetailTypes.EndOfLine:
                    throw new ParseException(expression,
                        $"Tokens from type {nameof(TokenDetailTypes.EndOfLine)} can never be the leading token in an expression");
                case TokenDetailTypes.EndOfLineBlock:
                    throw new ParseException(expression,
                        $"Tokens from type {nameof(TokenDetailTypes.EndOfLineBlock)} can never be the leading token in an expression");
                default:
                    throw new ParseException(
                        $"Token detail type {expression.First().DetailType} not implemented yet for token type {expression.First().Type}");
            }
        }

        private Expression ParseControlFlowExpression(List<Token> expression)
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

        private Expression ParseReturnExpression(List<Token> expression)
        {
            var expressionArray = expression.ToArray();
            if (expression.First().DetailType != TokenDetailTypes.Return)
                throw new ParseException(expression,
                    $"Return operations have to start with the {nameof(TokenDetailTypes.Return)} token.");

            if (expressionArray[^1].DetailType != TokenDetailTypes.EndOfLine)
                throw new ParseException(expression,
                    $"Return operation was not terminated with a {nameof(TokenDetailTypes.EndOfLine)} token.");

            if (expressionArray[1..].Length < 2)
                throw new ParseException(expression,
                    $"Return operation is invalid.");

            return ParseValueExpression(expressionArray[1..].ToList());
        }

        private Expression ParseConditionalInstructionExpression(List<Token> expression)
        {
            if (expression.First().DetailType != TokenDetailTypes.If)
                throw new ParseException(expression,
                    $"Control flow operations have to start with the {nameof(TokenDetailTypes.If)} token.");

            if (expression[1].DetailType == TokenDetailTypes.Then)
                throw new ParseException(expression,
                    $"Control flow operation is missing the condition.");

            var subexpression = new List<Token>();
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

        private Expression ParseConditionExpression(List<Token> expression)
        {
            throw new NotImplementedException();
        }

        private Expression ParseBlockExpression(List<Token> expression)
        {
            throw new NotImplementedException();
        }
    }
}