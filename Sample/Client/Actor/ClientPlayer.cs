using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientPlayer : MonoBehaviour
{
    [SerializeField] float _speed;

    private Vector3 _targetDir;
    private Vector3 _targetPosition;

    private NavMeshTriangle _currentNavMesh;
    private ObjModel _level;

    private void Start()
    {
        _targetDir = Vector3.zero;
        _targetPosition = this.transform.position;
    }

    public void InitMap(ObjModel level)
    {
        _level = level;

        Vector3 myPosition = this.transform.position;
        myPosition.y = 0.0f;

        var triangles = level.Triangles;
        for(int i = 0; i < triangles.Count; ++i)
        {
            NavMeshTriangle navMeshTriangle = triangles[i];
            bool inSide = navMeshTriangle.InSidePoint(myPosition);
            if (inSide)
            {
                Debug.LogError($"nav : {i}");
                _currentNavMesh = navMeshTriangle;
                break;
            }
        }

        if(_currentNavMesh == null)
        {
            Debug.LogError("Outside Navmesh!");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            var result = GetClickPosition();
            if (!result.isValid)
                return;

            _targetDir = result.position - this.transform.position;
            _targetDir.Normalize();
            _targetDir.y = 0.0f;
            _targetPosition = result.position;
            _targetPosition.y = 0.0f;
        }

        UpdateMove();
    }

    private void UpdateMove()
    {
        if(_currentNavMesh == null)
        {
            return;
        }

        if (_targetPosition == this.transform.position)
        {
            _targetDir = Vector3.zero;
            return;
        }

        var nextPos = Vector3.MoveTowards(this.transform.position, _targetPosition, Time.deltaTime * _speed);
        bool inSide = _currentNavMesh.InSidePoint(nextPos);
        if(inSide)
        {
            this.transform.position = nextPos;
            return;
        }

        var nextNavMesh = _currentNavMesh.CalcInSideSiblingNavMesh(nextPos);
        if (nextNavMesh != null)
        {
            _currentNavMesh = nextNavMesh;
            this.transform.position = nextPos;
            return;
        }

        var triangles = _level.Triangles;
        for (int i = 0; i < triangles.Count; ++i)
        {
            NavMeshTriangle navMeshTriangle = triangles[i];
            inSide = navMeshTriangle.InSidePoint(nextPos);
            if (inSide)
            {
                _currentNavMesh = navMeshTriangle;
                this.transform.position = nextPos;
                return;
            }
        }

        // TODO 네비 밖에 있을경우 삼각형 선분 까지 땡겨와야함.
    }

    private (bool isValid, Vector3 position) GetClickPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, Mathf.Infinity))
        {
            return (true, hit.point);
        }

        return (true, Vector3.zero);
    }
}
