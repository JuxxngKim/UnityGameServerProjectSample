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

    private List<Triangle> _tris;
    private List<Vector3> _norms;
    private List<Vector3> _monsterPositions;
    private List<Vector3> _monsterAngles;

    private List<NavMeshTriangle> _meshTriangles;

    public List<NavMeshTriangle> Triangles => _meshTriangles;
    public List<Vector3> MonsterPositions => _monsterPositions;
    public List<Vector3> MonsterAngles => _monsterAngles;

    public bool IsVaild { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjModel"/> class.
    /// </summary>
    /// <param name="path">The path of the .obj file to parse.</param>
    public ObjModel(string path)
    {
        _tris = new List<Triangle>();
        _norms = new List<Vector3>();
        List<Vector3> tempVerts = new List<Vector3>();
        List<Vector3> tempNorms = new List<Vector3>();

        IsVaild = false;

        using (StreamReader reader = new StreamReader(path))
        {
            try
            {
                string file = reader.ReadToEnd();
                foreach (string l in file.Split('\n'))
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

                                _tris.Add(new Triangle(tempVerts[v0], tempVerts[v1], tempVerts[v2]));
                                if (tempNorms.Count > n0)
                                    _norms.Add(tempNorms[n0]);
                                if (tempNorms.Count > n1)
                                    _norms.Add(tempNorms[n1]);
                                if (tempNorms.Count > n2)
                                    _norms.Add(tempNorms[n2]);
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

                                    _tris.Add(new Triangle(tempVerts[v0], tempVerts[vi], tempVerts[vii]));
                                    if (tempNorms.Count > n0)
                                        _norms.Add(tempNorms[n0]);
                                    if (tempNorms.Count > ni)
                                        _norms.Add(tempNorms[ni]);
                                    if (tempNorms.Count > nii)
                                        _norms.Add(tempNorms[nii]);
                                }
                            }
                            break;
                    }
                }
                CalculateTriSibling();
                IsVaild = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load NavMesh : {ex}");
            }
        }
    }

    public void InitMonsterFile(string path)
    {
        _monsterPositions = new List<Vector3>();
        _monsterAngles = new List<Vector3>();

        using (StreamReader reader = new StreamReader(path))
        {
            try
            {
                string file = reader.ReadToEnd();
                foreach (string l in file.Split('\n'))
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
                            if (!TryParseVec(line, 1, 2, 3, out var v)) continue;
                            _monsterPositions.Add(v);
                            break;

                        case "a":
                            if (line.Length < 4)
                                continue;
                            if (!TryParseVec(line, 1, 2, 3, out var a)) continue;
                            _monsterAngles.Add(a);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load NavMesh : {ex}");
            }
        }
    }

    public void CalculateTriSibling()
    {
        _meshTriangles = new List<NavMeshTriangle>(_tris.Count);

        for (int i = 0; i < _tris.Count; ++i)
        {
            _meshTriangles.Add(new NavMeshTriangle(_tris[i]));
        }

        for (int i = 0; i < _meshTriangles.Count; ++i)
        {
            NavMeshTriangle current = _meshTriangles[i];
            Dictionary<int, NavMeshTriangle> siblings = new Dictionary<int, NavMeshTriangle>();

            for (int j = 0; j < _meshTriangles.Count; ++j)
            {
                if (j == i)
                    continue;

                var other = _meshTriangles[j];
                if (current.A == other.A || current.A == other.B || current.A == other.C)
                {
                    if (!siblings.ContainsKey(j))
                    {
                        siblings.Add(j, other);
                    }
                    continue;
                }

                if (current.B == other.A || current.B == other.B || current.B == other.C)
                {
                    if (!siblings.ContainsKey(j))
                    {
                        siblings.Add(j, other);
                    }
                    continue;
                }

                if (current.C == other.A || current.C == other.B || current.C == other.C)
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

    /// <summary>
    /// Gets an array of the triangles in this model.
    /// </summary>
    /// <returns></returns>
    public Triangle[] GetTriangles()
    {
        return _tris.ToArray();
    }

    /// <summary>
    /// Gets an array of the normals in this model.
    /// </summary>
    /// <returns></returns>
    public Vector3[] GetNormals()
    {
        return _norms.ToArray();
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

