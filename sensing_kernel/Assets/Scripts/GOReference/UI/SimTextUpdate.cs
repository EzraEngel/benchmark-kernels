using TMPro;
using UnityEngine;

public class SimTextUpdate : MonoBehaviour
{

    public SimulationData simulationData;
    public TextMeshProUGUI frameRate;
    public TextMeshProUGUI hitCount;

    // Update is called once per frame
    void Update()
    {
        frameRate.text = $"FPS: {simulationData.FPS}";
        hitCount.text = $"Hit Count: {simulationData.target_count}";
    }
}
