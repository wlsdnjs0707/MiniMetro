using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingType
{
    None,
    Red,
    Blue,
    Green,
    Yellow
}

public class BuildingSpawn : MonoBehaviour
{
    public static BuildingSpawn instance;

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

    [Header("Spawn Setting")]
    public Coordinate startSpawnPosition;
    public Coordinate endSpawnPosition;
    public int spawnDistance = 10;

    [Header("Prefab")]
    [SerializeField] GameObject[] buildingPrefabs;
    [SerializeField] GameObject roadPrefab;

    [Header("Parents")]
    [SerializeField] private Transform buildingParent;
    [SerializeField] private Transform roadParent;

    public Dictionary<BuildingType, int> buildingInfo = new Dictionary<BuildingType, int>();

    private Coordinate spawnPoint;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SpawnBuilding();
        }
    }

    private void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            buildingInfo.Add((BuildingType)i, 0);
        }
    }

    public void SpawnBuilding()
    {
        int count = 0;

        while (count < 100)
        {
            spawnPoint.x = Random.Range((int)startSpawnPosition.x / spawnDistance, (int)endSpawnPosition.x / spawnDistance + 1) * spawnDistance;
            spawnPoint.z = Random.Range((int)startSpawnPosition.z / spawnDistance, (int)endSpawnPosition.z / spawnDistance + 1) * spawnDistance;

            if (!RoadControl.instance.CheckRoadExist(spawnPoint.x, spawnPoint.z))
            {
                break;
            }

            count += 1;
        }

        if (count >= 100)
        {
            return;
        }

        int type = Random.Range(1, 5);

        // 건물
        GameObject currentBuilding = Instantiate(buildingPrefabs[type - 1], new Vector3(spawnPoint.x, 0, spawnPoint.z), Quaternion.identity);
        currentBuilding.transform.SetParent(buildingParent);

        // 도로
        GameObject currentRoad = Instantiate(roadPrefab, new Vector3(spawnPoint.x, 0.01f, spawnPoint.z), Quaternion.identity);
        currentRoad.transform.SetParent(roadParent);
        currentRoad.GetComponent<Road>().roadType = RoadType.BuildingTile;
        currentRoad.GetComponent<Road>().coordinate.x = spawnPoint.x;
        currentRoad.GetComponent<Road>().coordinate.z = spawnPoint.z;
        currentRoad.GetComponent<Road>().buildingType = (BuildingType)type;
        currentRoad.GetComponent<PassengerSpawn>().StartSpawn();
        RoadControl.instance.roads.Add(new Coordinate(spawnPoint.x, spawnPoint.z));

        currentRoad.GetComponent<PassengerSpawn>().canvas = currentBuilding.transform.GetChild(0).gameObject;

        buildingInfo[(BuildingType)type] += 1;
    }
}
