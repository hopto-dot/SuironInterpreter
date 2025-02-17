using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Object = System.Object;

namespace SuironInterpreter
{
    class Interpreter : Expr.IVisitor<Object>
    {
        public void interpret(Expr expression)
        {
            try
            {
                Object value = evaluate(expression);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Result: {stringify(value)}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (RuntimeErrorException error)
            {
                Program.RuntimeError(error);
            }
        }

        private string stringify(Object @object)
        {
            if (@object == null) return "無";

            if (@object is Double) {
                string text = @object.ToString();
                if (text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - 2);
                }
                return text;
            }
            else if (@object is string)
            {
                return $"\"{@object}\"";
            }

                return @object.ToString();
        }


        public Object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.Value;
        }
        public Object VisitGroupingExpr(Expr.Grouping expr)
        {
            return evaluate(expr.Expression);
        }
        public Object VisitUnaryExpr(Expr.Unary expr)
        {
            Object right = evaluate(expr.Right);

            switch (expr.Operator.Type)
            {
                case TokenType.MINUS:
                    checkNumberOperand(expr.Operator, right);
                    return -(double)right;
                case TokenType.BANG:
                    return !isTruthy(right);
            }

            return null; // should be unreachable
        }

        private bool isTruthy(Object @object)
        {
            if (@object == null) return false;
            if (@object is bool) return (bool)@object;
            return true;
        }
        public Object VisitBinaryExpr(Expr.Binary expr)
        {
            Object left = evaluate(expr.Left);
            Object right = evaluate(expr.Right);

            switch (expr.Operator.Type)
            {
                case TokenType.GREATER:
                    checkNumberOperands(expr.Operator, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    checkNumberOperands(expr.Operator, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    checkNumberOperands(expr.Operator, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    checkNumberOperands(expr.Operator, left, right);
                    return (double)left <= (double)right;

                case TokenType.BANG_EQUAL: return !isEqual(left, right);
                case TokenType.EQUAL_EQUAL: return isEqual(left, right);

                case TokenType.MINUS:
                    checkNumberOperands(expr.Operator, left, right);
                    return (double)left - (double)right;
                case TokenType.PLUS:
                    checkNumberOperands(expr.Operator, left, right);
                    if (left is double && right is Double)
                    {
                        return (double)left + (double)right;
                    }

                    throw new RuntimeErrorException(expr.Operator, $"Operands must be numbers. If you are trying to concatenate strings use '&'.");
                case TokenType.AMPERSAND: // & should be used for concat instead of +
                    if (left is string && right is string)
                    {
                        return (string)left + (string)right;
                    }
                    else if (left is string && right is Double)
                    {
                        return (string)left + right.ToString();
                    }
                    else if (left is Double && right is string)
                    {
                        return left.ToString() + (string)right;
                    }
                        break;
                case TokenType.SLASH:
                    checkNumberOperands(expr.Operator, left, right);
                    return (double)left / (double)right;
                case TokenType.STAR:
                    checkNumberOperands(expr.Operator, left, right);
                    return (double)left * (double)right;
            }

            return null; // should be unreachable
        }

        private bool isEqual(Object a, Object b)
        {
            if (a == null || b == null) return true;
            if (a == null) return false;
            if (b == null) return false;    

            return a.Equals(b);
        }

        private void checkNumberOperand(Token @operator, Object operand)
        {
            if (operand is Double) return;
            throw new RuntimeErrorException(@operator, $"Operand cannot be '{@operator}'. Must be a number.");
        }
        private void checkNumberOperands(Token @operator, Object left, Object right)
        {
            if (left is Double && right is Double) return;

            throw new RuntimeErrorException(@operator, $"Operands must be numbers.");
        }


        private Object evaluate(Expr expr)
        {
            return expr.Accept(this); // each expression class (inheriting from Expr) has it's own override of Accept which calls the appropriate Visit method - each visit method being part of the visitor interface which is required to be implemented
        }
    }
}
