using ALE_Core.Utils;
using Sandbox.Game.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Collections;
using VRage.Groups;

namespace ALE_Biggest_Grids_Broadcast.GridDetection {
    internal class AbandonedGridDetectionStrategy : AbstractGridDetectionStrategy {

        public static readonly AbandonedGridDetectionStrategy INSTANCE = new AbandonedGridDetectionStrategy();

        private AbandonedGridDetectionStrategy() {

        }

        public override string GetStrategyName() {
            return "Inactive Grid";
        }

        public override string GetUnitName() {
            return "Days";
        }

        public override List<KeyValuePair<long, List<MyCubeGrid>>> FindGrids(GridsBroadcastConfig config, bool connected) {

            List<KeyValuePair<long, List<MyCubeGrid>>> gridsList = new List<KeyValuePair<long, List<MyCubeGrid>>>();

            bool checkFaction = config.MinDaysFactionCheck;

            DateTime today = DateTime.Today;

            foreach (var group in MyCubeGridGroups.Static.Mechanical.Groups) {

                var relevantGrids = CheckGroupsDays(group.Nodes, checkFaction, today);

                if(relevantGrids.Value.Count > 0)
                    gridsList.Add(relevantGrids);
            }

            gridsList.Sort(delegate (KeyValuePair<long, List<MyCubeGrid>> pair1, KeyValuePair<long, List<MyCubeGrid>> pair2) {
                return pair2.Key.CompareTo(pair1.Key);
            });

            return gridsList;
        }

        private KeyValuePair<long, List<MyCubeGrid>> CheckGroupsDays(HashSetReader<MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Node> nodes, bool checkFaction, DateTime today) {

            List<MyCubeGrid> gridsList = new List<MyCubeGrid>();

            foreach (var groupNodes in nodes) {

                MyCubeGrid cubeGrid = groupNodes.NodeData;

                if (cubeGrid.Physics == null)
                    continue;

                gridsList.Add(cubeGrid);
            }

            if(gridsList.Count == 0)
                return new KeyValuePair<long, List<MyCubeGrid>>(0, gridsList);

            MyCubeGrid biggestGrid = GridUtils.GetBiggestGridInGroup(gridsList);

            if(biggestGrid == null)
                return new KeyValuePair<long, List<MyCubeGrid>>(0, new List<MyCubeGrid>());

            long ownerId = OwnershipUtils.GetOwner(biggestGrid);
            long daysInactive = 0;

            if (ownerId != 0L && !PlayerUtils.IsNpc(ownerId)) {

                var identity = PlayerUtils.GetIdentityById(ownerId);
                var lastSeenDate = PlayerUtils.GetLastSeenDate(identity);

                daysInactive = (today - lastSeenDate).Days;

                if (checkFaction) {

                    var faction = FactionUtils.GetPlayerFaction(ownerId);

                    if (faction != null) {

                        foreach (long member in faction.Members.Keys) {

                            identity = PlayerUtils.GetIdentityById(member);
                            lastSeenDate = PlayerUtils.GetLastSeenDate(identity);

                            daysInactive = Math.Min(daysInactive, (today - lastSeenDate).Days);
                        }
                    }
                }
            }

            return new KeyValuePair<long, List<MyCubeGrid>>(daysInactive, gridsList);
        }

        public override bool CheckIfGridsAreRelevant(List<MyCubeGrid> grids, int distance, bool filterOffline, bool ignoreNpcs) {
            return true;
        }

        public override void WriteSettings(StringBuilder sb, int top, int playerdistance, int min, bool filterOffline, bool ignoreNpcs, bool connected, GridsBroadcastConfig pluginConfig) {

            sb.AppendLine("[Abandoned grids in " + GetUnitName() + "]");
            sb.AppendLine();
            sb.AppendLine("Used Settings");
            sb.AppendLine("---------------------------------------");
            sb.AppendLine("Min " + GetUnitName() + ": " + min);
            sb.AppendLine("Faction: " + (pluginConfig.MinDaysFactionCheck ? "true" : "false"));
        }

        public override List<KeyValuePair<long, List<MyCubeGrid>>> GetFilteredGrids(
            List<KeyValuePair<long, List<MyCubeGrid>>> sortedGrids, int min,
            int playerdistance, int top, bool filterOffline, bool ignoreNpcs) {

            return base.GetFilteredGrids(sortedGrids, min, playerdistance, int.MaxValue, filterOffline, ignoreNpcs);
        }
    }
}
