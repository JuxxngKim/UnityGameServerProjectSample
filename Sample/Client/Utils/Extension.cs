using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Extension
{
	public static T GetOrAddComponent<T>(this GameObject go) where T : UnityEngine.Component
	{
		return Util.GetOrAddComponent<T>(go);
	}

	public static Float3 ToFloat3(this Vector3 vector3)
    {
		var float3 = new Float3();
		float3.X = vector3.x;
		float3.Y = 0.0f;
		float3.Z = vector3.z;

		return float3;
	}

	public static Vector3 ToVector3(this Float3 float3)
	{
		return new Vector3(float3.X, float3.Y, float3.Z);
	}
}
