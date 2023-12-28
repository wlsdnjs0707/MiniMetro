using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BusControl : MonoBehaviour
{
    [Header("Bus Info")]
    public float busSpeed = 2.0f;
    public float waitTime = 1.0f;
    private WaitForSeconds wfs;
    public List<Coordinate> route = new List<Coordinate>();
    public Coordinate[] points = new Coordinate[2];
    public BuildingType[] buildingTypes = new BuildingType[2];

    [Header("UI")]
    [SerializeField] private GameObject canvas;
    [SerializeField] private Sprite[] colors;

    [Header("Passenger Info")]
    public int passengerCount = 0;
    public List<Passenger> passengers = new List<Passenger>();

    private void Start()
    {
        canvas = transform.GetChild(0).gameObject;

        wfs = new WaitForSeconds(waitTime);

        StartCoroutine(InitPatrol_co());
    }

    private IEnumerator InitPatrol_co()
    {
        int index = 0;

        for (int i = 0; i < route.Count; i++)
        {
            if (route[i].x == transform.position.x && route[i].z == transform.position.z)
            {
                index = i + 1;
            }
        }

        Coordinate destination = new Coordinate();

        while (index < route.Count)
        {
            destination = route[index];

            ChangeRotation(destination);

            while (true)
            {
                if (transform.position.x != destination.x)
                {
                    if (transform.position.x > destination.x)
                    {
                        transform.position -= new Vector3(0.01f, 0, 0) * busSpeed;
                    }
                    else
                    {
                        transform.position += new Vector3(0.01f, 0, 0) * busSpeed;
                    }
                }

                if (transform.position.z != destination.z)
                {
                    if (transform.position.z > destination.z)
                    {
                        transform.position -= new Vector3(0, 0, 0.01f) * busSpeed;
                    }
                    else
                    {
                        transform.position += new Vector3(0, 0, 0.01f) * busSpeed;
                    }
                }

                if (Mathf.Abs(destination.x - transform.position.x) < 0.01f && Mathf.Abs(destination.z - transform.position.z) < 0.01f)
                {
                    transform.position = new Vector3(destination.x, 0, destination.z);

                    index += 1;
                    break;
                }

                yield return null;
            }
        }

        // ½Â°´ Å¾½Â
        Road road = RoadControl.instance.ReturnRoadAtPoint(destination.x, destination.z);

        int passengerIndex = 0;

        bool myTurn = false;

        while (passengerIndex < road.gameObject.GetComponent<PassengerSpawn>().passengers.Count)
        {
            if (road.gameObject.GetComponent<PassengerSpawn>().isTransporting && !myTurn) // µ¿½Ã Á¢±Ù Á¦¾î
            {
                yield return wfs;
                continue;
            }
            else
            {
                myTurn = true;

                if (road.gameObject.GetComponent<PassengerSpawn>().passengers[passengerIndex].destination == buildingTypes[0] ||
                road.gameObject.GetComponent<PassengerSpawn>().passengers[passengerIndex].destination == buildingTypes[1])
                {
                    road.gameObject.GetComponent<PassengerSpawn>().isTransporting = true;

                    yield return wfs;
                    passengers.Add(road.gameObject.GetComponent<PassengerSpawn>().passengers[passengerIndex]);
                    road.gameObject.GetComponent<PassengerSpawn>().passengers.Remove(road.gameObject.GetComponent<PassengerSpawn>().passengers[passengerIndex]);
                    road.gameObject.GetComponent<PassengerSpawn>().UpdatePassengerUI();
                    UpdateBusUI();
                }
                else
                {
                    passengerIndex += 1;
                }
            }
        }

        road.gameObject.GetComponent<PassengerSpawn>().isTransporting = false;

        StartCoroutine(Patrol_co());
        yield break;
    }

    private IEnumerator Patrol_co()
    {
        bool isBackward = false;

        int index = 0;

        while (true)
        {
            if (transform.position.x == route[0].x && transform.position.z == route[0].z)
            {
                // ½Â°´ Å¾½Â or  ÇÏÂ÷
                Road road = RoadControl.instance.ReturnRoadAtPoint(route[0].x, route[0].z);

                int passengerIndex = 0;
                
                if (road.gameObject.GetComponent<PassengerSpawn>().isTransporting) // µ¿½Ã Á¢±Ù Á¦¾î
                {
                    yield return wfs;
                    continue;
                }
                else
                {
                    road.gameObject.GetComponent<PassengerSpawn>().isTransporting = true;

                    // ÇÏÂ÷
                    while (passengerIndex < passengers.Count)
                    {
                        if (passengers[passengerIndex].destination == buildingTypes[0])
                        {
                            yield return wfs;
                            passengers.Remove(passengers[passengerIndex]);
                            GameManager.instance.ChangeScore(100);
                            UpdateBusUI();
                        }
                        else
                        {
                            passengerIndex += 1;
                        }
                    }

                    passengerIndex = 0;

                    // Å¾½Â
                    while (passengerIndex < road.gameObject.GetComponent<PassengerSpawn>().passengers.Count)
                    {
                        if (road.gameObject.GetComponent<PassengerSpawn>().passengers[passengerIndex].destination == buildingTypes[1])
                        {
                            yield return wfs;
                            passengers.Add(road.gameObject.GetComponent<PassengerSpawn>().passengers[passengerIndex]);
                            road.gameObject.GetComponent<PassengerSpawn>().passengers.Remove(road.gameObject.GetComponent<PassengerSpawn>().passengers[passengerIndex]);
                            road.gameObject.GetComponent<PassengerSpawn>().UpdatePassengerUI();
                            UpdateBusUI();
                        }
                        else
                        {
                            passengerIndex += 1;
                        }
                    }
                }

                road.gameObject.GetComponent<PassengerSpawn>().isTransporting = false;
                isBackward = false;
                index = 1;
            }
            else if (transform.position.x == route[route.Count - 1].x && transform.position.z == route[route.Count - 1].z)
            {
                // ½Â°´ Å¾½Â or  ÇÏÂ÷
                Road road = RoadControl.instance.ReturnRoadAtPoint(route[route.Count - 1].x, route[route.Count - 1].z);

                int passengerIndex = 0;

                if (road.gameObject.GetComponent<PassengerSpawn>().isTransporting) // µ¿½Ã Á¢±Ù Á¦¾î
                {
                    yield return wfs;
                    continue;
                }
                else
                {
                    road.gameObject.GetComponent<PassengerSpawn>().isTransporting = true;

                    // ÇÏÂ÷
                    while (passengerIndex < passengers.Count)
                    {
                        if (passengers[passengerIndex].destination == buildingTypes[1])
                        {
                            yield return wfs;
                            passengers.Remove(passengers[passengerIndex]);
                            GameManager.instance.ChangeScore(100);
                            UpdateBusUI();
                        }
                        else
                        {
                            passengerIndex += 1;
                        }
                    }

                    passengerIndex = 0;

                    // Å¾½Â
                    while (passengerIndex < road.gameObject.GetComponent<PassengerSpawn>().passengers.Count)
                    {
                        if (road.gameObject.GetComponent<PassengerSpawn>().passengers[passengerIndex].destination == buildingTypes[0])
                        {
                            yield return wfs;
                            passengers.Add(road.gameObject.GetComponent<PassengerSpawn>().passengers[passengerIndex]);
                            road.gameObject.GetComponent<PassengerSpawn>().passengers.Remove(road.gameObject.GetComponent<PassengerSpawn>().passengers[passengerIndex]);
                            road.gameObject.GetComponent<PassengerSpawn>().UpdatePassengerUI();
                            UpdateBusUI();
                        }
                        else
                        {
                            passengerIndex += 1;
                        }
                    }
                }

                road.gameObject.GetComponent<PassengerSpawn>().isTransporting = false;
                isBackward = true;
                index = route.Count - 2;
            }

            Coordinate destination = route[index];

            ChangeRotation(destination);

            if (isBackward)
            {
                while (true)
                {
                    if (transform.position.x != destination.x)
                    {
                        if (transform.position.x > destination.x)
                        {
                            transform.position -= new Vector3(0.01f, 0, 0) * busSpeed;
                        }
                        else
                        {
                            transform.position += new Vector3(0.01f, 0, 0) * busSpeed;
                        }
                    }

                    if (transform.position.z != destination.z)
                    {
                        if (transform.position.z > destination.z)
                        {
                            transform.position -= new Vector3(0, 0, 0.01f) * busSpeed;
                        }
                        else
                        {
                            transform.position += new Vector3(0, 0, 0.01f) * busSpeed;
                        }
                    }

                    if (Mathf.Abs(destination.x - transform.position.x) < 0.01f && Mathf.Abs(destination.z - transform.position.z) < 0.01f)
                    {
                        transform.position = new Vector3(destination.x, 0, destination.z);
                        index -= 1;
                        break;
                    }

                    yield return null;
                }
            }
            else
            {
                while (true)
                {
                    if (transform.position.x != destination.x)
                    {
                        if (transform.position.x > destination.x)
                        {
                            transform.position -= new Vector3(0.01f, 0, 0) * busSpeed;
                        }
                        else
                        {
                            transform.position += new Vector3(0.01f, 0, 0) * busSpeed;
                        }
                    }

                    if (transform.position.z != destination.z)
                    {
                        if (transform.position.z > destination.z)
                        {
                            transform.position -= new Vector3(0, 0, 0.01f) * busSpeed;
                        }
                        else
                        {
                            transform.position += new Vector3(0, 0, 0.01f) * busSpeed;
                        }
                    }

                    if (Mathf.Abs(destination.x - transform.position.x) < 0.01f && Mathf.Abs(destination.z - transform.position.z) < 0.01f)
                    {
                        transform.position = new Vector3(destination.x, 0, destination.z);
                        index += 1;
                        break;
                    }

                    yield return null;
                }
            }
        }

    }

    private void ChangeRotation(Coordinate nextPosition)
    {
        if (transform.position.x == nextPosition.x)
        {
            // »óÇÏ ÀÌµ¿
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
        else if (transform.position.z == nextPosition.z)
        {
            // ÁÂ¿ì ÀÌµ¿
            transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
        }

        canvas.GetComponent<RectTransform>().localRotation = transform.rotation;
    }

    private void UpdateBusUI()
    {
        for (int i = 0; i < 3; i++)
        {
            if (i < passengers.Count)
            {
                canvas.transform.GetChild(0).transform.GetChild(i).gameObject.SetActive(true);
                canvas.transform.GetChild(0).transform.GetChild(i).GetComponent<Image>().sprite = colors[(int)passengers[i].destination - 1];
            }
            else
            {
                canvas.transform.GetChild(0).transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
