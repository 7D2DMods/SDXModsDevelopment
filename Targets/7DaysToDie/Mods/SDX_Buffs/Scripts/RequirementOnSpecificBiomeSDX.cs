using System;
using System.Xml;
using UnityEngine;

// 	<requirement name="RequirementOnSpecificBiomeSDX, Mods" biome="something" />

public class RequirementOnSpecificBiomeSDX : RequirementBase
{
    public string strBiome = "";

    public override bool ParamsValid(MinEventParams _params)
    {

        Debug.Log(" Current Biome: " + _params.Self.MinEventContext.Biome.m_sBiomeName);
        if (_params.Self.MinEventContext.Biome.m_sBiomeName == this.strBiome)
            return true;

        return false;
    }

    public override bool ParseXmlAttribute(XmlAttribute _attribute)
    {
        string name = _attribute.Name;
        if (name != null)
        {
            if (name == "biome")
            {
                strBiome = _attribute.Value.ToString();
                return true;
            }
        }
        return base.ParseXmlAttribute(_attribute);
    }
}
