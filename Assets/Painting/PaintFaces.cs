using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintFaces : MonoBehaviour
{
	[Header("Base?")]
	public bool isBase = false;
	public PaintFaces basePaintFaces;
	[Header("Settings")]
	public GameObject brushCursor, brushContainer, brushSpritePF; //The cursor that overlaps the model and our container for the brushes painted
	public Camera sceneCamera, canvasCam;  //The camera that looks at the model, and the camera that looks at the canvas.
	public Sprite cursorPaint; // Cursor for the differen functions 
	public RenderTexture canvasTexture; // Render Texture that looks at our Base Texture and the painted brushes
	public MeshRenderer canvasRenderer;
	public Material baseMaterial; // The material of our base texture (Were we will save the painted texture)
	public Material canvasMaterial; // The material rendered onto final products.

	public MeshRenderer paintTarget;
	public int RTLayer;
	public Spherize planet;
	public PlanetSide.Side mySide;

	public float brushSize = 1.0f; //The size of our brush
	public Color brushColor; //The selected color
	int brushCounter = 0, MAX_BRUSH_COUNT = 1000; //To avoid having millions of brushes
	bool saving = false; //Flag to check if we are saving the texture
	bool needsDelete = false;

	[Header("Neighbors")]

	public PaintFaces NeighborLeft;
	public Vector3 NeighborLeftRotate;
	public PaintFaces NeighborUp;
	public Vector3 NeighborUpRotate;
	public PaintFaces NeighborRight;
	public Vector3 NeighborRightRotate;
	public PaintFaces NeighborDown;
	public Vector3 NeighborDownRotate;

	[Header("Collapse Corners")]
	public bool collapseCorners = true;
	public float collapseCornerRange = 1;



    private void Awake()
    {
	}

    // Start is called before the first frame update
    void Start()
	{
		if (isBase)
			return;

		brushCursor = basePaintFaces.brushCursor;
		sceneCamera = basePaintFaces.sceneCamera;
		baseMaterial = new Material(basePaintFaces.baseMaterial);
		canvasRenderer.material = baseMaterial;

		canvasMaterial = new Material(basePaintFaces.canvasMaterial);


		canvasTexture = new RenderTexture(basePaintFaces.canvasTexture.width, basePaintFaces.canvasTexture.height, basePaintFaces.canvasTexture.depth, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
		canvasTexture.antiAliasing = basePaintFaces.canvasTexture.antiAliasing;
		canvasCam.targetTexture = canvasTexture;
		canvasMaterial.SetTexture("_MainTex", canvasTexture);

		if (paintTarget)
        {
			paintTarget.sharedMaterial = canvasMaterial;

			if (paintTarget.sharedMaterial != canvasMaterial)
			{
				Debug.Log("ZZZZZZNot synced material!" + paintTarget.name);
			}
		}
			

		

		if (planet)
		{
			planet.heightMapRT = canvasTexture;
			StartCoroutine(planetFaces());
		}


	}


	public void Init()
    {
		brushCursor = basePaintFaces.brushCursor;
		sceneCamera = basePaintFaces.sceneCamera;
		baseMaterial = new Material(basePaintFaces.baseMaterial);
		canvasRenderer.material = baseMaterial;

		canvasMaterial = new Material(basePaintFaces.canvasMaterial);


		canvasTexture = new RenderTexture(basePaintFaces.canvasTexture.width, basePaintFaces.canvasTexture.height, basePaintFaces.canvasTexture.depth, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
		canvasTexture.antiAliasing = basePaintFaces.canvasTexture.antiAliasing;
		canvasCam.targetTexture = canvasTexture;
		canvasMaterial.SetTexture("_MainTex", canvasTexture);

		if (paintTarget)
			paintTarget.sharedMaterial = canvasMaterial;

		if (paintTarget.sharedMaterial != canvasMaterial)
		{
			Debug.Log("ZZZZZZNot synced material!" + paintTarget.name);
		}

		if (planet)
		{
			planet.heightMapRT = canvasTexture;
			StartCoroutine(planetFaces());
		}

	}

    IEnumerator planetFaces()
	{
		yield return 1f;
		foreach (var face in planet.faces)
		{
			face.GetComponent<MeshRenderer>().material.SetTexture("_NoiseTexture", canvasTexture);
			//face.GetComponent<MeshRenderer>().material.SetTexture("_BarrenTexture", canvasTexture);
		}
	}


	public float autoSaveTime = 5f;
	float timeSinceLastInput = 0;
	bool hasAutoSaved = true;
	// Update is called once per frame
	void Update()
	{
		if (isBase)
			return;



		//timeSinceLastInput += Time.deltaTime;
		if (timeSinceLastInput > autoSaveTime && hasAutoSaved == false)
		{
			hasAutoSaved = true;
			brushCursor.SetActive(false);
			saving = true;
			Invoke("SaveTexture", 0.1f);
		}

		
		//UpdateBrushCursor();


		/* Needs work.
		if (Input.GetKey(KeyCode.LeftCommand) && Input.GetMouseButton(0))//if (Input.GetKeyDown(KeyCode.F))
			MatchEdges();
		if (Input.GetKeyDown(KeyCode.F))
			MatchEdges();
		*/

	}

	


	//The main action, instantiates a brush or decal entity at the clicked position on the UV map
	public bool DoAction(LayerMask paintRayLayer, bool white = true) // Returns true if brush container is full, use for saving itself and neighbors without clearing.
	{
		if (needsDelete == false)
			brushContainer.SetActive(true);

		if (white)
			brushColor = Color.white;
		else
			brushColor = Color.black;


		timeSinceLastInput = 0;
		hasAutoSaved = false;
		if (saving || needsDelete)
			return false;
		Vector3 uvWorldPosition = Vector3.zero;
		if (HitTestUVPosition(ref uvWorldPosition, paintRayLayer))
		{
			GameObject brushObj;
			brushObj = Instantiate(brushSpritePF); //Paint a brush
			//brushObj = (GameObject)Instantiate(Resources.Load("TexturePainter-Instances/BrushEntity")); //Paint a brush
			//brushObj = (GameObject)Instantiate(Resources.Load("TexturePainter-Instances/BioBrushEntity")); //Paint a brush
			brushObj.GetComponent<SpriteRenderer>().color = brushColor; //Set the brush color

			brushColor.a = brushSize * 2.0f; // Brushes have alpha to have a merging effect when painted over.
			brushObj.transform.parent = brushContainer.transform; //Add the brush to our container to be wiped later
			brushObj.transform.localPosition = uvWorldPosition; //The position of the brush (in the UVMap)
			brushObj.transform.localScale = Vector3.one * brushSize;//The size of the brush
			brushObj.layer = RTLayer;
		}
		brushCounter++; //Add to the max brushes
		if (brushCounter >= MAX_BRUSH_COUNT)
		{ //If we reach the max brushes available, flatten the texture and clear the brushes
			return true;
		}
		return false;
	}

	public void TriggerSaveMethod()
    {
		//brushCursor.SetActive(false);
		saving = true;
		//Invoke("SaveTexture", 0.1f);
		SaveTexture();
	}

	
	//Returns the position on the texuremap according to a hit in the mesh collider
	bool HitTestUVPosition(ref Vector3 uvWorldPosition, LayerMask paintRayLayer)
	{
		RaycastHit hit;
		Vector3 cursorPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.0f);
		Ray cursorRay = sceneCamera.ScreenPointToRay(cursorPos);
		if (Physics.Raycast(cursorRay, out hit, 20000, paintRayLayer))
		{
			MeshCollider meshCollider = hit.collider as MeshCollider;
			if (meshCollider == null || meshCollider.sharedMesh == null)
				return false;


			Vector2 pixelUV = new Vector2(hit.textureCoord.x, hit.textureCoord.y);

			if(collapseCorners)
            {
				if (pixelUV.x < collapseCornerRange && pixelUV.y<collapseCornerRange)
					pixelUV.y = pixelUV.x;
				if (pixelUV.x > (1f - collapseCornerRange) && pixelUV.y > (1f - collapseCornerRange))
					pixelUV.x = pixelUV.y;

				


			}

			uvWorldPosition.x = pixelUV.x - canvasCam.orthographicSize;//To center the UV on X
			uvWorldPosition.y = pixelUV.y - canvasCam.orthographicSize;//To center the UV on Y

			if(collapseCorners)
            {
				//Debug.Log("uvWP: " + uvWorldPosition + ", pUV: " + pixelUV);
				GameObject.FindGameObjectWithTag("PaintTool").GetComponent<PaintBoss>().cursorOverride = uvWorldPosition;
			}

			

			//uvWorldPosition.x = pixelUV.x - canvasCam.orthographicSize;//To center the UV on X
			//uvWorldPosition.y = pixelUV.y - canvasCam.orthographicSize;//To center the UV on Y
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
	public void SaveTexture()
	{
		
		brushCounter = 0;
		System.DateTime date = System.DateTime.Now;
		RenderTexture.active = canvasTexture;
		Texture2D tex = new Texture2D(canvasTexture.width, canvasTexture.height, TextureFormat.RGBA32, false);
		tex.ReadPixels(new Rect(0, 0, canvasTexture.width, canvasTexture.height), 0, 0);
		tex.Apply();
		RenderTexture.active = null;
		var oldTex = baseMaterial.mainTexture;
		baseMaterial.mainTexture = tex; //Put the painted texture as the base
		Destroy(oldTex); // Free up old memory, prevents memory leaks.

		//StartCoroutine ("SaveTextureToFile"); //Do you want to save the texture? This is your method!
		needsDelete = true;
		saving = false;

		brushContainer.SetActive(false); // We disable the brushes so the terrain doesn't flick in the delay it takes to clear it.

		// MatchEdges(); Needs work.
		
	}

	bool firstCall = true;


	Color32 ColorStep(Color32 target, Color32 dest, float stepAmount)
    {
		target.r = (byte)Mathf.Clamp(((target.r + dest.r) / 2f),target.r - stepAmount, target.r + stepAmount);
		target.g = (byte)Mathf.Clamp(((target.g + dest.g) / 2f),target.g - stepAmount, target.g + stepAmount);
		target.b = (byte)Mathf.Clamp(((target.b + dest.b) / 2f),target.b - stepAmount, target.b + stepAmount);
		target.a = (byte)Mathf.Clamp(((target.a + dest.a) / 2f),target.a - stepAmount, target.a + stepAmount);

		return target;
	}

	public void CodePaint(Vector2 texPos, int brushSize, Color32 brushColor, float brushSpeed)
	{
		if (firstCall)
		{
			firstCall = false;
			return;
		}

		

		

		float timer = Time.realtimeSinceStartup;

		Texture2D tex = baseMaterial.mainTexture as Texture2D;

		var pixelsMain = tex.GetPixels32();

		Texture2D texLeft = NeighborLeft.baseMaterial.mainTexture as Texture2D;
		Texture2D texUp = NeighborUp.baseMaterial.mainTexture as Texture2D;
		Texture2D texRight = NeighborRight.baseMaterial.mainTexture as Texture2D;
		Texture2D texDown = NeighborDown.baseMaterial.mainTexture as Texture2D;
		var pixelsLeft = texLeft.GetPixels32();
		var pixelsUp = texUp.GetPixels32();
		var pixelsRight = texRight.GetPixels32();
		var pixelsDown = texDown.GetPixels32();


		Color32 softColor = new Color32();
		Color32 colorNeighbor = new Color32();
		Color32 colorMain = new Color32();

		
		
		
		for (int x = 0; x < tex.width; x++)
        {
			for (int y = 0; y < tex.width; y++)
            {

				
				//pixelsMain[y + tex.width * ((tex.width - 1) - x)] = brushColor;
				if (Vector2.Distance(new Vector2(texPos.y*tex.width,texPos.x*tex.width),new Vector2(x,y)) < brushSize)
                {
					colorMain = pixelsMain[y + tex.width * x];
					pixelsMain[y + tex.width * x] = ColorStep(colorMain, brushColor, brushSpeed);

					//softColor = Color32.Lerp(colorMain, brushColor, brushSpeed);

					//softColor = brushColor;

					//softColor = ColorStep(colorMain, brushColor, brushSpeed);

					//softColor.r = (byte)((colorMain.r + brushColor.r) / 2f);
					//softColor.g = (byte)((colorMain.g + brushColor.g) / 2f);
					//softColor.b = (byte)((colorMain.b + brushColor.b) / 2f);
					//softColor.a = (byte)((colorMain.a + brushColor.a) / 2f);


					//pixelsMain[y + tex.width * x] = softColor;

				}



            }
		}


		tex.SetPixels32(pixelsMain);
		tex.Apply();

		if (mySide != PlanetSide.Side.down)
			return;


		//Neighbor Up
		for (int x = 0; x < 5; x++)
		{
			for (int y = 0; y < tex.width; y++)
			{
				colorMain = pixelsMain[y + tex.width * ((tex.width - 1) - x)];
				colorNeighbor = pixelsUp[y + tex.width * x];

				softColor.r = (byte)((colorMain.r + colorNeighbor.r) / 2f);
				softColor.g = (byte)((colorMain.g + colorNeighbor.g) / 2f);
				softColor.b = (byte)((colorMain.b + colorNeighbor.b) / 2f);
				softColor.a = (byte)((colorMain.a + colorNeighbor.a) / 2f);

				//pixelsMain[y + tex.width * ((tex.width - 1) - x)] = softColor;
				//pixelsUp[y + tex.width * x] = softColor;
				pixelsUp[y + tex.width * x] = pixelsMain[y + tex.width * ((tex.width - 1) - x)];
			}
		}


		//Neighbor Left
		for (int x = 0; x < tex.width; x++)
		{
			for (int y = 0; y < 10; y++)
			{
				colorMain = pixelsMain[y + tex.width * ((tex.width - 1) - x)];
				colorNeighbor = pixelsLeft[x + tex.width * ((tex.width - 1) - y)];

				softColor.r = (byte)((colorMain.r + colorNeighbor.r) / 2f);
				softColor.g = (byte)((colorMain.g + colorNeighbor.g) / 2f);
				softColor.b = (byte)((colorMain.b + colorNeighbor.b) / 2f);
				softColor.a = (byte)((colorMain.a + colorNeighbor.a) / 2f);

				//Debug.Log("Offset y by one?");

				//pixelsMain[y + tex.width * ((tex.width - 1) - x)] = softColor;
				//pixelsLeft[x + tex.width * ((tex.width - 1) - y)] = softColor;
				//if(!(Input.GetKey(KeyCode.G)))
				//	pixelsLeft[x + tex.width * ((tex.width - 1) - (int)(Mathf.Clamp(y, 0, 10)))] = pixelsMain[y + tex.width * ((tex.width - 1) - x)];
				//else
				//{
				//	pixelsLeft[x + tex.width * ((tex.width - 1) - (int)(Mathf.Clamp(y, 0, 10)))] = Color.white;
				//}
				pixelsLeft[x + tex.width * ((tex.width - 1) - y)] = pixelsMain[y + tex.width * ((tex.width - 1) - x)];
			}
		}


		//Neighbor Right
		for (int x = 0; x < tex.width; x++)
		{
			for (int y = 0; y < tex.width; y++)
			{
				colorMain = pixelsMain[y + tex.width * ((tex.width - 1) - x)];
				colorNeighbor = pixelsRight[x + tex.width * ((tex.width - 1) - y)];

				softColor.r = (byte)((colorMain.r + colorNeighbor.r) / 2f);
				softColor.g = (byte)((colorMain.g + colorNeighbor.g) / 2f);
				softColor.b = (byte)((colorMain.b + colorNeighbor.b) / 2f);
				softColor.a = (byte)((colorMain.a + colorNeighbor.a) / 2f);

				//Debug.Log("Offset y by one?");

				//pixelsMain[y + tex.width * x] = softColor;
				//pixelsRight[x + tex.width * ((tex.width - 1) - y)] = softColor;
				pixelsRight[x + tex.width * ((tex.width - 1) - y)] = pixelsMain[x + tex.width * y];
			}
		}


		//Neighbor Down
		for (int x = 0; x < 5; x++)
		{
			for (int y = 0; y < tex.width; y++)
			{
				colorMain = pixelsMain[y + tex.width * x];
				colorNeighbor = pixelsDown[y + tex.width * ((tex.width - 1) - x)];

				softColor.r = (byte)((colorMain.r + colorNeighbor.r) / 2f);
				softColor.g = (byte)((colorMain.g + colorNeighbor.g) / 2f);
				softColor.b = (byte)((colorMain.b + colorNeighbor.b) / 2f);
				softColor.a = (byte)((colorMain.a + colorNeighbor.a) / 2f);

				//pixelsMain[y + tex.width * x] = softColor;
				//pixelsDown[y + tex.width * ((tex.width - 1) - x)] = softColor;
				pixelsDown[y + tex.width * ((tex.width - 1) - x)] = pixelsMain[y + tex.width * x];
			}
		}








		tex.SetPixels32(pixelsMain);
		tex.Apply();
		texLeft.SetPixels32(pixelsLeft);
		texLeft.Apply();
		texUp.SetPixels32(pixelsUp);
		texUp.Apply();
		texRight.SetPixels32(pixelsRight);
		texRight.Apply();
		texDown.SetPixels32(pixelsDown);
		texDown.Apply();



		Debug.Log("Code Paint Time: " + (Time.realtimeSinceStartup - timer) * 1000f + "ms");
	}


	void MatchEdges()
    {
		if(firstCall)
		{ 
			firstCall = false;
			return;
		}

		if (mySide != PlanetSide.Side.down)
			return;

		float timer = Time.realtimeSinceStartup;

		Texture2D tex = baseMaterial.mainTexture as Texture2D;

		var pixelsMain = tex.GetPixels32();

		Texture2D texLeft = NeighborLeft.baseMaterial.mainTexture as Texture2D;
		Texture2D texUp = NeighborUp.baseMaterial.mainTexture as Texture2D;
		Texture2D texRight = NeighborRight.baseMaterial.mainTexture as Texture2D;
		Texture2D texDown = NeighborDown.baseMaterial.mainTexture as Texture2D;
		var pixelsLeft = texLeft.GetPixels32();
		var pixelsUp = texUp.GetPixels32();
		var pixelsRight = texRight.GetPixels32();
		var pixelsDown = texDown.GetPixels32();


		Color32 softColor = new Color32();
		Color32 colorNeighbor = new Color32();
		Color32 colorMain = new Color32();




		//Neighbor Up
		for (int x = 0; x < 5; x++)
		{
			for (int y = 0; y < tex.width; y++)
			{
				colorMain = pixelsMain[y + tex.width * ((tex.width - 1) - x)];
				colorNeighbor = pixelsUp[y + tex.width * x];

				softColor.r = (byte)((colorMain.r + colorNeighbor.r) / 2f);
				softColor.g = (byte)((colorMain.g + colorNeighbor.g) / 2f);
				softColor.b = (byte)((colorMain.b + colorNeighbor.b) / 2f);
				softColor.a = (byte)((colorMain.a + colorNeighbor.a) / 2f);

				//pixelsMain[y + tex.width * ((tex.width - 1) - x)] = softColor;
				//pixelsUp[y + tex.width * x] = softColor;
				pixelsUp[y + tex.width * x] = pixelsMain[y + tex.width * ((tex.width - 1) - x)];
			}
		}


		//Neighbor Left
		for (int x = 0; x < tex.width; x++)
		{
			for (int y = 0; y < 10; y++)
			{
				colorMain = pixelsMain[y + tex.width * ((tex.width - 1) - x)];
				colorNeighbor = pixelsLeft[x + tex.width * ((tex.width - 1) - y)];

				softColor.r = (byte)((colorMain.r + colorNeighbor.r) / 2f);
				softColor.g = (byte)((colorMain.g + colorNeighbor.g) / 2f);
				softColor.b = (byte)((colorMain.b + colorNeighbor.b) / 2f);
				softColor.a = (byte)((colorMain.a + colorNeighbor.a) / 2f);

				//Debug.Log("Offset y by one?");

				//pixelsMain[y + tex.width * ((tex.width - 1) - x)] = softColor;
				//pixelsLeft[x + tex.width * ((tex.width - 1) - y)] = softColor;
				//if(!(Input.GetKey(KeyCode.G)))
				//	pixelsLeft[x + tex.width * ((tex.width - 1) - (int)(Mathf.Clamp(y, 0, 10)))] = pixelsMain[y + tex.width * ((tex.width - 1) - x)];
				//else
				//{
				//	pixelsLeft[x + tex.width * ((tex.width - 1) - (int)(Mathf.Clamp(y, 0, 10)))] = Color.white;
				//}
				pixelsLeft[x + tex.width * ((tex.width - 1) - y)] = pixelsMain[y + tex.width * ((tex.width - 1) - x)];
			}
		}


		//Neighbor Right
		for (int x = 0; x < tex.width; x++)
		{
			for (int y = 0; y < tex.width; y++)
			{
				colorMain = pixelsMain[y + tex.width * ((tex.width - 1) - x)];
				colorNeighbor = pixelsRight[x + tex.width * ((tex.width - 1) - y)];

				softColor.r = (byte)((colorMain.r + colorNeighbor.r) / 2f);
				softColor.g = (byte)((colorMain.g + colorNeighbor.g) / 2f);
				softColor.b = (byte)((colorMain.b + colorNeighbor.b) / 2f);
				softColor.a = (byte)((colorMain.a + colorNeighbor.a) / 2f);

				//Debug.Log("Offset y by one?");

				//pixelsMain[y + tex.width * x] = softColor;
				//pixelsRight[x + tex.width * ((tex.width - 1) - y)] = softColor;
				pixelsRight[x + tex.width * ((tex.width - 1) - y)] = pixelsMain[x + tex.width * y];
			}
		}


		//Neighbor Down
		for (int x = 0; x < 5; x++)
		{
			for (int y = 0; y < tex.width; y++)
			{
				colorMain = pixelsMain[y + tex.width * x];
				colorNeighbor = pixelsDown[y + tex.width * ((tex.width - 1) - x)];

				softColor.r = (byte)((colorMain.r + colorNeighbor.r) / 2f);
				softColor.g = (byte)((colorMain.g + colorNeighbor.g) / 2f);
				softColor.b = (byte)((colorMain.b + colorNeighbor.b) / 2f);
				softColor.a = (byte)((colorMain.a + colorNeighbor.a) / 2f);

				//pixelsMain[y + tex.width * x] = softColor;
				//pixelsDown[y + tex.width * ((tex.width - 1) - x)] = softColor;
				pixelsDown[y + tex.width * ((tex.width - 1) - x)] = pixelsMain[y + tex.width * x];
			}
		}




		tex.SetPixels32(pixelsMain);
		tex.Apply();
		texLeft.SetPixels32(pixelsLeft);
		texLeft.Apply();
		texUp.SetPixels32(pixelsUp);
		texUp.Apply();
		texRight.SetPixels32(pixelsRight);
		texRight.Apply();
		texDown.SetPixels32(pixelsDown);
		texDown.Apply();
		

		Debug.Log(mySide +", Seam Fix Time: " + (Time.realtimeSinceStartup - timer) * 1000f + "ms");
	}


	public IEnumerator EmptyBrushContainer()
    {
		
		yield return 0.1f;
		if (needsDelete == false)
        {
			yield return new WaitForSeconds(0.1f);
		}

		foreach (Transform child in brushContainer.transform)
		{//Clear brushes
			Destroy(child.gameObject);
		}
		brushContainer.SetActive(true);
		needsDelete = false;

		var PWT = GetComponentInChildren<PaintWorkTag>();
		if(PWT)
        {
			if (mySide == PlanetSide.Side.center)
				PWT.transform.localPosition = transform.position + (Vector3.forward * 10);
			if (mySide == PlanetSide.Side.left)
				PWT.transform.localPosition = transform.position + (Vector3.left * 10);
			if (mySide == PlanetSide.Side.up)
				PWT.transform.localPosition = transform.position+(Vector3.up * 10);
			if (mySide == PlanetSide.Side.right)
				PWT.transform.localPosition = transform.position + (Vector3.right * 10);
			if (mySide == PlanetSide.Side.down)
				PWT.transform.localPosition = transform.position + (Vector3.down * 10);
			if (mySide == PlanetSide.Side.ddown)
				PWT.transform.localPosition = transform.position + (Vector3.back * 10);


		}

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
