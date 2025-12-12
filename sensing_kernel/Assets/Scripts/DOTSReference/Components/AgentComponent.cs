using Unity.Entities;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public struct AgentComponent : IComponentData
{
    public float3 look_direction;
    public float speed;
    public float view_range;
    public float field_of_view;
    public int random_seed;
}
