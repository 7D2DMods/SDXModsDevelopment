using System;
using System.Collections.Generic;
using UnityEngine;

class EAIVisitLocation : EAIApproachSpot
{

    //private List<TileEntity> NearbyEntities = new List<TileEntity>();

    //public void ConfigureTargetEntity()
    //{
    //    this.NearbyEntities.Clear();
    //    this.theEntity.otherEntitySDX = null;

    //    // Search in the bounds are to try to find the most appealing entity to follow.
    //    Bounds bb = new Bounds(this.theEntity.position, new Vector3(20f, 20f, 20f));
    //    //this.theEntity.world.GetTileEntity(typeof(TileEntity), bb, this.NearbyEntities);
    //    for (int i = this.NearbyEntities.Count - 1; i >= 0; i--)
    //    {
    //        EntityAlive x = (EntityAlive)this.NearbyEntities[i];
    //        if (x != this.theEntity)
    //        {
    //            // Check if the there's an entity in our area that is allowed to control us.

    //            // first, we check if we are controlled via an activate buff.
    //            if (x.Buffs.HasBuff(this.strControlMechanism))
    //                this.theEntity.otherEntitySDX = x;

    //            // Then we check if the control mechanism is an item being held.
    //            if (x.inventory.holdingItem.Name == this.strControlMechanism)
    //                this.theEntity.otherEntitySDX = x;

    //            // If we do have a master entity, and it's still in range, then set it as the target.
    //            if (this.theEntity.otherEntitySDX != null)
    //            {
    //                base.entityTarget = this.theEntity.otherEntitySDX;
    //                return;

    //            }
    //        }
    //    }

    //}

    //public override bool CanExecute()
    //{
    //    bool result = base.CanExecute();
    //    if ( result )
    //    {
    //        // Don't target our master!
    //        if (this.theEntity.otherEntitySDX != null && base.targetEntity != null && base.targetEntity == this.theEntity.otherEntitySDX)
    //            return false;
    //    }
    //    return result;
    //}
}

