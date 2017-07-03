using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;



namespace ConstantTWR
{

    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    public class CTWR : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "General Settings"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Constant TWR"; } }
        public override string DisplaySection { get { return "Constant TWR"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return true; } }

        [GameParameters.CustomParameterUI("Mod Enabled")]
        public bool EnabledForSave = true;      // is enabled for this save file

#if false
        [GameParameters.CustomParameterUI("Show Debug Stats")]
#endif
        public bool DebugStats = false;         // show debug stats of the part in the right-click menu


        [GameParameters.CustomFloatParameterUI("Min TWR", minValue = 1.0f, maxValue = 5.0f, stepCount = 90, displayFormat = "N1", asPercentage = false,
            toolTip ="Absolute minimum TWR that can be requested")]
        public float MinTWR = 1.0f;

        [GameParameters.CustomFloatParameterUI("Default Min TWR", minValue = 1.0f, maxValue = 5.0f, stepCount = 90, displayFormat = "N1", asPercentage = false,
            toolTip ="New settings default to this as the minimum TWR")]
        public float DefaultMinTWR = 1.0f;

        [GameParameters.CustomFloatParameterUI("Default Max TWR", minValue = 1.0f, maxValue = 5.0f, stepCount = 90, displayFormat = "N1", asPercentage = false,
            toolTip = "New settings default to this as the default maximum TWR")]
        public float DefaultMaxTWR = 1.5f;  

        [GameParameters.CustomFloatParameterUI("Max TWR", minValue = 1.0f, maxValue = 10.0f, stepCount = 90, displayFormat = "N1", asPercentage = false,
            toolTip = "New settings default to this as the absolute maximum TWR")]
        public float MaxTWR = 5.0f;

        [GameParameters.CustomFloatParameterUI("Default Altitude limit (km)", minValue = 0.0f, maxValue = 250, displayFormat = "N1", asPercentage = false,
            toolTip = "Set to 0 to use the planet's atmosphere limit")]
        public float DefaultAltitude = 35;

        [GameParameters.CustomFloatParameterUI("Default Max Speed", minValue = 1.0f, maxValue = 5000.0f, stepCount = 90, displayFormat = "N0", asPercentage = false)]
        public float DefaultMaxSpeed = 2500f;

        [GameParameters.CustomFloatParameterUI("Default Max Mach", minValue = 1.0f, maxValue = 10.0f, stepCount =90, displayFormat = "N1", asPercentage = false)]
        public float DefaultMaxMach = 1.1f;

        [GameParameters.CustomParameterUI("Default Disable upon completion",
            toolTip ="Disable the mod after completing all requested levels")]
        public bool defaultDisableUponCompletion = true;      // is enabled for this save file



        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
        }

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "EnabledForSave") //This Field must always be enabled.
                return true;

            return EnabledForSave; //otherwise return true
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            return true;
            //            return true; //otherwise return true
        }

        public override IList ValidValues(MemberInfo member)
        {
            return null;
        }
    }
}
