using System.Collections.Generic;
using InputSystemExtension.ActionMaps;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputSystemExtension
{
    public class InputManager
    {
        private readonly Controls _controls = new();
        private readonly List<ActionMap> _mapsList = new();
        
        public GlobalMap GlobalMap { get; private set; }
        public Vector2 CursorPosition { get; private set; }
        
        public void Awake()
        {
            Managers.Instance.UpdateRegistrar.RegisterOnUpdate(UpdateCursorPosition);
            InitializeMaps();
            BuildMapsList();
            
        }

        public void Destroy()
        {
            Managers.Instance.UpdateRegistrar.UnregisterFromUpdate(UpdateCursorPosition);
        }

        public void DisableAllMaps()
        {
            foreach (ActionMap actionMap in _mapsList)
            {
                actionMap.Disable();
            }
        }

        private void UpdateCursorPosition() => CursorPosition = Mouse.current.position.ReadValue();

        private void InitializeMaps()
        {
            GlobalMap = new GlobalMap(_controls.Global);
        }

        private void BuildMapsList()
        {
            _mapsList.Add(GlobalMap);
        }
    }
}