using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SealTheSlice
{
    public class CutResult : IEnumerable<GameObject>
    {
        public GameObject outterShape;
        public GameObject innerShape;
        public GameObject outerShapeFill;
        public GameObject innerShapeFill;

        public CutResult() { }

        public CutResult(GameObject outterShape, GameObject innerShape)
        {
            this.outterShape = outterShape;
            this.innerShape = innerShape;
        }

        public CutResult(GameObject outterShape, GameObject innerShape, GameObject outerShapeFill, GameObject innerShapeFill)
        {
            this.outterShape = outterShape;
            this.innerShape = innerShape;
            this.outerShapeFill = outerShapeFill;
            this.innerShapeFill = innerShapeFill;
        }

        public IEnumerator<GameObject> GetEnumerator()
        {
            if (outterShape != null)
                yield return outterShape;
            if (innerShape != null)
                yield return innerShape;
            if (outerShapeFill != null)
                yield return outerShapeFill;
            if (innerShapeFill != null)
                yield return innerShapeFill;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
    }
}