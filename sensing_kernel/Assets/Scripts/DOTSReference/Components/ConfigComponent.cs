using Unity.Entities;
using Unity.Burst;
using Unity.Collections;

[BurstCompile]
public struct ConfigComponent : IComponentData
{
    public FixedString64Bytes START_BENCHMARK_FLAG;
    public FixedString64Bytes END_BENCHMARK_FLAG;

    public Entity agentPrefabEntity;
    public Entity occluderPrefabEntity;

    public int numIterations;
    public ScenarioType scenarioType;

    public FixedString512Bytes scenarioName;
    public FixedString512Bytes scenarioPath;

    public bool importComplete;
    public bool configurationComplete;

    public int iterationsCompleted;

    public uint agentMask;
    public uint occluderMask;
}
