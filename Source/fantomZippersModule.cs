using System;
using Microsoft.Xna.Framework.Audio;

namespace Celeste.Mod.fantomZippers;

public class fantomZippersModule : EverestModule
{
  public static fantomZippersModule Instance { get; private set; }

  public override Type SettingsType => typeof(fantomZippersModuleSettings);
  public static fantomZippersModuleSettings Settings => (fantomZippersModuleSettings)Instance._Settings;

  public Microphone microphone;
  private byte[] micSamples;

  public float percent;

  public fantomZippersModule()
  {
    Instance = this;
#if DEBUG
    // debug builds use verbose logging
    Logger.SetLogLevel(nameof(fantomZippersModule), LogLevel.Verbose);
#else
    // release builds use info logging to reduce spam in log files
    Logger.SetLogLevel(nameof(fantomZippersModule), LogLevel.Info);
#endif


    microphone = Microphone.Default;
    if (microphone != null)
    {
      microphone.BufferDuration = TimeSpan.FromMilliseconds(100);
      microphone.BufferReady += BufferReady;
      micSamples = new byte[microphone.GetSampleSizeInBytes(microphone.BufferDuration)];
    }
  }

  public override void Load()
  {
    if (microphone != null)
    {
      microphone.Start();
    }
  }

  void BufferReady(object sender, EventArgs e)
  {
    try
    {
      microphone.GetData(micSamples);
      double rms = CalculateRms(micSamples);
      double volume = Settings.Threshold + 20 * Math.Log10(rms / short.MaxValue);
      percent = (float)Math.Min(Math.Max(volume * Settings.Boost, 0), 100);
    }
    catch (NoMicrophoneConnectedException)
    {
      // Microphone was disconnected - let the user know.
    }
  }

  public override void Unload()
  {

    if (microphone != null)
    {
      microphone.Stop();
    }
  }

  static double CalculateRms(byte[] pcm)
  {
    int sampleCount = pcm.Length / 2; // 16-bit samples
    double sumSquares = 0;

    for (int i = 0; i < pcm.Length; i += 2)
    {
      short sample = BitConverter.ToInt16(pcm, i);
      sumSquares += sample * sample;
    }

    double meanSquares = sumSquares / sampleCount;
    return Math.Sqrt(meanSquares);
  }

  public static Microphone InitMic(Microphone microphone)
  {
    if (microphone != null)
    {
      microphone.BufferDuration = TimeSpan.FromMilliseconds(10);
      microphone.BufferReady += Instance.BufferReady;
      Instance.micSamples = new byte[microphone.GetSampleSizeInBytes(microphone.BufferDuration)];
    }

    return microphone;
  }
}