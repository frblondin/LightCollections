using Ploeh.AutoFixture.Xunit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Blondin.LightCollections.Tests
{
    public class NoLohInfoProviderFixture
    {
        public struct FakeStructure
        {
            public int Field;
#pragma warning disable CS0169
            private OtherStructure OtherField;
#pragma warning restore CS0169
        }
        public struct OtherStructure
        {
            public int Field;
        }

        [Fact]
        public void ElementSizeInBytesForStructure()
        {
            Assert.Equal(8, NoLohInfoProvider<FakeStructure>.ElementSizeInBytes);
        }

        [Fact]
        public void ElementSizeInBytesForReference()
        {
            Assert.Equal(4, NoLohInfoProvider<object>.ElementSizeInBytes);
        }

        [Fact]
        public void MaxArrayElementCount()
        {
            Assert.Equal(NoLohInfoProvider.LohStartSize / NoLohInfoProvider<FakeStructure>.ElementSizeInBytes - 1, NoLohInfoProvider<FakeStructure>.MaxChunkElementCount);
        }

        [Fact]
        public void Arrays()
        {
            Assert.True(NoLohInfoProvider<FakeStructure>.ProgressiveArraySize.Max(i => i.Size) < NoLohInfoProvider<FakeStructure>.MaxChunkElementCount);
        }

        [Fact]
        public void SmallIndexChunkInfo()
        {
            //var info = NoLohInfoProvider<FakeStructure>.GetChunkAndIndexInChunk(20);
            //Assert.Equal(1, info.Chunk);
            //Assert.Equal(4, info.IndexInChunk);
        }

        [Fact]
        public void LargeIndexChunkInfo()
        {
            //var info = NoLohInfoProvider<FakeStructure>.GetChunkAndIndexInChunk(20000000);
            //Assert.Equal(1890, info.Chunk);
            //Assert.Equal(10512, info.IndexInChunk);
        }
    }
}
