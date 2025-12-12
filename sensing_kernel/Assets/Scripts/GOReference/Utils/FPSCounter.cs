using UnityEngine;
using TMPro; // Make sure to import the TextMeshPro namespace

/// <summary>
/// A simple MonoBehaviour to calculate and display the current framerate.
/// </summary>
public class FPSCounter : MonoBehaviour
{
    private SimulationData simulationData;
    private float pollingTime = 1.0f;
    private float timePassed;
    private int frameCount;

    private void Awake()
    {
        simulationData = GetComponent<SimulationData>();
    }
    private void Update()
    {
        timePassed += Time.unscaledDeltaTime;
        frameCount++;
        if (timePassed >= pollingTime)
        {
            int fps = Mathf.RoundToInt(frameCount / timePassed);
            simulationData.FPS = fps;
            timePassed = 0;
            frameCount = 0;
        }
    }
}