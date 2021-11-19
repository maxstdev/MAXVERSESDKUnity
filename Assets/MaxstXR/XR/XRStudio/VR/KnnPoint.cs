using UnityEngine;
using Unity.Mathematics;
public class KnnPoint : MonoBehaviour
{
    public float3 Point
    {
        get => new float3(transform.position);
    }
}
