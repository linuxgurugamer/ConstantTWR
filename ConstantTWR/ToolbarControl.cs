﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

#if false
namespace ConstantTWR
{
    class ToolbarControl
    {
        static IButton btnReturn = null;
        private const string _tooltipOn = "Hide Craft Import";
        private const string _tooltipOff = "Show Craft Import";
        public const string MOD_DIR = "CraftImport/";
        public const string TEXTURE_DIR = MOD_DIR + "Textures/";

        public void setToolbarButtonVisibility(bool v)
        {

            btnReturn.Visible = v;
        }

        public void ToolbarToggle()
        {
            Log.Info("ToolbarToggle, visible: " + gui.Visible().ToString());
            if (gui.Visible())
            {
                gui.endGUIToggle();
                gui.SetVisible(false);
                GUI.enabled = false;
                btnReturn.ToolTip = _tooltipOff;
                gui.GUI_SaveData();

                if (gui.thisCI.configuration.BlizzyToolbarIsAvailable && gui.thisCI.configuration.useBlizzyToolbar)
                {
                    btnReturn.TexturePath = TEXTURE_DIR + "CI-24";
                    gui.OnGUIHideApplicationLauncher();
                    //InitToolbarButton ();
                }
                else
                {
                    gui.setAppLauncherHidden();
                    gui.OnGUIApplicationLauncherReady();

                    //GameEvents.onGUIApplicationLauncherReady.Add (gui.OnGUIApplicationLauncherReady);
                    gui.OnGUIShowApplicationLauncher();
                    // Hide blizzy toolbar button
                    setToolbarButtonVisibility(false);

                }
                configuration.Save();
                ToolBarActive();

            }
            else
            {
                gui.initGUIToggle();
                //gui.SetVisible (true);
                GUI.enabled = true;
                btnReturn.ToolTip = _tooltipOn;

                btnReturn.TexturePath = TEXTURE_DIR + "CI-24";

                //InputLockManager.SetControlLock((ControlTypes.EDITOR_LOCK | ControlTypes.EDITOR_GIZMO_TOOLS), "CraftImportLock");
            }
        }

        public /*static*/ void ToolBarActive()
        {
            btnReturn.TexturePath = TEXTURE_DIR + "CI-24";
        }


        /// <summary>
        /// initialises a Toolbar Button for this mod
        /// </summary>
        /// <returns>The ToolbarButtonWrapper that was created</returns>
        public void InitToolbarButton()
        {

            try
            {
                btnReturn = ToolbarManager.Instance.add("CraftImport", "btnReturn");
                btnReturn.Visibility = new GameScenesVisibility(GameScenes.EDITOR);
                btnReturn.TexturePath = TEXTURE_DIR + "CI-24";
                btnReturn.ToolTip = TITLE;
                btnReturn.OnClick += e => ToolbarToggle();
            }
            catch (Exception ex)
            {
                DestroyToolbarButton(btnReturn);
                Log.Info("Error Initialising Toolbar Button: " + ex.Message);
            }
            return;
        }

        public void DelToolbarButton()
        {
            DestroyToolbarButton(btnReturn);
        }
        /// <summary>
        /// Destroys theToolbarButtonWrapper object
        /// </summary>
        /// <param name="btnToDestroy">Object to Destroy</param>
        static void DestroyToolbarButton(IButton btnToDestroy)
        {
            if (btnToDestroy != null)
            {
                Log.Info("Destroying Toolbar Button");
                btnToDestroy.Destroy();
            }
            btnToDestroy = null;
        }

    }
}
#endif