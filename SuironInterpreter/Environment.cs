using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuironInterpreter
{
    class Environment
    {
        public readonly Environment? enclosing = null;
        private readonly Dictionary<String, Object> values = new Dictionary<String, Object>();

        public Environment() // for main global environment
        {
            enclosing = null;
        }

        public Environment(Environment enclosing) // for local scopes
        {
            this.enclosing = enclosing;
        }

        public void define(string name, Object value)
        {
            if (values.ContainsKey(name))
            {
                throw new RuntimeErrorException($"Variable {name} is already defined.");
            }
            //values.Add(name, value);s
            values.Add(name, value);
        }

        public void assign(Token name, Object value)
        {
            if (values.ContainsKey(name.Lexeme))
            {
                values[name.Lexeme] = value;
                return;
            }

            if (enclosing != null)
            {
                enclosing.assign(name, value);
                return;
            }

            throw new RuntimeErrorException(name, $"Variable {name.Lexeme} is undefined.");
        }

        public Object get(Token name)
        {
            if (values.ContainsKey(name.Lexeme))
            {
                return values[name.Lexeme];
            }

            // code will reach here if can't find variable in current scope
            if (enclosing != null)
            {
                return enclosing.get(name); // try outer scope
            }

            throw new RuntimeErrorException($"Variable {name.Lexeme} is undefined.");

        }
    }
}
