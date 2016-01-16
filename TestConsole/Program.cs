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
            int count = 10000000;
            BenchImpl(new Blondin.LightCollections.Dictionary<string, int>(), count);
            BenchImpl(new LightDictionary<string, int>(), count);
            //BenchImpl(new Blondin.LightCollections.Dictionary<int, string>(), count);
            //BenchImpl(new LightDictionary<int, string>(), count);
        }

        private static void BenchImpl(IDictionary<string, int> dictionary, int count)
        {
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
                dictionary[i.ToString()] = i;
            int ignored;
            for (int i = 0; i < count; i++)
                ignored = dictionary[i.ToString()];
            Console.WriteLine(string.Format("Time to process with dictionary of type '{0}': {1}.",
                dictionary.GetType(),
                watch.Elapsed));
        }
    }
}
