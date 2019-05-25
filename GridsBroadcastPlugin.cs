using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Torch;
using Torch.API;
using Torch.API.Plugins;

namespace ALE_Biggest_Grids_Broadcast
{
    public class GridsBroadcastPlugin : TorchPluginBase, IWpfPlugin {

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private Persistent<GridsBroadcastConfig> _config;
        public GridsBroadcastConfig Config => _config?.Data;

        public void Save() => _config.Save();

        public int TopGrids { get { return Config.TopGrids; } }
        public int MaxDistancePlayers { get { return Config.MaxDistancePlayers; } }
        public bool UseConnectedGrids { get { return Config.UseConnectedGrids; } }
        public int MinPCU { get { return Config.MinPCU; } }
        
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
        }
    }
}
