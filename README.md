# Silence detection for WAVE raw buffer

This project can be used to detect silences with a specific threshold both in terms of minimum silence duration and in terms of db.

It is a byte array extention which makes it easy when manipulating raw wave files. 

That can be useful to send only voice to recognize to Azure Speech Services for example and skip the silences to optimize the cost.

Another usage is to split a long speech into smaller part between words.

## Usage

The [PcmSilenceDetection](./PcmSilenceDetection) project gives an example of how to use it:

```csharp
// In this case, toFindSilence is a raw byte array containing the sound to analyze
// the minimum silence detection is 500 milliseconds
// the threshold for silence is -40 db
var slicers = toFindSilence.GetAllSilences(reader.WaveFormat.SampleRate, reader.WaveFormat.Channels, reader.WaveFormat.BitsPerSample / 8, new TimeSpan(0, 0, 0, 0, 500), -40);
            
foreach (var slice in slicers)
{
    Console.WriteLine($"Start: {slice.Start.TotalMilliseconds} ms, duration: {slice.Duration.TotalMilliseconds} ms, index start: {slice.IndexStart}, index end: {slice.IndexEnd}");
}
```

The silence detection is based on ```Span<byte>``` and ```byte[]``` extentions. It does allow to detect silences based out of decibel threshold and a minimum duration for a silence.

## Supported formats

The supported formats are raw byte arrays of PCM Wave format. They can be presented as ```Span<byte>``` or ```byte[]```. The representation follows the traditional Wave format specification.

Supported bits per sample are 8, 16 and 32, so 1, 2 and 4 bytes per sample. Number of channels starts at 1. There It starts to work with a sample rate of 1 byte per second and there is virtually no maximum.

In case of multiple bytes per sample, the encoding is int little endian. The case of 1 byte per sample is different, the data is coded as a non signed byte. All decoding for the 3 cases is implemented according to the specification.

To detect silences, it is necessary to have silence on all the channels at the same time. 

The minimum length of a silence is expressed with a ```TimeSpan```, internally, the math is done with milliseconds, so you can't detect a silence which is less than 1 millisecond.

The detection of silence is based out of the following algorythm: 

```csharp
public static bool IsSilence(float amplitude, sbyte threshold)
{
    if (amplitude == 0)
        return true;
    double dB = 20 * Math.Log10(Math.Abs(amplitude));
    return dB < threshold;
}
```

Where:
* ```amplitude``` is a float between -1 and +&, so it's a normalized view of the signal.
* ```threshold``` is the threshold in decibel which is a negative number

In order to have less math when running this algorythm on a large amount of data, the math is done on the raw data themselves:

```csharp
// This is done once
float floatThreshold = (float)(Math.Pow(10.0, silenceThreshold / 20.0) * Math.Pow(2, bytePerSample * 8));

// toCheck is the raw data you want to check
if (Math.Abs(toCheck) < floatThreshold)
{
    // Silence detected
}
```

This is optimized as you'll do only the math for the threshold once and compare it with the raw data.

## Tests

Simple tests has been implemented to understand how it's working, you'll find them into the [TestSilenceDectection](./TestSilenceDectection) folder. They allow to quickly test if anything is wrong after making modifications.

As an example, here is a simple test to detect a silence of more than 200 milliseconds with a threshold at -40 db.

The array contains only 4 elements simulating a 1 second sample so the sample rate is 4 with 1 channel and 8 bits per sample so 1 byte per sample. In this case 0x80 is the 0 value once the signal is converted.
The index of the start of the silence is 2 witch is the same as the end one as we have only 1 element. The start of the silence is 500 ms and the duration 250 ms.

```csharp
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
```