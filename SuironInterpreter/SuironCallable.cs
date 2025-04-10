using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuironInterpreter
{
    public interface SuironCallable
    {
        public int Arity();

        public Object call(Interpreter interpreter, List<Object> arguments)
        {
            throw new NotImplementedException();
        }
    }
}
