using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Substance")]
public class Substance : ScriptableObject
{
    public float amount;
    public string id;
    public float weightPerUnit;
    public Color color;
    public Sprite icon;

    public sub Subify()
    {
        sub returnSub = new sub();
        returnSub.SO = this;
        returnSub.init();
        return returnSub;
    }

    [System.Serializable]
    public class sub
    {
        public Substance SO;
        [SerializeField] public float amount;
        [SerializeField] public string id;
        [SerializeField] public float weightPerUnit;
        [SerializeField] public Color color;
        [SerializeField] public Sprite icon;

        public void init()
        {
            if (SO == null)
                return;
            id = SO.id;
            weightPerUnit = SO.weightPerUnit;
            color = SO.color;
            icon = SO.icon;
        }
    }

}


