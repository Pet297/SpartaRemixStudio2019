using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpartaRemixStudio2019
{
    public class Freeverb2
    {
        float[] DelayBuffer;
        float[] WorkBuffer;

        //ALOC
        float samp;
        float samp2;
        int[] pts;
        int[] ptstart;
        int[] ptend;
        int[] ptlenght;

        public Freeverb2()
        {
            DelayBuffer = new float[25450];
            WorkBuffer = new float[0];
            pts = new int[24] { 0,1557,3174,4665,6087,7364,8720,9908,
            11024,12604,14244,15758,17203,18503,19882,21093,
            22232,22457,23013,23454,23795,24043,24622,25086};
            ptstart = new int[24] { 0,1557,3174,4665,6087,7364,8720,9908,
            11024,12604,14244,15758,17203,18503,19882,21093,
            22232,22457,23013,23454,23795,24043,24622,25086};
            ptend = new int[24] { 1557,3174,4665,6087,7364,8720,9908,
            11024,12604,14244,15758,17203,18503,19882,21093,
            22232,22457,23013,23454,23795,24043,24622,25086,25450};
            ptlenght = new int[24] { 1557, 1617, 1491, 1422, 1277, 1356, 1188, 1116,
            1557 + 23, 1617 + 23, 1491 + 23, 1422 + 23, 1277 + 23, 1356 + 23, 1188 + 23, 1116 + 23,
            225, 556, 441, 341, 225 + 23, 556 + 23, 441 + 23, 341 + 23};
        }

        public void ProccessR(ref float[] samples, float f, float d, float g, float pre)
        {
            if (WorkBuffer.Length != samples.Length) WorkBuffer = new float[samples.Length];
            Array.Clear(WorkBuffer, 0, samples.Length);
            samp = 0;
            samp2 = 0;
            // LFBC 8 - 15
            for (int i = 1; i < samples.Length; i += 2) samples[i] *= pre;
            for (int i = 1; i < samples.Length; i += 2)
            {
                for (int j = 8; j < 16; j++)
                {
                    if (pts[j] > ptstart[j]) samp = samples[i] + (DelayBuffer[pts[j]] * (1 - d) + DelayBuffer[pts[j] - 1] * d) * f;
                    else samp = samples[i] + (DelayBuffer[pts[j]] * (1 - d) + DelayBuffer[ptend[j] - 1] * d) * f;

                    if (samp < 1e-15f && samp > -1e-15f ) samp = 0;

                    DelayBuffer[pts[j]] = samp;
                    pts[j]++;
                    if (pts[j] == ptend[j]) pts[j] -= ptlenght[j];

                    WorkBuffer[i] += samp;
                }
                // Ted vse ve workbufferu
            }
            // APF 4 - 6
            for (int j = 20; j < 23; j++)
            {
                for (int i = 1; i < samples.Length; i += 2)
                {
                    samp = DelayBuffer[pts[j]] - g * WorkBuffer[i];
                    samp2 = samp * g + WorkBuffer[i];
                    if (samp < 1e-15f && samp > -1e-15f) samp = 0;
                    if (samp2 < 1e-15f && samp2 > -1e-15f) samp2 = 0;
                    DelayBuffer[pts[j]] = samp2;
                    pts[j]++;
                    if (pts[j] == ptend[j]) pts[j] -= ptlenght[j];
                    WorkBuffer[i] = samp;
                }
            }
            // APF 7
            for (int i = 1; i < samples.Length; i += 2)
            {
                samp = DelayBuffer[pts[23]] - g * WorkBuffer[i];
                samp2 = samp * g + WorkBuffer[i];
                if (samp < 1e-15f && samp > -1e-15f) samp = 0;
                if (samp2 < 1e-15f && samp2 > -1e-15f) samp2 = 0;
                DelayBuffer[pts[23]] = samp2;
                pts[23]++;
                if (pts[23] == ptend[23]) pts[23] -= ptlenght[23];
                samples[i] = samp;
            }
        }
        public void ProccessL(ref float[] samples, float f, float d, float g, float pre)
        {
            samp = 0;
            samp2 = 0;
            if (WorkBuffer.Length != samples.Length) WorkBuffer = new float[samples.Length];
            Array.Clear(WorkBuffer, 0, samples.Length);
            // LFBC 0 - 7
            for (int i = 0; i < samples.Length; i += 2) samples[i] *= pre;
            for (int i = 0; i < samples.Length; i += 2)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (pts[j] > ptstart[j]) samp = samples[i] + (DelayBuffer[pts[j]] * (1 - d) + DelayBuffer[pts[j] - 1] * d) * f;
                    else samp = samples[i] + (DelayBuffer[pts[j]] * (1 - d) + DelayBuffer[ptend[j] - 1] * d) * f;

                    if (samp < 1e-15f && samp > -1e-15f) samp = 0;

                    DelayBuffer[pts[j]] = samp;
                    pts[j]++;
                    if (pts[j] == ptend[j]) pts[j] -= ptlenght[j];

                    WorkBuffer[i] += samp;
                }
            }
            // APF 0 - 2
            for (int j = 16; j < 19; j++)
            {
                for (int i = 0; i < samples.Length; i += 2)
                {
                    samp = DelayBuffer[pts[j]] - g * WorkBuffer[i];
                    samp2 = samp * g + WorkBuffer[i];
                    if (samp < 1e-15f && samp > -1e-15f) samp = 0;
                    if (samp2 < 1e-15f && samp2 > -1e-15f) samp2 = 0;
                    DelayBuffer[pts[j]] = samp2;
                    pts[j]++;
                    if (pts[j] == ptend[j]) pts[j] -= ptlenght[j];
                    WorkBuffer[i] = samp;
                }
            }
            // APF 3
            for (int i = 0; i < samples.Length; i += 2)
            {
                samp = DelayBuffer[pts[19]] - g * WorkBuffer[i];
                samp2 = samp * g + WorkBuffer[i];
                if (samp < 1e-15f && samp > -1e-15f) samp = 0;
                if (samp2 < 1e-15f && samp2 > -1e-15f) samp2 = 0;
                DelayBuffer[pts[19]] = samp2;
                pts[19]++;
                if (pts[19] == ptend[19]) pts[19] -= ptlenght[19];
                samples[i] = samp;
            }
        }
        public void Clear()
        {
            DelayBuffer = new float[25450];
            pts = new int[24] { 0,1557,3174,4665,6087,7364,8720,9908,
            11024,12604,14244,15758,17203,18503,19882,21093,
            22232,22457,23013,23454,23795,24043,24622,25086};
        }
    }
    public class TwoPoleFilter
    {
        public float a2;
        public float a1;
        public float b0;
        public float b1;
        public float b2;

        float smpL0 = 0;
        float smpL1 = 0;
        float smpL2 = 0;
        float outL1 = 0;
        float outL2 = 0;

        float smpR0 = 0;
        float smpR1 = 0;
        float smpR2 = 0;
        float outR1 = 0;
        float outR2 = 0;

        public void ProccessL(ref float[] samples)
        {
            samples[0] = b0 * samples[0] + b1 * smpL1 + b2 * smpL2 - a1 * outL1 - a2 * outL2;

            //i==0
            smpL0 = samples[0];
            samples[0] = b0 * samples[0] + b1 * smpL1 + b2 * smpL2 - a1 * outL1 - a2 * outL2;
            smpL2 = smpL1;
            smpL1 = smpL0;
            //i==2
            smpL0 = samples[0 + 2];
            samples[0 + 2] = b0 * samples[0 + 2] + b1 * smpL1 + b2 * smpL2 - a1 * samples[0] - a2 * outL1;
            smpL2 = smpL1;
            smpL1 = smpL0;

            for (int i = 4; i < samples.Length; i += 2)
            {
                smpL0 = samples[i];
                samples[i] = b0 * samples[i] + b1 * smpL1 + b2 * smpL2 - a1 * samples[i - 2] - a2 * samples[i - 4];
                smpL2 = smpL1;
                smpL1 = smpL0;
            }

            outL2 = samples[samples.Length - 4];
            outL1 = samples[samples.Length - 2];
        }
        public void ProccessR(ref float[] samples)
        {
            samples[1] = b0 * samples[1] + b1 * smpR1 + b2 * smpR2 - a1 * outR1 - a2 * outR2;

            //i==0
            smpR0 = samples[1];
            samples[1] = b0 * samples[1] + b1 * smpR1 + b2 * smpR2 - a1 * outR1 - a2 * outR2;
            smpR2 = smpR1;
            smpR1 = smpR0;
            //i==2
            smpR0 = samples[3];
            samples[3] = b0 * samples[3] + b1 * smpR1 + b2 * smpR2 - a1 * samples[1] - a2 * outR1;
            smpR2 = smpR1;
            smpR1 = smpR0;

            for (int i = 5; i < samples.Length; i += 2)
            {
                smpR0 = samples[i];
                samples[i] = b0 * samples[i] + b1 * smpR1 + b2 * smpR2 - a1 * samples[i - 2] - a2 * samples[i - 4];
                smpR2 = smpR1;
                smpR1 = smpR0;
            }

            outR2 = samples[samples.Length - 3];
            outR1 = samples[samples.Length - 1];
        }
        public void Clear()
        {
            smpL0 = 0;
            smpL1 = 0;
            smpL2 = 0;
            outL1 = 0;
            outL2 = 0;
            smpR0 = 0;
            smpR1 = 0;
            smpR2 = 0;
            outR1 = 0;
            outR2 = 0;
        }

        public void SetResonator(float R, float Hz)
        {
            a1 = -(2 * R) * (float)Math.Cos(2 * Math.PI * Hz / 48000f);
            a2 = R * R;
            b0 = (1 - a2) / 2;
            b1 = 0;
            b2 = -b0;
        }
    }
    public class LowPassFilter
    {
        float outL1 = 0;
        float outR1 = 0;
        float alpha = 0;

        public void ProccessL(ref float[] samples, float frequency)
        {
            alpha = (1 / 48000f) / (1 / 48000f + (1 / (2f * (float)Math.PI * frequency)));

            samples[0] = alpha * samples[0] + (1 - alpha) * outL1;
            for (int i = 2; i < samples.Length; i += 2)
            {
                samples[i] = alpha * samples[i] + (1 - alpha) * samples[i - 2];
            }
            outL1 = samples[samples.Length - 2];
        }
        public void ProccessR(ref float[] samples, float frequency)
        {
            alpha = (1 / 48000f) / (1 / 48000f + (1 / (2f * (float)Math.PI * frequency)));

            samples[1] = alpha * samples[1] + (1 - alpha) * outR1;
            for (int i = 3; i < samples.Length; i += 2)
            {
                samples[i] = alpha * samples[i] + (1 - alpha) * samples[i - 2];
            }
            outR1 = samples[samples.Length - 1];
        }
        public void ProccessM(ref float[] samples, float frequency)
        {
            alpha = (1 / 48000f) / (1 / 48000f + (1 / (2f * (float)Math.PI * frequency)));

            samples[0] = alpha * samples[0] + (1 - alpha) * outL1;
            for (int i = 1; i < samples.Length; i ++)
            {
                samples[i] = alpha * samples[i] + (1 - alpha) * samples[i - 1];
            }
            outL1 = samples[samples.Length - 2];
        }
        public void Clear()
        {
            outL1 = 0;
            outR1 = 0;
        }
    }
    public class HighPassFilter
    {
        float bufL0 = 0;
        float bufL1 = 0;
        float bufR0 = 0;
        float bufR1 = 0;
        float outL1 = 0;
        float outR1 = 0;
        float alpha = 0;

        public void ProccessL(ref float[] samples, float frequency)
        {
            alpha = 1 / (2f * (float)Math.PI * frequency / 48000f + 1);

            bufL0 = samples[0];
            if (bufL0 < 1e-15f && bufL0 > -1e-15f) bufL0 = 0;
            samples[0] = alpha * (samples[0] - bufL1 + outL1);
            
            bufL1 = bufL0;

            for (int i = 2; i < samples.Length; i += 2)
            {
                bufL0 = samples[i];
                if (bufL0 < 1e-15f && bufL0 > -1e-15f) bufL0 = 0;
                samples[i] = alpha * (samples[i] - bufL1 + samples[i - 2]);
                bufL1 = bufL0;
            }
            outL1 = samples[samples.Length - 2];
            if (outL1 < 1e-15f && outL1 > -1e-15f) outL1 = 0;
        }
        public void ProccessR(ref float[] samples, float frequency)
        {
            alpha = 1 / (2f * (float)Math.PI * frequency / 48000f + 1);

            bufR0 = samples[1];
            if (bufR0 < 1e-15f && bufR0 > -1e-15f) bufR0 = 0;
            samples[1] = alpha * (samples[1] - bufR1 + outR1);
            bufR1 = bufR0;

            for (int i = 3; i < samples.Length; i += 2)
            {
                bufR0 = samples[i];
                if (bufR0 < 1e-15f && bufR0 > -1e-15f) bufR0 = 0;

                samples[i] = alpha * (samples[i] - bufR1 + samples[i - 2]);
                bufR1 = bufR0;
            }
            outR1 = samples[samples.Length - 1];
            if (outR1 < 1e-15f && outR1 > -1e-15f) outR1 = 0;
        }
        public void ProccessM(ref float[] samples, float frequency)
        {
            alpha = (1 / 48000f) / (1 / 48000f + (1 / (2f * (float)Math.PI * frequency)));

            bufL0 = samples[0];
            samples[0] = alpha * (samples[0] - bufL1 + outL1);
            bufL1 = bufL0;

            for (int i = 1; i < samples.Length; i += 1)
            {
                bufL0 = samples[i];
                samples[i] = alpha * (samples[i] - bufL1 + samples[i - 1]);
                bufL1 = bufL0;
            }
            outL1 = samples[samples.Length - 1];
        }
        public void Clear()
        {
            bufL0 = 0;
            bufL1 = 0;
            bufR0 = 0;
            bufR1 = 0;
            outL1 = 0;
            outR1 = 0;
        }
    }
    public class DelayFX
    {
        float[] bufferR;
        float[] bufferL;
        int arrayPos;

        public DelayFX(int maxLenght)
        {
            bufferL = new float[maxLenght];
            bufferR = new float[maxLenght];
            arrayPos = maxLenght - 1;
        }

        public void Clear()
        {
            Array.Clear(bufferR, 0, bufferR.Length);
            Array.Clear(bufferL, 0, bufferL.Length);
        }
        public void ProccessLR(ref float[] samples, int delay, float amt, float pingpongAmt)
        {
            int pl = bufferL.Length + arrayPos - delay;

            for (int i = 0; i < samples.Length; i += 2)
            {
                pl++;
                pl %= bufferL.Length;
                arrayPos++;
                arrayPos %= bufferL.Length;

                if (delay < bufferL.Length)
                {
                    samples[i] += amt * bufferL[pl];
                    samples[i + 1] += amt * bufferR[pl];
                }
                bufferL[arrayPos] = ((1 - pingpongAmt) * samples[i] + pingpongAmt * samples[i + 1]) / (1 + amt);
                if (bufferL[arrayPos] < 1e-10f && bufferL[arrayPos] > -1e-10f) bufferL[arrayPos] = 0;
                bufferR[arrayPos] = ((1 - pingpongAmt) * samples[i + 1] + pingpongAmt * samples[i]) / (1 + amt);
                if (bufferR[arrayPos] < 1e-10f && bufferR[arrayPos] > -1e-10f) bufferR[arrayPos] = 0;
            }
        }
    }
    public class FeedForwardComb
    {
        float[] bufferR;
        float[] bufferL;
        int arrayPos;

        public FeedForwardComb(int maxLenght)
        {
            bufferL = new float[maxLenght];
            bufferR = new float[maxLenght];
            arrayPos = maxLenght - 1;
        }

        public void Clear()
        {
            Array.Clear(bufferR, 0, bufferR.Length);
            Array.Clear(bufferL, 0, bufferL.Length);
        }
        public void ProccessLR(ref float[] samples, int delay, float amt)
        {
            for (int i = 0; i < samples.Length; i += 2)
            {
                arrayPos++;
                arrayPos %= bufferR.Length;

                bufferL[arrayPos] = samples[i];
                bufferR[arrayPos] = samples[i + 1];

                if (delay < bufferR.Length)
                {
                    samples[i] = samples[i] + amt * bufferL[(bufferL.Length + arrayPos - delay) % bufferL.Length];
                    samples[i + 1] = samples[i + 1] + amt * bufferR[(bufferR.Length + arrayPos - delay) % bufferR.Length];
                }
            }
        }
    }
    public class FeedBackComb
    {
        float[] bufferR;
        float[] bufferL;
        int arrayPos;

        public FeedBackComb(int maxLenght)
        {
            bufferL = new float[maxLenght];
            bufferR = new float[maxLenght];
            arrayPos = maxLenght - 1;
        }

        public void Clear()
        {
            Array.Clear(bufferR, 0, bufferR.Length);
            Array.Clear(bufferL, 0, bufferL.Length);
        }
        public void ProccessLR(ref float[] samples, int delay, float amt)
        {
            for (int i = 0; i < samples.Length; i += 2)
            {
                arrayPos++;
                arrayPos %= bufferR.Length;

                if (delay < bufferR.Length)
                {
                    bufferL[arrayPos] = (1 - amt) * samples[i] + amt *  bufferL[(bufferL.Length + arrayPos - delay) % bufferL.Length];
                    bufferR[arrayPos] = (1 - amt) * samples[i + 1] + amt * bufferR[(bufferR.Length + arrayPos - delay) % bufferR.Length];

                    samples[i] = bufferL[arrayPos];
                    samples[i + 1] = bufferR[arrayPos];
                }
                else
                {
                    bufferL[arrayPos] = (1 - amt) * samples[i] + 0;
                    bufferR[arrayPos] = (1 - amt) * samples[i + 1] + 0;

                    samples[i] = bufferL[arrayPos];
                    samples[i + 1] = bufferR[arrayPos];
                }
            }
        }
    }
    public class Flanger
    {
        float[] bufferR;
        float[] bufferL;
        int arrayPos;

        float mid = 0;
        float pos = 0;
        float ax = 0;

        public Flanger(int maxLenght)
        {
            bufferL = new float[maxLenght];
            bufferR = new float[maxLenght];
            arrayPos = maxLenght - 1;
        }

        public void Clear()
        {
            Array.Clear(bufferR, 0, bufferR.Length);
            Array.Clear(bufferL, 0, bufferL.Length);
        }
        public void ProccessLR(ref float[] samples, float HalfRange, float Offset, float phase0, float phase1, float amt)
        {
            for (int i = 0; i < samples.Length; i += 2)
            {
                arrayPos++;
                arrayPos %= bufferR.Length;

                bufferL[arrayPos] = samples[i];
                bufferR[arrayPos] = samples[i + 1];


                pos = 2 * (phase1 - phase0) / samples.Length;
                pos = phase0 + pos * i;
                pos = (float)(Math.Sin(2 * Math.PI * pos) * HalfRange + Offset);
                ax = pos % 1;

                if (pos + 1 < bufferL.Length)
                {
                    samples[i] = samples[i] + (1-ax) * amt * bufferL[(bufferL.Length + arrayPos - (int)pos) % bufferL.Length] + (ax) * amt * bufferL[(bufferL.Length + arrayPos - (int)pos - 1) % bufferL.Length];
                    samples[i + 1] = samples[i + 1] + (1-ax) * amt * bufferR[(bufferR.Length + arrayPos - (int)pos) % bufferR.Length] + (ax) * amt * bufferR[(bufferR.Length + arrayPos - (int)pos - 1) % bufferR.Length];
                }

            }
        }
    }
    public class WahWah
    {
        float[] bufferR;
        float[] bufferL;
        int arrayPos;

        float mid = 0;
        float pos = 0;
        float ax = 0;

        public WahWah(int maxLenght)
        {
            bufferL = new float[maxLenght];
            bufferR = new float[maxLenght];
            arrayPos = maxLenght - 1;
        }

        public void Clear()
        {
            Array.Clear(bufferR, 0, bufferR.Length);
            Array.Clear(bufferL, 0, bufferL.Length);
        }
        public void ProccessLR(ref float[] samples, float MultRange, float MidFreq, float phase0, float phase1, float amt)
        {
            for (int i = 0; i < samples.Length; i += 2)
            {
                arrayPos++;
                arrayPos %= bufferR.Length;

                bufferL[arrayPos] = samples[i];
                bufferR[arrayPos] = samples[i + 1];


                pos = 2 * (phase1 - phase0) / samples.Length;
                pos = phase0 + pos * i;
                pos = 48000f / (float)(MidFreq * Math.Pow(2 , MultRange * Math.Sin(2 * Math.PI * pos)));
                ax = pos % 1;

                if (pos + 1 < bufferL.Length)
                {
                    samples[i] = samples[i] + (1 - ax) * amt * bufferL[(bufferL.Length + arrayPos - (int)pos) % bufferL.Length] + ax * amt * bufferL[(bufferL.Length + arrayPos - (int)pos - 1) % bufferL.Length];
                    samples[i + 1] = samples[i + 1] + (1 - ax) * amt * bufferR[(bufferR.Length + arrayPos - (int)pos) % bufferR.Length] + ax * amt * bufferR[(bufferR.Length + arrayPos - (int)pos - 1) % bufferR.Length];
                }

            }
        }
    }
    public class ADSR
    {
        int A;
        int D;
        float S;
        int R;
        float Lmult = 0;

        bool release = false;

        int part = 0;
        int left = 0;

        public ADSR(float a, float d, float s, float r)
        {
            A = (int)(a * 48000);
            D = (int)(d * 48000);
            S = s;
            R = (int)(r * 48000);
            part = 0;
            left = A;
        }
        public void ProccessLR(ref float[] samples)
        {
            for (int i = 0; i < samples.Length / 2; i++)
            {
                if (left == 0 && part == 0)
                {
                    part++;
                    left = D;
                }
                if (left == 0 && part == 1)
                {
                    part++;
                    left = 0;
                }
                if (left <= 0 && part == 3) part = 4;
                if (R == 0 && part == 3) part = 4;

                if (part == 0)
                {
                    Lmult = (A - (float)left) / A;
                    samples[i * 2 + 0] *= Lmult;
                    samples[i * 2 + 1] *= Lmult;
                }
                else if (part == 1)
                {
                    Lmult = (1 - (1 - S) * (D - (float)left) / D);
                    samples[i * 2 + 0] *= Lmult;
                    samples[i * 2 + 1] *= Lmult;
                }
                else if (part == 2)
                {
                    Lmult = S;
                    samples[i * 2 + 0] *= S;
                    samples[i * 2 + 1] *= S;
                }
                else if (part == 3)
                {
                    samples[i * 2 + 0] *= Lmult *  left / R;
                    samples[i * 2 + 1] *= Lmult *  left / R;
                }
                else if (part == 4)
                {
                    samples[i * 2 + 0] *= 0;
                    samples[i * 2 + 1] *= 0;
                }
                left--;
            }
        }
        public void Release()
        {
            if (!release) left = R;
            part = 3;
            release = true;
        }

        public bool ReleaseEnded => part > 3;
    }


    //---------------------------
    //KOMBINACE

    public class LPRes
    {
        LowPassFilter lpf;
        TwoPoleFilter tpf;

        float[] workBuffer;

        public LPRes()
        {
            lpf = new LowPassFilter();
            tpf = new TwoPoleFilter();
            workBuffer = new float[0];
        }

        public void ProccessLR(ref float[] samples, float freq, float peakAmt, float peakWidth)
        {
            if (workBuffer.Length != samples.Length) workBuffer = new float[samples.Length];

            Array.Copy(samples, 0, workBuffer, 0, samples.Length);

            lpf.ProccessL(ref samples, freq);
            lpf.ProccessR(ref samples, freq);

            tpf.SetResonator(peakWidth, freq);
            tpf.ProccessL(ref workBuffer);
            tpf.ProccessR(ref workBuffer);

            for (int i = 0; i < samples.Length; i++) samples[i] = (1-peakAmt) * samples[i] + peakAmt * workBuffer[i];
        }
        public void Clear()
        {
            lpf.Clear();
            tpf.Clear();
        }
    }
    public class HPRes
    {
        HighPassFilter hpf;
        TwoPoleFilter tpf;

        float[] workBuffer;

        public HPRes()
        {
            hpf = new HighPassFilter();
            tpf = new TwoPoleFilter();
            workBuffer = new float[0];
        }

        public void ProccessLR(ref float[] samples, float freq, float peakAmt, float peakWidth)
        {
            if (workBuffer.Length != samples.Length) workBuffer = new float[samples.Length];

            Array.Copy(samples, 0, workBuffer, 0, samples.Length);

            hpf.ProccessL(ref samples, freq);
            hpf.ProccessR(ref samples, freq);

            tpf.SetResonator(peakWidth, freq);
            tpf.ProccessL(ref workBuffer);
            tpf.ProccessR(ref workBuffer);

            for (int i = 0; i < samples.Length; i++) samples[i] = (1 - peakAmt) * samples[i] + peakAmt * workBuffer[i];
        }
        public void Clear()
        {
            hpf.Clear();
            tpf.Clear();
        }
    }
}
