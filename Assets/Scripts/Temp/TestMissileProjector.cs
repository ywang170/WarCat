using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMissileProjector : MonoBehaviour
{
    private float moveSpeed = 0.1f;
    private float minXValue = -10f;


    private void FixedUpdate()
    {
        if (gameObject.transform.position.x < minXValue)
        {
            gameObject.transform.position = new Vector3(10,-2 ,0);
        } else
        {
            gameObject.transform.Translate(Vector3.left * moveSpeed);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            BattleObject target = collision.transform.GetComponent<BattleObject>();
            if (target != null)
            {
                target.TakeHit(new Hit(100, 100, true, -100, new Vector2(transform.position.x, transform.position.y), null));
            }
        }
    }
}
