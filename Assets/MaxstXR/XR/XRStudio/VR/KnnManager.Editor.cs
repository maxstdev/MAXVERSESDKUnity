#if UNITY_EDITOR
using KNN;
using KNN.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public partial class KnnManager : MonoBehaviour
{
    public float SubsampleRadius = 0.5f;
    public bool SubsampleRandomly;

    enum SampleState
    {
        Undetermined, Inlier, Outlier
    }

    public void Subsample()
    {
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
        var result = new NativeList<int>(Allocator.TempJob);

        // Perform Poisson sampling
        var indicesEnumerable = Enumerable.Range(0, l);
        if (SubsampleRandomly) indicesEnumerable = indicesEnumerable.OrderBy(_ => Random.Range(0, l));
        var indices = indicesEnumerable.ToArray();
        var states = new SampleState[l];
        foreach (var i in indices)
        {
            if (states[i] != SampleState.Undetermined) continue; // skip determined points
            new QueryRangeJob(container, points[i], SubsampleRadius, result).Schedule().Complete();
            foreach (var j in result)
            {
                if (states[j] != SampleState.Undetermined) continue; // skip determined points
                states[j] = i == j ? SampleState.Inlier : SampleState.Outlier;
            }
        }

        // Delete outliers
        knnPoints.Where((knnPoint, i) => states[i] == SampleState.Outlier)
            .Select(knnPoint => knnPoint.gameObject)
            .ToList().ForEach(DestroyImmediate);

        // Cleanup
        result.Dispose();
        container.Dispose();
        points.Dispose();
    }
}
#endif