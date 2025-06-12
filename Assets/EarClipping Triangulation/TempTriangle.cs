using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

public class TempTriangle
{
    public Vector3[]points = new Vector3[3];

    public TempTriangle(Point p)
    {
        points[0] = p.lastPoint.position;
        points[1] = p.position;
        points[2] = p.nextPoint.position;
        
    }


    
}
