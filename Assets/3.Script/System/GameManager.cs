using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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

    [Header("System")]
    public float score = 0;
    public int roadCount = 0;
    public int busCount = 0;
    private int passengerCount = 0;

    [Header("UI")]
    [SerializeField] private Text roadCountText;
    [SerializeField] private Text busCountText;
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private Text gameOverText;
    [SerializeField] private Volume volume;

    [Header("Camera")]
    [SerializeField] private GameObject mainCamera;

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
        passengerCount += 1;

        this.score += score;
        scoreText.text = $"Score : {(int)this.score}";

        if (this.score == 300)
        {
            BuildingSpawn.instance.SpawnBuilding();
            ChangeRoadCount(1);
            ChangeBusCount(1);
        }
        else if (this.score == 500)
        {
            BuildingSpawn.instance.SpawnBuilding();
            ChangeRoadCount(1);
            ChangeBusCount(1);
        }
        else if (this.score % 1000 == 0)
        {
            BuildingSpawn.instance.SpawnBuilding();
            ChangeRoadCount(1);
            ChangeBusCount(1);
        }
    }

    public void GameOver(Vector3 position)
    {
        StartCoroutine(GameOver_co(position));
    }

    private IEnumerator GameOver_co(Vector3 position)
    {
        Vignette vignette;
        volume.profile.TryGet(out vignette);
        float focusDistance = 10.0f;

        /*ClampedFloatParameter focalLength;
        volume.profile.TryGet(out focalLength);*/

        DepthOfField depthOfField;
        volume.profile.TryGet(out depthOfField);
        float intensity = 0;

        float timer = 5.0f;

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            mainCamera.transform.position = new Vector3(Mathf.Lerp(mainCamera.transform.position.x, position.x, Time.deltaTime), mainCamera.transform.position.y, Mathf.Lerp(mainCamera.transform.position.z, position.z - 13, Time.deltaTime));
            mainCamera.GetComponent<Camera>().orthographicSize = Mathf.Lerp(mainCamera.GetComponent<Camera>().orthographicSize, 5.0f, Time.deltaTime);

            if (focusDistance > 0.01f)
            {
                focusDistance -= Time.deltaTime;
            }
            depthOfField.focusDistance.Override(focusDistance);

            if (intensity < 0.35f)
            {
                intensity += Time.deltaTime;
            }
            vignette.intensity.Override(intensity);

            yield return null;
        }

        Time.timeScale = 0;
        UpdateGameOverUI();

        yield break;
    }

    private void UpdateGameOverUI()
    {
        gameOverUI.SetActive(true);
        gameOverText.text = $"High Score : {score}\n�� {passengerCount}���� �°��� ���θ� �̿��߽��ϴ�.";
    }
}
