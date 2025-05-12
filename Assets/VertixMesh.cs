using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class VertexMesh
{
    public List<VertexData> vertices = new List<VertexData>();
    public List<int> triangles = new List<int>();

    public VertexMesh() 
    {
        // nothing
    }
    public VertexMesh(Mesh mesh)
    {
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            VertexData vertexData = new VertexData(i, mesh.vertices[i], mesh.normals[i], mesh.uv[i]);
            vertices.Add(vertexData);
        }

        triangles = mesh.triangles.ToList();
    }

    public void AddTringle(VertexData a, VertexData b, VertexData c)
    {
        if (!vertices.Contains(a)) vertices.Add(a);
        if (!vertices.Contains(b)) vertices.Add(b);
        if (!vertices.Contains(c)) vertices.Add(c);

        int indexA = vertices.IndexOf(a);
        int indexB = vertices.IndexOf(b);
        int indexC = vertices.IndexOf(c);
        triangles.Add(indexA);
        triangles.Add(indexB);
        triangles.Add(indexC);
    }


    public VertexData CreateIntersectionVertex(VertexData vertexA, VertexData vertexB, Plane plane)
    {
        // TODO : make a check when inserting a new vertix
        // to prevent both tringles of the same face of doing creating new vertix

        Ray ray = new Ray(vertexA.position, vertexB.position - vertexA.position);
        plane.Raycast(ray, out float distance);
        Vector3 position = ray.GetPoint(distance);
        Debug.Log(position);

        float distanceA = Vector3.Distance(vertexA.position, position);
        float distanceB = Vector3.Distance(vertexB.position, position);
        float t = distanceA / (distanceA + distanceB);

        Vector2 uv = Vector2.Lerp(vertexA.position, vertexB.position, t);
        Vector3 normal = Vector3.Lerp(vertexA.normal, vertexB.normal, t);
        VertexData vertexData = new VertexData(vertices.Count, position, normal, uv);
        vertices.Add(vertexData);
        return vertexData;
    }

    public Mesh ToMesh()
    {
        Mesh mesh = new Mesh();
        int length = vertices.Count;
        var positions = new Vector3[length];
        var normals = new Vector3[length];
        var uv = new Vector2[length];

        for (int i = 0; i < length; i++)
        {
            positions[i] = vertices[i].position;
            normals[i] = vertices[i].normal;
            uv[i] = vertices[i].uv;
        }

        mesh.vertices = positions;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.triangles = triangles.ToArray();

        return mesh;
    }

    private bool PointIntersectsAPlane(Vector3 from, Vector3 to, Vector3 planeOrigin, Vector3 normal, out Vector3 result)
    {
        Vector3 translation = to - from;
        float dot = Vector3.Dot(normal, translation);
        //Check if lines are not perpendicular
        if (Mathf.Abs(dot) > Single.Epsilon)
        {
            Vector3 fromOrigin = from - planeOrigin;
            float fac = -Vector3.Dot(normal, fromOrigin) / dot;
            translation *= fac;
            result = from + translation;
            return true;
        }


        result = Vector3.zero;
        return false;
    }

    public void JoinPointsAlongPlane(ref VertexMesh positive, ref VertexMesh negative, Vector3 cutNormal, List<VertexData> pointsAlongPlane)
    {

        VertexData halfway = new VertexData(vertices.Count, GetHalfwayPoint(pointsAlongPlane),cutNormal, pointsAlongPlane[0].uv);

        for (int i = 0; i < pointsAlongPlane.Count; i += 2)
        {
            VertexData firstVertex = pointsAlongPlane[i];
            VertexData secondVertex = pointsAlongPlane[i + 1];

            Vector3 normal = ComputeNormal(halfway, secondVertex, firstVertex);

            float dot = Vector3.Dot(normal, cutNormal);

            if (dot > 0)
            {
                //used if calculated normal aligns with plane normal                           
                positive.AddTringle(firstVertex.CloneWithNormal(-cutNormal), secondVertex.CloneWithNormal(-cutNormal), halfway.CloneWithNormal(-cutNormal));
                negative.AddTringle(secondVertex.CloneWithNormal(cutNormal), firstVertex.CloneWithNormal(cutNormal), halfway.CloneWithNormal(cutNormal));
            }
            else
            {
                //used if calculated normal is opposite to plane normal
                negative.AddTringle(firstVertex.CloneWithNormal(cutNormal), secondVertex.CloneWithNormal(cutNormal), halfway.CloneWithNormal(cutNormal));
                positive.AddTringle(secondVertex.CloneWithNormal(-cutNormal), firstVertex.CloneWithNormal(-cutNormal), halfway.CloneWithNormal(-cutNormal));
            }
        }
    }
    private Vector3 GetHalfwayPoint(List<VertexData> pointsAlongPlane)
    {
        if (pointsAlongPlane.Count > 0)
        {
            Vector3 firstPoint = pointsAlongPlane[0].position;
            Vector3 furthestPoint = Vector3.zero;
            float distance = 0f;

            for (int index = 0; index < pointsAlongPlane.Count; index++)
            {
                Vector3 point = pointsAlongPlane[index].position;
                float currentDistance = 0f;
                currentDistance = Vector3.Distance(firstPoint, point);

                if (currentDistance > distance)
                {
                    distance = currentDistance;
                    furthestPoint = point;
                }
            }

            return Vector3.Lerp(firstPoint, furthestPoint, 0.5f);
        }
        else
        {
            return Vector3.zero;
        }
    }
    private Vector3 ComputeNormal(VertexData a, VertexData b, VertexData c)
    {
        Vector3 sideL = b.position - a.position;
        Vector3 sideR = c.position - a.position;

        Vector3 normal = Vector3.Cross(sideL, sideR);

        return normal.normalized;
    }
}

public class VertexData
{
    public int index;
    public Vector3 position;
    public Vector3 normal;
    public Vector2 uv;

    public VertexData(int index, Vector3 position, Vector3 normal, Vector2 uv)
    {
        this.index = index;
        this.position = position;
        this.normal = normal;
        this.uv = uv;
    }

    public VertexData CloneWithNormal(Vector3 newNormal)
    {
        return new VertexData(index, position, newNormal, uv);

    }
}
