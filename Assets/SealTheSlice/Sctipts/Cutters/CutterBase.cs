using System.Collections.Generic;
using UnityEngine;

namespace SealTheSlice
{
    public class CutterBase : MonoBehaviour
    {
        public List<Vector2> points = new List<Vector2>();
        [SerializeField]
        private CuttableObject defaultTarget;
        [SerializeField]
        private Material materialForTheFiller;
        [SerializeField]
        private float fadeTime = 3;
        [SerializeField]
        private bool keepInside;

        public Material Material => materialForTheFiller;
        public float FadeTime => fadeTime;

        public bool KeepInside => keepInside;

        public void CutShapeInFront()
        {
            CuttableObject target;
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo)
                && hitInfo.transform.TryGetComponent(out target))
            {
                Debug.Log("slicing " + target.name);
            }
            else
            {
                Debug.Log("no object there, will use default");
                target = defaultTarget;
            }

            Cut(target);
        }

        public CutResult Cut(CuttableObject target)
        {
            if (target.BeenCuted)
                return null;
            CutResult cutResult = MeshSlicer.SeperateByCut_InsertWithEarClipper(target, this);
            target.AfterCutCleanup();
            return cutResult;
        }

        public virtual CutShape GetShape()
        {
            List<Vector2> copyPoints = new List<Vector2>();
            for (int i = 0; i < points.Count; i++)
            {
                copyPoints.Add(Vector2.Scale(points[i], transform.lossyScale));
            }
            return new CutShape(copyPoints);
        }

        public Polygon ToPolygon()
        {
            var vertices = points.ConvertAll(p => new VertexData(-1, p, Vector3.zero, Vector2.zero));
            return new Polygon(vertices);
        }

        #region Utilites
        public void ReversDirection()
        {
            points.Reverse();
        }
        #endregion
    }
}