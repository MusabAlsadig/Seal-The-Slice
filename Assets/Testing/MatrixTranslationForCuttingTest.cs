using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Extentions;

internal class MatrixTranslationForCuttingTest : MonoBehaviour
{

    Mesh target;
    Transform target_transform;

    [SerializeField]
    private List<Vector2> points = new List<Vector2>();

    [Button]
    private void Check()
    {
        if (!Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo))
        {
            Debug.Log("no object there");
            target = null;
            return;
        }

        target_transform = hitInfo.transform;
        target = hitInfo.transform.GetComponent<MeshFilter>().sharedMesh;
    }

    private void OnDrawGizmos()
    {
        if (target == null)
            return;

        Matrix4x4 targetMatrix = Matrix4x4.Rotate(target_transform.rotation);
        Matrix4x4 cutterMatrix = Matrix4x4.Rotate(transform.rotation).inverse;

        for (int i = 0; i < target.vertexCount; i++)
        {
            int nextIndex = i + 1 < target.vertexCount ? i + 1 : 0;

            Vector3 point = target.vertices[i];
            Vector3 nextPoint = target.vertices[nextIndex];
            Vector3 normal = target.normals[i];

            Vector3 scale = target_transform.lossyScale;
            point.Scale(scale);
            nextPoint.Scale(scale);

            // translate with target matrix
            point = targetMatrix.MultiplyPoint(point);
            nextPoint = targetMatrix.MultiplyPoint(nextPoint);
            normal = targetMatrix.MultiplyPoint(normal);

            // add the movement of target
            point += target_transform.position;
            nextPoint += target_transform.position;

            // remove the movement of cutter 
            point -= transform.position;
            nextPoint -= transform.position;

            // translate with cutter matrix
            point = cutterMatrix.MultiplyPoint(point);
            nextPoint = cutterMatrix.MultiplyPoint(nextPoint);
            normal = cutterMatrix.MultiplyPoint(normal);

            

            // lines
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(point, nextPoint);

            // vertices
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(point, 0.06f);

            // normals
            Gizmos.color = Color.gray;
            Gizmos.DrawLine(point, point + normal);



            // ===================return back======================

            cutterMatrix = cutterMatrix.inverse;
            targetMatrix = targetMatrix.inverse;

            

            // translate with cutter matrix
            point = cutterMatrix.MultiplyPoint(point);
            nextPoint = cutterMatrix.MultiplyPoint(nextPoint);
            normal = cutterMatrix.MultiplyPoint(normal);

            

            // add the movement of target
            point -= target_transform.position;
            nextPoint -= target_transform.position;

            // remove the movement of cutter 
            point += transform.position;
            nextPoint += transform.position;

            // translate with target matrix
            point = targetMatrix.MultiplyPoint(point);
            nextPoint = targetMatrix.MultiplyPoint(nextPoint);
            normal = targetMatrix.MultiplyPoint(normal);

            Vector3 inverse = scale.OneOver();

            point.Scale(inverse);
            nextPoint.Scale(inverse);

            // lines
            Gizmos.color = Color.white;
            Gizmos.DrawLine(point, nextPoint);

            // vertices
            Gizmos.DrawSphere(point, 0.06f);

            // normals
            Gizmos.DrawLine(point, point + normal);



            cutterMatrix = cutterMatrix.inverse;
            targetMatrix = targetMatrix.inverse;
        }



        for (int i = 0; i < points.Count; i++)
        {
            int nextIndex = i + 1 < points.Count ? i + 1 : 0;
            Vector3 point = (Vector3)points[i];
            Vector3 nextPoint = (Vector3)points[nextIndex];

            point.Scale(transform.lossyScale);
            nextPoint.Scale(transform.lossyScale);

            // lines
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(point, nextPoint);

            // vertices
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(point, 0.06f);
        }

    }


}