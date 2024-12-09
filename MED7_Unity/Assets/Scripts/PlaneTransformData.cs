using UnityEngine;

public struct PlaneTransformData
{
    public Vector3 position;
    public Quaternion rotation;

    public PlaneTransformData(Vector3 position, Quaternion rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }
}
