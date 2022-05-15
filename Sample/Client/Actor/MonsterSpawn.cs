using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;


public class ExportMonster
{
    [MenuItem("Custom/Export Monster")]
    static void Export()
    {
        var monsterSpawn = GameObject.FindGameObjectsWithTag("MonsterSpawn");
        string fileName = Application.dataPath + "/" + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + "MonsterPosition.obj";

        PositionToFile(monsterSpawn, fileName);
        AssetDatabase.Refresh();
    }

    static string PositionToString(GameObject[] monsterSpawn)
    {
        StringBuilder sb = new StringBuilder();

        foreach(var monster in monsterSpawn)
        {
            var child = monster.transform.GetChild(0);
            var position = child.transform.position;
            position.y = 0.0f;
            var angles = monster.transform.forward;

            sb.Append(string.Format("v {0} {1} {2}", position.x, position.y, position.z));
            sb.Append("\n");
            sb.Append(string.Format("a {0} {1} {2}", angles.x, angles.y, angles.z));
            sb.Append("\n");
        }

        return sb.ToString();
    }

    static void PositionToFile(GameObject[] monsterSpawn, string filename)
    {
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(PositionToString(monsterSpawn));
        }
    }
}

public class MonsterSpawn : MonoBehaviour
{

}

#endif