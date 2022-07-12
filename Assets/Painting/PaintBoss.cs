using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintBoss : MonoBehaviour
{
    public float brushSize = 1;
    public int brushSizeCode = 10;
    public Color32 brushColorCode;
    public float brushSpeed = 0.05f;

    public GameObject brushSpritePF;
    public GameObject cursor;

    public bool collapseCorners; // For Corner Collapsing the brushes position in PaintFaces(To avoid seam snailshelling)
    [Range(0f,1f)]
    public float collapseCornersRange;

    
    
    
    public Camera sceneCamera;

    // Start is called before the first frame update
    void Start()
    {

        Debug.Log("Lock the brush to synced X/Y when it's within the brushes range of corners.");
    }

    bool _triggerSave = false;
    bool _isPainting = false;
    PaintTag targetHit = null;

    public PaintFaces planetPainterBase;
    public GameObject planetPainterPF;

    [HideInInspector]
    public Vector3 cursorOverride; // For Corner Collapsing the brushes position in PaintFaces(To avoid seam snailshelling)

    public bool allowDisableRenderers = true;

    // Update is called once per frame
    void Update()
    {
        cursorOverride = Vector3.zero;

        bool paintable = PlanetCheckLoop();

        
        PaintLoop(paintable);

        UpdateCursor();
    }

    Vector3 _lastHitPos;
    void UpdateCursor()
    {
        if (!cursor)
            return;

        RaycastHit hit;
        Vector3 cursorPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.0f);
        Ray cursorRay = sceneCamera.ScreenPointToRay(cursorPos);
        if (Physics.Raycast(cursorRay, out hit, 200))
        {
            cursor.SetActive(true);
            cursor.transform.position = hit.point;
            if (cursorOverride != Vector3.zero)
                cursor.transform.position = cursorOverride;
            cursor.transform.eulerAngles = hit.normal;
            cursor.transform.localScale = Vector3.one * brushSize;
        }
        else
        {
            cursor.SetActive(false);
        }
    }

    bool PlanetCheckLoop()
    {
        //if (Input.GetMouseButton(0))
        {
            PlanetSide newTarget = CheckForPlanetSide();
            if (newTarget != null)
            {
                var paintTag = newTarget.GetComponent<PaintTag>();
                if (paintTag)
                {
                    //if (paintTag.timeAlive > 1f)
                    return true;
                }
                else
                {
                    MakePlanetPaintable(newTarget.planetParent);
                    return false;
                }
            }
        }
        
        return false;
    }    

    IEnumerator SetPlanetsideTextureToRenderTexture(PlanetSide ps, PaintFaces painter)
    {
        yield return new WaitForSeconds(1f);
        var meshRend = ps.GetComponent<MeshRenderer>();
        meshRend.material = new Material(meshRend.material);
        meshRend.material.SetTexture("_HeightMap", painter.canvasTexture);
    }
    void MakePlanetPaintable(GameObject planetObject)
    {
        PaintFaces painterCenter = null;
        PaintFaces painterLeft = null;
        PaintFaces painterUp = null;
        PaintFaces painterRight = null;
        PaintFaces painterDown = null;
        PaintFaces painterDDown = null;

        var planetSides = planetObject.GetComponentsInChildren<PlanetSide>();
        foreach (var ps in planetSides)
        {
            var paintTag = ps.gameObject.AddComponent<PaintTag>();
            var painterGO = Instantiate(planetPainterPF, planetObject.transform);
            
            var painter = painterGO.GetComponent<PaintFaces>();
            painter.basePaintFaces = planetPainterBase;
            paintTag.owner = painter;

            
            StartCoroutine(SetPlanetsideTextureToRenderTexture(ps, painter)); // Delay the texture so it has time to setup.

            // painter.paintTarget = ps.GetComponent<MeshRenderer>();

            if (ps.side == PlanetSide.Side.center)
            {
                painterCenter = painter;
                painter.mySide = ps.side;
            }
                
            if (ps.side == PlanetSide.Side.left)
            {
                painterLeft = painter;
                painter.mySide = ps.side;
            }
                
            if (ps.side == PlanetSide.Side.up)
            {
                painterUp = painter;
                painter.mySide = ps.side;
            }
                
            if (ps.side == PlanetSide.Side.right)
            {
                painterRight = painter;
                painter.mySide = ps.side;
            }
                
            if (ps.side == PlanetSide.Side.down)
            {
                painterDown = painter;
                painter.mySide = ps.side;
            }
                
            if (ps.side == PlanetSide.Side.ddown)
            {
                painterDDown = painter;
                painter.mySide = ps.side;
            }
                
            //painter.NeighborDown
        }

        // Center
        painterCenter.NeighborLeft = painterRight;
        painterCenter.NeighborLeftRotate = new Vector3(0, 0, 0);
        painterCenter.NeighborUp = painterDown;
        painterCenter.NeighborUpRotate = new Vector3(0, 0, 0);
        painterCenter.NeighborRight = painterLeft;
        painterCenter.NeighborRightRotate = new Vector3(0, 0, 0);
        painterCenter.NeighborDown = painterUp;
        painterCenter.NeighborDownRotate = new Vector3(0, 0, 0);

        // Left
        painterLeft.NeighborLeft = painterCenter;
        painterLeft.NeighborLeftRotate = new Vector3(0, 0, 0);
        painterLeft.NeighborUp = painterDown;
        painterLeft.NeighborUpRotate = new Vector3(0, 0, -90);
        painterLeft.NeighborRight = painterDDown;
        painterLeft.NeighborRightRotate = new Vector3(0, 0, -180);
        painterLeft.NeighborDown = painterUp;
        painterLeft.NeighborDownRotate = new Vector3(0, 0, 90);

        // Up
        painterUp.NeighborLeft = painterRight;
        painterUp.NeighborLeftRotate = new Vector3(0, 0, 90);
        painterUp.NeighborUp = painterCenter;
        painterUp.NeighborUpRotate = new Vector3(0, 0, 0);
        painterUp.NeighborRight = painterLeft;
        painterUp.NeighborRightRotate = new Vector3(0, 0, -90);
        painterUp.NeighborDown = painterDDown;
        painterUp.NeighborDownRotate = new Vector3(0, 0, 0);

        // Right
        painterRight.NeighborLeft = painterDDown;
        painterRight.NeighborLeftRotate = new Vector3(0, 0, -180);
        painterRight.NeighborUp = painterDown;
        painterRight.NeighborUpRotate = new Vector3(0, 0, 90);
        painterRight.NeighborRight = painterCenter;
        painterRight.NeighborRightRotate = new Vector3(0, 0, 0);
        painterRight.NeighborDown = painterUp;
        painterRight.NeighborDownRotate = new Vector3(0, 0, -90);

        // Down
        painterDown.NeighborLeft = painterRight;
        painterDown.NeighborLeftRotate = new Vector3(0, 0, -90);
        painterDown.NeighborUp = painterDDown;
        painterDown.NeighborUpRotate = new Vector3(0, 0, 0);
        painterDown.NeighborRight = painterLeft;
        painterDown.NeighborRightRotate = new Vector3(0, 0, 90);
        painterDown.NeighborDown = painterCenter;
        painterDown.NeighborDownRotate = new Vector3(0, 0, 0);

        // DDown
        painterDDown.NeighborLeft = painterRight;
        painterDDown.NeighborLeftRotate = new Vector3(0, 0, -180);
        painterDDown.NeighborUp = painterUp;
        painterDDown.NeighborUpRotate = new Vector3(0, 0, 0);
        painterDDown.NeighborRight = painterLeft;
        painterDDown.NeighborRightRotate = new Vector3(0, 0, -180);
        painterDDown.NeighborDown = painterDown;
        painterDDown.NeighborDownRotate = new Vector3(0, 0, 0);




    }

    

    void PaintLoop(bool safe = false)
    {
        bool brushFull = false;
        if (Input.GetMouseButton(0) && safe)
        {
            PaintTag newTarget = CheckForPaintTargets();
            if (targetHit != null && targetHit != newTarget)
            {
                //Debug.Log("Not drawing on same target now!");
                SaveLastTarget();
            }

            targetHit = newTarget;
            _isPainting = true;
            brushFull = PaintTarget();
        }
        else if (_isPainting)
        {
            _isPainting = false;
            _triggerSave = true;
        }

        if (Input.GetMouseButton(2) && safe)
        {
            //CodePaintTarget(); Needs work.
            

        }

        if (brushFull || Input.GetMouseButtonDown(1) || _triggerSave)
        {
            SaveLastTarget();
        }
    }

    PaintTag CodePaintTarget()
    {
        RaycastHit hit;
        Vector3 cursorPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.0f);
        Ray cursorRay = sceneCamera.ScreenPointToRay(cursorPos);
        if (Physics.Raycast(cursorRay, out hit, 200))
        {
            var paintTarget = hit.collider.GetComponent<PaintTag>();
            if (paintTarget)
            {
                MeshCollider meshCollider = hit.collider as MeshCollider;
                if (meshCollider == null || meshCollider.sharedMesh == null)
                    return null;


                Color32 brushColor = Color.white;
                if(Input.GetKey(KeyCode.LeftShift))
                    brushColor = Color.black;
                paintTarget.owner.CodePaint(hit.textureCoord, brushSizeCode, brushColorCode, brushSpeed);

                TriggerTerrainMorph(paintTarget.GetComponentInParent<Spherize>());

                return paintTarget;
            }

            return null;
        }
        return null;
    }


    void TogglePaintWorks(PaintTag target, bool on)
    {
        if (!target) // Nullcheck.
        {
            Debug.Log(on + "Target was Null in TogglePaintWorks somehow.");
            return;
        }
        if (!allowDisableRenderers && on == false)
            return;

        target.owner.gameObject.SetActive(on);
        target.owner.NeighborLeft.gameObject.SetActive(on);
        target.owner.NeighborUp.gameObject.SetActive(on);
        target.owner.NeighborRight.gameObject.SetActive(on);
        target.owner.NeighborDown.gameObject.SetActive(on);
    }

    void TriggerTerrainMorph(Spherize target)
    {
        target.shapeFaces = true;
    }

    bool PaintTarget()
    {
        bool brushFull = false;
        if (targetHit)
        {
            targetHit.owner.brushSize = brushSize;
            targetHit.owner.brushSpritePF = brushSpritePF;
            targetHit.owner.collapseCorners = collapseCorners;
            targetHit.owner.collapseCornerRange = collapseCornersRange;

            TogglePaintWorks(targetHit,true);
            TriggerTerrainMorph(targetHit.GetComponentInParent<Spherize>());

            var paintWork = targetHit.owner.GetComponentInChildren<PaintWorkTag>();
            if (paintWork)
            {
                Vector3 offset = Vector3.one * 100;
                Vector3 centerPaintPoint = targetHit.transform.position;

                if (targetHit.owner.mySide == PlanetSide.Side.center)
                    centerPaintPoint.x += 100;
                if (targetHit.owner.mySide == PlanetSide.Side.left)
                    centerPaintPoint.x += 200;
                if (targetHit.owner.mySide == PlanetSide.Side.up)
                    centerPaintPoint.x += 300;
                if (targetHit.owner.mySide == PlanetSide.Side.right)
                    centerPaintPoint.x += 400;
                if (targetHit.owner.mySide == PlanetSide.Side.down)
                    centerPaintPoint.x += 500;
                if (targetHit.owner.mySide == PlanetSide.Side.ddown)
                    centerPaintPoint.x += 600;



                paintWork.transform.position = centerPaintPoint + offset;
                paintWork.transform.eulerAngles = Vector3.zero;

                

                if(targetHit.owner.NeighborLeft)
                {
                    var paintWorkNeighbor = targetHit.owner.NeighborLeft.GetComponentInChildren<PaintWorkTag>();
                    paintWorkNeighbor.transform.position = centerPaintPoint + (Vector3.left*1f)+ offset;
                    paintWorkNeighbor.transform.eulerAngles = targetHit.owner.NeighborLeftRotate;
                    // Mr. Center's Left: 0
                    // Mr. Left's   Left: 0
                    // Mr. Up's     Left: 90
                    // Mr. Right's  Left: -180
                    // Mr. Down's   Left: -90
                    // Mr. DDown's  Left: -180

                }
                if (targetHit.owner.NeighborUp)
                {
                    var paintWorkNeighbor = targetHit.owner.NeighborUp.GetComponentInChildren<PaintWorkTag>();
                    paintWorkNeighbor.transform.position = centerPaintPoint + (Vector3.up * 1f) + offset;
                    paintWorkNeighbor.transform.eulerAngles = targetHit.owner.NeighborUpRotate;
                    // Mr. Center's Up: 0
                    // Mr. Left's   Up: -90
                    // Mr. Up's     Up: 0
                    // Mr. Right's  Up: 90
                    // Mr. Down's   Up: 0
                    // Mr. DDown's  Up: 0

                }
                if (targetHit.owner.NeighborRight)
                {
                    var paintWorkNeighbor = targetHit.owner.NeighborRight.GetComponentInChildren<PaintWorkTag>();
                    paintWorkNeighbor.transform.position = centerPaintPoint + (Vector3.right * 1f) + offset;
                    paintWorkNeighbor.transform.eulerAngles = targetHit.owner.NeighborRightRotate;
                    // Mr. Center's Right: 0
                    // Mr. Left's   Right: -180
                    // Mr. Up's     Right: -90
                    // Mr. Right's  Right: 0
                    // Mr. Down's   Right: 90
                    // Mr. DDown's  Right: -180
                }
                if (targetHit.owner.NeighborDown)
                {
                    var paintWorkNeighbor = targetHit.owner.NeighborDown.GetComponentInChildren<PaintWorkTag>();
                    paintWorkNeighbor.transform.position = centerPaintPoint + (Vector3.down * 1f) + offset;
                    paintWorkNeighbor.transform.eulerAngles = targetHit.owner.NeighborDownRotate;
                    // Mr. Center's Down: 0
                    // Mr. Left's   Down: 90
                    // Mr. Up's     Down: 0
                    // Mr. Right's  Down: -90
                    // Mr. Down's   Down: 0
                    // Mr. DDown's  Down: 0
                }
            }

            // Debug.Log("We hit a paint target!" + hit.collider.name);
            // Debug.Log(targetHit.owner.name);

            

            if (Input.GetKey(KeyCode.LeftShift))
                brushFull = targetHit.owner.DoAction(false);
            else
                brushFull = targetHit.owner.DoAction(true);
        }
        return brushFull;
    }

    void SaveLastTarget()
    {
        if (targetHit)
        {
            targetHit.owner.TriggerSaveMethod();


            if (targetHit.owner.NeighborLeft)
                targetHit.owner.NeighborLeft.TriggerSaveMethod();
            if (targetHit.owner.NeighborUp)
                targetHit.owner.NeighborUp.TriggerSaveMethod();
            if (targetHit.owner.NeighborRight)
                targetHit.owner.NeighborRight.TriggerSaveMethod();
            if (targetHit.owner.NeighborDown)
                targetHit.owner.NeighborDown.TriggerSaveMethod();

            StartCoroutine(targetHit.owner.EmptyBrushContainer());

            if (targetHit.owner.NeighborLeft)
                StartCoroutine(targetHit.owner.NeighborLeft.EmptyBrushContainer());
            if (targetHit.owner.NeighborUp)
                StartCoroutine(targetHit.owner.NeighborUp.EmptyBrushContainer());
            if (targetHit.owner.NeighborRight)
                StartCoroutine(targetHit.owner.NeighborRight.EmptyBrushContainer());
            if (targetHit.owner.NeighborDown)
                StartCoroutine(targetHit.owner.NeighborDown.EmptyBrushContainer());

            _triggerSave = false;

            TogglePaintWorks(targetHit, false);
        }
    }

    PaintTag CheckForPaintTargets()
    {
        RaycastHit hit;
        Vector3 cursorPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.0f);
        Ray cursorRay = sceneCamera.ScreenPointToRay(cursorPos);
        if (Physics.Raycast(cursorRay, out hit, 200))
        {
            var paintTarget = hit.collider.GetComponent<PaintTag>();
            if (paintTarget)
            {
                MeshCollider meshCollider = hit.collider as MeshCollider;
                if (meshCollider == null || meshCollider.sharedMesh == null)
                    return null;
                
                return paintTarget;
            }

            return null;    
        }
        return null;
    }

    PlanetSide CheckForPlanetSide()
    {

        RaycastHit hit;
        Vector3 cursorPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.0f);
        Ray cursorRay = sceneCamera.ScreenPointToRay(cursorPos);
        if (Physics.Raycast(cursorRay, out hit, 200))
        {
            var planetTarget = hit.collider.GetComponent<PlanetSide>();
            if (planetTarget)
            {
                MeshCollider meshCollider = hit.collider as MeshCollider;
                if (meshCollider == null || meshCollider.sharedMesh == null)
                    return null;

                return planetTarget;
            }

            return null;
        }
        return null;
    }
}
