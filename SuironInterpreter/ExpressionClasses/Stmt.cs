using System;
using System.Collections.Generic;

namespace SuironInterpreter
{
    public abstract class Stmt
    {
        public interface IVisitor<R>
        {
            R VisitBlockStmt(Block stmt);
            R VisitExpressionStmt(Expression stmt);
            R VisitFunctionStmt(Function stmt);
            R VisitIfStmt(If stmt);
            R VisitPrintStmt(Print stmt);
            R VisitReturnStmt(Return stmt);
            R VisitVarStmt(Var stmt);
            R VisitWhileStmt(While stmt);
        }

        public abstract R Accept<R>(IVisitor<R> visitor);

        public class Block : Stmt
        {
            public Block(List<Stmt> statements)
            {
                Statements = statements;
            }

            public readonly List<Stmt> Statements;

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitBlockStmt(this);
            }
        }

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

        public class Function : Stmt
        {
            public Function(Token name, List<Token> @params, List<Stmt> body)
            {
                Name = name;
                Params = @params;
                Body = body;
            }

            public readonly Token Name;
            public readonly List<Token> Params;
            public readonly List<Stmt> Body;

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitFunctionStmt(this);
            }
        }

        public class If : Stmt
        {
            public If(Expr condition, Stmt thenBranch, Stmt elseBranch)
            {
                Condition = condition;
                Thenbranch = thenBranch;
                Elsebranch = elseBranch;
            }

            public readonly Expr Condition;
            public readonly Stmt Thenbranch;
            public readonly Stmt Elsebranch;

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitIfStmt(this);
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

        public class Return : Stmt
        {
            public Return(Token keyword, Expr value)
            {
                Keyword = keyword;
                Value = value;
            }

            public readonly Token Keyword;
            public readonly Expr Value;

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitReturnStmt(this);
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

        public class While : Stmt
        {
            public While(Expr condition, Stmt body)
            {
                Condition = condition;
                Body = body;
            }

            public readonly Expr Condition;
            public readonly Stmt Body;

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitWhileStmt(this);
            }
        }

    }
}