using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Celeste.Mod.fantomZippers;

public class fantomZippersModuleSettings : EverestModuleSettings
{

  public int Mic { get; set; } = 0;

  private TextMenu.Slider MicEntry;

  // Specify how to create the menu item
  public void CreateMicEntry(TextMenu menu, bool inGame)
  {
    MicEntry = new TextMenu.Slider(
            "Device", (i) =>
            {
              return $"{Microphone.All[i].Name}";
            }, 0, Microphone.All.Count - 1, Mic
        );
    MicEntry.Change(v =>
    {
      Mic = v;

      fantomZippersModule.Instance.microphone.Stop();
      fantomZippersModule.Instance.microphone = fantomZippersModule.InitMic(Microphone.All[Mic]);

      fantomZippersModule.Instance.microphone.Start();
    });

    menu.Add(MicEntry);

    menu.Add(new See());
  }



  [SettingRange(min: 0, max: 100)]
  public int Threshold { get; set; } = 50;

  [SettingRange(min: 0, max: 10)]
  public int Boost { get; set; } = 2;

  public class See : TextMenu.Item
  {
    public string Label;

    public See()
        => Label = "Preview";

    // Menu item properties

    public override float LeftWidth()
        => ActiveFont.Measure(Label).X;

    public override float Height()
        => ActiveFont.LineHeight;

    // Mod Search support

    public override string SearchLabel()
        => Label;

    // Interactions

    public override void ConfirmPressed()
        => PlaySound();

    public override void LeftPressed()
        => PlaySound();

    public override void RightPressed()
        => PlaySound();

    private static void PlaySound()
        => Audio.Play(SFX.ui_game_increment_strawberry);

    // Rendering

    public override void Render(Vector2 position, bool highlighted)
    {
      float alpha = Container.Alpha;
      bool isTwoColumn = Container.InnerContent == TextMenu.InnerContentMode.TwoColumn;

      ActiveFont.DrawOutline(
          Label + " " + fantomZippersModule.Instance.percent.ToString("0.00") + "%",
          position: position + (isTwoColumn
              ? Vector2.Zero
              : Vector2.UnitX * (Container.Width / 2f)),
          justify: isTwoColumn
              ? Vector2.UnitY / 2f
              : Vector2.One / 2f,
          scale: Vector2.One,
          color: Disabled
              ? Color.DarkSlateGray
              : (highlighted ? Container.HighlightColor : Color.White) * alpha,
          stroke: 2f,
          strokeColor: Color.Black * (alpha * alpha * alpha)
      );
    }
  }
}