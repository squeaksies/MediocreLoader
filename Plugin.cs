using System;
using System.Collections.Generic;
using System.Linq;
using IllusionPlugin;

namespace MediocreLoader
{
    public class Plugin : IPlugin
    {
        public string Name => "MediocreLoader";
        public string Version => "2.0";

        private bool _init = false;

        static Plugin instance;

        public void OnApplicationStart()
        {
            if (_init) return;
            _init = true;
            instance = this;

            LoaderManager.OnLoad();
        }

        public static string PluginName
        {
            get
            {
                return instance.Name;
            }
        }


        public void OnApplicationQuit()
        {
        }

        public void OnLevelWasLoaded(int level)
        {
        }

        public void OnLevelWasInitialized(int level)
        {
        }

        public void OnUpdate()
        {
        }

        public void OnFixedUpdate()
        {
        }
    }
}
