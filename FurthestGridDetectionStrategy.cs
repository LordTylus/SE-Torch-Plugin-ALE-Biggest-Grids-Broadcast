using System;
using System.Collections.Generic;
using Sandbox.Game.Entities;
using VRage.Collections;
using VRage.Groups;

namespace ALE_Biggest_Grids_Broadcast {
    class FurthestGridDetectionStrategy : AbstractGridDetectionStrategy {

        public static readonly FurthestGridDetectionStrategy INSTANCE = new FurthestGridDetectionStrategy();

        private FurthestGridDetectionStrategy() {

        }

        public override string GetUnitName() {
            return "m";
        }

        public override List<KeyValuePair<long, List<MyCubeGrid>>> FindGrids(bool connected) {

            List<KeyValuePair<long, List<MyCubeGrid>>> gridsList = new List<KeyValuePair<long, List<MyCubeGrid>>>();

            if (connected) {

                foreach (var group in MyCubeGridGroups.Static.Physical.Groups)
                    gridsList.Add(CheckGroupsDistance(group.Nodes));

            } else {

                foreach (var group in MyCubeGridGroups.Static.Mechanical.Groups)
                    gridsList.Add(CheckGroupsDistance(group.Nodes));
            }

            gridsList.Sort(delegate (KeyValuePair<long, List<MyCubeGrid>> pair1, KeyValuePair<long, List<MyCubeGrid>> pair2) {
                return pair2.Key.CompareTo(pair1.Key);
            });

            return gridsList;
        }

        private KeyValuePair<long, List<MyCubeGrid>> CheckGroupsDistance(HashSetReader<MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Node> nodes) {

            List<MyCubeGrid> gridsList = new List<MyCubeGrid>();
            double distance = 0;

            foreach (var groupNodes in nodes) {

                MyCubeGrid cubeGrid = groupNodes.NodeData;

                if (cubeGrid.Physics == null)
                    continue;

                gridsList.Add(cubeGrid);

                distance = Math.Max(distance, cubeGrid.PositionComp.GetPosition().Length());
            }

            return new KeyValuePair<long, List<MyCubeGrid>>((long) distance, gridsList);
        }

        private KeyValuePair<long, List<MyCubeGrid>> CheckGroupsDistance(HashSetReader<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node> nodes) {

            List<MyCubeGrid> gridsList = new List<MyCubeGrid>();
            double distance = 0;

            foreach (var groupNodes in nodes) {

                MyCubeGrid cubeGrid = groupNodes.NodeData;

                if (cubeGrid.Physics == null)
                    continue;

                gridsList.Add(cubeGrid);

                distance = Math.Max(distance, cubeGrid.PositionComp.GetPosition().Length());
            }

            return new KeyValuePair<long, List<MyCubeGrid>>((long) distance, gridsList);
        }
    }
}
