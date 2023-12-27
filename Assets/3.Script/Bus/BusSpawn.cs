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

        routes[0] = new List<Coordinate>();
        routes[1] = new List<Coordinate>();

        route_final = new List<Coordinate>();
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
    private List<Coordinate> bfsRoute = new List<Coordinate>();

    // DFS
    private List<Coordinate> route_final;
    private List<Coordinate>[] routes = new List<Coordinate>[2];
    private Coordinate[] points = new Coordinate[2];
    private int findCount = 0;

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

                    //BFS
                    FindNearestPoints(hit.collider.transform.position.x, hit.collider.transform.position.z);
                    // ���� bfsRoute�� DFS�Ͽ� ��Ʈ ����

                    //DFS
                    //FindRoute(hit.collider.transform.position.x, hit.collider.transform.position.z);

                    MakeFinalRoute();
                    currentBus.GetComponent<BusControl>().points[0] = points[0];
                    currentBus.GetComponent<BusControl>().points[1] = points[1];
                    currentBus.transform.SetParent(transform);
                    for (int i = 0; i < route_final.Count; i++)
                    {
                        currentBus.GetComponent<BusControl>().route.Add(route_final[i]);
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

    private void FindNearestPoints(float x, float z)
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

            bfsRoute.Add(currentCoordinate);

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

    private void FindRoute(float x, float z)
    {
        if (findCount >= 2)
        {
            return;
        }

        // �����¿� üũ�� �ε���
        float[] nx = { 0, 0, -2.5f, 2.5f };
        float[] nz = { -2.5f, 2.5f, 0, 0 };

        Coordinate currentCoordinate = new Coordinate(x, z);
        visited.Add(currentCoordinate, true);
        routes[findCount].Add(currentCoordinate);

        GameObject roads = GameObject.FindGameObjectWithTag("Roads");

        // BuildingTile���� üũ
        for (int i = 0; i < roads.transform.childCount; i++)
        {
            Road road = roads.transform.GetChild(i).GetComponent<Road>();

            if ((road.coordinate.x == currentCoordinate.x && road.coordinate.z == currentCoordinate.z) && road.roadType == RoadType.BuildingTile)
            {
                points[findCount].x = road.coordinate.x;
                points[findCount].z = road.coordinate.z;
                findCount += 1;
                return;
            }
        }

        for (int i = 0; i < 4; i++)
        {
            Coordinate nextCoordinate = new Coordinate(currentCoordinate.x + nx[i], currentCoordinate.z + nz[i]);

            if (RoadControl.instance.roads.Contains(nextCoordinate)) // ���� ��ġ�� ���ΰ� �����ϰ�
            {
                if (!visited.ContainsKey(nextCoordinate)) // �湮���� ���� ���ζ��
                {
                    FindRoute(nextCoordinate.x, nextCoordinate.z);
                }
            }
        }
    }

    private void MakeFinalRoute()
    {
        route_final.Clear();

        for (int i = routes[0].Count - 1; i >= 0; i--)
        {
            route_final.Add(routes[0][i]);
        }

        for (int i = 0; i < routes[1].Count; i++)
        {
            route_final.Add(routes[1][i]);
        }

        routes[0].Clear();
        routes[1].Clear();
        visited.Clear();
        findCount = 0;

        // ---------- bfsRoute ����

        
    }

    private void DFSForFinalRoute()
    {

    }

    public void CreateShowPrefab()
    {
        if (showPrefab == null)
        {
            showPrefab = Instantiate(showBusPrefab, Input.mousePosition, Quaternion.Euler(new Vector3(0, 90, 0)));
        }
    }
}
