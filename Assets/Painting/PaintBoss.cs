using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintBoss : MonoBehaviour
{
    public Camera sceneCamera;

    // Start is called before the first frame update
    void Start()
    {
    }

    RaycastHit hit;

    bool _triggerSave = false;
    bool _isPainting = false;
    PaintTag targetHit = null;

    // Update is called once per frame
    void Update()
    {
        bool brushFull = false;
        if (Input.GetMouseButton(0))
        {
            PaintTag newTarget = CheckForPaintTargets();
            if(targetHit != null && targetHit != newTarget)
            {
                //Debug.Log("Not drawing on same target now!");
                SaveLastTarget();
            }
            
            targetHit = newTarget;
            _isPainting = true;
            brushFull = PaintTarget();
        }
        else if(_isPainting)
        {
            
            _isPainting = false;
            _triggerSave = true;
        }
            
        //Debug.Log("Make it save when you let go of the mouse button.");
        

        

        if (brushFull || Input.GetMouseButtonDown(1) || _triggerSave)
        {
            
            SaveLastTarget();
        }

    }

    bool PaintTarget()
    {
        bool brushFull = false;
        if (targetHit)
        {
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
        }
    }

    PaintTag CheckForPaintTargets()
    {
        //RaycastHit hit;
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
}
