using Sandbox.Game.Entities;
using System.Collections.Generic;
using VRage.Collections;
using VRage.Groups;

namespace ALE_Biggest_Grids_Broadcast.GridDetection {
    internal class BiggestGridDetectionStrategy : AbstractGridDetectionStrategy {

        public static readonly BiggestGridDetectionStrategy INSTANCE = new BiggestGridDetectionStrategy();

        private BiggestGridDetectionStrategy() {

        }

        public override string GetUnitName() {
            return "PCU";
        }

        public override List<KeyValuePair<long, List<MyCubeGrid>>> FindGrids(GridsBroadcastConfig config, bool connected) {

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

        private KeyValuePair<long, List<MyCubeGrid>> CheckGroupsPcu(HashSetReader<MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Node> nodes) {

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

        private KeyValuePair<long, List<MyCubeGrid>> CheckGroupsPcu(HashSetReader<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node> nodes) {

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
    }
}
