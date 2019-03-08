using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SDX.Compiler;
using SDX.Core;

public class NoExceptionHijack : IPatcherMod
{
  public bool Patch(ModuleDefinition module)
  {
    Logging.LogInfo("Start patch process..." + System.Reflection.MethodBase.GetCurrentMethod().ReflectedType);
    ModifyAssembly(module);
    Logging.LogInfo("Patch mod complete.");
    return true;

  }

  private void ModifyAssembly(ModuleDefinition module)
  {
        
    //if (!ModifyGUIWindowConsole(module))
    if ( !DisableOpenConsole( module ))
      throw new Exception("Failed to find and modify the required method!");
  }


    private bool DisableOpenConsole( ModuleDefinition module )
    {
        var guiWindowConsole = module.Types.FirstOrDefault(c => c.Name == "GUIWindowConsole");
        if (guiWindowConsole == null)
        {
            Logging.LogError(string.Format("Failed to find the class: GUIWindowConsole"));
            return false;
        }


        // openConsole() is only called from Logcallback, so it's easier to just blow away the contents of that method.
        Logging.LogInfo("Blowing away openConsole");
        var openConsole = guiWindowConsole.Methods.FirstOrDefault(m => m.Name == "openConsole");
        openConsole.Body.Instructions.Clear();
        openConsole.Body.Instructions.Add( Instruction.Create( OpCodes.Ret));
        return true;

    }
  private bool ModifyGUIWindowConsole(ModuleDefinition module)
  {
    var guiWindowConsole = module.Types.FirstOrDefault(c => c.Name == "GUIWindowConsole");

    if (guiWindowConsole == null)
    {
      Logging.LogError(string.Format("Failed to find the class: GUIWindowConsole"));
      return false;
    }

    var logCallback = guiWindowConsole.Methods.FirstOrDefault(m => m.Name == "LogCallback");

    if (logCallback == null)
      logCallback = guiWindowConsole.Methods.FirstOrDefault(m => m.Parameters.Count == 3 && m.Parameters[0].ParameterType.FullName == "System.String" && m.Parameters[1].ParameterType.FullName == "System.String" && m.Parameters[2].ParameterType.FullName == "UnityEngine.LogType");

    if (logCallback == null)
    {
      Logging.LogError(string.Format("Failed to find the method: GUIWindowConsole::LogCallback"));
      return false;
    }

    Logging.LogInfo(string.Format("Found method: {0}", logCallback.FullName));
    var instructions = logCallback.Body.Instructions;

    if (instructions[1].OpCode == OpCodes.Switch && instructions[5].OpCode == OpCodes.Call && instructions[9].OpCode == OpCodes.Call && instructions[10].OpCode == OpCodes.Ldarg_0)
    {
      int delNextLines = 0;

      foreach (var inst in instructions.Reverse())
      {
        if (delNextLines > 0)
        {
          Logging.LogInfo(string.Format("Removing OpCode: {0} Operand: {1}", inst.OpCode, inst.Operand));
          instructions.Remove(inst);
          delNextLines--;
          continue;
        }

        if (inst.OpCode == OpCodes.Ldarg_0 && inst.Previous.OpCode == OpCodes.Call && inst.Previous.Operand.ToString().Contains("System.String"))
        {
          Logging.LogInfo(string.Format("Found opcode to begin editing method: {0} at offset {1}.  Deleting 10 opcodes.", inst.OpCode, inst.Offset));
          delNextLines = 10;
        }
      }

      return true;
    }

    return false;

    #region IL code
    /*
       0  0000  ldarg.3
       1  0001  stloc.0
       2  0002  ldloc.0
       3  0003  switch   [12(002E), 5(001E), 12(002E), 12(002E), 9(0027)]
       4  001C br.s  12(002E) ldarg.0
       5  001E  ldarg.0
       6  001F  ldarg.1
       7  0020  call instance void GUIWindowConsole::VUG(string)
       8  0025  br.s  12(002E) ldarg.0
       9  0027  ldarg.0
       10 0028  ldarg.1
       11 0029  call instance void GUIWindowConsole::VUG(string)

    12 002E  ldarg.0
    13 002F  ldarg.1
    14 0030  ldarg.2
    15 0031  ldarg.3
    16 0032  newobj instance void GUIWindowConsole/ JE::.ctor(string, string, valuetype[UnityEngine]UnityEngine.LogType)
    17 0037  call instance void GUIWindowConsole::MUG(valuetype GUIWindowConsole / JE)
    18 003C ret
    */
    #endregion
  }

  public bool Link(ModuleDefinition gameModule, ModuleDefinition modModule)
  {
    return true;
  }

  private void SetMethodToVirtual(MethodDefinition meth)
  {
    meth.IsVirtual = true;
  }
  private void SetFieldToPublic(FieldDefinition field)
  {
    field.IsFamily = false;
    field.IsPrivate = false;
    field.IsPublic = true;
  }

  private void SetClassToPublic(TypeDefinition classDef)
  {
    if (classDef == null)
      return;

    classDef.IsPublic = true;
    classDef.IsNotPublic = false;
  }

  private void SetNestedClassToPublic(TypeDefinition classDef)
  {
    if (classDef == null)
      return;
    classDef.IsNestedPublic = true;
  }

  private void SetMethodToPublic(MethodDefinition field)
  {
    field.IsFamily = false;
    field.IsPrivate = false;
    field.IsPublic = true;
  }
}
