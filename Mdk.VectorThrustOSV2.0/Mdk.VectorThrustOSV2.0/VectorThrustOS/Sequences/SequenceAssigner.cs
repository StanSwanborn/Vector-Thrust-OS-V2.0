using System.Collections.Generic;

namespace IngameScript.VectorThrustOS.Sequences
{
    internal class SequenceAssigner //Modified SimpleTimerSM by Digi
    {
        public bool AutoStart { get; set; }
        public bool Running { get; private set; }
        public IEnumerable<int> Sequence;
        private IEnumerator<int> sequenceSM;

        public int SequenceCount { get; private set; }
        public bool Doneloop { get; set; }

        public SequenceAssigner(IEnumerable<int> sequence = null, bool autoStart = false)
        {
            Sequence = sequence;
            AutoStart = autoStart;

            if (AutoStart)
            {
                Start();
            }
        }
        public void Start()
        {
            Doneloop = false;
            SetSequenceSM(Sequence);
        }
        public void Run()
        {
            if (sequenceSM == null)
                return;

            if (SequenceCount > 0)
            {
                SequenceCount--;
                return;
            }

            bool hasValue = sequenceSM.MoveNext();

            if (hasValue)
            {
                SequenceCount = sequenceSM.Current;

                if (SequenceCount <= -1)
                    hasValue = false;
            }

            if (!hasValue)
            {
                if (AutoStart)
                    SetSequenceSM(Sequence);
                else
                    SetSequenceSM(null);
            }
        }

        private void SetSequenceSM(IEnumerable<int> seq)
        {
            Running = false;
            SequenceCount = 0;

            sequenceSM?.Dispose();
            sequenceSM = null;

            if (seq != null)
            {
                Running = true;
                sequenceSM = seq.GetEnumerator();
            }
        }

        public bool Loop(bool cond)
        {
            while (!Doneloop)
            {
                Run();
                if (cond) return cond;
            }
            Doneloop = false;
            return Doneloop;
        }
    }
}