using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    public class ShapeInserter
    {
        public static void Insert(VertexMesh baseMesh, VertexMesh cutMesh)
        {


            for (int i = 0; i < baseMesh.Triangles.Count; i += 3)
            {
                //VertexData vertexA = baseMesh.vertices[baseMesh.triangles[i]];
                //VertexData vertexB = baseMesh.vertices[baseMesh.triangles[i + 1]];
                //VertexData vertexC = baseMesh.vertices[baseMesh.triangles[i + 2]];
                //
                //Line lineAB = new Line(vertexA, vertexB);
                //Line lineBC = new Line(vertexB, vertexC);
                //Line lineCA = new Line(vertexC, vertexA);
                
            }
        }
    }
}
