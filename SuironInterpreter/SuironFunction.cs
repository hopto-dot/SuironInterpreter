using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SuironInterpreter
{

    class SuironFunction : SuironCallable
    {
        private readonly Stmt.Function declaration;
        public int Arity()
        {
            return declaration.Params.Count;
        }

        public String toString()
        {
            return $"<function {declaration.Name.Lexeme}>";
        }

        public SuironFunction(Stmt.Function declaration)
        {
            this.declaration = declaration;
        }
        public Object call(Interpreter interpreter, List<Object> arguments)
        {
            Environment environment = new Environment(interpreter.globals);
            for (int i = 0; i < declaration.Params.Count; i++)
            {
                environment.define(declaration.Params[i].Lexeme, arguments[i]);
            }

            interpreter.executeBlock(declaration.Body, environment);
            return null;
        }


    }
}
