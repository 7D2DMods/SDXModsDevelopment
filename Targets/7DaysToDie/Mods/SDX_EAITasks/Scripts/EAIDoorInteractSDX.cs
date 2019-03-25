using System;

using UnityEngine;
class EAIDoorInteractSDX : EAIDoorInteract
{
    public bool bWentThroughDoor = false;
    private bool blDisplayLog = false;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.GetType() + " :" + this.theEntity.EntityName + ": " + strMessage);
    }

  
    public override bool Continue()
    {
        if ( bWentThroughDoor )
        {
            Ray lookRay = new Ray(this.theEntity.position,this.doorPos.ToVector3());
            if (!Voxel.Raycast(this.theEntity.world, lookRay, Constants.cDigAndBuildDistance, -538480645, 4095, 0f))
                return false; // Not seeing the target.

            if (!Voxel.voxelRayHitInfo.bHitValid)
                return false; // Missed the target. Overlooking?

            this.targetDoor.OnBlockActivated(this.theEntity.world, Voxel.voxelRayHitInfo.hit.clrIdx, this.doorPos, Block.GetBlockValue(this.targetDoor.blockID), null);
            return false;
        }

        this.bWentThroughDoor = base.Continue();
        return !this.bWentThroughDoor;
    }
}

