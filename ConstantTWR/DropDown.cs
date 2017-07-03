// 
//     Kerbal Engineer Redux
// 
//     Copyright (C) 2014 CYBUTEK
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

#region Using Directives

using System;
using System.IO;
using KSP.UI.Screens;

using System.Collections.Generic;
using System.Linq;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using UnityEngine;

#endregion

namespace ConstantTWR
{
    public static class RectExtensions
    {
        /// <summary>
        ///     Clamps the rectangle inside the screen region.
        /// </summary>
        public static Rect ClampInsideScreen(this Rect value)
        {
            value.x = Mathf.Clamp(value.x, 0, Screen.width - value.width);
            value.y = Mathf.Clamp(value.y, 0, Screen.height - value.height);

            return value;
        }

        /// <summary>
        ///     Clamps the rectangle into the screen region by the specified margin.
        /// </summary>
        public static Rect ClampToScreen(this Rect value, float margin = 25.0f)
        {
            value.x = Mathf.Clamp(value.x, -(value.width - margin), Screen.width - margin);
            value.y = Mathf.Clamp(value.y, -(value.height - margin), Screen.height - margin);

            return value;
        }

        /// <summary>
        ///     Returns whether the mouse is within the coordinates of this rectangle.
        /// </summary>
        public static bool MouseIsOver(this Rect value)
        {
            return value.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y));
        }

        public static Rect Translate(this Rect value, Rect rectangle)
        {
            value.x += rectangle.x;
            value.y += rectangle.y;

            return value;
        }
    }
    public class DropDown : MonoBehaviour
    {
        Log Log;

        #region Fields

        private Rect button;
        private Rect position;

        #endregion

        #region Properties

        public bool Resize { get; set; }

        public Callback DrawCallback { get; set; }

        public Rect Position
        {
            get { return this.position; }
        }

        #endregion

        #region Initialisation

        private void Awake()
        {
            try
            {
                this.enabled = false;
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }

        private void Start()
        {
            Log = ConstantTWR.instance.Log;
            try
            {
                this.InitialiseStyles();
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        #endregion

        #region Styles

        private GUIStyle windowStyle;

        private void InitialiseStyles()
        {
            try
            {
                this.windowStyle = new GUIStyle
                {
                    normal =
                    {
                        background = GameDatabase.Instance.GetTexture("KerbalEngineer/Textures/DropDownBackground", false)
                    },
                    border = new RectOffset(8, 8, 1, 8),
                    margin = new RectOffset(),
                    padding = new RectOffset(5, 5, 5, 5)
                };
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        #endregion

        #region Updating

        private void Update()
        {
            try
            {
                if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) && !this.position.MouseIsOver() && !this.button.MouseIsOver())
                {
                    this.enabled = false;
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        #endregion

        #region Drawing

        private void OnGUI()
        {
            try
            {
                if (this.Resize)
                {
                    this.position.height = 0;
                    this.Resize = false;
                }

                GUI.skin = null;
                this.position = GUILayout.Window(this.GetInstanceID(), this.position, this.Window, string.Empty, this.windowStyle);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        private void Window(int windowId)
        {
            try
            {
                GUI.BringWindowToFront(windowId);
                this.DrawCallback.Invoke();
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        #endregion

        #region Public Methods

        public void SetPosition(Rect button)
        {
            try
            {
                this.position.x = button.x;
                this.position.y = button.y + button.height;
                this.position.width = button.width;
                this.button = button;
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        #endregion
    }
}