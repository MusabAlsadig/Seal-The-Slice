using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ColliderBasedCutter : Cutter
{
    List<AdvancedCutableObject> objectsCurrentlyCutting = new List<AdvancedCutableObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out AdvancedCutableObject cutableObject))
        {
            if (objectsCurrentlyCutting.Contains(cutableObject))
                return;

            objectsCurrentlyCutting.Add(cutableObject);
            var newObjects = cutableObject.CutWith(this);

            foreach (var submeshObject in newObjects)
            {
                objectsCurrentlyCutting.Add(submeshObject.GetComponent<AdvancedCutableObject>());
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out AdvancedCutableObject cutableObject))
            objectsCurrentlyCutting.Remove(cutableObject);
    }

}
