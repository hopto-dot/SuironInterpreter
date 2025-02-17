using System;
using System.Collections.Generic;

namespace SuironInterpreter
{
    public abstract class Stmt
    {
        public interface IVisitor<R>
        {
            R VisitExpressionStmt(Expression stmt);
            R VisitPrintStmt(Print stmt);
        }

        public abstract R Accept<R>(IVisitor<R> visitor);

        public class Expression : Stmt
        {
            public Expression(Expr expression)
            {
                Expression = expression;
            }

            public readonly Expr Expression;

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitExpressionStmt(this);
            }
        }

        public class Print : Stmt
        {
            public Print(Expr expression)
            {
                Expression = expression;
            }

            public readonly Expr Expression;

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitPrintStmt(this);
            }
        }

    }
}