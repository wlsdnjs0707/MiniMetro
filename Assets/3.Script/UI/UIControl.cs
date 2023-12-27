using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIControl : MonoBehaviour
{
    [Header("Image")]
    [SerializeField] private Sprite normal;
    [SerializeField] private Sprite selected;

    [Header("Button")]
    [SerializeField] private Image roadButtonImage;
    [SerializeField] private Image busButtonImage;

    public void SelectRoadButton()
    {
        roadButtonImage.sprite = selected;
        busButtonImage.sprite = normal;

        RoadControl.instance.canDraw = true;
    }

    public void SelectBusButton()
    {
        if (GameManager.instance.busCount > 0)
        {
            roadButtonImage.sprite = normal;
            busButtonImage.sprite = selected;

            RoadControl.instance.canDraw = false;
            BusSpawn.instance.CreateShowPrefab();
            BusSpawn.instance.canBusSpawn = true;
        }
    }

    public void DisableBusButton()
    {
        busButtonImage.sprite = normal;
    }
}
