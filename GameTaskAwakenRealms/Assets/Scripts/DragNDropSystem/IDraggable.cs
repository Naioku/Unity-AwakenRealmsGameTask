using UnityEngine;

namespace DragNDropSystem
{
    public interface IDraggable
    {
        public Rigidbody Rigidbody { get; }
        public void Drag();
        public void Drop();
    }
}