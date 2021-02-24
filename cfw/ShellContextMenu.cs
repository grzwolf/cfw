using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Shell {
    #region ShellContextMenu
    /// <summary>
    /// Displays the shell context menu, which is the menu that is shown when a user right clicks on a file in Windows Explorer.
    /// </summary>
    /// <remarks>
    /// Andrew Vos - http://www.andrewvos.com/?p=190
    /// </remarks>
    public class ShellContextMenu {
        /// <summary>
        /// True to allow renaming.
        /// </summary>
        public bool AllowRename { get; set; }

        /// <summary>
        /// True to show only verbs.
        /// </summary>
        public bool ShowOnlyVerbs { get; set; }

        /// <summary>
        /// True to hide all verbs.
        /// </summary>
        public bool HideVerbs { get; set; }

        /// <summary>
        /// True to show extended verbs. Extended verbs should only be shown when the shift key is pressed.
        /// </summary>
        public bool ShowExtendedVerbs { get; set; }

        /// <summary>
        /// Displays the context menu.
        /// </summary>
        /// <param name="control">A Control that specifies the control with which this context menu is associated.</param>
        /// <param name="position">A Point that specifies the coordinates at which to display the menu. These coordinates are specified relative to the client coordinates of the control specified in the control parameter.</param>
        /// <param name="specialFolder">A SpecialFolder to show the context menu for.</param>
        /// <returns>One of the ContextMenuResult values.</returns>
        /// <exception cref="System.ArgumentNullException">The control parameter is null.</exception>
        /// <exception cref="System.ArgumentException">The handle of the control does not exist or the control is not visible.</exception>
        public ContextMenuResult Show(Control control, Point position, Environment.SpecialFolder specialFolder) {
            if ( control == null )
                throw new ArgumentNullException("control");
            if ( (!control.IsHandleCreated || !control.Visible) )
                throw new ArgumentException("ShellContextMenu cannot be shown on an invisible control.");

            Point screenPosition = control.PointToScreen(position);
            internalShellContextMenu iscm = new internalShellContextMenu(this, control.Handle, screenPosition);
            return iscm.Show(specialFolder);
        }

        /// <summary>
        /// Displays the context menu.
        /// </summary>
        /// <param name="control">A Control that specifies the control with which this context menu is associated.</param>
        /// <param name="position">A Point that specifies the coordinates at which to display the menu. These coordinates are specified relative to the client coordinates of the control specified in the control parameter.</param>
        /// <param name="path">A FileSystemInfo object representing the path that the context menu is being displayed for.</param>
        /// <returns>One of the ContextMenuResult values.</returns>
        /// <exception cref="System.ArgumentNullException">The control parameter is null, or the path parameter is null.</exception>
        /// <exception cref="System.ArgumentException">The handle of the control does not exist or the control is not visible.</exception>
        public ContextMenuResult Show(Control control, Point position, FileSystemInfo path) {
            if ( control == null )
                throw new ArgumentNullException("control");
            if ( (!control.IsHandleCreated || !control.Visible) )
                throw new ArgumentException("ShellContextMenu cannot be shown on an invisible control.");
            if ( path == null )
                throw new ArgumentNullException("path");

            Point screenPosition = control.PointToScreen(position);
            internalShellContextMenu iscm = new internalShellContextMenu(this, control.Handle, screenPosition);
            return iscm.Show(new FileSystemInfo[] { path });
        }

        /// <summary>
        /// Displays the context menu.
        /// </summary>
        /// <param name="control">A Control that specifies the control with which this context menu is associated.</param>
        /// <param name="position">A Point that specifies the coordinates at which to display the menu. These coordinates are specified relative to the client coordinates of the control specified in the control parameter.</param>
        /// <param name="paths">An array of FileSystemInfo objects representing the paths that the context menu is being displayed for.</param>
        /// <returns>One of the ContextMenuResult values.</returns>
        /// <exception cref="System.ArgumentNullException">The control parameter is null, or the paths parameter is null.</exception>
        /// <exception cref="System.ArgumentException">The handle of the control does not exist or the control is not visible.</exception>
        public ContextMenuResult Show(Control control, Point position, FileSystemInfo[] paths) {
            if ( control == null )
                throw new ArgumentNullException("control");
            if ( (!control.IsHandleCreated || !control.Visible) )
                throw new ArgumentException("ShellContextMenu cannot be shown on an invisible control.");
            if ( paths == null )
                throw new ArgumentNullException("paths");

            Point screenPosition = control.PointToScreen(position);
            internalShellContextMenu iscm = new internalShellContextMenu(this, control.Handle, screenPosition);
            return iscm.Show(paths);
        }

        #region internalShellContextMenu
        private class internalShellContextMenu : NativeWindow {
            private SafeNativeMethods.IContextMenu2 contextMenu2;
            private readonly ShellContextMenu shellContextMenu;
            private readonly IntPtr parent;
            private Point pos;

            public internalShellContextMenu(ShellContextMenu shellContextMenu, IntPtr parent, Point pos) {
                this.shellContextMenu = shellContextMenu;
                this.parent = parent;
                this.pos = pos;
            }

            public ContextMenuResult Show(FileSystemInfo[] paths) {
                List<IntPtr> pidls = new List<IntPtr>(paths.Length);
                Array.ForEach(paths, path => pidls.Add(SafeNativeMethods.ILCreateFromPath(path.FullName)));
                ContextMenuResult result = this.showInternal(pidls.ToArray());
                return result;
            }
            public ContextMenuResult Show(Environment.SpecialFolder specialFolder) {
                IntPtr[] pidls = new IntPtr[] { internalShellContextMenu.getPidlFromFolderId(this.parent, specialFolder) };
                return this.showInternal(pidls);
            }

            private ContextMenuResult showInternal(IntPtr[] pidls) {
                ContextMenuResult result = ContextMenuResult.NoUserFeedback;

                this.AssignHandle(this.parent);

                if ( this.contextMenu2 != null ) {
                    Marshal.ReleaseComObject(this.contextMenu2);
                    this.contextMenu2 = null;
                }
                this.contextMenu2 = internalShellContextMenu.createContextMenu2(pidls);

                using ( ContextMenu menu = new ContextMenu() ) {
                    if ( this.contextMenu2 != null ) {
                        int queryContextMenuResult = this.contextMenu2.QueryContextMenu(menu.Handle, 0, 1, 65535, internalShellContextMenu.createContextMenuFlags(this.shellContextMenu));

                        UInt32 trackPopupMenuResult = SafeNativeMethods.TrackPopupMenu(menu.Handle, SafeNativeMethods.TPM_RETURNCMD, this.pos.X, this.pos.Y, 0, this.parent, IntPtr.Zero);
                        if ( trackPopupMenuResult != 0 ) {
                            SafeNativeMethods.CMINVOKECOMMANDINFO cmici = new SafeNativeMethods.CMINVOKECOMMANDINFO();
                            cmici.cbSize = Marshal.SizeOf(cmici);
                            cmici.fMask = 0;
                            cmici.hwnd = this.Handle;
                            cmici.lpVerb = (IntPtr)((trackPopupMenuResult - 1) & 65535);
                            cmici.lpParameters = IntPtr.Zero;
                            cmici.lpDirectory = IntPtr.Zero;
                            cmici.nShow = 1;
                            cmici.dwHotKey = 0;
                            cmici.hIcon = IntPtr.Zero;
                            int invokeCommandResult = this.contextMenu2.InvokeCommand(ref cmici);
                        }
                        result = (ContextMenuResult)trackPopupMenuResult;
                    }
                }

                this.ReleaseHandle();

                return result;
            }

            private static SafeNativeMethods.IContextMenu2 createContextMenu2(IntPtr[] pidls) {
                SafeNativeMethods.IShellFolder iShellFolder = null;

                List<IntPtr> pidlList = new List<IntPtr>();
                foreach ( IntPtr pidl in pidls ) {
                    IntPtr ppidlLast = new IntPtr();
                    int result = SafeNativeMethods.SHBindToParent(pidl, SafeNativeMethods.IID_IShellFolder, ref iShellFolder, ref ppidlLast);
                    if ( result == SafeNativeMethods.S_OK ) {
                        if ( pidl != IntPtr.Zero ) {
                            pidlList.Add(ppidlLast);
                        }
                    }
                }

                if ( iShellFolder != null ) {
                    IntPtr[] apidl = pidlList.ToArray();
                    UInt32 rgfReserved = 0;
                    Guid riid = SafeNativeMethods.IID_IContextMenu;
                    object contextMenuObject = null;
                    iShellFolder.GetUIObjectOf(IntPtr.Zero, System.Convert.ToUInt32(apidl.Length), apidl, ref riid, ref rgfReserved, ref contextMenuObject);

                    if ( iShellFolder != null ) {
                        Marshal.ReleaseComObject(iShellFolder);
                        iShellFolder = null;
                    }

                    return contextMenuObject as SafeNativeMethods.IContextMenu2;
                } else {
                    return null;
                }
            }
            private static UInt32 createContextMenuFlags(ShellContextMenu shellContextMenu) {
                int flags = 0;
                if ( shellContextMenu.AllowRename )
                    flags |= SafeNativeMethods.CMF_CANRENAME;
                if ( shellContextMenu.ShowOnlyVerbs )
                    flags |= SafeNativeMethods.CMF_VERBSONLY;
                if ( shellContextMenu.HideVerbs )
                    flags |= SafeNativeMethods.CMF_NOVERBS;
                if ( shellContextMenu.ShowExtendedVerbs )
                    flags |= SafeNativeMethods.CMF_EXTENDEDVERBS;
                return (UInt32)flags;
            }
            private static IntPtr getPidlFromFolderId(IntPtr owner, Environment.SpecialFolder specialFolder) {
                IntPtr pidl = new IntPtr();
                if ( SafeNativeMethods.SHGetSpecialFolderLocation(owner, (int)specialFolder, ref pidl) == SafeNativeMethods.S_OK ) {
                    return pidl;
                }
                return IntPtr.Zero;
            }

            protected override void WndProc(ref Message m) {
                switch ( m.Msg ) {
                    case SafeNativeMethods.WM_INITMENUPOPUP:
                    case SafeNativeMethods.WM_DRAWITEM:
                    case SafeNativeMethods.WM_MEASUREITEM:
                        if ( this.contextMenu2 != null ) {
                            this.contextMenu2.HandleMenuMsg(m.Msg, m.WParam, m.LParam);
                            return;
                        }
                        break;
                    case SafeNativeMethods.WM_CONTEXTMENU:
                        return;
                    default:
                        base.WndProc(ref m);
                        break;
                }
            }
        }
        #endregion

        #region NativeMethods
        private static class SafeNativeMethods {
            #region Api
            [DllImport("shell32.dll")]
            public static extern int SHBindToParent(IntPtr pidl, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, ref IShellFolder ppv, ref IntPtr ppidlLast);

            [DllImport("shell32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr ILCreateFromPath([MarshalAs(UnmanagedType.LPTStr)] string pszPath);

            [DllImport("user32.dll")]
            public static extern UInt32 TrackPopupMenu(IntPtr hMenu, UInt32 uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

            [DllImport("shell32.dll")]
            public static extern int SHGetSpecialFolderLocation(IntPtr hwnd, int csidl, ref IntPtr ppidl);
            #endregion

            #region Constants
            public const int S_OK = 0;

            public const int WM_CONTEXTMENU = 123;
            public const int WM_INITMENUPOPUP = 279;
            public const int WM_DRAWITEM = 43;
            public const int WM_MEASUREITEM = 44;

            public const int CMF_NORMAL = 0;
            public const int CMF_DEFAULTONLY = 1;
            public const int CMF_VERBSONLY = 2;
            public const int CMF_EXPLORE = 4;
            public const int CMF_NOVERBS = 8;
            public const int CMF_CANRENAME = 16;
            public const int CMF_NODEFAULT = 32;
            public const int CMF_INCLUDESTATIC = 64;
            public const int CMF_EXTENDEDVERBS = 256;
            // public const int CMF_RESERVED = 4294901760;

            public const int TPM_RETURNCMD = 256;

            public static Guid IID_IShellFolder = new Guid("{000214E6-0000-0000-C000-000000000046}");
            public static Guid IID_IContextMenu = new Guid("{000214e4-0000-0000-c000-000000000046}");
            #endregion

            #region Structures
            [StructLayout(LayoutKind.Sequential)]
            public struct CMINVOKECOMMANDINFO {
                public int cbSize;
                public int fMask;
                public IntPtr hwnd;
                public IntPtr lpVerb;
                public IntPtr lpParameters;
                public IntPtr lpDirectory;
                public int nShow;
                public int dwHotKey;
                public IntPtr hIcon;
            }
            #endregion

            #region Interfaces
            [ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214F4-0000-0000-c000-000000000046")]
            public interface IContextMenu2 {
                [PreserveSig()]
                int QueryContextMenu(IntPtr hMenu, UInt32 indexMenu, UInt32 idCmdFirst, UInt32 idCmdLast, UInt32 uFlags);

                [PreserveSig()]
                int InvokeCommand(ref CMINVOKECOMMANDINFO pici);

                void GetCommandString(UInt32 idCmd, UInt32 uFlags, ref int pwReserved, IntPtr commandstring, UInt32 cch);

                [PreserveSig()]
                int HandleMenuMsg(int uMsg, IntPtr wParam, IntPtr lParam);
            }

            [ComImport(), Guid("000214F2-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IEnumIDList {
                [PreserveSig()]
                int Next(UInt32 celt, ref IntPtr rgelt, object pceltFetched);
                void Skip(UInt32 celt);
                void Reset();
                void Clone(ref IEnumIDList ppenum);
            }

            [ComImport(), Guid("000214E6-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IShellFolder {
                [PreserveSig()]
                int ParseDisplayName(IntPtr hwnd, IntPtr pbc, [MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName, ref UInt32 pchEaten, ref IntPtr ppidl, ref UInt32 pdwAttributes);

                [PreserveSig()]
                int EnumObjects(IntPtr hwnd, int grfFlags, ref IEnumIDList ppenumIDList);

                [PreserveSig()]
                int BindToObject(IntPtr pidl, IntPtr pbc, ref Guid riid, ref IShellFolder ppv);

                void BindToStorage(IntPtr pidl, IntPtr pbc, Guid riid, ref IntPtr ppv);

                [PreserveSig()]
                int CompareIDs(IntPtr lParam, IntPtr pidl1, IntPtr pidl2);

                void CreateViewObject(IntPtr hwndOwner, Guid riid, ref IntPtr ppv);

                [PreserveSig()]
                int GetAttributesOf([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] UInt32 cidl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IntPtr[] apidl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ref UInt32 rgfInOut);

                void GetUIObjectOf(IntPtr hwndOwner, UInt32 cidl, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, ref Guid riid, ref UInt32 rgfReserved, [MarshalAs(UnmanagedType.Interface)] ref object ppv);

                void GetDisplayNameOf(IntPtr pidl, UInt32 uFlags, ref IntPtr pName);

                void SetNameOf(IntPtr hwnd, IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszName, UInt32 uFlags, ref IntPtr ppidlOut);
            }
            #endregion
        }
        #endregion
    }
    #endregion

    #region ContextMenuResult
    /// <summary>Specifies identifiers to indicate the return value of a shell context menu.</summary>
    public enum ContextMenuResult {
        /// <summary>An error occured and the context menu couldn't be displayed.</summary>
        ContextMenuError,

        /// <summary>The context menu displayed without error, and the user clicked nothing.</summary>
        NoUserFeedback,

        /// <summary>The context menu displayed without error, and the user clicked Cut.</summary>
        Cut = 25,

        /// <summary>The context menu displayed without error, and the user clicked Copy.</summary>
        Copy = 26,

        /// <summary>The context menu displayed without error, and the user clicked Paste.</summary>
        Paste = 27,

        /// <summary>The context menu displayed without error, and the user clicked Create Shortcut.</summary>
        CreateShortcut = 17,

        /// <summary>The context menu displayed without error, and the user clicked Delete.</summary>
        Delete = 18,

        /// <summary>The context menu displayed without error, and the user clicked Rename.</summary>
        Rename = 19,

        /// <summary>The context menu displayed without error, and the user clicked Properties.</summary>
        Properties = 20,

        /// <summary>The context menu displayed without error, and the user clicked Sharing and Security.</summary>
        SharingAndSecurity = 94
    }
    #endregion
}