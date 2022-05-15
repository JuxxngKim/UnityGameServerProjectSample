using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Triangle
{
    public Vector3 A { get; private set; }
    public Vector3 B { get; private set; }
    public Vector3 C { get; private set; }

    public Triangle(Vector3 a, Vector3 b, Vector3 c)
    {
        a.y = 0;
        b.y = 0;
        c.y = 0;

        A = a;
        B = b;
        C = c;
    }

    public bool InSidePoint(Vector3 point)
    {
        Vector3 a = Vector3.Cross(B - A, C - A);
        Vector3 b = Vector3.Cross(B - point, C - point);
        Vector3 c = Vector3.Cross(C - point, A - point);
        Vector3 d = Vector3.Cross(A - point, B - point);

        float r = b.magnitude + c.magnitude + d.magnitude;
        return (int)r == (int)a.magnitude;
    }
}

public class NavMeshTriangle
{
    public Triangle Triangle { get; private set; }
    public Dictionary<int, NavMeshTriangle> Siblings { get; private set; }

    public UnityEngine.Vector3[] Vertices
    {
        get
        {
            if (_vertices == null)
                _vertices = new Vector3[] { Triangle.A, Triangle.B, Triangle.C };

            return _vertices;
        }
    }

    private Vector3[] _vertices;

    public Vector3 A => Triangle.A;
    public Vector3 B => Triangle.B;
    public Vector3 C => Triangle.C;

    public NavMeshTriangle(Triangle triangle)
    {
        Triangle = triangle;
    }

    public void SetSiblings(Dictionary<int, NavMeshTriangle> siblings)
    {
        Siblings = siblings;
    }


    public bool InSidePoint(Vector3 point, bool writeLog = false)
    {
        Vector3 a = Vector3.Cross(B - A, C - A);
        Vector3 b = Vector3.Cross(B - point, C - point);
        Vector3 c = Vector3.Cross(C - point, A - point);
        Vector3 d = Vector3.Cross(A - point, B - point);

        float r = b.magnitude + c.magnitude + d.magnitude;
        if (writeLog)
        {
            System.Console.WriteLine($"r : {r}");
            System.Console.WriteLine($"a : {a.magnitude}");
        }

        return Mathf.Abs(r - a.magnitude) <= 1.0f;
    }

    public NavMeshTriangle CalcInSideSiblingNavMesh(Vector3 point)
    {
        var d_enum = Siblings.GetEnumerator();
        while (d_enum.MoveNext())
        {
            int navIndex = d_enum.Current.Key;
            NavMeshTriangle sibling = d_enum.Current.Value;
            if (sibling.InSidePoint(point))
            {
                return sibling;
            }
        }

        return null;
    }
}