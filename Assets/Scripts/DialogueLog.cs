using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Yarn.Unity;

public class DialogueLog : DialogueViewBase
{
    private List<DialogueEntry> dialogueHistory;
    [SerializeField] private TextMeshProUGUI replayText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogueHistory = new List<DialogueEntry>();
    }

    // Update is called once per frame
    void Update()
    {
        //get the replay text box
        if (!replayText)
        {
            replayText = GameObject.FindWithTag("DialogReplayBox").GetComponent<TextMeshProUGUI>();
        }
        //reset the text
        replayText.text = "";
        //loop through and add new lines
        for(int i = 0; i < dialogueHistory.Count; i++)
        {
            //only add the speaker if there is one
            if (dialogueHistory[i].speaker != null)
            {
                replayText.text += dialogueHistory[i].speaker + ": ";
            }
            //add the remainder of the dialogue replay line
            replayText.text += dialogueHistory[i].text + "\n";
         }
    }

    public override void RunLine(LocalizedLine localizedLine, System.Action onLineFinished)
    {
        //
        DialogueEntry newLine = new DialogueEntry
        {
            speaker = localizedLine.CharacterName,
            text = localizedLine.TextWithoutCharacterName.Text,
        };

        //log the line in the dialogue history
        dialogueHistory.Add(newLine);

        //complete the line through yarnspinner
        onLineFinished();
    }
}

//dialogue entry class
public class DialogueEntry
{
    public string speaker;
    public string text;
}
