using Extentions;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class MatrixTranslator
{
    private Transform target;
    private Transform cutter;

    public MatrixTranslator(Transform target, Transform cutter)
    {
        this.target = target;
        this.cutter = cutter;
    }


    /// <summary>
    /// <b>Scale</b>, <b>Move</b> and <b>Rotate</b> the mesh<br/>
    /// remember that the cutting is based on world origin (0,0,0) on XY plane
    /// </summary>
    /// <param name="target"></param>
    /// <param name="cutter"></param>
    public void MoveAround(VertexMesh mesh)
    {
        if (target.lossyScale.HaveZero())
        {
            Debug.LogError($"can't do a cut, since <color=blue>{target.name}</color> global scale have a 0", target);
            return;
        }

        if (cutter.lossyScale.HaveZero())
        {
            Debug.LogError($"can't do a cut, since <color=blue>{cutter.name}</color> global scale have a 0", cutter);
            return;
        }

        Matrix4x4 targetMatrix = Matrix4x4.Rotate(target.rotation);
        Matrix4x4 cutterMatrix = Matrix4x4.Rotate(cutter.rotation).inverse;

        Vector3 offset = target.position - cutter.position;
        Vector3 scale = target.lossyScale;
        foreach (var vertex in mesh.Vertices)
        {
            vertex.position.Scale(scale);
            vertex.position = targetMatrix.MultiplyPoint(vertex.position);
            vertex.normal = targetMatrix.MultiplyPoint(vertex.normal);

            vertex.position += offset;

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
    public void ReturnNormal(VertexMesh mesh)
    {
        // same as "MoveAround" method, just on the other direction

        Matrix4x4 targetMatrix = Matrix4x4.Rotate(target.rotation).inverse;
        Matrix4x4 cutterMatrix = Matrix4x4.Rotate(cutter.rotation);

        Vector3 offsetBack = cutter.position - target.position;
        Vector3 scale = target.lossyScale.OneOver();
        foreach (var vertex in mesh.Vertices)
        {
            // make sure to only move each vertex once
            if (vertex.isOnCurrectPosition)
                continue;


            vertex.position = cutterMatrix.MultiplyPoint(vertex.position);
            vertex.normal = cutterMatrix.MultiplyPoint(vertex.normal);

            vertex.position += offsetBack;

            vertex.position = targetMatrix.MultiplyPoint(vertex.position);
            vertex.normal = targetMatrix.MultiplyPoint(vertex.normal).normalized;


            vertex.position.Scale(scale);
            vertex.isOnCurrectPosition = true;
        }
    }



    public Bounds MoveAround(Bounds bounds)
    {

        if (target.lossyScale.HaveZero())
        {
            Debug.LogError($"can't do a cut, since <color=blue>{target.name}</color> global scale have a 0", target);
            return bounds;
        }

        if (cutter.lossyScale.HaveZero())
        {
            Debug.LogError($"can't do a cut, since <color=blue>{cutter.name}</color> global scale have a 0", cutter);
            return bounds;
        }

        Matrix4x4 targetMatrix = Matrix4x4.Rotate(target.rotation);
        Matrix4x4 cutterMatrix = Matrix4x4.Rotate(cutter.rotation).inverse;

        Vector3 offset = target.position - cutter.position;
        Vector3 scale = target.lossyScale;

        Vector3 min = bounds.min;

        min.Scale(scale);
        min = targetMatrix.MultiplyPoint(min);
        min += offset;
        min = cutterMatrix.MultiplyPoint(min);


        Vector3 max = bounds.max;

        max.Scale(scale);
        max = targetMatrix.MultiplyPoint(max);
        max += offset;
        max = cutterMatrix.MultiplyPoint(max);

        return new Bounds(min, max);
    }
}
