using NLog;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Session;
using VRage.Game.ModAPI;
using VRageMath;

namespace ALE_Biggest_Grids_Broadcast
{
    public class GridsBroadcastPlugin : TorchPluginBase, IWpfPlugin {

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private Persistent<GridsBroadcastConfig> _config;
        public GridsBroadcastConfig Config => _config?.Data;

        private IMultiplayerManagerBase multiplayerManagerBase;
        private TorchSessionManager torchSessionManager;

        public void Save() => _config.Save();

        public int TopGrids { get { return Config.TopGrids; } }
        public bool UseConnectedGrids { get { return Config.UseConnectedGrids; } }
        public bool IgnoreNPCs { get { return Config.IgnoreNPCs; } }
        public int MaxDistancePlayersBiggest { get { return Config.MaxDistancePlayersBiggest; } }
        public int MaxDistancePlayersBiggestBlocks { get { return Config.MaxDistancePlayersBiggestBlocks; } }
        public int MaxDistancePlayersFurthest { get { return Config.MaxDistancePlayersFurthest; } }
        public int MinDistance { get { return Config.MinDistance; } }
        public bool IgnoreOfflineBiggest { get { return !Config.ShowOfflineBiggest; } }
        public bool IgnoreOfflineBiggestBlocks { get { return !Config.ShowOfflineBiggestBlocks; } }
        public bool IgnoreOfflineFurthest { get { return !Config.ShowOfflineFurthest; } }
        public int MinPCU { get { return Config.MinPCU; } }
        public int MinBlocks { get { return Config.MinBlocks; } }
        public bool RemoveGpsOnJoin { get { return Config.RemoveGpsOnJoin; } }
        public string GpsIdentifierName { get { return Config.GpsIdentifierName; } }
        public int MinDays { get { return Config.MinDays; } }
        public bool LogBroadcastedGrids { get { return Config.LogBroadcastedGrids; } }

        public Color GpsColor {

            get {

                return new Color(
                    Math.Min(255, Config.ColorRed),
                    Math.Min(255, Config.ColorGreen),
                    Math.Min(255, Config.ColorBlue));
            }
        }

        private Control _control;
        public UserControl GetControl() => _control ?? (_control = new Control(this));

        public override void Init(ITorchBase torch) {

            base.Init(torch);

            var configFile = Path.Combine(StoragePath, "BiggestGridsBroadcast.cfg");

            try {

                _config = Persistent<GridsBroadcastConfig>.Load(configFile);

            } catch (Exception e) {
                Log.Warn(e);
            }

            if (_config?.Data == null) {

                Log.Info("Create Default Config, because none was found!");

                _config = new Persistent<GridsBroadcastConfig>(configFile, new GridsBroadcastConfig());
                _config.Save();
            }

            torchSessionManager = Torch.Managers.GetManager<TorchSessionManager>();
            if (torchSessionManager != null)
                torchSessionManager.SessionStateChanged += SessionChanged;
            else
                Log.Warn("No session manager loaded!");
        }

        private void SessionChanged(ITorchSession session, TorchSessionState state) {

            switch (state) {

                case TorchSessionState.Loaded:

                    multiplayerManagerBase = Torch.CurrentSession.Managers.GetManager<IMultiplayerManagerBase>();

                    if (multiplayerManagerBase != null) {

                        multiplayerManagerBase.PlayerJoined += PlayerJoined;
                        multiplayerManagerBase.PlayerLeft += PlayerLeft;

                    } else {
                        Log.Warn("No multiplayer manager loaded!");
                    }

                    RemoveGpsFromAllPlayers();

                    break;

                case TorchSessionState.Unloading:

                    if (multiplayerManagerBase != null) {

                        multiplayerManagerBase.PlayerJoined -= PlayerJoined;
                        multiplayerManagerBase.PlayerLeft -= PlayerLeft;
                    }
                        
                    break;
            }
        }

        private void PlayerLeft(IPlayer player) {

            long idendity = MySession.Static.Players.TryGetIdentityId(player.SteamId);

            Log.Debug("Player " + player.Name + " with ID " + player.SteamId + " and Identity " + idendity + " left.");

            if (idendity == 0)
                return;

            if (Config.RemoveGpsOnJoin) {

                Log.Debug("Removing Biggest Grid GPS for Player #" + idendity);
                RemoveGpsFromPlayer(idendity);
            }
        }

        private void PlayerJoined(IPlayer player) {

            long idendity = MySession.Static.Players.TryGetIdentityId(player.SteamId);

            Log.Debug("Player "+player.Name+" with ID "+player.SteamId+" and Identity "+idendity+" joined.");

            if (idendity == 0) 
                return;

            if (Config.RemoveGpsOnJoin) {

                Log.Debug("Removing Biggest Grid GPS for Player #" + idendity);
                RemoveGpsFromPlayer(idendity);
            }
        }

        public void RemoveGpsFromAllPlayers() {

            Log.Info("Removing Biggest Grid GPS from all Players.");

            foreach (var identity in MySession.Static.Players.GetAllIdentities()) 
                RemoveGpsFromPlayer(identity.IdentityId);
        }

        public void RemoveGpsFromPlayer(long idendity) {

            List<IMyGps> gpsList = MyAPIGateway.Session?.GPS.GetGpsList(idendity);

            if (gpsList == null)
                return;

            foreach (IMyGps gps in gpsList) {

                if (!(gps is MyGps myGps))
                    continue;

                string desc = myGps.Description;

                if (desc == null)
                    continue;

                if (!desc.Contains("by "+GpsIdentifierName) || !desc.Contains("Top Grid:")) 
                    continue;

                MyAPIGateway.Session?.GPS.RemoveGps(idendity, gps);
            }
        }
    }
}
