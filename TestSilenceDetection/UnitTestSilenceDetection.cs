using System;
using Xunit;
using SilenceDetection;

namespace TestSilenceDetection
{
    public class UnitTestSilenceDetection
    {
        [Fact]
        public void TestSingleSilenceInSmallArrayBytePerSample1()
        {
            Span<byte> toTest = stackalloc byte[4] { 0x78, 0x78, 0x80, 0x78 };
            int sampleRate = 4;
            int channels = 1;
            int bitsPerSaple = 8;

            var silence = toTest.GetSilence(sampleRate, channels, bitsPerSaple / 8, new TimeSpan(0, 0, 0, 0, 200), -40);

            Assert.Equal(2, silence.IndexStart);
            Assert.Equal(2, silence.IndexEnd);
            Assert.Equal(500, silence.Start.TotalMilliseconds);
            Assert.Equal(250, silence.Duration.TotalMilliseconds);
        }

        [Fact]
        public void TestSingleSilenceInSmallArray2Channels()
        {
            byte[] toTest = new byte[8] { 0x78, 0x78, 0x78, 0x78, 0x80, 0x80, 0x78, 0x78 };
            int sampleRate = 4;
            int channels = 2;
            int bitsPerSaple = 8;

            var silence = toTest.GetSilence(sampleRate, channels, bitsPerSaple / 8, new TimeSpan(0, 0, 0, 0, 200), -40);
            
            Assert.Equal(4, silence.IndexStart);
            Assert.Equal(5, silence.IndexEnd);
            Assert.Equal(500, silence.Start.TotalMilliseconds);
            Assert.Equal(250, silence.Duration.TotalMilliseconds);
        }
        
        [Fact]
        public void TestSingleSilenceInLargeArrayBytePerSample2()
        {
            byte[] toTest = new byte[16] { 0x78, 0x78, 0x88, 0x78, 0x00, 0x00, 0x78, 0x78, 0x78, 0x78, 0x88, 0x78, 0x00, 0x00, 0x78, 0x78 };
            int sampleRate = 4;
            int channels = 1;
            int bitsPerSaple = 16;

            var silence = toTest.GetAllSilences(sampleRate, channels, bitsPerSaple / 8, new TimeSpan(0, 0, 0, 0, 200), -40);

            Assert.Equal(2, silence.Count);
            Assert.Equal(4, silence[0].IndexStart);
            Assert.Equal(5, silence[0].IndexEnd);
            Assert.Equal(500, silence[0].Start.TotalMilliseconds);
            Assert.Equal(250, silence[0].Duration.TotalMilliseconds);
            Assert.Equal(12, silence[1].IndexStart);
            Assert.Equal(13, silence[1].IndexEnd);
            Assert.Equal(1500, silence[1].Start.TotalMilliseconds);
            Assert.Equal(250, silence[1].Duration.TotalMilliseconds);
        }

        [Fact]
        public void TestSingleSilenceInSmallArrayBytePerSample2()
        {
            byte[] toTest = new byte[8] { 0x78, 0x78, 0x88, 0x78, 0x00, 0x00, 0x78, 0x78 };
            int sampleRate = 4;
            int channels = 1;
            int bitsPerSaple = 16;

            var silence = toTest.GetSilence(sampleRate, channels, bitsPerSaple / 8, new TimeSpan(0, 0, 0, 0, 200), -40);

            Assert.Equal(4, silence.IndexStart);
            Assert.Equal(5, silence.IndexEnd);
            Assert.Equal(500, silence.Start.TotalMilliseconds);
            Assert.Equal(250, silence.Duration.TotalMilliseconds);
        }

        [Fact]
        public void TestSingleSilenceInSmallArrayBytePerSample4()
        {
            byte[] toTest = new byte[16] { 0x78, 0x78, 0x88, 0x78, 0x78, 0x78, 0x88, 0x78, 0x00, 0x00, 0x00, 0x00, 0x78, 0x78, 0x88, 0x78 };
            int sampleRate = 4;
            int channels = 1;
            int bitsPerSaple = 32;

            var silence = toTest.GetSilence(sampleRate, channels, bitsPerSaple / 8, new TimeSpan(0, 0, 0, 0, 200), -40);

            Assert.Equal(8, silence.IndexStart);
            Assert.Equal(11, silence.IndexEnd);
            Assert.Equal(500, silence.Start.TotalMilliseconds);
            Assert.Equal(250, silence.Duration.TotalMilliseconds);
        }

        [Fact]
        public void TestSingleSilenceInLargerArrayBytePerSample1()
        {
            byte[] toTest = new byte[8] { 0x70, 0x70, 0x80, 0x70, 0x70, 0x70, 0x80, 0x70 };
            int sampleRate = 4;
            int channels = 1;
            int bitsPerSaple = 8;

            var silence = toTest.GetAllSilences(sampleRate, channels, bitsPerSaple / 8, new TimeSpan(0, 0, 0, 0, 200), -40);

            Assert.Equal(2, silence.Count);
            Assert.Equal(2, silence[0].IndexStart);
            Assert.Equal(2, silence[0].IndexEnd);
            Assert.Equal(500, silence[0].Start.TotalMilliseconds);
            Assert.Equal(250, silence[0].Duration.TotalMilliseconds);
            Assert.Equal(6, silence[1].IndexStart);
            Assert.Equal(6, silence[1].IndexEnd);
            Assert.Equal(1500, silence[1].Start.TotalMilliseconds);
            Assert.Equal(250, silence[1].Duration.TotalMilliseconds);
        }


    }
}
