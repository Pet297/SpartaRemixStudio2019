using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpartaRemixStudio2019
{
    public static class StreamHelper
    {
        public static void SaveBytes(FileStream fs, float input)
        {
            byte[] b = BitConverter.GetBytes(input);
            fs.Write(b, 0, b.Length);
        }
        public static void SaveBytes(FileStream fs, short input)
        {
            byte[] b = BitConverter.GetBytes(input);
            fs.Write(b, 0, b.Length);
        }
        public static void SaveBytes(FileStream fs, ushort input)
        {
            byte[] b = BitConverter.GetBytes(input);
            fs.Write(b, 0, b.Length);
        }
        public static void SaveBytes(FileStream fs, int input)
        {
            byte[] b = BitConverter.GetBytes(input);
            fs.Write(b, 0, b.Length);
        }
        public static void SaveBytes(FileStream fs, uint input)
        {
            byte[] b = BitConverter.GetBytes(input);
            fs.Write(b, 0, b.Length);
        }
        public static void SaveBytes(FileStream fs, long input)
        {
            byte[] b = BitConverter.GetBytes(input);
            fs.Write(b, 0, b.Length);
        }
        public static void SaveBytes(FileStream fs, ulong input)
        {
            byte[] b = BitConverter.GetBytes(input);
            fs.Write(b, 0, b.Length);
        }
        public static void SaveBytes(FileStream fs, bool input)
        {
            byte[] b = BitConverter.GetBytes(input);
            fs.Write(b, 0, b.Length);
        }

        public static void SaveString(FileStream fs, string input)
        {
            byte[] b = Encoding.UTF32.GetBytes(input);
            SaveBytes(fs, b.Length);
            fs.Write(b, 0, b.Length);
        }

        public static short LoadShort(FileStream fs)
        {
            byte[] b = new byte[2];
            fs.Read(b, 0, 2);
            return BitConverter.ToInt16(b, 0);
        }
        public static ushort LoadUShort(FileStream fs)
        {
            byte[] b = new byte[2];
            fs.Read(b, 0, 2);
            return BitConverter.ToUInt16(b, 0);
        }
        public static int LoadInt(FileStream fs)
        {
            byte[] b = new byte[4];
            fs.Read(b, 0, 4);
            return BitConverter.ToInt32(b, 0);
        }
        public static uint LoadUInt(FileStream fs)
        {
            byte[] b = new byte[4];
            fs.Read(b, 0, 4);
            return BitConverter.ToUInt32(b, 0);
        }
        public static long LoadLong(FileStream fs)
        {
            byte[] b = new byte[8];
            fs.Read(b, 0, 8);
            return BitConverter.ToInt64(b, 0);
        }
        public static ulong LoadULong(FileStream fs)
        {
            byte[] b = new byte[8];
            fs.Read(b, 0, 8);
            return BitConverter.ToUInt64(b, 0);
        }
        public static float LoadFloat(FileStream fs)
        {
            byte[] b = new byte[4];
            fs.Read(b, 0, 4);
            return BitConverter.ToSingle(b, 0);
        }
        public static bool LoadBool(FileStream fs)
        {
            byte[] b = new byte[1];
            fs.Read(b, 0, 1);
            return BitConverter.ToBoolean(b, 0);
        }

        public static string LoadString(FileStream fs)
        {
            int l = LoadInt(fs);
            byte[] b = new byte[l];
            fs.Read(b, 0, l);

            return Encoding.UTF32.GetString(b);
        }
    }
    public static class StreamHelperList
    {
        public static void SaveBytes(FileStream fs, List<float> input)
        {
            StreamHelper.SaveBytes(fs, input.Count);
            foreach (float f in input) StreamHelper.SaveBytes(fs, f);
        }
        public static void SaveBytes(FileStream fs, List<int> input)
        {
            StreamHelper.SaveBytes(fs, input.Count);
            foreach (int f in input) StreamHelper.SaveBytes(fs, f);
        }
        public static void SaveBytes(FileStream fs, List<long> input)
        {
            StreamHelper.SaveBytes(fs, input.Count);
            foreach (long f in input) StreamHelper.SaveBytes(fs, f);
        }
        public static void SaveBytes(FileStream fs, List<ulong> input)
        {
            StreamHelper.SaveBytes(fs, input.Count);
            foreach (ulong f in input) StreamHelper.SaveBytes(fs, f);
        }
        public static void SaveBytes(FileStream fs, List<bool> input)
        {
            StreamHelper.SaveBytes(fs, input.Count);
            foreach (bool f in input) StreamHelper.SaveBytes(fs, f);
        }
        public static void SaveBytes(FileStream fs, List<string> input)
        {
            StreamHelper.SaveBytes(fs, input.Count);
            foreach (string f in input) StreamHelper.SaveString(fs, f);
        }

        public static List<int> LoadIntL(FileStream fs)
        {
            int l = StreamHelper.LoadInt(fs);
            List<int> output = new List<int>();
            for (int i = 0; i < l; i++) output.Add(StreamHelper.LoadInt(fs));
            return output;
        }
        public static List<long> LoadLongL(FileStream fs)
        {
            int l = StreamHelper.LoadInt(fs);
            List<long> output = new List<long>();
            for (int i = 0; i < l; i++) output.Add(StreamHelper.LoadLong(fs));
            return output;
        }
        public static List<ulong> LoadULongL(FileStream fs)
        {
            int l = StreamHelper.LoadInt(fs);
            List<ulong> output = new List<ulong>();
            for (int i = 0; i < l; i++) output.Add(StreamHelper.LoadULong(fs));
            return output;
        }
        public static List<float> LoadFloatL(FileStream fs)
        {
            int l = StreamHelper.LoadInt(fs);
            List<float> output = new List<float>();
            for (int i = 0; i < l; i++) output.Add(StreamHelper.LoadFloat(fs));
            return output;
        }
        public static List<bool> LoadBoolL(FileStream fs)
        {
            int l = StreamHelper.LoadInt(fs);
            List<bool> output = new List<bool>();
            for (int i = 0; i < l; i++) output.Add(StreamHelper.LoadBool(fs));
            return output;
        }

        public static List<string> LoadStringL(FileStream fs)
        {
            int l = StreamHelper.LoadInt(fs);
            List<string> output = new List<string>();
            for (int i = 0; i < l; i++) output.Add(StreamHelper.LoadString(fs));
            return output;
        }
    }

    public static class SRDHelper
    {
        public static void SerializeVideoSource(FileStream fs, SampleAV sav)
        {
            if (sav.ivs == null) StreamHelper.SaveBytes(fs,(int)-1);
            else if (sav.ivs is VideoSourceS)
            {
                StreamHelper.SaveBytes(fs, (int)0);
                StreamHelper.SaveString(fs, (sav.ivs as VideoSourceS).path);
            }
            else if (sav.ivs is QuickLoadVideoSample)
            {
                StreamHelper.SaveBytes(fs, (int)3);
                StreamHelper.SaveBytes(fs, (int)(sav.ivs as QuickLoadVideoSample).SampleVideoSource);
                StreamHelper.SaveBytes(fs, (sav.ivs as QuickLoadVideoSample).TimeSecIn);
                StreamHelper.SaveBytes(fs, (sav.ivs as QuickLoadVideoSample).defaultSpd);
            }
            else if (sav.ivs is BitmapSource)
            {
                StreamHelper.SaveBytes(fs, (int)6);
                StreamHelper.SaveString(fs, (sav.ivs as BitmapSource).path);
            }
            else StreamHelper.SaveBytes(fs, (int)-1);
        }
        public static void SerializeAllNoteEvents(FileStream fs, VideoTrack vt)
        {
            List<EventDescriptor> desc = new List<EventDescriptor>();
            foreach (uint u in vt.Media)
            {
                if (Form1.Project.ProjectMedia[u].Property is TLPattern)
                {
                    long startOff = (long)Form1.Project.ProjectMedia[u].TimeFrom - (long)Form1.Project.ProjectMedia[u].BeatIn;
                    foreach (Note n in Form1.Project.ProjectPatterns[(Form1.Project.ProjectMedia[u].Property as TLPattern).Pattern].ActivationTrack.Notes)
                    {
                        if (n.BeatNoteOn >= (long)Form1.Project.ProjectMedia[u].BeatIn && n.BeatNoteOn < (long)Form1.Project.ProjectMedia[u].TimeLenght)
                        {
                            desc.Add(EventDescriptor.CreateNoteOn(
                            startOff + n.BeatNoteOn,
                            (int)(Form1.Project.ProjectMedia[u].Property as TLPattern).Samples[(int)n.Sample],
                            0,
                            1,
                            Form1.Project.ProjectMedia[u].Volume + n.volume,
                            Form1.Project.ProjectMedia[u].Pitch + n.Pitch,
                            Form1.Project.ProjectMedia[u].Opacity * n.opacity));
                            desc.Add(EventDescriptor.CreateNoteOff(startOff + n.BeatNoteOff, 0));
                        }
                    }
                }
                else if (Form1.Project.ProjectMedia[u].Property is TLSample)
                {
                    desc.Add(EventDescriptor.CreateNoteOn((long)Form1.Project.ProjectMedia[u].TimeFrom,
                        (int)(Form1.Project.ProjectMedia[u].Property as TLSample).Sample,
                        Form1.Project.ProjectMedia[u].SecondsIn,
                        Form1.Project.ProjectMedia[u].Stretch,
                        Form1.Project.ProjectMedia[u].Volume,
                        Form1.Project.ProjectMedia[u].Pitch,
                        Form1.Project.ProjectMedia[u].Opacity));
                    desc.Add(EventDescriptor.CreateNoteOff((long)Form1.Project.ProjectMedia[u].TimeEnd,0));
                }
            }
            desc.Sort((a, b) => a.CompareTo(b));
            StreamHelper.SaveBytes(fs, (int)desc.Count);
            foreach (EventDescriptor ed in desc)
            {
                SerializeNoteEvent(fs, ed);
            }
        }
        public static void SerializeNoteEvent(FileStream fs, EventDescriptor evd)
        {
            StreamHelper.SaveBytes(fs, evd.Word);
            StreamHelper.SaveBytes(fs, evd.Time192);
            StreamHelper.SaveBytes(fs, evd.Data.Length);
            fs.Write(evd.Data, 0, evd.Data.Length);
        }
        public class EventDescriptor : IComparable
        {
            public uint Word = 0xFFFFFFFF;
            public long Time192 = 0;
            public byte[] Data = new byte[0];

            public static EventDescriptor CreateNoteOn(long time, int sample, float timeInSample, float speed,float velocity, float pitch, float opacity)
            {
                byte[] p0 = BitConverter.GetBytes(sample);
                byte[] p1 = BitConverter.GetBytes(timeInSample);
                byte[] p2 = BitConverter.GetBytes(speed);
                byte[] p3 = BitConverter.GetBytes(velocity);
                byte[] p4 = BitConverter.GetBytes(pitch);
                byte[] p5 = BitConverter.GetBytes(opacity);

                return new EventDescriptor()
                {

                    Word = 0x4e4f544e,
                    Time192 = time,
                    Data = new byte[24]
                    {
                        p0[0],p0[1],p0[2],p0[3],
                        p1[0],p1[1],p1[2],p1[3],
                        p2[0],p2[1],p2[2],p2[3],
                        p3[0],p3[1],p3[2],p3[3],
                        p4[0],p4[1],p4[2],p4[3],
                        p5[0],p5[1],p5[2],p5[3],
                    }
                };
            }
            public static EventDescriptor CreateNoteOff(long time, float release)
            {
                byte[] p0 = BitConverter.GetBytes(release);

                return new EventDescriptor()
                {
                    Word = 0x4e4f5446,
                    Time192 = time,
                    Data = new byte[4]
                    {
                        p0[0],p0[1],p0[2],p0[3],
                    }
                };
            }

            public int CompareTo(object obj)
            {
                if (obj is EventDescriptor)
                {
                    if (Time192 != (obj as EventDescriptor).Time192) return Time192 - (obj as EventDescriptor).Time192 > 0 ? 1 : -1;
                    else
                    {
                        if (Word == 0x4e4f544e && (obj as EventDescriptor).Word == 0x4e4f5446) return 1;
                        else if (Word == 0x4e4f5446 && (obj as EventDescriptor).Word == 0x4e4f544e) return -1;
                        else return 0;
                    }
                }
                return 1;
            }
        }
    }
}
