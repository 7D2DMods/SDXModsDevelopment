using System;
using UnityEngine;
class BlockLightSDX : BlockLight
    {

    private string particleName;
    private Vector3 offset;
    private ParticleSystem myParticleSystem;

    public override void Init()
    {
        base.Init();
        if (this.Properties.Values.ContainsKey("ParticleName"))
        {
            this.particleName = this.Properties.Values["ParticleName"];
            ConfigureParticles(this.particleName);
        }
        if (this.Properties.Values.ContainsKey("ParticleOffset"))
        {
            this.offset = StringParsers.ParseVector3(this.Properties.Values["ParticleOffset"], 0, -1);
        }
    }

    protected virtual void checkParticles(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        if (this.particleName != null && !_world.GetGameManager().HasBlockParticleEffect(_blockPos))
        {
            this.addParticles(_world, _clrIdx, _blockPos.x, _blockPos.y, _blockPos.z, _blockValue);
        }
    }

    public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
        this.removeParticles(_world, _blockPos.x, _blockPos.y, _blockPos.z, _blockValue);
    }

    public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
        this.checkParticles(_world, _chunk.ClrIdx, _blockPos, _blockValue);
    }

    public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
        if (this.particleName != null)
        {
            this.checkParticles(_world, _clrIdx, _blockPos, _blockValue);
        }
    }

    public override void OnBlockUnloaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockUnloaded(_world, _clrIdx, _blockPos, _blockValue);
        this.removeParticles(_world, _blockPos.x, _blockPos.y, _blockPos.z, _blockValue);
    }
    public override bool updateLightState(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bSwitchLight , bool _enableState)
    {
        checkParticles(_world, _cIdx, _blockPos, _blockValue);
        return base.updateLightState(_world, _cIdx, _blockPos, _blockValue, _bSwitchLight, _enableState);
    }

    protected virtual void addParticles(WorldBase _world, int _clrIdx, int _x, int _y, int _z, BlockValue _blockValue)
    {
        if (this.particleName == null || this.particleName == string.Empty)
        {
            return;
        }
        float num = 0f;
        if (_y > 0 && Block.list[_blockValue.type].IsTerrainDecoration && Block.list[_world.GetBlock(_x, _y - 1, _z).type].shape.IsTerrain())
        {
            sbyte density = _world.GetDensity(_clrIdx, _x, _y, _z);
            sbyte density2 = _world.GetDensity(_clrIdx, _x, _y - 1, _z);
            num = MarchingCubes.GetDecorationOffsetY(density, density2);
        }
        _world.GetGameManager().SpawnBlockParticleEffect(new Vector3i(_x, _y, _z), new ParticleEffect(this.particleName, new Vector3((float)_x, (float)_y + num, (float)_z) + this.getParticleOffset(_blockValue), this.shape.GetRotation(_blockValue), 1f, Color.white));

    }

    public void ConfigureParticles(String strMyAssetBundle)
    {
        if (strMyAssetBundle.IndexOf('#') == 0 && strMyAssetBundle.IndexOf('?') > 0)
        {
            String strFilename = strMyAssetBundle.Split('?')[0];
            GameObject temp =  DataLoader.LoadAsset<GameObject>(strMyAssetBundle);
            foreach( var particle in temp.GetComponents<Transform>())
            {
                string text = ((Transform)particle).gameObject.name;
                if (text.StartsWith(ParticleEffect.prefix))
                {
                    text = text.Substring(ParticleEffect.prefix.Length);
                    if (!ParticleEffect.m_prefabParticleEffects.ContainsKey(text.GetHashCode()))
                        ParticleEffect.m_prefabParticleEffects.Add(text.GetHashCode(), (Transform)particle);
                    this.particleName = text;
                    break;
                }
            }
        }
    }
    protected virtual Vector3 getParticleOffset(BlockValue _blockValue)
    {
        return this.shape.GetRotation(_blockValue) * (this.offset - new Vector3(0.5f, 0.5f, 0.5f)) + new Vector3(0.5f, 0.5f, 0.5f);
    }

    protected virtual void removeParticles(WorldBase _world, int _x, int _y, int _z, BlockValue _blockValue)
    {
        _world.GetGameManager().RemoveBlockParticleEffect(new Vector3i(_x, _y, _z));
    }
}

