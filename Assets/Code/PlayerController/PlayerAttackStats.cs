using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Attack Stats", menuName = "Player/Attack Stats")]
public class PlayerAttackStats : ScriptableObject
{
    [Header("Attack Chain Cooldowns/Ground")]
    public float groundAttackCD1;
    public float groundAttackCD2;
    public float groundAttackCD3;
    private float[] groundCooldowns;
    [Header("Attack Chain Cooldowns/Air")]
    public float airAttackCD1;
    public float airAttackCD2;
    public float airAttackCD3;
    private float[] airCooldowns;
    [Header("Attack Chain Length")]
    public int comboLength;
    [Header("End of Combo Cooldown")]
    public float chainEndedCD;
    [Header("Combo Persistence Duration")]
    public float comboPersistenceDuration = 3f;
    [Header("Attack Momentum")]
    public float groundBoostStrength = 10f;
    public float groundBoostDecayRate = 40f;
    public float airBoostStrength = 20f;
    public float airBoostDecayRate = 80f;
    public float maxMouseDistance = 25f;
    public float minMouseDistance = 5f;
    private void OnValidate()
    {
        groundCooldowns = new float[] {groundAttackCD1, groundAttackCD2, groundAttackCD3, chainEndedCD};
        airCooldowns = new float[] {airAttackCD1, airAttackCD2, airAttackCD3, chainEndedCD};
        comboLength = groundCooldowns.Length - 1;
    }

    private void OnEnable()
    {
        groundCooldowns = new float[] {groundAttackCD1, groundAttackCD2, groundAttackCD3, chainEndedCD};
        airCooldowns = new float[] {airAttackCD1, airAttackCD2, airAttackCD3, chainEndedCD};
        comboLength = groundCooldowns.Length - 1;
    }

    public float getCD (bool grounded, int combo)
    {
        if (grounded)
        {
            return groundCooldowns[combo];
        }
        else
        {
            return airCooldowns[combo];
        }
    }
}
