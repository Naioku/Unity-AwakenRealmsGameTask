using System;
using System.Collections.Generic;
using UnityEngine;

namespace UpdateSystem
{
    public class UpdateManager : IUpdateRegistrar
    {
        private readonly List<Action> _objectsToUpdate = new();
        private readonly List<Action> _objectsToFixedUpdate = new();
        private readonly List<Action> _objectsToLateUpdate = new();
        private int _currentIndex;

        public void RegisterOnUpdate(Action updatableObj) => AddAction(updatableObj, _objectsToUpdate);
        public void RegisterOnFixedUpdate(Action updatableObj) => AddAction(updatableObj, _objectsToFixedUpdate);
        public void RegisterOnLateUpdate(Action updatableObj) => AddAction(updatableObj, _objectsToLateUpdate);

        public void UnregisterFromUpdate(Action updatableObj) => RemoveAction(updatableObj, _objectsToUpdate);
        public void UnregisterFromFixedUpdate(Action updatableObj) => RemoveAction(updatableObj, _objectsToFixedUpdate);
        public void UnregisterFromLateUpdate(Action updatableObj) => RemoveAction(updatableObj, _objectsToLateUpdate);

        public void Update() => InvokeActions(_objectsToUpdate);
        public void FixedUpdate() => InvokeActions(_objectsToFixedUpdate);
        public void LateUpdate() => InvokeActions(_objectsToLateUpdate);

        private void AddAction(Action what, IList<Action> where)
        {
            where.Add(what);
            int index = where.IndexOf(what);
            if (index == -1)
            {
                Debug.LogWarning("Trying to remove action that is not registered");
                return;
            }
            
            if (index <= _currentIndex)
            {
                _currentIndex++;
            }
        }
    
        private void RemoveAction(Action what, IList<Action> from)
        {
            int index = from.IndexOf(what);
            if (index == -1)
            {
                Debug.LogWarning("Trying to remove action that is not registered");
                return;
            }

            from.RemoveAt(index);
            if (index <= _currentIndex)
            {
                _currentIndex--;
            }
        }
    
        private void InvokeActions(IReadOnlyList<Action> actionsList)
        {
            for (_currentIndex = 0; _currentIndex < actionsList.Count; _currentIndex++)
            {
                var updatableObj = actionsList[_currentIndex];
                updatableObj.Invoke();
            }
        }
    }
}