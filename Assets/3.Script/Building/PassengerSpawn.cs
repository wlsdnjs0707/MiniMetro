using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct Passenger
{
    public BuildingType destination;

    public Passenger(BuildingType destination)
    {
        this.destination = destination;
    }
}

public class PassengerSpawn : MonoBehaviour
{
    public List<Passenger> passengers = new List<Passenger>();

    [Header("Passenger Spawn Info")]
    public int maxPassengerCount = 3;
    public float spawnTimer = 10.0f;

    [Header("UI")]
    public GameObject canvas;
    [SerializeField] private Sprite[] colors;
    private GameObject passengerPanel;
    private GameObject timerPanel;
    private GameObject warningPanel;

    [Header("Timer")]
    public float gameOverTime = 30.0f;
    private float gameOverTimer = 0;
    private bool isTimerOn = false;
    private Coroutine gameOverCoroutine;

    [Header("Status")]
    public bool isTransporting = false;

    private void Start()
    {
        if (canvas != null)
        {
            passengerPanel = canvas.transform.GetChild(1).gameObject;
            timerPanel = canvas.transform.GetChild(2).gameObject;
            warningPanel = canvas.transform.GetChild(0).gameObject;
        }
    }

    public void StartSpawn()
    {
        StartCoroutine(StartSpawn_co());
    }

    private IEnumerator StartSpawn_co()
    {
        float timer = spawnTimer;

        while (timer > 0)
        {
            if (passengers.Count < maxPassengerCount)
            {
                timer -= Time.deltaTime;

                if (isTimerOn)
                {
                    isTimerOn = false;
                }
            }
            else
            {
                if (gameOverCoroutine == null)
                {
                    gameOverCoroutine = StartCoroutine(GameOverTimer_co());
                }
            }
            
            yield return null;
        }

        SpawnPassenger();

        yield return StartSpawn_co();
    }

    private IEnumerator GameOverTimer_co()
    {
        StartCoroutine(Warning_co());

        if (timerPanel == null)
        {
            timerPanel = canvas.transform.GetChild(2).gameObject;
        }

        isTimerOn = true;

        timerPanel.SetActive(true);

        gameOverTimer = 0;

        while (gameOverTimer < gameOverTime)
        {
            gameOverTimer += Time.deltaTime;

            timerPanel.transform.GetChild(0).GetComponent<Image>().fillAmount = gameOverTimer / gameOverTime;

            yield return null;

            if (!isTimerOn)
            {
                timerPanel.SetActive(false);
                yield break;
            }
        }

        GameManager.instance.GameOver(transform.position);
    }

    private IEnumerator Warning_co()
    {
        if (warningPanel == null)
        {
            warningPanel = canvas.transform.GetChild(0).gameObject;
        }

        warningPanel.SetActive(true);

        float scale = 0.1f;

        Color newColor = warningPanel.GetComponent<Image>().color;

        float alpha = 1;

        while (true)
        {
            if (!warningPanel.activeSelf)
            {
                warningPanel.SetActive(true);
            }

            scale += Time.deltaTime * 3.0f;
            alpha -= Time.deltaTime * 1.25f;

            newColor.a = alpha;

            warningPanel.GetComponent<RectTransform>().localScale = new Vector3(scale, scale, 1);
            warningPanel.GetComponent<Image>().color = newColor;

            if (scale > 5.0f)
            {
                warningPanel.SetActive(false);
                scale = 0.1f;
                alpha = 1.0f;
                yield return new WaitForSeconds(3.0f);
            }

            yield return null;

            if (!isTimerOn)
            {
                warningPanel.SetActive(false);
                yield break;
            }
        }
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

        passengers.Add(new Passenger((BuildingType)destination));

        UpdatePassengerUI();
    }

    public void UpdatePassengerUI()
    {
        if (passengerPanel == null)
        {
            passengerPanel = canvas.transform.GetChild(1).gameObject;
        }

        for (int i = 0; i < maxPassengerCount; i++)
        {
            if (i < passengers.Count)
            {
                passengerPanel.transform.GetChild(i).gameObject.SetActive(true);
                passengerPanel.transform.GetChild(i).GetComponent<Image>().sprite = colors[(int)passengers[i].destination - 1];
            }
            else
            {
                passengerPanel.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
