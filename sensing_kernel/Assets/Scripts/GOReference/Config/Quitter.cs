using Unity.Entities;
using UnityEngine;

public class Quitter : MonoBehaviour
{
    private Config config;
    private int current_iteration = 0;

    private void Awake()
    {
        config = GetComponent<Config>();
    }
    void LateUpdate()
    {
        current_iteration++;
        if (current_iteration >= config.num_iterations)
        {
            Debug.Log($"Successfully ran simulation with {current_iteration} iterations.");
            SimulationData simData = GameObject.FindFirstObjectByType<SimulationData>();
            Debug.Log(Config.END_BENCHMARK_FLAG);
            Debug.Log($"### QUERY RESULT |{simData.target_count}| ###");
            Quit();
        }
    }

    public void Quit()
    {
        #if UNITY_STANDALONE
                Application.Quit();
        #endif
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
