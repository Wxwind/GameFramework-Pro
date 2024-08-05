using System;

namespace Tool
{
    public class Log
    {
        public static void Trace(string message)
        {
            Console.WriteLine(message);
        }

        public static void Warning(string message)
        {
            lock (Console.Error)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Error.WriteLine(message);
                Console.ForegroundColor = oldColor;
            }
        }

        public static void Info(string message)
        {
            Console.WriteLine(message);
        }

        public static void Debug(string message)
        {
            Console.WriteLine(message);
        }

        public static void Error(string message)
        {
            lock (Console.Error)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(message);
                Console.ForegroundColor = oldColor;
            }
        }

        public static void Error(Exception e)
        {
            Error(e.ToString());
        }

        public static void Trace(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            Trace(message.ToStringAndClear());
        }

        public static void Warning(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            Warning(message.ToStringAndClear());
        }

        public static void Info(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            Info(message.ToStringAndClear());
        }

        public static void Debug(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            Debug(message.ToStringAndClear());
        }

        public static void Error(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            Error(message.ToStringAndClear());
        }
    }
}