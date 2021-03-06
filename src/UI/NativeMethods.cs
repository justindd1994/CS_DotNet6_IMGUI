using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;

public static class WindowsNativeMethods
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_LAYERED = 0x80000;
    private const int WS_EX_TRANSPARENT = 0x20;

    private const int SW_HIDE = 0x00;
    private const int SW_SHOW = 0x05;

    private static bool isClickable = true;
    private static IntPtr GWL_EXSTYLE_CLICKABLE = IntPtr.Zero;
    private static IntPtr GWL_EXSTYLE_NOT_CLICKABLE = IntPtr.Zero;

    /// <summary>
    /// Allows the SDL2Window to become transparent.
    /// </summary>
    /// <param name="handle">
    /// Veldrid window handle in IntPtr format.
    /// </param>
    internal static void InitTransparency(IntPtr handle)
    {
        GWL_EXSTYLE_CLICKABLE = GetWindowLongPtr(handle, GWL_EXSTYLE);
        GWL_EXSTYLE_NOT_CLICKABLE = new IntPtr(
            GWL_EXSTYLE_CLICKABLE.ToInt64() | WS_EX_LAYERED | WS_EX_TRANSPARENT);

        var margins = Margins.FromRectangle(new Rectangle(-1, -1, -1, -1));
        DwmExtendFrameIntoClientArea(handle, ref margins);
    }

    /// <summary>
    /// Enables (clickable) / Disables (not clickable) the SDL2Window keyboard/mouse inputs.
    /// NOTE: This function depends on InitTransparency being called when the SDL2Winhdow was created.
    /// </summary>
    /// <param name="handle">Veldrid window handle in IntPtr format.</param>
    /// <param name="wantClickable">Set to true if you want to make the window clickable otherwise false.</param>
    internal static void SetOverlayClickable(IntPtr handle, bool wantClickable)
    {
        if (!isClickable && wantClickable)
        {
            SetWindowLongPtr(handle, GWL_EXSTYLE, GWL_EXSTYLE_CLICKABLE);
            BringToForeground(handle);
            isClickable = true;
        }
        else if (isClickable && !wantClickable)
        {
            SetWindowLongPtr(handle, GWL_EXSTYLE, GWL_EXSTYLE_NOT_CLICKABLE);
            isClickable = false;
        }
    }

    /// <summary>
    /// Allows showing/hiding the console window.
    /// </summary>
    internal static void SetWindowVisibility(IntPtr handle, bool visible)
    {
        if (visible)
            ShowWindow(handle, SW_SHOW);
        else
            ShowWindow(handle, SW_HIDE);
    }

    internal static void BringToForeground(IntPtr handle)
    {
        SwitchToThisWindow(handle, true);
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins pMarInset);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    // [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
    // private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll")]
    public static extern void SwitchToThisWindow(IntPtr hWnd, bool turnon);

    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
     public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

    [StructLayout(LayoutKind.Sequential)]
    private struct Margins
    {
        private int left;
        private int right;
        private int top;
        private int bottom;

        public static Margins FromRectangle(Rectangle rectangle)
        {
            var margins = new Margins
            {
                left = rectangle.Left,
                right = rectangle.Right,
                top = rectangle.Top,
                bottom = rectangle.Bottom
            };
            return margins;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator Point(POINT p)
        {
            return new Point(p.X, p.Y);
        }

        public static implicit operator Vector2(POINT p)
        {
            return new Vector2(p.X, p.Y);
        }
    }
}