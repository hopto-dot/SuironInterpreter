using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuironInterpreter
{
    class Environment
    {
        private readonly Dictionary<String, Object> values = new Dictionary<String, Object>();

        public void define(string name, Object value)
        {
            //if (values.ContainsKey(name)) return;
            //values.Add(name, value);
            values[name] = value;
        }

        public Object get(Token name)
        {
            if (values.ContainsKey(name.Lexeme))
            {
                return values[name.Lexeme];
            }

            throw new RuntimeErrorException($"Variable {name.Lexeme} is undefined.");

        }
    }
}
