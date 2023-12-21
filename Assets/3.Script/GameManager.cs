using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

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

    // BFS
    private Queue<Coordinate> queue = new Queue<Coordinate>(); // ���� �湮�� ��ǥ�� ���� ť
    private Dictionary<Coordinate, bool> visited = new Dictionary<Coordinate, bool>(); // �湮 üũ�� ��ųʸ�

    public bool CheckCanReach(float startPointX, float startPointZ, float endPointX, float endPointZ, List<Coordinate> coordinates, out int count)
    {
        // �����¿� üũ�� �ε���
        float[] nx = { 0, 0, -2.5f, 2.5f };
        float[] nz = { -2.5f, 2.5f, 0, 0 };

        int moveCount = 0;

        // �������� ť�� ���� �� ����
        queue.Enqueue(new Coordinate(startPointX, startPointZ));

        // BFS�� ���������� ���ΰ� ����Ǿ��ִ��� Ȯ��
        while (queue.Count > 0)
        {
            // ť���� ��ǥ ����
            Coordinate currentCoordinate = queue.Dequeue();

            // �湮 üũ
            visited[currentCoordinate] = true;

            moveCount += 1;

            // ���������� üũ
            if (currentCoordinate.x == endPointX && currentCoordinate.z == endPointZ)
            {
                // ���� �����ϸ� return true
                queue.Clear();
                visited.Clear();
                count = moveCount;
                return true;
            }

            for (int i = 0; i < 4; i++)
            {
                Coordinate nextCoordinate = new Coordinate(currentCoordinate.x + nx[i], currentCoordinate.z + nz[i]);

                if (coordinates.Contains(nextCoordinate)) // ���� ��ġ�� ���ΰ� �����ϰ�
                {
                    if (!visited.ContainsKey(nextCoordinate)) // �湮���� ���� ���ζ��
                    {
                        queue.Enqueue(nextCoordinate); // ť�� ����
                    }
                }
            }
        }

        queue.Clear();
        visited.Clear();
        count = moveCount;
        return false;
    }
}
