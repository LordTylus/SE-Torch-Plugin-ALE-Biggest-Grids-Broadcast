using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch;

namespace ALE_Biggest_Grids_Broadcast {

    public class GridsBroadcastConfig : ViewModel {

        private int _topGrids = 10; //Top 10 Grids
        private int _maxDistancePlayers = 1000; //1 Km
        private bool _physicalGroup = true; //Only Grids and Subgrids (no connectors)
        private int _minPCU = 5000; //5000 everything below that will be ignored

        public int TopGrids { get => _topGrids; set => SetValue(ref _topGrids, value); }

        public int MaxDistancePlayers { get => _maxDistancePlayers; set => SetValue(ref _maxDistancePlayers, value); }

        public bool PhysicalGroup { get => _physicalGroup; set => SetValue(ref _physicalGroup, value); }

        public int MinPCU { get => _minPCU; set => SetValue(ref _minPCU, value); }
    }
}
