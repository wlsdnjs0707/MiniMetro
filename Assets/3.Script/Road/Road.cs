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
        // �����¿� üũ�� �ε���
        float[] nx = { 0, 0, -2.5f, 2.5f };
        float[] nz = { -2.5f, 2.5f, 0, 0 };

        // ����� ���� ǥ�ÿ� �迭 (1: ����)
        int[] connectedRoad = { 0, 0, 0, 0 };

        // �����¿� ��ȸ�ϸ鼭 ���ΰ� ����Ǿ��ִ��� üũ
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

        // < Ÿ�� ���� ���� >
        // �����¿쿡 ���ΰ� �ϳ��� ����Ǿ� ���� -> BuildingTile
        if (roadCount == 1)
        {
            roadType = RoadType.BuildingTile;
        }
        // ���ΰ� �ΰ� ����Ǿ� ���� -> Straight or Corner
        else if (roadCount == 2)
        {
            // ���� or �¿�� ����Ǿ� ���� -> Straight
            if ((connectedRoad[0] == 1 && connectedRoad[1] == 1) | (connectedRoad[2] == 1 && connectedRoad[3] == 1))
            {
                roadType = RoadType.Straight;

                // ��, ��
                if (connectedRoad[2] == 1 && connectedRoad[3] == 1)
                {
                    transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));
                }
                
            }
            // �������� ����Ǿ� ���� -> Corner
            else
            {
                roadType = RoadType.Corner;

                // ��, ��
                if (connectedRoad[0] == 1 && connectedRoad[2] == 1)
                {
                    transform.rotation = Quaternion.Euler(new Vector3(0, 270, 0));
                }
                // ��, ��
                else if (connectedRoad[0] == 1 && connectedRoad[3] == 1)
                {
                    transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                }
                // ��, ��
                else if (connectedRoad[1] == 1 && connectedRoad[2] == 1)
                {
                    transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                }
                // ��, ��
                else if (connectedRoad[1] == 1 && connectedRoad[3] == 1)
                {
                    transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
                }
            }
        }
        // ���ΰ� ���� ����Ǿ� ���� -> Intersection
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
        // ���ΰ� ��� ����Ǿ� ���� -> Test
        else if (roadCount == 4)
        {
            roadType = RoadType.Test;
        }

        GetComponent<MeshFilter>().mesh = roadMeshs[(int)roadType];
    }
}
