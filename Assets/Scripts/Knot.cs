using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "Knot", menuName = "ScriptableObjects/KnotCombination")]
public class Knot : ScriptableObject
{
    public string knotName;
    public List<string> knotCombination;
}
