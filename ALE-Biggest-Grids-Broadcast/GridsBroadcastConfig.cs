﻿using System;
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
        private bool _logBroadcastedGrids = true;
        private string _gpsIdentifierName = "Doom Plugin";

        private int _red = 255;
        private int _green = 0;
        private int _blue = 0;

        private bool _separateColors = false;

        private bool _showOnHudBiggest = true;
        private int _redBiggest = 255;
        private int _greenBiggest = 0;
        private int _blueBiggest = 0;

        private bool _showOnHudDistance = true;
        private int _redDistance = 255;
        private int _greenDistance = 50;
        private int _blueDistance = 50;

        private bool _showOnHudInactive = true;
        private int _redInactive = 255;
        private int _greenInactive = 100;
        private int _blueInactive = 100;

        private bool _showOnHudBlocks = true;
        private int _redBlocks = 255;
        private int _greenBlocks = 150;
        private int _blueBlocks = 150;

        private int _maxDistancePlayersBiggest = 1000; //1 Km
        private int _minPCU = 5000; //5000 everything below that will be ignored
        private bool _showOfflineBiggest = false;

        private int _maxDistancePlayersBiggestBlocks = 1000; //1 Km
        private int _minBlocks = 3000; //3000 everything below that will be ignored
        private bool _showOfflineBiggestBlocks = false;
        private bool _excludeProjectionPCU = false;

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

        private bool _playGpsSound = true;
        private bool _gpsFollowGrids = false;

        public int TopGrids { get => _topGrids; set => SetValue(ref _topGrids, value); }
        public bool UseConnectedGrids { get => _useConnectedGrids; set => SetValue(ref _useConnectedGrids, value); }

        public int MaxDistancePlayersBiggest{ get => _maxDistancePlayersBiggest; set => SetValue(ref _maxDistancePlayersBiggest, value); }
        public int MinPCU { get => _minPCU; set => SetValue(ref _minPCU, value); }
        public bool ExcludeProjectionPCU { get => _excludeProjectionPCU; set => SetValue(ref _excludeProjectionPCU, value); }
        public bool ShowOfflineBiggest { get => _showOfflineBiggest; set => SetValue(ref _showOfflineBiggest, value); }

        public int MaxDistancePlayersFurthest { get => _maxDistancePlayersFurthest; set => SetValue(ref _maxDistancePlayersFurthest, value); }
        public int MinDistance { get => _minDistance; set => SetValue(ref _minDistance, value); }
        public bool ShowOfflineFurthest { get => _showOfflineFurthest; set => SetValue(ref _showOfflineFurthest, value); }

        public bool RemoveGpsOnJoin { get => _removeGpsOnJoin; set => SetValue(ref _removeGpsOnJoin, value); }
        public string GpsIdentifierName { get => _gpsIdentifierName; set => SetValue(ref _gpsIdentifierName, value); }

        public int ColorRed { get => _red; set => SetValue(ref _red, value); }
        public int ColorGreen { get => _green; set => SetValue(ref _green, value); }
        public int ColorBlue { get => _blue; set => SetValue(ref _blue, value); }

        public bool SeparateColors { get => _separateColors; set => SetValue(ref _separateColors, value); }

        public bool ShowOnHudDistance { get => _showOnHudDistance; set => SetValue(ref _showOnHudDistance, value); }
        public int ColorRedDistance { get => _redDistance; set => SetValue(ref _redDistance, value); }
        public int ColorGreenDistance { get => _greenDistance; set => SetValue(ref _greenDistance, value); }
        public int ColorBlueDistance { get => _blueDistance; set => SetValue(ref _blueDistance, value); }

        public bool ShowOnHudBiggest { get => _showOnHudBiggest; set => SetValue(ref _showOnHudBiggest, value); }
        public int ColorRedBiggest { get => _redBiggest; set => SetValue(ref _redBiggest, value); }
        public int ColorGreenBiggest { get => _greenBiggest; set => SetValue(ref _greenBiggest, value); }
        public int ColorBlueBiggest { get => _blueBiggest; set => SetValue(ref _blueBiggest, value); }

        public bool ShowOnHudInactive{ get => _showOnHudInactive; set => SetValue(ref _showOnHudInactive, value); }
        public int ColorRedInactive { get => _redInactive; set => SetValue(ref _redInactive, value); }
        public int ColorGreenInactive { get => _greenInactive; set => SetValue(ref _greenInactive, value); }
        public int ColorBlueInactive { get => _blueInactive; set => SetValue(ref _blueInactive, value); }

        public bool ShowOnHudBlocks { get => _showOnHudBlocks; set => SetValue(ref _showOnHudBlocks, value); }
        public int ColorRedBlocks { get => _redBlocks; set => SetValue(ref _redBlocks, value); }
        public int ColorGreenBlocks { get => _greenBlocks; set => SetValue(ref _greenBlocks, value); }
        public int ColorBlueBlocks { get => _blueBlocks; set => SetValue(ref _blueBlocks, value); }

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

        public bool PlayGpsSound { get => _playGpsSound; set => SetValue(ref _playGpsSound, value); }
        public bool GpsFollowGrids { get => _gpsFollowGrids; set => SetValue(ref _gpsFollowGrids, value); }

        public int MaxDistancePlayersBiggestBlocks { get => _maxDistancePlayersBiggestBlocks; set => SetValue(ref _maxDistancePlayersBiggestBlocks, value); }
        public int MinBlocks { get => _minBlocks; set => SetValue(ref _minBlocks, value); }
        public bool ShowOfflineBiggestBlocks { get => _showOfflineBiggestBlocks; set => SetValue(ref _showOfflineBiggestBlocks, value); }

        public bool LogBroadcastedGrids { get => _logBroadcastedGrids; set => SetValue(ref _logBroadcastedGrids, value); }
    }
}
