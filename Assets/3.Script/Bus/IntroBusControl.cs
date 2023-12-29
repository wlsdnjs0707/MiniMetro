using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroBusControl : MonoBehaviour
{
    private bool goLeft = false;

    private void Update()
    {
        if (goLeft)
        {
            transform.position -= new Vector3(0.01f, 0, 0);
        }
        else
        {
            transform.position += new Vector3(0.01f, 0, 0);
        }

        if (transform.position.x < -8.75f)
        {
            goLeft = false;
        }

        if (transform.position.x > 8.75f)
        {
            goLeft = true;
        }
    }
}
