using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCharaAttackWave : MonoBehaviour
{

    public int framesToLive = 5;
    public float moveSpeed = 0.75f;
    public Vector3 direction = new Vector3(1, 0, 0);

    private int framesRemaining;
    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer> ();
        direction.Normalize();
        framesRemaining = framesToLive;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        framesRemaining--;
        float factor = ((float)framesRemaining / (float)framesToLive) * 0.5f + 0.5f;
        float tmpMoveSpeed = moveSpeed * factor;
        transform.Translate(direction * tmpMoveSpeed);
        spriteRenderer.color = new Color(1f, 1f, 1f, factor);
        if (framesRemaining <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    public void SetupDirection(Vector3 direction)
    {
        this.direction = direction;
    }
}
