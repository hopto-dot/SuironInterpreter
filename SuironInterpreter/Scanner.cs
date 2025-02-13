using System;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SuironInterpreter
{
    class Scanner
    {
        private readonly string source;
        private readonly List<Token> tokens = new List<Token>();

        private static readonly Dictionary<string, TokenType> keywords;

        private int start = 0; // points to the first character in the lexeme being scanned
        private int current = 0; // points at the character currently being considered
        private int line = 1; // tracks what source line `current` is on so we can produce tokens that know their location

        static Scanner()
        {
            keywords = new Dictionary<string, TokenType>
            {
                { "and", TokenType.AND }, // と
                { "class", TokenType.CLASS }, // クラス
                { "else", TokenType.ELSE }, // 違えば
                { "false", TokenType.FALSE }, // 否
                { "for", TokenType.FOR }, // の
                { "fun", TokenType.FUN }, // 関数
                { "if", TokenType.IF }, // もし
                { "nil", TokenType.NIL }, // 無
                { "or", TokenType.OR }, // または // や
                { "print", TokenType.PRINT }, // を表示
                { "return", TokenType.RETURN }, // 返す
                { "super", TokenType.SUPER }, // 親
                { "this", TokenType.THIS }, // この
                { "true", TokenType.TRUE }, // 肯
                { "var", TokenType.VAR }, // は
                { "while", TokenType.WHILE } // 間
            };
        }

        public Scanner(string source)
        {
            this.source = source;
        }

        public List<Token> scanTokens()
        {
            while (!isAtEnd())
            {
                // We are at the beginning of the next lexeme
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
            char c = advance();
            switch (c)
            {
                // these tokens don't have an associated value that needs to be stored
                // so we call addToken without the `literal` arg
                
                case '(': addToken(TokenType.LEFT_PAREN); break;
                case ')': addToken(TokenType.RIGHT_PAREN); break;
                case '{': addToken(TokenType.LEFT_BRACE); break;
                case '}': addToken(TokenType.RIGHT_BRACE); break;
                case ',': addToken(TokenType.COMMA); break;
                case '.': addToken(TokenType.DOT); break;
                case '-': addToken(TokenType.MINUS); break;
                case '+': addToken(TokenType.PLUS); break;
                case ';': addToken(TokenType.SEMICOLON); break;
                case '*': addToken(TokenType.STAR); break;
                case '&': addToken(TokenType.AMPERSAND); break;

                case 'は': addToken(TokenType.BANG); break;

                case '!': addToken(match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break; // if !, and if = then "!=" else "!"
                case '=': addToken(match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
                case '<': addToken(match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
                case '>': addToken(match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;
                
                case '/':
                    if (match('/')) // if the next character is also a slash, then it's a comment
                    {
                        // A comment goes until the end of the line - we must use peek because we also need to see newlines in the switch statement
                        while (peek() != '\n' && !isAtEnd()) // while 'look at this character' != \n, consume character
                        {
                            advance();
                        }
                    }
                    else
                    {
                        addToken(TokenType.SLASH);
                    }
                    break;

                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace (this means the above up until the next break)
                    break;

                case '\n':
                    line++;
                    break;

                case '"':
                    scanString();
                    break;

                default:
                    if (isDigit(c))
                    {
                        scanNumber();
                    }
                    else if (isAlpha(c))
                    {
                        scanIdentifier();
                    }
                    else
                    {
                        Program.error(line, $"Unexpected character: `{c}`"); ;
                    }

                    break;
            }

        }

        private void scanString () {
            while (peek() != '"' && !isAtEnd()) // keep peeking until reach another string or end of line
            {
                if (peek() == '\n') line++;
                advance();
            }

            if (isAtEnd())
            {
                Program.error(line, "Unterminated string.");
                return;
            }

            // The closing `"`
            advance();

            string value = source.Substring(start + 1, current - start - 2);
            addToken(TokenType.STRING, value);
        }

        private void scanNumber()
        {
            while (isDigit(peek()))
            {
                advance(); // keep increase current until we reach a non-digit (keeping start the same)
            }

            // Look for a fractional part.
            if (peek() == '.' && isDigit(peekNext())) // after the digit checking loop has ended, if the next character is a dot and the one after that is a digit
            {
                // Consume the `.`
                advance();

                while (isDigit(peek())) // consume the rest of the digits
                {
                    advance();
                }
            }

            addToken(TokenType.NUMBER,
                Double.Parse(source.Substring(start, current - start))
                );
        }

        private char peekNext()
        {
            if (current + 1 >= source.Length) return '\0';
            return source[current + 1];
        }


        private char advance() // consume the next character in the source file and returns it - for input
        {
            current++;
            return source[current - 1];
        }
        private void addToken(TokenType type) // grab the text of the current lexeme and creates a new token for it - for output
        {
            addToken(type, null);
        }

        private void addToken(TokenType type, object literal)
        {
            string text = source.Substring(start, current - start);
            tokens.Add(new Token(type, text, literal, line));
            // Console.WriteLine($"Added token of type {type}: {text}");
        }

        private bool match(char expected)
        {
            if (isAtEnd()) return false;
            if (source[current] != expected) return false;

            current++;
            return true;
        }

        private char peek() //  like advance() but doesn’t consume the character - 
        {
            if (isAtEnd()) return '\0';
            return source[current];
        }

        private bool isDigit(char c)
        {
            return c >= '0' && c <= '9';
        }
        private bool isAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                    c == '_';
        }

        private bool isAlphaNumeric(char c)
        {
            return isAlpha(c) || isDigit(c);
        }

        private void scanIdentifier()
        {
            while ( isAlphaNumeric(peek()) )
            {
                advance();
            }

            string text = source.Substring(start, current - start);
            TokenType type = keywords.ContainsKey(text) ? keywords[text] : TokenType.IDENTIFIER; // check if identifier is actually a reserved keyword
            addToken(type);
        }
    }
}