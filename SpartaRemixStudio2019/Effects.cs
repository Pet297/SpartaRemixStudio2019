using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;

namespace SpartaRemixStudio2019
{
    public interface IAutomatable : ICloneable, IDisposable
    {
        string DisplayName { get; }
        ushort Type { get; }

        FXValue GetFloatValueReference(int index);
        string GetAutomatedValueName(int index);
        string ConvertFXFToNumber(int index, float value);
        int FXFloatValueCount { get; }

        Tuple<float, string, bool, float, float> GetValues(int index);

        bool HasEditForm { get; }
        void ShowEditForm();
        void Init();

        void UpdateValues(float timeBeat);
        void ClearInstance();

        void SaveData(FileStream fs);
        void LoadData(FileStream fs);

        int NumberTrackCount { get; }
        uint GetNumberTrackReference(int index);
        void SetNumberTrackReference(int index, uint trackIndex);
    }
    public interface IAudioFX : IAutomatable
    {
        void Apply(ref float[] audio);
    }
    public interface IVideoFX : IAutomatable
    {
        void Apply(ref TextureInfo texin);
    }
    public interface INumberFX : IAutomatable
    {
        float Apply(float value);
    }
    
    public static class EffectHelper
    {
        public static IAudioFX AFXFromNumber(ushort u)
        {
            if (u == 0) return new AFX_Lowpass();
            if (u == 1) return new AFX_Highpass();
            if (u == 2) return new AFX_LowpassResonant();
            if (u == 3) return new AFX_HighpassResonant();
            if (u == 4) return new AFX_FFComb();
            if (u == 5) return new AFX_FBComb();

            if (u == 8) return new AFX_Delay();
            if (u == 9) return new AFX_Reverb();
            //if (u == 10) return new AFX_Delay2();
            if (u == 11) return new AFX_Reverb2();

            if (u == 18) return new AFX_WahWah();
            if (u == 20) return new AFX_Flange();
            return null;
        }
        public static IVideoFX VFXFromNumber(ushort u)
        {
            if (u == 0) return new VFX_S_UVFlipX();
            if (u == 1) return new VFX_S_UVFlipY();
            if (u == 2) return new VFX_S_Opac();
            if (u == 3) return new VFX_S_UVZoom();
            if (u == 4) return new VFX_S_Multiply();
            if (u == 5) return new VFX_S_Contrast();
            if (u == 6) return new VFX_S_Brightness();
            if (u == 7) return new VFX_S_BlackAndWhite();

            if (u == 8) return new VFX_S_Invert();
            if (u == 9) return new VFX_S_GridVis();
            if (u == 10) return new VFX_S_ARZoom();
            if (u == 11) return new VFX_S_ARStretch();

            if (u == 16) return new VFX_Border();
            if (u == 17) return new VFX_DirectionalBlur();
            if (u == 18) return new VFX_DirectionalBlurMP();
            if (u == 19) return new VFX_GaussianBlur();

            if (u == 24) return new VFX_Flat();
            if (u == 25) return new VFX_FlatX();
            if (u == 26) return new VFX_FlatY();
            if (u == 27) return new VFX_FlatXY();

            if (u == 32) return new VFX_T_Txyz();
            if (u == 33) return new VFX_T_Sxyz();
            if (u == 34) return new VFX_T_Rz();
            if (u == 35) return new VFX_T_Ry();
            if (u == 36) return new VFX_T_Rx();
            return null;
        }
        public static INumberFX NFXFromNumber(ushort u)
        {
            if (u == 0) return new NFX_AutomationAdd();
            if (u == 1) return new NFX_AutomationMult();
            if (u == 2) return new NFX_LFO_Sine_Add();
            if (u == 3) return new NFX_LFO_Sine_Mult();
            if (u == 4) return new NFX_NOISE_Add();
            if (u == 5) return new NFX_NOISE_Mult();
            return null;
        }

        public static void Save(FileStream fs, IAutomatable afx)
        {
            StreamHelper.SaveBytes(fs, (int)4);
            StreamHelper.SaveBytes(fs, afx.Type);
            StreamHelper.SaveBytes(fs, afx.FXFloatValueCount);
            for (int i = 0; i < afx.FXFloatValueCount; i++)
            {
                afx.GetFloatValueReference(i).Save(fs);
            }
            StreamHelper.SaveBytes(fs, afx.NumberTrackCount);
            for (int i = 0; i < afx.NumberTrackCount; i++)
            {
                StreamHelper.SaveBytes(fs, afx.GetNumberTrackReference(i));
            }
            afx.SaveData(fs);
        }

        public static IAudioFX LoadAFX(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);

            ushort u = StreamHelper.LoadUShort(fs);
            IAudioFX afx = AFXFromNumber(u);
            lenght--;

            if (lenght > 0)
            {
                int l0 = StreamHelper.LoadInt(fs);
                for (int i = 0; i < l0; i++)
                {
                    afx.GetFloatValueReference(i).Load(fs);
                }
                lenght--;
            }
            if (lenght > 0)
            {
                int l0 = StreamHelper.LoadInt(fs);
                for (int i = 0; i < l0; i++)
                {
                    afx.SetNumberTrackReference(l0,StreamHelper.LoadUInt(fs));
                }
                lenght--;
            }
            if (lenght > 0)
            {
                afx.LoadData(fs);
                lenght--;
            }
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
            return afx;
        }
        public static IVideoFX LoadVFX(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);

            ushort u = StreamHelper.LoadUShort(fs);
            IVideoFX afx = VFXFromNumber(u);
            lenght--;

            if (lenght > 0)
            {
                int l0 = StreamHelper.LoadInt(fs);
                for (int i = 0; i < l0; i++)
                {
                    afx.GetFloatValueReference(i).Load(fs);
                }
                lenght--;
            }
            if (lenght > 0)
            {
                int l0 = StreamHelper.LoadInt(fs);
                for (int i = 0; i < l0; i++)
                {
                    afx.SetNumberTrackReference(l0, StreamHelper.LoadUInt(fs));
                }
                lenght--;
            }
            if (lenght > 0)
            {
                afx.LoadData(fs);
                lenght--;
            }
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
            return afx;
        }
        public static INumberFX LoadNFX(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);

            ushort u = StreamHelper.LoadUShort(fs);
            INumberFX afx = NFXFromNumber(u);
            lenght--;

            if (lenght > 0)
            {
                int l0 = StreamHelper.LoadInt(fs);
                for (int i = 0; i < l0; i++)
                {
                    afx.GetFloatValueReference(i).Load(fs);
                }
                lenght--;
            }
            if (lenght > 0)
            {
                int l0 = StreamHelper.LoadInt(fs);
                for (int i = 0; i < l0; i++)
                {
                    afx.SetNumberTrackReference(l0, StreamHelper.LoadUInt(fs));
                }
                lenght--;
            }
            if (lenght > 0)
            {
                afx.LoadData(fs);
                lenght--;
            }
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
            return afx;
        }

        public static void GetNumberTracks(IAutomatable ia, List<uint> list)
        {
            for (int i = 0; i < ia.NumberTrackCount; i++)
            {
                list.Add(ia.GetNumberTrackReference(i));
            }
            for (int i = 0; i < ia.FXFloatValueCount; i++)
            {
                foreach(INumberFX nfx in ia.GetFloatValueReference(i).Nfx)
                GetNumberTracks(nfx, list);
            }
        }

        public static TextureInfo Flatten(TextureInfo ti, RenderTarget rt)
        {
            if (ti.IncludesPreprocessing)
            {
                rt.Use();

                Matrix4 m4 = Matrix4.CreateTranslation(0, 0, -1f) * Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 2, rt.Width / (float)rt.Height, 0.01f, 1000f);
                GL.ClearColor(0, 0, 0, 0);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                GLDraw.DrawVideoDefault(ti, rt, m4);
                ti = new TextureInfo(rt.TextureIndex, rt.Width, rt.Height);
            }
            return ti;
        }
        public static TextureInfo Flatten(TextureInfo ti, RenderTarget rt, bool infX, bool infY)
        {
            if (ti.IncludesPreprocessing)
            {
                rt.Use();

                Matrix4 m4 = Matrix4.CreateTranslation(0, 0, -1f) * Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 2, rt.Width / (float)rt.Height, 0.01f, 1000f);
                GL.ClearColor(0, 0, 0, 0);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                GLDraw.DrawVideoDefault(ti, rt, m4, infX, infY);
                ti = new TextureInfo(rt.TextureIndex, rt.Width, rt.Height);
            }
            return ti;
        }

        static EffectHelper()
        {
            int VertexSource = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexSource, VertexDef);
            GL.CompileShader(VertexSource);
            int VertexSource17 = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexSource17, VertexDef);
            GL.CompileShader(VertexSource17);

            int FragmentSource16 = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentSource16, Fragment16);
            GL.CompileShader(FragmentSource16);
            shader16 = GL.CreateProgram();
            GL.AttachShader(shader16, VertexSource);
            GL.AttachShader(shader16, FragmentSource16);
            GL.LinkProgram(shader16);

            int FragmentSourceBlur = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentSourceBlur, FragmentBlur);
            GL.CompileShader(FragmentSourceBlur);
            string sl = GL.GetShaderInfoLog(FragmentSourceBlur);
            shaderBlur = GL.CreateProgram();
            GL.AttachShader(shaderBlur, VertexSource17);
            GL.AttachShader(shaderBlur, FragmentSourceBlur);
            GL.LinkProgram(shaderBlur);
            string sl2 = GL.GetProgramInfoLog(shaderBlur);
            string sl3 = GL.GetShaderInfoLog(shaderBlur);
        }
        public static int shader16;
        public static int shaderBlur;

        public static string VertexDef = @"
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
        public static string Fragment16 = @"
#version 430 core

uniform sampler2D texture;
layout(location = 161) uniform vec4 colorB;
layout(location = 162) uniform float width;
layout(location = 163) uniform float aspect;

in vec2 uv;

out vec4 fragment;

void main(void)
{
    if ((uv.x > width) && (uv.x < 1-width) && (uv.y * aspect > width) && ((uv.y * aspect) < (aspect-width)))
    {
        fragment = texture2D(texture, uv);
    }
    else
    {   
        fragment = colorB;
    }
}
";

        public static string FragmentBlur = @"
#version 430 core

uniform sampler2D texture;
layout(location = 171) uniform float widthDif;
layout(location = 172) uniform float heightDif;

layout(location = 180) uniform float k0;
layout(location = 181) uniform float km1;
layout(location = 182) uniform float km2;
layout(location = 183) uniform float km3;
layout(location = 184) uniform float km4;
layout(location = 185) uniform float km5;
layout(location = 186) uniform float km6;
layout(location = 187) uniform float km7;

layout(location = 179) uniform float kp1;
layout(location = 178) uniform float kp2;
layout(location = 177) uniform float kp3;
layout(location = 176) uniform float kp4;
layout(location = 175) uniform float kp5;
layout(location = 174) uniform float kp6;
layout(location = 173) uniform float kp7;

in vec2 uv;

out vec4 fragment;

void main(void)
{
    fragment = km7 * texture2D(texture, uv - 7 * vec2(widthDif,heightDif)) +
	km6 * texture2D(texture, uv - 6 * vec2(widthDif,heightDif)) +
	km5 * texture2D(texture, uv - 5 * vec2(widthDif,heightDif)) +
	km4 * texture2D(texture, uv - 4 * vec2(widthDif,heightDif)) +
	km3 * texture2D(texture, uv - 3 * vec2(widthDif,heightDif)) +
	km2 * texture2D(texture, uv - 2 * vec2(widthDif,heightDif)) +
	km1 * texture2D(texture, uv - 1 * vec2(widthDif,heightDif)) +
	k0 * texture2D(texture, uv) +
	kp1 * texture2D(texture, uv + 1 * vec2(widthDif,heightDif)) +
	kp2 * texture2D(texture, uv + 2 * vec2(widthDif,heightDif)) +
	kp3 * texture2D(texture, uv + 3 * vec2(widthDif,heightDif)) +
	kp4 * texture2D(texture, uv + 4 * vec2(widthDif,heightDif)) +
	kp5 * texture2D(texture, uv + 5 * vec2(widthDif,heightDif)) +
	kp6 * texture2D(texture, uv + 6 * vec2(widthDif,heightDif)) +
	kp7 * texture2D(texture, uv + 7 * vec2(widthDif,heightDif));	
}
";
    }

    public class FXValue : ICloneable, IDisposable
    {
        public float BaseValue;
        public List<INumberFX> Nfx = new List<INumberFX>();

        public object Clone()
        {
            FXValue f = new FXValue(BaseValue);
            foreach (INumberFX nfx in Nfx)
            {
                f.Nfx.Add(nfx.Clone() as INumberFX);
            }
            return f;
        }
        public void Dispose()
        {
            foreach(INumberFX nfx in Nfx)
            {
                nfx.Dispose();
            }
            Nfx.Clear();
        }
        public float GetValue(float timeBeat)
        {
            float f = BaseValue;
            foreach (INumberFX nfx in Nfx)
            {
                nfx.UpdateValues(timeBeat);
                f = nfx.Apply(f);
            }
            return f;
        }

        public FXValue(float baseVal)
        {
            BaseValue = baseVal;
            Nfx = new List<INumberFX>();
        }

        public void Save(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)2);

            StreamHelper.SaveBytes(fs, BaseValue);
            StreamHelper.SaveBytes(fs, Nfx.Count);
            for (int i = 0; i < Nfx.Count; i++)
            {
                EffectHelper.Save(fs, Nfx[i]);
            }
        }
        public void Load(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { BaseValue = StreamHelper.LoadFloat(fs); lenght--; }
            if (lenght > 0)
            {
                int l0 = StreamHelper.LoadInt(fs);
                for (int i = 0; i < l0; i++)
                {
                    Nfx.Add(EffectHelper.LoadNFX(fs));
                }
                lenght--;
            }
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }
    }

    // AFX - 00
    public class AFX_Lowpass : IAudioFX
    {
        public string DisplayName => "LOW-PASS";
        public ushort Type => 0;
        LowPassFilter filter = new LowPassFilter();

        FXValue[] FXValues = new FXValue[1];
        float normVal0;

        public int FXFloatValueCount => FXValues.Length;

        public void Apply(ref float[] audio)
        {
            filter.ProccessL(ref audio, normVal0);
            filter.ProccessR(ref audio, normVal0);
        }
        public void ClearInstance()
        {
            filter.Clear();
        }
        public object Clone()
        {
            AFX_Lowpass clone = new AFX_Lowpass();
            for (int i = 0; i < FXFloatValueCount; i++) clone.FXValues[i] = (FXValue)FXValues[i].Clone();
            return clone;
        }
        public void Dispose()
        {
            filter = null;
        }

        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Frequency (LOG, Hz)";
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, -0.5f, 1.5f);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return ((float)(20 * Math.Pow(1000,value))).ToString("00000.0") + " Hz";
            return "[WRONG NUMBER]";
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }

        public void Init() {}
        public void ShowEditForm() {}
        public void UpdateValues(float timeBeat)
        {
            normVal0 = (float)(20 * Math.Pow(1000, FXValues[0].GetValue(timeBeat)));
        }

        public AFX_Lowpass()
        {
            FXValues[0] = new FXValue(0.447f);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex){}
    }
    public class AFX_Highpass : IAudioFX
    {
        public string DisplayName => "HIGH-PASS";
        public ushort Type => 1;
        HighPassFilter filter = new HighPassFilter();

        FXValue[] FXValues = new FXValue[1];
        float normVal0;

        public int FXFloatValueCount => FXValues.Length;

        public void Apply(ref float[] audio)
        {
            filter.ProccessL(ref audio, normVal0);
            filter.ProccessR(ref audio, normVal0);
        }
        public void ClearInstance()
        {
            filter.Clear();
        }
        public object Clone()
        {
            AFX_Highpass clone = new AFX_Highpass();
            for (int i = 0; i < FXFloatValueCount; i++) clone.FXValues[i] = (FXValue)FXValues[i].Clone();
            return clone;
        }
        public void Dispose()
        {
            filter = null;
        }

        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, -0.5f, 1.5f);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Frequency (LOG, Hz)";
            return "[WRONG NUMBER]";
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return ((float)(20 * Math.Pow(1000, value))).ToString("00000.0") + " Hz";
            return "[WRONG NUMBER]";
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }

        public void Init() { }
        public void ShowEditForm() { }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = (float)(20 * Math.Pow(1000, FXValues[0].GetValue(timeBeat)));
        }

        public AFX_Highpass()
        {
            FXValues[0] = new FXValue(0.447f);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class AFX_LowpassResonant : IAudioFX
    {
        public string DisplayName => "LP-RESONANT";
        public ushort Type => 2;
        LPRes filter = new LPRes();

        FXValue[] FXValues = new FXValue[3];
        float normVal0;
        float normVal1;
        float normVal2;

        public int FXFloatValueCount => FXValues.Length;

        public void Apply(ref float[] audio)
        {
            filter.ProccessLR(ref audio, normVal0, normVal1, normVal2);
        }
        public void ClearInstance()
        {
            filter.Clear();
        }
        public object Clone()
        {
            AFX_LowpassResonant clone = new AFX_LowpassResonant();
            for (int i = 0; i < FXFloatValueCount; i++) clone.FXValues[i] = (FXValue)FXValues[i].Clone();
            return clone;
        }
        public void Dispose()
        {
            filter = null;
        }

        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Frequency (LOG, Hz)";
            if (index == 1) return "LP <-> Peak (0 <-> 1)";
            if (index == 2) return "Peak Width (0.9 - 1)";
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, -0.5f, 1.5f);
            if (index == 1) return new Tuple<float, string, bool, float, float>(0.01f, "0.00", true, 0, 1);
            if (index == 2) return new Tuple<float, string, bool, float, float>(0.01f, "0.00", true, 0, 1);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return ((float)(20 * Math.Pow(1000, value))).ToString("00000.0") + " Hz";
            if (index == 1) return (value).ToString("0.000");
            if (index == 2) return (value/10f + 0.9f).ToString("0.000");
            return "[WRONG NUMBER]";
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }

        public void Init() { }
        public void ShowEditForm() { }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = (float)(20 * Math.Pow(1000, FXValues[0].GetValue(timeBeat)));
            normVal1 = FXValues[1].GetValue(timeBeat);
            normVal2 = FXValues[2].GetValue(timeBeat) / 10f + 0.9f;

            normVal1 = Math.Min(1f, Math.Max(0, normVal2));
            normVal2 = Math.Min(0.999f, normVal2);
        }

        public AFX_LowpassResonant()
        {
            FXValues[0] = new FXValue(0.447f);
            FXValues[1] = new FXValue(0.2f);
            FXValues[2] = new FXValue(0.5f);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class AFX_HighpassResonant : IAudioFX
    {
        public string DisplayName => "HP-RESONANT";
        public ushort Type => 3;
        HPRes filter = new HPRes();

        FXValue[] FXValues = new FXValue[3];
        float normVal0;
        float normVal1;
        float normVal2;

        public int FXFloatValueCount => FXValues.Length;

        public void Apply(ref float[] audio)
        {
            filter.ProccessLR(ref audio, normVal0, normVal1, normVal2);
        }
        public void ClearInstance()
        {
            filter.Clear();
        }
        public object Clone()
        {
            AFX_HighpassResonant clone = new AFX_HighpassResonant();
            for (int i = 0; i < FXFloatValueCount; i++) clone.FXValues[i] = (FXValue)FXValues[i].Clone();
            return clone;
        }
        public void Dispose()
        {
            filter = null;
        }

        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Frequency (LOG, Hz)";
            if (index == 1) return "LP <-> Peak (0 <-> 1)";
            if (index == 2) return "Peak Width (0.9 - 1)";
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, -0.5f, 1.5f);
            if (index == 1) return new Tuple<float, string, bool, float, float>(0.01f, "0.00", true, 0, 1);
            if (index == 2) return new Tuple<float, string, bool, float, float>(0.01f, "0.00", true, 0, 1);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return ((float)(20 * Math.Pow(1000, value))).ToString("00000.0") + " Hz";
            if (index == 1) return (value).ToString("0.000");
            if (index == 2) return (value / 10f + 0.9f).ToString("0.000");
            return "[WRONG NUMBER]";
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }

        public void Init() { }
        public void ShowEditForm() { }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = (float)(20 * Math.Pow(1000, FXValues[0].GetValue(timeBeat)));
            normVal1 = FXValues[1].GetValue(timeBeat);
            normVal2 = FXValues[2].GetValue(timeBeat) / 10f + 0.9f;

            normVal1 = Math.Min(1f, Math.Max(0, normVal2));
            normVal2 = Math.Min(0.999f, normVal2);
        }

        public AFX_HighpassResonant()
        {
            FXValues[0] = new FXValue(0.447f);
            FXValues[1] = new FXValue(0.2f);
            FXValues[2] = new FXValue(0.5f);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class AFX_FFComb : IAudioFX
    {
        public string DisplayName => "COMB-FILTER(FF)";
        public ushort Type => 4;
        FeedForwardComb filter = new FeedForwardComb(2400);

        FXValue[] FXValues = new FXValue[2];
        float normVal0;
        float normVal1;

        public int FXFloatValueCount => FXValues.Length;

        public void Apply(ref float[] audio)
        {
            filter.ProccessLR(ref audio,(int)normVal0,normVal1);
        }
        public void ClearInstance()
        {
            filter.Clear();
        }
        public object Clone()
        {
            AFX_FFComb clone = new AFX_FFComb();
            for (int i = 0; i < FXFloatValueCount; i++) clone.FXValues[i] = (FXValue)FXValues[i].Clone();
            return clone;
        }
        public void Dispose()
        {
            filter = null;
        }

        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Frequency (LOG, Hz)";
            if (index == 1) return "Amount";
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, -0.5f, 1.5f);
            if (index == 1) return new Tuple<float, string, bool, float, float>(0.01f, "0.000", true, -5, 5);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return ((float)(20 * Math.Pow(1000, value))).ToString("00000.0") + " Hz";
            if (index == 1) return value.ToString("0.000") + "x";
            return "[WRONG NUMBER]";
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }

        public void Init() { }
        public void ShowEditForm() { }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = (float)(20 * Math.Pow(1000, FXValues[0].GetValue(timeBeat)));
            normVal0 = 48000 / normVal0;
            normVal1 = FXValues[1].GetValue(timeBeat);
        }

        public AFX_FFComb()
        {
            FXValues[0] = new FXValue(0.447f);
            FXValues[1] = new FXValue(0.7f);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class AFX_FBComb : IAudioFX
    {
        public string DisplayName => "COMB-FILTER(FB)";
        public ushort Type => 4;
        FeedBackComb filter = new FeedBackComb(2400);

        FXValue[] FXValues = new FXValue[2];
        float normVal0;
        float normVal1;

        public int FXFloatValueCount => FXValues.Length;

        public void Apply(ref float[] audio)
        {
            filter.ProccessLR(ref audio, (int)normVal0, normVal1);
        }
        public void ClearInstance()
        {
            filter.Clear();
        }
        public object Clone()
        {
            AFX_FBComb clone = new AFX_FBComb();
            for (int i = 0; i < FXFloatValueCount; i++) clone.FXValues[i] = (FXValue)FXValues[i].Clone();
            return clone;
        }
        public void Dispose()
        {
            filter = null;
        }

        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Frequency (LOG, Hz)";
            if (index == 1) return "Amount";
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, -0.5f, 1.5f);
            if (index == 1) return new Tuple<float, string, bool, float, float>(0.01f, "0.00", true, 0, 0.99f);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return ((float)(20 * Math.Pow(1000, value))).ToString("00000.0") + " Hz";
            if (index == 1) return value.ToString("0.00") + "x";
            return "[WRONG NUMBER]";
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }

        public void Init() { }
        public void ShowEditForm() { }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = (float)(20 * Math.Pow(1000, FXValues[0].GetValue(timeBeat)));
            normVal0 = 48000 / normVal0;
            normVal1 = FXValues[1].GetValue(timeBeat);
        }

        public AFX_FBComb()
        {
            FXValues[0] = new FXValue(0.447f);
            FXValues[1] = new FXValue(0.3f);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    // 06-3-PASS
    // 07-8-BAND

    // AFX - 08
    public class AFX_Delay : IAudioFX
    {
        public string DisplayName => "DELAY-FX";
        public ushort Type => 8;
        DelayFX filter = new DelayFX(48000);
        Project p;

        FXValue[] FXValues = new FXValue[3];
        float normVal0;
        float normVal1;
        float normVal2;

        public int FXFloatValueCount => FXValues.Length;

        public void Apply(ref float[] audio)
        {
            filter.ProccessLR(ref audio, (int)normVal0, normVal1, normVal2);
        }
        public void ClearInstance()
        {
            filter.Clear();
        }
        public object Clone()
        {
            AFX_Delay clone = new AFX_Delay();
            for (int i = 0; i < FXFloatValueCount; i++) clone.FXValues[i] = (FXValue)FXValues[i].Clone();
            return clone;
        }
        public void Dispose()
        {
            filter = null;
        }

        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Delay (Beats)";
            if (index == 1) return "Amount";
            if (index == 2) return "Ping-Pong";
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
            if (index == 1) return new Tuple<float, string, bool, float, float>(0.01f, "0.00", true, 0, 10);
            if (index == 2) return new Tuple<float, string, bool, float, float>(0.01f, "0.00", true, 0, 1);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return value.ToString("0.000") + " Beats";
            if (index == 1) return value.ToString("0.000") + "";
            if (index == 2) return value.ToString("0.000") + "";
            return "[WRONG NUMBER]";
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }

        public void Init() { }
        public void ShowEditForm() { }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = Form1.Project.GetBeatDurationSamp(timeBeat) * FXValues[0].GetValue(timeBeat);
            normVal1 = FXValues[1].GetValue(timeBeat);
            normVal2 = FXValues[2].GetValue(timeBeat);

            normVal1 = Math.Min(0.99f, Math.Max(0, normVal1));
        }

        public AFX_Delay()
        {
            FXValues[0] = new FXValue(0.125f);
            FXValues[1] = new FXValue(0.75f);
            FXValues[2] = new FXValue(0.0f);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class AFX_Reverb : IAudioFX
    {
        public string DisplayName => "REVERB";
        public ushort Type => 9;
        Freeverb2 filter = new Freeverb2();

        FXValue[] FXValues = new FXValue[4];
        float normVal0;
        float normVal1;
        float normVal2;
        float normVal3;

        public int FXFloatValueCount => FXValues.Length;

        public void Apply(ref float[] audio)
        {
            filter.ProccessR(ref audio, normVal0, normVal1, normVal2, normVal3);
            filter.ProccessL(ref audio, normVal0, normVal1, normVal2, normVal3);
        }
        public void ClearInstance()
        {
            filter.Clear();
        }
        public object Clone()
        {
            AFX_Reverb clone = new AFX_Reverb();
            for (int i = 0; i < FXFloatValueCount; i++) clone.FXValues[i] = (FXValue)FXValues[i].Clone();
            return clone;
        }
        public void Dispose()
        {
            filter = null;
        }

        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Room size (0.7 to 0.98)";
            if (index == 1) return "Damping";
            if (index == 2) return "AP Gain";
            if (index == 3) return "Pre Volume (Mult)";
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
            if (index == 1) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
            if (index == 2) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
            if (index == 3) return new Tuple<float, string, bool, float, float>(0.01f, "0.00", true, 0, 10);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return (value * 0.7f + 0.28f).ToString("0.000");
            if (index == 1) return value.ToString("0.000") + "";
            if (index == 2) return value.ToString("0.000") + "";
            if (index == 3) return value.ToString("0.000") + "x";
            return "[WRONG NUMBER]";
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }

        public void Init() { }
        public void ShowEditForm() { }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = 0.7f * FXValues[0].GetValue(timeBeat) + 0.28f;
            normVal1 = FXValues[1].GetValue(timeBeat);
            normVal2 = FXValues[2].GetValue(timeBeat);
            normVal3 = FXValues[3].GetValue(timeBeat);

            normVal0 = Math.Min(0.98f, Math.Max(0.7f, normVal0));
        }
        public void Init(Project p)
        {
        }

        public AFX_Reverb()
        {
            FXValues[0] = new FXValue(0.90f);
            FXValues[1] = new FXValue(0.10f);
            FXValues[2] = new FXValue(0.50f);
            FXValues[3] = new FXValue(0.20f);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class AFX_Delay2 : IAudioFX
    {
        public string DisplayName => "DELAY2-FX";
        public ushort Type => 9;
        DelayFX filter = new DelayFX(48000);
        Project p;

        FXValue[] FXValues = new FXValue[3];
        float normVal0;
        float normVal1;
        float normVal2;

        public int FXFloatValueCount => FXValues.Length;

        public void Apply(ref float[] audio)
        {
            filter.ProccessLR(ref audio, (int)normVal0, normVal1, normVal2);
        }
        public void ClearInstance()
        {
            filter.Clear();
        }
        public object Clone()
        {
            AFX_Delay2 clone = new AFX_Delay2();
            for (int i = 0; i < FXFloatValueCount; i++) clone.FXValues[i] = (FXValue)FXValues[i].Clone();
            return clone;
        }
        public void Dispose()
        {
            filter = null;
        }

        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Delay (Beats)";
            if (index == 1) return "Amount";
            if (index == 2) return "Ping-Pong";
            if (index == 3) return "LF Damp (LOG, Hz)";
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
            if (index == 1) return new Tuple<float, string, bool, float, float>(0.01f, "0.00", true, 0, 10);
            if (index == 2) return new Tuple<float, string, bool, float, float>(0.01f, "0.00", true, 0, 1);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return value.ToString("0.000") + " Beats";
            if (index == 1) return value.ToString("0.000") + "";
            if (index == 2) return value.ToString("0.000") + "";
            return "[WRONG NUMBER]";
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }

        public void Init() { }
        public void ShowEditForm() { }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = Form1.Project.GetBeatDurationSamp(timeBeat) * FXValues[0].GetValue(timeBeat);
            normVal1 = FXValues[1].GetValue(timeBeat);
            normVal2 = FXValues[2].GetValue(timeBeat);

            normVal1 = Math.Min(0.99f, Math.Max(0, normVal1));
        }

        public AFX_Delay2()
        {
            FXValues[0] = new FXValue(0.125f);
            FXValues[1] = new FXValue(0.75f);
            FXValues[2] = new FXValue(0.0f);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class AFX_Reverb2 : IAudioFX
    {
        public string DisplayName => "REVERB2";
        public ushort Type => 11;
        Freeverb2 filter = new Freeverb2();

        FXValue[] FXValues = new FXValue[5];
        float normVal0;
        float normVal1;
        float normVal2;
        float normVal3;
        float normVal4;

        float[] buff2 = new float[0];

        public int FXFloatValueCount => FXValues.Length;

        public void Apply(ref float[] audio)
        {
            if (buff2.Length != audio.Length) buff2 = new float[audio.Length];
            for (int i = 0; i < audio.Length; i++) buff2[i] = normVal4 * audio[i];
            filter.ProccessR(ref audio, normVal0, normVal1, normVal2, normVal3);
            filter.ProccessL(ref audio, normVal0, normVal1, normVal2, normVal3);
            for (int i = 0; i < audio.Length; i++) audio[i] += buff2[i];
        }
        public void ClearInstance()
        {
            filter.Clear();
        }
        public object Clone()
        {
            AFX_Reverb2 clone = new AFX_Reverb2();
            for (int i = 0; i < FXFloatValueCount; i++) clone.FXValues[i] = (FXValue)FXValues[i].Clone();
            return clone;
        }
        public void Dispose()
        {
            filter = null;
        }

        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Room size (0.7 to 0.98)";
            if (index == 1) return "Damping";
            if (index == 2) return "AP Gain";
            if (index == 3) return "Pre Volume (Mult)";
            if (index == 4) return "Dry Volume (Mult)";
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
            if (index == 1) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
            if (index == 2) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
            if (index == 3) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 10);
            if (index == 4) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 10);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return (value * 0.7f + 0.28f).ToString("0.000");
            if (index == 1) return value.ToString("0.000") + "";
            if (index == 2) return value.ToString("0.000") + "";
            if (index == 3) return value.ToString("0.000") + "x";
            if (index == 4) return value.ToString("0.000") + "x";
            return "[WRONG NUMBER]";
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }

        public void Init() { }
        public void ShowEditForm() { }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = 0.7f * FXValues[0].GetValue(timeBeat) + 0.28f;
            normVal1 = FXValues[1].GetValue(timeBeat);
            normVal2 = FXValues[2].GetValue(timeBeat);
            normVal3 = FXValues[3].GetValue(timeBeat);
            normVal4 = FXValues[4].GetValue(timeBeat);

            normVal0 = Math.Min(0.98f, Math.Max(0.7f, normVal0));
        }
        public void Init(Project p)
        {
        }

        public AFX_Reverb2()
        {
            FXValues[0] = new FXValue(0.90f);
            FXValues[1] = new FXValue(0.10f);
            FXValues[2] = new FXValue(0.50f);
            FXValues[3] = new FXValue(0.10f);
            FXValues[4] = new FXValue(0.40f);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }

    // AFX - 16
    // 16-TREMOLO
    // 17-PAN
    public class AFX_WahWah : IAudioFX
    {
        public string DisplayName => "WAHWAH-FX";
        public ushort Type => 18;
        WahWah filter = new WahWah(4800);

        FXValue[] FXValues = new FXValue[4];
        float normVal0;
        float normVal1;
        float normVal2;
        float normVal3;
        float tb;
        float tbdiff = -1;

        public int FXFloatValueCount => FXValues.Length;

        public void Apply(ref float[] audio)
        {
            filter.ProccessLR(ref audio, normVal0, normVal1 + normVal0, (tb - tbdiff) * normVal2, (tb) * normVal2, normVal3);
        }
        public void ClearInstance()
        {
            filter.Clear();
        }
        public object Clone()
        {
            AFX_WahWah clone = new AFX_WahWah();
            for (int i = 0; i < FXFloatValueCount; i++) clone.FXValues[i] = (FXValue)FXValues[i].Clone();
            return clone;
        }
        public void Dispose()
        {
            filter = null;
        }

        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.01f, "0.00", true, 0f, 3f);
            if (index == 1) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, -0.5f, 1.5f);
            if (index == 2) return new Tuple<float, string, bool, float, float>(0.01f, "0.00", true, 0, 1);
            if (index == 3) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Freq range (octave)";
            if (index == 1) return "Mid freq (LOG, Hz)";
            if (index == 2) return "Oscilation (Beats)";
            if (index == 3) return "Amount (-1 to 1)";
            return "[WRONG NUMBER]";
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return value.ToString("0.000") + " Octaves";
            if (index == 1) return ((float)(20 * Math.Pow(1000, value))).ToString("00000.0") + " Hz";
            if (index == 2) return value.ToString("0.000") + " Beats";
            if (index == 3) return (value * 2 - 1).ToString("0.000") + "";
            return "[WRONG NUMBER]";
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }

        public void Init() { }
        public void ShowEditForm() { }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = FXValues[0].GetValue(timeBeat);
            normVal1 = (float)(20 * Math.Pow(1000, FXValues[1].GetValue(timeBeat)));
            normVal2 = FXValues[2].GetValue(timeBeat);
            normVal3 = FXValues[3].GetValue(timeBeat) * 2f - 1f;

            if (tbdiff != -1) tbdiff = timeBeat - tb;
            else tbdiff = 0;
            tb = timeBeat;
        }

        public AFX_WahWah()
        {
            FXValues[0] = new FXValue(1.0f);
            FXValues[1] = new FXValue(0.447f);
            FXValues[2] = new FXValue(0.5f);
            FXValues[3] = new FXValue(1.0f);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    // 19-???
    public class AFX_Flange : IAudioFX
    {
        public string DisplayName => "FLANGE-FX";
        public ushort Type => 20;
        Flanger filter = new Flanger(9600);

        FXValue[] FXValues = new FXValue[4];
        float normVal0;
        float normVal1;
        float normVal2;
        float normVal3;
        float tb;
        float tbdiff = -1;

        public int FXFloatValueCount => FXValues.Length;

        public void Apply(ref float[] audio)
        {
            filter.ProccessLR(ref audio, normVal0, normVal1 + normVal0, (tb - tbdiff) * normVal2, (tb) * normVal2, normVal3);
        }
        public void ClearInstance()
        {
            filter.Clear();
        }
        public object Clone()
        {
            AFX_Flange clone = new AFX_Flange();
            for (int i = 0; i < FXFloatValueCount; i++) clone.FXValues[i] = (FXValue)FXValues[i].Clone();
            return clone;
        }
        public void Dispose()
        {
            filter = null;
        }

        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Time Range (Sec)";
            if (index == 1) return "Ofset (Sec)";
            if (index == 2) return "Oscilation (Beats)";
            if (index == 3) return "Amount (-1 to 1)";
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.0001f, "0.0000", true, 0f, 1f);
            if (index == 1) return new Tuple<float, string, bool, float, float>(0.0001f, "0.0000", true, 0f, 1f);
            if (index == 2) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
            if (index == 3) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return (value).ToString("0.0000") + " Seconds";
            if (index == 1) return (value).ToString("0.0000") + " Seconds";
            if (index == 2) return value.ToString("0.000") + " Beats";
            if (index == 3) return (value * 2 - 1).ToString("0.000") + "";
            return "[WRONG NUMBER]";
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }

        public void Init() { }
        public void ShowEditForm() { }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = FXValues[0].GetValue(timeBeat) * 48000f;
            normVal1 = FXValues[1].GetValue(timeBeat) * 48000f;
            normVal2 = FXValues[2].GetValue(timeBeat);
            normVal3 = FXValues[3].GetValue(timeBeat) * 2f - 1f;

            if (tbdiff != -1) tbdiff = timeBeat - tb;
            else tbdiff = 0;
            tb = timeBeat;
        }

        public AFX_Flange()
        {
            FXValues[0] = new FXValue(0.03f);
            FXValues[1] = new FXValue(0.5f);
            FXValues[2] = new FXValue(0.5f);
            FXValues[3] = new FXValue(1.0f);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }

    // VFX - 00
    public class VFX_S_UVFlipX : IVideoFX
    {
        public string DisplayName => "Prep-Flip Horizontal";
        public ushort Type => 0;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[0];

        public void Apply(ref TextureInfo texin)
        {
            texin.FlipX();
        }

        public object Clone()
        {
            VFX_S_UVFlipX fx = new VFX_S_UVFlipX();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            return "[WRONG NUMBER]";
        }
        public void Dispose()
        {
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public string GetAutomatedValueName(int index)
        {
            return "[WRONG NUMBER]";
        }

        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public void Init()
        {
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class VFX_S_UVFlipY : IVideoFX
    {
        public string DisplayName => "Prep-Flip Vertical";
        public ushort Type => 1;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[0];

        public void Apply(ref TextureInfo texin)
        {
            texin.FlipY();
        }

        public object Clone()
        {
            VFX_S_UVFlipY fx = new VFX_S_UVFlipY();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            return "[WRONG NUMBER]";
        }

        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public void Init()
        {
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class VFX_S_Opac : IVideoFX
    {
        public string DisplayName => "Prep-Opacity";
        public ushort Type => 2;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[1];

        float normVal0;

        public void Apply(ref TextureInfo texin)
        {
            texin.AppendTransformColor(new OpenTK.Matrix4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, normVal0), OpenTK.Vector4.Zero);
        }
        public object Clone()
        {
            VFX_S_Opac fx = new VFX_S_Opac();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return (value * 100).ToString("0.0") + "%";
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.004f, "0.000", true, 0, 1);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Opacity";
            return "[WRONG NUMBER]";
        }

        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public VFX_S_Opac()
        {
            FXValues[0] = new FXValue(1);
        }
        public void Init()
        {
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = FXValues[0].GetValue(timeBeat);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class VFX_S_UVZoom : IVideoFX
    {
        public string DisplayName => "Prep-UV Zoom";
        public ushort Type => 3;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[3];

        float normVal0; //zoom
        float normVal1; //centerX
        float normVal2; //centerY

        public void Apply(ref TextureInfo texin)
        {
            float uvx0 = texin.RenderUVX0;
            float uvx1 = texin.RenderUVX1;
            float uvy0 = texin.RenderUVY0;
            float uvy1 = texin.RenderUVY1;

            float nv1 = (1 - normVal1) * uvx0 + normVal1 * uvx1;
            float nv2 = (1 - normVal2) * uvy0 + normVal2 * uvy1;

            texin.RenderUVX0 = (1 - normVal0) * uvx0 + normVal0 * nv1;
            texin.RenderUVX1 = (1 - normVal0) * uvx1 + normVal0 * nv1;
            texin.RenderUVY0 = (1 - normVal0) * uvy0 + normVal0 * nv2;
            texin.RenderUVY1 = (1 - normVal0) * uvy1 + normVal0 * nv2;
        }
        public object Clone()
        {
            VFX_S_UVZoom fx = new VFX_S_UVZoom();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.004f, "0.000", true, 0, 1);
            if (index == 1) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
            if (index == 2) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return (value * 100).ToString("0.0") + "%";
            if (index == 1) return (value).ToString("0.0");
            if (index == 2) return (value).ToString("0.0");
            return "[WRONG NUMBER]";
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Zoom";
            if (index == 1) return "Center X";
            if (index == 2) return "Center Y";
            return "[WRONG NUMBER]";
        }

        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public void Init()
        {
        }
        public VFX_S_UVZoom()
        {
            FXValues[0] = new FXValue(0);
            FXValues[1] = new FXValue(0.5f);
            FXValues[2] = new FXValue(0.5f);
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = FXValues[0].GetValue(timeBeat);
            normVal0 = Math.Min(1, Math.Max(0, normVal0));
            normVal1 = FXValues[1].GetValue(timeBeat);
            normVal2 = FXValues[2].GetValue(timeBeat);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class VFX_S_Multiply : IVideoFX
    {
        public string DisplayName => "Prep-Multiply";
        public ushort Type => 4;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[4];

        float normVal0;
        float normVal1;
        float normVal2;
        float normVal3;

        public void Apply(ref TextureInfo texin)
        {
            texin.AppendTransformColor(new OpenTK.Matrix4(normVal0, 0, 0, 0, 0, normVal1, 0, 0, 0, 0, normVal2, 0, 0, 0, 0, normVal3), OpenTK.Vector4.Zero);
        }
        public object Clone()
        {
            VFX_S_Multiply fx = new VFX_S_Multiply();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return (value).ToString("0.000");
            if (index == 1) return (value).ToString("0.000");
            if (index == 2) return (value).ToString("0.000");
            if (index == 3) return (value).ToString("0.000");
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.004f, "0.000", true, 0, 1);
            if (index == 1) return new Tuple<float, string, bool, float, float>(0.004f, "0.000", true, 0, 1);
            if (index == 2) return new Tuple<float, string, bool, float, float>(0.004f, "0.000", true, 0, 1);
            if (index == 3) return new Tuple<float, string, bool, float, float>(0.004f, "0.000", true, 0, 1);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "R";
            if (index == 1) return "G";
            if (index == 2) return "B";
            if (index == 3) return "A";
            return "[WRONG NUMBER]";
        }

        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public void Init()
        {
        }
        public VFX_S_Multiply()
        {
            FXValues[0] = new FXValue(1);
            FXValues[1] = new FXValue(1);
            FXValues[2] = new FXValue(1);
            FXValues[3] = new FXValue(1);
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = FXValues[0].GetValue(timeBeat);
            normVal1 = FXValues[1].GetValue(timeBeat);
            normVal2 = FXValues[2].GetValue(timeBeat);
            normVal3 = FXValues[3].GetValue(timeBeat);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class VFX_S_Contrast : IVideoFX
    {
        public string DisplayName => "Prep-Contrast";
        public ushort Type => 5;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[2];

        float normVal0;
        float normVal1;

        public void Apply(ref TextureInfo texin)
        {
            texin.AppendTransformColor(new OpenTK.Matrix4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1), new OpenTK.Vector4(normVal1, normVal1, normVal1, 0));
            texin.AppendTransformColor(new OpenTK.Matrix4(normVal0, 0, 0, 0, 0, normVal0, 0, 0, 0, 0, normVal0, 0, 0, 0, 0, 1), new OpenTK.Vector4(0, 0, 0, 0));
            texin.AppendTransformColor(new OpenTK.Matrix4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1), new OpenTK.Vector4(-normVal1, -normVal1, -normVal1, 0));
        }
        public object Clone()
        {
            VFX_S_Contrast fx = new VFX_S_Contrast();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return (value).ToString("0.000");
            if (index == 1) return (value).ToString("0.000");
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.05f, "0.00", true, -20f, 20f);
            if (index == 1) return new Tuple<float, string, bool, float, float>(0.01f, "0.00", true, -20f, 20f);
            return new Tuple<float, string, bool, float, float>(0.01f, "0.00", true, -20, 20);
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Amount";
            if (index == 1) return "Center";
            return "[WRONG NUMBER]";
        }

        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public void Init()
        {
        }
        public VFX_S_Contrast()
        {
            FXValues[0] = new FXValue(1);
            FXValues[1] = new FXValue(0.5f);
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = FXValues[0].GetValue(timeBeat);
            normVal1 = FXValues[1].GetValue(timeBeat);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class VFX_S_Brightness : IVideoFX
    {
        public string DisplayName => "Prep-Brightness";
        public ushort Type => 6;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[1];

        float normVal0;

        public void Apply(ref TextureInfo texin)
        {
            texin.AppendTransformColor(new OpenTK.Matrix4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1), new OpenTK.Vector4(normVal0, normVal0, normVal0, 0));
        }
        public object Clone()
        {
            VFX_S_Brightness fx = new VFX_S_Brightness();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return (value).ToString("0.000");
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.004f, "0.000", true, -1f, 1f);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Amount";
            return "[WRONG NUMBER]";
        }

        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public void Init()
        {
        }
        public VFX_S_Brightness()
        {
            FXValues[0] = new FXValue(0);
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = FXValues[0].GetValue(timeBeat);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class VFX_S_BlackAndWhite : IVideoFX
    {
        public string DisplayName => "Prep-Black and white";
        public ushort Type => 7;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[1];

        float normVal0;

        public void Apply(ref TextureInfo texin)
        {
            texin.AppendTransformColor(new OpenTK.Matrix4(
                (1 - normVal0) + normVal0 * 0.2125f, normVal0 * 0.2125f, normVal0 * 0.2125f, 0,
                normVal0 * 0.7154f, (1 - normVal0) + normVal0 * 0.7154f, normVal0 * 0.7154f, 0,
                normVal0 * 0.0721f, normVal0 * 0.0721f, (1 - normVal0) + normVal0 * 0.0721f, 0,
                0, 0, 0, 1), new OpenTK.Vector4(0, 0, 0, 0));
        }
        public object Clone()
        {
            VFX_S_BlackAndWhite fx = new VFX_S_BlackAndWhite();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return (value).ToString("0.000");
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.004f, "0.000", true, 0f, 1f);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Amount";
            return "[WRONG NUMBER]";
        }

        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public void Init()
        {
        }
        public VFX_S_BlackAndWhite()
        {
            FXValues[0] = new FXValue(0);
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = FXValues[0].GetValue(timeBeat);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }

    // VFX - 08
    public class VFX_S_Invert : IVideoFX
    {
        public string DisplayName => "Prep-Black and white";
        public ushort Type => 8;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[0];

        public void Apply(ref TextureInfo texin)
        {
            texin.AppendTransformColor(new OpenTK.Matrix4(
                -1, 0, 0, 0,
                0, -1, 0, 0,
                0, 0, -1, 0,
                0, 0, 0, 1), new OpenTK.Vector4(1, 1, 1, 0));
        }
        public object Clone()
        {
            VFX_S_Invert fx = new VFX_S_Invert();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            return new Tuple<float, string, bool, float, float>(0.01f, "0.00", true, -20, 20);
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            return "[WRONG NUMBER]";
        }

        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public void Init()
        {
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class VFX_S_GridVis : IVideoFX
    {
        public string DisplayName => "Prep-Grid Visual";
        public ushort Type => 9;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[5];

        float normVal0; //NxN
        float normVal1; //X
        float normVal2; //Y
        float normVal3; //s
        float normVal4; //r

        public void Apply(ref TextureInfo texin)
        {
            float scale = normVal3 / normVal0;
            float rotation = normVal4 * (float)Math.PI * 2;
            float movX = -((normVal0 - 1) - 2 * normVal1) / normVal0;
            float movY = ((normVal0 - 1) - 2 * normVal2 ) / normVal0;

            texin.RenderMatrix *= OpenTK.Matrix4.CreateScale(scale, scale, 1) * OpenTK.Matrix4.CreateRotationZ(rotation) * OpenTK.Matrix4.CreateTranslation(movX * (Form1.Project.WIDTH / (float)Form1.Project.HEIGHT), movY, 0);
        }
        public object Clone()
        {
            VFX_S_GridVis fx = new VFX_S_GridVis();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return (value).ToString("0.0") + "x" + (value).ToString("0.0");
            if (index == 1) return (value).ToString("0.00");
            if (index == 2) return (value).ToString("0.00");
            if (index == 3) return (value).ToString("0.00");
            if (index == 4) return (value * 2 * Math.PI).ToString("0.00");
            return "[WRONG NUMBER]";


        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.25f, "0.00", true, 1, 16);
            if (index == 1) return new Tuple<float, string, bool, float, float>(0.25f, "0.00", true, 0, 15);
            if (index == 2) return new Tuple<float, string, bool, float, float>(0.25f, "0.00", true, 0, 15);
            if (index == 3) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, -2, 2);
            if (index == 4) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", false, -999, 999);
            return new Tuple<float, string, bool, float, float>(0.01f, "0.00", true, -20, 20);
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "NxN visuals";
            if (index == 1) return "X";
            if (index == 2) return "Y";
            if (index == 3) return "Scale";
            if (index == 4) return "Rotation";
            return "[WRONG NUMBER]";
        }

        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public void Init()
        {
        }
        public VFX_S_GridVis()
        {
            FXValues[0] = new FXValue(3);
            FXValues[1] = new FXValue(1);
            FXValues[2] = new FXValue(1);
            FXValues[3] = new FXValue(1);
            FXValues[4] = new FXValue(0);
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = FXValues[0].GetValue(timeBeat);
            normVal1 = FXValues[1].GetValue(timeBeat);
            normVal2 = FXValues[2].GetValue(timeBeat);
            normVal3 = FXValues[3].GetValue(timeBeat);
            normVal4 = FXValues[4].GetValue(timeBeat);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class VFX_S_ARZoom : IVideoFX
    {
        public string DisplayName => "Prep-Aspect Zoom";
        public ushort Type => 10;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[2];
        
        float normVal0; //Goal Num
        float normVal1; //Goal Den

        public void Apply(ref TextureInfo texin)
        {
            float inputAR = texin.Width / texin.Height;
            float outputAR = normVal0 / normVal1;
            float ratio = outputAR / inputAR;

            if (ratio != 0 && !float.IsInfinity(ratio) && !float.IsNaN(ratio))
            {

                if (ratio > 1)
                {
                    ratio = 1 / ratio;
                    float mid = (texin.RenderUVY0 + texin.RenderUVY1) / 2;
                    texin.RenderUVY0 = ratio * texin.RenderUVY0 + (1 - ratio) * mid;
                    texin.RenderUVY1 = ratio * texin.RenderUVY1 + (1 - ratio) * mid;
                }
                else
                {
                    float mid = (texin.RenderUVX0 + texin.RenderUVX1) / 2;
                    texin.RenderUVX0 = ratio * texin.RenderUVX0 + (1 - ratio) * mid;
                    texin.RenderUVX1 = ratio * texin.RenderUVX1 + (1 - ratio) * mid;
                }
            }

            texin.Width = normVal0;
            texin.Height = normVal1;
        }
        public object Clone()
        {
            VFX_S_ARZoom fx = new VFX_S_ARZoom();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return (value).ToString("0.000");
            if (index == 1) return (value).ToString("0.000");
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(1f, "000", true, 0f, 999999f);
            if (index == 1) return new Tuple<float, string, bool, float, float>(1f, "000", true, 0f, 999999f);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Goal AR Width";
            if (index == 1) return "Goal AR Height";
            return "[WRONG NUMBER]";
        }

        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public void Init()
        {

        }
        public VFX_S_ARZoom()
        {
            FXValues[0] = new FXValue(Form1.Project.WIDTH);
            FXValues[1] = new FXValue(Form1.Project.HEIGHT);
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = FXValues[0].GetValue(timeBeat);
            normVal1 = FXValues[1].GetValue(timeBeat);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class VFX_S_ARStretch : IVideoFX
    {
        public string DisplayName => "Prep-Aspect Stretch";
        public ushort Type => 11;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[2];

        float normVal0; //Goal Num
        float normVal1; //Goal Den

        public void Apply(ref TextureInfo texin)
        {
            texin.Width = normVal0;
            texin.Height = normVal1;
        }
        public object Clone()
        {
            VFX_S_ARStretch fx = new VFX_S_ARStretch();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return (value).ToString("0");
            if (index == 1) return (value).ToString("0");
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(1f, "000", true, 0f, 999999f);
            if (index == 1) return new Tuple<float, string, bool, float, float>(1f, "000", true, 0f, 999999f);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Goal AR Width";
            if (index == 1) return "Goal AR Height";
            return "[WRONG NUMBER]";
        }

        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public VFX_S_ARStretch()
        {
            FXValues[0] = new FXValue(Form1.Project.WIDTH);
            FXValues[1] = new FXValue(Form1.Project.HEIGHT);
        }
        public void Init()
        {
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = FXValues[0].GetValue(timeBeat);
            normVal1 = FXValues[1].GetValue(timeBeat);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }

    // VFX - 16
    public class VFX_Border : IVideoFX
    {
        public string DisplayName => "Border-Solid";
        public ushort Type => 16;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[5];

        public void Apply(ref TextureInfo texin)
        {
            if (texin.TextureIndex == 0) return;

            texin = EffectHelper.Flatten(texin, GLDraw.GetFreeTarget(texin.TextureIndex));
            RenderTarget rt2 = GLDraw.GetFreeTarget(texin.TextureIndex);

            rt2.Use();
            
            GL.BindTexture(TextureTarget.Texture2D, texin.TextureIndex);
            GL.UseProgram(EffectHelper.shader16);
            GL.BindVertexArray(GLDraw.QuadVAO);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.BindAttribLocation(EffectHelper.shader16, 0, "vertexPosition");
            GL.BindAttribLocation(EffectHelper.shader16, 1, "vertexUV");

            GL.Uniform4(161, normVal1);
            GL.Uniform1(162, normVal0);
            GL.Uniform1(163, texin.Height / texin.Width);

            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.BindVertexArray(0);
            texin.TextureIndex = rt2.TextureIndex;
        }
        public object Clone()
        {
            VFX_Border fx = new VFX_Border();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return value.ToString("0.000");
            if (index == 1) return value.ToString("0.000");
            if (index == 2) return value.ToString("0.000");
            if (index == 3) return value.ToString("0.000");
            if (index == 4) return value.ToString("0.000");
            return "[WRONG NUMBER]";
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Width";
            if (index == 1) return "R";
            if (index == 2) return "G";
            if (index == 3) return "B";
            if (index == 4) return "A";
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0f, 1f);
            if (index == 1) return new Tuple<float, string, bool, float, float>(0.004f, "0.000", true, 0f, 1f);
            if (index == 2) return new Tuple<float, string, bool, float, float>(0.004f, "0.000", true, 0, 1);
            if (index == 3) return new Tuple<float, string, bool, float, float>(0.004f, "0.000", true, 0, 1);
            if (index == 4) return new Tuple<float, string, bool, float, float>(0.004f, "0.000", true, 0, 1);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }

        public VFX_Border()
        {
            FXValues[0] = new FXValue(0.03f);
            FXValues[1] = new FXValue(0);
            FXValues[2] = new FXValue(0);
            FXValues[3] = new FXValue(0);
            FXValues[4] = new FXValue(1);
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public void Init()
        {
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = FXValues[0].GetValue(timeBeat);
            normVal1 = new Vector4( FXValues[1].GetValue(timeBeat), FXValues[2].GetValue(timeBeat), FXValues[3].GetValue(timeBeat), FXValues[4].GetValue(timeBeat));
        }
        float normVal0 = 0;
        Vector4 normVal1 = Vector4.Zero;

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class VFX_DirectionalBlur : IVideoFX
    {
        public string DisplayName => "Blur-dir";
        public ushort Type => 17;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[2];

        public void Apply(ref TextureInfo texin)
        {
            if (texin.TextureIndex == 0) return;

            texin = EffectHelper.Flatten(texin, GLDraw.GetFreeTarget(texin.TextureIndex));
            RenderTarget rt2 = GLDraw.GetFreeTarget(texin.TextureIndex);

            rt2.Use();

            GL.BindTexture(TextureTarget.Texture2D, texin.TextureIndex);
            GL.UseProgram(EffectHelper.shaderBlur);
            GL.BindVertexArray(GLDraw.QuadVAO);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.BindAttribLocation(EffectHelper.shaderBlur, 0, "vertexPosition");
            GL.BindAttribLocation(EffectHelper.shaderBlur, 1, "vertexUV");

            GL.Uniform1(171, (float)(Math.Cos(normVal1 * 2 * Math.PI) * normVal0));
            GL.Uniform1(172, (float)(Math.Sin(normVal1 * 2 * Math.PI) * normVal0 * (rt2.Width / (float)rt2.Height)));

            GL.Uniform1(173, 0.000007f);
            GL.Uniform1(174, 0.000116f);
            GL.Uniform1(175, 0.001227f);
            GL.Uniform1(176, 0.008456f);
            GL.Uniform1(177, 0.037975f);
            GL.Uniform1(178, 0.110865f);
            GL.Uniform1(179, 0.210786f);

            GL.Uniform1(180, 0.261117f);

            GL.Uniform1(187, 0.000007f);
            GL.Uniform1(186, 0.000116f);
            GL.Uniform1(185, 0.001227f);
            GL.Uniform1(184, 0.008456f);
            GL.Uniform1(183, 0.037975f);
            GL.Uniform1(182, 0.110865f);
            GL.Uniform1(181, 0.210786f);

            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.BindVertexArray(0);
            texin.TextureIndex = rt2.TextureIndex;
        }
        public object Clone()
        {
            VFX_DirectionalBlur fx = new VFX_DirectionalBlur();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return value.ToString("0.000");
            if (index == 1) return value.ToString("0.000");
            return "[WRONG NUMBER]";
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Width";
            if (index == 1) return "Direction";
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.0001f, "0.0000", false, 0f, 1f);
            if (index == 1) return new Tuple<float, string, bool, float, float>(0.004f, "0.000", false, 0f, 1f);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }

        public VFX_DirectionalBlur()
        {
            FXValues[0] = new FXValue(0.003f);
            FXValues[1] = new FXValue(0.00f);
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public void Init()
        {
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = FXValues[0].GetValue(timeBeat);
            normVal1 = FXValues[1].GetValue(timeBeat);
        }
        float normVal0 = 0;
        float normVal1 = 0;

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class VFX_DirectionalBlurMP : IVideoFX
    {
        public string DisplayName => "Blur-dirMP";
        public ushort Type => 18;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[3];

        public void Apply(ref TextureInfo texin)
        {
            if (texin.TextureIndex == 0) return;

            texin = EffectHelper.Flatten(texin, GLDraw.GetFreeTarget(texin.TextureIndex));

            for (int i = 0; i < normVal2; i++)
            {
                RenderTarget rt2 = GLDraw.GetFreeTarget(texin.TextureIndex);
                rt2.Use();
                GL.BindTexture(TextureTarget.Texture2D, texin.TextureIndex);
                GL.UseProgram(EffectHelper.shaderBlur);
                GL.BindVertexArray(GLDraw.QuadVAO);
                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);
                GL.BindAttribLocation(EffectHelper.shaderBlur, 0, "vertexPosition");
                GL.BindAttribLocation(EffectHelper.shaderBlur, 1, "vertexUV");
                GL.Uniform1(171, (float)(Math.Cos(normVal1 * 2 * Math.PI) * normVal0 * (i + 1) / normVal2));
                GL.Uniform1(172, (float)(Math.Sin(normVal1 * 2 * Math.PI) * normVal0 * (rt2.Width / (float)rt2.Height) * (i + 1) / normVal2));
                GL.Uniform1(173, 0.000007f);
                GL.Uniform1(174, 0.000116f);
                GL.Uniform1(175, 0.001227f);
                GL.Uniform1(176, 0.008456f);
                GL.Uniform1(177, 0.037975f);
                GL.Uniform1(178, 0.110865f);
                GL.Uniform1(179, 0.210786f);
                GL.Uniform1(180, 0.261117f);
                GL.Uniform1(187, 0.000007f);
                GL.Uniform1(186, 0.000116f);
                GL.Uniform1(185, 0.001227f);
                GL.Uniform1(184, 0.008456f);
                GL.Uniform1(183, 0.037975f);
                GL.Uniform1(182, 0.110865f);
                GL.Uniform1(181, 0.210786f);
                GL.ClearColor(0, 0, 0, 0);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
                GL.DisableVertexAttribArray(0);
                GL.DisableVertexAttribArray(1);
                GL.BindVertexArray(0);
                texin.TextureIndex = rt2.TextureIndex;
            }

        }
        public object Clone()
        {
            VFX_DirectionalBlurMP fx = new VFX_DirectionalBlurMP();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return value.ToString("0.000");
            if (index == 1) return value.ToString("0.000");
            if (index == 2) return value.ToString();
            return "[WRONG NUMBER]";
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Width";
            if (index == 1) return "Direction";
            if (index == 2) return "Passes";
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.0001f, "0.0000", false, 0f, 1f);
            if (index == 1) return new Tuple<float, string, bool, float, float>(0.004f, "0.000", false, 0f, 1f);
            if (index == 2) return new Tuple<float, string, bool, float, float>(1f, "0", false, 0f, 10f);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }

        public VFX_DirectionalBlurMP()
        {
            FXValues[0] = new FXValue(0.003f);
            FXValues[1] = new FXValue(0.00f);
            FXValues[2] = new FXValue(3.00f);
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public void Init()
        {
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = FXValues[0].GetValue(timeBeat);
            normVal1 = FXValues[1].GetValue(timeBeat);
            normVal2 = (int)FXValues[2].GetValue(timeBeat);
        }
        float normVal0 = 0;
        float normVal1 = 0;
        int normVal2 = 0;

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class VFX_GaussianBlur : IVideoFX
    {
        public string DisplayName => "Blur-Gauss";
        public ushort Type => 19;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[2];

        public void Apply(ref TextureInfo texin)
        {
            if (texin.TextureIndex == 0) return;

            texin = EffectHelper.Flatten(texin, GLDraw.GetFreeTarget(texin.TextureIndex));

            for (int i = 0; i < normVal1 * 2; i++)
            {
                RenderTarget rt2 = GLDraw.GetFreeTarget(texin.TextureIndex);
                rt2.Use();
                GL.BindTexture(TextureTarget.Texture2D, texin.TextureIndex);

                GL.TextureParameter(texin.TextureIndex, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);
                GL.TextureParameter(texin.TextureIndex, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);

                GL.UseProgram(EffectHelper.shaderBlur);
                GL.BindVertexArray(GLDraw.QuadVAO);
                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);
                GL.BindAttribLocation(EffectHelper.shaderBlur, 0, "vertexPosition");
                GL.BindAttribLocation(EffectHelper.shaderBlur, 1, "vertexUV");
                if (i % 2 == 0)
                {
                    GL.Uniform1(171, (float)(normVal0 * (i / 2 + 1) / normVal1));
                    GL.Uniform1(172, 0.0001f);
                }
                else
                {
                    GL.Uniform1(171, 0.0001f);
                    GL.Uniform1(172, (float)(normVal0 * (rt2.Width / (float)rt2.Height) * (i / 2 + 1) / normVal1));
                }
                GL.Uniform1(173, 0.000007f);
                GL.Uniform1(174, 0.000116f);
                GL.Uniform1(175, 0.001227f);
                GL.Uniform1(176, 0.008456f);
                GL.Uniform1(177, 0.037975f);
                GL.Uniform1(178, 0.110865f);
                GL.Uniform1(179, 0.210786f);
                GL.Uniform1(180, 0.261117f);
                GL.Uniform1(187, 0.000007f);
                GL.Uniform1(186, 0.000116f);
                GL.Uniform1(185, 0.001227f);
                GL.Uniform1(184, 0.008456f);
                GL.Uniform1(183, 0.037975f);
                GL.Uniform1(182, 0.110865f);
                GL.Uniform1(181, 0.210786f);
                GL.ClearColor(0, 0, 0, 0);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
                GL.DisableVertexAttribArray(0);
                GL.DisableVertexAttribArray(1);
                GL.BindVertexArray(0);
                texin.TextureIndex = rt2.TextureIndex;
            }

        }
        public object Clone()
        {
            VFX_GaussianBlur fx = new VFX_GaussianBlur();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return value.ToString("0.000");
            if (index == 1) return value.ToString();
            return "[WRONG NUMBER]";
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Width";
            if (index == 1) return "Passes";
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.0001f, "0.0000", false, 0f, 1f);
            if (index == 1) return new Tuple<float, string, bool, float, float>(1f, "0", false, 0f, 10f);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }

        public VFX_GaussianBlur()
        {
            FXValues[0] = new FXValue(0.003f);
            FXValues[1] = new FXValue(3.00f);
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public void Init()
        {
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = FXValues[0].GetValue(timeBeat);
            normVal1 = (int)FXValues[1].GetValue(timeBeat);
        }
        float normVal0 = 0;
        int normVal1 = 0;

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }

    // VFX - 24
    public class VFX_Flat : IVideoFX
    {
        public string DisplayName => "Flatten";
        public ushort Type => 24;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[0];

        public void Apply(ref TextureInfo texin)
        {
            if (texin.TextureIndex == 0) return;

            texin = EffectHelper.Flatten(texin, GLDraw.GetFreeTarget(texin.TextureIndex));
        }
        public object Clone()
        {
            VFX_Flat fx = new VFX_Flat();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            return "[WRONG]";
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            return "[WRONG]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }

        public VFX_Flat()
        {
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public void Init()
        {
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class VFX_FlatX : IVideoFX
    {
        public string DisplayName => "FlattenX";
        public ushort Type => 25;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[0];

        public void Apply(ref TextureInfo texin)
        {
            if (texin.TextureIndex == 0) return;

            texin = EffectHelper.Flatten(texin, GLDraw.GetFreeTarget(texin.TextureIndex), true, false);
        }
        public object Clone()
        {
            VFX_FlatX fx = new VFX_FlatX();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            return "[WRONG]";
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            return "[WRONG]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }

        public VFX_FlatX()
        {
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public void Init()
        {
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class VFX_FlatY : IVideoFX
    {
        public string DisplayName => "FlattenY";
        public ushort Type => 26;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[0];

        public void Apply(ref TextureInfo texin)
        {
            if (texin.TextureIndex == 0) return;

            texin = EffectHelper.Flatten(texin, GLDraw.GetFreeTarget(texin.TextureIndex), false, true);
        }
        public object Clone()
        {
            VFX_FlatY fx = new VFX_FlatY();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            return "[WRONG]";
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            return "[WRONG]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }

        public VFX_FlatY()
        {
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public void Init()
        {
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class VFX_FlatXY : IVideoFX
    {
        public string DisplayName => "FlattenXY";
        public ushort Type => 27;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[0];

        public void Apply(ref TextureInfo texin)
        {
            if (texin.TextureIndex == 0) return;

            texin = EffectHelper.Flatten(texin, GLDraw.GetFreeTarget(texin.TextureIndex), true, true);
        }
        public object Clone()
        {
            VFX_FlatXY fx = new VFX_FlatXY();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            return "[WRONG]";
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            return "[WRONG]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }

        public VFX_FlatXY()
        {
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public void Init()
        {
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }

    // VFX - 32
    public class VFX_T_Txyz : IVideoFX
    {
        public string DisplayName => "T-translate";
        public ushort Type => 32;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[3];

        float normVal0 = 0;
        float normVal1 = 0;
        float normVal2 = 0;

        public void Apply(ref TextureInfo texin)
        {
            texin.RenderMatrix *= Matrix4.CreateTranslation(2 * normVal0 / (Form1.Project.HEIGHT / (float)Form1.Project.WIDTH), 2 * normVal1, normVal2);
        }
        public object Clone()
        {
            VFX_T_Txyz fx = new VFX_T_Txyz();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return value.ToString("0.00");
            if (index == 1) return value.ToString("0.00");
            if (index == 2) return value.ToString("0.00");
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.004f, "0.000", false, 0, 1);
            if (index == 1) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", false, 0, 1);
            if (index == 2) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", false, 0, 1);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "X";
            if (index == 1) return "Y";
            if (index == 2) return "Z";
            return "[WRONG NUMBER]";
        }

        public VFX_T_Txyz()
        {
            FXValues[0] = new FXValue(0);
            FXValues[1] = new FXValue(0);
            FXValues[2] = new FXValue(0);
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public void Init()
        {
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = FXValues[0].GetValue(timeBeat);
            normVal1 = FXValues[1].GetValue(timeBeat);
            normVal2 = FXValues[2].GetValue(timeBeat);
        }
        
        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class VFX_T_Sxyz : IVideoFX
    {
        public string DisplayName => "T-scale";
        public ushort Type => 33;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[3];

        float normVal0 = 0;
        float normVal1 = 0;
        float normVal2 = 0;

        public void Apply(ref TextureInfo texin)
        {
            texin.RenderMatrix *= Matrix4.CreateScale(normVal0, normVal1, normVal2);
        }
        public object Clone()
        {
            VFX_T_Sxyz fx = new VFX_T_Sxyz();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return value.ToString("0.00");
            if (index == 1) return value.ToString("0.00");
            if (index == 2) return value.ToString("0.00");
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.003f, "0.000", false, 0, 1);
            if (index == 1) return new Tuple<float, string, bool, float, float>(0.003f, "0.000", false, 0, 1);
            if (index == 2) return new Tuple<float, string, bool, float, float>(0.003f, "0.000", false, 0, 1);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "X";
            if (index == 1) return "Y";
            if (index == 2) return "Z";
            return "[WRONG NUMBER]";
        }

        public VFX_T_Sxyz()
        {
            FXValues[0] = new FXValue(1);
            FXValues[1] = new FXValue(1);
            FXValues[2] = new FXValue(1);
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public void Init()
        {
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = FXValues[0].GetValue(timeBeat);
            normVal1 = FXValues[1].GetValue(timeBeat);
            normVal2 = FXValues[2].GetValue(timeBeat);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class VFX_T_Rz : IVideoFX
    {
        public string DisplayName => "T-Rot-Z";
        public ushort Type => 34;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[1];

        float normVal0 = 0;

        public void Apply(ref TextureInfo texin)
        {
            texin.RenderMatrix *= Matrix4.CreateRotationZ((float)(normVal0 * Math.PI * 2));
        }
        public object Clone()
        {
            VFX_T_Rz fx = new VFX_T_Rz();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return value.ToString("0.00");
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.003f, "0.000", false, 0, 1);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Rot (in revs)";
            return "[WRONG NUMBER]";
        }

        public VFX_T_Rz()
        {
            FXValues[0] = new FXValue(0);
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public void Init()
        {
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = FXValues[0].GetValue(timeBeat);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class VFX_T_Ry : IVideoFX
    {
        public string DisplayName => "T-Rot-Y";
        public ushort Type => 35;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[1];

        float normVal0 = 0;

        public void Apply(ref TextureInfo texin)
        {
            texin.RenderMatrix *= Matrix4.CreateRotationY((float)(normVal0 * Math.PI * 2));
        }
        public object Clone()
        {
            VFX_T_Ry fx = new VFX_T_Ry();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return value.ToString("0.00");
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.003f, "0.000", false, 0, 1);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Rot (in revs)";
            return "[WRONG NUMBER]";
        }

        public VFX_T_Ry()
        {
            FXValues[0] = new FXValue(0);
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public void Init()
        {
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = FXValues[0].GetValue(timeBeat);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }
    public class VFX_T_Rx : IVideoFX
    {
        public string DisplayName => "T-Rot-X";
        public ushort Type => 36;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[1];

        float normVal0 = 0;

        public void Apply(ref TextureInfo texin)
        {
            texin.RenderMatrix *= Matrix4.CreateRotationX((float)(normVal0 * Math.PI * 2));
        }
        public object Clone()
        {
            VFX_T_Rx fx = new VFX_T_Rx();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return value.ToString("0.00");
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.003f, "0.000", false, 0, 1);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Rot (in revs)";
            return "[WRONG NUMBER]";
        }

        public VFX_T_Rx()
        {
            FXValues[0] = new FXValue(0);
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public void Init()
        {
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = FXValues[0].GetValue(timeBeat);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }

    // VFX - 40
    /*public class VFX_T_Autoshake_XY
    {
        public string DisplayName => "T-autoTxy";
        public ushort Type => 40;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[3];

        float normVal0 = 0;
        float normVal1 = 0;
        float normVal2 = 0;

        public void Apply(ref TextureInfo texin)
        {
            texin.RenderMatrix *= Matrix4.CreateTranslation(2 * normVal0 / (Form1.Project.HEIGHT / (float)Form1.Project.WIDTH), 2 * normVal1, normVal2);
        }
        public object Clone()
        {
            VFX_T_Txyz fx = new VFX_T_Txyz();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return value.ToString("0.00");
            if (index == 1) return value.ToString("0.00");
            if (index == 2) return value.ToString("0.00");
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.004f, "0.000", false, 0, 1);
            if (index == 1) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", false, 0, 1);
            if (index == 2) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", false, 0, 1);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "X";
            if (index == 1) return "Y";
            if (index == 2) return "Z";
            return "[WRONG NUMBER]";
        }

        public VFX_T_Txyz()
        {
            FXValues[0] = new FXValue(0);
            FXValues[1] = new FXValue(0);
            FXValues[2] = new FXValue(0);
        }
        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXValues.Length) return FXValues[index];
            return null;
        }
        public void ClearInstance()
        {
        }
        public void Init()
        {
        }
        public void ShowEditForm()
        {
        }
        public void UpdateValues(float timeBeat)
        {
            normVal0 = FXValues[0].GetValue(timeBeat);
            normVal1 = FXValues[1].GetValue(timeBeat);
            normVal2 = FXValues[2].GetValue(timeBeat);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }
    }*/

    // NFX - 00
    public class NFX_AutomationAdd : INumberFX
    {
        public string DisplayName => "Automation Track (op +)";
        public ushort Type => 0;
        public int FXFloatValueCount => 0;
        FXValue[] FXValues = new FXValue[0];

        public bool HasEditForm => false;
        public int NumberTrackCount => 1;
        public uint GetNumberTrackReference(int index) => TrackIndex;
        public void SetNumberTrackReference(int index, uint trackIndex) { TrackIndex = trackIndex; }

        uint TrackIndex;

        public float Apply(float value)
        {
            value += Form1.Project.NumberTracks[TrackIndex].NumberOutput;
            return value;
        }

        public void ClearInstance()
        {
        }
        public object Clone()
        {
            NFX_AutomationAdd fx = new NFX_AutomationAdd();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            for (int i = 0; i < NumberTrackCount; i++)
            {
                Form1.Project.NumberTracks[fx.GetNumberTrackReference(i)] = (NumberTrack)Form1.Project.NumberTracks[GetNumberTrackReference(i)].Clone();
            }

            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            return "[WRONG NUMBER]";
        }

        public FXValue GetFloatValueReference(int index)
        {
            return null;
        }
        public void Init()
        {
        }

        public NFX_AutomationAdd()
        {
            NumberTrack dnt = new NumberTrack();
            TrackIndex = Form1.Project.AddNumberTrack(dnt);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public void ShowEditForm()
        {
        }

        public void UpdateValues(float timeBeat)
        {
            Form1.Project.NumberTracks[TrackIndex].Read(timeBeat);
        }
    }
    public class NFX_AutomationMult : INumberFX
    {
        public string DisplayName => "Automation Track (op *)";
        public ushort Type => 1;
        public int FXFloatValueCount => 0;
        FXValue[] FXValues = new FXValue[0];

        public bool HasEditForm => false;
        public int NumberTrackCount => 1;
        public uint GetNumberTrackReference(int index) => TrackIndex;
        public void SetNumberTrackReference(int index, uint trackIndex) { TrackIndex = trackIndex; }

        uint TrackIndex;

        public float Apply(float value)
        {
            value += Form1.Project.NumberTracks[TrackIndex].NumberOutput;
            return value;
        }

        public void ClearInstance()
        {
        }
        public object Clone()
        {
            NFX_AutomationMult fx = new NFX_AutomationMult();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();

            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            return "[WRONG NUMBER]";
        }

        public FXValue GetFloatValueReference(int index)
        {
            return null;
        }
        public void Init()
        {
        }

        public NFX_AutomationMult()
        {
            NumberTrack dnt = new NumberTrack();
            TrackIndex = Form1.Project.AddNumberTrack(dnt);
        }
        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public void ShowEditForm()
        {
        }

        public void UpdateValues(float timeBeat)
        {
            Form1.Project.NumberTracks[TrackIndex].Read(timeBeat);
        }
    }
    public class NFX_LFO_Sine_Add : INumberFX
    {
        public string DisplayName => "Sine LFO (op +)";
        public ushort Type => 2;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[3];

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }

        public float Apply(float value)
        {
            value += normVal0 * (float)Math.Sin((tb * normVal1 + normVal2) * Math.PI * 2);
            return value;
        }

        public void ClearInstance()
        {
        }
        public object Clone()
        {
            NFX_LFO_Sine_Add fx = new NFX_LFO_Sine_Add();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();

            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return value.ToString("0.000");
            if (index == 1) return value.ToString("0.000");
            if (index == 2) return value.ToString("0.000");
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", false, 0, 1);
            if (index == 1) return new Tuple<float, string, bool, float, float>(0.01f, "0.00", true, 0, 1000);
            if (index == 2) return new Tuple<float, string, bool, float, float>(0.01f, "0.00", true, 0, 1);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Amplitude";
            if (index == 1) return "OSCs per beat";
            if (index == 2) return "Phase (0 to 1)";
            return "[WRONG NUMBER]";
        }

        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXFloatValueCount) return FXValues[index];
            return null;
        }
        public void Init()
        {
        }

        public NFX_LFO_Sine_Add()
        {
            FXValues[0] = new FXValue(1);
            FXValues[1] = new FXValue(1);
            FXValues[2] = new FXValue(0);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public void ShowEditForm()
        {
        }

        float normVal0 = 0;
        float normVal1 = 1;
        float normVal2 = 2;
        float tb = 0;
        public void UpdateValues(float timeBeat)
        {
            tb = timeBeat;
            normVal0 = FXValues[0].GetValue(timeBeat);
            normVal1 = FXValues[1].GetValue(timeBeat);
            normVal2 = FXValues[2].GetValue(timeBeat);
        }
    }
    public class NFX_LFO_Sine_Mult : INumberFX
    {
        public string DisplayName => "Sine LFO (op *)";
        public ushort Type => 3;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[3];

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }

        public float Apply(float value)
        {
            value *= normVal0 * (float)Math.Sin((tb * normVal1 + normVal2) * Math.PI * 2);
            return value;
        }

        public void ClearInstance()
        {
        }
        public object Clone()
        {
            NFX_LFO_Sine_Mult fx = new NFX_LFO_Sine_Mult();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();

            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return value.ToString("0.000");
            if (index == 1) return value.ToString("0.000");
            if (index == 2) return value.ToString("0.000");
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", false, 0, 1);
            if (index == 1) return new Tuple<float, string, bool, float, float>(0.01f, "0.00", true, 0, 1000);
            if (index == 2) return new Tuple<float, string, bool, float, float>(0.01f, "0.00", true, 0, 1);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Amplitude";
            if (index == 1) return "OSCs per beat";
            if (index == 2) return "Phase (0 to 1)";
            return "[WRONG NUMBER]";
        }

        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXFloatValueCount) return FXValues[index];
            return null;
        }
        public void Init()
        {
        }

        public NFX_LFO_Sine_Mult()
        {
            FXValues[0] = new FXValue(1);
            FXValues[1] = new FXValue(1);
            FXValues[2] = new FXValue(0);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public void ShowEditForm()
        {
        }

        float normVal0 = 0;
        float normVal1 = 1;
        float normVal2 = 2;
        float tb = 0;
        public void UpdateValues(float timeBeat)
        {
            tb = timeBeat;
            normVal0 = FXValues[0].GetValue(timeBeat);
            normVal1 = FXValues[1].GetValue(timeBeat);
            normVal2 = FXValues[2].GetValue(timeBeat);
        }
    }
    public class NFX_NOISE_Add : INumberFX
    {
        public string DisplayName => "Noise (op +)";
        public ushort Type => 4;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[3];
    
        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }

        public float Apply(float value)
        {
            float phase = (ph + tb * osc) * 128;
            phase %= 128;
            phase += 128;
            phase %= 128;
            int p1 = (int)phase; ;
            int p2 = (p1 + 1) % 128;
            phase %= 1;

            value += (float)(((1 - phase) * Wave[p1] + phase * Wave[p2]) * rang);
            return value;
        }

        public void ClearInstance()
        {
        }
        public object Clone()
        {
            NFX_NOISE_Add fx = new NFX_NOISE_Add();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();

            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return value.ToString("0.000");
            if (index == 1) return value.ToString("0.000");
            if (index == 2) return value.ToString("0.000");
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", false, 0, 1);
            if (index == 1) return new Tuple<float, string, bool, float, float>(0.01f, "0.00", true, 0, 1000);
            if (index == 2) return new Tuple<float, string, bool, float, float>(0.01f, "0.00", true, 0, 1);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Amplitude";
            if (index == 1) return "OSCs per beat";
            if (index == 2) return "Phase (0 to 1)";
            return "[WRONG NUMBER]";
        }

        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXFloatValueCount) return FXValues[index];
            return null;
        }
        public void Init()
        {
        }

        public NFX_NOISE_Add()
        {
            FXValues[0] = new FXValue(1);
            FXValues[1] = new FXValue(0.1f);
            FXValues[2] = new FXValue(0);

            Wave = new float[128];

            Random r = new Random();

            for (int i = 0; i < 32; i++)
            {
                float amp = (float)(r.NextDouble() / Math.Sqrt(i + 1));
                float phase = (float)(r.NextDouble() * Math.PI * 2);

                for (int j = 0; j < 128; j++)
                {
                    Wave[j] += amp * (float)Math.Sin(phase + (float)(i * j / 64f * Math.PI));
                }
            }
            float max = 0;
            for (int j = 0; j < 128; j++) if (Math.Abs(Wave[j]) > max) max = Math.Abs(Wave[j]);
            for (int j = 0; j < 128; j++) Wave[j] /= max;
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public void ShowEditForm()
        {
        }

        float rang = 0;
        float osc = 1;
        float ph = 2;
        float tb = 0;
        float[] Wave = new float[128];

        public void UpdateValues(float timeBeat)
        {
            tb = timeBeat;
            rang = FXValues[0].GetValue(timeBeat);
            osc = FXValues[1].GetValue(timeBeat);
            ph = FXValues[2].GetValue(timeBeat);
        }
    }
    public class NFX_NOISE_Mult : INumberFX
    {
        public string DisplayName => "Noise (op *)";
        public ushort Type => 5;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[3];

        public bool HasEditForm => false;
        public int NumberTrackCount => 0;
        public uint GetNumberTrackReference(int index) => 0;
        public void SetNumberTrackReference(int index, uint trackIndex) { }

        public float Apply(float value)
        {
            float phase = (ph + tb * osc) * 128;
            phase %= 128;
            phase += 128;
            phase %= 128;
            int p1 = (int)phase; ;
            int p2 = (p1 + 1) % 128;
            phase %= 1;

            value *= (float)(((1 - phase) * Wave[p1] + phase * Wave[p2]) * rang);
            return value;
        }

        public void ClearInstance()
        {
        }
        public object Clone()
        {
            NFX_NOISE_Mult fx = new NFX_NOISE_Mult();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();

            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return value.ToString("0.000");
            if (index == 1) return value.ToString("0.000");
            if (index == 2) return value.ToString("0.000");
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", false, 0, 1);
            if (index == 1) return new Tuple<float, string, bool, float, float>(0.01f, "0.00", true, 0, 1000);
            if (index == 2) return new Tuple<float, string, bool, float, float>(0.01f, "0.00", true, 0, 1);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Amplitude";
            if (index == 1) return "OSCs per beat";
            if (index == 2) return "Phase (0 to 1)";
            return "[WRONG NUMBER]";
        }

        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXFloatValueCount) return FXValues[index];
            return null;
        }
        public void Init()
        {
        }

        public NFX_NOISE_Mult()
        {
            FXValues[0] = new FXValue(1);
            FXValues[1] = new FXValue(0.1f);
            FXValues[2] = new FXValue(0);

            Wave = new float[128];

            Random r = new Random();

            for (int i = 0; i < 32; i++)
            {
                float amp = (float)(r.NextDouble() / Math.Sqrt(i + 1));
                float phase = (float)(r.NextDouble() * Math.PI * 2);

                for (int j = 0; j < 128; j++)
                {
                    Wave[j] += amp * (float)Math.Sin(phase + (float)(i * j / 64f * Math.PI));
                }
            }
            float max = 0;
            for (int j = 0; j < 128; j++) if (Math.Abs(Wave[j]) > max) max = Math.Abs(Wave[j]);
            for (int j = 0; j < 128; j++) Wave[j] /= max;
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public void ShowEditForm()
        {
        }

        float rang = 0;
        float osc = 1;
        float ph = 2;
        float tb = 0;
        float[] Wave = new float[128];

        public void UpdateValues(float timeBeat)
        {
            tb = timeBeat;
            rang = FXValues[0].GetValue(timeBeat);
            osc = FXValues[1].GetValue(timeBeat);
            ph = FXValues[2].GetValue(timeBeat);
        }
    }

    /*public class NFX_HitAdd : INumberFX
    {
        public string DisplayName => "Hit (op +)";
        public ushort Type => 9;
        public int FXFloatValueCount => FXValues.Length;
        FXValue[] FXValues = new FXValue[3];

        public bool HasEditForm => false;
        public int NumberTrackCount => 1;
        public uint GetNumberTrackReference(int index) => TrackIndex;
        public void SetNumberTrackReference(int index, uint trackIndex) { TrackIndex = trackIndex; }

        uint TrackIndex;

        float lastVal;
        float lastBeat;
        float normval0 = 0;
        float normval1 = 1;
        float normval2 = 2;
        float tb = 0;
        bool goUp = false;
        public float Apply(float value)
        {
            if (tb < lastBeat)
            {
                lastBeat = tb;
                lastVal = 0;
                return value;
            }
            else
            {
                if (Form1.Project.NumberTracks[TrackIndex].HitCount > 0) goUp = true;
                Form1.Project.NumberTracks[TrackIndex].HitCount = 0;
                float timeLeft = tb - lastBeat;
                if (goUp && lastVal < normval0)
                {
                    lastVal += timeLeft * normval1;
                    timeLeft = 0;
                    if (lastVal > normval0)
                    {
                        timeLeft = normval1 / (lastVal - normval0);
                        lastVal = normval0;
                        goUp = false;
                    }
                    
                }
                if (lastVal > 0 && !goUp)
                {
                    lastVal = normval0 - normval2 * timeLeft;
                    if (lastVal * normval2 < 0)
                    {
                        lastVal = 0;
                    }
                }
            }

            value += lastVal;
            return value;
        }

        public void ClearInstance()
        {
            lastVal = 0;
            lastBeat = 0;
        }
        public object Clone()
        {
            NFX_HitAdd fx = new NFX_HitAdd();
            for (int i = 0; i < FXFloatValueCount; i++) fx.FXValues[i] = (FXValue)FXValues[i].Clone();
            for (int i = 0; i < NumberTrackCount; i++)
            {
                Form1.Project.NumberTracks[fx.GetNumberTrackReference(i)] = (NumberTrack)Form1.Project.NumberTracks[GetNumberTrackReference(i)].Clone();
            }

            return fx;
        }
        public string ConvertFXFToNumber(int index, float value)
        {
            if (index == 0) return value.ToString("0.000");
            if (index == 1) return value.ToString("0.000");
            if (index == 2) return value.ToString("0.000");
            return "[WRONG NUMBER]";
        }
        public Tuple<float, string, bool, float, float> GetValues(int index)
        {
            if (index == 0) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", false, 0, 1);
            if (index == 1) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", false, 0, 1);
            if (index == 2) return new Tuple<float, string, bool, float, float>(0.001f, "0.000", false, 0, 1);
            return new Tuple<float, string, bool, float, float>(0.001f, "0.000", true, 0, 1);
        }
        public void Dispose()
        {
        }
        public string GetAutomatedValueName(int index)
        {
            if (index == 0) return "Value on hit";
            if (index == 1) return "Go up speed (unit/beat)";
            if (index == 2) return "Go down speed (unit/beat)";
            return "[WRONG NUMBER]";
        }

        public FXValue GetFloatValueReference(int index)
        {
            if (index >= 0 && index < FXFloatValueCount) return FXValues[index];
            return null;
        }
        public void Init()
        {
        }

        public NFX_HitAdd()
        {
            NumberTrack dnt = new NumberTrack();
            TrackIndex = Form1.Project.AddNumberTrack(dnt);
            FXValues[0] = new FXValue(0.1f);
            FXValues[1] = new FXValue(1f);
            FXValues[2] = new FXValue(0.1f);
        }

        public void SaveData(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)0);
        }
        public void LoadData(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public void ShowEditForm()
        {
        }

        public void UpdateValues(float timeBeat)
        {
            Form1.Project.NumberTracks[TrackIndex].Read(timeBeat);
            normval0 = FXValues[0].GetValue(timeBeat);
            normval1 = FXValues[1].GetValue(timeBeat);
            normval2 = FXValues[2].GetValue(timeBeat);
            lastBeat = tb;
            tb = timeBeat;
        }
    }*/
}
