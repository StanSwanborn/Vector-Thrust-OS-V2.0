using IngameScript.VectorThrustOS.Architecture.Abstractions;
using System.Collections.Generic;
using System;

namespace IngameScript.VectorThrustOS.SequenceRuntime
{
    internal class SequenceAssigner<T> where T : BaseSequence<T> //Modified SimpleTimerSM by Digi, and then modified again by StanSwanborn (2025/06)
    {
        public bool AutoStart { get; set; }
        public bool Running { get; private set; }

        private readonly T _assignedSequence;
        public IEnumerable<int> Sequence;
        private IEnumerator<int> sequenceStateMachine;

        public int SequenceCount { get; private set; }
        public bool Doneloop { get; set; }

        public SequenceAssigner(Func<SequenceAssigner<T>, T> assignedSequenceCtor, bool autoStart = false)
        {
            _assignedSequence = assignedSequenceCtor(this);
            Sequence = _assignedSequence.Process();
            AutoStart = autoStart;

            if (AutoStart) Start();
        }
        public void Start()
        {
            Doneloop = false;
            SetSequenceStateMachine(Sequence);
        }

        public void Run()
        {
            if (sequenceStateMachine == null)
                return;

            if (SequenceCount > 0)
            {
                SequenceCount--;
                return;
            }

            bool hasValue = sequenceStateMachine.MoveNext();

            if (hasValue)
            {
                SequenceCount = sequenceStateMachine.Current;

                if (SequenceCount <= -1)
                    hasValue = false;
            }

            if (!hasValue)
            {
                if (AutoStart)
                    SetSequenceStateMachine(Sequence);
                else
                    SetSequenceStateMachine(null);
            }
        }

        private void SetSequenceStateMachine(IEnumerable<int> seq)
        {
            Running = false;
            SequenceCount = 0;

            sequenceStateMachine?.Dispose();
            sequenceStateMachine = null;

            if (seq != null)
            {
                Running = true;
                sequenceStateMachine = seq.GetEnumerator();
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