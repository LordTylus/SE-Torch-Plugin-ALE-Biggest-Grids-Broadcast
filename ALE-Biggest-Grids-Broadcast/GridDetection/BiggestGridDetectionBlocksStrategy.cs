using Sandbox.Game.Entities;
using System.Collections.Generic;
using System.Text;
using VRage.Collections;
using VRage.Groups;

namespace ALE_Biggest_Grids_Broadcast.GridDetection {
    internal class BiggestGridDetectionBlocksStrategy : AbstractGridDetectionStrategy {

        public static readonly BiggestGridDetectionBlocksStrategy INSTANCE = new BiggestGridDetectionBlocksStrategy();

        private BiggestGridDetectionBlocksStrategy() {

        }

        public override DetectionType GetDetectionType() {
            return DetectionType.BIGGEST_BLOCKS;
        }

        public override string GetStrategyName() {
            return "Biggest Grid Blocks";
        }

        public override string GetUnitName() {
            return "Blocks";
        }

        public override List<KeyValuePair<long, List<MyCubeGrid>>> FindGrids(GridsBroadcastConfig config, bool connected) {

            List<KeyValuePair<long, List<MyCubeGrid>>> gridsList = new List<KeyValuePair<long, List<MyCubeGrid>>>();

            if (connected) {

                foreach (var group in MyCubeGridGroups.Static.Physical.Groups) {

                    var grids = CheckGroupsBlocks(group.Nodes, config);

                    if(grids.Value.Count > 0)
                        gridsList.Add(grids);
                }

            } else {

                foreach (var group in MyCubeGridGroups.Static.Mechanical.Groups) {

                    var grids = CheckGroupsBlocks(group.Nodes, config);

                    if (grids.Value.Count > 0)
                        gridsList.Add(grids);
                }
            }

            gridsList.Sort(delegate (KeyValuePair<long, List<MyCubeGrid>> pair1, KeyValuePair<long, List<MyCubeGrid>> pair2) {
                return pair2.Key.CompareTo(pair1.Key);
            });

            return gridsList;
        }

        private KeyValuePair<long, List<MyCubeGrid>> CheckGroupsBlocks(HashSetReader<MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Node> nodes, GridsBroadcastConfig config) {

            List<MyCubeGrid> gridsList = new List<MyCubeGrid>();
            long blocks = 0;

            foreach (var groupNodes in nodes) {

                MyCubeGrid cubeGrid = groupNodes.NodeData;

                if (cubeGrid.Physics == null)
                    continue;

                if (!IsGridInsideFilter(cubeGrid, config))
                    continue;

                gridsList.Add(cubeGrid);

                blocks += cubeGrid.BlocksCount;
            }

            return new KeyValuePair<long, List<MyCubeGrid>>(blocks, gridsList);
        }

        private KeyValuePair<long, List<MyCubeGrid>> CheckGroupsBlocks(HashSetReader<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node> nodes, GridsBroadcastConfig config) {

            List<MyCubeGrid> gridsList = new List<MyCubeGrid>();
            long blocks = 0;

            foreach (var groupNodes in nodes) {

                MyCubeGrid cubeGrid = groupNodes.NodeData;

                if (cubeGrid.Physics == null)
                    continue;

                if (!IsGridInsideFilter(cubeGrid, config))
                    continue;

                gridsList.Add(cubeGrid);

                blocks += cubeGrid.BlocksCount;
            }

            return new KeyValuePair<long, List<MyCubeGrid>>(blocks, gridsList);
        }

        public override void WriteSettings(StringBuilder sb, int top, int playerdistance, int min, bool filterOffline, bool ignoreNpcs, bool connected, GridsBroadcastConfig pluginConfig) {

            sb.AppendLine("[Top " + top + " grids by " + GetUnitName() + "]");
            sb.AppendLine();
            sb.AppendLine("Used Settings");
            sb.AppendLine("---------------------------------------");
            sb.AppendLine("Number of grids: " + top);
            sb.AppendLine("Player distance: " + playerdistance);
            sb.AppendLine("Min " + GetUnitName() + ": " + min);
            sb.AppendLine("Show Offline: " + !filterOffline);
            sb.AppendLine("Show NPCs: " + !ignoreNpcs);
            sb.AppendLine("Include: " + (!connected ? "Phyiscal Connections (Rotors, Pistons)" : "Mechanical Connections (Rotors, Pistons, Connectors)"));
        }
    }
}
