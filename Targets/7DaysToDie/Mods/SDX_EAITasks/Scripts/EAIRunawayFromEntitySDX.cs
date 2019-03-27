using System;
using System.Collections.Generic;
using UnityEngine;


// <property name="AITask-2" value="RunAwayFromEntitySDX, Mods" />
class EAIRunAwayFromEntitySDX : EAIRunAway
{
    private List<Entity> NearbyEntities = new List<Entity>();
    private List<Entity> NearbyEnemies = new List<Entity>();
    private EntityAlive avoidEntity;
    private int fleeCounter;
    private int fleeDistance = 10;

    private bool blDisplayLog = false;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.GetType() + " : " + this.theEntity.EntityName + ": " + this.theEntity.entityId + ": " + strMessage);
    }
    public override bool CanExecute()
    {
        if ( CheckSurroundingEntities())
        {
            base.FindFleeDirection(this.avoidEntity.position, this.fleeDistance);
            return true;
        }

        return false;
    }

    public bool CheckFactionForEnemy(EntityAlive Entity)
    {
        FactionManager.Relationship myRelationship = FactionManager.Instance.GetRelationshipTier(this.theEntity, Entity);
        switch( myRelationship)
        {
            case FactionManager.Relationship.Hate:
                this.fleeDistance = 40;
                break;
            case FactionManager.Relationship.Dislike:
                this.fleeDistance = 20;
                break;
            case FactionManager.Relationship.Neutral:
                this.fleeDistance = 10;
                break;
            case FactionManager.Relationship.Like:
                return false;
            case FactionManager.Relationship.Love:
                return false;

        }

        return true;
    }

    public bool CheckSurroundingEntities()
    {
        this.NearbyEntities.Clear();
        NearbyEnemies.Clear();

        EntityAlive leader = null;
        if (this.theEntity.Buffs.HasCustomVar("Leader"))
        {
            DisplayLog(" leader Detected.");
            int EntityID = (int)this.theEntity.Buffs.GetCustomVar("Leader");
            leader = this.theEntity.world.GetEntity(EntityID) as EntityAlive;

        }

        // Search in the bounds are to try to find the most appealing entity to follow.
        Bounds bb = new Bounds(this.theEntity.position, new Vector3(this.theEntity.GetSeeDistance(), 20f, this.theEntity.GetSeeDistance()));
        this.theEntity.world.GetEntitiesInBounds(typeof(EntityAlive), bb, this.NearbyEntities);
        DisplayLog(" Nearby Entities: " + this.NearbyEntities.Count);
        for (int i = this.NearbyEntities.Count - 1; i >= 0; i--)
        {
            EntityAlive x = (EntityAlive)this.NearbyEntities[i];
            if (x != this.theEntity && x.IsAlive())
            {
                if (x == leader)
                    continue;

                DisplayLog("Nearby Entity: " + x.EntityName);
                if (CheckFactionForEnemy(x))
                    NearbyEnemies.Add(x);
            }
        }

        return NearestEnemy();

    }

    public bool NearestEnemy()
    {
        if (this.NearbyEnemies.Count == 0)
            return false;

        // Finds the closet block we matched with.
        EntityAlive closeEnemy = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = this.theEntity.position;
        foreach (EntityAlive enemy in NearbyEnemies)
        {
            float dist = Vector3.Distance(enemy.position, currentPos);
            DisplayLog(" Entity: " + enemy.EntityName + "'s distance is: " + dist);
            if (dist < minDist)
            {
                closeEnemy = enemy;
                minDist = dist;
            }
        }

        if (closeEnemy != null)
        {
            DisplayLog(" Closes Enemy: " + closeEnemy.ToString());
            this.avoidEntity = closeEnemy;
            return true;
        }
        return false;
    }
}

