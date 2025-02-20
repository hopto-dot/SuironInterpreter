using System;
using System.Collections.Generic;

namespace SuironInterpreter
{
    public abstract class Expr
    {
        public interface IVisitor<R>
        {
            R VisitAssignExpr(Assign expr);
            R VisitGroupingExpr(Grouping expr);
            R VisitLiteralExpr(Literal expr);
            R VisitUnaryExpr(Unary expr);
            R VisitVariableExpr(Variable expr);
        }

        public abstract R Accept<R>(IVisitor<R> visitor);

        public class Assign : Expr
        {
            public Assign(Token name, Expr valueBinary)
            {
                Name = name;
                Valuebinary = valueBinary;
            }

            public readonly Token Name;
            public readonly Expr Valuebinary;

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitAssignExpr(this);
            }
        }

        public class Grouping : Expr
        {
            public Grouping(Expr expression)
            {
                Expression = expression;
            }

            public readonly Expr Expression;

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitGroupingExpr(this);
            }
        }

        public class Literal : Expr
        {
            public Literal(object value)
            {
                Value = value;
            }

            public readonly object Value;

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitLiteralExpr(this);
            }
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

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitUnaryExpr(this);
            }
        }

        public class Variable : Expr
        {
            public Variable(Token name)
            {
                Name = name;
            }

            public readonly Token Name;

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitVariableExpr(this);
            }
        }

    }
}