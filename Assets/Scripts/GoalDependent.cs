using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalDependent : MonoBehaviour
{
    [SerializeField] GoalType type;
    [SerializeField] bool isMoneyDependent;
    void Start()
    {
        if (!isMoneyDependent)
            gameObject.SetActive(DifficultyManager.GetGoalType() == type);
        else
        {
            var actualGotMoney = Inventory.Instance.ConvertToUnlockedCurrency(DifficultyManager.GetGoalType());
            gameObject.SetActive(actualGotMoney == type);
        }
    }
}
