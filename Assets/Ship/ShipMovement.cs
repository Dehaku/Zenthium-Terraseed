using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipMovement : MonoBehaviour
{
    public Rigidbody rb;
    public float shipSpeed;
    public float shipTorque;

    public bool dampenRotation = true;
    public float dampenRotationAmount = 1f;
    Transform _transform;
    // Start is called before the first frame update
    void Start()
    {
        _transform = transform;
    }

    void HandleInputs()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            rb.AddForce(_transform.forward * shipSpeed);
        }

        if (Input.GetKey(KeyCode.D))
        {
            rb.AddTorque(_transform.up * shipTorque * 1f);
        }

        if (Input.GetKey(KeyCode.A))
        {
            rb.AddTorque(_transform.up * shipTorque * -1f);
        }

        if (Input.GetKey(KeyCode.W))
        {
            rb.AddTorque(_transform.right * shipTorque * 1f);
        }

        if (Input.GetKey(KeyCode.S))
        {
            rb.AddTorque(_transform.right * shipTorque * -1f);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            rb.AddTorque(_transform.forward * shipTorque * 1f);
        }

        if (Input.GetKey(KeyCode.E))
        {
            rb.AddTorque(_transform.forward * shipTorque * -1f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        DampRotation();

        HandleInputs();
        

    }

    void DampRotation()
    {
        if (dampenRotation)
            rb.angularDrag = dampenRotationAmount;
        else
            rb.angularDrag = 0f;
    }
}
