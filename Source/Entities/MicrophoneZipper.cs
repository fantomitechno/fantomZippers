using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.ZipMover;

namespace Celeste.Mod.fantomZippers.Entities
{
  [CustomEntity("MikeTheMapPlease/MikeTheZipper", "fantomZippers/MicrophoneZipper")]
  public class MicrophoneZipper : Solid
  {
    public MicrophoneZipper(Vector2 position, int width, int height, Vector2 target, Themes theme) : base(position, (float)width, (float)height, false)
    {
      if (theme == Themes.Moon)
      {
        this.spritePath = "fantomitechno/fantomZippers/microphone/light";
        id = "objects/zipmover/moon/block";
        key = "objects/zipmover/moon/innercog";
        drawBlackBorder = false;
      }
      else
      {
        this.spritePath = "fantomitechno/fantomZippers/microphone/light";
        id = "objects/zipmover/block";
        key = "objects/zipmover/innercog";
        drawBlackBorder = true;
      }
      this.edges = new MTexture[3, 3];
      List<MTexture> tempInnerCogs = GFX.Game.GetAtlasSubtextures(key);
      if (tempInnerCogs.Count > 0)
      {
        this.innerCogs = tempInnerCogs;
      }
      else
      {
        this.innerCogs = GFX.Game.GetAtlasSubtextures(key);
      }
      this.temp = new MTexture();
      this.percent = 0f;
      this.sfx = new SoundSource();
      base.Depth = -9999;
      this.start = this.Position;
      this.target = target;
      this.firstDirection = true;
      base.Add(new Coroutine(this.Sequence(), true));
      base.Add(new LightOcclude(1f));
      try
      {
        base.Add(this.streetlight = new Sprite(GFX.Game, this.spritePath));
        this.streetlight.Add("frames", "", 1f);
      }
      catch
      {
        base.Add(this.streetlight = new Sprite(GFX.Game, this.spritePath));
        this.streetlight.Add("frames", "", 1f);
      }
      this.streetlight.Play("frames", false, false);
      this.streetlight.Active = false;
      this.streetlight.SetAnimationFrame(1);
      this.streetlight.Position = new Vector2(base.Width / 2f - this.streetlight.Width / 2f, base.Height / 2f - this.streetlight.Height / 2f);
      base.Add(this.bloom = new BloomPoint(1f, 6f));
      this.bloom.Position = new Vector2(base.Width / 2f, base.Height / 2f - this.streetlight.Height / 2f + 3f);
      for (int i = 0; i < 3; i++)
      {
        for (int j = 0; j < 3; j++)
        {
          this.edges[i, j] = GFX.Game[id].GetSubtexture(i * 8, j * 8, 8, 8, null);
        }
      }
      this.SurfaceSoundIndex = 7;
      this.sfx.Position = new Vector2(base.Width, base.Height) / 2f;
      base.Add(this.sfx);
    }

    public MicrophoneZipper(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Height, data.Nodes[0] + offset, data.Enum("theme", Themes.Normal))
    {
    }

    public override void Added(Scene scene)
    {
      base.Added(scene);
      scene.Add(this.pathRenderer = new ZipMoverPathRenderer(this, key));
    }

    public override void Removed(Scene scene)
    {
      scene.Remove(this.pathRenderer);
      this.pathRenderer = null;
      base.Removed(scene);
    }

    public override void Update()
    {
      base.Update();
      this.bloom.Y = (float)(base.Height / 2f - this.streetlight.Height / 2f + this.streetlight.CurrentAnimationFrame * 3);
    }

    public override void Render()
    {
      Vector2 position = Position;
      Position += base.Shake;
      Draw.Rect(base.X + 1f, base.Y + 1f, base.Width - 2f, base.Height - 2f, Color.Black);
      int num = 1;
      float num2 = 0f;
      int count = innerCogs.Count;
      for (int i = 4; (float)i <= base.Height - 4f; i += 8)
      {
        int num3 = num;
        for (int j = 4; (float)j <= base.Width - 4f; j += 8)
        {
          int index = (int)(mod((num2 + (float)num * percent * MathF.PI * 4f) / (MathF.PI / 2f), 1f) * (float)count);
          MTexture mTexture = innerCogs[index];
          Rectangle rectangle = new Rectangle(0, 0, mTexture.Width, mTexture.Height);
          Vector2 zero = Vector2.Zero;
          if (j <= 4)
          {
            zero.X = 2f;
            rectangle.X = 2;
            rectangle.Width -= 2;
          }
          else if ((float)j >= base.Width - 4f)
          {
            zero.X = -2f;
            rectangle.Width -= 2;
          }
          if (i <= 4)
          {
            zero.Y = 2f;
            rectangle.Y = 2;
            rectangle.Height -= 2;
          }
          else if ((float)i >= base.Height - 4f)
          {
            zero.Y = -2f;
            rectangle.Height -= 2;
          }
          mTexture = mTexture.GetSubtexture(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, temp);
          mTexture.DrawCentered(Position + new Vector2(j, i) + zero, Color.White * ((num < 0) ? 0.5f : 1f));
          num = -num;
          num2 += MathF.PI / 3f;
        }
        if (num3 == num)
        {
          num = -num;
        }
      }
      for (int k = 0; (float)k < base.Width / 8f; k++)
      {
        for (int l = 0; (float)l < base.Height / 8f; l++)
        {
          int num4 = ((k != 0) ? (((float)k != base.Width / 8f - 1f) ? 1 : 2) : 0);
          int num5 = ((l != 0) ? (((float)l != base.Height / 8f - 1f) ? 1 : 2) : 0);
          if (num4 != 1 || num5 != 1)
          {
            edges[num4, num5].Draw(new Vector2(base.X + (float)(k * 8), base.Y + (float)(l * 8)));
          }
        }
      }
      base.Render();
      Position = position;
    }

    private IEnumerator Sequence()
    {
      Vector2 start = this.Position;
      for (; ; )
      {
        yield return null;

        this.percent = fantomZippersModule.Instance.percent / 100;
        if (percent < 0.15)
        {
          streetlight.SetAnimationFrame(1);
        }
        else if (percent < 0.6)
        {
          streetlight.SetAnimationFrame(2);
        }
        else
        {
          streetlight.SetAnimationFrame(3);
        }
        Vector2 to = Vector2.Lerp(start, target, percent);
        this.MoveTo(to);
        to = default;
      }
    }

    private float mod(float x, float m)
    {
      return (x % m + m) % m;
    }

    static MicrophoneZipper()
    {
      ropeColor = Calc.HexToColor("d1d1d1");
      ropeLightColor = Calc.HexToColor("9e9e9e");
    }

    private string spritePath;
    private string id;
    private string key;
    private bool drawBlackBorder;

    private MTexture[,] edges;

    private Sprite streetlight;

    private BloomPoint bloom;

    private ZipMoverPathRenderer pathRenderer;

    public static ParticleType P_Scrape = ZipMover.P_Scrape;

    public static ParticleType P_Sparks = ZipMover.P_Sparks;

    private List<MTexture> innerCogs;

    private MTexture temp;

    private Vector2 start;

    private Vector2 target;

    private float percent;

    private static readonly Color ropeColor;

    private static readonly Color ropeLightColor;

    private SoundSource sfx;

    private bool firstDirection;

    private class ZipMoverPathRenderer : Entity
    {
      public ZipMoverPathRenderer(MicrophoneZipper zipMover, string spritePath) : base()
      {
        this.cog = GFX.Game[spritePath.Replace("innercog", "cog")];
        base.Depth = 5000;
        this.ZipMover = zipMover;
        this.from = this.ZipMover.start + new Vector2(this.ZipMover.Width / 2f, this.ZipMover.Height / 2f);
        this.to = this.ZipMover.target + new Vector2(this.ZipMover.Width / 2f, this.ZipMover.Height / 2f);
        this.sparkAdd = (this.from - this.to).SafeNormalize(5f).Perpendicular();
        float num = (this.from - this.to).Angle();
        this.sparkDirFromA = num + 0.3926991f;
        this.sparkDirFromB = num - 0.3926991f;
        this.sparkDirToA = num + 3.14159274f - 0.3926991f;
        this.sparkDirToB = num + 3.14159274f + 0.3926991f;
      }

      public void CreateSparks()
      {
        base.SceneAs<Level>().ParticlesBG.Emit(P_Sparks, this.from + this.sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), this.sparkDirFromA);
        base.SceneAs<Level>().ParticlesBG.Emit(P_Sparks, this.from - this.sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), this.sparkDirFromB);
        base.SceneAs<Level>().ParticlesBG.Emit(P_Sparks, this.to + this.sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), this.sparkDirToA);
        base.SceneAs<Level>().ParticlesBG.Emit(P_Sparks, this.to - this.sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), this.sparkDirToB);
      }

      public override void Render()
      {
        DrawCogs(Vector2.UnitY, Color.Black);
        DrawCogs(Vector2.Zero);
        if (ZipMover.drawBlackBorder)
        {
          Draw.Rect(new Rectangle((int)(ZipMover.X + ZipMover.Shake.X - 1f), (int)(ZipMover.Y + ZipMover.Shake.Y - 1f), (int)ZipMover.Width + 2, (int)ZipMover.Height + 2), Color.Black);
        }
      }

      private void DrawCogs(Vector2 offset, Color? colorOverride = null)
      {
        Vector2 vector = (to - from).SafeNormalize();
        Vector2 vector2 = vector.Perpendicular() * 3f;
        Vector2 vector3 = -vector.Perpendicular() * 4f;
        float rotation = ZipMover.percent * MathF.PI * 2f;
        Draw.Line(from + vector2 + offset, to + vector2 + offset, colorOverride.HasValue ? colorOverride.Value : ropeColor);
        Draw.Line(from + vector3 + offset, to + vector3 + offset, colorOverride.HasValue ? colorOverride.Value : ropeColor);
        for (float num = 4f - ZipMover.percent * MathF.PI * 8f % 4f; num < (to - from).Length(); num += 4f)
        {
          Vector2 vector4 = from + vector2 + vector.Perpendicular() + vector * num;
          Vector2 vector5 = to + vector3 - vector * num;
          Draw.Line(vector4 + offset, vector4 + vector * 2f + offset, colorOverride.HasValue ? colorOverride.Value : ropeLightColor);
          Draw.Line(vector5 + offset, vector5 - vector * 2f + offset, colorOverride.HasValue ? colorOverride.Value : ropeLightColor);
        }
        cog.DrawCentered(from + offset, colorOverride.HasValue ? colorOverride.Value : Color.White, 1f, rotation);
        cog.DrawCentered(to + offset, colorOverride.HasValue ? colorOverride.Value : Color.White, 1f, rotation);
      }

      public MicrophoneZipper ZipMover;

      private MTexture cog;

      private Vector2 from;

      private Vector2 to;

      private Vector2 sparkAdd;

      private float sparkDirFromA;

      private float sparkDirFromB;

      private float sparkDirToA;

      private float sparkDirToB;
      private MTexture temp = new MTexture();
    }
  }
}