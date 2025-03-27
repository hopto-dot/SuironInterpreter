using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuironInterpreter
{
    public class Token
    {
        public TokenType Type { get; } // TokenType enum value - for example LEFT_PAREN, SEMICOLON or STRING
        public string Lexeme { get; } // The actual token text from the source code
        public object Literal { get; } // The parsed value as opposed to text form (for strings, numbers, etc.)
        public int Line { get; } // Line no code where token appears

        public Token(TokenType type, string lexeme, object literal, int line)
        {
            // Example class instance: Token(TokenType.NUMBER, "123.45", 123.45, 1)

            Type = type; 
            Lexeme = lexeme; 
            Literal = literal;  
            Line = line; 
        }

        public override string ToString()
        {
            return $"{Type} {Lexeme} {Literal}";
        }
    }
}
