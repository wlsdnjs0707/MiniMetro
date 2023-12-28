using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public enum RoadType
{
    Straight, // 직진 도로
    Corner, // 코너 도로
    Intersection, // 교차로
    BuildingTile, // 건물 밑에 설치될 도로
    Test // 선 없는 기본 타일
}

public enum ClickState
{ 
    None, // 초기 상태
    Create, // 도로를 그리는 중
}

[Serializable]
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

public class RoadControl : MonoBehaviour
{
    public static RoadControl instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Header("Ground Layer")]
    public LayerMask groundLayer;

    [Header("Road Prefab")]
    [SerializeField] private GameObject roadPrefab;
    [SerializeField] private GameObject tempRoadPrefab;

    [Header("Click State")]
    public ClickState currentClickState = ClickState.None;
    public bool canDraw = false;

    [Header("Point")]
    public Vector3 startPoint = Vector3.zero;
    public Vector3 endPoint = Vector3.zero;

    [Header("Spawn Road")]
    public List<Coordinate> roads = new List<Coordinate>();
    private List<Coordinate> coordinates = new List<Coordinate>();
    private int roadCountX = 0;
    private int roadCountZ = 0;

    [Header("Parents")]
    [SerializeField] private Transform roadParent;
    [SerializeField] private Transform tempParent;

    public event Action OnRoadCreated;

    private void Update()
    {
        GetMouseInput();
    }

    private void GetMouseInput()
    {
        if (!canDraw)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            StartDraw();
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            EndDraw();
        }

        ShowRoad();
    }

    private void StartDraw()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Road") && hit.collider.GetComponent<Road>().roadType == RoadType.BuildingTile) // 마우스 클릭
            {
                if (GameManager.instance.roadCount > 0)
                {
                    if (currentClickState == ClickState.None) // 도로 그리기 시작할 건물 선택 (도로 그리기 시작)
                    {
                        currentClickState = ClickState.Create;
                        startPoint = hit.collider.gameObject.transform.position;
                        //Debug.Log("그리기 시작");
                    }
                }
                else
                {
                    // UI 띄우기 (도로 개수가 부족합니다)
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
            if (hit.collider.CompareTag("Road")) // 마우스 떼기
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

                    //Debug.Log("그리기 완료");

                    // temp 지우기
                    GameObject temp = GameObject.FindGameObjectWithTag("Temp");

                    for (int i = 0; i < temp.transform.childCount; i++)
                    {
                        GameObject roadToDelete = temp.transform.GetChild(i).gameObject;
                        DeleteRoad(roadToDelete.GetComponent<Road>().coordinate.x, roadToDelete.GetComponent<Road>().coordinate.z);
                        Destroy(roadToDelete);
                    }

                    GameManager.instance.ChangeRoadCount(-1);

                    //Road_Optimize(3);
                    MeasureRoad(false);
                }
            }
            else // 건물 이외에서 마우스 떼기
            {
                if (currentClickState == ClickState.Create)
                {
                    currentClickState = ClickState.None;
                    coordinates.Clear();
                    //Debug.Log("그리기 취소");
                }
            }
        }

        OnRoadCreated?.Invoke();
    }

    private void ShowRoad()
    {
        if (currentClickState == ClickState.Create)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Road") && hit.collider.GetComponent<Road>().roadType == RoadType.BuildingTile)
                {
                    float newX = hit.collider.transform.position.x;
                    float newZ = hit.collider.transform.position.z;

                    for (float i = Mathf.Round(hit.collider.transform.position.x) - 1; i <= Mathf.Round(hit.collider.transform.position.x) + 1; i += 0.25f)
                    {
                        if (Mathf.Abs(i - hit.collider.transform.position.x) <= 1.25f && i % 2.5f == 0)
                        {
                            newX = i;
                            break;
                        }
                    }

                    for (float i = Mathf.Round(hit.collider.transform.position.z) - 1; i <= Mathf.Round(hit.collider.transform.position.z) + 1; i += 0.25f)
                    {
                        if (Mathf.Abs(i - (hit.collider.transform.position.z)) <= 1.25f && i % 2.5f == 0)
                        {
                            newZ = i;
                            break;
                        }
                    }

                    if (newX != endPoint.x || newZ != endPoint.z)
                    {
                        endPoint.x = newX;
                        endPoint.z = newZ;
                    }

                    // 생성
                    if (!(startPoint.x == endPoint.x && startPoint.z == endPoint.z))
                    {
                        MeasureRoad(true);
                    }
                }
                else
                {
                    // temp 지우기
                    GameObject temp = GameObject.FindGameObjectWithTag("Temp");

                    for (int i = 0; i < temp.transform.childCount; i++)
                    {
                        GameObject roadToDelete = temp.transform.GetChild(0).gameObject;
                        DeleteRoad(roadToDelete.GetComponent<Road>().coordinate.x, roadToDelete.GetComponent<Road>().coordinate.z);
                        Destroy(roadToDelete);
                    }

                    coordinates.Clear();

                    return;
                }
            }
        }
    }

    private void Road_Optimize(int n) // start와 end의 주위 n칸에 이미 이어진 도로가 있는지 확인후 연결
    {
        float startX = startPoint.x;
        float startZ = startPoint.z;

        float endX = endPoint.x;
        float endZ = endPoint.z;

        // ( ↘ 방향 )
        if (startPoint.x < endPoint.x && startPoint.z > endPoint.z)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Coordinate nextCoordinate = new Coordinate(startPoint.x + i * 2.5f, startPoint.z - j * 2.5f);
                    if (RoadControl.instance.roads.Contains(nextCoordinate))
                    {
                        startX = startPoint.x + i * 2.5f;
                        startZ = startPoint.z - j * 2.5f;
                    }
                }
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Coordinate nextCoordinate = new Coordinate(endPoint.x - i * 2.5f, endPoint.z + j * 2.5f);
                    if (RoadControl.instance.roads.Contains(nextCoordinate))
                    {
                        endX = endPoint.x - i * 2.5f;
                        endZ = endPoint.z + j * 2.5f;
                    }
                }
            }
        }
        // ( ↗ 방향 )
        else if (startPoint.x < endPoint.x && startPoint.z < endPoint.z)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Coordinate nextCoordinate = new Coordinate(startPoint.x + i * 2.5f, startPoint.z + j * 2.5f);
                    if (RoadControl.instance.roads.Contains(nextCoordinate))
                    {
                        startX = startPoint.x + i * 2.5f;
                        startZ = startPoint.z + j * 2.5f;
                    }
                }
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Coordinate nextCoordinate = new Coordinate(endPoint.x - i * 2.5f, endPoint.z - j * 2.5f);
                    if (RoadControl.instance.roads.Contains(nextCoordinate))
                    {
                        endX = endPoint.x - i * 2.5f;
                        endZ = endPoint.z - j * 2.5f;
                    }
                }
            }
        }
        // ( ↖ 방향 )
        else if (startPoint.x > endPoint.x && startPoint.z < endPoint.z)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Coordinate nextCoordinate = new Coordinate(startPoint.x - i * 2.5f, startPoint.z + j * 2.5f);
                    if (RoadControl.instance.roads.Contains(nextCoordinate))
                    {
                        startX = startPoint.x - i * 2.5f;
                        startZ = startPoint.z + j * 2.5f;
                    }
                }
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Coordinate nextCoordinate = new Coordinate(endPoint.x + i * 2.5f, endPoint.z - j * 2.5f);
                    if (RoadControl.instance.roads.Contains(nextCoordinate))
                    {
                        endX = endPoint.x + i * 2.5f;
                        endZ = endPoint.z - j * 2.5f;
                    }
                }
            }
        }
        // ( ↙ 방향 )
        else if (startPoint.x > endPoint.x && startPoint.z > endPoint.z)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Coordinate nextCoordinate = new Coordinate(startPoint.x - i * 2.5f, startPoint.z - j * 2.5f);
                    if (RoadControl.instance.roads.Contains(nextCoordinate))
                    {
                        startX = startPoint.x - i * 2.5f;
                        startZ = startPoint.z - j * 2.5f;
                    }
                }
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Coordinate nextCoordinate = new Coordinate(endPoint.x + i * 2.5f, endPoint.z + j * 2.5f);
                    if (RoadControl.instance.roads.Contains(nextCoordinate))
                    {
                        endX = endPoint.x + i * 2.5f;
                        endZ = endPoint.z + j * 2.5f;
                    }
                }
            }
        }

        startPoint.x = startX;
        startPoint.z = startZ;

        endPoint.x = endX;
        endPoint.z = endZ;
    }

    private void MeasureRoad(bool isTest) // startPoint 부터 endPoint 까지 생성할 도로의 위치를 계산하여 저장
    {
        // 도로 하나의 길이 지정 (정사각형)
        float roadWidth = 2.5f;

        // 필요 도로 개수 계산
        roadCountX = (int)(Mathf.Abs(startPoint.x - endPoint.x) / roadWidth) + 1;
        roadCountZ = (int)(Mathf.Abs(startPoint.z - endPoint.z) / roadWidth) + 1;

        #region 방향에 따라 체크
        // ( ↘ 방향 )
        // start.x < end.x
        // start.z > end.z
        if (startPoint.x < endPoint.x && startPoint.z > endPoint.z)
        {
            for (float i = startPoint.x; i <= endPoint.x; i += 1.25f)
            {
                Check_Test(i);
            }

            for (float j = startPoint.z; j >= endPoint.z; j -= 1.25f)
            {
                Check_Test_Z(j);
            }
        }
        // ( ↗ 방향 )
        // start.x < end.x
        // start.z < end.z
        else if (startPoint.x < endPoint.x && startPoint.z < endPoint.z)
        {
            for (float i = startPoint.x; i <= endPoint.x; i += 1.25f)
            {
                Check_Test(i);
            }

            for (float j = startPoint.z; j <= endPoint.z; j += 1.25f)
            {
                Check_Test_Z(j);
            }
        }
        // ( ↖ 방향 )
        // start.x > end.x
        // start.z > end.z
        else if (startPoint.x > endPoint.x && startPoint.z > endPoint.z)
        {
            for (float i = startPoint.x; i >= endPoint.x; i -= 1.25f)
            {
                Check_Test(i);
            }

            for (float j = startPoint.z; j >= endPoint.z; j -= 1.25f)
            {
                Check_Test_Z(j);
            }
        }
        // ( ↙ 방향 )
        // start.x > end.x
        // start.z < end.z
        else if (startPoint.x > endPoint.x && startPoint.z < endPoint.z)
        {
            for (float i = startPoint.x; i >= endPoint.x; i -= 1.25f)
            {
                Check_Test(i);
            }

            for (float j = startPoint.z; j <= endPoint.z; j += 1.25f)
            {
                Check_Test_Z(j);
            }
        }
        // ( - 방향 )
        // start.x < end.x
        else if (startPoint.x < endPoint.x && startPoint.z == endPoint.z)
        {
            for (float i = startPoint.x; i <= endPoint.x; i += 2.5f)
            {
                Check_Straight(i, startPoint.z);
            }
        }
        // ( - 방향 )
        // start.x > end.x
        else if (startPoint.x > endPoint.x && startPoint.z == endPoint.z)
        {
            for (float i = startPoint.x; i >= endPoint.x; i -= 2.5f)
            {
                Check_Straight(i, startPoint.z);
            }
        }
        // ( ↑ 방향 )
        // start.z < end.z
        else if (startPoint.z < endPoint.z && startPoint.x == endPoint.x)
        {
            for (float j = startPoint.z; j <= endPoint.z; j += 2.5f)
            {
                Check_Straight(startPoint.x, j);
            }
        }
        // ( ↓ 방향 )
        // start.z > end.z
        else if (startPoint.z > endPoint.z && startPoint.x == endPoint.x)
        {
            for (float j = startPoint.z; j >= endPoint.z; j -= 2.5f)
            {
                Check_Straight(startPoint.x, j);
            }
        }
        #endregion

        if (roadCountX == roadCountZ)
        {
            Check_Additonal_Diagonal();
        }

        // 더 빠른 도로가 있으면 생성 X
        if (GameManager.instance.CheckCanReach(startPoint.x, startPoint.z, endPoint.x, endPoint.z, roads, out int count_original))
        {
            // 비교 시작
            GameManager.instance.CheckCanReach(startPoint.x, startPoint.z, endPoint.x, endPoint.z, coordinates, out int count_new);

            if (count_original > count_new)
            {
                SpawnRoad(coordinates, isTest);
            }
        }
        else
        {
            SpawnRoad(coordinates, isTest);
        }
    }

    private void Check_Test(float i)
    {
        // 시작점과 끝점을 잇는 직선위의 점들만 검사
        float a = (endPoint.z - startPoint.z) / (endPoint.x - startPoint.x);
        float b = startPoint.z - a * startPoint.x;
        float j = a * i + b;

        // x, z가 모두 중앙을 지날 때
        if (i % 2.5f == 0 && j % 2.5f == 0)
        {
            Coordinate currentCord = new Coordinate();
            currentCord.x = i;
            currentCord.z = j;
            coordinates.Add(currentCord);
        }
        // x는 중앙을, z는 경계를 지날 때 
        else if (i % 2.5f == 0 && j % 1.25 == 0)
        {
            Coordinate currentCord = new Coordinate();
            currentCord.x = i;
            currentCord.z = j - 1.25f;
            coordinates.Add(currentCord);

            Coordinate currentCord2 = new Coordinate();
            currentCord2.x = i;
            currentCord2.z = j + 1.25f;
            coordinates.Add(currentCord2);
        }
        // x는 중앙을, z는 애매할 때
        else if (i % 2.5f == 0)
        {
            float newZPoint = 0;

            if (startPoint.z < endPoint.z)
            {
                for (float t = startPoint.z; t <= endPoint.z; t += 2.5f)
                {
                    if (Mathf.Abs(j - t) < 1.25f)
                    {
                        newZPoint = t;
                        break;
                    }
                }
            }
            else
            {
                for (float t = startPoint.z; t >= endPoint.z; t -= 2.5f)
                {
                    if (Mathf.Abs(j - t) < 1.25f)
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
        // x가 경계를, z는 중앙을 지날 때
        else if (i % 1.25f == 0 && j % 2.5f == 0)
        {
            Coordinate currentCord = new Coordinate();
            currentCord.x = i - 1.25f;
            currentCord.z = j;
            coordinates.Add(currentCord);

            Coordinate currentCord2 = new Coordinate();
            currentCord2.x = i + 1.25f;
            currentCord2.z = j;
            coordinates.Add(currentCord2);
        }
        // x가 경계를, z는 애매할 때
        else if (i % 1.25f == 0 && j % 1.25f != 0)
        {
            float newZPoint = 0;

            if (startPoint.z < endPoint.z)
            {
                for (float t = startPoint.z; t <= endPoint.z; t += 2.5f)
                {
                    if (Mathf.Abs(j - t) < 1.25f)
                    {
                        newZPoint = t;
                        break;
                    }
                }
            }
            else
            {
                for (float t = startPoint.z; t >= endPoint.z; t -= 2.5f)
                {
                    if (Mathf.Abs(j - t) < 1.25f)
                    {
                        newZPoint = t;
                        break;
                    }
                }
            }

            Coordinate currentCord = new Coordinate();
            currentCord.x = i - 1.25f;
            currentCord.z = newZPoint;
            coordinates.Add(currentCord);

            Coordinate currentCord2 = new Coordinate();
            currentCord2.x = i + 1.25f;
            currentCord2.z = newZPoint;
            coordinates.Add(currentCord2);
        }
        // 둘다 경계를 지날 때
        else if ((roadCountX != roadCountZ) && (i % 1.25f == 0) && (j % 1.25f == 0))
        {
            Check_Additonal_Else(i, j);
        }
    }

    private void Check_Test_Z(float j)
    {
        // 시작점과 끝점을 잇는 직선위의 점들만 검사
        float a = (endPoint.z - startPoint.z) / (endPoint.x - startPoint.x);
        float b = startPoint.z - a * startPoint.x;
        float i = (j - b) / a;

        if (j % 1.25f == 0 && (j / 1.25f) % 2 != 0 && i % 1.25f != 0)
        {
            float newXPoint = 0;

            if (startPoint.x < endPoint.x)
            {
                for (float t = startPoint.x; t <= endPoint.x; t += 2.5f)
                {
                    if (Mathf.Abs(i - t) < 1.25f)
                    {
                        newXPoint = t;
                        break;
                    }
                }
            }
            else
            {
                for (float t = startPoint.x; t >= endPoint.x; t -= 2.5f)
                {
                    if (Mathf.Abs(i - t) < 1.25f)
                    {
                        newXPoint = t;
                        break;
                    }
                }
            }

            Coordinate currentCord = new Coordinate();
            currentCord.x = newXPoint;
            currentCord.z = j - 1.25f;
            coordinates.Add(currentCord);

            Coordinate currentCord2 = new Coordinate();
            currentCord2.x = newXPoint;
            currentCord2.z = j + 1.25f;
            coordinates.Add(currentCord2);
        }
    }

    private void Check_Straight(float i, float j)
    {
        if (i % 2.5f == 0 && j % 2.5f == 0) // 점이 칸의 정 중앙을 지날 때 해당 칸 체크
        {
            Coordinate currentCord = new Coordinate();
            currentCord.x = i;
            currentCord.z = j;
            coordinates.Add(currentCord);
        }
    }

    private void Check_Additonal_Diagonal()
    {
        // ( ↘ 방향 )
        if (startPoint.x < endPoint.x && startPoint.z > endPoint.z)
        {
            for (int n = 0; n < roadCountX - 1; n++)
            {
                Coordinate currentCord = new Coordinate();
                currentCord.x = startPoint.x + 2.5f * (n + 1);
                currentCord.z = startPoint.z - 2.5f * n;
                coordinates.Add(currentCord);
            }
        }
        // ( ↗ 방향 )
        else if (startPoint.x < endPoint.x && startPoint.z < endPoint.z)
        {
            for (int n = 0; n < roadCountX - 1; n++)
            {
                Coordinate currentCord = new Coordinate();
                currentCord.x = startPoint.x + 2.5f * (n + 1);
                currentCord.z = startPoint.z + 2.5f * n;
                coordinates.Add(currentCord);
            }
        }
        // ( ↖ 방향 )
        else if (startPoint.x > endPoint.x && startPoint.z < endPoint.z)
        {
            for (int n = 0; n < roadCountX - 1; n++)
            {
                Coordinate currentCord = new Coordinate();
                currentCord.x = startPoint.x - 2.5f * (n + 1);
                currentCord.z = startPoint.z + 2.5f * n;
                coordinates.Add(currentCord);
            }
        }
        // ( ↙ 방향 )
        else if (startPoint.x > endPoint.x && startPoint.z > endPoint.z)
        {
            for (int n = 0; n < roadCountX - 1; n++)
            {
                Coordinate currentCord = new Coordinate();
                currentCord.x = startPoint.x - 2.5f * (n + 1);
                currentCord.z = startPoint.z - 2.5f * n;
                coordinates.Add(currentCord);
            }
        }
    }

    private void Check_Additonal_Else(float i, float j)
    {
        // ( ↘ 방향 )
        if (startPoint.x < endPoint.x && startPoint.z > endPoint.z)
        {
            //Debug.Log("↘");
            Coordinate currentCord = new Coordinate();
            currentCord.x = i + 1.25f;
            currentCord.z = j + 1.25f;
            coordinates.Add(currentCord);
        }
        // ( ↗ 방향 )
        else if (startPoint.x < endPoint.x && startPoint.z < endPoint.z)
        {
            //Debug.Log("↗");
            Coordinate currentCord = new Coordinate();
            currentCord.x = i - 1.25f;
            currentCord.z = j + 1.25f;
            coordinates.Add(currentCord);
        }
        // ( ↖ 방향 )
        else if (startPoint.x > endPoint.x && startPoint.z < endPoint.z)
        {
            //Debug.Log("↖");
            Coordinate currentCord = new Coordinate();
            currentCord.x = i + 1.25f;
            currentCord.z = j + 1.25f;
            coordinates.Add(currentCord);
        }
        // ( ↙ 방향 )
        else if (startPoint.x > endPoint.x && startPoint.z > endPoint.z)
        {
            //Debug.Log("↙");
            Coordinate currentCord = new Coordinate();
            currentCord.x = i - 1.25f;
            currentCord.z = j + 1.25f;
            coordinates.Add(currentCord);
        }
    }

    private void SpawnRoad(List<Coordinate> coordinates, bool isTest) // 저장된 위치마다 도로를 생성하는 메서드
    {
        for (int i = 0; i < coordinates.Count; i++)
        {
            if (!roads.Contains(coordinates[i])) // 해당 위치에 이미 도로가 생성되어있는지 체크
            {
                roads.Add(coordinates[i]);

                GameObject currentRoad;

                if (isTest)
                {
                    currentRoad = Instantiate(tempRoadPrefab, new Vector3(coordinates[i].x, 0.01f, coordinates[i].z), Quaternion.identity);
                    currentRoad.transform.SetParent(tempParent);
                }
                else
                {
                    currentRoad = Instantiate(roadPrefab, new Vector3(coordinates[i].x, 0.01f, coordinates[i].z), Quaternion.identity);
                    currentRoad.transform.SetParent(roadParent);
                }
                
                currentRoad.GetComponent<Road>().coordinate.x = coordinates[i].x;
                currentRoad.GetComponent<Road>().coordinate.z = coordinates[i].z;
            }
        }

        coordinates.Clear();
    }

    public bool CheckRoadExist(float x, float z)
    {
        for (int i = 0; i < roads.Count; i++)
        {
            if (roads[i].x == x && roads[i].z == z)
            {
                return true;
            }
        }

        return false;
    }

    public Road ReturnRoadAtPoint(float x, float z)
    {
        GameObject roads = GameObject.FindGameObjectWithTag("Roads");

        for (int i = 0; i < roads.transform.childCount; i++)
        {
            Road road = roads.transform.GetChild(i).GetComponent<Road>();

            if (road.coordinate.x == x && road.coordinate.z == z)
            {
                return road;
            }
        }

        return null;
    }

    private void DeleteRoad(float x, float z) // 해당 좌표를 roads에서 제거
    {
        for (int i = 0; i < roads.Count; i++)
        {
            if (roads[i].x == x && roads[i].z == z)
            {
                roads.Remove(roads[i]);
                return;
            }
        }
    }
}
