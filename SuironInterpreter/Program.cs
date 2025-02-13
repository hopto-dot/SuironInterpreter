using System.IO;
using System.Runtime.InteropServices;

namespace SuironInterpreter
{
    internal class Program
    {
        static bool hadError = false;

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
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.scanTokens();

            foreach (Token token in tokens) 
            {
                Console.WriteLine(token);
            }

            if (hadError)
            {
                throw new System.Exception("Lexing error.");
            }
        }

        public static void error(int line, String message, String where = "")
        {
            report(line, where, message);
        }

        private static void report(int line, String where, String message)
        {
            // Console.WriteLine("[line " + line + "] Error" + where + ": " + message);
            // Console.Error.WriteLine("[line " + line + "] Error" + where + ": " + message);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Error [line {line}]: {message}");
            Console.ForegroundColor = ConsoleColor.White;
            hadError = true;
        }

        private static void runPrompt()
        {
            while (true)
            {
                Console.Write("〉");
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
