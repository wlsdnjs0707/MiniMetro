using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusSpawn : MonoBehaviour
{
    public static BusSpawn instance;

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

    [Header("Prefab")]
    [SerializeField] private GameObject busPrefab;
    [SerializeField] private GameObject showBusPrefab;
    private GameObject showPrefab;

    [Header("Status")]
    public bool canBusSpawn = false;

    [Header("UI")]
    [SerializeField] private GameObject canvas;

    // BFS
    private Queue<Coordinate> queue = new Queue<Coordinate>(); // 다음 방문할 좌표를 담을 큐
    private Dictionary<Coordinate, bool> visited = new Dictionary<Coordinate, bool>(); // 방문 체크용 딕셔너리
    private Coordinate[] points = new Coordinate[2];

    private List<Coordinate> route = new List<Coordinate>();
    private List<Coordinate> temp = new List<Coordinate>();
    private int shortestLength = int.MaxValue;
    private int count = 0;

    private void Update()
    {
        if (canBusSpawn && showPrefab != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Ground") || hit.collider.CompareTag("Road"))
                {
                    showPrefab.transform.position = hit.point;
                }
            }

            if (Input.GetMouseButton(0))
            {
                if (hit.collider.CompareTag("Road"))
                {
                    // 버스 생성
                    GameObject currentBus = Instantiate(busPrefab, hit.collider.transform.position, Quaternion.identity);

                    // BFS로 가장 가까운 두 플랫폼 탐색
                    FindNearestPoints(hit.collider.transform.position.x, hit.collider.transform.position.z);

                    // 이후 두 점의 최단 경로를 구함
                    visited.Clear();
                    temp.Clear();
                    route.Clear();
                    count = 0;
                    FindShortestRoute(points[0].x, points[0].z);

                    // 생성한 버스에 왕복할 두 지점과 최단 경로를 할당
                    currentBus.GetComponent<BusControl>().points[0] = points[0];
                    currentBus.GetComponent<BusControl>().points[1] = points[1];

                    currentBus.GetComponent<BusControl>().buildingTypes[0] = RoadControl.instance.ReturnRoadAtPoint(points[0].x, points[0].z).gameObject.GetComponent<Road>().buildingType;
                    currentBus.GetComponent<BusControl>().buildingTypes[1] = RoadControl.instance.ReturnRoadAtPoint(points[1].x, points[1].z).gameObject.GetComponent<Road>().buildingType;

                    currentBus.transform.SetParent(transform);
                    for (int i = 0; i < route.Count; i++)
                    {
                        currentBus.GetComponent<BusControl>().route.Add(route[i]);
                    }

                    canvas.GetComponent<UIControl>().DisableBusButton();
                    GameManager.instance.ChangeBusCount(-1);
                    canBusSpawn = false;
                    Destroy(showPrefab);
                }
                else
                {
                    canvas.GetComponent<UIControl>().DisableBusButton();
                    canBusSpawn = false;
                    Destroy(showPrefab);
                }
            }
        }
    }

    private void FindNearestPoints(float x, float z) // BFS
    {
        // 상하좌우 체크용 인덱스
        float[] nx = { 0, 0, -2.5f, 2.5f };
        float[] nz = { -2.5f, 2.5f, 0, 0 };

        // 시작지점 큐에 저장 후 시작
        queue.Enqueue(new Coordinate(x, z));

        int findCount = 0;

        GameObject roads = GameObject.FindGameObjectWithTag("Roads");

        // BFS로 가장 가까운 건물 좌표 체크
        while (queue.Count > 0)
        {
            // 큐에서 좌표 꺼냄
            Coordinate currentCoordinate = queue.Dequeue();

            // 방문 체크
            visited[currentCoordinate] = true;

            // BuildingTile인지 체크
            for (int i = 0; i < roads.transform.childCount; i++)
            {
                Road road = roads.transform.GetChild(i).GetComponent<Road>();

                if ((road.coordinate.x == currentCoordinate.x && road.coordinate.z == currentCoordinate.z) && road.roadType == RoadType.BuildingTile)
                {
                    points[findCount].x = road.coordinate.x;
                    points[findCount].z = road.coordinate.z;
                    findCount += 1;
                    break;
                }
            }

            if (findCount == 2)
            {
                queue.Clear();
                visited.Clear();
                return;
            }

            for (int i = 0; i < 4; i++)
            {
                Coordinate nextCoordinate = new Coordinate(currentCoordinate.x + nx[i], currentCoordinate.z + nz[i]);

                if (RoadControl.instance.roads.Contains(nextCoordinate)) // 다음 위치에 도로가 존재하고
                {
                    if (!visited.ContainsKey(nextCoordinate)) // 방문하지 않은 도로라면
                    {
                        queue.Enqueue(nextCoordinate); // 큐에 저장
                    }
                }
            }
        }
    }

    private void FindShortestRoute(float x, float z) // 두 점의 최단 경로 구하기
    {
        // points[0]부터 points[1]까지의 최단 경로 구하기

        // 상하좌우 체크용 인덱스
        float[] nx = { 0, 0, -2.5f, 2.5f };
        float[] nz = { -2.5f, 2.5f, 0, 0 };

        Coordinate currentCoordinate = new Coordinate(x, z);
        visited.Add(currentCoordinate, true);
        temp.Add(currentCoordinate);
        count += 1;

        if (x == points[1].x && z == points[1].z)
        {
            if (count < shortestLength)
            {
                count = shortestLength;

                route.Clear();

                for (int i = 0; i < temp.Count; i++)
                {
                    route.Add(temp[i]);
                }
            }
        }

        for (int i = 0; i < 4; i++)
        {
            Coordinate nextCoordinate = new Coordinate(currentCoordinate.x + nx[i], currentCoordinate.z + nz[i]);

            if (RoadControl.instance.roads.Contains(nextCoordinate)) // 다음 위치에 도로가 존재하고
            {
                if (!visited.ContainsKey(nextCoordinate)) // 방문하지 않은 도로라면
                {
                    FindShortestRoute(nextCoordinate.x, nextCoordinate.z);
                }
            }
        }

        visited.Remove(currentCoordinate);
        temp.Remove(currentCoordinate);
        count -= 1;

    }

    public void CreateShowPrefab()
    {
        if (showPrefab == null)
        {
            showPrefab = Instantiate(showBusPrefab, Input.mousePosition, Quaternion.Euler(new Vector3(0, 90, 0)));
        }
    }
}
