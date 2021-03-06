using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintFacesBrokenSplit : MonoBehaviour
{
    public GameObject brushCursor, brushContainer; //The cursor that overlaps the model and our container for the brushes painted
	public Camera sceneCamera;
	public List<Camera> canvasCam;  //The camera that looks at the model, and the camera that looks at the canvas.
    public Sprite cursorPaint; // Cursor for the differen functions 
    public RenderTexture canvasTexture; // Render Texture that looks at our Base Texture and the painted brushes
    public List<Material> baseMaterial; // The material of our base texture (Were we will save the painted texture)
	public List<Material> canvasMaterial; // The material rendered onto final products.

	public Spherize planet;

	public float brushSize = 1.0f; //The size of our brush
    public Color brushColor; //The selected color
    int brushCounter = 0, MAX_BRUSH_COUNT = 1000; //To avoid having millions of brushes
    bool saving = false; //Flag to check if we are saving the texture

	public List<Face> faces;
	[SerializeField] GameObject facePF;

    // Start is called before the first frame update
    void Start()
    {
		

		StartCoroutine(planetFaces());
			
	}

	IEnumerator planetFaces()
    {
		yield return 1f;
		{ 
		canvasTexture = new RenderTexture(canvasTexture.width, canvasTexture.height, canvasTexture.depth, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
		canvasTexture.name = "RendTex" + UnityEngine.Random.Range(0, 1000);
		canvasTexture.antiAliasing = canvasTexture.antiAliasing;
		canvasCam[0].targetTexture = canvasTexture;
		canvasMaterial[0].SetTexture("_MainTex", canvasTexture);
		planet.heightMapRT = canvasTexture;
		}

		int iterator = 0;
		foreach (var face in planet.faces)
		{
			face.GetComponent<MeshRenderer>().material.SetTexture("_NoiseTexture", canvasTexture);

			Face newFace = Instantiate(facePF).GetComponent<Face>();
			newFace.faceGO = face;

			newFace.canvasTexture = new RenderTexture(canvasTexture.width, canvasTexture.height, canvasTexture.depth, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
			newFace.canvasTexture.name = "RendTex" + iterator;
			newFace.canvasTexture.antiAliasing = canvasTexture.antiAliasing;
			newFace.faceCanvasCam = canvasCam[iterator];
			newFace.faceCanvasCam.targetTexture = newFace.canvasTexture;

			newFace.canvasMaterial = new Material(canvasMaterial[0]);
			newFace.canvasMaterial.name = newFace.canvasMaterial.name + iterator;

			newFace.canvasMaterial.SetTexture("_MainTex", newFace.canvasTexture);
			//planet.heightMapRT = canvasTexture;

			newFace.faceGO.GetComponent<MeshRenderer>().material.SetTexture("_NoiseTexture", newFace.canvasTexture);
			

			faces.Add(newFace);
			iterator++;
		}
	}


	public float autoSaveTime = 5f;
	float timeSinceLastInput = 0;
	bool hasAutoSaved = true;
    // Update is called once per frame
    void Update()
    {
		timeSinceLastInput += Time.deltaTime;
		if(timeSinceLastInput > autoSaveTime && hasAutoSaved == false)
        {
			hasAutoSaved = true;
			brushCursor.SetActive(false);
			saving = true;
			Invoke("SaveTexture", 0.1f);
		}
			

		UpdateBrushColor();
        if (Input.GetMouseButton(0))
        {
			brushColor = Color.white;
			if (Input.GetKey(KeyCode.LeftShift))
				brushColor = Color.black;
            DoAction();
        }
		if(Input.GetMouseButtonDown(1))
        {
			hasAutoSaved = true;
			brushCursor.SetActive(false);
			saving = true;
			Invoke("SaveTexture", 0.1f);
		}
        UpdateBrushCursor();
    }

    private void UpdateBrushColor()
    {
        //throw new NotImplementedException();
    }


	//The main action, instantiates a brush or decal entity at the clicked position on the UV map
	void DoAction()
	{
		timeSinceLastInput = 0;
		hasAutoSaved = false;
		if (saving)
			return;
		Vector3 uvWorldPosition = Vector3.zero;
		if (HitTestUVPosition(ref uvWorldPosition))
		{
			GameObject brushObj;
			brushObj = (GameObject)Instantiate(Resources.Load("TexturePainter-Instances/BrushEntity")); //Paint a brush
			brushObj.GetComponent<SpriteRenderer>().color = brushColor; //Set the brush color

			brushColor.a = brushSize * 2.0f; // Brushes have alpha to have a merging effect when painted over.
			brushObj.transform.parent = brushContainer.transform; //Add the brush to our container to be wiped later
			brushObj.transform.localPosition = uvWorldPosition; //The position of the brush (in the UVMap)
			brushObj.transform.localScale = Vector3.one * brushSize;//The size of the brush
		}
		brushCounter++; //Add to the max brushes
		if (brushCounter >= MAX_BRUSH_COUNT)
		{ //If we reach the max brushes available, flatten the texture and clear the brushes
			brushCursor.SetActive(false);
			saving = true;
			Invoke("SaveTexture", 0.1f);

		}
	}
	//To update at realtime the painting cursor on the mesh
	void UpdateBrushCursor()
	{
		Vector3 uvWorldPosition = Vector3.zero;
		if (HitTestUVPosition(ref uvWorldPosition) && !saving)
		{
			brushCursor.SetActive(true);
			brushCursor.transform.position = uvWorldPosition + brushContainer.transform.position;
		}
		else
		{
			brushCursor.SetActive(false);
		}
	}
	//Returns the position on the texuremap according to a hit in the mesh collider
	bool HitTestUVPosition(ref Vector3 uvWorldPosition)
	{
		RaycastHit hit;
		Vector3 cursorPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.0f);
		Ray cursorRay = sceneCamera.ScreenPointToRay(cursorPos);
		if (Physics.Raycast(cursorRay, out hit, 200))
		{
			MeshCollider meshCollider = hit.collider as MeshCollider;
			if (meshCollider == null || meshCollider.sharedMesh == null)
				return false;

			
			Vector2 pixelUV = new Vector2(hit.textureCoord.x,hit.textureCoord.y);
			
			uvWorldPosition.x = pixelUV.x - canvasCam[0].orthographicSize;//To center the UV on X
			uvWorldPosition.y = pixelUV.y - canvasCam[0].orthographicSize;//To center the UV on Y
			uvWorldPosition.z = 0.0f;
			//Debug.Log("Hit: " + hit.collider.name + ", at " + hit.point + ", translated to : " + pixelUV + ", Ult: " + uvWorldPosition);
			return true;
		}
		else
		{
			return false;
		}

	}
	//Sets the base material with a our canvas texture, then removes all our brushes
	void SaveTexture()
	{
		brushCounter = 0;
		System.DateTime date = System.DateTime.Now;
		RenderTexture.active = canvasTexture;
		Texture2D tex = new Texture2D(canvasTexture.width, canvasTexture.height, TextureFormat.RGBA32, false);
		tex.ReadPixels(new Rect(0, 0, canvasTexture.width, canvasTexture.height), 0, 0);
		tex.Apply();
		RenderTexture.active = null;
		baseMaterial[0].mainTexture = tex; //Put the painted texture as the base
		foreach (Transform child in brushContainer.transform)
		{//Clear brushes
			Destroy(child.gameObject);
		}
		//StartCoroutine ("SaveTextureToFile"); //Do you want to save the texture? This is your method!
		Invoke("ShowCursor", 0.1f);
	}
	//Show again the user cursor (To avoid saving it to the texture)
	void ShowCursor()
	{
		saving = false;
	}

	////////////////// PUBLIC METHODS //////////////////

	
	public void SetBrushSize(float newBrushSize)
	{ //Sets the size of the cursor brush or decal
		brushSize = newBrushSize;
		brushCursor.transform.localScale = Vector3.one * brushSize;
	}

	////////////////// OPTIONAL METHODS //////////////////


}
