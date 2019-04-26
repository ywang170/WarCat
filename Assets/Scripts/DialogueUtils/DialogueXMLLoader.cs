using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class DialogueXMLLoader
{
    public static ConversationPage[] LoadDialogue(TextAsset dialogueXml)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(dialogueXml.text);
        ConversationPage[] pages = 
            new ConversationPage[xmlDoc.DocumentElement.ChildNodes.Count];
        foreach(XmlNode pageNode in xmlDoc.DocumentElement.ChildNodes)
        {
            int index = 
                System.Convert.ToInt32(pageNode.Attributes["index"].Value);

            string content = pageNode["content"].InnerText;

            List<ConversationOption> options = null;
            if (pageNode["options"] != null)
            {
                options = new List<ConversationOption>();
                foreach(XmlNode optionNode in pageNode["options"].ChildNodes)
                {
                    int optionNextPageIndex =
                        optionNode.Attributes["nextPageIndex"] == null ?
                        -1 :
                        System.Convert.ToInt32(optionNode.Attributes["nextPageIndex"].Value);
                    ConversationOption option = new ConversationOption(optionNode.InnerText, optionNextPageIndex);
                    options.Add(option);
                }
            }

            int nextPageIndex = -1;
            if(pageNode["nextPageIndex"] != null)
            {
                nextPageIndex = 
                    System.Convert.ToInt32(pageNode["nextPageIndex"].InnerText);
            }

            pages[index] = new ConversationPage(index, content, options, nextPageIndex);
        }
        return pages;
    }
}
