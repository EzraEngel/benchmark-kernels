using UnityEngine;

[System.Serializable]
public class Float3Data
{
    public float x;
    public float y;
    public float z;

    public Vector3 ToVector()
    {
        return new Vector3(x, y, z);
    }
}

[System.Serializable]
public class Float4Data
{
    public float x;
    public float y;
    public float z;
    public float w;

    public Quaternion ToQuat()
    {
        return new Quaternion(x, y, z, w);
    }
}

[System.Serializable]
public class AgentData
{
    public string type;
    public Float3Data position;
    public Float4Data rotation;
    public float speed;
    public Float3Data look_direction;
    public float view_range;
    public float field_of_view;
    public int random_seed;
}


[System.Serializable]
public class OccluderData
{
    public string type;
    public Float3Data position;
    public Float4Data rotation;
    public int random_seed;
    public float scale;
    public string shape;
}

// These represent the macro structure of the json:
// A list inside an object. (Or two lists).
[System.Serializable]
public class AgentList
{
    public AgentData[] agents;
}

[System.Serializable]
public class AgentOccluderList : AgentList
{
    public OccluderData[] occluders;
}
