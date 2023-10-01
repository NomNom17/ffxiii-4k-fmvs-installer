using System;

namespace WDBtool
{
    internal class CmnMethods
    {
        public static void ErrorExit(string errorMsg)
        {
            Console.WriteLine(errorMsg);
            Console.ReadLine();
            Environment.Exit(0);
        }

        public static string RecordsList = "!!WDB_Records";
    }
}