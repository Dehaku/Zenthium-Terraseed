using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarkerController : MonoBehaviour
{
    public GameObject onScreen;
    public GameObject offScreen;

    Image onScreenImage;
    Image offScreenImage;

    public bool isOnScreen = true;

    public void SetColor(Color color)
    {
        onScreenImage.color = color;
        offScreenImage.color = color;
    }

    // Start is called before the first frame update
    void Start()
    {
        onScreenImage = onScreen.GetComponent<Image>();
        offScreenImage = offScreen.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isOnScreen)
        {
            onScreen.SetActive(true);
            offScreen.SetActive(false);
        }
        else
        {
            onScreen.SetActive(false);
            offScreen.SetActive(true);
        }
            

    }
}
