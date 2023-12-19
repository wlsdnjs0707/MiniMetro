using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoadType
{
    Straight, // ���� ����
    Corner, // �ڳ� ����
    Intersection, // ������
    BuildingTile // �ǹ� �ؿ� ��ġ�� ����
}

public enum ClickState
{ 
    None, // �ʱ� ����
    Create // ���θ� �׸��� ��
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
        float roadWidth = 5.0f;

        // �ʿ� ���� ���� ���
        roadCountX = (int)(Mathf.Abs(startPoint.x - endPoint.x) / roadWidth) + 1;
        roadCountZ = (int)(Mathf.Abs(startPoint.z - endPoint.z) / roadWidth) + 1;

        #region ���⿡ ���� üũ
        // ( \ ���� )
        // start.x < end.x
        // start.z > end.z
        if (startPoint.x < endPoint.x && startPoint.z > endPoint.z)
        {
            for (float i = startPoint.x; i <= endPoint.x; i += 2.5f)
            {
                Check_Test(i);
            }
        }
        // ( / ���� )
        // start.x < end.x
        // start.z < end.z
        else if (startPoint.x < endPoint.x && startPoint.z < endPoint.z)
        {
            for (float i = startPoint.x; i <= endPoint.x; i += 2.5f)
            {
                Check_Test(i);
            }
        }
        // ( \ ���� )
        // start.x > end.x
        // start.z > end.z
        else if (startPoint.x > endPoint.x && startPoint.z > endPoint.z)
        {
            for (float i = startPoint.x; i >= endPoint.x; i -= 2.5f)
            {
                Check_Test(i);
            }
        }
        // ( / ���� )
        // start.x > end.x
        // start.z < end.z
        else if (startPoint.x > endPoint.x && startPoint.z < endPoint.z)
        {
            for (float i = startPoint.x; i >= endPoint.x; i -= 2.5f)
            {
                Check_Test(i);
            }
        }
        // ( - ���� )
        // start.x < end.x
        else if (startPoint.x < endPoint.x && startPoint.z == endPoint.z)
        {
            for (float i = startPoint.x; i <= endPoint.x; i += 5.0f)
            {
                Check_Straight(i, startPoint.z);
            }
        }
        // ( - ���� )
        // start.x > end.x
        else if (startPoint.x > endPoint.x && startPoint.z == endPoint.z)
        {
            for (float i = startPoint.x; i >= endPoint.x; i -= 5.0f)
            {
                Check_Straight(i, startPoint.z);
            }
        }
        // ( �� ���� )
        // start.z < end.z
        else if (startPoint.z < endPoint.z && startPoint.x == endPoint.x)
        {
            for (float j = startPoint.z; j <= endPoint.z; j += 5.0f)
            {
                Check_Straight(startPoint.x, j);
            }
        }
        // ( �� ���� )
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
        // �������� ������ �մ� �������� ���鸸 �˻�
        float a = (endPoint.z - startPoint.z) / (endPoint.x - startPoint.x);
        float b = startPoint.z - a * startPoint.x;
        float j = a * i + b;

        // x, z�� ��� �߾��� ���� ��
        if (i % 5.0f == 0 && j % 5.0f == 0)
        {
            Coordinate currentCord = new Coordinate();
            currentCord.x = i;
            currentCord.z = j;
            coordinates.Add(currentCord);
        }
        // x�� �߾���, z�� ��踦 ���� �� 
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
        // x�� �߾���, z�� �ָ��� ��
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
        // x�� ��踦, z�� �ָ��� ��
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
        if (i % 5.0f == 0 && j % 5.0f == 0) // ���� ĭ�� �� �߾��� ���� �� �ش� ĭ üũ
        {
            Coordinate currentCord = new Coordinate();
            currentCord.x = i;
            currentCord.z = j;
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
                Instantiate(roadPrefabs[4], new Vector3(coordinates[i].x, 0.01f, coordinates[i].z), Quaternion.identity);
            }
        }
        
        coordinates.Clear();
    }
}
