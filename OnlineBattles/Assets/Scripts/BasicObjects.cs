using System;
using UnityEngine;

[Serializable]
public class ModifiedQuaternion
{
    public readonly float X;
    public readonly float Y;
    public readonly float Z;
    public readonly float W;

    public ModifiedQuaternion(Quaternion quaternion)
    {
        X = quaternion.x;
        Y = quaternion.y;
        Z = quaternion.z;
        W = quaternion.w;
    }

    public Quaternion GetQuaternion()
    {
        return new Quaternion(X, Y, Z, W);
    }
}

[Serializable]
public class ModifiedVector3
{
    public readonly float X;
    public readonly float Y;
    public readonly float Z;

    public ModifiedVector3(Vector3 vector)
    {
        X = vector.x;
        Y = vector.y;
        Z = vector.z;
    }

    public Vector3 GetVector3()
    {
        return new Vector3(X, Y, Z);
    }
}