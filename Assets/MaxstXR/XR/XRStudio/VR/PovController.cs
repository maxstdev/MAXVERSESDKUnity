using System.IO;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class PovController : MonoBehaviour, IPov
{
    public Transform Indicator;
    public float Scale = 0.25f;

    #region Impl. of IPov

    public string Name => name;

    public string Spot => transform.parent.name;

    public Matrix4x4 WorldToLocalMatrix => transform.worldToLocalMatrix;

    public Vector3 WorldPosition => transform.position;

    #endregion

#if UNITY_EDITOR
    public void PlaceIndicator()
    {
        var ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Indicator.position = hit.point;
            Indicator.SetParent(transform, true);
            Indicator.rotation = Quaternion.identity;
        }
    }

    public void OnDrawGizmos()
    {
        var originalMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.white;
        Gizmos.DrawCube(Vector3.zero, Vector3.one * Scale * 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawFrustum(Vector3.zero, 90, 0, Scale, 1);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(Vector3.zero, Vector3.right * Scale);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(Vector3.zero, Vector3.up * Scale);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(Vector3.zero, Vector3.forward * Scale);
        Gizmos.matrix = originalMatrix;
    }

#endif

    async public void StartPlace()
    {
        var self = (PovController)this;
        var cameraGo = GameObject.FindWithTag("360Camera");
        var parentTransform = self.transform.parent.parent;

        // update camera position
        cameraGo.transform.position = self.transform.position;

        await parentTransform.GetComponent<IbrManager>().StartFrom(self);
    }
}