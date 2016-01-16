using Blondin.LightCollections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            int count = 1000000;
            //BenchImpl(new Dictionary<int, string>(), count);
            BenchImpl(new LightDictionary<int, string>(), count);
            //BenchImpl(new Dictionary<int, string>(), count);
            //BenchImpl(new LightDictionary<int, string>(), count);
        }

        private static void BenchImpl(IDictionary<int, string> dictionary, int count)
        {
            dictionary[0] = "0"; // Force JIT

            var watch = Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
                dictionary[i] = i.ToString();
            string ignored;
            for (int i = 0; i < count; i++)
                ignored = dictionary[i];
            Console.WriteLine(string.Format("Time to process with dictionary of type '{0}': {1}.",
                dictionary.GetType(),
                watch.Elapsed));
        }
    }
}
