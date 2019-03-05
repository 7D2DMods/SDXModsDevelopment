SDX_Quests
==========

ObjectiveBuffSDX:
	
	This new Objective type allows you to set a condition on a buff before continueing. In the below example, in order to transition from Phase 1 to Phase 2, the entity must have the buff called "buffAnimalAdult".

	  <objective type="BuffSDX, Mods">
        <property name="phase" value="1" />
        <property name="buff" value="buffAnimalAdult" />
      </objective>


	The ObjectiveBuffSDX was working on an Update call, so it was happening too much in the run of a second. Since buffs are usually longer, and don't require such an agressive check, the following event was added

QuestActionSpawnEntitySDX:

	This QuestAction allows you to spawn an entity, spawning it beside the SharedOwnerID's entity. Unlike SpawnEnemy, it won't automatically set the player or spawn entity trigger as ane enemy. Like SpawnEnemy, it can accept comma delimited entities in the id="animalFarmCow,zombieArlene".
		
      <!-- This will spawn in an zombieBear entity during Phase 3 -->
      <action type="SpawnEntitySDX, Mods" id="zombieBear" value="1" phase="3" />

RewardItemSDX:

	This RewardItem allows you to give an Item to the SharedOwnerID entity.

		<reward type="ItemSDX, Mods" id="casinoCoin" value="1" />

RewardQuestSDX:

	This RewardQuest allows you to give a Quest to the SharedOwnerID entity.

	    <reward type="QuestSDX, Mods" id="buffProgression_Quest" />


Here's an example of a Quest line that works based on buff progression

    <quest id="buffProgression_Quest">
      <property name="name_key" value="buffProgression_Quest" />
      <property name="subtitle_key" value="buffProgression_Quest" />
      <property name="description_key" value="buffProgression_Quest" />
      <property name="icon" value="ui_game_symbol_zombie" />
      <property name="repeatable" value="true" />
      <property name="category_key" value="challenge" />
      <property name="offer_key" value="buffProgression_Quest" />
      <property name="difficulty" value="veryeasy" />

      <!-- Before progressing, make sure the entity is at infection1 -->
      <objective type="BuffSDX, Mods">
        <property name="phase" value="1" />
        <property name="buff" value="buffIllInfection1" />
      </objective>

      <!-- Before progressing, make sure the entity is at infection2 -->
      <objective type="BuffSDX, Mods">
        <property name="phase" value="2" />
        <property name="buff" value="buffIllInfection2" />
      </objective>

      <!-- Before progressing, make sure the entity is at infection3 -->
      <objective type="BuffSDX, Mods">
        <property name="phase" value="3" />
        <property name="buff" value="buffIllInfection3" />
      </objective>
      
      <!-- This will spawn in an zombieBear entity during Phase 3 -->
      <action type="SpawnEntitySDX, Mods" id="zombieBear" value="1" phase="3" />
      
      <!-- Once the entity is spawned, give back the same quest, and give it a casino coin for a good job. -->
      <reward type="QuestSDX, Mods" id="buffProgression_Quest" />
      <reward type="ItemSDX, Mods" id="casinoCoin" value="1" />

    </quest>
