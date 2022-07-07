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

    // Update is called once per frame
    void Update()
    {
        PaintTag targetHit = null;
        if (Input.GetMouseButton(0))
            targetHit = CheckForPaintTargets();

        if(targetHit)
        {
            Debug.Log("We hit a paint target!" + hit.collider.name);
            Debug.Log(targetHit.owner.name);
            if (Input.GetKey(KeyCode.LeftShift))
                targetHit.owner.DoAction(false);
            targetHit.owner.DoAction(true);
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
