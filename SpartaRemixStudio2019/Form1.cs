using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using FFmpeg.AutoGen;
using System.IO;
using System.Runtime.InteropServices;
using FFMPEG;
using NAudio;
using NAudio.Wave;
using System.Diagnostics;
using System.Threading;

namespace SpartaRemixStudio2019
{
    public partial class Form1 : Form
    {
        private OpenTK.GLControl glControl;
        public static Project Project;
        RenderTarget rt = null;
        public static string projectName;
        public static bool changeAR;
        public static bool newProject;
        public static int changeWidth;
        public static int changeHeight;

        public static List<uint> patternsEdited = new List<uint>();


        public class SoundOut : ISampleProvider, IDisposable
        {
            public List<ISampleProvider> providers;
            public IWavePlayer outputDevice;
            public int pos = 0;

            List<int> remove;

            public SoundOut(Form f)
            {

                if (File.Exists("AudioSettings.txt"))
                {
                    try
                    {
                        FileStream fs = new FileStream("AudioSettings.txt", FileMode.Open);

                        string s = StreamHelper.LoadString(fs);
                        int delay = StreamHelper.LoadInt(fs);
                        int buffers = StreamHelper.LoadInt(fs);
                        bool shared = StreamHelper.LoadBool(fs);

                        fs.Close();
                        fs.Dispose();

                        if (s == "WaveOut") outputDevice = new WaveOut() { DesiredLatency = delay, NumberOfBuffers = buffers };
                        if (s == "WaveOutEvent") outputDevice = new WaveOutEvent() { DesiredLatency = delay, NumberOfBuffers = buffers };
                        if (s == "WasapiOut") outputDevice = new WasapiOut((NAudio.CoreAudioApi.AudioClientShareMode)(shared ? 0 : 1), delay);
                        if (s == "AsioOut") outputDevice = new AsioOut();
                        if (s == "DirectSoundOut") outputDevice = new DirectSoundOut();
                    }
                    catch
                    {
                        MessageBox.Show("Audio settings file was corrupted. Reseting to default.");
                        File.Delete("AudioSettings.txt");
                        outputDevice = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, false, 10);
                    }

                }
                else outputDevice = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, false, 10);
                outputDevice.Init(this, true);
                providers = new List<ISampleProvider>();
            }

            public void Play()
            {
                outputDevice.Play();
            }

            public void Stop()
            {
                outputDevice.Stop();
            }

            public void Dispose()
            {
                outputDevice.Stop();
                outputDevice.Dispose();
            }

            public WaveFormat WaveFormat
            {
                get
                {
                    return WaveFormat.CreateIeeeFloatWaveFormat(48000, 2);
                }
            }
            public int Read(float[] buffer, int offset, int count)
            {
                sw.Start();
                float[] secodaryBuffer = new float[count];
                Array.Clear(buffer, 0, buffer.Length);
                try
                {
                    foreach (ISampleProvider provider in providers)
                    {
                        Array.Clear(secodaryBuffer, 0, count);
                        int r = 0;
                        if (provider.WaveFormat.Channels == 2) r = provider.Read(secodaryBuffer, offset, count);
                        else r = provider.Read(secodaryBuffer, offset, count / 2);
                        if (provider.WaveFormat.Channels == 2) for (int i = 0; i < r; i++) buffer[i] += secodaryBuffer[i];
                        else for (int i = 0; i < r; i++) { buffer[2 * i] += secodaryBuffer[i]; buffer[2 * i + 1] += secodaryBuffer[i]; }
                    }
                }
                catch { }

                pos += count;

                sw.Stop();
                if (sw.ElapsedMilliseconds / 1000f > buffer.Length / 48000f * 3)
                {
                    toBeStoped = true;
                }
                sw.Reset();


                return count;
            }
            public bool toBeStoped = false;
            Stopwatch sw = new Stopwatch();

            public void ReplaceAudioProvider(string type, int delay, int buffers, bool shared)
            {
                outputDevice?.Stop();
                outputDevice.Dispose();
                outputDevice = null;
                if (type == "WaveOut") outputDevice = new WaveOut() { DesiredLatency = delay, NumberOfBuffers = buffers };
                if (type == "WaveOutEvent") outputDevice = new WaveOutEvent() { DesiredLatency = delay, NumberOfBuffers = buffers };
                if (type == "WasapiOut") outputDevice = new WasapiOut((NAudio.CoreAudioApi.AudioClientShareMode)(shared?0:1), delay);
                if (type == "AsioOut") outputDevice = new AsioOut();
                if (type == "DirectSoundOut") outputDevice = new DirectSoundOut();

                if (outputDevice == null)
                {
                    outputDevice = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, false, 10);
                    outputDevice.Init(this, true);
                    outputDevice.Play();
                }
                else
                {
                    outputDevice.Init(this, true);
                    outputDevice.Play();
                }
            }
            public void SaveAudioProvider(string type, int delay, int buffers, bool shared)
            {
                FileStream fs = new FileStream("AudioSettings.txt", FileMode.Create);

                StreamHelper.SaveString(fs, type);
                StreamHelper.SaveBytes(fs, delay);
                StreamHelper.SaveBytes(fs, buffers);
                StreamHelper.SaveBytes(fs, shared);

                fs.Close();
                fs.Dispose();
            }
        }
        public static SoundOut so;

        public Form1()
        {
            InitializeComponent();

            glControl = new OpenTK.GLControl();
            glControl.Location = new Point(5, 50);
            glControl.Size = new Size(480, 270);
            this.Controls.Add(glControl);

        }
        private void Render()
        {
            if (rt != null && !rendering)
            {

                if (VideoToBeStoped)
                {
                    StopVideo();
                    VideoToBeStoped = false;
                    VideoPlaying = false;
                }
                else if (VideoPlaying)
                {
                    foreach (uint u in Project.TrackList) Project.Tracks[u].ReadV(Project.m.BeatPos);

                    rt.Use();
                    GL.ClearColor(0f, 0f, 0f, 0f);
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    for (int i = 0; i < Project.TrackList.Count; i++)
                    {
                        if (Project.Tracks[Project.TrackList[i]].ChildIndex == 0)
                        {
                            Project.Tracks[Project.TrackList[i]].Render(rt, Matrix4.CreateTranslation(0, 0, -1f) * Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 2, rt.Width / (float)rt.Height, 0.01f, 1000f), Project.m.BeatPos);
                        }
                    }
                }

                rt.UnibindTextures();
                rt.UseScreen();

                GL.Disable(EnableCap.Blend);
                GL.Disable(EnableCap.DepthTest);

                GL.ClearColor(1f, 0, 0f, 1f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.Viewport(0, 0, glControl.Width, glControl.Height);
                GLDraw.DrawQuad(1, 1, 0, 1, 0, 1, false, false, rt.TextureIndex);

                GL.Flush();
                glControl.SwapBuffers();
            }
        }
        private Bitmap RenderR()
        {
            if (rt != null)
            {
                {
                    foreach (uint u in Project.TrackList) Project.Tracks[u].ReadV(Project.m.BeatPos);

                    rt.Use();
                    GL.ClearColor(0f, 0f, 0f, 0f);
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    for (int i = 0; i < Project.TrackList.Count; i++)
                    {
                        if (Project.Tracks[Project.TrackList[i]].ChildIndex == 0)
                        {
                            Project.Tracks[Project.TrackList[i]].Render(rt, Matrix4.CreateTranslation(0, 0, -1f) * Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 2, rt.Width / (float)rt.Height, 0.01f, 1000f), Project.m.BeatPos);
                        }
                    }
                }

                rt.UnibindTextures();
                rt.UseScreen();

                /*
                 * glBindTexture(GL_TEXTURE_2D, screenTex);
glCopyTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, 0, 0, w, h);
glPixelStorei(GL_PACK_ALIGNMENT, 1);
glReadPixels(0, 0, w, h, GL_RGB, GL_UNSIGNED_BYTE, (GLvoid *)pixels);
                 */

                Bitmap b = new Bitmap(Project.WIDTH, Project.HEIGHT, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                System.Drawing.Imaging.BitmapData bd = b.LockBits(new Rectangle(0, 0, Project.WIDTH, Project.HEIGHT), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.BindTexture(TextureTarget.Texture2D, rt.TextureIndex);
                GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bd.Scan0);
                //GL.CopyTexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, 0, 0, Project.WIDTH, Project.HEIGHT);
                //GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
                //GL.ReadPixels(0, 0, Project.WIDTH, Project.HEIGHT, PixelFormat.Rgba, PixelType.UnsignedByte, bd.Scan0);

                b.UnlockBits(bd);
                b.RotateFlip(RotateFlipType.RotateNoneFlipY);

                GL.Flush();
                glControl.SwapBuffers();

                return b;
            }
            return new Bitmap(Project.WIDTH, Project.HEIGHT, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            PickProjectForm ppf = new PickProjectForm();
            ppf.ShowDialog();

            if (projectName == null)
            {
                Close();
            }

            else
            {
                SetupVideo();
                glControl.Paint += (s, e2) => Render();
                KeyPreview = true;
                glControl.Parent = groupBox3;
                glControl.Dock = DockStyle.Fill;

                //projectName = "Troll";
                Project = new Project();

                if (newProject)
                {
                    CreateNewProject();
                }
                else
                {
                    ProjectHelper.LoadProject(Form1.Project, projectName);
                }


                rt = new RenderTarget(Project.WIDTH, Project.HEIGHT, PixelInternalFormat.Rgba, RenderbufferStorage.DepthComponent);
                timer1.Start();

                PreloadTextures();


                timeLineControl1.RearangeList();
                Invalidate();
                timeLineControl1.Invalidate();

                so = new SoundOut(this);
                so.Play();
                so.providers.Add(Project.m);

                timeLineControl1.parentForm = this;

                Form2 patternEdit = new Form2();
                patternEdit.Show();

                GLDraw.Init();
                //AudioFXEdit ae = new AudioFXEdit(pp, 0);
                //ae.Show();
                AudioPrerenderManager.CreateAllRequired();
                AudioPrerenderManager.ClearOutNotNeeded();
            }
        }
        private void CreateNewProject()
        {
            VideoTrack vt0 = new VideoTrack();

            Project.TrackList.Add(Project.AddTrack(vt0));
            Project.m.Layers.Add(new MixerLayer());
            Project.m.Layers.Add(new MixerLayer());

            uint mp0 = Project.AddMixerPoint(new MixerPoint());

            Project.m.Layers[1].ListOfLinks.Add(new MixerLink(mp0));
            Project.m.AddNode(0);

            Project.AddPattern(new Pattern());

            Project.WIDTH = changeWidth;
            Project.HEIGHT = changeHeight;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Render();
            if (UpdateTimeLine)
            {
                timeLineControl1.UpdateTimeLine();
            }
            if (StartRendering)
            {
                StartRendering = false;
                if (!renderVideoDescriptor) Render2(RenderFPS, RenderBeats);
                else Render3(RenderBeats);
            }
            timeLineControl1.UpdateIndicatorPos();
        } 
        private static void SetupVideo()
        {
            FFmpegBinariesHelper.RegisterFFmpegBinaries();
        }
        private void PreloadTextures()
        {
            int[] textures = new int[10];
            GL.GenTextures(10, textures);
            foreach (int i in textures) GLDraw.AddAvailableTexture(i);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            so?.Stop();
            so?.Dispose();
        }
        private void StopVideo()
        {
            foreach(KeyValuePair<uint, VideoTrack> t in Form1.Project.Tracks)
            {
                t.Value.StopVideo();
            }
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            timeLineControl1.Location = new Point(12, 12);
            timeLineControl1.Size = new Size(Width - 741 + 237,Height -66);

            //ml
            groupBox1.Location = new Point(Width - 720 + 237, Height - 523);
            groupBox1.Size = new Size(692 - 237, 441);

            //vid
            groupBox3.Location = new Point(Width - 720 + 237, 12);
            int W = 524 - 237;
            int H = Height - 541;

            if ((W/(float)H) > (Project.WIDTH / (float)Project.HEIGHT))
            {
                W = (int)(W * ((Project.WIDTH / (float)Project.HEIGHT) / (W / (float)H)));
            }
            else
            {
                H = (int)(H * ((W / (float)H) / (Project.WIDTH / (float)Project.HEIGHT)));
            }

            groupBox3.Size = new Size(W,H);

            panel1.Location = new Point(Width - 190, 12);
            panel1.Size = new Size(162,Height - 541);
            label2.Location = new Point(Width-195,Height-79);

            groupBox1.Invalidate(true);
        } 

        private void timeLineControl1_Load(object sender, EventArgs e)
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {
            AudioPrerenderManager.CreateAllRequired();
            AudioPrerenderManager.ClearOutNotNeeded(patternsEdited);
            patternsEdited.Clear();

            Form1.Project.m.Play();
            VideoPlaying = true;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            VideoToBeStoped = true;
            Form1.Project.m.Stop();
        }
        bool VideoToBeStoped = false;
        bool VideoPlaying = false;

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            timeLineControl1.KeyUpMedia(e.KeyCode);
        }
        private void Form1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            timeLineControl1.KeyDownMedia(e.KeyCode);
        }

        public static bool UpdateTimeLine = false;
        private bool rendering = false;
        public static bool renderVideoDescriptor = false;
        private void Render2(int FPS, long NumberOfBeats)
        {
            rendering = true;
            VideoToBeStoped = true;
            so.Stop();
            Form1.Project.m.Stop();
            Form1.Project.m.Read(new float[0], 0, 0);
            Project.m.BeatPos = 0;

            long audioLenght = (long)(48000 * NumberOfBeats * 240f / Project.BPM);
            long frameCount = (long)(FPS * NumberOfBeats * 240f / Project.BPM);
            double timePerFrame = (1f / FPS) * Project.BPM / 240f;
            double timePerAudioSample = (1f / 48000) * Project.BPM / 240f;

            double timeIn = -timePerFrame;
            double timeInP = 0;

            string dir = @"rendered\" + projectName;

            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            foreach (string f in Directory.GetFiles(dir)) File.Delete(f);

            bool ct2 = false;
            for (int i = 0; i < frameCount; i++)
            {
                Project.m.BeatPos += (float)timePerFrame;
                Bitmap b = RenderR();

                bool end2 = false;
                if (Debug2)
                {
                    DebugImage di = new DebugImage(b, "Frame #" + i+ "-Does this look right?");
                    di.ShowDialog();
                    if (!Debug2)
                    {
                        Debug2 = true;
                        end2 = true;
                    }
                }


                b.Save(dir + @"\video" + (i) + ".png");
                if (Debug2)
                {
                    try
                    {
                        Bitmap c = new Bitmap(dir + @"\video" + (i) + ".png");
                        DebugImage di = new DebugImage(c, "This was saved, Is it the same?");
                        di.ShowDialog();
                        Debug2 = true;
                    }
                    catch
                    {
                        MessageBox.Show("REPORT ME: Image maybe rendered internaly, but file doesn't exist");
                    }
                }
                b.Dispose();
                GC.Collect();
                timeInP = timeIn;

                if (end2)
                {
                    Debug2 = false;
                    ct2 = true;
                }
            }

            if (ct2 == true) Debug2 = true;

            Project.m.UseAuto = false;

            Project.m.BeatPos = 0;
            timeIn = -timePerAudioSample * 100;
            timeInP = 0;
            Project.m.Play();
            float[] buf = new float[200];
            float[] debugbuf = new float[48000];

            float max = 0;
            for (int i = 0; i < audioLenght; i += 100)
            {
                timeIn += timePerAudioSample * 100;

                Array.Clear(buf, 0, 200);
                Project.m.Read(buf, 0, 200);
                for (int j = 0; j < 200; j++)
                {
                    if (Math.Abs(buf[j]) > max) max = Math.Abs(buf[j]);
                }

                if (Debug2 && i < 48000)
                {
                    for (int j = 0; j < 100; j++) debugbuf[j + i] = buf[j * 2]; 
                }
                timeInP = timeIn;
            }

            Form1.Project.m.Stop();
            Form1.Project.m.Read(new float[0], 0, 0);
            
                Form1.Project.m.Stop();
                Form1.Project.m.Read(new float[0], 0, 0);
                Project.m.BeatPos = 0;
                timeIn = -timePerAudioSample * 100;
                timeInP = 0;
                buf = new float[200];
                Project.m.Play();

                using (WaveFileWriter writer = new WaveFileWriter(dir + @"\audio.wav", new WaveFormat(48000, 16, 2)))
                {
                    for (int i = 0; i < audioLenght; i += 100)
                    {
                        timeIn = timePerAudioSample * i;

                        Array.Clear(buf, 0, 200);
                        Project.m.Read(buf, 0, 200);
                        for (int j = 0; j < 200; j++)
                        {
                            buf[j] /= max;
                            if (buf[j] > 0.999f) buf[j] = 0.999f;
                            if (buf[j] < -0.999f) buf[j] = -0.999f;
                        }
                        writer.WriteSamples(buf, 0, 200);

                        timeInP = timeIn;
                    }

                    writer.Close();
                    writer.Dispose();
                }

            Project.m.UseAuto = true;

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe"
            };

            if (Debug2) if (!File.Exists(dir + @"\audio.wav")) MessageBox.Show("REPORT ME - audio file doesn't get created");

            string path = PathHelper.CurrentDirectory();
            if (path.StartsWith("file:\\")) path = path.Remove(0, 6);
            string arg = "/c " + path[0] + ": & cd \"" + path + @"\"+ dir + "\" & \"" + ""+ path + "\\FFMPEG\\bin\\x86\\ffmpeg" + "\" -framerate " + FPS + " -i video%d.png -i audio.wav -pix_fmt yuv420p video.mp4";
            startInfo.Arguments = arg;
            process.StartInfo = startInfo;
            process.ErrorDataReceived += (a, sss) => { throw new Exception(); };

            process.Start();
            while (!process.HasExited) ;


            VideoToBeStoped = true;
            Form1.Project.m.Stop();

            if (Debug2) if (!File.Exists(dir + @"\video.mp4")) MessageBox.Show("REPORT ME - video file doesn't get created");
            if (Debug2) MessageBox.Show("Now, rendering is over. If there were no problems, go to correct directory to find the file.");

            so.Play();
            rendering = false;
        }
        private void Render3(long NumberOfBeats)
        {
            rendering = true;
            VideoToBeStoped = true;
            so.Stop();
            Form1.Project.m.Stop();
            Form1.Project.m.Read(new float[0], 0, 0);
            Project.m.BeatPos = 0;


            long audioLenght = (long)(48000 * NumberOfBeats * 240f / Project.BPM);
            double timePerAudioSample = (1f / 48000) * Project.BPM / 240f;

            double timeIn = 0;
            double timeInP = 0;

            string dir = @"rendered\" + projectName;

            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            foreach (string f in Directory.GetFiles(dir)) File.Delete(f);

            FileStream fs = new FileStream(dir + @"\descriptor.srd", FileMode.Create);
            DescribeVideo(fs);
            fs.Close();
            fs.Dispose();

            Project.m.UseAuto = false;

            Project.m.BeatPos = 0;
            timeIn = -timePerAudioSample * 100;
            timeInP = 0;
            Project.m.Play();
            float[] buf = new float[200];
            float[] debugbuf = new float[48000];

            float max = 0;
            for (int i = 0; i < audioLenght; i += 100)
            {
                timeIn += timePerAudioSample * 100;

                Array.Clear(buf, 0, 200);
                Project.m.Read(buf, 0, 200);
                for (int j = 0; j < 200; j++)
                {
                    if (Math.Abs(buf[j]) > max) max = Math.Abs(buf[j]);
                }

                if (Debug2 && i < 48000)
                {
                    for (int j = 0; j < 100; j++) debugbuf[j + i] = buf[j * 2];
                }
                timeInP = timeIn;
            }

            Form1.Project.m.Stop();
            Form1.Project.m.Read(new float[0], 0, 0);

            Form1.Project.m.Stop();
            Form1.Project.m.Read(new float[0], 0, 0);
            Project.m.BeatPos = 0;
            timeIn = -timePerAudioSample * 100;
            timeInP = 0;
            buf = new float[200];
            Project.m.Play();

            using (WaveFileWriter writer = new WaveFileWriter(dir + @"\audio.wav", new WaveFormat(48000, 16, 2)))
            {
                for (int i = 0; i < audioLenght; i += 100)
                {
                    timeIn = timePerAudioSample * i;

                    Array.Clear(buf, 0, 200);
                    Project.m.Read(buf, 0, 200);
                    for (int j = 0; j < 200; j++)
                    {
                        buf[j] /= max;
                        if (buf[j] > 0.999f) buf[j] = 0.999f;
                        if (buf[j] < -0.999f) buf[j] = -0.999f;
                    }
                    writer.WriteSamples(buf, 0, 200);

                    timeInP = timeIn;
                }

                writer.Close();
                writer.Dispose();
            }

            Project.m.UseAuto = true;

            VideoToBeStoped = true;
            Form1.Project.m.Stop();

            so.Play();
            rendering = false;
        }
        private void DescribeVideo(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);

            StreamHelper.SaveBytes(fs, (int)Project.ProjectSamples.Count);
            foreach(KeyValuePair<uint, SampleAV> k in Project.ProjectSamples)
            {
                SRDHelper.SerializeVideoSource(fs, k.Value);
            }
            StreamHelper.SaveBytes(fs, (int)Project.Tracks.Count);
            foreach(uint u in Project.TrackList)
            {
                SRDHelper.SerializeAllNoteEvents(fs, Project.Tracks[u]);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            timeLineControl1.placingMediaStyle = TimeLineControl.MediaPlaceStyle.FromMediaLibrary;
        }
        public TimelineMediaType getTLM()
        {
            return mediaLibrary1.getTLM();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ProjectHelper.SaveProject(Project, projectName);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            timeLineControl1.ToggleNT();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Form2 f2 = new Form2();
            f2.Show();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            LoaderForm lf = new LoaderForm();
            lf.Show();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Text_Generator tg = new Text_Generator();
            tg.Show();
        }

        public static bool StartRendering = false;
        public static int RenderBeats = 76;
        public static int RenderFPS = 60;

        private void button9_Click(object sender, EventArgs e)
        {
            RenderForm rf = new RenderForm();
            rf.Show();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            SettingsForm sf = new SettingsForm();
            sf.Show();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Project.m.AutoMultiplier = 1;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            DebugForm df = new DebugForm();
            df.Show();
        }

        public static bool Debug1 = false;
        public static bool Debug2 = false;
        public static bool Debug3 = false;
    }

    public class VideoSource : IDisposable
    {
        public unsafe void SeekTo(float timeKey)
        {
            if (DefaultToNull) return;
            VideoPos = (int)Math.Round(timeKey * FPS);

            ffmpeg.av_seek_frame(vsd._pFormatContext, -1, (long)(timeKey * 1000000) + 1, (int)ffmpeg.AVSEEK_FLAG_BACKWARD);
        }

        VideoStreamDecoder vsd = null;
        VideoFrameConverter vfc = null;
        public int Tex { get; private set; }
        public float FPS { get; private set; }
        private int VideoPos = 0;
        string videoFile = null;

        public VideoSource(string file)
        {
            try
            {
                videoFile = file;
                SetupDecoder(file);
            }
            catch
            {
                DefaultToNull = true;
            }
        }
        public VideoSource(string file, int tex)
        {
            try
            {
                videoFile = file;
                SetupDecoder(file, tex);
            }
            catch
            {
                DefaultToNull = true;
            }
        }
        public bool DefaultToNull { get; private set; }

        private bool DisposeTexture = false;
        private unsafe void SetupDecoder(string video)
        {
            DisposeTexture = true;

            var url = video;
            vsd = new VideoStreamDecoder(url);

            var info = vsd.GetContextInfo();
            info.ToList().ForEach(x => Console.WriteLine($"{x.Key} = {x.Value}"));
            var sourceSize = vsd.FrameSize;
            var sourcePixelFormat = vsd.PixelFormat;
            var destinationSize = sourceSize;
            var destinationPixelFormat = AVPixelFormat.AV_PIX_FMT_BGR24;
            FPS = vsd._pFormatContext->streams[0]->r_frame_rate.num / (float)vsd._pFormatContext->streams[0]->r_frame_rate.den;
            if (float.IsNaN(FPS)) FPS = 30f;

            Tex = GL.GenTexture();

            vfc = new VideoFrameConverter(sourceSize, sourcePixelFormat, destinationSize, destinationPixelFormat);
        }
        private unsafe void SetupDecoder(string video, int textureNumber)
        {
            var url = video;
            vsd = new VideoStreamDecoder(url);

            var info = vsd.GetContextInfo();
            info.ToList().ForEach(x => Console.WriteLine($"{x.Key} = {x.Value}"));
            var sourceSize = vsd.FrameSize;
            var sourcePixelFormat = vsd.PixelFormat;
            var destinationSize = sourceSize;
            var destinationPixelFormat = AVPixelFormat.AV_PIX_FMT_BGR24;
            FPS = vsd._pFormatContext->streams[0]->r_frame_rate.num / (float)vsd._pFormatContext->streams[0]->r_frame_rate.den;
            if (float.IsNaN(FPS)) FPS = 30f;

            Tex = textureNumber;

            vfc = new VideoFrameConverter(sourceSize, sourcePixelFormat, destinationSize, destinationPixelFormat);
        }
        private unsafe bool DecodeNextFrame(bool moreExpected)
        {
            if (DefaultToNull) return false;
            if (vsd != null)
                if (vsd.TryDecodeNextFrame(out var frame))
                {
                    var convertedFrame = vfc.Convert(frame);

                    if (!moreExpected)
                    {
                        GL.BindTexture(TextureTarget.Texture2D, Tex);
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, convertedFrame.width, convertedFrame.height, 0, PixelFormat.Bgr, PixelType.UnsignedByte, (IntPtr)convertedFrame.data[0]);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                    }

                    VideoPos++;

                    return true;
                }
            return false;
        }
        private unsafe bool DecodeNextFrame(out int TextureNumber)
        {
            TextureNumber = 0;

            if (DefaultToNull) return false;
            if (vsd != null)
                if (vsd.TryDecodeNextFrame(out var frame))
                {
                    var convertedFrame = vfc.Convert(frame);

                    TextureNumber = GL.GenTexture();
                    
                    GL.BindTexture(TextureTarget.Texture2D, TextureNumber);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, convertedFrame.width, convertedFrame.height, 0, PixelFormat.Bgr, PixelType.UnsignedByte, (IntPtr)convertedFrame.data[0]);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                    

                    VideoPos++;

                    return true;
                }
            return false;
        }
        private unsafe bool DecodeNextFrame(out System.Drawing.Bitmap Bitmap)
        {
            Bitmap = null;
            if (DefaultToNull) return false;
            if (vsd != null)
                if (vsd.TryDecodeNextFrame(out var frame))
                {
                    var convertedFrame = vfc.Convert(frame);

                    Bitmap = new Bitmap(convertedFrame.width, convertedFrame.height,3*convertedFrame.width, System.Drawing.Imaging.PixelFormat.Format24bppRgb, (IntPtr)convertedFrame.data[0]);


                    VideoPos++;

                    return true;
                }
            return false;
        }
        public void readFrame(float AudioPos)
        {
            if (DefaultToNull) return;

            float sec = AudioPos / 48000f;
            int expFrNumber = (int)(sec * FPS);

            for (long i = VideoPos; i < expFrNumber; i++) if (!DecodeNextFrame(VideoPos < expFrNumber - 1)) break;
        }

        public int extractFrameNumber(int Frame)
        {
            if (DefaultToNull) return 0;

            int expFrNumber = Frame;

            for (long i = VideoPos; i < expFrNumber; i++)
            {
                if (!DecodeNextFrame(true)) break;
            }

            int tex = 0;
            DecodeNextFrame(out tex);
            return tex;
        }
        public int extractNext()
        {
            if (DefaultToNull) return 0;

            int tex = 0;
            DecodeNextFrame(out tex);
            return tex;
        }
        public System.Drawing.Bitmap extractBitmap()
        {
            if (DefaultToNull) return null;

            Bitmap b = null;
            DecodeNextFrame(out b);
            return b;
        }

        public void Dispose()
        {
            vsd.Dispose();
            vfc.Dispose();
            if (DisposeTexture) GL.DeleteTexture(Tex);
        }

    }

    public static class GLDraw
    {
        static int QuadVertex = 0;
        static int QuadUV = 0;
        public static int QuadVAO = 0;

        static int InfXVertex = 0;
        static int InfXUV = 0;
        public static int InfXVAO = 0;

        static int InfYVertex = 0;
        static int InfYUV = 0;
        public static int InfYVAO = 0;

        static int InfXYVertex = 0;
        static int InfXYUV = 0;
        public static int InfXYVAO = 0;


        static int VertexSource = 0;
        static int FragmentSource = 0;
        public static int DefaultShader = 0;

        public static void Init()
        {
            for (int i = 0; i < 10; i++)
            {
                RenderTargets[i] = new RenderTarget(Form1.Project.WIDTH, Form1.Project.HEIGHT, PixelInternalFormat.Rgba, RenderbufferStorage.DepthComponent);
            }
        }
        public static RenderTarget GetFreeTarget(int textureIndex)
        {
            if (textureIndex == RenderTargets[0].TextureIndex) return RenderTargets[1];
            return RenderTargets[0];
        }

        public static RenderTarget[] RenderTargets = new RenderTarget[10];

        static GLDraw()
        {
            AvailableTextures = new List<int>();

            VertexSource = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexSource, VertexShader);
            GL.CompileShader(VertexSource);

            FragmentSource = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentSource, FragmentShader);
            GL.CompileShader(FragmentSource);

            string s = GL.GetShaderInfoLog(VertexSource);
            string s2 = GL.GetShaderInfoLog(FragmentSource);

            DefaultShader = GL.CreateProgram();
            GL.AttachShader(DefaultShader, VertexSource);
            GL.AttachShader(DefaultShader, FragmentSource);
            GL.LinkProgram(DefaultShader);

            string s3 = GL.GetProgramInfoLog(DefaultShader);

            float[] quadVertex = new float[] { -1f, -1f, 0f, 1f, -1f, 0f, 1f, 1f, 0f,
            -1f, -1f, 0f, 1f, 1f, 0f, -1f, 1f, 0f,};
            float[] quadUV = new float[] { 0f, 0f, 1f, 0f, 1f, 1f, 0f, 0f, 1f, 1f, 0f, 1f };
            QuadVAO = GL.GenVertexArray();
            GL.BindVertexArray(QuadVAO);
            QuadVertex = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, QuadVertex);
            GL.BufferData<float>(BufferTarget.ArrayBuffer, 12 * 6, quadVertex, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            QuadUV = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, QuadUV);
            GL.BufferData<float>(BufferTarget.ArrayBuffer, 8 * 6, quadUV, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            float[] infxVertex = new float[] { -1001f, -1f, 0f, 1001f, -1f, 0f, 1001f, 1f, 0f,
            -1001f, -1f, 0f, 1001f, 1f, 0f, -1001f, 1f, 0f,};
            float[] infxUV = new float[] { -500f, 0f, 501f, 0f, 501f, 1f, -500f, 0f, 501f, 1f, -500f, 1f };
            InfXVAO = GL.GenVertexArray();
            GL.BindVertexArray(InfXVAO);
            InfXVertex = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, InfXVertex);
            GL.BufferData<float>(BufferTarget.ArrayBuffer, 12 * 6, infxVertex, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            InfXUV = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, InfXUV);
            GL.BufferData<float>(BufferTarget.ArrayBuffer, 8 * 6, infxUV, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            float[] infyVertex = new float[] { -1f, -1001f, 0f, 1f, -1001f, 0f, 1f, 1001f, 0f,
            -1f, -1001f, 0f, 1f, 1001f, 0f, -1f, 1001f, 0f,};
            float[] infyUV = new float[] { 0f, -500f, 1f, -500f, 1f, 501f, 0f, -500f, 1f, 501f, 0f, 501f };
            InfYVAO = GL.GenVertexArray();
            GL.BindVertexArray(InfYVAO);
            InfYVertex = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, InfYVertex);
            GL.BufferData<float>(BufferTarget.ArrayBuffer, 12 * 6, infyVertex, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            InfYUV = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, InfYUV);
            GL.BufferData<float>(BufferTarget.ArrayBuffer, 8 * 6, infyUV, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            float[] infxyVertex = new float[] { -1001f, -1001f, 0f, 1001f, -1001f, 0f, 1001f, 1001f, 0f,
            -1001f, -1001f, 0f, 1001f, 1001f, 0f, -1001f, 1001f, 0f,};
            float[] infxyUV = new float[] { -500f, -500f, 501f, -500f, 501f, 501f, -500f, -500f, 501f, 501f, -500f, 501f };
            InfXYVAO = GL.GenVertexArray();
            GL.BindVertexArray(InfXYVAO);
            InfXYVertex = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, InfXYVertex);
            GL.BufferData<float>(BufferTarget.ArrayBuffer, 12 * 6, infxyVertex, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            InfXYUV = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, InfXYUV);
            GL.BufferData<float>(BufferTarget.ArrayBuffer, 8 * 6, infxyUV, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);


            GL.EnableVertexAttribArray(0);
            GL.BindVertexArray(0);            

            //GL.DetachShader(DefaultShader, VertexSource);
            //GL.DetachShader(DefaultShader, FragmentSource);

            
        }

        public static void DrawCube(float size, float ux0, float ux1, float uy0, float uy1, bool XFlip, bool YFlip)
        {

            if (XFlip)
            {
                float p = ux0;
                ux0 = ux1;
                ux1 = p;
            }
            if (YFlip)
            {
                float p = uy0;
                uy0 = uy1;
                uy1 = p;
            }

            GL.Begin(BeginMode.Quads);

            // -Z
            GL.TexCoord2(ux0, uy0);
            GL.Vertex3(-size, -size, -size);
            GL.TexCoord2(ux1, uy0);
            GL.Vertex3(size, -size, -size);
            GL.TexCoord2(ux1, uy1);
            GL.Vertex3(size, size, -size);
            GL.TexCoord2(ux0, uy1);
            GL.Vertex3(-size, size, -size);

            // -Y
            GL.TexCoord2(ux0, uy0);
            GL.Vertex3(-size, -size, -size);
            GL.TexCoord2(ux1, uy0);
            GL.Vertex3(size, -size, -size);
            GL.TexCoord2(ux1, uy1);
            GL.Vertex3(size, -size, size);
            GL.TexCoord2(ux0, uy1);
            GL.Vertex3(-size, -size, size);

            // -X
            GL.TexCoord2(ux1, uy0);
            GL.Vertex3(-size, -size, -size);
            GL.TexCoord2(ux0, uy0);
            GL.Vertex3(-size, -size, size);
            GL.TexCoord2(ux0, uy1);
            GL.Vertex3(-size, size, size);
            GL.TexCoord2(ux1, uy1);
            GL.Vertex3(-size, size, -size);

            // +Z
            GL.TexCoord2(ux1, uy0);
            GL.Vertex3(-size, -size, size);
            GL.TexCoord2(ux0, uy0);
            GL.Vertex3(size, -size, size);
            GL.TexCoord2(ux0, uy1);
            GL.Vertex3(size, size, size);
            GL.TexCoord2(ux1, uy1);
            GL.Vertex3(-size, size, size);

            // +Y
            GL.TexCoord2(ux0, uy0);
            GL.Vertex3(-size, size, -size);
            GL.TexCoord2(ux0, uy1);
            GL.Vertex3(-size, size, size);
            GL.TexCoord2(ux1, uy1);
            GL.Vertex3(size, size, size);
            GL.TexCoord2(ux1, uy0);
            GL.Vertex3(size, size, -size);


            // +X
            GL.TexCoord2(ux0, uy0);
            GL.Vertex3(size, -size, -size);
            GL.TexCoord2(ux0, uy1);
            GL.Vertex3(size, size, -size);
            GL.TexCoord2(ux1, uy1);
            GL.Vertex3(size, size, size);
            GL.TexCoord2(ux1, uy0);
            GL.Vertex3(size, -size, size);

            GL.End();
        }
        public static void DrawQuad(float sizeX, float sizeY, float ux0, float ux1, float uy0, float uy1, bool XFlip, bool YFlip, int tex)
        {
            GL.BindTexture(TextureTarget.Texture2D, tex);

            GL.UseProgram(DefaultShader);

            GL.BindVertexArray(QuadVAO);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.BindAttribLocation(DefaultShader, 0, "vertexPosition");
            GL.BindAttribLocation(DefaultShader, 1, "vertexUV");
           
            Matrix4 m = Matrix4.Identity;
            GL.UniformMatrix4(102, false, ref m);
            GL.Uniform4(101, Vector4.Zero);
            GL.UniformMatrix4(20, false, ref m);
            GL.Uniform1(21, 0f);
            GL.Uniform1(22, 1f);
            GL.Uniform1(23, 0f);
            GL.Uniform1(24, 1f);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.BindVertexArray(0);
        }


        public static void DrawVideoDefault(TextureInfo ti, RenderTarget rt, Matrix4 m4)
        {
            rt.Use();

            GL.Viewport(0, 0, rt.Width, rt.Height);

            GL.BindTexture(TextureTarget.Texture2D, ti.TextureIndex);
            GL.UseProgram(DefaultShader);
            GL.BindVertexArray(QuadVAO);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.BindAttribLocation(DefaultShader, 0, "vertexPosition");
            GL.BindAttribLocation(DefaultShader, 1, "vertexUV");
            
            Matrix4 m = Matrix4.CreateScale(ti.Width/(float)ti.Height, 1f, 1f) * ti.RenderMatrix * m4;
            Matrix4 m2 = ti.RenderColorMatrix;

            GL.Uniform4(101, ti.RenderColorOffset);
            GL.UniformMatrix4(102, false, ref m2);
            GL.UniformMatrix4(20, false, ref m);
            GL.Uniform1(21, ti.RenderUVX0);
            GL.Uniform1(22, ti.RenderUVX1);
            GL.Uniform1(23, ti.RenderUVY0 + 0.0f);
            GL.Uniform1(24, ti.RenderUVY1 + 0.0f);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.BindVertexArray(0);

        }
        public static void DrawVideoDefault(TextureInfo ti, RenderTarget rt, Matrix4 m4, bool infX, bool infY)
        {
            rt.Use();

            GL.Viewport(0, 0, rt.Width, rt.Height);

            GL.BindTexture(TextureTarget.Texture2D, ti.TextureIndex);
            GL.UseProgram(DefaultShader);

            if (!infX && !infY) GL.BindVertexArray(QuadVAO);
            if (infX && !infY) GL.BindVertexArray(InfXVAO);
            if (!infX && infY) GL.BindVertexArray(InfYVAO);
            if (infX && infY) GL.BindVertexArray(InfXYVAO);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.BindAttribLocation(DefaultShader, 0, "vertexPosition");
            GL.BindAttribLocation(DefaultShader, 1, "vertexUV");

            Matrix4 m = Matrix4.CreateScale(ti.Width / (float)ti.Height, 1f, 1f) * ti.RenderMatrix * m4;
            Matrix4 m2 = ti.RenderColorMatrix;

            GL.Uniform4(101, ti.RenderColorOffset);
            GL.UniformMatrix4(102, false, ref m2);
            GL.UniformMatrix4(20, false, ref m);
            GL.Uniform1(21, ti.RenderUVX0);
            GL.Uniform1(22, ti.RenderUVX1);
            GL.Uniform1(23, ti.RenderUVY0 + 0.0f);
            GL.Uniform1(24, ti.RenderUVY1 + 0.0f);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.BindVertexArray(0);

        }
        public static void DrawVideoDefault(TextureInfo ti, RenderTarget rt, Matrix4 m4, float ar)
        {
            rt.Use();

            GL.Viewport(0, 0, rt.Width, rt.Height);

            GL.BindTexture(TextureTarget.Texture2D, ti.TextureIndex);
            GL.UseProgram(DefaultShader);
            GL.BindVertexArray(QuadVAO);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.BindAttribLocation(DefaultShader, 0, "vertexPosition");
            GL.BindAttribLocation(DefaultShader, 1, "vertexUV");

            Matrix4 m = Matrix4.CreateScale(ar, 1f, 1f) * ti.RenderMatrix * m4;
            Matrix4 m2 = ti.RenderColorMatrix;

            GL.Uniform4(101, ti.RenderColorOffset);
            GL.UniformMatrix4(102, false, ref m2);
            GL.UniformMatrix4(20, false, ref m);
            GL.Uniform1(21, ti.RenderUVX0);
            GL.Uniform1(22, ti.RenderUVX1);
            GL.Uniform1(23, ti.RenderUVY0 + 0.0f);
            GL.Uniform1(24, ti.RenderUVY1 + 0.0f);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.BindVertexArray(0);

        }

        public static int GetTextureName()
        {
            if (AvailableTextures.Count > 0)
            {
                int i = AvailableTextures[0];
                AvailableTextures.RemoveAt(0);
                return i;
            }
            return 0;
        }

        private static List<int> AvailableTextures;
        public static void AddAvailableTexture(int i)
        {
            AvailableTextures.Add(i);
        }
        public static int AvailableTexturesCount()
        {
            return AvailableTextures.Count;
        }

        public static string VertexShader = @"
#version 430 core
layout(location = 0) in vec3 vertexPosition;
layout(location = 1) in vec2 vertexUV;

layout(location = 20) uniform mat4 matrix;

layout(location = 21) uniform float uvX0;
layout(location = 22) uniform float uvX1;
layout(location = 23) uniform float uvY0;
layout(location = 24) uniform float uvY1;

out vec2 uv;

void main(void)
{
    uv = mix(vec2(uvX0,uvY0),vec2(uvX1,uvY1),vertexUV);
    gl_Position = matrix * vec4(vertexPosition, 1);
}
";
        public static string FragmentShader = @"
#version 430 core

uniform sampler2D texture;

layout(location = 101) uniform vec4 colorO;
layout(location = 102) uniform mat4 colorM;

in vec2 uv;

out vec4 fragment;

void main(void)
{
    fragment = colorO + colorM * texture2D(texture, uv);
}
";
        public static string VertexShaderFXBase = @"
#version 430 core
layout(location = 0) in vec3 vertexPosition;
layout(location = 1) in vec2 vertexUV;

out vec2 uv;

void main(void)
{
    uv = vertexUV;
    gl_Position = vec4(vertexPosition, 1);
}
";
        public static string FragmentShader2 = @"
#version 430 core

uniform sampler2D texture;
in vec2 uv;

out vec4 fragment;

void main(void)
{
    fragment = colorO + colorM * texture2D(texture, uv);
}
";
    }

   

    public class RenderTarget : IDisposable
    {
        static int FrameBufferIndex = 0;

        public int TextureIndex { get; private set; }
        public int RenderBufferIndex { get; private set; }

        public int Height = 1;
        public int Width = 1;

        public RenderTarget(int width, int height, PixelInternalFormat textureFormat, RenderbufferStorage depthFormat)
        {
            //FRAMEBUFFER?
            if (FrameBufferIndex == 0)
            {
                FrameBufferIndex = GL.GenFramebuffer();
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferIndex);

            //COLOR
            TextureIndex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, TextureIndex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, textureFormat, width, height, 0, PixelFormat.Rgb, PixelType.Byte, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);

            //DEPTH
            RenderBufferIndex = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RenderBufferIndex);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, depthFormat, width, height);

            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, RenderBufferIndex);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, TextureIndex, 0);

            DrawBuffersEnum[] dbe = new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0 };
            GL.DrawBuffers(1, dbe);

            Width = width;
            Height = height;
        }
        public void Use()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferIndex);
            GL.BindTexture(TextureTarget.Texture2D, TextureIndex);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, TextureIndex, 0);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RenderBufferIndex);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, RenderBufferIndex);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
        }
        public void UnibindTextures()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferIndex);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, 0, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, 0);
        }
        public void UseScreen()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Dispose()
        {
            GL.DeleteTexture(TextureIndex);
            GL.DeleteRenderbuffer(RenderBufferIndex);
        }
        public void FinalDispose()
        {
            GL.DeleteFramebuffer(FrameBufferIndex);
        }
    }

    public class FloatArraySource : ISampleProvider
    {
        private float[] samples;
        private long pos;
        private int sampleRate;

        public FloatArraySource(float[] samples, int sr)
        {
            this.samples = samples;
            sampleRate = sr;
        }

        public WaveFormat WaveFormat
        {
            get
            {
                return WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
            }
        }
        public int Read(float[] buffer, int offset, int count)
        {
            int read = 0;
            if (samples != null) for (int i = 0; i < count; i++)
                {
                    if ((pos + i) < samples.Length)
                    {
                        buffer[i] = samples[pos + i];
                        read++;
                    }
                }
            pos += count;
            return read;
        }
    }

    public class PsolaReader : ISampleProvider
    {
        float pos = 0;
        float relPos = 0;
        float basePitch = 0;
        float baseLenght = 0;
        int baseLenght2 = 0;
        ISampleProvider isp;

        List<float> floatSamples;
        int startAt;
        int endAt;

        private void RemoveSamples(int newIndex)
        {
            int c = newIndex - startAt;
            if (c <= 0) return;
            startAt += c;
            for (int i = 0; i < c; i++) floatSamples.RemoveAt(0);
        }
        private void ReadUntil(int index)
        {
            int c = index - endAt;
            if (c <= 0) return;
            endAt += c;
            float[] f = new float[c];
            isp.Read(f, 0, c);
            floatSamples.AddRange(f);
        }
        public PsolaReader(ISampleProvider isp, float basePitch)
        {
            this.isp = isp;
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(isp.WaveFormat.SampleRate, 1);
            startAt = 0;

            Pitch = 1;
            Speed = 1;

            this.basePitch = basePitch;

            floatSamples = new List<float>();

            baseLenght = isp.WaveFormat.SampleRate / basePitch;
            baseLenght2 = (int)(2f * baseLenght);
        }

        public float Speed { get; set; }
        public float Pitch { get; set; }

        public WaveFormat WaveFormat { get; }

        private float ReadOne()
        {
            float actualHz = basePitch;
            float mult = Pitch;
            float fundamentalLenght = 48000f / actualHz;

            //pozice
            float relativniZmena = mult - Speed;
            relativniZmena = relativniZmena / fundamentalLenght;

            //ziskani
            float samplePos1 = pos + (relPos * fundamentalLenght);
            float samplePos2 = pos + ((relPos - 1) * fundamentalLenght);

            float vol2 = relPos;
            float vol1 = 1 - vol2;

            float output = 0;
            output += vol1 * this[samplePos1];
            output += vol2 * this[samplePos2];

            pos += Speed;
            relPos += relativniZmena;
            relPos %= 1f;

            return output;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            ReadUntil((int)pos + count * ((int)Speed + 1) + 2 * baseLenght2);
            for (int i = 0; i < count; i++) buffer[i + offset] = ReadOne();
            RemoveSamples((int)pos - count * ((int)Speed + 1) - 2 * baseLenght2);
            return count;
        }

        public float this[float f]
        {
            get
            {
                if (f < startAt + 1) return 0;
                if (f > endAt + 1) return 0;
                if (f % 1 == 0f) return floatSamples[(int)f - startAt];
                else return (1-f%1)*floatSamples[(int)f - startAt] + (f % 1f)*floatSamples[(int)f - startAt + 1];
            }
        }
    }

    public static class SampleComputationHelper
    {
        //ADSR
        //Modulation
        public static void CalculateModulation(float beatFreq, float beatof, float timeBeat0, float timeBeat1, int updateRate, out float sineTimeParam0, out float sineTimeParamDiff)
        {
            float tbs = (timeBeat0 - beatof) * beatFreq;
            float tbe = (timeBeat1 - beatof) * beatFreq;
            float dx = (tbe - tbs) / updateRate;

            sineTimeParam0 = (float)(tbs * 2 * Math.PI);
            sineTimeParamDiff = (float)(dx * 2 * Math.PI);
        }
        //FINOUT
        public static void CalculateFadeIn(float beatFadeIn, float timeBeat0, float timeBeat1, float timeBeatStart, float timeBeatOff, int updateRate, out float multStart, out float multDiff)
        {
            float t0 = (timeBeat0 - timeBeatOff - timeBeatStart) / beatFadeIn;
            float t1 = (timeBeat1 - timeBeatOff - timeBeatStart) / beatFadeIn;

            multStart = t0;
            multDiff = (t1 - t0) / updateRate;
        }
        public static void CalculateFadeOut(float beatFadeOut, float timeBeat0, float timeBeat1, float timeBeatEnd, int updateRate, out float multStart, out float multDiff)
        {
            float t0 = (timeBeatEnd - timeBeat0) / beatFadeOut;
            float t1 = (timeBeatEnd - timeBeat1) / beatFadeOut;

            multStart = t0;
            multDiff = (t1 - t0) / updateRate;
        }

        // 2 / 4 PART AUTOMATION
        public static float CalculatePropertySine4(ref float[] PROPERTIES, byte index, float timeBeatIn)
        {
            if (timeBeatIn < PROPERTIES[index + 2]) return 0;
            else if (PROPERTIES[index + 3] != 0) return (Math.Min(1, (timeBeatIn - PROPERTIES[index + 2]) / PROPERTIES[index + 3])) * PROPERTIES[index + 1] * (float)Math.Sin(((timeBeatIn - PROPERTIES[index + 2]) * PROPERTIES[index]) * 2 * Math.PI);
            else return PROPERTIES[index + 1] * (float)Math.Sin(((timeBeatIn - PROPERTIES[index + 2]) * PROPERTIES[index]) * 2 * Math.PI);
        }
        // ALL Audio OPS per SAMPLE
        public static void PerSampleAuto(ref float[] samples, float L, float R, ADSR adsr, float fin0, float fin1, float fout0, float fout1)
        {
            for (int i = 0; i < samples.Length; i += 2)
            {
                samples[i] *= L;
                samples[i + 1] *= R;
            }
            if (adsr != null) adsr.ProccessLR(ref samples);
            if (!(fin0 == 1 && fin1 == 1 && fout0 == 1 && fout1 == 1))
            {
                for (int i = 0; i < samples.Length; i += 2)
                {
                    samples[i] *= Math.Min(1, Math.Max(0, fin0 + 2 * (fin1 - fin0) / samples.Length));
                    samples[i + 1] *= Math.Min(1, Math.Max(0, fin0 + 2 * (fin1 - fin0) / samples.Length));
                }
                for (int i = 0; i < samples.Length; i += 2)
                {
                    samples[i] *= Math.Min(1, Math.Max(0, fout0 + 2 * (fout1 - fout0) / samples.Length));
                    samples[i + 1] *= Math.Min(1, Math.Max(0, fout0 + 2 * (fout1 - fout0) / samples.Length));
                }
            }
        }
    }
    public static class VideoHelper
    {
        public static void GetIFramesList(string videoFile, string outputFile)
        {
            try
            {
                string path = PathHelper.CurrentDirectory() + "\\FFMPEG\\bin\\x86";

                if (path.StartsWith("file:\\")) path = path.Remove(0, 6);
                string arg = "/C " + path[0] + ": & cd \"" + path + "\" & ffprobe -v error -skip_frame nokey -show_entries frame=pkt_pts_time -select_streams v -of csv=p=0 \"" + videoFile + "\" > \"" + outputFile + "\"";
                Process p = System.Diagnostics.Process.Start("CMD.exe", arg);
                p.WaitForExit();

            }
            catch
            {
            }
        }
        public static void GetInfoFile(string videoFile, string outputFile)
        {
            try
            {
                string path = PathHelper.CurrentDirectory() + "\\FFMPEG\\bin\\x86";
                //avg_frame_rate,display_aspect_ratio, width, height, duration
                if (path.StartsWith("file:\\")) path = path.Remove(0, 6);
                string arg = "/C " + path[0] + ": & cd \"" + path + "\" & ffprobe -v 0 -of compact=p=0:nokey=1 -select_streams 0 -show_entries format=duration:stream=avg_frame_rate,width,height,display_aspect_ratio \"" + videoFile + "\" > \"" + outputFile + "\"";
                Process p = Process.Start("CMD.exe", arg);
                p.WaitForExit();
            }
            catch
            {

            }
        }
    }
    public static class PitchHelper
    {
        public static float[] GeneratePTC(float[] input)
        {
            Tuple<List<float[]>, int[], List<int>> t = Granularize(input);

            float[] Output = new float[input.Length];

            float period = 48000 / 440.000f;
            float place = 0.9f;
            float max = 0;
            float pos = 0;

            for (int i = 0; i < input.Length; i++)
            {
                pos += 1;
                place += 1 / period;

                if (place > 1)
                {
                    place %= 1;
                    if ((int)pos >= 0 && (int)pos < input.Length)
                    {
                        float[] f = t.Item1[t.Item2[(int)pos]];
                        for (int j = 0; j < f.Length; j++)
                        {
                            if (i + j - f.Length / 2 < Output.Length && i + j - f.Length / 2 >= 0)
                            {
                                Output[i + j - f.Length / 2] += f[j];
                                if (Math.Abs(Output[i + j - f.Length / 2]) > max) max = Math.Abs(Output[i + j - f.Length / 2]);
                            }
                        }
                    }
                }
            }
            max *= 2f;
            for (int i = 0; i < input.Length; i++)
            {
                Output[i] /= max;
            }
            return Output;
        }
        public static Tuple<List<float[]>, List<int>> GenerateFMT(float[] input)
        {
            Tuple<List<float[]>, int[], List<int>> t = Granularize(input);
            return new Tuple<List<float[]>, List<int>>(t.Item1, t.Item3);
            
        }
        public static float[] GeneratePAD(float[] input, int inputPos, float detune)
        {
            int padSize = 262144;

            //input band width
            float bandwith = detune * 2 + 4;

            float[] padBuffer = new float[padSize * 2];

            float[] ana = new float[8192 * 2];

            for (int i = 0; i < 8192; i++)
            {
                if (i + inputPos < input.Length) ana[i * 2] = input[inputPos + i];
            }
            //FFT
            ShortTimeFourierTransform(ana, 8192, -1);
            for (int i = 0; i < 8192; i++)
            {
                ana[i * 2] = (float)Math.Sqrt(ana[i * 2] * ana[i * 2] + ana[i * 2 + 1] * ana[i * 2 + 1]);
            }
            int maxSalI = 0;
            float maxSal = 0;
            for (int i = 60; i < 4000; i++)
            {
                float prod = 1;
                for (int j = 1; j < 6; j++)
                {
                    int ind = j * i / 5;
                    prod *= (ana[(ind - 1) * 2] + ana[(ind - 0) * 2] + ana[(ind + 1) * 2]);
                }
                if (prod > maxSal)
                {
                    maxSal = prod;
                    maxSalI = i;
                }
            }
            float[] mags = new float[128];
            float maxSal2 = maxSalI / 5f;

            for (int i = 1; i < 129; i++)
            {
                int ind = (int)(maxSal2 * i);
                if (ind == 0) break;
                if ((ind + 1) * 2 >= ana.Length) break;
                mags[i - 1] = ana[(ind - 1) * 2] + ana[(ind - 0) * 2] + ana[(ind + 1) * 2];

                float max = 0;
                for (int j = -2; j < 3; j++)
                {
                    if ((ind + j) * 2 < ana.Length) if (ana[(ind + j) * 2] > max)
                    {
                        max = ana[(ind + j) * 2];
                        maxSal2 = (float)(ind + j) / i;
                    }

                }
            }

            //VELKE FFT
            Random RNG = new Random();
            float profile(float fi, float bwi)
            {
                float x = fi / bwi;
                return (float)(Math.Exp(-x * x) / bwi);
            };
            for (int i = 0; i < 64; i++)
            {

                float bw_Hz = (float)((Math.Pow(2, bandwith / 1200) - 1.0) * 220 * (i + 1));
                float bwi = bw_Hz / (2.0f * 48000);
                float fi = 220 * (i + 1) / 48000f;
                for (int j = 0; j < padSize / 2; j++)
                {
                    float hprofile = profile((j / (float)padSize) - fi, bwi);

                    padBuffer[j * 2] = padBuffer[j * 2] + hprofile * mags[i];
                    /*padBuffer[j * 2 + 1] = (float)Math.Cos(f) * padBuffer[j * 2 + 1] + hprofile * mags[i];*/
                }
            }
            for (int j = 0; j < padSize / 2; j++)
            {
                float f = (float)(RNG.NextDouble() * Math.PI * 2);
                padBuffer[j * 2 + 1] = (float)Math.Cos(f) * padBuffer[j * 2 + 0];
                padBuffer[j * 2] *= (float)Math.Sin(f);
            }
            ShortTimeFourierTransform(padBuffer, padSize, 1);
            float[] padBuffer2 = new float[padSize];

            float m2 = 0;
            for (int i = 0; i < padSize; i++)
            {
                padBuffer2[i] = padBuffer[i * 2];
                if (Math.Abs(padBuffer2[i]) > m2) m2 = Math.Abs(padBuffer2[i]);
            }
            for (int i = 0; i < padSize; i++)
            {
                padBuffer2[i] /= m2;
            }

            return padBuffer2;
        }

        public static float[] GetMonoL(IPitchReader ipr, int lenght)
        {
            float[] f = new float[2 * lenght];

            ipr.ReadAdd(ref f, 1, 0);
            float[] g = new float[lenght];
            for (int i = 0; i < lenght; i++) g[i] = f[2 * i];

            return g;
        }

        public static Tuple<List<float[]>,int[], List<int>> Granularize(float[] input)
        {
            float max = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (Math.Abs(input[i]) > max)
                {
                    max = Math.Abs(input[i]);
                }

            }
            for (int i = 0; i < input.Length; i++)
            {
                input[i] /= max;
            }

            // input holds selected audio.

            int[] pitch = new int[input.Length / 512];
            int[] GranuleIndex = new int[input.Length];

            // gets autocorelation of AUDIO against itself to get estimated pitch at various time points spread by 512 samples.
            int numn1 = 0;
            for (int i = 0; i < input.Length / 512; i++)
            {
                if (numn1 <= 5) pitch[i] = Autocorelation(ref input, 512 * i, 512, 2048);
                else pitch[i] = -1;
                if (pitch[i] == -1) numn1++;
                else numn1 = 0;
            }

            // try to detect and fix "octave" errors.

            float[] pitch2 = new float[input.Length / 512];

            for (int i = 0; i < input.Length / 512; i++)
            {
                List<int> pp = new List<int>();
                for (int j = -7 / 2; j < 7 / 2 + 1; j++) if (j + i >= 0 && j + i < input.Length / 512) if (pitch[j + i] > -1) pp.Add(pitch[j + i]);
                if (pp.Count == 0) { pitch2[i] = -1; continue; }
                pp.Sort();

                int ppp = pp[pp.Count / 2];
                List<int> cand = new List<int>();

                int newp = -1;
                int minDiff = int.MaxValue;

                for (int j = 1; j < 5; j++)
                {
                    for (int k = 1; k < 5; k++)
                    {
                        if (Math.Abs(ppp - pitch[i] * j / k) < minDiff)
                        {
                            newp = pitch[i] * j / k;
                            minDiff = Math.Abs(ppp - pitch[i] * j / k);
                        }
                    }
                }

                pitch2[i] = newp;
                if (newp == 0) pitch2[i] = -1;
            }

            // pitch2 is now used to find "pitch points" (ptchpt) requered for "PSOLA"

            int firstPitchIndex = -1;
            for (int i = 0; i < pitch2.Length; i++)
            {
                if (pitch2[i] > -1)
                {
                    firstPitchIndex = i;
                    break;
                }
            }

            if (firstPitchIndex == -1) return new Tuple<List<float[]>, int[], List<int>>( new List<float[]>(), new int[0], new List<int>());

            List<int> ptchpt = new List<int>();

            max = 0;
            int maxi = -1;

            int jj = 0;
            while (maxi == -1)
            {

                for (int i = firstPitchIndex * 512 + jj * (int)pitch2[firstPitchIndex]; i < pitch2[firstPitchIndex] * (jj + 1) + firstPitchIndex * 512; i++)
                {
                    if (i > input.Length) break;
                    if (input[i] > max)
                    {
                        max = input[i];
                        maxi = i;
                    }
                }
                jj++;
            }

            float last = 100;
            for (int i = 0; i < pitch2.Length; i++)
            {
                if (pitch2[i] == -1 || pitch2[i] == 0) pitch2[i] = last;
                else last = pitch2[i];

                if (pitch2[i] < 20) pitch2[i] = 20;
            }

            float averagePitch = 0;
            int count = 16;

            for (int i = 8; i < 24; i++)
            {
                if (pitch2[i] == 20) count--;
                else averagePitch += (pitch2[i]);
            }
            averagePitch /= count;
            averagePitch = 48000f / averagePitch;

            // assume max deviation from detected pitch by 1/5

            if (maxi != -1)
            {
                ptchpt.Add(maxi);
                int lasti = maxi;

                while (true)
                {
                    float m = (lasti % 512) / 512f;
                    int p = lasti / 512;
                    if (p > input.Length / 512 - 2) break;
                    int dist = (int)((1 - m) * pitch2[p] + m * pitch2[p + 1]);
                    int dist0 = dist * 6 / 5;
                    int dist1 = dist * 4 / 5;

                    if (dist <= 0) break;

                    max = 0;
                    maxi = lasti + dist;
                    for (int i = lasti + dist1; i < lasti + dist0; i++)
                    {
                        if (i >= input.Length) break;
                        if (input[i] > max)
                        {
                            max = input[i];
                            maxi = i;
                        }

                    }
                    ptchpt.Add(maxi);
                    lasti = maxi;
                }
            }

            int posin = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (posin < ptchpt.Count - 1) if (i > (ptchpt[posin] + ptchpt[posin + 1]) / 2) posin++;
                GranuleIndex[i] = posin;
            }

            List<float[]> Granules = new List<float[]>();

            Granules.Clear();

            for (int i = 0; i < ptchpt.Count; i++)
            {
                int size = 0;
                if (i == 0) size = 2 * (ptchpt[1] - ptchpt[0]);
                else if (i == ptchpt.Count - 1) size = 2 * (ptchpt[ptchpt.Count - 1] - ptchpt[ptchpt.Count - 2]);
                else size = ptchpt[i + 1] - ptchpt[i - 1];

                float[] g = new float[size];
                for (int j = 0; j < size; j++)
                {
                    float vol = (float)Math.Sin(j * Math.PI / size);
                    if (ptchpt[i] - size / 2 + j > 0 && ptchpt[i] - size / 2 + j < input.Length) g[j] = input[ptchpt[i] - size / 2 + j] * vol * vol;
                }
                Granules.Add(g);
            }

            return new Tuple<List<float[]>, int[], List<int>>(Granules, GranuleIndex, ptchpt);
        }
        public static int Autocorelation(ref float[] input, int startPos, int searchSize, int window)
        {
            float[] array = new float[window];

            for (int i = 0; i < searchSize; i++)
            {
                array[i] = 0;

                for (int j = 0; j < window; j++)
                {
                    if (startPos + j + i < input.Length) array[i] += input[startPos + j] * input[startPos + j + i];
                }
            }

            float maxMagn = 0;
            int L = -1;

            for (int i = 1; i < array.Length - 1; i++)
            {
                if (array[i] > array[i + 1] && array[i] >= array[i - 1]) if (array[i] > maxMagn)
                    {
                        maxMagn = array[i];
                        L = i;
                    }
            }

            return L;
        }
        public static void ShortTimeFourierTransform(float[] fftBuffer, long fftFrameSize, long sign)
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

        public static List<Tuple<int, float>>[] ExtractPitchInfo(float[] input)
        {
            float max = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (Math.Abs(input[i]) > max)
                {
                    max = Math.Abs(input[i]);
                }
            }
            if (max == 0) return null;
            for (int i = 0; i < input.Length; i++)
            {
                input[i] /= max;
            }

            // input holds selected audio.

            List<Tuple<int, float>>[] pitch = new List<Tuple<int, float>>[input.Length / 480];
            int[] GranuleIndex = new int[input.Length];

            // gets autocorelation of AUDIO against itself to get estimated pitch at various time points spread by 480 samples.
            for (int i = 0; i < input.Length / 480; i++)
            {
                pitch[i] = AutocorelationPeaks(ref input, 480 * i, 960, 960);
            }

            return pitch;
        }
        public static List<Tuple<int, float>> AutocorelationPeaks(ref float[] input, int startPos, int searchSize, int window)
        {
            float[] array = new float[searchSize];
            float[] array2 = new float[searchSize];

            for (int i = 0; i < searchSize; i++)
            {
                array[i] = 0;

                for (int j = 0; j < window; j++)
                {
                    if (startPos + j + i < input.Length) array[i] += input[startPos + j] * input[startPos + j + i];
                }
            }

            List<Tuple<int, float>> Peaks = new List<Tuple<int, float>>();
            if (array[0] == 0) return Peaks;
            for (int i = 1; i < array.Length; i++)
            {
                array[i] /= array[0];
            }
            array[0] = 1;

            for (int i = 1; i < array.Length - 1; i++)
            {
                if (array[i] >= array[i - 1] && array[i] >= array[i + 1] && array[i] > 0)
                {
                    Peaks.Add(new Tuple<int, float>(i, array[i]));
                }
            }

            return Peaks;
        }
    }

    public class IPRtoISP : ISampleProvider
    {
        public WaveFormat WaveFormat => WaveFormat.CreateIeeeFloatWaveFormat(48000,2);

        IPitchReader ipr = null;
        public IPRtoISP(IPitchReader ipr)
        {
            this.ipr = ipr;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            if (ipr != null)
            {
                ipr.ReadAdd(ref buffer, 1, 1);
            }
            return count;
        }
    }

    public static class PathHelper
    {
        public static string CurrentDirectory() => Environment.CurrentDirectory;
    }
}

namespace FFMPEG
{
    public sealed unsafe class VideoStreamDecoder : IDisposable
    {
        public readonly AVCodecContext* _pCodecContext;
        public readonly AVFormatContext* _pFormatContext;
        private readonly int _streamIndex;
        private readonly AVFrame* _pFrame;
        private readonly AVPacket* _pPacket;
        public readonly AVStream* pStream;

        public VideoStreamDecoder(string url)
        {
            _pFormatContext = ffmpeg.avformat_alloc_context();

            var pFormatContext = _pFormatContext;
            ffmpeg.avformat_open_input(&pFormatContext, url, null, null).ThrowExceptionIfError();

            ffmpeg.avformat_find_stream_info(_pFormatContext, null).ThrowExceptionIfError();

            // find the first video stream
            /*AVStream*/
            pStream = null;
            for (var i = 0; i < _pFormatContext->nb_streams; i++)
                if (_pFormatContext->streams[i]->codec->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
                {
                    pStream = _pFormatContext->streams[i];
                    break;
                }

            if (pStream == null) throw new InvalidOperationException("Could not found video stream.");

            _streamIndex = pStream->index;
            _pCodecContext = pStream->codec;

            var codecId = _pCodecContext->codec_id;
            var pCodec = ffmpeg.avcodec_find_decoder(codecId);
            if (pCodec == null) throw new InvalidOperationException("Unsupported codec.");

            ffmpeg.avcodec_open2(_pCodecContext, pCodec, null).ThrowExceptionIfError();
            //long dfn = ffmpeg.av_rescale(10*600000, pFormatContext->streams[0]->time_base.den, pFormatContext->streams[0]->time_base.num);
            //dfn /= 1000;
            //ffmpeg.av_seek_frame(_pFormatContext, -1, 660_000000, (int)SeekOrigin.Begin);

            CodecName = ffmpeg.avcodec_get_name(codecId);
            FrameSize = new Size(_pCodecContext->width, _pCodecContext->height);
            PixelFormat = _pCodecContext->pix_fmt;

            _pPacket = ffmpeg.av_packet_alloc();
            _pFrame = ffmpeg.av_frame_alloc();
        }

        public string CodecName { get; }
        public Size FrameSize { get; }
        public AVPixelFormat PixelFormat { get; }

        bool disposed = false;

        public void Dispose()
        {
            disposed = true;

            ffmpeg.av_frame_unref(_pFrame);
            ffmpeg.av_free(_pFrame);

            ffmpeg.av_packet_unref(_pPacket);
            ffmpeg.av_free(_pPacket);

            ffmpeg.avcodec_close(_pCodecContext);
            var pFormatContext = _pFormatContext;
            ffmpeg.avformat_close_input(&pFormatContext);
        }

        public bool TryDecodeNextFrame(out AVFrame frame)
        {

            if (disposed)
            {
                frame = new AVFrame();
                return false;
            }
            ffmpeg.av_frame_unref(_pFrame);

            int error;
            do
            {
                try
                {
                    do
                    {
                        error = ffmpeg.av_read_frame(_pFormatContext, _pPacket);
                        if (error == ffmpeg.AVERROR_EOF)
                        {
                            frame = *_pFrame;
                            return false;
                        }

                        error.ThrowExceptionIfError();
                    } while (_pPacket->stream_index != _streamIndex);

                    ffmpeg.avcodec_send_packet(_pCodecContext, _pPacket).ThrowExceptionIfError();
                }
                finally
                {
                    ffmpeg.av_packet_unref(_pPacket);
                }

                error = ffmpeg.avcodec_receive_frame(_pCodecContext, _pFrame);
            } while (error == ffmpeg.AVERROR(ffmpeg.EAGAIN));

            error.ThrowExceptionIfError();
            frame = *_pFrame;
            return true;
        }

        public IReadOnlyDictionary<string, string> GetContextInfo()
        {
            AVDictionaryEntry* tag = null;
            var result = new Dictionary<string, string>();
            while ((tag = ffmpeg.av_dict_get(_pFormatContext->metadata, "", tag, ffmpeg.AV_DICT_IGNORE_SUFFIX)) != null)
            {
                var key = Marshal.PtrToStringAnsi((IntPtr)tag->key);
                var value = Marshal.PtrToStringAnsi((IntPtr)tag->value);
                result.Add(key, value);
            }

            return result;
        }
    }
    static class FFmpegHelper
    {
        public static unsafe string av_strerror(int error)
        {
            var bufferSize = 1024;
            var buffer = stackalloc byte[bufferSize];
            ffmpeg.av_strerror(error, buffer, (ulong)bufferSize);
            var message = Marshal.PtrToStringAnsi((IntPtr)buffer);
            return message;
        }

        public static int ThrowExceptionIfError(this int error)
        {
            if (error < 0) throw new ApplicationException(av_strerror(error));
            return error;
        }
    }
    public class FFmpegBinariesHelper
    {
        private const string LD_LIBRARY_PATH = "LD_LIBRARY_PATH";

        internal static void RegisterFFmpegBinaries()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                    var current = Environment.CurrentDirectory;
                    var probe = Path.Combine("FFmpeg", "bin", Environment.Is64BitProcess ? "x64" : "x86");
                    while (current != null)
                    {
                        var ffmpegDirectory = Path.Combine(current, probe);
                        if (Directory.Exists(ffmpegDirectory))
                        {
                            Console.WriteLine($"FFmpeg binaries found in: {ffmpegDirectory}");
                            RegisterLibrariesSearchPath(ffmpegDirectory);
                            return;
                        }
                        current = Directory.GetParent(current)?.FullName;
                    }
                    MessageBox.Show("Error loading FFMPEG. All video playing will be anavailable.");
                    break;
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    var libraryPath = Environment.GetEnvironmentVariable(LD_LIBRARY_PATH);
                    RegisterLibrariesSearchPath(libraryPath);
                    break;
            }
        }
        private static void RegisterLibrariesSearchPath(string path)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                    SetDllDirectory(path);
                    break;
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    string currentValue = Environment.GetEnvironmentVariable(LD_LIBRARY_PATH);
                    if (string.IsNullOrWhiteSpace(currentValue) == false && currentValue.Contains(path) == false)
                    {
                        string newValue = currentValue + Path.PathSeparator + path;
                        Environment.SetEnvironmentVariable(LD_LIBRARY_PATH, newValue);
                    }
                    break;
            }
        }

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);
    }
    public sealed unsafe class VideoFrameConverter : IDisposable
    {
        private readonly IntPtr _convertedFrameBufferPtr;
        private readonly Size _destinationSize;
        private readonly byte_ptrArray4 _dstData;
        private readonly int_array4 _dstLinesize;
        private readonly SwsContext* _pConvertContext;

        public VideoFrameConverter(Size sourceSize, AVPixelFormat sourcePixelFormat,
            Size destinationSize, AVPixelFormat destinationPixelFormat)
        {
            _destinationSize = destinationSize;

            _pConvertContext = ffmpeg.sws_getContext(sourceSize.Width, sourceSize.Height, sourcePixelFormat,
            destinationSize.Width,
            destinationSize.Height, destinationPixelFormat,
            ffmpeg.SWS_FAST_BILINEAR, null, null, null);
            if (_pConvertContext == null) throw new ApplicationException("Could not initialize the conversion context.");

            var convertedFrameBufferSize = ffmpeg.av_image_get_buffer_size(destinationPixelFormat, destinationSize.Width, destinationSize.Height, 1);
            _convertedFrameBufferPtr = Marshal.AllocHGlobal(convertedFrameBufferSize);
            _dstData = new byte_ptrArray4();
            _dstLinesize = new int_array4();

            ffmpeg.av_image_fill_arrays(ref _dstData, ref _dstLinesize, (byte*)_convertedFrameBufferPtr, destinationPixelFormat, destinationSize.Width, destinationSize.Height, 1);

            //int[] a = _dstLinesize.ToArray();
            //a[0] = 1350;
            //a[1] = 2;
            //a[2] = 0;
            //a[3] = 0;
            //_dstLinesize.UpdateFrom(a);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(_convertedFrameBufferPtr);
            ffmpeg.sws_freeContext(_pConvertContext);
        }

        public AVFrame Convert(AVFrame sourceFrame)
        {
            ffmpeg.sws_scale(_pConvertContext, sourceFrame.data, sourceFrame.linesize, 0, sourceFrame.height, _dstData, _dstLinesize);

            var data = new byte_ptrArray8();
            data.UpdateFrom(_dstData);
            var linesize = new int_array8();
            linesize.UpdateFrom(_dstLinesize);

            return new AVFrame
            {
                data = data,
                linesize = linesize,
                width = _destinationSize.Width,
                height = _destinationSize.Height
            };
        }
    }
    public sealed unsafe class VideoConverter : IDisposable
    {
        private readonly SwsContext* _pConvertContext;

        public VideoConverter(Size sourceSize, AVPixelFormat sourcePixelFormat,
            Size destinationSize, AVPixelFormat destinationPixelFormat)
        {
            _pConvertContext = ffmpeg.sws_getContext(sourceSize.Width, sourceSize.Height, sourcePixelFormat,
                destinationSize.Width,
                destinationSize.Height, destinationPixelFormat,
                ffmpeg.SWS_FAST_BILINEAR, null, null, null);
            if (_pConvertContext == null) throw new ApplicationException("Could not initialize the conversion context.");

            var convertedFrameBufferSize = ffmpeg.av_image_get_buffer_size(destinationPixelFormat, destinationSize.Width, destinationSize.Height, 1);
            var convertedFrameBufferPtr = Marshal.AllocHGlobal(convertedFrameBufferSize);
            var dstData = new byte_ptrArray4();
            var dstLinesize = new int_array4();

            ffmpeg.av_image_fill_arrays(ref dstData, ref dstLinesize, (byte*)convertedFrameBufferPtr, destinationPixelFormat, destinationSize.Width, destinationSize.Height, 1);
        }

        public void Dispose()
        {
        }

        public AVFrame Convert(AVFrame sourceFrame)
        {
            var dstData = new byte_ptrArray4();
            var dstLinesize = new int_array4();

            ffmpeg.sws_scale(_pConvertContext, sourceFrame.data, sourceFrame.linesize, 0, sourceFrame.height, dstData, dstLinesize);

            return new AVFrame();
        }
    }
    public sealed unsafe class H264VideoStreamEncoder : IDisposable
    {
        private readonly Size _frameSize;
        private readonly int _linesizeU;
        private readonly int _linesizeV;
        private readonly int _linesizeY;
        private readonly AVCodec* _pCodec;
        private readonly AVCodecContext* _pCodecContext;
        private readonly Stream _stream;
        private readonly int _uSize;
        private readonly int _ySize;

        static H264VideoStreamEncoder()
        {
            ffmpeg.avcodec_register_all();
        }

        public H264VideoStreamEncoder(Stream stream, int fps, Size frameSize)
        {
            _stream = stream;
            _frameSize = frameSize;

            var codecId = AVCodecID.AV_CODEC_ID_H264;
            _pCodec = ffmpeg.avcodec_find_encoder(codecId);
            if (_pCodec == null) throw new InvalidOperationException("Codec not found.");

            _pCodecContext = ffmpeg.avcodec_alloc_context3(_pCodec);
            _pCodecContext->width = frameSize.Width;
            _pCodecContext->height = frameSize.Height;
            _pCodecContext->time_base = new AVRational { num = 1, den = fps };
            _pCodecContext->pix_fmt = AVPixelFormat.AV_PIX_FMT_YUV420P;
            ffmpeg.av_opt_set(_pCodecContext->priv_data, "preset", "veryslow", 0);

            ffmpeg.avcodec_open2(_pCodecContext, _pCodec, null).ThrowExceptionIfError();

            _linesizeY = frameSize.Width;
            _linesizeU = frameSize.Width / 2;
            _linesizeV = frameSize.Width / 2;

            _ySize = _linesizeY * frameSize.Height;
            _uSize = _linesizeU * frameSize.Height / 2;
        }

        public void Dispose()
        {
            ffmpeg.avcodec_close(_pCodecContext);
            ffmpeg.av_free(_pCodecContext);
            ffmpeg.av_free(_pCodec);
        }

        public void Encode(AVFrame frame)
        {
            if (frame.format != (int)_pCodecContext->pix_fmt) throw new ArgumentException("Invalid pixel format.", nameof(frame));
            if (frame.width != _frameSize.Width) throw new ArgumentException("Invalid width.", nameof(frame));
            if (frame.height != _frameSize.Height) throw new ArgumentException("Invalid height.", nameof(frame));
            if (frame.linesize[0] != _linesizeY) throw new ArgumentException("Invalid Y linesize.", nameof(frame));
            if (frame.linesize[1] != _linesizeU) throw new ArgumentException("Invalid U linesize.", nameof(frame));
            if (frame.linesize[2] != _linesizeV) throw new ArgumentException("Invalid V linesize.", nameof(frame));
            if (frame.data[1] - frame.data[0] != _ySize) throw new ArgumentException("Invalid Y data size.", nameof(frame));
            if (frame.data[2] - frame.data[1] != _uSize) throw new ArgumentException("Invalid U data size.", nameof(frame));

            var pPacket = ffmpeg.av_packet_alloc();
            try
            {
                int error;
                do
                {
                    ffmpeg.avcodec_send_frame(_pCodecContext, &frame).ThrowExceptionIfError();

                    error = ffmpeg.avcodec_receive_packet(_pCodecContext, pPacket);
                } while (error == ffmpeg.AVERROR(ffmpeg.EAGAIN));

                error.ThrowExceptionIfError();

                using (var packetStream = new UnmanagedMemoryStream(pPacket->data, pPacket->size)) packetStream.CopyTo(_stream);
            }
            finally
            {
                ffmpeg.av_packet_unref(pPacket);
            }
        }
    }

    public sealed unsafe class VideoReader : IDisposable
    {
        private readonly AVCodecContext* _pCodecContext;
        private readonly AVFormatContext* _pFormatContext;
        private readonly int _streamIndex;
        private readonly AVFrame* _pFrame;
        private readonly AVPacket* _pPacket;
        private readonly long TimeBase;

        public VideoReader(string url)
        {
            _pFormatContext = ffmpeg.avformat_alloc_context();

            var pFormatContext = _pFormatContext;
            ffmpeg.avformat_open_input(&pFormatContext, url, null, null).ThrowExceptionIfError();

            ffmpeg.avformat_find_stream_info(_pFormatContext, null).ThrowExceptionIfError();

            // find the first video stream
            AVStream* pStream = null;
            for (var i = 0; i < _pFormatContext->nb_streams; i++)
                if (_pFormatContext->streams[i]->codec->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
                {
                    pStream = _pFormatContext->streams[i];
                    break;
                }

            if (pStream == null) throw new InvalidOperationException("Could not found video stream.");

            _streamIndex = pStream->index;
            _pCodecContext = pStream->codec;

            TimeBase = ((long)(_pCodecContext->time_base.num) * 1000000) / (_pCodecContext->time_base.den);

            var codecId = _pCodecContext->codec_id;
            var pCodec = ffmpeg.avcodec_find_decoder(codecId);
            if (pCodec == null) throw new InvalidOperationException("Unsupported codec.");

            ffmpeg.avcodec_open2(_pCodecContext, pCodec, null).ThrowExceptionIfError();

            CodecName = ffmpeg.avcodec_get_name(codecId);
            FrameSize = new Size(_pCodecContext->width, _pCodecContext->height);
            PixelFormat = _pCodecContext->pix_fmt;

            _pPacket = ffmpeg.av_packet_alloc();
            _pFrame = ffmpeg.av_frame_alloc();
        }

        public string CodecName { get; }
        public Size FrameSize { get; }
        public AVPixelFormat PixelFormat { get; }

        public void Dispose()
        {
            ffmpeg.av_frame_unref(_pFrame);
            ffmpeg.av_free(_pFrame);

            ffmpeg.av_packet_unref(_pPacket);
            ffmpeg.av_free(_pPacket);

            ffmpeg.avcodec_close(_pCodecContext);
            var pFormatContext = _pFormatContext;
            ffmpeg.avformat_close_input(&pFormatContext);
        }

        public bool TryDecodeNextFrame(out AVFrame frame)
        {
            ffmpeg.av_frame_unref(_pFrame);
            int error;
            do
            {
                try
                {
                    do
                    {
                        error = ffmpeg.av_read_frame(_pFormatContext, _pPacket);
                        if (error == ffmpeg.AVERROR_EOF)
                        {
                            frame = *_pFrame;
                            return false;
                        }

                        error.ThrowExceptionIfError();
                    } while (_pPacket->stream_index != _streamIndex);

                    ffmpeg.avcodec_send_packet(_pCodecContext, _pPacket).ThrowExceptionIfError();
                }
                finally
                {
                    ffmpeg.av_packet_unref(_pPacket);
                }

                error = ffmpeg.avcodec_receive_frame(_pCodecContext, _pFrame);
            } while (error == ffmpeg.AVERROR(ffmpeg.EAGAIN));

            error.ThrowExceptionIfError();
            frame = *_pFrame;
            CurrentFrameIndex++;
            return true;
        }
        private bool TryDecodeNextFrame()
        {
            ffmpeg.av_frame_unref(_pFrame);
            int error;
            do
            {
                try
                {
                    do
                    {
                        error = ffmpeg.av_read_frame(_pFormatContext, _pPacket);
                        if (error == ffmpeg.AVERROR_EOF)
                        {
                            return false;
                        }

                        error.ThrowExceptionIfError();
                    } while (_pPacket->stream_index != _streamIndex);

                    ffmpeg.avcodec_send_packet(_pCodecContext, _pPacket).ThrowExceptionIfError();
                }
                finally
                {
                    ffmpeg.av_packet_unref(_pPacket);
                }

                error = ffmpeg.avcodec_receive_frame(_pCodecContext, _pFrame);
            } while (error == ffmpeg.AVERROR(ffmpeg.EAGAIN));

            error.ThrowExceptionIfError();
            CurrentFrameIndex++;
            return true;
        }

        public IReadOnlyDictionary<string, string> GetContextInfo()
        {
            AVDictionaryEntry* tag = null;
            var result = new Dictionary<string, string>();
            while ((tag = ffmpeg.av_dict_get(_pFormatContext->metadata, "", tag, ffmpeg.AV_DICT_IGNORE_SUFFIX)) != null)
            {
                var key = Marshal.PtrToStringAnsi((IntPtr)tag->key);
                var value = Marshal.PtrToStringAnsi((IntPtr)tag->value);
                result.Add(key, value);
            }

            return result;
        }

        private long CurrentFrameIndex = 0;
        public bool Seek(long FrameIndex)
        {
            if (_pFormatContext == null) return false;

            if (FrameIndex == CurrentFrameIndex) return true;
            int Flags = 8; //AVSEEK_FLAG_FRAME
            if (FrameIndex < CurrentFrameIndex) Flags |= 1; //AVSEEK_FLAG_BACKWARD - BYTE-ANY-2-4

            long SeekTarget = ffmpeg.av_rescale_q(FrameIndex, new AVRational() { num = 1, den = ffmpeg.AV_TIME_BASE }, _pFormatContext->streams[0]->time_base);


            //if (ffmpeg.av_seek_frame(_pFormatContext, -1, SeekTarget, 5) < 0) return false;
            if (ffmpeg.av_seek_frame(_pFormatContext, -1, SeekTarget, 0) < 0) return false;

            ffmpeg.avcodec_flush_buffers(_pCodecContext);

            long l = _pFormatContext->pb->pos;

            TryDecodeNextFrame();

            return true;
        }

        private void GetIFramesList()
        {
            // ffprobe -select_streams v -show_frames -show_entries frame=pkt_pts_time,pict_type -v quiet <INPUT> > D:\\output.txt 2>&1
            // "D:\\Media\LLL - nová 666\LLLP.mp4"
        }
    }
}
