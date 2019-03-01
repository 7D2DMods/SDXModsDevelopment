using System;
using UnityEngine;

public class BlockJumpPadSDX : BlockJumpPad
{
    public override void OnEntityWalking(WorldBase _world, int _x, int _y, int _z, BlockValue _blockValue, Entity entity)
    {
        entity.motion.y = 3f;
        if (entity is EntityAlive)
        {
            (entity as EntityAlive).moveHelper.StartJump(false, 0f, 3f);
        }
    }
}