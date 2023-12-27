using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    private Queue<Coordinate> queue = new Queue<Coordinate>(); // 다음 방문할 좌표를 담을 큐
    private Dictionary<Coordinate, bool> visited = new Dictionary<Coordinate, bool>(); // 방문 체크용 딕셔너리

    [Header("System")]
    public float score = 0;
    public int roadCount = 0;
    public int busCount = 0;

    [Header("UI")]
    [SerializeField] private Text roadCountText;
    [SerializeField] private Text busCountText;
    [SerializeField] private Text scoreText;

    public bool CheckCanReach(float startPointX, float startPointZ, float endPointX, float endPointZ, List<Coordinate> coordinates, out int count)
    {
        // 상하좌우 체크용 인덱스
        float[] nx = { 0, 0, -2.5f, 2.5f };
        float[] nz = { -2.5f, 2.5f, 0, 0 };

        int moveCount = 0;

        // 시작지점 큐에 저장 후 시작
        queue.Enqueue(new Coordinate(startPointX, startPointZ));

        // BFS로 목적지까지 도로가 연결되어있는지 확인
        while (queue.Count > 0)
        {
            // 큐에서 좌표 꺼냄
            Coordinate currentCoordinate = queue.Dequeue();

            // 방문 체크
            visited[currentCoordinate] = true;

            moveCount += 1;

            // 목적지인지 체크
            if (currentCoordinate.x == endPointX && currentCoordinate.z == endPointZ)
            {
                // 도달 가능하면 return true
                queue.Clear();
                visited.Clear();
                count = moveCount;
                return true;
            }

            for (int i = 0; i < 4; i++)
            {
                Coordinate nextCoordinate = new Coordinate(currentCoordinate.x + nx[i], currentCoordinate.z + nz[i]);

                if (coordinates.Contains(nextCoordinate)) // 다음 위치에 도로가 존재하고
                {
                    if (!visited.ContainsKey(nextCoordinate)) // 방문하지 않은 도로라면
                    {
                        queue.Enqueue(nextCoordinate); // 큐에 저장
                    }
                }
            }
        }

        queue.Clear();
        visited.Clear();
        count = moveCount;
        return false;
    }

    public void ChangeRoadCount(int count)
    {
        roadCount += count;
        roadCountText.text = $"{roadCount}";
    }

    public void ChangeBusCount(int count)
    {
        busCount += count;
        busCountText.text = $"{busCount}";
    }

    public void ChangeScore(float score)
    {
        this.score += score;
        scoreText.text = $"Score : {(int)score}";
    }
}
