using System.Collections.Generic;
using UnityEngine;

internal class MeshSlicer
{

    public static Mesh[] SliceMesh(Mesh normalMesh, Vector3 cutNormal, float distance)
    {
        Plane plane = new Plane(cutNormal, distance);
        cutNormal.Normalize();
        VertexMesh positiveMesh = new VertexMesh();
        VertexMesh negativeMesh = new VertexMesh();

        VertexMesh mesh = new VertexMesh(normalMesh);
        List<VertexData> pointsAlongPlane = new List<VertexData>();
        for (int i = 0; i < normalMesh.triangles.Length; i += 3)
        {
            VertexData vertexA = mesh.vertices[mesh.triangles[i]];
            VertexData vertexB = mesh.vertices[mesh.triangles[i + 1]];
            VertexData vertexC = mesh.vertices[mesh.triangles[i + 2]];

            bool isABSameSide = plane.SameSide( vertexA.position, vertexB.position);
            bool isBCSameSide = plane.SameSide( vertexB.position, vertexC.position);


            if (isABSameSide && isBCSameSide)
            {
                // all this tringle is in the same side
                VertexMesh helper = plane.GetSide(vertexA.position) ? positiveMesh : negativeMesh;
                helper.AddTringle(vertexA, vertexB, vertexC);
            }
            else
            {
                //we have to find intersection between triangle and cutting plane 
                VertexData intersectionD;
                VertexData intersectionE;
                //Determine appropirate helper for each triangle corner
                VertexMesh helperA = plane.GetSide(vertexA.position) ? positiveMesh : negativeMesh;
                VertexMesh helperB = plane.GetSide(vertexB.position) ? positiveMesh : negativeMesh;
                VertexMesh helperC = plane.GetSide(vertexC.position) ? positiveMesh : negativeMesh;

                if (isABSameSide)
                {
                    intersectionD = mesh.CreateIntersectionVertex(vertexA, vertexC, plane);
                    intersectionE = mesh.CreateIntersectionVertex(vertexB, vertexC, plane);

                    helperA.AddTringle(vertexA, vertexB, intersectionE);
                    helperA.AddTringle(vertexA, intersectionE, intersectionD);
                    helperC.AddTringle(intersectionE, vertexC, intersectionD);
                }
                else if (isBCSameSide)
                {
                    intersectionD = mesh.CreateIntersectionVertex(vertexB, vertexA, plane);
                    intersectionE = mesh.CreateIntersectionVertex(vertexC, vertexA, plane);

                    helperB.AddTringle(vertexB, vertexC, intersectionE);
                    helperB.AddTringle(vertexB, intersectionE, intersectionD);
                    helperA.AddTringle(intersectionE, vertexA, intersectionD);
                }
                else
                {
                    intersectionD = mesh.CreateIntersectionVertex(vertexA, vertexB, plane);
                    intersectionE = mesh.CreateIntersectionVertex(vertexC, vertexB, plane);

                    helperA.AddTringle(vertexA, intersectionE, vertexC);
                    helperA.AddTringle(intersectionD, intersectionE, vertexA);
                    helperB.AddTringle(vertexB, intersectionE, intersectionD);
                }


                pointsAlongPlane.Add(intersectionD);
                pointsAlongPlane.Add(intersectionE);
            }

        }

        // fill the faces inside of the shape
        mesh.JoinPointsAlongPlane(ref positiveMesh, ref negativeMesh, cutNormal, pointsAlongPlane);

        return new[] { positiveMesh.ToMesh(), negativeMesh.ToMesh() };
    }

    

}
