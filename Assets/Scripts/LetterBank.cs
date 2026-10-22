using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LetterBlank : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    public bool IsFilled { get; private set; }

    private string currentWord;

    public void SetWord(string word)
    {
        currentWord = word;
        text.text = word;

        IsFilled = true;
    }

    public string GetWord()
    {
        return currentWord;
    }
}