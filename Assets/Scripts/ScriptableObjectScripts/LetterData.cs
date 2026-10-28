using System.Collections.Generic;
using UnityEngine;

public enum LetterMoodType
{
    Affectionate, // Pink Heart - Romantic, sweet, tender
    Melancholic,  // Green Leaf - Nostalgic, gentle, wistful, quiet
    Somber,       // Blue Moon - Dark, mysterious, solemn, grieving
    Passionate    // Orange Sun/Flame - Energetic, fiery, joyous, bold
}

[System.Serializable]
public class WordChoice
{
    public string word;
    public LetterMoodType mood = LetterMoodType.Affectionate;
    public int points = 1;

    public WordChoice() { }

    public WordChoice(string word, LetterMoodType mood, int points = 1)
    {
        this.word = word;
        this.mood = mood;
        this.points = points;
    }
}

[System.Serializable]
public class BlankData
{
    public string blankId;
    public string placeholder = "............";
    public List<WordChoice> choices = new List<WordChoice>();

    public BlankData() { }

    public BlankData(string blankId, string placeholder, params WordChoice[] wordChoices)
    {
        this.blankId = blankId;
        this.placeholder = placeholder;
        if (wordChoices != null)
        {
            choices.AddRange(wordChoices);
        }
    }
}

[CreateAssetMenu(fileName = "NewLetter", menuName = "Letter Game/Letter")]
public class LetterData : ScriptableObject
{
    public string letterName = "A Letter from the Orchard";
    public string recipient = "Dearest";
    public string signoff = "Ever yours";

    [TextArea(3, 10)]
    public string[] textParts;

    public List<BlankData> blanks = new List<BlankData>();

    // Backwards compatibility
    public string[] correctWords;
    public string[] wordBank;

    public int BlankCount
    {
        get
        {
            if (blanks != null && blanks.Count > 0) return blanks.Count;
            if (textParts != null && textParts.Length > 1) return textParts.Length - 1;
            return 0;
        }
    }

    public BlankData GetBlank(int index)
    {
        if (blanks != null && index >= 0 && index < blanks.Count)
        {
            return blanks[index];
        }
        return null;
    }
}
