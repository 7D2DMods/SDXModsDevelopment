using System;
using System.Collections.Generic;

    class TileEntityFarmingAnimal : TileEntity
    {
    public TileEntityFarmingAnimal(Chunk _chunk) : base(_chunk)
    {
        
    }

    public override TileEntityType GetTileEntityType()
    {
        return TileEntityType.Powered;
    }
}

