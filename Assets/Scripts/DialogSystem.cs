using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogSystem : MonoBehaviour
{

    public Transform fixer;

    private struct Word
    {
        public string content;
        public float timeLeft;

        public Word(string c, float t)
        {
            content = c;
            timeLeft = t;
        }
    }

    private Word currentWord;
    private Queue<Word> words;


    void Start()
    {
        currentWord = new Word("", 0);
        words = new Queue<Word>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 ownerScreenPosition = Camera.main.WorldToScreenPoint(fixer.position);
        transform.position = ownerScreenPosition;
    }

    private void FixedUpdate()
    {
        if (currentWord.timeLeft > 0)
        {
            currentWord.timeLeft = Mathf.Max(0, currentWord.timeLeft - Time.deltaTime);
        }

        if (currentWord.timeLeft <= 0)
        {
            if (words.Count > 0)
            {
                currentWord = words.Dequeue();
            }
            else
            {
                currentWord = new Word("", 0);
            }
        }

        gameObject.GetComponent<Text>().text = currentWord.content;

    }

    public void SetNextWord(string content, float lastingTime, bool neglectable = false)
    {
        if (!neglectable || currentWord.content == "")
        {
            words.Enqueue(new Word("\"" + content + "\"", lastingTime));
        }
    }
}
