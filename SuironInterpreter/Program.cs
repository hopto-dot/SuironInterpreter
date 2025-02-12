using System.IO;
using System.Runtime.InteropServices;

namespace SuironInterpreter
{
    internal class Program
    {
        static void Main(string[] args)
        {
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
                runPrompt();
            }
        }

        private static void runFile(String path)
        {
            String sourceText = File.ReadAllText(path);
            Run(sourceText);
        }

        private static void Run(String source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.scanTokens();

            foreach (Token token in tokens)
            {
                Console.WriteLine(token);
            }
        }

        private static void runPrompt()
        {

        }
    }
}
