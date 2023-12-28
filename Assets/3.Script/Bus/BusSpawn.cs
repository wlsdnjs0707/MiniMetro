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
    private Queue<Coordinate> queue = new Queue<Coordinate>(); // ���� �湮�� ��ǥ�� ���� ť
    private Dictionary<Coordinate, bool> visited = new Dictionary<Coordinate, bool>(); // �湮 üũ�� ��ųʸ�
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
                    // ���� ����
                    GameObject currentBus = Instantiate(busPrefab, hit.collider.transform.position, Quaternion.identity);

                    // BFS�� ���� ����� �� �÷��� Ž��
                    FindNearestPoints(hit.collider.transform.position.x, hit.collider.transform.position.z);

                    // ���� �� ���� �ִ� ��θ� ����
                    visited.Clear();
                    temp.Clear();
                    route.Clear();
                    count = 0;
                    FindShortestRoute(points[0].x, points[0].z);

                    // ������ ������ �պ��� �� ������ �ִ� ��θ� �Ҵ�
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
        // �����¿� üũ�� �ε���
        float[] nx = { 0, 0, -2.5f, 2.5f };
        float[] nz = { -2.5f, 2.5f, 0, 0 };

        // �������� ť�� ���� �� ����
        queue.Enqueue(new Coordinate(x, z));

        int findCount = 0;

        GameObject roads = GameObject.FindGameObjectWithTag("Roads");

        // BFS�� ���� ����� �ǹ� ��ǥ üũ
        while (queue.Count > 0)
        {
            // ť���� ��ǥ ����
            Coordinate currentCoordinate = queue.Dequeue();

            // �湮 üũ
            visited[currentCoordinate] = true;

            // BuildingTile���� üũ
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

                if (RoadControl.instance.roads.Contains(nextCoordinate)) // ���� ��ġ�� ���ΰ� �����ϰ�
                {
                    if (!visited.ContainsKey(nextCoordinate)) // �湮���� ���� ���ζ��
                    {
                        queue.Enqueue(nextCoordinate); // ť�� ����
                    }
                }
            }
        }
    }

    private void FindShortestRoute(float x, float z) // �� ���� �ִ� ��� ���ϱ�
    {
        // points[0]���� points[1]������ �ִ� ��� ���ϱ�

        // �����¿� üũ�� �ε���
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

            if (RoadControl.instance.roads.Contains(nextCoordinate)) // ���� ��ġ�� ���ΰ� �����ϰ�
            {
                if (!visited.ContainsKey(nextCoordinate)) // �湮���� ���� ���ζ��
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
