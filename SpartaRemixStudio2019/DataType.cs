using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpartaRemixStudio2019
{
    public class AutomationPoint
    {
        public long time;
        public float valueA;
        public float valueB;
        public uint interpolationMethod;

        public void Save(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, time);
            StreamHelper.SaveBytes(fs, valueA);
            StreamHelper.SaveBytes(fs, valueB);
            StreamHelper.SaveBytes(fs, interpolationMethod);
        }
        public void Load(FileStream fs)
        {
            time = StreamHelper.LoadLong(fs);
            valueA = StreamHelper.LoadFloat(fs);
            valueB = StreamHelper.LoadFloat(fs);
            interpolationMethod = StreamHelper.LoadUInt(fs);
        }
    }
    public class Automation : ICloneable
    {
        public long timeBase = 192;
        public List<AutomationPoint> points = new List<AutomationPoint>();

        public void Sort() => points.Sort((ap1, ap2) => (int)(ap1.time - ap2.time));
        public void Upscale(int factor)
        {
            timeBase *= factor;
            foreach (AutomationPoint ap in points) ap.time *= factor;
        }
        public void Downscale(int factor)
        {
            timeBase /= factor;
            foreach (AutomationPoint ap in points) ap.time /= factor;
        }
        public float GetValue(float timeBeat)
        {
            if (points.Count == 0) return 0;

            timeBeat *= timeBase;
            int index = 0;
            foreach (AutomationPoint ap in points)
            {
                if (ap.time > timeBeat) break;
                else index++;
            }

            float proc = 0;

            if (index == 0) proc = points[0].valueA;
            else if (index == points.Count) proc = points[index - 1].valueB;
            else
            {
                float p = (timeBeat - points[index - 1].time) / (points[index].time - points[index - 1].time);
                proc = p * points[index].valueA + (1 - p) * points[index - 1].valueB;
            }
            return proc;
        }

        public object Clone()
        {
            Automation a = new Automation();
            a.timeBase = timeBase;
            a.points = new List<AutomationPoint>();

            foreach (AutomationPoint ap in points)
            {
                a.points.Add(new AutomationPoint() { valueA = ap.valueA, valueB = ap.valueB,
                    interpolationMethod = ap.interpolationMethod, time = ap.time });
            }

            return a;
        }
        
        // SAVE AP
        public void Save(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, timeBase);
            StreamHelper.SaveBytes(fs, points.Count);

            foreach (AutomationPoint ap in points)
            {
                ap.Save(fs);
            }
        }
        public void Load(FileStream fs)
        {
            timeBase = StreamHelper.LoadLong(fs);
            int c = StreamHelper.LoadInt(fs);

            points.Clear();

            for (int i = 0; i < c; i++)
            {
                AutomationPoint ap = new AutomationPoint();
                ap.Load(fs);
                points.Add(ap);
            }
        }

        public Automation(float f)
        {
            AutomationPoint ap = new AutomationPoint();
            ap.valueA = f;
            ap.valueB = f;
            ap.time = 0;
            ap.interpolationMethod = 0;
            points.Add(ap);
        }
        public Automation()
        {

        }
    }

    // 00 - VIDEO,AUDIO
    // 01 - FLOAT 1
    // 02 - FLOAT 2
    // 03 - FLOAT 3
    // 04 - FLOAT 4
    // 05 - MATRIX 4
    // 06 - LINEAR
    // 07 - STRING
    // 08 - BOOL

    public struct LinearCombination
    {
        public Dictionary<int, float> Coefs;

        public LinearCombination(Tuple<int, float>[] coefs)
        {
            Coefs = new Dictionary<int, float>();
            foreach (Tuple<int, float> t in coefs)
            {
                if (!Coefs.ContainsKey(t.Item1)) Coefs.Add(t.Item1, 0);
                Coefs[t.Item1] += t.Item2;
            }
        }
        public LinearCombination(int coef)
        {
            Coefs = new Dictionary<int, float>();
            Coefs.Add(coef, 1);
        }
        public LinearCombination(int coef1, int coef2, float p)
        {
            Coefs = new Dictionary<int, float>();
            if (coef1 != coef2)
            {
                Coefs.Add(coef1, 1 - p);
                Coefs.Add(coef2, p);
            }
            else Coefs.Add(coef1, 1);
        }

        public static LinearCombination Zero => new LinearCombination(new Tuple<int, float>[0]);
    }

    public class PatternBase : ICloneable
    {
        // DEFINICE
        public List<Note> Notes;
        public long TimeBase;

        //CONS
        public PatternBase()
        {
            Notes = new List<Note>();
            TimeBase = 192;
        }

        // PRACE
        public void SortNotes()
        {
            List<Note> notesNew = new List<Note>();

            float TimeBeatMin = 9999999;
            int ListIndex = -1;
            int NewIndex = 0;

            while (Notes.Count > 0)
            {
                TimeBeatMin = 9999999;
                int actIndex = 0;
                foreach (Note n in Notes)
                {
                    if (n.BeatNoteOn < TimeBeatMin)
                    {
                        TimeBeatMin = n.BeatNoteOn;
                        ListIndex = actIndex;
                    }
                    actIndex++;
                }
                if (Notes[ListIndex].BeatNoteOn < Notes[ListIndex].BeatNoteOff)
                {
                    notesNew.Add(Notes[ListIndex]);
                    Notes.RemoveAt(ListIndex);
                    notesNew[NewIndex].SetIndex(NewIndex);
                    NewIndex++;
                }
            }
            Notes = notesNew;
        }
        /*
        public Note ColideMedia(int X, int ptch, int width, float from, float lenght, ref int BorderLeft, ref int BorderRight, long AddLenght, long RoundPos)
        {
            float timeBeat = from;

            timeBeat += ((float)lenght * X / width) * ti;

            foreach (Note itl in Notes)
            {
                if (timeBeat >= itl.BeatNoteOn && timeBeat < itl.BeatNoteOff && ptch == itl.Pitch)
                {
                    int pixelS = (int)((((float)itl.BeatNoteOn - from) / lenght) * width);
                    int pixelE = (int)((((float)itl.BeatNoteOff - from) / lenght) * width);

                    BorderLeft = X - pixelS;
                    BorderRight = pixelE - X;

                    return itl;
                }
            }
            BorderLeft = 999;
            BorderRight = 999;
            Note n = new Note(ptch, (int)(timeBeat), (int)(timeBeat) + AddLenght, Notes.Count, 0);
            Notes.Add(n);
            return n;
        }
        public Note ColideMedia(long ptch, float tb, ref long tbs, ref long tbe, ref bool found)
        {
            foreach (Note n in Notes)
            {
                if (ptch == n.Pitch && tb >= n.BeatNoteOn && tb < n.BeatNoteOff)
                {
                    tbs = n.BeatNoteOn;
                    tbe = n.BeatNoteOff;
                    found = true;

                    return n;
                }
            }
            tbs = -1;
            tbe = -1;
            found = false;
            return null;
        }
        */
        // SAVE
        public void Save(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)2);

            StreamHelper.SaveBytes(fs, TimeBase);
            StreamHelper.SaveBytes(fs, Notes.Count);

            foreach (Note n in Notes)
            {
                n.Save(fs);
            }

            
        }
        public void Load(FileStream fs)
        {

            int l2 = StreamHelper.LoadInt(fs);
            if (l2 > 0)
            {
                TimeBase = StreamHelper.LoadLong(fs);
                l2--;
            }
            if (l2 > 0)
            {
                int l1 = StreamHelper.LoadInt(fs);
                Notes.Clear();

                for (int i = 0; i < l1; i++)
                {
                    Note n = new Note(0, 0, 0, 0, 0, 0);
                    n.Load(fs);
                    Notes.Add(n);
                }
                l2--;
            }
            if (l2 > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }

            SortNotes();
        }

        public object Clone()
        {
            PatternBase pb2 = new PatternBase();
            pb2.Notes = new List<Note>();
            pb2.TimeBase = TimeBase;
            foreach (Note n in Notes)
            {
                pb2.Notes.Add((Note)n.Clone());
            }
            return pb2;
        }
    }
    public class Pattern : ICloneable
    {
        //DEFINICE
        public PatternBase ActivationTrack;
        public Dictionary<byte, Automation> PropertyTracks;
        public string Name = "";

        public void ApplyPropertiesToSample(IPitchReader ipr, float time)
        {
            foreach (KeyValuePair<byte, Automation> k in PropertyTracks)
            {
                ipr.SetProperty(k.Key, k.Value.GetValue(time));
            }
        }
        public void ApplyPropertiesToSample(IVideoReader ivr, float time)
        {
            foreach (KeyValuePair<byte, Automation> k in PropertyTracks)
            {
                ivr.SetProperty(k.Key, k.Value.GetValue(time));
            }
        }

        public Pattern()
        {
            ActivationTrack = new PatternBase();
            PropertyTracks = new Dictionary<byte, Automation>();
        }

        public void Save(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, 3);

            ActivationTrack.Save(fs);

            StreamHelper.SaveBytes(fs, PropertyTracks.Count);
            foreach (KeyValuePair<byte, Automation> k in PropertyTracks)
            {
                fs.WriteByte(k.Key);
                k.Value.Save(fs);
            }

            StreamHelper.SaveString(fs, Name);
        }
        public void Load(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);

            if (lenght > 0)
            {
                ActivationTrack.Load(fs);
                lenght--;
            }

            if (lenght > 0)
            {
                int l1 = StreamHelper.LoadInt(fs);
                PropertyTracks.Clear();

                for (int i = 0; i < l1; i++)
                {
                    byte b = (byte)fs.ReadByte();
                    Automation p = new Automation();
                    p.Load(fs);
                    PropertyTracks.Add(b, p);
                }
                lenght--;
            }
            if (lenght > 0)
            {
                Name = StreamHelper.LoadString(fs);
                lenght--;
            }
            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }

        public object Clone()
        {
            Pattern p = new Pattern();
            p.Name = Name;
            foreach (KeyValuePair<byte, Automation> k in PropertyTracks)
            {
                p.PropertyTracks.Add(k.Key, (Automation)PropertyTracks[k.Key].Clone());
            }
            p.ActivationTrack = (PatternBase)ActivationTrack.Clone();
            return p;
        }

        public int SampleCount
        {
            get
            {
                int max = 0;
                foreach (Note n in ActivationTrack.Notes) if (n.Sample > max) max = (int)n.Sample;
                return max + 1;
            }
        }
    }
    public class Note : ICloneable
    {
        public Note(long pitch, long sample, long timeOn, long timeOff, long index, long vs)
        {
            BeatNoteOn = timeOn;
            BeatNoteOff = timeOff;
            NoteIndex = index;

            VisualStyle = vs;
            Pitch = pitch;
            Sample = sample;

            volume = 0;
            pan = 0;
            opacity = 1;
        }

        public long BeatNoteOn;
        public long BeatNoteOff;
        public long NoteIndex;

        public long VisualStyle;
        public long Sample;
        public long Pitch;

        public float volume;
        public float opacity;
        public float pan;

        public void SetIndex(int i) => NoteIndex = i;

        public void Save(FileStream fs)
        {
            StreamHelper.SaveBytes(fs, (int)9);

            StreamHelper.SaveBytes(fs, Pitch);
            StreamHelper.SaveBytes(fs, Sample);
            StreamHelper.SaveBytes(fs, VisualStyle);

            StreamHelper.SaveBytes(fs, BeatNoteOn);
            StreamHelper.SaveBytes(fs, BeatNoteOff);
            StreamHelper.SaveBytes(fs, NoteIndex);

            StreamHelper.SaveBytes(fs, volume);
            StreamHelper.SaveBytes(fs, opacity);
            StreamHelper.SaveBytes(fs, pan);
        }
        public void Load(FileStream fs)
        {
            int lenght = StreamHelper.LoadInt(fs);

            if (lenght > 0) { Pitch = StreamHelper.LoadLong(fs); lenght--; }
            if (lenght > 0) { Sample = StreamHelper.LoadLong(fs); lenght--; }
            if (lenght > 0) { VisualStyle = StreamHelper.LoadLong(fs); lenght--; }

            if (lenght > 0) { BeatNoteOn = StreamHelper.LoadLong(fs); lenght--; }
            if (lenght > 0) { BeatNoteOff = StreamHelper.LoadLong(fs); lenght--; }
            if (lenght > 0) { NoteIndex = StreamHelper.LoadLong(fs); lenght--; }

            if (lenght > 0) { volume = StreamHelper.LoadFloat(fs); lenght--; }
            if (lenght > 0) { opacity = StreamHelper.LoadFloat(fs); lenght--; }
            if (lenght > 0) { pan = StreamHelper.LoadFloat(fs); lenght--; }

            if (lenght > 0) { MessageBox.Show("Project possibly from higher version detected. Can be opened, but possible problems may arise."); }
        }
        public object Clone()
        {
            Note n = new Note(Pitch, Sample, BeatNoteOn, BeatNoteOff, NoteIndex, VisualStyle);
            n.volume = volume;
            n.opacity = opacity;
            n.pan = pan;
            return n;
        }
    }

    public class PatternReader2
    {
        private static long globalOffset = 0;
        int positionInList = 0;

        long indexOffset = 0;
        float TimeBeatOffSet = 0;
        Pattern referedPattern = null;
        INoteReciever2 recipient = null;
        float ConsideredTBMin = 0;
        float ConsideredTBMax = 0;

        List<int> activeNotes = new List<int>();
        List<int> toStop = new List<int>();

        public float GetProperty(byte key, float timeBeat)
        {
            if (referedPattern != null)
            {
                if (referedPattern.PropertyTracks.ContainsKey(key))
                {
                    return referedPattern.PropertyTracks[key].GetValue(timeBeat);
                }
            }
            if (key == 48) return 1;
            if (key == 2) return 1;
            else return 0;
        }

        public void GetValue(float TimeBeatPrev, float TimeBeat)
        {
            foreach (KeyValuePair<byte,Automation> k in referedPattern.PropertyTracks) recipient.SetProperty(k.Key, k.Value.GetValue(TimeBeat));
            if (referedPattern != null)
            {
                TimeBeatPrev -= TimeBeatOffSet;
                TimeBeat -= TimeBeatOffSet;
                TimeBeatPrev *= referedPattern.ActivationTrack.TimeBase;
                TimeBeat *= referedPattern.ActivationTrack.TimeBase;

                if (TimeBeat > ConsideredTBMax * referedPattern.ActivationTrack.TimeBase)
                {
                    foreach (int i in activeNotes)
                    {
                        toStop.Add(i);
                        recipient?.NoteOff(i + indexOffset);
                    }
                    foreach (int i in toStop)
                    {
                        activeNotes.Remove(i);
                    }
                    toStop.Clear();
                }
                else
                {
                    while (positionInList < referedPattern.ActivationTrack.Notes.Count)
                    {
                        if (referedPattern.ActivationTrack.Notes[positionInList].BeatNoteOff < ConsideredTBMin)
                        {
                            positionInList++;
                        }
                        else if (referedPattern.ActivationTrack.Notes[positionInList].BeatNoteOn < TimeBeat)
                        {
                            activeNotes.Add(positionInList);
                            recipient?.NoteOn(referedPattern.ActivationTrack.Notes[positionInList], indexOffset + positionInList);
                            positionInList++;
                        }
                        else break;
                    }
                    foreach (int i in activeNotes)
                    {
                        if (referedPattern.ActivationTrack.Notes[i].BeatNoteOff < TimeBeat)
                        {
                            toStop.Add(i);
                            recipient?.NoteOff(i + indexOffset);
                        }
                    }
                    foreach (int i in toStop)
                    {
                        activeNotes.Remove(i);
                    }
                    toStop.Clear();
                }
            }
        }
        public PatternReader2(float ConsideredTBMin, float ConsideredTBMax, float TimeBeatOffSet, Pattern reference, INoteReciever2 rec)
        {
            indexOffset = 0;
            globalOffset += 65536;
            this.TimeBeatOffSet = TimeBeatOffSet;
            referedPattern = reference;
            recipient = rec;
            
            this.ConsideredTBMax = ConsideredTBMax;
            this.ConsideredTBMin = ConsideredTBMin;
        }
        public void ApplyPropertiesToSample(IPitchReader ipr, float time)
        {
            referedPattern.ApplyPropertiesToSample(ipr, time);
        }
        public void ApplyPropertiesToSample(IVideoReader ivr, float time)
        {
            referedPattern.ApplyPropertiesToSample(ivr, time);
        }
        public void StopAll()
        {
            foreach (int i in activeNotes)
            {
                recipient?.NoteOff(i + indexOffset);
            }
            activeNotes.Clear();
        }
    }
    public interface INoteReciever2
    {
        void NoteOn(Note n, long index);
        void NoteOff(long index);
        void SetProperty(byte index, float value);
    }
}
