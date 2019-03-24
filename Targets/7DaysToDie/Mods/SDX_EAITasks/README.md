EAI Tasks and Targets

EAI Tasks
---------

The SDX_EAITasks class introduces new AI tasks for entities to perform.

ApproachAndFollowTargetSDX, Mods
================================

This AI task is useful for getting followers, based on potential incentives.

Incentives can be a block name that you are holding, an item name that you are holding, a buff, or a CVar value. You do not need to specify which
type of incentive it is. The class will go through all the various combinations to try to identify which of your incentives is active.

For the cvar checks, it stored the EntityID of the target. For example, Leader is the one who has hired the entity, so it's the player.entityID. For Mother, 
it's the entity ID of the mother that spawned it.

Example Usage:
      <property name="AITask-4" value="ApproachAndFollowTargetSDX, Mods" param1="Mother,Leader,hayBaleBlock,foodCornOnTheCob"/>

	  The above AI Task would allow the entity to follow if you are the Mother or Leader entity, if you are holding a haybale, or if you are holding corn on the cob.


PatrolSDX, Mods
===============

This AI Task is useful for asking followers and NPCs to patrol a path for you, once they are hired.

To set a patrol task, the Order must be SetPatrol. By default, this is set through opening up a dialog chat by interacting with the NPC. Once SetPatrol is set, walk the path you want the entity to follow. The entity will record every step you make.

Once the path is set, interact with them again, and ask them to Patrol. They will turn around and retrace their steps to their starting position, and start looping back and forth until interrupted.

Example Usage:
		<property name="AITask-3" value="PatrolSDX, Mods"/>


WanderSDX, Mods
===============

This AI Task is useful for assigning to NPCs and animals alike. By default, the base Wander Ai task will begin attacking a block that is in front of it. The WanderSDX task stops the entity from wandering in the direction that its stuck in, rather than trying to break through. Very useful for farm animals that you don't want to break out of your fences.

Example Usage:
		<property name="AITask-9" value="WanderSDX, Mods"/>
		
MaslowLevel1SDX, Mods
=====================

This AI Task enables rules based on losely on Maslow laws. It allows the entity to react to its own stats, including seeking food and water if hungry or thirsty, sanitation needs, and even farm-animal production, such as laying an egg or producing milk from a cow.

Production is based on cvars

This AI Task requires extra property lines in the entityclass:

     <!-- which containers to look for food in -->
      <property name="FoodBins" value="cntSecureStorageChest,cntStorageChest" />

      <!-- what it can drink out of -->
      <property name="WaterBins" value="water,waterMoving,waterStaticBucket,waterMovingBucket,terrWaterPOI" />

      <!-- Default thirsty and hungry buffs -->
      <property name="ThirstyBuffs" value="buffStatusThirsty1,buffStatusThirsty2" />
      <property name="HungryBuffs" value="buffStatusHungry1,buffStatusHungry2" />

      <!-- Sanitation Buff: Aka, make 'em poop part. Add in  buffSanitationStatusCheck in the Buffs entry here to enable. -->
      <property name="SanitationBuffs" value="buffStatusSanitation1,buffStatusSanitation2" />

      <!-- Food items and bins that this entity will eat, if using the right AI Task -->
	  <property name="FoodItems" value="hayBaleBlock,resourceYuccaFibers,foodCropYuccaFruit,foodCornBread,foodCornOnTheCob,foodCornMeal,foodCropCorn,foodCropGraceCorn"/>
     
	  <!-- For NPCs, you can assign a ToiletBlocks where it will go to relieve itself
      <property name="ToiletBlocks" value="cntToilet01,cntToilet02,cntToilet03" />
      <property name="SanitationBlock" value="terrDirt" /> <!-- Poop block. If ToiletBlocks is configured, it'll use those rather than generate this block. -->

	  <!-- Chicken specific. Where the chicken will place its items. -->
      <property name="Beds" value="cntBirdnest" />  <!-- this is where the chicken will go to 'bed' at, or rather, lay an egg -->
      <property name="ProductionFinishedBuff" value="buffAnimalChickenEgg" />  <!-- Which buff is on the entity when its produced something -->

      <property class="ProductionItems">
        <property name="foodEgg" value="1" param1="$EggValue" /> <!-- which item to product, how many, and if a cvar needs resetting to 0 -->
        <property name="resourceFeather" value="5"/>
      </property>


Example Usage:
		<property name="AITask-7" value="MaslowLevel1SDX, Mods"/>



LootLocationSDX, Mods
=====================

This AI task is useful for assinging to NPCs who will help you loot a POI with you. They will scan for all the loot containers in the bounds of a prefab, and start searching them. You may intereact with the NPC and ask to see their Inventory to see what they have picked up.

Example Usage:
		<property name="AITarget-5" value="LootLocationSDX, Mods" />


AI Targets
----------

SetAsTargetIfHurtSDX, Mods
==========================

This AI Target helps assign whether there is an revenge target if something attacks it. However, if its your leader, you forgive them...

Example Usage:
		<property name="AITarget-1" value="SetAsTargetIfHurtSDX, Mods" param1="Entity"/>


SetAsTargetIfLeaderAttackedSDX, Mods
====================================

This AI Target helps assign whether the Leader, if assigned, is being attacked and if they should become the attack target of the entity.

Example Usage:
		<property name="AITarget-3" value="SetAsTargetIfLeaderAttackedSDX, Mods" param1="Entity"/>

SetAsTargetNearestEnemySDX, Mods
=================================

This AI Target helps assign whether any entities that are close by belong to an enemy faction, and attacks accordingly.

Example Usage:
		<property name="AITarget-4" value="SetAsTargetNearestEnemySDX, Mods" param1="Entity,0"/>
