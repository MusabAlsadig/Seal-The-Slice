using System.Collections.Generic;
using UnityEngine;

public class ColliderBasedCutter : CutterBase
{
    private List<CuttableObject> objectsCurrentlyCutting = new List<CuttableObject>();

    int count = 0;
    private void OnTriggerEnter(Collider other)
    {

        if (count > 10)
            return;
            if (other.TryGetComponent(out CuttableObject cuttableObject))
        {
            if (objectsCurrentlyCutting.Contains(cuttableObject))
                return;

            objectsCurrentlyCutting.Add(cuttableObject);
            CutResult cutResult = Cut(cuttableObject);

            foreach (var submeshObject in cutResult)
            {
                objectsCurrentlyCutting.Add(submeshObject.GetComponent<CuttableObject>());
            }

            Debug.Log("cutting" + other.name);
            count++;


        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out CuttableObject cuttableObject))
            objectsCurrentlyCutting.Remove(cuttableObject);
    }

}
