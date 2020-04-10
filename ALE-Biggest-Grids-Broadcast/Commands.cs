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

            SendGridsInternal(true, false);

            Context.Respond("Biggest grid GPS added!");
        }

        [Command("sendfargps", "Sends Top X furthest Grids from center to all Players!")]
        [Permission(MyPromoteLevel.Admin)]
        public void SendFurthestGrids() {

            SendGridsInternal(false, true);

            Context.Respond("Furthest grid GPS added!");
        }

        [Command("sendmixgps", "Sends Top X defined Grids from center to all Players!")]
        [Permission(MyPromoteLevel.Admin)]
        public void SendGrids(bool biggest, bool furthest) {

            SendGridsInternal(biggest, furthest);

            Context.Respond("Defined grid GPS added!");
        }

        private void SendGridsInternal(bool biggest, bool furthest) {

            Plugin.RemoveGpsFromAllPlayers();

            HashSet<MyGps> gpsSet = new HashSet<MyGps>();
            long seconds = GetTimeMs();

            if (biggest)
                gpsSet.UnionWith(FindGrids(BiggestGridDetectionStrategy.INSTANCE, Plugin.MinPCU, Plugin.MaxDistancePlayersBiggest, Plugin.IgnoreOfflineBiggest, Plugin.IgnoreNPCs, seconds));

            if(furthest)
                gpsSet.UnionWith(FindGrids(FurthestGridDetectionStrategy.INSTANCE, Plugin.MinDistance, Plugin.MaxDistancePlayersFurthest, Plugin.IgnoreOfflineFurthest, Plugin.IgnoreNPCs, seconds));

            if (gpsSet.Count > 0)
                SendGps(gpsSet);
        }

        private List<MyGps> FindGrids(GridDetectionStrategy gridDetectionStrategy, int min, int distance, bool ignoreOffline, bool ignoreNpcs, long seconds) {

            List<KeyValuePair<long, List<MyCubeGrid>>> grids = gridDetectionStrategy.FindGrids(Plugin.Config, Plugin.UseConnectedGrids);
            List<KeyValuePair<long, List<MyCubeGrid>>> filteredGrids = gridDetectionStrategy.GetFilteredGrids(grids,
                min, distance, Plugin.TopGrids, ignoreOffline, ignoreNpcs);

            List<MyGps> gpsList = new List<MyGps>();

            int i = 0;

            Color gpsColor = Plugin.GpsColor;

            foreach (KeyValuePair<long, List<MyCubeGrid>> pair in filteredGrids) {

                i++;

                MyCubeGrid grid = pair.Value[0]; /* Cannot be empty because where do the PCUs come from? */

                var position = grid.PositionComp.GetPosition();

                MyGps gps = CreateGps(i, grid, gpsColor, seconds);

                gpsList.Add(gps);
            }

            return gpsList;
        }

        private void SendGps(HashSet<MyGps> gpsSet) {

            foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers())
                foreach (MyGps gps in gpsSet)
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

            Plugin.RemoveGpsFromAllPlayers();

            Context.Respond("Biggest grid GPS removed!");
        }

        [Command("listbiggrids", "Lists the Top X biggest grids (those which would be send to all players)!")]
        [Permission(MyPromoteLevel.Moderator)]
        public void ListBiggestGrids() {
            ListGridsInternal(true, false);
        }

        [Command("listfargrids", "Lists the Top X furthest grids from world center (those which would be send to all players)!")]
        [Permission(MyPromoteLevel.Moderator)]
        public void ListFurthestGrids() {
            ListGridsInternal(false, true);
        }


        [Command("listmixgrids", "Lists the Top X furthest and biggest grids (configurable)!")]
        [Permission(MyPromoteLevel.Moderator)]
        public void ListFurthestGrids(bool biggest, bool furthest) {
            ListGridsInternal(biggest, furthest);
        }

        private void ListGridsInternal(bool biggest, bool furthest) {

            StringBuilder sb = new StringBuilder();
            long seconds = GetTimeMs();

            if (biggest)
                AddGridsToSb(BiggestGridDetectionStrategy.INSTANCE, Plugin.MinPCU, Plugin.MaxDistancePlayersBiggest, Plugin.IgnoreOfflineBiggest,  sb, seconds);

            if (furthest)
                AddGridsToSb(FurthestGridDetectionStrategy.INSTANCE, Plugin.MinDistance, Plugin.MaxDistancePlayersFurthest, Plugin.IgnoreOfflineFurthest, sb, seconds);

            if (Context.Player == null) 
                Context.Respond(sb.ToString());
            else 
                ModCommunication.SendMessageTo(new DialogMessage("List of Grids", "Top " + Plugin.TopGrids + " grids", sb.ToString()), Context.Player.SteamUserId);
        }

        private void AddGridsToSb(GridDetectionStrategy gridDetectionStrategy, int min, int distance, bool ignoreOffline, StringBuilder sb, long seconds) {

            int top = Plugin.TopGrids;
            int playerdistance = distance;
            bool connected = Plugin.UseConnectedGrids;
            bool filterOffline = ignoreOffline;
            bool gps = false;
            bool ignoreNpcs = Plugin.IgnoreNPCs;

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

                if (arg.StartsWith("-ignoreNpcs=")) {

                    string localArg = arg.Replace("-ignoreNpcs=", "");
                    bool.TryParse(localArg, out ignoreNpcs);
                }
            }

            var PluginConfig = Plugin.Config;

            List<KeyValuePair<long, List<MyCubeGrid>>> grids = gridDetectionStrategy.FindGrids(PluginConfig, connected);
            List<KeyValuePair<long, List<MyCubeGrid>>> filteredGrids = gridDetectionStrategy.GetFilteredGrids(grids, min, playerdistance, top, filterOffline, ignoreNpcs);

            sb.AppendLine("[Top " + top + " grids by " + gridDetectionStrategy.GetUnitName()+"]");
            sb.AppendLine();
            sb.AppendLine("Used Settings");
            sb.AppendLine("---------------------------------------");
            sb.AppendLine("Number of grids: " + top);
            sb.AppendLine("Player distance: " + playerdistance);
            sb.AppendLine("Min "+gridDetectionStrategy.GetUnitName()+": "+ min);
            sb.AppendLine("Show Offline: " + !filterOffline);
            sb.AppendLine("Show NPCs: " + !ignoreNpcs);
            sb.AppendLine("Include: "+ (!connected ? "Phyiscal Connections (Rotors, Pistons)" : "Mechanical Connections (Rotors, Pistons, Connectors)"));
            sb.AppendLine("Center: " + PluginConfig.CenterX + ", " + PluginConfig.CenterY + ", " + PluginConfig.CenterZ);

            sb.AppendLine();

            sb.AppendLine("Result");
            sb.AppendLine("---------------------------------------");

            int i = 0;

            Color gpsColor = Plugin.GpsColor;

            if (gps && Context.Player != null)
                Plugin.RemoveGpsFromPlayer(Context.Player.IdentityId);

            if(filteredGrids.Count == 0)
                sb.AppendLine("-");

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

            sb.AppendLine();
            sb.AppendLine();
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
