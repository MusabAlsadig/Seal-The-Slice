using UnityEngine;

namespace Assets
{
    public class Line
    {
        private VertexData pointA;
        private VertexData pointB;

        private float x, y, h, k;

        public Line(VertexData pointA, VertexData pointB)
        {
            this.pointA = pointA;
            this.pointB = pointB;

        }

    }
}
