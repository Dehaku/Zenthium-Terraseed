using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockMouse : MonoBehaviour
{
    public bool isMouseConfined;
    public bool isMouseHidden;
    public bool RMBFreesMouse;

    // Start is called before the first frame update
    void Start()
    {
        LockTheMouse();
    }

    public void HideTheMouse()
    {
        isMouseHidden = true;
        Cursor.visible = false;
    }

    public void ShowTheMouse()
    {
        isMouseHidden = false;
        Cursor.visible = true;
    }

    public void UnlockTheMouse()
    {
        isMouseConfined = false;
        Cursor.lockState = CursorLockMode.None;
    }

    public void LockTheMouse()
    {
        isMouseConfined = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void ToggleMouse()
    {
        if(isMouseHidden)
        {
            UnlockTheMouse();
            ShowTheMouse();
        }
        else
        {
            LockTheMouse();
            HideTheMouse();
        }
    }

    

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.Tab))
        {
            ShowTheMouse();
            UnlockTheMouse();
        }

        if(Input.GetKeyDown(KeyCode.LeftAlt))
        {
            ToggleMouse();
        }

        FreeMouse();

    }

    bool _previousHidden = false;
    bool _holdingRMB = false;
    void FreeMouse()
    {
        if (!RMBFreesMouse)
            return;

        if (Input.GetMouseButton(1))
        {
            if (!_holdingRMB)
            {
                _holdingRMB = true;
                _previousHidden = isMouseHidden;
            }

            ShowTheMouse();
        }
        else
        {
            if (_holdingRMB)
            {
                _holdingRMB = false;
                if (_previousHidden == true)
                    HideTheMouse();
            }
        }
    }
}
