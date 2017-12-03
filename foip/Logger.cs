using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foip
{
    public static class Logger
    {
        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        private static readonly Object lck = new Object();

        public static void WriteLine(string str)
        {
            lock (lck)
            {
                ClearCurrentConsoleLine();
                Console.WriteLine(str);
            }
        }

        public static void Write(string str)
        {
            lock (lck)
            {
                Console.Write(str);
            }
        }
    }
}
