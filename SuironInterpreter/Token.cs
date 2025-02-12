using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuironInterpreter
{
    public class Token
    {
        public TokenType Type { get; }
        public string Lexeme { get; }
        public object Literal { get; }
        public int Line { get; }

        public Token(TokenType type, string lexeme, object literal, int line)
        {
            // Example class instance: Token(TokenType.NUMBER, "123.45", 123.45, 1)

            Type = type; // TokenType enum value - for example LEFT_PAREN, SEMICOLON or STRING
            Lexeme = lexeme; // The actual token text from the source code
            Literal = literal;  // The parsed value (for strings, numbers, etc.)
            Line = line; // Line no code where token appears
        }

        public override string ToString()
        {
            return $"{Type} {Lexeme} {Literal}";
        }
    }
}
