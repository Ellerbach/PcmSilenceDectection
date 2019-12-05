using NAudio.Wave;
using System;
using System.IO;
using SilenceDetection;

namespace PcmSilenceDetection
{
    class Program
    {
        static void Main(string[] args)
        {
            const string fileName = "Test.wav";
            Console.WriteLine("Hello silence detection!");
            // Check it is a valid raw WAVE file
            // We will use NAudio for this and just read the header
            var fileTest = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            WaveFileReader reader = new WaveFileReader(fileTest);
            if (reader.WaveFormat.Encoding != WaveFormatEncoding.Pcm)
                throw new Exception($"This sample supports only PCM raw WAVE format");

            Console.WriteLine($"SampleRate: {reader.WaveFormat.SampleRate}, Channels: {reader.WaveFormat.Channels}, BitsPerSample: {reader.WaveFormat.BitsPerSample}");
            Console.WriteLine($"Buffer length: {reader.Length}");
            
            // Create a buffer to read everything
            Span<byte> toFindSilence = new byte[reader.Length];
            reader.Read(toFindSilence);
            // Now detect silences
            var slicers = toFindSilence.GetAllSilences(reader.WaveFormat.SampleRate, reader.WaveFormat.Channels, reader.WaveFormat.BitsPerSample / 8, new TimeSpan(0, 0, 0, 0, 500), -40);
            
            foreach (var slice in slicers)
            {
                Console.WriteLine($"Start: {slice.Start.TotalMilliseconds} ms, duration: {slice.Duration.TotalMilliseconds} ms, index start: {slice.IndexStart}, index end: {slice.IndexEnd}");
            }


        }
    }
}
