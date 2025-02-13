using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuironInterpreter.ExpressionClasses
{
    class AstPrinter : Expr.IVisitor<String>
    {
        // this method is to be able to print using printerVisitorInstance.Print(exprClassInstance) rather than exprClassInstance.Accept(printerVisitorInstance)
        public String Print(Expr expr)
        {
            return expr.Accept(this);
        }

        //  Visit methods
        public string VisitBinaryExpr(Expr.Binary expr)
        {
            return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
        }

        public string VisitGroupingExpr(Expr.Grouping expr)
        {
            return Parenthesize("group", expr.Expression);
        }

        public string VisitLiteralExpr(Expr.Literal expr)
        {
            if (expr.Value == null) return "無";
            return expr.Value.ToString();
        }

        public string VisitUnaryExpr(Expr.Unary expr)
        {
            return Parenthesize(expr.Operator.Lexeme, expr.Right);
        }



        private string Parenthesize(String name, params Expr[] exprs)
        {
            StringBuilder builder = new StringBuilder();

            // Special case for binary expressions
            if (exprs.Length == 2)
            {
                // Binary operators should be in infix notation
                builder.Append("(");
                builder.Append(exprs[0].Accept(this));
                builder.Append(" ").Append(name).Append(" ");
                builder.Append(exprs[1].Accept(this));
                builder.Append(")");
            }
            else
            {
                // Keep prefix notation for unary operators and grouping
                builder.Append("(");
                builder.Append(name);
                foreach (Expr expr in exprs)
                {
                    builder.Append(" ");
                    builder.Append(expr.Accept(this));
                }
                builder.Append(")");
            }

            return builder.ToString();
        }
    }
}
