using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class IntroUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject exitButton;

    public float f = 0.005f;

    private Color originalColor;
    private Color transparentColor;

    public void StartGame()
    {
        SceneManager.LoadScene("MainGame");
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private void Awake()
    {
        originalColor = startButton.transform.GetChild(0).GetComponent<Text>().color;
        transparentColor = startButton.transform.GetChild(0).GetComponent<Text>().color;
        transparentColor.a = 0.5f;
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            if (ColliderCheck() == 1)
            {
                if (startButton.GetComponent<RectTransform>().localScale.x < 1.1f)
                {
                    startButton.GetComponent<RectTransform>().localScale += new Vector3(f, f, 0);
                    exitButton.GetComponent<RectTransform>().localScale -= new Vector3(f, f, 0);

                    startButton.transform.GetChild(0).GetComponent<Text>().color = originalColor;
                    exitButton.transform.GetChild(0).GetComponent<Text>().color = transparentColor;
                }
            }
            else if (ColliderCheck() == 2)
            {
                if (exitButton.GetComponent<RectTransform>().localScale.x < 1.1f)
                {
                    startButton.GetComponent<RectTransform>().localScale -= new Vector3(f, f, 0);
                    exitButton.GetComponent<RectTransform>().localScale += new Vector3(f, f, 0);

                    startButton.transform.GetChild(0).GetComponent<Text>().color = transparentColor;
                    exitButton.transform.GetChild(0).GetComponent<Text>().color = originalColor;
                }
            }
        }
        else // Mouse Over False
        {
            if (startButton.GetComponent<RectTransform>().localScale.x < 1.0f)
            {
                startButton.GetComponent<RectTransform>().localScale += new Vector3(f, f, 0);
            }

            if (startButton.GetComponent<RectTransform>().localScale.x > 1.0f)
            {
                startButton.GetComponent<RectTransform>().localScale -= new Vector3(f, f, 0);
            }

            if (exitButton.GetComponent<RectTransform>().localScale.x < 1.0f)
            {
                exitButton.GetComponent<RectTransform>().localScale += new Vector3(f, f, 0);
            }

            if (exitButton.GetComponent<RectTransform>().localScale.x > 1.0f)
            {
                exitButton.GetComponent<RectTransform>().localScale -= new Vector3(f, f, 0);
            }

            startButton.transform.GetChild(0).GetComponent<Text>().color = originalColor;
            exitButton.transform.GetChild(0).GetComponent<Text>().color = originalColor;
        }
    }

    private int ColliderCheck()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        for (int i = 0; i < raycastResults.Count; i++)
        {
            if (raycastResults[i].gameObject.CompareTag("StartButton"))
            {
                return 1;
            }
            else if (raycastResults[i].gameObject.CompareTag("ExitButton"))
            {
                return 2;
            }
        }

        return 0;
    }
}
