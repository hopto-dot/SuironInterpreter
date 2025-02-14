using SuironInterpreter.ExpressionClasses;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace SuironInterpreter
{
    internal class Program
    {
        static bool hadError = false;
        static bool hadRuntimeError = false;

        private static readonly Interpreter interpreter = new Interpreter();

        static void Main(string[] args)
        {
            // args = [];
            
            
            
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: SuironInterpreter [script]");
                return;
            }
            else if (args.Length == 1)
            {
                runFile(args[0]);
            }
            else
            {
                //Expr expression = new Expr.Binary(
                //                                new Expr.Unary(
                //                                            new Token(TokenType.MINUS, "-", null, 1),
                //                                            new Expr.Literal(123)
                //                                              ),

                //                                new Token(TokenType.STAR, "*", null, 1),

                //                                new Expr.Grouping(
                //                                                 new Expr.Literal(45.67))
                //                                );

                //Console.WriteLine(new AstPrinter().Print(expression));

                Console.WriteLine("Entering interactive prompt...");
                runPrompt();
            }
        }

        private static void runFile(String path)
        {
            String sourceText = File.ReadAllText(path);
            Run(sourceText);

            if (hadError)
            {
                return; // error 65
            }
        }

        private static void Run(String source)
        {
            //Scanner scanner = new Scanner(source);
            //List<Token> tokens = scanner.scanTokens();

            //foreach (Token token in tokens) 
            //{
            //    Console.WriteLine(token);
            //}

            //if (hadError)
            //{
            //    throw new System.Exception("Lexing error.");
            //}

            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.scanTokens();

            Parser parser = new Parser(tokens);
            Expr expression = parser.parse();

            // Stop if there was a syntax error.
            if (hadError)
            {
                return;
            }

            AstPrinter printer = new AstPrinter();

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Evaluating '{printer.Print(expression)}'");
            Console.ForegroundColor = ConsoleColor.White;

            interpreter.interpret(expression);

            // object value = interpreter.evaluate(expression);
        }

        public static void error(int line, String message)
        {
            report(line, "", message);
        }

        public static void error(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
            {
                report(token.Line, " at end", message);
            }
            else
            {
                report(token.Line, $" at '{token.Lexeme}'", message);
            }
        }

        private static void report(int line, String where, String message)
        {
            // Console.WriteLine("[line " + line + "] Error" + where + ": " + message);
            // Console.Error.WriteLine("[line " + line + "] Error" + where + ": " + message);
            Console.ForegroundColor = ConsoleColor.Red;
            // Console.Error.WriteLine("[line " + line + "] Error" + where + ": " + message);
            Console.Error.WriteLine($"Parsing error{where} [line {line}]: {message}");
            Console.ForegroundColor = ConsoleColor.White;
            hadError = true;
        }

        public static void RuntimeError(RuntimeErrorException error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            // Console.WriteLine(error.Message + "\n[line " + error.Token.Line + "]");
            Console.Error.WriteLine($"Runtime error at '{error.Token.Lexeme}': {error.Message}");
            Console.ForegroundColor = ConsoleColor.White;
            hadRuntimeError = true;
        }

        private static void runPrompt()
        {
            while (true)
            {
                Console.Write("〉 ");
                String userInput = Console.ReadLine();
                if (userInput == "exit" || userInput == String.Empty)
                {
                    break;
                }

                else
                {
                    Run(userInput);
                    hadError = false;
                }
            }
        }


    }
}
