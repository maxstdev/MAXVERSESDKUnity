using UnityEngine;

public interface IPov
{
    string Name { get; }

    string Spot { get; }
    
    Matrix4x4 WorldToLocalMatrix { get; }

    Vector3 WorldPosition { get; }
}
