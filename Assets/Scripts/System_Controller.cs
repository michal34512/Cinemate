using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class System_Controller : MonoBehaviour
{
    public static System_Controller Instance;

    #region Fullscreen
    private Vector2Int ScreenSize = Vector2Int.zero;
    /// <summary>Setting fullscreen or resizable window</summary>
    public static void FullScreenToggle()
    {
        if(Instance!=null)
        {
            if (Screen.fullScreen)
            {
                if (Instance.ScreenSize != Vector2Int.zero)
                    Screen.SetResolution(Instance.ScreenSize.x, Instance.ScreenSize.y, false);
                else Screen.SetResolution(1920, 1080, false);
                //if (Instance.SetHide) Instance.SetHide = false;
            }
            else
            {
                Instance.ScreenSize = new Vector2Int(Screen.width, Screen.height);
                Screen.SetResolution(1920, 1080, true);
            }
            Screen.fullScreen = !Screen.fullScreen;
        }
    }
    public static void FullScreenToggle(bool Fullscreen)
    {
        Screen.fullScreen = !Fullscreen;
        FullScreenToggle();
    }

    #endregion Fullscreen

   /* #region Hide

    const int GWL_EXSTYLE = -20;
    const uint WS_EX_LAYERED = 0x00080000;
    const uint WS_EX_TRANSPARENT = 0x00000020;
    const uint LWA_COLORKEY = 0x00000001;
    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    static readonly IntPtr HWND_TOP = new IntPtr(0); 
    private bool _SetHide;
    public bool SetHide
    {
        get
        {
            return _SetHide;
        }
        set
        {
#if !UNITY_EDITOR_
            _SetHide = value;
            if (value)
            {
                IntPtr window = GetActiveWindow();
                MARGINS margins = new MARGINS { cxLeftWidth = -1 };
                DwmExtendFrameIntoClientArea(window, ref margins);
                SetWindowLong(window, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT);
                //SetLayeredWindowAttributes(window, 0, 0, LWA_COLORKEY);
                SetWindowPos(window, HWND_TOPMOST, 0, 0, 0, 0, 0);
                Application.runInBackground = true;

            }
            else
            {
                IntPtr window = GetActiveWindow();
                MARGINS margins = new MARGINS
                {
                    cxLeftWidth = 0,
                    cxRightWidth = 0,
                    cyTopHeight = 0,
                    cyBottomHeight = 0
                };
                DwmExtendFrameIntoClientArea(window, ref margins);
                SetWindowLong(window, GWL_EXSTYLE, 0);
                //SetLayeredWindowAttributes(window, 0, 0, 0);
                SetWindowPos(window, HWND_TOP, 0, 0, 0, 0, 0);
                Application.runInBackground = false;
            }
#endif
        }
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    [DllImport("user32.dll")]
    private static extern int SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);
    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

#endregion Hide*/
    private void _Keyboard_Buttons_Update()
    {
        if (Input.GetKeyDown(KeyCode.F12)) //Min max
            FullScreenToggle();
        //if (Input.GetMouseButtonDown(2) && Screen.fullScreen)
        //    SetHide = !SetHide;
    }

    private void Update()
    {
        _Keyboard_Buttons_Update();
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else Destroy(this);
    }
}
