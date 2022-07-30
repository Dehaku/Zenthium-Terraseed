using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class ShipMovementRB : MonoBehaviour
{
    [Header("=== Ship Movement Settings ===")]
    [SerializeField] float yawTorque = 500f;
    [SerializeField] bool invertPitch = false;
    [SerializeField] float pitchTorque = 1000f;
    [SerializeField] float rollTorque = 1000f;
    [SerializeField] float thrust = 100f;
    [SerializeField] float upThrust = 50f;
    [SerializeField] float strafeThrust = 50f;
    [SerializeField] float thrustDeadZone = 0.1f;
    [SerializeField] bool inertialDampener = false;
    [SerializeField] float inertialDampenerAmount = 0.5f;
    [SerializeField] bool velocityMatch = false;
    [SerializeField] float velocityMatchClamp = 1f;


    [Header("=== Boost Settings ===")]
    [SerializeField] float maxBoostAmount = 2f; // Storage
    [SerializeField] float boostDepreciationRate = 0.25f; // Drain Rate
    [SerializeField] float boostRechargeRate = 0.5f; // Recharge Rate
    [SerializeField] float boostMultiplier = 5f; // Extra Speed
    [SerializeField] float currentBoostAmount = 0f;
    public bool boosting = false;
    public bool gainBoostWhileBoosting = false;



    [SerializeField, Range(0.001f, 0.999f)] float thrustGlideReduction = 0.999f;
    [SerializeField, Range(0.001f, 0.999f)] float upDownGlideReduction = 0.111f;
    [SerializeField, Range(0.001f, 0.999f)] float leftRightGlideReduction = 0.111f;
    float glide, verticalGlide, horizontalGlide = 0f;

    Rigidbody rb;

    float thrust1D;
    float upDown1D;
    float strafe1D;
    float roll1D;
    Vector2 pitchYaw;

    [Header("References")]
    public GameObject camGO;
    Camera cam;
    Cinemachine3rdPersonFollow vBody;
    public GameObject velocityTarget;

    public float GetBoostMax()
    {
        return maxBoostAmount;
    }

    public float GetBoost()
    {
        return currentBoostAmount;
    }

    public float GetBoostPercentFull()
    {
        return currentBoostAmount / maxBoostAmount;
    }

    Cinemachine3rdPersonFollow GetCam()
    {
        if (!vBody)
        {
            if (!camGO)
                Debug.LogError("No camGO");
            var vCam = camGO.GetComponent<CinemachineVirtualCamera>();
            if (!vCam)
                Debug.LogError("No vCam");
            vBody = vCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            if(!vBody)
                Debug.LogError("No vBody");

        }



        return vBody;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        MatchVelocityWithVelocityTarget();
        HandleBoosting();
        HandleMovement();
        HandleCamera();
    }


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.M))
        {
            // Wipe old target
            velocityTarget = null;
            velocityMatch = false;

            // Cast our ray from camera center.
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 100.0f))
            {
                velocityTarget = hit.collider.gameObject;
                velocityMatch = true;
            }
        }
    }

    void MatchVelocityWithVelocityTarget()
    {
        if (!velocityMatch)
            return;
        // Make sure we have valid target, and they have a rigid body.
        if (!velocityTarget)
            return;
        var tarRB = velocityTarget.GetComponent<Rigidbody>();
        if (!tarRB)
            return;

        float xVelDiff = Mathf.Clamp(tarRB.velocity.x - rb.velocity.x,-velocityMatchClamp, velocityMatchClamp);
        float yVelDiff = Mathf.Clamp(tarRB.velocity.y - rb.velocity.y,-velocityMatchClamp, velocityMatchClamp);
        float zVelDiff = Mathf.Clamp(tarRB.velocity.z - rb.velocity.z,-velocityMatchClamp, velocityMatchClamp);

        rb.AddForce(new Vector3(xVelDiff, yVelDiff, zVelDiff), ForceMode.VelocityChange);
    }


    void HandleCamera()
    {
        return;
        //if (!GetCam())
        //    return;
        //if(boosting)
        //{
        //    GetCam().Damping.z = 0.4f;
        //}
        //else
        //{
        //    GetCam().Damping.z = 0.1f;
        //}
    }

    void HandleBoosting()
    {
        if(boosting && currentBoostAmount > 0f)
        {
            currentBoostAmount -= boostDepreciationRate;
            if(currentBoostAmount <= 0f)
            {
                boosting = false;
            }
            if(gainBoostWhileBoosting)
            {
                if (currentBoostAmount < maxBoostAmount)
                {
                    currentBoostAmount += boostRechargeRate;
                }
            }
        }
        else
        {
            if(currentBoostAmount < maxBoostAmount)
            {
                currentBoostAmount += boostRechargeRate;
            }
        }
        if (currentBoostAmount > maxBoostAmount)
            currentBoostAmount = maxBoostAmount;
    }

    void HandleMovement()
    {
        // Roll
        rb.AddRelativeTorque(Vector3.back * roll1D * rollTorque * Time.deltaTime);
        // Pitch
        if(invertPitch)
            rb.AddRelativeTorque(Vector3.right * Mathf.Clamp(-pitchYaw.y, -1f, 1f) * pitchTorque * Time.deltaTime);
        else
            rb.AddRelativeTorque(Vector3.right * Mathf.Clamp(pitchYaw.y, -1f, 1f) * pitchTorque * Time.deltaTime);

        // Yaw 
        rb.AddRelativeTorque(Vector3.up * Mathf.Clamp(pitchYaw.x, -1f, 1f) * yawTorque * Time.deltaTime);
        
        // Thrust
        if(thrust1D > thrustDeadZone || thrust1D < -thrustDeadZone)
        {
            float currentThrust;

            if(boosting)
            {
                currentThrust = thrust * boostMultiplier;
            }
            else
            {
                currentThrust = thrust;
            }

            rb.AddRelativeForce(Vector3.forward * thrust1D * currentThrust * Time.deltaTime);
            glide = thrust;
        }
        else
        {
            rb.AddRelativeForce(Vector3.forward * glide * Time.deltaTime);
            glide *= thrustGlideReduction;
        }

        // UP/DOWN
        if (upDown1D > thrustDeadZone || upDown1D < -thrustDeadZone)
        {
            rb.AddRelativeForce(Vector3.up * upDown1D * upThrust * Time.fixedDeltaTime);
            verticalGlide = upDown1D * upThrust;
        }
        else
        {
            rb.AddRelativeForce(Vector3.up * verticalGlide * Time.fixedDeltaTime);
            verticalGlide *= upDownGlideReduction;
        }

        // STRAFING
        if (strafe1D > thrustDeadZone || strafe1D < -thrustDeadZone)
        {
            rb.AddRelativeForce(Vector3.right * strafe1D * upThrust * Time.fixedDeltaTime);
            horizontalGlide = strafe1D * strafeThrust;
        }
        else
        {
            rb.AddRelativeForce(Vector3.right * horizontalGlide * Time.fixedDeltaTime);
            horizontalGlide *= leftRightGlideReduction;
        }


    }

    void ToggleInertialDampener()
    {
        inertialDampener = !inertialDampener;
        if(inertialDampener)
        {
            rb.drag = inertialDampenerAmount;
        }
        else
        {
            rb.drag = 0f;
        }
    }

    #region Input Methods

    public void OnThrust(InputAction.CallbackContext context)
    {
        thrust1D = context.ReadValue<float>();
    }

    public void OnStrafe(InputAction.CallbackContext context)
    {
        strafe1D = context.ReadValue<float>();
    }

    public void OnUpDown(InputAction.CallbackContext context)
    {
        upDown1D = context.ReadValue<float>();
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        roll1D = context.ReadValue<float>();
    }

    public void OnPitchYaw(InputAction.CallbackContext context)
    {
        pitchYaw = context.ReadValue<Vector2>();
    }

    public void OnBoost(InputAction.CallbackContext context)
    {
        boosting = context.performed;
    }
    public void OnInertialDampener(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            ToggleInertialDampener();
        }
    }

    

    #endregion

}
