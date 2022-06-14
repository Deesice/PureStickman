using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Custom/Bot properties", fileName = "NewBotProperties")]
public class BotProperties : ScriptableObject
{
    public float safeDistance;
    public float sightDistance;
    public float tensionTime;
    public float reaction;
    public float movingSafeTime;
    public bool canMoveWhileAiming;
    public float targetingError;
}
