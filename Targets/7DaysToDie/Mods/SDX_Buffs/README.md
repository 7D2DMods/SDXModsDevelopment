SDX Buffs
---------

This SDX Mod adds additional Action events and Requirements that can be used in the buffs.xml.

AnimatorSpeedSDX, Mods
======================

This class allows you to change the animation speed

Example Usage:
    <triggered_effect trigger="onSelfBuffStart" action="AnimatorSpeedSDX, Mods" target="self" value="1" /> // normal speed
    <triggered_effect trigger="onSelfBuffStart" action="AnimatorSpeedSDX, Mods" target="self" value="2" /> // twice the speed

CreateItemSDX, Mods
===================

This class allows you to get an item from a buff.

Example Usage:
	<triggered_effect trigger="onSelfBuffRemove" action="CreateItem" item="drinkJarCoffee" count="2"/>


GiveQuestSDX, Mods
==================

This class allows you to give a Quest to a non-player character.

Example Usage:
    <triggered_effect trigger="onSelfBuffStart" action="GiveQuestSDX, Mods" target="self" quest="myNewQuest" />

ModifySkillSDX, Mods
====================

This class allows you to add a skill point to a perk or skill

Example Usage:
    <triggered_effect trigger="onSelfBuffStart" action="ModifySkillSDX, Mods" tag="skill_name" operation="add" value="1" /> // levels up skill_name by 1


PumpQuestSDX, Mods
==================

Some new requirements and objectives added to the Quest system are not checked at regular intervals, such as if you have an Objective that progresses by a Buff. Adding a PumpQuest will cause each quest that the entity has to recheck all its objectives to see if it can progress.

Example Usage:
	<triggered_effect trigger="onSelfBuffStart" action="PumpQuestSDX, Mods" target="self"  />

SkillPointSDX, Mods
===================

This class allows you to add a new Skill Point to an entity via a buff

Example Usage:
	<triggered_effect trigger="onSelfBuffStart" action="SkillPointSDX, Mods" target="self" value="2" /> // two Skill points


SpawnBabySDX, Mods:
===================

This class allows you to spawn an entity, or an entity from a spawn group, and sets a Mother cvar, linking the new entity to the original entity. This allows mothers to have babies that are linked to each other.

Example usage:
        <triggered_effect trigger="onSelfBuffFinish" action="SpawnBabySDX, Mods" target="self" SpawnGroup="farmAnimalsCow" />


RequirementEveryDaySDX, Mods:
=============================

This class adds a new requirement that triggers every day.

Example usage:
		<requirement name="RequirementEveryXDaySDX, Mods" value="0"/> <!-- triggers on horde days -->
		<requirement name="RequirementEveryXDaySDX, Mods" value="2"/> <!-- triggers every two days -->


RequirementEveryHourSDX, Mods:
==============================

This class adds a new requirement that triggers at the appointed time.

Example Usage:
        <requirement name="RequirementEveryXHourSDX, Mods" value="22"/> <!-- triggers at 22:00 hours -->


RequirementOnSpecificBiomeSDX, Mods
===================================

This class adds a new requirement based on a biome name filter.

Example Usage: 
 	<requirement name="RequirementOnSpecificBiomeSDX, Mods" biome="desert" />

	<triggered_effect trigger="onSelfEnteredBiome" action="AddBuff" target="self" buff="bufffnamehere">
		<requirement name="RequirementOnSpecificBiomeSDX, Mods" biome="desert" />
	</triggered_effect>
