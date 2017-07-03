using System;
using System.IO;
using KSP.UI.Screens;

using System.Collections.Generic;
using System.Linq;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using UnityEngine;


namespace ConstantTWR
{

    public class TWRsetting
    {
        public enum SettingType { Altitude, Speed, Mach, GLimit };

        public bool initted = false;
        public bool activated = true;
        public double min = 0;
        public double max = 99999999;
        public double maxValue = 0;
        public double minTWR = HighLogic.CurrentGame.Parameters.CustomParams<CTWR>().MinTWR;
        public double maxTWR = HighLogic.CurrentGame.Parameters.CustomParams<CTWR>().MaxTWR;
        public SettingType settingType = SettingType.Altitude;        


        public void SetDefaults(CelestialBodies.BodyInfo body)
        {
            switch (settingType)
            {
                case SettingType.Altitude:
                    if (HighLogic.CurrentGame.Parameters.CustomParams<CTWR>().DefaultAltitude == 0)
                        max = (float)(body.GetAtmoDepth() / 1000);
                    else
                        max = HighLogic.CurrentGame.Parameters.CustomParams<CTWR>().DefaultAltitude;
                    maxValue = body.GetAtmoDepth() / 1000;
                    return;
                case SettingType.Mach:
                    max = HighLogic.CurrentGame.Parameters.CustomParams<CTWR>().DefaultMaxMach;
                    
                    break;
                case SettingType.Speed:
                    max = HighLogic.CurrentGame.Parameters.CustomParams<CTWR>().DefaultMaxSpeed;
                    break;
                case SettingType.GLimit:
                    max = PhysicsGlobals.KerbalGThresholdLOC * HighLogic.CurrentGame.Parameters.CustomParams<GameParameters.AdvancedParams>().KerbalGToleranceMult / 10000;
                    break;
            }
           
            maxValue = max;
        }
        public TWRsetting()
        {

        }
        public TWRsetting(SettingType type, CelestialBodies.BodyInfo body)
        {
            settingType = type;
            SetDefaults(body);
        }

        public void getDisplayValues(out string s, out string format,out double minValueOut, out double maxValueOut)
        {
            s = "";
            format = "";
            minValueOut = 0;
            maxValueOut = max;
            maxValueOut = maxValue;
            minValueOut = 0;
            switch (settingType)
            {
                case SettingType.Altitude:
                    s = "Altitude (km)";
                    format = "n0";
                    maxValueOut = maxValue;
                    minValueOut = 0;
                    break;
                case SettingType.Speed:
                    s = "Speed (m/sec)";
                    format = "n0";
                    break;
                case SettingType.Mach:
                    s = "Mach";
                    format = "N2";
                    break;
                case SettingType.GLimit:
                    s = "G";
                    format = "N1";
                    break;
            }
        }
    }
}
