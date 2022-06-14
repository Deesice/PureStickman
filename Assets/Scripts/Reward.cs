using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AOUAD
{
    [CreateAssetMenu(menuName = "Custom/Reward", fileName = "NewReward")]
    public class Reward : ScriptableObject
    {
        public int money;
        public int xp;
    }
}
