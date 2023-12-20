using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road : MonoBehaviour
{
    [Header("Road Info")]
    public RoadType roadType;
    public Coordinate coordinate;

    [Header("Road Prefab")]
    [SerializeField] private Mesh[] roadMeshs; // <Index> 0:Straight, 1:Corner, 2:Intersection, 3:BuildingTile, 4:Test

    private void Update()
    {
        ChangeType();
    }

    private void ChangeType()
    {
        // 상하좌우 체크용 인덱스
        float[] nx = { 0, 0, -2.5f, 2.5f };
        float[] nz = { -2.5f, 2.5f, 0, 0 };

        // 연결된 도로 표시용 배열 (1: 연결)
        int[] connectedRoad = { 0, 0, 0, 0 };

        // 상하좌우 순회하면서 도로가 연결되어있는지 체크
        for (int i = 0; i < 4; i++)
        {
            Coordinate nextCoordinate = new Coordinate(coordinate.x + nx[i], coordinate.z + nz[i]);

            if (RoadControl.instance.roads.Contains(nextCoordinate))
            {
                connectedRoad[i] = 1;
            }
        }
        
        int roadCount = 0;
        for (int i = 0; i < connectedRoad.Length; i++)
        {
            if (connectedRoad[i] == 1)
            {
                roadCount += 1;
            }
        }

        // < 타입 결정 기준 >
        // 상하좌우에 도로가 하나만 연결되어 있음 -> BuildingTile
        if (roadCount == 1)
        {
            roadType = RoadType.BuildingTile;
        }
        // 도로가 두개 연결되어 있음 -> Straight or Corner
        else if (roadCount == 2)
        {
            // 상하 or 좌우로 연결되어 있음 -> Straight
            if ((connectedRoad[0] == 1 && connectedRoad[1] == 1) | (connectedRoad[2] == 1 && connectedRoad[3] == 1))
            {
                roadType = RoadType.Straight;

                // 좌, 우
                if (connectedRoad[2] == 1 && connectedRoad[3] == 1)
                {
                    transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));
                }
                
            }
            // 직각으로 연결되어 있음 -> Corner
            else
            {
                roadType = RoadType.Corner;

                // 상, 좌
                if (connectedRoad[0] == 1 && connectedRoad[2] == 1)
                {
                    transform.rotation = Quaternion.Euler(new Vector3(0, 270, 0));
                }
                // 상, 우
                else if (connectedRoad[0] == 1 && connectedRoad[3] == 1)
                {
                    transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                }
                // 하, 좌
                else if (connectedRoad[1] == 1 && connectedRoad[2] == 1)
                {
                    transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                }
                // 하, 우
                else if (connectedRoad[1] == 1 && connectedRoad[3] == 1)
                {
                    transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
                }
            }
        }
        // 도로가 세개 연결되어 있음 -> Intersection
        else if (roadCount == 3)
        {
            roadType = RoadType.Intersection;

            if (connectedRoad[0] == 0)
            {
                transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
            }
            else if (connectedRoad[1] == 0)
            {
                transform.rotation = Quaternion.Euler(new Vector3(0, 270, 0));
            }
            else if (connectedRoad[2] == 0)
            {
                transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
            }
            else if (connectedRoad[3] == 0)
            {
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            }
        }
        // 도로가 모두 연결되어 있음 -> Test
        else if (roadCount == 4)
        {
            roadType = RoadType.Test;
        }

        GetComponent<MeshFilter>().mesh = roadMeshs[(int)roadType];
    }
}
