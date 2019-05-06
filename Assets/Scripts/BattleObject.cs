using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Hit
{
    public float damage;
    public float power;
    public bool guardable;
    public int side;
    public Vector2 position;
    public BattleObject attacker;

    public Hit(
        int damage,
        int power,
        bool guardable,
        int side,
        Vector2 position,
        BattleObject attacker = null
        )
    {
        this.damage = damage;
        this.power = power;
        this.guardable = guardable;
        this.side = side;
        this.position = position;
        this.attacker = attacker;
    }
}

public abstract class BattleObject : PhysicsObject
{
    public float hitPoint;
    public float invulnerabilityPeriod = 0f;
    public int side;

    protected double status = 0;
    protected Queue<double> nextIntentions;

    // Start
    protected override void PhysicsObjectStartInternal()
    {
        nextIntentions = new Queue<double>();
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
        // Won't take hit under invulnerability or if attack is from same side  
        if (invulnerabilityPeriod <= 0 && hit.side != this.side)
        {
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
