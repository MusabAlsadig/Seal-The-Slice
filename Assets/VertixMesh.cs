using System.Collections.Generic;
using UnityEngine;

public class VertexMesh
{
    private List<VertexData> vertices = new List<VertexData>();
    private List<Triangle> triangles = new List<Triangle>();

    public List<VertexData> Vertices => vertices;
    public List<Triangle> Triangles => triangles;
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

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            VertexData vertexA = vertices[mesh.triangles[i]];
            VertexData vertexB = vertices[mesh.triangles[i + 1]];
            VertexData vertexC = vertices[mesh.triangles[i + 2]];
            Triangle triangle = new Triangle(vertexA, vertexB, vertexC);

            triangles.Add(triangle);
        }
    }

    public void AddTriangle(VertexData a, VertexData b, VertexData c)
    {
        AddTriangle(new Triangle(a, b, c));
    }

    public void AddTriangle(Triangle triangle)
    {

        if (TryFind(triangle.vertexA, out VertexData vertexA))
            triangle.vertexA = vertexA;
        else
            vertices.Add(triangle.vertexA);

        if (TryFind(triangle.vertexB, out VertexData vertexB))
            triangle.vertexB = vertexB;
        else
            vertices.Add(triangle.vertexB);

        if (TryFind(triangle.vertexC, out VertexData vertexC))
            triangle.vertexC = vertexC;
        else
            vertices.Add(triangle.vertexC);

        triangles.Add(triangle);
    }

    public void RemoveTriangle(Triangle triangle)
    {
        triangles.Remove(triangle);
    }
    
    /// <summary>
    /// move and rotate the mesh<br/>
    /// remember that the cutting is based on world origin (0,0,0) on XY plane
    /// </summary>
    /// <param name="target"></param>
    /// <param name="cutter"></param>
    public void MoveAround(Transform target, Transform cutter)
    {
        Matrix4x4 targetMatrix = Matrix4x4.Rotate(target.rotation);
        Matrix4x4 cutterMatrix = Matrix4x4.Rotate(cutter.rotation).inverse;


        foreach (var vertex in vertices)
        {
            vertex.position = targetMatrix.MultiplyPoint(vertex.position);
            vertex.normal = targetMatrix.MultiplyPoint(vertex.normal);

            vertex.position += target.position;
            vertex.position -= cutter.position;

            vertex.position = cutterMatrix.MultiplyPoint(vertex.position);
            vertex.normal = cutterMatrix.MultiplyPoint(vertex.normal);
        }
    }

    /// <summary>
    /// return the mesh back to it's origin<br/>
    /// remember that the cutting is based on world origin (0,0,0) on XY plane
    /// </summary>
    /// <param name="target"></param>
    /// <param name="cutter"></param>
    public void ReturnNormal(Transform target, Transform cutter)
    {
        // same as "MoveAround" method, just on the other direction

        Matrix4x4 targetMatrix = Matrix4x4.Rotate(target.rotation).inverse;
        Matrix4x4 cutterMatrix = Matrix4x4.Rotate(cutter.rotation);


        foreach (var vertex in vertices)
        {
            if (vertex.isOnCurrectPosition)
                continue;


            vertex.position = cutterMatrix.MultiplyPoint(vertex.position);
            vertex.normal = cutterMatrix.MultiplyPoint(vertex.normal);

            vertex.position -= target.position;
            vertex.position += cutter.position;

            vertex.position = targetMatrix.MultiplyPoint(vertex.position);
            vertex.normal = targetMatrix.MultiplyPoint(vertex.normal).normalized;
            
            vertex.isOnCurrectPosition = true;
        }
    }

    public VertexData CreateIntersectionVertex(VertexData vertexA, VertexData vertexB, Plane plane)
    {
        // TODO : make a check when inserting a new vertix
        // to prevent both tringles of the same face of doing creating new vertix
        
        Ray ray = new Ray(vertexA.position, vertexB.position - vertexA.position);
        plane.Raycast(ray, out float distance);
        Vector3 position = ray.GetPoint(distance);

        float distanceA = Vector3.Distance(vertexA.position, position);
        float distanceB = Vector3.Distance(vertexB.position, position);
        float t = distanceA / (distanceA + distanceB);

        Vector2 uv = Vector2.Lerp(vertexA.uv, vertexB.uv, t);
        Vector3 normal = Vector3.Lerp(vertexA.normal, vertexB.normal, t);
        VertexData vertexData = new VertexData(vertices.Count, position, normal, uv);

        if (TryFind(vertexData, out VertexData oldVertex))
            return oldVertex;
        else
        {
            vertices.Add(vertexData);
            return vertexData;
        }
    }

    public Mesh ToMesh()
    {
        Mesh mesh = new Mesh();
        int length = vertices.Count;
        var positions = new Vector3[length];
        var normals = new Vector3[length];
        var uv = new Vector2[length];
        var trianglesList = new List<int>();

        for (int i = 0; i < length; i++)
        {
            positions[i] = vertices[i].position;
            normals[i] = vertices[i].normal;
            uv[i] = vertices[i].uv;

            // refreash indexes
            vertices[i].index = i;
        }
        for (int i = 0; i < triangles.Count; i++)
        {
            trianglesList.Add(triangles[i].vertexA.index);
            trianglesList.Add(triangles[i].vertexB.index);
            trianglesList.Add(triangles[i].vertexC.index);
        }

        mesh.vertices = positions;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.triangles = trianglesList.ToArray();

        return mesh;
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
                positive.AddTriangle(firstVertex.CloneWithNormal(-cutNormal), secondVertex.CloneWithNormal(-cutNormal), halfway.CloneWithNormal(-cutNormal));
                negative.AddTriangle(secondVertex.CloneWithNormal(cutNormal), firstVertex.CloneWithNormal(cutNormal), halfway.CloneWithNormal(cutNormal));
            }
            else
            {
                //used if calculated normal is opposite to plane normal
                negative.AddTriangle(firstVertex.CloneWithNormal(cutNormal), secondVertex.CloneWithNormal(cutNormal), halfway.CloneWithNormal(cutNormal));
                positive.AddTriangle(secondVertex.CloneWithNormal(-cutNormal), firstVertex.CloneWithNormal(-cutNormal), halfway.CloneWithNormal(-cutNormal));
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

    private bool TryFind(VertexData vertex, out VertexData result)
    {
        result = vertices.Find(v => v.position == vertex.position && v.normal == vertex.normal);

        return result != null;
    }
}

public class VertexData
{
    public int index;
    public Vector3 position;
    public Vector3 normal;
    public Vector2 uv;

    public bool isOnCurrectPosition;

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

public class Triangle
{
    public VertexData vertexA;
    public VertexData vertexB;
    public VertexData vertexC;

    public Triangle(VertexData vertexA, VertexData vertexB, VertexData vertexC)
    {
        this.vertexA = vertexA;
        this.vertexB = vertexB;
        this.vertexC = vertexC;
    }

    public Vector3 GetPointInTheMiddle()
    {
        return new Vector3
        {
            x = (vertexA.position.x + vertexB.position.x + vertexC.position.x) / 3,
            y = (vertexA.position.y + vertexB.position.y + vertexC.position.y) / 3,
            z = (vertexA.position.z + vertexB.position.z + vertexC.position.z) / 3,
        };
    }
}
