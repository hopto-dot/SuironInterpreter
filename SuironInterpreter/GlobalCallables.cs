using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuironInterpreter
{
    public class ClockFunction : SuironCallable
    {
        public int Arity()
        {
            return 0;
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            return (double)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
        }

        public override string ToString()
        {
            return "<native function>";
        }
    }

    public class UserInputFunction : SuironCallable
    {
        public int Arity()
        {
            return 1;
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            return Console.ReadLine();
        }

        public override string ToString()
        {
            return "<native function>";
        }
    }
}
