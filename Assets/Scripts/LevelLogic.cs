using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LevelLogic : MonoBehaviour
{
    [Header("Knot Used in this Level")]
    public Knot knot;
    public Sprite keyUp;
    public Sprite keyDown;
    public Sprite keyLeft;
    public Sprite keyRight;
    [Header("Knot Images")]
    public Image knotImage;
    public List<Sprite> knotImages;
    public Sprite knotDoneImage;
    [Header("Timer Settings")]
    public Image timer;
    [Tooltip("Time is measured in seconds")]
    public float timeToCompleteKnot = 30f;
    [Header("Button Combination")]
    public GameObject buttonsHolder;
    public List<GameObject> buttonsCombination;

    void OnEnable()
    {
        knotImage.sprite = knotImages[0];
    }

    void Update()
    {
        // timer logic goes here
    }
    public void StartCountdown()
    {

    }
}
