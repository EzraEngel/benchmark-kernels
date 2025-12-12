using Newtonsoft.Json;
using System.IO;
using Unity.Entities;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(ConfigSystem))]
public partial class ImportSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<ConfigComponent>();
    }

    protected override void OnUpdate()
    {
        var config = SystemAPI.GetSingleton<ConfigComponent>();
        if (!config.configurationComplete) return;

        LoadScenario(in config);
        SystemAPI.GetSingletonRW<ConfigComponent>().ValueRW.importComplete = true;

        Debug.Log(config.START_BENCHMARK_FLAG);
        Enabled = false;
    }

    private void LoadScenario(in ConfigComponent config)
    {
        
        string scenarioPath = Path.Combine(Application.persistentDataPath, config.scenarioName.ToString());
        Debug.Log(scenarioPath);
        if (!File.Exists(scenarioPath)) Debug.LogError("Cannot find scenario file in persistent data path.");

        Debug.Log("Parsing JSON data stream and instantiating objects...");
        using (var sr = new StreamReader(scenarioPath))
        using (var jr = new JsonTextReader(sr))
        {
            var serializer = new JsonSerializer();

            while (jr.Read())
            {
                if (jr.TokenType == JsonToken.PropertyName)
                {
                    if ((string)jr.Value == "agents")
                    {
                        jr.Read();
                        while (jr.Read() && jr.TokenType != JsonToken.EndArray)
                        {
                            AgentData agentData = serializer.Deserialize<AgentData>(jr);
                            LoadSingleAgent(agentData, in config);
                        }
                    }
                    else if (jr.TokenType == JsonToken.PropertyName)
                    {
                        jr.Read();
                        while (jr.Read() && jr.TokenType != JsonToken.EndArray)
                        {
                            OccluderData occluderData = serializer.Deserialize<OccluderData>(jr);
                            LoadSingleOccluder(occluderData, in config);
                        }
                    }
                }
            }
        }
        Debug.Log("Finished parsing data stream.");
    }

    private void LoadSingleAgent(AgentData agentData, in ConfigComponent config)
    {
        Entity agent = EntityManager.Instantiate(config.agentPrefabEntity);

        EntityManager.AddComponentData<AgentComponent>(agent, new AgentComponent
        {
            look_direction = agentData.look_direction.ToVector(),
            speed = agentData.speed,
            view_range = agentData.view_range,
            field_of_view = agentData.field_of_view,
            random_seed = agentData.random_seed
        });

        EntityManager.AddComponentData<SensorComponent>(agent, new SensorComponent
        {
            target_count = 0
        });

        EntityManager.SetComponentData<LocalTransform>(agent, new LocalTransform 
        { 
            Position = agentData.position.ToVector(),
            Rotation = agentData.rotation.ToQuat(),
            Scale = 0.2f
        });

    }

    private void LoadSingleOccluder(OccluderData occluderData, in ConfigComponent config)
    {
        Entity occluder = EntityManager.Instantiate(config.occluderPrefabEntity);

        EntityManager.AddComponentData<OccluderComponent>(occluder, new OccluderComponent { });

        EntityManager.SetComponentData<LocalTransform>(occluder, new LocalTransform
        {
            Position = occluderData.position.ToVector(),
            Rotation = occluderData.rotation.ToQuat(),
            Scale = occluderData.scale
        });
    }
}
