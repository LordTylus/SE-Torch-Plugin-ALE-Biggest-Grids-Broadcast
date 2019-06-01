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
        private bool _useConnectedGrids = false; //Only Grids and Subgrids (no connectors)
        private int _minPCU = 5000; //5000 everything below that will be ignored
        private bool _removeGpsOnJoin = true;
        private string _gpsIdentifierName = "Doom Plugin";
        private int _red = 255;
        private int _green = 0;
        private int _blue = 0;


        public int TopGrids { get => _topGrids; set => SetValue(ref _topGrids, value); }

        public int MaxDistancePlayers { get => _maxDistancePlayers; set => SetValue(ref _maxDistancePlayers, value); }

        public bool UseConnectedGrids { get => _useConnectedGrids; set => SetValue(ref _useConnectedGrids, value); }

        public int MinPCU { get => _minPCU; set => SetValue(ref _minPCU, value); }

        public bool RemoveGpsOnJoin { get => _removeGpsOnJoin; set => SetValue(ref _removeGpsOnJoin, value); }

        public string GpsIdentifierName { get => _gpsIdentifierName; set => SetValue(ref _gpsIdentifierName, value); }

        public int ColorRed { get => _red; set => SetValue(ref _red, value); }
        public int ColorGreen { get => _green; set => SetValue(ref _green, value); }
        public int ColorBlue { get => _blue; set => SetValue(ref _blue, value); }
    }
}
