using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static SuironInterpreter.Stmt;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Object = System.Object;

namespace SuironInterpreter
{
    public class Interpreter : Expr.IVisitor<Object>, Stmt.IVisitor<Object?>
    {
        public readonly Environment globals = new Environment();
        private Environment environment;

        public Interpreter()
        {
            this.globals.define("clock", new ClockFunction());
            this.globals.define("input", new UserInputFunction());
            this.globals.define("substring", new SubstringFunction());
            this.globals.define("floor", new FloorFunction());
            this.globals.define("isInt", new IsIntFunction());
            this.globals.define("toInt", new ToIntFunction());
            this.globals.define("wait", new WaitFunction());

            this.environment = this.globals;
        }
        
        public void interpret(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    execute(statement);
                }
            }
            catch (RuntimeErrorException error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{error.Message}\n\n");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(error.StackTrace);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private void execute(Stmt stmt)
        {
            stmt.Accept(this);

        }

        private string stringify(Object @object)
        {
            if (@object == null) return "無"; // return "無";

            if (@object is Double)
            {
                string text = @object.ToString();

                if (text == null)
                {
                    return "";
                }

                if (text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - 2);
                }
                return text;
            }
            else if (@object is string)
            {
                return $"{@object}";
            }
            else if (@object is bool b)
            {
                // Suiron keywords are 真 (True) and 偽 (False)
                // You might want to output these Japanese keywords instead of C#'s "True"/"False"
                // return b ? "真" : "偽";
                return b.ToString(); // This will output "True" or "False" (standard C# bool.ToString())
                                     // Choose which representation you want for printed booleans.
                                     // If you want "真" / "偽", use the commented line above.
            }


            return @object.ToString();
        }
        public Object VisitFunctionStmt(Stmt.Function stmt)
        {
            SuironFunction function = new SuironFunction(stmt);
            environment.define(stmt.Name.Lexeme, function);

            return null;
        }
        public Object VisitCallExpr(Expr.Call expr)
        {
            Object callee = evaluate(expr.Callee);

            List<Object> arguments = new List<Object>();
            foreach (Expr argument in expr.Arguments)
            {
                arguments.Add(evaluate(argument));
            }

            if (!(callee is SuironCallable)) {
                throw new RuntimeErrorException(expr.Paren, "Can only call functions and classes.");
            }

            SuironCallable function = (SuironCallable)callee;

            if (arguments.Count != function.Arity())
            {
                throw new RuntimeErrorException(expr.Paren, $"Expected {function.Arity()} arguments but got {arguments.Count}.");
            }

            return function.call(this, arguments);
        }

        public Object VisitWhileStmt(Stmt.While stmt)
        {
            while (isTruthy(evaluate(stmt.Condition)))
            {
                execute(stmt.Body);
            }
            return null;
        }

        public Object VisitLogicalExpr(Expr.Logical expr)
        {
            Object left = evaluate(expr.Left);

            if (expr.Operator.Type == TokenType.OR)
            {
                if (isTruthy(left))
                {
                    return left;
                }
            }
            else
            {
                if (!isTruthy(left))
                {
                    return left;
                }
            }

            return evaluate(expr.Right);
        }

        public Object? VisitIfStmt(Stmt.If stmt)
        {
            if (isTruthy(evaluate(stmt.Condition)))
            {
                execute(stmt.Thenbranch);
            }
            else if (stmt.Elsebranch != null)
            {
                execute(stmt.Elsebranch);
            }
            return null;
        }

        public Object? VisitBlockStmt(Stmt.Block stmt)
        {
            executeBlock(stmt.Statements, new Environment(environment));
            return null;
        }

        public void executeBlock(List<Stmt> statements, Environment environment)
        {
            Environment previous = this.environment;
            try
            {
                this.environment = environment;

                foreach (Stmt statement in statements)
                {
                    execute(statement);
                }
            }
            finally
            {
                this.environment = previous;
            }
        }

        public Object? VisitAssignExpr(Expr.Assign expr)
        {
            Object value = evaluate(expr.Value);

            environment.assign(expr.Name, value); 

            return value;
        }

        public Object? VisitVarStmt(Stmt.Var stmt)  
        {
            Object value = null;
            if (stmt.Initialiser != null)
            {
                // AstPrinter printer = new AstPrinter();
                // Console.Write(printer.Print(stmt.Initialiser));
                value = evaluate(stmt.Initialiser);
                // Console.WriteLine($" = {value}");
            }

            environment.define(stmt.Name.Lexeme, value);

            return null;
        }

        public Object VisitVariableExpr(Expr.Variable expr)
        {
            return environment.get(expr.Name);
        }

        public Object? VisitExpressionStmt(Stmt.Expression stmt)
        {
            evaluate(stmt.expression);
            return null;
        }

        public Object? VisitPrintStmt(Stmt.Print stmt)
        {
            Object value = evaluate(stmt.Expression);
            Console.WriteLine(stringify(value));
            return null;
        }

        public Object? VisitReturnStmt(Stmt.Return stmt)
        {
            Object value = null;
            if (stmt.Value != null) value = evaluate(stmt.Value);

            throw new Return(value);
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
                    //checkNumberOperands(expr.Operator, left, right);
                    //if (left is double && right is Double)
                    //{
                    //    return (double)left + (double)right;
                    //}

                    checkNumberOperands(expr.Operator, left, right);

                    // Convert operands to doubles if they're strings
                    double leftValue = left is double ? (double)left : Double.Parse((string)left);
                    double rightValue = right is double ? (double)right : Double.Parse((string)right);

                    return leftValue + rightValue;


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
                    
                    // added extra cases later
                    else if (left is string && right is bool)
                    {
                        return (string)left + right.ToString().Replace("True", "真").Replace("False", "偽");
                    }
                    else if (left is bool && right is string)
                    {
                        return left.ToString() + (string)right;
                    }
                    else if (left is Double && right is bool)
                    {
                        return left.ToString() + right.ToString();
                    }
                    else if (left is bool && right is Double)
                    {
                        return left.ToString() + right.ToString();
                    }
                    else if (left is bool && right is bool)
                    {
                        return left.ToString().Replace("True", "真").Replace("False", "偽") + right.ToString().Replace("True", "真").Replace("False", "偽"); // concatenate two booleans
                    }
                    
                    else if (left is null && right is string)
                    {
                        return "無" + (string)right; // concatenate null with string
                    }
                    else if (left is string && right is null)
                    {
                        return (string)left + "無"; // concatenate string with null
                    }
                    else if (left is null && left is null)
                    {
                        return "無無";
                    }

                    return left.ToString() + right.ToString();

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
        private bool checkNumberOperands(Token @operator, Object left, Object right)
        {
            if (left is Double && right is Double) return true;

            if (left is string leftStr && right is string rightStr)
            {
                if (Double.TryParse(leftStr, out _) && Double.TryParse(rightStr, out _))
                {
                    return true;
                }
            }

            throw new RuntimeErrorException(@operator, $"Operands must be numbers.");
        }


        private Object evaluate(Expr expr)
        {
            return expr.Accept(this); // each expression class (inheriting from Expr) has it's own override of Accept which calls the appropriate Visit method - each visit method being part of the visitor interface which is required to be implemented
        }
    }
}
