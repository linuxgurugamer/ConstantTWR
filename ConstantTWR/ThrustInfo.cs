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
    class StageInfo
    {
      
        #region VerticalVelocityControl
        // Following code derived from the Vertical Velocity Control by @SirDiazo
        // https://github.com/SirDiazo/TWR1/blob/master/TWR1/TWR1Module.cs#L202-L316
        //
        static ModuleEngines TWR1EngineModule;
        static ModuleEnginesFX TWR1EngineModuleFX;

        //double vesselMaxThrust = 0f; //Max thrust contolled by throttle, modified by current vessel angle
        //double vesselMinThrust = 0f; //Min thrust controlled by throttle, not necessarily zero if solid rocket boosters are firing, modified by current vessel angle
        //double vesselMaxThrustVertical = 0f; //Max thrust if we are pefectly vertical
        //double vesselMinThrustVertical = 0f; //Min thrust controlled by throttle, not necessarily zero if solid rocket boosters are firing, if vessel is perfectly vertical

      //  [KSPField(isPersistant = true, guiActive = false)]
        static int controlDirection = 0; //control direction         
#if false
        double GetThrustInfo(List<Part> Parts, int stage, CelestialBodies.BodyInfo mainBody, double altitude, ref double minThrust)
        {

            Debug.Log("GetThrustInfo, stage: " + stage.ToString());
            vesselMaxThrustVertical = 0f;
            vesselMinThrustVertical = 0f;
            vesselMaxThrust = 0f;
            vesselMinThrust = 0f;
            vesselMaxThrustVertical = 0f;
            int enginecnt = 0;
            const float KerbinAtm = 101.325f;

            Vector3 TWR1Up = Vector3.up;

            double staticPressure = mainBody.GetPressure(altitude) / KerbinAtm;
            Debug.Log("altitude: " + altitude.ToString() + ",  Pressure: " + mainBody.GetPressure(altitude).ToString() + ",   staticPressure: " + staticPressure.ToString("n3"));

            foreach (Part part in Parts) //go through each part on vessel
            {
                Debug.Log("part: " + part.partInfo.title + ",   seperationIndex: " + part.separationIndex.ToString()+ ",   originalStage: " + part.originalStage.ToString() +",   inverseStage: " + part.inverseStage.ToString());
                if (part.separationIndex <= stage - 1 && part.inverseStage >= stage - 1)
                {
                    if (part.Modules.Contains("ModuleEngines") | part.Modules.Contains("ModuleEnginesFX")) //is part an engine?
                    {
                        Debug.Log("GetThrustInfo Engine: " + part.partInfo.title);
                        float DavonThrottleID = 0;
                        if (part.Modules.Contains("DifferentialThrustEngineModule")) //Devon Throttle Control Installed?
                        {
                            foreach (PartModule pm in part.Modules)
                            {

                                if (pm.moduleName == "DifferentialThrustEngineModule")
                                {
                                    DavonThrottleID = (float)pm.Fields.GetValue("throttleFloatSelect"); //which throttle is engine assigned to?
                                }
                            }
                        }
                        if (DavonThrottleID == 0f)
                        {
                            foreach (PartModule TWR1PartModule in part.Modules) //change from part to partmodules
                            {
                                if (TWR1PartModule.moduleName == "ModuleEngines") //find partmodule engine on th epart
                                {
                                    Debug.Log("GetThrustInfo, part: " + part.partInfo.title + ", module: ModuleEngines");
                                    TWR1EngineModule = (ModuleEngines)TWR1PartModule; //change from partmodules to moduleengines
                                                                                      
                                    double offsetMultiplier;
                                    try
                                    {
                                        offsetMultiplier = Math.Max(0, Math.Cos(Mathf.Deg2Rad * Vector3.Angle(TWR1EngineModule.thrustTransforms[0].forward, -TWR1Up)));
                                    }
                                    catch
                                    {
                                        offsetMultiplier = 1;
                                    }
                                    Debug.Log("offsetMultplier: " + offsetMultiplier.ToString("n2"));

                                    double atmoThrust = TWR1EngineModule.atmosphereCurve.Evaluate((float)staticPressure);

                                    Debug.Log("atmoThrust: " + atmoThrust.ToString("n2")+ ",  thrustPercentage: " + TWR1EngineModule.thrustPercentage.ToString("n2"));
                                    atmoThrust *= TWR1EngineModule.thrustPercentage / 100;
                                   // atmoThrust *= TWR1EngineModule.GetMaxThrust();

                                    if (/* useSRBs &&*/ TWR1EngineModule.throttleLocked)//if throttlelocked is true, this is solid rocket booster.
                                    {
                                        //Debug.Log("locked " + TWR1EngineModule.finalThrust);
                                        Debug.Log("from part module, solids maxThrust: " + TWR1EngineModule.GetMaxThrust().ToString("n2") + ", minThrust: " + TWR1EngineModule.minThrust.ToString("n2"));

                                        Debug.Log("thrustPercentage: " + TWR1EngineModule.thrustPercentage.ToString() + ",  TWR1EngineModule.atmosphereCurve.Evaluate((float)staticPressure): " + TWR1EngineModule.atmosphereCurve.Evaluate((float)staticPressure).ToString("n2"));

                                        vesselMaxThrust += atmoThrust; //add engine thrust to MaxThrust
                                        vesselMaxThrustVertical +=  offsetMultiplier * atmoThrust;
                                        vesselMinThrust += atmoThrust; //add engine thrust to MinThrust since this is an SRB
                                        vesselMinThrustVertical += atmoThrust * offsetMultiplier; //add engine thrust to MinThrust since this is an SRB

                                        enginecnt++;
                                    }
                                    else //we know it is an engine and not a solid rocket booster so:
                                    {
                                        Debug.Log("from part module,lqd maxThrust: " + TWR1EngineModule.GetMaxThrust().ToString("n2") + ", minThrust: " + TWR1EngineModule.minThrust.ToString("n2"));

                                        vesselMaxThrust += atmoThrust; //add engine thrust to MaxThrust
                                        vesselMaxThrustVertical += atmoThrust * offsetMultiplier ;

                                        enginecnt++;
                                        Debug.Log("enginecnt: " + enginecnt.ToString());
                                    }

                                }
                                else if (TWR1PartModule.moduleName == "ModuleEnginesFX") //find partmodule engine on th epart
                                {
                                    Debug.Log("ModuleEnginesFX, GetThrustInfo, part: " + part.partInfo.title + ", module: ModuleEnginesFX");

                                    TWR1EngineModuleFX = (ModuleEnginesFX)TWR1PartModule; //change from partmodules to moduleengines
                                    double offsetMultiplier;

                                    try
                                    {
                                        //errLine = "17b";

                                        offsetMultiplier = Math.Cos(Mathf.Deg2Rad * Vector3.Angle(TWR1EngineModuleFX.thrustTransforms[0].forward, -TWR1Up)); //how far off vertical is this engine?
                                    }
                                    catch
                                    {
                                        offsetMultiplier = 1;
                                    }

                                    double atmoThrust = TWR1EngineModuleFX.atmosphereCurve.Evaluate((float)staticPressure);
                                    Debug.Log("atmoThrust: " + atmoThrust.ToString("n2") + ",  thrustPercentage: " + TWR1EngineModuleFX.thrustPercentage.ToString("n2"));
                                    atmoThrust *= TWR1EngineModuleFX.thrustPercentage / 100;
                                   // atmoThrust *= TWR1EngineModuleFX.GetMaxThrust();

                                    if (TWR1EngineModuleFX.throttleLocked)//if throttlelocked is true, this is solid rocket booster
                                    {
                                        vesselMaxThrust += atmoThrust; //add engine thrust to MaxThrust
                                        vesselMaxThrustVertical += atmoThrust * offsetMultiplier;
                                        vesselMinThrust += atmoThrust; //add engine thrust to MinThrust since this is an SRB
                                        vesselMinThrustVertical += atmoThrust * offsetMultiplier; //add engine thrust to MinThrust since this is an SRB

                                        enginecnt++;

                                    }
                                    else //we know it is an engine and not a solid rocket booster so:
                                    {
                                        vesselMaxThrust += atmoThrust; //add engine thrust to MaxThrust
                                        vesselMaxThrustVertical += atmoThrust * offsetMultiplier;

                                        enginecnt++;

                                    }
                                }
                            }
                        }
                    }
                }
            }
            Debug.Log("GetThrustInfo, vesselMaxThrust: " + vesselMaxThrust.ToString("n2") + ",   vesselMaxThrustVertical: " + vesselMaxThrustVertical.ToString("n2") + ",  vesselMinthrust: " + vesselMinThrust.ToString("n2"));
            minThrust = vesselMinThrust;
            return vesselMaxThrust;
        }
#endif

        static Vector3 SetDirection(int ctrlDir, Vessel vessel)
        {
            if (ctrlDir == 0)
            {
                return (vessel.rootPart.transform.up);
            }
            if (ctrlDir == 1)
            {
                return (vessel.rootPart.transform.forward);
            }
            if (ctrlDir == 2)
            {
                return (-vessel.rootPart.transform.up);
            }
            if (ctrlDir == 3)
            {
                return (-vessel.rootPart.transform.forward);
            }
            if (ctrlDir == 4)
            {
                return (vessel.rootPart.transform.right);
            }
            if (ctrlDir == 5)
            {
                return (-vessel.rootPart.transform.right);
            }
            else
            {
                return (vessel.rootPart.transform.up);
            }
        }

        /// <summary>
        /// Get thrust info for active vessel in flight
        /// </summary>
        /// <param name="altitude"></param>
        /// <param name="isp"></param>
        /// <param name="ispavg"></param>
        /// <param name="spoolTime"></param>
        /// <param name="MaxFuelFlow"></param>
        /// <returns></returns>
        public static double GetThrustInfo(double altitude, ref double minThrust, ref double maxThrust)
        {
            Vessel activeVessel = FlightGlobals.ActiveVessel;

            var vesselCOM = activeVessel.GetWorldPos3D();
            var vesselUP = (vesselCOM - activeVessel.mainBody.position).normalized;
            var TWR1CoM = activeVessel.GetWorldPos3D(); //add exception here for destoryed parts
            var TWR1Up = (TWR1CoM - activeVessel.mainBody.position).normalized;

            double TWR1MaxThrust = 0;
            double TWR1MaxThrustVertical = 0;
            double TWR1MinThrust = 0;
            double TWR1MinThrustVertical = 0;

            double actualThrustLastFrame = 0;
            var TWR1ControlUp = SetDirection(controlDirection, activeVessel);

            double staticPressure = activeVessel.mainBody.GetPressure(altitude) * PhysicsGlobals.KpaToAtmospheres;

            foreach (Part part in activeVessel.Parts) //go through each part on vessel
            {
                if (part.Modules.Contains("ModuleEngines") | part.Modules.Contains("ModuleEnginesFX")) //is part an engine?
                {
                    float DavonThrottleID = 0;
                    if (part.Modules.Contains("DifferentialThrustEngineModule")) //Devon Throttle Control Installed?
                    {
                        foreach (PartModule pm in part.Modules)
                        {

                            if (pm.moduleName == "DifferentialThrustEngineModule")
                            {
                                DavonThrottleID = (float)pm.Fields.GetValue("throttleFloatSelect"); //which throttle is engine assigned to?
                            }
                        }

                    }
                    if (DavonThrottleID == 0f)
                    {
                        foreach (PartModule TWR1PartModule in part.Modules) //change from part to partmodules
                        {
                            if (TWR1PartModule.moduleName == "ModuleEngines") //find partmodule engine on th epart
                            {
                                TWR1EngineModule = (ModuleEngines)TWR1PartModule; //change from partmodules to moduleengines

                                double offsetMultiplier;
                                try
                                {
                                    offsetMultiplier = Math.Max(0, Math.Cos(Mathf.Deg2Rad * Vector3.Angle(TWR1EngineModule.thrustTransforms[0].forward, -TWR1ControlUp)));
                                }
                                catch
                                {
                                    offsetMultiplier = 1;
                                }

                                if ((bool)TWR1PartModule.Fields.GetValue("throttleLocked") && TWR1EngineModule.isOperational)//if throttlelocked is true, this is solid rocket booster. then check engine is operational. if the engine is flamedout, disabled via-right click or not yet activated via stage control, isOperational returns false
                                {
                                    //errLine = "16c";
                                    //Debug.Log("locked " + TWR1EngineModule.finalThrust);
                                    TWR1MaxThrust += (double)((TWR1EngineModule.finalThrust) * offsetMultiplier); //add engine thrust to MaxThrust
                                    TWR1MaxThrustVertical += (double)(TWR1EngineModule.finalThrust);
                                    TWR1MinThrust += (double)((TWR1EngineModule.finalThrust) * offsetMultiplier); //add engine thrust to MinThrust since this is an SRB
                                    TWR1MinThrustVertical += (double)(TWR1EngineModule.finalThrust);
                                }
                                else if (TWR1EngineModule.isOperational)//we know it is an engine and not a solid rocket booster so:
                                {
                                   // errLine = "16d";
                                    //ModuleEngines engTest = (ModuleEngines)TWR1PartModule;  
                                    //Debug.Log("twr1test " + TWR1EngineModule.thrustPercentage + ":" + TWR1EngineModule.maxFuelFlow * TWR1EngineModule.g * TWR1EngineModule.atmosphereCurve.Evaluate((float)(TWR1EngineModule.vessel.staticPressurekPa * PhysicsGlobals.KpaToAtmospheres)));
                                    //Debug.Log("twr1test " + TWR1EngineModule.finalThrust / TWR1EngineModule.currentThrottle + ":" + TWR1EngineModule.maxFuelFlow + ":" + TWR1EngineModule.g + ":" + TWR1EngineModule.atmosphereCurve.Evaluate(1f) + ":" + TWR1EngineModule.vessel.staticPressurekPa * PhysicsGlobals.KpaToAtmospheres );
                                    TWR1MaxThrust += (double)((TWR1EngineModule.maxFuelFlow * TWR1EngineModule.g * TWR1EngineModule.atmosphereCurve.Evaluate((float)(TWR1EngineModule.vessel.staticPressurekPa * PhysicsGlobals.KpaToAtmospheres)) * TWR1EngineModule.thrustPercentage / 100F) * offsetMultiplier); //add engine thrust to MaxThrust
                                   // errLine = "16d1";
                                    TWR1MaxThrustVertical += (double)((TWR1EngineModule.maxFuelFlow * TWR1EngineModule.g * TWR1EngineModule.atmosphereCurve.Evaluate((float)(TWR1EngineModule.vessel.staticPressurekPa * PhysicsGlobals.KpaToAtmospheres)) * TWR1EngineModule.thrustPercentage / 100F));
                                   // errLine = "16d2";
                                    //TWR1MinThrust += (double)((TWR1EngineModule.minThrust * TWR1EngineModule.thrustPercentage / 100F) * offsetMultiplier); //add engine thrust to MinThrust, stock engines all have min thrust of zero, but mods may not be 0
                                   // errLine = "16d3";
                                    //TWR1MinThrustVertical += (double)((TWR1EngineModule.minThrust * TWR1EngineModule.thrustPercentage / 100F));
                                   // errLine = "16d4";
                                }
                               // errLine = "16e";
                                actualThrustLastFrame += (float)TWR1EngineModule.finalThrust * (float)offsetMultiplier;
                            }
                            else if (TWR1PartModule.moduleName == "ModuleEnginesFX") //find partmodule engine on th epart
                            {
                                TWR1EngineModuleFX = (ModuleEnginesFX)TWR1PartModule; //change from partmodules to moduleengines
                                double offsetMultiplier;
                                try
                                {
                                    offsetMultiplier = Math.Cos(Mathf.Deg2Rad * Vector3.Angle(TWR1EngineModuleFX.thrustTransforms[0].forward, -TWR1ControlUp)); //how far off vertical is this engine?
                                }
                                catch
                                {
                                    offsetMultiplier = 1;
                                }
                                if ((bool)TWR1PartModule.Fields.GetValue("throttleLocked") && TWR1EngineModuleFX.isOperational)//if throttlelocked is true, this is solid rocket booster. then check engine is operational. if the engine is flamedout, disabled via-right click or not yet activated via stage control, isOperational returns false
                                {
                                   // errLine = "17d";
                                    TWR1MaxThrust += (double)((TWR1EngineModuleFX.finalThrust) * offsetMultiplier); //add engine thrust to MaxThrust
                                    TWR1MaxThrustVertical += (double)((TWR1EngineModuleFX.finalThrust));
                                    TWR1MinThrust += (double)((TWR1EngineModuleFX.finalThrust) * offsetMultiplier); //add engine thrust to MinThrust since this is an SRB
                                    TWR1MinThrustVertical += (double)((TWR1EngineModuleFX.finalThrust));
                                }
                                else if (TWR1EngineModuleFX.isOperational)//we know it is an engine and not a solid rocket booster so:
                                {
                                   // errLine = "17e";
                                    TWR1MaxThrust += (double)((TWR1EngineModuleFX.maxFuelFlow * TWR1EngineModuleFX.g * TWR1EngineModuleFX.atmosphereCurve.Evaluate((float)(TWR1EngineModuleFX.vessel.staticPressurekPa * PhysicsGlobals.KpaToAtmospheres)) * TWR1EngineModuleFX.thrustPercentage / 100F) * offsetMultiplier); //add engine thrust to MaxThrust
                                   // errLine = "17e1";
                                    TWR1MaxThrustVertical += (double)((TWR1EngineModuleFX.maxFuelFlow * TWR1EngineModuleFX.g * TWR1EngineModuleFX.atmosphereCurve.Evaluate((float)(TWR1EngineModuleFX.vessel.staticPressurekPa * PhysicsGlobals.KpaToAtmospheres)) * TWR1EngineModuleFX.thrustPercentage / 100F));
                                   // errLine = "17e2";
                                    //TWR1MinThrust += (double)((TWR1EngineModuleFX.minThrust * TWR1EngineModuleFX.thrustPercentage / 100F) * offsetMultiplier); //add engine thrust to MinThrust, stock engines all have min thrust of zero, but mods may not be 0
                                   // errLine = "17e3";
                                    //TWR1MinThrustVertical += (double)((TWR1EngineModuleFX.minThrust * TWR1EngineModuleFX.thrustPercentage / 100F));
                                   // errLine = "17e4";
                                }
                               // errLine = "17f";
                                actualThrustLastFrame += (float)TWR1EngineModuleFX.finalThrust * (float)offsetMultiplier;
                            }

                        }
                    }
                }
            }
            minThrust = TWR1MinThrust;
            maxThrust = TWR1MaxThrust;
            return actualThrustLastFrame;
        }
#endregion
    }
}
