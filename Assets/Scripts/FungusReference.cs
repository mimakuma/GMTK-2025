using UnityEngine;
using Fungus;

public class FungusReference : MonoBehaviour
{
    private Fungus.Flowchart flowchart;

    private void Awake() { flowchart = GetComponent<Fungus.Flowchart>(); }

    public void PlayCutscene(string flowchartID)
    {
        flowchart.ExecuteBlock(flowchartID);
    }
}
