using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class Cutter : MonoBehaviour
{
    public List<Vector2> points = new List<Vector2>();
    [SerializeField]
    private Transform defaultTarget;


    public void CutShapeInFront()
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
