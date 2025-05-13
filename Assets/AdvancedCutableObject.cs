using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AdvancedCutableObject : MonoBehaviour
{
    [SerializeField]
    private MeshFilter _meshFilter;
    [SerializeField]
    private List<Vector2> points = new List<Vector2>();

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        for (int i = 0; i < points.Count; i++)
        {
            int nextIndex = i + 1 < points.Count ? i + 1 : 0;
            Gizmos.DrawLine(points[i], points[nextIndex]);
        }
        
        Gizmos.color = Color.green;
        for (int i = 0;i < points.Count; i++)
        {
            Gizmos.DrawSphere(points[i], 0.06f);
        }
    }
    private void OnValidate()
    {
        _meshFilter = GetComponent<MeshFilter>();
    }

    [Button]
    private void Cut()
    {
        Mesh[] meshes = MeshSlicer.SeperateByCut(new VertexMesh(_meshFilter.sharedMesh), new CutShape(points));

        for (int index = 0; index < meshes.Length; index++)
        {
            Mesh mesh = meshes[index];
            GameObject submesh = Instantiate(this.gameObject);
            submesh.gameObject.transform.position += (2 * transform.right);
            submesh.GetComponent<MeshFilter>().sharedMesh = mesh;
        }
    }
}
