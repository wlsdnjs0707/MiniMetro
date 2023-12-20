using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    Create // ���θ� �׸��� ��
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
    [SerializeField] private GameObject[] roadPrefabs; // <Index> 0:Straight, 1:Corner, 2:Intersection, 3:BuildingTile

    [Header("Click State")]
    public ClickState currentClickState = ClickState.None;

    [Header("Point")]
    //private Vector3 mousePosition;
    public Vector3 startPoint = Vector3.zero;
    public Vector3 endPoint = Vector3.zero;

    [Header("Spawn Road")]
    public List<Coordinate> roads = new List<Coordinate>();
    private List<Coordinate> coordinates = new List<Coordinate>();
    private int roadCountX = 0;
    private int roadCountZ = 0;

    

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
            if (hit.collider.CompareTag("Building")) // �ǹ� ������ ���콺 Ŭ��
            {
                if (currentClickState == ClickState.None) // ���� �׸��� ������ �ǹ� ���� (���� �׸��� ����)
                {
                    currentClickState = ClickState.Create;
                    startPoint = hit.collider.gameObject.transform.position;
                    //Debug.Log("�׸��� ����");
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
            if (hit.collider.CompareTag("Building")) // �ǹ� ������ ���콺 ����
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
                    MeasureRoad();
                }
            }
            else // �ǹ� �̿ܿ��� ���콺 ����
            {
                if (currentClickState == ClickState.Create)
                {
                    currentClickState = ClickState.None;
                    //Debug.Log("�׸��� ���");
                }
            }
        }
    }

    private void MeasureRoad() // startPoint ���� endPoint ���� ������ ������ ��ġ�� ����Ͽ� ����
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

        SpawnRoad(coordinates);
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
        else if (i % 1.25f == 0 && j % 1.25f == 0)
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
        else if (startPoint.x > endPoint.x && startPoint.z > endPoint.z)
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
        else if (startPoint.x > endPoint.x && startPoint.z < endPoint.z)
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

    private void SpawnRoad(List<Coordinate> coordinates) // ����� ��ġ���� ���θ� �����ϴ� �޼���
    {
        for (int i = 0; i < coordinates.Count; i++)
        {
            if (!roads.Contains(coordinates[i])) // �ش� ��ġ�� �̹� ���ΰ� �����Ǿ��ִ��� üũ
            {
                roads.Add(coordinates[i]);
                GameObject currentRoad = Instantiate(roadPrefabs[4], new Vector3(coordinates[i].x, 0.01f, coordinates[i].z), Quaternion.identity);
                currentRoad.GetComponent<Road>().coordinate.x = coordinates[i].x;
                currentRoad.GetComponent<Road>().coordinate.z = coordinates[i].z;
            }
        }
        
        coordinates.Clear();
    }
}
