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
                Console.WriteLine("Starting interactive prompt...");
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

        public static void error(int line, String message)
        {
            report(line, "", message);
        }

        private static void report(int line, String where, String message)
        {
            // Console.WriteLine("[line " + line + "] Error" + where + ": " + message);
            Console.Error.WriteLine("[line " + line + "] Error" + where + ": " + message);
            hadError = true;
        }

        private static void runPrompt()
        {
            while (true)
            {
                Console.Write("> ");
                String userInput = Console.ReadLine();

                if (userInput == String.Empty)
                {
                    break;
                }
                else
                {
                    Console.WriteLine($"Got `{userInput}`");
                    Run(userInput);
                    hadError = false;
                }
            }
        }
    }
}
