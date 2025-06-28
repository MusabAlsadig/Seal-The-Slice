using SealTheSlice.Extentions;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SealTheSlice
{
    internal static class MeshSlicer
    {
        public static CutResult SeperateByCut(CuttableObject cuttable, CutterBase cutter)
        {
            MatrixTranslator matrixTranslator = new MatrixTranslator(cuttable.transform, cutter.transform);
            VertexMesh mesh = new VertexMesh(cuttable.SharedMesh);
            matrixTranslator.MoveAround(mesh);
            CutShape cut = cutter.GetShape();

            foreach (var plane in cut.planes)
            {
                SliceMesh(ref mesh, plane.Plane);
            }

            VertexMesh insideMesh = new VertexMesh();
            VertexMesh outsideMesh = new VertexMesh();
            foreach (var triangle in mesh.Triangles)
            {
                if (cut.IsInside(triangle))
                    insideMesh.AddTriangle(triangle);
                else
                    outsideMesh.AddTriangle(triangle);
            }

            VertexMesh insideMeshFill = new VertexMesh();
            VertexMesh outsideMeshFill = new VertexMesh();
            FillTheInside(insideMesh, true, cut, insideMeshFill);
            FillTheInside(outsideMesh, false, cut, outsideMeshFill);

            Bounds translatedBounds = matrixTranslator.MoveAround(cuttable.OriginalBounds);
            FixMeshUVs(insideMeshFill, translatedBounds);
            FixMeshUVs(outsideMeshFill, translatedBounds);

            GameObject insideObject;
            if (cutter.KeepInside)
                insideObject = CloneObject(cuttable, cutter, insideMesh, "inside", matrixTranslator);
            else
                insideObject = null;

            GameObject insideObjectFill;
            if (cutter.KeepInside)
                insideObjectFill = CloneObject(cuttable, cutter, insideMeshFill, "inside fill", matrixTranslator);
            else
                insideObjectFill = null;

            GameObject outsideObject = CloneObject(cuttable, cutter, outsideMesh, "outside", matrixTranslator);
            GameObject outsideObjectFill = CloneObject(cuttable, cutter, outsideMeshFill, "outside fill", matrixTranslator);


            if (cutter.Material != null)
            {
                Material m = new Material(cutter.Material);
                if (cutter.KeepInside)
                    insideObjectFill.AddComponent<CutFillEffect>().Setup(cutter.FadeTime, m);

                outsideObjectFill.AddComponent<CutFillEffect>().Setup(cutter.FadeTime, m);
            }

            var result = new CutResult(outsideObject, insideObject, outsideObjectFill, insideObjectFill);

            // reparent to base object
            foreach (var subobject in result)
            {
                subobject.transform.SetParent(cuttable.transform, false);
            }

            if (cutter.KeepInside)
                insideObjectFill.transform.SetParent(insideObject.transform);

            outsideObjectFill.transform.SetParent(outsideObject.transform);

            return result;
        }

        private static GameObject CloneObject(CuttableObject baseObject, CutterBase cutter, VertexMesh submesh, string name, MatrixTranslator matrixTranslator, bool isSeperateObject = false)
        {
            string undoLable = "Cut " + baseObject.name;

            matrixTranslator.ReturnNormal(submesh);
            Mesh mesh = submesh.ToMesh();
            mesh.RecalculateBounds();

            mesh.name = baseObject.name + " cut ";
            GameObject submeshObject = new GameObject($"{baseObject.name} {name}");
            Undo.RegisterCreatedObjectUndo(submeshObject, undoLable);

            var meshCollider = submeshObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
            meshCollider.convex = true;
            submeshObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            submeshObject.AddComponent<MeshRenderer>().materials = baseObject.GetComponent<MeshRenderer>().materials;

            if (isSeperateObject)
                submeshObject.AddComponent<CuttableRootObject>();
            else
                submeshObject.AddComponent<CuttableSubObject>().Setup(baseObject.Root);

            return submeshObject;
        }

        public static void SliceMesh(ref VertexMesh mesh, Plane plane)
        {
            List<Triangle> trianglesToRemove = new List<Triangle>();
            List<Triangle> trianglesToAdd = new List<Triangle>();
            foreach (var triangle in mesh.Triangles)
            {
                VertexData vertexA = triangle.vertexA;
                VertexData vertexB = triangle.vertexB;
                VertexData vertexC = triangle.vertexC;

                bool isABSameSide = plane.SameSide(vertexA.position, vertexB.position);
                bool isBCSameSide = plane.SameSide(vertexB.position, vertexC.position);

                // this tringle is outside this plane, just leave it
                if (isABSameSide && isBCSameSide)
                    continue;

                // this tringle cross this plane, need to edit it
                trianglesToRemove.Add(triangle);
                VertexData intersectionD;
                VertexData intersectionE;

                if (isABSameSide)
                {
                    intersectionD = mesh.CreateIntersectionVertex(vertexA, vertexC, plane);
                    intersectionE = mesh.CreateIntersectionVertex(vertexB, vertexC, plane);

                    trianglesToAdd.Add(new Triangle(vertexA, vertexB, intersectionE));
                    trianglesToAdd.Add(new Triangle(vertexA, intersectionE, intersectionD));
                    trianglesToAdd.Add(new Triangle(intersectionE, vertexC, intersectionD));
                }
                else if (isBCSameSide)
                {
                    intersectionD = mesh.CreateIntersectionVertex(vertexB, vertexA, plane);
                    intersectionE = mesh.CreateIntersectionVertex(vertexC, vertexA, plane);

                    trianglesToAdd.Add(new Triangle(vertexB, vertexC, intersectionE));
                    trianglesToAdd.Add(new Triangle(vertexB, intersectionE, intersectionD));
                    trianglesToAdd.Add(new Triangle(intersectionE, vertexA, intersectionD));
                }
                else
                {
                    intersectionD = mesh.CreateIntersectionVertex(vertexA, vertexB, plane);
                    intersectionE = mesh.CreateIntersectionVertex(vertexC, vertexB, plane);

                    trianglesToAdd.Add(new Triangle(vertexA, intersectionE, vertexC));
                    trianglesToAdd.Add(new Triangle(intersectionD, intersectionE, vertexA));
                    trianglesToAdd.Add(new Triangle(vertexB, intersectionE, intersectionD));
                }
            }

            foreach (var triangle in trianglesToRemove)
            {
                mesh.RemoveTriangle(triangle);
            }
            foreach (var triangle in trianglesToAdd)
            {
                mesh.AddTriangle(triangle);
            }
        }

        private static void Organize(ref List<VertexData> vertices, Vector3 normal)
        {
            Vector3 centerPoint = Vector3.zero;

            foreach (var vertex in vertices)
            {
                centerPoint += vertex.position;
            }
            centerPoint /= vertices.Count;
            vertices = vertices.OrderBy(v => Vector3.SignedAngle(Vector3.forward, v.position - centerPoint, normal)).ToList();
        }

        private static void Organize(ref List<VertexData> vertices)
        {

            Vector2 centerPoint = Vector2.zero;

            foreach (var vertex in vertices)
            {
                centerPoint += (Vector2)vertex.position;
            }
            centerPoint /= vertices.Count;
            vertices = vertices.OrderByDescending(v => Vector2.SignedAngle(Vector2.right, (Vector2)v.position - centerPoint)).ToList();
        }

        private static void Organize(ref List<Point> points)
        {

            Vector2 centerPoint = Vector2.zero;

            foreach (var point in points)
            {
                centerPoint += (Vector2)point.Position;
            }
            centerPoint /= points.Count;
            points = points.OrderByDescending(p => Vector2.SignedAngle(Vector2.right, (Vector2)p.Position - centerPoint)).ToList();
        }




        private static void FillTheInside(VertexMesh mesh, bool isPositive, CutShape cut, VertexMesh otherMeshToPutInto = null)
        {
            Dictionary<LimitedPlane, List<VertexData>> newVertices = new Dictionary<LimitedPlane, List<VertexData>>();
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                // check all vertices and assigne them with the plane which cut them
                if (cut.TryGetPlanesThatCross(mesh.Vertices[i], out List<LimitedPlane> planes))
                {

                    foreach (var plane in planes)
                    {
                        // to ensure sure no null errors happen
                        if (!newVertices.ContainsKey(plane))
                            newVertices.Add(plane, new List<VertexData>());

                        newVertices[plane].Add(mesh.Vertices[i].Copy());
                    }
                }
            }

            if (otherMeshToPutInto != null)
                mesh = otherMeshToPutInto;

            foreach (var pair in newVertices)
            {
                LimitedPlane plane = pair.Key;
                List<VertexData> vertices = pair.Value;

                Organize(ref vertices, plane.Plane.normal);

                GenerateTrianglesForPlane(ref mesh, isPositive, plane.Plane.normal, vertices);
            }
        }

        #region Inside Filling
        private static void GenerateTrianglesForPlane(ref VertexMesh mesh, bool isPositive, Vector3 cutNormal, List<VertexData> pointsAlongPlane)
        {
            Vector3 centerPoint = Vector3.zero;
            foreach (var vertex in pointsAlongPlane)
            {
                centerPoint += vertex.position;
            }
            centerPoint /= pointsAlongPlane.Count;


            VertexData halfway = new VertexData(-1, centerPoint, cutNormal, -Vector2.one);

            for (int i = 0; i < pointsAlongPlane.Count; i++)
            {
                int nextindex = i + 1 < pointsAlongPlane.Count ? i + 1 : 0;
                VertexData firstVertex = pointsAlongPlane[i];
                VertexData secondVertex = pointsAlongPlane[nextindex];

                Vector3 normal = ComputeNormal(halfway, secondVertex, firstVertex);

                float dot = Vector3.Dot(normal, cutNormal);

                if (dot > 0)
                {
                    //used if calculated normal aligns with plane normal
                    if (isPositive)
                        mesh.AddTriangle(firstVertex.CloneWithNormal(-cutNormal), secondVertex.CloneWithNormal(-cutNormal), halfway.CloneWithNormal(-cutNormal));
                    else
                        mesh.AddTriangle(secondVertex.CloneWithNormal(cutNormal), firstVertex.CloneWithNormal(cutNormal), halfway.CloneWithNormal(cutNormal));
                }
                else
                {
                    //used if calculated normal is opposite to plane normal
                    if (isPositive)
                        mesh.AddTriangle(secondVertex.CloneWithNormal(-cutNormal), firstVertex.CloneWithNormal(-cutNormal), halfway.CloneWithNormal(-cutNormal));
                    else
                        mesh.AddTriangle(firstVertex.CloneWithNormal(cutNormal), secondVertex.CloneWithNormal(cutNormal), halfway.CloneWithNormal(cutNormal));
                }
            }
        }

        private static void FixMeshUVs(VertexMesh mesh, Bounds? customBounds = null, bool makeSquareUV = true)
        {
            Bounds bounds;
            if (customBounds.HasValue)
                bounds = customBounds.Value;
            else
            {
                Vector3 min = Vector3.one * float.MaxValue;
                Vector3 max = Vector3.one * float.MinValue;
                foreach (var v in mesh.Vertices)
                {
                    min = Vector3.Min(v.position, min);
                    max = Vector3.Max(v.position, max);
                }

                bounds = new Bounds(min, max);
            }

            Vector3 totalDistance = bounds.size;

            if (makeSquareUV)
            {
                totalDistance = Vector3.one * totalDistance.HighestValue();
            }

            Vector3 minPoint = bounds.min;
            foreach (var vertex in mesh.Vertices)
            {
                Vector3 distance = vertex.position - minPoint;

                // first axis is always (z) in this cut calculations
                Axis secondUVAxis;
                if (Mathf.Abs(vertex.normal.x) < Mathf.Abs(vertex.normal.y))
                    secondUVAxis = Axis.x;
                else
                    secondUVAxis = Axis.y;

                float zRatio = distance.z / totalDistance.z;
                float secondRatio = distance.Value(secondUVAxis) / totalDistance.Value(secondUVAxis);
                vertex.uv.x = Mathf.Lerp(0, 1, zRatio);
                vertex.uv.y = Mathf.Lerp(0, 1, secondRatio);

                Debug.Log(vertex.uv);
            }
        }

        private static Vector3 GetHalfwayPoint(List<VertexData> pointsAlongPlane)
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

        private static Vector3 ComputeNormal(VertexData a, VertexData b, VertexData c)
        {
            Vector3 sideL = b.position - a.position;
            Vector3 sideR = c.position - a.position;

            Vector3 normal = Vector3.Cross(sideL, sideR);

            return normal.normalized;
        }
        #endregion


        public static CutResult SeperateByCut_InsertWithEarClipper(CuttableObject cuttable, CutterBase cutter)
        {
            MatrixTranslator matrixTranslator = new MatrixTranslator(cuttable.transform, cutter.transform);
            VertexMesh mesh = new VertexMesh(cuttable.SharedMesh);
            matrixTranslator.MoveAround(mesh);
            Polygon cut = cutter.ToPolygon();
            cut.Refresh();
            Polygon hole = cut.Copy();

            List<Triangle> trianglesSurroundingHole = new List<Triangle>();
            foreach (var triangle in mesh.Triangles)
            {
                // ignore backward ones for now
                if (triangle.Normal.z <= 0)
                    continue;

                if (hole.IsAroundTheShape(triangle))
                    trianglesSurroundingHole.Add(triangle);
                else if (hole.IsInsideTheShape(triangle))
                    trianglesSurroundingHole.Add(triangle);
            }

            trianglesSurroundingHole.ForEach(t => mesh.RemoveTriangle(t));

            // raycast the hole forward into valid triangles
            foreach (var point in hole.points)
            {
                foreach (var triangle in trianglesSurroundingHole)
                {
                    if (EarClipper.IsPointInTriangle(point, triangle))
                    {
                        triangle.TryRaycastVertexIntoThisTriangle(point.vertex, Vector3.forward);
                        triangle.TryRaycastVertexIntoThisTriangle(point.vertex, Vector3.back);
                        break; // each point can only be inside 1 triangle, move to next point
                    }
                }

            }

            Polygon polygone = Utilities.CreatePolygon(trianglesSurroundingHole, out List<Edge> multiNormalEdges);


            List<Point> points = new List<Point>(hole.points);
            foreach (var point in points)
            {
                foreach (var edge in multiNormalEdges)
                {
                    if (TryFindIntesectionPoint(edge.a.position, edge.b.position, point.Position, point.nextPoint.Position, out Vector3 intersection))
                    {
                        VertexData intersectionVertex = new VertexData(-1, intersection, edge.Normal, edge.UV);
                        int holeIndex = hole.points.IndexOf(point);
                        hole.points.Insert((holeIndex + 1) % hole.PointsCount, new Point(intersectionVertex));
                    }
                }

            }

            // get all normals
            List<Vector3> normals = new List<Vector3>();
            foreach (var point in polygone)
            {
                if (!normals.Contains(point.vertex.normal))
                    normals.Add(point.vertex.normal);
            }

            // seperate shapes by normals
            foreach (var normal in normals)
            {
                List<VertexData> subPolygon = new List<VertexData>();
                foreach (var point in polygone)
                {
                    if (point.vertex.normal == normal)
                        subPolygon.Add(point.vertex);
                }

                List<VertexData> subHole = new List<VertexData>();
                foreach (var point in hole)
                {
                    if (point.vertex.normal == normal)
                        subHole.Add(point.vertex);
                }


                PolyTree polyTree = new PolyTree();
                polyTree.shape = new Polygon(subPolygon);
                polyTree.AddChild(new Polygon(subHole));

                foreach (var trianlge in EarClipper.FillPolygoneTree(polyTree))
                {
                    mesh.AddTriangle(trianlge);
                }

            }

            GameObject outsideObject = CloneObject(cuttable, cutter, mesh, "outside", matrixTranslator);
            return new CutResult();

        }

        private static void AddVerticesTouchingTriangle(ref List<VertexData> vertices, Triangle triangle, ref List<Triangle> trianglesToSearch, ref List<Triangle> currentTriangles)
        {

            for (int i = 0; i < 3; i++)
            {
                VertexData vertex = triangle[i];
                if (!vertices.Contains(vertex))
                    vertices.Add(vertex);

                Triangle t = trianglesToSearch.Find(t => t.Containt(vertex.position));
                if (t != null)
                {

                    trianglesToSearch.Remove(t);
                    currentTriangles.Add(t);
                    AddVerticesTouchingTriangle(ref vertices, t, ref trianglesToSearch, ref currentTriangles);
                }
            }
        }

        private static Dictionary<Vector3, Polygon> SeperateByNormals(List<VertexData> verts)
        {
            Dictionary<Vector3, Polygon> polygons = new Dictionary<Vector3, Polygon>();
            foreach (var v in verts)
            {
                if (polygons.ContainsKey(v.normal))
                    polygons[v.normal].points.Add(new Point(v));
                else
                {
                    Polygon p = new Polygon();
                    p.points.Add(new Point(v));
                    polygons.Add(v.normal, p);
                }
            }

            foreach (Polygon p in polygons.Values)
            {
                p.Refresh();
            }

            return polygons;
        }

        private static Dictionary<Vector3, List<Triangle>> SeperateByNormals(List<Triangle> triangles)
        {
            Dictionary<Vector3, List<Triangle>> result = new Dictionary<Vector3, List<Triangle>>();
            foreach (var t in triangles)
            {
                if (result.ContainsKey(t.Normal))
                    result[t.Normal].Add(t);
                else
                {
                    result.Add(t.Normal, new List<Triangle>() { t });
                }
            }

            return result;
        }

        public static bool TryFindIntesectionPoint(Vector3 p11, Vector3 p12, Vector3 p21, Vector3 p22, out Vector3 intersection)
        {
            intersection = Vector3.zero;


            Vector3 p = p11;
            Vector3 r = p12 - p11;

            Vector3 q = p21;
            Vector3 s = p22 - p21;

            float cross_rs = Cross(r, s);

            float t = Cross(q - p, s) / cross_rs;
            float u = Cross(q - p, r) / cross_rs;

            if (cross_rs != 0 &&
                t >= 0 && t <= 1 &&
                u >= 0 && u <= 1)

            {
                intersection = p + t * r;
                return true;
            }
            else
                return false;

            float Cross(Vector2 v, Vector2 w)
            {
                return v.x * w.y - v.y * w.x;
            }
        }

    }
}