using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

static class Util
{
    public static Vector3 ToVector3(this Float3 float3)
    {
        return new Vector3(float3.X, float3.Y, float3.Z);
    }

    public static Float3 ToFloat3(this Vector3 vector3)
    {
        var float3 = new Float3();
        float3.X = vector3.x;
        float3.Y = 0.0f;
        float3.Z = vector3.z;

        return float3;
    }

    public static PositionInfo Vector3ToPosInfo(Vector3 position, Vector3 diretion)
    {
        var posInfo = new PositionInfo();
        posInfo.Position = position.ToFloat3();
        posInfo.Direction = diretion.ToFloat3();
        return posInfo;
    }

    public static float FrameToTime(int frame)
    {
        return frame * 0.1f;
    }

    public static int TimeToTick(float time)
    {
        return (int)(time * 1000.0f);
    }

}
