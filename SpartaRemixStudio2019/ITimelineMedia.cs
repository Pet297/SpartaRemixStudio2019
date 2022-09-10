using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;

namespace SpartaRemixStudio2019
{
    public class TimelineMedia : ICloneable
    {
        public TimelineMediaType Property { get; set; }

        public ulong TimeFrom { get; set; }
        public ulong TimeLenght { get; set; }
        public ulong TimeEnd { get => TimeLenght + TimeFrom; }

        public float Pitch { get; set; }
        public float Volume { get; set; }
        public float Opacity { get; set; }
        public float Pan { get; set; }
        public float Stretch { get; set; }
        public float Formant { get; set; }

        public float SecondsIn { get; set; }
        public ulong BeatIn { get; set; }

        public MediaFlags Flags0 { get; set; }

        public ulong OpacityIn { get; set; }
        public ulong OpacityOut { get; set; }
        public ulong FadeIn { get; set; }
        public ulong FadeOut { get; set; }

        public TimelineMedia(TimelineMediaType type, bool beatSync, ulong audioFade)
        {
            Property = type;

            TimeFrom = 0;
            TimeLenght = 1;

            Pitch = 0;
            Volume = 0;
            Opacity = 1;
            Pan = 0;
            Stretch = 1;

            SecondsIn = 0;
            BeatIn = 0;

            Flags0 = MediaFlags.AudioEnabled | MediaFlags.VideoEnabled | (beatSync ? MediaFlags.UseBeatIn : 0);

            OpacityIn = 0;
            OpacityOut = 0;
            FadeIn = audioFade;
            FadeOut = audioFade;
        }

        public object Clone()
        {
            TimelineMediaType tt = Property.Clone() as TimelineMediaType;
            return new TimelineMedia(tt, Flags0.HasFlag(MediaFlags.UseBeatIn), 0)
            {
                TimeFrom = this.TimeFrom, TimeLenght = this.TimeLenght,
                Pitch = this.Pitch, Volume = this.Volume, Opacity = this.Opacity,
                Pan = this.Pan, Stretch = this.Stretch, Formant = this.Formant,
                SecondsIn = this.SecondsIn, BeatIn = this.BeatIn, Flags0 = this.Flags0,
                OpacityIn = this.OpacityIn, OpacityOut = this.OpacityOut, FadeIn = this.FadeIn, FadeOut = this.FadeOut
            };
        }
    }
    [Flags] public enum MediaFlags
    {
        None = 0,
        AudioEnabled = 1<<0,
        VideoEnabled = 1<<1,
        UseBeatIn = 1<<2
    }

    public interface ITimelineMediaFactory
    {
        long DataType { get; }
        long UniqueIdentifier { get; }

        TimelineMediaType Generate();
    }
    // PAT READ
    public abstract class TimelineMediaType : ICloneable
    {
        public virtual uint DataType => uint.MaxValue;
        public virtual uint MediaType => uint.MaxValue;
        public virtual bool HasAudio => false;

        public virtual IPitchReader GetAudio(Project p, TimelineMedia parent, ulong partitioning) => null;
        public virtual IVideoReader GetVideo(Project p, TimelineMedia parent, ulong partitioning) => null;
        public virtual float GetNumber(TimelineMedia parent, ulong partitioning, float TimeBeat_Partitioned) => 0;
        public virtual OpenTK.Vector2 GetNumber2(Project p, TimelineMedia parent, ulong partitioning, float TimeBeat_Partitioned) => OpenTK.Vector2.Zero;
        public virtual OpenTK.Vector3 GetNumber3(Project p, TimelineMedia parent, ulong partitioning, float TimeBeat_Partitioned) => OpenTK.Vector3.Zero;
        public virtual OpenTK.Vector4 GetNumber4(Project p, TimelineMedia parent, ulong partitioning, float TimeBeat_Partitioned) => OpenTK.Vector4.Zero;
        public virtual OpenTK.Matrix4 GetMatrix(Project p, TimelineMedia parent, ulong partitioning, float TimeBeat_Partitioned) => OpenTK.Matrix4.Identity;
        public virtual string GetString(Project p, TimelineMedia parent, ulong partitioning, float TimeBeat_Partitioned) => "";
        public virtual LinearCombination GetLinear(Project p, TimelineMedia parent, ulong partitioning, float TimeBeat_Partitioned) => LinearCombination.Zero;
        public virtual bool GetBool(Project p, TimelineMedia parent, ulong partitioning, float TimeBeat_Partitioned) => false;

        public virtual void Save(System.IO.FileStream fs) { }
        public virtual void Load(System.IO.FileStream fs) { }

        public virtual object Clone()
        {
            return null;
        }
    }

    public static class TimeLineMediaHelper
    {
        public static void Save(System.IO.FileStream fs, TimelineMedia tlm)
        {
            StreamHelper.SaveBytes(fs, (int)16);
            StreamHelper.SaveBytes(fs, tlm.BeatIn);
            StreamHelper.SaveBytes(fs, tlm.FadeIn);
            StreamHelper.SaveBytes(fs, tlm.FadeOut);
            StreamHelper.SaveBytes(fs, (int)tlm.Flags0);
            StreamHelper.SaveBytes(fs, tlm.Formant);
            StreamHelper.SaveBytes(fs, tlm.Opacity);
            StreamHelper.SaveBytes(fs, tlm.OpacityIn);
            StreamHelper.SaveBytes(fs, tlm.OpacityOut);
            StreamHelper.SaveBytes(fs, tlm.Pan);
            StreamHelper.SaveBytes(fs, tlm.Pitch);
            StreamHelper.SaveBytes(fs, tlm.SecondsIn);
            StreamHelper.SaveBytes(fs, tlm.Stretch);
            StreamHelper.SaveBytes(fs, tlm.TimeFrom);
            StreamHelper.SaveBytes(fs, tlm.TimeLenght);
            StreamHelper.SaveBytes(fs, tlm.Volume);

            StreamHelper.SaveBytes(fs, tlm.Property.MediaType);
            tlm.Property.Save(fs);
        }
        public static void Load(System.IO.FileStream fs, TimelineMedia tlm)
        {
            int lenght = StreamHelper.LoadInt(fs);

            if (lenght > 0) { tlm.BeatIn = StreamHelper.LoadULong(fs); lenght--; }
            if (lenght > 0) { tlm.FadeIn = StreamHelper.LoadULong(fs); lenght--; }
            if (lenght > 0){tlm.FadeOut = StreamHelper.LoadULong(fs); lenght--; }
            if (lenght > 0) { tlm.Flags0 = (MediaFlags)StreamHelper.LoadInt(fs); lenght--; }
            if (lenght > 0) { tlm.Formant = StreamHelper.LoadFloat(fs); lenght--; }
            if (lenght > 0) { tlm.Opacity = StreamHelper.LoadFloat(fs);lenght--; }
                if (lenght > 0) { tlm.OpacityIn = StreamHelper.LoadULong(fs);lenght--; }
                if (lenght > 0) { tlm.OpacityOut = StreamHelper.LoadULong(fs);lenght--; }
                if (lenght > 0) { tlm.Pan = StreamHelper.LoadFloat(fs);lenght--; }
                if (lenght > 0) { tlm.Pitch = StreamHelper.LoadFloat(fs);lenght--; }
                if (lenght > 0) { tlm.SecondsIn = StreamHelper.LoadFloat(fs);lenght--; }
                if (lenght > 0) { tlm.Stretch = StreamHelper.LoadFloat(fs);lenght--; }
                if (lenght > 0) { tlm.TimeFrom = StreamHelper.LoadULong(fs);lenght--; }
                if (lenght > 0) { tlm.TimeLenght = StreamHelper.LoadULong(fs);lenght--; }
                if (lenght > 0) { tlm.Volume = StreamHelper.LoadFloat(fs);lenght--; }

            if (lenght > 0)
            {
                uint type = StreamHelper.LoadUInt(fs);
                tlm.Property = null;

                if (type == 00) tlm.Property = new TLSample();
                if (type == 01) tlm.Property = new TLPattern();

                if (type == 08) tlm.Property = new TLNumber();
                if (type == 09) tlm.Property = new TLNumberTrans();
                if (type == 10) tlm.Property = new TLNumber2();
                if (type == 11) tlm.Property = new TLNumber2Trans();
                if (type == 12) tlm.Property = new TLNumber3();
                if (type == 13) tlm.Property = new TLNumber3Trans();
                if (type == 14) tlm.Property = new TLNumber4();
                if (type == 15) tlm.Property = new TLNumber4Trans();

                if (type == 16) tlm.Property = new TLString();
                if (type == 17) tlm.Property = new TLStringTrans();
                if (type == 18) tlm.Property = new TLStringSingleChar();

                if (type == 24) tlm.Property = new TLComb1();
                if (type == 25) tlm.Property = new TLComb2();

                if (type == 32) tlm.Property = new TLFalse();
                if (type == 33) tlm.Property = new TLTrue();
                lenght--;
            }
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }

                if (tlm.Property != null) tlm.Property.Load(fs);
        }
    }

    //0x00 - 0x01
    public class TLSample : TimelineMediaType
    {
        public uint Sample;
        public List<IVideoFX> VFX = new List<IVideoFX>();

        public override uint DataType => 0;
        public override uint MediaType => 0;
        public override bool HasAudio => true;

        public override IPitchReader GetAudio(Project p, TimelineMedia parent, ulong partitioning)
        {
            IPitchReader r = null;
            if (!p.ProjectSamples.ContainsKey(Sample)) return null;
            if (p.ProjectSamples[Sample].ips == null) return null;

            if (parent.Flags0.HasFlag(MediaFlags.UseBeatIn)) r = p.ProjectSamples[Sample].ips.GetReader(parent.BeatIn / (float)partitioning);
            else r = p.ProjectSamples[Sample].ips.GetReader(parent.SecondsIn);

            return r;
        }
        public override object Clone()
        {
            List<IVideoFX> fxCopy = new List<IVideoFX>();
            foreach (IVideoFX vfx in VFX) fxCopy.Add((IVideoFX)vfx.Clone());
            return new TLSample() { Sample = this.Sample, VFX = fxCopy};
        }

        public override void Save(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)2);

            StreamHelper.SaveBytes(fs, Sample);
            StreamHelper.SaveBytes(fs, VFX.Count);
            foreach (IVideoFX vfx in VFX) EffectHelper.Save(fs, vfx);
        }
        public override void Load(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0)
            {
                Sample = StreamHelper.LoadUInt(fs);
                lenght--;
            }
            if (lenght > 0)
            {
                int l0 = StreamHelper.LoadInt(fs);
                for (int i = 0; i < l0; i++) VFX.Add(EffectHelper.LoadVFX(fs));
                lenght--;
            }
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }
    }
    public class TLPattern : TimelineMediaType
    {
        public List<uint> Samples = new List<uint>();
        public List<float> Volumes = new List<float>();
        public List<float> Positions = new List<float>();
        public uint Pattern;
        public bool KeepVideo = false;

        public override uint MediaType => 1;
        public override uint DataType => 0;
        public override bool HasAudio => true;

        public override object Clone()
        {
            return new TLPattern() { Samples = new List<uint>(Samples), Pattern = this.Pattern };
        }

        public override void Save(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, Pattern);
            StreamHelper.SaveBytes(fs, Samples.Count);
            foreach (uint u in Samples) StreamHelper.SaveBytes(fs, u);
            StreamHelper.SaveBytes(fs, Volumes.Count);
            foreach (float u in Volumes) StreamHelper.SaveBytes(fs, u);
            StreamHelper.SaveBytes(fs, Positions.Count);
            foreach (float u in Positions) StreamHelper.SaveBytes(fs, u);
        }
        public override void Load(FileStream fs)
        {
            Samples.Clear();
            Volumes.Clear();
            Positions.Clear();

            Pattern = StreamHelper.LoadUInt(fs);
            int l0 = StreamHelper.LoadInt(fs);
            for (int i = 0; i < l0; i++)
            {
                Samples.Add(StreamHelper.LoadUInt(fs));
            }
            l0 = StreamHelper.LoadInt(fs);
            for (int i = 0; i < l0; i++)
            {
                Volumes.Add(StreamHelper.LoadFloat(fs));
            }
            l0 = StreamHelper.LoadInt(fs);
            for (int i = 0; i < l0; i++)
            {
                Positions.Add(StreamHelper.LoadFloat(fs));
            }
        }
    }
    //0x08 - 0x0F
    public class TLNumber : TimelineMediaType
    {
        public float Number;

        public override uint DataType => 1;
        public override uint MediaType => 8;

        public override float GetNumber(TimelineMedia parent, ulong partitioning, float TimeBeat_Partitioned)
        {
            return Number;
        }

        public override object Clone()
        {
            return new TLNumber() { Number = this.Number };
        }

        public override void Save(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, Number);
        }
        public override void Load(FileStream fs)
        {
            Number = StreamHelper.LoadFloat(fs);
        }
    }
    public class TLNumberTrans : TimelineMediaType
    {
        public float Number0;
        public float Number1;

        public override uint DataType => 1;
        public override uint MediaType => 9;

        public override float GetNumber(TimelineMedia parent, ulong partitioning, float TimeBeat_Partitioned)
        {
            float pr = (TimeBeat_Partitioned - parent.TimeFrom) / (float)parent.TimeLenght;
            return Number0 * (1-pr) + Number1 * pr;
        }

        public override object Clone()
        {
            return new TLNumberTrans() { Number0 = this.Number0, Number1 = this.Number1 };
        }

        public override void Save(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, Number0);
            StreamHelper.SaveBytes(fs, Number1);
        }
        public override void Load(FileStream fs)
        {
            Number0 = StreamHelper.LoadFloat(fs);
            Number1 = StreamHelper.LoadFloat(fs);
        }
    }
    public class TLNumber2 : TimelineMediaType
    {
        public float NumberX;
        public float NumberY;

        public override uint DataType => 2;
        public override uint MediaType => 10;

        public override Vector2 GetNumber2(Project p, TimelineMedia parent, ulong partitioning, float TimeBeat_Partitioned)
        {
            return new Vector2(NumberX, NumberY);
        }

        public override object Clone()
        {
            return new TLNumber2() { NumberX = this.NumberX, NumberY = this.NumberY };
        }

        public override void Save(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, NumberX);
            StreamHelper.SaveBytes(fs, NumberY);
        }
        public override void Load(FileStream fs)
        {
            NumberX = StreamHelper.LoadUInt(fs);
            NumberY = StreamHelper.LoadUInt(fs);
        }
    }
    public class TLNumber2Trans : TimelineMediaType
    {
        public float NumberX0;
        public float NumberX1;
        public float NumberY0;
        public float NumberY1;

        public override uint DataType => 2;
        public override uint MediaType => 11;
        
        public override Vector2 GetNumber2(Project p, TimelineMedia parent, ulong partitioning, float TimeBeat_Partitioned)
        {
            float pr = (TimeBeat_Partitioned - parent.TimeFrom) / (float)parent.TimeLenght;
            return (new Vector2(NumberX0, NumberY0) * (1-pr) + new Vector2(NumberX1, NumberY1) * pr);
        }

        public override object Clone()
        {
            return new TLNumber2Trans() { NumberX0 = this.NumberX0, NumberX1 = this.NumberX1, NumberY0 = this.NumberY0, NumberY1 = this.NumberY1 };
        }

        public override void Save(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, NumberX0);
            StreamHelper.SaveBytes(fs, NumberX1);
            StreamHelper.SaveBytes(fs, NumberY0);
            StreamHelper.SaveBytes(fs, NumberY1);
        }
        public override void Load(FileStream fs)
        {
            NumberX0 = StreamHelper.LoadFloat(fs);
            NumberX1 = StreamHelper.LoadFloat(fs);
            NumberY0 = StreamHelper.LoadFloat(fs);
            NumberY1 = StreamHelper.LoadFloat(fs);
        }
    }
    public class TLNumber3 : TimelineMediaType
    {
        public float NumberX;
        public float NumberY;
        public float NumberZ;

        public override uint DataType => 3;
        public override uint MediaType => 12;

        public override Vector3 GetNumber3(Project p, TimelineMedia parent, ulong partitioning, float TimeBeat_Partitioned)
        {
            return new Vector3(NumberX, NumberY, NumberZ);
        }

        public override object Clone()
        {
            return new TLNumber3() { NumberX = this.NumberX, NumberY = this.NumberY, NumberZ = this.NumberZ };
        }

        public override void Save(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, NumberX);
            StreamHelper.SaveBytes(fs, NumberY);
            StreamHelper.SaveBytes(fs, NumberZ);
        }
        public override void Load(FileStream fs)
        {
            NumberX = StreamHelper.LoadFloat(fs);
            NumberY = StreamHelper.LoadFloat(fs);
            NumberZ = StreamHelper.LoadFloat(fs);
        }
    }
    public class TLNumber3Trans : TimelineMediaType
    {
        public float NumberX0;
        public float NumberX1;
        public float NumberY0;
        public float NumberY1;
        public float NumberZ0;
        public float NumberZ1;

        public override uint DataType => 3;
        public override uint MediaType => 13;

        public override Vector3 GetNumber3(Project p, TimelineMedia parent, ulong partitioning, float TimeBeat_Partitioned)
        {
            float pr = (TimeBeat_Partitioned - parent.TimeFrom) / (float)parent.TimeLenght;
            return (new Vector3(NumberX0, NumberY0, NumberZ0) * (1 - pr) + new Vector3(NumberX1, NumberY1, NumberZ1) * pr);
        }

        public override object Clone()
        {
            return new TLNumber3Trans() { NumberX0 = this.NumberX0, NumberX1 = this.NumberX1, NumberY0 = this.NumberY0, NumberY1 = this.NumberY1, NumberZ0 = this.NumberZ0, NumberZ1 = this.NumberZ1 };
        }

        public override void Save(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, NumberX0);
            StreamHelper.SaveBytes(fs, NumberX1);
            StreamHelper.SaveBytes(fs, NumberY0);
            StreamHelper.SaveBytes(fs, NumberY1);
            StreamHelper.SaveBytes(fs, NumberZ0);
            StreamHelper.SaveBytes(fs, NumberZ1);
        }
        public override void Load(FileStream fs)
        {
            NumberX0 = StreamHelper.LoadFloat(fs);
            NumberX1 = StreamHelper.LoadFloat(fs);
            NumberY0 = StreamHelper.LoadFloat(fs);
            NumberY1 = StreamHelper.LoadFloat(fs);
            NumberZ0 = StreamHelper.LoadFloat(fs);
            NumberZ1 = StreamHelper.LoadFloat(fs);
        }
    }
    public class TLNumber4 : TimelineMediaType
    {
        public float NumberX;
        public float NumberY;
        public float NumberZ;
        public float NumberW;

        public override uint DataType => 4;
        public override uint MediaType => 14;

        public override Vector4 GetNumber4(Project p, TimelineMedia parent, ulong partitioning, float TimeBeat_Partitioned)
        {
            return new Vector4(NumberX, NumberY, NumberZ, NumberW);
        }

        public override object Clone()
        {
            return new TLNumber4() { NumberX = this.NumberX, NumberY = this.NumberY, NumberZ = this.NumberZ, NumberW = this.NumberW };
        }

        public override void Save(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, NumberX);
            StreamHelper.SaveBytes(fs, NumberY);
            StreamHelper.SaveBytes(fs, NumberZ);
            StreamHelper.SaveBytes(fs, NumberW);
        }
        public override void Load(FileStream fs)
        {
            NumberX = StreamHelper.LoadFloat(fs);
            NumberY = StreamHelper.LoadFloat(fs);
            NumberZ = StreamHelper.LoadFloat(fs);
            NumberW = StreamHelper.LoadFloat(fs);
        }
    }
    public class TLNumber4Trans : TimelineMediaType
    {
        public float NumberX0;
        public float NumberX1;
        public float NumberY0;
        public float NumberY1;
        public float NumberZ0;
        public float NumberZ1;
        public float NumberW0;
        public float NumberW1;

        public override uint DataType => 4;
        public override uint MediaType => 15;

        public override Vector4 GetNumber4(Project p, TimelineMedia parent, ulong partitioning, float TimeBeat_Partitioned)
        {
            float pr = (TimeBeat_Partitioned - parent.TimeFrom) / (float)parent.TimeLenght;
            return (new Vector4(NumberX0, NumberY0, NumberZ0, NumberW0) * (1 - pr) + new Vector4(NumberX1, NumberY1, NumberZ1, NumberW1) * pr);
        }

        public override object Clone()
        {
            return new TLNumber4Trans() { NumberX0 = this.NumberX0, NumberX1 = this.NumberX1, NumberY0 = this.NumberY0, NumberY1 = this.NumberY1, NumberZ0 = this.NumberZ0, NumberZ1 = this.NumberZ1, NumberW0 = this.NumberW0, NumberW1 = this.NumberW1 };
        }

        public override void Save(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, NumberX0);
            StreamHelper.SaveBytes(fs, NumberX1);
            StreamHelper.SaveBytes(fs, NumberY0);
            StreamHelper.SaveBytes(fs, NumberY1);
            StreamHelper.SaveBytes(fs, NumberZ0);
            StreamHelper.SaveBytes(fs, NumberZ1);
            StreamHelper.SaveBytes(fs, NumberW0);
            StreamHelper.SaveBytes(fs, NumberW1);
        }
        public override void Load(FileStream fs)
        {
            NumberX0 = StreamHelper.LoadFloat(fs);
            NumberX1 = StreamHelper.LoadFloat(fs);
            NumberY0 = StreamHelper.LoadFloat(fs);
            NumberY1 = StreamHelper.LoadFloat(fs);
            NumberZ0 = StreamHelper.LoadFloat(fs);
            NumberZ1 = StreamHelper.LoadFloat(fs);
            NumberW0 = StreamHelper.LoadFloat(fs);
            NumberW1 = StreamHelper.LoadFloat(fs);
        }
    }
    //0x10 - 0x12
    public class TLString : TimelineMediaType
    {
        public string String;

        public override uint DataType => 7;
        public override uint MediaType => 16;

        public override string GetString(Project p, TimelineMedia parent, ulong partitioning, float TimeBeat_Partitioned)
        {
            return String;
        }

        public override object Clone()
        {
            return new TLString() { String = this.String };
        }

        public override void Save(FileStream fs)
        {
            StreamHelper.SaveString(fs, String);
        }
        public override void Load(FileStream fs)
        {
            String = StreamHelper.LoadString(fs);
        }
    }
    public class TLStringTrans : TimelineMediaType
    {
        public string String;
        public int Count0;
        public int Count1;

        public override uint DataType => 7;
        public override uint MediaType => 17;

        public override string GetString(Project p, TimelineMedia parent, ulong partitioning, float TimeBeat_Partitioned)
        {
            float pr = (TimeBeat_Partitioned - parent.TimeFrom) / (float)parent.TimeLenght;
            int count = (int)(Count0 * (1 - pr) + Count1 * pr);
            if (count <= 0) return "";
            else if (count >= String.Length) return String;
            else return String.Remove((int)count);
        }

        public override object Clone()
        {
            return new TLStringTrans() { String = this.String, Count0 = this.Count0, Count1 = this.Count1 };
        }

        public override void Save(FileStream fs)
        {
            StreamHelper.SaveString(fs, String);
            StreamHelper.SaveBytes(fs, Count0);
            StreamHelper.SaveBytes(fs, Count1);
        }
        public override void Load(FileStream fs)
        {
            String = StreamHelper.LoadString(fs);
            Count0 = StreamHelper.LoadInt(fs);
            Count1 = StreamHelper.LoadInt(fs);
        }
    }
    public class TLStringSingleChar : TimelineMediaType
    {
        public string String;
        public int Start;
        public int End;

        public override uint DataType => 7;
        public override uint MediaType => 18;

        public override string GetString(Project p, TimelineMedia parent, ulong partitioning, float TimeBeat_Partitioned)
        {
            float pr = (TimeBeat_Partitioned - parent.TimeFrom) / (float)parent.TimeLenght;
            int count = (int)(Start * (1 - pr) + End * pr);
            if (count < 0) return "";
            else if (count >= String.Length) return "";
            else return String[count].ToString();
        }

        public override object Clone()
        {
            return new TLStringSingleChar() { String = this.String, Start = this.Start, End = this.End };
        }

        public override void Save(FileStream fs)
        {
            StreamHelper.SaveString(fs, String);
            StreamHelper.SaveBytes(fs, Start);
            StreamHelper.SaveBytes(fs, End);
        }
        public override void Load(FileStream fs)
        {
            String = StreamHelper.LoadString(fs);
            Start = StreamHelper.LoadInt(fs);
            End = StreamHelper.LoadInt(fs);
        }
    }
    //0x18 - 0x19
    public class TLComb1 : TimelineMediaType
    {
        public int Number;

        public override uint DataType => 6;
        public override uint MediaType => 24;

        public override LinearCombination GetLinear(Project p, TimelineMedia parent, ulong partitioning, float TimeBeat_Partitioned)
        {
            return new LinearCombination(Number);
        }

        public override object Clone()
        {
            return new TLComb1() { Number = this.Number };
        }

        public override void Save(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, Number);
        }
        public override void Load(FileStream fs)
        {
            Number = StreamHelper.LoadInt(fs);
        }
    }
    public class TLComb2 : TimelineMediaType
    {
        public int Number0;
        public int Number1;

        public override uint DataType => 6;
        public override uint MediaType => 25;

        public override LinearCombination GetLinear(Project p, TimelineMedia parent, ulong partitioning, float TimeBeat_Partitioned)
        {
            float pr = (TimeBeat_Partitioned - parent.TimeFrom) / (float)parent.TimeLenght;
            return new LinearCombination(Number0, Number1, pr);
        }

        public override object Clone()
        {
            return new TLComb2() { Number0 = this.Number0, Number1 = this.Number1 };
        }

        public override void Save(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, Number0);
            StreamHelper.SaveBytes(fs, Number1);
        }
        public override void Load(FileStream fs)
        {
            Number0 = StreamHelper.LoadInt(fs);
            Number1 = StreamHelper.LoadInt(fs);
        }
    }
    //0x20 - 0x21
    public class TLFalse : TimelineMediaType
    {
        public override uint DataType => 8;
        public override uint MediaType => 32;

        public override bool GetBool(Project p, TimelineMedia parent, ulong partitioning, float TimeBeat_Partitioned)
        {
            return false;
        }

        public override object Clone()
        {
            return new TLFalse();
        }
    }
    public class TLTrue : TimelineMediaType
    {
        public override uint DataType => 8;
        public override uint MediaType => 33;

        public override bool GetBool(Project p, TimelineMedia parent, ulong partitioning, float TimeBeat_Partitioned)
        {
            return true;
        }

        public override object Clone()
        {
            return new TLTrue();
        }
    }
}
