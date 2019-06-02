using NLog;
using Sandbox.Game.Entities;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch.Commands;
using Torch.Commands.Permissions;
using Torch.Mod;
using Torch.Mod.Messages;
using VRage.Collections;
using VRage.Game.ModAPI;
using VRage.Groups;
using VRageMath;

namespace ALE_Biggest_Grids_Broadcast {

    public class Commands : CommandModule {

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public GridsBroadcastPlugin Plugin => (GridsBroadcastPlugin) Context.Plugin;

        [Command("sendbiggps", "Sends Top X biggest Grids to all Players!")]
        [Permission(MyPromoteLevel.Admin)]
        public void SendBiggestGrids() {

            Plugin.removeGpsFromAllPlayers();

            List<KeyValuePair<long, List<MyCubeGrid>>> grids = FindGrids(Plugin.UseConnectedGrids);
            List<KeyValuePair<long, List<MyCubeGrid>>> filteredGrids = GetFilteredGrids(grids, 
                Plugin.MinPCU, Plugin.MaxDistancePlayers, Plugin.TopGrids, true);

            List<MyGps> gpsList = new List<MyGps>();

            long seconds = getTimeMs();

            int i = 0;

            Color gpsColor = Plugin.GpsColor;

            foreach (KeyValuePair<long, List<MyCubeGrid>> pair in filteredGrids) {

                i++;

                MyCubeGrid grid = pair.Value[0]; /* Cannot be empty because where do the PCUs come from? */

                var position = grid.PositionComp.GetPosition();

                MyGps gps = createGps(i, grid, gpsColor, seconds);

                gpsList.Add(gps);
            }

            foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers()) 
                foreach (MyGps gps in gpsList)
                    MyAPIGateway.Session?.GPS.AddGps(player.Identity.IdentityId, gps);

            Context.Respond("Biggest grid GPS added!");
        }

        [Command("removebiggps", "Deletes active biggest grids coordinates from all players!")]
        [Permission(MyPromoteLevel.Admin)]
        public void Removebiggrids() {

            Plugin.removeGpsFromAllPlayers();

            Context.Respond("Biggest grid GPS removed!");
        }

        [Command("listbiggrids", "Lists the Top X biggest grids (those who would be send to all players)!")]
        [Permission(MyPromoteLevel.Moderator)]
        public void ListBiggestGrids() {

            int top = Plugin.TopGrids;
            int distance = Plugin.MaxDistancePlayers;
            int min = Plugin.MinPCU;
            bool connected = Plugin.UseConnectedGrids;
            bool filterOffline = true;
            bool gps = false;

            List<string> args = Context.Args;
            foreach(string arg in args) {

                if (arg == "-phsical")
                    connected = true;

                if (arg == "-mechanical")
                    connected = false;

                if (arg == "-gps")
                    gps = true;

                if (arg == "-showOffline")
                    filterOffline = false;

                if (arg.StartsWith("-top=")) {

                    string localArg = arg.Replace("-top=", "");
                    int.TryParse(localArg, out top);
                }

                if (arg.StartsWith("-min=")) {

                    string localArg = arg.Replace("-min=", "");
                    int.TryParse(localArg, out min);
                }

                if (arg.StartsWith("-distance=")) {

                    string localArg = arg.Replace("-distance=", "");
                    int.TryParse(localArg, out distance);
                }
            }

            List<KeyValuePair<long, List<MyCubeGrid>>> grids = FindGrids(connected);
            List<KeyValuePair<long, List<MyCubeGrid>>> filteredGrids = GetFilteredGrids(grids, min, distance, top, filterOffline);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Used Settings");
            sb.AppendLine("---------------------------------------");
            sb.AppendLine("Number of grids: " + top);
            sb.AppendLine("Player distance: " + distance);
            sb.AppendLine("Min PCU: "+ min);
            sb.AppendLine("Include: "+ (!connected ? "Phyiscal Connections (Rotors, Pistons)" : "Mechanical Connections (Rotors, Pistons, Connectors)"));
            sb.AppendLine();

            sb.AppendLine("Result");
            sb.AppendLine("---------------------------------------");

            long seconds = getTimeMs();

            int i = 0;

            Color gpsColor = Plugin.GpsColor;

            if (gps && Context.Player != null)
                Plugin.removeGpsFromPlayer(Context.Player.IdentityId);

            foreach (KeyValuePair<long, List<MyCubeGrid>> pair in filteredGrids) {

                i++;

                MyCubeGrid biggestGrid = null;
                double num = 0;

                foreach (MyCubeGrid cubeGrid in pair.Value) {

                    if (cubeGrid.Physics == null)
                        continue;

                    double volume = cubeGrid.PositionComp.WorldAABB.Size.Volume;
                    if (volume > num) {
                        num = volume;
                        biggestGrid = cubeGrid;
                    }
                }

                var gridOwnerList = biggestGrid.BigOwners;
                var ownerCnt = gridOwnerList.Count;
                var gridOwner = 0L;

                if (ownerCnt > 0 && gridOwnerList[0] != 0)
                    gridOwner = gridOwnerList[0];
                else if (ownerCnt > 1)
                    gridOwner = gridOwnerList[1];

                IMyFaction faction = GetFactionForPlayer(gridOwner);

                String factionString = "";
                if (faction != null)
                    factionString = "[" + faction.Tag + "]";


                sb.AppendLine(i+". "+pair.Key.ToString("#,##0") + " PCU"+" - "+ biggestGrid.DisplayName);
                sb.AppendLine("   "+pair.Value.Count +" Grids.");
                sb.AppendLine("   Owned by: " + GetPlayerNameById(gridOwner) +" "+ factionString);

                MyCubeGrid grid = pair.Value[0]; /* Cannot be empty because where do the PCUs come from? */

                var position = grid.PositionComp.GetPosition();

                sb.AppendLine($"   X: {position.X.ToString("#,##0.00")}, Y: {position.Y.ToString("#,##0.00")}, Z: {position.Z.ToString("#,##0.00")}");

                if (gps && Context.Player != null) {

                    MyGps gridGPS = createGps(i, grid, gpsColor, seconds);

                    MyAPIGateway.Session?.GPS.AddGps(Context.Player.IdentityId, gridGPS);
                }
            }

            if (Context.Player == null) {

                Context.Respond("Top " + top + " grids by PCU");
                Context.Respond(sb.ToString());

            } else {

                ModCommunication.SendMessageTo(new DialogMessage("List of Biggest Grids", "Top "+top+" grids by PCU", sb.ToString()), Context.Player.SteamUserId);
            }
        }

        public List<KeyValuePair<long, List<MyCubeGrid>>> FindGrids(bool connected) {

            StringBuilder sb = new StringBuilder();

            List<KeyValuePair<long, List<MyCubeGrid>>> gridsList = new List<KeyValuePair<long, List<MyCubeGrid>>>();

            if (connected) {

                foreach (var group in MyCubeGridGroups.Static.Physical.Groups)
                    gridsList.Add(CheckGroupsPcu(group.Nodes));

            } else {

                foreach (var group in MyCubeGridGroups.Static.Mechanical.Groups)
                    gridsList.Add(CheckGroupsPcu(group.Nodes));
            }

            gridsList.Sort(delegate (KeyValuePair<long, List<MyCubeGrid>> pair1, KeyValuePair<long, List<MyCubeGrid>> pair2) {
                return pair2.Key.CompareTo(pair1.Key);
            });

            return gridsList;
        }

        public KeyValuePair<long, List<MyCubeGrid>> CheckGroupsPcu(HashSetReader<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node> nodes) {

            List<MyCubeGrid> gridsList = new List<MyCubeGrid>();
            long pcu = 0;

            foreach (var groupNodes in nodes) {

                MyCubeGrid cubeGrid = groupNodes.NodeData;

                if (cubeGrid.Physics == null)
                    continue;

                gridsList.Add(cubeGrid);

                pcu += cubeGrid.BlocksPCU;
            }

            return new KeyValuePair<long, List<MyCubeGrid>>(pcu, gridsList);
        }

        public KeyValuePair<long, List<MyCubeGrid>> CheckGroupsPcu(HashSetReader<MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Node> nodes) {

            List<MyCubeGrid> gridsList = new List<MyCubeGrid>();
            long pcu = 0;

            foreach (var groupNodes in nodes) {

                MyCubeGrid cubeGrid = groupNodes.NodeData;

                if (cubeGrid.Physics == null)
                    continue;

                gridsList.Add(cubeGrid);

                pcu += cubeGrid.BlocksPCU;
            }

            return new KeyValuePair<long, List<MyCubeGrid>>(pcu, gridsList);
        }

        private List<KeyValuePair<long, List<MyCubeGrid>>> GetFilteredGrids(
            List<KeyValuePair<long, List<MyCubeGrid>>> sortedGrids, int min, int distance, int top, bool filterOffline) {

            List<KeyValuePair<long, List<MyCubeGrid>>> gridsList = new List<KeyValuePair<long, List<MyCubeGrid>>>();

            foreach(KeyValuePair<long, List<MyCubeGrid>> pair in sortedGrids) {

                if (pair.Key == 0)
                    continue;

                /* Too little PCU ignore */
                if (pair.Key < min)
                    continue;

                bool relevant = CheckIfGridsAreRelevant(pair.Value, distance, filterOffline);

                if (relevant)
                    gridsList.Add(pair);

                if (gridsList.Count == top)
                    break;
            }

            return gridsList;
        }

        private bool CheckIfGridsAreRelevant(List<MyCubeGrid> grids, int distance, bool filterOffline) {

            int minDistanceSquared = distance * distance;

            foreach (MyCubeGrid grid in grids) {

                var gridOwnerList = grid.BigOwners;
                var ownerCnt = gridOwnerList.Count;
                var gridOwner = 0L;

                if (ownerCnt > 0 && gridOwnerList[0] != 0)
                    gridOwner = gridOwnerList[0];
                else if (ownerCnt > 1)
                    gridOwner = gridOwnerList[1];

                if (gridOwner == 0)
                    continue;

                MyFaction faction = MySession.Static.Factions.GetPlayerFaction(gridOwner);

                Vector3D gridPosition = grid.PositionComp.GetPosition();

                if(faction != null) {

                    foreach(long factionMember in faction.Members.Keys) {

                        MyPlayer member = GetPlayerById(factionMember);
                        if (filterOffline && member == null)
                            continue;

                        if (!filterOffline)
                            return true;

                        Vector3D position = member.GetPosition();

                        if (Vector3D.DistanceSquared(gridPosition, position) <= minDistanceSquared)
                            return true;
                    }

                } else {

                    MyPlayer owner = GetPlayerById(gridOwner);
                    if (owner == null)
                        continue;

                    Vector3D position = owner.GetPosition();

                    if (Vector3D.DistanceSquared(gridPosition, position) <= minDistanceSquared)
                        return true;
                }
            }

            return false;
        }

        private long getTimeMs() {
            return (long)(DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        private MyGps createGps(int index, MyCubeGrid grid, Color gpsColor, long seconds) {

            MyGps gps = new MyGps();
            gps.Coords = grid.PositionComp.GetPosition();
            gps.Name = "Top Grid - " + grid.DisplayName + " " + seconds;
            gps.DisplayName = "Top Grid: " + grid.DisplayName;
            gps.Description = ($"Top Grid: Grid currently in Top {index} by {Plugin.GpsIdentifierName}");
            gps.GPSColor = gpsColor;
            gps.IsContainerGPS = true;
            gps.ShowOnHud = true;
            gps.DiscardAt = new TimeSpan?();
            gps.UpdateHash();

            return gps;
        }

        public static MyIdentity GetIdentityById(long playerId) {

            foreach (var identity in MySession.Static.Players.GetAllIdentities())
                if (identity.IdentityId == playerId)
                    return (MyIdentity)identity;

            return null;
        }

        public static MyPlayer GetPlayerById(long id) {

            if (MySession.Static.Players.TryGetPlayerId(id, out MyPlayer.PlayerId playerId))
                if (MySession.Static.Players.TryGetPlayerById(playerId, out MyPlayer player)) 
                    return player;

            return null;
        }

        public static string GetPlayerNameById(long playerId) {

            MyIdentity identity = GetIdentityById(playerId);

            if (identity != null)
                return identity.DisplayName;

            return "Nobody";
        }

        public static IMyFaction GetFactionForPlayer(long playerId) {
            return MySession.Static.Factions.TryGetPlayerFaction(playerId);
        }
    }
}
