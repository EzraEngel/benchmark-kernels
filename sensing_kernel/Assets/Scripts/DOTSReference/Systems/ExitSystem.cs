using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Unity.Entities;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(SensingSystem))]
public partial class ExitSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<ConfigComponent>();
    }

    protected override void OnUpdate()
    {
        ConfigComponent config = SystemAPI.GetSingleton<ConfigComponent>();
        if (config.iterationsCompleted >= config.numIterations)
        {
            UnityEngine.Debug.Log(config.END_BENCHMARK_FLAG.ToString());
            Debug.Log($"### QUERY RESULT |{GetTargetCount()}| ###");
            Exit();
        }
    }

    public void Exit()
    {
        #if UNITY_STANDALONE
                Application.Quit();
        #endif
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public int GetTargetCount()
    {
        int target_count = 0;
        foreach (var sensor in SystemAPI.Query<RefRO<SensorComponent>>())
        {
            target_count += sensor.ValueRO.target_count;
        }
        return target_count;
    }
}
