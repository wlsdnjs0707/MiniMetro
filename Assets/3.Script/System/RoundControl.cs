using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundControl : MonoBehaviour
{
    private void Start()
    {
        BuildingSpawn.instance.SpawnBuilding();
        BuildingSpawn.instance.SpawnBuilding();

        GameManager.instance.roadCount += 1;
    }

    private void StartRound()
    {
        BuildingSpawn.instance.SpawnBuilding();
    }
}
