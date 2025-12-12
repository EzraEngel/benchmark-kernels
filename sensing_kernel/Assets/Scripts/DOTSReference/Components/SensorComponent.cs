using Unity.Entities;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public struct SensorComponent : IComponentData
{
    public int target_count;
}
