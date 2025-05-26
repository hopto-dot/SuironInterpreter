using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuironInterpreter
{
    public class Return : Exception
    {
        public readonly object Value; // The value being returned.

        public Return(object value) : base(null, null)
        {
            this.Value = value;
        }
    }
}
