using System;
using UnityEngine;

[Serializable]
public class ModifiedQuaternion
{
    public float X;
    public float Y;
    public float Z;
    public float W;

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
    public float X;
    public float Y;
    public float Z;

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

[Serializable]
public class myVector2
{
    public float X;
    public float Y;

    public myVector2(Vector2 vector)
    {
        X = vector.x;
        Y = vector.y;
    }

    public Vector2 GetVector2()
    {
        return new Vector2(X, Y);
    }
}