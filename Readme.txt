### Introduction
If you are running a PVP server you may or may not need a bunch of Mods in order for players to find each other. Especially when planets are in the world you cannot really limit the worlds size and therefore players can hide themselves in deep space while building monstrosities that will never see any actual combat. 

This Plugin is one way to help you dealing with that. It adds functionality to the game to determine the biggest grids by PCU and broadcast them to all players on the server. So people are more likely to be found. Alternatively you can also broadcast the grids which are furthest away from world center. 

### How does it work?

First of all the Plugin only adds the functionality. If you want to take Advantage of it in an automated way **I recommend using the Essentials mod and setting up Auto-Commands**. (Example Config and Commands are found below).

The Plugin itself detects the PCU / or distance of each and every grid on the server and broadcasts a configurable amount of them to all Players on the server. Grids are only broadcasted when:

- Owner or Factionmember is online
- Owner or Factionmember is within configured range
- Grid has at least an configured amount of PCU or distance to center
- Grid is owned by an actual Player

The default config broadcasts the Top 10 grids which have at least 5000 PCU with a Player within 1km of range. 

The limit for min PCU makes sure that new players which basically only have a small base or starter ship to not being attacked immediately. 

If the biggest grid has no player online or otherwise close by the next available grid will take its place on the top spot in the list. 

### Commands

There are 3 Commands:

- !listbiggrids
 - Shows a list of the biggest Grids by pcu that would be send to all players. 
- !sendbiggps
 - Sends the detected top X biggest grids by pcu to all players
- !listbigblockgrids
 - Shows a list of the biggest Grids by blockcount that would be send to all players. 
- !sendbigblockgps
 - Sends the detected top X biggest grids by blockcount to all players
- !listfargrids
 - Shows a list of the furthest Grids that would be send to all players. 
- !sendfargps
 - Sends the detected top X furthest grids from world center to all players
- !listabandonedgrids
 - Shows a list of the abandoned Grids that would be send to all players. 
- !sendabandonedgps
 - Sends the detected top X furthest grids from world center to all players
- !sendmixgps &lt;biggest true|false&gt; &lt;furthest true|false&gt; &lt;abandoned true|false&gt; &lt;biggest by blocks true|false&gt;
 - Sends top X biggest and/or furthest grids depending on your config. Basically its like using !sendbiggps and !sendfargps individually but without deleting the gps coords in between. 
- !listmixgrids &lt;biggest by pcu true|false&gt; &lt;furthest true|false&gt; &lt;abandoned true|false&gt; &lt;biggest by blocks true|false&gt;
 - Shows a list of the top X biggest and/or furthest grids depending on your config.
- !removegps
 - Removes said GPS again. 

### Configuration

There are the following configuration methods:

#### UI

- Broadcast tp X grids (Default 10)
 - How many grids will be broadcasted
- Only check grids where factionmember is in radius of (m) (Default 1000)
 - Distance in meters where owner or factionmember must be for this grid to be considered
 - For offline protection reasons
- Show only grids with min PCU (Default 5000)
 - Distance in meters of how many PCU a grid must have in order to be relevant.
 - For new player protection
- Include connected grids (Default false)
 - If false only grids and subgrids via rotor or piston are checked. If true also drillships connected via connector are relevant. 
- Gps Marker Identifier (Default Doom Plugin)
 - Just a name that appears in the description of the GPS. Its used for identifying the GPS on delete again.
- Remove GPS on player join (Default true)
 - if true when a player disconnects the gps are removed from him again. if false even after reconnect the GPS will still be present.

#### XML

&lt;?xml version="1.0" encoding="utf-8"?&gt;

&lt;GridsBroadcastConfig xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"&gt;

  &lt;TopGrids&gt;10&lt;/TopGrids&gt;

  &lt;MaxDistancePlayers&gt;1000&lt;/MaxDistancePlayers&gt;

  &lt;UseConnectedGrids&gt;false&lt;/UseConnectedGrids&gt;

  &lt;MinPCU&gt;5000&lt;/MinPCU&gt;

  &lt;RemoveGpsOnJoin&gt;true&lt;/RemoveGpsOnJoin&gt;

  &lt;GpsIdentifierName&gt;Doom Plugin&lt;/GpsIdentifierName&gt;

&lt;/GridsBroadcastConfig&gt;

### Auto Command Example

When setting up with auto commands there are the following options:

#### Delayed execution

By that the GPS will be sent out daily at 7PM and removed from every player 30 minutes after. 

&lt;AutoCommand&gt;

&lt;CommandTrigger&gt;Scheduled&lt;/CommandTrigger&gt;

&lt;Name&gt;Broadcast&lt;/Name&gt;

&lt;ScheduledTime&gt;19:00:00&lt;/ScheduledTime&gt;

&lt;Interval&gt;00:00:00&lt;/Interval&gt;

&lt;TriggerRatio&gt;0&lt;/TriggerRatio&gt;

&lt;TriggerCount&gt;0&lt;/TriggerCount&gt;

&lt;DayOfWeek&gt;All&lt;/DayOfWeek&gt;

&lt;Steps&gt;

&lt;CommandStep&gt;

&lt;Delay&gt;00:30:00&lt;/Delay&gt;

&lt;Command&gt;!sendbiggps&lt;/Command&gt;

&lt;/CommandStep&gt;

&lt;CommandStep&gt;

&lt;Delay&gt;00:00:00&lt;/Delay&gt;

&lt;Command&gt;!removebiggps&lt;/Command&gt;

&lt;/CommandStep&gt;

&lt;/Steps&gt;

&lt;/AutoCommand&gt;

#### Scheduled execution

The first one surely works fine, but has the risk of not removing the GPS when the server crashes. If remove on join is enabled its not a problem at all. If its not enabled you may consider using 2 commands:

&lt;AutoCommand&gt;

&lt;CommandTrigger&gt;Scheduled&lt;/CommandTrigger&gt;

&lt;Name&gt;Broadcast&lt;/Name&gt;

&lt;ScheduledTime&gt;19:00:00&lt;/ScheduledTime&gt;

&lt;Interval&gt;00:00:00&lt;/Interval&gt;

&lt;TriggerRatio&gt;0&lt;/TriggerRatio&gt;

&lt;TriggerCount&gt;0&lt;/TriggerCount&gt;

&lt;DayOfWeek&gt;All&lt;/DayOfWeek&gt;

&lt;Steps&gt;

&lt;CommandStep&gt;

&lt;Delay&gt;00:00:00&lt;/Delay&gt;

&lt;Command&gt;!sendbiggps&lt;/Command&gt;

&lt;/CommandStep&gt;

&lt;/Steps&gt;

&lt;/AutoCommand&gt;

&lt;AutoCommand&gt;

&lt;CommandTrigger&gt;Scheduled&lt;/CommandTrigger&gt;

&lt;Name&gt;Remove&lt;/Name&gt;

&lt;ScheduledTime&gt;19:30:00&lt;/ScheduledTime&gt;

&lt;Interval&gt;00:00:00&lt;/Interval&gt;

&lt;TriggerRatio&gt;0&lt;/TriggerRatio&gt;

&lt;TriggerCount&gt;0&lt;/TriggerCount&gt;

&lt;DayOfWeek&gt;All&lt;/DayOfWeek&gt;

&lt;Steps&gt;

&lt;CommandStep&gt;

&lt;Delay&gt;00:00:00&lt;/Delay&gt;

&lt;Command&gt;!removebiggps&lt;/Command&gt;

&lt;/CommandStep&gt;

&lt;/Steps&gt;

&lt;/AutoCommand&gt;

### Github
[See Here](https://github.com/LordTylus/SE-Torch-Plugin-ALE-Biggest-Grids-Broadcast)