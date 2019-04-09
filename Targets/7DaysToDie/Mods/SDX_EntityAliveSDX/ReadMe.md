SDX EntityAlive
==================

This project contains a few different classes to enable more flexible NPCs and animals.

EntityAliveSDX
--------------

This is the new base class, which inherits from EntityNPC and all the sub classes. It has new features, such as allowing them to be hired and execute you orders.

NPCs can accept orders by interacting with them and selecting one of pre-made options. Using a combination of XML dialog settings and code, some dialog options will not appear unless they make sense. For example, 
when you first meet an NPC, you will not be able to give them orders. 

NPCs can be hired either by paying their Hire Cost, completing a quest line, or have some sort of incenntive for them to follow.

##Hiring NPCs##

Each unique NPC can have its own hiring cost and currency, available through the XML properties.  By default, this is 1000 casino Coins. The HireCurrency is any available Item, and that item will be removed 
from your inventory when you hire them. 

####Examples:####

Default:
~~~~~~~~~~~~~~~~~{.xml}
 <property name="HireCost" value="1000"/>
 <property name="HireCurrency" value="casinoCoin"/>
~~~~~~~~~~~~~~~~~

Unique Item:
~~~~~~~~~~~~~~~~~{.xml}
 <property name="HireCost" value="1"/>
 <property name="HireCurrency" value="BloomsFamilyHeirloom_1"/>
~~~~~~~~~~~~~~~~~

Currently, all NPCs are permanent hires. Once NPCs are hired, your entity ID is stored with the entity as a cvar "Leader". As soon as they are hired, they'll begin to follow you, and try to keep pace with your speed.

Initially, unhired NPCs will only show a few options in the dialog. However, when they are hired, they can do more for you.

###Orders###

\li ShowMe - This displays information about the entity in a tool tip window, and in the console. This will show you their health, name, and hunger levels.
\li ShowAffection - This just displays a tool tip. TODO: have it positively impact the entity
\li FollowMe  - The entity will follow you, keeping a distance of 3 to 5 spaces behind you. They'll try to match your walk and run speed.
\li Stayhere - The entity will stay in place
\li GuardHere - The entity will stand in your place, facing your initial direction. This will set the GuardPosition, allow the guard to return to that spot after a fight.
\li Wander - The entity will walk around the area
\li SetPatrol - The entity will begin following your foot steps, recording them as Patrol Coordinates
\li Patrol - The entity, if it has Patrol Coordinates, will begin tracing the coordinates back and forth. This option only shows if it has Patrol Coordinates
\li Hire - If unhired, the entity will allow you to hire them. Their cost depends on their XML settings.
\li OpenInventory - This will open their inventory
\li Loot - This tells the entity to Loot the POI you are in now.

Options can be filtered through using the SDX_Dialog class that adds new conditions to show statements based on if they are hired. Not all NPCs need to support all the tasks.

Here's a sample Dialog XML that show's the Hire option, if the NPC is not hired, and will show ShowMe and StayHere orders if it is hired.

####Sample Dialog####

~~~~~~~~~~~~~~~~~{.xml}
<response id="Hire" text="I am interested in hiring you." >
	<requirement type="HiredSDX, Mods" requirementtype="Hide" value="not"/>
	<action type="OpenDialogSDX, Mods" id="Hire" />
</response>
      
<response id="FollowMe" text="Follow me" >
	<requirement type="HiredSDX, Mods" requirementtype="Hide"/>
	<action type="ExecuteCommandSDX, Mods" id="FollowMe" />
</response>

<response id="ShowMe" text="Show Me your inventory" >
	<requirement type="HiredSDX, Mods" requirementtype="Hide" />
	<action type="ExecuteCommandSDX, Mods" id="OpenInventory" />
</response>

<response id="StayHere" text="Stay here" >
	<requirement type="HiredSDX, Mods" requirementtype="Hide" />
	<action type="ExecuteCommandSDX, Mods" id="StayHere" />
</response>
~~~~~~~~~~~~~~~~~



