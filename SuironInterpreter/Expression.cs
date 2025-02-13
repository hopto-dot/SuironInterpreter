using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuironInterpreter
{
    public abstract class Expr
    {
        public class Binary : Expr
        {
            public Binary(Expr left, Token @operator, Expr right)
            {
                Left = left;
                Operator = @operator; // operator token
                Right = right;
            }

            public readonly Expr Left;
            public readonly Token Operator;
            public readonly Expr Right;
        }

        // Other expressions...
    }
}
