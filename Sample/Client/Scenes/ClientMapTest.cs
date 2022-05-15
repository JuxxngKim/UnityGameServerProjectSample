using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientMapTest : MonoBehaviour
{
    [SerializeField] ClientPlayer _clientPlayer;

    public ObjModel Level { get; private set; }
     
    void Start()
    {
        Level = new ObjModel("Assets/NavMesh.obj");
        _clientPlayer?.InitMap(Level);
    }


#if UNITY_EDITOR

    private int totalIndex = 0;

    private void OnDrawGizmos()
    {
        if (Level == null || Level.IsVaild == false)
            return;
        if (Level.Triangles == null)
            return;

        Gizmos.color = Color.red;

        for (int i = 0; i < Level.Triangles.Count; ++i)
        {
            var triangle = Level.Triangles[i];

            if(i == 0)
            {
                Gizmos.color = Color.yellow;
            }
            else
            {
                Gizmos.color = Color.red;
            }

            Gizmos.DrawLine(triangle.A, triangle.B);
            Gizmos.DrawLine(triangle.B, triangle.C);
            Gizmos.DrawLine(triangle.C, triangle.A);

            if (totalIndex >= 20)
                totalIndex = 0;
        }
    }
#endif
}
