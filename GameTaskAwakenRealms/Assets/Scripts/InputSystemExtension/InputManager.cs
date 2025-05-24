using System.Collections.Generic;
using InputSystemExtension.ActionMaps;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputSystemExtension
{
    public class InputManager : IInputManager
    {
        private readonly Controls _controls = new();
        private readonly List<ActionMap> _mapsList = new();
        
        public GameplayMap GameplayMap { get; private set; }
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
            GameplayMap = new GameplayMap(_controls.Gameplay);
        }

        private void BuildMapsList()
        {
            _mapsList.Add(GameplayMap);
        }
    }
}