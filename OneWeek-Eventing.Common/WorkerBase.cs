using System;

namespace OneWeek_Eventing.Common
{
    public enum WorkerState
    {
        NotStarted,
        Running,
        Stopped,
        Error
    };

    public class WorkerBase
    {
        private WorkerState _state = WorkerState.NotStarted;

        public WorkerState State
        {
            get
            {
                lock (this)
                {
                    return _state;
                }
            }
            protected set
            {
                lock (this)
                {
                    _state = value;
                }
            }
        }
    }
}
