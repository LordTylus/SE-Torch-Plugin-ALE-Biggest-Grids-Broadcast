using NLog;
using Sandbox.Game.Entities;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using Torch.Commands;
using Torch.Commands.Permissions;
using Torch.Mod;
using Torch.Mod.Messages;
using VRage.Game.ModAPI;
using VRageMath;

namespace ALE_Biggest_Grids_Broadcast {

    public class Commands : CommandModule {

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public GridsBroadcastPlugin Plugin => (GridsBroadcastPlugin) Context.Plugin;

        [Command("sendbiggps", "Sends Top X biggest Grids to all Players!")]
        [Permission(MyPromoteLevel.Admin)]
        public void SendBiggestGrids() {

            SendGrids(BiggestGridDetectionStrategy.INSTANCE, Plugin.MinPCU);

            Context.Respond("Biggest grid GPS added!");
        }

        [Command("sendfargps", "Sends Top X furthest Grids from center to all Players!")]
        [Permission(MyPromoteLevel.Admin)]
        public void SendFurthestGrids() {

            SendGrids(FurthestGridDetectionStrategy.INSTANCE, Plugin.MinDistance);

            Context.Respond("Furthest grid GPS added!");
        }

        private void SendGrids(GridDetectionStrategy gridDetectionStrategy, int min) {

            Plugin.removeGpsFromAllPlayers();

            List<KeyValuePair<long, List<MyCubeGrid>>> grids = gridDetectionStrategy.FindGrids(Plugin.UseConnectedGrids);
            List<KeyValuePair<long, List<MyCubeGrid>>> filteredGrids = gridDetectionStrategy.GetFilteredGrids(grids,
                min, Plugin.MaxDistancePlayers, Plugin.TopGrids, true);

            List<MyGps> gpsList = new List<MyGps>();

            long seconds = GetTimeMs();

            int i = 0;

            Color gpsColor = Plugin.GpsColor;

            foreach (KeyValuePair<long, List<MyCubeGrid>> pair in filteredGrids) {

                i++;

                MyCubeGrid grid = pair.Value[0]; /* Cannot be empty because where do the PCUs come from? */

                var position = grid.PositionComp.GetPosition();

                MyGps gps = CreateGps(i, grid, gpsColor, seconds);

                gpsList.Add(gps);
            }

            foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers())
                foreach (MyGps gps in gpsList)
                    MyAPIGateway.Session?.GPS.AddGps(player.Identity.IdentityId, gps);
        }

        [Command("removebiggps", "obsolete use !removegps instead")]
        [Permission(MyPromoteLevel.Admin)]
        public void Removebiggrids() {
            Removegps();
        }

        [Command("removegps", "Deletes active biggest and/or furthest grids coordinates from all players!")]
        [Permission(MyPromoteLevel.Admin)]
        public void Removegps() {

            Plugin.removeGpsFromAllPlayers();

            Context.Respond("Biggest grid GPS removed!");
        }

        [Command("listbiggrids", "Lists the Top X biggest grids (those which would be send to all players)!")]
        [Permission(MyPromoteLevel.Moderator)]
        public void ListBiggestGrids() {
            ListGrids(BiggestGridDetectionStrategy.INSTANCE, Plugin.MinPCU);
        }

        [Command("listfargrids", "Lists the Top X furthest grids from world center (those which would be send to all players)!")]
        [Permission(MyPromoteLevel.Moderator)]
        public void ListFurthestGrids() {
            ListGrids(FurthestGridDetectionStrategy.INSTANCE, Plugin.MinDistance);
        }

        private void ListGrids(GridDetectionStrategy gridDetectionStrategy, int min) {

            int top = Plugin.TopGrids;
            int playerdistance = Plugin.MaxDistancePlayers;
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

                if (arg.StartsWith("-playerdistance=")) {

                    string localArg = arg.Replace("-playerdistance=", "");
                    int.TryParse(localArg, out playerdistance);
                }
            }

            List<KeyValuePair<long, List<MyCubeGrid>>> grids = gridDetectionStrategy.FindGrids(connected);
            List<KeyValuePair<long, List<MyCubeGrid>>> filteredGrids = gridDetectionStrategy.GetFilteredGrids(grids, min, playerdistance, top, filterOffline);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Used Settings");
            sb.AppendLine("---------------------------------------");
            sb.AppendLine("Number of grids: " + top);
            sb.AppendLine("Player distance: " + playerdistance);
            sb.AppendLine("Min "+gridDetectionStrategy.GetUnitName()+": "+ min);
            sb.AppendLine("Include: "+ (!connected ? "Phyiscal Connections (Rotors, Pistons)" : "Mechanical Connections (Rotors, Pistons, Connectors)"));
            sb.AppendLine();

            sb.AppendLine("Result");
            sb.AppendLine("---------------------------------------");

            long seconds = GetTimeMs();

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

                string factionString = "";
                if (faction != null)
                    factionString = "[" + faction.Tag + "]";


                sb.AppendLine(i+". "+pair.Key.ToString("#,##0") + " "+ gridDetectionStrategy.GetUnitName() + " - "+ biggestGrid.DisplayName);
                sb.AppendLine("   "+pair.Value.Count +" Grids.");
                sb.AppendLine("   Owned by: " + GetPlayerNameById(gridOwner) +" "+ factionString);

                MyCubeGrid grid = pair.Value[0]; /* Cannot be empty because where do the PCUs come from? */

                var position = grid.PositionComp.GetPosition();

                sb.AppendLine($"   X: {position.X.ToString("#,##0.00")}, Y: {position.Y.ToString("#,##0.00")}, Z: {position.Z.ToString("#,##0.00")}");

                if (gps && Context.Player != null) {

                    MyGps gridGPS = CreateGps(i, grid, gpsColor, seconds);

                    MyAPIGateway.Session?.GPS.AddGps(Context.Player.IdentityId, gridGPS);
                }
            }

            if (Context.Player == null) {

                Context.Respond("Top " + top + " grids by "+ gridDetectionStrategy.GetUnitName());
                Context.Respond(sb.ToString());

            } else {

                ModCommunication.SendMessageTo(new DialogMessage("List of Grids", "Top "+top+" grids by "+ gridDetectionStrategy.GetUnitName(), sb.ToString()), Context.Player.SteamUserId);
            }
        }

        private long GetTimeMs() {
            return (long)(DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        private MyGps CreateGps(int index, MyCubeGrid grid, Color gpsColor, long seconds) {

            MyGps gps = new MyGps {
                Coords = grid.PositionComp.GetPosition(),
                Name = "Top Grid - " + grid.DisplayName + " " + seconds,
                DisplayName = "Top Grid: " + grid.DisplayName,
                Description = ($"Top Grid: Grid currently in Top {index} by {Plugin.GpsIdentifierName}"),
                GPSColor = gpsColor,
                IsContainerGPS = true,
                ShowOnHud = true,
                DiscardAt = new TimeSpan?()
            };
            gps.UpdateHash();

            return gps;
        }

        public static MyIdentity GetIdentityById(long playerId) {

            foreach (var identity in MySession.Static.Players.GetAllIdentities())
                if (identity.IdentityId == playerId)
                    return identity;

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
