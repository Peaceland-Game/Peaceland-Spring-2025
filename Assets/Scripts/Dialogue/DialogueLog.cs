using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Yarn;
using Yarn.Unity;

public class DialogueLog : DialogueViewBase
{
    private List<DialogueEntry> dialogueHistory;
    private TextMeshProUGUI replayText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogueHistory = new List<DialogueEntry>();
    }

    //
    public void OnEnable()
    {
        CustomOptionsListView.OptionSelected += LogPlayerChoice;
    }

    public void OnDisable()
    {
        CustomOptionsListView.OptionSelected -= LogPlayerChoice;
    }

    // Update is called once per frame
    void Update()
    {
        //get the replay text box if necessary
        if (!replayText && GameManager.Instance.dialogueReplayActive)
        {
            replayText = GameObject.FindWithTag("DialogReplayBox").GetComponent<TextMeshProUGUI>();
        }
        //update the current text box
        else if(replayText)
        {
            //reset the text
            replayText.text = "";
            //loop through and add new lines
            for (int i = 0; i < dialogueHistory.Count; i++)
            {
                //if this is a selection, start italicize of the text
                if (dialogueHistory[i].isSelection)
                {
                    replayText.text += "<i><b>";
                }
                //only add the speaker if there is one
                if (dialogueHistory[i].speaker != null)
                {
                    replayText.text += dialogueHistory[i].speaker + ": ";
                }
                //add the remainder of the dialogue replay line
                replayText.text += dialogueHistory[i].text;
                //finish italicization
                if (dialogueHistory[i].isSelection)
                {
                    replayText.text += "</b></i>";
                }
                //end the line by moving to the next
                replayText.text += "\n";
            }
        }

    }

    public override void RunLine(LocalizedLine localizedLine, System.Action onLineFinished)
    {
        //create the new dialogue entry
        DialogueEntry newLine = new DialogueEntry
        {
            speaker = localizedLine.CharacterName,
            text = localizedLine.TextWithoutCharacterName.Text,
            isSelection = false
        };

        //log the line in the dialogue history
        dialogueHistory.Add(newLine);

        //complete the line through yarnspinner
        onLineFinished();
    }

    //to be called on the option selected event from OptionsListView.cs
    //adds a dialogue entry for the selected option
    private void LogPlayerChoice(DialogueOption selectedOption)
    {
        dialogueHistory.Add(new DialogueEntry
        {
            speaker = selectedOption.Line.CharacterName,
            text = selectedOption.Line.TextWithoutCharacterName.Text,
            isSelection = true
        });
    }
}

//dialogue entry class
public class DialogueEntry
{
    public string speaker;
    public string text;
    public bool isSelection;
}
