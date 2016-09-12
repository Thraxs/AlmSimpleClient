using System;
using System.IO;

namespace ALMSimpleClient.IO
{
    static class Logger
    {
        private static readonly string ExceptionsFile =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\ALM Simple Client\\exception.log";

        public static void LogException(Exception exception)
        {
            var contents = new[] {exception.Message, exception.StackTrace};
            File.WriteAllLines(ExceptionsFile, contents);
        }
    }
}
