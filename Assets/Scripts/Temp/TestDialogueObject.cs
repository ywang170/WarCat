using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDialogueObject : MonoBehaviour
{
    public TextAsset dialogueYaml;
    private InteractiveConversationSystem interactiveConversationSystem;
    private ConversationPage[] loadedPages;
    // Start is called before the first frame update
    void Start()
    {
        
        interactiveConversationSystem = 
            GameObject
            .Find("InteractiveConversationSystem")
            .GetComponent<InteractiveConversationSystem>();
        loadedPages = DialogueXMLLoader.LoadDialogue(dialogueYaml);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            Debug.Log("Collided into enemy");
            List < ConversationOption > options = new List<ConversationOption>();
            ConversationOption option0 = new ConversationOption("Yes", 1);
            ConversationOption option1 = new ConversationOption("No", 2);
            ConversationOption option2 = new ConversationOption("Cancel", -1);
            ConversationOption option3 = new ConversationOption("Quit", -1);
            options.Add(option0);
            options.Add(option1);
            options.Add(option2);
            options.Add(option3);
            ConversationPage[] pages = new ConversationPage[3];
            pages[0] = new ConversationPage(0, "I have one favor to ask... ", null, 1);
            pages[1] = new ConversationPage(1, "Are you sure you want to do it? I need to double check", options, -1);
            pages[2] = new ConversationPage(2, "Seriously you don't want to do it? Let me ask again...", options, -1);
            interactiveConversationSystem.StartConversation("Mysteriou Cube", loadedPages);
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
