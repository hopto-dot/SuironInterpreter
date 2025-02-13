using System;

namespace SuironInterpreter
{
    public class RuntimeErrorException : Exception
    {
        public Token Token { get; }

        public RuntimeErrorException(Token token, string message) : base(message)
        {
            Token = token;
        }

        // These constructors are recommended for proper serialization
        public RuntimeErrorException() { }

        public RuntimeErrorException(string message) : base(message) { }

        public RuntimeErrorException(string message, Exception inner) : base(message, inner) { }
    }
}
