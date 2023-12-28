using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseOver : MonoBehaviour
{
    [SerializeField] private GameObject buttonPanel;
    [SerializeField] private GameObject togglePanel;

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject() && ColliderCheck()) // Mouse Over True
        {
            if (buttonPanel.GetComponent<RectTransform>().position.y < 100)
            {
                buttonPanel.GetComponent<RectTransform>().localPosition += new Vector3(0, 5, 0);
                togglePanel.GetComponent<RectTransform>().localPosition -= new Vector3(0, 5, 0);
            }
        }
        else // Mouse Over False
        {
            if (buttonPanel.GetComponent<RectTransform>().position.y > -100)
            {
                buttonPanel.GetComponent<RectTransform>().localPosition -= new Vector3(0, 5, 0);
                togglePanel.GetComponent<RectTransform>().localPosition += new Vector3(0, 5, 0);
            }
        }
    }

    private bool ColliderCheck()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        for (int i = 0; i < raycastResults.Count; i++)
        {
            if (raycastResults[i].gameObject.CompareTag("ToggleCollider"))
            {
                return true;
            }
        }

        return false;
    }
}
