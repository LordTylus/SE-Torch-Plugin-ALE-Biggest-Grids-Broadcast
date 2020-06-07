using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch;

namespace ALE_Biggest_Grids_Broadcast {

    public class GridsBroadcastConfig : ViewModel {

        private int _topGrids = 10; //Top 10 Grids
        private bool _useConnectedGrids = false; //Only Grids and Subgrids (no connectors)
        private bool _removeGpsOnJoin = true;
        private string _gpsIdentifierName = "Doom Plugin";

        private int _red = 255;
        private int _green = 0;
        private int _blue = 0;

        private int _maxDistancePlayersBiggest = 1000; //1 Km
        private int _minPCU = 5000; //5000 everything below that will be ignored
        private bool _showOfflineBiggest = false;

        private int _maxDistancePlayersFurthest = 1000; //1 Km
        private int _minDistance = 3_000_000; //only further than 3,000 M away
        private bool _showOfflineFurthest = false;

        private double _centerX = 0;
        private double _centerY = 0;
        private double _centerZ = 0;

        private bool _ignoreNpcs = false;

        private int _minDays = 10;
        private bool _minDaysFactionCheck = false;

        private bool _sendShips = true;
        private bool _sendStations = true;
        private bool _sendLargeGrids = true;
        private bool _sendSmallGrids = true;

        public int TopGrids { get => _topGrids; set => SetValue(ref _topGrids, value); }
        public bool UseConnectedGrids { get => _useConnectedGrids; set => SetValue(ref _useConnectedGrids, value); }

        public int MaxDistancePlayersBiggest{ get => _maxDistancePlayersBiggest; set => SetValue(ref _maxDistancePlayersBiggest, value); }
        public int MinPCU { get => _minPCU; set => SetValue(ref _minPCU, value); }
        public bool ShowOfflineBiggest { get => _showOfflineBiggest; set => SetValue(ref _showOfflineBiggest, value); }

        public int MaxDistancePlayersFurthest { get => _maxDistancePlayersFurthest; set => SetValue(ref _maxDistancePlayersFurthest, value); }
        public int MinDistance { get => _minDistance; set => SetValue(ref _minDistance, value); }
        public bool ShowOfflineFurthest { get => _showOfflineFurthest; set => SetValue(ref _showOfflineFurthest, value); }

        public bool RemoveGpsOnJoin { get => _removeGpsOnJoin; set => SetValue(ref _removeGpsOnJoin, value); }
        public string GpsIdentifierName { get => _gpsIdentifierName; set => SetValue(ref _gpsIdentifierName, value); }

        public int ColorRed { get => _red; set => SetValue(ref _red, value); }
        public int ColorGreen { get => _green; set => SetValue(ref _green, value); }
        public int ColorBlue { get => _blue; set => SetValue(ref _blue, value); }

        public double CenterX { get => _centerX; set => SetValue(ref _centerX, value); }
        public double CenterY { get => _centerY; set => SetValue(ref _centerY, value); }
        public double CenterZ { get => _centerZ; set => SetValue(ref _centerZ, value); }

        public bool IgnoreNPCs { get => _ignoreNpcs; set => SetValue(ref _ignoreNpcs, value); }

        public int MinDays { get => _minDays; set => SetValue(ref _minDays, value); }
        public bool MinDaysFactionCheck { get => _minDaysFactionCheck; set => SetValue(ref _minDaysFactionCheck, value); }

        public bool SendShips { get => _sendShips; set => SetValue(ref _sendShips, value); }
        public bool SendStations { get => _sendStations; set => SetValue(ref _sendStations, value); }
        public bool SendLargeGrids { get => _sendLargeGrids; set => SetValue(ref _sendLargeGrids, value); }
        public bool SendSmallGrids { get => _sendSmallGrids; set => SetValue(ref _sendSmallGrids, value); }

    }
}
