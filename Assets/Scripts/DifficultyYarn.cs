using UnityEngine;
using Yarn.Unity;

public class DifficultyYarn : MonoBehaviour
{
    [SerializeField]
    private DialogueRunner dialogueRunner;

    [SerializeField]
    private GameObject dethornShears;

    [SerializeField]
    private GameObject trimShears;

    [SerializeField]
    private GameObject arrange;

    public void Awake()
    {
        dialogueRunner.AddCommandHandler<int>("difficulty", SetDifficulty);
    }

    private void SetDifficulty(int i)
    {
        GameManager.Instance.difficulty = i;
    }
}


