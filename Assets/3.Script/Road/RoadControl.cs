using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public enum RoadType
{
    Straight, // ���� ����
    Corner, // �ڳ� ����
    Intersection, // ������
    BuildingTile, // �ǹ� �ؿ� ��ġ�� ����
    Test // �� ���� �⺻ Ÿ��
}

public enum ClickState
{ 
    None, // �ʱ� ����
    Create, // ���θ� �׸��� ��
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
            if (hit.collider.CompareTag("Road") && hit.collider.GetComponent<Road>().roadType == RoadType.BuildingTile) // ���콺 Ŭ��
            {
                if (GameManager.instance.roadCount > 0)
                {
                    if (currentClickState == ClickState.None) // ���� �׸��� ������ �ǹ� ���� (���� �׸��� ����)
                    {
                        currentClickState = ClickState.Create;
                        startPoint = hit.collider.gameObject.transform.position;
                        //Debug.Log("�׸��� ����");
                    }
                }
                else
                {
                    // UI ���� (���� ������ �����մϴ�)
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
            if (hit.collider.CompareTag("Road")) // ���콺 ����
            {
                endPoint = hit.collider.gameObject.transform.position;

                if (startPoint == endPoint)
                {
                    currentClickState = ClickState.None;
                    //Debug.Log("�׸��� ���");
                    return;
                }

                if (currentClickState == ClickState.Create) // ���θ� ������ �ǹ� ���� (���� ����)
                {
                    currentClickState = ClickState.None;

                    //Debug.Log("�׸��� �Ϸ�");

                    // temp �����
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
            else // �ǹ� �̿ܿ��� ���콺 ����
            {
                if (currentClickState == ClickState.Create)
                {
                    currentClickState = ClickState.None;
                    coordinates.Clear();
                    //Debug.Log("�׸��� ���");
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

                    // ����
                    if (!(startPoint.x == endPoint.x && startPoint.z == endPoint.z))
                    {
                        MeasureRoad(true);
                    }
                }
                else
                {
                    // temp �����
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

    private void Road_Optimize(int n) // start�� end�� ���� nĭ�� �̹� �̾��� ���ΰ� �ִ��� Ȯ���� ����
    {
        float startX = startPoint.x;
        float startZ = startPoint.z;

        float endX = endPoint.x;
        float endZ = endPoint.z;

        // ( �� ���� )
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
        // ( �� ���� )
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
        // ( �� ���� )
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
        // ( �� ���� )
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

    private void MeasureRoad(bool isTest) // startPoint ���� endPoint ���� ������ ������ ��ġ�� ����Ͽ� ����
    {
        // ���� �ϳ��� ���� ���� (���簢��)
        float roadWidth = 2.5f;

        // �ʿ� ���� ���� ���
        roadCountX = (int)(Mathf.Abs(startPoint.x - endPoint.x) / roadWidth) + 1;
        roadCountZ = (int)(Mathf.Abs(startPoint.z - endPoint.z) / roadWidth) + 1;

        #region ���⿡ ���� üũ
        // ( �� ���� )
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
        // ( �� ���� )
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
        // ( �� ���� )
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
        // ( �� ���� )
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
        // ( - ���� )
        // start.x < end.x
        else if (startPoint.x < endPoint.x && startPoint.z == endPoint.z)
        {
            for (float i = startPoint.x; i <= endPoint.x; i += 2.5f)
            {
                Check_Straight(i, startPoint.z);
            }
        }
        // ( - ���� )
        // start.x > end.x
        else if (startPoint.x > endPoint.x && startPoint.z == endPoint.z)
        {
            for (float i = startPoint.x; i >= endPoint.x; i -= 2.5f)
            {
                Check_Straight(i, startPoint.z);
            }
        }
        // ( �� ���� )
        // start.z < end.z
        else if (startPoint.z < endPoint.z && startPoint.x == endPoint.x)
        {
            for (float j = startPoint.z; j <= endPoint.z; j += 2.5f)
            {
                Check_Straight(startPoint.x, j);
            }
        }
        // ( �� ���� )
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

        // �� ���� ���ΰ� ������ ���� X
        if (GameManager.instance.CheckCanReach(startPoint.x, startPoint.z, endPoint.x, endPoint.z, roads, out int count_original))
        {
            // �� ����
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
        // �������� ������ �մ� �������� ���鸸 �˻�
        float a = (endPoint.z - startPoint.z) / (endPoint.x - startPoint.x);
        float b = startPoint.z - a * startPoint.x;
        float j = a * i + b;

        // x, z�� ��� �߾��� ���� ��
        if (i % 2.5f == 0 && j % 2.5f == 0)
        {
            Coordinate currentCord = new Coordinate();
            currentCord.x = i;
            currentCord.z = j;
            coordinates.Add(currentCord);
        }
        // x�� �߾���, z�� ��踦 ���� �� 
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
        // x�� �߾���, z�� �ָ��� ��
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
        // x�� ��踦, z�� �߾��� ���� ��
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
        // x�� ��踦, z�� �ָ��� ��
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
        // �Ѵ� ��踦 ���� ��
        else if ((roadCountX != roadCountZ) && (i % 1.25f == 0) && (j % 1.25f == 0))
        {
            Check_Additonal_Else(i, j);
        }
    }

    private void Check_Test_Z(float j)
    {
        // �������� ������ �մ� �������� ���鸸 �˻�
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
        if (i % 2.5f == 0 && j % 2.5f == 0) // ���� ĭ�� �� �߾��� ���� �� �ش� ĭ üũ
        {
            Coordinate currentCord = new Coordinate();
            currentCord.x = i;
            currentCord.z = j;
            coordinates.Add(currentCord);
        }
    }

    private void Check_Additonal_Diagonal()
    {
        // ( �� ���� )
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
        // ( �� ���� )
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
        // ( �� ���� )
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
        // ( �� ���� )
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
        // ( �� ���� )
        if (startPoint.x < endPoint.x && startPoint.z > endPoint.z)
        {
            //Debug.Log("��");
            Coordinate currentCord = new Coordinate();
            currentCord.x = i + 1.25f;
            currentCord.z = j + 1.25f;
            coordinates.Add(currentCord);
        }
        // ( �� ���� )
        else if (startPoint.x < endPoint.x && startPoint.z < endPoint.z)
        {
            //Debug.Log("��");
            Coordinate currentCord = new Coordinate();
            currentCord.x = i - 1.25f;
            currentCord.z = j + 1.25f;
            coordinates.Add(currentCord);
        }
        // ( �� ���� )
        else if (startPoint.x > endPoint.x && startPoint.z < endPoint.z)
        {
            //Debug.Log("��");
            Coordinate currentCord = new Coordinate();
            currentCord.x = i + 1.25f;
            currentCord.z = j + 1.25f;
            coordinates.Add(currentCord);
        }
        // ( �� ���� )
        else if (startPoint.x > endPoint.x && startPoint.z > endPoint.z)
        {
            //Debug.Log("��");
            Coordinate currentCord = new Coordinate();
            currentCord.x = i - 1.25f;
            currentCord.z = j + 1.25f;
            coordinates.Add(currentCord);
        }
    }

    private void SpawnRoad(List<Coordinate> coordinates, bool isTest) // ����� ��ġ���� ���θ� �����ϴ� �޼���
    {
        for (int i = 0; i < coordinates.Count; i++)
        {
            if (!roads.Contains(coordinates[i])) // �ش� ��ġ�� �̹� ���ΰ� �����Ǿ��ִ��� üũ
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

    private void DeleteRoad(float x, float z) // �ش� ��ǥ�� roads���� ����
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
