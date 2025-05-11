using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public class SlicableObject : MonoBehaviour
{
    [SerializeField]
    private MeshFilter _meshFilter;
    [SerializeField]
    private float _distance;
    [SerializeField]
    private Vector3 _normal;
    StringBuilder message = new StringBuilder();

    private void OnValidate()
    {
        _meshFilter = GetComponent<MeshFilter>();
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 _origin = Vector3.back * _distance;
        //We construct new gizmos matrix taking our _normal as forward position
        Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.LookRotation(_normal), Vector3.one);
        //We draw cubes that will now represent our slicing plane
        Gizmos.color = new Color(0, 1, 0, 0.4f);
        Gizmos.DrawCube(_origin, new Vector3(2, 2, 0.01f));
        Gizmos.color = new Color(0, 1, 0, 1f);
        Gizmos.DrawWireCube(_origin, new Vector3(2, 2, 0.01f));
        //We set matrix to our object matrix and draw all of the normals.
        //It will be especially usefull after we start
        //slicing mesh and have to check
        //if all faces where created correctly 
        Gizmos.color = Color.blue;
        Gizmos.matrix = transform.localToWorldMatrix;
        for (int i = 0; i < _meshFilter.sharedMesh.normals.Length; i++)
        {
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

    
    [Button]
    private void SliceMesh()
    {
        Undo.RecordObject(this, "Slice");
        Mesh[] meshes = MeshSlicer.SliceMesh(_meshFilter.sharedMesh, _normal, _distance);
        for (int index = 0; index < meshes.Length; index++)
        {
            Mesh mesh = meshes[index];
            GameObject submesh = Instantiate(this.gameObject);
            submesh.gameObject.transform.position += (2 * transform.right);
            submesh.GetComponent<MeshFilter>().sharedMesh = mesh;
        }
        EditorUtility.SetDirty(this);
    }
}