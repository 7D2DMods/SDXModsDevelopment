SDX_Dialog
==========

The SDX Dialog adds in new actions and triggers used for the dialogs.xml, as well as include new UI controllers.


ExecuteCommandSDX, Mods
=======================

This class executes commands in the EntityAliveSDX.cs class. These commands must be added to the switch statement in EntityAliveSDX.cs to be effective.

Example Usage:

      <response id="FollowMe" text="Follow me" >
        <requirement type="HiredSDX, Mods" requirementtype="Hide"/>
        <action type="ExecuteCommandSDX, Mods" id="FollowMe" />
      </response>


OpenDialogSDX, Mods
===================

This class allows us to open up a new window

Example Usage:

     <response id="Hire" text="I am interested in hiring you." >
        <requirement type="HiredSDX, Mods" requirementtype="Hide" value="not"/>
        <action type="OpenDialogSDX, Mods" id="Hire" />
      </response>

HireSDX, Mods
=============

This class allows us to filter on if the statement should be visible, depending on if its hired or not.

Example Usage:
      <response id="FollowMe" text="Follow me" >
        <requirement type="HiredSDX, Mods" requirementtype="Hide"/>  <!-- This hides the entry if its not Hired. -->
        <action type="ExecuteCommandSDX, Mods" id="FollowMe" />
      </response>

	    <response id="Hire" text="I am interested in hiring you." >
        <requirement type="HiredSDX, Mods" requirementtype="Hide" value="not"/> <!-- The value="not" flips the condition, so will only show if its not hired. -->
        <action type="OpenDialogSDX, Mods" id="Hire" />
      </response>

PatrolSDX, Mods
===============

This class shows or hides if the "Patrol" option is visible. If it does not have Patrol Coordinates (provided through the "Follow me for your patrol route" task"), then it won't show.'

Example Usage:

     <response id="Patrol" text="Patrol your route" >
        <requirement type="PatrolSDX, Mods" requirementtype="Hide" />        
        <action type="ExecuteCommandSDX, Mods" id="Patrol" />
      </response>


