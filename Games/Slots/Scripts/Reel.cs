using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class Reel
{
    public GameObject spot1;
    public GameObject spot2;
    public GameObject spot3;
    public GameObject spot4;
    public Image spot1Img;
    public Image spot2Img;
    public Image spot3Img;
    public Image spot4Img;
    public Vector3 spot1Pos;
    public Vector3 spot2Pos;
    public Vector3 spot3Pos;
    public Vector3 spot4Pos;
    public List<ReelImages> slotImages; 
}

[System.Serializable]
public class ReelImages
{
    public Sprite sprite;
    [Range(0, 1)] public float frequency;
}
