using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Burst;
using UnityEngine;
using Unity.Physics.Systems;
using System.Diagnostics;
using System.Linq;
using System;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Collections.LowLevel.Unsafe;


[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct SensingSystem : ISystem
{
    private EntityQuery agentQuery;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ConfigComponent>();
        state.RequireForUpdate<PhysicsWorldSingleton>();
        agentQuery = SystemAPI.QueryBuilder().WithAll<AgentComponent, LocalTransform, SensorComponent>().Build();
    }


    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingletonRW<ConfigComponent>();
        if (!config.ValueRO.importComplete) return;

        var rays = new NativeList<Ray>(5 * agentQuery.CalculateEntityCount(), Allocator.TempJob);

        var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        var agentFilter = new CollisionFilter
        {
            BelongsTo = config.ValueRO.agentMask,
            CollidesWith = config.ValueRO.agentMask,
            GroupIndex = 0
        };
        var occluderFilter = new CollisionFilter
        {
            BelongsTo = config.ValueRO.occluderMask,
            CollidesWith = config.ValueRO.occluderMask,
            GroupIndex = 0
        };

        var broadphaseJob = new PhysicsSensingJob()
        {
            lineOfSight = (config.ValueRO.scenarioType == ScenarioType.GEO_LOS),
            collisionWorld = collisionWorld,
            agentFilter = agentFilter,
            occluderFilter = occluderFilter,
            rays = rays.AsParallelWriter()
        };

        JobHandle jobHandle = broadphaseJob.ScheduleParallel(state.Dependency);

        jobHandle.Complete();
        rays.Dispose();

        config.ValueRW.iterationsCompleted++;
        //int iteration = config.ValueRO.iterationsCompleted;
        //int target_count = GetTargetCount(ref state);
        //var logMessage = new FixedString64Bytes();
        //logMessage.Append("Iteration #");
        //logMessage.Append(iteration);
        //logMessage.Append(" | ");
        //logMessage.Append("Target Count: ");
        //logMessage.Append(target_count);
        //UnityEngine.Debug.Log(logMessage);
    }



    [BurstCompile]
    public partial struct BruteForceSensingJob : IJobEntity
    {
        [ReadOnly] public NativeArray<LocalTransform> allTransforms;

        public readonly void Execute(in AgentComponent agent, in LocalTransform transform, ref SensorComponent sensor)
        {
            sensor.target_count = 0;
            foreach (var otherT in allTransforms)
            {
                var dirToTarget = otherT.Position - transform.Position;
                if (!otherT.Position.Equals(transform.Position) && 
                    math.distance(transform.Position, otherT.Position) <= agent.view_range &&
                    AngleBetween(agent.look_direction, dirToTarget) <= agent.field_of_view/2)
                {
                    sensor.target_count++;
                }
            }
        }
    }



    [BurstCompile]
    public partial struct PhysicsSensingJob : IJobEntity
    {
        [ReadOnly] public bool lineOfSight;
        [ReadOnly] public CollisionWorld collisionWorld;
        [ReadOnly] public CollisionFilter agentFilter;
        [ReadOnly] public CollisionFilter occluderFilter;

        public NativeList<Ray>.ParallelWriter rays;
        public void Execute(in Entity agentEntity, in AgentComponent agent, in LocalTransform transform, ref SensorComponent sensor)
        {

            var fovCollector = new FOVBroadPhaseCollector(
                agentEntity,
                transform.Position,
                agent.look_direction,
                agent.field_of_view,
                agent.view_range,
                ref rays,
                lineOfSight,
                ref collisionWorld,
                occluderFilter
            );


            collisionWorld.OverlapSphereCustom(
                transform.Position,
                agent.view_range,
                ref fovCollector,
                agentFilter
            );


            sensor.target_count = fovCollector.NumHits;
        }
    }


    [BurstCompile]
    public struct FOVBroadPhaseCollector : ICollector<DistanceHit>
    {
        private readonly Entity AgentEntity;
        private readonly float3 AgentPosition;
        private readonly float3 AgentLookDirection;
        private readonly float AgentFieldOfView;
        private readonly bool LineOfSight;
        private readonly CollisionWorld World;
        private readonly CollisionFilter OccluderFilter;

        private NativeList<Ray>.ParallelWriter Rays;
        public readonly bool EarlyOutOnFirstHit => false;
        public float MaxFraction { get; private set; }
        public int NumHits { get; private set; }

        public FOVBroadPhaseCollector(Entity agentEntity,
                                      float3 agentPosition, 
                                      float3 agentLookDirection, 
                                      float agentFieldOfView, 
                                      float agentViewRange, 
                                      ref NativeList<Ray>.ParallelWriter rays, 
                                      bool lineOfSight, 
                                      ref CollisionWorld collisionWorld, 
                                      CollisionFilter occluderFilter)
        {
            AgentEntity = agentEntity;
            AgentPosition = agentPosition;
            AgentLookDirection = agentLookDirection;
            AgentFieldOfView = agentFieldOfView;
            MaxFraction = agentViewRange;
            NumHits = 0;
            Rays = rays;
            LineOfSight = lineOfSight;
            World = collisionWorld;
            OccluderFilter = occluderFilter;
        }

        public bool AddHit(DistanceHit hit)
        {

            if (hit.Entity == AgentEntity)
            {
                return false;
            }

            var targetDirection = hit.Position - AgentPosition;
            if (AngleBetween(targetDirection, AgentLookDirection) > AgentFieldOfView / 2)
            {
                return false;
            }

            var ray = new RaycastInput() { Start = AgentPosition, End = hit.Position, Filter = OccluderFilter };
            if (LineOfSight && World.CastRay(ray)) { return false; }  // IMPORTANT to use lazy evaluator here as second check is non-trivial &&

            NumHits++;


            /*var ray = new Ray()
            {
                sensorEntity = AgentEntity,
                targetEntity = hit.Entity,
                sensorPosition = AgentPosition,
                targetPosition = hit.Position
            };
            Rays.AddNoResize(ray);*/
            return true;
        }
    }


    [BurstCompile]
    public static float AngleBetween(in float3 from, in float3 to)
    {
        float3 normalizedFrom = math.normalize(from);
        float3 normalizedTo = math.normalize(to);
        float dotProduct = math.dot(normalizedFrom, normalizedTo);
        float angleRadians = math.acos(dotProduct);
        float angleDegrees = math.degrees(angleRadians);
        return angleDegrees;
    }

    public struct Ray
    {
        public Entity sensorEntity;
        public Entity targetEntity;
        public float3 sensorPosition;
        public float3 targetPosition;
    }

    [BurstCompile]
    public int GetTargetCount(ref SystemState state)
    {
        int target_count = 0;
        foreach (var sensor in SystemAPI.Query<RefRO<SensorComponent>>())
        {
            target_count += sensor.ValueRO.target_count;
        }
        return target_count;
    }

}
