using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NAudio.Wave;
using System.IO;

public class mediaTranscoding : MonoBehaviour
{

    void hoge()
    {
        var path = "おめでとう_noHeader.m4a";
        var s = new RawSourceWaveStream(File.OpenRead(path), new WaveFormat(16000, 1));
        var outpath = "example_noHeader_2.wav";
        WaveFileWriter.CreateWaveFile(outpath, s);
    }

    void hogehoge()
    {
        WaveFormat format = new WaveFormat(16000, 16, 1);
        try
        {
            using (MediaFoundationReader reader = new MediaFoundationReader("D:\\#WorkSpace\\#PersonalDevelop\\BaseUnityProject\\BaseProject\\ykbr0-i5fv9.wav"))
            {
                using (MediaFoundationResampler resampler = new MediaFoundationResampler(reader, format))
                {
                    WaveFileWriter.CreateWaveFile("sample.wav", resampler);
                }
            }

        }
        catch (FileNotFoundException e)
        {
            Debug.LogError(e.Message);
        }
    }

    void hogehogehoge()
    {
        //https://codeday.me/jp/qa/20190626/1109062.html

        // convert source audio to AAC
        // create media foundation reader to read the source (can be any supported format, mp3, wav, ...)
        using (MediaFoundationReader reader = new MediaFoundationReader(@"d:\ykbr0-i5fv9.wav"))
        {
            MediaFoundationEncoder.EncodeToAac(reader, @"D:\test.mp4");
        }

        // convert "back" to WAV
        // create media foundation reader to read the AAC encoded file
        using (MediaFoundationReader reader = new MediaFoundationReader(@"D:\test.mp4"))
        // resample the file to PCM with same sample rate, channels and bits per sample
        using (ResamplerDmoStream resampledReader = new ResamplerDmoStream(reader,
            new WaveFormat(reader.WaveFormat.SampleRate, reader.WaveFormat.BitsPerSample, reader.WaveFormat.Channels)))
        // create WAVe file
        using (WaveFileWriter waveWriter = new WaveFileWriter(@"d:\test.wav", resampledReader.WaveFormat))
        {
            // copy samples
            resampledReader.CopyTo(waveWriter);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        hogehogehoge();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
