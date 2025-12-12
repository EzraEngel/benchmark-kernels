using UnityEngine;
using System.Collections.Generic;
public enum SensingType
{
    NAIVE,
    PHYSICS
}
public class SensorAccumulator : MonoBehaviour
{
    private int target_count = 0;
    public List<Vector3> positions;
    public SensingType sensingType;
    public bool lineOfSight;

    private SimulationData simulationData;

    private void Awake()
    {
        simulationData = GetComponent<SimulationData>();
        lineOfSight = (GetComponent<Config>().scenario_type == ScenarioType.GEO_LOS);
    }

    void LateUpdate()
    {
        simulationData.target_count = target_count;
        target_count = 0;
    }

    public void IncrementTargets()
    {
        target_count++;
    }
    public void IncrementTargets(int n)
    {
        target_count += n;
    }
}
