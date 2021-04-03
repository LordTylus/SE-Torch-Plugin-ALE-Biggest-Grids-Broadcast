using Sandbox.Game.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALE_Biggest_Grids_Broadcast.GridDetection {

    interface IGridDetectionStrategy {

        List<KeyValuePair<long, List<MyCubeGrid>>> FindGrids(GridsBroadcastConfig config, bool connected);

        List<KeyValuePair<long, List<MyCubeGrid>>> GetFilteredGrids(
        List<KeyValuePair<long, List<MyCubeGrid>>> sortedGrids,
        int min, int playerdistance, int top, bool filterOffline,
        bool ignoreNpcs);

        string GetUnitName();

        string GetStrategyName();

        void WriteSettings(StringBuilder sb, int top, int playerdistance, int min, bool filterOffline, bool ignoreNpcs, bool connected, GridsBroadcastConfig pluginConfig);
    }
}
