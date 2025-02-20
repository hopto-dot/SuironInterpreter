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
            R VisitVarStmt(Var stmt);
        }

        public abstract R Accept<R>(IVisitor<R> visitor);

        public class Expression : Stmt
        {
            public Expression(Expr expression)
            {
                this.expression = expression;
            }

            public readonly Expr expression;

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitExpressionStmt(this);
            }
        }

        public class Print : Stmt
        {
            public Print(Expr expression)
            {
                this.expression = expression;
            }

            public readonly Expr expression;

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitPrintStmt(this);
            }
        }

        public class Var : Stmt
        {
            public Var(Token name, Expr initialiser)
            {
                Name = name;
                Initialiser = initialiser;
            }

            public readonly Token Name;
            public readonly Expr Initialiser;

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitVarStmt(this);
            }
        }

    }
}