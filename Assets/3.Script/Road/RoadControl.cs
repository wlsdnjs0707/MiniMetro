using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoadType
{
    Straight, // 직진 도로
    Corner, // 코너 도로
    Intersection, // 교차로
    BuildingTile // 건물 밑에 설치될 도로
}

public enum ClickState
{ 
    None, // 초기 상태
    Create // 도로를 생성중인 상태
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
                // 건물 클릭

                if (currentClickState == ClickState.None)
                {
                    // 도로 그리기 시작할 건물 선택 (도로 그리기 시작)
                }
                else if (currentClickState == ClickState.Create)
                {
                    // 도로 그리기 끝낼 건물 선택 (도로 생성)
                }
            }
        }
    }
}
