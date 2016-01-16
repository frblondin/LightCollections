using Ploeh.AutoFixture.Xunit2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Blondin.LightCollections.Tests
{
    public class LightDictionaryFixture
    {
        private readonly ITestOutputHelper _output;

        public LightDictionaryFixture(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Theory, AutoData]
        public void SingleValue(LightDictionary<int, string> sut, int key, string value)
        {
            sut[key] = value;
            Assert.Equal(value, sut[key]);
        }

        [Theory, AutoData]
        public void ManyValues(LightDictionary<int, string> sut)
        {
            for (int i = 0; i < 100000; i++)
                sut[i] = i.ToString();
            for (int i = 0; i < 100000; i++)
                Assert.Equal(i.ToString(), sut[i]);
        }

        [Fact]
        public void Bench()
        {
            int count = 10000000;
            BenchImpl(new Dictionary<int, string>(), count);
            BenchImpl(new LightDictionary<int, string>(), count);
            BenchImpl(new Dictionary<int, string>(), count);
            BenchImpl(new LightDictionary<int, string>(), count);
        }

        private void BenchImpl(IDictionary<int, string> dictionary, int count)
        {
            dictionary[0] = "0"; // Force JIT

            var watch = Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
                dictionary[i] = i.ToString();
            string ignored;
            for (int i = 0; i < count; i++)
                ignored = dictionary[i];
            _output.WriteLine(string.Format("Time to process with dictionary of type '{0}': {1}.",
                dictionary.GetType(),
                watch.Elapsed));
        }
    }
}
