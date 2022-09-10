using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SpartaRemixStudio2019
{
    public interface IPitchSample
    {
        int PreviewTexture { get; }
        bool BeatSync { get; }
        bool AffectedByMasterPitch { get; }
        PitchSampleFlags Possibilities { get; }

        IPitchReader GetReader(float time);

        void SaveArbData(FileStream fs);
        void LoadArbData(FileStream fs);
        void PostLoad();
        ushort Type { get; }
    }
    public interface IPitchReader : IDisposable
    {
        void SetProperty(byte index, float value);

        void ReadAdd(ref float[] samples, float L, float R);
        void NoteOff();

        bool ReleaseEnded { get; }
    }
    public class PitchReader : INoteReciever2
    {
        Dictionary<long, NotePitchReader> samplers;
        List<uint> ptSamples;

        List<long> toRemove = new List<long>();
        public float pitchOffset = 0;

        float[] PROPERTIES = new float[256];
        float[] buffer = new float[0];
        public void ReadAdd(ref float[] samples, float TimeBeat0, float TimeBeat1)
        {
            float beatSmp = samples.Length / 2f / (TimeBeat1 - TimeBeat0);

            float VOL = 1;
            float PTC = 0;
            float PAN = 0;
            float SPD = 1;
            float FMT = 0;

            VOL *= (float)Math.Pow(10, PROPERTIES[0] / 10);
            PAN += PROPERTIES[1];
            SPD *= PROPERTIES[2];
            PTC += PROPERTIES[3];
            FMT += PROPERTIES[5];

            if (buffer.Length != samples.Length) buffer = new float[samples.Length];
            Array.Clear(buffer, 0, buffer.Length);


            toRemove.Clear();
            foreach (KeyValuePair<long, NotePitchReader> npr in samplers)
            {
                npr.Value.s.SetProperty(2, SPD);
                npr.Value.s.SetProperty(3, PTC + npr.Value.n.Pitch + Form1.Project.GetMasterPitch(TimeBeat0) + pitchOffset);
                npr.Value.s.SetProperty(5, FMT);
                float P = Math.Min(Math.Max(npr.Value.n.pan, -1), 1);
                float R = (float)Math.Pow(10, npr.Value.n.volume / 10) * (float)((Math.Cos(P * Math.PI / 4) - Math.Sin(P * Math.PI / 4)));
                float L = (float)Math.Pow(10, npr.Value.n.volume / 10) * (float)((Math.Cos(P * Math.PI / 4) + Math.Sin(P * Math.PI / 4)));
                npr.Value.s.ReadAdd(ref buffer, L, R); //<--- PER NOTE

                if (npr.Value.s.ReleaseEnded) toRemove.Add(npr.Key);
            }
            foreach (long l in toRemove) samplers.Remove(l);
            toRemove.Clear();

            //1. VOL + PAN
            PAN = Math.Min(Math.Max(PAN, -1), 1);
            float PL = VOL * (float)(Math.Sqrt(2) / 2 * (Math.Cos(PAN * Math.PI / 4) - Math.Sin(PAN * Math.PI / 4)));
            float PR = VOL * (float)(Math.Sqrt(2) / 2 * (Math.Cos(PAN * Math.PI / 4) + Math.Sin(PAN * Math.PI / 4)));
            for (int i = 0; i < samples.Length / 2; i++)
            {
                buffer[i * 2] *= PL;
                buffer[i * 2 + 1] *= PR;
            }

            for (int i = 0; i < samples.Length; i++) samples[i] += buffer[i];
        }
        void AddSampler(IPitchReader smp, Note n)
        {
            NotePitchReader npr = new NotePitchReader();
            npr.s = smp;
            npr.n = n;
        }
        public void SetProperty(byte index, float value)
        {
            PROPERTIES[index] = value;
        }

        public void NoteOn(Note n, long index)
        {
            if ((int)n.Sample >= 0 && (int)n.Sample < ptSamples.Count)
            {
                if (Form1.Project.ProjectSamples[ptSamples[(int)n.Sample]].ips != null)
                    samplers.Add(index, new NotePitchReader() { s = Form1.Project.ProjectSamples[ptSamples[(int)n.Sample]].ips.GetReader(0), n = n });
            }
        }
        public void NoteOff(long index)
        {
            if (samplers.ContainsKey(index)) samplers[index].s.NoteOff();
        }

        public bool ReleaseEnded
        {
            get
            {
                if (samplers.Count == 0) return true;
                else return false;
            }
        }

        public PitchReader(List<uint> ps)
        {
            samplers = new Dictionary<long, NotePitchReader>();

            PROPERTIES[2] = 1;
            PROPERTIES[8] = 999;
            PROPERTIES[10] = -999;
            PROPERTIES[14] = 1;
            PROPERTIES[36] = 0.125f;

            ptSamples = ps;
        }
    }
    public class PitchReaderSingle
    {
        IPitchReader sampler;

        float[] buffer = new float[0];
        public void ReadAdd(ref float[] samples, float TimeBeat0, float TimeBeat1)
        {
            float beatSmp = samples.Length / 2f / (TimeBeat1 - TimeBeat0);

            if (buffer.Length != samples.Length) buffer = new float[samples.Length];
            Array.Clear(buffer, 0, buffer.Length);


            sampler.SetProperty(2, SPD);
            sampler.SetProperty(3, PTC + Form1.Project.GetMasterPitch(TimeBeat0));
            sampler.SetProperty(5, FMT);
            float P = Math.Min(Math.Max(PAN, -1), 1);
            float R = (float)((Math.Cos(P * Math.PI / 4) - Math.Sin(P * Math.PI / 4)));
            float L = (float)((Math.Cos(P * Math.PI / 4) + Math.Sin(P * Math.PI / 4)));
            sampler.ReadAdd(ref buffer, L * VOL, R * VOL);

            //1. VOL + PAN
            PAN = Math.Min(Math.Max(PAN, -1), 1);
            float PL = VOL * (float)(Math.Sqrt(2) / 2 * (Math.Cos(PAN * Math.PI / 4) - Math.Sin(PAN * Math.PI / 4)));
            float PR = VOL * (float)(Math.Sqrt(2) / 2 * (Math.Cos(PAN * Math.PI / 4) + Math.Sin(PAN * Math.PI / 4)));
            for (int i = 0; i < samples.Length / 2; i++)
            {
                buffer[i * 2] *= PL;
                buffer[i * 2 + 1] *= PR;
            }

            for (int i = 0; i < samples.Length; i++) samples[i] += buffer[i];
        }
        public void NoteOff()
        {
            sampler.NoteOff();
        }

        public float VOL = 1;
        public float PTC = 0;
        public float PAN = 0;
        public float SPD = 1;
        public float FMT = 0;

        public bool ReleaseEnded
        {
            get
            {
                if (sampler != null) return sampler.ReleaseEnded;
                else return true;
            }
        }

        public PitchReaderSingle(IPitchReader ipr)
        {
            sampler = ipr;
        }
    }
    [Flags] public enum PitchSampleFlags { None = 0, Speed = 1, Pitch = 2, Formant = 4, Mult = 8 }
    public struct NotePitchReader
    {
        public Note n;
        public IPitchReader s;
    }

    public interface IVideoSample
    {
        System.Drawing.Bitmap PreviewBitmap { get; }
        int PreviewTexture { get; }
        float FPS { get; }
        bool BeatSync { get; }
        int FrameEnd { get; }
        int FrameStart { get; }

        bool RequiresReader { get; }

        TextureInfo GetFrame(float timeIn);
        IVideoReader GetReader(float time, int texture);

        void SaveArbData(FileStream fs);
        void LoadArbData(FileStream fs);
        void PostLoad();
        ushort Type { get; }
    }
    public interface IVideoReader : IDisposable
    {
        void SetProperty(byte index, float val);
        float GetPropertyValue(byte index);

        void Read(TextureInfo ti, float time);
        void NoteOff();
        bool ReleaseEnded { get; }
    }

    public struct SampleAV
    {
        public IVideoSample ivs;
        public IPitchSample ips;
        public string name;
        public SampleStatFX ssfx;
    }

    // SSFX
    public class SampleStatFX
    {
        public List<SSFX_Porta> effects = new List<SSFX_Porta>();

        public bool IsZero => effects.Count == 0;

        public void Save(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)effects.Count);

            foreach (SSFX_Porta ssfx in effects)
            {
                StreamHelper.SaveBytes(fs, (ushort)0);
                StreamHelper.SaveBytes(fs, (ushort)2);
                StreamHelper.SaveBytes(fs, ssfx.pitchOffset);
                StreamHelper.SaveBytes(fs, ssfx.timeSamp);
            }
        }
        public void Load(FileStream fs)
        {
            int c = StreamHelper.LoadInt(fs);

            for (int i = 0; i < c; i++)
            {
                SSFX_Porta ssfx = new SSFX_Porta();
                StreamHelper.LoadUShort(fs);
                StreamHelper.LoadUShort(fs);
                ssfx.pitchOffset = StreamHelper.LoadFloat(fs);
                ssfx.timeSamp = StreamHelper.LoadInt(fs);
                effects.Add(ssfx);
            }
        }
    }
    public class SSFX_Porta
    {
        public SSFX_Porta()
        {
            pitchOffset = -12;
            timeSamp = 1800;
        }
        public float pitchOffset;
        public int timeSamp;
    }
    // SSFX

    // !!! (SL na projekt)
    public static class SampleHelper
    {
        public static IPitchSample GetIPSByNumber(Project p, ushort type)
        {
            if (type == 0) return new VideoSourceS();
            if (type == 1) return new CorrectedPitchSample(new float[0], 1);
            if (type == 2) return new GranuleSample(new List<float[]>(), new List<int>(), 1, 0);
            if (type == 4) return new PitchBufferSample(new float[0], 1);
            if (type == 7) return new AudioCutSample(new float[0], false, false, 1, 0.03f, 0.00f, 1f, 0.03f, false);

            if (type == 0x14) return new PitchBufferSample2(new float[0], 1);

            return null;
        }
        public static IVideoSample GetIVSByNumber(Project p, ushort type)
        {
            if (type == 0) return new VideoSourceS();
            if (type == 3) return new QuickLoadVideoSample(0, 0, false, 0, 1);
            if (type == 6) return new BitmapSource("");
            return null;
        }
        public static Dictionary<string, VideoSourceS> loadedSources = new Dictionary<string, VideoSourceS>();
    }

    //DRUHY
    // AV typ 00 - ziskat FPS soubor a video-datovy soubor
    public class VideoSourceS : IDisposable, IVideoSample, IPitchSample
    {
        public ushort Type => 0;

        //soubor
        public string path;
        public int FPSn;
        public int FPSd;
        public int Width;
        public int Height;
        public float Duration;
        public int FrameCount;
        public int ArW;
        public int ArH;

        //reader data
        [NonSerialized] public byte[] wave;
        public bool audio = false;
        public bool video = false;
        public string directory;

        List<float> timeStamps = new List<float>();
        public List<Bitmap> keyFramePreviews = new List<Bitmap>();
        public Bitmap PreviewBitmap => keyFramePreviews.Count > 0 ? keyFramePreviews[keyFramePreviews.Count / 2] : null;

        //konstruktor
        public VideoSourceS(string p)
        {
            path = p;

            uint i = (uint)p.GetHashCode();
            Directory.CreateDirectory("VideoDescriptors\\" + i);
            directory = "VideoDescriptors\\" + i;
            if (!File.Exists(directory + "\\KeyFrames.txt")) VideoHelper.GetIFramesList(p, PathHelper.CurrentDirectory() + "\\" + directory + "\\KeyFrames.txt");
            if (!File.Exists(directory + "\\Stats.txt")) VideoHelper.GetInfoFile(p, PathHelper.CurrentDirectory() + "\\" + directory + "\\Stats.txt");


            InitAudio();
            timeStamps.Clear();

            StreamReader sr = new StreamReader(directory + "\\KeyFrames.txt");
            while (!sr.EndOfStream)
            {
                string s = sr.ReadLine();
                float f = float.Parse(s, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);
                timeStamps.Add(f);
            }
            sr.Close();
            sr.Dispose();

            StreamReader sr2 = new StreamReader(directory + "\\Stats.txt");
            string s1 = sr2.ReadLine();
            string s2 = sr2.ReadLine();

            sr2.Close();
            sr2.Dispose();

            try
            {
                string[] s1a = s1.Split('|');
                string[] s1a41 = s1a[3].Split('/');
                Width = int.Parse(s1a[0], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);
                Height = int.Parse(s1a[1], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);

                FPSn = int.Parse(s1a41[0], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);
                FPSd = int.Parse(s1a41[1], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);
                Duration = float.Parse(s2, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);
                FrameCount = (int)Math.Round(Duration * FPS);

                if (s1a[2] == "N/A")
                {
                    ArW = Width;
                    ArH = Height;
                }
                else
                {
                    string[] s1a31 = s1a[2].Split(':');
                    ArW = int.Parse(s1a31[0], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);
                    ArH = int.Parse(s1a31[1], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);
                }

                GetKeyFrames();
            }
            catch
            {
                FrameCount = 0;
            }
        }
        public VideoSourceS()
        {
        }

        //video
        int IVideoSample.PreviewTexture => 0;
        int IPitchSample.PreviewTexture => 0;

        public float FPS => FPSn / (float)FPSd;
        public bool BeatSync => false;
        public int FrameStart => 0;
        public int FrameEnd => FrameCount - 1;

        public bool RequiresReader => true;

        public bool AffectedByMasterPitch => false;

        public PitchSampleFlags Possibilities => PitchSampleFlags.None;


        //VLNA 20
        private void InitAudio()
        {
            if (File.Exists(directory + @"\sourceD20.txt"))
            {
                LoadHelperFile();
            }
            else
            {
                GenHelperFile();
                LoadHelperFile();
            }
            if (wave.Length == 1 && wave[0] == 0) audio = false;
            else audio = true;
        }
        private void GenHelperFile()
        {
            FileStream fs = new FileStream(directory + @"\sourceD20.txt", FileMode.Create);
            try
            {
                AudioFileReader afr = new AudioFileReader(path);
                NAudio.Wave.SampleProviders.WdlResamplingSampleProvider wdl = new NAudio.Wave.SampleProviders.WdlResamplingSampleProvider(afr, 48000);

                float[] buf = new float[afr.WaveFormat.Channels * 2400];

                while (wdl.Read(buf, 0, buf.Length) > 0)
                {
                    float max = 0;
                    for (int i = 0; i < buf.Length; i++) if (Math.Abs(buf[i]) > max) max = Math.Abs(buf[i]);
                    fs.WriteByte((byte)(max * 255f));
                }


                fs.Close();
                fs.Dispose();
                afr.Close();
                afr.Dispose();
                wdl = null;
            }
            catch
            {
                fs.WriteByte(0);
                fs.Close();
                fs.Dispose();
            }

        }
        private void LoadHelperFile()
        {
            wave = File.ReadAllBytes(directory + @"\sourceD20.txt");
        }
        private void GetKeyFrames()
        {
            VideoSource vs = vs = new VideoSource(path);
            int index = 0;
            foreach (float ts in timeStamps)
            {
                if (!File.Exists(directory + @"\kf" + index + @".png"))
                {
                    vs.SeekTo(ts);
                    Bitmap b = vs.extractBitmap();
                    Bitmap c = new Bitmap(b, 40, 30);

                    b.Dispose();
                    keyFramePreviews.Add(c);

                    c.Save(directory + @"\kf" + index + @".png");
                }
                else
                {
                    Bitmap c = new Bitmap(directory + @"\kf" + index + @".png");
                    keyFramePreviews.Add(c);
                }
                index++;
            }
            if (timeStamps.Count == 0) video = false;
            else video = true;

        }
        public Bitmap GetKeyFrame(float timeSec)
        {
            if (keyFramePreviews.Count == 0) return null;
            int index = 0;
            while (index < timeStamps.Count)
            {
                if (timeSec > timeStamps[index])
                {
                    index++;
                }
                else break;
            }
            if (index > 0) index--;
            return keyFramePreviews[index];
        }

        public void DrawWaveForm(int x, int y, int h, int w, float s, float l, Color b, Color f, Graphics g)
        {
            System.Drawing.Rectangle R = new System.Drawing.Rectangle(x, y, h, w);
            float waveFrom = s;
            float waveLenght = l;
            float waveTo = s + l;


            float t = (waveFrom * 20f);
            float inc = (waveLenght * 20f) / R.Width;
            float end = ((waveFrom + waveLenght) * 20f);
            int pX = 0;
            if (inc > 0.01f) for (float i = t; i < end; i += inc)
                {
                    if (i >= 0 && i < wave.Length)
                    {
                        float min = -wave[(int)i] / 256f;
                        float max = wave[(int)i] / 256f;
                        min = min / 2 + 0.5f;
                        max = max / 2 + 0.5f;
                        g.DrawLine(new System.Drawing.Pen(f), R.X + pX, R.Y + min * R.Height, R.X + pX, R.Y + max * R.Height);
                    }
                    pX++;
                }


        }
        public void DrawWaveForm2(int x, int y, int h, int w, float s, float l, Color b, Color f, Graphics g)
        {
            System.Drawing.Rectangle R = new System.Drawing.Rectangle(x, y, h, w);
            float waveFrom = s;
            float waveLenght = l;
            float waveTo = s + l;


            float t = (waveFrom * 20f);
            float inc = (waveLenght * 20f) / R.Width;
            float end = ((waveFrom + waveLenght) * 20f);
            int pX = 0;
            for (float i = t; i < end; i += inc)
            {
                if (i >= 0 && i < wave.Length)
                {
                    float max = wave[(int)i] / 256f;
                    max = max / 2 + 0.5f;
                    g.DrawLine(new System.Drawing.Pen(f), R.X + pX, R.Y + R.Height, R.X + pX, R.Y + (1 - max) * R.Height
                        );
                }
                pX++;
            }


        }
        public List<string> GetExpectedExtensions()
        {
            return new List<string> { ".mp3", ".mp4", ".wav", ".ogg", ".ogv", ".wma", ".wmv", ".avi" };
        }

        public void Dispose()
        {

        }
        public void GenerateT2D()
        {
        }

        TextureInfo IVideoSample.GetFrame(float timeIn) => new TextureInfo(0, 0, 0);
        public IVideoReader GetReader(float time, int tex) => new VideoSourceVReader(path, time, this, tex);
        IPitchReader IPitchSample.GetReader(float time)
        {
            AudioFileReader isp = null;
            try
            {
                isp = new AudioFileReader(path);
                isp.Position = (long)(isp.WaveFormat.AverageBytesPerSecond * Math.Max(0, time - 4096f / isp.WaveFormat.SampleRate)) / 16 * 16;
                isp.Read(new float[4096], 0, 4096);
            }
            catch
            {
                return null;
            }
            return new VideoSourceSReader(isp, time);
        }

        //SAVE LOAD
        public void SaveArbData(FileStream fs)
        {
            StreamHelper.SaveString(fs, path);
        }
        public void LoadArbData(FileStream fs)
        {
            path = StreamHelper.LoadString(fs);

            uint i = (uint)path.GetHashCode();
            Directory.CreateDirectory("VideoDescriptors\\" + i);
            directory = "VideoDescriptors\\" + i;
            if (!File.Exists(directory + "\\KeyFrames.txt")) VideoHelper.GetIFramesList(path, PathHelper.CurrentDirectory() + "\\" + directory + "\\KeyFrames.txt");
            if (!File.Exists(directory + "\\Stats.txt")) VideoHelper.GetInfoFile(path, PathHelper.CurrentDirectory() + "\\" + directory + "\\Stats.txt");


            InitAudio();
            timeStamps.Clear();

            StreamReader sr = new StreamReader(directory + "\\KeyFrames.txt");
            while (!sr.EndOfStream)
            {
                string s = sr.ReadLine();
                float f = float.Parse(s, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);
                timeStamps.Add(f);
            }
            sr.Close();
            sr.Dispose();

            StreamReader sr2 = new StreamReader(directory + "\\Stats.txt");
            string s1 = sr2.ReadLine();
            string s2 = sr2.ReadLine();

            sr2.Close();
            sr2.Dispose();
            try
            {
                string[] s1a = s1.Split('|');
                string[] s1a41 = s1a[3].Split('/');
                Width = int.Parse(s1a[0], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);
                Height = int.Parse(s1a[1], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);

                FPSn = int.Parse(s1a41[0], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);
                FPSd = int.Parse(s1a41[1], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);
                Duration = float.Parse(s2, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);
                FrameCount = (int)Math.Round(Duration * FPS);

                if (s1a[2] == "N/A")
                {
                    ArW = Width;
                    ArH = Height;
                }
                else
                {
                    string[] s1a31 = s1a[2].Split(':');
                    ArW = int.Parse(s1a31[0], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);
                    ArH = int.Parse(s1a31[1], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);
                }

                GetKeyFrames();
            }
            catch
            {
                FrameCount = 0;
            }
        }
        public void PostLoad()
        {
        }

        public float GetTimeStamp(float time)
        {
            int i = 0;
            while (timeStamps[i] <= time)
            {
                i++;
                if (i == timeStamps.Count) return timeStamps[i - 1];
            }
            if (i == 0) i = 1;
            return timeStamps[i - 1];
        }
    }
    public class VideoSourceSReader : IPitchReader
    {
        public VideoSourceSReader(AudioFileReader afr, float time)
        {
            this.afr = afr;
            this.isp = afr;

            buffer = new float[0];

            //PROPERTIES[2] = 0;
            //PROPERTIES[3] = 0;
            //PROPERTIES[4] = 0;
            //PROPERTIES[5] = 0;

            if (this.isp.WaveFormat.SampleRate != 48000)
            {
                NAudio.Wave.SampleProviders.WdlResamplingSampleProvider wdl = new NAudio.Wave.SampleProviders.WdlResamplingSampleProvider(this.isp, 48000);
                this.isp = wdl;
            }
            if (this.isp.WaveFormat.Channels == 1)
            {
                NAudio.Wave.SampleProviders.MonoToStereoSampleProvider msp = new NAudio.Wave.SampleProviders.MonoToStereoSampleProvider(this.isp);
                this.isp = msp;
            }
        }

        private AudioFileReader afr;
        private ISampleProvider isp;
        private float[] buffer;

        private float[] PROPERTIES = new float[256];
        public void SetProperty(byte index, float val)
        {
            PROPERTIES[index] = val;
        }
        public float GetPropertyValue(byte index)
        {
            return PROPERTIES[index];
        }

        bool end = false;
        public void NoteOff()
        {
            end = true;
        }

        public void Dispose()
        {
            afr.Close();
            afr.Dispose();
        }

        public void ReadAdd(ref float[] samples, float L, float R)
        {
            if (buffer.Length != samples.Length) buffer = new float[samples.Length];
            Array.Clear(buffer, 0, samples.Length);
            //PROC
            isp.Read(buffer, 0, buffer.Length);
            //PROC
            for (int i = 0; i < samples.Length; i++) samples[i] += buffer[i];
        }

        public bool ReleaseEnded => end;
    }
    public class VideoSourceVReader : IVideoReader
    {
        public VideoSource vs;
        VideoSourceS parent;
        float ts = 0;

        public VideoSourceVReader(string path, float timeSec, VideoSourceS parent, int tex)
        {
            if (tex != 0) vs = new VideoSource(path, tex);
            else vs = new VideoSource(path);
            PROPERTIES[49] = timeSec;
            this.parent = parent;
            ts = timeSec;

            vs.SeekTo(parent.GetTimeStamp(timeSec));
        }

        public bool ReleaseEnded => releaseEnded;
        private bool releaseEnded = false;
        float[] PROPERTIES = new float[256];

        public void Dispose()
        {
            vs.Dispose();
        }

        public float GetPropertyValue(byte index)
        {
            return PROPERTIES[index];
        }

        public void NoteOff()
        {
            releaseEnded = true;
        }

        public void SetProperty(byte index, float val)
        {
            PROPERTIES[index] = val;
        }

        // Frames needed
        public void Read(TextureInfo ti, float time)
        {
            ti.Reset();

            vs.readFrame((Math.Max(ts, time)) * 48000);

            ti.Width = parent.ArW;
            ti.Height = parent.ArH;
            ti.TextureIndex = vs.Tex;
            ti.FlipY();
        }

        public int[] ReadExtract(int startFrame, int count)
        {
            int[] textureNumbers = new int[count];

            if (count > 0)
            {
                textureNumbers[0] = vs.extractFrameNumber(startFrame);
                for (int i = 1; i < count; i++) textureNumbers[i] = vs.extractFrameNumber(startFrame);
            }

            return textureNumbers;
        }
        public int TimeToFrame(float timeSec)
        {
            return (int)(vs.FPS * timeSec);
        }
    }

    // 999
    public class TestSamp : IPitchSample
    {
        public int PreviewTexture => 0;
        public bool BeatSync => false;
        public bool AffectedByMasterPitch => true;
        public PitchSampleFlags Possibilities => PitchSampleFlags.Pitch;
        public ushort Type => 999;
        public IPitchReader GetReader(float time)
        {
            return new TestReader();
        }
        public void LoadArbData(FileStream fs)
        {
        }
        public void SaveArbData(FileStream fs)
        {
        }
        public void PostLoad()
        {
        }
    }
    public class TestReader : IPitchReader
    {
        public void Dispose()
        {
        }
        public void NoteOff()
        {
            no = true;
        }
        bool no = false;

        public void ReadAdd(ref float[] samples, float L, float R)
        {
            for (int i = 0; i < samples.Length; i += 2)
            {
                pos++;
                if (pos > period)
                {
                    samples[i] += 1;
                    //samples[i + 1] += 1;
                    pos -= period;
                }
            }
        }

        float period = 48000f;
        float pos = 0;

        public void SetProperty(byte index, float value)
        {
            if (index == 3) period = 48000f / (float)(440 * Math.Pow(2, value / 12));
        }

        public bool ReleaseEnded => true;
    }

    // A typ 01
    public class CorrectedPitchSample : IPitchSample
    {
        public float[] Audio;
        public float pitch;
        public float speedOffset;
        public float formantOffset;

        public float A;
        public float D;
        public float S;
        public float R;


        public int PreviewTexture => 0;
        public bool BeatSync => false;
        public bool AffectedByMasterPitch => true;
        public PitchSampleFlags Possibilities => PitchSampleFlags.Speed | PitchSampleFlags.Pitch;
        public ushort Type => 1;
        public IPitchReader GetReader(float time)
        {
            return new CorrectedPitchReader(this, time);
        }

        public void SaveArbData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, pitch);
            StreamHelper.SaveBytes(fs, speedOffset);
            StreamHelper.SaveBytes(fs, formantOffset);

            StreamHelper.SaveBytes(fs, Audio.Length);

            for (int i = 0; i < Audio.Length; i++) StreamHelper.SaveBytes(fs, Audio[i]);
        }
        public void LoadArbData(FileStream fs)
        {
            pitch = StreamHelper.LoadFloat(fs);
            speedOffset = StreamHelper.LoadFloat(fs);
            formantOffset = StreamHelper.LoadFloat(fs);

            int l = StreamHelper.LoadInt(fs);
            Audio = new float[l];

            for (int i = 0; i < l; i++) Audio[i] = StreamHelper.LoadFloat(fs);
        }
        public void PostLoad()
        {
        }

        public float this[float sample]
        {
            get
            {

                sample *= 4;
                if (sample < 1) return 0;
                else if (sample >= Audio.Length - 2) return 0;
                else if (sample % 1 == 0f) return Audio[(int)sample];
                else
                {
                    return Audio[(int)sample];
                    fp = sample % 1f;
                    return (1 - fp) * Audio[(int)sample] + fp * Audio[(int)sample + 1];
                }
            }
        }
        float fp = 0;
        public CorrectedPitchSample(float[] samples, float freq)
        {
            //Audio = samples;
            pitch = freq;
            speedOffset = 1;

            ISampleProvider mfr = new NAudio.Wave.SampleProviders.WaveToSampleProvider(new MediaFoundationResampler(new NAudio.Wave.SampleProviders.SampleToWaveProvider(new FloatArraySource(samples, 48000)), WaveFormat.CreateIeeeFloatWaveFormat(4 * 48000, 1)));
            float[] newSample = new float[samples.Length * 4];
            mfr.Read(newSample, 0, newSample.Length);
            Audio = newSample;
        }
        public CorrectedPitchSample(float[] samples, float freq, float speed)
        {
            //Audio = samples;
            pitch = freq;
            speedOffset = speed;

            ISampleProvider mfr = new NAudio.Wave.SampleProviders.WaveToSampleProvider(new MediaFoundationResampler(new NAudio.Wave.SampleProviders.SampleToWaveProvider(new FloatArraySource(samples, 48000)), WaveFormat.CreateIeeeFloatWaveFormat(4 * 48000, 1)));
            float[] newSample = new float[samples.Length * 4];
            mfr.Read(new float[200], 0, 200);
            mfr.Read(newSample, 0, newSample.Length);
            Audio = newSample;
        }
        public CorrectedPitchSample(CorrectedPitchSample cps)
        {
            Audio = (float[])cps.Audio.Clone();
            pitch = cps.pitch;
            speedOffset = cps.speedOffset;
            formantOffset = cps.formantOffset;

            A = cps.A;
            D = cps.D;
            S = cps.S;
            R = cps.R;
        }
    }
    public class CorrectedPitchReader : IPitchReader
    {
        float relPos = 0;
        float[] buffer = new float[0];
        CorrectedPitchSample cps;
        ADSR adsr;

        public CorrectedPitchReader(CorrectedPitchSample cps)
        {
            adsr = new ADSR(0.03f, 0, 1, 0.03f);
            this.cps = cps;
            PROPERTIES[4] = 0;
            PROPERTIES[2] = 1;
        }

        public CorrectedPitchReader(CorrectedPitchSample cps, float timeSec)
        {
            adsr = new ADSR(0.03f, 0, 1, 0.03f);
            this.cps = cps;
            PROPERTIES[4] = timeSec * 48000;
            PROPERTIES[2] = 1;
        }

        private float Read()
        {

            float Frequency = 440f * (float)Math.Pow(2, PROPERTIES[3] / 12f);
            float Speed = PROPERTIES[2] * cps.speedOffset;

            float actualHz = cps.pitch;
            float mult = Frequency / actualHz;
            float fundamentalLenght = 48000f / actualHz;

            //pozice
            float relativniZmena = mult - Speed;
            relativniZmena = relativniZmena / fundamentalLenght;

            //ziskani
            float samplePos1 = PROPERTIES[4] + (relPos * fundamentalLenght);
            float samplePos2 = PROPERTIES[4] + ((relPos - 1) * fundamentalLenght);

            float vol2 = relPos;
            float vol1 = 1 - vol2;

            float output = 0;
            output += vol1 * cps[samplePos1];
            output += vol2 * cps[samplePos2];

            PROPERTIES[4] += Speed;
            relPos += relativniZmena;
            relPos %= 1f;

            return output;
        }

        private float[] PROPERTIES = new float[256];
        public void SetProperty(byte index, float val)
        {
            PROPERTIES[index] = val;
        }
        public float GetPropertyValue(byte index)
        {
            return PROPERTIES[index];
        }
        public string GetPropertyName(byte index)
        {
            if (index == 0) return "Volume (dB)";
            else if (index == 1) return "Pan (-1 to 1)";
            else if (index == 2) return "Speed";
            else if (index == 3) return "Pitch";
            else if (index == 4) return "Position";

            else return null;
        }

        bool end = false;
        public void NoteOff()
        {
            end = true;
        }

        public void ReadAdd(ref float[] samples, float L, float R)
        {
            if (end) adsr.Release();

            if (buffer.Length != samples.Length) buffer = new float[samples.Length];
            Array.Clear(buffer, 0, buffer.Length);
            //PROC
            for (int i = 0; i < samples.Length / 2; i++)
            {
                float f = Read();
                buffer[i * 2] += f * L;
                buffer[i * 2 + 1] += f * R;
            }
            //PROC
            adsr.ProccessLR(ref buffer);
            for (int i = 0; i < samples.Length; i++) samples[i] += buffer[i];

        }
        public void Dispose()
        {
        }
        public bool ReleaseEnded { get => adsr.ReleaseEnded; }
    }

    // A typ 02
    public class GranuleSample : IPitchSample
    {
        public List<float[]> GranuleBuffer;
        public List<int> Timing;
        public float defaultSpd;
        public float defaultTimeIn;

        public int PreviewTexture => 0;
        public bool BeatSync => false;
        public bool AffectedByMasterPitch => true;
        public PitchSampleFlags Possibilities => PitchSampleFlags.Pitch | PitchSampleFlags.Speed | PitchSampleFlags.Formant;
        public ushort Type => 2;

        public GranuleSample(List<float[]> granules, List<int> timing, float spd, float timeIn)
        {
            GranuleBuffer = new List<float[]>(granules);
            Timing = new List<int>(timing);
            defaultSpd = spd;
            defaultTimeIn = timeIn;
        }
        public GranuleSample(GranuleSample gs)
        {
            this.GranuleBuffer = new List<float[]>();
            foreach (float[] fa in gs.GranuleBuffer) GranuleBuffer.Add((float[])fa.Clone());
            Timing = new List<int>();
            foreach (int i in gs.Timing) Timing.Add(i);
            defaultSpd = gs.defaultSpd;
            defaultTimeIn = gs.defaultTimeIn;
        }
        public IPitchReader GetReader(float time)
        {
            return new GranuleSampleReader(this, (int)((time + defaultTimeIn) * 48000));
        }


        public float[] GetGranule(int position)
        {
            for (int i = 0; (i < Timing.Count) && (i < GranuleBuffer.Count); i++)
            {
                if (Timing[i] > position)
                {
                    if (i == 0) return GranuleBuffer[0];
                    if (i - 1 >= GranuleBuffer.Count) return new float[0];
                    if (Timing[i] - position > position - Timing[i - 1]) return GranuleBuffer[i - 1];
                    else return GranuleBuffer[i];
                }
            }
            if (GranuleBuffer.Count == 0) return new float[0] { };
            return GranuleBuffer[GranuleBuffer.Count - 1];
        }

        public void SaveArbData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, defaultSpd);
            StreamHelper.SaveBytes(fs, defaultTimeIn);

            StreamHelper.SaveBytes(fs, Timing.Count);

            for (int i = 0; i < Timing.Count; i++)
            {
                StreamHelper.SaveBytes(fs, Timing[i]);
            }

            StreamHelper.SaveBytes(fs, GranuleBuffer.Count);

            for (int i = 0; i < GranuleBuffer.Count; i++)
            {
                StreamHelper.SaveBytes(fs, GranuleBuffer[i].Length);

                for (int j = 0; j < GranuleBuffer[i].Length; j++)
                {
                    StreamHelper.SaveBytes(fs, GranuleBuffer[i][j]);
                }
            }
        }
        public void LoadArbData(FileStream fs)
        {
            defaultSpd = StreamHelper.LoadFloat(fs);
            defaultTimeIn = StreamHelper.LoadFloat(fs);

            int l = StreamHelper.LoadInt(fs);
            Timing.Clear();

            for (int i = 0; i < l; i++)
            {
                Timing.Add(StreamHelper.LoadInt(fs));
            }

            l = StreamHelper.LoadInt(fs);
            GranuleBuffer.Clear();

            for (int i = 0; i < l; i++)
            {
                int l2 = StreamHelper.LoadInt(fs);
                float[] f = new float[l2];

                for (int j = 0; j < l2; j++)
                {
                    f[j] = StreamHelper.LoadFloat(fs);
                }

                GranuleBuffer.Add(f);
            }
        }
        public void PostLoad()
        {
        }
    }
    public class GranuleSampleReader : IPitchReader
    {
        GranuleSample parent;
        ADSR adsr;

        private float pos = 0;
        private float[] buf = new float[1000];
        private float[] buffer = new float[1000];

        private int prevGroup = 0;
        private int group = 0;
        private float p = 0;
        private int posi = 0;
        private float phase = 0;

        public GranuleSampleReader(GranuleSample parent, int time)
        {
            this.parent = parent;
            pos = time + parent.defaultTimeIn;

            adsr = new ADSR(0.03f, 0, 1, 0.02f);

            PROPERTIES[2] = 1;
        }

        private List<List<float>> FiredGranules = new List<List<float>>();
        float[] GranulePos = new float[400];
        private float GranuleSpd = 1;

        public void ReadAdd(ref float[] samples, float L, float R)
        {
            float SPD = (float)(PROPERTIES[2]) * parent.defaultSpd;
            float PTCH = 440f * (float)Math.Pow(2, PROPERTIES[3] / 12);
            float FORMANT = (float)Math.Pow(2, PROPERTIES[5] / 12);



            if (buffer.Length != samples.Length) buffer = new float[samples.Length];
            Array.Clear(buffer, 0, samples.Length);

            if (end) adsr.Release();
            //PROC
            for (int i = 0; i < samples.Length / 2; i++)
            {
                if (phase > 1 && FiredGranules.Count < 400)
                {
                    phase %= 1;
                    FiredGranules.Add(new List<float>(parent.GetGranule((int)pos) as IEnumerable<float>));
                    GranulePos[FiredGranules.Count - 1] = -500 + parent.GetGranule((int)pos).Length / FORMANT / 2;
                }

                float samp = 0;
                if (FORMANT == 1) for (int j = 0; j < FiredGranules.Count; j++)
                    {
                        if (FiredGranules[j].Count > GranulePos[j] && GranulePos[j] >= 0)
                        {
                            samp += FiredGranules[j][(int)GranulePos[j]];

                        }
                        GranulePos[j]++;

                    }
                else for (int j = 0; j < FiredGranules.Count; j++)
                    {
                        if (FiredGranules[j].Count - 1 > GranulePos[j] && GranulePos[j] >= 0)
                        {
                            samp += FiredGranules[j][(int)GranulePos[j]] + FiredGranules[j][(int)GranulePos[j] + 1];
                        }
                        GranulePos[j] += 1 / FORMANT;

                    }
                for (int j = FiredGranules.Count - 1; j >= 0; j--)
                {
                    if (FiredGranules[j].Count <= GranulePos[j])
                    {
                        FiredGranules.RemoveAt(j);
                        for (int k = j; k < GranulePos.Length; k++)
                        {
                            if (k == GranulePos.Length - 1) GranulePos[GranulePos.Length - 1] = 0;
                            else GranulePos[k] = GranulePos[k + 1];
                        }
                    }
                }




                samples[i * 2] += samp * L;
                samples[i * 2 + 1] += samp * R;

                phase += PTCH / 48000;
                pos += SPD;
            }
            adsr.ProccessLR(ref buffer);
        }

        private float[] PROPERTIES = new float[256];
        public void SetProperty(byte index, float val)
        {
            PROPERTIES[index] = val;
        }
        public float GetPropertyValue(byte index)
        {
            return PROPERTIES[index];
        }

        bool end = false;
        public void NoteOff()
        {
            end = true;
        }
        public void Dispose()
        {
            FiredGranules.Clear();
        }

        public bool ReleaseEnded => adsr.ReleaseEnded;
    }

    // V typ 03 SL
    // stand alone cast source - rychle nacteni (+ casovy odstup)
    // A typ 03
    public class QuickLoadVideoSample : IVideoSample
    {
        public uint SampleVideoSource = 0; //SAVE
        int MaxFrames = -1;     //SAVE
        public float TimeSecIn = 0;    //SAVE
        float fps = 30;
        int NextToExtract = 0;
        int W = 0;
        int H = 0;
        public float defaultSpd = 1;

        List<int> Frames = new List<int>();

        VideoSourceS vss = null;

        public QuickLoadVideoSample(uint source, int maxFrames, bool dynamicalyProlong, float timeSec, float defaultSpd)
        {
            SampleVideoSource = source;
            if (!dynamicalyProlong) MaxFrames = maxFrames;
            TimeSecIn = timeSec;

            if (Form1.Project.ProjectSamples.ContainsKey(source))
            {
                if (Form1.Project.ProjectSamples[source].ivs is VideoSourceS) vss = Form1.Project.ProjectSamples[source].ivs as VideoSourceS;
                H = 100;
                W = 100;
                if (vss != null) H = vss.ArH;
                if (vss != null) W = vss.ArW;
            }
            if (vss != null)
            {
                this.fps = vss.FPS;
                this.defaultSpd = defaultSpd;
                NextToExtract = (int)(timeSec * fps);

                if (maxFrames != -1) ExtractNext(maxFrames);
                else ExtractNext(15);

                PreviewBitmap = new Bitmap(vss.Width, vss.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                System.Drawing.Imaging.BitmapData bd = PreviewBitmap.LockBits(new Rectangle(0, 0, W, H), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Bgr, PixelType.Byte, bd.Scan0);
                PreviewBitmap.UnlockBits(bd);
            }
        }

        private void ExtractNext(int count)
        {
            VideoSourceVReader vsvr = vss.GetReader(TimeSecIn, 0) as VideoSourceVReader;
            if (count > 0)
            {
                Frames.Add(vsvr.vs.extractFrameNumber(NextToExtract));
                NextToExtract++;
                count--;
            }
            while (count > 0)
            {
                Frames.Add(vsvr.vs.extractNext());
                NextToExtract++;
                count--;
            }
            vsvr.Dispose();
        }

        public int PreviewTexture => Frames.Count > 0 ? Frames[0] : 0;
        public float FPS => fps;
        public bool BeatSync => false;
        public int FrameEnd => MaxFrames == -1 ? 9999999 : MaxFrames;
        public int FrameStart => 0;
        public bool RequiresReader => false;
        public ushort Type => 3;

        public Bitmap PreviewBitmap { get; set; }

        public TextureInfo GetFrame(float timeIn)
        {
            int i = (int)(timeIn * FPS * defaultSpd);
            if (i < 0) i = 0;
            if (i >= Frames.Count) i = Frames.Count - 1;
            TextureInfo ti = new TextureInfo(Frames[i], W, H);
            ti.FlipY();
            return ti;
        }
        public IVideoReader GetReader(float time, int texture)
        {
            return null;
        }
        public void LoadArbData(FileStream fs)
        {
            StreamHelper.LoadInt(fs);
            SampleVideoSource = StreamHelper.LoadUInt(fs);
            MaxFrames = StreamHelper.LoadInt(fs);
            TimeSecIn = StreamHelper.LoadFloat(fs);
        }
        public void SaveArbData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)3);
            StreamHelper.SaveBytes(fs, SampleVideoSource);
            StreamHelper.SaveBytes(fs, MaxFrames);
            StreamHelper.SaveBytes(fs, TimeSecIn);
        }
        public void PostLoad()
        {
            if (Form1.Project.ProjectSamples.ContainsKey(SampleVideoSource))
            {
                if (Form1.Project.ProjectSamples[SampleVideoSource].ivs is VideoSourceS) vss = Form1.Project.ProjectSamples[SampleVideoSource].ivs as VideoSourceS;
                H = vss.ArH;
                W = vss.ArW;
            }

            this.fps = vss.FPS;
            NextToExtract = (int)(TimeSecIn * fps);

            if (MaxFrames != -1) ExtractNext(MaxFrames);
            else ExtractNext(15);

            PreviewBitmap = new Bitmap(vss.Width, vss.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            System.Drawing.Imaging.BitmapData bd = PreviewBitmap.LockBits(new Rectangle(0, 0, vss.Width, vss.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Bgr, PixelType.Byte, bd.Scan0);
            PreviewBitmap.UnlockBits(bd);
        }
    }

    // A typ 04 (PAD)
    public class PitchBufferSample : IPitchSample
    {
        public float[] Audio;
        public float pitch;

        public float A;
        public float D;
        public float S;
        public float R;

        public float PP;
        public float PL;

        public int PreviewTexture => 0;
        public bool BeatSync => false;
        public bool AffectedByMasterPitch => true;
        public PitchSampleFlags Possibilities => PitchSampleFlags.Pitch;
        public ushort Type => 4;

        public IPitchReader GetReader(float time)
        {
            return new PitchBufferReader(this, time, A, D, S, R);
        }

        public void SaveArbData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, pitch);

            StreamHelper.SaveBytes(fs, A);
            StreamHelper.SaveBytes(fs, D);
            StreamHelper.SaveBytes(fs, S);
            StreamHelper.SaveBytes(fs, R);

            StreamHelper.SaveBytes(fs, Audio.Length);

            for (int i = 0; i < Audio.Length; i++)
            {
                StreamHelper.SaveBytes(fs, Audio[i]);
            }
        }
        public void LoadArbData(FileStream fs)
        {
            pitch = StreamHelper.LoadFloat(fs);

            A = StreamHelper.LoadFloat(fs);
            D = StreamHelper.LoadFloat(fs);
            S = StreamHelper.LoadFloat(fs);
            R = StreamHelper.LoadFloat(fs);

            int l = StreamHelper.LoadInt(fs);

            Audio = new float[l];

            for (int i = 0; i < l; i++)
            {
                Audio[i] = StreamHelper.LoadFloat(fs);
            }
        }

        public float this[float sample]
        {
            get
            {
                if (sample < 0) sample = (sample % Audio.Length + Audio.Length);
                if (sample > Audio.Length) sample = sample % Audio.Length;
                float p = sample % 1;
                return (1 - p) * Audio[(int)sample % Audio.Length] + p * Audio[((int)sample + 1) % Audio.Length];
            }
        }

        public PitchBufferSample(float[] samples, float pitch48000)
        {
            Audio = samples;
            pitch = pitch48000;
            A = 0;
            D = 0;
            S = 1;
            R = 0;
        }
        public PitchBufferSample(float[] samples, float pitch48000, float A, float D, float S, float R)
        {
            Audio = samples;
            pitch = pitch48000;
            this.A = A;
            this.D = D;
            this.S = S;
            this.R = R;
        }
        public PitchBufferSample(PitchBufferSample pbs)
        {
            Audio = (float[])pbs.Audio.Clone();
            pitch = pbs.pitch;
            this.A = pbs.A;
            this.D = pbs.D;
            this.S = pbs.S;
            this.R = pbs.R;
        }
        public void PostLoad()
        {
        }
    }
    public class PitchBufferReader : IPitchReader
    {
        float pos = 0;
        float posr = 0;
        PitchBufferSample cps;
        ADSR adsr;

        public PitchBufferReader(PitchBufferSample cps, float A, float D, float S, float R)
        {
            this.cps = cps;
            PROPERTIES[2] = 1;

            pos = 0;
            posr = 0 + 15;

            adsr = new ADSR(A, D, S, R);
        }
        public PitchBufferReader(PitchBufferSample cps, float timeIn, float A, float D, float S, float R)
        {
            this.cps = cps;
            PROPERTIES[2] = 1;

            pos = (int)(timeIn * 48000);
            posr = pos + 15;

            adsr = new ADSR(A, D, S, R);
        }

        private float spd = 1;
        float[] buffer = new float[0];

        private float[] PROPERTIES = new float[256];
        public void SetProperty(byte index, float val)
        {
            PROPERTIES[index] = val;
        }
        public float GetPropertyValue(byte index)
        {
            return PROPERTIES[index];
        }

        public void NoteOff()
        {
            adsr.Release();
        }

        public void ReadAdd(ref float[] samples, float L, float R)
        {
            spd = (float)Math.Pow(2, PROPERTIES[3] / 12f) * 2;

            if (buffer.Length != samples.Length) buffer = new float[samples.Length];
            Array.Clear(buffer, 0, samples.Length);
            //PROC
            for (int i = 0; i < samples.Length / 2; i++)
            {
                pos += spd;
                posr += spd;
                buffer[i * 2] = cps[pos] * L;
                buffer[i * 2 + 1] = cps[posr] * R;
            }
            adsr.ProccessLR(ref buffer);
            //PROC
            for (int i = 0; i < samples.Length; i++) samples[i] += buffer[i];
        }

        public void Dispose()
        {
        }

        public bool ReleaseEnded => adsr.ReleaseEnded;
    }

    // A typ 05
    public class IntroPitchBufferSample : IPitchSample
    {
        public float[] Intro;
        public float[] Audio;
        public float pitch;

        public float A;
        public float D;
        public float S;
        public float R;

        public int PreviewTexture => 0;
        public bool BeatSync => false;
        public bool AffectedByMasterPitch => true;
        public PitchSampleFlags Possibilities => PitchSampleFlags.Pitch;
        public ushort Type => 5;

        public IPitchReader GetReader(float time)
        {
            return new IntroPitchBufferReader(this, time, A, D, S, R);
        }

        public void SaveArbData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, pitch);
            StreamHelper.SaveBytes(fs, A);
            StreamHelper.SaveBytes(fs, D);
            StreamHelper.SaveBytes(fs, S);
            StreamHelper.SaveBytes(fs, R);


            StreamHelper.SaveBytes(fs, Intro.Length);
            for (int i = 0; i < Intro.Length; i++)
            {
                StreamHelper.SaveBytes(fs, Intro[i]);
            }

            StreamHelper.SaveBytes(fs, Audio.Length);
            for (int i = 0; i < Audio.Length; i++)
            {
                StreamHelper.SaveBytes(fs, Audio[i]);
            }
        }
        public void LoadArbData(FileStream fs)
        {
            pitch = StreamHelper.LoadFloat(fs);
            A = StreamHelper.LoadFloat(fs);
            D = StreamHelper.LoadFloat(fs);
            S = StreamHelper.LoadFloat(fs);
            R = StreamHelper.LoadFloat(fs);


            int l = StreamHelper.LoadInt(fs);

            Intro = new float[l];

            for (int i = 0; i < l; i++)
            {
                Intro[i] = StreamHelper.LoadFloat(fs);
            }

            l = StreamHelper.LoadInt(fs);
            Audio = new float[l];
            for (int i = 0; i < l; i++)
            {
                Audio[i] = StreamHelper.LoadFloat(fs);
            }
        }

        public IntroPitchBufferSample(float[] intro, float[] samples, float pitch48000)
        {
            Audio = samples;
            Intro = intro;
            pitch = pitch48000;
            A = 0;
            D = 0;
            S = 1;
            R = 0;
        }
        public IntroPitchBufferSample(float[] intro, float[] samples, float pitch48000, float A, float D, float S, float R)
        {
            Audio = samples;
            Intro = intro;
            pitch = pitch48000;
            this.A = A;
            this.D = D;
            this.S = S;
            this.R = R;
        }
        public void PostLoad()
        {
        }

        public float this[float sample]
        {
            get
            {
                if (sample < -Intro.Length) return 0;
                if (sample < 0)
                {
                    float px = sample % 1;
                    sample = sample + Intro.Length;
                    return (1 - px) * Intro[(int)sample] + px * ((sample + 1 <= Intro.Length - 1) ? Intro[(int)sample + 1] : Audio[0]);
                }
                if (sample > Audio.Length) sample = sample % Audio.Length;
                float p = sample % 1;
                return (1 - p) * Audio[(int)sample % Audio.Length] + p * Audio[((int)sample + 1) % Audio.Length];
            }
        }
    }
    public class IntroPitchBufferReader : IPitchReader
    {
        float pos = 0;
        float posr = 0;
        IntroPitchBufferSample cps;
        ADSR adsr;

        public IntroPitchBufferReader(IntroPitchBufferSample cps, float A, float D, float S, float R)
        {
            this.cps = cps;
            PROPERTIES[2] = 1;

            pos = -cps.Intro.Length;
            posr = 0 + 15;

            adsr = new ADSR(A, D, S, R);
        }
        public IntroPitchBufferReader(IntroPitchBufferSample cps, float timeIn, float A, float D, float S, float R)
        {
            this.cps = cps;
            PROPERTIES[2] = 1;

            pos = (int)(timeIn * 48000) - cps.Intro.Length;
            posr = pos + 15;

            adsr = new ADSR(A, D, S, R);
        }

        private float spd = 1;
        float[] buffer = new float[0];

        private float[] PROPERTIES = new float[256];
        public void SetProperty(byte index, float val)
        {
            PROPERTIES[index] = val;
        }
        public float GetPropertyValue(byte index)
        {
            return PROPERTIES[index];
        }

        public void NoteOff()
        {
            adsr.Release();
        }

        public void ReadAdd(ref float[] samples, float L, float R)
        {
            spd = 440 * (float)Math.Pow(2, PROPERTIES[3] / 12f) / cps.pitch;

            if (buffer.Length != samples.Length) buffer = new float[samples.Length];
            Array.Clear(buffer, 0, samples.Length);
            //PROC
            for (int i = 0; i < samples.Length / 2; i++)
            {
                pos += spd;
                posr += spd;
                buffer[i * 2] = cps[pos] * L;
                buffer[i * 2 + 1] = cps[posr] * R;
            }
            adsr.ProccessLR(ref buffer);
            //PROC
            for (int i = 0; i < samples.Length; i++) samples[i] += buffer[i];
        }

        public void Dispose()
        {
        }

        public bool ReleaseEnded => adsr.ReleaseEnded;
    }

    // V typ 06
    public class BitmapSource : IDisposable, IVideoSample
    {
        public ushort Type => 6;

        //soubor
        public string path;
        public int Width;
        public int Height;

        public bool RequiresReader => false;
        public bool AffectedByMasterPitch => false;
        public PitchSampleFlags Possibilities => PitchSampleFlags.None;
        public Bitmap PreviewBitmap { get; set; }
        public float FPS => 30;
        public bool BeatSync => false;
        public int FrameEnd => 9999999;
        public int FrameStart => -99999999;
        private int TextureIndex;

        //konstruktor
        public BitmapSource(string p)
        {
            path = p;
            try
            {
                PreviewBitmap = new Bitmap(p);
                TextureIndex = GL.GenTexture();

                System.Drawing.Imaging.BitmapData bd = PreviewBitmap.LockBits(new Rectangle(0, 0, PreviewBitmap.Width, PreviewBitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, PreviewBitmap.PixelFormat);

                GL.BindTexture(TextureTarget.Texture2D, TextureIndex);
                if (bd.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb) GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, PreviewBitmap.Width, PreviewBitmap.Height, 0, PixelFormat.Bgr, PixelType.UnsignedByte, bd.Scan0);
                else GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, PreviewBitmap.Width, PreviewBitmap.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bd.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                PreviewBitmap.UnlockBits(bd);
            }
            catch
            {
                PreviewBitmap = null;
                TextureIndex = 0;
            }
        }

        //video
        int IVideoSample.PreviewTexture => 0;

        public void Dispose()
        {

        }
        public void GenerateT2D()
        {
        }

        TextureInfo IVideoSample.GetFrame(float timeIn)
        {
            if (PreviewBitmap == null) return new TextureInfo(TextureIndex, 0, 0);
            else
            {
                TextureInfo ti = new TextureInfo(TextureIndex, PreviewBitmap.Width, PreviewBitmap.Height);
                ti.FlipY();
                return ti;
            }
        }
        public IVideoReader GetReader(float time, int tex) => null;

        //SAVE LOAD
        public void SaveArbData(FileStream fs)
        {
            StreamHelper.SaveString(fs, path);
        }
        public void LoadArbData(FileStream fs)
        {
            path = StreamHelper.LoadString(fs);

            try
            {
                PreviewBitmap = new Bitmap(path);
                TextureIndex = GL.GenTexture();

                System.Drawing.Imaging.BitmapData bd = PreviewBitmap.LockBits(new Rectangle(0, 0, PreviewBitmap.Width, PreviewBitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, PreviewBitmap.PixelFormat);

                GL.BindTexture(TextureTarget.Texture2D, TextureIndex);
                if (bd.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb) GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, PreviewBitmap.Width, PreviewBitmap.Height, 0, PixelFormat.Bgr, PixelType.Byte, bd.Scan0);
                else GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, PreviewBitmap.Width, PreviewBitmap.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bd.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                PreviewBitmap.UnlockBits(bd);
            }
            catch
            {
                PreviewBitmap = null;
                TextureIndex = 0;
            }
        }

        public void PostLoad()
        {
        }
    }

    // A typ 07
    public class AudioCutSample : IPitchSample
    {
        public float[] Audio;
        public bool Shiftable;
        public bool TwoChannel;
        public float SpdAt440;
        public float A;
        public float D;
        public float S;
        public float R;
        public bool playFull;

        public int PreviewTexture => 0;
        public bool BeatSync => false;
        public bool AffectedByMasterPitch => true;
        public PitchSampleFlags Possibilities => PitchSampleFlags.Pitch;
        public ushort Type => 7;

        public IPitchReader GetReader(float time)
        {
            return new AudioCutReader(this, time, A, D, S, R);
        }

        public void SaveArbData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, TwoChannel);
            StreamHelper.SaveBytes(fs, Shiftable);
            StreamHelper.SaveBytes(fs, playFull);
            StreamHelper.SaveBytes(fs, SpdAt440);

            StreamHelper.SaveBytes(fs, A);
            StreamHelper.SaveBytes(fs, D);
            StreamHelper.SaveBytes(fs, S);
            StreamHelper.SaveBytes(fs, R);

            StreamHelper.SaveBytes(fs, Audio.Length);
            for (int i = 0; i < Audio.Length; i++)
            {
                StreamHelper.SaveBytes(fs, Audio[i]);
            }
        }
        public void LoadArbData(FileStream fs)
        {
            TwoChannel = StreamHelper.LoadBool(fs);
            Shiftable = StreamHelper.LoadBool(fs);
            playFull = StreamHelper.LoadBool(fs);
            SpdAt440 = StreamHelper.LoadFloat(fs);

            A = StreamHelper.LoadFloat(fs);
            D = StreamHelper.LoadFloat(fs);
            S = StreamHelper.LoadFloat(fs);
            R = StreamHelper.LoadFloat(fs);

            int l = StreamHelper.LoadInt(fs);

            Audio = new float[l];

            for (int i = 0; i < l; i++)
            {
                Audio[i] = StreamHelper.LoadFloat(fs);
            }
        }

        public void PostLoad()
        {
        }

        public float this[float sample]
        {
            get
            {
                if (sample < 0) sample = (sample % Audio.Length + Audio.Length);
                if (sample > Audio.Length) sample = sample % Audio.Length;
                float p = sample % 1;
                return (1 - p) * Audio[(int)sample % Audio.Length] + p * Audio[((int)sample + 1) % Audio.Length];
            }
        }

        public AudioCutSample(float[] samples, bool twoChanel, bool shiftable, float SpdAt440, float A, float D, float S, float R, bool playfull)
        {
            Audio = samples;
            Shiftable = shiftable;
            this.SpdAt440 = SpdAt440;
            TwoChannel = twoChanel;
            this.A = A;
            this.D = D;
            this.S = S;
            this.R = R;
            playFull = playfull;
        }

        public AudioCutSample(AudioCutSample acs)
        {
            Audio = (float[])acs.Audio.Clone();
            Shiftable = acs.Shiftable;
            this.SpdAt440 = acs.SpdAt440;
            TwoChannel = acs.TwoChannel;
            this.A = acs.A;
            this.D = acs.D;
            this.S = acs.S;
            this.R = acs.R;
            playFull = acs.playFull;
        }
    }
    public class AudioCutReader : IPitchReader
    {
        float pos = 0;
        float posr = 0;
        AudioCutSample cps;
        ADSR adsr;

        public AudioCutReader(AudioCutSample cps, float timeIn, float A, float D, float S, float R)
        {
            this.cps = cps;
            PROPERTIES[2] = 1;

            pos = (int)(timeIn * 48000);
            posr = pos;

            adsr = new ADSR(A, D, S, R);
        }

        private float spd = 1;
        float[] buffer = new float[0];

        private float[] PROPERTIES = new float[256];
        public void SetProperty(byte index, float val)
        {
            PROPERTIES[index] = val;
        }
        public float GetPropertyValue(byte index)
        {
            return PROPERTIES[index];
        }

        public void NoteOff()
        {
            if (!cps.playFull) adsr.Release();
        }

        public void ReadAdd(ref float[] samples, float L, float R)
        {
            if (cps.TwoChannel)
            {
                spd = 1;

                if (buffer.Length != samples.Length) buffer = new float[samples.Length];
                Array.Clear(buffer, 0, samples.Length);
                //PROC
                for (int i = 0; i < samples.Length / 2; i++)
                {
                    pos += 1;
                    posr += 1;
                    if (pos > 0 && pos * 2 < cps.Audio.Length)
                    {
                        buffer[i * 2] = cps.Audio[(int)pos * 2] * L;
                        buffer[i * 2 + 1] = cps.Audio[(int)posr * 2 + 1] * R;
                    }
                }
                adsr.ProccessLR(ref buffer);
                //PROC
                for (int i = 0; i < samples.Length; i++) samples[i] += buffer[i];
            }
            else
            {
                spd = (cps.Shiftable ? (float)Math.Pow(2, PROPERTIES[3] / 12f) * cps.SpdAt440 : 1);

                if (buffer.Length != samples.Length) buffer = new float[samples.Length];
                Array.Clear(buffer, 0, buffer.Length);
                //PROC
                for (int i = 0; i < samples.Length / 2; i++)
                {
                    pos += spd;
                    if (pos > 0 && pos + 1 < cps.Audio.Length)
                    {
                        buffer[i * 2] = ((1 - pos % 1) * cps.Audio[(int)pos] + (pos % 1) * cps.Audio[(int)pos + 1]) * L;
                        buffer[i * 2 + 1] = ((1 - pos % 1) * cps.Audio[(int)pos] + (pos % 1) * cps.Audio[(int)pos + 1]) * R;
                    }
                }
                adsr.ProccessLR(ref buffer);
                //PROC
                for (int i = 0; i < samples.Length; i++) samples[i] += buffer[i];
            }
        }

        public void Dispose()
        {
        }

        public bool ReleaseEnded => cps.playFull ? (pos > cps.Audio.Length) : adsr.ReleaseEnded;
    }

    // A typ 08
    public class FFTSample : IPitchSample
    {
        public int PreviewTexture => 0;
        public bool BeatSync => false;
        public bool AffectedByMasterPitch => false;
        public PitchSampleFlags Possibilities => PitchSampleFlags.Speed | PitchSampleFlags.Pitch;

        public ushort Type => 8;

        //FFT
        int fftSize;
        int osamp;
        public float[] sample;

        public FFTSample(int fftSize, int osamp, float speed, float pitch, float[] sample)
        {
            this.sample = sample;

            this.osamp = osamp;
            this.fftSize = fftSize;
        }

        public IPitchReader GetReader(float time)
        {
            return new FFTReader(this);
        }

        public void SaveArbData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, sample.Length);
            for (int i = 0; i < sample.Length; i++)
            {
                StreamHelper.SaveBytes(fs, sample[i]);
            }
        }
        public void LoadArbData(FileStream fs)
        {
            int l = StreamHelper.LoadInt(fs);

            sample = new float[l];

            for (int i = 0; i < l; i++)
            {
                sample[i] = StreamHelper.LoadFloat(fs);
            }
        }

        public void PostLoad()
        {
        }
    }
    public class FFTReader : IPitchReader
    {
        FFTSample ptm;

        private static int MAX_FRAME_LENGTH = 17000;
        private float[] gInFIFO = new float[MAX_FRAME_LENGTH];
        private float[] gOutFIFO = new float[MAX_FRAME_LENGTH];
        private float[] gFFTworksp = new float[2 * MAX_FRAME_LENGTH];
        private float[] gLastPhase = new float[MAX_FRAME_LENGTH / 2 + 1];
        private float[] gSumPhase = new float[MAX_FRAME_LENGTH / 2 + 1];
        private float[] gOutputAccum = new float[2 * MAX_FRAME_LENGTH];
        private float[] gAnaFreq = new float[MAX_FRAME_LENGTH];
        private float[] gAnaMagn = new float[MAX_FRAME_LENGTH];
        private float[] gSynFreq = new float[MAX_FRAME_LENGTH];
        private float[] gSynMagn = new float[MAX_FRAME_LENGTH];
        private long gRover;

        private float Position = 0;
        private int Ps;

        /// <summary>
        /// Pitch Shift 
        /// </summary>
        public void PitchShift(float pitchShift, float speed, long numSampsToProcess, long fftFrameSize,
            long osamp, float sampleRate, float[] outdata)
        {
            double magn, phase, tmp, window, real, imag;
            double freqPerBin, expct;
            long i, k, qpd, inFifoLatency, stepSize, stepSizeX, fftFrameSize2;


            /* set up some handy variables */
            fftFrameSize2 = fftFrameSize / 2;
            stepSize = (int)(fftFrameSize / osamp);
            stepSizeX = (int)(fftFrameSize * speed / osamp);
            freqPerBin = sampleRate / (double)fftFrameSize;
            expct = 2.0 * Math.PI * (double)stepSizeX / (double)fftFrameSize;
            inFifoLatency = fftFrameSize - stepSize;
            if (gRover == 0)
            {
                gRover = inFifoLatency;
            }


            /* main processing loop */
            for (i = 0; i < numSampsToProcess; i++)
            {
                /* As long as we have not yet collected enough data just read in */
                if (i + Ps >= 0 && i + Ps < ptm.sample.Length) gInFIFO[gRover] = ptm.sample[i + Ps];
                else gInFIFO[gRover] = 0;
                outdata[i] = gOutFIFO[gRover - inFifoLatency];
                gRover++;

                /* now we have enough data for processing */
                if (gRover >= fftFrameSize)
                {
                    gRover = inFifoLatency;

                    /* do windowing and re,im interleave */
                    for (k = 0; k < fftFrameSize; k++)
                    {
                        window = -.5 * Math.Cos(2.0 * Math.PI * (double)k / (double)fftFrameSize) + .5;
                        gFFTworksp[2 * k] = (float)(gInFIFO[k] * window);
                        gFFTworksp[2 * k + 1] = 0.0F;
                    }


                    /* ***************** ANALYSIS ******************* */
                    /* do transform */
                    ShortTimeFourierTransform(gFFTworksp, fftFrameSize, -1);

                    /* this is the analysis step */
                    for (k = 0; k <= fftFrameSize2; k++)
                    {
                        /* de-interlace FFT buffer */
                        real = gFFTworksp[2 * k];
                        imag = gFFTworksp[2 * k + 1];

                        /* compute magnitude and phase */
                        magn = 2.0 * Math.Sqrt(real * real + imag * imag);
                        phase = Math.Atan2(imag, real);

                        /* compute phase difference */
                        tmp = phase - gLastPhase[k];
                        gLastPhase[k] = (float)phase;

                        /* subtract expected phase difference */
                        tmp -= (double)k * expct;

                        /* map delta phase into +/- Pi interval */
                        qpd = (long)(tmp / Math.PI);
                        if (qpd >= 0) qpd += qpd & 1;
                        else qpd -= qpd & 1;
                        tmp -= Math.PI * (double)qpd;

                        /* get deviation from bin frequency from the +/- Pi interval */
                        tmp = osamp * tmp / (2.0 * Math.PI);

                        /* compute the k-th partials' true frequency */
                        tmp = (double)k * freqPerBin + tmp * freqPerBin;

                        /* store magnitude and true frequency in analysis arrays */
                        gAnaMagn[k] = (float)magn;
                        gAnaFreq[k] = (float)tmp;

                    }

                    //PitchArrayEventArgs p = new PitchArrayEventArgs(gAnaFreq, gAnaMagn);
                    //if (ReadIn || ExportOut) PitchNotification(this, p);
                    //gSynFreq = p.Freq;
                    //gSynMagn = p.Magn;
                    long index = 0;

                    for (k = 0; k <= fftFrameSize2; k++)
                    {
                        index = (long)(k * pitchShift);
                        if (index <= fftFrameSize2)
                        {
                            gSynMagn[index] += gAnaMagn[k];
                            gSynFreq[index] = gAnaFreq[k] * pitchShift;
                        }
                    }

                    /* ***************** SYNTHESIS ******************* */
                    /* this is the synthesis step */
                    for (k = 0; k <= fftFrameSize2; k++)
                    {

                        /* get magnitude and true frequency from synthesis arrays */
                        magn = gSynMagn[k];
                        tmp = gSynFreq[k];

                        /* subtract bin mid frequency */
                        tmp -= (double)k * freqPerBin;

                        /* get bin deviation from freq deviation */
                        tmp /= freqPerBin;

                        /* take osamp into account */
                        tmp = 2.0 * Math.PI * tmp / osamp;

                        /* add the overlap phase advance back in */
                        tmp += (double)k * expct;

                        /* accumulate delta phase to get bin phase */
                        gSumPhase[k] += (float)tmp;
                        if (gSumPhase[k] == float.NaN) gSumPhase[k] = 0;
                        phase = gSumPhase[k];


                        /* get real and imag part and re-interleave */
                        gFFTworksp[2 * k] = (float)(magn * Math.Cos(phase));
                        gFFTworksp[2 * k + 1] = (float)(magn * Math.Sin(phase));
                    }

                    /* zero negative frequencies */
                    for (k = fftFrameSize + 2; k < 2 * fftFrameSize; k++) gFFTworksp[k] = 0.0F;

                    /* do inverse transform */
                    ShortTimeFourierTransform(gFFTworksp, fftFrameSize, 1);

                    /* do windowing and add to output accumulator */
                    for (k = 0; k < fftFrameSize; k++)
                    {
                        window = -.5 * Math.Cos(2.0 * Math.PI * (double)k / (double)fftFrameSize) + .5;
                        gOutputAccum[k] += (float)(2.0 * window * gFFTworksp[2 * k] / (fftFrameSize2 * osamp));
                    }
                    for (k = 0; k < stepSize; k++) gOutFIFO[k] = gOutputAccum[k];

                    /* shift accumulator */
                    //memmove(gOutputAccum, gOutputAccum + stepSize, fftFrameSize * sizeof(float));
                    for (k = 0; k < fftFrameSize; k++)
                    {
                        gOutputAccum[k] = gOutputAccum[k + stepSize];
                    }

                    /* move input FIFO */
                    for (k = 0; k < inFifoLatency; k++) gInFIFO[k] = gInFIFO[k + stepSize];

                    Position += speed * stepSize;
                    Ps = (int)Position;
                }
            }
        }

        /// <summary>
        /// Short Time Fourier Transform
        /// </summary>
        public void ShortTimeFourierTransform(float[] fftBuffer, long fftFrameSize, long sign)
        {
            float wr, wi, arg, temp;
            float tr, ti, ur, ui;
            long i, bitm, j, le, le2, k;

            for (i = 2; i < 2 * fftFrameSize - 2; i += 2)
            {
                for (bitm = 2, j = 0; bitm < 2 * fftFrameSize; bitm <<= 1)
                {
                    if ((i & bitm) != 0) j++;
                    j <<= 1;
                }
                if (i < j)
                {
                    temp = fftBuffer[i];
                    fftBuffer[i] = fftBuffer[j];
                    fftBuffer[j] = temp;
                    temp = fftBuffer[i + 1];
                    fftBuffer[i + 1] = fftBuffer[j + 1];
                    fftBuffer[j + 1] = temp;
                }
            }
            long max = (long)(Math.Log(fftFrameSize) / Math.Log(2.0) + .5);
            for (k = 0, le = 2; k < max; k++)
            {
                le <<= 1;
                le2 = le >> 1;
                ur = 1.0F;
                ui = 0.0F;
                arg = (float)Math.PI / (le2 >> 1);
                wr = (float)Math.Cos(arg);
                wi = (float)(sign * Math.Sin(arg));
                for (j = 0; j < le2; j += 2)
                {

                    for (i = j; i < 2 * fftFrameSize; i += le)
                    {
                        tr = fftBuffer[i + le2] * ur - fftBuffer[i + le2 + 1] * ui;
                        ti = fftBuffer[i + le2] * ui + fftBuffer[i + le2 + 1] * ur;
                        fftBuffer[i + le2] = fftBuffer[i] - tr;
                        fftBuffer[i + le2 + 1] = fftBuffer[i + 1] - ti;
                        fftBuffer[i] += tr;
                        fftBuffer[i + 1] += ti;

                    }
                    tr = ur * wr - ui * wi;
                    ui = ur * wi + ui * wr;
                    ur = tr;
                }
            }
        }



        public bool ReleaseEnded { get; set; }
        float[] buffer = new float[0];
        float ptc = 1;
        float spd = 1;
        public void SetProperty(byte index, float value)
        {
            if (index == 2) spd = value;
            if (index == 4) ptc = (float)Math.Pow(2, value);
        }
        public void ReadAdd(ref float[] samples, float L, float R)
        {
            if (!(buffer.Length != samples.Length / 2)) buffer = new float[samples.Length / 2];
            Array.Clear(buffer, 0, buffer.Length);

            PitchShift(ptc, spd, buffer.Length, 2048, 4, 48000, buffer);
            for (int i = 0; i < buffer.Length; i++)
            {
                samples[i * 2 + 0] = L * buffer[i];
                samples[i * 2 + 1] = R * buffer[i];
            }
        }
        public void NoteOff()
        {
            ReleaseEnded = true;
        }
        public void Dispose()
        {
        }

        public FFTReader(FFTSample ptm)
        {
            ptm = this.ptm;
        }
    }

    // A typ 09
    public class RTPSSample : IPitchSample
    {
        public int PreviewTexture => 0;
        public bool BeatSync => false;
        public bool AffectedByMasterPitch => false;
        public PitchSampleFlags Possibilities => PitchSampleFlags.Speed | PitchSampleFlags.Pitch;

        public ushort Type => 9;

        //FFT
        int fftSize;
        int osamp;
        public float[] sample;

        public RTPSSample(int fftSize, int osamp, float speed, float pitch, float[] sample)
        {
            this.sample = sample;

            this.osamp = osamp;
            this.fftSize = fftSize;
        }

        public IPitchReader GetReader(float time)
        {
            return new RTPSReader(this);
        }

        public void SaveArbData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, sample.Length);
            for (int i = 0; i < sample.Length; i++)
            {
                StreamHelper.SaveBytes(fs, sample[i]);
            }
        }
        public void LoadArbData(FileStream fs)
        {
            int l = StreamHelper.LoadInt(fs);

            sample = new float[l];

            for (int i = 0; i < l; i++)
            {
                sample[i] = StreamHelper.LoadFloat(fs);
            }
        }

        public void PostLoad()
        {
        }
    }
    public class RTPSReader : IPitchReader
    {
        RealTimePSOLAShifter rtpss;
        public bool ReleaseEnded { get; set; }
        public void Dispose()
        {
        }
        public void NoteOff()
        {
            ReleaseEnded = true;
        }
        public RTPSReader(RTPSSample parent)
        {
            rtpss = new RealTimePSOLAShifter(new FloatArraySource(parent.sample, 48000), 60000, 180, 300, 750);
        }

        float[] buffer = new float[0];
        public void ReadAdd(ref float[] samples, float L, float R)
        {
            if ((buffer.Length != samples.Length / 2)) buffer = new float[samples.Length / 2];
            Array.Clear(buffer, 0, buffer.Length);

            rtpss.Pitch = pitch;
            rtpss.Speed = speed;
            rtpss.Read(buffer, 0, buffer.Length);
            for (int i = 0; i < buffer.Length; i++)
            {
                samples[i * 2 + 0] = L * buffer[i];
                samples[i * 2 + 1] = R * buffer[i];
            }
        }
        public void SetProperty(byte index, float value)
        {
            if (index == 2) speed = value;
            if (index == 4) pitch = (float)Math.Pow(2, value);
        }
        float pitch = 1f;
        float speed = 1f;
    }
    class RealTimePSOLAShifter : ISampleProvider
    {
        ISampleProvider sourceStream;
        public WaveFormat WaveFormat => sourceStream.WaveFormat;
        public int Read(float[] buffer, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                c = i % chanels;

                p1 = (int)(readerHead0) * chanels + c;
                p2 = (int)(readerHead1) * chanels + c;
                float m1 = 1 - (nextSearchFor2 ? fade : (1 - fade));
                buffer[offset + i] = internalBuffer[p1] * (1 - m1) * (1 - readerHead0 % 1f) + internalBuffer[(p1 + chanels) % internalBuffer.Length] * (1 - m1) * (readerHead0 % 1f) + internalBuffer[p2] * (m1) * (1 - readerHead1 % 1f) + internalBuffer[(p2 + chanels) % internalBuffer.Length] * (m1) * (readerHead1 % 1f);

                if (c == chanels - 1)
                {
                    fade += 1 / GranuleLenght;
                    readerHead0 += readerHeadSpd0;
                    readerHead1 += readerHeadSpd1;
                    readerHead0 %= bufferLenght * 3;
                    readerHead1 %= bufferLenght * 3;
                    RealPos += Speed;
                    RealPos %= bufferLenght * 3;

                    if (fade > 1)
                    {
                        fade %= 1;
                        FindNextGranulePosition();
                    }
                    if (writePhase == 0 && readerHead0Start > bufferLenght && readerHead1Start > bufferLenght) BufferMore();
                    else if (writePhase == 1 && readerHead0Start > 2 * bufferLenght && readerHead1Start > 2 * bufferLenght) BufferMore();
                    else if (writePhase == 2 && readerHead0Start < 2 * bufferLenght && readerHead1Start < 2 * bufferLenght) BufferMore();
                }
            }
            return count;
        }
        int c = 0;
        int p1 = 0;
        int p2 = 0;

        float GranuleLenght = 500f;
        float RealPos = 0;
        int SearchRadius = 100;
        int ACWindow = 30;

        float fade = 0f;
        float[] internalBuffer = new float[0];
        int writePhase = 0;
        int readerHead0Start = 0;
        float readerHead0 = 0f;
        float readerHeadSpd0 = 1f;
        int readerHead1Start = 0;
        float readerHead1 = 0f;
        float readerHeadSpd1 = 0f;

        int bufferLenght = 0;
        int chanels = 0;
        bool ended = false;
        bool nextSearchFor2 = true;
        private void BufferMore()
        {
            if (!ended)
            {
                int s = sourceStream.Read(internalBuffer, bufferLenght * chanels * writePhase, bufferLenght * chanels);
                if (s < bufferLenght * chanels)
                {
                    ended = true;
                    Array.Clear(internalBuffer, bufferLenght * chanels * writePhase + s, bufferLenght * chanels - s);
                }
                writePhase++;
                if (writePhase == 3) writePhase = 0;

            }
            else
            {
                Array.Clear(internalBuffer, bufferLenght * chanels * writePhase, bufferLenght * chanels);
                writePhase++;
                if (writePhase == 3) writePhase = 0;
            }
        }
        private void FindNextGranulePosition2()
        {
            if (nextSearchFor2)
            {
                readerHead1Start = (int)(readerHead0Start + GranuleLenght * Speed) % (bufferLenght * 3);
                readerHead1 = readerHead1Start;
                readerHeadSpd1 = Pitch;
                nextSearchFor2 = !nextSearchFor2;
            }
            else
            {
                readerHead0Start = (int)(readerHead1Start + GranuleLenght * Speed) % (bufferLenght * 3);
                readerHead0 = readerHead0Start;
                readerHeadSpd0 = Pitch;
                nextSearchFor2 = !nextSearchFor2;
            }
        }
        private void FindNextGranulePosition()
        {
            int L = bufferLenght * 3;

            float ACresult = 0;
            float maxResult = 0;
            int maxIndex = 0;

            if (nextSearchFor2)
            {
                for (int i = -SearchRadius; i < SearchRadius; i += 5)
                {
                    ACresult = 0;
                    for (int j = 0; j < ACWindow; j += 3)
                    {
                        ACresult += internalBuffer[(((int)readerHead0 + j + L) % L) * chanels] * internalBuffer[(((int)RealPos + j + i + L) % L) * chanels];
                    }
                    if (ACresult > maxResult)
                    {
                        maxResult = ACresult;
                        maxIndex = i;
                    }
                }
                readerHead1Start = ((maxIndex + (int)RealPos + L) % L);
                readerHead1 = readerHead1Start;
                readerHeadSpd1 = Pitch;
            }
            else
            {
                for (int i = -SearchRadius; i < SearchRadius; i += 5)
                {
                    ACresult = 0;
                    for (int j = 0; j < ACWindow; j += 3)
                    {
                        ACresult += internalBuffer[(((int)readerHead1 + j + L) % L) * chanels] * internalBuffer[(((int)RealPos + j + i + L) % L) * chanels];
                    }
                    if (ACresult > maxResult)
                    {
                        maxResult = ACresult;
                        maxIndex = i;
                    }
                }
                readerHead0Start = ((maxIndex + (int)RealPos + L) % L);
                readerHead0 = readerHead0Start;
                readerHeadSpd0 = Pitch;
            }

            nextSearchFor2 = !nextSearchFor2;
        }

        public RealTimePSOLAShifter(ISampleProvider source, int internalBufferLenght, int ACWindow, int SearchRadius, float GranuleLenght)
        {
            bufferLenght = internalBufferLenght;
            sourceStream = source;


            this.ACWindow = ACWindow;
            this.SearchRadius = SearchRadius;
            this.GranuleLenght = GranuleLenght;

            chanels = source.WaveFormat.Channels;
            internalBuffer = new float[3 * bufferLenght * chanels];

            BufferMore();
            BufferMore();
        }

        public float Speed { get; set; }
        public float Pitch { get; set; }
    }

    // A typ 0x14 (PAD - Refactored)
    public class PitchBufferSample2 : IPitchSample
    {
        public float[] Audio;
        public float pitch;

        public float A;
        public float D;
        public float S;
        public float R;

        public float PP;
        public float PL;

        public int PreviewTexture => 0;
        public bool BeatSync => false;
        public bool AffectedByMasterPitch => true;
        public PitchSampleFlags Possibilities => PitchSampleFlags.Pitch;
        public ushort Type => 0x14;

        public IPitchReader GetReader(float time)
        {
            return new PitchBufferReader2(this, time, A, D, S, R, PP, PL);
        }

        public void SaveArbData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (ushort)7);

            StreamHelper.SaveBytes(fs, pitch);

            StreamHelper.SaveBytes(fs, A);
            StreamHelper.SaveBytes(fs, D);
            StreamHelper.SaveBytes(fs, S);
            StreamHelper.SaveBytes(fs, R);

            StreamHelper.SaveBytes(fs, PP);
            StreamHelper.SaveBytes(fs, PL);

            StreamHelper.SaveBytes(fs, Audio.Length);

            for (int i = 0; i < Audio.Length; i++)
            {
                StreamHelper.SaveBytes(fs, Audio[i]);
            }
        }
        public void LoadArbData(FileStream fs)
        {
            ushort u = StreamHelper.LoadUShort(fs);

            if (u > 0) { pitch = StreamHelper.LoadFloat(fs); u--; }

            if (u > 0) { A = StreamHelper.LoadFloat(fs); u--; }
            if (u > 0) { D = StreamHelper.LoadFloat(fs); u--; }
            if (u > 0) { S = StreamHelper.LoadFloat(fs); u--; }
            if (u > 0) { R = StreamHelper.LoadFloat(fs); u--; }

            if (u > 0) { PP = StreamHelper.LoadFloat(fs); u--; }
            if (u > 0) { PL = StreamHelper.LoadFloat(fs); u--; }

            int l = StreamHelper.LoadInt(fs);

            Audio = new float[l];

            for (int i = 0; i < l; i++)
            {
                Audio[i] = StreamHelper.LoadFloat(fs);
            }
        }

        public float this[float sample]
        {
            get
            {
                if (sample < 0) sample = (sample % Audio.Length + Audio.Length);
                if (sample > Audio.Length) sample = sample % Audio.Length;
                float p = sample % 1;
                return (1 - p) * Audio[(int)sample % Audio.Length] + p * Audio[((int)sample + 1) % Audio.Length];
            }
        }

        public PitchBufferSample2(float[] samples, float pitch48000)
        {
            Audio = samples;
            pitch = pitch48000;
            A = 0;
            D = 0;
            S = 1;
            R = 0;
        }
        public PitchBufferSample2(float[] samples, float pitch48000, float A, float D, float S, float R)
        {
            Audio = samples;
            pitch = pitch48000;
            this.A = A;
            this.D = D;
            this.S = S;
            this.R = R;
        }
        public PitchBufferSample2(PitchBufferSample2 pbs)
        {
            Audio = (float[])pbs.Audio.Clone();
            pitch = pbs.pitch;
            this.A = pbs.A;
            this.D = pbs.D;
            this.S = pbs.S;
            this.R = pbs.R;
            this.PP = pbs.PP;
            this.PL = pbs.PL;
        }
        public void PostLoad()
        {
        }
    }
    public class PitchBufferReader2 : IPitchReader
    {
        float pos = 0;
        float posr = 0;
        PitchBufferSample2 cps;
        ADSR adsr;
        float PP = 0;
        float PL = 0;

        int posreal = 0;

        public PitchBufferReader2(PitchBufferSample2 cps, float A, float D, float S, float R)
        {
            this.cps = cps;
            PROPERTIES[2] = 1;

            pos = 0;
            posr = 0 + 15;

            adsr = new ADSR(A, D, S, R);
        }
        public PitchBufferReader2(PitchBufferSample2 cps, float timeIn, float A, float D, float S, float R, float PP, float PL)
        {
            this.cps = cps;
            PROPERTIES[2] = 1;

            pos = (int)(timeIn * 48000);
            posr = pos + 15;

            adsr = new ADSR(A, D, S, R);

            this.PP = PP;
            this.PL = PL;
        }

        private float spd = 1;
        float[] buffer = new float[0];

        private float[] PROPERTIES = new float[256];
        public void SetProperty(byte index, float val)
        {
            PROPERTIES[index] = val;
        }
        public float GetPropertyValue(byte index)
        {
            return PROPERTIES[index];
        }

        public void NoteOff()
        {
            adsr.Release();
        }

        public void ReadAdd(ref float[] samples, float L, float R)
        {
            spd = (float)Math.Pow(2, PROPERTIES[3] / 12f) * 2;

            if (buffer.Length != samples.Length) buffer = new float[samples.Length];
            Array.Clear(buffer, 0, samples.Length);
            //PROC
            for (int i = 0; i < samples.Length / 2; i++)
            {
                if (posreal < PL)
                {
                    spd = (float)Math.Pow(2, (PROPERTIES[3] + PP * (1 - posreal / PL)) / 12f) * 2;
                }
                    pos += spd;
                    posr += spd;
                
                buffer[i * 2] = cps[pos] * L;
                buffer[i * 2 + 1] = cps[posr] * R;

                posreal++;
            }
            adsr.ProccessLR(ref buffer);
            //PROC
            for (int i = 0; i < samples.Length; i++) samples[i] += buffer[i];
        }

        public void Dispose()
        {
        }

        public bool ReleaseEnded => adsr.ReleaseEnded;
    }
}
