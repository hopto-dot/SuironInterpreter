using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

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
                    // return arguments[0]
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
    public class WaitFunction : SuironCallable
    {
        public int Arity()
        {
            return 1;
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            if (arguments[0] == null)
            {
                throw new RuntimeErrorException(null, "Argument to wait() cannot be nil.");
            }

            if (arguments[0] is double seconds)
            {
                if (seconds < 0)
                {
                    throw new RuntimeErrorException(null, "Wait duration cannot be negative.");
                }

                try
                {
                    int milliseconds = Convert.ToInt32(seconds * 1000);
                    if (milliseconds < 0)
                    {
                        throw new RuntimeErrorException(null, "Wait duration is too large and resulted in an overflow.");
                    }
                    Thread.Sleep(milliseconds);
                }
                catch (OverflowException)
                {
                    throw new RuntimeErrorException(null, "Wait duration is too large to be represented as milliseconds.");
                }

                return null; // wait function doesn't need to return a meaningful value
            }
            else
            {
                throw new RuntimeErrorException(null, $"Argument to wait() must be a number (seconds). Got {arguments[0].GetType()}.");
            }
        }

        public override string ToString()
        {
            return "<native function>";
        }
    }

    public class LenFunction : SuironCallable
    {
        public int Arity()
        {
            return 1; // Takes one argument: the string
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            if (arguments[0] == null)
            {
                throw new RuntimeErrorException(null, "Cannot get length of nil.");
            }

            if (arguments[0] is string str)
            {
                return (double)str.Length; // Return length as a Suiron number (double)
            }
            else
            {
                throw new RuntimeErrorException(null, $"Argument to len must be a string. Got {arguments[0].GetType()}.");
            }
        }

        public override string ToString()
        {
            return "<native function>";
        }
    }

    public class ReadFileFunction : SuironCallable
    {
        public int Arity()
        {
            return 1; // Takes one argument: filepath (string)
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            if (arguments[0] == null)
            {
                throw new RuntimeErrorException(null, "File path cannot be nil for readFile.");
            }

            if (arguments[0] is string filePath)
            {
                try
                {
                    // Use File.ReadAllText to read the entire file
                    return File.ReadAllText(filePath);
                }
                catch (FileNotFoundException)
                {
                    throw new RuntimeErrorException(null, $"File not found: '{filePath}'.");
                }
                catch (IOException ex)
                {
                    throw new RuntimeErrorException(null, $"Error reading file '{filePath}': {ex.Message}");
                }
                catch (UnauthorizedAccessException)
                {
                    throw new RuntimeErrorException(null, $"Access to path '{filePath}' is denied.");
                }
            }
            else
            {
                throw new RuntimeErrorException(null, $"Argument to readFile must be a string (file path). Got {arguments[0].GetType()}.");
            }
        }

        public override string ToString()
        {
            return "<native function>";
        }
    }

    public class WriteFileFunction : SuironCallable
    {
        // Arity can be 2 or 3 because of the optional argument
        public int Arity()
        {
            return 2; // Minimum arguments: filepath, content
        }

        // You'll need to override the call method to handle variable arguments
        public object call(Interpreter interpreter, List<object> arguments)
        {
            // Basic argument count validation
            if (arguments.Count < 2 || arguments.Count > 3)
            {
                throw new RuntimeErrorException(null, $"writeFile() expects 2 or 3 arguments, but got {arguments.Count}.");
            }

            if (arguments[0] == null || !(arguments[0] is string filePath))
            {
                throw new RuntimeErrorException(null, "First argument to writeFile (file path) must be a string and not nil.");
            }

            if (arguments[1] == null || !(arguments[1] is string content))
            {
                throw new RuntimeErrorException(null, "Second argument to writeFile (content) must be a string and not nil.");
            }

            bool append = false; // Default value for append

            if (arguments.Count == 3)
            {
                if (arguments[2] == null)
                {
                    // If nil is passed for append, treat as false (or throw error if strict)
                    append = false;
                }
                else if (arguments[2] is bool boolAppend)
                {
                    append = boolAppend;
                }
                else
                {
                    throw new RuntimeErrorException(null, $"Third argument to writeFile (append) must be a boolean. Got {arguments[2].GetType()}.");
                }
            }

            try
            {
                if (append)
                {
                    File.AppendAllText(filePath, content);
                }
                else
                {
                    File.WriteAllText(filePath, content);
                }
                return null; // Suiron functions typically return nil if no explicit value is returned
            }
            catch (IOException ex)
            {
                throw new RuntimeErrorException(null, $"Error writing to file '{filePath}': {ex.Message}");
            }
            catch (UnauthorizedAccessException)
            {
                throw new RuntimeErrorException(null, $"Access to path '{filePath}' is denied.");
            }
        }

        public override string ToString()
        {
            return "<native function>";
        }
    }

    public class ExecuteCommandFunction : SuironCallable
    {
        // Arity can be 1 or 2
        public int Arity()
        {
            return 1; // Minimum arguments: command string
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            // Basic argument count validation
            if (arguments.Count < 1 || arguments.Count > 2)
            {
                throw new RuntimeErrorException(null, $"executeCommand() expects 1 or 2 arguments, but got {arguments.Count}.");
            }

            if (arguments[0] == null || !(arguments[0] is string command))
            {
                throw new RuntimeErrorException(null, "First argument to executeCommand (command string) must be a string and not nil.");
            }

            bool printOutput = false; // Default value for printOutput

            if (arguments.Count == 2)
            {
                if (arguments[1] == null)
                {
                    // If nil is passed for printOutput, treat as false
                    printOutput = false;
                }
                else if (arguments[1] is bool boolPrintOutput)
                {
                    printOutput = boolPrintOutput;
                }
                else
                {
                    throw new RuntimeErrorException(null, $"Second argument to executeCommand (print output) must be a boolean. Got {arguments[1].GetType()}.");
                }
            }

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "/bin/bash";
                startInfo.Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"/C \"{command}\"" : $"-c \"{command}\"";
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true; // Don't show command prompt window

                using (Process process = Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit(); // Wait for the command to finish

                    if (printOutput && !string.IsNullOrWhiteSpace(output))
                    {
                        Console.WriteLine(output);
                    }
                    if (printOutput && !string.IsNullOrWhiteSpace(error))
                    {
                        Console.Error.WriteLine(error);
                    }

                    // Return the standard output as a string (or nil if empty)
                    return string.IsNullOrEmpty(output) ? null : output;
                }
            }
            catch (Exception ex) // Catch general exceptions during process execution
            {
                throw new RuntimeErrorException(null, $"Error executing command '{command}': {ex.Message}");
            }
        }

        public override string ToString()
        {
            return "<native function>";
        }
    }

    public class RandomFunction : SuironCallable
    {
        private static readonly Random _random = new Random(); // Re-use Random instance for better randomness

        public int Arity()
        {
            return 2; // min, max
        }

        private bool IsInteger(double number)
        {
            return Math.Abs(number % 1) <= (Double.Epsilon * 100);
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            if (arguments[0] == null || arguments[1] == null)
            {
                throw new RuntimeErrorException(null, "Arguments to random() cannot be nil.");
            }

            if (!(arguments[0] is double minDouble) || !IsInteger(minDouble))
            {
                throw new RuntimeErrorException(null, "First argument to random() (min) must be an integer.");
            }
            if (!(arguments[1] is double maxDouble) || !IsInteger(maxDouble))
            {
                throw new RuntimeErrorException(null, "Second argument to random() (max) must be an integer.");
            }

            int min = Convert.ToInt32(minDouble);
            int max = Convert.ToInt32(maxDouble);

            if (min > max)
            {
                throw new RuntimeErrorException(null, "Min argument to random() cannot be greater than max argument.");
            }

            // _random.Next(minValue, maxValue) generates a random integer that is greater than or equal to minValue,
            // and less than maxValue. So we need to add 1 to max.
            return (double)_random.Next(min, max + 1);
        }

        public override string ToString()
        {
            return "<native function>";
        }
    }

    public class FileExistsFunction : SuironCallable
    {
        public int Arity()
        {
            return 1; // filepath
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            if (arguments[0] == null)
            {
                throw new RuntimeErrorException(null, "File path argument to fileExists() cannot be nil.");
            }

            if (!(arguments[0] is string filePath))
            {
                throw new RuntimeErrorException(null, "Argument to fileExists() must be a string (file path).");
            }

            try
            {
                return File.Exists(filePath);
            }
            catch (Exception ex) // Catch potential exceptions from File.Exists, though rare
            {
                throw new RuntimeErrorException(null, $"Error checking file existence for '{filePath}': {ex.Message}");
            }
        }

        public override string ToString()
        {
            return "<native function>";
        }
    }

    public class ToLowerFunction : SuironCallable
    {
        public int Arity()
        {
            return 1; // string
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            if (arguments[0] == null)
            {
                throw new RuntimeErrorException(null, "Argument to toLower() cannot be nil.");
            }

            if (!(arguments[0] is string str))
            {
                throw new RuntimeErrorException(null, "Argument to toLower() must be a string.");
            }

            return str.ToLowerInvariant(); // Using ToLowerInvariant for consistency
        }

        public override string ToString()
        {
            return "<native function>";
        }
    }

    public class ReplaceFunction : SuironCallable
    {
        public int Arity()
        {
            return 3; // inputString, stringToReplace, replacementString
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            if (arguments[0] == null || arguments[1] == null || arguments[2] == null)
            {
                throw new RuntimeErrorException(null, "Arguments to replace() cannot be nil.");
            }

            if (!(arguments[0] is string inputString))
            {
                throw new RuntimeErrorException(null, "First argument to replace() (inputString) must be a string.");
            }
            if (!(arguments[1] is string stringToReplace))
            {
                throw new RuntimeErrorException(null, "Second argument to replace() (stringToReplace) must be a string.");
            }
            if (!(arguments[2] is string replacementString))
            {
                throw new RuntimeErrorException(null, "Third argument to replace() (replacementString) must be a string.");
            }

            return inputString.Replace(stringToReplace, replacementString);
        }

        public override string ToString()
        {
            return "<native function>";
        }
    }
}
