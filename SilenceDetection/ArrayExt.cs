using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace SilenceDetection
{
    /// <summary>
    /// Extension for byte array to be used with WAVE raw files to detect silences
    /// </summary>
    public static class ArrayExt
    {
        /// <summary>
        /// Check if there is a silence. Based on db calculation
        /// dB = 20 * log10(amplitude)
        /// </summary>
        /// <param name="amplitude">amplitude = value / sampling rate (so a -1.0 to 1.0 value)</param>
        /// <param name="threshold">the value under which it is a silence in db</param>
        /// <returns>True if a silence is detected</returns>
        public static bool IsSilence(float amplitude, int threshold)
        {
            if (amplitude == 0)
                return true;
            double dB = 20 * Math.Log10(Math.Abs(amplitude));
            return dB < threshold;
        }

        /// <summary>
        /// Get the silence duration on an raw WAVE byte array
        /// </summary>
        /// <param name="reader">The byte array</param>
        /// <param name="sampleRate">The wave sample rate</param>
        /// <param name="channels">The number of channel</param>
        /// <param name="bytePerSample">must be either 1, 2 or 4 (so 8, 16 or 32 bits)</param>
        /// <param name="minSilence">The minimum threshold for silence detection, anything lower than this value will be not 
        /// considered as silence except at the end of the buffer to allow adding with any previous silence detected.</param>
        /// <param name="silenceThreshold"></param>
        /// <returns>A tupple with the start of the silence and length of the silence</returns>
        /// <remarks>The silence needs to be on both channel to be considered as silence</remarks>
        public static (TimeSpan start, TimeSpan duration, int indexStart, int indexCount) GetSilenceDuration(this byte[] reader,
                                                  int sampleRate, int channels, int bytePerSample, TimeSpan minSilence,
                                                  int silenceThreshold = -40)
        {
            return reader.AsSpan().GetSilenceDuration(sampleRate, channels, bytePerSample, minSilence, silenceThreshold);
        }

        /// <summary>
        /// Get the silence duration on an raw WAVE byte array
        /// </summary>
        /// <param name="reader">The Span of byte</param>
        /// <param name="sampleRate">The wave sample rate</param>
        /// <param name="channels">The number of channel</param>
        /// <param name="bytePerSample">must be either 1, 2 or 4 (so 8, 16 or 32 bits)</param>
        /// <param name="minSilence">The minimum threshold for silence detection, anything lower than this value will be not 
        /// considered as silence except at the end of the buffer to allow adding with any previous silence detected.</param>
        /// <param name="silenceThreshold"></param>
        /// <returns>A tupple with the start of the silence and length of the silence</returns>
        /// <remarks>The silence needs to be on both channel to be considered as silence</remarks>
        public static (TimeSpan start, TimeSpan duration, int indexStart, int indexCount) GetSilenceDuration(this Span<byte> reader,
                                                  int sampleRate, int channels, int bytePerSample, TimeSpan minSilence,
                                                  int silenceThreshold = -40)
        {
            int counterStart = -1;
            int counterLength = 0;
            int countMinSilence = (int)(minSilence.TotalMilliseconds * channels * sampleRate / (1000.0 * bytePerSample));
            float floatThreshold = (float)(Math.Pow(10.0, silenceThreshold / 20.0) * Math.Pow(2, bytePerSample * 8));

            for (int n = 0; n < reader.Length / bytePerSample; n++)
            {
                float toCheck = 0;
                // Little endian conversion
                if (bytePerSample == 1)
                {
                    toCheck = reader[n] - 128;
                }
                else if (bytePerSample == 2)
                {
                    // Do not use stackalloc otherwise, you'll quickly go into a stackoverflow
                    Span<byte> toConvert = new byte[2] { reader[n * bytePerSample], reader[n * bytePerSample + 1] };
                    toCheck = BinaryPrimitives.ReadInt16LittleEndian(toConvert);
                }
                else if (bytePerSample == 4)
                {
                    // Do not use stackalloc otherwise, you'll quickly go into a stackoverflow
                    Span<byte> toConvert = new byte[4] {
                        reader[n * bytePerSample],
                        reader[n * bytePerSample + 1],
                        reader[n * bytePerSample + 2],
                        reader[n * bytePerSample + 3]
                    };
                    toCheck = BinaryPrimitives.ReadInt32LittleEndian(toConvert);
                }

                if (Math.Abs(toCheck) < floatThreshold)
                {
                    if (counterStart == -1)
                        counterStart = n;
                    counterLength++;
                }
                else
                {
                    if (counterStart != -1)
                    {
                        if (counterLength >= countMinSilence)
                            break;
                        else
                        {
                            counterStart = -1;
                            counterLength = 0;
                        }
                    }
                }
            }


            if ((counterStart == -1))
                return (TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1), -1, -1);

            double silenceStart = counterStart * 1000.0 / (sampleRate * channels);
            double silenceDuration = counterLength * 1000.0 / (sampleRate * channels);

            return (TimeSpan.FromMilliseconds(silenceStart), TimeSpan.FromMilliseconds(silenceDuration), counterStart * bytePerSample, counterLength * bytePerSample);
        }


        /// <summary>
        /// Get the silence duration on an raw WAVE byte array
        /// </summary>
        /// <param name="reader">The byte array</param>
        /// <param name="sampleRate">The wave sample rate</param>
        /// <param name="channels">The number of channel</param>
        /// <param name="bytePerSample">must be either 1, 2 or 4 (so 8, 16 or 32 bits)</param>
        /// <param name="minSilence">The minimum threshold for silence detection, anything lower than this value will be not 
        /// considered as silence except at the end of the buffer to allow adding with any previous silence detected.</param>
        /// <param name="silenceThreshold"></param>
        /// <returns>A Silence class</returns>
        /// <remarks>The silence needs to be on both channel to be considered as silence</remarks>
        public static Silence GetSilence(this byte[] reader,
                                                  int sampleRate, int channels, int bytePerSample, TimeSpan minSilence,
                                                  int silenceThreshold = -40)
        {
            return reader.AsSpan().GetSilence(sampleRate, channels, bytePerSample, minSilence, silenceThreshold);
        }

        /// <summary>
        /// Get the silence duration on an raw WAVE byte array
        /// </summary>
        /// <param name="reader">a Span of byte</param>
        /// <param name="sampleRate">The wave sample rate</param>
        /// <param name="channels">The number of channel</param>
        /// <param name="bytePerSample">must be either 1, 2 or 4 (so 8, 16 or 32 bits)</param>
        /// <param name="minSilence">The minimum threshold for silence detection, anything lower than this value will be not 
        /// considered as silence except at the end of the buffer to allow adding with any previous silence detected.</param>
        /// <param name="silenceThreshold"></param>
        /// <returns>A Silence class</returns>
        /// <remarks>The silence needs to be on both channel to be considered as silence</remarks>
        public static Silence GetSilence(this Span<byte> reader,
                                                  int sampleRate, int channels, int bytePerSample, TimeSpan minSilence,
                                                  int silenceThreshold = -40)
        {
            var t = reader.GetSilenceDuration(sampleRate, channels, bytePerSample, minSilence, silenceThreshold);
            return new Silence() { Start = t.start, Duration = t.duration, IndexStart = t.indexStart, IndexEnd = t.indexStart + t.indexCount - 1 };
        }

        /// <summary>
        /// Get all the silences in a specific raw byte array in PCM form
        /// </summary>
        /// <param name="reader">The byte array</param>
        /// <param name="sampleRate">The wave sample rate</param>
        /// <param name="channels">The number of channel</param>
        /// <param name="bytePerSample">must be either 1, 2 or 4 (so 8, 16 or 32 bits)</param>
        /// <param name="minSilence">The minimum threshold for silence detection, anything lower than this value will be not 
        /// considered as silence except at the end of the buffer to allow adding with any previous silence detected.</param>
        /// <param name="silenceThreshold"></param>
        /// <returns>A list of Silence containing all the silences meeting the criteria</returns>
        /// <remarks>The silence needs to be on both channel to be considered as silence</remarks>
        public static List<Silence> GetAllSilences(this byte[] reader,
                                                  int sampleRate, int channels, int bytePerSample, TimeSpan minSilence,
                                                  int silenceThreshold = -40)
        {
            return reader.AsSpan().GetAllSilences(sampleRate, channels, bytePerSample, minSilence, silenceThreshold);
        }

        /// <summary>
        /// Get all the silences in a specific raw byte array in PCM form
        /// </summary>
        /// <param name="reader">The Span of byte</param>
        /// <param name="sampleRate">The wave sample rate</param>
        /// <param name="channels">The number of channel</param>
        /// <param name="bytePerSample">must be either 1, 2 or 4 (so 8, 16 or 32 bits)</param>
        /// <param name="minSilence">The minimum threshold for silence detection, anything lower than this value will be not 
        /// considered as silence except at the end of the buffer to allow adding with any previous silence detected.</param>
        /// <param name="silenceThreshold"></param>
        /// <returns>A list of Silence containing all the silences meeting the criteria</returns>
        /// <remarks>The silence needs to be on both channel to be considered as silence</remarks>
        public static List<Silence> GetAllSilences(this Span<byte> reader,
                                                  int sampleRate, int channels, int bytePerSample, TimeSpan minSilence,
                                                  int silenceThreshold = -40)
        {
            List<Silence> silences = new List<Silence>();
            int slicer = 0;
            TimeSpan timeSinceStart = TimeSpan.Zero;
            do
            {
                var t = reader.Slice(slicer).GetSilenceDuration(sampleRate, channels, bytePerSample, minSilence, silenceThreshold);
                // We don't have any silence
                if (t.start.TotalMilliseconds < 0)
                    break;

                timeSinceStart += t.start;

                Silence silence = new Silence()
                {
                    Start = timeSinceStart,
                    Duration = t.duration,
                    IndexStart = slicer + t.indexStart,
                    IndexEnd = slicer + t.indexStart + t.indexCount - 1
                };

                timeSinceStart += t.duration;

                silences.Add(silence);
                slicer += t.indexStart + t.indexCount;
            } while (slicer < reader.Length);
            return silences;
        }
    }
}
