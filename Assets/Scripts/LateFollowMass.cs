using UnityEngine;

// LateFollow.cs
// A simple concave collider solution for raycasts
// Compatible with offset position and rotation
//
// To use, place MeshCollider object outside physics object,
// attach this script to the MeshCollider object, and
// assign the physics object as FollowTarget.
// Be sure to disable collision between the objects via
// physics layers!
//
// For questions: /u/ActionScripter9109

public class LateFollowMass : MonoBehaviour
{
    public bool SetTransformToTarget = true;
    
    public Transform FollowTarget;

    Vector3 _localPosShift;
    Quaternion _localRotShift;

    void Awake()
    {
        FollowTarget = transform.parent?.GetComponentInChildren<Mass>()?.transform;

        if(SetTransformToTarget)
        {
            transform.rotation = FollowTarget.rotation * _localRotShift;
            transform.position = FollowTarget.TransformPoint(_localPosShift);
        }

        if (FollowTarget == null)
        {
            Debug.LogError("Failed to find sibling Mass to follow " + gameObject.name);
            enabled = false;
            return;
        }
        _localPosShift = FollowTarget.InverseTransformPoint(transform.position);
        _localRotShift = Quaternion.Inverse(FollowTarget.rotation) * transform.rotation;
        
    }

    void LateUpdate()
    {
        transform.rotation = FollowTarget.rotation * _localRotShift;
        transform.position = FollowTarget.TransformPoint(_localPosShift);
    } 
    
}