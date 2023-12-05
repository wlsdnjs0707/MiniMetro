using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoadType
{
    Straight, // ���� ����
    Corner, // �ڳ� ����
    Intersection, // ������
    BuildingTile // �ǹ� �ؿ� ��ġ�� ����
}

public enum ClickState
{ 
    None, // �ʱ� ����
    Create // ���θ� �������� ����
}

public class RoadControl : MonoBehaviour
{
    [Header("Ground Layer")]
    public LayerMask groundLayer;

    [Header("Road")]
    [SerializeField] private GameObject[] roadPrefabs; // <Index> 0:Straight, 1:Corner, 2:Intersection, 3:BuildingTile

    [Header("State")]
    private ClickState currentClickState = ClickState.None;

    //private Vector3 mousePosition;

    private void Update()
    {
        GetMouseInput();
    }

    private void GetMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MouseClick();
        }
    }

    private void MouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Building"))
            {
                // �ǹ� Ŭ��

                if (currentClickState == ClickState.None)
                {
                    // ���� �׸��� ������ �ǹ� ���� (���� �׸��� ����)
                }
                else if (currentClickState == ClickState.Create)
                {
                    // ���� �׸��� ���� �ǹ� ���� (���� ����)
                }
            }
        }
    }
}
