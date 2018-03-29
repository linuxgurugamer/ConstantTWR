using System;
using System.IO;
using KSP;
using KSP.UI.Screens;
using CommNet;
using KSP.UI.Screens.Flight;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace ConstantTWR
{
    public class stageTWRs
    {
        public string name = "Default";
        public Log Log = new Log("stageTWRs");
        public List<TWRsetting> list;
        public bool disableUponCompletion;

        public stageTWRs()
        {
            Init();
        }
        void Init()
        {
            name = "Default";
            if (Log == null)
                Log = new Log("stageTWRs");
            if (list == null)
                list = new List<TWRsetting>();
            else
                list.Clear();
            disableUponCompletion = HighLogic.CurrentGame.Parameters.CustomParams<CTWR>().defaultDisableUponCompletion;
        }
        public void DeleteData(string path)
        {
            string fullPath = path + "/" + name + ".twr";
            if (File.Exists(fullPath))
                File.Delete(fullPath);
            Init();
        }
        public void SaveData(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string fullPath = path + "/" + name + ".twr";
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            var configFile = new ConfigNode();
            var configFileNode = new ConfigNode("ConstantTWR");
            configFileNode.AddValue("name", name);
            configFileNode.AddValue("disableUponCompletion", disableUponCompletion.ToString());

            foreach (var s in list)
            {
                var settingsNode = new ConfigNode("TWRSetting");
                settingsNode.AddValue("settingType", s.settingType.ToString());
                settingsNode.AddValue("min", s.min.ToString());
                settingsNode.AddValue("max", s.max.ToString());
                settingsNode.AddValue("maxValue", s.maxValue.ToString());
                settingsNode.AddValue("minTWR", s.minTWR.ToString());
                settingsNode.AddValue("maxTWR", s.maxTWR.ToString());
                configFileNode.AddNode(settingsNode);
            }
            configFile.AddNode(configFileNode);
            configFile.Save(fullPath);
        }

        //
        // The following functions are used when loading data from the config file
        // They make sure that if a value is missing, that the old value will be used.
        //
        static string SafeLoad(string value, string oldvalue)
        {
            if (value == null)
                return oldvalue;
            return value;
        }

        static string SafeLoad(string value, double oldvalue)
        {
            if (value == null)
                return oldvalue.ToString();
            return value;
        }
        static string SafeLoad(string value, bool oldvalue)
        {
            if (value == null)
                return oldvalue.ToString();
            return value;
        }

        public void LoadData(string path, string loadname)
        {
            string fullPath = path + "/" + loadname + ".twr";
            Log.Info("LoadData, fullpath: " + fullPath);
            if (File.Exists(fullPath))
            {
                Log.Info("file exists");
                var configFile = ConfigNode.Load(fullPath);
                if (configFile != null)
                {
                    Log.Info("configFile loaded");
                    var configFileNode = configFile.GetNode("ConstantTWR");
                    if (configFileNode != null)
                    {
                        Log.Info("configFileNode loaded");
                        list.Clear();
                        disableUponCompletion = bool.Parse(SafeLoad(configFileNode.GetValue("disableUponCompletion"), disableUponCompletion));
                        name = SafeLoad(configFileNode.GetValue("name"), name);
                        ConfigNode[] settings = configFileNode.GetNodes("TWRSetting");
                        foreach (var s in settings)
                        {
                            TWRsetting ts = new TWRsetting();
                            string t = s.GetValue("settingType");
                            ts.settingType = (TWRsetting.SettingType)Enum.Parse(typeof(TWRsetting.SettingType), t);
                            ts.min = double.Parse(SafeLoad(s.GetValue("min"), ts.min));
                            ts.max = double.Parse(SafeLoad(s.GetValue("max"), ts.max));
                            ts.maxValue = double.Parse(SafeLoad(s.GetValue("maxValue"), ts.maxValue));
                            ts.minTWR = double.Parse(SafeLoad(s.GetValue("minTWR"), ts.minTWR));
                            ts.maxTWR = double.Parse(SafeLoad(s.GetValue("maxTWR"), ts.maxTWR));
                            list.Add(ts);
                        }

                    }
                }
            }
        }
    }

}
