using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    [SerializeField] Image fillImage;
    UIAnimationHelper animationHelper;
    void Start()
    {
        animationHelper = GetComponent<UIAnimationHelper>();
        var health = FindObjectOfType<Health>();
        health.Damaged += OnDamage;
        health.Dead += OnDead;
        health.Healed += OnHealed;
        animationHelper.Show();
    }
    void OnDamage(float value)
    {
        fillImage.fillAmount = value;
    }
    void OnDead()
    {
        animationHelper.Hide();
    }
    void OnHealed(float value)
    {
        animationHelper.Show();
        OnDamage(value);
    }
}
