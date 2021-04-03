using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.Game.Entities;
using VRage.Collections;
using VRage.Groups;
using VRageMath;

namespace ALE_Biggest_Grids_Broadcast.GridDetection {
    class FurthestGridDetectionStrategy : AbstractGridDetectionStrategy {

        public static readonly FurthestGridDetectionStrategy INSTANCE = new FurthestGridDetectionStrategy();

        private FurthestGridDetectionStrategy() {

        }

        public override string GetStrategyName() {
            return "Furthest Grid";
        }

        public override string GetUnitName() {
            return "m";
        }

        public override List<KeyValuePair<long, List<MyCubeGrid>>> FindGrids(GridsBroadcastConfig config, bool connected) {

            List<KeyValuePair<long, List<MyCubeGrid>>> gridsList = new List<KeyValuePair<long, List<MyCubeGrid>>>();

            var origin = new Vector3D(config.CenterX, config.CenterY, config.CenterZ);

            if (connected) {

                foreach (var group in MyCubeGridGroups.Static.Physical.Groups) {

                    var grids = CheckGroupsDistance(origin, group.Nodes, config);

                    if (grids.Value.Count > 0)
                        gridsList.Add(grids);
                }

            } else {

                foreach (var group in MyCubeGridGroups.Static.Mechanical.Groups) {

                    var grids = CheckGroupsDistance(origin, group.Nodes, config);

                    if (grids.Value.Count > 0)
                        gridsList.Add(grids);
                }
            }

            gridsList.Sort(delegate (KeyValuePair<long, List<MyCubeGrid>> pair1, KeyValuePair<long, List<MyCubeGrid>> pair2) {
                return pair2.Key.CompareTo(pair1.Key);
            });

            return gridsList;
        }

        private KeyValuePair<long, List<MyCubeGrid>> CheckGroupsDistance(Vector3D origin, HashSetReader<MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Node> nodes, GridsBroadcastConfig config) {

            List<MyCubeGrid> gridsList = new List<MyCubeGrid>();
            double distance = 0;

            foreach (var groupNodes in nodes) {

                MyCubeGrid cubeGrid = groupNodes.NodeData;

                if (cubeGrid.Physics == null)
                    continue;

                if (!IsGridInsideFilter(cubeGrid, config))
                    continue;

                gridsList.Add(cubeGrid);

                var gridPositon = cubeGrid.PositionComp.GetPosition();

                double distanceSquared = Vector3D.DistanceSquared(origin, gridPositon);

                distance = Math.Max(distance, distanceSquared);
            }

            return new KeyValuePair<long, List<MyCubeGrid>>((long) Math.Sqrt(distance), gridsList);
        }

        private KeyValuePair<long, List<MyCubeGrid>> CheckGroupsDistance(Vector3D origin, HashSetReader<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node> nodes, GridsBroadcastConfig config) {

            List<MyCubeGrid> gridsList = new List<MyCubeGrid>();
            double distance = 0;

            foreach (var groupNodes in nodes) {

                MyCubeGrid cubeGrid = groupNodes.NodeData;

                if (cubeGrid.Physics == null)
                    continue;

                if (!IsGridInsideFilter(cubeGrid, config))
                    continue;

                gridsList.Add(cubeGrid);

                var gridPositon = cubeGrid.PositionComp.GetPosition();

                double distanceSquared = Vector3D.DistanceSquared(origin, gridPositon);

                distance = Math.Max(distance, distanceSquared);
            }

            return new KeyValuePair<long, List<MyCubeGrid>>((long) Math.Sqrt(distance), gridsList);
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
            sb.AppendLine("Center: " + pluginConfig.CenterX + ", " + pluginConfig.CenterY + ", " + pluginConfig.CenterZ);
        }
    }
}
