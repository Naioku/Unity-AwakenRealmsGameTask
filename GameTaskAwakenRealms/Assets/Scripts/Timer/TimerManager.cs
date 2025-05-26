using System;
using System.Collections.Generic;
using UnityEngine;

namespace Timer
{
    public class TimerManager : ITimerManager
    {
        private readonly Dictionary<Guid, Task> _tasksLookup = new();

        public Guid RunDelayedAction(Action action, float delay)
        {
            var newGuid = Guid.NewGuid();
            _tasksLookup.Add(newGuid, new Task{ action = action, time = delay });
        
            return newGuid;
        }

        public void CancelDelayedAction(Guid delayedActionSwitchStateThrow)
        {
            if (!_tasksLookup.TryGetValue(delayedActionSwitchStateThrow, out Task task)) return;
            
            task.cancelled = true;
        }

        public void Awake()
        {
            Managers.Instance.UpdateRegistrar.RegisterOnUpdate(Update);
        }

        private void Update()
        {
            List<Guid> tasksToRemove = new List<Guid>();
            
            foreach (var entry in _tasksLookup)
            {
                var task = entry.Value;
                
                if (task.cancelled)
                {
                    tasksToRemove.Add(entry.Key);
                    continue;
                }
                
                task.time -= Time.deltaTime;
                
                if (task.time <= 0)
                {
                    task.action.Invoke();
                    tasksToRemove.Add(entry.Key);
                }
            }
            
            foreach (var guid in tasksToRemove)
            {
                _tasksLookup.Remove(guid);
            }
        }

        private class Task
        {
            public Action action;
            public float time;
            public bool cancelled;
        }
    }
}