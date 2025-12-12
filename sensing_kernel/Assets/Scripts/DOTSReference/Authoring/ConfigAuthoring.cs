using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class ConfigAuthoring : MonoBehaviour
{
    public GameObject agentPrefab;
    public GameObject occluderPrefab;
    
    public int numIterations;
    public ScenarioType scenarioType;

    public string scenarioName;
    public string scenarioPath;
}

public class ConfigBaker : Baker<ConfigAuthoring>
{
    public override void Bake(ConfigAuthoring authoring)
    {
        var configEntity = GetEntity(TransformUsageFlags.None);

        AddComponent(configEntity, new ConfigComponent
        {
            START_BENCHMARK_FLAG = "### START BENCHMARK ###",
            END_BENCHMARK_FLAG = "### END BENCHMARK ###",

            agentPrefabEntity = GetEntity(authoring.agentPrefab, TransformUsageFlags.Dynamic),
            occluderPrefabEntity = GetEntity(authoring.occluderPrefab, TransformUsageFlags.Dynamic),

            numIterations = authoring.numIterations,
            scenarioType = authoring.scenarioType,

            scenarioName = authoring.scenarioName,
            scenarioPath = authoring.scenarioPath,

            configurationComplete = false,
            importComplete = false,

            iterationsCompleted = 0,

            agentMask = (uint)LayerMask.GetMask("Agent"),
            occluderMask = (uint)LayerMask.GetMask("Occluder")
        });
    }
}
