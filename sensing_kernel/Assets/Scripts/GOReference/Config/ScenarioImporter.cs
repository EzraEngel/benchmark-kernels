using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class ScenarioImporter : MonoBehaviour
{
    public GameObject agentPrefab;
    public GameObject occluderPrefab;
    private Config config;
    private SensorAccumulator accumulator;

 
    private void Awake()
    {
        accumulator = GetComponent<SensorAccumulator>();
        config = GetComponent<Config>();
        LoadScenario();
        Debug.Log(Config.START_BENCHMARK_FLAG);
    }


    private void LoadScenario()
    {
        string scenarioPath = Path.Combine(Application.persistentDataPath, config.scenario_name);
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
                            LoadSingleAgent(agentData, ref accumulator);
                        }
                    }
                    else if (jr.TokenType == JsonToken.PropertyName)
                    {
                        jr.Read();
                        while (jr.Read() && jr.TokenType != JsonToken.EndArray)
                        {
                            OccluderData occluderData= serializer.Deserialize<OccluderData>(jr);
                            LoadSingleOccluder(occluderData);
                        }
                    }
                }
            }
        }
        Debug.Log("Finished parsing data stream.");
    }

    private void LoadSingleAgent(AgentData agentData, ref SensorAccumulator accumulator)
    {
        accumulator.positions.Add(agentData.position.ToVector());

        GameObject agentGO = Instantiate(agentPrefab);
        Agent agent = agentGO.GetComponent<Agent>();

        agent.accumulator = accumulator;
        agent.type = agentData.type;
        agentGO.transform.SetPositionAndRotation(agentData.position.ToVector(), agentData.rotation.ToQuat());
        agent.speed = agentData.speed;
        agent.look_direction = agentData.look_direction.ToVector();
        agent.field_of_view = agentData.field_of_view;
        agent.view_range = agentData.view_range;
        agent.random_seed = agentData.random_seed;
    }

    private void LoadSingleOccluder(OccluderData occluderData)
    {
        GameObject occluderGO = Instantiate(occluderPrefab);

        occluderGO.transform.SetPositionAndRotation(occluderData.position.ToVector(), occluderData.rotation.ToQuat());
        occluderGO.transform.localScale = occluderData.scale * Vector3.one;
    }
}
