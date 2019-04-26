using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConversationPage
{
    public int index;
    public string content;
    public List<ConversationOption> options;
    public int nextPageIndex;

    public ConversationPage(
        int index,
        string content,
        List<ConversationOption> options = null, 
        int nextPageIndex = -1)
    {
        this.index = index;
        this.content = content;
        this.options = options;
        this.nextPageIndex = nextPageIndex;
    }
}

public class ConversationOption
{
    public string option;
    public int nextPageIndex;
    
    public ConversationOption(string option, int nextPageIndex = -1)
    {
        this.option = option;
        this.nextPageIndex = nextPageIndex;
    }
}

public class ConversationOnGoingException : System.Exception
{
    public ConversationOnGoingException(string message): base(message)
    {
    }
}

public class InteractiveConversationSystem : MonoBehaviour
{
    private GameObject conversation;
    private Text conversationTitle;
    private Text conversationContent;
    private GameObject[] conversationOptions;
    private GameObject conversationNext;

    private ConversationPage[] pages;
    private int currentPageIndex;
    private int[] selectedOptions;

    void Start()
    {
        conversation = transform.Find("Conversation").gameObject;
        conversationTitle = 
            conversation.transform.Find("ConversationTitle").GetComponent<Text>();
        conversationContent = 
            conversation.transform.Find("ConversationContent").GetComponent<Text>();
        conversationOptions = new GameObject[4];
        conversationOptions[0] = 
            conversation.transform.Find("ConversationOption0").gameObject;
        conversationOptions[1] = 
            conversation.transform.Find("ConversationOption1").gameObject;
        conversationOptions[2] = 
            conversation.transform.Find("ConversationOption2").gameObject;
        conversationOptions[3] = 
            conversation.transform.Find("ConversationOption3").gameObject;
        conversationNext = conversation.transform.Find("ConversationNext").gameObject;

        conversation.SetActive(false);
    }

    public void StartConversation(string title, ConversationPage[] pages, int startPageIndex = 0)
    {
        if (conversation.activeSelf)
        {
            throw new ConversationOnGoingException(
                "Can't initiate conversation while another one is in process.");
        }
        // Stop time flow
        Time.timeScale = 0;
        ActionInputUtils.SetActionInputEnable(false);
        // Set up fields for this conversation
        selectedOptions = new int[pages.Length];
        for (int i = 0; i < selectedOptions.Length; i ++)
        {
            selectedOptions[i] = -1;
        }
        this.pages = pages;
        conversationTitle.text = title;
        // Set up page display
        showPage(startPageIndex);
        // Set active
        conversation.SetActive(true);
    }

    public void SelectOption(int optionNumber)
    {
        selectedOptions[currentPageIndex] = optionNumber;
        showPage(pages[currentPageIndex].options[optionNumber].nextPageIndex);
    }

    public void NextPage()
    {
        showPage(pages[currentPageIndex].nextPageIndex);
    }

    private void endConversation()
    {
        Time.timeScale = 1;
        ActionInputUtils.SetActionInputEnable(true);
        conversation.SetActive(false);
    }

    private void showPage(int pageIndex)
    {
        if (pageIndex < 0)
        {
            endConversation();
            return;
        }
        currentPageIndex = pageIndex;
        conversationContent.text = pages[pageIndex].content;
        ConversationPage currentPage = pages[currentPageIndex];
        // Show and hide options
        for (int i = 0; i < 4; i++)
        {
            if (currentPage.options != null && i < currentPage.options.Count)
            {
                conversationOptions[i].SetActive(true);
                conversationOptions[i].transform.Find("Text").GetComponent<Text>().text = 
                    currentPage.options[i].option;
            }
            else
            {
                conversationOptions[i].SetActive(false);
            }

        }
        // Show and hide next page
        conversationNext.SetActive(currentPage.options == null || currentPage.nextPageIndex >= 0);
    }
}
