using UnityEngine;

public class CutFillEffect : MonoBehaviour
{
    private float maxTime = 1;
    private float time;
    private Material material;
    public void Setup(float time, Material material)
    {
        maxTime = time;
        this.time = time;
        this.material = material;
        GetComponent<MeshRenderer>().material = material;
    }

    private void Update()
    {
        if (time <= 0)
        {
            Destroy(this);
            return;
        }

        time -= Time.deltaTime;

        material.SetFloat("_Fade", time / maxTime);
    }
}
