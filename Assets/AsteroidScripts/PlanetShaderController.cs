using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetShaderController : MonoBehaviour
{
    public Mass mass;
    public List<Material> materials;
    public Rigidbody rb;

    public float iceCoverageMultiplier = -0.75f;
    public float icePower = 1;
    public float iceNoiseIntensity = 80;

    // Start is called before the first frame update
    void Start()
    {
        CacheMaterials();
        if (Random.Range(0, 1000) == 1)
            Debug.Log("Find a way to make a numerical gradiant with multiple points, and sample from it for polar ice caps scaling");
    }

    public void CacheMaterials()
    {
        StartCoroutine(CacheMaterialsIE());
    }

    IEnumerator CacheMaterialsIE()
    {
        materials.Clear();
        yield return new WaitForSeconds(1f);
        var Spherize = GetComponentInChildren<Spherize>();
        if (Spherize)
        {
            var mRends = Spherize.GetComponentsInChildren<MeshRenderer>();
            foreach (var item in mRends)
            {
                materials.Add(item.material);
            }
        }
    }

    void UpdateShaders()
    {
        if (materials.Count == 0)
            return;
        float iceCoverage = (transform.localScale.x) * (iceCoverageMultiplier);
        foreach (var item in materials)
        {

            item.SetFloat("IceCapReach", iceCoverage);
            item.SetVector("WorldPos", rb.position);
            item.SetFloat("IceNoiseIntensity", iceNoiseIntensity);



        }

        // Do shader updates
    }

    // Update is called once per frame
    void Update()
    {
        UpdateShaders();
    }
}
