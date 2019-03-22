using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialBackgroundStorm : MonoBehaviour
{

    public float moveSpeed = 0.1f;
    public float maxXValue = 25f;

    public Vector3 startPosition;

    private void FixedUpdate()
    {
        if (gameObject.transform.position.x > maxXValue)
        {
            gameObject.transform.position = startPosition;
        } else
        {
            gameObject.transform.Translate(Vector3.right * moveSpeed);
        }
    }
}
