using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AdvancedCutableObject : MonoBehaviour
{

    [SerializeField]
    private MeshFilter meshFilter;

    private void OnValidate()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    public List<GameObject> CutWith(Cutter cutter)
    {

        VertexMesh vertexMesh = new VertexMesh(meshFilter.sharedMesh);
        vertexMesh.MoveAround(transform, cutter.transform);
        VertexMesh[] meshes = MeshSlicer.SeperateByCut(vertexMesh, cutter.GetShape());

        string undoLable = "Cut " + name;

        List<GameObject> submeshObjects = new List<GameObject>();
        for (int i = 0; i < meshes.Length; i++)
        {
            VertexMesh vm = meshes[i];
            vm.ReturnNormal(transform, cutter.transform);
            Mesh mesh = vm.ToMesh();
            mesh.RecalculateBounds();

            mesh.name = name + " cut " + i;
            GameObject submeshObject = Instantiate(gameObject);
            submeshObject.name = name + " inner " + i;
            Undo.RegisterCreatedObjectUndo(submeshObject, undoLable);
            submeshObject.GetComponent<MeshFilter>().sharedMesh = mesh;

            submeshObjects.Add(submeshObject);
        }

        foreach (var _submeshObject in submeshObjects)
        {
            _submeshObject.transform.SetParent(transform);
            _submeshObject.transform.localPosition = Vector3.zero;
        }

        Undo.RecordObject(gameObject, undoLable);
        name += " (sliced)";

        if (Application.isPlaying)
        {
            Destroy(meshFilter);
            Destroy(this);
        }
        else
        {
            DestroyImmediate(meshFilter);
            DestroyImmediate(this);
        }

        return submeshObjects;
    }



}
