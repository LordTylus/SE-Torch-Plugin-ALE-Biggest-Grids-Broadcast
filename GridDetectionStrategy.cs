using Sandbox.Game.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALE_Biggest_Grids_Broadcast {

    interface GridDetectionStrategy {

        List<KeyValuePair<long, List<MyCubeGrid>>> FindGrids(GridsBroadcastConfig config, bool connected);

        List<KeyValuePair<long, List<MyCubeGrid>>> GetFilteredGrids(
        List<KeyValuePair<long, List<MyCubeGrid>>> sortedGrids,
        int min, int playerdistance, int top, bool filterOffline);

        string GetUnitName();
    }
}
