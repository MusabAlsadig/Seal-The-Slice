using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CuttableObject : MonoBehaviour
{
    [SerializeField]
    private MeshFilter meshFilter;

    public Mesh SharedMesh => meshFilter.sharedMesh;

    private void OnValidate()
    {
        meshFilter = GetComponent<MeshFilter>();
    }



    public void AfterCutCleanup()
    {
        string undoLable = "Cut " + name;

        Undo.RecordObject(gameObject, undoLable);
        name += " (sliced)";

        if (Application.isPlaying)
        {
            Destroy(meshFilter);
            Destroy(this);
        }
        else
        {
            Undo.DestroyObjectImmediate(meshFilter);
            Undo.DestroyObjectImmediate(this);
        }
    }



}
