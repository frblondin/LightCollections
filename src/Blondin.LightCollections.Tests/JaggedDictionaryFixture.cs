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
    public class JaggedDictionaryFixture
    {
        #region Autofixture customization
        internal class CustomAutoDataAttribute : AutoDataAttribute
        {
            private class Customization : ICustomization
            {
                private int _depth;

                public Customization(int depth)
                {
                    _depth = depth;
                }

                public void Customize(IFixture fixture)
                {
                    fixture.Register<JaggedDictionary<int, string>>(() => new JaggedDictionary<int, string>(_depth, dictionaryFactory: SortedDictionaryFactory<int>.Default));
                }
            }
            public CustomAutoDataAttribute(int depth) : base(new Fixture { RepeatCount = 50 }.Customize(new Customization(depth))) { }
        }
        #endregion

        private readonly ITestOutputHelper _output;

        public JaggedDictionaryFixture(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Theory, CustomAutoData(1)]
        public void SetAndGetIndexedCoordinates_1Dimension(JaggedDictionary<int, string> sut, JaggedIndex1<int> key, string value)
        {
            sut[key] = value;
            Assert.Equal(value, sut[key]);
        }

        [Theory, CustomAutoData(1)]
        public void Add_1Dimension(JaggedDictionary<int, string> sut, JaggedIndex1<int> key, string value)
        {
            sut.Add(key, value);
            Assert.Equal(value, sut[key]);
            Assert.Equal(1, sut.Count);
        }

        [Theory, CustomAutoData(1)]
        public void NotContainsKey_1Dimension(JaggedDictionary<int, string> sut, JaggedIndex1<int> key, string value)
        {
            Assert.False(sut.ContainsKey(key));
        }

        [Theory, CustomAutoData(1)]
        public void AddAndContainsKey_1Dimension(JaggedDictionary<int, string> sut, JaggedIndex1<int> key, string value)
        {
            sut.Add(key, value);
            Assert.True(sut.ContainsKey(key));
        }

        [Theory, CustomAutoData(1)]
        public void NotTryGetValue_1Dimension(JaggedDictionary<int, string> sut, JaggedIndex1<int> key, string value)
        {
            string val;
            Assert.False(sut.TryGetValue(key, out val));
        }

        [Theory, CustomAutoData(1)]
        public void AddAndTryGetValue_1Dimension(JaggedDictionary<int, string> sut, JaggedIndex1<int> key, string value)
        {
            sut.Add(key, value);
            string val;
            Assert.True(sut.TryGetValue(key, out val));
            Assert.Equal(value, val);
        }

        [Theory, CustomAutoData(1)]
        public void NotRemove_1Dimension(JaggedDictionary<int, string> sut, JaggedIndex1<int> key, string value)
        {
            Assert.False(sut.Remove(key));
        }

        [Theory, CustomAutoData(1)]
        public void AddAndRemove_1Dimension(JaggedDictionary<int, string> sut, JaggedIndex1<int> key, string value)
        {
            sut.Add(key, value);
            Assert.True(sut.Remove(key));
            Assert.False(sut.ContainsKey(key));
            Assert.Equal(0, sut.Count);
        }

        [Theory, CustomAutoData(1)]
        public void Clear_1Dimension(JaggedDictionary<int, string> sut, JaggedIndex1<int> key, string value)
        {
            sut.Clear();
            Assert.Equal(0, sut.Count);
        }

        [Theory, CustomAutoData(1)]
        public void AddAndClear_1Dimension(JaggedDictionary<int, string> sut, JaggedIndex1<int> key, string value)
        {
            sut.Add(key, value);
            sut.Clear();
            Assert.Equal(0, sut.Count);
        }

        [Theory, CustomAutoData(1)]
        public void SetAndGetIndexedArrayKey_1Dimension(JaggedDictionary<int, string> sut, JaggedIndex1<int> key, string value)
        {
            sut[key.GetValues()] = value;
            Assert.Equal(value, sut[key.GetValues()]);
        }

        [Theory, CustomAutoData(1)]
        public void SetAndGetIndexedIListKey_1Dimension(JaggedDictionary<int, string> sut, JaggedIndex1<int> key, string value)
        {
            sut[(IList<int>)key.GetValues()] = value;
            Assert.Equal(value, sut[(IList<int>)key.GetValues()]);
        }

        [Theory, CustomAutoData(1)]
        public void SetAndGetIndexedCoordinates_5Dimension(JaggedDictionary<int, string> sut, JaggedIndex5<int> key, string value)
        {
            sut[key] = value;
            Assert.Equal(value, sut[key]);
        }

        [Theory, CustomAutoData(1)]
        public void SetAndGetIndexedArrayKey_5Dimension(JaggedDictionary<int, string> sut, JaggedIndex5<int> key, string value)
        {
            sut[key.GetValues()] = value;
            Assert.Equal(value, sut[key.GetValues()]);
        }

        [Theory, CustomAutoData(1)]
        public void SetAndGetIndexedIListKey_5Dimension(JaggedDictionary<int, string> sut, JaggedIndex5<int> key, string value)
        {
            sut[(IList<int>)key.GetValues()] = value;
            Assert.Equal(value, sut[(IList<int>)key.GetValues()]);
        }

        [Theory, CustomAutoData(1)]
        public void Add_5Dimension(JaggedDictionary<int, string> sut, JaggedIndex5<int> key, string value)
        {
            sut.Add(key, value);
            Assert.Equal(value, sut[key]);
            Assert.Equal(1, sut.Count);
        }

        [Theory, CustomAutoData(1)]
        public void NotContainsKey_5Dimension(JaggedDictionary<int, string> sut, JaggedIndex5<int> key, string value)
        {
            Assert.False(sut.ContainsKey(key));
        }

        [Theory, CustomAutoData(1)]
        public void AddAndContainsKey_5Dimension(JaggedDictionary<int, string> sut, JaggedIndex5<int> key, string value)
        {
            sut.Add(key, value);
            Assert.True(sut.ContainsKey(key));
        }

        [Theory, CustomAutoData(1)]
        public void NotTryGetValue_5Dimension(JaggedDictionary<int, string> sut, JaggedIndex5<int> key, string value)
        {
            string val;
            Assert.False(sut.TryGetValue(key, out val));
        }

        [Theory, CustomAutoData(1)]
        public void AddAndTryGetValue_5Dimension(JaggedDictionary<int, string> sut, JaggedIndex5<int> key, string value)
        {
            sut.Add(key, value);
            string val;
            Assert.True(sut.TryGetValue(key, out val));
            Assert.Equal(value, val);
        }

        [Theory, CustomAutoData(1)]
        public void NotRemove_5Dimension(JaggedDictionary<int, string> sut, JaggedIndex5<int> key, string value)
        {
            Assert.False(sut.Remove(key));
        }

        [Theory, CustomAutoData(1)]
        public void AddAndRemove_5Dimension(JaggedDictionary<int, string> sut, JaggedIndex5<int> key, string value)
        {
            sut.Add(key, value);
            Assert.True(sut.Remove(key));
            Assert.False(sut.ContainsKey(key));
            Assert.Equal(0, sut.Count);
        }

        [Theory, CustomAutoData(1)]
        public void Clear_5Dimension(JaggedDictionary<int, string> sut, JaggedIndex5<int> key, string value)
        {
            sut.Clear();
            Assert.Equal(0, sut.Count);
        }

        [Theory, CustomAutoData(1)]
        public void AddAndClear_5Dimension(JaggedDictionary<int, string> sut, JaggedIndex5<int> key, string value)
        {
            sut.Add(key, value);
            sut.Clear();
            Assert.Equal(0, sut.Count);
        }

        [Theory, CustomAutoData(4)]
        public void Enumerate(JaggedDictionary<int, string> sut, JaggedIndex4<int>[] keys, string value)
        {
            foreach (var key in keys) sut[key] = value;

            var keyValuePairs = sut.ToList();
            Assert.Equal(keys.Length, keyValuePairs.Count);
            foreach (var key in keys)
                Assert.Contains(key, keyValuePairs.Select(kvp => kvp.Key));
        }
    }
}
