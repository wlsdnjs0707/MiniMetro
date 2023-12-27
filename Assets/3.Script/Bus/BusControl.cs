using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusControl : MonoBehaviour
{
    public List<Coordinate> route = new List<Coordinate>();
    public Coordinate[] points = new Coordinate[2];

    public float busSpeed = 2.0f;

    private void Start()
    {
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

        while (index < route.Count)
        {
            Coordinate destination = route[index];

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
                isBackward = false;
                index = 1;
            }
            else if (transform.position.x == route[route.Count - 1].x && transform.position.z == route[route.Count - 1].z)
            {
                isBackward = true;
                index = route.Count - 2;
            }

            Coordinate destination = route[index];

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
}
