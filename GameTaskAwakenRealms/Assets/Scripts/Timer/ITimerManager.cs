using System;

namespace Timer
{
    public interface ITimerManager
    {
        public Guid RunDelayedAction(Action action, float delay);
        public void CancelDelayedAction(Guid delayedActionSwitchStateThrow);
    }
}