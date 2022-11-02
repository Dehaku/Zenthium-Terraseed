using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetShaderController : MonoBehaviour
{
    public Mass mass;
    public List<Material> materials;
    public Rigidbody rb;
    public GameObject planetOceanPF;
    Transform planetOcean;
    [SerializeField] Transform planetGFX;
    Transform planetAtmosphere;
    
    public float iceCoverageMultiplier = -0.75f;
    public float icePower = 1;
    public float iceNoiseIntensity = 80;

    [Header("Ocean")]
    public float oceanMinLevel = 2f;
    public float oceanMaxAdditionalLevel = 1f;
    public float oceanArbitraryMulti = 3;
    public bool shrinkLandByPercentage = false;

    // Start is called before the first frame update
    void Start()
    {
        CacheMaterials();
        if (Random.Range(0, 1000) == 1)
        {
            Debug.Log("Find a way to make a numerical gradiant with multiple points, and sample from it for polar ice caps scaling");
            // https://stackoverflow.com/questions/66522629/given-3-or-more-numbers-or-vectors-how-do-i-interpolate-between-them-based-on-a
            Debug.Log("Make Nebulas by using 'cloud generator' type assets free floating in space.");
            Debug.Log("Reflections for ocean");
            Debug.Log("How to convert colors to HDR colors and vice versa.");
            // https://forum.unity.com/threads/how-to-change-hdr-color-intensity-on-custom-material-via-script.1075846/
            // https://answers.unity.com/questions/1084467/assigning-hdr-color-to-material-property-via-scrip.html
            Debug.Log("How to average colors");
            // https://answers.unity.com/questions/725895/best-way-to-mix-color-values.html
        }

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

            IceyLand(item);

                //item.SetFloat("IceNoiseIntensity", iceNoiseIntensity);


        }

        // Do shader updates
    }

    void IceyLand(Material item)
    {
        if(mass.GetEnergy(false) < 0)
        {
            item.SetFloat("BlendIce", 1);
        }
        else
            item.SetFloat("BlendIce", 0);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateShaders();
        UpdateOcean();
    }

    void SpawnOcean()
    {
        if(planetOcean == null)
        {
            
            planetOcean = Instantiate(planetOceanPF, transform).transform;
        }
    }

    void UpdateOcean()
    {
        if(mass.GetOceanLevel(false) <= 0)
        {
            planetOcean?.gameObject.SetActive(false);
            return;
        }
        else
            planetOcean?.gameObject.SetActive(true);

        if (!planetOcean)
        {
            SpawnOcean();
            return;
        }

        var oceanScale = (oceanMinLevel + (oceanMaxAdditionalLevel * mass.GetOceanLevel(false))) * oceanArbitraryMulti;

        planetOcean.localScale =  Vector3.one * oceanScale;

        if(shrinkLandByPercentage)
            CounterScaleLandAndSea();
    }

    void CounterScaleLandAndSea()
    {
        var percents = mass.GetPercentageOfMatterStates();
        planetGFX.localScale = Vector3.one * percents[0];
    }
}
