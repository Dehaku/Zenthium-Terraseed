﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class World : MonoBehaviour {

    private static World _instance; // Private reference to the world instance we're going to use.
    public static World Instance { get { return _instance; } } // Public reference to that same instance.

    public Texture2D[] terrainTextures;
    [HideInInspector]
    public Texture2DArray terrainTexArray;

    public int dayStartTime = 240; // 4am.
    public int dayEndTime = 1360; // 10pm - 22 * 60 = 1360
    public int dayOverflow = 45;
    private int dayLength { get { return dayEndTime - dayStartTime; } }
    private float sunDayRotationPerMinute { get { return 180f / dayLength; } }
    private float sunNightRotationPerMinute { get { return 180f / (1440 - dayLength); } }

    public Transform sun;
    public Light sunLight;
    public TextMeshProUGUI clock;

    public int HordeNightFrequency = 7;

    [Range(0,1)]
    public float speedForce = 1;


    public Material skyboxDay;
    public Material skyboxNight;


    // Check if the current day is divisible by the horde night frequency. If yes, return true.
    public bool IsHordeNight {
        get {
            if (Day % HordeNightFrequency == 0)
                return true;
            else
                return false;
        }
    }

    [Range(0f, 100f)] // Higher is faster
    public float ClockSpeed = 1f;

    public int Day = 1;

    public int TimeOfDayInt{
        get { return Mathf.FloorToInt(_timeOfDay); }
    }

    // The current time in minutes. We can't call it "Time" because that would conflict with Unity's built in Time function.
    [SerializeField] private float _timeOfDay; // Serialize so we can see in Inspector.
    public float TimeOfDay {

        get { return _timeOfDay; }
        set {

            _timeOfDay = value;
            // There are 1440 minutes in a day wrap our value back around to 0 when it goes over that.
            if (_timeOfDay > 1439) {

                _timeOfDay = 0;
                Day++;

            }

            UpdateClock();

            float rotAmount;

            // The start of the "day" is zero rotation on the sunlight, so that's the most straightforward
            // calculation.
            if (_timeOfDay > dayStartTime && _timeOfDay < dayEndTime) {

                rotAmount = (_timeOfDay - dayStartTime) * sunDayRotationPerMinute;

            // At the end of the "day" we switch to night rotation speed, but in order to keep the rotation
            // seamless, we need to account for the daytime rotation as well.
            } else if (_timeOfDay >= dayEndTime) {
                
                // Calculate the amount of rotation through the day so far.
                rotAmount = dayLength * sunDayRotationPerMinute;
                // Add the rotation since the end of the day.
                rotAmount += ((_timeOfDay - dayStartTime - dayLength) * sunNightRotationPerMinute);

            // Else we're at the start of a new day but because we're still in the same rotation cycle, we need to
            // to account for all the previous rotation since dayStartTime the previous day.
            } else {

                rotAmount = dayLength * sunDayRotationPerMinute; // Previous day's rotation.
                rotAmount += (1440 - dayEndTime) * sunNightRotationPerMinute; // Previous night's rotation.
                rotAmount += _timeOfDay * sunNightRotationPerMinute; // Rotation since midnight.

            }

            sun.eulerAngles = new Vector3(rotAmount, 0f, 0f);

        }
    }

    private void UpdateClock () {

        int TimeOfDayInt = Mathf.FloorToInt(TimeOfDay);
        int hours = TimeOfDayInt / 60;
        int minutes = TimeOfDayInt - (hours * 60);

        string dayText;
        if (IsHordeNight)
            dayText = string.Format("<color=red>{0}</color>", Day.ToString());
        else
            dayText = Day.ToString();


        int enemyCount = 0;
        //var enemies = FindObjectsOfType<Enemy>();
        GameObject[] enemiesGO = GameObject.FindGameObjectsWithTag("GameEntity");
        foreach (var enemy in enemiesGO)
        {
            var enemyHealth = enemy.GetComponent<Enemy>();
            if(enemyHealth)
                if (enemyHealth.Conscious)
                    enemyCount++;
        }


        // Gradually dim the sun as it goes down and brighten as it comes up.
        if (TimeOfDayInt > dayEndTime)
        {
            sunLight.intensity -= 1 / ((float) dayOverflow/4);
            if (sunLight.intensity < 0)
            {
                sunLight.intensity = 0;
            }
        }
        else if (TimeOfDayInt > dayStartTime+dayOverflow)
            
        {
            sunLight.intensity += 1 / ((float) dayOverflow/4);
            if (sunLight.intensity > 1)
            {
                sunLight.intensity = 1;
            }
        }

        // Rotate the night skybox
        float skyboxRotate = skyboxNight.GetFloat("_RotationY");
        float dayPercent =  TimeOfDay / 1440;
        float dayRot = 360-(360 * dayPercent);
        skyboxNight.SetFloat("_RotationY", dayRot);




        float skyboxBlend = skyboxNight.GetFloat("_SkyBlend");

        //Gradually fade in/out the Night Skybox
        if(TimeOfDayInt == dayEndTime+dayOverflow)
        {
            float newBlend = Mathf.Min(1, skyboxBlend + 1 / ((float)dayOverflow / 4));
            //skyboxNight.SetFloat("_Blend", newBlend);
            skyboxNight.DOFloat(1, "_SkyBlend", dayOverflow).SetEase(Ease.InQuad);
        }
        else if(TimeOfDayInt == dayStartTime-(dayOverflow*2))
        {
            float newBlend = Mathf.Max(0, skyboxBlend - 1 / ((float)dayOverflow / 4));
            //skyboxNight.SetFloat("_Blend", newBlend);
            skyboxNight.DOFloat(0, "_SkyBlend", dayOverflow).SetEase(Ease.OutQuad);
        }
        



        if (TimeOfDayInt > (dayEndTime + dayOverflow) || TimeOfDayInt < (dayStartTime - dayOverflow) )
        {
            RenderSettings.skybox = skyboxNight;
            sun.gameObject.SetActive(false);
        }
        else
        {
            RenderSettings.skybox = skyboxDay;
            sun.gameObject.SetActive(true);
        }



        // Adding "D2" to the ToString() command ensures that there will always be two digits displayed. D2 == Ints, 00 = Floats/Doubles
        int hourInt = Mathf.FloorToInt(hours);
        int minuteInt = Mathf.FloorToInt(minutes);
        clock.text = string.Format("DAY: {0} TIME: {1}:{2} ENEMIES: {3}", dayText, hourInt.ToString("D2"), minuteInt.ToString("D2"), enemyCount);

            
        

    }

    private void Awake() {

        // The first thing this script does is check to see if an instance of the it has already been assigned.
        // If it has, and if that instance is not THIS instance, it deletes itself because we can't have more
        // than one instance.
        if (_instance != null && _instance != this) {

            Debug.LogWarning("More than one instance of World present. Removing additional instance.");
            Destroy(this.gameObject);

        // Else we set the instance to this script. It will now be accessible everywhere through "World.Instance"
        } else
            _instance = this;

        PopulateTextureArray();

        skyboxNight.SetFloat("_RotationY", 0);
        skyboxNight.SetFloat("_SkyBlend", 0);



    }

    private float secondCounter = 0;



    private void Update() {

        // Increment TimeOfDay every second. Change 1f to speed up/slow down time. (2f would make days twice as long, 0.5f half as long).
        secondCounter += Time.deltaTime;
        TimeOfDay += Time.deltaTime*ClockSpeed;
        if (secondCounter > ClockSpeed) {
            //TimeOfDay++;
            //secondCounter = 0;
        }

    }

    void PopulateTextureArray() {

        terrainTexArray = new Texture2DArray(1024, 1024, terrainTextures.Length, TextureFormat.ARGB32, false);

        for (int i = 0; i < terrainTextures.Length; i++) {

            // Set the pixes for each texture using the same index as we're storing them in our array.
            terrainTexArray.SetPixels(terrainTextures[i].GetPixels(0), i, 0);

        }

        terrainTexArray.Apply();

        var material = Resources.Load<Material>("Materials/Terrain");
        material.SetTexture("_TexArr", terrainTexArray);

    }

    private void OnDestroy()
    {
        skyboxNight.SetFloat("_RotationY", 0);
        skyboxNight.SetFloat("_SkyBlend", 0);
    }

}
