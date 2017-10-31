using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace foip
{
    public static class Extensions
    {
        public static UInt32 ToUInt32(this IPAddress ip)
        {
            byte[] fromIPBytes = ip.GetAddressBytes();
            Array.Reverse(fromIPBytes);
            UInt32 result = BitConverter.ToUInt32(fromIPBytes, 0);
            return result;
        }

        public static IPAddress ToIPAddress(this UInt32 val)
        {
            byte[] bytes = BitConverter.GetBytes(val);
            Array.Reverse(bytes); // flip little-endian to big-endian(network order)
            var result = new IPAddress(bytes);
            return result;
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            T[] elements = source.ToArray();
            for (int i = elements.Length - 1; i >= 0; i--)
            {
                // Swap element "i" with a random earlier element it (or itself)
                // ... except we don't really need to swap it fully, as we can
                // return it immediately, and afterwards it's irrelevant.
                int swapIndex = rng.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }
        }

        private enum TimeSpanElement
        {
            Millisecond,
            Second,
            Minute,
            Hour,
            Day
        }

        public static string ToFriendlyDisplay(this TimeSpan timeSpan, int maxNrOfElements)
        {
            maxNrOfElements = Math.Max(Math.Min(maxNrOfElements, 5), 1);
            var parts = new[]
                            {
                            Tuple.Create(TimeSpanElement.Day, timeSpan.Days),
                            Tuple.Create(TimeSpanElement.Hour, timeSpan.Hours),
                            Tuple.Create(TimeSpanElement.Minute, timeSpan.Minutes),
                            Tuple.Create(TimeSpanElement.Second, timeSpan.Seconds),
                            Tuple.Create(TimeSpanElement.Millisecond, timeSpan.Milliseconds)
                        }
                                        .SkipWhile(i => i.Item2 <= 0)
                                        .Take(maxNrOfElements);

            return string.Join(", ", parts.Select(p => string.Format("{0} {1}{2}", p.Item2, p.Item1, p.Item2 > 1 ? "s" : string.Empty)));
        }
    }
}
