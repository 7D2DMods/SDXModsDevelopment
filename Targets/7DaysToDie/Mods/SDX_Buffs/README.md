

Example of using the Buff System and QuestSDX System
====================================================
The following example of a quest and buff system is based around the life cycle of a cow. 

The quest's main goal is to allow the cow to have a baby, and allow the cow to produce milk. The production of milk, however, only happens after the cow has had a baby. Every cow spawned in would be given the farmAnimal_Cow_Pregnancy quest line for it to follow. 

The Cow's life cycle is controlled via a series of buffs, starting off with a simple buffAnimalBaby buff. Once the duration of that buff expires, it fires off the buffAnimalJuvenile. When it expires, it calls buffAnimalAdult.

The duration of the buff is in Seconds. In this example, the AnimalBaby buff will stay on for 90 seconds, before it expires, and calls the Juvenile buff. A CVAR is set on the AnimalBaby and AnimalJunvenile. The Cow class uses this buff to downscale the entity.

    <!-- Controls the animal's age -->
    <buff name="buffAnimalBaby" hidden="true">
      <stack_type value="ignore"/>
      <duration value="90"/>
      <effect_group>
        <triggered_effect trigger="onSelfBuffStart" action="ModifyCVar" cvar="$sizeScale" operation="set" value="0.3"/>
        <triggered_effect trigger="onSelfBuffFinish" action="AddBuff" target="self" buff="buffAnimalJuvenile" />
      </effect_group>
    </buff>

    <buff name="buffAnimalJuvenile" hidden="true">
      <stack_type value="ignore"/>
      <duration value="90"/>
      <effect_group>
        <triggered_effect trigger="onSelfBuffStart" action="ModifyCVar" cvar="$sizeScale" operation="set" value="0.7"/>
        <triggered_effect trigger="onSelfBuffFinish" action="AddBuff" target="self" buff="buffAnimalAdult" />
      </effect_group>
    </buff>

The buffAnimalAdult is a bit different. This is where its interactions with the quest line comes in. It has two effect groups. 

The first effect group determines when the animal can get pregnant:

	If that random roll is less than 25.	
	If it's not currently pregnant
	If it's not a new Mother
	
	That gives the cow a high chance of 25% ever time the buff updates itself. Without a update_rate, once per second. Every second, it has a 25% chance of being given the pregnant buff.

	The second effect_group sets the scale to 1f (full size), pumps the quest chain to see if it meets any new conditions. Once the AnimalAdult buff expires, it progresses to a Senior buff. When the Adult buff expires, it removes the buffAnimalHarvestable buff. The cow will no longer produce milk.

    <buff name="buffAnimalAdult" hidden="true">
      <stack_type value="ignore"/>
      <duration value="90"/>
      
      <!-- If it's an adult, have a 25% chance of becoming pregnant, but only if its not currently pregnant and is not a new MOther.-->
      <effect_group>
        <requirement name="RandomRoll" seed_type="Random" target="self" min_max="0,100" operation="LTE" value="25"/>
        <requirement name="NotHasBuff" buff="buffAnimalPregenant"/>
        <requirement name="NotHasBuff" buff="buffAnimalNewMother"/>
        <triggered_effect trigger="onSelfBuffUpdate" action="AddBuff" target="self" buff="buffAnimalPregenant"/>
      </effect_group>

      <effect_group>
        <triggered_effect trigger="onSelfBuffStart" action="ModifyCVar" cvar="$sizeScale" operation="set" value="1"/>
        <triggered_effect trigger="onSelfBuffStart" action="PumpQuestSDX, Mods" target="self"  />

        <!-- Progress through the aging to Senior -->
        <triggered_effect trigger="onSelfBuffFinish" action="AddBuff" target="self" buff="buffAnimalSenior" />   
		<triggered_effect trigger="onSelfBuffFinish" action="RemoveBuff" target="self" buff="buffAnimalHarvestable" />   

      </effect_group>
    </buff>

In the buffAnimalAdult stage, there's a 25% chance, re-calculated every second, for the cow to be pregnant. If it meets those conditions, it'll trigger the pregnancy buff.

	 <buff name="buffAnimalPregenant" hidden="true">
      <stack_type value="ignore"/>
      <duration value="70"/>
      <effect_group>
		<! Pump the Quest update to see if any of the Objectives have changed -->
        <triggered_effect trigger="onSelfBuffStart" action="PumpQuestSDX, Mods" target="self"  />

		<!-- Add the New Mother buff, which is just a staging buff as a cool down -->
        <triggered_effect trigger="onSelfBuffFinish" action="AddBuff" target="self" buff="buffAnimalNewMother" />

        <!-- Add the Harvestable buff. This is useful, for let's say, a cow, which will start producing milk only after it's had a baby. -->
        <triggered_effect trigger="onSelfBuffFinish" action="AddBuff" target="self" buff="buffAnimalHarvestable" />
      </effect_group>
    </buff>

	<!-- New Mother buff is just a 50 second buff as a middle stage. -->
    <buff name="buffAnimalNewMother" hidden="true">
      <stack_type value="ignore"/>
      <duration value="50"/>
      <effect_group>
   
      </effect_group>
    </buff>'

The last two stages of the Cow's life cycle is handled by the buffAnimalSenior and buffAnimalDeath buffs. During these stages, the cow doesn't contribute anything. It simple consumes food and becomes a burden. During it's final phase, it'll slowly die.

   <buff name="buffAnimalSenior" hidden="true">
      <stack_type value="ignore"/>
      <duration value="180"/>
      <effect_group>
        <triggered_effect trigger="onSelfBuffFinish" action="AddBuff" target="self" buff="buffAnimalDeath" />
      </effect_group>
    </buff>  
    <buff name="buffAnimalDeath" hidden="true">
      <stack_type value="ignore"/>
      <damage_type value="disease"/>
      <duration value="0"/>
      <update_rate value="2"/>

      <effect_group>
        <passive_effect name="HealthChangeOT" operation="base_subtract" value="1,500" duration="0,60"/>
      </effect_group>
    </buff>


While the buffs control the age of the animal, it doesn't really do much to produce anything of substance. However, if you combine it with the new QuestSDX hooks, the animal can become more alive.

Here's an example of a Quest line that handles the cow's pregnancy cycle:

    <quest id="farmingAnimal_Cow_Pregnancy">
      <property name="name_key" value="farmingAnimal_Cow_Pregnancy" />
      <property name="subtitle_key" value="farmingAnimal_Cow_Pregnancy" />
      <property name="description_key" value="farmingAnimal_Cow_Pregnancy" />
      <property name="icon" value="ui_game_symbol_zombie" />
      <property name="repeatable" value="true" />
      <property name="category_key" value="challenge" />
      <property name="offer_key" value="farmingAnimal_Cow_Pregnancy" />
      <property name="difficulty" value="veryeasy" />

      <!-- Before progressing, make sure the Animal has the buffAnimalAdult buff -->
      <objective type="BuffSDX, Mods">
        <property name="phase" value="1" />
        <property name="buff" value="buffAnimalAdult" />
      </objective>

      <!-- Before progressing, make sure the animal has a buffAnimalPregnant buff -->
      <objective type="BuffSDX, Mods">
        <property name="phase" value="2" />
        <property name="buff" value="buffAnimalPrgenant" />
      </objective>

      <!-- Before progressing, make sure the animal has a buffAnimalNewMother buff -->
      <objective type="BuffSDX, Mods">
        <property name="phase" value="3" />
        <property name="buff" value="buffAnimalNewMother" />
      </objective>
      
      <!-- This will spawn in an animlFarmCow entity during Phase 3 -->
      <action type="SpawnEntitySDX, Mods" id="animalFarmCow" value="1" phase="3" />
      
      <!-- Once the entity is spawned, give back the same quest, and give it a casino coin for a good job. -->
      <reward type="QuestSDX, Mods" id="farmingAnimal_Cow_Pregnancy" />
      <reward type="ItemSDX, Mods" id="casinoCoin" value="1" />

    </quest>

	
