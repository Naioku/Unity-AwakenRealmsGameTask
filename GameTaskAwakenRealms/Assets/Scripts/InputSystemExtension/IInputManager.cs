using InputSystemExtension.ActionMaps;
using UnityEngine;

namespace InputSystemExtension
{
    public interface IInputManager
    {
        public GlobalMap GlobalMap { get; }
        public Vector2 CursorPosition { get; }
        public void DisableAllMaps();
    }
}