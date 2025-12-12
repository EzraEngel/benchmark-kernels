using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public SensorAccumulator accumulator;
    public string type;
    public Vector3 look_direction;
    public float speed;
    public float view_range;
    public float field_of_view;
    public int random_seed;

    private void Update()
    {
        switch (accumulator.sensingType)
        {
            case SensingType.NAIVE:
                NaiveSense(accumulator.lineOfSight);
                break;
            case SensingType.PHYSICS:
                PhysicsSense(accumulator.lineOfSight);
                break;
            default:
                Debug.LogError("Sensing type not set. Check your settings!");
                break;
        }
    }

    private void NaiveSense(bool line_of_sight)
    {
        if (line_of_sight) Debug.LogError("Naive sensing engine does not support LOS. Check your settings!");
        foreach (Vector3 target_pos in accumulator.positions)
        {
            if (IsTargetInFieldOfView(target_pos)) accumulator.IncrementTargets();
        }
    }

    void PhysicsSense(bool line_of_sight)
    {
        List<Vector3> targets = AgentAreaQuery(line_of_sight);
        accumulator.IncrementTargets(targets.Count);
    }

    List<Vector3> AgentAreaQuery(bool los)
    {
        int layerMask = LayerMask.GetMask("Agent");
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, view_range, layerMask);
        var positions = new List<Vector3>(8);
        foreach (var hitCollider in hitColliders)
        {
            Vector3 target_pos = hitCollider.transform.position;
            if (!IsTargetInFieldOfView(target_pos))
            {
                continue;
            }
            if (los && TargetIsOccluded(target_pos))
            {
                continue;
            }
            positions.Add(target_pos);
        }
        return positions;
    }

    bool TargetIsOccluded(in Vector3 target_pos)
    {
        int layerMask = LayerMask.GetMask("Occluder");
        Vector3 direction = target_pos - transform.position;
        float maxRange = direction.magnitude;
        return Physics.Raycast(transform.position, direction, maxRange, layerMask);
    }

    private bool IsTargetInFieldOfView(Vector3 target_pos)
    {
        if (target_pos == transform.position) return false;
        Vector3 direction = target_pos - transform.position;
        float distance = direction.magnitude;
        float angle = Vector3.Angle(direction, look_direction);
        if (distance <= view_range && angle <= field_of_view / 2.0f)
        {
            return true;
        }
        return false;
    }
}
