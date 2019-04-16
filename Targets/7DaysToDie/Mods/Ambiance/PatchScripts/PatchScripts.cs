using System;
using SDX.Compiler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;


public class BlockLightPatch : IPatcherMod
{
    public bool Patch(ModuleDefinition module)
    {
        Console.WriteLine("== BlockLight Patcher===");
        var gm = module.Types.First(d => d.Name == "BlockLight");
        var method = gm.Methods.FirstOrDefault(d => d.Name == "updateLightState");
        SetMethodToPublic(method);
        SetMethodToVirtual(method);

        gm = module.Types.First(d => d.Name == "ParticleEffect");
        var field = gm.Fields.First(d => d.Name == "m_prefabParticleEffects");
        SetFieldToPublic(field);
        return true;
    }

    // Called after the patching process and after scripts are compiled.
    // Used to link references between both assemblies
    // Return true if successful
    public bool Link(ModuleDefinition gameModule, ModuleDefinition modModule)
    {
        return true;
    }

    // Helper functions to allow us to access and change variables that are otherwise unavailable.
    private void SetMethodToVirtual(MethodDefinition meth)
    {

        meth.IsVirtual = true;
    }

    private void SetFieldToPublic(FieldDefinition field)
    {
        if (field == null)
            return;
        field.IsFamily = false;
        field.IsPrivate = false;
        field.IsPublic = true;

    }
    private void SetMethodToPublic(MethodDefinition field)
    {
        if (field == null)
            return;

        field.IsFamily = false;
        field.IsPrivate = false;
        field.IsPublic = true;

    }

}
