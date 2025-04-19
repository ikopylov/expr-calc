﻿using ExprCalc.ExpressionParsing.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Representation
{
    internal readonly struct EmptyNode { }
    internal readonly struct ValidationExpressionNodesFactory : IExpressionNodesFactory<EmptyNode>, IAsyncExpressionNodesFactory<EmptyNode>
    {
        public ValidationExpressionNodesFactory(bool validateNumbersCanBeRepresentedAsDouble)
        {
            ValidateNumbersCanBeRepresentedAsDouble = validateNumbersCanBeRepresentedAsDouble;
        }

        public bool ValidateNumbersCanBeRepresentedAsDouble { get; }

        public EmptyNode Number(ReadOnlySpan<char> numberText, int offsetInExpression)
        {
            if (ValidateNumbersCanBeRepresentedAsDouble)
                ExpressionParser.ParseNumberAsDouble(numberText, offsetInExpression);

            return default;
        }
        public EmptyNode BinaryOp(ExpressionOperationType opType, int offsetInExpression, EmptyNode left, EmptyNode right) => default;
        public EmptyNode UnaryOp(ExpressionOperationType opType, int offsetInExpression, EmptyNode value) => default;


        private static ValueTask<EmptyNode> ParseNumberSlow(ReadOnlySpan<char> numberText, int offsetInExpression)
        {
            try
            {
                ExpressionParser.ParseNumberAsDouble(numberText, offsetInExpression);
                return new ValueTask<EmptyNode>(new EmptyNode());
            }
            catch (Exception ex)
            {
                return ValueTask.FromException<EmptyNode>(ex);
            }
        }
        public ValueTask<EmptyNode> NumberAsync(ReadOnlySpan<char> numberText, int offsetInExpression, CancellationToken cancellationToken)
        {
            if (ValidateNumbersCanBeRepresentedAsDouble)
                return ParseNumberSlow(numberText, offsetInExpression);

            return new ValueTask<EmptyNode>(new EmptyNode());
        }
        public ValueTask<EmptyNode> BinaryOpAsync(ExpressionOperationType opType, EmptyNode left, EmptyNode right, int offsetInExpression, CancellationToken cancellationToken) => new ValueTask<EmptyNode>(new EmptyNode());
        public ValueTask<EmptyNode> UnaryOpAsync(ExpressionOperationType opType, EmptyNode value, int offsetInExpression, CancellationToken cancellationToken) => new ValueTask<EmptyNode>(new EmptyNode());
    }
}
