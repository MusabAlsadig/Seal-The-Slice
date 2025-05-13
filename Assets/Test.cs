using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sirenix.OdinInspector;
internal class Test : MonoBehaviour
{
    [Button]
    private void Calculate()
    {
        foreach (Transform child in transform)
        {
            Debug.Log(child.name + " have angle= " + Vector3.Angle(Vector3.forward, child.localPosition));
        }
    }
}