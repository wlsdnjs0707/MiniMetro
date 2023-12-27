using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundControl : MonoBehaviour
{
    private void Start()
    {
        BuildingSpawn.instance.InitBuilding();
        GameManager.instance.ChangeRoadCount(2);
        GameManager.instance.ChangeBusCount(2);
    }

    private void StartRound()
    {
        BuildingSpawn.instance.SpawnBuilding();
    }
}
