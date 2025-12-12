using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public enum ScenarioType
{
    GEO_NO_LOS,
    GEO_LOS,
    GRAPHS,
    PROP
}
public class Config : MonoBehaviour
{
    public const string START_BENCHMARK_FLAG = "### START BENCHMARK ###";
    public const string END_BENCHMARK_FLAG = "### END BENCHMARK ###";

    public int num_iterations = 100;

    public ScenarioType scenario_type = ScenarioType.GEO_NO_LOS;
    private Dictionary<string, ScenarioType> scenario_type_map;

    // --- These are just default path and scenario values ---
    public string scenario_name = "normal_dynamic_los_md.json";
    private string scenario_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                                                "Python", 
                                                "benchmarking", 
                                                "benchmarks", 
                                                "geometry", 
                                                "normal_dynamic_los",
                                                "normal_dynamic_los_md.json");
    private string destination_path;
    

    private void Awake()
    {
        scenario_type_map = new Dictionary<string, ScenarioType>
        {
            { "GEO_NO_LOS", ScenarioType.GEO_NO_LOS },
            { "GEO_LOS", ScenarioType.GEO_LOS },
            { "GRAPHS", ScenarioType.GRAPHS},
            { "PROP", ScenarioType.PROP }
        };

        if (Application.isBatchMode)
        {
            var args = ParseCommandLineArgs();
            num_iterations = int.Parse(args["-numUpdates"]);
            scenario_path = args["-scenarioPath"];
            scenario_name = args["-scenarioName"];
            scenario_type = scenario_type_map[args["-scenarioType"]];
        }
        
        destination_path = Path.Combine(Application.persistentDataPath, scenario_name);
        File.Copy(scenario_path, destination_path, true);
        Debug.Log("Running Simulation with Parameters:");
        Debug.Log($"Num Updates: {num_iterations}");
        Debug.Log($"Scenario Name: {scenario_name}");
        Debug.Log($"Scenario Path: {scenario_path}");
    }

    private Dictionary<string, string> ParseCommandLineArgs()
    {
        var argsDict = new Dictionary<string, string>();
        string[] args = Environment.GetCommandLineArgs();

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

}
