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
            Console.Write(arguments[0]);
            string userInput = Console.ReadLine();
            return userInput;
        }

        public override string ToString()
        {
            return "<native function>";
        }
    }

    public class SubstringFunction : SuironCallable
    {
        public int Arity()
        {
            return 3;
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            if (arguments[0] is not string str)
            {
                throw new RuntimeErrorException(null, "First argument to substring must be a string.");
            }
            if (arguments[1] is not double startIndexDouble || !IsInteger(startIndexDouble))
            {
                throw new RuntimeErrorException(null, "Second argument to substring (start index) must be an integer.");
            }
            if (arguments[2] is not double lengthDouble || !IsInteger(lengthDouble))
            {
                throw new RuntimeErrorException(null, "Third argument to substring (length) must be an integer.");
            }

            int startIndex = Convert.ToInt32(startIndexDouble);
            int length = Convert.ToInt32(lengthDouble);

            if (startIndex < 0 || startIndex > str.Length)
            {
                throw new RuntimeErrorException(null, "Substring start index is out of bounds.");
            }
            if (length < 0 || startIndex + length > str.Length)
            {
                throw new RuntimeErrorException(null, "Substring length is out of bounds or extends beyond string length.");
            }

            return str.Substring(startIndex, length);
        }

        private bool IsInteger(double number)
        {
            return Math.Abs(number % 1) <= (Double.Epsilon * 100);
        }

        public override string ToString()
        {
            return "<native function>";
        }
    }

    public class IsIntFunction : SuironCallable
    {
        public int Arity()
        {
            return 1; // Takes one argument
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            if (arguments[0] == null)
            {
                return false; // or throw an error, depending on desired behavior for nil
            }

            if (arguments[0] is double number)
            {
                // A double is an integer if it has no fractional part.
                // Comparing with its floored value is a reliable way to check this.
                return number == Math.Floor(number);
            }
            // If it's not a double, it's definitely not an integer in the context of Suiron's number type
            return false;
        }

        public override string ToString()
        {
            return "<native function>";
        }
    }

    public class FloorFunction : SuironCallable
    {
        public int Arity()
        {
            return 1; // Takes one argument
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            if (arguments[0] == null)
            {
                // Decide how to handle nil: throw error or return nil/0?
                // Throwing an error is often safer for math functions.
                throw new RuntimeErrorException(null, "Cannot call floor on nil.");
            }

            if (arguments[0] is double number)
            {
                return Math.Floor(number);
            }

            // If not a double, it's an invalid type for floor
            throw new RuntimeErrorException(null, $"Operand for floor must be a number. Got {arguments[0].GetType()}.");
        }

        public override string ToString()
        {
            return "<native function>";
        }
    }

    public class ToIntFunction : SuironCallable
    {
        public int Arity()
        {
            return 1;
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            if (arguments[0] == null)
            {
                throw new RuntimeErrorException(null, "Cannot call toInt on nil.");
            }

            if (arguments[0] is string strValue)
            {
                if (double.TryParse(strValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double numberValue))
                {
                    // Check if the parsed double has no fractional part
                    if (numberValue == Math.Floor(numberValue))
                    {
                        return numberValue; // Return the double
                    }
                    else
                    {
                        throw new RuntimeErrorException(null, $"String '{strValue}' represents a number with a decimal part, cannot convert to integer.");
                    }
                }
                else
                {
                    throw new RuntimeErrorException(null, $"String '{strValue}' cannot be converted to a valid number.");
                }
            }
            else
            {
                throw new RuntimeErrorException(null, $"Argument to toInt must be a string. Got {arguments[0].GetType()}.");
            }
        }

        public override string ToString()
        {
            return "<native function>";
        }
    }
}
