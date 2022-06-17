#if UNITY_EDITOR

using System.Threading.Tasks;
using UnityEngine;

[ExecuteInEditMode]
public partial class IbrManager : MonoBehaviour
{

    public void Update()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            IbrCullBack.SetVector("_runtimeData", new Vector4(1, 0, 0, 0));
            IbrCullBack.SetColor("_Color", meshColor);
            UpdateMaterialEditor(IbrCullBack);
            UpdateMaterialEditor(IbrCullFront);
        }
    }
}

#endif