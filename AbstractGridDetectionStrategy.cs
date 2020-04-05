using Sandbox.Game.Entities;
using Sandbox.Game.World;
using System.Collections.Generic;
using VRageMath;

namespace ALE_Biggest_Grids_Broadcast {

    abstract class AbstractGridDetectionStrategy : GridDetectionStrategy {

        public List<KeyValuePair<long, List<MyCubeGrid>>> GetFilteredGrids(
            List<KeyValuePair<long, List<MyCubeGrid>>> sortedGrids, int min,
            int playerdistance, int top, bool filterOffline) {

            List<KeyValuePair<long, List<MyCubeGrid>>> gridsList = new List<KeyValuePair<long, List<MyCubeGrid>>>();

            foreach (KeyValuePair<long, List<MyCubeGrid>> pair in sortedGrids) {

                if (pair.Key == 0)
                    continue;

                /* Too little PCU ignore */
                if (pair.Key < min)
                    continue;

                bool relevant = CheckIfGridsAreRelevant(pair.Value, playerdistance, filterOffline);

                if (relevant)
                    gridsList.Add(pair);

                if (gridsList.Count == top)
                    break;
            }

            return gridsList;
        }

        public bool CheckIfGridsAreRelevant(List<MyCubeGrid> grids, int distance, bool filterOffline) {

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

                if (faction != null) {

                    bool allOffline = true;

                    foreach (long factionMember in faction.Members.Keys) {

                        MyPlayer member = GetPlayerById(factionMember);

                        /* if member is not online and we filter offlines ignore. */
                        if (filterOffline && member == null)
                            continue;

                        /* If member is online we need to remember that not all players are offline. */
                        if (member != null) {

                            allOffline = false;

                            Vector3D position = member.GetPosition();

                            /* If player is close by grid is relevant */
                            if (Vector3D.Distance(gridPosition, position) <= distance)
                                return true;
                        }
                    }

                    /* If all players of that faction are offline. And we dont want to filter offlines grid is relevant. */
                    if (allOffline && !filterOffline)
                        return true;

                } else {

                    MyPlayer owner = GetPlayerById(gridOwner);

                    /* If owner is offline and we dont want to see offlines continue */
                    if (filterOffline && owner == null)
                        continue;

                    if (owner != null) {
                                            
                        /* If player is online check distance */

                        Vector3D position = owner.GetPosition();

                        if (Vector3D.Distance(gridPosition, position) <= distance)
                            return true;

                    } else if(!filterOffline) {

                        /* if player is offline and we want to see offlines mark as relevant */
                        return true;
                    }
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

        public abstract List<KeyValuePair<long, List<MyCubeGrid>>> FindGrids(GridsBroadcastConfig config, bool connected);

        public abstract string GetUnitName();
    }
}
