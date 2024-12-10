using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestGiver : MonoBehaviour
{
    [SerializeField]
    private Quest[] quests;

    // Start is called before the first frame update
    private void Start()
    {
        foreach (var quest in quests)
        {
            if (quest.IsAcceptable && !Managers.Quest.ContainsInCompletedQuests(quest))
                Managers.Quest.Register(quest);
        }
    }
}
