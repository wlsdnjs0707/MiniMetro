using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct Passenger
{
    public float timer;
    public BuildingType destination;

    public Passenger(BuildingType destination, float timer)
    {
        this.timer = timer;
        this.destination = destination;
    }
}

public class PassengerSpawn : MonoBehaviour
{
    public List<Passenger> passengers = new List<Passenger>();

    [Header("Passenger Spawn Info")]
    public float passengerTimer = 30.0f;
    public float spawnTimer = 10.0f;

    [Header("UI")]
    public GameObject canvas;
    [SerializeField] private Sprite[] colors;

    public void StartSpawn()
    {
        StartCoroutine(StartSpawn_co());
    }

    private IEnumerator StartSpawn_co()
    {
        float timer = spawnTimer;

        while (timer > 0)
        {
            if (passengers.Count < 5)
            {
                timer -= Time.deltaTime;
            }
            
            yield return null;
        }

        SpawnPassenger();

        yield return StartSpawn_co();
    }

    public void SpawnPassenger()
    {
        int destination;

        int count = 0;

        while (true)
        {
            destination = UnityEngine.Random.Range(1, 5);

            if (BuildingSpawn.instance.buildingInfo[(BuildingType)destination] > 0 && (destination != (int)GetComponent<Road>().buildingType))
            {
                break;
            }

            count += 1;

            if (count > 30)
            {
                return;
            }
        }

        passengers.Add(new Passenger((BuildingType)destination, passengerTimer));

        // Update UI
        if (passengers.Count == 1)
        {
            canvas.transform.GetChild(0).gameObject.SetActive(true);
        }

        canvas.transform.GetChild(0).transform.GetChild(passengers.Count - 1).gameObject.SetActive(true);
        canvas.transform.GetChild(0).transform.GetChild(passengers.Count - 1).GetComponent<Image>().sprite = colors[destination - 1];
    }
}
