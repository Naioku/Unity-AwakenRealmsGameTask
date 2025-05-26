using System;
using UnityEngine;

public interface IDraggable
{
    public Rigidbody Rigidbody { get; }
    public void Drag();
    public void Drop();
}