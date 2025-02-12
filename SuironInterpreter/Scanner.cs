using System;
using System.Collections.Generic;

namespace SuironInterpreter
{
    class Scanner
    {
        private readonly string source;
        private readonly List<Token> tokens = new List<Token>();

        private int start = 0; // points to the first character in the lexeme being scanned
        private int current = 0; // points at the character currently being considered
        private int line = 1; // tracks what source line `current` is on so we can produce tokens that know their location

        public Scanner(string source)
        {
            this.source = source;
        }

        public List<Token> scanTokens()
        {
            while (!isAtEnd())
            {
                // We are at the beginning of the next lexeme.
                start = current;
                scanToken();
            }

            tokens.Add(new Token(TokenType.EOF, "", null, line));
            return tokens;
        }

        private bool isAtEnd()
        {
            return current >= source.Length;
        }

        private void scanToken()
        {
            // This will be implemented in the next step
            throw new NotImplementedException();
        }
    }
}