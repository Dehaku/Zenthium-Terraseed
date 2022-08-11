using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetShaderController : MonoBehaviour
{
    public Mass mass;
    public List<Material> materials;
    public Rigidbody rb;

    public float iceCoverageMultiplier = 1;

    // Start is called before the first frame update
    void Start()
    {
        CacheMaterials();
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

        float iceCoverage = rb.transform.localScale.x * iceCoverageMultiplier;
        foreach (var item in materials)
        {

            item.SetFloat("IceCapReach", iceCoverage);
            item.SetVector("WorldPos", rb.position);

        }

        // Do shader updates
    }

    // Update is called once per frame
    void Update()
    {
        UpdateShaders();
    }
}
