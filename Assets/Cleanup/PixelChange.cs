using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelChange : MonoBehaviour
{
    public RenderTexture renderTexture; // renderTextuer that you will be rendering stuff on
    public Renderer renderer; // renderer in which you will apply changed texture
    Texture2D texture;

    void Start() 
    {

        texture = new Texture2D(renderTexture.width, renderTexture.height);
        renderer.material.mainTexture = texture;
        //make texture2D because you want to "edit" it. 
        //however this is not a way to apply any post rendering effects because
        //this way, you are reading it through CPU(slow).
    }

    int at = 0;
    // Update is called once per frame
    void Update()
    {
        at++;
        RenderTexture.active = renderTexture;
        //don't forget that you need to specify rendertexture before you call readpixels
        //otherwise it will read screen pixels.
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        for (int i = 0; i < renderTexture.width * .2f; i++)
            for (int j = 0; j < renderTexture.height; j++)
            {
                texture.SetPixel((at + i), j, new Color(1, 0, 0));
            }
        texture.Apply();
        RenderTexture.active = null; //don't forget to set it back to null once you finished playing with it. 
    }
}