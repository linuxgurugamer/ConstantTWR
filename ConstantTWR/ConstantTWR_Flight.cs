using System;
using System.IO;
using KSP.UI.Screens;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;


namespace ConstantTWR
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class ConstantTWR_Flight : MonoBehaviour
    {
        private void Start()
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                CelestialBodies.SetSelectedBody(FlightGlobals.currentMainBody.name);
            }
        }

        static Log Log = new Log("ConstantTWR.ConstantTWR_Flight");

        double minThrust, maxThrust;
        double vmass;


        float GetCurTWR()
        {
            double thrust, curTWR;
            thrust = StageInfo.GetThrustInfo(FlightGlobals.ActiveVessel.altitude, ref minThrust, ref maxThrust);
            vmass = FlightGlobals.ActiveVessel.GetTotalMass();

            curTWR = (thrust / (CelestialBodies.SelectedBody.Gravity * vmass));
            return (float)curTWR;
        }

        float CalcNewThrottle(double desiredTWR)
        {
            double t = StageInfo.GetThrustInfo(FlightGlobals.ActiveVessel.altitude, ref minThrust, ref maxThrust);
            vmass = FlightGlobals.ActiveVessel.GetTotalMass();
            double thrust = desiredTWR * (CelestialBodies.SelectedBody.Gravity * vmass);
            if (thrust <= minThrust || maxThrust == 0)
                return 0;
            
            return Math.Min(1, (float)(thrust / maxThrust));
        }

        float lastTWR = 0;
        float calculateTWR(ref TWRsetting twrs, double value, double min, double max)
        {
            if (value <= min)
                return (float)twrs.minTWR;
            if (ConstantTWR.stageTWRsClass == null || ConstantTWR.stageTWRsClass.list == null || ConstantTWR.stageTWRsClass.list.Count == 0)
                return (float)twrs.minTWR;
            if (value >= max)
            {
                if (twrs != ConstantTWR.stageTWRsClass.list.Last())
                {
                    Log.Info("Setting: " + twrs.settingType.ToString() + " to false");
                    twrs.activated = false;
                    return getTWR();
                }
                if (ConstantTWR.stageTWRsClass.disableUponCompletion)
                {
                    ConstantTWR.activeFlight = false;
                }
                return (float)twrs.maxTWR;
            }

            double range = (max - min);
            double r = value - min;
            double l = (float)(r / range);
           
            lastTWR = (float)Mathf.Lerp((float)twrs.minTWR, (float)twrs.maxTWR, (float)l);
            return lastTWR;
        }

        float getTWR()
        {
            if (ConstantTWR.stageTWRsClass == null || ConstantTWR.stageTWRsClass.list == null || ConstantTWR.stageTWRsClass.list.Count == 0)
                return 0;
            TWRsetting twrs = ConstantTWR.stageTWRsClass.list.Last();

            foreach (var t in ConstantTWR.stageTWRsClass.list)
            {
                if (t.activated)
                {
                    twrs = t;
                    break;
                }
            }
            ConstantTWR.instance.activeTwrs = twrs;
            switch (twrs.settingType)
            {
                case TWRsetting.SettingType.Altitude:
                    return calculateTWR(ref twrs, FlightGlobals.ActiveVessel.altitude, twrs.min * 1000, twrs.max * 1000);

                case TWRsetting.SettingType.Speed:
                    return calculateTWR(ref twrs, FlightGlobals.ActiveVessel.speed, twrs.min, twrs.max);

                case TWRsetting.SettingType.Mach:
                    return calculateTWR(ref twrs, FlightGlobals.ActiveVessel.mach, twrs.min, twrs.max);

                case TWRsetting.SettingType.GLimit:
                   // Log.Info("geeForce_immediate: " + FlightGlobals.ActiveVessel.geeForce_immediate.ToString("n3") + "   twrs.min: " + twrs.min.ToString("n2") + "   twrs.max: " + twrs.max.ToString("n2"));
                
                    return calculateTWR(ref twrs, FlightGlobals.ActiveVessel.geeForce_immediate, twrs.min, twrs.max);

            }
            return lastTWR;
        }

        void FixedUpdate()
        {
            Log.Info("FixedUpdate, activeFlight: " + ConstantTWR.activeFlight.ToString() + ",   LandedOrSpashed: " + FlightGlobals.ActiveVessel.LandedOrSplashed.ToString());
            if (ConstantTWR.activeFlight && !FlightGlobals.ActiveVessel.LandedOrSplashed)
            {
                ConstantTWR.instance.activeRequestedTWR = getTWR();
                ConstantTWR.instance.activeTWR = GetCurTWR();
                float targetThrottle = CalcNewThrottle(ConstantTWR.instance.activeRequestedTWR);

                FlightInputHandler.state.mainThrottle = targetThrottle;

                Log.Info("Setting main throttle to: " + targetThrottle.ToString("n2"));
            }
        }
    }
}
