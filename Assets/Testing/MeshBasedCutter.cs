using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class MeshBasedCutter : MonoBehaviour
{
    public List<Vector2> points = new List<Vector2>();
    [Button]
    private void Construct()
    {
        VertexMesh mesh = new VertexMesh(GetComponent<MeshFilter>().sharedMesh);
        List<Edge> edges = new List<Edge>();
        foreach (var triangle in mesh.Triangles)
        {
            if (triangle.Normal.z <= 0)
                continue;

            for (int i = 0; i < 3; i++)
            {
                Edge edge = new Edge(triangle[i], triangle[(i + 1) % 3]);

                if (edges.Contains(edge))
                {
                    edges.Remove(edge);
                }
                else
                    edges.Add(edge);
            }
        }

        Edge currentEdge = edges[0];
        VertexData startVertex = currentEdge.a;
        VertexData currentVertex = startVertex;
        points.Clear();
        for (int saftyCounter = 0; saftyCounter <= 1000; saftyCounter++)
        {
            points.Add(currentVertex.position);
            currentEdge = edges.Find(e => e.Contain(currentVertex));
            edges.Remove(currentEdge);
            currentVertex = currentEdge.b;

            if (currentVertex == startVertex)
                break;

            if (saftyCounter == 1000)
            {
                Debug.Log("too much trys");
            }
        }
        GetComponent<CutterBase>().points = points;
    }
}

public class Edge
{
    public VertexData a;
    public VertexData b;

    public Vector3 Normal => a.normal;

    public Vector2 UV => (a.uv + b.uv) / 2;
    public Edge(VertexData a, VertexData b)
    {
        this.a = a;
        this.b = b;
    }

    public bool Contain(VertexData vertex)
    {
        return vertex.position == a.position || vertex.position == b.position;
    }

    public override bool Equals(object obj)
    {
        return obj is Edge edge && edge == this;
    }


    public static bool operator ==(Edge left, Edge right)
    {
        return left.Contain(right.a) && left.Contain(right.b);
    }
    
    public static bool operator !=(Edge left, Edge right)
    {
        return !left.Contain(right.a) || !left.Contain(right.b);
    }
}
