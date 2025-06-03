using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public class MeshDebugger : MonoBehaviour
{
    [SerializeField]
    private MeshFilter _meshFilter;
    StringBuilder message = new StringBuilder();

    private void OnValidate()
    {
        _meshFilter = GetComponent<MeshFilter>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.matrix = transform.localToWorldMatrix;
        for (int i = 0; i < _meshFilter.sharedMesh.normals.Length; i++)
        {
            // show normals
            Vector3 normal = _meshFilter.sharedMesh.normals[i];
            Vector3 vertex = _meshFilter.sharedMesh.vertices[i];
            Gizmos.DrawLine(vertex, vertex + normal);
        }
    }

    [Button]
    private void ShowUV()
    {
        foreach (var item in _meshFilter.mesh.uv)
        {
            message.AppendLine(item.ToString());
        }
        Debug.Log(message.ToString());
        message.Clear();
    }


    [Button]
    private void ShowVertices()
    {
        message.AppendLine("vertices = " + _meshFilter.mesh.vertices.Length);
        List<Vector3> old = new List<Vector3>();
        foreach (var item in _meshFilter.mesh.vertices)
        {
            if (old.Contains(item))
                Debug.Log(item.ToString() + " is repeated");
            else
            {
                old.Add(item);
                message.AppendLine(item.ToString());
            }

        }
        Debug.Log(message.ToString());
        message.Clear();
    }
}