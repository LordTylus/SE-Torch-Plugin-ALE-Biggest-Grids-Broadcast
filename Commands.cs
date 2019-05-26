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

        [Command("sendbiggrids", "Looks for rotorguns on the server!")]
        [Permission(MyPromoteLevel.Owner)]
        public void SendBiggestGrids() {

            List<KeyValuePair<long, List<MyCubeGrid>>> grids = FindGrids();
            List<KeyValuePair<long, List<MyCubeGrid>>> filteredGrids = GetFilteredGrids(grids);

            List<MyGps> gpsList = new List<MyGps>();

            double seconds = (long) (DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;

            int i = 0;

            foreach (KeyValuePair<long, List<MyCubeGrid>> pair in filteredGrids) {

                i++;

                MyCubeGrid grid = pair.Value[0]; /* Cannot be empty because where do the PCUs come from? */

                var position = grid.PositionComp.GetPosition();

                MyGps gps = new MyGps();
                gps.Coords = grid.PositionComp.GetPosition();
                gps.Name = "Top Grid: " + grid.DisplayName+ " " +seconds;
                gps.DisplayName = "Top Grid: " + grid.DisplayName;
                gps.Description = ($"Grid currently in Top {i}");
                gps.GPSColor = new Color(255, 0, 0);
                gps.IsContainerGPS = true;
                gps.ShowOnHud = true;
                gps.DiscardAt = new TimeSpan?();
                gps.UpdateHash();

                gpsList.Add(gps);
            }

            foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers()) 
                foreach (MyGps gps in gpsList)
                    MyAPIGateway.Session?.GPS.AddGps(player.Identity.IdentityId, gps);
        }

        [Command("listbiggrids", "Looks for rotorguns on the server!")]
        [Permission(MyPromoteLevel.Moderator)]
        public void ListBiggestGrids() {

            List<KeyValuePair<long, List<MyCubeGrid>>> grids = FindGrids();
            List<KeyValuePair<long, List<MyCubeGrid>>> filteredGrids = GetFilteredGrids(grids);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Used Settings");
            sb.AppendLine("---------------------------------------");
            sb.AppendLine("Number of grids: " + Plugin.TopGrids);
            sb.AppendLine("Player distance: " + Plugin.MaxDistancePlayers);
            sb.AppendLine("Min PCU: "+ Plugin.MinPCU);
            sb.AppendLine("Include: "+ (!Plugin.UseConnectedGrids ? "Phyiscal Connections (Rotors, Pistons)" : "Mechanical Connections (Rotors, Pistons, Connectors)"));
            sb.AppendLine();

            sb.AppendLine("Result");
            sb.AppendLine("---------------------------------------");

            int i = 0;

            foreach(KeyValuePair<long, List<MyCubeGrid>> pair in filteredGrids) {

                i++;

                sb.AppendLine(i+". "+pair.Key.ToString("#,##0") + " PCU");
                sb.AppendLine("   "+pair.Value.Count +" Grids.");

                MyCubeGrid grid = pair.Value[0]; /* Cannot be empty because where do the PCUs come from? */

                var position = grid.PositionComp.GetPosition();

                sb.AppendLine($"   X: {position.X.ToString("#,##0.00")}, Y: {position.Y.ToString("#,##0.00")}, Z: {position.Z.ToString("#,##0.00")}");
            }

            if (Context.Player == null) {

                Context.Respond("Top " + Plugin.TopGrids + " grids by PCU");
                Context.Respond(sb.ToString());

            } else {

                ModCommunication.SendMessageTo(new DialogMessage("List of Biggest Grids", "Top "+Plugin.TopGrids+" grids by PCU", sb.ToString()), Context.Player.SteamUserId);
            }
        }

        public List<KeyValuePair<long, List<MyCubeGrid>>> FindGrids() {

            StringBuilder sb = new StringBuilder();

            List<KeyValuePair<long, List<MyCubeGrid>>> gridsList = new List<KeyValuePair<long, List<MyCubeGrid>>>();

            if (!Plugin.UseConnectedGrids) {

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

        private List<KeyValuePair<long, List<MyCubeGrid>>> GetFilteredGrids(List<KeyValuePair<long, List<MyCubeGrid>>> sortedGrids) {

            List<KeyValuePair<long, List<MyCubeGrid>>> gridsList = new List<KeyValuePair<long, List<MyCubeGrid>>>();

            foreach(KeyValuePair<long, List<MyCubeGrid>> pair in sortedGrids) {

                if (pair.Key == 0)
                    continue;

                /* Too little PCU ignore */
                if (pair.Key < Plugin.MinPCU)
                    continue;

                bool relevant = CheckIfGridsAreRelevant(pair.Value);

                if (relevant)
                    gridsList.Add(pair);

                if (gridsList.Count == Plugin.TopGrids)
                    break;
            }

            return gridsList;
        }

        private bool CheckIfGridsAreRelevant(List<MyCubeGrid> grids) {

            int minDistanceSquared = Plugin.MaxDistancePlayers * Plugin.MaxDistancePlayers;

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
                        if (member == null)
                            continue;

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

        public static MyPlayer GetPlayerById(long id) {

            if (MySession.Static.Players.TryGetPlayerId(id, out MyPlayer.PlayerId playerId))
                if (MySession.Static.Players.TryGetPlayerById(playerId, out MyPlayer player)) 
                    return player;

            return null;
        }
    }
}
