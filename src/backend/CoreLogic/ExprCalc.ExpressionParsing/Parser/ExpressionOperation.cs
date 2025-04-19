﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprCalc.ExpressionParsing.Parser
{
    internal enum OperatorAssociativity
    {
        None,
        Left,
        Right
    }


    internal class ExpressionOperation
    {
        public static readonly ExpressionOperation Add = new ExpressionOperation("+", ExpressionOperationType.Add, 1, OperatorAssociativity.Left, 2, false);
        public static readonly ExpressionOperation Subtract = new ExpressionOperation("-", ExpressionOperationType.Subtract, 1, OperatorAssociativity.Left, 2, false);
        public static readonly ExpressionOperation Multiply = new ExpressionOperation("*", ExpressionOperationType.Multiply, 2, OperatorAssociativity.Left, 2, false);
        public static readonly ExpressionOperation Divide = new ExpressionOperation("/", ExpressionOperationType.Divide, 2, OperatorAssociativity.Left, 2, false);
        public static readonly ExpressionOperation Exponent = new ExpressionOperation("^", ExpressionOperationType.Exponent, 3, OperatorAssociativity.Right, 2, false);

        public static readonly ExpressionOperation UnaryMinus = new ExpressionOperation("-u", ExpressionOperationType.UnaryMinus, 4, OperatorAssociativity.Right, 1, false);
        public static readonly ExpressionOperation UnaryPlus = new ExpressionOperation("+u", ExpressionOperationType.UnaryPlus, 4, OperatorAssociativity.Right, 1, false);

        public static readonly ExpressionOperation OpeningBracket = new ExpressionOperation("(", null, 0, OperatorAssociativity.None, 0, false);
        public static readonly ExpressionOperation ClosingBracket = new ExpressionOperation(")", null, 0, OperatorAssociativity.None, 0, false);

        public static readonly ExpressionOperation LnFunc = new ExpressionOperation("ln", ExpressionOperationType.Ln, 10, OperatorAssociativity.Right, 1, true);

        public static readonly ExpressionOperation[] Functions = [LnFunc];


        public static ExpressionOperation GetOperatorForLexerToken(Lexer.TokenType token)
        {
            return token switch
            {
                Lexer.TokenType.Plus => Add,
                Lexer.TokenType.Minus => Subtract,
                Lexer.TokenType.MultiplicationSign => Multiply,
                Lexer.TokenType.DivisionSign => Divide,
                Lexer.TokenType.ExponentSign => Exponent,
                _ => throw new UncatchableParserException("Unexpected token for expression operation: " + token.ToString()),
            };
        }

        public static ExpressionOperation GetUnaryOperatorForLexerToken(Lexer.TokenType token)
        {
            return token switch
            {
                Lexer.TokenType.Plus => UnaryPlus,
                Lexer.TokenType.Minus => UnaryMinus,
                _ => throw new UncatchableParserException("Unexpected token for unary operator: " + token.ToString()),
            };
        }

        // =========

        private ExpressionOperation(string name, ExpressionOperationType? operationType,
            int priority, OperatorAssociativity associativity, int operandCount, bool isFunction)
        {
            Name = name;
            OperationType = operationType;
            Priority = priority;
            Associativity = associativity;
            IsFunction = isFunction;
            OperandCount = operandCount;
        }

        public string Name { get; }
        public ExpressionOperationType? OperationType { get; }
        public int Priority { get; }
        public OperatorAssociativity Associativity { get; }
        public bool IsFunction { get; }
        public int OperandCount { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}
