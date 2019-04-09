AnimationSDX
=======================

Provides a mecanim animation layer for use for external entities to control their animations. This version does not support Root Motion, so must be disabled in the Unity3d bundle in the XML.

A sample entityclass entry called SDXTemplate is provided as an easy way to include the MecanimSDX class. It extends off of the zombieTemplateMale.

Example Usage:
--------------

~~~~~~~~~~~{.xml}
 <configs>
    <append xpath="/entity_classes">
      <entity_class name="SDXTemplate" extends="zombieTemplateMale">
        <property name="AvatarController" value="MecanimSDX, Mods" />
        <property name="RootMotion" value="false" />
      </entity_class>
    </append>
</configs>
~~~~~~~~~~~