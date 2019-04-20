using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCharaAttackWave : MonoBehaviour
{

    public float timeToLive = 0.5f;
    public float moveSpeed = 0.75f;
    public Vector3 direction = new Vector3(1, 0, 0);

    private float timeRemaining;
    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer> ();
        direction.Normalize();
        timeRemaining = timeToLive;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        timeRemaining -= Time.deltaTime;
        Debug.Log(timeRemaining);
        if (timeRemaining <= 0)
        {
            Destroy(this.gameObject);
        }
        float factor = (timeRemaining / timeToLive) * 0.5f + 0.5f;
        float tmpMoveSpeed = moveSpeed * factor;
        transform.Translate(direction * tmpMoveSpeed);
        spriteRenderer.color = new Color(1f, 1f, 1f, factor);
    }

    public void SetupDirection(Vector3 direction)
    {
        this.direction = direction;
    }
}
