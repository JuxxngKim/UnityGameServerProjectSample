// Copyright (c) 2013, 2015 Robert Rouhani <robert.rouhani@gmail.com> and other contributors (see CONTRIBUTORS file).
// Licensed under the MIT License - https://raw.github.com/Robmaister/SharpNav/master/LICENSE

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

/// <summary>
/// Parses a model in .obj format.
/// </summary>
public class ObjModel
{
    private static readonly char[] lineSplitChars = { ' ' };

    private List<Triangle> tris;
    private List<Vector3> norms;

    private List<NavMeshTriangle> _meshTriangles;

    public List<NavMeshTriangle> Triangles => _meshTriangles;

    public bool IsVaild { get; private set; }

    public const float kEpsilon = 0.01F;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjModel"/> class.
    /// </summary>
    /// <param name="path">The path of the .obj file to parse.</param>
    public ObjModel(string path)
    {
        tris = new List<Triangle>();
        norms = new List<Vector3>();
        List<Vector3> tempVerts = new List<Vector3>();
        List<Vector3> tempNorms = new List<Vector3>();

        IsVaild = false;

        TextAsset textAsset = Resources.Load<TextAsset>(path);
        string text = textAsset?.text ?? string.Empty;
        foreach (string l in text.Split('\n'))
        {
            //trim any extras
            string tl = l;
            int commentStart = l.IndexOf("#");
            if (commentStart != -1)
                tl = tl.Substring(0, commentStart);
            tl = tl.Trim();

            string[] line = tl.Split(lineSplitChars, StringSplitOptions.RemoveEmptyEntries);
            if (line == null || line.Length == 0)
                continue;

            switch (line[0])
            {
                case "v":
                    if (line.Length < 4)
                        continue;

                    Vector3 v;
                    if (!TryParseVec(line, 1, 2, 3, out v)) continue;
                    tempVerts.Add(v);
                    break;
                case "vn":
                    if (line.Length < 4)
                        continue;

                    Vector3 n;
                    if (!TryParseVec(line, 1, 2, 3, out n)) continue;
                    tempNorms.Add(n);
                    break;
                case "f":

                    if (line.Length < 4)
                        continue;
                    else if (line.Length == 4)
                    {
                        int v0, v1, v2;
                        int n0, n1, n2;
                        if (!int.TryParse(line[1].Split('/')[0], out v0)) continue;
                        if (!int.TryParse(line[2].Split('/')[0], out v1)) continue;
                        if (!int.TryParse(line[3].Split('/')[0], out v2)) continue;
                        if (!int.TryParse(line[1].Split('/')[2], out n0)) continue;
                        if (!int.TryParse(line[2].Split('/')[2], out n1)) continue;
                        if (!int.TryParse(line[3].Split('/')[2], out n2)) continue;

                        v0 -= 1;
                        v1 -= 1;
                        v2 -= 1;
                        n0 -= 1;
                        n1 -= 1;
                        n2 -= 1;

                        tris.Add(new Triangle(tempVerts[v0], tempVerts[v1], tempVerts[v2]));
                        if (tempNorms.Count > n0)
                            norms.Add(tempNorms[n0]);
                        if (tempNorms.Count > n1)
                            norms.Add(tempNorms[n1]);
                        if (tempNorms.Count > n2)
                            norms.Add(tempNorms[n2]);
                    }
                    else
                    {
                        int v0, n0;
                        if (!int.TryParse(line[1].Split('/')[0], out v0)) continue;
                        if (!int.TryParse(line[1].Split('/')[2], out n0)) continue;

                        v0 -= 1;
                        n0 -= 1;

                        for (int i = 2; i < line.Length - 1; i++)
                        {
                            int vi, vii;
                            int ni, nii;
                            if (!int.TryParse(line[i].Split('/')[0], out vi)) continue;
                            if (!int.TryParse(line[i + 1].Split('/')[0], out vii)) continue;
                            if (!int.TryParse(line[i].Split('/')[2], out ni)) continue;
                            if (!int.TryParse(line[i + 1].Split('/')[2], out nii)) continue;

                            vi -= 1;
                            vii -= 1;
                            ni -= 1;
                            nii -= 1;

                            tris.Add(new Triangle(tempVerts[v0], tempVerts[vi], tempVerts[vii]));
                            if (tempNorms.Count > n0)
                                norms.Add(tempNorms[n0]);
                            if (tempNorms.Count > ni)
                                norms.Add(tempNorms[ni]);
                            if (tempNorms.Count > nii)
                                norms.Add(tempNorms[nii]);
                        }
                    }
                    break;
            }
        }
        CalculateTriSibling();
    }

    public void CalculateTriSibling()
    {
        _meshTriangles = new List<NavMeshTriangle>(tris.Count);

        for (int i = 0; i < tris.Count; ++i)
        {
            _meshTriangles.Add(new NavMeshTriangle(tris[i]));
        }

        for(int i = 0; i < _meshTriangles.Count; ++i)
        {
            NavMeshTriangle current = _meshTriangles[i];
            Dictionary<int, NavMeshTriangle> siblings = new Dictionary<int, NavMeshTriangle>();

            for (int j = 0; j < _meshTriangles.Count; ++j)
            {
                if (j == i)
                    continue;

                var other = _meshTriangles[j];
                if(TriangleEquils(current, other))
                {
                    if (!siblings.ContainsKey(j))
                    {
                        siblings.Add(j, other);
                    }
                }
            }

            current.SetSiblings(siblings);
        }
    }

    public bool IsVaildPosition(Vector3 position)
    {
        position.y = 0.0f;

        for (int i = 0; i < Triangles.Count; ++i)
        {
            if (Triangles[i].InSidePoint(position))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Gets an array of the triangles in this model.
    /// </summary>
    /// <returns></returns>
    public Triangle[] GetTriangles()
    {
        return tris.ToArray();
    }

    /// <summary>
    /// Gets an array of the normals in this model.
    /// </summary>
    /// <returns></returns>
    public Vector3[] GetNormals()
    {
        return norms.ToArray();
    }

    private bool TriangleEquils(NavMeshTriangle current, NavMeshTriangle other)
    {
        if (VectorEquils(current.A, other.A) || VectorEquils(current.A, other.B) || VectorEquils(current.A, other.C))
        {
            return true;
        }

        if (VectorEquils(current.B, other.A) || VectorEquils(current.B, other.B) || VectorEquils(current.B, other.C))
        {
            return true;
        }

        if (VectorEquils(current.C, other.A) || VectorEquils(current.C, other.B) || VectorEquils(current.C, other.C))
        {
            return true;
        }

        return false;
    }

    private bool VectorEquils(Vector3 lhs, Vector3 rhs)
    {
        float diff_x = lhs.x - rhs.x;
        float diff_y = lhs.y - rhs.y;
        float diff_z = lhs.z - rhs.z;
        float sqrmag = diff_x * diff_x + diff_y * diff_y + diff_z * diff_z;
        return sqrmag < 0.1f;
    }

    private bool TryParseVec(string[] values, int x, int y, int z, out Vector3 v)
    {
        v = Vector3.zero;

        if (!float.TryParse(values[x], NumberStyles.Any, CultureInfo.InvariantCulture, out v.x))
            return false;
        if (!float.TryParse(values[y], NumberStyles.Any, CultureInfo.InvariantCulture, out v.y))
            return false;
        if (!float.TryParse(values[z], NumberStyles.Any, CultureInfo.InvariantCulture, out v.z))
            return false;

        return true;
    }
}

