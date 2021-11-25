using System;

namespace DevOpsDeploy
{
    class Log
    {
        internal void WriteLine(string text)
        {
            Console.WriteLine(text);
        }

        internal void WriteError(string text)
        {
            Console.WriteLine($"ERROR: {text}");
        }
    }
}
