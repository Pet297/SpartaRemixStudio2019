using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using System.Drawing;

namespace SpartaRemixStudio2019
{
    public static class TrackHelper
    {
        public static List<uint> GetNumberTracks(uint videoTrackIndex)
        {
            if (Form1.Project.Tracks.ContainsKey(videoTrackIndex))
            {
                VideoTrack vt = Form1.Project.Tracks[videoTrackIndex];
                List<uint> list = new List<uint>();

                foreach (IAudioFX iafx in vt.afx)
                {
                    EffectHelper.GetNumberTracks(iafx, list);
                }
                foreach (IVideoFX ivfx in vt.vfx)
                {
                    EffectHelper.GetNumberTracks(ivfx, list);
                }
                return list;
            }
            else return new List<uint>();
        }
    }
    // DRUHY
    public class MixerPoint
    {
        public List<IAudioFX> afx = new List<IAudioFX>();
        public float[] Buffer = new float[0];

        public MixerPoint()
        {
        }
        public object Clone()
        {
            return new MixerPoint();
        }
        
        public void OutputAddAudio(ref float[] buffer, float timeBeat0, float timeBeat1) {}

        public void Save(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)1);
            StreamHelper.SaveBytes(fs, afx.Count);
            for (int i = 0; i < afx.Count; i++)
            {
                EffectHelper.Save(fs, afx[i]);
            }
        }
        public void Load(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0)
            {
                int l0 = StreamHelper.LoadInt(fs);
                for (int i = 0; i < l0; i++) afx.Add(EffectHelper.LoadAFX(fs));
                lenght--;
            }
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }
        public void Init(Project p)
        {
        }
    }
    public class VideoTrack : INoteReciever2
    {
        // EdITABLE
        public List<IAudioFX> afx = new List<IAudioFX>();
        public List<IVideoFX> vfx = new List<IVideoFX>();
        public bool ShowAutomation = true;
        public List<uint> Media = new List<uint>();

        public byte ChildIndex = 0;
        public byte RenderType = 0; // (0 = 3D) (1 = zplostit 2D)
        public byte RenderShape = 0; // (0 = quad) (1 = infX) (2 = infY) (3 = infXY)
        public byte BlendMode = 0; // (0 = a2d) (1 = w3d) (2 = *) (3 = *)

        public float Volume = 0;
        public float Pan = 0;
        // END EDITABLE

        private int TextureVideo = 0;
        
        private bool StopPatternVideo = false;
        public void ShowVFXForm()
        {
            VideoFXEdit vfe = new VideoFXEdit(vfx);
            vfe.Show();
        }
        public void ShowAFXForm()
        {
            AudioFXEdit afe = new AudioFXEdit(afx);
            afe.Show();
        }

        float PatternPitch = 0;
        
        public void StopAudio()
        {
            foreach (IAudioFX af in afx) af.ClearInstance();
            Patterns.Clear();
            Samples.Clear();
            ToRemove.Clear();
            QuickSamples.Clear();
            QuickPatterns.Clear();
        }
        public void StopVideo()
        {
            if (VSample != null)
            {
                VSample.Dispose();
                VSample = null;
            }
            VSample0 = null;
            LastNoteIndex = -1;
            LastMediaIndex = uint.MaxValue;
            pr2?.StopAll();
            pr2 = null;
        }

        public void ProccessMedia(TimelineMedia tlm, uint mediaIndex, long timeBase, float timeBeat)
        {
            if (tlm.Property is TLPattern)
            {
                if (AudioPrerenderManager.CanBeInDatabase(new PatternDesriptor(tlm)))
                {
                    if (!QuickPatterns.ContainsKey(mediaIndex))
                    {
                        AudioPrerenderManager.PrerenderSample(new PatternDesriptor(tlm));
                        QuickPatterns.Add(mediaIndex, new Tuple<PatternDesriptor, int, float, int>(new PatternDesriptor(tlm), (int)tlm.TimeFrom, (float)Math.Pow(10, tlm.Volume/10), (int)tlm.TimeEnd));
                    }
                }
                else if (!Patterns.ContainsKey(mediaIndex))
                {
                    TLPattern tlp = (tlm.Property as TLPattern);
                    PitchReader prc = new PitchReader(tlp.Samples);
                    PatternReader2 pr2 = new PatternReader2(tlm.SecondsIn, tlm.SecondsIn + tlm.TimeLenght, (tlm.TimeFrom + tlm.BeatIn) / (float)timeBase, Form1.Project.ProjectPatterns[tlp.Pattern], prc);
                    prc.pitchOffset = tlm.Pitch;
                    Patterns.Add(mediaIndex, new Tuple<PatternReader2, float, PitchReader>(pr2, tlm.TimeEnd / (float)timeBase, prc));
                }
            }
            else if (tlm.Property is TLSample)
            {
                if (AudioPrerenderManager.CanBeInDatabase(new SampleDesriptor(tlm)))
                {
                    if (!QuickSamples.ContainsKey(mediaIndex))
                    {
                        AudioPrerenderManager.PrerenderSample(new SampleDesriptor(tlm));
                        QuickSamples.Add(mediaIndex, new Tuple<SampleDesriptor, int, float, int>(new SampleDesriptor(tlm), (int)tlm.TimeFrom, (float)Math.Pow(10, tlm.Volume / 10), (int)tlm.TimeEnd));
                    }
                }
                else if (!Samples.ContainsKey(mediaIndex))
                {
                    //TLSample tls = (tlm.Property as TLSample);
                    if (Form1.Project.ProjectSamples.ContainsKey((tlm.Property as TLSample).Sample))
                    {
                        IPitchReader ipr = Form1.Project.ProjectSamples[(tlm.Property as TLSample).Sample].ips?.GetReader(tlm.SecondsIn - ((tlm.TimeFrom / (float)timeBase) - timeBeat) * Form1.Project.GetBeatDurationSec(0));
                        if (ipr != null)
                        {
                            PitchReaderSingle prs = new PitchReaderSingle(ipr);
                            prs.PTC = tlm.Pitch + Form1.Project.GetMasterPitch(tlm.TimeFrom / (float)timeBase);
                            prs.PAN = tlm.Pan;
                            prs.FMT = tlm.Formant;
                            prs.SPD = tlm.Stretch;
                            prs.VOL = (float)Math.Pow(10, tlm.Volume / 10);

                            Samples.Add(mediaIndex, new Tuple<float, PitchReaderSingle>(tlm.TimeEnd / (float)timeBase, prs));
                        }
                    }
                }
            }
        }
        public void ProccessMedia0(TimelineMedia tlm, uint mediaIndex, long timeBase, float timeBeat)
        {
            if (tlm.Property is TLPattern)
            {
                if (!Patterns.ContainsKey(mediaIndex))
                {
                    TLPattern tlp = (tlm.Property as TLPattern);
                    PitchReader prc = new PitchReader(tlp.Samples);
                    PatternReader2 pr2 = new PatternReader2(tlm.SecondsIn, tlm.SecondsIn + tlm.TimeLenght, (tlm.TimeFrom + tlm.BeatIn) / (float)timeBase, Form1.Project.ProjectPatterns[tlp.Pattern], prc);
                    prc.pitchOffset = tlm.Pitch;
                    Patterns.Add(mediaIndex, new Tuple<PatternReader2, float, PitchReader>(pr2, tlm.TimeEnd / (float)timeBase, prc));
                }
            }
            else if (tlm.Property is TLSample)
            {
                if (!Samples.ContainsKey(mediaIndex))
                {
                    //TLSample tls = (tlm.Property as TLSample);
                    if (Form1.Project.ProjectSamples.ContainsKey((tlm.Property as TLSample).Sample))
                    {
                        IPitchReader ipr = Form1.Project.ProjectSamples[(tlm.Property as TLSample).Sample].ips?.GetReader(tlm.SecondsIn - ((tlm.TimeFrom / (float)timeBase) - timeBeat) * Form1.Project.GetBeatDurationSec(0));
                        if (ipr != null)
                        {
                            PitchReaderSingle prs = new PitchReaderSingle(ipr);
                            prs.PTC = tlm.Pitch + Form1.Project.GetMasterPitch(tlm.TimeFrom / (float)timeBase);
                            prs.PAN = tlm.Pan;
                            prs.FMT = tlm.Formant;
                            prs.SPD = tlm.Stretch;
                            prs.VOL = (float)Math.Pow(10, tlm.Volume / 10);

                            Samples.Add(mediaIndex, new Tuple<float, PitchReaderSingle>(tlm.TimeEnd / (float)timeBase, prs));
                        }
                    }
                }
            }
        }
        public void OutputAddAudio(ref float[] buffer, float timeBeat0, float timeBeat1)
        {
            ToRemove.Clear();
            try
            {
                foreach (KeyValuePair<uint, Tuple<PatternReader2, float, PitchReader>> k in Patterns)
                {
                    k.Value.Item1.GetValue(timeBeat0, timeBeat1);
                    k.Value.Item3.ReadAdd(ref buffer, timeBeat0, timeBeat1);

                    if (k.Value.Item2 < timeBeat0 && k.Value.Item3.ReleaseEnded)
                    {
                        ToRemove.Add(k.Key);
                    }
                }
            }
            catch { }
            foreach (uint u in ToRemove) Patterns.Remove(u);
            ToRemove.Clear();
            try
            {
                foreach (KeyValuePair<uint, Tuple<float, PitchReaderSingle>> k in Samples)
                {
                    k.Value.Item2.ReadAdd(ref buffer, timeBeat0, timeBeat1);
                    if (k.Value.Item1 < timeBeat0)
                    {
                        k.Value.Item2.NoteOff();
                    }
                    if (k.Value.Item1 < timeBeat0 && k.Value.Item2.ReleaseEnded)
                    {
                        ToRemove.Add(k.Key);
                    }
                }
                foreach (KeyValuePair<uint, Tuple<SampleDesriptor, int, float, int>> k in QuickSamples)
                {
                    int timeIn = (int)((timeBeat0 - k.Value.Item2 / 192f) * Form1.Project.GetBeatDurationSamp(0));
                    if (AudioPrerenderManager.WriteAddArray(ref buffer, k.Value.Item1, timeIn * 2, k.Value.Item3, k.Value.Item3) && timeBeat0 > k.Value.Item4 / 192f) ToRemove.Add(k.Key);
                }
                foreach (KeyValuePair<uint, Tuple<PatternDesriptor, int, float, int>> k in QuickPatterns)
                {
                    int timeIn = (int)((timeBeat0 - k.Value.Item2 / 192f) * Form1.Project.GetBeatDurationSamp(0));
                    if (AudioPrerenderManager.WriteAddArray(ref buffer, k.Value.Item1, timeIn, k.Value.Item3, k.Value.Item3) && timeBeat0 > k.Value.Item4 / 192f) ToRemove.Add(k.Key);
                }
            }
            catch { }
            foreach (uint u in ToRemove)
            {
                if (Samples.ContainsKey(u)) Samples.Remove(u);
                if (QuickSamples.ContainsKey(u)) QuickSamples.Remove(u);
                if (QuickPatterns.ContainsKey(u)) QuickPatterns.Remove(u);
            }
        }

        public void ProccessMediaV(TimelineMedia tlm, uint mediaIndex, long timeBase, float timeBeat)
        {
            if (tlm.Flags0.HasFlag(MediaFlags.VideoEnabled))
            {
                if (tlm.Property is TLSample && mediaIndex != LastMediaIndex)
                {
                    if (VSample != null) VSample.Dispose();
                    TLSample tls = tlm.Property as TLSample;
                    if (Form1.Project.ProjectSamples[tls.Sample].ivs != null)
                    {
                        pr2 = null;
                        if (Form1.Project.ProjectSamples[tls.Sample].ivs.RequiresReader)
                        {
                            if (TextureVideo == 0) TextureVideo = GLDraw.GetTextureName();

                            VSample0 = null;
                            float tin = 0;
                            if (Form1.Project.ProjectSamples[tls.Sample].ivs.BeatSync) tin = tlm.BeatIn;
                            else tin = tlm.SecondsIn - ((tlm.TimeFrom / (float)timeBase) - timeBeat) * Form1.Project.GetBeatDurationSec(0);
                            VSample = Form1.Project.ProjectSamples[tls.Sample].ivs.GetReader(tin, TextureVideo);
                            VSampleOff = tlm.SecondsIn;


                            LastMediaIndex = mediaIndex;
                        }
                        else
                        {
                            VSample0 = Form1.Project.ProjectSamples[tls.Sample].ivs;
                            VSample = null;
                            LastMediaIndex = uint.MaxValue;
                        }
                        VSampleStart = tlm.TimeFrom / (float)timeBase;
                        VSampleEnd = tlm.TimeEnd / (float)timeBase;
                        VSampleFadeIn = tlm.OpacityIn / (float)timeBase;
                        VSampleFadeOut = tlm.OpacityOut / (float)timeBase;
                        vidVs = long.MaxValue;
                        TLSampleVFX = tls.VFX;
                    }
                }
                if (tlm.Property is TLPattern)
                {
                    TLPattern tlp = tlm.Property as TLPattern;
                    if (VSample != null) VSample.Dispose();
                    PatternStart = (int)tlm.TimeFrom;
                    PatternEnd = (int)tlm.TimeEnd;
                    VSample0 = null;
                    VSample = null;
                    vidVs = 0;
                    StopPatternVideo = !tlp.KeepVideo;

                    if (Form1.Project.ProjectPatterns.ContainsKey(tlp.Pattern))
                    {
                        pr2 = new PatternReader2(tlm.BeatIn / (float)timeBase, (tlm.BeatIn + tlm.TimeLenght) / (float)timeBase, -(tlm.BeatIn / (float)timeBase), Form1.Project.ProjectPatterns[tlp.Pattern], this);
                        PTSamp.Clear();
                        PTSamp.AddRange(tlp.Samples);
                        PatTB = Form1.Project.ProjectPatterns[tlp.Pattern].ActivationTrack.TimeBase;
                        VPatOff = tlm.TimeEnd / (float)timeBase;
                    }
                    else pr2 = null;
                }
            }
        }
        float tbp = 0;
        bool readprev = false;
        public void Render(RenderTarget rt, Matrix4 ParentTransform, float timeBeat)
        {
            if (Form1.Project != null)
            {
                if (timeBeat > VPatOff) pr2 = null;
                if (pr2 != null)
                {
                    pr2.GetValue(tbp - PatternStart / 192f, timeBeat - PatternStart / 192f);
                    readprev = true;
                }
                else
                {
                    if (readprev) VSample0 = null;
                    readprev = false;
                }

                tbp = timeBeat;

                Matrix4 m4 = ParentTransform;

                TextureInfo ti = new TextureInfo(0, 1, 1);
                float timein = 0;
                if (BeatSync) timein = VSampleOff + (timeBeat - VSampleStart) * VSampleSpeed;
                else timein = VSampleOff + Form1.Project.GetBeatDurationSec(timeBeat) * (timeBeat - VSampleStart) * VSampleSpeed;
                if (timeBeat < VSampleEnd)
                {
                    if (VSample0 != null) ti = VSample0.GetFrame(timein);
                    else if (VSample != null) VSample.Read(ti, timein);

                    if (pr2 != null)
                    {
                        if (vidVs == 1) ti.FlipX();
                        if (vidVs == 2) { ti.FlipX(); ti.FlipY(); }
                        if (vidVs == 3) { ti.FlipY(); }
                    }
                }

                if (vidVs == long.MaxValue)
                {
                    foreach (IVideoFX fx in TLSampleVFX)
                    {
                        fx.UpdateValues(timeBeat - VSampleStart);
                        fx.Apply(ref ti);
                    }
                }

                ti.AppendTransformColor(new Matrix4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,VSampleTransparency), Vector4.Zero);

                foreach (IVideoFX fx in vfx)
                {
                    fx.UpdateValues(timeBeat);
                    fx.Apply(ref ti);
                }

                rt.Use();

                if (ti.TextureIndex != 0)
                {
                    GL.Viewport(0, 0, rt.Width, rt.Height);

                    // 2d alpha
                    if (BlendMode == 0)
                    {
                        GL.Enable(EnableCap.Blend);
                        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                        GL.BlendEquationSeparate(BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd);
                        GL.Disable(EnableCap.DepthTest);
                    }
                    // 3d write
                    if (BlendMode == 1)
                    {
                        GL.Disable(EnableCap.Blend);
                        GL.Enable(EnableCap.DepthTest);
                    }
                    // mult
                    if (BlendMode == 2)
                    {
                        GL.Enable(EnableCap.DepthTest);
                        GL.Enable(EnableCap.Blend);
                        GL.BlendFunc(BlendingFactor.DstColor, BlendingFactor.Zero);
                        GL.BlendEquationSeparate(BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd);
                    }
                    // add
                    if (BlendMode == 3)
                    {
                        GL.Enable(EnableCap.DepthTest);
                        GL.Enable(EnableCap.Blend);
                        GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);
                        GL.BlendEquationSeparate(BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd);
                    }
                    // subtract
                    if (BlendMode == 4)
                    {
                        GL.Enable(EnableCap.DepthTest);
                        GL.Enable(EnableCap.Blend);
                        GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);
                        GL.BlendEquationSeparate(BlendEquationMode.FuncSubtract, BlendEquationMode.FuncAdd);
                    }
                    GLDraw.DrawVideoDefault(ti, rt, m4);
                }
            }
        }

        public void NoteOn(Note n, long index)
        {
            if (VSample != null) VSample.Dispose();
            VSample0 = null;
            VSample = null;
            LastNoteIndex = index;
            if (n.Sample < PTSamp.Count && n.Sample >=0)
            {
                if (Form1.Project.ProjectSamples.ContainsKey(PTSamp[(int)n.Sample]))
                {
                    if (Form1.Project.ProjectSamples[PTSamp[(int)n.Sample]].ivs != null)
                    {
                        if (Form1.Project.ProjectSamples[PTSamp[(int)n.Sample]].ivs.RequiresReader)
                        {
                            VSample0 = null;
                            if (TextureVideo == 0) GLDraw.GetTextureName();
                            VSample = Form1.Project.ProjectSamples[PTSamp[(int)n.Sample]].ivs.GetReader(0, TextureVideo);
                        }
                        else
                        {
                            if (VSample != null) VSample.Dispose();
                            VSample = null;
                            VSample0 = Form1.Project.ProjectSamples[PTSamp[(int)n.Sample]].ivs;
                        }

                        VSampleStart = (n.BeatNoteOn + PatternStart) / PatTB;
                        VSampleEnd = (n.BeatNoteOff + PatternStart) / PatTB;
                        VSampleFadeIn = 0;
                        VSampleFadeOut = 0;
                        VSampleTransparency = n.opacity;
                    }
                }
                vidVs = n.VisualStyle;
            }
        }
        public void NoteOff(long index)
        {
            if (index == LastNoteIndex)
            {
                if (StopPatternVideo)
                {
                    if (VSample != null) VSample.Dispose();
                    VSample0 = null;
                    VSample = null;
                }
            }
        }
        public void SetProperty(byte index, float value)
        {
            if (index == 48) PatProp48 = value;
            if (index == 49) PatProp49 = value;
            if (index == 50) PatProp50 = value;
            if (index == 51) PatProp51 = value;
        }

        public object Clone()
        {
            VideoTrack dvt = new VideoTrack();

            dvt.afx = new List<IAudioFX>();
            foreach (IAudioFX afx in afx) dvt.afx.Add((IAudioFX)afx.Clone());

            dvt.vfx = new List<IVideoFX>();
            foreach (IVideoFX vfx in vfx) dvt.vfx.Add((IVideoFX)vfx.Clone());

            dvt.Media = new List<uint>();
            foreach (uint v in Media)
            {
                TimelineMedia tlm = Form1.Project.ProjectMedia[v];
                TimelineMedia tlm2 = (TimelineMedia)tlm.Clone();
                uint u = Form1.Project.AddMedia(tlm2);
                dvt.Media.Add(u);
            }

            return dvt;
        }
        public void PasteSettings(VideoTrack vt)
        {
            afx.Clear();         
            foreach (IAudioFX vtafx in vt.afx) afx.Add((IAudioFX)vtafx.Clone());

            vfx.Clear();
            foreach (IVideoFX vtvfx in vt.vfx) vfx.Add((IVideoFX)vtvfx.Clone());
        }

        List<uint> ToRemove = new List<uint>();
        Dictionary<uint, Tuple<PatternReader2, float, PitchReader>> Patterns = new Dictionary<uint, Tuple<PatternReader2, float, PitchReader>>();
        Dictionary<uint, Tuple<float, PitchReaderSingle>> Samples = new Dictionary<uint, Tuple<float, PitchReaderSingle>>();

        Dictionary<uint, Tuple<SampleDesriptor, int, float, int>> QuickSamples = new Dictionary<uint, Tuple<SampleDesriptor, int, float, int>>();
        Dictionary<uint, Tuple<PatternDesriptor, int, float, int>> QuickPatterns = new Dictionary<uint, Tuple<PatternDesriptor, int, float, int>>();

        List<uint> PTSamp = new List<uint>();
        long vidVs = 0;
        uint LastMediaIndex = uint.MaxValue;
        long LastNoteIndex = -1;
        PatternReader2 pr2 = null;
        IVideoSample VSample0 = null;
        IVideoReader VSample = null;
        float PatProp48 = 1;
        float PatProp49 = 0;
        float PatProp50 = 1;
        float PatProp51 = 0;
        bool BeatSync = false;
        float VSampleSpeed = 1;
        float VSampleOff = 0;
        float VSampleStart = 0;
        float VSampleEnd = 0;
        float VSampleFadeIn = 0;
        float VSampleFadeOut = 0;
        float VSampleTransparency = 1;
        private List<IVideoFX> TLSampleVFX = new List<IVideoFX>();

        float VPatOff = 0;
        float PatTB = 1;
        int PatternStart = 0;
        int PatternEnd = 0;
        //Dictionary<uint, Tuple<PatternReader2, float, PitchReader>> Patterns = new Dictionary<uint, Tuple<PatternReader2, float, PitchReader>>();

        public void Save(FileStream fs)
        {
            StreamHelper.SaveBytes(fs,(int)10);

            fs.WriteByte(RenderType);
            fs.WriteByte(ChildIndex);
            StreamHelper.SaveBytes(fs, ShowAutomation);

            StreamHelper.SaveBytes(fs, vfx.Count);
            foreach (IVideoFX fx in vfx) EffectHelper.Save(fs,fx);

            StreamHelper.SaveBytes(fs, afx.Count);
            foreach (IAudioFX fx in afx) EffectHelper.Save(fs, fx);

            StreamHelper.SaveBytes(fs, Media.Count);
            foreach (uint m in Media) StreamHelper.SaveBytes(fs, m);

            StreamHelper.SaveBytes(fs, Volume);
            StreamHelper.SaveBytes(fs, Pan);
            fs.WriteByte(RenderShape);
            fs.WriteByte(BlendMode);
        }
        public void Load(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);

            if (lenght > 0) { RenderType = (byte)fs.ReadByte(); lenght--; }
            if (lenght > 0) { ChildIndex = (byte)fs.ReadByte(); lenght--; }
            if (lenght > 0) { ShowAutomation = StreamHelper.LoadBool(fs); lenght--; }
            if (lenght > 0)
            {
                int l0 = StreamHelper.LoadInt(fs);
                for (int i = 0; i < l0; i++) vfx.Add(EffectHelper.LoadVFX(fs));
                lenght--;
            }
            if (lenght > 0)
            {
                int l0 = StreamHelper.LoadInt(fs);
                for (int i = 0; i < l0; i++) afx.Add(EffectHelper.LoadAFX(fs));
                lenght--;
            }
            if (lenght > 0)
            {
                int l0 = StreamHelper.LoadInt(fs);
                for (int i = 0; i < l0; i++) Media.Add(StreamHelper.LoadUInt(fs));
                lenght--;
            }
            if (lenght > 0) { Volume = StreamHelper.LoadFloat(fs); lenght--; }
            if (lenght > 0) { Pan = StreamHelper.LoadFloat(fs); lenght--; }
            if (lenght > 0) { RenderShape = (byte)fs.ReadByte(); lenght--; }
            if (lenght > 0) { BlendMode = (byte)fs.ReadByte(); lenght--; }
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }
        public void Init()
        {
            foreach (IVideoFX vfx in vfx) vfx.Init();
            foreach (IAudioFX afx in afx)
            {
                afx.Init();
                afx.ClearInstance();
            }
        }

        // TROLOLOL
        public uint Index = 0;

        public VideoTrack()
        {
            Buffer = new float[0];
        }

        public void Read(float timeBeat)
        {
            foreach (uint u in Media) if (Form1.Project.ProjectMedia[u].TimeFrom / 192f <= timeBeat && Form1.Project.ProjectMedia[u].TimeEnd / 192f > timeBeat) ProccessMedia(Form1.Project.ProjectMedia[u], u, 192, timeBeat);
        }
        public void ReadV(float timeBeat)
        {
            foreach (uint u in Media)
            {
                if (Form1.Project.ProjectMedia[u].TimeFrom / 192f <= timeBeat && timeBeat < Form1.Project.ProjectMedia[u].TimeEnd / 192f) ProccessMediaV(Form1.Project.ProjectMedia[u], u, 192, timeBeat);
            }
        }

        public void GetAudio(float timeBeat0, float timeBeat1)
        {
            OutputAddAudio(ref Buffer, timeBeat0, timeBeat1);
        }

        public float[] Buffer = new float[0];
    }
    public class NumberTrack : INoteReciever2
    {
        public List<uint> Media = new List<uint>();
        public float NumberOutput = 0;
        //PatternReader2 pr2 = null;
        public void ProccessMedia(TimelineMedia tlm, uint mediaIndex, long timeBase, float timeBeat)
        {
            NumberOutput = tlm.Property.GetNumber(tlm, (ulong)timeBase, timeBeat * timeBase);

            /*if (tlm.Property is TLPattern)
            {
                if (tlm.Property is TLPattern)
                {
                    TLPattern tlp = tlm.Property as TLPattern;

                    if (Form1.Project.ProjectPatterns.ContainsKey(tlp.Pattern))
                    {
                        pr2 = new PatternReader2(tlm.BeatIn / (float)timeBase, (tlm.BeatIn + tlm.TimeLenght) / (float)timeBase, tlm.BeatIn / (float)timeBase, Form1.Project.ProjectPatterns[tlp.Pattern], this);
                        PatternStart = (int)tlm.TimeFrom;
                        PatternEnd = (int)tlm.TimeEnd;
                    }
                    else pr2 = null;
                }
            }*/
        }
        public void Read(float timeBeat)
        {
            foreach (uint u in Media) if (Form1.Project.ProjectMedia[u].TimeFrom / 192f <= timeBeat && Form1.Project.ProjectMedia[u].TimeEnd / 192f > timeBeat ) ProccessMedia(Form1.Project.ProjectMedia[u], u, 192, timeBeat);

           // if (timeBeat > PatternEnd / 192f) pr2 = null;
            //if (pr2 != null) pr2.GetValue(tbp, timeBeat - PatternStart / 192f);
           // tbp = timeBeat;
        }
        //float tbp = 0;
        //float PatternEnd = 0;
        //float PatternStart = 0;

        public object Clone()
        {
            NumberTrack dnt = new NumberTrack();
            foreach (uint u in Media)
            {
                uint u2 = Form1.Project.AddMedia((TimelineMedia)Form1.Project.ProjectMedia[u].Clone());
                dnt.Media.Add(u2);
            }
            return dnt;
        }
        public void Save(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)1);

            StreamHelper.SaveBytes(fs, Media.Count);
            for (int i = 0; i < Media.Count; i++) StreamHelper.SaveBytes(fs, Media[i]);
        }
        public void Load(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0)
            {
                int l0 = StreamHelper.LoadInt(fs);
                for (int i = 0; i < l0; i++) Media.Add(StreamHelper.LoadUInt(fs));
                lenght--;
            }

            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public int HitCount = 0;
        public void NoteOn(Note n, long index)
        {
            HitCount++;
        }
        public void NoteOff(long index)
        {
        }
        public void SetProperty(byte index, float value)
        {
        }
    }

    public class SparseList<T>
    {
        private Dictionary<int, T> dictionary = new Dictionary<int, T>();

        public void Set(int i, T t)
        {
            if (dictionary.ContainsKey(i)) dictionary[i] = t;
            else dictionary.Add(i, t);
        }
        public void Unset(int i)
        {
            if (dictionary.ContainsKey(i)) dictionary.Remove(i);
        }
        public T GetValue(int i, T defaultValue)
        {
            if (dictionary.ContainsKey(i)) return dictionary[i];
            else return defaultValue;
        }
        public void Clear()
        {
            dictionary.Clear();
        }
    }
    public struct TrackVideoPrerender
    {
        public SparseList<TextureInfo> PrerenderedFrames;

        public void RerenderCompletely(VideoTrack t)
        {
            PrerenderedFrames.Clear();
            foreach(uint n in t.Media)
            {

            }
        }
    }
    public struct SampleDesriptor
    {

        public bool complex;
        public int sample;
        public float pitch;
        public float speed;
        public float pan;
        public int posin;
        public int lenght;

        public SampleDesriptor(bool complex, int sample, float pitch, float speed, float pan, int posin, int lenght)
        {
            this.complex = complex;
            this.sample = sample;
            this.pitch = pitch;
            this.speed = speed;
            this.pan = pan;
            this.posin = posin;
            this.lenght = lenght;
        }
        public SampleDesriptor(TimelineMedia tlm)
        {
            complex = false;
            sample = (int)(tlm.Property as TLSample).Sample;
            pitch = Form1.Project.MasterPitch + tlm.Pitch;
            speed = tlm.Stretch;
            pan = tlm.Pan;
            posin = (int)(tlm.SecondsIn * 48000f);
            lenght = (int)(tlm.TimeLenght * Form1.Project.GetBeatDurationSamp(0) / 192f);
        }

        public bool differentInLenght(SampleDesriptor sda)
        {
            if (sda.complex || complex) return false;
            if (sda.sample == sample && sda.pitch == pitch && sda.speed == speed && sda.pan == pan) return true;
            return false;
        }
        public static bool operator ==(SampleDesriptor sda, SampleDesriptor sdb)
        {
            if (sda.complex || sdb.complex) return false;
            if (sda.sample == sdb.sample && sda.pitch == sdb.pitch && sda.speed == sdb.speed && sda.pan == sdb.pan && sda.posin == sdb.posin && sda.lenght == sdb.lenght) return true;
            return false;
        }
        public static bool operator !=(SampleDesriptor sda, SampleDesriptor sdb)
        {
            return !(sda == sdb);
        }
        public override bool Equals(object obj)
        {
            if (!(obj is SampleDesriptor))
            {
                return false;
            }

            return this == (SampleDesriptor)obj;
        }
        public override int GetHashCode()
        {
            var hashCode = -1660843661;
            hashCode = hashCode * -1521134295 + complex.GetHashCode();
            hashCode = hashCode * -1521134295 + sample.GetHashCode();
            hashCode = hashCode * -1521134295 + pitch.GetHashCode();
            hashCode = hashCode * -1521134295 + speed.GetHashCode();
            hashCode = hashCode * -1521134295 + pan.GetHashCode();
            hashCode = hashCode * -1521134295 + lenght.GetHashCode();
            return hashCode;
        }
    }
    public struct PatternDesriptor
    {
        public bool complex;
        public int pattern;
        public List<uint> samples;
        public float bpm;
        public float pitch;
        public float pan;
        public int beatin;
        public int lenght;

        public PatternDesriptor(bool complex, int pattern, List<uint> samples, float bpm, float pitch, float pan, int beatin, int lenght)
        {
            this.complex = complex;
            this.pattern = pattern;
            this.samples = new List<uint>(samples);
            this.bpm = bpm;
            this.pitch = pitch;
            this.pan = pan;
            this.beatin = beatin;
            this.lenght = lenght;
        }
        public PatternDesriptor(TimelineMedia tlm)
        {
            complex = false;
            pattern = (int)(tlm.Property as TLPattern).Pattern;
            samples = new List<uint>((tlm.Property as TLPattern).Samples);
            bpm = Form1.Project.BPM;
            pitch = Form1.Project.MasterPitch + tlm.Pitch;
            pan = tlm.Pan;
            beatin = (int)(tlm.BeatIn);
            lenght = (int)(tlm.TimeLenght * Form1.Project.GetBeatDurationSamp(0) / 192f);
        }

        public bool differentInLenght(PatternDesriptor pda)
        {
            if (pda.complex || complex) return false;
            if (pda.pan == pan && pda.pattern == pattern && pda.pitch == pitch && pda.bpm == bpm)
            {
                if (pda.samples.Count == samples.Count)
                {
                    for (int i = 0; i < pda.samples.Count; i++)
                    {
                        if (pda.samples[i] != samples[i]) return false;
                    }
                    return true;
                }
            }
            return false;
        }
        public static bool operator ==(PatternDesriptor pda, PatternDesriptor pdb)
        {
            if (pda.complex || pdb.complex) return false;
            if (pda.pan == pdb.pan && pda.pattern == pdb.pattern && pda.pitch == pdb.pitch && pda.bpm == pdb.bpm && pda.beatin == pdb.beatin && pda.lenght == pdb.lenght)
            {
                if (pda.samples.Count == pdb.samples.Count)
                {
                    for (int i = 0; i < pda.samples.Count; i++)
                    {
                        if (pda.samples[i] != pdb.samples[i]) return false;
                    }
                    return true;
                }
            }
            return false;
        }
        public static bool operator !=(PatternDesriptor pda, PatternDesriptor pdb)
        {
            return !(pda == pdb);
        }
        public override bool Equals(object obj)
        {
            if (!(obj is PatternDesriptor))
            {
                return false;
            }
            return this == (PatternDesriptor)obj;
        }
        public override int GetHashCode()
        {
            var hashCode = -1375788322;
            hashCode = hashCode * -1521134295 + complex.GetHashCode();
            hashCode = hashCode * -1521134295 + pattern.GetHashCode();
            hashCode = hashCode * -1521134295 + bpm.GetHashCode();
            hashCode = hashCode * -1521134295 + pitch.GetHashCode();
            hashCode = hashCode * -1521134295 + pan.GetHashCode();
            hashCode = hashCode * -1521134295 + beatin.GetHashCode();
            hashCode = hashCode * -1521134295 + lenght.GetHashCode();
            return hashCode;
        }
    }
    public static class AudioPrerenderManager
    {
        static Dictionary<SampleDesriptor, float[]> PrerenderedSamples = new Dictionary<SampleDesriptor, float[]>();
        static Dictionary<PatternDesriptor, float[]> PrerenderedPatterns = new Dictionary<PatternDesriptor, float[]>();
        static Dictionary<SampleDesriptor, float[]> PrerenderedSamplesP = new Dictionary<SampleDesriptor, float[]>();
        static Dictionary<PatternDesriptor, float[]> PrerenderedPatternsP = new Dictionary<PatternDesriptor, float[]>();

        static public void RerenderPattern(int ptNum)
        {
            List<PatternDesriptor> toRerender = new List<PatternDesriptor>();
            foreach (PatternDesriptor pd in PrerenderedPatterns.Keys)
            {
                if (pd.pattern == ptNum)
                {
                    toRerender.Add(pd);
                }
            }
            foreach (PatternDesriptor pd in toRerender)
            {
                PrerenderedPatterns.Remove(pd);
                PrerenderedPatternsP.Remove(pd);
                PrerenderSample(pd);
            }
        }
        static public void Clear()
        {
            PrerenderedSamples.Clear();
            PrerenderedPatterns.Clear();
            PrerenderedSamplesP.Clear();
            PrerenderedPatternsP.Clear();
        }
        static public void CreateAllRequired()
        {
            foreach (uint u in Form1.Project.TrackList)
            {
                foreach (uint media in Form1.Project.Tracks[u].Media)
                {
                    TimelineMedia tlm = Form1.Project.ProjectMedia[media];
                    if (tlm.Property is TLSample)
                    {
                        SampleDesriptor sd = new SampleDesriptor(tlm);
                        PrerenderSample(sd);
                    }
                    if (tlm.Property is TLPattern)
                    {
                        PatternDesriptor pd = new PatternDesriptor(tlm);
                        PrerenderSample(pd);
                    }
                }
            }
        }
        static public void ClearOutNotNeeded()
        {
            Dictionary<SampleDesriptor, float[]> PrerenderedSamples2 = new Dictionary<SampleDesriptor, float[]>();
            Dictionary<PatternDesriptor, float[]> PrerenderedPatterns2 = new Dictionary<PatternDesriptor, float[]>();
            Dictionary<SampleDesriptor, float[]> PrerenderedSamplesP2 = new Dictionary<SampleDesriptor, float[]>();
            Dictionary<PatternDesriptor, float[]> PrerenderedPatternsP2 = new Dictionary<PatternDesriptor, float[]>();

            foreach (uint u in Form1.Project.TrackList)
            {
                foreach (uint media in Form1.Project.Tracks[u].Media)
                {
                    TimelineMedia tlm = Form1.Project.ProjectMedia[media];
                    if (tlm.Property is TLSample)
                    {
                        SampleDesriptor sd = new SampleDesriptor(tlm);

                        if (!PrerenderedSamples2.ContainsKey(sd) && PrerenderedSamples.ContainsKey(sd)) PrerenderedSamples2.Add(sd, PrerenderedSamples[sd]);
                        if (!PrerenderedSamplesP2.ContainsKey(sd)&& PrerenderedSamplesP.ContainsKey(sd)) PrerenderedSamplesP2.Add(sd, PrerenderedSamplesP[sd]);
                    }
                    if (tlm.Property is TLPattern)
                    {
                        PatternDesriptor pd = new PatternDesriptor(tlm);

                        if (!PrerenderedPatterns2.ContainsKey(pd) && PrerenderedPatterns.ContainsKey(pd)) PrerenderedPatterns2.Add(pd, PrerenderedPatterns[pd]);
                        if (!PrerenderedPatternsP2.ContainsKey(pd) && PrerenderedPatternsP.ContainsKey(pd)) PrerenderedPatternsP2.Add(pd, PrerenderedPatternsP[pd]);
                    }
                }
            }

            PrerenderedSamples.Clear();
            PrerenderedSamples = PrerenderedSamples2;

            PrerenderedSamplesP.Clear();
            PrerenderedSamplesP = PrerenderedSamplesP2;

            PrerenderedPatterns.Clear();
            PrerenderedPatterns = PrerenderedPatterns2;

            PrerenderedPatternsP.Clear();
            PrerenderedPatternsP = PrerenderedPatternsP2;
        }
        static public void ClearOutNotNeeded(List<uint> patterns)
        {
            Dictionary<SampleDesriptor, float[]> PrerenderedSamples2 = new Dictionary<SampleDesriptor, float[]>();
            Dictionary<PatternDesriptor, float[]> PrerenderedPatterns2 = new Dictionary<PatternDesriptor, float[]>();
            Dictionary<SampleDesriptor, float[]> PrerenderedSamplesP2 = new Dictionary<SampleDesriptor, float[]>();
            Dictionary<PatternDesriptor, float[]> PrerenderedPatternsP2 = new Dictionary<PatternDesriptor, float[]>();

            foreach (uint u in Form1.Project.TrackList)
            {
                foreach (uint media in Form1.Project.Tracks[u].Media)
                {
                    TimelineMedia tlm = Form1.Project.ProjectMedia[media];
                    if (tlm.Property is TLSample)
                    {
                        SampleDesriptor sd = new SampleDesriptor(tlm);

                        if (!PrerenderedSamples2.ContainsKey(sd)&& PrerenderedSamples.ContainsKey(sd)) PrerenderedSamples2.Add(sd, PrerenderedSamples[sd]);
                        if (!PrerenderedSamplesP2.ContainsKey(sd)&& PrerenderedSamplesP.ContainsKey(sd)) PrerenderedSamplesP2.Add(sd, PrerenderedSamplesP[sd]);
                    }
                    if (tlm.Property is TLPattern)
                    {
                        PatternDesriptor pd = new PatternDesriptor(tlm);
                        if (!patterns.Contains((uint)pd.pattern))
                        {
                            if (!PrerenderedPatterns2.ContainsKey(pd) && PrerenderedPatterns.ContainsKey(pd)) PrerenderedPatterns2.Add(pd, PrerenderedPatterns[pd]);
                            if (!PrerenderedPatternsP2.ContainsKey(pd) && PrerenderedPatternsP.ContainsKey(pd)) PrerenderedPatternsP2.Add(pd, PrerenderedPatternsP[pd]);
                        }
                    }
                }
            }

            PrerenderedSamples.Clear();
            PrerenderedSamples = PrerenderedSamples2;

            PrerenderedSamplesP.Clear();
            PrerenderedSamplesP = PrerenderedSamplesP2;

            PrerenderedPatterns.Clear();
            PrerenderedPatterns = PrerenderedPatterns2;

            PrerenderedPatternsP.Clear();
            PrerenderedPatternsP = PrerenderedPatternsP2;
        }

        static public bool CanBeInDatabase(SampleDesriptor sd)
        {
            if (!Form1.Project.ProjectSamples.ContainsKey((uint)sd.sample)) return true;
            else
            {
                IPitchSample ips = Form1.Project.ProjectSamples[(uint)sd.sample].ips;
                if (ips == null && sd.lenght < 120000) return true;
                if (sd.lenght < 120000) return true;
                else return false;
            }
        }
        static public bool CanBeInDatabase(PatternDesriptor pd)
        {
            return true;
        }
        static public void PrerenderSample(SampleDesriptor sd)
        {
            if (!PrerenderedSamples.ContainsKey(sd) && sd.lenght < 120000)
            {
                float[] prerender = new float[2 * sd.lenght];
                IPitchReader ipr = null;
                if (Form1.Project.ProjectSamples.ContainsKey((uint)sd.sample)) if (Form1.Project.ProjectSamples[(uint)sd.sample].ips != null) ipr = Form1.Project.ProjectSamples[(uint)sd.sample].ips.GetReader(sd.posin / 48000f);

                if (ipr != null)
                {
                    ipr.SetProperty(2, sd.speed);
                    ipr.SetProperty(3, sd.pitch);

                    PitchReaderSingle prs = new PitchReaderSingle(ipr);
                    prs.ReadAdd(ref prerender, 0, 1);
                    prs.NoteOff();
                    while (!prs.ReleaseEnded)
                    {
                        float[] smp2 = new float[1000];
                        prs.ReadAdd(ref smp2, 0, 1);
                        float[] prer2 = new float[prerender.Length + 1000];
                        for (int i = 0; i < prerender.Length; i++) prer2[i] = prerender[i];
                        for (int i = 0; i < 1000; i++) prer2[i + prerender.Length] = smp2[i];
                        prerender = prer2;
                    }
                    PrerenderedSamples.Add(sd, prerender);
                    float[] p2 = new float[prerender.Length / 4800 + 1];
                    for (int i = 0; i < p2.Length; i++)
                    {
                        float max = 0;
                        for (int j = 0; j < 4800; j++)
                        {
                            if (i * 4800 + j < prerender.Length)
                            {
                                if (prerender[i * 4800 + j] > max) max = prerender[i * 4800 + j];
                            }
                        }
                        p2[i] = max;
                    }
                    PrerenderedSamplesP.Add(sd, p2);
                }
                else
                {
                    PrerenderedSamples.Add(sd, new float[2]);
                    PrerenderedSamplesP.Add(sd, new float[2]);
                }
            }
        }
        static public void PrerenderSample(PatternDesriptor pd)
        {
            if (!PrerenderedPatterns.ContainsKey(pd))
            {
                float[] prerender = new float[2 * pd.lenght / 32 * 32 + 32];
                PitchReader pr = new PitchReader(pd.samples);

                if (Form1.Project.ProjectPatterns.ContainsKey((uint)pd.pattern))
                {
                    PatternReader2 pr2 = new PatternReader2(pd.beatin, pd.beatin + (float)Math.Round(192f * pd.lenght / Form1.Project.GetBeatDurationSamp(0)), -pd.beatin / 192f, Form1.Project.ProjectPatterns[(uint)pd.pattern], pr); //UPRAVIT
                    
                    pr.SetProperty(3, 0);
                    pr.pitchOffset = pd.pitch - Form1.Project.MasterPitch;

                    float[] smp1 = new float[2];
                    for (int i = 0; i < pd.lenght; i+=1)
                    {
                        Array.Clear(smp1,0,2);
                        pr2.GetValue(0 + i / Form1.Project.GetBeatDurationSamp(0), 0 + (i + 1) / Form1.Project.GetBeatDurationSamp(0));
                        pr.ReadAdd(ref smp1, 0 + i / Form1.Project.GetBeatDurationSamp(0), 0 + (i + 1) / Form1.Project.GetBeatDurationSamp(0));

                        for (int j = 0; j < 2; j++) prerender[i * 2 + j] = smp1[j];
                    }
                    while (!pr.ReleaseEnded)
                    {
                        pr2.GetValue(0 + (prerender.Length) / Form1.Project.GetBeatDurationSamp(0), 0 + (prerender.Length + 100) / Form1.Project.GetBeatDurationSamp(0));
                        float[] smp2 = new float[100];
                        pr.ReadAdd(ref smp2, 0 + (prerender.Length) / Form1.Project.GetBeatDurationSamp(0), 0 + (prerender.Length + 100) / Form1.Project.GetBeatDurationSamp(0));
                        float[] prer2 = new float[prerender.Length + 100];
                        for (int i = 0; i < prerender.Length; i++) prer2[i] = prerender[i];
                        for (int i = 0; i < 100; i++) prer2[i + prerender.Length] = smp2[i];
                        prerender = prer2;
                    }

                    if (!PrerenderedPatterns.ContainsKey(pd)) PrerenderedPatterns.Add(pd, prerender);
                    float[] p2 = new float[prerender.Length / 4800 + 1];
                    for (int i = 0; i < p2.Length; i++)
                    {
                        float max = 0;
                        for (int j = 0; j < 4800; j++)
                        {
                            if (i * 4800 + j < prerender.Length)
                            {
                                if (prerender[i * 4800 + j] > max) max = prerender[i * 4800 + j];
                            }
                        }
                        p2[i] = max;
                    }
                    if (!PrerenderedPatternsP.ContainsKey(pd)) PrerenderedPatternsP.Add(pd, p2);
                }
                else
                {
                    PrerenderedPatterns.Add(pd, new float[2]);
                    PrerenderedPatternsP.Add(pd, new float[2]);
                }
            }
        }

        static public bool WriteAddArray(ref float[] array, SampleDesriptor sd, int timeInArray, float R, float L)
        {
            if (PrerenderedSamples[sd].Length == 2) return false;

            bool end = false;
            for (int i = 0; i < array.Length; i+=2)
            {
                if (PrerenderedSamples[sd].Length > timeInArray + i && timeInArray + i >= 0)
                {
                    array[i] += PrerenderedSamples[sd][timeInArray + i] * L;
                    array[i + 1] += PrerenderedSamples[sd][timeInArray + i + 1] * R;
                }
                else if (PrerenderedSamples[sd].Length <= timeInArray + i)
                {
                    end = true;
                    break;
                }
            }
            return end;
        }
        static public bool WriteAddArray(ref float[] array, PatternDesriptor pd, int timeInArray, float R, float L)
        {
            bool end = false;
            for (int i = 0; i < array.Length; i += 2)
            {
                if (PrerenderedPatterns[pd].Length > timeInArray * 2 + i && timeInArray * 2 + i >= 0)
                {
                    array[i] += PrerenderedPatterns[pd][timeInArray * 2 + i] * L;
                    array[i + 1] += PrerenderedPatterns[pd][timeInArray * 2 + i + 1] * R;
                }
                else if (PrerenderedPatterns[pd].Length <= timeInArray * 2 + i)
                {
                    end = true;
                    break;
                }
            }
            return end;
        }

        public static void DrawWaveForm(SampleDesriptor sd, int x, int y, int h, int w, float s, float l, Color b, Color f, Graphics g)
        {
            if (CanBeInDatabase(sd))
            {
                if (!PrerenderedSamplesP.ContainsKey(sd))
                {
                    foreach (SampleDesriptor sd2 in PrerenderedSamplesP.Keys)
                    {
                        if (sd2.differentInLenght(sd))
                        {
                            if (sd2.lenght > sd.lenght && sd2.posin == sd.posin)
                            {
                                sd = sd2;
                            }
                        }
                    }
                }
                if (!PrerenderedSamplesP.ContainsKey(sd)) return;
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
                        if (i >= 0 && i < PrerenderedSamplesP[sd].Length)
                        {
                            float min = -PrerenderedSamplesP[sd][(int)i] / 2f + 0.5f;
                            float max = PrerenderedSamplesP[sd][(int)i] / 2f + 0.5f;

                            if (min < -1) min = -1;
                            if (min > 1) min = 1;
                            if (max < -1) max = -1;
                            if (max > 1) max = 1;


                            min = min / 2 + 0.5f;
                            max = max / 2 + 0.5f;
                            g.DrawLine(new System.Drawing.Pen(f), R.X + pX, R.Y + min * R.Height, R.X + pX, R.Y + max * R.Height);
                        }
                        pX++;
                    }
            }
        }
        public static void DrawWaveForm(PatternDesriptor sd, int x, int y, int h, int w, float s, float l, Color b, Color f, Graphics g)
        {
            if (CanBeInDatabase(sd))
            {
                if (!PrerenderedPatternsP.ContainsKey(sd))
                {
                    foreach (PatternDesriptor sd2 in PrerenderedPatternsP.Keys)
                    {
                        if (sd2.differentInLenght(sd))
                        {
                            if (sd2.lenght > sd.lenght && sd2.beatin == sd.beatin)
                            {
                                sd = sd2;
                            }
                        }
                    }
                }
                if (!PrerenderedPatternsP.ContainsKey(sd)) return;

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
                        if (i >= 0 && i < PrerenderedPatternsP[sd].Length)
                        {
                            float min = -PrerenderedPatternsP[sd][(int)i] / 2f + 0.5f;
                            float max = PrerenderedPatternsP[sd][(int)i] / 2f + 0.5f;

                            if (min < -1) min = -1;
                            if (max < -1) max = -1;
                            if (max > 1) max = 1;
                            if (min > 1) min = 1;


                            min = min / 2 + 0.5f;
                            max = max / 2 + 0.5f;
                            g.DrawLine(new System.Drawing.Pen(f), R.X + pX, R.Y + min * R.Height, R.X + pX, R.Y + max * R.Height);
                        }
                        pX++;
                    }
            }
        }
    }
}
