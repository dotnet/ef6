// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Runtime.InteropServices;

    // This class is shared between assemblies
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    internal static class NativeMethods
    {
        // used in a function that expects a ref parameter - cannot be readonly
        public static Guid IID_IUnknown = new Guid("00000000-0000-0000-c000-000000000046");

        public static readonly Guid GUID_VSStandardCommandSet97 = new Guid("{5efc7975-14bc-11cf-9b2b-00aa00573819}");

        public static bool Succeeded(int hr)
        {
            return (hr >= 0);
        }

        public static bool Failed(int hr)
        {
            return (hr < 0);
        }

        public static int ThrowOnFailure(int hr)
        {
            if (Failed(hr))
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            return hr;
        }

        public const int
            S_FALSE = 0x00000001,
            S_OK = 0x00000000,
            E_FAIL = unchecked((int)0x80004005),
            E_NOINTERFACE = unchecked((int)0x80004002),
            E_INVALIDARG = unchecked((int)0x80070057),
            SVC_E_UNKNOWNSERVICE = unchecked((int)0x80004005);

        public const uint
            VSITEMID_ROOT = unchecked((uint)-2);

        // for ISelectionContainer flags
        public const uint
            ALL = 0x1,
            SELECTED = 0x2;

        [DllImport("uxtheme", CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(IntPtr hWndTreeView, string appName, string subIdList);

        public const int LB_GETITEMRECT = 0x0198;

        [DllImport("User32", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

        #region SendMessage overloads

        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable")]
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, out Rectangle rect);

        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable")]
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable")]
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable")]
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable")]
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int[] lParam);

        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable")]
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, out RECT rect);

        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable")]
        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SendMessage")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref TOOLINFO info);

        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable")]
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref MSG passMsg);

        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable")]
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref HDITEM passMsg);

        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable")]
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref HDHITTESTINFO hitInfo);

        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable")]
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref HDLAYOUT layoutInfo);

        #endregion

        [StructLayout(LayoutKind.Sequential)]
        public struct Rectangle
        {
            internal int Left;
            internal int Top;
            internal int Right;
            internal int Bottom;

            public Rectangle(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public Rectangle(System.Drawing.Rectangle rect)
            {
                Left = rect.Left;
                Top = rect.Top;
                Right = rect.Right;
                Bottom = rect.Bottom;
            }
        }

        [Flags]
        public enum RedrawWindowFlags
        {
            Invalidate = 0x0001,
            InternalPaint = 0x0002,
            Erase = 0x0004,
            Validate = 0x0008,
            NoInternalPaint = 0x0010,
            NoErase = 0x0020,
            NoChildren = 0x0040,
            AllChildren = 0x0080,
            UpdateNow = 0x0100,
            EraseNow = 0x0200,
            Frame = 0x0400,
            NoFrame = 0x0800,
        }

        public const int
            UIS_CLEAR = 2,
            UISF_HIDEFOCUS = 0x1;

        public const int LB_GETITEMHEIGHT = 0x01A1;

        // Tree View messages (see http://msdn.microsoft.com/en-us/library/ff486106(v=vs.85).aspx)
        public const int TVS_EX_DOUBLEBUFFER = 0x0004;
        public const int TVS_EX_FADEINOUTEXPANDOS = 0x0040;
        public const int TVS_NONEVENHEIGHT = 0x4000;
        public const int TVM_SETEXTENDEDSTYLE = 0x1100 + 44;
        public const int TVM_GETEXTENDEDSTYLE = 0x1100 + 45;

        // public constants for Accessibility readers through NotifyWinEvents.
        public const int
            EVENT_OBJECT_CREATE = 0x8000,
            EVENT_OBJECT_DESTROY = 0x8001,
            EVENT_OBJECT_SHOW = 0x8002,
            EVENT_OBJECT_HIDE = 0x8003,
            EVENT_OBJECT_REORDER = 0x8004,
            EVENT_OBJECT_FOCUS = 0x8005,
            EVENT_OBJECT_SELECTION = 0x8006,
            EVENT_OBJECT_SELECTIONADD = 0x8007,
            EVENT_OBJECT_SELECTIONREMOVE = 0x8008,
            EVENT_OBJECT_SELECTIONWITHIN = 0x8009,
            EVENT_OBJECT_STATECHANGE = 0x800A,
            EVENT_OBJECT_LOCATIONCHANGE = 0x800B,
            EVENT_OBJECT_NAMECHANGE = 0x800C,
            EVENT_OBJECT_DESCRIPTIONCHANGE = 0x800D,
            EVENT_OBJECT_VALUECHANGE = 0x800E,
            EVENT_OBJECT_PARENTCHANGE = 0x800F,
            EVENT_OBJECT_HELPCHANGE = 0x8010,
            EVENT_OBJECT_DEFACTIONCHANGE = 0x8011,
            EVENT_OBJECT_ACCELERATORCHANGE = 0x8012;

        #region Windows events constants

        internal const int
            CS_DROPSHADOW = 0x00020000,
            EM_GETRECT = 0x00B2,
            EM_SETSEL = 0x00B1,
            ES_AUTOHSCROLL = 0x0080,
            ES_AUTOVSCROLL = 0x0040,
            GWL_EXSTYLE = (-20),
            GWL_STYLE = (-16),
            HC_ACTION = 0,
            HDS_HORZ = 0x0000,
            HDS_BUTTONS = 0x0002,
            HDS_HOTTRACK = 0x0004,
            HDS_DRAGDROP = 0x0040,
            HDS_FULLDRAG = 0x0080,
            HDM_GETITEMCOUNT = (0x1200 + 0),
            HDM_INSERTITEMW = (0x1200 + 10),
            HDM_DELETEITEM = (0x1200 + 2),
            HDM_GETITEMW = (0x1200 + 11),
            HDM_SETITEMW = (0x1200 + 12),
            HDM_LAYOUT = (0x1200 + 5),
            HDM_HITTEST = (0x1200 + 6),
            HDM_SETIMAGELIST = (0x1200 + 8),
            HDM_ORDERTOINDEX = (0x1200 + 15),
            HDM_GETORDERARRAY = (0x1200 + 17),
            HDM_SETORDERARRAY = (0x1200 + 18),
            HDN_ITEMCLICKW = ((0 - 300) - 22),
            HDN_ITEMDBLCLICKW = ((0 - 300) - 23),
            HDN_DIVIDERDBLCLICKW = ((0 - 300) - 25),
            HDN_BEGINTRACKW = ((0 - 300) - 26),
            HDN_ENDTRACKW = ((0 - 300) - 27),
            HDN_TRACKW = ((0 - 300) - 28),
            HDN_BEGINDRAG = ((0 - 300) - 10),
            HDN_ENDDRAG = ((0 - 300) - 11),
            HTTRANSPARENT = (-1),
            ICC_LISTVIEW_CLASSES = 0x00000001,
            ICC_TAB_CLASSES = 0x00000008,
            LBS_NODATA = 0x2000,
            LBS_NOINTEGRALHEIGHT = 0x0100,
            LBS_NOSEL = 0x4000,
            LBS_NOTIFY = 0x0001,
            LBS_OWNERDRAWFIXED = 0x0010,
            LB_GETANCHORINDEX = 0x019D,
            LB_GETCOUNT = 0x018B,
            LB_GETHORIZONTALEXTENT = 0x0193,
            LB_GETSELCOUNT = 0x0190,
            LB_GETTOPINDEX = 0x018E,
            LB_ITEMFROMPOINT = 0x01A9,
            LB_SETANCHORINDEX = 0x019C,
            LB_SETCOUNT = 0x01A7,
            LB_SETHORIZONTALEXTENT = 0x0194,
            LB_SETITEMHEIGHT = 0x01A0,
            LB_SETSEL = 0x0185,
            LB_SETTOPINDEX = 0x0197,
            MA_NOACTIVATE = 0x0003,
            MK_SHIFT = 4,
            MK_CONTROL = 8,
            MSGF_COMMCTRL_BEGINDRAG = 0x4200,
            QS_ALLINPUT = QS_INPUT | QS_POSTMESSAGE | QS_TIMER | QS_PAINT | QS_HOTKEY | QS_SENDMESSAGE,
            QS_HOTKEY = 0x0080,
            QS_INPUT = QS_MOUSE | QS_KEY,
            QS_KEY = 0x0001,
            QS_MOUSE = QS_MOUSEMOVE | QS_MOUSEBUTTON,
            QS_MOUSEBUTTON = 0x0004,
            QS_MOUSEMOVE = 0x0002,
            QS_PAINT = 0x0020,
            QS_POSTMESSAGE = 0x0008,
            QS_SENDMESSAGE = 0x0040,
            QS_TIMER = 0x0010,
            TTDT_INITIAL = 3,
            TTDT_RESHOW = 1,
            TTF_IDISHWND = 0x0001,
            TTF_TRANSPARENT = 0x0100,
            TTM_ACTIVATE = WM_USER + 1,
            TTM_ADDTOOLA = WM_USER + 4,
            TTM_ADDTOOLW = WM_USER + 50,
            TTM_ADJUSTRECT = WM_USER + 31,
            TTM_GETCURRENTTOOLA = WM_USER + 15,
            TTM_GETCURRENTTOOLW = WM_USER + 59,
            TTM_NEWTOOLRECTA = WM_USER + 6,
            TTM_NEWTOOLRECTW = WM_USER + 52,
            TTM_POP = WM_USER + 28,
            TTM_RELAYEVENT = WM_USER + 7,
            TTM_SETDELAYTIME = WM_USER + 3,
            TTN_NEEDTEXTA = ((0 - 520) - 0),
            TTN_NEEDTEXTW = ((0 - 520) - 10),
            TTN_POP = ((0 - 520) - 2),
            TTN_SHOW = ((0 - 520) - 1),
            TTS_NOPREFIX = 0x02,
            WA_INACTIVE = 0,
            WH_MOUSE = 7,
            WM_ACTIVATE = 0x0006,
            WM_KILLFOCUS = 0x0008,
            WM_CANCELMODE = 0x001F,
            WM_CHAR = 0x0102,
            WM_CHARTOITEM = 0x002F,
            WM_CLOSE = 0x0010,
            WM_CONTEXTMENU = 0x007B,
            WM_DRAWITEM = 0x002B,
            WM_ERASEBKGND = 0x0014,
            WM_HSCROLL = 0x0114,
            WM_KEYDOWN = 0x0100,
            WM_LBUTTONDBLCLK = 0x0203,
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MBUTTONDBLCLK = 0x0209,
            WM_MBUTTONDOWN = 0x0207,
            WM_MBUTTONUP = 0x0208,
            WM_MOUSEACTIVATE = 0x0021,
            WM_MOUSEFIRST = 0x0200,
            WM_MOUSELAST = 0x020A,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSELEAVE = 0x02A3,
            WM_MOUSEWHEEL = 0x020A,
            WM_NCCALCSIZE = 0x0083,
            WM_NCHITTEST = 0x0084,
            WM_NCLBUTTONDOWN = 0x00A1,
            WM_NCMBUTTONDOWN = 0x00A7,
            WM_NCMOUSEMOVE = 0x00A0,
            WM_NCRBUTTONDOWN = 0x00A4,
            WM_NOTIFY = 0x004E,
            WM_PAINT = 0x000F,
            WM_RBUTTONDBLCLK = 0x0206,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205,
            WM_REFLECT = WM_USER + 0x1C00,
            WM_SETCURSOR = 0x0020,
            WM_SETFOCUS = 0x0007,
            WM_SETREDRAW = 0x000B,
            WM_SIZE = 0x0005,
            WM_SYSCOLORCHANGE = 0x0015,
            WM_THEMECHANGED = 0x031A,
            WM_UPDATEUISTATE = 0x0128,
            WM_USER = 0x0400,
            WM_VSCROLL = 0x0115,
            WM_WINDOWPOSCHANGED = 0x0047,
            WM_WINDOWPOSCHANGING = 0x0046,
            WM_XBUTTONDBLCLK = 0x020D,
            WM_XBUTTONDOWN = 0x020B,
            WM_XBUTTONUP = 0x020C,
            WM_CUT = 0x0300,
            WM_COPY = 0x0301,
            WM_PASTE = 0x0302,
            WS_BORDER = 0x00800000,
            WS_CLIPCHILDREN = 0x02000000,
            WS_EX_LEFTSCROLLBAR = 0x00004000,
            WS_EX_TOOLWINDOW = 0x00000080,
            WS_EX_TOPMOST = 0x00000008,
            WS_HSCROLL = 0x00100000,
            WS_POPUP = unchecked((int)0x80000000),
            WS_VSCROLL = 0x00200000;

        #endregion

        internal const string WC_LISTBOX = "LISTBOX",
                              TOOLTIPS_CLASS = "tooltips_class32",
                              WC_HEADER = "SysHeader32",
                              WC_TREEVIEW = "TREEVIEW",
                              WC_EXPLORER = "Explorer";

        internal static readonly IntPtr LPSTR_TEXTCALLBACK = new IntPtr(-1);
        internal static readonly int TTM_GETCURRENTTOOL = Marshal.SystemDefaultCharSize == 1 ? TTM_GETCURRENTTOOLA : TTM_GETCURRENTTOOLW;
        internal static readonly int TTM_NEWTOOLRECT = Marshal.SystemDefaultCharSize == 1 ? TTM_NEWTOOLRECTA : TTM_NEWTOOLRECTW;
        internal static readonly int TTM_ADDTOOL = Marshal.SystemDefaultCharSize == 1 ? TTM_ADDTOOLA : TTM_ADDTOOLW;

#if DEBUG
        //Types used to assert offsets, nothing else

        [ComVisible(false)]
        [StructLayout(LayoutKind.Sequential)]
        internal struct NMHDR
        {
            public IntPtr hwndFrom;
            public int idFrom;
            public int code;
        }

        [ComVisible(false)]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct TOOLTIPTEXT
        {
            public NMHDR hdr;
            public IntPtr lpszText;
            //			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=80)]
            //			public string szText;
            //			public IntPtr hinst;
            //			public int    uFlags;
        }
#endif

        internal enum ScrollAction : short
        {
            LineUp = 0,
            LineLeft = 0,
            LineDown = 1,
            LineRight = 1,
            PageUp = 2,
            PageLeft = 2,
            PageDown = 3,
            PageRight = 3,
            ThumbPosition = 4,
            ThumbTrack = 5,
            Top = 6,
            Left = 6,
            Bottom = 7,
            Right = 7,
            EndScroll = 8,
        };

        internal enum ScrollBarType
        {
            Horizontal = 0,
            Vertical = 1,
            Control = 2,
            Both = 3,
        }

        internal enum ScrollInfoFlags
        {
            Range = 0x0001,
            Page = 0x0002,
            Position = 0x0004,
            DisableNoScroll = 0x0008,
            TrackPosition = 0x0010,
            All = Range | Page | Position | DisableNoScroll | TrackPosition,
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SCROLLINFO
        {
            public int cbSize;
            public ScrollInfoFlags fMask;
            public int nMin;
            public int nMax;
            public int nPage;
            public int nPos;
            public int nTrackPos;

            public SCROLLINFO(ScrollInfoFlags mask, int min, int max, int page, int pos)
            {
                cbSize = 28; //ndirect.DllLib.sizeOf(this);
                fMask = mask;
                nMin = min;
                nMax = max;
                nPage = page;
                nPos = pos;
                nTrackPos = 0;
            }

            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dummy")]
            public SCROLLINFO(int dummy)
            {
                cbSize = 28; //ndirect.DllLib.sizeOf(this);
                fMask = 0;
                nMin = 0;
                nMax = 0;
                nPage = 0;
                nPos = 0;
                nTrackPos = 0;
            }
        }

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetScrollInfo(IntPtr hWnd, ScrollBarType fnBar, ref SCROLLINFO si);

        internal enum ScrollWindowFlags : short
        {
            ScrollChildren = 0x1,
            Invalidate = 0x2,
            Erase = 0x4,
            SmoothScroll = 0x10,
        }

        internal static int ScrollWindowFlagsParam(ScrollWindowFlags flags)
        {
            flags &= ~ScrollWindowFlags.SmoothScroll;
            return (int)flags;
        }

        internal static int ScrollWindowFlagsParam(ScrollWindowFlags flags, short smoothScrollInterval)
        {
            flags |= ScrollWindowFlags.SmoothScroll;
            return MAKELONG((int)flags, smoothScrollInterval);
        }

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int SetScrollPos(
            IntPtr hWnd, // handle to window
            ScrollBarType nBar, // scroll bar
            int nPos, // new position of scroll box
            [MarshalAs(UnmanagedType.Bool)] bool bRedraw // redraw flag
            );

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int GetScrollPos(IntPtr hWnd, ScrollBarType nBar);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int SetScrollRange(
            IntPtr hWnd, // handle to window
            ScrollBarType nBar, // scroll bar
            int nMinPos, // minimum scrolling position
            int nMaxPos, // maximum scrolling position
            [MarshalAs(UnmanagedType.Bool)] bool bRedraw // redraw flag
            );

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ScrollWindowEx(
            IntPtr hWnd, int nXAmount, int nYAmount, ref RECT rectScrollRegion, ref RECT rectClip, IntPtr hrgnUpdate, out RECT prcUpdate,
            int flags);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ScrollWindowEx(
            IntPtr hWnd, int nXAmount, int nYAmount, ref RECT rectScrollRegion, ref RECT rectClip, IntPtr hrgnUpdate, IntPtr nullUpdate,
            int flags);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ScrollWindowEx(
            IntPtr hWnd, int nXAmount, int nYAmount, IntPtr prcScrollRegion, IntPtr prcClip, IntPtr hrgnUpdate, IntPtr prcUpdate, int flags);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ScrollWindowEx(
            IntPtr hWnd, int nXAmount, int nYAmount, IntPtr prcScrollRegion, IntPtr prcClip, IntPtr hrgnUpdate, out RECT prcUpdate,
            int flags);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CallMsgFilter([In] ref MSG msg, int nCode);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool TranslateMessage([In] ref MSG msg);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr DispatchMessage([In] ref MSG msg);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr GetCapture();

        [DllImport("User32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr GetFocus();

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll",
#if WIN64
         EntryPoint="GetWindowLongPtr",
#endif
            CharSet = CharSet.Unicode)
        ]
        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable")]
        internal static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        internal static IntPtr GetWindowStyle(IntPtr hWnd)
        {
            return GetWindowLong(hWnd, GWL_STYLE);
        }

        internal static IntPtr GetWindowExStyle(IntPtr hWnd)
        {
            return GetWindowLong(hWnd, GWL_EXSTYLE);
        }

        [DllImport("gdi32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr /* HPALETTE */ SelectPalette(IntPtr hDC, IntPtr hPal, int fForceBackground);

        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            public int x;
            public int y;

            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public int width
            {
                get { return right - left; }
            }

            public int height
            {
                get { return bottom - top; }
            }

            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            public RECT(System.Drawing.Rectangle rect)
            {
                left = rect.Left;
                top = rect.Top;
                right = rect.Right;
                bottom = rect.Bottom;
            }

            public RECT(Rectangle rect)
            {
                left = rect.Left;
                top = rect.Top;
                right = rect.Right;
                bottom = rect.Bottom;
            }

            public static RECT FromXYWH(int x, int y, int width, int height)
            {
                return new RECT(
                    x,
                    y,
                    x + width,
                    y + height);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SIZE
        {
            public int cx;
            public int cy;
        }

        [Flags]
        internal enum SetWindowPosFlags
        {
            SWP_NOSIZE = 0x0001,
            SWP_NOMOVE = 0x0002,
            SWP_NOZORDER = 0x0004,
            SWP_NOREDRAW = 0x0008,
            SWP_NOACTIVATE = 0x0010,
            SWP_FRAMECHANGED = 0x0020,
            SWP_SHOWWINDOW = 0x0040,
            SWP_HIDEWINDOW = 0x0080,
            SWP_NOCOPYBITS = 0x0100,
            SWP_NOOWNERZORDER = 0x0200,
            SWP_NOSENDCHANGING = 0x0400,
            SWP_DRAWFRAME = 0x0020,
            SWP_NOREPOSITION = 0x0200,
            SWP_DEFERERASE = 0x2000,
            SWP_ASYNCWINDOWPOS = 0x4000,
        }

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int GetClientRect(IntPtr hWnd, out RECT rect);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int GetWindowRect(IntPtr hWnd, out RECT rect);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int ValidateRect(IntPtr hWnd, [In] ref RECT rect);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int ValidateRect(IntPtr hWnd, IntPtr nullRect);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int InvalidateRect(IntPtr hWnd, [In] ref RECT rect, [MarshalAs(UnmanagedType.Bool)] bool erase);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int InvalidateRect(IntPtr hWnd, [In] IntPtr nullRect, [MarshalAs(UnmanagedType.Bool)] bool erase);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool RedrawWindow(IntPtr hwnd, [In] ref RECT rcUpdate, IntPtr hrgnUpdate, RedrawWindowFlags flags);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RedrawWindow(IntPtr hwnd, [In] ref Rectangle rcUpdate, IntPtr hrgnUpdate, RedrawWindowFlags flags);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowCaret(IntPtr hwnd);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool HideCaret(IntPtr hwnd);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(out POINT pt);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ScreenToClient(IntPtr hWnd, ref POINT pt);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, SetWindowPosFlags flags);

        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable")]
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AdjustWindowRectEx(ref RECT lpRect, IntPtr dwStyle, [MarshalAs(UnmanagedType.Bool)] bool bMenu, IntPtr dwExStyle);

        [StructLayout(LayoutKind.Sequential)]
        internal class DRAWITEMSTRUCT
        {
            public int CtlType;
            public int CtlId;
            public int itemId;
            public int itemAction;
            public int itemState;
            public IntPtr hwndItem;
            public IntPtr hDC;
            public RECT rcItem;
            public IntPtr itemData;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NCCALCSIZE_PARAMS
        {
            public RECT rgrc0;
            public RECT rgrc1;
            public RECT rgrc2;
            public IntPtr lppos;
            public const int rgrc0TopOffset = 4;
#if DEBUG
            [SuppressMessage("Microsoft.Usage", "CA2207:InitializeValueTypeStaticFieldsInline")]
            static NCCALCSIZE_PARAMS()
            {
                var offset1 = Marshal.OffsetOf(typeof(NCCALCSIZE_PARAMS), "rgrc0");
                var offset2 = Marshal.OffsetOf(typeof(RECT), "top");
                Debug.Assert(((int)offset1 + (int)offset2) == rgrc0TopOffset);
            }
#endif
        }

        internal static int MAKELONG(int low, int high)
        {
            return (high << 16) | (low & 0xffff);
        }

        internal static IntPtr MAKELPARAM(int low, int high)
        {
            return (IntPtr)((high << 16) | (low & 0xffff));
        }

        public static int UnsignedHIWORD(int n)
        {
            return (n >> 16) & 0xffff;
        }

        public static int UnsignedLOWORD(int n)
        {
            return n & 0xffff;
        }

        public static int UnsignedLOWORD(IntPtr n)
        {
            return UnsignedLOWORD((int)n);
        }

        public static int SignedHIWORD(int n)
        {
            int i = (short)((n >> 16) & 0xffff);

            i = i << 16;
            i = i >> 16;
            return i;
        }

        public static int SignedHIWORD(IntPtr n)
        {
            return SignedHIWORD((int)n);
        }

        public static int SignedLOWORD(int n)
        {
            int i = (short)(n & 0xFFFF);

            i = i << 16;
            i = i >> 16;
            return i;
        }

        public static int SignedLOWORD(IntPtr n)
        {
            return SignedLOWORD((int)n);
        }

        internal enum RegionType
        {
            Error = 0,
            Null = 1,
            Simple = 2,
            Complex = 3,
        }

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern RegionType GetUpdateRgn(IntPtr hWnd, IntPtr hRgn, [MarshalAs(UnmanagedType.Bool)] bool bErase);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetUpdateRect(IntPtr hWnd, out RECT lpRect, [MarshalAs(UnmanagedType.Bool)] bool bErase);

        [StructLayout(LayoutKind.Sequential)]
        [ComVisible(false)]
        internal struct MSG
        {
            public IntPtr hwnd;
            public int message;
            public IntPtr wParam;
            public IntPtr lParam;
            public int time;
            // pt was a by-value POINT structure
            public int pt_x;
            public int pt_y;
        }

        internal enum PeekMessageAction
        {
            NoRemove = 0x0000,
            Remove = 0x0001,
            NoYield = 0x0002,
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool PeekMessage(out MSG msg, IntPtr hwnd, int msgMin, int msgMax, PeekMessageAction remove);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int GetMessageTime();

        [DllImport("user32.dll", EntryPoint = "GetMessagePos", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int _GetMessagePos();

        internal static Point GetMessagePos()
        {
            return new Point(_GetMessagePos());
        }

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool MessageBeep(int type);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal class INITCOMMONCONTROLSEX
        {
            public int dwSize = 8; //ndirect.DllLib.sizeOf(this);
            public int dwICC;
        }

        [DllImport("comctl32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool InitCommonControlsEx(INITCOMMONCONTROLSEX icc);

        [ComVisible(false)]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct TOOLINFO
        {
            public void SetSize()
            {
                Debug.Assert(Marshal.SizeOf(typeof(TOOLINFO)) == 40);
                cbSize = 40;
            }

            public int cbSize; // ndirect.DllLib.sizeOf( this )
            public int uFlags;
            public IntPtr hwnd;
            public int uId;
            public RECT rect;
            public IntPtr hinst;
            public IntPtr lpszText; // Use custom buffer to avoid extra marshalling
            //public IntPtr	lParam;
        }

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int GetWindowThreadProcessId(HandleRef hWnd, out int lpdwProcessId);

        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int GetCurrentThreadId();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SetWindowsHookEx(int hookid, HookProc pfnhook, HandleRef hinst, int threadid);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(HandleRef hhook);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CallNextHookEx(HandleRef hhook, int code, IntPtr wparam, IntPtr lparam);

        [DllImport("user32", CharSet = CharSet.Unicode)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern short GetKeyState(int keyCode);

        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable")]
        [DllImport("user32", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int MsgWaitForMultipleObjects(int nCount, int pHandles, [MarshalAs(UnmanagedType.Bool)] bool fWaitAll, int dwMilliseconds, int dwWakeMask);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential)]
        internal class MouseHookStruct
        {
            // pt was a by-value POINT structure
            public int x;
            public int y;
            public IntPtr handle;
            public int wHitTestCode;
            public int extraInfo;
        }

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int ValidateRect(IntPtr hWnd, [In] ref Rectangle rect);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetUpdateRect(IntPtr hWnd, IntPtr lpRect, [MarshalAs(UnmanagedType.Bool)] bool bErase);

        public delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

        public static HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);

        [StructLayout(LayoutKind.Sequential)]
        internal class NMHEADER
        {
            public IntPtr hwndFrom;
            public int idFrom;
            public int code;
            public int iItem;
            public int iButton;
            public IntPtr pItem; // HDITEM*
            public const int iItemOffset = 12;
#if DEBUG
            static NMHEADER()
            {
                Debug.Assert((int)Marshal.OffsetOf(typeof(NMHEADER), "iItem") == iItemOffset);
            }
#endif
        }

        internal struct HDHITTESTINFO
        {
            [Flags]
            internal enum Flags
            {
                HHT_NOWHERE = 0x0001,
                HHT_ONHEADER = 0x0002,
                HHT_ONDIVIDER = 0x0004,
                HHT_ONDIVOPEN = 0x0008,
                HHT_ABOVE = 0x0100,
                HHT_BELOW = 0x0200,
                HHT_TORIGHT = 0x0400,
                HHT_TOLEFT = 0x0800,
            }

            public HDHITTESTINFO(Point pos)
            {
                pt = new POINT(pos.X, pos.Y);
                flags = Flags.HHT_NOWHERE;
                iItem = 0;
            }

            public POINT pt;
            public Flags flags;
            public int iItem;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HDITEM
        {
            [Flags]
            internal enum Mask
            {
                HDI_WIDTH = 0x0001,
                HDI_HEIGHT = 0x0001,
                HDI_TEXT = 0x0002,
                HDI_FORMAT = 0x0004,
                HDI_LPARAM = 0x0008,
                HDI_BITMAP = 0x0010,
                HDI_IMAGE = 0x0020,
                HDI_DI_SETITEM = 0x0040,
                HDI_ORDER = 0x0080,
            }

            [Flags]
            internal enum Format
            {
                HDF_LEFT = 0,
                HDF_RIGHT = 1,
                HDF_CENTER = 2,
                HDF_JUSTIFYMASK = 0x0003,
                HDF_RTLREADING = 4,
                HDF_OWNERDRAW = unchecked(0x8000),
                HDF_STRING = 0x4000,
                HDF_BITMAP = 0x2000,
                HDF_BITMAP_ON_RIGHT = 0x1000,
                HDF_IMAGE = 0x0800,
                HDF_SORTUP = 0x0400,
                HDF_SORTDOWN = 0x0200,
            }

            public Mask mask;
            public int cxy;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszText;

            public IntPtr hbm;
            public int cchTextMax;
            public Format fmt;
            public IntPtr lParam;
            public int iImage; // index of bitmap in ImageList
            public int iOrder;
            public uint type; // [in] filter type (defined what pvFilter is a pointer to)
            public IntPtr pvFilter; // [in] filter data see above
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public SetWindowPosFlags flags;
            public const int flagsOffset = 24;
#if DEBUG
            [SuppressMessage("Microsoft.Usage", "CA2207:InitializeValueTypeStaticFieldsInline")]
            static WINDOWPOS()
            {
                Debug.Assert((int)Marshal.OffsetOf(typeof(WINDOWPOS), "flags") == flagsOffset);
            }
#endif
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HDLAYOUT
        {
            public IntPtr prc;
            public IntPtr pwpos;
        }

        #region Accessibility events and messages

        // public constants for Accessibility readers through NotifyWinEvents.

        public const int CHILDID_SELF = 0;
        public const int WM_GETOBJECT = 0x003D;

        public const string uuid_IAccessible = "{618736E0-3C3D-11CF-810C-00AA00389B71}";

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern void NotifyWinEvent(int winEvent, HandleRef hwnd, int objType, int objId);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int IsWinEventHookInstalled(int winEvent);

        [DllImport("oleacc.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LresultFromObject(ref Guid refiid, IntPtr wParam, HandleRef pAcc);

        #endregion

        #region theme imports

        internal const int TVP_GLYPH = 2;
        internal const int GLPS_CLOSED = 1;
        internal const int GLPS_OPENED = 2;

        internal enum THEME_SIZE
        {
            TS_MIN,
            TS_TRUE,
            TS_DRAW
        }

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr OpenThemeData(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] string classList);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        internal static extern int CloseThemeData(IntPtr hTheme);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        internal static extern int DrawThemeBackground(
            IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, ref RECT pRect, IntPtr pClipRect);

        // pClipRect marshalled as IntPtr so we can pass null

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        internal static extern int GetThemePartSize(
            IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, IntPtr pClipRect, THEME_SIZE eSize, ref SIZE pSize);

        #endregion

        #region user32 imports

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr GetDC(IntPtr hdc);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern int FillRect(IntPtr hdc, ref RECT lprc, IntPtr hbr);

        #endregion user32 imports

        #region gdi imports

        [DllImport("gdi32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("gdi32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool RectInRegion(IntPtr hRgn, ref RECT prc);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr SelectObject(IntPtr hdc, IntPtr hGdiObj);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateSolidBrush(int crColor);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        internal static extern int DeleteDC(IntPtr hDC);

        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable")]
        [DllImport("gdi32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr DeleteObject(IntPtr hObject);

        internal static int RGB(byte r, byte g, byte b)
        {
            return r | (g << 8) | (b << 16);
        }

        #endregion

        #region OS version helpers

        internal static readonly bool WinXPOrHigher = _WinXPOrHigher;

        private static bool _WinXPOrHigher
        {
            get
            {
                var ver = Environment.OSVersion.Version;
                var xp = new Version(5, 1);
                return (ver >= xp);
            }
        }

        internal static readonly bool WinVistaOrHigher = _WinVistaOrHigher;

        private static bool _WinVistaOrHigher
        {
            get
            {
                var ver = Environment.OSVersion.Version;
                var xp = new Version(6, 0);
                return (ver >= xp);
            }
        }

        #endregion

        #region painting imports

        [StructLayout(LayoutKind.Sequential)]
        internal struct PAINTSTRUCT
        {
            public IntPtr hdc;
            public bool fErase;
            // rcPaint was a by-value RECT structure
            public int rcPaint_left;
            public int rcPaint_top;
            public int rcPaint_right;
            public int rcPaint_bottom;
            public bool fRestore;
            public bool fIncUpdate;
            public int reserved1;
            public int reserved2;
            public int reserved3;
            public int reserved4;
            public int reserved5;
            public int reserved6;
            public int reserved7;
            public int reserved8;
        }

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr BeginPaint(IntPtr hWnd, [In] [Out] ref PAINTSTRUCT lpPaint);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT lpPaint);

        #endregion
    }
}
