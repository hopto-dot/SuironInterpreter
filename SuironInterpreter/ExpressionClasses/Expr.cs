using System;
using System.Collections.Generic;

namespace SuironInterpreter
{
    public abstract class Expr
    {
        public class Binary : Expr
        {
            public Binary(Expr left, Token @operator, Expr right)
            {
                Left = left;
                Operator = @operator;
                Right = right;
            }

            public readonly Expr Left;
            public readonly Token Operator;
            public readonly Expr Right;
        }

        public class Grouping : Expr
        {
            public Grouping(Expr expression)
            {
                Expression = expression;
            }

            public readonly Expr Expression;
        }

        public class Literal : Expr
        {
            public Literal(object value)
            {
                Value = value;
            }

            public readonly object Value;
        }

        public class Unary : Expr
        {
            public Unary(Token @operator, Expr right)
            {
                Operator = @operator;
                Right = right;
            }

            public readonly Token Operator;
            public readonly Expr Right;
        }

    }
}