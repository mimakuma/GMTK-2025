using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LevelLogic : MonoBehaviour
{
    [Header("Knot Used in this Level:")]
    public Knot knot;
    [Space(5)]
    [Header("Timer Settings")]
    public Image timer;
    [Tooltip("Time is measured in seconds")]
    public float timeToCompleteKnot = 30f;
    [Header("Button Combination")]
    public GameObject buttonsHolder;
    public List<GameObject> buttonsCombination;

    void Update()
    {
        // timer logic goes here
    }
    public void StartCountdown()
    {

    }
}
