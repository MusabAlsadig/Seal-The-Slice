using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;


public class Cutter : MonoBehaviour
{
    [SerializeField]
    private List<Vector2> points = new List<Vector2>();

    [SerializeField]
    private Transform defaultTarget;

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.color = Color.blue;
        for (int i = 0; i < points.Count; i++)
        {
            int nextIndex = i + 1 < points.Count ? i + 1 : 0;
            Gizmos.DrawLine((Vector3)points[i], (Vector3)points[nextIndex]);
        }

        Gizmos.color = Color.green;
        for (int i = 0; i < points.Count; i++)
        {
            Gizmos.DrawSphere((Vector3)points[i], 0.06f);
        }
    }

    [Button]
    private void CutShapeInFront()
    {
        Transform target;
        if (!Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo))
        {
            Debug.Log("no object there");
            target = defaultTarget;
        }
        else
        {
            Debug.Log(hitInfo.transform.name);
            target = hitInfo.transform;
        }
        
        VertexMesh vertexMesh = new VertexMesh(target.GetComponent<MeshFilter>().sharedMesh);
        vertexMesh.MoveAround(target, transform);
        VertexMesh[] meshes = MeshSlicer.SeperateByCut(vertexMesh, new CutShape(points));


        for (int index = 0; index < meshes.Length; index++)
        {
            VertexMesh vm = meshes[index];
            vm.ReturnNormal(target, transform);
            Mesh mesh = vm.ToMesh();
            GameObject submesh = Instantiate(target.gameObject);
            submesh.transform.position += (2 * submesh.transform.right) + submesh.transform.forward * index;
            mesh.RecalculateBounds();
            submesh.GetComponent<MeshFilter>().sharedMesh = mesh;
        }
    }

}
