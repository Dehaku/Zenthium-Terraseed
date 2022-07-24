using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class AsteroidHighlighter : MonoBehaviour
{
    [Header("Object script to pull from")]
    // ** Section Start: Objects that could be tracked **
    // This section can be replaced with any other game objects, enemies, items, etc.

    public AsteroidLogic asteroidManager;

    void FillTrackableObjects()
    {
        var rbs = asteroidManager.GetObjectRBs();

        _objectsToTrack.Clear();

        GameObject itemGO = null;
        foreach (var item in rbs)
        {
            itemGO = item.gameObject;
            if (itemGO.activeInHierarchy) // We only care about objects that are currently enabled.
            {
                _objectsToTrack.Add(itemGO.transform);
            }
        }
    }

    // ** Section End: Objects that could be tracked **


    
    List<Transform> _objectsToTrack = new List<Transform>(); // Objects to pull from.
    List<Transform> _trackedObjects = new List<Transform>(); // Objects actually being tracked/rendered on screen.

    [Header("Tracker Stats")]
    public Transform playerTransform;
    public int maxTrackedAtOnce = 10;
    public Sprite spriteOnScreen;
    public Sprite spriteEdgeScreen;
    public GameObject markerPF;
    public bool displayTrackers = true;

    public float trackedObjectsRefreshInterval = 1.3f;
    float _trackFillTimer = 0;

    List<MarkerController> markers = new List<MarkerController>();

    void FillTrackedObjectsFromTrackables()
    {
        _trackedObjects.Clear();
        foreach (var item in _objectsToTrack)
        {
            if (item.gameObject.activeInHierarchy)
                _trackedObjects.Add(item);
        }
    }


    
    void FillTrackedTimer() // We refresh the list every so often to account for dead/disabled objects.
    {
        _trackFillTimer += Time.deltaTime;
        if (_trackFillTimer > trackedObjectsRefreshInterval)
        {
            _trackFillTimer = 0;
            FillTrackableObjects();
            FillTrackedObjectsFromTrackables();
        }
    }

    void SortObjectsByDistance()
    {
        // Using Linq's Orderby function to sort stuff.
        if(_trackedObjects != null)
        {
            _trackedObjects = _trackedObjects.OrderBy(trans => Vector3.Distance(trans.position, playerTransform.position)).ToList();
        }
            
    }

    List<Transform> GetNearestObjects(int amountToGrab)
    {
        if (_trackedObjects == null)
            return null;

        List<Transform> nearestTransforms = new List<Transform>();
        for(int i = 0; i < Mathf.Clamp(amountToGrab,0, _trackedObjects.Count-1); i++)
        {
            nearestTransforms.Add(_trackedObjects[i]);
        }
        return nearestTransforms;
    }

    


    void DisplayTrackedObjects(List<Transform> objects)
    {
        if (markers.Count < maxTrackedAtOnce)
        {
            Debug.Log("Not enough yet");
            return;
        }
            

        //foreach (var item in objects)
        for(int i = 0; i < objects.Count; i++)
        {
            float distance = Vector3.Distance(objects[i].position, playerTransform.position);
            //Debug.Log(objects[i].name + " Dist: " + distance);

            Vector3 screenpos = Camera.main.WorldToScreenPoint(objects[i].position);

            if(screenpos.z>0 &&
                screenpos.x > 0 && screenpos.x<Screen.width &&
                screenpos.y> 0 && screenpos.y < Screen.height)
            {
                markers[i].isOnScreen = true;
                markers[i].transform.position = screenpos;
                markers[i].transform.localRotation = Quaternion.identity;
            }
            else // Everything below here was taken from digijin's vid https://www.youtube.com/watch?v=gAQpR1GN0Os
            { // Offscreen
                if (screenpos.z < 0)
                { // Flip it if it's behind us.
                    screenpos *= -1;
                }

                Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2;

                // make 00 the center of the screen instead of bottom left
                screenpos -= screenCenter;

                // find angle from center of screen to mouse position
                float angle = Mathf.Atan2(screenpos.y, screenpos.x);
                angle -= 90 * Mathf.Deg2Rad;

                float cos = Mathf.Cos(angle);
                float sin = -Mathf.Sin(angle);

                screenpos = screenCenter + new Vector3(sin * 150, cos * 150, 0);

                // y = mx+b format
                float m = cos / sin;
                
                Vector3 screenBounds = screenCenter * 0.9f;

                // check up and down first
                if(cos > 0)
                {
                    screenpos = new Vector3(screenBounds.y / m, screenBounds.y, 0);
                }
                else
                { // down
                    screenpos = new Vector3(-screenBounds.y / m, -screenBounds.y, 0);
                }
                // if out of bounds, get point on appropriote side
                if (screenpos.x > screenBounds.x)
                { // out of bounds! must be on the right
                    screenpos = new Vector3(screenBounds.x, screenBounds.x * m, 0);
                }
                else if (screenpos.x < -screenBounds.x)
                { // out of bounds left
                    screenpos = new Vector3(-screenBounds.x, -screenBounds.x * m, 0);
                } // else in bounds

                // remove coordinate translation;
                screenpos += screenCenter;


                markers[i].isOnScreen = false;
                markers[i].transform.position = screenpos;
                markers[i].transform.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);

            }
        }
    }


    public bool turnOn = false;


    void UpdateMarkers()
    {
        while(markers.Count < maxTrackedAtOnce)
        {
            var marker = Instantiate(markerPF, this.transform);
            markers.Add(marker.GetComponent<MarkerController>());
        }
        if (markers.Count > maxTrackedAtOnce)
        {
            //var marker = Instantiate(markerPF, this.transform);
            Destroy(markers.Last());
        }
    }

    void ToggleTracker()
    {
        displayTrackers = !displayTrackers;
    }

    // Update is called once per frame
    void Update()
    {
        if (!displayTrackers)
            return;

        UpdateMarkers();

        FillTrackedTimer();
        
            

    }

    private void FixedUpdate()
    {
        if (!displayTrackers)
            return;

        SortObjectsByDistance();
        if (displayTrackers)
        {
            var nearest = GetNearestObjects(maxTrackedAtOnce);
            if (nearest != null)
                DisplayTrackedObjects(nearest);
        }
    }




    public void OnTracker(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ToggleTracker();
        }
    }
}
