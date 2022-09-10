using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace SpartaRemixStudio2019
{
    public static class ProjectHelper
    {
        public static void SaveProject(Project p, string projectName)
        {
            if (!Directory.Exists("Projects/" + projectName))
            {
                Directory.CreateDirectory("Projects/" + projectName);
            }
            
            FileStream fs = new FileStream("Projects/" + projectName + "/PROJ.SRP", FileMode.Create);

            StreamHelper.SaveBytes(fs, (int)7);
            // 0) "Next" Data
            StreamHelper.SaveBytes(fs, (int)6);
            StreamHelper.SaveBytes(fs, p.NextTrackNumber);
            StreamHelper.SaveBytes(fs, p.NextPatternNumber);
            StreamHelper.SaveBytes(fs, p.NextSampleNumber);
            StreamHelper.SaveBytes(fs, p.NextMediaNumber);
            StreamHelper.SaveBytes(fs, p.NextNTNumber);
            StreamHelper.SaveBytes(fs, p.NextMPNumber);
            // 1) General Data (DATA)
            StreamHelper.SaveBytes(fs, (int)6);
            StreamHelper.SaveString(fs, "1.1");
            StreamHelper.SaveBytes(fs, p.WIDTH);
            StreamHelper.SaveBytes(fs, p.HEIGHT);
            StreamHelper.SaveBytes(fs, p.MasterPitch);
            StreamHelper.SaveBytes(fs, p.BPM);
            StreamHelper.SaveBytes(fs, p.m.OutputArray);
            // 2) Mixer (MIXE)
            StreamHelper.SaveBytes(fs, (int)2);
            StreamHelper.SaveBytes(fs, p.TrackList.Count);
            for (int i = 0; i < p.TrackList.Count; i++)
            {
                StreamHelper.SaveBytes(fs, p.TrackList[i]);
            }
            StreamHelper.SaveBytes(fs, p.m.Layers.Count);
            for (int i = 0; i < p.m.Layers.Count; i++)
            {
                StreamHelper.SaveBytes(fs, p.m.Layers[i].ListOfLinks.Count);
                for (int j = 0; j < p.m.Layers[i].ListOfLinks.Count; j++)
                {
                    StreamHelper.SaveBytes(fs, (int)4);
                    StreamHelper.SaveBytes(fs, p.m.Layers[i].ListOfLinks[j].MixerPosX);
                    StreamHelper.SaveBytes(fs, p.m.Layers[i].ListOfLinks[j].MixerPosY);
                    StreamHelper.SaveBytes(fs, p.m.Layers[i].ListOfLinks[j].Links.Item1);
                    StreamHelper.SaveBytes(fs, p.m.Layers[i].ListOfLinks[j].Links.Item2.Count);
                    for (int k = 0; k < p.m.Layers[i].ListOfLinks[j].Links.Item2.Count; k++)
                    {
                        StreamHelper.SaveBytes(fs, p.m.Layers[i].ListOfLinks[j].Links.Item2[k]);
                    }
                }
            }
            // 3) TLM (PTLM)
            StreamHelper.SaveBytes(fs, (int)1);
            StreamHelper.SaveBytes(fs, p.ProjectMedia.Count);
            foreach (KeyValuePair<uint, TimelineMedia> k in p.ProjectMedia)
            {
                StreamHelper.SaveBytes(fs, (int)2);
                StreamHelper.SaveBytes(fs, k.Key);
                TimeLineMediaHelper.Save(fs, k.Value);
            }
            // 4) NMT (NMT)
            StreamHelper.SaveBytes(fs, (int)1);
            StreamHelper.SaveBytes(fs, p.NumberTracks.Count);
            foreach (KeyValuePair<uint, NumberTrack> k in p.NumberTracks)
            {
                StreamHelper.SaveBytes(fs, (int)2);
                StreamHelper.SaveBytes(fs, k.Key);
                k.Value.Save(fs);
            }
            // 5) TLT (PTLT)
            StreamHelper.SaveBytes(fs, (int)1);
            StreamHelper.SaveBytes(fs, p.Tracks.Count);
            foreach (KeyValuePair<uint, VideoTrack> k in p.Tracks)
            {
                StreamHelper.SaveBytes(fs, (int)2);
                StreamHelper.SaveBytes(fs, k.Key);
                k.Value.Save(fs);
            }
            // 6) MXT (MXT)
            StreamHelper.SaveBytes(fs, (int)1);
            StreamHelper.SaveBytes(fs, p.AudioMPs.Count);
            foreach (KeyValuePair<uint, MixerPoint> k in p.AudioMPs)
            {
                StreamHelper.SaveBytes(fs, (int)2);
                StreamHelper.SaveBytes(fs, k.Key);
                k.Value.Save(fs);
            }

            fs.Close();
            fs.Dispose();

            foreach (KeyValuePair<uint,SampleAV> sav in p.ProjectSamples)
            {
                if (sav.Value.ips != null)
                {
                    FileStream fsa = new FileStream("Projects/" + projectName + "/" + sav.Key +".SMPA", FileMode.Create);

                    StreamHelper.SaveBytes(fsa, sav.Value.ips.Type);
                    sav.Value.ips.SaveArbData(fsa);

                    fsa.Close();
                    fsa.Dispose();
                }
                if (sav.Value.ivs != null)
                {
                    FileStream fsv = new FileStream("Projects/" + projectName + "/" + sav.Key + ".SMPV", FileMode.Create);

                    StreamHelper.SaveBytes(fsv, sav.Value.ivs.Type);
                    sav.Value.ivs.SaveArbData(fsv);

                    fsv.Close();
                    fsv.Dispose();
                }
                if (sav.Value.name != null)
                {
                    FileStream fsv = new FileStream("Projects/" + projectName + "/" + sav.Key + ".NAME", FileMode.Create);

                    StreamHelper.SaveString(fsv, sav.Value.name);

                    fsv.Close();
                    fsv.Dispose();
                }
                if (sav.Value.ssfx != null)
                {
                    FileStream fsv = new FileStream("Projects/" + projectName + "/" + sav.Key + ".SSFX", FileMode.Create);
                    
                    sav.Value.ssfx.Save(fsv);

                    fsv.Close();
                    fsv.Dispose();
                }
            }
            foreach (KeyValuePair<uint, Pattern> pt in p.ProjectPatterns)
            {
                FileStream fsp = new FileStream("Projects/" + projectName + "/" + pt.Key + ".SPAT", FileMode.Create);
                pt.Value.Save(fsp);
                fsp.Close();
                fsp.Dispose();
            }
        }
        public static List<string> GetProjectNames()
        {
            string[] s = Directory.GetDirectories("Projects");
            List<string> ret = new List<string>();
            foreach(string ss in s)
            {
                string[] sss = ss.Split('/');
                ret.Add(sss[sss.Length - 1]);
            }
            return ret;
        }
        public static void LoadProject(Project p, string projectName)
        {
            if (projectName != null)
            {
                FileStream fs = new FileStream("Projects/" + projectName + "/PROJ.SRP", FileMode.Open);

                int lenght0 = StreamHelper.LoadInt(fs);
                int lenght = 0;
                int l0 = 0;

                // 0) "Next" Data
                if (lenght0 > 0)
                {
                    lenght = StreamHelper.LoadInt(fs);
                    if (lenght > 0) { p.NextTrackNumber = StreamHelper.LoadUInt(fs); lenght--; }
                    if (lenght > 0) { p.NextPatternNumber = StreamHelper.LoadUInt(fs); lenght--; }
                    if (lenght > 0) { p.NextSampleNumber = StreamHelper.LoadUInt(fs); lenght--; }
                    if (lenght > 0) { p.NextMediaNumber = StreamHelper.LoadUInt(fs); lenght--; }
                    if (lenght > 0) { p.NextNTNumber = StreamHelper.LoadUInt(fs); lenght--; }
                    if (lenght > 0) { p.NextMPNumber = StreamHelper.LoadUInt(fs); lenght--; }
                    if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can't be opened. Terminating SRS"); throw new Exception("Couldn't open project"); }
                }
                lenght0--;
                // 1) General Data (DATA)
                if (lenght0 > 0)
                {
                    lenght = StreamHelper.LoadInt(fs);
                    if (lenght > 0) { StreamHelper.LoadString(fs); lenght--; }
                    if (lenght > 0) { p.WIDTH = StreamHelper.LoadInt(fs); lenght--; if (Form1.changeAR) p.WIDTH = Form1.changeWidth; }
                    if (lenght > 0) { p.HEIGHT = StreamHelper.LoadInt(fs); lenght--; if (Form1.changeAR) p.HEIGHT = Form1.changeHeight; }
                    if (lenght > 0) { p.MasterPitch = StreamHelper.LoadFloat(fs); lenght--; }
                    if (lenght > 0) { p.BPM = StreamHelper.LoadFloat(fs); lenght--; }
                    if (lenght > 0) { p.m.OutputArray = StreamHelper.LoadUInt(fs); lenght--; }
                    if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can't be opened. Terminating SRS"); throw new Exception("Couldn't open project"); }
                }
                lenght0--;
                // 2) Mixer (MIXE)
                if (lenght0 > 0)
                {
                    lenght = StreamHelper.LoadInt(fs);
                    if (lenght > 0)
                    {
                        l0 = StreamHelper.LoadInt(fs);
                        p.TrackList.Clear();
                        for (int i = 0; i < l0; i++)
                        {
                            p.TrackList.Add(StreamHelper.LoadUInt(fs));
                        }
                    }
                    lenght--;
                    if (lenght > 0)
                    {
                        l0 = StreamHelper.LoadInt(fs);
                        for (int i = 0; i < l0; i++)
                        {
                            p.m.Layers.Add(new MixerLayer());
                            int l1 = StreamHelper.LoadInt(fs);
                            for (int j = 0; j < l1; j++)
                            {
                                lenght = StreamHelper.LoadInt(fs);
                                int px = 0;
                                int py = 0;
                                if (lenght > 0) { px = StreamHelper.LoadInt(fs); lenght--; }
                                if (lenght > 0) { py = StreamHelper.LoadInt(fs); lenght--; }
                                if (lenght > 0) { p.m.Layers[i].ListOfLinks.Add(new MixerLink(StreamHelper.LoadUInt(fs))); lenght--; }
                                p.m.Layers[i].ListOfLinks[j].MixerPosX = px;
                                p.m.Layers[i].ListOfLinks[j].MixerPosY = py;
                                int l2 = 0;
                                if (lenght > 0) { l2 = StreamHelper.LoadInt(fs); lenght--; }
                                if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can't be opened. Terminating SRS"); throw new Exception("Couldn't open project"); }
                                for (int k = 0; k < l2; k++)
                                {
                                    p.m.Layers[i].ListOfLinks[j].Links.Item2.Add(StreamHelper.LoadUInt(fs));
                                }
                            }
                        }
                    }
                    lenght--;
                    if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can't be opened. Terminating SRS"); throw new Exception("Couldn't open project"); }
                }
                lenght0--;
                // 3) TLM (PTLM)
                if (lenght0 > 0)
                {
                    lenght = StreamHelper.LoadInt(fs);
                    if (lenght > 0)
                    {
                        l0 = StreamHelper.LoadInt(fs);
                        for (int i = 0; i < l0; i++)
                        {
                            int lenght1 = StreamHelper.LoadInt(fs);
                            if (lenght1 >= 2)
                            {
                                uint Key = StreamHelper.LoadUInt(fs);
                                TimelineMedia tlm = new TimelineMedia(null, false, 0);
                                TimeLineMediaHelper.Load(fs, tlm);
                                p.ProjectMedia.Add(Key, tlm);
                                lenght1 -= 2;
                            }
                            if (lenght1 > 0) { MessageBox.Show("Project possibly from higher version detected. Can't be opened. Terminating SRS"); throw new Exception("Couldn't open project"); }
                        }
                    }
                    lenght--;
                    if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can't be opened. Terminating SRS"); throw new Exception("Couldn't open project"); }
                }
                lenght0--;
                // 4) NMT (PTLT)
                if (lenght0 > 0)
                {
                    lenght = StreamHelper.LoadInt(fs);
                    if (lenght > 0)
                    {
                        l0 = StreamHelper.LoadInt(fs);
                        for (int i = 0; i < l0; i++)
                        {
                            int lenght1 = StreamHelper.LoadInt(fs);
                            if (lenght1 >= 2)
                            {
                                uint Key = StreamHelper.LoadUInt(fs);
                                NumberTrack tlt = new NumberTrack();
                                tlt.Load(fs);
                                p.NumberTracks.Add(Key, tlt);
                                lenght1 -= 2;
                            }
                            if (lenght1 > 0) { MessageBox.Show("Project possibly from higher version detected. Can't be opened. Terminating SRS"); throw new Exception("Couldn't open project"); }
                        }
                    }
                    lenght--;
                    if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can't be opened. Terminating SRS"); throw new Exception("Couldn't open project"); }
                }
                lenght0--;
                // 5) TLT (PTLT)
                if (lenght0 > 0)
                {
                    lenght = StreamHelper.LoadInt(fs);
                    if (lenght > 0)
                    {
                        l0 = StreamHelper.LoadInt(fs);
                        for (int i = 0; i < l0; i++)
                        {
                            int lenght1 = StreamHelper.LoadInt(fs);
                            if (lenght1 >= 2)
                            {
                                uint Key = StreamHelper.LoadUInt(fs);
                                VideoTrack tlt = new VideoTrack();
                                tlt.Load(fs);
                                p.Tracks.Add(Key, tlt);
                                lenght1 -= 2;
                            }
                            if (lenght1 > 0) { MessageBox.Show("Project possibly from higher version detected. Can't be opened. Terminating SRS"); throw new Exception("Couldn't open project"); }
                        }
                    }
                    lenght--;
                    if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can't be opened. Terminating SRS"); throw new Exception("Couldn't open project"); }
                }
                lenght0--;
                // 6) TLT (PTLT)
                if (lenght0 > 0)
                {
                    lenght = StreamHelper.LoadInt(fs);
                    if (lenght > 0)
                    {
                        l0 = StreamHelper.LoadInt(fs);
                        for (int i = 0; i < l0; i++)
                        {
                            int lenght1 = StreamHelper.LoadInt(fs);
                            if (lenght1 >= 2)
                            {
                                uint Key = StreamHelper.LoadUInt(fs);
                                MixerPoint mp = new MixerPoint();
                                mp.Load(fs);
                                p.AudioMPs.Add(Key, new MixerPoint());
                                lenght1 -= 2;
                            }
                            if (lenght1 > 0) { MessageBox.Show("Project possibly from higher version detected. Can't be opened. Terminating SRS"); throw new Exception("Couldn't open project"); }
                        }
                    }
                    lenght--;
                    if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can't be opened. Terminating SRS"); throw new Exception("Couldn't open project"); }
                }
                lenght0--;
                if (lenght0 > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }

                fs.Close();
                fs.Dispose();

                p.ProjectPatterns = new Dictionary<uint, Pattern>();
                p.ProjectSamples = new Dictionary<uint, SampleAV>();

                string[] sa = Directory.GetFiles("Projects/" + projectName);

                foreach (string file in sa)
                {
                    string s2 = Path.GetExtension(file);

                    if (s2 == ".SMPA")
                    {
                        string s3 = Path.GetFileNameWithoutExtension(file);
                        uint u = uint.Parse(s3);

                        FileStream fs2 = new FileStream(file, FileMode.Open);

                        ushort t = StreamHelper.LoadUShort(fs2);
                        IPitchSample ips = SampleHelper.GetIPSByNumber(p, t);
                        ips.LoadArbData(fs2);

                        fs2.Close();
                        fs2.Dispose();

                        if (!p.ProjectSamples.ContainsKey(u)) p.ProjectSamples.Add(u, new SampleAV());
                        p.ProjectSamples[u] = new SampleAV() { ips = ips, ivs = p.ProjectSamples[u].ivs, name = p.ProjectSamples[u].name, ssfx = p.ProjectSamples[u].ssfx };
                    }
                    if (s2 == ".SMPV")
                    {
                        string s3 = Path.GetFileNameWithoutExtension(file);
                        uint u = uint.Parse(s3);

                        FileStream fs2 = new FileStream(file, FileMode.Open);

                        ushort t = StreamHelper.LoadUShort(fs2);
                        IVideoSample ivs = SampleHelper.GetIVSByNumber(p, t);
                        ivs.LoadArbData(fs2);

                        fs2.Close();
                        fs2.Dispose();

                        if (!p.ProjectSamples.ContainsKey(u)) p.ProjectSamples.Add(u, new SampleAV());
                        p.ProjectSamples[u] = new SampleAV() { ips = p.ProjectSamples[u].ips, ivs = ivs, name = p.ProjectSamples[u].name, ssfx = p.ProjectSamples[u].ssfx };
                    }
                    if (s2 == ".NAME")
                    {
                        string s3 = Path.GetFileNameWithoutExtension(file);
                        uint u = uint.Parse(s3);

                        FileStream fs2 = new FileStream(file, FileMode.Open);

                        string t = StreamHelper.LoadString(fs2);

                        fs2.Close();
                        fs2.Dispose();

                        if (!p.ProjectSamples.ContainsKey(u)) p.ProjectSamples.Add(u, new SampleAV());
                        p.ProjectSamples[u] = new SampleAV() { ips = p.ProjectSamples[u].ips, ivs = p.ProjectSamples[u].ivs, name = t, ssfx = p.ProjectSamples[u].ssfx };
                    }
                    if (s2 == ".SSFX")
                    {
                        string s3 = Path.GetFileNameWithoutExtension(file);
                        uint u = uint.Parse(s3);

                        FileStream fs2 = new FileStream(file, FileMode.Open);

                        SampleStatFX ssfx = new SampleStatFX();
                        ssfx.Load(fs2);

                        fs2.Close();
                        fs2.Dispose();

                        if (!p.ProjectSamples.ContainsKey(u)) p.ProjectSamples.Add(u, new SampleAV());
                        p.ProjectSamples[u] = new SampleAV() { ips = p.ProjectSamples[u].ips, ivs = p.ProjectSamples[u].ivs, name = p.ProjectSamples[u].name, ssfx = ssfx};
                    }
                    if (s2 == ".SPAT")
                    {
                        string s3 = Path.GetFileNameWithoutExtension(file);
                        uint u = uint.Parse(s3);

                        FileStream fs2 = new FileStream(file, FileMode.Open);
                        Pattern pt = new Pattern();
                        pt.Load(fs2);

                        fs2.Close();
                        fs2.Dispose();

                        p.ProjectPatterns.Add(u, pt);
                    }
                }
                foreach (KeyValuePair<uint, SampleAV> sav in p.ProjectSamples)
                {
                    sav.Value.ips?.PostLoad();
                    sav.Value.ivs?.PostLoad();
                }

                List<KeyValuePair<uint, SampleAV>> smpsort = new List<KeyValuePair<uint, SampleAV>>(p.ProjectSamples);
                smpsort.Sort((KeyValuePair<uint, SampleAV> a, KeyValuePair<uint, SampleAV> b) => (int)a.Key - (int)b.Key);
                p.ProjectSamples = new Dictionary<uint, SampleAV>();
                foreach (KeyValuePair<uint, SampleAV> k in smpsort) p.ProjectSamples.Add(k.Key, k.Value);

                List<KeyValuePair<uint, Pattern>> ptsort = new List<KeyValuePair<uint, Pattern>>(p.ProjectPatterns);
                ptsort.Sort((KeyValuePair<uint, Pattern> a, KeyValuePair<uint, Pattern> b) => (int)a.Key - (int)b.Key);
                p.ProjectPatterns = new Dictionary<uint, Pattern>();
                foreach (KeyValuePair<uint, Pattern> k in ptsort) p.ProjectPatterns.Add(k.Key, k.Value);

            }
        }
    }

    public class Project
    {
        public Dictionary<uint, VideoTrack> Tracks = new Dictionary<uint, VideoTrack>();
        public uint NextTrackNumber { get; set; }
        public Dictionary<uint, MixerPoint> AudioMPs = new Dictionary<uint, MixerPoint>();
        public uint NextMPNumber { get; set; }
        public Dictionary<uint, NumberTrack> NumberTracks = new Dictionary<uint, NumberTrack>();
        public uint NextNTNumber { get; set; }

        public Dictionary<uint, SampleAV> ProjectSamples = new Dictionary<uint, SampleAV>();
        public uint NextSampleNumber { get; set; }
        public Dictionary<uint, TimelineMedia> ProjectMedia = new Dictionary<uint, TimelineMedia>();
        public uint NextMediaNumber { get; set; }
        public Dictionary<uint, Pattern> ProjectPatterns = new Dictionary<uint, Pattern>();
        public uint NextPatternNumber { get; set; }

        public List<uint> TrackList = new List<uint>();

        public uint AddTrack(VideoTrack t)
        {
            Tracks.Add(NextTrackNumber, t);
            NextTrackNumber++;
            return NextTrackNumber - 1;
        }
        public uint AddMixerPoint(MixerPoint t)
        {
            AudioMPs.Add(NextMPNumber, t);
            NextMPNumber++;
            return NextMPNumber - 1;
        }
        public uint AddNumberTrack(NumberTrack t)
        {
            NumberTracks.Add(NextNTNumber, t);
            NextNTNumber++;
            return NextNTNumber - 1;
        }
        public uint AddSample(SampleAV sav)
        {
            ProjectSamples.Add(NextSampleNumber, sav);
            NextSampleNumber++;
            return NextSampleNumber - 1;
        }
        public uint AddMedia(TimelineMedia tlm)
        {
            ProjectMedia.Add(NextMediaNumber, tlm);
            NextMediaNumber++;
            return NextMediaNumber - 1;
        }
        public uint AddPattern(Pattern p)
        {
            ProjectPatterns.Add(NextPatternNumber, p);
            NextPatternNumber++;
            return NextPatternNumber - 1;
        }

        public Project()
        {
            m = new Mixer(this);
            NextMPNumber = 1048576;
        }

        public Mixer m;

        public float GetBPM(float timeBeat)
        {
            return BPM;
        }
        public float GetBeatDurationSec(float timeBeat)
        {
            return 240 / GetBPM(timeBeat);
        }
        public float GetBeatDurationSamp(float timeBeat)
        {
            return (240 * 48000) / GetBPM(timeBeat);
        }
        public float GetMasterPitch(float timeBeat)
        {
            return MasterPitch;
        }

        public int WIDTH = 1280;
        public int HEIGHT = 720;
        public float MasterPitch = -7f;
        public float BPM = 140f;
    }
    public class Mixer : ISampleProvider
    {
        public float AutoMultiplier = 1;
        public bool UseAuto = true;

        public List<MixerLayer> Layers = new List<MixerLayer>();
        Project p;
        public uint OutputArray = 1048576;

        public float BeatPos = 0;
        public int SamplePos = 0;
        public bool Playing = false;

        public WaveFormat WaveFormat => WaveFormat.CreateIeeeFloatWaveFormat(48000, 2);
        public int Read(float[] buffer, int offset, int count)
        {
            if (!Playing && !Stoped)
            {
                foreach (uint u in p.Tracks.Keys)
                {
                    p.Tracks[u].StopAudio();
                }
                BeatPos = 0;
                Stoped = true;
            }
            if (Playing)
            {
                int samplePos2 = SamplePos + (buffer.Length / 2);
                float BeatPos2 = (samplePos2 / p.GetBeatDurationSamp(0));

                //PARALEL
                Parallel.ForEach<NumberTrack>(p.NumberTracks.Values, (NumberTrack tlt) =>
                {
                    tlt.Read(BeatPos);
                });
                Parallel.ForEach<VideoTrack>(p.Tracks.Values, (VideoTrack tlt) =>
                {
                    if (tlt.Buffer.Length != buffer.Length) tlt.Buffer = new float[buffer.Length];
                    Array.Clear(tlt.Buffer, 0, tlt.Buffer.Length);

                    tlt.Read(BeatPos);
                });
                Parallel.ForEach<MixerPoint>(p.AudioMPs.Values, (MixerPoint tlt) =>
                {
                    if (tlt.Buffer.Length != buffer.Length) tlt.Buffer = new float[buffer.Length];
                    Array.Clear(tlt.Buffer, 0, tlt.Buffer.Length);
                });
                //HALF PARALEL
                for (int i = 0; i < Layers.Count; i++)
                {
                    MixerLayer ml = Layers[i];
                    //PARALEL
                        Parallel.ForEach(ml.ListOfLinks, (MixerLink t) =>
                    {
                        uint u = t.Links.Item1;

                        if (u >> 20 == 0 && p.Tracks.ContainsKey(u))
                        {
                            p.Tracks[t.Links.Item1].GetAudio(BeatPos, BeatPos2);
                            try
                            {
                                foreach (IAudioFX afx in p.Tracks[t.Links.Item1].afx)
                                {
                                    afx.UpdateValues(BeatPos);
                                    afx.Apply(ref p.Tracks[t.Links.Item1].Buffer);
                                }
                            }
                            catch
                            {

                            }
                        }
                        if (u >> 20 == 1 && p.AudioMPs.ContainsKey(u))
                        {
                            try
                            {
                                foreach (IAudioFX afx in p.AudioMPs[t.Links.Item1].afx)
                                {
                                    afx.UpdateValues(BeatPos);
                                    afx.Apply(ref p.AudioMPs[t.Links.Item1].Buffer);
                                }
                            }
                            catch
                            {

                            }
                        }
                    });
                    
                    //NEPARALEL
                    foreach (MixerLink t in ml.ListOfLinks)
                        {
                            uint u = t.Links.Item1;

                            if (u >> 20 == 0 && p.Tracks.ContainsKey(u))
                            {
                                foreach (uint v in t.Links.Item2)
                                {
                                    float P = Math.Min(Math.Max(p.Tracks[u].Pan, -1), 1);
                                    float R = (float)Math.Pow(10, p.Tracks[u].Volume / 10) * (float)((Math.Cos(P * Math.PI / 4) - Math.Sin(P * Math.PI / 4)));
                                    float L = (float)Math.Pow(10, p.Tracks[u].Volume / 10) * (float)((Math.Cos(P * Math.PI / 4) + Math.Sin(P * Math.PI / 4)));

                                    if (v >> 20 == 0 && p.Tracks.ContainsKey(v))
                                    {
                                        for (int j = 0; j < buffer.Length; j += 2)
                                        {
                                            p.Tracks[v].Buffer[j] += p.Tracks[u].Buffer[j] * L;
                                            p.Tracks[v].Buffer[j + 1] += p.Tracks[u].Buffer[j + 1] * R;
                                        }
                                    }
                                    if (v >> 20 == 1 && p.AudioMPs.ContainsKey(v))
                                    {
                                        for (int j = 0; j < buffer.Length; j += 2)
                                        {
                                            p.AudioMPs[v].Buffer[j] += p.Tracks[u].Buffer[j] * L;
                                            p.AudioMPs[v].Buffer[j + 1] += p.Tracks[u].Buffer[j + 1] * R;
                                        }
                                    }

                                }
                            }
                            if (u >> 20 == 1 && p.AudioMPs.ContainsKey(u))
                            {
                                foreach (uint link in t.Links.Item2)
                                {
                                    uint v = t.Links.Item1;
                                    if (v >> 20 == 0 && p.Tracks.ContainsKey(v))
                                    {
                                        for (int j = 0; j < buffer.Length; j++) p.Tracks[v].Buffer[j] += p.AudioMPs[u].Buffer[j];
                                    }
                                    if (v >> 20 == 1 && p.AudioMPs.ContainsKey(v))
                                    {
                                        for (int j = 0; j < buffer.Length; j++) p.AudioMPs[v].Buffer[j] += p.AudioMPs[u].Buffer[j];
                                    }
                                }
                            }

                        }
                }
                //NOT PARALEL
                if (OutputArray >> 20 == 0)
                {
                    if (p.Tracks.ContainsKey(OutputArray))
                    {
                        for (int j = 0; j < p.Tracks[OutputArray].Buffer.Length; j++)
                        {
                            {
                                if (AutoMultiplier == 0) AutoMultiplier = 1;
                                if (Math.Abs(p.Tracks[OutputArray].Buffer[j] * AutoMultiplier) > 0.95f)
                                {
                                    AutoMultiplier = Math.Abs(0.95f / p.Tracks[OutputArray].Buffer[j]);

                                }
                                if (UseAuto) buffer[j] = AutoMultiplier * p.Tracks[OutputArray].Buffer[j];
                                else buffer[j] = p.Tracks[OutputArray].Buffer[j];
                            }
                        }
                    }
                }
                else if (OutputArray >> 20 == 1)
                {
                    if (p.AudioMPs.ContainsKey(OutputArray))
                    {
                        for (int j = 0; j < p.AudioMPs[OutputArray].Buffer.Length; j++)
                        {
                            {
                                if (AutoMultiplier == 0) AutoMultiplier = 1;
                                if (Math.Abs(p.AudioMPs[OutputArray].Buffer[j] * AutoMultiplier) > 0.95f)
                                {
                                    AutoMultiplier = Math.Abs(0.95f / p.AudioMPs[OutputArray].Buffer[j]);

                                }
                                if (UseAuto) buffer[j] = AutoMultiplier * p.AudioMPs[OutputArray].Buffer[j];
                                else buffer[j] = p.AudioMPs[OutputArray].Buffer[j];
                            }
                        }
                    }
                }

                if (AutoMultiplier < 0.000001f)
                {
                    AutoMultiplier = 1;
                    Stop();
                }

                SamplePos += (buffer.Length / 2);
                BeatPos = (float)(SamplePos / p.GetBeatDurationSamp(0));
            }

            return count;
        }

        public Mixer(Project p)
        {
            this.p = p;
        }
        public bool Stoped = false;
        public void Play()
        {
            foreach (VideoTrack tlt in p.Tracks.Values) tlt.Init();
            SamplePos = (int)(p.GetBeatDurationSamp(0) * BeatPos);
            Playing = true;
        }
        public void Stop()
        {
            Playing = false;
            Stoped = false;
        }

        public void AddNode(uint u)
        {
            Layers[0].AddTrack(u);
            Layers[0].UnLinkXtoY(u, OutputArray);
        }

        public uint GetNodeLayer(uint track)
        {
            uint index = uint.MaxValue;
            uint i = 0;
            foreach (MixerLayer ml in Layers)
            {
                foreach (MixerLink ml2 in ml.ListOfLinks)
                {
                    if (ml2.Links.Item1 == track) index = i;
                }
                i++;
            }
            return index;
        }
        public MixerLink GetNode(uint track)
        {
            uint index = uint.MaxValue;
            uint i = 0;
            foreach (MixerLayer ml in Layers)
            {
                foreach (MixerLink ml2 in ml.ListOfLinks)
                {
                    if (ml2.Links.Item1 == track) return ml2;
                }
                i++;
            }
            return null;
        }
        public void CloneStructure(List<uint> TrackNamesSource, List<uint> TrackNamesDestination)
        {
            for (int i = 0; i < TrackNamesSource.Count && i < TrackNamesDestination.Count; i++)
            {
                uint NodeLayer = GetNodeLayer(TrackNamesSource[i]);
                Layers[(int)NodeLayer].AddTrack(TrackNamesDestination[i]);
                MixerLink added = GetNode(TrackNamesDestination[i]);
                MixerLink source = GetNode(TrackNamesSource[i]);

                foreach (uint u in source.Links.Item2)
                {
                    if (TrackNamesSource.Contains(u))
                    {
                        int pos = TrackNamesSource.IndexOf(u);
                        added.Links.Item2.Add(TrackNamesDestination[pos]);
                    }
                    else
                    {
                        added.Links.Item2.Add(u);
                    }
                }
            }
        }
    }
    public class MixerLayer
    {
        public List<MixerLink> ListOfLinks = new List<MixerLink>();

        public void AddTrack(uint i)
        {
            MixerLink ml = new MixerLink(i);
            ml.MixerPosX = 0 + 35 * (ListOfLinks.Count % 1);
            ml.MixerPosY = 10 + 25 * (ListOfLinks.Count / 1);
            ListOfLinks.Add(ml);
        }
        public void UnLinkXtoY(uint from, uint to)
        {
            foreach (MixerLink ml in ListOfLinks)
            {
                if (ml.Links.Item1 == from) ml.UnLinkTo(to);
            }
        }
    }
    public class MixerLink
    {
        public Tuple<uint, List<uint>> Links;
        public int MixerPosX = 0;
        public int MixerPosY = 0;

        public MixerLink(uint i)
        {
            Links = new Tuple<uint, List<uint>>(i, new List<uint>());
        }
        public void UnLinkTo(uint track)
        {
            if (Links.Item2.Contains(track)) Links.Item2.Remove(track);
            else Links.Item2.Add(track);
        }
    }
}
