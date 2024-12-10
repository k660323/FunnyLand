using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestSystemSaveTest : MonoBehaviour
{
    [SerializeField]
    private Quest quest;
    [SerializeField]
    private Category category;
    [SerializeField]
    private TaskTarget target;

    // Start is called before the first frame update
    void Start()
    {
        var questSystem = Managers.Quest;

        if(questSystem.ActiveQuests.Count == 0)
        {
            Debug.Log("Register");
            var newQuest = questSystem.Register(quest);
        }
        else
        {
            questSystem.onQuestCompleted += (quest) =>
            {
                Debug.Log("Complete");
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
            };
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Managers.Quest.ReceiveReport(category, target, 1);
    }
}
