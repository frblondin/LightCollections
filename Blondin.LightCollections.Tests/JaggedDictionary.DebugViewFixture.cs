using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;
using Ploeh.AutoFixture.Xunit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Blondin.LightCollections.Tests
{
    public class JaggedDictionaryDebugViewFixture
    {
        private readonly ITestOutputHelper _output;

        public JaggedDictionaryDebugViewFixture(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Theory, JaggedDictionaryFixture.CustomAutoData(4)]
        public void KeyCollection(JaggedDictionary<int, string> sut, KeyValuePair<JaggedIndex4<int>, string>[] values)
        {
            foreach (var kvp in values) sut[kvp.Key] = kvp.Value;

            var debugView = new JaggedDictionaryKeyCollectionDebugView<int, string>(sut.Keys);
            var items = debugView.Items;
            Assert.Equal(values.Select(kvp => kvp.Key).Distinct().Count(), items.Length);
        }

        [Theory, JaggedDictionaryFixture.CustomAutoData(4)]
        public void ValueCollection(JaggedDictionary<int, string> sut, KeyValuePair<JaggedIndex4<int>, string>[] values)
        {
            foreach (var kvp in values) sut[kvp.Key] = kvp.Value;

            var debugView = new JaggedDictionaryValueCollectionDebugView<int, string>(sut.Values);
            var items = debugView.Items;
            Assert.Equal(values.Distinct().Count(), items.Length);
        }
    }
}
