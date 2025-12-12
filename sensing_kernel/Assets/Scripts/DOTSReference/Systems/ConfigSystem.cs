using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Unity.Entities;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class ConfigSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<ConfigComponent>();
    }

    protected override void OnUpdate()
    {
        ConfigComponent config = SystemAPI.GetSingleton<ConfigComponent>();


        var args = ParseCommandLineArgs();
        if (args.ContainsKey("-numUpdates")) config.numIterations = int.Parse(args["-numUpdates"]);
        if (args.ContainsKey("-scenarioPath")) config.scenarioPath = args["-scenarioPath"];
        if (args.ContainsKey("-scenarioName")) config.scenarioName = args["-scenarioName"];
        if (args.ContainsKey("-scenarioType")) config.scenarioType = StringToScenarioType(args["-scenarioType"]);
        
        string destinationPath = Path.Combine(Application.persistentDataPath, config.scenarioName.ToString());
        File.Copy(config.scenarioPath.ToString(), destinationPath, true);

        config.configurationComplete = true;
        SystemAPI.SetSingleton<ConfigComponent>(config);

        Enabled = false;
    }

    private Dictionary<string, string> ParseCommandLineArgs()
    {
        var argsDict = new Dictionary<string, string>();
        string[] args = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; ++i)
        {
            string arg = args[i];
            if (arg.StartsWith("-") || arg.StartsWith("--"))
            {
                string key = arg;
                string value = null;
                if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                {
                    value = args[i + 1];
                    i++;
                }
                argsDict[key] = value;
            }
        }
        return argsDict;
    }

    private ScenarioType StringToScenarioType(string scenarioType)
    {
        var map = new Dictionary<string, ScenarioType>
        {
            { "GEO_NO_LOS", ScenarioType.GEO_NO_LOS },
            { "GEO_LOS", ScenarioType.GEO_LOS },
            { "GRAPHS", ScenarioType.GRAPHS},
            { "PROP", ScenarioType.PROP }
        };

        return map[scenarioType];
    }
}
