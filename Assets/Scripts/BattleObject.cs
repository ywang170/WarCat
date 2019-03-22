using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BattleObject : PhysicsObject
{
    public struct Hit
    {
        public int damage;
        public int power;
        public bool guardable;
        public BattleObject attacker;
        public int serial;
    }

    public int hitPoint;
    public float invulnerabilityPeriod = 0f;

    private double intention = 0;
    private Queue<double> nextIntentions;
    private Dictionary<BattleObject, int> attackerHitSerialMap;

    // Start
    protected override void PhysicsObjectStartInternal()
    {
        nextIntentions = new Queue<double>();
        attackerHitSerialMap = new Dictionary<BattleObject, int>();
        BattleObjectStartInternal();
    }

    protected abstract void BattleObjectStartInternal();

    // Update
    protected override void PhysicsObjectUpdateInternal(float deltaTime)
    {
        invulnerabilityPeriod = Mathf.Max(0, invulnerabilityPeriod - deltaTime);
        UpdateIntention(deltaTime);
        PerformIntention(deltaTime);
    }

    // Based on condition, see if need to change intention
    protected abstract void UpdateIntention(float deltaTime);

    // Put intention into action. Like set animation vars, set targetVelocity, etc
    protected abstract void PerformIntention(float deltaTime);

    // Called by other battle object to deal hit
    public void TakeHit(Hit hit)
    {
        if (!attackerHitSerialMap.ContainsKey(hit.attacker))
        {
            attackerHitSerialMap.Add(hit.attacker, -1);
        }
        if (attackerHitSerialMap[hit.attacker] < hit.serial)
        {
            attackerHitSerialMap[hit.attacker] = hit.serial;
            TakeHitInternal(hit);
        }
       
    }

    // Implemented internally to receive hit
    protected abstract void TakeHitInternal(Hit hit);

    // Called both internally and externally to try switch action
    public abstract bool TrySwitchIntention(double newIntention);
  
    // Called both internally and externally to switch or queue an action
    public void SwitchIntentionOrQueueIt(double newIntention)
    {
        if (!TrySwitchIntention(newIntention))
        {
            nextIntentions.Enqueue(newIntention);
        }
    }

}
