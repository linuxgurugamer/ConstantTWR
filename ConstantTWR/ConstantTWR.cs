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
using ClickThroughFix;

namespace ConstantTWR
{
    [KSPAddon(KSPAddon.Startup.FlightAndEditor, false)]
    public partial class ConstantTWR : MonoBehaviour
    {
        public static ConstantTWR instance;
        const string TITLE = "Constant TWR";
        public static readonly String ROOT_PATH = KSPUtil.ApplicationRootPath;
        private static readonly String CONFIG_BASE_FOLDER = ROOT_PATH + "GameData/";
        public static string TEXTURE_DIR = "ConstantTWR/" + "Images/";
        bool CTWR_Texture_Load = false;

        private static Texture2D CTWR_button_off_img = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        private static Texture2D CTWR_button_on_img = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        public static ApplicationLauncherButton CTWR_Button = null;

        private const int INFOWIDTH = 250;
        private const int INFOHEIGHT = 200;
        private Rect allStagesWindow = new Rect(Screen.width - INFOWIDTH, Screen.height / 4 - INFOHEIGHT / 2, INFOWIDTH, INFOHEIGHT);

        private const int TWR_WIDTH = 300;
        private const int TWR_HEIGHT = 275;
        private Rect twr_Bounds = new Rect(Screen.width / 2 + TWR_WIDTH / 2, Screen.height / 2 - TWR_HEIGHT / 2, TWR_WIDTH, TWR_HEIGHT);

        private const int FILE_SEL_WIDTH = 150;
        private const int FILE_SEL_HEIGHT = 250;
        private Rect filesel_Bounds = new Rect(Screen.width / 2 + FILE_SEL_WIDTH / 2, Screen.height / 2 - FILE_SEL_HEIGHT / 2, FILE_SEL_WIDTH, FILE_SEL_HEIGHT);

        private const int GETNAME_WIDTH = 300;
        private const int GETNAME_HEIGHT = 75;
        private Rect getname_Bounds = new Rect(Screen.width / 2 + GETNAME_WIDTH / 2, Screen.height / 2 - GETNAME_HEIGHT / 2, GETNAME_WIDTH, GETNAME_HEIGHT);


        public bool active = false;
        bool displaySingleStageTWR = false;
        bool displayGetName = false;
        bool displayFileSelection = false;
        int editTWRSetting = 0;

        public static bool activeFlight = false;
        public float activeTWR = 0;
        public float activeRequestedTWR = 0;
        public float activeRequestedG = 0;
        public TWRsetting activeTwrs = null;

        public Log Log = new Log("ConstantTWR");

        //these are called when F2 is pressed to hide/show the UI
        bool showUI = true;

        void ShowUI()
        {
            showUI = true;
        }
        void HideUI()
        {
            showUI = false;
        }


        public static stageTWRs stageTWRsClass = new stageTWRs();

        public void UpdateToolbarStock()
        {
            if (!GameDatabase.Instance.IsReady())
                return;


            if (!CTWR_Texture_Load)
            {

                if (GameDatabase.Instance.ExistsTexture(TEXTURE_DIR + "off-38"))
                    CTWR_button_off_img = GameDatabase.Instance.GetTexture(TEXTURE_DIR + "off-38", false);
                if (GameDatabase.Instance.ExistsTexture(TEXTURE_DIR + "on-38"))
                    CTWR_button_on_img = GameDatabase.Instance.GetTexture(TEXTURE_DIR + "on-38", false);

                CTWR_Texture_Load = true;
            }
            if (CTWR_Button == null)
            {
                CTWR_Button = ApplicationLauncher.Instance.AddModApplication(GUIToggleToolbar, GUIToggleToolbar,
                        null, null,
                        null, null,
                        ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH,
                        CTWR_button_off_img);
            }

        }

        public void GUIToggleToolbar()
        {
            active = !active;
        }

        public void OnGUIApplicationLauncherReady()
        {
            UpdateToolbarStock();
        }

        protected void DestroyLauncher()
        {
            if (CTWR_Button != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(CTWR_Button);
                CTWR_Button = null;
            }
#if false
            if (ToolbarButton != null)
            {
                ToolbarButton.Destroy();
                ToolbarButton = null;
            }
#endif
        }
        bool isEditorLocked = false;
        private void EditorLock(bool state)
        {
            if (state)
            {
                InputLockManager.SetControlLock(ControlTypes.All, "ConstantTWR");

                isEditorLocked = true;
            }
            else
            {
                InputLockManager.SetControlLock(ControlTypes.None, "ConstantTWR");
                isEditorLocked = false;
            }
        }

        /// <summary>
        ///     Checks whether the editor should be locked to stop click-through.
        /// </summary>
        private void CheckEditorLock()
        {
            if ( /* position.MouseIsOver() ||  bodiesList.Position.MouseIsOver()) && */ !isEditorLocked)
            {
                EditorLock(true);
            }
            else if ( /*!position.MouseIsOver() &&  !bodiesList.Position.MouseIsOver() && */ isEditorLocked)
            {
                EditorLock(false);
            }
        }


        #region StartDestroy
        public void Start()
        {
            instance = this;
            if (CTWR_Button == null)
                GameEvents.onGUIApplicationLauncherReady.Add(OnGUIApplicationLauncherReady);
            UpdateToolbarStock();

            GameEvents.onFlightReady.Add(CallbackOnFlightReady);
            GameEvents.onEditorRestart.Add(OnEditorRestart);
            GameEvents.onEditorScreenChange.Add(OnEditorScreenChanged);
            GameEvents.onLaunch.Add(OnLaunch);
        }

        protected void OnDestroy()
        {
            DestroyLauncher();
            GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIApplicationLauncherReady);
            active = false;

            GameEvents.onFlightReady.Remove(CallbackOnFlightReady);
            GameEvents.onEditorRestart.Remove(OnEditorRestart);
            GameEvents.onEditorScreenChange.Remove(OnEditorScreenChanged);
            GameEvents.onLaunch.Remove(OnLaunch);
        }
        #endregion

        #region Events
        // KSP Events

        void OnLaunch(EventReport evt) { }

        void doTWRupdate() { activeFlight = false; }

        protected void OnEditorScreenChanged(EditorScreen screen) { doTWRupdate(); }

        public void OnEditorRestart() { doTWRupdate(); }

        private void CallbackOnFlightReady() { doTWRupdate(); }

        #endregion

        public void OnGUI()
        {
            //resizeTWRlist();
            if (active && showUI)
            {

                allStagesWindow.x = Math.Max(0, Math.Min(allStagesWindow.x, Screen.width - 40 - allStagesWindow.width));
                allStagesWindow.y = Math.Max(0, Math.Min(allStagesWindow.y, Screen.height - allStagesWindow.height));

                allStagesWindow = ClickThruBlocker.GUILayoutWindow(GetInstanceID() + 1, allStagesWindow, AllStagesInfoWindow, TITLE, HighLogic.Skin.window);

                if (displaySingleStageTWR)
                {
                    twr_Bounds.width = 300;
                    twr_Bounds.x = Math.Max(0, Math.Min(twr_Bounds.x, Screen.width - 40 - twr_Bounds.width));
                    twr_Bounds.y = Math.Max(0, Math.Min(twr_Bounds.y, Screen.height - twr_Bounds.height));
                    twr_Bounds = ClickThruBlocker.GUILayoutWindow(GetInstanceID() + 2, twr_Bounds, TwrWindow, TITLE, HighLogic.Skin.window);
                }                if (displayFileSelection)
                {
                    filesel_Bounds.x = Math.Max(0, Math.Min(filesel_Bounds.x, (Screen.width - filesel_Bounds.width) / 2));
                    filesel_Bounds.y = Math.Max(0, Math.Min(filesel_Bounds.y, Screen.height - filesel_Bounds.height));
                    filesel_Bounds = ClickThruBlocker.GUILayoutWindow(GetInstanceID() + 3, filesel_Bounds, FileSelWindow, TITLE, HighLogic.Skin.window);
                }
                if (displayGetName)
                {
                    getname_Bounds.width = 300;
                    getname_Bounds.x = Math.Max(0, Math.Min(getname_Bounds.x, (Screen.width - getname_Bounds.width) / 2));
                    getname_Bounds.y = Math.Max(0, Math.Min(getname_Bounds.y, Screen.height - getname_Bounds.height));
                    getname_Bounds = ClickThruBlocker.GUILayoutWindow(GetInstanceID() + 4, getname_Bounds, GetNameWindow, TITLE, HighLogic.Skin.window);
                }
            }
        }


        #region Styles
        public static GUIStyle readoutButtonStyle = new GUIStyle(HighLogic.Skin.button)
        {
            normal =
            {
                textColor = Color.white
            },
            //margin = new RectOffset(2, 2, 2, 2),
            margin = new RectOffset(),
            padding = new RectOffset(),
            alignment = TextAnchor.MiddleCenter,
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            stretchHeight = false

        };
        public static GUIStyle deleteButtonStyle = new GUIStyle(HighLogic.Skin.button)
        {
            normal =
            {
                textColor = Color.red
            },
            //margin = new RectOffset(2, 2, 2, 2),
            margin = new RectOffset(),
            padding = new RectOffset(),
            alignment = TextAnchor.MiddleCenter,
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            stretchHeight = false

        };
        static GUIStyle bodyButtonStyle = new GUIStyle(HighLogic.Skin.button)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 12,
            fontStyle = FontStyle.Bold
        };
        static GUIStyle bodyButtonStyleGreen = new GUIStyle(HighLogic.Skin.button)
        {
            normal =
            {
                textColor = Color.green
            },
            alignment = TextAnchor.MiddleCenter,
            fontSize = 12,
            fontStyle = FontStyle.Bold
        };

        enum Shift { none, up, down };

        //Class to hold custom gui styles
        public static class MyGUIStyles
        {
            private static GUIStyle m_line = null;

            //constructor
            static MyGUIStyles()
            {

                m_line = new GUIStyle("box");
                m_line.border.top = m_line.border.bottom = 1;
                m_line.margin.top = m_line.margin.bottom = 1;
                m_line.padding.top = m_line.padding.bottom = 1;
            }

            public static GUIStyle EditorLine
            {
                get { return m_line; }
            }
        }
        #endregion

        float VerticalSlider(double value, double leftValue, double rightValue, params GUILayoutOption[] options)
        {
            return (float)GUILayout.VerticalSlider((float)value, (float)leftValue, (float)rightValue, options);
        }
        FileInfo[] FilesList = null;
        Vector2 fileSelectionScrollPosition = new Vector2();
        int selectedFileIdx = -1;
        void GetFileList()
        {
            if (Directory.Exists(AS_CFG_DIR))
            {
                DirectoryInfo d = new DirectoryInfo(AS_CFG_DIR);
                FilesList = d.GetFiles("*.twr");
            }
            else
                Directory.CreateDirectory(AS_CFG_DIR);
        }

        private void FileSelWindow(int id)
        {
            if (FilesList == null)
                GetFileList();

            GUILayout.BeginHorizontal();            GUILayout.Label("Select TWRSettings to use:");            GUILayout.FlexibleSpace();            GUILayout.EndHorizontal();            GUILayout.BeginHorizontal();            fileSelectionScrollPosition = GUILayout.BeginScrollView(fileSelectionScrollPosition, GUILayout.Width(FILE_SEL_WIDTH - 10), GUILayout.Height(FILE_SEL_HEIGHT * 2 / 3));            GUILayout.BeginVertical();            for (int i = 0; i < FilesList.Count(); i++)            {                GUILayout.BeginHorizontal();                GUIStyle gs = bodyButtonStyle;                if (i == selectedFileIdx)                    gs = bodyButtonStyleGreen;                if (GUILayout.Button(FilesList[i].Name.Substring(0, FilesList[i].Name.Length - 4), gs, GUILayout.Width(FILE_SEL_WIDTH - 15)))                {                    loadTWRs(FilesList[i].Name.Substring(0, FilesList[i].Name.Length - 4));                    selectedFileIdx = i;                }                GUILayout.EndHorizontal();            }            GUILayout.EndVertical();            GUILayout.EndScrollView();            GUILayout.EndHorizontal();            GUILayout.BeginHorizontal();            GUILayout.FlexibleSpace();            if (GUILayout.Button("OK", GUILayout.Width(60)))            {                loadTWRs(FilesList[selectedFileIdx].Name.Substring(0, FilesList[selectedFileIdx].Name.Length - 4));                displayFileSelection = false;                return;            }            GUILayout.FlexibleSpace();            if (GUILayout.Button("Cancel", GUILayout.Width(60)))            {                displayFileSelection = false;                FilesList = null;                return;            }            GUILayout.FlexibleSpace();            GUILayout.EndHorizontal();            GUI.DragWindow();
        }

        string tmpName = "";
        private void GetNameWindow(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Enter Settings Name: ");
            tmpName = GUILayout.TextField(tmpName, GUILayout.Width(200));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel", GUILayout.Width(60)))
                displayGetName = false;
            if (GUILayout.Button("OK", GUILayout.Width(60)))
            {
                displayGetName = false;
                stageTWRsClass.name = tmpName;
            }
            GUILayout.EndHorizontal();
        }

        #region TWRWindow
        private void TwrWindow(int id)
        {
            double minTWR, maxTWR;
            double min, max;

            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Box(GUIContent.none, MyGUIStyles.EditorLine, GUILayout.ExpandWidth(true), GUILayout.Height(1f));
            GUILayout.BeginHorizontal();


            TWRsetting twrsetting = stageTWRsClass.list[editTWRSetting];
            if (twrsetting.activated)
            {
                min = twrsetting.min;
                max = twrsetting.max;

                minTWR = twrsetting.minTWR;
                maxTWR = twrsetting.maxTWR;


                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(twrsetting.settingType.ToString());
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Box(GUIContent.none, MyGUIStyles.EditorLine, GUILayout.ExpandWidth(true), GUILayout.Height(1f));

                string s = "";
                string format = "";
                double maxValue = 0, minValue = 0;

                twrsetting.getDisplayValues(out s, out format, out minValue, out maxValue);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(s);
                GUILayout.FlexibleSpace();

                GUILayout.FlexibleSpace();
                GUILayout.Label("TWR");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Min (" + min.ToString(format) + ")");
                GUILayout.FlexibleSpace();
                GUILayout.Label("Max (" + max.ToString(format) + ")");
                GUILayout.FlexibleSpace();

                GUILayout.FlexibleSpace();
                GUILayout.Label("Min (" + minTWR.ToString("n2") + ")");
                GUILayout.FlexibleSpace();
                GUILayout.Label("Max (" + maxTWR.ToString("n2") + ")");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                twrsetting.min = this.VerticalSlider(twrsetting.min, maxValue, 0, GUILayout.Height(250));
                GUILayout.FlexibleSpace();
                twrsetting.max = this.VerticalSlider(twrsetting.max, maxValue, 0, GUILayout.Height(250));
                GUILayout.FlexibleSpace();

                GUILayout.FlexibleSpace();
                twrsetting.minTWR = this.VerticalSlider(twrsetting.minTWR, HighLogic.CurrentGame.Parameters.CustomParams<CTWR>().MaxTWR, HighLogic.CurrentGame.Parameters.CustomParams<CTWR>().MinTWR, GUILayout.Height(250));
                GUILayout.FlexibleSpace();
                twrsetting.maxTWR = this.VerticalSlider(twrsetting.maxTWR, HighLogic.CurrentGame.Parameters.CustomParams<CTWR>().MaxTWR, HighLogic.CurrentGame.Parameters.CustomParams<CTWR>().MinTWR, GUILayout.Height(250));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                if (minTWR != twrsetting.minTWR)
                {
                    if (twrsetting.maxTWR < twrsetting.minTWR)
                        twrsetting.maxTWR = twrsetting.minTWR;
                }
                if (maxTWR != twrsetting.maxTWR)
                {
                    if (twrsetting.maxTWR < twrsetting.minTWR)
                        twrsetting.minTWR = twrsetting.maxTWR;
                }

                if (min != twrsetting.min)
                {
                    if (twrsetting.max < twrsetting.min)
                        twrsetting.max = twrsetting.min;
                }
                if (max != twrsetting.maxTWR)
                {
                    if (twrsetting.max < twrsetting.min)
                        twrsetting.min = twrsetting.max;
                }
            }

            GUILayout.EndHorizontal();
            //GUILayout.EndVertical();
            GUILayout.Space(5);
            GUILayout.Box(GUIContent.none, MyGUIStyles.EditorLine, GUILayout.ExpandWidth(true), GUILayout.Height(1f));
            //            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("OK", GUILayout.Width(60)))
            {
                displaySingleStageTWR = false;

            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }
        #endregion

        void addNewTWRs(TWRsetting.SettingType type)
        {
            TWRsetting t = new TWRsetting(type, CelestialBodies.SelectedBody);
            stageTWRsClass.list.Add(t);
        }


        private static String AS_BASE_FOLDER = CONFIG_BASE_FOLDER + "ConstantTWR/";
        private static String AS_CFG_DIR = AS_BASE_FOLDER + "PluginData";

        void loadTWRs(string name)
        {
            Log.Info("Loading file: " + AS_CFG_DIR + "/" + name);
            stageTWRsClass.LoadData(AS_CFG_DIR, name);
        }

        void saveTWRs()
        {
            stageTWRsClass.SaveData(AS_CFG_DIR);
        }

        void deleteTWRs()
        {
            stageTWRsClass.DeleteData(AS_CFG_DIR);
        }
        #region AllStagesInfoWindow

        private void AllStagesInfoWindow(int id)
        {
            bool delete = false;
            GUI.enabled = true;
            Shift shift = Shift.none;
            int shiftid = 0;
            if (displaySingleStageTWR || displayGetName || displayFileSelection)
                GUI.enabled = false;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Name: " + stageTWRsClass.name))
            {
                displayGetName = true;
                tmpName = stageTWRsClass.name;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            stageTWRsClass.disableUponCompletion = GUILayout.Toggle(stageTWRsClass.disableUponCompletion, "Disable upon completion");
            GUILayout.EndHorizontal();
            for (int twrIdx = 0; twrIdx < stageTWRsClass.list.Count; twrIdx++)
            {
                GUILayout.BeginHorizontal();
                {

                    TWRsetting twrsetting = stageTWRsClass.list[twrIdx];

                    string s = "";
                    string typestr = "";
                    string format = "";
                    double maxValue = 0, minValue = 0;

                    twrsetting.getDisplayValues(out typestr, out format, out minValue, out maxValue);

                    s = typestr + ": " + twrsetting.min.ToString(format) + "/" + twrsetting.max.ToString(format);
                    s += "\nTWR: " + twrsetting.minTWR.ToString("n2") + "/" + twrsetting.maxTWR.ToString("n2");
                    if (!activeFlight)
                    {
                        if (GUILayout.Button("^", readoutButtonStyle, GUILayout.Width(15.0f), GUILayout.Height(50)))
                        {
                            shift = Shift.up;
                            shiftid = twrIdx;
                        }
                        if (GUILayout.Button("v", readoutButtonStyle, GUILayout.Width(15.0f), GUILayout.Height(50)))
                        {
                            shift = Shift.down;
                            shiftid = twrIdx;
                        }
                    }

                    GUILayout.FlexibleSpace();
                    bool b = GUI.enabled;
                    if (displaySingleStageTWR || displayGetName || displayFileSelection)
                        GUI.enabled = false;
                    else
                        GUI.enabled = twrsetting.activated;

                    GUIStyle btn = new GUIStyle(HighLogic.Skin.button);
                    if (activeFlight && activeTwrs == twrsetting)
                    {
                        btn.normal.textColor = Color.green;
#if false
                        Color[] g = new Color[1];
                        g[0] = Color.green;
                        var t = new Texture2D(2, 2);
                        t.SetPixels(g);
                        btn.normal.background = t;
#endif
                    }
                    if (GUILayout.Button(s, btn, GUILayout.Width(150), GUILayout.Height(50)))
                    {
                        if (!activeFlight || FlightGlobals.ActiveVessel.LandedOrSplashed)
                        {
                            displaySingleStageTWR = true;
                            editTWRSetting = twrIdx;
                        }
                    }

                    GUILayout.FlexibleSpace();
                    if (!activeFlight)
                    {
                        if (GUILayout.Button("X", deleteButtonStyle, GUILayout.Width(15.0f), GUILayout.Height(50)))
                        {
                            delete = true;
                            shiftid = twrIdx;
                        }
                    }
                    GUI.enabled = b;

                    switch (shift)
                    {
                        case Shift.up:
                            TWRsetting twrsettingU = stageTWRsClass.list[shiftid - 1];
                            stageTWRsClass.list[shiftid - 1] = stageTWRsClass.list[shiftid];
                            stageTWRsClass.list[shiftid] = twrsettingU;
                            break;
                        case Shift.down:
                            TWRsetting twrsettingD = stageTWRsClass.list[shiftid + 1];
                            stageTWRsClass.list[shiftid + 1] = stageTWRsClass.list[shiftid];
                            stageTWRsClass.list[shiftid] = twrsettingD;
                            break;
                    }
                    shift = Shift.none;
                    if (delete)
                    {
                        stageTWRsClass.list.Remove(stageTWRsClass.list[shiftid]);
                        delete = false;
                    }

                }

                GUILayout.EndHorizontal();
            }

            GUILayout.FlexibleSpace();
            GUILayout.Space(10);

            if (HighLogic.LoadedScene == GameScenes.EDITOR || FlightGlobals.ActiveVessel.LandedOrSplashed)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Add:");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Altitude", GUILayout.Width(60)))
                    addNewTWRs(TWRsetting.SettingType.Altitude);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Speed", GUILayout.Width(60)))
                    addNewTWRs(TWRsetting.SettingType.Speed);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Mach", GUILayout.Width(60)))
                    addNewTWRs(TWRsetting.SettingType.Mach);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("G-Limit", GUILayout.Width(60)))
                    addNewTWRs(TWRsetting.SettingType.GLimit);

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Clear", GUILayout.Width(60)))
                    stageTWRsClass.list.Clear();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Close", GUILayout.Width(60)))
                    GUIToggleToolbar();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Load", GUILayout.Width(60)))
                    displayFileSelection = true;
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Save", GUILayout.Width(60)))
                    saveTWRs();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                    deleteTWRs();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                if (HighLogic.LoadedScene == GameScenes.FLIGHT && stageTWRsClass.list.Count > 0)
                {
                    if (activeFlight)
                    {
                        if (GUILayout.Button("DeActivate"))
                        {
                            activeFlight = false;
                            CTWR_Button.SetTexture(CTWR_button_off_img);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Activate"))
                        {
                            activeFlight = true;
                            CTWR_Button.SetTexture(CTWR_button_on_img);
                        }
                    }
                }
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Throttle: " + (FlightInputHandler.state.mainThrottle * 100).ToString("n1"));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("TWR (target/actual): " + activeRequestedTWR.ToString("n3") + "/" + activeTWR.ToString("n3"));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                if (HighLogic.CurrentGame.Parameters.CustomParams<GameParameters.AdvancedParams>().GKerbalLimits)
                {
                    //FlightGlobals.ActiveVessel.geeForce_immediate
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("G: " + FlightGlobals.ActiveVessel.geeForce_immediate.ToString("n1"));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
                if (stageTWRsClass.list.Count > 0 && GUILayout.Button("DeActivate"))
                {
                    activeFlight = false;
                    CTWR_Button.SetTexture(CTWR_button_off_img);
                }
                GUILayout.EndHorizontal();
            }

            GUI.DragWindow();
        }
        #endregion
    }
}