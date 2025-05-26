using System.IO;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
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
            //args = ["test.suiron"];

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
                Console.WriteLine("Entering interactive prompt...\nType valid Suiron code to run it with the interpreter.");
                runPrompt();
            }
        }

        private static void runFile(String path)
        {
            String sourceText = File.ReadAllText(path);
            // Console.WriteLine(sourceText);
            // Console.WriteLine("Running code:");
            Run(sourceText);

            if (hadError)
            {
                return; // error 65
            }
        }

        private static void Run(String source)
        {
            // source = "print \"hello\";";
            // if (!source.TrimEnd().EndsWith(";")) source += ";";

            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.scanTokens();

            Parser parser = new Parser(tokens);
            List<Stmt> statements = parser.parse();

            // Stop if there was a syntax error.
            if (hadError)
            {
                return;
            }

            //Console.ForegroundColor = ConsoleColor.Blue;
            //Console.WriteLine($"Evaluating '{printer.Print(expression)}'");
            //Console.ForegroundColor = ConsoleColor.White;

            interpreter.interpret(statements);

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
