using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoadType
{
    Straight, // 직진 도로
    Corner, // 코너 도로
    Intersection, // 교차로
    BuildingTile // 건물 밑에 설치될 도로
}

public enum ClickState
{ 
    None, // 초기 상태
    Create // 도로를 그리는 중
}

public class RoadControl : MonoBehaviour
{
    [Header("Ground Layer")]
    public LayerMask groundLayer;

    [Header("Road Prefab")]
    [SerializeField] private GameObject[] roadPrefabs; // <Index> 0:Straight, 1:Corner, 2:Intersection, 3:BuildingTile

    [Header("Click State")]
    public ClickState currentClickState = ClickState.None;

    [Header("Point")]
    //private Vector3 mousePosition;
    public Vector3 startPoint = Vector3.zero;
    public Vector3 endPoint = Vector3.zero;

    // Spawn Road
    private List<Coordinate> roads = new List<Coordinate>();
    private List<Coordinate> coordinates = new List<Coordinate>();
    private int roadCountX = 0;
    private int roadCountZ = 0;

    public struct Coordinate
    {
        public float x;
        public float z;

        public Coordinate(float x, float z)
        {
            this.x = x;
            this.z = z;
        }
    }

    private void Update()
    {
        GetMouseInput();
    }

    private void GetMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartDraw();
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            EndDraw();
        }
    }

    private void StartDraw()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Building")) // 건물 위에서 마우스 클릭
            {
                if (currentClickState == ClickState.None) // 도로 그리기 시작할 건물 선택 (도로 그리기 시작)
                {
                    currentClickState = ClickState.Create;
                    startPoint = hit.collider.gameObject.transform.position;
                    //Debug.Log("그리기 시작");
                }
            }
        }
    }

    private void EndDraw()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Building")) // 건물 위에서 마우스 떼기
            {
                endPoint = hit.collider.gameObject.transform.position;

                if (startPoint == endPoint)
                {
                    currentClickState = ClickState.None;
                    //Debug.Log("그리기 취소");
                    return;
                }

                if (currentClickState == ClickState.Create) // 도로를 연결할 건물 선택 (도로 생성)
                {
                    currentClickState = ClickState.None;
                    MeasureRoad();
                }
            }
            else // 건물 이외에서 마우스 떼기
            {
                if (currentClickState == ClickState.Create)
                {
                    currentClickState = ClickState.None;
                    //Debug.Log("그리기 취소");
                }
            }
        }
    }

    private void MeasureRoad() // startPoint 부터 endPoint 까지 생성할 도로의 위치를 계산하여 저장
    {
        // 도로 하나의 길이 지정 (정사각형)
        float roadWidth = 5.0f;

        // 필요 도로 개수 계산
        roadCountX = (int)(Mathf.Abs(startPoint.x - endPoint.x) / roadWidth) + 1;
        roadCountZ = (int)(Mathf.Abs(startPoint.z - endPoint.z) / roadWidth) + 1;

        #region 방향에 따라 체크
        // ( \ 방향 )
        // start.x < end.x
        // start.z > end.z
        if (startPoint.x < endPoint.x && startPoint.z > endPoint.z)
        {
            for (float i = startPoint.x; i <= endPoint.x; i += 2.5f)
            {
                Check_Test(i);
            }
        }
        // ( / 방향 )
        // start.x < end.x
        // start.z < end.z
        else if (startPoint.x < endPoint.x && startPoint.z < endPoint.z)
        {
            for (float i = startPoint.x; i <= endPoint.x; i += 2.5f)
            {
                Check_Test(i);
            }
        }
        // ( \ 방향 )
        // start.x > end.x
        // start.z > end.z
        else if (startPoint.x > endPoint.x && startPoint.z > endPoint.z)
        {
            for (float i = startPoint.x; i >= endPoint.x; i -= 2.5f)
            {
                Check_Test(i);
            }
        }
        // ( / 방향 )
        // start.x > end.x
        // start.z < end.z
        else if (startPoint.x > endPoint.x && startPoint.z < endPoint.z)
        {
            for (float i = startPoint.x; i >= endPoint.x; i -= 2.5f)
            {
                Check_Test(i);
            }
        }
        // ( - 방향 )
        // start.x < end.x
        else if (startPoint.x < endPoint.x && startPoint.z == endPoint.z)
        {
            for (float i = startPoint.x; i <= endPoint.x; i += 5.0f)
            {
                Check_Straight(i, startPoint.z);
            }
        }
        // ( - 방향 )
        // start.x > end.x
        else if (startPoint.x > endPoint.x && startPoint.z == endPoint.z)
        {
            for (float i = startPoint.x; i >= endPoint.x; i -= 5.0f)
            {
                Check_Straight(i, startPoint.z);
            }
        }
        // ( ↑ 방향 )
        // start.z < end.z
        else if (startPoint.z < endPoint.z && startPoint.x == endPoint.x)
        {
            for (float j = startPoint.z; j <= endPoint.z; j += 5.0f)
            {
                Check_Straight(startPoint.x, j);
            }
        }
        // ( ↓ 방향 )
        // start.z > end.z
        else if (startPoint.z > endPoint.z && startPoint.x == endPoint.x)
        {
            for (float j = startPoint.z; j >= endPoint.z; j -= 5.0f)
            {
                Check_Straight(startPoint.x, j);
            }
        }
        #endregion

        SpawnRoad(coordinates);
    }

    private void Check_Test(float i)
    {
        // 시작점과 끝점을 잇는 직선위의 점들만 검사
        float a = (endPoint.z - startPoint.z) / (endPoint.x - startPoint.x);
        float b = startPoint.z - a * startPoint.x;
        float j = a * i + b;

        // x, z가 모두 중앙을 지날 때
        if (i % 5.0f == 0 && j % 5.0f == 0)
        {
            Coordinate currentCord = new Coordinate();
            currentCord.x = i;
            currentCord.z = j;
            coordinates.Add(currentCord);
        }
        // x는 중앙을, z는 경계를 지날 때 
        else if (i % 5.0f == 0 && j % 2.5 == 0)
        {
            Coordinate currentCord = new Coordinate();
            currentCord.x = i;
            currentCord.z = j - 2.5f;
            coordinates.Add(currentCord);

            Coordinate currentCord2 = new Coordinate();
            currentCord2.x = i;
            currentCord2.z = j + 2.5f;
            coordinates.Add(currentCord2);
        }
        // x는 중앙을, z는 애매할 때
        else if (i % 5.0f == 0)
        {
            float newZPoint = 0;

            if (startPoint.z < endPoint.z)
            {
                for (float t = startPoint.z; t <= endPoint.z; t += 5.0f)
                {
                    if (Mathf.Abs(j - t) < 2.5f)
                    {
                        newZPoint = t;
                        break;
                    }
                }
            }
            else
            {
                for (float t = startPoint.z; t >= endPoint.z; t -= 5.0f)
                {
                    if (Mathf.Abs(j - t) < 2.5f)
                    {
                        newZPoint = t;
                        break;
                    }
                }
            }

            Coordinate currentCord = new Coordinate();
            currentCord.x = i;
            currentCord.z = newZPoint;
            coordinates.Add(currentCord);
        }
        // x가 경계를, z는 애매할 때
        else if (i % 2.5f == 0 && j % 2.5f != 0)
        {
            float newZPoint = 0;

            if (startPoint.z < endPoint.z)
            {
                for (float t = startPoint.z; t <= endPoint.z; t += 5.0f)
                {
                    if (Mathf.Abs(j - t) < 2.5f)
                    {
                        newZPoint = t;
                        break;
                    }
                }
            }
            else
            {
                for (float t = startPoint.z; t >= endPoint.z; t -= 5.0f)
                {
                    if (Mathf.Abs(j - t) < 2.5f)
                    {
                        newZPoint = t;
                        break;
                    }
                }
            }

            Coordinate currentCord = new Coordinate();
            currentCord.x = i - 2.5f;
            currentCord.z = newZPoint;
            coordinates.Add(currentCord);

            Coordinate currentCord2 = new Coordinate();
            currentCord2.x = i + 2.5f;
            currentCord2.z = newZPoint;
            coordinates.Add(currentCord2);
        }
    }

    private void Check_Straight(float i, float j)
    {
        if (i % 5.0f == 0 && j % 5.0f == 0) // 점이 칸의 정 중앙을 지날 때 해당 칸 체크
        {
            Coordinate currentCord = new Coordinate();
            currentCord.x = i;
            currentCord.z = j;
            coordinates.Add(currentCord);
        }
    }

    private void SpawnRoad(List<Coordinate> coordinates) // 저장된 위치마다 도로를 생성하는 메서드
    {
        for (int i = 0; i < coordinates.Count; i++)
        {
            if (!roads.Contains(coordinates[i])) // 해당 위치에 이미 도로가 생성되어있는지 체크
            {
                roads.Add(coordinates[i]);
                Instantiate(roadPrefabs[4], new Vector3(coordinates[i].x, 0.01f, coordinates[i].z), Quaternion.identity);
            }
        }
        
        coordinates.Clear();
    }
}
