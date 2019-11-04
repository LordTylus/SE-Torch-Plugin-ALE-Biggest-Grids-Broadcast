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
        public int MaxDistancePlayers { get { return Config.MaxDistancePlayers; } }
        public int MinDistance { get { return Config.MinDistance; } }
        public bool UseConnectedGrids { get { return Config.UseConnectedGrids; } }
        public int MinPCU { get { return Config.MinPCU; } }
        public bool RemoveGpsOnJoin { get { return Config.RemoveGpsOnJoin; } }
        public string GpsIdentifierName { get { return Config.GpsIdentifierName; } }

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

                        multiplayerManagerBase.PlayerJoined += playerJoined;
                        multiplayerManagerBase.PlayerLeft += playerLeft;

                    } else {
                        Log.Warn("No multiplayer manager loaded!");
                    }

                    removeGpsFromAllPlayers();

                    break;

                case TorchSessionState.Unloading:

                    if (multiplayerManagerBase != null) {

                        multiplayerManagerBase.PlayerJoined -= playerJoined;
                        multiplayerManagerBase.PlayerLeft -= playerLeft;
                    }
                        
                    break;
            }
        }

        private void playerLeft(IPlayer player) {

            long idendity = MySession.Static.Players.TryGetIdentityId(player.SteamId);

            Log.Debug("Player " + player.Name + " with ID " + player.SteamId + " and Identity " + idendity + " left.");

            if (idendity == 0)
                return;

            if (Config.RemoveGpsOnJoin) {

                Log.Debug("Removing Biggest Grid GPS for Player #" + idendity);
                removeGpsFromPlayer(idendity);
            }
        }

        private void playerJoined(IPlayer player) {

            long idendity = MySession.Static.Players.TryGetIdentityId(player.SteamId);

            Log.Debug("Player "+player.Name+" with ID "+player.SteamId+" and Identity "+idendity+" joined.");

            if (idendity == 0) 
                return;

            if (Config.RemoveGpsOnJoin) {

                Log.Debug("Removing Biggest Grid GPS for Player #" + idendity);
                removeGpsFromPlayer(idendity);
            }
        }

        public void removeGpsFromAllPlayers() {

            Log.Info("Removing Biggest Grid GPS from all Players.");

            foreach (var identity in MySession.Static.Players.GetAllIdentities()) 
                removeGpsFromPlayer(identity.IdentityId);
        }

        public void removeGpsFromPlayer(long idendity) {

            List<IMyGps> gpsList = MyAPIGateway.Session?.GPS.GetGpsList(idendity);

            if (gpsList == null)
                return;

            foreach (IMyGps gps in gpsList) {

                MyGps myGps = gps as MyGps;

                if (myGps == null)
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
