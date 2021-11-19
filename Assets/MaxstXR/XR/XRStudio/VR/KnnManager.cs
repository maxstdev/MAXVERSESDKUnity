using KNN;
using KNN.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using System.Linq;

public partial class KnnManager : MonoBehaviour
{
    public GameObject FindNearest(Vector3 position)
    {
        var queryPosition = new float3(position);

        // Create a native array for input points
        var knnPoints = GetComponentsInChildren<KnnPoint>();
        var l = knnPoints.Length;
        var points = new NativeArray<float3>(l, Allocator.TempJob);
        for (var i = 0; i < points.Length; ++i)
            points[i] = knnPoints[i].Point;

        // Create a container, i.e., a K-d tree
        var container = new KnnContainer(points, false, Allocator.TempJob);
        new KnnRebuildJob(container).Schedule().Complete();

        // Create a native list for output indices
        var result = new NativeArray<int>(1, Allocator.TempJob);

        new QueryKNearestJob(container, queryPosition, result).Schedule().Complete();

        var nearest = result.First();

        // Cleanup
        result.Dispose();
        container.Dispose();
        points.Dispose();

        return knnPoints[nearest].gameObject;
    }

    public GameObject[] FindNearestK(Vector3 position, int k)
    {
        var queryPosition = new float3(position);

        // Create a native array for input points
        var knnPoints = GetComponentsInChildren<KnnPoint>();
        var l = knnPoints.Length;
        var points = new NativeArray<float3>(l, Allocator.TempJob);
        for (var i = 0; i < points.Length; ++i)
            points[i] = knnPoints[i].Point;

        // Create a container, i.e., a K-d tree
        var container = new KnnContainer(points, false, Allocator.TempJob);
        new KnnRebuildJob(container).Schedule().Complete();

        // Create a native list for output indices
        var result = new NativeArray<int>(k, Allocator.TempJob);

        new QueryKNearestJob(container, queryPosition, result).Schedule().Complete();

        var gameObjects = knnPoints.Where((p, i) => result.Contains(i))
            .Select(p => p.gameObject).ToArray();

        // Cleanup
        result.Dispose();
        container.Dispose();
        points.Dispose();

        return gameObjects;
    }
}