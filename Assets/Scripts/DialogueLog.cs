using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class DialogueLog : DialogueViewBase
{
    private List<DialogueEntry> dialogueHistory;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogueHistory = new List<DialogueEntry>();
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < dialogueHistory.Count; i++)
        {
            Debug.Log(dialogueHistory[i].speaker + ": " + dialogueHistory[i].text);
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
