using ChangeModifiedTime;                      // self made dialog to change the file/folder times
using ICSharpCode.SharpZipLib.Zip;             // zip - crucial: ZipEntry.CleanName(..) ensures, win explorer can open & show the zip 
using MetadataExtractor;                       // meta data from EXIF https://www.nuget.org/packages/MetadataExtractor/
using Microsoft.CSharp;                        // on the fly creation of executable  
//using Microsoft.VisualBasic.FileIO;          // "Solution Explorer" "Add Reference" --> "Reference Manager" --> expand Assemblies --> select Framework --> Microsoft.VisualBasic
using Microsoft.VisualBasic.FileIO;            // delete to trash bin & alternative IO
using OpenHardwareMonitor.Hardware;            // cpu temperature
using Shell;                                   // shell context menu from ShellContextMenu.cs
using System;
using System.CodeDom.Compiler;                 // on the fly creation of executable
using System.Collections;                      // ListView sorting
using System.Collections.Generic;
using System.Collections.Specialized;          // StringCollection
using System.ComponentModel;
using System.Data;
using System.Diagnostics;                      // process & stopwatch
using System.Drawing;
using System.Globalization;                    // CultureInfo.InvariantCulture
using System.IO;                               // GetCurrentDirectory()
using System.Linq;
using System.Management;                       // ManagementObjectCollection --> needs reference to System.Management too
using System.Reflection;                       // on the fly creation of executable 
using System.Resources;                        // ResourceManager 
using System.Runtime.InteropServices;          // DLLImport
using System.Security.Cryptography;            // MD5 
using System.Security.Permissions;             // FileSystemWatcher needs permission settings 
using System.Security.Principal;               // re start app as admin
using System.Text;
using System.Text.RegularExpressions;          // Regex
using System.Threading;                        // process
using System.Threading.Tasks;                  // tasks
using System.Windows.Forms;

namespace cfw {

    // FileSystemWatcher needs permission settings
    [IODescriptionAttribute("FileSystemWatcherDesc")]
    [PermissionSetAttribute(SecurityAction.LinkDemand, Name = "FullTrust")]
    [PermissionSetAttribute(SecurityAction.InheritanceDemand, Name = "FullTrust")]

    public partial class MainForm : Form, IMessageFilter {
        readonly string DRVC = "Computer";                         // default path
        readonly Panel m_Panel;                                    // holds status on 2xlistview, 2x button, 2x labels  
        bool m_bShowPanels = true;                                 // cmd status or listview status  
        readonly CommandHistory m_CommandHistory = new CommandHistory();    // command history
        bool m_bStealFocus = true;                                 // normally the cmd line has the focus  
        readonly ListViewColumnSorter[][] m_lvwColumnSorter;
        readonly GrzTools.FastFileFind m_fff;                      // fast file find
        bool m_bListViewAutoSelect = false;                        // listview autoselect while trying to "move" an item further down / up 
        readonly ShfoWorker m_shfo;                                // wrapper for SHFileOperation
        bool m_bFileSystemChangeActionLeft = true;                 // file system update only once per timer tick 
        bool m_bFileSystemChangeActionRight = true;
        ListViewHitTestInfo m_hitinfoRename;                       // edit listview subitem
        readonly TextBox m_editbox = new TextBox();                // edit listview subitem 
        string m_original = "";                                    // edit listview subitem 
        bool m_bKeepFocused = false;                               // edit listview subitem TBD: "Computer" needs this flag to avoid m_editbox.LostFocus msg when opening the context menu, while "ListView" doesn't   
        DateTime m_dtLastLoad = DateTime.Now;                      // block resize events when listview columns are automatically adjusted
        bool m_bBlockListViewActivity = false;                     // block resize during full load a large folder (winsxs)      
        DateTime m_dtLostFocus = DateTime.Now;                     // prevent rename per mouseclick on F7 short after focus from edit control was lost
        readonly int m_PointerDownMessage = 0;                     // it's WM_LBUTTONDOWN 0x201 for mouse / 0x246 for touch/pen down
        readonly int m_PointerUpMessage = 0;                       // it's WM_LBUTTONUP 0x202 for mouse / 0x247 for touch/pen down
        readonly ToolTip m_toolTip;                                // a general tooltip
        bool m_bDoc;                                               // preview what files? 
        bool m_bImg;
        bool m_bZip;
        bool m_bPdf;
        bool m_bHtm;
        bool m_bAsIs;
        bool m_bCfwVideo, m_bWmpAudio, m_bWmpVideo;
        readonly List<AltF7Form> m_AltF7Dlg = new List<AltF7Form>();
        readonly SelectFolderOrFile m_sff = new SelectFolderOrFile();
        bool m_sffNeedsRefresh = false;                            // "select folder file" dialog needs a refresh, ie. after media change
        Process m_process = null;                                  // when cmd window wants to start another process, also used as Process for jpg-->mpg conversion
        string m_sLHSfilter = "*.*";                               // lists left and right could be filtered, here we store the filter rules for each side
        string m_sRHSfilter = "*.*";
        readonly GrzTools.DriveDetector[] m_driveDetector = new GrzTools.DriveDetector[2] { null, null }; // 2x drive detector for removable devices
        bool m_bSizing = false;                                    // prevents flicker in selected ListView items, when size of form is changing
        bool m_bLeftMouseHitItem = false;                          // listview autoselect when mouse leaves the listview: this is an indicator, that the starting mouse click hit a valid item
        string m_LastPreview = "";                                 // the file last pre viewed: prevents a tiny flicker whne changing the file selection  
        readonly string[] m_ExtensionIconIndexArray;               // files' associated icons extensions 
        readonly string dummytext = "UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU";
        //        Stopwatch m_sw;                                  // multi purpose stopeatch 
        bool m_shfoIsActive = false;                               // indicator for active SHFO 
        readonly Image m_imgAdm = null;                            // admin image for F1, F2, F11, F12 buttons 
        BackgroundWorker m_outputWorker = new BackgroundWorker();  // communication with cmd proces via bgw 
        BackgroundWorker m_errorWorker = new BackgroundWorker();
        private StreamWriter m_inputWriter;
        private TextReader m_outputReader;
        private TextReader m_errorReader;
        bool m_bCopyConMode = false;
        bool m_bDirChangeDetected = false;
        List<string> m_CmdFilesFoldersList = new List<string>();
        int m_iListViewLimit = 1500;
        SimpleList m_sl = null;                                     // a simple list of last visited folders 
        int m_iSplitterPosition = -1;                               // split container position: -1 indicates 50:50
        ShowShortcuts m_frm = null;                                 // a window showing shortcuts
        FlyingLabel m_fldlg = new FlyingLabel();                    // flying label showing the path of one of the two path buttons 
        static SimpleProgress m_sp = new SimpleProgress();          // common simple progress dialog
        DateTime m_dtDebounce = DateTime.Now;                       // Alt-key debouncer
        DateTime m_dtDebounceDel = DateTime.Now;                    // Del debouncer
        bool m_draggingFromLv = false;                              // drag & drop operations  
        bool m_bRunSize = false;                                    // folder view sizes are calculated in Tasks, this is the global kill switch for let's say winsxs  
        List<Task> m_lstTasksRunSize = new List<Task>();            // a list holds all tasks, which compute folder sizes - a timer checks their completion status and signals "resort" to the list
        bool m_bgRunWorkerCompleted = true;                         // the 1501 aka m_iListViewLimit limit is finally processed
        string m_searchItem = "";                                   // Shift-Enter shall search a matching listview item   
        bool m_bInitOngoing = true;
        cfwListView m_listViewL = new cfw.cfwListView();            // introduced with listview tabs to avoid too many program changes 
        cfwListView m_listViewR = new cfw.cfwListView();            // both hold copies of the currently activated tab listview (listViewL0..listViewL1, listViewR0..listViewR1)
        string m_startfile = "";                                    // if cfw is started from altf7dlg, it's nice to select the file if it was provided 
        Task m_cpuTemperature = null;                               // Task to measure cpu temperature
        bool m_bRunTemperatureCPU = false;
        string m_richTextBoxCommandOutputSearchText = "";
        bool m_richTextBoxCommandOutputSearchBackward = true;

        public class WPD {
            public WPD(PortableDevices.PortableDevice wpd, string deviceName, string currentFolderID) {
                this.wpd = wpd;
                this.deviceName = deviceName;
                this.currentFolderID = currentFolderID;
            }
            public PortableDevices.PortableDevice wpd;
            public string deviceName;
            public string currentFolderID;
        }
        List<WPD> m_WPD = new List<WPD>();

        // Show about in system menu
        [DllImport("user32.dll")]
        private static extern int GetSystemMenu(int hwnd, int bRevert);
        [DllImport("user32.dll")]
        private static extern int AppendMenu(int hMenu, int Flagsw, int IDNewItem, string lpNewItem);

        public MainForm() {
            // INI: prepare read from ini
            GrzTools.IniFile ini = new GrzTools.IniFile(System.Windows.Forms.Application.ExecutablePath + ".ini");

            // INI: localization is needed before InitializeComponent();
            Thread.CurrentThread.CurrentCulture = new CultureInfo(ini.IniReadValue("cfw", "local", "en"));
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            // INI: start this app as admin
            bool runAsAdmin = bool.Parse(ini.IniReadValue("cfw", "adminmode", "false"));
            if ( runAsAdmin ) {
                ProcessStartInfo startInfo = new ProcessStartInfo(Application.ExecutablePath);
                // restart as admin only in case, it's not already admin mode
                if ( !new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) ) {
                    startInfo.Verb = "runas";
                    try {
                        // try to restart the app as admin
                        System.Diagnostics.Process.Start(startInfo);
                        // close recently started MainForm
                        this.Close();
                        // w/o this return, at least parts of the remaining actions in MainForm are executed
                        return;
                    } catch ( Exception ) {
                        // exception thrown: if UAC-question was answered with NO == no admin mode, therefore app continues to start in user mode
                        //MessageBox.Show(ex.Message);
                    }
                }
            }

            // standard MainForm init
            this.InitializeComponent();

            // set checkmark for "start in admin mode"
            this.runCfWAlwaysAsAdministratorToolStripMenuItem.Checked = runAsAdmin;

            // 20160221: icons for different file extensions; needs prior exec of InitializeComponent(); 
            List<string> ExtensionIconIndex = new List<string>();
            Image[] il = GrzTools.FlatImage.Deserialize();                               // re use list of serialized images from a previous run
            Hashtable iconsInfo = GrzTools.RegisteredFileType.GetFileTypeAndIcon();      // current list from registry
            if ( il.Length != (iconsInfo.Count + this.imageListLv.Images.Count) ) {      // if both lists differ from each other (just count), we reload the images
                foreach ( DictionaryEntry pair in iconsInfo ) {
                    Icon icon = GrzTools.RegisteredFileType.ExtractIconFromFile(pair.Value.ToString(), true);
                    if ( icon != null ) {
                        this.imageListLv.Images.Add(icon);
                    } else {
                        this.imageListLv.Images.Add(this.imageListLv.Images[1]);
                    }
                    ExtensionIconIndex.Add(pair.Key.ToString());
                }
                GrzTools.FlatImage.Serialize(this.imageListLv);                           // serialize recent image list, incl. fixed icons to re-use at the next run of cfw
            } else {
                this.imageListLv.Images.Clear();
                this.imageListLv.Images.AddRange(il);                                     // re-use image list from a previous run 
                foreach ( DictionaryEntry pair in iconsInfo ) {
                    ExtensionIconIndex.Add(pair.Key.ToString());
                }
            }
            this.m_ExtensionIconIndexArray = ExtensionIconIndex.ToArray();

            // PdfiumViewer.dll belongs to the project: I couldn't find another way to start cfw without having PdfiumViewer.dll inplace
            string dll = Path.Combine(Application.StartupPath, "PdfiumViewer.dll");
            if ( !File.Exists(dll) ) {
                // class Localizer automatically picks the right string depending on the app's culture setting
                string str = Localizer.GetString("pdfiumnotfound");
                System.Windows.Forms.MessageBox.Show(str);
            }

            // set localization checkmark accordingly AFTER InitializeComponent();
            foreach ( ToolStripMenuItem ti in this.languageToolStripMenuItem.DropDownItems ) {
                string str = Thread.CurrentThread.CurrentCulture.ToString();
                if ( ti.Tag.ToString().Contains(str) ) {
                    ti.Checked = true;
                } else {
                    ti.Checked = false;
                }
            }

            // run as admin
            this.Tag = "Commander for Windows";
            if ( new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) ) {
                this.Tag = "Commander for Windows - admin mode";
                this.Text = this.Tag.ToString();
            }

            // AltF7-Dialog may return with "open new cfw", therefore we have start arguments - starts cfw in a certain folder
            string[] args = Environment.GetCommandLineArgs();
            string @startpath = "";
            if ( args.Length > 1 ) {
                for ( int i = 1; i < args.Length; i++ ) {
                    @startpath += @args[i] + " ";
                }
                @startpath = @startpath.Trim();
                if ( File.Exists(startpath) ) {
                    this.@m_startfile = Path.GetFileName(@startpath);
                    @startpath = @Path.GetDirectoryName(@startpath);
                }
                if ( !GrzTools.FileTools.PathExists(@startpath, 500, this.m_WPD) ) {
                    startpath = "";
                }
            }


            // add "about entry" to system menu
            this.SetupSystemMenu();

            // tooltip when hovering over shortened pathes: left & right button; labelPrompt; listview tabs 
            this.components = new System.ComponentModel.Container();
            this.m_toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.m_toolTip.OwnerDraw = true;
            this.m_toolTip.BackColor = System.Drawing.Color.Yellow;
            this.m_toolTip.Draw += new DrawToolTipEventHandler(this.toolTip_Draw);

            // to be able to catch mousewheel messages and forward them to whatever control is hovered, even without having focus 
            // - also needed: class declaration "public partial class MainForm: Form, IMessageFilter"
            // - also needed: event handler "public bool PreFilterMessage( ref Message m )"
            Application.AddMessageFilter(this);

            // init fast file finder
            this.m_fff = new GrzTools.FastFileFind(this);

            // Create an array of a ListView column sorters, for two lists each having 8 columns
            this.m_lvwColumnSorter = new ListViewColumnSorter[2][];
            for ( int i = 0; i < 2; i++ ) {
                this.m_lvwColumnSorter[i] = new ListViewColumnSorter[8];
                for ( int j = 0; j < 8; j++ ) {
                    this.m_lvwColumnSorter[i][j] = new ListViewColumnSorter();
                    this.m_lvwColumnSorter[i][j].Order = (j > 0) && (j < 3) ? SortOrder.Descending : SortOrder.Ascending; // 20161016: default descending sort order for time and size is the preferrable use case
                    this.m_lvwColumnSorter[i][j].SortColumn = j;
                }
            }

            // add the command output window on top of the left/top-button (ie. the same cell 0,0 in the tableLayoutPanel): doesn't work in designer, must be done at runtime
            this.tableLayoutPanelMain.Controls.Add(this.richTextBoxCommandOutput, 0, 0);
            this.tableLayoutPanelMain.SetColumnSpan(this.richTextBoxCommandOutput, 2);
            this.tableLayoutPanelMain.SetRowSpan(this.richTextBoxCommandOutput, 3);
            this.richTextBoxCommandOutput.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
            this.richTextBoxCommandOutput.Location = new System.Drawing.Point(3, 3);
            this.richTextBoxCommandOutput.Visible = false;

            // avoids flickering, when ListView is in virtual mode AND owner draw - RDP has supposedly issues? could not see this
            this.m_listViewL = this.cfwListViewL0;
            this.m_listViewR = this.cfwListViewR0;
            SetDoubleBuffered(this.cfwListViewL0);
            SetDoubleBuffered(this.cfwListViewR0);
            SetDoubleBuffered(this.cfwListViewL1);
            SetDoubleBuffered(this.cfwListViewR1);
            SetDoubleBuffered(this.cfwListViewL2);
            SetDoubleBuffered(this.cfwListViewR2);
            SetDoubleBuffered(this.cfwListViewL3);
            SetDoubleBuffered(this.cfwListViewR3);
            SetDoubleBuffered(this.cfwListViewL4);
            SetDoubleBuffered(this.cfwListViewR4);
            // http://stackoverflow.com/questions/76993/how-to-double-buffer-net-controls-on-a-form via extension class - works pretty well, even in RDP sessions -- pretty much the same as above
            // listViewLeft.MakeDoubleBuffered(true);
            // listViewRight.MakeDoubleBuffered(true);

            // init the panels' maintainer
            this.m_Panel = new Panel(this);
            this.m_Panel.InitPanel(ref this.buttonLeft, ref this.buttonRight, ref this.m_listViewL, ref this.m_listViewR, ref this.labelLeft, ref this.labelRight, ref this.buttonLhsPrev, ref this.buttonLhsNext, ref this.buttonRhsPrev, ref this.buttonRhsNext);

            // folder history
            this.folderHistoryToolStripMenuItem.Checked = bool.Parse(ini.IniReadValue("cfw", "FolderHistory", "true"));
            this.m_Panel.folders.MaintainFolderHistory = this.folderHistoryToolStripMenuItem.Checked;
            if ( this.folderHistoryToolStripMenuItem.Checked ) {
                string fldFile = System.Windows.Forms.Application.ExecutablePath + ".fld";
                if ( File.Exists(fldFile) ) {
                    try {
                        this.m_Panel.folders = Panel.ReadFromBinaryFile<Panel.FolderHistory>(fldFile);
                    } catch {; }
                }
            }

            // take care about WPD
            getWPD(ref this.m_WPD);

            // INI: preview
            this.m_bDoc = bool.Parse(ini.IniReadValue("cfw", "doc", "true"));
            this.m_bImg = bool.Parse(ini.IniReadValue("cfw", "img", "true"));
            this.m_bZip = bool.Parse(ini.IniReadValue("cfw", "zip", "true"));
            this.m_bPdf = bool.Parse(ini.IniReadValue("cfw", "pdf", "true"));
            this.m_bHtm = bool.Parse(ini.IniReadValue("cfw", "htm", "true"));
            this.m_bAsIs = bool.Parse(ini.IniReadValue("cfw", "asi", "true"));
            this.m_bWmpAudio = bool.Parse(ini.IniReadValue("cfw", "wmpAudio", "true"));
            this.m_bWmpVideo = bool.Parse(ini.IniReadValue("cfw", "wmpVideo", "false"));
            this.m_bCfwVideo = bool.Parse(ini.IniReadValue("cfw", "cfwVideo", "true"));
            // INI: cfw start position & window size
            string strX = ini.IniReadValue("cfw", "StartPosition X", "0");
            string strY = ini.IniReadValue("cfw", "StartPosition Y", "0");
            // form start location: https://stackoverflow.com/questions/1363374/showing-a-windows-form-on-a-secondary-monitor
            Screen[] screens = Screen.AllScreens;
            bool minusHorizontalAllowed = false;
            int minX = 30;
            int minY = 30;
            foreach ( Screen s in screens ) {
                if ( s.Bounds.Left < 0 ) {
                    minusHorizontalAllowed = true;
                    minX = s.Bounds.Left < minX ? s.Bounds.Left : minX;
                    minY = s.Bounds.Top < minY ? s.Bounds.Top : minY;
                }
            }
            if ( minusHorizontalAllowed ) {
                this.Location = new Point(Math.Max(int.Parse(strX), minX), Math.Max(int.Parse(strY), minY));
            } else {
                if ( int.Parse(strX) < 0 || int.Parse(strY) < 0 ) {
                    this.Location = new Point();
                } else {
                    this.Location = new Point(int.Parse(strX), int.Parse(strY));
                }
            }
            // form size
            strX = ini.IniReadValue("cfw", "StartPosition Width", "800");
            strY = ini.IniReadValue("cfw", "StartPosition Height", "600");
            this.m_dtLastLoad = DateTime.Now;
            this.Size = new Size(Math.Max(int.Parse(strX), 800), Math.Max(int.Parse(strY), 600));
            // INI: splitter position
            this.m_iSplitterPosition = int.Parse(ini.IniReadValue("cfw", "Splitter Position", "-1"));
            this.setSplitContainerBar(this.m_iSplitterPosition);
            // INI: Computer at folder select
            this.folderSelectStartsFromComputerToolStripMenuItem.Checked = bool.Parse(ini.IniReadValue("cfw", "ComputerAtFolderSelect", "true"));
            // INI: htm connect to folder
            this.connectHtmWithItsFilesFolderToolStripMenuItem.Checked = bool.Parse(ini.IniReadValue("cfw", "ConnectHtmWithItsFilesFolder", "false"));
            // INI: Computer view shows shared folder or not 
            this.computerShowsShareFoldersToolStripMenuItem.Checked = bool.Parse(ini.IniReadValue("cfw", "SharedFolders", "true"));
            // INI: Computer view shows folders with size info or not 
            this.computerShowsFolderSizesToolStripMenuItem.Checked = bool.Parse(ini.IniReadValue("cfw", "ComputerFoldersSizes", "false"));
            this.listsShowFolderSizesToolStripMenuItem.Checked = bool.Parse(ini.IniReadValue("cfw", "ListsFoldersSizes", "false"));
            this.m_bRunSize = this.listsShowFolderSizesToolStripMenuItem.Checked;
            // INI: automatic network scan 
            this.autoNetworkScanToolStripMenuItem.Checked = bool.Parse(ini.IniReadValue("cfw", "AutoNetworkScan", "false"));
            this.m_sff.AutoNetworkScan = this.autoNetworkScanToolStripMenuItem.Checked;
            // INI: highligt empty folders
            this.highlightEmptyFolderToolStripMenuItem.Checked = bool.Parse(ini.IniReadValue("cfw", "HighlightEmptyFolders", "true"));
            // INI: ListView Limit
            string test = ini.IniReadValue("cfw", "LimitListView", "1501");
            this.m_iListViewLimit = int.Parse(test);
            // INI: listview font size
            string lvf = ini.IniReadValue("cfw", "Font ListView", "8.25");
            lvf = lvf.Replace(',', '.');
            float lvfs = 0;
            switch ( lvf ) {
                case "8.25":
                    this.normalToolStripMenuItem.Checked = true;
                    lvfs = 8.25f;
                    break;
                case "9.00":
                    lvfs = 9.00f;
                    this.largeToolStripMenuItem.Checked = true;
                    break;
                case "9.75":
                    lvfs = 9.75f;
                    this.extraToolStripMenuItem.Checked = true;
                    break;
                default:
                    try {
                        lvfs = float.Parse(lvf, System.Globalization.CultureInfo.CreateSpecificCulture("en-us"));
                    } catch ( Exception ) {
                        lvfs = 8.25f;
                    }
                    break;
            }
            Font ft = new Font("Microsoft Sans Serif", lvfs, FontStyle.Regular);
            this.cfwListViewL0.Font = ft;
            this.cfwListViewR0.Font = ft;
            this.cfwListViewL1.Font = ft;
            this.cfwListViewR1.Font = ft;
            this.cfwListViewL2.Font = ft;
            this.cfwListViewR2.Font = ft;
            this.cfwListViewL3.Font = ft;
            this.cfwListViewR3.Font = ft;
            this.cfwListViewL4.Font = ft;
            this.cfwListViewR4.Font = ft;

            // init tabs right (no 'load listview' happens here)
            string tmp = "";
            for ( int i = 0; i < 5; i++ ) {
                tmp = ini.IniReadValue("cfw", "rTab" + i.ToString(), "");
                if ( !GrzTools.FileTools.PathExists(tmp, 100, this.m_WPD) ) {
                    tmp = "";
                }
                this.m_Panel.SetListPath(Side.right, i, tmp, true);
                this.setTabControlText(Side.right, i, tmp);
            }
            // the next line causes to call "tabControlLeft_SelectedIndexChanged( object sender, EventArgs e )" via event, which finally loads the listview connected to this tab
            this.tabControlRight.SelectedIndex = Math.Min(4, Math.Max(0, int.Parse(ini.IniReadValue("cfw", "rTabIndex", "0"))));
            // have all internal vars correctly assigned
            this.m_Panel.SetActiveListView(Side.right, this.m_listViewR, this.tabControlRight.SelectedIndex);

            // init tabs left
            this.tabControlLeft.SelectedIndex = Math.Min(4, Math.Max(0, int.Parse(ini.IniReadValue("cfw", "lTabIndex", "0"))));
            this.m_Panel.SetActiveListView(Side.left, this.m_listViewL, this.tabControlLeft.SelectedIndex);
            for ( int i = 0; i < 5; i++ ) {
                tmp = ini.IniReadValue("cfw", "lTab" + i.ToString(), "");
                // start cfw from altf7 dialog with path as startup parameter ALWAYS shows up in leftmost tab
                if ( i == 0 ) {
                    if ( @startpath.Length > 0 ) {
                        tmp = @startpath;
                        this.tabControlLeft.SelectedIndex = 0;
                        this.m_Panel.SetActiveListView(Side.left, this.m_listViewL, this.tabControlLeft.SelectedIndex);
                    }
                }
                if ( !GrzTools.FileTools.PathExists(tmp, 100, this.m_WPD) ) {
                    tmp = "";
                }
                this.m_Panel.SetListPath(Side.left, i, tmp, true);
                this.setTabControlText(Side.left, i, tmp);
            }

            // set active listview side EITHER from ini OR if we have a startpath to left
            Side activeSide = Side.left;
            if ( @startpath.Length == 0 ) {
                tmp = ini.IniReadValue("cfw", "ActiveSide", "left");
                if ( tmp == "right" ) {
                    activeSide = Side.right;
                }
            }
            this.m_Panel.SetActiveSide(activeSide);

            // lists are shown in tabs or not
            this.listsInTabsToolStripMenuItem.Checked = bool.Parse(ini.IniReadValue("cfw", "ListsInTabs", "true"));
            this.showListsInTabs(this.listsInTabsToolStripMenuItem.Checked);

            // set command line as ActiveControl means, it always gets the focus and its caret blinks permanently
            this.ActiveControl = this.textBoxCommandLine;

            // Set KeyPreview object to true to allow the form to process a keypress event before the control with focus processes it
            this.KeyPreview = true;

            // prepare for copy/move/delete: only needed for "Samba mapped network drives", normally the FSW works well
            this.m_shfo = new ShfoWorker();
            this.m_shfo.ShfoEvent += new ShfoWorker.EventHandler(this.ShfoEventHandler);

            // register event handlers for editbox (listview item edit box) key events and lost focus
            this.m_editbox.Parent = null;
            this.m_editbox.KeyDown += new KeyEventHandler(this.OnEditBoxKeyDown);
            this.m_editbox.LostFocus += new EventHandler(this.OnEditBoxLostFocus);
            this.m_editbox.Hide();

            // distinguish between mouse (normal PC) and pen (Tablet)
            this.m_PointerDownMessage = GrzTools.TouchTools.IsTouchDevice() ? 0x246 : 0x201;
            this.m_PointerUpMessage = GrzTools.TouchTools.IsTouchDevice() ? 0x247 : 0x202;

            // admin icon image
            this.m_imgAdm = cfw.Properties.Resources.restartAsAdministratorToolStripMenuItem_Image;
            this.m_imgAdm.Tag = "admin";
            // INI: pre set F keys
            Icon ico = null;
            Image img = null;
            this.buttonF1.Text = ini.IniReadValue("cfw", "F1txt", "F1");
            this.buttonF1.Tag = ini.IniReadValue("cfw", "F1prg", "");
            ico = GrzTools.RegisteredFileType.ExtractIconFromFile(this.@buttonF1.Tag.ToString(), false);
            img = ico != null ? ico.ToBitmap() : null;
            this.buttonF1.Image = ini.IniReadValue("cfw", "F1adm", "") == this.m_imgAdm.Tag.ToString() ? this.m_imgAdm : img;
            this.buttonF2.Text = ini.IniReadValue("cfw", "F2txt", "F2");
            this.buttonF2.Tag = ini.IniReadValue("cfw", "F2prg", "");
            ico = GrzTools.RegisteredFileType.ExtractIconFromFile(this.@buttonF2.Tag.ToString(), false);
            img = ico != null ? ico.ToBitmap() : null;
            this.buttonF2.Image = ini.IniReadValue("cfw", "F2adm", "") == this.m_imgAdm.Tag.ToString() ? this.m_imgAdm : img;
            this.buttonF11.Text = ini.IniReadValue("cfw", "F11txt", "F11");
            this.buttonF11.Tag = ini.IniReadValue("cfw", "F11prg", "");
            ico = GrzTools.RegisteredFileType.ExtractIconFromFile(this.@buttonF11.Tag.ToString(), false);
            img = ico != null ? ico.ToBitmap() : null;
            this.buttonF11.Image = ini.IniReadValue("cfw", "F11adm", "") == this.m_imgAdm.Tag.ToString() ? this.m_imgAdm : img;
            this.buttonF12.Text = ini.IniReadValue("cfw", "F12txt", "F12");
            this.buttonF12.Tag = ini.IniReadValue("cfw", "F12prg", "");
            ico = GrzTools.RegisteredFileType.ExtractIconFromFile(this.@buttonF12.Tag.ToString(), false);
            img = ico != null ? ico.ToBitmap() : null;
            this.buttonF12.Image = ini.IniReadValue("cfw", "F12adm", "") == this.m_imgAdm.Tag.ToString() ? this.m_imgAdm : img;

            // synchronized scrolling
            this.m_listViewL.Buddy = null;
            this.m_listViewR.Buddy = null;

            // subscribe to media change events
            MediaChangeEvent += new EventHandler<MediaChangeEventArgs>(this.MediaChangeEvent_Received);

            // subscribe to messages sent from "cmd.exe process"
            OnProcessOutput += new ProcessEventHandler(this.MainForm_OnProcessOutput);
            OnProcessError += new ProcessEventHandler(this.MainForm_OnProcessError);

            // unicode character arrow up available in "Calibri"
            this.buttonLhsUp.Text = "\u2191";
            this.buttonRhsUp.Text = "\u2191";

            // 20161016: drag&drop in listviews
            this.m_listViewL.DragDrop += new DragEventHandler(this.listViewLeft_DragDrop);
            this.m_listViewL.DragEnter += new DragEventHandler(this.listViewLeft_DragEnter);
            this.m_listViewR.DragDrop += new DragEventHandler(this.listViewRight_DragDrop);
            this.m_listViewR.DragEnter += new DragEventHandler(this.listViewRight_DragEnter);

            // checkmark + image in a menu item
            ((ToolStripDropDownMenu)this.optionToolStripMenuItem.DropDown).ShowCheckMargin = true;
            ((ToolStripDropDownMenu)this.optionToolStripMenuItem.DropDown).ShowImageMargin = true;

            // will be called, as soon as MainForm is shown
            Shown += this.MainForm_Shown;
        }
        private void MainForm_Shown(object sender, EventArgs e) {
            // fit columns
            this.listViewFitColumnsToolStripMenuItem_Click(null, null);
            // command line
            this.RenderCommandline(this.m_Panel.GetActivePath());
            //// very ugly fix: buttons above lists show shortened text, although it would fit unshortened; caused by LoadListView in constructor, at a time when button width is unknown
            // computation is now (2018/08/01) based on (MainForm.Width - 180 / 2), which is the button width 
            this.m_bInitOngoing = false;
            this.setSplitContainerBar(this.m_iSplitterPosition);
            // cpu temperature monitoring !!does not work, when called from MainForm, because it is executed in a separate thread and MainForm needs to completely exist prior 
            GrzTools.IniFile ini = new GrzTools.IniFile(System.Windows.Forms.Application.ExecutablePath + ".ini");
            this.cPUTemperatureMonitoringToolStripMenuItem.Checked = bool.Parse(ini.IniReadValue("cfw", "temperatureCPU", "false"));
            if ( this.cPUTemperatureMonitoringToolStripMenuItem.Checked ) {
                if ( new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) ) {
                    this.m_bRunTemperatureCPU = true;
                    this.m_cpuTemperature = new Task(() => this.cpuTemperature(this.buttonF12, ref this.m_bRunTemperatureCPU));
                    this.m_cpuTemperature.Start();
                }
            }
        }

        //// StackOverflow App wide double buffering: doesn't work well, makes mouse hover over ListView (white selection) lagging
        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams cp = base.CreateParams;
        //        cp.ExStyle |= 0x02000000;
        //        return cp;
        //    }
        //}

        void setTabControlText(Side side, int index, string text) {
            text = text.EndsWith("\\") ? text.Substring(0, text.Length - 1) : text;
            string txt = text.Length > 3 ? Path.GetFileName(text) : text;
            if ( txt.Length > 54 ) {
                txt = "..  " + txt.Substring(txt.Length - 50);
            }
            TabControl tctl = side == Side.left ? this.tabControlLeft : this.tabControlRight;
            tctl.TabPages[index].Text = txt;
            tctl.TabPages[index].Tag = text;
        }

        // 20161016: couple of methods to implement drag&drop file(s) from & to & inside listviews
        bool m_bListViewAutoScroll = false;
        void lvAutoScroll(ListView lv, DIRECTION dir) {
            if ( dir == DIRECTION.NA ) {
                return;
            }
            int n = lv.TopItem.Index;
            int visibleItemsCount = 0;
            if ( dir == DIRECTION.DOWN ) {
                for ( int i = n + 1; i < lv.Items.Count; i++ ) {
                    Rectangle itemBounds = lv.Items[i].Bounds;
                    itemBounds.Width = 50;
                    if ( lv.ClientRectangle.Contains(itemBounds) ) {
                        visibleItemsCount = i;
                    } else {
                        break;
                    }
                }
                n = visibleItemsCount;
            }
            this.m_bListViewAutoScroll = true;
            do {
                if ( dir == DIRECTION.DOWN ) {
                    n++;
                } else {
                    n--;
                }
                if ( n < 0 ) {
                    break;
                }
                if ( n == lv.Items.Count ) {
                    break;
                }
                if ( MouseButtons != MouseButtons.Left ) {
                    break;
                }
                ListViewItem lvn = lv.Items[n];
                lv.EnsureVisible(lvn.Index);
                Application.DoEvents();
            } while ( this.m_bListViewAutoScroll );
        }
        enum OBJID : uint {
            HSCROLL = 0xFFFFFFFA,
            VSCROLL = 0xFFFFFFFB
        }
        public struct RECT {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct SCROLLBARINFO {
            public int cbSize;
            public RECT rcScrollBar;
            public int dxyLineButton;
            public int xyThumbTop;
            public int xyThumbBottom;
            public int reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public int[] rgstate;
        }
        [DllImport("user32.dll", SetLastError = true, EntryPoint = "GetScrollBarInfo")]
        private static extern int GetScrollBarInfo(IntPtr hWnd, OBJID idObject, ref SCROLLBARINFO sbi);
        enum DIRECTION : uint {
            UP = 0,
            DOWN = 1,
            NA = 2
        }
        private bool IsScrollBarVisible(IntPtr handle, OBJID scrollBar) {
            SCROLLBARINFO sbi = new SCROLLBARINFO();
            sbi.cbSize = Marshal.SizeOf(sbi);
            int result = GetScrollBarInfo(handle, scrollBar, ref sbi);
            if ( result == 0 )
                throw new Win32Exception();
            return sbi.rgstate[0] == 0;
        }
        void listViewLeftRight_DragLeave(object sender, EventArgs e) {
            Point cursor = Cursor.Position;
            ListView lv = (ListView)sender;
            int ofs = this.IsScrollBarVisible(lv.Handle, OBJID.HSCROLL) ? 17 : 0;
            Point lvPos = lv.PointToScreen(lv.Location);
            Rectangle hi = new Rectangle(lvPos.X, lvPos.Y - 40, lv.ClientSize.Width, 40);
            Rectangle lo = new Rectangle(lvPos.X, lvPos.Y + lv.ClientSize.Height - ofs, lv.ClientSize.Width, 40);
            DIRECTION dir = DIRECTION.NA;
            if ( hi.Contains(cursor) ) {
                dir = DIRECTION.UP;
            }
            if ( lo.Contains(cursor) ) {
                dir = DIRECTION.DOWN;
            }
            this.lvAutoScroll(lv, dir);
        }
        private void listViewLeftRight_DragOver(object sender, DragEventArgs e) {
            // get string array of files from drag'drop operation data
            string[] files = (string[])((DataObject)e.Data).GetData(DataFormats.FileDrop);
            if ( (files == null) || (files.Length == 0) ) {
                return;
            }

            // the following would apply to Computer-View
            ListView lv = (ListView)sender;
            Side side = this.m_Panel.GetSideFromView(lv);
            if ( this.m_Panel.button(side).Text[1] != ':' ) {
                return;
            }

            // base source path
            string sourcePath = File.Exists(files[0]) ? Path.GetDirectoryName(files[0]) : files[0];

            // the drop target is ALWAYS a path
            string basePath = this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString();
            string dropTargetPath = basePath;
            if ( basePath.Length > 3 ) {
                dropTargetPath = basePath.TrimEnd('\\');
            }

            // check if perhaps an unselected folder is the drop target
            Point pt = lv.PointToClient(new Point(e.X, e.Y));
            ListViewItem item = this.listviewGetItemAt(lv, pt);
            if ( item != null ) {
                // drop target is a normal folder
                if ( (item.ImageIndex == 0) || (item.ImageIndex == 3) ) {
                    dropTargetPath = Path.Combine(basePath, item.Text);
                } else {
                    // drop target is the [..] folder 'one level up'
                    if ( item.ImageIndex == 2 ) {
                        dropTargetPath = Path.GetDirectoryName(basePath);
                        if ( dropTargetPath == null ) {
                            // if upper most level is already reached, we give up
                            e.Effect = DragDropEffects.None;
                            return;
                        }
                    }
                }
                //// we give up: if drop location is selected AND drop location is a folder AND is the same as the drop source
                //if ( (sourcePath == dropTargetPath) && (item.Selected && ((item.ImageIndex == 0) || (item.ImageIndex == 3) || (item.ImageIndex == 2))) ) {
                //    e.Effect = DragDropEffects.None;
                //    return;
                //}
            }

            // normal effect is copy
            e.Effect = DragDropEffects.Copy;
        }
        void listViewLeft_DragEnter(object sender, DragEventArgs e) {
            this.m_bListViewAutoScroll = false;
            // WPD
            if ( this.m_Panel.listType[(int)this.m_Panel.GetActiveSide()] != ListType.FileSystem ) {
                return;
            }
            string path = this.m_Panel.button(Side.left).Text;
            if ( (path == "Computer") || (path == "Shared Folders") || (path == "Network") ) {
                return;
            }
            e.Effect = DragDropEffects.Copy;
            this.m_Panel.SetActiveSide(Side.left);
        }
        void listViewRight_DragEnter(object sender, DragEventArgs e) {
            this.m_bListViewAutoScroll = false;
            // WPD
            if ( this.m_Panel.listType[(int)this.m_Panel.GetActiveSide()] != ListType.FileSystem ) {
                MessageBox.Show("Not yet implemented for WPD.", "Sorry");
                return;
            }
            string path = this.m_Panel.button(Side.right).Text;
            if ( (path == "Computer") || (path == "Shared Folders") || (path == "Network") ) {
                return;
            }
            this.m_Panel.SetActiveSide(Side.right);
            e.Effect = DragDropEffects.Copy;
        }
        void listViewLeft_DragDrop(object sender, DragEventArgs e) {
            this.pasteFromDragDrop(sender, e);
        }
        void listViewRight_DragDrop(object sender, DragEventArgs e) {
            this.pasteFromDragDrop(sender, e);
        }
        Rectangle m_lvDragMouseDownPos;
        private void timerDragDropDebouncer_Tick(object sender, EventArgs e) {
            this.timerDragDropDebouncer.Stop();
            this.m_draggingFromLv = false;
        }
        void pasteFromDragDrop(object sender, DragEventArgs e) {
            // dragNdrop is over
            this.timerDragDropDebouncer.Start();

            // same point mouse down and up is no dragging
            Point cmp = MousePosition;
            if ( this.m_lvDragMouseDownPos.Contains(cmp) ) {
                return;
            }

            // get string array of files from drag'drop operation data
            string[] files = (string[])((DataObject)e.Data).GetData(DataFormats.FileDrop);
            if ( (files == null) || (files.Length == 0) ) {
                return;
            }

            // base source path
            string sourcePath = File.Exists(files[0]) ? Path.GetDirectoryName(files[0]) : files[0];

            // the drop target is ALWAYS a path
            string basePath = this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString();
            string dropTargetPath = basePath;
            if ( basePath.Length > 3 ) {
                dropTargetPath = basePath.TrimEnd('\\');
            }

            // check if perhaps an unselected folder (fs monitoring disabled OR stupid usb-drive) is the drop target
            bool bCopyToUnmonitoredSubfolder = false;
            ListView lv = (ListView)sender;
            Point pt = lv.PointToClient(new Point(e.X, e.Y));
            ListViewItem item = this.listviewGetItemAt(lv, pt);
            if ( item != null ) {
                // drop target is a normal folder
                if ( (item.ImageIndex == 0) || (item.ImageIndex == 3) ) {
                    dropTargetPath = Path.Combine(basePath, item.Text);
                    bCopyToUnmonitoredSubfolder = true;
                } else {
                    // drop target is the [..] folder 'one level up'
                    if ( item.ImageIndex == 2 ) {
                        dropTargetPath = Path.GetDirectoryName(basePath);
                        if ( dropTargetPath == null ) {
                            // if upper most level is already reached, we give up
                            return;
                        }
                    }
                }
                //// we give up: if drop location is the same as the drop source
                //if ( sourcePath == dropTargetPath ) {
                //    return;
                //}
                //// we give up: if drop location is selected AND drop location is a folder 
                //if ( (item.Selected && itemIsFolder) && true ) {
                //    MessageBox.Show("Drop path is either equal or superior to source.", "Error");
                //    return;
                //}
            }

            // make input data for a normal file/folder copy
            List<string> dst = new List<string>();
            List<string> src = new List<string>();
            foreach ( string srcFile in files ) {
                // skip folder dragging into itself
                if ( dropTargetPath != srcFile ) {
                    string dstFile = Path.Combine(dropTargetPath, Path.GetFileName(srcFile));
                    // skip non sense
                    if ( srcFile != dstFile ) {
                        src.Add(srcFile);
                        dst.Add(dstFile);
                    }
                }
            }

            // exec async copy operation
            GrzTools.ShellFileOperation.FileOperations fos = GrzTools.ShellFileOperation.FileOperations.FO_COPY;
            this.m_shfoIsActive = true;
            this.m_shfo.SetArguments(new ShfoWorker.Arguments(this.Handle, src, dst, fos, "Paste from Drag-Drop operation", true, bCopyToUnmonitoredSubfolder));
            Thread thread = new Thread(this.m_shfo.DoWorkShfo);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
        void lvDragMouseDown(object sender, MouseEventArgs e) {
            // a drag & drop operation is flagged, when left mouse goes down in the regions of column index > 0 AND item is selected
            ListView lv = (ListView)sender;
            Side side = Side.left;
            if ( lv == this.m_listViewR ) {
                side = Side.right;
            }
            // indicate ability to do something with the selection, which is allowed only in listview other than Computer
            if ( this.m_Panel.button(side).Text[1] != ':' ) {
                this.m_draggingFromLv = false;
                return;
            }
            if ( (e.Button == MouseButtons.Left) && (e.X > lv.Columns[0].Width) ) {
                ListViewItem item = this.listviewGetItemAt(lv, e.Location);
                if ( (item != null) && (item.Selected) ) {
                    this.m_draggingFromLv = true;
                    this.m_lvDragMouseDownPos = new Rectangle(MousePosition.X - 5, MousePosition.Y - 5, 10, 10);
                    this.m_mms = new mouseMoveSelect();
                }
            }
        }
        void lvDragMouseUp(object sender, MouseEventArgs e) {
            // unconditionally stop dragNdrop
            this.m_draggingFromLv = false;
        }
        void lvDragMouseMove(object sender, MouseEventArgs e) {
            if ( this.m_Panel == null ) {
                return;
            }

            // get current side
            ListView lv = (ListView)sender;
            Side side = Side.left;
            if ( lv == this.m_listViewR ) {
                side = Side.right;
            }
            // indicate ability to do something with the selection, which is allowed only in listview other than Computer
            if ( (this.m_Panel.button(side).Text.Length > 0) && (this.m_Panel.button(side).Text[1] != ':') ) {
                this.m_draggingFromLv = false;
                return;
            }
            // change mouse icon depending on the hovered region
            if ( e.Button == MouseButtons.None ) {
                ListViewItem item = this.listviewGetItemAt(lv, e.Location);
                if ( e.X > lv.Columns[0].Width ) {
                    if ( (item != null) && item.Selected && (item.ImageIndex != 2) ) {
                        lv.Cursor = Cursors.Hand;
                    } else {
                        lv.Cursor = Cursors.Default;
                    }
                } else {
                    lv.Cursor = Cursors.Default;
                }
            }
            // nothing to do here in case of not dragging anything
            if ( !this.m_draggingFromLv ) {
                return;
            }
            // a drag&drop operation starts, when the mouse moves in the regions of column index > 0
            if ( e.X > lv.Columns[0].Width ) {
                this.m_bSelectRule = !this.m_bSelectRule;
                this.StartDragging();
            }
            return;
        }
        private void StartDragging() {
            // fill a list of selected files & folders
            string wpdFolder = "";
            List<string> source = new List<String>();
            List<string> destin = new List<String>();
            ListType lt = this.GetSelectedListViewStrings(out source, out destin, null, out wpdFolder);
            if ( (source.Count == 0) || (lt != ListType.FileSystem) ) {
                return;
            }
            // make drag usable data & start drag operation
            DataObject data = new DataObject(DataFormats.FileDrop, source.ToArray());
            this.DoDragDrop(data, DragDropEffects.Copy);
        }

        // ToolTip background color change
        void toolTip_Draw(object sender, DrawToolTipEventArgs e) {
            e.DrawBackground();
            e.DrawBorder();
            e.DrawText();
        }

        // make listview font size adjustable
        private void fontSizeToolStripMenuItem_Click(object sender, EventArgs e) {
            ToolStripMenuItem tsmi = (ToolStripMenuItem)sender;
            switch ( tsmi.Name ) {
                case "normalToolStripMenuItem":
                    this.normalToolStripMenuItem.Checked = true;
                    this.largeToolStripMenuItem.Checked = false;
                    this.extraToolStripMenuItem.Checked = false;
                    this.m_listViewL.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular);
                    this.m_listViewR.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular);
                    break;
                case "largeToolStripMenuItem":
                    this.normalToolStripMenuItem.Checked = false;
                    this.largeToolStripMenuItem.Checked = true;
                    this.extraToolStripMenuItem.Checked = false;
                    this.m_listViewL.Font = new Font("Microsoft Sans Serif", 9.0f, FontStyle.Regular);
                    this.m_listViewR.Font = new Font("Microsoft Sans Serif", 9.0f, FontStyle.Regular);
                    break;
                case "extraToolStripMenuItem":
                    this.normalToolStripMenuItem.Checked = false;
                    this.largeToolStripMenuItem.Checked = false;
                    this.extraToolStripMenuItem.Checked = true;
                    this.m_listViewL.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular);
                    this.m_listViewR.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular);
                    break;
            }
            this.listViewFitColumnsToolStripMenuItem_Click(null, null);
        }
        // eventhandler fit/no fit listview
        private void listViewFitColumns(Side side) {
            ListView lv = null;
            if ( side == Side.left ) {
                lv = this.m_listViewL;
            }
            if ( side == Side.right ) {
                lv = this.m_listViewR;
            }
            lv.BeginUpdate();
            this.m_dtLastLoad = DateTime.Now;
            if ( this.m_Panel.button(side).Text != "Computer" ) {
                lv.Columns[1].Width = -2;
                lv.Columns[2].Width = TextRenderer.MeasureText(this.dummytext.Substring(0, this.m_Panel.maxLen2[(int)side, this.m_Panel.GetActiveTabIndex(side)]), lv.Font).Width;
                lv.Columns[3].Width = TextRenderer.MeasureText(this.dummytext.Substring(0, this.m_Panel.maxLen3[(int)side, this.m_Panel.GetActiveTabIndex(side)]), lv.Font).Width;
                lv.Columns[0].Width = Math.Max(150, lv.ClientSize.Width - lv.Columns[1].Width - lv.Columns[2].Width);
                lv.Columns[7].Width = 0;
            } else {
                lv.Columns[0].Width = -2;
                lv.Columns[1].Width = -2;
                lv.Columns[2].Width = -2;
                lv.Columns[3].Width = 90;
                lv.Columns[4].Width = -2;
                lv.Columns[5].Width = -2;
                lv.Columns[6].Width = -2;
                lv.Columns[7].Width = 0;
            }
            lv.EndUpdate();
        }
        private void listViewFitColumnsToolStripMenuItem_Click(object sender, EventArgs e) {
            this.m_dtLastLoad = DateTime.Now;
            if ( this.listViewFitColumnsToolStripMenuItem.Checked ) {
                this.listViewFitColumns(Side.left);
                this.listViewFitColumns(Side.right);
            } else {
                if ( this.m_Panel.GetActiveSide() == Side.left ) {
                    this.LoadListView(Side.left, this.m_Panel.button(Side.left).Tag.ToString(), "");
                    this.LoadListView(Side.right, this.m_Panel.button(Side.right).Tag.ToString(), "");
                }
                if ( this.m_Panel.GetActiveSide() == Side.right ) {
                    this.LoadListView(Side.right, this.m_Panel.button(Side.right).Tag.ToString(), "");
                    this.LoadListView(Side.left, this.m_Panel.button(Side.left).Tag.ToString(), "");
                }
            }
        }

        // all key events are monitored by MainForm
        private void MainForm_KeyUp(object sender, KeyEventArgs e) {
            // shift shall alter F7 text
            if ( ((ModifierKeys & Keys.None) == Keys.None) && (e.KeyCode == Keys.ShiftKey) ) {
                this.buttonF7.Text = "F7 - MkDir";
                this.buttonF4.Text = "F4 - Edit";
            }
        }
        private void MainForm_KeyDown(object sender, KeyEventArgs e) {
            // shift shall alter F7 text
            if ( ((ModifierKeys & Keys.Shift) == Keys.Shift) && (e.KeyCode == Keys.ShiftKey) && !this.richTextBoxCommandOutput.Visible ) {
                this.buttonF7.Text = "F7 - MkFile";
                this.buttonF4.Text = "F4 - New && Edit";
            }

            // Ctrl-F search in command window
            if ( (e.Modifiers == Keys.Control) && (e.KeyCode == Keys.F) && this.richTextBoxCommandOutput.Visible ) {
                this.searchToolStripMenuItem_Click(null, null);
            }
            //if ( (e.Modifiers == Keys.None) && (e.KeyCode == Keys.F3) && this.richTextBoxCommandOutput.Visible ) {
            //    richTextBoxCommandOutputSearchForward();
            //}
            if ( (e.Modifiers == Keys.Shift) && (e.KeyCode == Keys.F3) && this.richTextBoxCommandOutput.Visible ) {
                this.richTextBoxCommandOutputSearchBackward();
            }

            // Ctrl-R refresh/reload listviews via shortcut - doesn't work well via menu (left/right), because the shortcuts there don't distinguish between left&right
            if ( (e.Modifiers == Keys.Control) && (e.KeyCode == Keys.R) ) {
                if ( this.m_Panel.GetActiveSide() == Side.left ) {
                    this.refreshLeftToolStripMenuItem_Click(null, null);
                }
                if ( this.m_Panel.GetActiveSide() == Side.right ) {
                    this.refreshRightToolStripMenuItem_Click(null, null);
                }
            }
            // F1 F2 F11 F12
            if ( (e.Modifiers == Keys.None) && (e.KeyCode == Keys.F1) ) {
                this.buttonFx_Click(this.buttonF1, null);
            }
            if ( (e.Modifiers == Keys.None) && (e.KeyCode == Keys.F2) ) {
                this.buttonFx_Click(this.buttonF2, null);
            }
            if ( (e.Modifiers == Keys.None) && (e.KeyCode == Keys.F11) ) {
                this.buttonFx_Click(this.buttonF11, null);
            }
            if ( (e.Modifiers == Keys.None) && (e.KeyCode == Keys.F12) ) {
                this.buttonFx_Click(this.buttonF12, null);
            }
            // F3 - view current file
            if ( (e.Modifiers == Keys.None) && (e.KeyCode == Keys.F3) ) {
                this.buttonF3_Click(null, null);
            }
            // F4 - edit current file
            if ( (e.Modifiers == Keys.None) && (e.KeyCode == Keys.F4) ) {
                this.buttonF4_Click(null, null);
            }
            // Shift+F4 - create and edit an empty file
            if ( (e.Modifiers == Keys.Shift) && (e.KeyCode == Keys.F4) ) {
                string registeredApp = GrzTools.FileAssociation.Get(".txt");
                if ( (registeredApp != null) && (registeredApp != "") ) {
                    System.Diagnostics.Process p = new System.Diagnostics.Process();
                    p.StartInfo.FileName = registeredApp;
                    p.StartInfo.WorkingDirectory = this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString();
                    p.Start();
                }
            }
            // F5 - copy 
            if ( (e.Modifiers == Keys.None) && (e.KeyCode == Keys.F5) ) {
                this.buttonF5_Click(null, null);
            }
            // F6 - move
            if ( (e.Modifiers == Keys.None) && (e.KeyCode == Keys.F6) ) {
                this.buttonF6_Click(null, null);
            }
            // F7 - make directory
            if ( ((e.Modifiers == Keys.None) || (e.Modifiers == Keys.Shift)) && (e.KeyCode == Keys.F7) ) {
                this.buttonF7_Click(null, null);
            }
            // Alt-F7 --> find in files !! not working from here !!
            //            if ( (e.Modifiers == Keys.Alt) && (e.KeyCode == Keys.F7) ) {
            //                findFileToolStripMenuItem_Click(null, null);
            //            }

            if ( (this.textBoxCommandLine.Text.Length != 0) ) {

            }
            // F8 - delete | Del delete is tricky: Del shall primarily delete form command line; only if command line is empty, Del shall be forwarded to FileDelete; the debouncer (set in command line text change) prevents firing FileDelete right after deleting the command line
            if ( ((e.Modifiers == Keys.None) && (e.KeyCode == Keys.F8)) || ((e.KeyCode == Keys.Delete) && (this.textBoxCommandLine.Text.Length == 0) && ((DateTime.Now - this.m_dtDebounceDel).Milliseconds > 50)) ) {
                // 20160717: in case a rename process is active, DEL shall ONLY alter the editbox content
                if ( !this.m_editbox.Visible ) {
                    this.buttonF8_Click(null, null);
                    e.Handled = true;
                } else {
                    e.Handled = false;
                }

            }

            // F9 - rename file/folder
            //            if ( (e.Modifiers == Keys.None) && (e.KeyCode == Keys.F9) ) {
            //                this.buttonF9_MouseDown(null, null);
            //            }
            // Ctrl-O --> toggle main window (2x lists etc) with show&hide command window
            //            if ( (e.Modifiers == Keys.Control) && (e.KeyCode == Keys.O) ) {
            //                m_bShowPanels = ShowPanels(!m_bShowPanels);
            //            }
            // F10 - exit CFW
            if ( (e.Modifiers == Keys.None) && (e.KeyCode == Keys.F10) ) {
                this.buttonF10_Click(null, null);
            }
        }
        // Menu Command and Ctrl-O --> toggle main window (2x lists etc) with show&hide command window
        private void commandWindowToolStripMenuItem_Click(object sender, EventArgs e) {
            this.m_bShowPanels = this.ShowPanels(!this.m_bShowPanels);
        }

        // menu command "find files" 
        [ThreadStatic]
        AltF7Form _AltF7Dlg;
        private void findFileToolStripMenuItem_Click(object sender, EventArgs e) {
            // WPD
            if ( this.m_Panel.listType[(int)this.m_Panel.GetActiveSide()] != ListType.FileSystem ) {
                MessageBox.Show("Not yet implemented for WPD.", "Sorry");
                return;
            }
            int ndx = 0;
            if ( (this.m_AltF7Dlg.Count == 1) && !this.m_AltF7Dlg[0].Visible ) {
                // m_AltF7Dlg exists and we re use it --> this way previous search results of the laste closed AltF7 are shown
                this.m_AltF7Dlg[0].InitForm(this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), this.m_Panel.button(this.m_Panel.GetPassiveSide()).Tag.ToString());
                ndx = 0;
            } else {
                // create a new AltF7Form
                this._AltF7Dlg = new AltF7Form(this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), this.m_Panel.button(this.m_Panel.GetPassiveSide()).Tag.ToString());
                this._AltF7Dlg.LoadFolderRequest += new EventHandler<AltF7Form.LoadFolderEventArgs>(this.AltF7Form_LoadFolderRequest);
                this._AltF7Dlg.FormClosing += new FormClosingEventHandler(this.AltF7Form_FormClosing);
                this.m_AltF7Dlg.Add(this._AltF7Dlg);
                ndx = this.m_AltF7Dlg.Count - 1;
            }

            // 20160320: removed parentship "this" in Show(), now cfw and AltF7 can really coexist side by side (with this: if cfw minimizes all child dlgs minimize too)
            this.m_AltF7Dlg[ndx].Show();
        }
        private void AltF7Form_FormClosing(object sender, FormClosingEventArgs e) {
            try {
                AltF7Form dlg = (AltF7Form)sender;
                if ( this.m_AltF7Dlg.Count == 1 ) {
                    e.Cancel = true;
                    dlg.Hide();
                    return;
                }
                for ( int i = 0; i < this.m_AltF7Dlg.Count; i++ ) {
                    if ( dlg == this.m_AltF7Dlg[i] ) {
                        dlg.LoadFolderRequest -= new EventHandler<AltF7Form.LoadFolderEventArgs>(this.AltF7Form_LoadFolderRequest);
                        this.m_AltF7Dlg.RemoveAt(i);
                        dlg.Dispose();
                    }
                }
            } catch {; }
        }
        void AltF7Form_LoadFolderRequest(object sender, AltF7Form.LoadFolderEventArgs ea) {
            Side side = this.m_Panel.GetActiveSide();
            string file = Path.GetFileName(ea.Folder);
            string path = Path.GetDirectoryName(ea.Folder);
            this.LoadListView(side, path, file);
            this.m_Panel.folders.InsertTopFolder(side, this.m_Panel.GetActiveTabIndex(side), path);
            AltF7Form dlg = (AltF7Form)sender;
            if ( dlg != null ) {
                dlg.LoadFolderRequest -= new EventHandler<AltF7Form.LoadFolderEventArgs>(this.AltF7Form_LoadFolderRequest);
            }
        }

        // switch between the two listviews
        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData) {
            // intercept TAB key
            if ( keyData == Keys.Tab ) {
                // 20161016: prevent changing lv when preview is shown
                if ( this.previewCtl.Visible ) {
                    return true;
                }
                if ( this.m_bShowPanels ) {
                    // only when ListViews are shown --> toggle active listview
                    this.m_Panel.SetActiveSide(this.m_Panel.GetActiveSide() == Side.left ? Side.right : Side.left);
                    this.RenderCommandline(this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString());
                    return true;
                } else {
                    // in cmd.exe window
                    this.textBoxCommandLine_KeyDown(null, new KeyEventArgs(keyData));
                    return true;
                }
            }

            // 20161016: listview selection via keyboard (0x100 == WM_KEYDOWN) [previous implementation in textbox Command line didn't work, when Preview was visible]
            if ( msg.Msg == 0x100 ) {

                // F3 in command window
                if ( (keyData == Keys.F3) && this.richTextBoxCommandOutput.Visible ) {
                    this.richTextBoxCommandOutputSearchForward();
                }

                // select listview via keyboard: +(all/wildcard), -(none), *(invert), Insert(toggle) 
                if ( keyData == Keys.Add ) {
                    this.selectAllToolStripMenuItem_Click(null, null);
                    return true;
                }
                if ( keyData == Keys.Subtract ) {
                    this.deselectAllToolStripMenuItem_Click(null, null);
                    return true;
                }
                if ( keyData == Keys.Multiply ) {
                    this.invertSelectionToolStripMenuItem_Click(null, null);
                    return true;
                }
                if ( keyData == Keys.Insert ) {
                    this.SelectListViewItems(2);
                    return true;
                }
                //if ( keyData == Keys.Down ) {
                //}
                //if ( keyData == Keys.Up ) {
                //    if ( (msg.HWnd == listViewLeft.Handle) || (msg.HWnd == listViewRight.Handle) ) {
                //        if ( listViewLeft.FocusedItem.Index > 0 ) {
                //            listViewLeft.FocusedItem = listViewLeft.Items[listViewLeft.FocusedItem.Index - 1];
                //        }
                //        return true;
                //    }
                //}
            }

            // regular behaviour
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // update preview
        private void listViewLeftRight_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e) {
            if ( this.previewCtl.Visible ) {
                if ( e.IsSelected ) {
                    this.PreviewFile();
                }
            }
        }

        // render label according to listview selection
        private void listViewLeftRight_SelectedIndexChanged(object sender, EventArgs e) {
            // show selection in ListView label
            ListView lv = (ListView)sender;
            Side side = this.m_Panel.GetSideFromView(lv);
            this.m_Panel.RenderListviewLabel(side);

            // 20161016: drag >1 items from right to left, delete all on left, leave mouse left and move to right into non item region ==> left [..] is not selected anymore   <== FIX
            //           negative side effect --> copy over 1 item to empty list causes [..] being selected, fixed in LoadListView(..) 
            if ( lv.SelectedIndices.Count == 0 ) {
                if ( lv.Items.Count == 1 ) {
                    lv.Items[0].Selected = true;
                }
            }

            // detect special text
            if ( this.m_Panel.button(side).Text[1] != ':' ) {
                // special case, when we are at "Computer" level
                if ( this.m_Panel.button(side).Text == "Computer" ) {
                    this.RenderCommandline("Computer");
                }
                // special case, when we are at "Shared Folders" level
                if ( this.m_Panel.button(side).Text == "Shared Folders" ) {
                    this.RenderCommandline("Shared Folders");
                }
            }
        }

        // preview of certain files instead of listview of files/folders
        void PreviewFile() {
            if ( this.previewCtl.Visible ) {
                // get list item
                Side side = Side.none;
                if ( !this.m_listViewR.Visible ) {
                    side = Side.left;
                }
                if ( !this.m_listViewL.Visible ) {
                    side = Side.right;
                }
                string itemTxt = "";
                if ( this.m_Panel.listview(side).SelectedIndices.Count > 0 ) {
                    ListViewItem[] arr = this.m_Panel.GetListViewArr(side);
                    itemTxt = arr[this.m_Panel.listview(side).SelectedIndices[0]].Text;
                } else {
                    return;
                }

                // get filename to preview
                string filename = Path.Combine(this.m_Panel.button(side).Tag.ToString(), itemTxt);

                // file association check
                FileVersionInfo wmpInfo = FileVersionInfo.GetVersionInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Windows Media Player", "wmplayer.exe"));
                string ext = Path.GetExtension(filename);
                string typename = "";
                if ( ".avi.AVI.wmv.WMV.mp4.MP4.mov.MOV".Contains(ext) && (wmpInfo.FileName.Length > 0) ) {
                    typename = "WMP";
                }

                // start preview
                if ( !File.Exists(filename) ) {
                    this.m_LastPreview = filename;
                    this.previewCtl.LoadDocument("", "", this.m_Panel.listview(side));
                    return;
                }
                if ( !IsFileReady(filename) ) {
                    if ( this.m_LastPreview == filename ) {
                        this.previewCtl.Invalidate(true);
                        this.previewCtl.RePaint();
                        return;
                    }
                    this.m_LastPreview = filename;
                    this.previewCtl.LoadDocument("", "", this.m_Panel.listview(side));
                    return;
                }
                if ( this.m_LastPreview != filename ) {
                    this.m_LastPreview = filename;
                    this.previewCtl.LoadDocument(filename, typename, this.m_Panel.listview(side));
                }
            }
        }
        private void PreviewFileRightToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.richTextBoxCommandOutput.Visible || !this.m_listViewL.Visible ) {
                this.PreviewFileRightToolStripMenuItem.Checked = false;
                return;
            }
            if ( this.PreviewFileRightToolStripMenuItem.Checked ) {
                if ( this.m_listViewR.Visible ) {
                    this.m_dtLastLoad = DateTime.Now;
                    this.fileSystemWatcherRight.EnableRaisingEvents = false;
                    this.m_listViewR.Visible = false;
                    this.m_Panel.SetActiveSide(Side.left);
                    if ( this.listsInTabsToolStripMenuItem.Checked ) {
                        this.tableLayoutPanelRight.Controls.Remove(this.tabControlRight);
                    } else {
                        this.tableLayoutPanelRight.Controls.Remove(this.m_listViewR);
                    }
                    this.tableLayoutPanelRight.Controls.Add(this.previewCtl);
                    this.tableLayoutPanelRight.SetCellPosition(this.previewCtl, new TableLayoutPanelCellPosition(1, 1));
                    this.tableLayoutPanelRight.SetRowSpan(this.previewCtl, 1);
                    this.previewCtl.Dock = DockStyle.Fill;
                    this.previewCtl.Visible = true;
                    this.previewCtl.SetPreviewFiles(this.m_bImg, this.m_bDoc, this.m_bPdf, this.m_bHtm, this.m_bZip, this.m_bAsIs, this.m_bCfwVideo, this.m_bWmpAudio, this.m_bWmpVideo);
                    this.m_bStealFocus = false;
                    this.listRightToolStripMenuItem.Enabled = false;
                    this.detailsRightToolStripMenuItem.Enabled = false;
                    this.drivesToolStripMenuItem.Enabled = false;
                    this.refreshRightToolStripMenuItem.Enabled = false;
                    this.PreviewFile();
                }
            } else {
                this.m_LastPreview = "";
                if ( this.previewCtl.Visible ) {
                    this.m_bStealFocus = true;
                    this.textBoxCommandLine.Focus();
                    this.m_dtLastLoad = DateTime.Now;
                    this.previewCtl.Visible = false;
                    this.previewCtl.Clear();
                    this.tableLayoutPanelRight.Controls.Remove(this.previewCtl);
                    if ( this.listsInTabsToolStripMenuItem.Checked ) {
                        this.tableLayoutPanelRight.Controls.Add(this.tabControlRight, 1, 1);
                    } else {
                        this.tableLayoutPanelRight.Controls.Add(this.m_listViewR, 1, 1);
                    }
                    this.m_listViewR.Visible = true;
                    if ( System.IO.Directory.Exists(this.fileSystemWatcherLeft.Path) && this.filesystemMonitoringToolStripMenuItem.Checked ) {
                        this.fileSystemWatcherLeft.EnableRaisingEvents = true;
                    }
                    if ( System.IO.Directory.Exists(this.fileSystemWatcherRight.Path) && this.filesystemMonitoringToolStripMenuItem.Checked ) {
                        this.fileSystemWatcherRight.EnableRaisingEvents = true;
                    }
                    this.listRightToolStripMenuItem.Enabled = true;
                    this.detailsRightToolStripMenuItem.Enabled = true;
                    this.drivesToolStripMenuItem.Enabled = true;
                    this.refreshRightToolStripMenuItem.Enabled = true;
                    this.LoadListView(Side.right, this.m_Panel.button(Side.right).Tag.ToString(), "");  // 20160207
                }
            }
        }
        private void PreviewFileLeftToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.richTextBoxCommandOutput.Visible || !this.m_listViewR.Visible ) {
                this.PreviewFileLeftToolStripMenuItem.Checked = false;
                return;
            }
            if ( this.PreviewFileLeftToolStripMenuItem.Checked ) {
                if ( this.m_listViewL.Visible ) {
                    this.m_dtLastLoad = DateTime.Now;
                    this.fileSystemWatcherLeft.EnableRaisingEvents = false;
                    this.m_Panel.SetActiveSide(Side.right);
                    this.m_listViewL.Visible = false;
                    if ( this.listsInTabsToolStripMenuItem.Checked ) {
                        this.tableLayoutPanelLeft.Controls.Remove(this.tabControlLeft);
                    } else {
                        this.tableLayoutPanelLeft.Controls.Remove(this.m_listViewL);
                    }
                    this.tableLayoutPanelLeft.Controls.Add(this.previewCtl);
                    this.tableLayoutPanelLeft.SetCellPosition(this.previewCtl, new TableLayoutPanelCellPosition(0, 1));
                    this.tableLayoutPanelLeft.SetRowSpan(this.previewCtl, 1);
                    this.previewCtl.Dock = DockStyle.Fill;
                    this.previewCtl.Visible = true;
                    this.previewCtl.SetPreviewFiles(this.m_bImg, this.m_bDoc, this.m_bPdf, this.m_bHtm, this.m_bZip, this.m_bAsIs, this.m_bCfwVideo, this.m_bWmpAudio, this.m_bWmpVideo);
                    this.m_bStealFocus = false;
                    this.listLeftToolStripMenuItem.Enabled = false;
                    this.detailsLeftToolStripMenuItem.Enabled = false;
                    this.drivesToolStripMenuItem1.Enabled = false;
                    this.refreshLeftToolStripMenuItem.Enabled = false;
                    this.PreviewFile();
                }
            } else {
                this.m_LastPreview = "";
                if ( this.previewCtl.Visible ) {
                    this.m_bStealFocus = true;
                    this.textBoxCommandLine.Focus();
                    this.m_dtLastLoad = DateTime.Now;

                    this.previewCtl.Visible = false;
                    this.previewCtl.Clear();
                    this.tableLayoutPanelLeft.Controls.Remove(this.previewCtl);
                    if ( this.listsInTabsToolStripMenuItem.Checked ) {
                        this.tableLayoutPanelLeft.Controls.Add(this.tabControlLeft, 0, 1);
                    } else {
                        this.tableLayoutPanelLeft.Controls.Add(this.m_listViewL, 0, 1);
                    }
                    this.m_listViewL.Visible = true;

                    if ( System.IO.Directory.Exists(this.fileSystemWatcherLeft.Path) && this.filesystemMonitoringToolStripMenuItem.Checked ) {
                        this.fileSystemWatcherLeft.EnableRaisingEvents = true;
                    }
                    if ( System.IO.Directory.Exists(this.fileSystemWatcherRight.Path) && this.filesystemMonitoringToolStripMenuItem.Checked ) {
                        this.fileSystemWatcherRight.EnableRaisingEvents = true;
                    }
                    this.listLeftToolStripMenuItem.Enabled = true;
                    this.detailsLeftToolStripMenuItem.Enabled = true;
                    this.drivesToolStripMenuItem1.Enabled = true;
                    this.refreshLeftToolStripMenuItem.Enabled = true;
                    this.LoadListView(Side.left, this.m_Panel.button(Side.left).Tag.ToString(), ""); // 20160207
                }
            }
        }
        public static bool IsFileReady(String sFilename) {
            try {
                using ( FileStream inputStream = File.Open(sFilename, FileMode.Open, FileAccess.Read, FileShare.None) ) {
                    if ( inputStream.Length > 0 ) {
                        inputStream.Close();
                        return true;
                    } else {
                        return false;
                    }
                }
            } catch ( Exception ) {
                return false;
            }
        }
        private void SomeProcessInYourApp() {
            // Get association for doc/avi
            string docAsscData = AssociationsHelper.GetAssociation(".doc"); // returns : docAsscData = "C:\\Program Files\\Microsoft Office\\Office12\\WINWORD.EXE"
            string aviAsscData = AssociationsHelper.GetAssociation(".avi"); // returns : aviAsscData = "C:\\Program Files\\Windows Media Player\\wmplayer.exe"

            // Get association for an unassociated extension
            string someAsscData = AssociationsHelper.GetAssociation(".blahdeblahblahblah"); // returns : someAsscData = "C:\\Windows\\system32\\shell32.dll"
        }
        // check file associations
        internal static class AssociationsHelper {
            [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
            static extern uint AssocQueryString(AssocF flags, AssocStr str, string pszAssoc, string pszExtra, [Out] StringBuilder pszOut, [In][Out] ref uint pcchOut);
            [Flags]
            enum AssocF {
                Init_NoRemapCLSID = 0x1,
                Init_ByExeName = 0x2,
                Open_ByExeName = 0x2,
                Init_DefaultToStar = 0x4,
                Init_DefaultToFolder = 0x8,
                NoUserSettings = 0x10,
                NoTruncate = 0x20,
                Verify = 0x40,
                RemapRunDll = 0x80,
                NoFixUps = 0x100,
                IgnoreBaseClass = 0x200
            }
            enum AssocStr {
                Command = 1,
                Executable,
                FriendlyDocName,
                FriendlyAppName,
                NoOpen,
                ShellNewValue,
                DDECommand,
                DDEIfExec,
                DDEApplication,
                DDETopic
            }
            public static string GetAssociation(string doctype) {
                // size of output buffer
                uint pcchOut = 0;
                // First call is to get the required size of output buffer
                AssocQueryString(AssocF.Verify, AssocStr.Executable, doctype, null, null, ref pcchOut);
                // Allocate the output buffer
                StringBuilder pszOut = new StringBuilder((int)pcchOut);
                // Get the full pathname to the program in pszOut
                AssocQueryString(AssocF.Verify, AssocStr.Executable, doctype, null, pszOut, ref pcchOut);
                string doc = pszOut.ToString();
                return doc;
            }
        }


        // TBD: all about tree view instead of listview/preview
        private void treeViewToolStripMenuItemLeft_Click(object sender, EventArgs e) {
        }
        private void treeViewToolStripMenuItemRight_Click(object sender, EventArgs e) {
            this.treeViewToolStripMenuItemRight.Checked = false;
            return;
        }

        // shows either Main Window or Command Window
        bool ShowPanels(bool show) {
            if ( show ) {
                // make command output window invisible
                this.richTextBoxCommandOutput.Visible = false;
                // make rest of UI elements visible: they are all placed on top of the split container
                this.splitContainer1.Visible = true;
                // take preview into account
                if ( this.PreviewFileLeftToolStripMenuItem.Checked ) {
                    this.PreviewFileLeftToolStripMenuItem_Click(null, null);
                }
                if ( this.PreviewFileRightToolStripMenuItem.Checked ) {
                    this.PreviewFileRightToolStripMenuItem_Click(null, null);
                }
            } else {
                // make almost all UI elements invisible: they are all placed on top of the split container
                this.splitContainer1.Visible = false;
                // make command output windows visible
                this.richTextBoxCommandOutput.Visible = true;
                // on any open of the console, jump to the current folder
                string folder = this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString();
                if ( !GrzTools.FileTools.PathExists(folder, 500, this.m_WPD) ) {
                    folder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                }
                string cmd = "cd /D " + folder;
                // create "cmd.exe process" on first show, or use it but jump to the current dir
                if ( this.m_process == null ) {
                    this.RunCmdCommand(cmd);
                } else {
                    this.m_inputWriter.WriteLine(cmd);
                    this.m_inputWriter.Flush();
                    this.m_CommandHistory.Add(cmd);
                }
            }
            return show;
        }

        // render labelPrompt & CommandLine, match size of labelPrompt
        void RenderCommandline(string path) {
            bool exist = GrzTools.FileTools.PathExists(path, 500, this.m_WPD);
            if ( (path != "Computer") && !exist ) {
                return;
            }
            if ( this.WindowState == FormWindowState.Minimized ) {
                return;
            }

            // special case, when we are at "Computer" level
            bool isDriveReachable = true;
            if ( path == "Computer" ) {
                Side side = this.m_Panel.GetActiveSide();
                // I did see this to happen, that path and ActiveSide did not correlate
                if ( this.m_Panel.button(side).Tag.ToString() != path ) {
                    return;
                }
                ListViewItem[] arr = this.m_Panel.GetListViewArr(side);
                if ( (arr != null) && (arr.Length > 0) ) {
                    ListView lv = this.m_Panel.GetActiveView();
                    if ( lv != null ) {
                        if ( lv.SelectedIndices.Count > 0 ) {
                            int ndx = Math.Max(0, Math.Min(arr.Length - 1, lv.SelectedIndices[0]));
                            string selText = arr[ndx].Text;
                            if ( (selText == "Desktop") || (selText == "Downloads") || (selText == "Documents") ) {
                                selText = arr[ndx].SubItems[6].Text;
                            }
                            path = selText;
                            if ( arr[ndx].ImageIndex == 9 ) {
                                isDriveReachable = false;
                            }
                        } else {
                            path = arr[0].SubItems[6].Text;
                        }
                        if ( (path == "Shared Folders") || (path == "Network") ) {
                            path = @"C:\";
                        }
                    } else {
                        path = arr[0].SubItems[6].Text;
                    }
                }
            }
            if ( (path == "Computer") || (this.getIndexOfWPD(path) != -1) ) {
                path = @"C:\";
            }

            // special case, when we enter "Shared Folders" level
            if ( path == "Shared Folders" ) {
                Side side = this.m_Panel.GetActiveSide();
                ListViewItem[] arr = this.m_Panel.GetListViewArr(side);
                if ( arr != null ) {
                    ListView lv = this.m_Panel.GetActiveView();
                    if ( lv != null ) {
                        if ( lv.SelectedIndices.Count > 0 ) {
                            path = arr[lv.SelectedIndices[0]].Text;
                        } else {
                            path = arr[0].SubItems[0].Text;
                        }
                        if ( (path == "[..]") && (arr[lv.SelectedIndices[0]].ImageIndex == 2) ) { // aka "LevelUp"
                            path = @"C:\";
                        }
                    }
                }
            }

            // set current path in this.labelPrompt & take prompt sign into account
            string toshow = path + ">";
            // 20160114: UNC pathes are possible, but they definitely need no : 
            // 20161016: isDriveReachable, di != null
            if ( isDriveReachable && !toshow.StartsWith("\\") && !toshow.StartsWith("/") ) {
                DirectoryInfo di = null;
                try {  // 20161016: occasional exception when switching to Computer
                    di = new DirectoryInfo(path);
                } catch ( Exception ) {; }
                if ( di != null ) {
                    if ( toshow.Length > 1 ) {
                        if ( toshow[1] != ':' ) {
                            toshow = toshow.Insert(1, ":");
                        }
                    }
                    if ( toshow.Length > 2 ) {
                        if ( toshow[2] != '\\' ) {
                            toshow = toshow.Insert(2, "\\");
                        }
                    }
                }
            }

            this.labelPrompt.Tag = toshow;
            // shorten labelPrompt text if needed
            Font font = this.labelPrompt.Font;
            Size textSize = TextRenderer.MeasureText(toshow, font);
            int cutter = 3;
            while ( textSize.Width > this.Width / 2 ) {
                toshow = toshow.Substring(0, toshow.Length / 2 - cutter) + " ... " + toshow.Substring(toshow.Length / 2 + cutter);
                cutter++;
                textSize = TextRenderer.MeasureText(toshow, font);
            }
            this.labelPrompt.Text = toshow;
            // adjust location and width of command line
            this.textBoxCommandLine.Location = new Point(textSize.Width + 4, this.textBoxCommandLine.Location.Y);
            this.textBoxCommandLine.Size = new System.Drawing.Size(this.panelCmd.ClientSize.Width - textSize.Width - 8, this.textBoxCommandLine.Size.Height);
            // restore focus to command line just in case it got lost
            this.textBoxCommandLine.Focus();
        }
        // MainForm resizing
        protected override void OnSizeChanged(EventArgs e) {
            if ( this.m_bInitOngoing ) {
                return;
            }

            this.m_bSizing = true;

            this.m_dtLastLoad = DateTime.Now;
            base.OnSizeChanged(e);
            // when minimizing, there's no need to do anything here
            if ( this.WindowState == FormWindowState.Minimized ) {
                return;
            }
            if ( this.m_Panel == null ) {
                return;
            }
            // when resizing, we need to re adjust the column width of the listviews
            this.listViewFitColumnsToolStripMenuItem_Click(null, null);
            // command line 
            this.RenderCommandline(this.m_Panel.GetActivePath());
            // show the top buttons containing the pathes
            this.m_Panel.SetButtonText(Side.left, this.m_Panel.button(Side.left).Tag.ToString(), this.m_sLHSfilter);
            this.m_Panel.SetButtonText(Side.right, this.m_Panel.button(Side.right).Tag.ToString(), this.m_sRHSfilter);
            // in case of preview 
            if ( this.previewCtl.Visible ) {
                this.previewCtl.RePaint();
            }

            this.m_bSizing = false;
        }

        // message handlers for regular and error output from "cmd.exe process" - they are fired from a bgw, listening to the "cmd.exe process"
        void MainForm_OnProcessOutput(object sender, ProcessEventArgs e) {
            // add to output
            this.richTextBoxCommandOutput.AppendText(e.Content);
            // this flag indicates, that the inbuilt cmd.console console is supposed to change directory/drive and we therefore need to resync a ListView
            if ( this.m_bDirChangeDetected ) {
                // get last output with '>' \r \n removed 
                string newDir = e.Content.Trim('>');
                newDir = newDir.Trim('\n');
                newDir = newDir.Trim('\r');
                newDir = newDir.Trim('\n');
                if ( System.IO.Directory.Exists(newDir) ) {
                    // get path from active ListView and remove trailing '\\'
                    string currentPath = this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString().TrimEnd('\\');
                    // any difference ?
                    if ( newDir != currentPath ) {
                        // sync active ListView to new Path
                        this.LoadListView(this.m_Panel.GetActiveSide(), newDir, "[..]");
                        Side side = this.m_Panel.GetActiveSide();
                        this.m_Panel.folders.InsertTopFolder(side, this.m_Panel.GetActiveTabIndex(side), newDir);
                        this.RenderCommandline(newDir);
                        // get a list with all file & folder names in the current directory
                        this.m_CmdFilesFoldersList = GrzTools.FastFileFind.FindTopFilesFolders(newDir);
                    }
                    // reset flag
                    this.m_bDirChangeDetected = false;
                }
            }
        }
        void MainForm_OnProcessError(object sender, ProcessEventArgs e) {
            this.richTextBoxCommandOutput.AppendText(e.Content);
        }
        // public delegate & events for regular and error output from "cmd.exe process"
        public delegate void ProcessEventHandler(object sender, ProcessEventArgs args);
        public event ProcessEventHandler OnProcessOutput;
        public event ProcessEventHandler OnProcessError;
        // event arguments class for "cmd.exe process"
        public class ProcessEventArgs : EventArgs {
            public ProcessEventArgs(string content) {
                this.Content = content;
            }
            public string Content { get; private set; }
        }
        // event method for regular output from "cmd.exe process" 
        private void FireProcessOutputEvent(string content) {
            ProcessEventHandler theEvent = OnProcessOutput;
            if ( theEvent != null ) {
                theEvent(this, new ProcessEventArgs(content));
            }
        }
        // event method for error output from "cmd.exe process" 
        private void FireProcessErrorEvent(string content) {
            ProcessEventHandler theEvent = OnProcessError;
            if ( theEvent != null ) {
                theEvent(this, new ProcessEventArgs(content));
            }
        }
        // bgw "DoWork" listening to regular output from "cmd.exe process" 
        void outputWorker_DoWork(object sender, DoWorkEventArgs e) {
            while ( this.m_outputWorker.CancellationPending == false ) {
                int count;
                char[] buffer = new char[1024];
                do {
                    StringBuilder builder = new StringBuilder();
                    count = this.m_outputReader.Read(buffer, 0, 1024);
                    builder.Append(buffer, 0, count);
                    this.m_outputWorker.ReportProgress(0, builder.ToString());
                } while ( count > 0 );

                System.Threading.Thread.Sleep(200);
            }
        }
        // regular output: bgw "ProgressChanged" from "cmd.exe process" is running in the main app's context and can update UI elements
        void outputWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            if ( e.UserState is string ) {
                this.FireProcessOutputEvent(e.UserState as string);
            }
        }
        // error output: bgw "ProgressChanged" from "cmd.exe process" is running in the main app's context and can update UI elements
        void errorWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            if ( e.UserState is string ) {
                this.FireProcessErrorEvent(e.UserState as string);
            }
        }
        // bgw "DoWork" listening to error output from "cmd.exe process" 
        void errorWorker_DoWork(object sender, DoWorkEventArgs e) {
            while ( this.m_errorWorker.CancellationPending == false ) {
                //  Any lines to read?
                int count;
                char[] buffer = new char[1024];
                do {
                    StringBuilder builder = new StringBuilder();
                    count = this.m_errorReader.Read(buffer, 0, 1024);
                    builder.Append(buffer, 0, count);
                    this.m_errorWorker.ReportProgress(0, builder.ToString());
                } while ( count > 0 );

                System.Threading.Thread.Sleep(200);
            }
        }
        // connects to a non closing cmd.exe console and executes "command" typed into cmdline in cfw
        void RunCmdCommand(string command, bool bCreateWindow = false) {
            //  Configure the output & error workers
            this.m_outputWorker = new BackgroundWorker();
            this.m_errorWorker = new BackgroundWorker();
            this.m_outputWorker.WorkerReportsProgress = true;
            this.m_outputWorker.WorkerSupportsCancellation = true;
            this.m_outputWorker.DoWork += this.outputWorker_DoWork;
            this.m_outputWorker.ProgressChanged += this.outputWorker_ProgressChanged;
            this.m_errorWorker.WorkerReportsProgress = true;
            this.m_errorWorker.WorkerSupportsCancellation = true;
            this.m_errorWorker.DoWork += this.errorWorker_DoWork;
            this.m_errorWorker.ProgressChanged += this.errorWorker_ProgressChanged;

            // a new process
            this.m_process = new Process();

            // process start info
            ProcessStartInfo si = new ProcessStartInfo();
            si.FileName = "cmd.exe";
            si.Arguments = "/k " + command;
            si.StandardOutputEncoding = System.Text.Encoding.GetEncoding(437);
            si.StandardErrorEncoding = System.Text.Encoding.GetEncoding(437);
            si.WorkingDirectory = System.IO.Directory.Exists(this.m_Panel.GetActivePath()) ? this.m_Panel.GetActivePath() : "";
            si.RedirectStandardOutput = true;
            si.RedirectStandardError = true;
            si.RedirectStandardInput = true;
            si.UseShellExecute = false;
            si.CreateNoWindow = !bCreateWindow;
            si.WindowStyle = ProcessWindowStyle.Minimized | ProcessWindowStyle.Hidden;
            si.ErrorDialog = false;

            // fire
            this.m_process.StartInfo = si;
            this.m_process.Start();

            //  Create the readers and writers
            this.m_inputWriter = this.m_process.StandardInput;
            this.m_outputReader = TextReader.Synchronized(this.m_process.StandardOutput);
            this.m_errorReader = TextReader.Synchronized(this.m_process.StandardError);

            //  run bgw workers, reading output and error
            this.m_outputWorker.RunWorkerAsync();
            this.m_errorWorker.RunWorkerAsync();

            // hide m_process.MainWindowHandle
            Thread.Sleep(50);
            try {
                ShowWindow(this.m_process.MainWindowHandle, 0);
            } catch {
                ;
            }

            // keep cfw in foreground
            SetForegroundWindow(this.Handle);
            this.textBoxCommandLine.Focus();

            // get a list with all file & folder names in the current directory
            this.m_CmdFilesFoldersList = GrzTools.FastFileFind.FindTopFilesFolders(si.WorkingDirectory);
        }
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        // replace last occurance of 'Find' with 'Replace' in 'Source'
        public static string ReplaceLastOccurrence(string Source, string Find, string Replace) {
            int place = Source.LastIndexOf(Find);
            if ( place == -1 )
                return Source;
            string result = Source.Remove(place, Find.Length).Insert(place, Replace);
            return result;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AttachConsole(uint dwProcessId);
        // send keybd_event 
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        private static void SendChar(IntPtr hWnd, byte chr) {
            SetForegroundWindow(hWnd);
            keybd_event(chr, 0, 0, 0);
            keybd_event(chr, 0, 2, 0);
        }
        private static void SendCtrlZ(IntPtr hWnd) {
            const uint keyeventfKeyup = 2;
            const byte vkControl = 0x11;
            SetForegroundWindow(hWnd);
            //sending keyboard event Ctrl+Z
            keybd_event(vkControl, 0, 0, 0);
            keybd_event(0x5a, 0, 0, 0);
            keybd_event(0x5a, 0, keyeventfKeyup, 0);
            keybd_event(vkControl, 0, keyeventfKeyup, 0);
        }
        //
        // key events inside 'command line textbox'
        //
        private readonly uint WM_KEYDOWN = 0x0100;
        [DllImport("user32.dll")]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        static extern short VkKeyScan(char ch);
        // Helper 20160417: auto cmd completion via TAB - m_CmdFilesFoldersList is a string list containing files&folders (the latter ending with !) according to m_Panel.GetActivePath()
        public string FindFileFolderInCmdListPartial(string text, bool bDir = false) {
            // build a list of strings containing all filenames starting with text, source list is m_CmdFilesFoldersList 
            string returnFile = text;
            List<string> lst = new List<string>();    // list with capitalization correct
            List<string> lstOri = new List<string>(); // list with all ToLower() - easier for later char comparison
            for ( int ndx = 0; ndx < this.m_CmdFilesFoldersList.Count; ndx++ ) {
                string fullText = this.m_CmdFilesFoldersList[ndx];
                if ( fullText.StartsWith(text, StringComparison.InvariantCultureIgnoreCase) ) {
                    if ( bDir ) {
                        // search for directory: happens after a cd command
                        if ( fullText.EndsWith("!") ) {
                            string tmp = fullText.Substring(0, fullText.Length - 1);
                            lstOri.Add(tmp);
                            lst.Add(tmp.ToLower());
                        }
                    } else {
                        // all other cases == search for file
                        if ( !fullText.EndsWith("!") ) {
                            string tmp = fullText;
                            lstOri.Add(tmp);
                            lst.Add(tmp.ToLower());
                        }
                    }
                }
            }
            // search the common prefix in a list of strings stored in lst
            if ( lst.Count > 0 ) {
                int pos = 0;
                bool bRun = true;
                do {
                    // get the cmp char from first filename at current pos
                    char cmp = '0';
                    if ( pos < lst[0].Length ) {
                        cmp = lst[0][pos];
                    } else {
                        returnFile = lstOri[0].Substring(0, pos);
                        bRun = false;
                        continue;
                    }
                    // loop lst
                    for ( int i = 1; i < lst.Count; i++ ) {
                        if ( pos < lst[i].Length ) {
                            // search for first deviating char inside the filename
                            char tstChr = lst[i][pos];
                            if ( cmp != tstChr ) {
                                returnFile = lstOri[0].Substring(0, pos);
                                bRun = false;
                                break;
                            }
                        } else {
                            returnFile = lstOri[0].Substring(0, pos);
                            bRun = false;
                            break;
                        }
                    }
                    // next position in filename
                    pos++;
                } while ( bRun );
            }
            // return the common prefix OR the full name if there are no multiple choices anymore 
            return returnFile;
        }
        private void textBoxCommandLine_KeyDown(object sender, KeyEventArgs e) {
            //
            // !m_bShowPanels == send keyboard commands to console, only when console is shown
            //
            if ( !this.m_bShowPanels ) {

                //
                // PFM 20160417: command completion in command line via TAB
                //
                if ( (e.KeyCode == Keys.Tab) && (this.textBoxCommandLine.Text.Length > 0) ) {
                    string file = "";
                    string fullText = "";
                    // is there at least a space indicating a command separated from a file
                    int ndx = this.textBoxCommandLine.Text.IndexOf(' ');
                    if ( ndx != -1 ) {
                        string startTxt = this.textBoxCommandLine.Text.Substring(0, ndx).Trim(' ');
                        file = this.textBoxCommandLine.Text.Substring(ndx).Trim(' ');
                        if ( file.Length > 0 ) {
                            if ( "cd.chdir.rd.rmdir.md.mkdir".IndexOf(startTxt, StringComparison.InvariantCultureIgnoreCase) != -1 ) {
                                // request to search for a directory
                                fullText = this.FindFileFolderInCmdListPartial(file, true);
                            } else {
                                // request to search for a file ...
                                fullText = this.FindFileFolderInCmdListPartial(file);
                            }
                        }
                    } else {
                        // if there is no space in the command line, it's perhaps a run command (aka a file)
                        file = this.textBoxCommandLine.Text.Trim(' ');
                        // request to search for a file
                        fullText = this.FindFileFolderInCmdListPartial(file);
                    }
                    if ( fullText.Length > 0 ) {
                        // replace command line with fullText
                        this.textBoxCommandLine.Text = ReplaceLastOccurrence(this.textBoxCommandLine.Text, file, fullText);
                    }
                    // set caret to the end of the command line
                    this.textBoxCommandLine.Select(this.textBoxCommandLine.Text.Length, 0);
                    // set focus back to command line
                    SetForegroundWindow(this.Handle);
                    this.textBoxCommandLine.Focus();
                }

                //
                // ^Keys
                //
                if ( e.Modifiers == Keys.Control ) {
                    //
                    // Ctrl-C shall end & restart "cmd.exe process"
                    //
                    if ( e.KeyCode == Keys.C ) {
                        if ( this.m_process != null ) {
                            if ( !this.m_process.HasExited ) {
                                this.m_bCopyConMode = false;
                                // exit call
                                this.m_inputWriter.WriteLine("\x3");
                                this.m_inputWriter.Flush();
                                this.m_process.Kill();
                                this.m_process.Close();
                                this.m_process = null;
                                // unsubscribe messages
                                this.m_outputWorker.DoWork -= this.outputWorker_DoWork;
                                this.m_outputWorker.ProgressChanged -= this.outputWorker_ProgressChanged;
                                this.m_errorWorker.DoWork -= this.errorWorker_DoWork;
                                this.m_errorWorker.ProgressChanged -= this.errorWorker_ProgressChanged;
                                // stop bgws
                                this.m_outputWorker.CancelAsync();
                                this.m_errorWorker.CancelAsync();
                                Thread.Sleep(500);
                                // empty output
                                this.richTextBoxCommandOutput.Clear();
                                // start new cmd.exe process
                                this.RunCmdCommand("ver");
                            }
                        }
                    }
                    //
                    // Ctrl-Z closes "copy con" mode and returns to normal cmd.exe mode
                    //
                    if ( e.KeyCode == Keys.Z ) {
                        try {
                            if ( !this.m_process.HasExited ) {
                                // PFM: normally cmd.exe fires ^Z and ET separately, I put them together - it's easier, because ET were required to send to the 'copy con PIPE' 
                                this.richTextBoxCommandOutput.AppendText("^Z\r\n");
                                SendCtrlZ(this.m_process.MainWindowHandle);
                                Thread.Sleep(100);
                                SendChar(this.m_process.MainWindowHandle, 13);
                                Thread.Sleep(100);
                                SetForegroundWindow(this.Handle);
                                this.textBoxCommandLine.Focus();
                                this.m_bCopyConMode = false;
                            }
                        } catch ( Exception ) {; }
                    }
                }

                //
                // enter key means command execution
                //
                if ( e.KeyCode == Keys.Enter ) {

                    //
                    // start a fully external cmd.exe from cfw console window --> sometimes it's nice to have a REAL separate console 
                    //
                    if ( this.textBoxCommandLine.Text.StartsWith("cmd.exe", StringComparison.InvariantCultureIgnoreCase) ) {
                        Process p = new Process();
                        p.StartInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe", "/k");
                        p.StartInfo.WorkingDirectory = this.m_Panel.GetActivePath();
                        p.StartInfo.UseShellExecute = true;
                        p.StartInfo.CreateNoWindow = false;
                        p.Start();
                        this.richTextBoxCommandOutput.AppendText(Environment.NewLine + this.m_Panel.GetActivePath() + ">" + this.textBoxCommandLine.Text);
                        this.richTextBoxCommandOutput.AppendText(Environment.NewLine + "Console was started in separate window.");
                        this.m_CommandHistory.Add(this.textBoxCommandLine.Text);
                        this.textBoxCommandLine.Text = "";
                        return;
                    }

                    //
                    // start a process in the cfw internal cmd window OR feed a running process (m_process != null) with new input data
                    //
                    if ( (this.m_process == null) || (this.m_process.HasExited) ) {
                        // app shouldn't ever come here, it's a just in case scenario: start new process "cmd.exe", keep it running and ecxecute "command"
                        string command = this.textBoxCommandLine.Text;
                        if ( command.Length == 0 ) {
                            command = "ver";
                        }
                        this.RunCmdCommand(command);
                    } else {
                        string command = this.textBoxCommandLine.Text;
                        //
                        // sending into 'copy con' - mode is very very special: we need to send to both CONSOLE and 'copy con PIPE', but taking care about the sequence
                        //
                        if ( this.m_bCopyConMode ) {
                            // PFM: answer to question '.... überschreiben? (Ja/Nein/Alle): ' - could be english too 
                            if ( this.richTextBoxCommandOutput.Text.EndsWith("): ") ) {
                                // send answer to CONSOLE
                                this.m_inputWriter.WriteLine(command);
                                this.m_inputWriter.Flush();
                                // end copy con mode ?
                                if ( "Nein.nein.No.no".Contains(command) ) {
                                    this.m_bCopyConMode = false;
                                }
                            } else {
                                // PFM: send normal text line to 'copy con PIPE'
                                SetForegroundWindow(this.m_process.MainWindowHandle);
                                SendKeys.SendWait(command + "{ENTER}");
                                this.richTextBoxCommandOutput.AppendText(command + "\r\n");
                            }
                            // set focus back to cfw                              
                            SetForegroundWindow(this.Handle);
                            this.textBoxCommandLine.Focus();
                        } else {
                            //
                            // 20160320: if "copy con" was typed in, then there's some extra effort needed and we start "copy con mode"
                            //
                            if ( command.StartsWith("copy con", StringComparison.InvariantCultureIgnoreCase) ) {
                                // we need to restart cmd.exe, but now with an associated window (we later will send to its handle) - downside: it is flashing ugly
                                this.m_outputWorker.DoWork -= this.outputWorker_DoWork;
                                this.m_outputWorker.ProgressChanged -= this.outputWorker_ProgressChanged;
                                this.m_errorWorker.DoWork -= this.errorWorker_DoWork;
                                this.m_errorWorker.ProgressChanged -= this.errorWorker_ProgressChanged;
                                this.m_outputWorker.CancelAsync();
                                this.m_errorWorker.CancelAsync();
                                this.m_process.Kill();
                                this.m_process.Close();
                                this.m_process = null;
                                Thread.Sleep(50);
                                this.RunCmdCommand("", true);
                                // delete last line of output and append an <ET>
                                this.richTextBoxCommandOutput.Lines = this.richTextBoxCommandOutput.Lines.Take(this.richTextBoxCommandOutput.Lines.Count() - 1).ToArray();
                                this.richTextBoxCommandOutput.AppendText("\r");
                                // copy con mode is active
                                this.m_bCopyConMode = true;
                                // how to do 'copy con'
                                this.m_CommandHistory.Add(this.textBoxCommandLine.Text);          // add command to history 
                                this.textBoxCommandLine.Text = "";                           // clear input
                                this.m_inputWriter.WriteLine(command);                            // CONSOLE: write command, will be shown on screen automatically 
                                this.m_inputWriter.Flush();                                       // CONSOLE: execute command
                                Thread.Sleep(100);
                                // ugly trick: first line being a simple <ET> (<-- this is an acceptable downside) forces "copy con PIPE" to ask for '... overwrite? yes/no/all' 
                                SetForegroundWindow(this.m_process.MainWindowHandle);             // 'copy con PIPE': set foreground window
                                SendKeys.SendWait("{ENTER}");                                // 'copy con PIPE': send <ET>
                                Thread.Sleep(50);
                                // focus back to cfw
                                SetForegroundWindow(this.Handle);
                                this.textBoxCommandLine.Focus();
                                return;
                            }
                            //
                            // feed new input to cmd.exe process
                            //
                            if ( command.Length > 0 ) {
                                // if the command is a just a file (we test for it), we need to take care about its spaces; if it's not a file, then adding " would destroy the command
                                string testFile = command;
                                try {
                                    testFile = Path.Combine(this.m_Panel.GetActivePath(), command);
                                } catch ( Exception ) {; }
                                if ( File.Exists(testFile) && command.Contains(' ') ) {
                                    // it is a real file
                                    command = "\"" + command + "\"";
                                } else {
                                    //// looks like we have a preceeding cmd command: !!TBD!! The following logic understands only OneWord commands.
                                    //string[] arr = command.Split(' ');
                                    //if ( arr.Length > 1 ) {
                                    //    // sample: type te st.txt will be translated into type "te st.txt"
                                    //    command = arr[0] + " \"";
                                    //    for ( int i = 1; i < arr.Length; i++ ) {
                                    //        command += arr[i] + " ";
                                    //    }
                                    //    command = command.TrimEnd(' ');
                                    //    command += "\"";
                                    //} else {
                                    //    // samples: date, time, dir
                                    //    command = arr[0];
                                    //}
                                }
                            }
                            // execute
                            this.m_inputWriter.WriteLine(@command);
                            this.m_inputWriter.Flush();
                        }
                    }

                    //
                    // special handling cls command: it only deletes the ouptut window
                    //
                    if ( this.textBoxCommandLine.Text == "cls" ) {
                        this.richTextBoxCommandOutput.Clear();
                    }

                    //
                    // we need to treat "cd .." AND "d:" separately, because they may have changed the current Path
                    //
                    string[] carr = this.textBoxCommandLine.Text.Split(' ');
                    if ( (".cd.chdir.push.pop".IndexOf("." + carr[0], StringComparison.InvariantCultureIgnoreCase) != -1) || ((this.textBoxCommandLine.Text.Length == 2) && (this.textBoxCommandLine.Text[1] == ':')) ) {
                        this.m_bDirChangeDetected = true;
                    }

                    //
                    // always save the current command to the command history
                    //
                    this.m_CommandHistory.Add(this.textBoxCommandLine.Text);
                    this.textBoxCommandLine.Text = "";
                }

                // key up / key down mean to pick a command from the command history
                if ( e.KeyCode == Keys.Up ) {
                    this.textBoxCommandLine.Text = this.m_CommandHistory.GetPreviousCommand();
                    this.timerCommandLineCursorToEnd.Start();
                }
                if ( e.KeyCode == Keys.Down ) {
                    this.textBoxCommandLine.Text = this.m_CommandHistory.GetNextCommand();
                    this.timerCommandLineCursorToEnd.Start();
                }
                // esc means clear current command line
                if ( e.KeyCode == Keys.Escape ) {
                    this.textBoxCommandLine.Text = "";
                }
            } else {

                //
                // when console is NOT shown
                //

                // escape key:
                if ( e.KeyCode == Keys.Escape ) {
                    // esc is a kill switch, it means "stop collecting folder sizes"
                    this.m_bRunSize = false;
                    // reset listview item search text
                    if ( ModifierKeys == Keys.Shift ) {
                        this.m_searchItem = "";
                    }
                }

                // enter key: 
                //       EITHER ET loads new folder (textbox empty + selection on [..] / other folder) 
                //       OR ET execute selection (textbox empty + selection) 
                //       OR treat cd commands (textbox contains cd <args>)
                //       OR execute whatever was typed into command line (textbox contains <whatever>)
                //       OR Shift/Ctrl-Enter jumps to matching listview item (textbox contains a "StartWith" string of a listview item)
                if ( e.KeyCode == Keys.Enter ) {

                    //
                    // user types something into the text box and confirms input with (Ctrl-)Shift-Enter --> jump to a matching list item or, if not found, jump to the topmost item
                    //
                    if ( (ModifierKeys == Keys.Shift) || (ModifierKeys == (Keys.Shift | Keys.Control)) ) {
                        // the listview array 
                        ListViewItem[] lviarr = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide());
                        // if m_searchItem is existing and Shift-Enter is executed at an empty comamnd line, we search forward | Ctrl-Shift-Enter searches backwards
                        int start = 0;
                        if ( ModifierKeys == (Keys.Shift | Keys.Control) ) {
                            start = lviarr.Length - 1;
                        }
                        // m_searchItem is set if cmd-line has useful input && cmd-line != the cached searchItem OR empty cmd-line forces to reset searchItem
                        if ( ((this.textBoxCommandLine.Text.Length > 0) && (this.textBoxCommandLine.Text != this.m_searchItem)) || (this.textBoxCommandLine.Text.Length == 0) ) {
                            this.m_searchItem = this.textBoxCommandLine.Text;
                        } else {
                            if ( this.m_searchItem.Length > 0 ) {
                                if ( ModifierKeys == (Keys.Shift | Keys.Control) ) {
                                    start = this.m_Panel.listview(this.m_Panel.GetActiveSide()).FocusedItem.Index - 1;
                                    if ( start < 0 ) {
                                        start = lviarr.Length - 1;
                                    }
                                } else {
                                    start = this.m_Panel.listview(this.m_Panel.GetActiveSide()).FocusedItem.Index + 1;
                                    if ( start >= lviarr.Length ) {
                                        start = 0;
                                    }
                                }
                            } else {
                                start = 0;
                            }
                        }
                        // reset all selection
                        this.m_Panel.GetActiveView().SelectedIndices.Clear();
                        // now we search the listview item: Shift-Enter --> forward  |  Ctrl-Shift-Enter --> backward
                        if ( ModifierKeys == (Keys.Shift | Keys.Control) ) {
                            for ( int index = start; index >= 0; index-- ) {                                // we need to use an artificial listview item index ...  
                                if ( lviarr[index].Text.IndexOf(this.m_searchItem, StringComparison.InvariantCultureIgnoreCase) != -1 ) {
                                    //m_Panel.GetActiveView().SelectedIndices.Clear();
                                    this.m_Panel.GetActiveView().EnsureVisible(index);                           // ... instead of lvi.Index: lvi.Index is -1 for a "virtual mode" listview item, which is located outside of the visible range 
                                    lviarr[index].Selected = true;                                          // now we select the item 
                                    this.m_Panel.GetActiveView().EnsureVisible(lviarr[index].Index);             // from now on lvi.Index is legit  
                                    this.m_Panel.listview(this.m_Panel.GetActiveSide()).FocusedItem = lviarr[index];  // that ensures, we navigate up/down beginning with the correct item 
                                    break;
                                }
                            }
                        } else {
                            for ( int index = start; index < lviarr.Length; index++ ) {                     // we need to use an artificial listview item index ...  
                                if ( lviarr[index].Text.IndexOf(this.m_searchItem, StringComparison.InvariantCultureIgnoreCase) != -1 ) {
                                    //m_Panel.GetActiveView().SelectedIndices.Clear();
                                    this.m_Panel.GetActiveView().EnsureVisible(index);                           // ... instead of lvi.Index: lvi.Index is -1 for a "virtual mode" listview item, which is located outside of the visible range 
                                    lviarr[index].Selected = true;                                          // now we select the item 
                                    this.m_Panel.GetActiveView().EnsureVisible(lviarr[index].Index);             // from now on lvi.Index is legit  
                                    this.m_Panel.listview(this.m_Panel.GetActiveSide()).FocusedItem = lviarr[index];  // that ensures, we navigate up/down beginning with the correct item 
                                    break;
                                }
                            }
                        }
                        // goto top item in case nothing was found matching to the user input 
                        if ( this.m_Panel.GetActiveView().SelectedIndices.Count == 0 ) {
                            this.m_Panel.GetActiveView().EnsureVisible(0);
                            lviarr[0].Selected = true;
                            this.m_Panel.listview(this.m_Panel.GetActiveSide()).FocusedItem = lviarr[0];
                        }
                        // disable ..ding..   
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        // get off from here, otherwise the selected item were executed 
                        return;
                    }

                    //
                    // excute a command
                    //
                    if ( this.textBoxCommandLine.Text.Length > 0 ) {
                        // there is a pending command in the textbox
                        if ( (this.textBoxCommandLine.Text.Length == 2) && (this.textBoxCommandLine.Text[1] == ':') ) {
                            // "d:" change drive
                            string newDrive = this.textBoxCommandLine.Text.ToUpper();
                            if ( !GrzTools.Network.PingNetDriveOk(newDrive) ) {
                                newDrive = this.DRVC;
                            }
                            this.LoadListView(this.m_Panel.GetActiveSide(), newDrive, "?");
                            this.m_CommandHistory.Add(this.textBoxCommandLine.Text);
                            Side side = this.m_Panel.GetActiveSide();
                            this.m_Panel.folders.InsertTopFolder(side, this.m_Panel.GetActiveTabIndex(side), newDrive);
                            this.RenderCommandline(newDrive);
                            this.textBoxCommandLine.Text = "";
                            return;
                        } else {
                            // explicitely catch for CD
                            if ( (this.textBoxCommandLine.Text.Length > 4) && ("cd CD ".Contains(this.textBoxCommandLine.Text.Substring(0, 3))) ) {
                                if ( this.textBoxCommandLine.Text[4] == ':' ) {
                                    // "cd x:something" change drive and path absolute
                                    string newDrivePath = this.textBoxCommandLine.Text.Substring(this.textBoxCommandLine.Text.IndexOf(' ') + 1, 1).ToUpper() + this.textBoxCommandLine.Text.Substring(this.textBoxCommandLine.Text.IndexOf(' ') + 2);
                                    if ( !GrzTools.Network.PingNetDriveOk(newDrivePath/*.Substring(0, 2)*/) ) {
                                        newDrivePath = this.DRVC;
                                    }
                                    DirectoryInfo di = new DirectoryInfo(newDrivePath);
                                    newDrivePath = GrzTools.FileTools.GetProperDirectoryCapitalization(di);
                                    this.LoadListView(this.m_Panel.GetActiveSide(), newDrivePath, "");
                                    this.m_CommandHistory.Add(this.textBoxCommandLine.Text);
                                    Side side = this.m_Panel.GetActiveSide();
                                    this.m_Panel.folders.InsertTopFolder(side, this.m_Panel.GetActiveTabIndex(side), newDrivePath);
                                    this.RenderCommandline(newDrivePath);
                                    this.textBoxCommandLine.Text = "";
                                    return;
                                } else {
                                    Side side = this.m_Panel.GetActiveSide();
                                    if ( (this.textBoxCommandLine.Text.Length == 5) && ("cd ..CD ..".Contains(this.textBoxCommandLine.Text.Substring(0, 5))) ) {
                                        // "cd .." one level up
                                        this.m_CommandHistory.Add(this.textBoxCommandLine.Text);
                                        this.textBoxCommandLine.Text = "";
                                        this.ListviewOneLevelUp(side);
                                        return;
                                    }
                                    // special folders?
                                    string basPath = this.m_Panel.button(side).Tag.ToString();
                                    string chgPath = this.textBoxCommandLine.Text.Substring(this.textBoxCommandLine.Text.IndexOf(' ') + 1);
                                    if ( chgPath.StartsWith("%") && chgPath.EndsWith("%") ) {
                                        string envvar = chgPath.ToLower().Substring(1, chgPath.Length - 2);
                                        chgPath = Environment.GetEnvironmentVariable(envvar);
                                        if ( System.IO.Directory.Exists(chgPath) ) {
                                            basPath = "";
                                        } else {
                                            return;
                                        }
                                    }
                                    // "cd something" change path relative
                                    string finPath = Path.Combine(basPath, chgPath);
                                    DirectoryInfo di = new DirectoryInfo(finPath);
                                    if ( !di.Exists ) {
                                        return;
                                    }
                                    finPath = GrzTools.FileTools.GetProperDirectoryCapitalization(di);
                                    if ( !GrzTools.Network.PingNetDriveOk(finPath/*.Substring(0, 2)*/) ) {
                                        return;
                                    }
                                    this.LoadListView(this.m_Panel.GetActiveSide(), finPath, "");
                                    this.m_CommandHistory.Add(this.textBoxCommandLine.Text);
                                    this.m_Panel.folders.InsertTopFolder(side, this.m_Panel.GetActiveTabIndex(side), finPath);
                                    this.RenderCommandline(finPath);
                                    this.textBoxCommandLine.Text = "";
                                    return;
                                }
                            }
                        }
                        // if it goes up here, we might have a useful command to execute
                        Process p = new Process();
                        p.StartInfo = new System.Diagnostics.ProcessStartInfo(this.textBoxCommandLine.Text);
                        p.StartInfo.WorkingDirectory = this.m_Panel.GetActivePath();
                        p.StartInfo.UseShellExecute = true;
                        p.StartInfo.CreateNoWindow = false;
                        string memo = this.textBoxCommandLine.Text;
                        this.m_CommandHistory.Add(memo);
                        this.textBoxCommandLine.Text = "";
                        try {
                            p.Start();
                        } catch ( Exception ) {
                            GrzTools.AutoMessageBox.Show("'" + memo + "' is not a valid command.", "Error", 2000);
                        }
                    } else {
                        // if the textbox is empty, we simulate a double click into the active list
                        ListView view = this.m_Panel.GetActiveView();
                        this.listViewLeftRight_DoubleClick(view, null);
                        // disable ..ding.. after empty <enter>
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                } else {
                    // we need to suppress F1 ... F10 from here, otherwise Alt-F7, F7, F8 etc. would fire twice - all other keys are passed to listview 
                    if ( !(e.KeyCode >= Keys.F1 && e.KeyCode <= Keys.F12) ) {
                        // 20161016: we only allow forward keystrokes to listview, if command line is empty - if it's not, the operator may rather play with commands/strings as to navigate the listview
                        if ( this.textBoxCommandLine.Text.Length == 0 ) {
                            ListView view = this.m_Panel.GetActiveView();
                            this.m_bStealFocus = false;
                            view.Focus();
                            PostMessage(view.Handle, this.WM_KEYDOWN, (IntPtr)e.KeyCode, IntPtr.Zero);
                            this.m_bStealFocus = true;
                        }
                        this.textBoxCommandLine.Focus();
                    }
                }
            }
        }
        // delete + - * if cmd length is 1
        private void textBoxCommandLine_TextChanged(object sender, EventArgs e) {
            if ( this.textBoxCommandLine.Text.Length == 1 ) {
                string sSingle = this.textBoxCommandLine.Text.Substring(0, 1);
                if ( "+-*".Contains(sSingle) ) {
                    this.textBoxCommandLine.Text = "";
                }
            }

            // load Del debouncer unconditionally at any text change
            this.m_dtDebounceDel = DateTime.Now;
        }
        // unconditionally reset Focus to command line edit 
        private void textBoxCommandLine_Leave(object sender, EventArgs e) {
            if ( this.m_bStealFocus ) {
                this.textBoxCommandLine.Focus();
            }
        }
        // command window output context menu
        private void toolStripMenuItemAll_Click(object sender, EventArgs e) {
            this.richTextBoxCommandOutput.Select(0, this.richTextBoxCommandOutput.Text.Length);
        }
        private void toolStripMenuItemCopy_Click(object sender, EventArgs e) {
            this.richTextBoxCommandOutput.Copy();
        }
        private void searchToolStripMenuItem_Click(object sender, EventArgs e) {
            string selection = this.richTextBoxCommandOutput.SelectedText;
            SimpleInput dlg = new SimpleInput();
            dlg.Text = "Search Text";
            dlg.Hint = "continue searching F3 / Shift-F3";
            dlg.Input = (this.richTextBoxCommandOutput.SelectedText != this.m_richTextBoxCommandOutputSearchText) ? this.richTextBoxCommandOutput.SelectedText : this.m_richTextBoxCommandOutputSearchText;
            dlg.SetOption("search backward", this.m_richTextBoxCommandOutputSearchBackward);
            dlg.ShowDialog();
            if ( dlg.DialogResult != DialogResult.OK ) {
                return;
            }
            this.m_richTextBoxCommandOutputSearchText = dlg.Input;
            this.m_richTextBoxCommandOutputSearchBackward = dlg.GetOption();
            if ( this.m_richTextBoxCommandOutputSearchBackward ) {
                this.richTextBoxCommandOutputSearchBackward();
            } else {
                this.richTextBoxCommandOutputSearchForward();
            }
        }
        // colorize the find string EVERYWEHRE in the given textbox
        void ColorizeAllFindText(RichTextBox richTextBox, string findText) {
            Color bc = richTextBox.SelectionBackColor;
            Color sc = richTextBox.SelectionColor;
            int memstartpos = richTextBox.SelectionStart;
            StringComparison scmp = StringComparison.OrdinalIgnoreCase;
            int start = 0;
            do {
                Application.DoEvents();
                int ndx = richTextBox.Text.IndexOf(findText, Math.Min(richTextBox.Text.Length, start), scmp);
                if ( ndx != -1 ) {
                    richTextBox.Select(ndx, findText.Length);
                    richTextBox.SelectionBackColor = Color.Yellow;
                    richTextBox.SelectionColor = Color.Red;
                    start = ndx + findText.Length;
                } else {
                    break;
                }
            } while ( true );
            richTextBox.SelectionStart = memstartpos;
            richTextBox.SelectionLength = 0;
            richTextBox.SelectionBackColor = bc;
            richTextBox.SelectionColor = sc;
        }
        private void selectSearchTextToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.selectSearchTextToolStripMenuItem.Checked ) {
                this.ColorizeAllFindText(this.richTextBoxCommandOutput, this.m_richTextBoxCommandOutputSearchText);
            } else {
                int ss = this.richTextBoxCommandOutput.SelectionStart;
                int sl = this.richTextBoxCommandOutput.SelectionLength;
                this.richTextBoxCommandOutput.Select(0, this.richTextBoxCommandOutput.Text.Length);
                this.richTextBoxCommandOutput.SelectionColor = Color.White;
                this.richTextBoxCommandOutput.SelectionBackColor = Color.Black;
                this.richTextBoxCommandOutput.Select(ss, sl);
            }
        }
        void richTextBoxCommandOutputSearchBackward() {
            int pos = this.richTextBoxCommandOutput.Find(this.m_richTextBoxCommandOutputSearchText, 0, this.richTextBoxCommandOutput.SelectionStart, RichTextBoxFinds.Reverse);
            if ( pos != -1 ) {
                this.richTextBoxCommandOutput.Select(pos, this.m_richTextBoxCommandOutputSearchText.Length);
            } else {
                // not found, but previously found --> wrap around
                if ( this.richTextBoxCommandOutput.SelectionStart != this.richTextBoxCommandOutput.Text.Length ) {
                    this.richTextBoxCommandOutput.SelectionStart = this.richTextBoxCommandOutput.Text.Length - 1;
                }
            }
        }
        void richTextBoxCommandOutputSearchForward() {
            int start = this.richTextBoxCommandOutput.SelectionStart + 1;
            if ( start > this.richTextBoxCommandOutput.Text.Length ) {
                start = 0;
            }
            int pos = this.richTextBoxCommandOutput.Find(this.m_richTextBoxCommandOutputSearchText, start, RichTextBoxFinds.NoHighlight);
            if ( pos != -1 ) {
                this.richTextBoxCommandOutput.Select(pos, this.m_richTextBoxCommandOutputSearchText.Length);
            } else {
                // not found, but previously found --> wrap around
                if ( this.richTextBoxCommandOutput.SelectionStart != 0 ) {
                    this.richTextBoxCommandOutput.Select(1, 1);
                }
            }
        }
        // this allows to select text inside the command window output
        private void richTextBoxCommandOutput_MouseEnter(object sender, EventArgs e) {
            this.m_bStealFocus = false;
        }
        private void richTextBoxCommandOutput_MouseLeave(object sender, EventArgs e) {
            this.m_bStealFocus = true;
            this.textBoxCommandLine.Focus();
        }
        private void richTextBoxCommandOutput_MouseUp(object sender, MouseEventArgs e) {
            this.textBoxCommandLine.Focus();
        }
        // key up/down scroll thru commands from the command history - BUT the caret gets misplaced by one character unless we wait for textBoxCommandLine_KeyDown() being fully processed
        private void timerCommandLineCursorToEnd_Tick(object sender, EventArgs e) {
            this.timerCommandLineCursorToEnd.Stop();
            this.textBoxCommandLine.Select(this.textBoxCommandLine.Text.Length, 0);
        }

        // command history class: 
        // - wrapper around List<string>
        // - maintains an index, representing the current item of the history list
        // - returns on request prev or next command in list
        public class CommandHistory {
            private int iCurrentItem = 0;
            private readonly List<string> list = new List<string>();
            public void Add(string command) {
                this.list.Add(command);
                this.iCurrentItem = this.list.Count - 1;
            }
            public string GetPreviousCommand() {
                if ( this.list.Count == 0 ) {
                    return "";
                }
                string tmp = this.list[this.iCurrentItem];
                if ( this.iCurrentItem > 0 ) {
                    this.iCurrentItem--;
                }
                return tmp;
            }
            public string GetNextCommand() {
                if ( this.list.Count == 0 ) {
                    return "";
                }
                if ( this.iCurrentItem < this.list.Count - 1 ) {
                    this.iCurrentItem++;
                }
                return this.list[this.iCurrentItem];
            }
        }

        // menu handlers for listview appearence : list vs. details
        private void listLeftToolStripMenuItem_Click(object sender, EventArgs e) {
            this.m_listViewL.View = View.List;
            this.listLeftToolStripMenuItem.Checked = true;
            this.detailsLeftToolStripMenuItem.Checked = false;
        }
        private void detailsLeftToolStripMenuItem_Click(object sender, EventArgs e) {
            this.m_listViewL.View = View.Details;
            this.listLeftToolStripMenuItem.Checked = false;
            this.detailsLeftToolStripMenuItem.Checked = true;
        }
        private void listRightToolStripMenuItem_Click(object sender, EventArgs e) {
            this.m_listViewR.View = View.List;
            this.listRightToolStripMenuItem.Checked = true;
            this.detailsRightToolStripMenuItem.Checked = false;
        }
        private void detailsRightToolStripMenuItem_Click(object sender, EventArgs e) {
            this.m_listViewR.View = View.Details;
            this.listRightToolStripMenuItem.Checked = false;
            this.detailsRightToolStripMenuItem.Checked = true;
        }

        // ListView filter setting 
        private void LHSfilterToolStripMenuItem_Click(object sender, EventArgs e) {
            InputBox ib = new InputBox();
            ib.Category = this.m_sLHSfilter;
            ib.Text = "Select file filter for left list";
            ib.ShowDialog();
            if ( ib.DialogResult == DialogResult.OK ) {
                this.m_sLHSfilter = ib.Category;
                this.LHSfilterToolStripMenuItem.Text = "List Filter " + this.m_sLHSfilter;
                this.LHSfilterToolStripMenuItem.ForeColor = Color.Black;
                if ( this.m_sLHSfilter != "*.*" ) {
                    this.LHSfilterToolStripMenuItem.ForeColor = Color.Red;
                }
                this.LoadListView(Side.left, this.m_Panel.button(Side.left).Tag.ToString(), "", this.m_sLHSfilter);
            }
        }
        private void RHSfilterToolStripMenuItem_Click(object sender, EventArgs e) {
            InputBox ib = new InputBox();
            ib.Category = this.m_sRHSfilter;
            ib.Text = "Select file filter for right list";
            ib.ShowDialog();
            if ( ib.DialogResult == DialogResult.OK ) {
                this.m_sRHSfilter = ib.Category;
                this.RHSfilterToolStripMenuItem.Text = "List Filter " + this.m_sRHSfilter;
                this.RHSfilterToolStripMenuItem.ForeColor = Color.Black;
                if ( this.m_sRHSfilter != "*.*" ) {
                    this.RHSfilterToolStripMenuItem.ForeColor = Color.Red;
                }
                this.LoadListView(Side.right, this.m_Panel.button(Side.right).Tag.ToString(), "", this.m_sRHSfilter);
            }
        }

        public List<ListViewItem> FindFilesFoldersWPD(Side side, int tabIndex, int wpdIndex, string sStartDir, out int maxLen2, out int maxLen3, string[] iconsInfo, ref ImageList imgLst, int iLimit = int.MaxValue, string filter = "*.*", bool bSlowDrive = false, bool bHighlightEmptyFolder = false) {
            maxLen2 = 8;
            maxLen3 = 7;

            // we fill a temporary List of ListViewItem and drop it at once to the ListView lst
            List<ListViewItem> retList = new List<ListViewItem>();
            string[] strarr = new string[8] { "[..]", " ", "<PARENT>", " ", " ", " ", " ", "0" };

            // we always have a "level up", at least to "Computer"
            retList.Add(new ListViewItem(strarr, 2));

            // vars
            string path = @Path.Combine(@sStartDir, @"*.*");
            bool bFilter = (filter == "*.*") ? false : true;          // winsxs: RegEx and one loop for folders&files gains ca. 50ms compared to a second file loop and no RegEx
            System.Text.RegularExpressions.Regex regex = GrzTools.FastFileFind.FindFilesPatternToRegex.Convert(filter);

            // WPD
            string currentFolderID = "";
            this.m_WPD[wpdIndex].wpd.Connect();
            List<PortableDevices.PortableDevice.wpdFileInfo> list = this.m_WPD[wpdIndex].wpd.GetFolderContentList(@sStartDir, ref currentFolderID);
            this.m_WPD[wpdIndex].wpd.Disconnect();
            WPD wpd = this.m_Panel.GetWPD(side, tabIndex);
            this.m_Panel.SetWPD(side, tabIndex, new WPD(this.m_WPD[wpdIndex].wpd, this.m_WPD[wpdIndex].deviceName, currentFolderID));

            foreach ( PortableDevices.PortableDevice.wpdFileInfo item in list ) {

                // item --> "x:filename" where x = p (path) or f (file)
                string[] itemSplit = item.name.Split(':');

                if ( itemSplit[0] == "p" ) {
                    //
                    // ALL DIRECTORIES
                    //
                    // folder name
                    strarr[0] = itemSplit[1];
                    // datetime
                    strarr[1] = item.date;
                    strarr[2] = "<SUBDIR>";
                    maxLen2 = Math.Max(maxLen2, strarr[2].Length);
                    strarr[3] = "";
                    strarr[6] = item.id; // fileID for a WPD object
                    try {
                        DateTime dtft = (DateTime.ParseExact(strarr[1], "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal)).ToUniversalTime();
                        strarr[7] = dtft.ToString("yyyyMMddHHmmss"); // allows faster sort 
                    } catch {; }
                    int imageNndx = 3;
                    // finally add folder to return list
                    retList.Add(new ListViewItem(strarr, imageNndx));
                } else {
                    //
                    // ALL FILES
                    //
                    // RegEx is slightly faster than a 2nd loop of FindFirstFile / FindNextFile
                    if ( bFilter && !regex.IsMatch(itemSplit[1]) ) {
                        continue;
                    }
                    string ext = Path.GetExtension(itemSplit[1]).ToLower();
                    strarr[0] = itemSplit[1];
                    strarr[1] = item.date;
                    strarr[2] = item.size.ToString("0,0", CultureInfo.InvariantCulture);
                    maxLen2 = Math.Max(maxLen2, strarr[2].Length);
                    strarr[3] = GrzTools.FastFileFind.GetMimeType(ext);
                    maxLen3 = Math.Max(maxLen3, strarr[3].Length);
                    strarr[6] = item.id;
                    DateTime dtft = (DateTime.ParseExact(strarr[1], "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal)).ToUniversalTime();
                    strarr[7] = dtft.ToString("yyyyMMddHHmmss"); // allows faster sort 
                    // 20160221: get matching file icon
                    int imageindex = 1;                                   // default icon index is simple file  
                    if ( ext == ".dll" ) {                                // .dll has a homebrewn icon
                        imageindex = 11;
                    } else {
                        if ( ext == ".exe" ) {                            // .exe has its own icon stored within executable
                            imageindex = 12;
                        } else {
                            int iconPos = Array.IndexOf(iconsInfo, ext);  // all other files' extensions are stored in iconsInfo, we simply look up the matching index 
                            if ( iconPos != -1 ) {
                                imageindex = iconPos + 13;
                            }
                        }
                    }
                    // finally add file to return list
                    retList.Add(new ListViewItem(strarr, imageindex));
                }
            }

            // return a list of ListViewItem
            return retList;
        }

        // central function to load a listview: side --> left/right, folder --> path to load, selectItem --> text select matching item / * select a new item / ? keep current selection, filter --> file mask pattern
        async private Task<int> LoadListView(Side side, string folder, string selectItem, string filter = "*.*") {
            // if cfw was started from altf7dlg a valid filename is provided to select it
            if ( side == Side.left ) {
                if ( this.m_startfile.Length > 0 ) {
                    selectItem = this.m_startfile;
                    this.m_startfile = "";
                }
            }

            // sanity check
            if ( folder == null ) {
                folder = this.DRVC;
            }
            if ( folder.Length == 0 ) {
                folder = this.DRVC;
            }

            // unconditionally stop self refresh Computer view
            this.timerRefeshComputerView.Stop();

            // we check for special folders
            if ( folder[1] != ':' ) {
                // Did we perhaps select "Computer" itself?
                if ( (folder == "Computer") || (folder == "My Computer") ) {
                    // the 3rd param sizeinfo == 'false' skips size, it's deferred till refresh and UI feels more responsive | 4th param skips network drives on first show because GetDrives may hang on not available netdrives
                    this.LoadDrivesList(side, selectItem, false, true);
                    if ( side == this.m_Panel.GetActiveSide() ) {
                        this.RenderCommandline("Computer");
                    }
                    // no FS watchdog in case of Computer View
                    if ( side == Side.left ) {
                        if ( this.m_driveDetector[0] != null ) {
                            this.m_driveDetector[0].DisableQueryRemove();
                            this.m_driveDetector[0].Dispose();
                            this.m_driveDetector[0] = null;
                        }
                        this.fileSystemWatcherLeft.EnableRaisingEvents = false;
                    }
                    if ( side == Side.right ) {
                        if ( this.m_driveDetector[1] != null ) {
                            this.m_driveDetector[1].DisableQueryRemove();
                            this.m_driveDetector[1].Dispose();
                            this.m_driveDetector[1] = null;
                        }
                        this.fileSystemWatcherRight.EnableRaisingEvents = false;
                    }
                    this.m_Panel.listType[(int)side] = ListType.FileSystem;
                    return 0;
                }
                // Did we select "Shared Folders"
                if ( (folder == @"Computer\Shared Folders") || (folder == "Shared Folders") ) {
                    this.LoadSharedFolders(side);
                    if ( side == this.m_Panel.GetActiveSide() ) {
                        this.RenderCommandline("Shared Folders");
                    }
                    return 0;
                }
                // Did we select "Network"
                if ( folder == "Network" ) {
                    this.LoadNetworkList(side);
                    return -1;
                }
                // other special folders
                if ( folder == @"Computer\Desktop" ) {
                    folder = GrzTools.FileTools.TranslateSpecialFolderNames("Desktop");
                }
                if ( folder == @"Computer\Documents" ) {
                    folder = GrzTools.FileTools.TranslateSpecialFolderNames("Documents");
                }
                if ( folder == @"Computer\Downloads" ) {
                    folder = GrzTools.FileTools.TranslateSpecialFolderNames("Downloads");
                }
            }

            // filter setting
            if ( side == Side.left ) {
                filter = this.m_sLHSfilter;
            } else {
                filter = this.m_sRHSfilter;
            }

            // drive letter and : (the only case with length == 2) we need special treatment
            if ( (folder[1] == ':') && (folder.Length == 2) ) {
                folder += "\\";
            }

            // check whether folder exists
            int wpdIndex = -1;
            if ( !GrzTools.FileTools.PathExists(folder, 2000) ) { // 20161016: !System.IO.Directory.Exists(folder) hangs for 20s on a not connected network drive 
                wpdIndex = this.getIndexOfWPD(folder);
                if ( wpdIndex == -1 ) {
                    GrzTools.AutoMessageBox.Show("Destination path might not be accessible:\n\n" + folder, "Error", 2000);
                }
            }

            // we need this info later on, even null is acceptable
            DriveInfo di = null;
            try {
                di = new DriveInfo(folder.Substring(0, 3));
            } catch ( Exception ) {; }

            List<ListViewItem> data = null;

            // expect huge lag in case of network drives
            bool networkDrive = false;
            bool highlightEmpty = this.highlightEmptyFolderToolStripMenuItem.Checked;
            if ( NetworkMapping.MappedDriveResolver.isNetworkDrive(folder) ) {
                highlightEmpty = false;
                networkDrive = true; ;
            }

            if ( wpdIndex != -1 ) {
                this.m_Panel.listType[(int)side] = ListType.WPDsrc;
                data = this.FindFilesFoldersWPD(side,
                                            this.m_Panel.GetActiveTabIndex(side),
                                            wpdIndex,
                                            folder,
                                            out this.m_Panel.maxLen2[(int)side, this.m_Panel.GetActiveTabIndex(side)],
                                            out this.m_Panel.maxLen3[(int)side, this.m_Panel.GetActiveTabIndex(side)],
                                            this.m_ExtensionIconIndexArray,
                                            ref this.imageListLv,
                                            this.m_iListViewLimit,
                                            filter,
                                            true/*di.DriveType != DriveType.Fixed*/,
                                            this.highlightEmptyFolderToolStripMenuItem.Checked);
            } else {
                // Stopwatch sw = Stopwatch.StartNew();
                // 20160424: retrieve just m_iListViewLimit items (winsxs slowdown), null returned is the indicator for a non accessible folder, only .Text == "[..]" is an empty folder
                this.m_Panel.listType[(int)side] = ListType.FileSystem;

                this.m_Panel.listview(side).Enabled = false;
                await Task.Run(()=> {
                    data = this.m_fff.FindFilesFolders(folder,
                                               out this.m_Panel.maxLen2[(int)side, this.m_Panel.GetActiveTabIndex(side)],
                                               out this.m_Panel.maxLen3[(int)side, this.m_Panel.GetActiveTabIndex(side)],
                                               this.m_ExtensionIconIndexArray,
                                               ref this.imageListLv,
                                               this.m_iListViewLimit,
                                               filter,
                                               true/*di.DriveType != DriveType.Fixed*/,
                                               highlightEmpty);
                });
                this.m_Panel.listview(side).Enabled = true;

                // this.Text = DateTime.Now.ToString("HH:mm:ss ", CultureInfo.InvariantCulture) + sw.ElapsedMilliseconds.ToString() + "ms";
            }


            // no useful data were returned
            if ( data == null ) {
                // perhaps we have an empty drive
                if ( folder.Length == 3 ) {
                    if ( di != null ) {
                        if ( di.IsReady ) {
                            data = new List<ListViewItem>();
                        }
                    }
                }
                // if there are still no data, we give up
                if ( data == null ) {
                    GrzTools.AutoMessageBox.Show("Destination is not accessible:\n\n" + folder, "Error", 2000);
                    this.m_Panel.listType[(int)side] = ListType.Error;
                    return -1;
                }
            }

            // '*' signals that the call came from someone, who wants to select EITHER a presumably new found item (copy, move) OR the next item to a missing item (move, del)
            if ( (selectItem.Length == 1) && (selectItem[0] == '*') ) {
                ListViewItem[] lvOld = this.m_Panel.GetListViewArr(side);
                // 20160821: is there any difference between the new list compared to the old list
                if ( lvOld.Length != data.Count ) {
                    if ( lvOld.Length > data.Count ) {
                        // 20160821: we need to look for missing items in new list
                        bool bFound = false;
                        foreach ( ListViewItem lviold in lvOld ) {
                            bFound = false;
                            foreach ( ListViewItem lvinew in data ) {
                                if ( lviold.Text == lvinew.Text ) {
                                    bFound = true;
                                    break;
                                }
                            }
                            if ( !bFound ) {
                                selectItem = lvOld[Math.Min(lviold.Index + 1, lvOld.Length - 1)].Text;
                                break;
                            }
                        }
                        if ( bFound ) {
                            selectItem = "?";
                        }
                    } else {
                        // we need to look for new items in new list
                        bool bFound = false;
                        foreach ( ListViewItem lvinew in data ) {
                            bFound = false;
                            foreach ( ListViewItem lviold in lvOld ) {
                                if ( lviold.Text == lvinew.Text ) {
                                    bFound = true;
                                    break;
                                }
                            }
                            // if a new item was found, we select it assuming it is a just created item
                            if ( !bFound ) {
                                selectItem = lvinew.Text;
                                break;
                            }
                        }
                        // if no new item was found, then we keep the current selection
                        if ( bFound ) {
                            selectItem = "?";
                        }
                    }
                } else {
                    // 20160821: if old and new lists are almost identical (at least from count prospective), then we keep the current selection
                    selectItem = "?";
                    // 20161016: although it doesn't sound very likely, that the lists are not identical in case their item counts are matching, IT STILL HAPPENS: dragNdrop op in listview into an empty folder!!!
                    // reverted --> return -1;
                }
            }

            // "?" signals that the call came from someone, who wants to keep the current selection
            List<string> lvsel = new List<string>();
            if ( (selectItem.Length == 1) && (selectItem[0] == '?') ) {
                for ( int i = 0; i < this.m_Panel.listview(side).SelectedIndices.Count; i++ ) {
                    lvsel.Add(this.m_Panel.GetListViewArr(side)[this.m_Panel.listview(side).SelectedIndices[i]].Text);
                }
            }

            //
            // all about the new listview content
            //
            this.m_dtLastLoad = DateTime.Now;                                                      // block resize events when listview columns are automatically adjusted
            Cursor.Current = Cursors.WaitCursor;                                              // now we begin being busy
            this.m_Panel.button(side).BackColor = SystemColors.Control;                            // set button text color to normal
            this.m_Panel.button(side).Tag = folder;                                                // set button Tag according to folder  
            this.m_Panel.SetListPath(side, this.m_Panel.GetActiveTabIndex(side), folder, false);
            this.m_Panel.SetButtonText(side, folder, filter);                                      // set button Text according to folder 
            this.setTabControlText(side, this.m_Panel.GetActiveTabIndex(side), folder);
            if ( side == Side.left ) {
                try {
                    if ( di != null ) {
                        if ( di.DriveType == DriveType.Removable ) {                          // removable devices need a drive detector monitoring the query remove question from OS
                            if ( this.m_driveDetector[0] != null ) {                               // it's crucial to unregister/dispose DriveDetector before attaching a new detector to a drive
                                this.m_driveDetector[0].DisableQueryRemove();
                                this.m_driveDetector[0].Dispose();
                                this.m_driveDetector[0] = null;
                            }
                            this.m_driveDetector[0] = new GrzTools.DriveDetector(this, di.RootDirectory.FullName);
                        } else {
                            if ( this.m_driveDetector[0] != null ) {
                                this.m_driveDetector[0].DisableQueryRemove();
                                this.m_driveDetector[0].Dispose();
                                this.m_driveDetector[0] = null;
                            }
                        }

                    } else {
                        if ( this.m_driveDetector[0] != null ) {
                            this.m_driveDetector[0].DisableQueryRemove();
                            this.m_driveDetector[0].Dispose();
                            this.m_driveDetector[0] = null;
                        }
                    }
                    this.fileSystemWatcherLeft.Path = this.m_Panel.button(Side.left).Tag.ToString(); // FS watcher monitors file/folder changes to any folder connected here
                    this.fileSystemWatcherLeft.Filter = "*";
                    this.m_Panel.InitFileSystemWatcher(Side.left, this.fileSystemWatcherLeft);
                    if ( System.IO.Directory.Exists(this.fileSystemWatcherLeft.Path) ) {
                        this.fileSystemWatcherLeft.EnableRaisingEvents = true;
                    }
                } catch ( Exception ) {; }
            }
            if ( side == Side.right ) {
                try {
                    if ( di != null ) {
                        if ( di.DriveType == DriveType.Removable ) {
                            if ( this.m_driveDetector[1] != null ) {
                                this.m_driveDetector[1].DisableQueryRemove();
                                this.m_driveDetector[1].Dispose();
                                this.m_driveDetector[1] = null;
                            }
                            this.m_driveDetector[1] = new GrzTools.DriveDetector(this, di.RootDirectory.FullName);
                        } else {
                            if ( this.m_driveDetector[1] != null ) {
                                this.m_driveDetector[1].DisableQueryRemove();
                                this.m_driveDetector[1].Dispose();
                                this.m_driveDetector[1] = null;
                            }
                        }

                    } else {
                        if ( this.m_driveDetector[1] != null ) {
                            this.m_driveDetector[1].DisableQueryRemove();
                            this.m_driveDetector[1].Dispose();
                            this.m_driveDetector[1] = null;
                        }
                    }
                    this.fileSystemWatcherRight.Path = this.m_Panel.button(Side.right).Tag.ToString();
                    this.fileSystemWatcherRight.Filter = "*";
                    this.m_Panel.InitFileSystemWatcher(Side.right, this.fileSystemWatcherRight);
                    if ( System.IO.Directory.Exists(this.fileSystemWatcherRight.Path) && this.filesystemMonitoringToolStripMenuItem.Checked ) {
                        this.fileSystemWatcherRight.EnableRaisingEvents = true;
                    }
                } catch ( Exception ) {; }
            }

            // the listview we deal with
            ListView lv = this.m_Panel.listview(side);
            lv.BeginUpdate();                                                              // stop paint
            lv.SelectedIndices.Clear();                                                    // vm - reset any selection previously made  
            lv.Columns[0].Text = "Name";                                                   // we unconditionally restore columns for regular folder views, because "Computer" uses different column headers   
            lv.Columns[1].Text = "Type";
            lv.Columns[1].TextAlign = HorizontalAlignment.Left;
            lv.Columns[2].Text = "Size";
            lv.Columns[2].TextAlign = HorizontalAlignment.Right;
            lv.Columns[3].Text = "Date";
            lv.Columns[3].TextAlign = HorizontalAlignment.Left;
            lv.Columns[4].Text = "";
            lv.Columns[5].Text = "";
            lv.Columns[6].Text = "";
            lv.Columns[4].Width = 0;
            lv.Columns[5].Width = 0;
            lv.Columns[6].Width = 0;
            lv.Columns[7].Width = 0;
            try {
                try {
                    //lv.VirtualListSize = data.Count;                                                // vm - the list needs to know, how large it is 
                    this.m_Panel.SetListViewArr(side, this.m_Panel.GetActiveTabIndex(side), data.ToArray());  // vm - we store the data not in the list itself
                } catch ( InvalidOperationException ioe ) {
                    this.Text = ioe.Message;
                    lv.EndUpdate();
                    return -1;
                }
            } catch ( NullReferenceException nre ) {
                this.Text = nre.Message;
                lv.EndUpdate();
                return -1;
            }

            // sort listview
            this.SortListView(side, side == Side.left ? this.m_Panel.LastSortedColumnLhs[this.m_Panel.GetActiveTabIndex(side)] : this.m_Panel.LastSortedColumnRhs[this.m_Panel.GetActiveTabIndex(side)], true);
            Cursor.Current = Cursors.Default;
            if ( lv.Items.Count > 0 ) {
                lv.EnsureVisible(0);                                                         // initially scroll to top
            }
            lv.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);                    // tbd: ohne diese Zeile läßt sich kein Item auswählen (später)
            lv.Columns[4].Width = 0;
            lv.Columns[5].Width = 0;
            lv.Columns[6].Width = 0;
            lv.Columns[7].Width = 0;

            // selection in ListView
            if ( selectItem.Length != 0 ) {
                ListViewItem lvi = this.m_Panel.FindListViewArrItemWithText(side, selectItem, -1);// search the virtual data array and return a ListViewItem !!! .Tag is the Index of the found Item !!! 
                if ( lvi != null ) {
                    int selNdx = (int)lvi.Tag;
                    lv.Items[selNdx].Focused = true;                                         // important for keyup/keydown messages 
                    lv.Items[selNdx].Selected = true;
                    if ( selNdx >= lv.Items.Count ) {
                        selNdx = lv.Items.Count - 1;
                    }
                    lv.EnsureVisible(selNdx);
                    lv.FocusedItem = this.m_Panel.listview(side).Items[selNdx];
                } else {
                    if ( lv.Items.Count > 0 ) {
                        if ( lvsel.Count == 0 ) {
                            lv.SelectedIndices.Add(0);                                       // either unconditionally select something ...
                            lv.FocusedItem = this.m_Panel.listview(side).Items[0];
                        } else {
                            for ( int ndx = 0; ndx < lvsel.Count; ndx++ ) {                  // ... or restore previous selection 
                                lvi = this.m_Panel.FindListViewArrItemWithText(side, lvsel[ndx], -1);
                                if ( lvi != null ) {
                                    lv.SelectedIndices.Add((int)lvi.Tag);
                                    lv.FocusedItem = this.m_Panel.listview(side).Items[(int)lvi.Tag];
                                }
                            }
                        }
                    }
                }
            } else {
                if ( lv.Items.Count > 0 ) {
                    lv.SelectedIndices.Add(0);
                    lv.FocusedItem = this.m_Panel.listview(side).Items[0];
                } else {
                    this.m_Panel.RenderListviewLabel(side);                                   // should happen when drive is empty
                }
            }

            // keep selected item visible 
            if ( lv.SelectedIndices.Count > 0 ) {
                // 20161016: don't select [..] if there is more then one item selected
                if ( lv.SelectedIndices.Count > 1 ) {
                    if ( lv.Items[0].Selected ) {
                        lv.Items[0].Selected = false;
                    }
                }
                // keep first selected item visible
                int ndx = lv.SelectedIndices[0];
                lv.EnsureVisible(ndx);
            }

            this.m_dtLastLoad = DateTime.Now;                                              // block resize events when listview columns are automatically adjusted
            if ( this.listViewFitColumnsToolStripMenuItem.Checked ) {                      // show listview column widths depending on "fit" or "nofit"
                lv.EndUpdate();    // w/o this lv.ClientSize.Width won't be correct in case a vertical scrollbar is shown due to the number of items 
                lv.BeginUpdate();  //     - " -
                lv.Columns[1].Width = -2;
                lv.Columns[2].Width = TextRenderer.MeasureText(this.dummytext.Substring(0, this.m_Panel.maxLen2[(int)side, this.m_Panel.GetActiveTabIndex(side)]), lv.Font).Width;  // 
                lv.Columns[3].Width = TextRenderer.MeasureText(this.dummytext.Substring(0, this.m_Panel.maxLen3[(int)side, this.m_Panel.GetActiveTabIndex(side)]), lv.Font).Width;  //
                lv.Columns[0].Width = Math.Max(150, lv.ClientSize.Width - lv.Columns[1].Width - lv.Columns[2].Width);
                lv.Columns[7].Width = 0;
            }
            lv.EndUpdate();                                                                // allow paint
            this.m_dtLastLoad = DateTime.Now;                                              // block resize events when listview columns are automatically adjusted

            // adjust prompt & command line
            if ( side == this.m_Panel.GetActiveSide() ) {
                this.RenderCommandline(folder);
            }

            // 20160221: refresh computer view
            //            if ( (m_Panel.button(Side.left).Text == "Computer") || (m_Panel.button(Side.right).Text == "Computer") ) {
            this.timerRefeshComputerView.Interval = 5000;
            this.timerRefeshComputerView.Start();
            //            }

            // a bgw shall reload ListView, all items (even incl. exe icons, empty folders):
            //    a) if there are more items than m_iListViewLimit + 1 (+ 1 is "[..]")
            //    b) if network drive, which may lag when getting empty folder info
            this.m_bBlockListViewActivity = true;
            if ( data.Count == this.m_iListViewLimit + 1 || networkDrive ) {
                BackgroundWorker bg = new BackgroundWorker();
                bg.WorkerSupportsCancellation = true;
                bg.WorkerReportsProgress = true;
                bg.DoWork += new DoWorkEventHandler(this.FinishListView);
                bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.bg_RunWorkerCompleted);
                this.m_bgRunWorkerCompleted = false;
                bool bSlowDrive = (di == null) ? false : (di.DriveType != DriveType.Fixed);
                string theFolder = this.m_Panel.button(side).Tag.ToString();
                bg.RunWorkerAsync(new DoWorkFinishListViewArgs(side, null, selectItem, theFolder, 0, 0, this.m_ExtensionIconIndexArray, this.imageListLv, filter, bSlowDrive, this.highlightEmptyFolderToolStripMenuItem.Checked));
            } else {
                // 20160626: get exe icons in a background thread, only in case of "no final full load" (it already runs in bgw including exe icon files)
                // Why? icon extraction from exe files is sometimes very slow, up to 10s for GERW206 Download folder
                // subset of listview containing exe files only
                // deep clone is needed, simple copy would end with exception
                ListViewItem[] mi = Array.ConvertAll(Array.FindAll(data.ToArray(), x => x.Text.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase)), a => (ListViewItem)a.Clone());
                if ( (mi != null) && (mi.Length > 0) ) {
                    BackgroundWorker bg = new BackgroundWorker();
                    bg.DoWork += new DoWorkEventHandler(this.bg_GetExeIconWork);
                    bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.bg_GetExeIconCompleted);
                    bg.RunWorkerAsync(new GetExeIconArgs(mi, this.m_Panel.button(side).Tag.ToString(), this.imageListLv, side));
                }
                // show folder sizes 
                if ( this.listsShowFolderSizesToolStripMenuItem.Checked ) {
                    new Task(() => this.updateFolderSizes(side)).Start();
                }
                this.m_bBlockListViewActivity = false;
            }

            // ok
            int returnValue = 0;
            return returnValue;
        }
        //
        //
        // 20160626: get exe icons - args
        struct GetExeIconArgs {
            public GetExeIconArgs(ListViewItem[] lviArr, string path, ImageList imgLst, Side side) {
                this.lviArr = lviArr;
                this.path = path;
                this.imgLst = imgLst;
                this.side = side;
            }
            public ListViewItem[] lviArr;
            public string path;
            public ImageList imgLst;
            public Side side;
        }
        // 20160626: get exe icons - bg_DoWork
        void bg_GetExeIconWork(object sender, DoWorkEventArgs e) {
            // cast input
            GetExeIconArgs args = (GetExeIconArgs)e.Argument;

            // process input
            Parallel.For(1, args.lviArr.Length, i => {
                // default .exe file icon index
                int imageindex = 11;
                // get exe icon
                Icon icon = null;
                try {
                    icon = Icon.ExtractAssociatedIcon(Path.Combine(@args.path, args.lviArr[i].Text));
                } catch ( ArgumentException ) {
                    ;
                } catch ( Exception ) {
                    ;
                }
                if ( icon != null ) {
                    // add icon to global list
                    args.imgLst.Images.Add(icon);
                    // set icon index accordingly 
                    imageindex = args.imgLst.Images.Count - 1;
                }
                // set icon index for listview item accordingly
                args.lviArr[i].ImageIndex = imageindex;
            });

            // hand over modified input data to bg_GetExeIconCompleted
            e.Result = new GetExeIconArgs(args.lviArr, args.path, args.imgLst, args.side);
        }
        // 20160626: get exe icons - bg_DoWorkCompleted 
        void bg_GetExeIconCompleted(object sender, RunWorkerCompletedEventArgs e) {
            // cast input, contains the exe icon modified listview items
            GetExeIconArgs args = (GetExeIconArgs)e.Result;
            ListViewItem[] lviArr = args.lviArr;

            // hand over modified image icon list
            this.imageListLv = args.imgLst;

            // update final ListView by loop thru modified lviArr
            ListViewItem[] lviFin = this.m_Panel.GetListViewArr(args.side);
            if ( lviFin == null ) {
                return;
            }
            Parallel.For(1, args.lviArr.Length, i => {
                int ndx = Array.FindIndex(lviFin, o => o.Text == lviArr[i].Text);
                if ( ndx != -1 ) {
                    lviFin[ndx].ImageIndex = args.lviArr[i].ImageIndex;
                }
            });

            // redraw listview
            this.m_Panel.listview(args.side).Invalidate();
        }
        //
        //
        // 20160424: bgw helpers to load large lists in two steps (1st step: a small number of items , 2nd step: all items in a background thread) 
        // bgw start arguments structure
        // INPUT: string folder, int out m_Panel.maxLen2[(int)side], int out m_Panel.maxLen3[(int)side], m_ExtensionIconIndexArray, ref this.imageListLv, filter, di.DriveType != DriveType.Fixed, highlightEmptyFolderToolStripMenuItem.Checked);
        struct DoWorkFinishListViewArgs {
            public DoWorkFinishListViewArgs(Side side, ListViewItem[] lviarr, string selectitemtext, string path, int len2, int len3, string[] extIconArr, ImageList il, string filter, bool slowdrive, bool bhighlightemptyfolders) {
                this.Side = side;
                this.LviArr = lviarr;
                this.SelectItemText = selectitemtext;
                this.Path = path;
                this.Len2 = len2;
                this.Len3 = len3;
                this.extensionIconArr = extIconArr;
                this.Il = il;
                this.Filter = filter;
                this.SlowDrive = slowdrive;
                this.bHighlightEmptyFolders = bhighlightemptyfolders;
            }
            public Side Side;
            public ListViewItem[] LviArr;
            public string SelectItemText;
            public string Path;
            public int Len2;
            public int Len3;
            public string[] extensionIconArr;
            public ImageList Il;
            public string Filter;
            public bool SlowDrive;
            public bool bHighlightEmptyFolders;
        }
        // method is started from bgw
        void FinishListView(object sender, DoWorkEventArgs e) {
            // cast all args
            Side side = ((DoWorkFinishListViewArgs)e.Argument).Side;
            ListViewItem[] lviarr = ((DoWorkFinishListViewArgs)e.Argument).LviArr;
            string selectitemtext = ((DoWorkFinishListViewArgs)e.Argument).SelectItemText;
            string path = ((DoWorkFinishListViewArgs)e.Argument).Path;
            int len2 = 0; // ((DoWorkFinishListViewArgs)e.Argument).Len2;
            int len3 = 0; // ((DoWorkFinishListViewArgs)e.Argument).Len3;
            string[] extIconArr = ((DoWorkFinishListViewArgs)e.Argument).extensionIconArr;
            string filter = ((DoWorkFinishListViewArgs)e.Argument).Filter;
            ImageList il = ((DoWorkFinishListViewArgs)e.Argument).Il;
            bool slowdrive = ((DoWorkFinishListViewArgs)e.Argument).SlowDrive;
            bool bHighlightEmptyFolder = ((DoWorkFinishListViewArgs)e.Argument).bHighlightEmptyFolders;

            // reload listview - now for all items, but leave directories' empty status out
            lviarr = this.m_fff.FindFilesFolders(path,
                                             out len2,
                                             out len3,
                                             extIconArr,
                                             ref il,
                                             int.MaxValue,
                                             filter,
                                             slowdrive,
                                             false).ToArray();

            // if enabled, folder view looks better
            if ( bHighlightEmptyFolder ) {
                GrzTools.FastFileFind.WIN32_FIND_DATA fdata = new GrzTools.FastFileFind.WIN32_FIND_DATA();
                //GrzTools.FastFileFind.Win32FindData winFindData = new GrzTools.FastFileFind.Win32FindData();
                IntPtr findHandle = IntPtr.Zero;
                // 20160501: Parallel.For(..) of IsDirEmpty (aka FindFirst/FindNext) to obtain empty status of directories, gains up to 1000ms on winsxs of gerw206 compared to serial op 
                Parallel.For(1, lviarr.Length, i => {
                    if ( lviarr[i].ImageIndex == 3 ) {
                        //if ( GrzTools.FastFileFind.IsDirEmptyEx(Path.Combine(path, lviarr[i].SubItems[0].Text + "\\*"), winFindData, findHandle) ) {
                        if ( GrzTools.FastFileFind.IsDirEmpty(Path.Combine(path, lviarr[i].SubItems[0].Text + "\\*"), fdata, findHandle) ) {
                            lviarr[i].ImageIndex = 0;
                        }
                    }
                });
            }

            // transfer some of the returned data to "completed", which runs in the UI thread
            e.Result = new DoWorkFinishListViewArgs(side, lviarr, selectitemtext, path, 0, 0, null, il, "", false, false);
        }
        // bgw with FinishListView(..) is ready 
        void bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            // cast args
            ListViewItem[] lviarr = ((DoWorkFinishListViewArgs)e.Result).LviArr;
            this.imageListLv = ((DoWorkFinishListViewArgs)e.Result).Il;
            Side side = ((DoWorkFinishListViewArgs)e.Result).Side;
            string selectitemtext = ((DoWorkFinishListViewArgs)e.Result).SelectItemText;
            string path = ((DoWorkFinishListViewArgs)e.Result).Path;

            // since bgw could be very slow, the active path might have changed meanwhile and the results from here became useless
            if ( path != this.m_Panel.button(side).Tag.ToString() ) {
                this.m_bBlockListViewActivity = false;
                return;
            }

            // listview
            ListView lv = this.m_Panel.listview(side);

            // memorize current selection before the reload was done
            List<int> selLst = new List<int>();
            if ( lv.SelectedIndices.Count > 0 ) {
                foreach ( int ndx in lv.SelectedIndices ) {
                    selLst.Add(ndx);
                }
            }

            // selLst.Count > 1 overvotes selectitemtext: user might made a selection in the span between return from folder until bg_RunWorkerCompleted
            if ( selLst.Count > 1 ) {
                selectitemtext = "";
            }

            // get the first visible item 
            int firstVisibleIndex = lv.TopItem.Index;

            // stop UI updates
            lv.BeginUpdate();
            lv.Enabled = false;

            // set full list
            this.m_Panel.SetListViewArr(side, this.m_Panel.GetActiveTabIndex(side), lviarr);
            // set list count
            //lv.VirtualListSize = lviarr.Length;
            // sort list
            this.SortListView(side, side == Side.left ? this.m_Panel.LastSortedColumnLhs[this.m_Panel.GetActiveTabIndex(side)] : this.m_Panel.LastSortedColumnRhs[this.m_Panel.GetActiveTabIndex(side)], true);
            // block resize events
            this.m_dtLastLoad = DateTime.Now;

            // allow UI updates
            lv.Enabled = true;
            lv.EndUpdate();

            // selection in ListView
            lv.SelectedIndices.Clear();
            if ( selectitemtext.Length != 0 ) {
                // if returned from a subfolder, "selectitemtext" has a meaningful value
                ListViewItem lvi = this.m_Panel.FindListViewArrItemWithText(side, selectitemtext, -1);
                if ( lvi != null ) {
                    // subfolder was found
                    lv.Items[(int)lvi.Tag].Focused = true;  // important for keyup/keydown messages 
                    lv.Items[(int)lvi.Tag].Selected = true;
                    int selNdx = (int)lvi.Tag + 1;
                    if ( selNdx >= lv.Items.Count ) {
                        selNdx = lv.Items.Count - 1;
                    }
                    lv.EnsureVisible(selNdx);
                } else {
                    if ( lv.Items.Count > 0 ) {
                        lv.SelectedIndices.Add(0);
                    }
                }
            } else {
                if ( lv.Items.Count > 0 ) {
                    if ( selLst.Count > 0 ) {
                        // there was some selection before the reload was done
                        if ( selLst.Count == this.m_iListViewLimit ) {
                            // TRICK: that is a signal to select all (the op pushed + to select all items) --> function param 3 means unconditionally "select all"
                            this.SelectListViewItems(3);
                        } else {
                            // restore the selection before the reload was done
                            foreach ( int ndx in selLst ) {
                                lv.SelectedIndices.Add(ndx);
                            }
                        }
                    } else {
                        lv.SelectedIndices.Add(0);
                    }
                }
            }

            // always keep the previously first visible item visible even after sorting
            lv.FocusedItem = this.m_Panel.listview(side).Items[firstVisibleIndex];
            lv.EnsureVisible(firstVisibleIndex);

            // block resize events
            this.m_bBlockListViewActivity = false;
            this.m_dtLastLoad = DateTime.Now;

            // show folder sizes 
            if ( this.listsShowFolderSizesToolStripMenuItem.Checked ) {
                new Task(() => this.updateFolderSizes(side)).Start();
            }

            // 1501 limit is finally processed
            this.m_bgRunWorkerCompleted = true;

            //this.Text = DateTime.Now.ToString("HH:mm:ss ", CultureInfo.InvariantCulture) + m_sw.ElapsedMilliseconds.ToString() + "ms";
        }

        // the buttons (left/right) have their own context menu, this method jumps directly to "Computer"
        private void computerToolStripMenuItem_Click(object sender, EventArgs e) {
            // determine which button (left or right) is the owner of the ToolStripItem
            Control ctl = null;
            ToolStripItem item = (sender as ToolStripItem);
            if ( item != null ) {
                ContextMenuStrip owner = item.Owner as ContextMenuStrip;
                if ( owner != null ) {
                    ctl = owner.SourceControl;
                }
            }
            // once we know about the button who brought us here, we know the side too
            Side side = Side.left;
            if ( (ctl == this.buttonRight) || (ctl == this.tabControlRight) ) {
                side = Side.right;
            }
            // load Computer view
            this.LoadListView(side, "Computer", "");
            this.m_Panel.folders.InsertTopFolder(side, this.m_Panel.GetActiveTabIndex(side), "Computer");
        }
        // the buttons (left/right) have their own context menu: this method select a folder, which is later shown in listview - Alt-F1 / Alt-F2 / right click popup menu are calling the buttons left/right 
        private void selectFolderToolStripMenuItem_Click(object sender, EventArgs e) {
            Control ctl = null;
            ToolStripItem item = (sender as ToolStripItem);
            if ( item != null ) {
                ContextMenuStrip owner = item.Owner as ContextMenuStrip;
                if ( owner != null ) {
                    ctl = owner.SourceControl;
                }
            }
            this.buttonLeftRight_Click(ctl, null);
        }
        // "menu left" entry handler
        private void drivesToolStripMenuItem1_Click(object sender, EventArgs e) {
            this.buttonLeftRight_Click(this.buttonLeft, null);
        }
        // "menu right" entry handler
        private void drivesToolStripMenuItem_Click(object sender, EventArgs e) {
            this.buttonLeftRight_Click(this.buttonRight, null);
        }
        // click event handler for button (left, right) on top of the listviews
        async private void buttonLeftRight_Click(object sender, EventArgs e) {
            this.m_bRunSize = false;
            Side side = Side.left;
            if ( (sender == this.buttonRight) || (sender == this.tabControlRight) ) {
                side = Side.right;
            }
            this.m_Panel.SetActiveSide(side);
            string currentPath = this.m_Panel.button(side).Tag.ToString();
            this.RenderCommandline(currentPath);

            // question of taste: start treeview where you come from OR always start treeview from PC root
            this.m_sff.Text = "Select Folder or File";
            if ( !this.folderSelectStartsFromComputerToolStripMenuItem.Checked && GrzTools.FileTools.PathExists(currentPath, 500) ) {
                this.m_sff.DefaultPath = currentPath;
            } else {
                this.m_sff.DefaultPath = this.DRVC;
            }
            if ( this.m_sffNeedsRefresh ) {
                this.m_sff.RefreshRequest("mediachanged");
                this.m_sffNeedsRefresh = false;
            }

            if ( !this.m_sff.Visible ) { // exception in case the dlg is already visible, could happen when dlg is called from kbd (Alt+Up)
                DialogResult dlr = this.m_sff.ShowDialog(this);
                if ( dlr == System.Windows.Forms.DialogResult.OK ) {
                    // returned value from dialog
                    string path = this.m_sff.ReturnPath;
                    // special folder selected?
                    if ( path.StartsWith("%") && path.EndsWith("%") ) {
                        string envvar = path.ToLower().Substring(1, path.Length - 2);
                        path = Environment.GetEnvironmentVariable(envvar);
                        if ( !System.IO.Directory.Exists(path) ) {
                            return;
                        }
                    }
                    // go for it 
                    await this.LoadListView(side, path, "");
                    this.m_Panel.folders.InsertTopFolder(side, this.m_Panel.GetActiveTabIndex(side), path);
                }
            }
        }
        // a right click shall activate the corresponding listview - opening the context menu happens automatically via the buttons' "ContextMenuStrip" property 
        private void buttonLeftRight_Down(object sender, MouseEventArgs e) {
            if ( e.Button == MouseButtons.Right ) {
                Side side = Side.left;
                if ( (Button)sender == this.buttonRight ) {
                    side = Side.right;
                }
                this.m_Panel.SetActiveSide(side);
            }
        }
        // the buttons (left/right) have their own context menu, this method shows properties matching to the shown content
        private void toolStripMenuItemProperties_Click(object sender, EventArgs e) {
            string path = this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString();
            if ( path == "Shared Folders" ) {
                Process.Start("fsmgmt.msc");
            } else {
                if ( path == "Computer" ) {
                    Process.Start("control", "system");
                } else {
                    GrzTools.FileTools.ShowFileProperties(path);
                }
            }
        }
        // the buttons (left/right) have their own context menu, this method copies the current path into clipboard
        private void toolStripMenuItemClipboard_Click(object sender, EventArgs e) {
            System.Windows.Forms.Clipboard.SetText(this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString());
        }

        // CD Rom status helper
        [DllImport("winmm.dll", EntryPoint = "mciSendString")]
        public static extern int mciSendStringA(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);
        private int CDRomDoubleClick(string driveName) {
            string driveLetter = driveName.Substring(0, 1);
            DriveInfo di = new DriveInfo(driveLetter);

            if ( (di.DriveType == DriveType.CDRom) && (di.Name.Contains(driveName)) ) {
                if ( !di.IsReady ) {
                    // CD not ready
                    CDOpenClose cddlg = new CDOpenClose();
                    DialogResult dr = cddlg.ShowDialog();
                    if ( dr == DialogResult.Yes ) {
                        // CD open & exit
                        string returnString = "";
                        mciSendStringA("open " + driveLetter + ": type CDAudio alias drive" + driveLetter, returnString, 0, 0);
                        mciSendStringA("set drive" + driveLetter + " door open", returnString, 0, 0);
                        mciSendStringA("open", returnString, 0, 0);
                        mciSendStringA("Set CDAudio Door Open", returnString, 0, 0);
                        return -1;
                    }
                    if ( dr == DialogResult.No ) {
                        // CD close & exit
                        string returnString = "";
                        mciSendStringA("open " + driveLetter + ": type CDaudio alias drive" + driveLetter, null, 0, 0);
                        mciSendStringA("set drive" + driveLetter + " door closed", null, 0, 0);
                        mciSendStringA("Set CDAudio Door Closed", returnString, 0, 0);
                        return -1;
                    }
                    if ( dr == DialogResult.Cancel ) {
                        // just exit
                        return -1;
                    }
                } else {
                    // CD is ready
                    using ( CustomMB dlg = new CustomMB("ROM Drive " + driveName + " is ready", "'Use Drive' to explore it, 'Open Tray' to replace the ROM or 'Cancel' for doing nothing.", "Use Drive", "Open Tray") ) {
                        if ( dlg.ShowDialog() != DialogResult.Cancel ) {
                            if ( dlg.ReturnValue == CustomMB.ReturnCustomMB.option1 ) {
                                // doing nothing explores the drive
                            }
                            if ( dlg.ReturnValue == CustomMB.ReturnCustomMB.option2 ) {
                                // CD open for replacement & exit
                                mciSendStringA("open " + driveLetter + ": type CDAudio alias drive" + driveLetter, null, 0, 0);
                                mciSendStringA("set drive" + driveLetter + " door open", null, 0, 0);
                                return -1;
                            }
                        } else {
                            return -1;
                        }
                    }
                }
            }

            // if there is nothing to expand anymore, so we could return the selected item and close the dialog 
            return 0;
        }
        // listview context menu entry, which opens the current selection by simulating a "double click" for the selection 
        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            object realSender = this.m_Panel.listview(this.m_Panel.GetActiveSide());
            this.listViewLeftRight_DoubleClick(realSender, null);
        }
        // double click event on listview item: in short, it should "open" whatever is selected - meaning of "open" varies depending on the context 
        async private void listViewLeftRight_DoubleClick(object sender, EventArgs e) {
            // what side was clicked? 
            Side side = Side.left;
            if ( sender == this.m_listViewR ) {
                side = Side.right;
            }
            // is there any selection?
            if ( (this.m_Panel.listview(side)).SelectedIndices.Count == 0 ) {
                return;
            }
            // perhaps no data?
            if ( this.m_Panel.GetListViewArr(side) == null ) {
                return;
            }

            // What item shall be treated here?
            int pos = this.m_Panel.listview(side).SelectedIndices[0];
            ListViewItem lvi = this.m_Panel.GetListViewArr(side)[pos];
            string selection = lvi.Text;                                           // per enter key from command line
            if ( e != null ) {                                                     // per mouse
                ListView lve = (ListView)sender;
                Point localPoint = lve.PointToClient(Cursor.Position);
                lvi = this.listviewGetItemAt(lve, localPoint);
                selection = lvi.Text;
            }

            // was perhaps a link to a folder clicked?
            if ( selection.EndsWith(".lnk", StringComparison.InvariantCultureIgnoreCase) ) {
                string fullpath = @Path.Combine((this.m_Panel.button(side)).Tag.ToString(), selection);
                string linkpath = GetLnkTarget(fullpath);                // this is a win32 specific thing
                if ( GrzTools.FileTools.PathExists(linkpath, 500) ) {    // we only treat a link to a directory
                    this.m_Panel.SetSymLink(side, this.m_Panel.GetActiveTabIndex(side), fullpath);                  // SymLink stores, where a lnk-directory was originally called from
                    await this.LoadListView(side, linkpath, "");
                    this.m_Panel.folders.InsertTopFolder(side, this.m_Panel.GetActiveTabIndex(side), linkpath);     // memorize folder change
                    return;
                }
            }

            // this is our "one level up folder", name and imageindex (=2) must match, because "[..]" is a legit filename  
            if ( (selection == "[..]") && (lvi.ImageIndex == 2) ) { // aka "LevelUp"
                this.ListviewOneLevelUp(side);
                return;
            }

            // regular cfw [folder] was selected --> generate new list
            if ( (lvi.ImageIndex == 3) || (lvi.ImageIndex == 0) ) {
                string folder = @Path.Combine((this.m_Panel.button(side)).Tag.ToString(), selection);
                // unconditionally reset the stored symlink folder
                this.m_Panel.SetSymLink(side, this.m_Panel.GetActiveTabIndex(side), "");
                // load listview
                await this .LoadListView(side, folder, "");
                // memorize folder change
                this.m_Panel.folders.InsertTopFolder(side, this.m_Panel.GetActiveTabIndex(side), folder);
                return;
            }

            // file was selected --> execute it 
            if ( (lvi.ImageIndex == 1) || (lvi.ImageIndex > 10) ) {
                string file = @Path.Combine((this.m_Panel.button(side)).Tag.ToString(), selection);
                ProcessStartInfo pi = new ProcessStartInfo(file);
                //                pi.Arguments = Path.GetFileName(file);               // <-- this line prevents coldeyed.exe from starting
                if ( (ModifierKeys & Keys.Alt) == Keys.Alt ) {
                    SimpleInput dlg = new SimpleInput();
                    dlg.Text = "Start argument";
                    dlg.Hint = "Provide a start argument for " + Path.GetFileName(file);
                    dlg.Input = "";
                    dlg.ShowDialog();
                    if ( dlg.DialogResult == DialogResult.OK ) {
                        pi.Arguments = dlg.Input;
                    }
                }
                pi.UseShellExecute = true;
                pi.WorkingDirectory = Path.GetDirectoryName(file);
                pi.FileName = file;
                pi.Verb = "OPEN";
                //                Environment.CurrentDirectory = pi.WorkingDirectory;  // not needed, because pi.WorkingDirectory works if pi.UseShellExecute is set to true
                try {
                    Process.Start(pi);
                } catch ( Exception ) {
                    string args = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll");
                    args += ",OpenAs_RunDLL " + file;
                    Process.Start("rundll32.exe", args);
                }
                return;
            }

            // anything else than file/folder/[..]/.lnk was selected: should be drive from the drive list
            if ( (new[] { 4, 5, 6, 7, 9, 10 }).Contains(lvi.ImageIndex) ) {
                // special treatment for mapped but not accessible network shares
                if ( lvi.ImageIndex == 9 ) {
                    GrzTools.AutoMessageBox.Show("Destination chosen is not accessible:\n\n" + selection, "Note", 2000);
                    return;
                }
                // special treatment for CD ROM: in case it returns -1, we shall wait and not re load the listview
                if ( lvi.ImageIndex == 6 ) {
                    if ( this.CDRomDoubleClick(selection) == -1 ) {
                        return;
                    }
                }
                // unconditionally reset the stored symlink folder
                this.m_Panel.SetSymLink(side, this.m_Panel.GetActiveTabIndex(side), "");
                // load listview
                if ( await this.LoadListView(side, selection, "") == -1 ) {
                    return;
                }
                // memorize folder change
                if ( selection != "Network" ) {
                    this.m_Panel.folders.InsertTopFolder(side, this.m_Panel.GetActiveTabIndex(side), selection);
                    return;
                }
            }

            // supposedly this won't happen at all unless something weird went wrong
            GrzTools.AutoMessageBox.Show("listViewLeftRight_DoubleClick() unexpected run thru", "Tell grzwolf", 5000);
        }
        // get info from lnk-file: works only in conjunction with Reference --> Add --> ..\windows\system32\Shell32.dll
        public static string GetLnkTarget(string lnkPath) {
            Shell32.Shell shl = new Shell32.Shell();
            lnkPath = System.IO.Path.GetFullPath(lnkPath);
            Shell32.Folder dir = shl.NameSpace(System.IO.Path.GetDirectoryName(lnkPath));
            Shell32.FolderItem itm = dir.Items().Item(System.IO.Path.GetFileName(lnkPath));
            Shell32.ShellLinkObject lnk = (Shell32.ShellLinkObject)itm.GetLink;
            return lnk.Target.Path;
        }
        // listview goes one level up
        async void ListviewOneLevelUp(Side side) {
            this.m_bRunSize = false;
            string folder = "";
            string selectItem = "";
            string basis = "";
            if ( (this.m_Panel.GetSymLink(side, this.m_Panel.GetActiveTabIndex(side)) != null) && (this.m_Panel.GetSymLink(side, this.m_Panel.GetActiveTabIndex(side)) != "") ) {
                // if m_Panel.SymLink is not empty, then we surely come back from a lnk-directory
                folder = Path.GetDirectoryName(this.m_Panel.GetSymLink(side, this.m_Panel.GetActiveTabIndex(side)));
                selectItem = Path.GetFileName(this.m_Panel.GetSymLink(side, this.m_Panel.GetActiveTabIndex(side)));
                // unconditionally reset the stored symlink folder to empty
                this.m_Panel.SetSymLink(side, this.m_Panel.GetActiveTabIndex(side), "");
            } else {
                // this is the normal "level up" sequence
                basis = (this.m_Panel.button(side)).Tag.ToString();
                // UNC path AND local machine: we switch to local file system when start folder == sharename
                if ( basis.StartsWith("\\\\" + System.Environment.MachineName) ) {
                    int count = basis.Split('\\').Length - 1;
                    if ( count == 3 ) {
                        string share = basis.Substring(basis.LastIndexOf("\\") + 1);
                        basis = this.GetSharedFolderLocalPath(share);
                    }
                }
                // get the last part of a directory as input for LoadListView() to select this item
                folder = Path.GetDirectoryName(basis);
                selectItem = Path.GetFileName(basis);
            }
            // selectItem == "" we are in root
            if ( (selectItem != null) && (selectItem.Length > 0) ) {
                if ( await this.LoadListView(side, folder, selectItem) == -1 ) {
                    folder = this.DRVC;
                    this.LoadListView(side, folder, "");
                }
                // memorize folder change: if selectItem is "", then we are in root
                this.m_Panel.folders.InsertTopFolder(side, this.m_Panel.GetActiveTabIndex(side), folder);
            } else {
                if ( this.m_Panel.button(side).Text == "Computer" ) {
                    // show FileFolder dialog
                    this.buttonLeftRight_Click(this.m_Panel.button(side), null);
                } else {
                    // show drives info
                    this.LoadListView(side, "Computer", basis);
                    this.m_Panel.folders.InsertTopFolder(side, this.m_Panel.GetActiveTabIndex(side), "Computer");
                }
            }
        }

        // refresh computer view
        private void timerRefeshComputerView_Tick(object sender, EventArgs e) {
            this.timerRefeshComputerView.Stop();
            this.timerRefeshComputerView.Interval = 5000;

            // if not in ComputerView there's nothing to do anymore, so start timer and exit
            if ( (this.m_Panel.button(Side.left).Text != "Computer") && (this.m_Panel.button(Side.right).Text != "Computer") ) {
                getWPD(ref this.m_WPD);
                this.timerRefeshComputerView.Start();
                return;
            }

            // call came from Menu --> Refresh
            string selectItem = "";
            if ( sender == null ) {
                selectItem = "::fullreset::";
            }

            // start backgroundworker 
            BackgroundWorker bg = new BackgroundWorker();
            bg.DoWork += new DoWorkEventHandler(bg_DoWorkGetDrives);
            bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.bg_DoWorkJustGetDrivesCompleted);
            bg.RunWorkerAsync(new DoWorkGetDrivesArgs(Side.none, selectItem, this.computerShowsFolderSizesToolStripMenuItem.Checked, this.GetBasicComputerViewList(true), null, false, this.m_WPD));

            // DEBUG: stopwatch
            //m_sw = Stopwatch.StartNew();
        }
        // bgw Completed: here we obtain just a new list for comparison
        void bg_DoWorkJustGetDrivesCompleted(object sender, RunWorkerCompletedEventArgs e) {
            // casting
            bool bSize = ((DoWorkGetDrivesArgs)e.Result).SizeInfo;
            string selectItem = ((DoWorkGetDrivesArgs)e.Result).SelectItem;
            if ( selectItem == "::fullreset::" ) {
                sender = null;
            }

            // we build a temporary new list of all ListViewItem belonging to ComputerView
            List<ListViewItem> newList = new List<ListViewItem>();

            // cast all computer items returned from BackGroundWorker
            newList.AddRange(((DoWorkGetDrivesArgs)e.Result).BasicList);
            newList.AddRange(((DoWorkGetDrivesArgs)e.Result).ListViewItems);

            // network in computer view
            string[] strarr = new string[8] { "", "", "", "", "", "", "", "" };
            strarr[0] = "Network";
            strarr[1] = "";
            strarr[2] = "";
            strarr[3] = "";
            strarr[4] = "";
            strarr[5] = "";
            strarr[6] = "";
            ListViewItem lvi = new ListViewItem(strarr, 5);
            newList.Add(lvi);

            // treat lhs & rhs
            if ( this.m_Panel.button(Side.left).Text == "Computer" ) {
                this.RefreshComputer(Side.left, newList, sender, bSize);
            }
            if ( this.m_Panel.button(Side.right).Text == "Computer" ) {
                this.RefreshComputer(Side.right, newList, sender, bSize);
            }

            // restart refresh timer
            this.timerRefeshComputerView.Interval = 5000;
            this.timerRefeshComputerView.Start();

            // debug output
            //this.Text = DateTime.Now.ToString("HH:mm:ss ", CultureInfo.InvariantCulture) + m_sw.ElapsedMilliseconds.ToString() + "ms";
        }
        // refresh Computer listview
        private void RefreshComputer(Side side, List<ListViewItem> newList, object sender, bool bSize) {
            // check new vs. current list
            List<ListViewItem> curList = this.m_Panel.GetListViewArr(side).ToList();
            bool bRefresh = false;
            // if count of items mismatches --> refresh
            if ( newList.Count == curList.Count ) {
                for ( int ndx = 0; ndx < newList.Count; ndx++ ) {
                    // if free or size mismatch --> refesh
                    if ( (newList[ndx].SubItems[2].Text != curList[ndx].SubItems[2].Text) || (newList[ndx].SubItems[1].Text != curList[ndx].SubItems[1].Text) ) {
                        bRefresh = true;
                        break;
                    }
                }
            } else {
                bRefresh = true;
            }
            // refresh list & restore old selection
            if ( bRefresh ) {
                string selectitem = "";
                if ( this.m_Panel.listview(side).SelectedIndices.Count == 1 ) {
                    selectitem = this.m_Panel.GetListViewArr(side)[this.m_Panel.listview(side).SelectedIndices[0]].Text;
                }
                this.m_bSizing = true;                               // selected items don't flicker while refresh, same as like form changes size
                this.LoadDrivesList(side, selectitem, true, false);  // 3rd param == sizeinfo; 4th param == "First Show Flag" == true --> netdrives are skipped (GetDrives() hangs on not available netshares) 
                this.m_bSizing = false;                              // selected items don't flicker while refresh, same as like form changes size
            }
            // computer listview refresh/reload via menu, this shall reset any selection - even it it is not needed
            if ( sender == null ) {
                this.m_Panel.listview(side).SelectedIndices.Clear();
                this.m_Panel.listview(side).SelectedIndices.Add(0);
            }
        }

        // GetComputerItems() helper: special folder sizes
        public static long GetWSHFolderSize(string Fldr) {
            IWshRuntimeLibrary.FileSystemObject FSO = new IWshRuntimeLibrary.FileSystemObject();
            long FldrSize = 0;
            try {
                // supposed to be the fastest method to obtain a folder size BUT not working on "Documents" --> catch calls GrzTools.FastFileFind.FileSizes
                FldrSize = (long)FSO.GetFolder(Fldr).Size;
            } catch ( Exception ) {
                // this method works on Documents of all users (elevated) or current user - BUT is much slower
                bool run = true;
                GrzTools.FastFileFind.FileSizes(ref run, Fldr, ref FldrSize);
            }
            Marshal.FinalReleaseComObject(FSO);

            return FldrSize;
        }
        // GetComputerItems() helper: translate a local share name into its file system path
        string GetSharedFolderLocalPath(string sharename) {
            string ret = "";
            try {
                ManagementObjectSearcher Win32Share = new ManagementObjectSearcher("SELECT Path FROM Win32_share WHERE Name = '" + sharename + "'");
                foreach ( ManagementObject ShareData in Win32Share.Get() ) {
                    ret = (String)ShareData["Path"];
                }
            } catch ( Exception ) {
                ;
            }
            return ret;
        }
        // return listview items with drive info
        private void LoadDrivesList(Side side, string selectItem, bool bSizeInfo, bool bFirstShow) {
            this.timerRefeshComputerView.Stop();

            // this way we make sure, we are in CompterView - which we will check again in RenderComputerView ==> once it returns from a hanging drive, we can simply discard these ComputerView data  
            this.m_Panel.button(side).Text = "Computer";

            // get ComputerView basic datalist once, we will re use it during the ProgressChanged event
            List<ListViewItem> basic = this.GetBasicComputerViewList(bSizeInfo);

            // start backgroundworker 
            BackgroundWorker bg = new BackgroundWorker();
            bg.WorkerReportsProgress = true;
            bg.DoWork += new DoWorkEventHandler(bg_DoWorkGetDrives);
            bg.ProgressChanged += new ProgressChangedEventHandler(this.bg_ProgressChangedGetDrives);
            bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.bg_DoWorkGetDrivesCompleted);
            bg.RunWorkerAsync(new DoWorkGetDrivesArgs(side, selectItem, bSizeInfo, basic, null, bFirstShow, this.m_WPD));
        }
        // bgw start arguments structure
        struct DoWorkGetDrivesArgs {
            public DoWorkGetDrivesArgs(Side side, string selectItem, bool bSizeInfo, List<ListViewItem> basic, List<ListViewItem> lst, bool bFirstShow, List<WPD> wpd) {
                this.Side = side;
                this.SelectItem = selectItem;
                this.SizeInfo = bSizeInfo;
                this.ListViewItems = lst;
                this.BasicList = basic;
                this.FirstShow = bFirstShow;
                this.Wpd = wpd;
            }
            public Side Side;
            public bool SizeInfo;
            public string SelectItem;
            public List<ListViewItem> ListViewItems;
            public List<ListViewItem> BasicList;
            public bool FirstShow;
            public List<WPD> Wpd;
        }
        // bgw DoWork
        static void bg_DoWorkGetDrives(object sender, DoWorkEventArgs e) {
            // the worker thread
            BackgroundWorker worker = (BackgroundWorker)sender;

            // list with basic items as shown in ComputerVieww
            List<ListViewItem> basic = ((DoWorkGetDrivesArgs)e.Argument).BasicList;
            // empty list with presumable variable items
            List<ListViewItem> retList = new List<ListViewItem>();
            List<WPD> wpd = ((DoWorkGetDrivesArgs)e.Argument).Wpd;

            // get all drives of the PC system: keeping the network drives at the end of the list, allows us to show other drives immediately AND slow/delayed/hanging network drives whenever they are ready
            List<DriveInfo> diList = DriveInfo.GetDrives().ToList();
            int lstCount = diList.Count;
            for ( int i = 0; i < lstCount; i++ ) {
                // put network drives at the end of the list
                if ( diList[i].DriveType == DriveType.Network ) {
                    diList.Add(diList[i]);
                    diList.RemoveAt(i);
                    i--;
                    lstCount--;
                }
            }
            DriveInfo[] di = diList.ToArray();


            // loop DriveInfo array, TBD: Would it work in a real parallel for? How to sync the final results?
            Stopwatch sw = Stopwatch.StartNew();
            foreach ( DriveInfo drive in di ) {

                // simple defaults: 4 = fixed drive
                int imageindex = 4;
                string[] strarr = new string[8] { "", "", "", "", "", "", "", "" };

                // 20160312: we simply skip all not pingable network drives, because if they are not available, GetDrives() will hang 
                if ( drive.DriveType == DriveType.Network ) {
                    if ( !GrzTools.Network.PingNetDriveOk(drive.Name/*.Substring(0, drive.Name.Length - 1)*/) ) {
                        strarr[0] = drive.Name.Substring(0, drive.Name.Length - 1);
                        imageindex = 9;
                        ListViewItem lvi = new ListViewItem(strarr, imageindex);
                        retList.Add(lvi);
                        continue;
                    }
                }

                try {

                    strarr[0] = drive.Name.Substring(0, drive.Name.Length - 1);
                    if ( drive.IsReady ) {
                        strarr[1] = GrzTools.StringTools.SizeSuffix(drive.TotalSize);
                        strarr[2] = GrzTools.StringTools.SizeSuffix(drive.TotalFreeSpace);
                        strarr[3] = Math.Round(100f * drive.TotalFreeSpace / drive.TotalSize).ToString() + "%";
                        strarr[4] = drive.DriveFormat.ToString();
                        strarr[5] = drive.VolumeLabel;
                        strarr[6] = GrzTools.clsDiskInfoEx.GetFirstPhysicalDriveString(strarr[0]); // 20160221: show physical drive
                    }
                    if ( drive.DriveType == DriveType.Removable ) {
                        if ( drive.IsReady ) {
                            imageindex = 7;
                        } else {
                            imageindex = 9;
                        }
                    }
                    if ( drive.DriveType == DriveType.Network ) {
                        imageindex = 5;
                        try {
                            strarr[6] = GrzTools.Network.LocalToUNC(strarr[0]);                    // 20160221: show network mapping
                        } catch ( Exception ex ) {
                            strarr[6] = ex.Message;
                        }
                        if ( !drive.IsReady ) {
                            imageindex = 9;
                        }
                    }
                    if ( drive.DriveType == DriveType.CDRom ) {
                        imageindex = 6;
                    }
                    ListViewItem lvi = new ListViewItem(strarr, imageindex);
                    retList.Add(lvi);

                    if ( sw.ElapsedMilliseconds > 50 ) {
                        // show bgw progress: what list items we got so far - if app hangs due to network delays (NAS is pingable but HDD motor is off), we show at least something AND UI is responsive
                        retList = retList.OrderBy(o => o.Text).ToList();
                        DoWorkGetDrivesArgs progressResult = new DoWorkGetDrivesArgs(((DoWorkGetDrivesArgs)e.Argument).Side, ((DoWorkGetDrivesArgs)e.Argument).SelectItem, ((DoWorkGetDrivesArgs)e.Argument).SizeInfo, basic, retList, false, wpd);
                        worker.ReportProgress(0, progressResult);
                        Application.DoEvents();
                    }
                    sw.Restart();


                } catch ( Exception ex ) {
                    strarr[6] = ex.Message;
                }
            }

            // take care about WPD
            List<ListViewItem> lst = getWPD(ref wpd);
            retList.AddRange(lst);

            // forward the final listview items to bgw "Completed"
            retList = retList.OrderBy(o => o.Text).ToList();
            DoWorkGetDrivesArgs result = new DoWorkGetDrivesArgs(((DoWorkGetDrivesArgs)e.Argument).Side, ((DoWorkGetDrivesArgs)e.Argument).SelectItem, ((DoWorkGetDrivesArgs)e.Argument).SizeInfo, basic, retList, false, wpd);
            e.Result = result;
        }
        static List<ListViewItem> getWPD(ref List<WPD> wpd) {
            List<ListViewItem> list = new List<ListViewItem>();
            // we need to be able to distinguish between WPD and USB-Drives
            List<string> deviceIDs = new List<string>();
            ManagementObjectSearcher theSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive WHERE InterfaceType='USB'");
            foreach ( ManagementObject currentObject in theSearcher.Get() ) {
                string id = currentObject["PNPDeviceID"].ToString();
                int ndx = id.LastIndexOf('\\');
                if ( ndx != -1 ) {
                    id = id.Substring(ndx + 1).ToLower();
                }
                deviceIDs.Add(id);
            }
            // get all WPD
            wpd.Clear();
            PortableDevices.PortableDeviceCollection collection = new PortableDevices.PortableDeviceCollection();
            collection.Refresh();
            foreach ( PortableDevices.PortableDevice device in collection ) {
                bool idFound = false;
                device.Connect();
                foreach ( string id in deviceIDs ) {
                    if ( device.DeviceId.Contains(id) ) {
                        idFound = true;
                    }
                }
                if ( !idFound ) {
                    wpd.Add(new WPD(device, device.DeviceModel, ""));
                    ListViewItem lvi = new ListViewItem(new string[8] { device.DeviceModel, "", "", "", "", "", "", "" }, 10);
                    list.Add(lvi);
                }
                device.Disconnect();
            }
            return list;
        }
        // bgw show Progress
        void bg_ProgressChangedGetDrives(object sender, ProgressChangedEventArgs e) {
            // casting user state arguments
            Side side = ((DoWorkGetDrivesArgs)e.UserState).Side;
            bool bSize = ((DoWorkGetDrivesArgs)e.UserState).SizeInfo;
            string selectItem = ((DoWorkGetDrivesArgs)e.UserState).SelectItem;
            List<ListViewItem> retList = ((DoWorkGetDrivesArgs)e.UserState).ListViewItems;
            List<ListViewItem> cvList = ((DoWorkGetDrivesArgs)e.UserState).BasicList;
            // show what we got so far
            this.RenderComputerView(side, cvList, retList, selectItem, bSize);
            Application.DoEvents();
        }
        // bgw Completed
        void bg_DoWorkGetDrivesCompleted(object sender, RunWorkerCompletedEventArgs e) {
            // casting result arguments
            Side side = ((DoWorkGetDrivesArgs)e.Result).Side;
            bool bSize = ((DoWorkGetDrivesArgs)e.Result).SizeInfo;
            string selectItem = ((DoWorkGetDrivesArgs)e.Result).SelectItem;
            List<ListViewItem> retList = ((DoWorkGetDrivesArgs)e.Result).ListViewItems;
            List<ListViewItem> cvList = ((DoWorkGetDrivesArgs)e.Result).BasicList;
            // show final list
            this.RenderComputerView(side, cvList, retList, selectItem, bSize);
            // start ComputerView auto refresh
            this.timerRefeshComputerView.Interval = bSize ? 5000 : 1;
            this.timerRefeshComputerView.Start();
        }
        List<ListViewItem> GetBasicComputerViewList(bool bSize) {
            bool bShowSize = bSize && this.computerShowsFolderSizesToolStripMenuItem.Checked;
            List<ListViewItem> newList = new List<ListViewItem>();
            string[] strarr = new string[8] { "", "", "", "", "", "", "", "" };

            // Desktop, Downloads, Documents
            strarr[0] = "Desktop";
            strarr[6] = GrzTools.FileTools.TranslateSpecialFolderNames(strarr[0]);
            strarr[1] = bShowSize ? GrzTools.StringTools.SizeSuffix(GetWSHFolderSize(strarr[6])) : "";
            ListViewItem lvi = new ListViewItem(strarr, 3);
            newList.Add(lvi);
            strarr[0] = "Downloads";
            strarr[6] = GrzTools.FileTools.TranslateSpecialFolderNames(strarr[0]);
            strarr[1] = bShowSize ? GrzTools.StringTools.SizeSuffix(GetWSHFolderSize(strarr[6])) : "";
            lvi = new ListViewItem(strarr, 3);
            newList.Add(lvi);
            strarr[0] = "Documents";
            strarr[6] = GrzTools.FileTools.TranslateSpecialFolderNames(strarr[0]);
            strarr[1] = bShowSize ? GrzTools.StringTools.SizeSuffix(GetWSHFolderSize(strarr[6])) : "";
            lvi = new ListViewItem(strarr, 3);
            newList.Add(lvi);

            // display an entry point for local shared folders
            if ( this.computerShowsShareFoldersToolStripMenuItem.Checked ) {
                strarr[0] = "Shared Folders";
                strarr[1] = bShowSize ? GrzTools.StringTools.SizeSuffix(GetSharedFoldersSizes()) : "";
                strarr[6] = "";
                lvi = new ListViewItem(strarr, 3);
                newList.Add(lvi);
            }

            return newList;
        }
        void RenderComputerView(Side side, List<ListViewItem> cvList, List<ListViewItem> retList, string selectItem, bool bSize) {
            string currentPath = this.m_Panel.button(side).Text;
            if ( currentPath != "Computer" ) {
                return;
            }

            List<ListViewItem> newList = new List<ListViewItem>();

            try {
                // add computer view items, we don't care if one of the lists is empty or even null
                newList.AddRange(cvList);
                newList.AddRange(retList);
            } catch {; }

            // network in computer view
            string[] strarr = new string[8] { "", "", "", "", "", "", "", "" };
            strarr[0] = "Network";
            strarr[1] = "";
            strarr[2] = "";
            strarr[3] = "";
            strarr[4] = "";
            strarr[5] = "";
            strarr[6] = "";
            ListViewItem lvi = new ListViewItem(strarr, 5);
            newList.Add(lvi);

            // all about the new listview content
            this.m_dtLastLoad = DateTime.Now;                                                     // block resize events when listview columns are automatically adjusted
            this.m_Panel.button(side).BackColor = SystemColors.Control;                           // set button text color to normal
            this.m_Panel.button(side).Tag = "Computer";                                           // set button Tag according to folder 
            this.m_Panel.SetListPath(side, this.m_Panel.GetActiveTabIndex(side), "Computer", false);
            this.setTabControlText(side, this.m_Panel.GetActiveTabIndex(side), "Computer");
            this.m_Panel.SetButtonText(side, "Computer", "*.*");                                  // set button Text according to folder 

            ListView lv = this.m_Panel.listview(side);                                            // the listview we deal with
            lv.BeginUpdate();                                                                // block paint  
            this.m_Panel.SetListViewArr(side, this.m_Panel.GetActiveTabIndex(side), newList.ToArray());// vm - we store the data not in the list itself
            //lv.VirtualListSize = newList.Count;                                              // vm - the list needs to know, how large it is 
            lv.SelectedIndices.Clear();                                                      // vm - reset any selection previously made  
            lv.Columns[0].Text = "Storage";
            lv.Columns[1].Text = "Size";
            lv.Columns[1].TextAlign = HorizontalAlignment.Right;
            lv.Columns[2].Text = "Free";
            lv.Columns[2].TextAlign = HorizontalAlignment.Right;
            lv.Columns[3].Text = "Free %";
            lv.Columns[3].TextAlign = HorizontalAlignment.Right;
            lv.Columns[4].Text = "FS";
            lv.Columns[5].Text = "Name";
            lv.Columns[6].Text = "Location";
            if ( lv.Items.Count > 0 ) {
                if ( selectItem.Length > 2 ) {                                               // in case we return from a drive, then we want to select it 
                    if ( (selectItem[1] == ':') && (selectItem[2] == '\\') ) {
                        selectItem = selectItem.Substring(0, 2);
                    }
                }
                lvi = this.m_Panel.FindListViewArrItemWithText(side, selectItem, -1);
                if ( lvi != null ) {                                                         // if possible, select the item, where we came from   
                    int selNdx = (int)lvi.Tag;
                    lv.Items[selNdx].Focused = true;                                         // important for keyup/keydown messages 
                    lv.Items[selNdx].Selected = true;
                    if ( selNdx >= lv.Items.Count ) {
                        selNdx = lv.Items.Count - 1;
                    }
                    lv.EnsureVisible(selNdx);
                    lv.FocusedItem = this.m_Panel.listview(side).Items[selNdx];
                } else {
                    lv.AutoResizeColumns(ColumnHeaderAutoResizeStyle.None);                  // tbd: ohne diese Zeile läßt sich kein Item auswählen (übernächste Zeile)
                    lv.EnsureVisible(0);                                                     // initially scroll to top
                    lv.SelectedIndices.Add(0);
                }
            }
            this.m_Panel.RenderListviewLabel(side);                                                       // should happen when drive is empty

            this.m_dtLastLoad = DateTime.Now;                                                     // block resize events when listview columns are automatically adjusted
            lv.Columns[0].Width = -2;                                                        // set column widths matching to the content, but col 3
            lv.Columns[1].Width = -2;
            lv.Columns[2].Width = -2;
            lv.Columns[3].Width = 90;
            lv.Columns[4].Width = -2;
            lv.Columns[5].Width = -2;
            lv.Columns[6].Width = -2;
            lv.Columns[7].Width = 0;

            lv.EndUpdate();                                                                  // allow paint
        }

        // helper: space consumption of local shared folders
        public static long GetSharedFoldersSizes() {
            long size = 0;
            /*
                        // serial op
                        ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from win32_share");
                        foreach ( ManagementObject share in searcher.Get() ) {
                            string type = share["Type"].ToString();
                            if ( type == "0" ) {
                                string @path = @share["Path"].ToString();
                                long cursize = GetWSHFolderSize(@path);
                                size += cursize;
                            }
                        }
            */
            // parallel op only gains a 30-40ms compared to serial (i3770k Jan 2016), perhaps it's even slower when I have only 1 shared folder
            List<Task<long>> tasks = new List<Task<long>>();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from win32_share");
            foreach ( ManagementObject share in searcher.Get() ) {
                string type = share["Type"].ToString();
                if ( type == "0" ) {
                    string @path = @share["Path"].ToString();
                    Task<long> t = new Task<long>(() => GetWSHFolderSize(@path));
                    t.Start();
                    tasks.Add(t);
                }
            }
            do {
                Application.DoEvents();
                for ( int i = 0; i < tasks.Count; i++ ) {
                    if ( tasks[i].IsCompleted ) {
                        size += tasks[i].Result;
                        tasks.RemoveAt(i);
                    }
                }
            } while ( tasks.Count > 0 );

            return size;
        }
        // helper: a list of shared folders
        public static List<ListViewItem> GetSharedFolders() {
            List<ListViewItem> retList = new List<ListViewItem>();
            string[] strarr = new string[8] { "[..]", "", "", "", "", "", "", "" };
            int imageindex = 2;  // aka "LevelUp"
            ListViewItem lvi = new ListViewItem(strarr, imageindex);
            retList.Add(lvi);

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from win32_share");
            foreach ( ManagementObject share in searcher.Get() ) {
                string type = share["Type"].ToString();
                // 0 = DiskDrive (1 = Print Queue, 2 = Device, 3 = IPH)
                if ( type == "0" ) {
                    string name = share["Name"].ToString();       //getting share name
                    string path = share["Path"].ToString();       //getting share path
                    string caption = share["Caption"].ToString(); //getting share description
                    strarr[0] = path;
                    strarr[1] = name;
                    strarr[2] = caption;
                    strarr[3] = "";
                    strarr[4] = "";
                    strarr[5] = "";
                    strarr[6] = "";
                    imageindex = 3;
                    lvi = new ListViewItem(strarr, imageindex);
                    retList.Add(lvi);
                }
            }
            return retList;
        }
        // display listview with all local shared folders
        private void LoadSharedFolders(Side side) {
            List<ListViewItem> retList = new List<ListViewItem>();

            retList.AddRange(GetSharedFolders());

            //
            // all about the new listview content
            //
            this.m_dtLastLoad = DateTime.Now;                                                       // block resize events when listview columns are automatically adjusted
            this.m_Panel.button(side).BackColor = SystemColors.Control;                             // set button text color to normal
            this.m_Panel.button(side).Tag = "Shared Folders";                                       // set button Tag according to folder 
            this.m_Panel.SetListPath(side, this.m_Panel.GetActiveTabIndex(side), "Shared Folders", false);
            this.setTabControlText(side, this.m_Panel.GetActiveTabIndex(side), "Shared Folders");
            this.m_Panel.SetButtonText(side, "Shared Folders", "*.*");                              // set button Text according to folder 
            ListView lv = this.m_Panel.listview(side);                                              // the listview we deal with
            lv.BeginUpdate();                                                                  // block paint  

            lv.SelectedIndices.Clear();                                                        // vm - reset any selection previously made  
            this.m_Panel.SetListViewArr(side, this.m_Panel.GetActiveTabIndex(side), retList.ToArray());  // vm - we store the data not in the list itself
            //lv.VirtualListSize = retList.Count;                                                // vm - the list needs to know, how large it is 

            lv.Columns[0].Text = "Location";                                                   // change headers
            lv.Columns[1].Text = "Share Name";
            lv.Columns[1].TextAlign = HorizontalAlignment.Left;
            lv.Columns[2].Text = "Caption";
            lv.Columns[2].TextAlign = HorizontalAlignment.Left;
            lv.Columns[3].Text = "";
            lv.Columns[3].TextAlign = HorizontalAlignment.Left;
            lv.Columns[3].Width = 0;
            lv.Columns[4].Text = "";
            lv.Columns[4].Width = 0;
            lv.Columns[5].Text = "";
            lv.Columns[5].Width = 0;
            lv.Columns[6].Text = "";
            lv.Columns[6].Width = 0;
            lv.Columns[7].Width = 0;

            this.listViewFitColumns(side);
            //listViewFitColumnsToolStripMenuItem_Click(null, null);                         // resize listview columns

            if ( lv.Items.Count > 0 ) {
                lv.EnsureVisible(0);                                                       // initially scroll to top
                lv.SelectedIndices.Add(0);                                                 // select something
            }

            this.m_Panel.RenderListviewLabel(side);                                                     // prompt and label

            lv.EndUpdate();                                                                // allow paint
            this.m_dtLastLoad = DateTime.Now;                                                   // block resize events when listview columns are automatically adjusted
        }

        // it actually starts the "select folder dialog" configured for network shares
        private void LoadNetworkList(Side side) {
            // select network folder
            this.m_sff.Text = "Select Network Folder";
            this.m_sff.DefaultPath = "Network";
            DialogResult dlr = this.m_sff.ShowDialog(this);
            if ( dlr != System.Windows.Forms.DialogResult.OK ) {
                return;
            }
            string sSelectedNetworkPath = this.m_sff.ReturnPath;

            if ( System.IO.Directory.Exists(sSelectedNetworkPath) ) {
                this.LoadListView(side, sSelectedNetworkPath, "");
                this.m_Panel.folders.InsertTopFolder(side, this.m_Panel.GetActiveTabIndex(side), sSelectedNetworkPath);
            }
        }

        // sort any listview
        private readonly string _strAscending = "  \x25b3";
        private readonly string _strDscending = "  \x25bd";
        void SortListView(Side lvSide, int iSortColumn, bool bFirstShow) {
            if ( (iSortColumn < 0) || (iSortColumn >= this.m_Panel.listview(lvSide).Columns.Count) ) {
                return;
            }
            //            m_Panel.listview(lvSide).BeginUpdate();

            // save current selection of listview items in a list of strings - and clear all selection
            ListView lvView = this.m_Panel.listview(lvSide);
            List<string> selectionList = new List<string>();
            foreach ( int ndx in lvView.SelectedIndices ) {
                selectionList.Add(this.m_Panel.GetListViewArr(lvSide)[ndx].Text);
            }
            lvView.SelectedIndices.Clear();

            // Determine whether clicked column is already the column that is being sorted AND listview was not yet already shown
            int lsc = lvSide == Side.left ? this.m_Panel.LastSortedColumnLhs[this.m_Panel.GetActiveTabIndex(lvSide)] : this.m_Panel.LastSortedColumnRhs[this.m_Panel.GetActiveTabIndex(lvSide)];
            if ( (iSortColumn == lsc) && !bFirstShow ) {
                // reverse the sort direction for this column
                if ( this.m_lvwColumnSorter[(int)lvSide][iSortColumn].Order == SortOrder.Ascending ) {
                    this.m_lvwColumnSorter[(int)lvSide][iSortColumn].Order = SortOrder.Descending;
                } else {
                    this.m_lvwColumnSorter[(int)lvSide][iSortColumn].Order = SortOrder.Ascending;
                }
            } else {
                // if a) another column is selected or b) listview is shown first time ---> set the column number that is to be sorted and don't touch sort order
                this.m_lvwColumnSorter[(int)lvSide][iSortColumn].SortColumn = iSortColumn;
            }
            this.m_lvwColumnSorter[(int)lvSide][iSortColumn].SortExtension = false;

            // memorize the last sorted column
            if ( lvSide == Side.left ) {
                this.m_Panel.LastSortedColumnLhs[this.m_Panel.GetActiveTabIndex(lvSide)] = iSortColumn;
            }
            if ( lvSide == Side.right ) {
                this.m_Panel.LastSortedColumnRhs[this.m_Panel.GetActiveTabIndex(lvSide)] = iSortColumn;
            }

            // show "sort order sign" in column text
            for ( int i = 0; i < lvView.Columns.Count; i++ ) {
                lvView.Columns[i].Text = (string)lvView.Columns[i].Tag;
            }
            if ( iSortColumn == 0 ) {
                if ( lvSide == Side.left ) {
                    if ( this.CHLtoolStripMenuItem_File.Checked ) {
                        lvView.Columns[iSortColumn].Text = lvView.Columns[iSortColumn].Tag + (this.m_lvwColumnSorter[(int)lvSide][iSortColumn].Order == SortOrder.Ascending ? this._strAscending : this._strDscending);
                    } else {
                        string str = Localizer.GetString("sortextension");
                        lvView.Columns[iSortColumn].Text = str + (this.m_lvwColumnSorter[(int)lvSide][iSortColumn].Order == SortOrder.Ascending ? this._strAscending : this._strDscending);
                        this.m_lvwColumnSorter[(int)lvSide][iSortColumn].SortExtension = true;
                    }
                } else {
                    if ( this.CHRtoolStripMenuItem_File.Checked ) {
                        lvView.Columns[iSortColumn].Text = lvView.Columns[iSortColumn].Tag + (this.m_lvwColumnSorter[(int)lvSide][iSortColumn].Order == SortOrder.Ascending ? this._strAscending : this._strDscending);
                    } else {
                        string str = Localizer.GetString("sortextension");
                        lvView.Columns[iSortColumn].Text = str + (this.m_lvwColumnSorter[(int)lvSide][iSortColumn].Order == SortOrder.Ascending ? this._strAscending : this._strDscending);
                        this.m_lvwColumnSorter[(int)lvSide][iSortColumn].SortExtension = true;
                    }
                }
            } else {
                lvView.Columns[iSortColumn].Text = lvView.Columns[iSortColumn].Tag + (this.m_lvwColumnSorter[(int)lvSide][iSortColumn].Order == SortOrder.Ascending ? this._strAscending : this._strDscending);
            }

            // Perform the sort with these new sort options
            try {
                ListViewItem[] lvarr = this.m_Panel.GetListViewArr(lvSide);
                Array.Sort(lvarr, this.m_lvwColumnSorter[(int)lvSide][iSortColumn]);
            } catch {; }

            // keep [..] always on top of the list
            ListViewItem found = this.m_Panel.FindListViewArrItemWithText(lvSide, "[..]", 2);
            if ( found != null ) {
                List<ListViewItem> newLst = this.m_Panel.GetListViewArr(lvSide).ToList();
                newLst.RemoveAt((int)found.Tag);
                newLst.Insert(0, new ListViewItem(new string[] { "[..]", " ", "<PARENT>", " ", "", "", "", "0" }, 2));
                this.m_Panel.SetListViewArr(lvSide, this.m_Panel.GetActiveTabIndex(lvSide), newLst.ToArray());
            }

            // restore original selection
            foreach ( string s in selectionList ) {
                ListViewItem lvi = this.m_Panel.FindListViewArrItemWithText(lvSide, s, -1);
                if ( lvi != null ) {
                    lvView.SelectedIndices.Add((int)lvi.Tag);
                }
            }

            // ensure first selected item is visible
            if ( lvView.SelectedIndices.Count > 0 ) {
                lvView.EnsureVisible(lvView.SelectedIndices[0]);
            }

            //            m_Panel.listview(lvSide).EndUpdate();

            // needed to reflect sorting
            lvView.Invalidate(true);
        }
        // sort listviews by clicking its column headers
        private void listViewLeftRight_ColumnClick(object sender, ColumnClickEventArgs e) {
            // current side
            Side side = (ListView)sender == this.m_listViewL ? Side.left : Side.right;
            // don't sort if we at "Computer" level
            if ( (this.m_Panel.button(side).Text == "Computer") || (this.m_Panel.button(side).Text == "Shared Folder") ) {
                return;
            }
            // block resize events
            this.m_dtLastLoad = DateTime.Now;
            // sort listview                          
            this.SortListView(side, e.Column, false);
        }

        private void commandToolStripMenuItem_DropDownOpening(object sender, EventArgs e) {
            // admin mode or not
            if ( (ModifierKeys & Keys.Shift) == Keys.Shift ) {
                this.runCmdexeHereToolStripMenuItem1.Image = null;
                this.runCmdexeHereToolStripMenuItem1.Text = this.runCmdexeHereToolStripMenuItem1.Tag.ToString() + " (user)";
                this.linkCmdexeHereToolStripMenuItem.Image = null;
                this.linkCmdexeHereToolStripMenuItem.Text = this.linkCmdexeHereToolStripMenuItem.Tag.ToString() + " (user)";
            } else {
                this.runCmdexeHereToolStripMenuItem1.Image = cfw.Properties.Resources.restartAsAdministratorToolStripMenuItem_Image;
                this.runCmdexeHereToolStripMenuItem1.Text = this.runCmdexeHereToolStripMenuItem1.Tag.ToString() + " (admin)";
                this.linkCmdexeHereToolStripMenuItem.Image = cfw.Properties.Resources.restartAsAdministratorToolStripMenuItem_Image;
                ;
                this.linkCmdexeHereToolStripMenuItem.Text = this.linkCmdexeHereToolStripMenuItem.Tag.ToString() + " (admin)";
            }
        }

        void processAltPPreview() {
            // event is always called twice, we need to suppress the 2nd message
            if ( (DateTime.Now - this.m_dtDebounce).Milliseconds < 30 ) {
                return;
            }
            // what side was clicked? 
            Side side = this.m_Panel.GetActiveSide();
            // exec preview
            if ( side == Side.right ) {
                if ( this.PreviewFileLeftToolStripMenuItem.Checked ) {
                    this.PreviewFileLeftToolStripMenuItem.Checked = false;
                } else {
                    this.PreviewFileLeftToolStripMenuItem.Checked = true;
                }
                this.PreviewFileLeftToolStripMenuItem_Click(null, null);
            } else {
                if ( this.PreviewFileRightToolStripMenuItem.Checked ) {
                    this.PreviewFileRightToolStripMenuItem.Checked = false;
                } else {
                    this.PreviewFileRightToolStripMenuItem.Checked = true;
                }
                this.PreviewFileRightToolStripMenuItem_Click(null, null);
            }
            this.m_dtDebounce = DateTime.Now;
        }

        // win32
        readonly int LVM_GETHEADER = 4127;
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);
        // win32
        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(Point pt);
        // IMessageFilter
        public bool PreFilterMessage(ref System.Windows.Forms.Message m) {
            // Alt + visible contextMenuStripListItems --> shall start win explorer context menu
            if ( (ModifierKeys == Keys.Alt) && this.contextMenuStripListItems.Visible ) {
                this.contextMenuStripListItems.Close();
                this.moreToolStripMenuItem_Click(null, null);
                return true;
            }

            // Alt-P --> show Preview for currently selected file: this way it should work with both syskeydown and syskeyup !!(m.Msg == 261) prevents PrtScreen from firing!!
            if ( (ModifierKeys == Keys.Alt) && ((Keys)m.WParam == Keys.P) && (m.Msg == 261) ) {
                this.processAltPPreview();
                return false;
            }

            // Alt-S --> show a Window with shortcuts
            if ( (ModifierKeys == Keys.Alt) && ((Keys)m.WParam == Keys.S) ) {
                Form fc = Application.OpenForms["ShowShortcuts"];
                if ( fc == null ) {
                    // generate a new one in case it's not existing
                    this.m_frm = new ShowShortcuts(this);
                }
                if ( !this.m_frm.Visible ) {
                    // a debouncer is needed as long as KeyUp events are not fully handled
                    if ( (DateTime.Now - this.m_dtDebounce).Milliseconds < 30 ) {
                        // show it if invisible and keep handled = true
                        this.m_frm.Show();
                        return true;
                    }
                    this.m_dtDebounce = DateTime.Now;
                }
                // handled = false to allow the Form ShowShortcuts to handle the key by itself
                return false;
            }

            // wm_keydown || wm_syskeydown                     wm_syskeyup
            if ( (m.Msg == 0x0100) || (m.Msg == 0x0104) /*|| (m.Msg == 0x0105)*/ ) {
                //
                // handle all Alt+<key> events
                //
                if ( ModifierKeys == Keys.Alt ) {
                    // if simple list of folder history is visible, we stop processing all other ALt+<key> events
                    if ( (this.m_sl != null) && (this.m_sl.Visible) ) {
                        return true;
                    }
                    // if m_sff is visible, we stop processing all other ALt+<key> events
                    if ( this.m_sff.Visible ) {
                        return true;
                    }
                    //
                    // Alt-Down --> show history
                    //
                    if ( (Keys)m.WParam == Keys.Down ) {
                        // event is always called twice, we need to suppress the 2nd message
                        if ( (DateTime.Now - this.m_dtDebounce).Milliseconds < 30 ) {
                            return true;
                        }
                        // what side was clicked? 
                        Side side = this.m_Panel.GetActiveSide();
                        // exec
                        this.ShowFolderList(side);
                        this.m_dtDebounce = DateTime.Now;
                        // mark key processing as done/final
                        return true;
                    }
                    //
                    // Alt-Up --> go one level up
                    //
                    if ( (Keys)m.WParam == Keys.Up ) {
                        // event is always called twice, we need to suppress the 2nd message
                        if ( (DateTime.Now - this.m_dtDebounce).Milliseconds < 30 ) {
                            return true;
                        }
                        // what side was clicked? 
                        Side side = this.m_Panel.GetActiveSide();
                        // exec
                        if ( side == Side.right ) {
                            this.buttonRhsUp_Click(null, null);
                        } else {
                            this.buttonLhsUp_Click(null, null);
                        }
                        this.m_dtDebounce = DateTime.Now;
                        // mark key processing as done/final
                        return true;
                    }
                    //
                    // Alt-left --> go back in history
                    //
                    if ( (Keys)m.WParam == Keys.Left ) {
                        // event is always called twice, we need to suppress the 2nd message
                        if ( (DateTime.Now - this.m_dtDebounce).Milliseconds < 30 ) {
                            return true;
                        }
                        // what side was clicked? 
                        Side side = this.m_Panel.GetActiveSide();
                        // exec
                        if ( side == Side.right ) {
                            this.buttonRhsPrev_Click(null, null);
                        } else {
                            this.buttonLhsPrev_Click(null, null);
                        }
                        this.m_dtDebounce = DateTime.Now;
                        // mark key processing as done/final
                        return true;
                    }
                    //
                    // Alt-right --> go forward in history
                    //
                    if ( (Keys)m.WParam == Keys.Right ) {
                        // event is always called twice, we need to suppress the 2nd message
                        if ( (DateTime.Now - this.m_dtDebounce).Milliseconds < 30 ) {
                            this.m_dtDebounce = DateTime.Now;
                            return true;
                        }
                        // what side was clicked? 
                        Side side = this.m_Panel.GetActiveSide();
                        // exec
                        if ( side == Side.right ) {
                            this.buttonRhsNext_Click(null, null);
                        } else {
                            this.buttonLhsNext_Click(null, null);
                        }
                        this.m_dtDebounce = DateTime.Now;
                        // mark key processing as done/final
                        return true;
                    }
                    //
                    // Alt-home --> goto Computer view
                    //
                    if ( (Keys)m.WParam == Keys.Home ) {
                        // event is always called twice, we need to suppress the 2nd message
                        if ( (DateTime.Now - this.m_dtDebounce).Milliseconds < 30 ) {
                            this.m_dtDebounce = DateTime.Now;
                            return true;
                        }
                        // what side was clicked? 
                        Side side = this.m_Panel.GetActiveSide();
                        // load Computer view
                        this.LoadListView(side, "Computer", "");
                        this.m_Panel.folders.InsertTopFolder(side, this.m_Panel.GetActiveTabIndex(side), "Computer");
                        this.m_dtDebounce = DateTime.Now;
                        // mark key processing as done/final
                        return true;
                    }
                    //
                    // Alt-1/2/3/4/5 --> change to tab on active side
                    //
                    if ( ((Keys)m.WParam >= Keys.D1) && ((Keys)m.WParam <= Keys.D5) ) {
                        int tabIndex = m.WParam.ToInt32() - 49;
                        Side side = this.m_Panel.GetActiveSide();
                        if ( side == Side.left ) {
                            this.tabControlLeft.SelectedIndex = tabIndex;
                        }
                        if ( side == Side.right ) {
                            this.tabControlRight.SelectedIndex = tabIndex;
                        }
                        this.m_Panel.SetActiveSide(side);
                        return true;
                    }
                }
            }

            // wm_keydown
            if ( m.Msg == 0x0100 ) {
                if ( (ModifierKeys & Keys.Shift) == Keys.Shift ) {
                    // distinguish between: Admin mode for 'run cmd.exe' menu item
                    if ( this.runCmdexeHereToolStripMenuItem1.Visible ) {
                        if ( this.runCmdexeHereToolStripMenuItem1.Image != null ) {
                            this.commandToolStripMenuItem.HideDropDown();
                            this.commandToolStripMenuItem.ShowDropDown();
                        }
                    }
                    // distinguish between: new file AND new folder in MainForm "file menu"
                    if ( this.newFolderToolStripMenuItem.Visible ) {
                        if ( this.newFolderToolStripMenuItem.Text.StartsWith("New Folder") ) {
                            this.toolStripMenuItem3.HideDropDown();
                            this.toolStripMenuItem3.ShowDropDown();
                        }
                    }
                    // context menu
                    if ( this.deleteSelectionF8ToolStripMenuItem.Visible ) {
                        if ( this.deleteSelectionF8ToolStripMenuItem.Text == "Delete to Trash" ) {
                            this.contextMenuStripListItems.Hide();
                            this.contextMenuStripListItems.Show();
                        }
                    }
                    // create shortcut
                    if ( this.linkOnDesktopToolStripMenuItem.Visible ) {
                        if ( this.linkOnDesktopToolStripMenuItem.Text == "Link to Desktop" ) {
                            this.contextMenuStripListItems.Hide();
                            this.contextMenuStripListItems.Show();
                        }
                    }
                }
            }
            // wm_keyup
            if ( m.Msg == 0x0101 ) {
                if ( (Keys)m.WParam == Keys.ShiftKey ) {
                    // distinguish between: Admin mode for 'run cmd.exe' menu item
                    if ( this.runCmdexeHereToolStripMenuItem1.Visible ) {
                        this.commandToolStripMenuItem.HideDropDown();
                        this.commandToolStripMenuItem.ShowDropDown();
                    }
                    // distinguish between: new file AND new folder in MainForm "file menu"
                    if ( this.newFolderToolStripMenuItem.Visible ) {
                        this.toolStripMenuItem3.HideDropDown();
                        this.toolStripMenuItem3.ShowDropDown();
                    }
                    // context menu
                    if ( this.deleteSelectionF8ToolStripMenuItem.Visible ) {
                        this.contextMenuStripListItems.Hide();
                        this.contextMenuStripListItems.Show();
                    }
                }
            }

            // left pen/touch down
            if ( m.Msg == 0x246 ) {
                object sender;
                IntPtr hWnd = WindowFromPoint(Control.MousePosition);
                if ( hWnd == this.m_listViewL.Handle ) {
                    sender = this.m_listViewL;
                } else {
                    sender = this.m_listViewR;
                }
                Point pos = ((ListView)sender).PointToClient(Cursor.Position);
            }
            // left pen/touch up
            if ( m.Msg == 0x247 ) {
                object sender;
                IntPtr hWnd = WindowFromPoint(Control.MousePosition);
                if ( hWnd == this.m_listViewL.Handle ) {
                    sender = this.m_listViewL;
                } else {
                    sender = this.m_listViewR;
                }
                Point pos = ((ListView)sender).PointToClient(Cursor.Position);
            }

            // any 0x0200 mouse move outside of the FlyingLabel m_fldlg shall make it disappear
            if ( m.Msg == 0x0200 ) {
                // 99.9999% of all the time m_fldlg is Disposed, therefore this MouseMove doesn't do anything
                if ( !this.m_fldlg.IsDisposed ) {
                    IntPtr hWnd = WindowFromPoint(Control.MousePosition);
                    if ( this.m_fldlg.LabelHandle != hWnd ) {
                        this.closeFlyingLabel();
                    }
                }
                // 20161016: stop listview auto scroll when dragging files coming from outside back into one of the listviews
                if ( this.m_bListViewAutoScroll ) {
                    IntPtr hWnd = WindowFromPoint(Control.MousePosition);
                    if ( (hWnd == this.m_listViewL.Handle) || (hWnd == this.m_listViewR.Handle) ) {
                        this.m_bListViewAutoScroll = false;
                    }
                }
            }

            // right mouse down LISTVIEW: open context menus (listview and header) and activate listview at position
            if ( m.Msg == 0x204 ) {
                // we only want to treat listviews and their headers: this routine is called too, when a right click happens on any other control
                IntPtr hWnd = WindowFromPoint(Control.MousePosition);
                IntPtr hhl = (IntPtr)SendMessage(this.m_listViewL.Handle, this.LVM_GETHEADER, false, 0);
                IntPtr hhr = (IntPtr)SendMessage(this.m_listViewR.Handle, this.LVM_GETHEADER, false, 0);
                if ( (hWnd != this.m_listViewL.Handle) && (hWnd != this.m_listViewR.Handle) && (hWnd != hhl) && (hWnd != hhr) ) {
                    return false;
                }
                // activate clicked listview 
                if ( hWnd == this.m_listViewL.Handle ) {
                    this.m_Panel.SetActiveSide(Side.left);
                } else {
                    if ( hWnd == this.m_listViewR.Handle ) {
                        this.m_Panel.SetActiveSide(Side.right);
                    } else {
                        // right mouse down on LISTVIEW COLUMNHEADER opens column header context menu
                        if ( hWnd == hhl ) {
                            if ( this.m_Panel.button(Side.left).Text != "Computer" ) {
                                this.colheadLHS_contextMenuStrip.Show(MousePosition);
                            }
                            return false;
                        } else {
                            if ( hWnd == hhr ) {
                                if ( this.m_Panel.button(Side.right).Text != "Computer" ) {
                                    this.colheadRHS_contextMenuStrip.Show(MousePosition);
                                }
                                return false;
                            }
                        }
                    }
                }

                // Change item selection to the item where the mouse came down, ONLY if one single item is selected. This keeps multiple selections. 
                ListView lv = this.m_Panel.GetActiveView();
                if ( lv.SelectedIndices.Count == 1 ) {
                    Point pt = lv.PointToClient(MousePosition);
                    ListViewItem item = this.listviewGetItemAt(lv, pt);
                    if ( item != null ) {
                        int ndx = lv.SelectedIndices[0];
                        lv.Items[ndx].Selected = false;     // side effect: clears lv.SelectedIndices
                        item.Selected = true;               // side effect: adds item index to lv.SelectedIndices
                        int ndxNew = lv.SelectedIndices[0]; // proof
                    }
                }

                // open one of our selfmade context menus
                if ( this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString() == "Computer" ) {
                    this.m_bKeepFocused = true;
                    this.contextMenuStripComputerView.Show(MousePosition);
                } else {
                    this.contextMenuStripListItems.Show(MousePosition);
                }
                return true;
            }

            // forward mouse wheel messages to the window, they belong to
            if ( m.Msg == 0x20a ) {
                IntPtr hWnd = WindowFromPoint(Control.MousePosition);
                Point pos = GrzTools.WinAPIHelper.GetPoint(m.LParam);
                if ( hWnd != IntPtr.Zero && hWnd != m.HWnd && Control.FromHandle(hWnd) != null ) {
                    PostMessage(hWnd, (uint)m.Msg, m.WParam, m.LParam);
                    return true;
                }
            }

            return false;
        }
        public void MouseRightDownEvent(MouseEventArgs e) {
            IntPtr hWnd = GrzTools.FindWindow.FindWindowWithText("Copy");
            if ( hWnd == IntPtr.Zero ) {
                hWnd = GrzTools.FindWindow.FindWindowWithText("Move");
            }
            if ( hWnd == IntPtr.Zero ) {
                hWnd = GrzTools.FindWindow.FindWindowWithText("Delete");
            }

            IntPtr btn = GrzTools.FindWindow.FindChildWindowWithText(hWnd, "Ok");
            if ( (hWnd != IntPtr.Zero) && (btn != IntPtr.Zero) ) {
                PostMessage(btn, 0x0201, IntPtr.Zero, IntPtr.Zero);
                PostMessage(btn, 0x0202, IntPtr.Zero, IntPtr.Zero);
            }
        }

        // save settings to ini
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            // kill running cmd.exe 
            if ( this.m_process != null ) {
                if ( !this.m_process.HasExited ) {
                    this.m_inputWriter.WriteLine("\x3");
                    this.m_inputWriter.Flush();
                    this.m_process.Kill();
                    this.m_process.Close();
                    this.m_process = null;
                    // unsubscribe messages
                    this.m_outputWorker.DoWork -= this.outputWorker_DoWork;
                    this.m_outputWorker.ProgressChanged -= this.outputWorker_ProgressChanged;
                    this.m_errorWorker.DoWork -= this.errorWorker_DoWork;
                    this.m_errorWorker.ProgressChanged -= this.errorWorker_ProgressChanged;
                    // stop bgws
                    this.m_outputWorker.CancelAsync();
                    this.m_errorWorker.CancelAsync();
                }
            }

            // INI: prepare write 
            GrzTools.IniFile ini = new GrzTools.IniFile(System.Windows.Forms.Application.ExecutablePath + ".ini");
            // INI: ListView Limit
            ini.IniWriteValue("cfw", "LimitListView", this.m_iListViewLimit.ToString());
            // INI: listview font size
            string str = this.m_listViewL.Font.Size.ToString("0.00");
            ini.IniWriteValue("cfw", "Font ListView", str);
            // INI: cfw start position & window size
            str = this.Location.X.ToString();
            ini.IniWriteValue("cfw", "StartPosition X", str);
            str = this.Location.Y.ToString();
            ini.IniWriteValue("cfw", "StartPosition Y", str);
            str = this.Size.Width.ToString();
            ini.IniWriteValue("cfw", "StartPosition Width", str);
            str = this.Size.Height.ToString();
            ini.IniWriteValue("cfw", "StartPosition Height", str);
            // INI: splitter position
            ini.IniWriteValue("cfw", "Splitter Position", Math.Min(this.m_iSplitterPosition, this.splitContainer1.SplitterDistance).ToString());
            // INI: preview file extensions
            ini.IniWriteValue("cfw", "doc", this.m_bDoc.ToString());
            ini.IniWriteValue("cfw", "img", this.m_bImg.ToString());
            ini.IniWriteValue("cfw", "zip", this.m_bZip.ToString());
            ini.IniWriteValue("cfw", "pdf", this.m_bPdf.ToString());
            ini.IniWriteValue("cfw", "htm", this.m_bHtm.ToString());
            ini.IniWriteValue("cfw", "asi", this.m_bAsIs.ToString());
            ini.IniWriteValue("cfw", "wmpAudio", this.m_bWmpAudio.ToString());
            ini.IniWriteValue("cfw", "wmpVideo", this.m_bWmpVideo.ToString());
            ini.IniWriteValue("cfw", "cfwVideo", this.m_bCfwVideo.ToString());
            // Computer at folder select
            ini.IniWriteValue("cfw", "ComputerAtFolderSelect", this.folderSelectStartsFromComputerToolStripMenuItem.Checked.ToString());
            // htm connect to folder
            ini.IniWriteValue("cfw", "ConnectHtmWithItsFilesFolder", this.connectHtmWithItsFilesFolderToolStripMenuItem.Checked.ToString());
            // show shared folders
            ini.IniWriteValue("cfw", "SharedFolders", this.computerShowsShareFoldersToolStripMenuItem.Checked.ToString());
            // show folders sizes
            ini.IniWriteValue("cfw", "ComputerFoldersSizes", this.computerShowsFolderSizesToolStripMenuItem.Checked.ToString());
            ini.IniWriteValue("cfw", "ListsFoldersSizes", this.listsShowFolderSizesToolStripMenuItem.Checked.ToString());
            // INI: automatic network can 
            ini.IniWriteValue("cfw", "AutoNetworkScan", this.autoNetworkScanToolStripMenuItem.Checked.ToString());
            // INI: highlight empty folders
            ini.IniWriteValue("cfw", "HighlightEmptyFolders", this.highlightEmptyFolderToolStripMenuItem.Checked.ToString());

            // INI: localization
            ini.IniWriteValue("cfw", "local", Thread.CurrentThread.CurrentUICulture.ToString());

            // write tabs
            for ( int i = 0; i < 5; i++ ) {
                ini.IniWriteValue("cfw", "rTab" + i.ToString(), this.m_Panel.GetListPath(Side.right, i));
            }
            for ( int i = 0; i < 5; i++ ) {
                ini.IniWriteValue("cfw", "lTab" + i.ToString(), this.m_Panel.GetListPath(Side.left, i));
            }
            ini.IniWriteValue("cfw", "rTabIndex", this.tabControlRight.SelectedIndex.ToString());
            ini.IniWriteValue("cfw", "lTabIndex", this.tabControlLeft.SelectedIndex.ToString());

            // folder history for all tabs and listviews
            string fldFile = System.Windows.Forms.Application.ExecutablePath + ".fld";
            if ( this.m_Panel.folders.MaintainFolderHistory ) {
                Panel.WriteToBinaryFile(fldFile, this.m_Panel.folders);
            } else {
                File.Delete(fldFile);
            }

            // listviews in tabs or not
            ini.IniWriteValue("cfw", "ListsInTabs", this.listsInTabsToolStripMenuItem.Checked.ToString());

            // active side
            str = this.m_Panel.GetActiveSide().ToString();
            ini.IniWriteValue("cfw", "ActiveSide", str);

            // cmd is connected to m_process
            try {
                if ( this.m_process != null ) {
                    this.m_process.Kill();
                }
            } catch {; }
        }

        // listview with style 'View.List only' is chosen: if text is hit then all is fine; here we look for a valid item at smaller x-position (== shorter text) 
        ListViewItem listviewGetItemAt(ListView lv, Point pt) {
            // return null; // lv.Items[0];

            ListViewItem item = lv.GetItemAt(pt.X, pt.Y);
            if ( item == null ) {
                if ( lv.View == View.List ) {
                    int xmax = Math.Min(lv.ClientSize.Width % lv.Columns[0].Width, lv.Columns[0].Width);
                    int x = 0;
                    do {
                        x++;
                        item = lv.GetItemAt(pt.X - x, pt.Y);
                    } while ( (item == null) && (x < xmax) );
                }
            }
            return item;
        }

        // select listview items with left mouse button
        bool m_bActivateList;                                   // select: indicator for listview activation, when mouse is coming from the other listview 
        bool m_bSelectRule;                                     // select: that is the rule how to select mouse moved items
        bool m_bStatusBeforeMouseDown;                          // select: selection status of item befor mouse went down 
        Point m_ptMove = new Point(0, 0);                       // select: mouse position after fake 4 pixel mouse move   
        int[] m_sic = new int[1];                               // memorize selection: in case it gets lost (after click into empty region), we can restore it   
        enum Direction { na, up, down };
        class mouseMoveSelect                                   // 
        {
            public Point prevMousePos;
            public Point buttonDownMousePos;
            public Direction moveDirection = Direction.na;
            public bool selectionRule;
            public ListViewItem itemMouseDown;
            public ListViewItem itemMouseDownOri;
        }
        mouseMoveSelect m_mms = null;
        /*
        // listview items selection rules
        // - reset selection by left click on an arbitrary item (down & up)
        // - select / deselect while mouse moves with left button down
        // - ensure at minimum one item is always selected 
        */
        private void listViewLeftRight_MouseDown(object sender, MouseEventArgs e) {
            this.lvDragMouseDown(sender, e);
            //            if ( m_PointerDownMessage == 0x201 ) {
            this.MouseDownAction(sender, e);
            //            }
        }
        void MouseDownAction(object sender, MouseEventArgs e) {
            ListView lve = (ListView)sender;
            if ( (((ModifierKeys & Keys.Control) == Keys.Control) || ((ModifierKeys & Keys.Shift) == Keys.Shift)) ) {
                // standard selection methods using shift & control key happens automatically
                // ...
                // Mouse down but ListView is not yet activated
                if ( lve != this.m_Panel.GetActiveView() ) {
                }
            } else {
                //  special selection behaviour
                if ( e.Button == MouseButtons.Left ) {

                    // 20161016: tooltip bothers a bit when selecting items
                    this.m_toolTip.Hide(lve);

                    // what listviewitem do we deal with
                    ListViewItem item = this.listviewGetItemAt(lve, e.Location);
                    if ( item != null ) {
                        // memorize selection status
                        this.m_bStatusBeforeMouseDown = item.Selected;
                        // listview autoselect when mouse leaves the listview: this is an indicator, that the starting mouse click hit a valid item
                        this.m_bLeftMouseHitItem = true;
                        // prepare select items
                        this.m_mms = new mouseMoveSelect();
                        this.m_mms.itemMouseDown = item;
                        this.m_mms.itemMouseDownOri = item;
                        this.m_mms.prevMousePos = Cursor.Position;
                        this.m_mms.buttonDownMousePos = this.m_mms.prevMousePos;
                        this.m_mms.buttonDownMousePos = this.m_mms.prevMousePos;
                        this.m_mms.moveDirection = Direction.na;
                    }

                    // activate listview per left mouse w/o resetting the selection, happens when mouse is coming from the other listview
                    this.m_bActivateList = false;
                    if ( lve != this.m_Panel.GetActiveView() ) {
                        // stop paint: prevents flickering when mouse activates passive listview
                        GrzTools.DrawingControl.SuspendDrawing(lve);
                        this.m_Panel.SetActiveSide(this.m_Panel.GetPassiveSide());
                        // 20161016: since the next line was missing, autoscroll when dragging at any selection >1 didn't repaint the affected listview
                        GrzTools.DrawingControl.ResumeDrawing(lve);
                        this.RenderCommandline(this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString());
                        // changing the listview side shall not reset a selection
                        if ( (lve.SelectedIndices.Count > 1) && (item != null) ) {
                            // TRICKY: a min of 4 pixels 'artificial' mouse move is needed to prevent resetting the current selection
                            this.m_bActivateList = true;
                            this.m_ptMove = new Point(Cursor.Position.X + 4, Cursor.Position.Y);
                            Cursor.Position = this.m_ptMove;
                        }
                    } else {
                        // 20161016: handles repeated clicks in the same listview with selection selection > 1 (prior to this change, any selection was unselected)
                        if ( (lve.SelectedIndices.Count > 1) && (item != null) ) {
                            // 20161016: click in filename column shall select this item, while click to column with index > 0 shall keep selection -> this allows to start dragNdrop ops (only in !Computer view)
                            if ( (e.X > lve.Columns[0].Width) && (this.m_Panel.button(this.m_Panel.GetActiveSide()).Text[1] == ':') ) {
                                GrzTools.DrawingControl.SuspendDrawing(lve);
                                this.m_bActivateList = true;
                                this.m_ptMove = new Point(Cursor.Position.X + 4, Cursor.Position.Y);
                                Cursor.Position = this.m_ptMove;
                                GrzTools.DrawingControl.ResumeDrawing(lve);
                            }
                        }
                    }

                    // any left mouse down shall end the rename proces either via abort OR via OnEditBoxLostFocus 
                    // AWKWARD - regular lv calls m_editbox.OnLostFocus, "Computer" lv come here first  
                    if ( this.m_editbox.Parent != null ) {
                        if ( this.m_editbox.Parent != this.m_Panel.GetActiveView() ) {
                            this.QuitRename();
                        } else {
                            this.m_bKeepFocused = false;
                            this.OnEditBoxLostFocus(null, null);
                        }
                    }

                    // memorize current selection for "Mouse Up": for unknown reason, selection gets lost at MouseUp when not clicking an item (ie. empty area)
                    this.m_sic = new int[lve.SelectedIndices.Count];
                    lve.SelectedIndices.CopyTo(this.m_sic, 0);    // that defers MouseDown from completing: WinSxS folder gets fully deselected

                    if ( item == null ) {
                        GrzTools.DrawingControl.ResumeDrawing(lve);
                        return;
                    }

                    // setup the selection status rule based on the selection status of the hovered (ie. current) item
                    this.m_bSelectRule = true;
                    if ( lve.SelectedIndices.Count != 1 ) {  // if there is only 1 selected item, we always take status = true  
                        this.m_bSelectRule = !item.Selected;
                    }
                    this.m_mms.selectionRule = this.m_bSelectRule;

                    // this is essential to allow a 'non connected multi selection', otherwise ANY mouse down resets any selection 
                    item.Selected = true;
                    e = new MouseEventArgs(e.Button, e.Clicks, 0, 0, e.Delta);
                }
            }
        }
        Point m_lastToolTipPosition = new Point(0, 0);
        private void listViewLeftRight_MouseMove(object sender, MouseEventArgs e) {
            this.lvDragMouseMove(sender, e);
            // debouncer after draNdrop operation is over
            if ( this.m_draggingFromLv ) {
                return;
            }

            ListView lv = (ListView)sender;
            ListViewItem item = this.listviewGetItemAt(lv, e.Location);
            if ( item == null ) {
                // ensure [..] is NEVER selected, in case it exists AND the selection count is > 1  
                if ( lv.SelectedIndices.Count > 1 ) {
                    if ( lv == this.m_Panel.GetActiveView() ) {
                        ListViewItem lvi = this.m_Panel.FindListViewArrItemWithText(this.m_Panel.GetActiveSide(), "[..]", 2);
                        if ( lvi != null ) {
                            lvi.Selected = false;
                        }
                    }
                }
                this.m_toolTip.Hide(lv);
                return;
            }

            // when no mouse button is hold AND text is shortened, then show a popup with the full name
            if ( e.Button == MouseButtons.None ) {
                Size textSize = TextRenderer.MeasureText(item.Text, lv.Font);
                if ( textSize.Width > lv.Columns[0].Width - 20 ) {
                    ListViewHitTestInfo hitTest = lv.HitTest(e.Location);
                    int columnIndex = hitTest.Item.SubItems.IndexOf(hitTest.SubItem);
                    if ( columnIndex == 0 ) {
                        if ( this.m_lastToolTipPosition != e.Location ) {
                            this.m_lastToolTipPosition = e.Location;
                            this.m_toolTip.Show(item.Text, lv, e.X - textSize.Width / 3, e.Y + textSize.Height);
                        }
                    }
                } else {
                    this.m_toolTip.Hide(lv);
                }
            } else {
                this.m_toolTip.Hide(lv);
            }

            // select / deselect items when mouse moves based on the selection rule established in 'mouse down' 
            if ( e.Button == MouseButtons.Left ) {

                // how far did the mouse move?
                if ( (Math.Abs(Cursor.Position.X - this.m_ptMove.X) > 5) || (Math.Abs(Cursor.Position.Y - this.m_ptMove.Y) > 5) ) { // change selection if move is > 5 pixels
                    //
                    // 20161016: apply selection rule while moving mouse
                    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                    // - obtain selection rule from mouse down event
                    // - revert selection rule, when mouse move reverses
                    // - revert selection rule, when mouse crosses original mouse down position
                    // - do not revert selection rule, when mouse move reverts over original mouse down position
                    if ( (this.m_mms != null) && !this.m_draggingFromLv ) {

                        // did the mouse position change?
                        bool selRuleChanged = false;
                        if ( Cursor.Position.Y != this.m_mms.prevMousePos.Y ) {
                            // mouse position did change    
                            if ( Cursor.Position.Y > this.m_mms.prevMousePos.Y ) {
                                // mouse moves downwards
                                if ( this.m_mms.moveDirection == Direction.up ) {
                                    // mouse move direction did reverse: if old mouse move direction was opposite, we reverse the selection rule ONLY if item is not the original mouse down item
                                    if ( item.Index != this.m_mms.itemMouseDownOri.Index ) {
                                        this.m_mms.selectionRule = !this.m_mms.selectionRule;
                                        selRuleChanged = true;
                                    }
                                    // declare new mouse down position
                                    this.m_mms.buttonDownMousePos = Cursor.Position;
                                }
                                // memorize the mouse move direction
                                this.m_mms.moveDirection = Direction.down;
                            } else {
                                // mouse moves upwards
                                if ( this.m_mms.moveDirection == Direction.down ) {
                                    if ( item.Index != this.m_mms.itemMouseDownOri.Index ) {
                                        this.m_mms.selectionRule = !this.m_mms.selectionRule;
                                        selRuleChanged = true;
                                    }
                                    this.m_mms.buttonDownMousePos = Cursor.Position;
                                }
                                this.m_mms.moveDirection = Direction.up;
                            }
                        }
                        // debouncer for current item: it should be detected only once
                        if ( (this.m_mms.itemMouseDown != null) && (this.m_mms.itemMouseDownOri != null) && (item.Index != this.m_mms.itemMouseDown.Index) ) {
                            this.m_mms.itemMouseDown = item;
                            // current item reached the original mouse down item? --> reverse selection rule ONLY if it wasn't changed due to move direction change
                            if ( item.Index == this.m_mms.itemMouseDownOri.Index ) {
                                if ( !selRuleChanged ) {
                                    this.m_mms.selectionRule = !this.m_mms.selectionRule;
                                }
                                this.m_mms.buttonDownMousePos = Cursor.Position;
                            }
                        }
                        // finally select all items between Cursor.Position.Y and m_mms.buttonDownMousePos.Y according to selection rule
                        ListViewItem lvtmp = this.listviewGetItemAt(lv, lv.PointToClient(this.m_mms.buttonDownMousePos));
                        if ( lvtmp == null ) {
                            return;
                        }
                        int ndxBeg = lvtmp.Index;
                        lvtmp = this.listviewGetItemAt(lv, lv.PointToClient(Cursor.Position));
                        if ( lvtmp == null ) {
                            return;
                        }
                        int ndxEnd = lvtmp.Index;
                        for ( int i = Math.Min(ndxBeg, ndxEnd); i <= Math.Max(ndxBeg, ndxEnd); i++ ) {
                            lv.Items[i].Selected = this.m_mms.selectionRule;
                        }
                        // save most recent mouse position to be able to detect a direction change
                        this.m_mms.prevMousePos = Cursor.Position;
                    }

                } else {
                    // Keep Selection --> this only applies during the 4 pixel "fake mouse move"
                    item.Selected = this.m_bStatusBeforeMouseDown;
                }

                // ensure there is always at least one selected item
                if ( lv.SelectedIndices.Count == 0 ) {
                    item.Selected = true;
                }
            }

            // unconditionally resume paint: turning ot off, prevented flickering when mouse activates thr passive listview
            GrzTools.DrawingControl.ResumeDrawing(lv);
            if ( item.Selected ) {
                lv.Invalidate();
            }
        }
        private void listViewLeftRight_MouseUp(object sender, MouseEventArgs e) {
            // control Insert key behaviour to toggle selection state by keyboard
            this.m_bFirstInsertAfterMouseUp = true;

            // discard all history about mouse move selection
            this.m_mms = new mouseMoveSelect();

            // drag op
            this.lvDragMouseUp(sender, e);

            // listview autoselect when mouse leaves the listview: this is an indicator, that the starting mouse click hit a valid item - now this is not longer true
            this.m_bLeftMouseHitItem = false;

            //if ( m_PointerUpMessage == 0x202 ) {
            this.MouseUpAction(sender, e);
            //}
        }
        bool MouseUpAction(object sender, MouseEventArgs e) {
            ListView lve = (ListView)sender;
            ListViewItem item = this.listviewGetItemAt(lve, e.Location);

            // item == null: the mouseup event occurs in an empty region of the listview
            if ( item == null ) {
                if ( lve.Items.Count == 0 ) {
                    return true;
                }

                // make sure, we have the same selection at "Mouse Up" as it had been at "Mouse Down": for unknown reason, selection gets lost at MouseUp w/o item clicked
                if ( lve.SelectedIndices.Count == 0 ) {
                    foreach ( int i in this.m_sic ) {
                        if ( i < lve.Items.Count ) {
                            lve.SelectedIndices.Add(i);
                        }
                    }
                }

                // ensure [..] is NEVER selected, in case it exists AND the selection count is > 1  
                if ( lve.SelectedIndices.Count > 1 ) {
                    ListViewItem lvi = this.m_Panel.FindListViewArrItemWithText(this.m_Panel.GetActiveSide(), "[..]", 2);
                    if ( lvi != null ) {
                        lvi.Selected = false;
                    }
                }

                return true;
            }

            // ensure there is always at least one selected item
            if ( lve.SelectedIndices.Count == 0 ) {
                item.Selected = true;
            }

            // mouseup event occurs over an item: ensure [..] is NEVER selected, in case it exists AND the selection count is > 1  
            if ( lve.SelectedIndices.Count > 1 ) {
                if ( lve == this.m_Panel.GetActiveView() ) {
                    ListViewItem lvi = this.m_Panel.FindListViewArrItemWithText(this.m_Panel.GetActiveSide(), "[..]", 2);
                    if ( lvi != null ) {
                        lvi.Selected = false;
                    }
                } else {
                    ListViewItem lvi = this.m_Panel.FindListViewArrItemWithText(this.m_Panel.GetPassiveSide(), "[..]", 2);
                    if ( lvi != null ) {
                        lvi.Selected = false;
                    }
                }
            }

            // TRICKY: mouseup event occurs over an item: in case of listview activation by mouse, correct the 4 pixels 'artificial' mouse move to re enable double click events
            if ( this.m_bActivateList ) {
                this.m_bActivateList = false;
                Cursor.Position = new Point(Cursor.Position.X - 4, Cursor.Position.Y);
            }

            return false;
        }

        // ListView top/bottom auto select behaviour, when held left mouse leaves the listview
        ListViewItem GetLastVisibleItem(ListView lv) {
            ListViewItem lastVisible = lv.TopItem;
            for ( int i = lv.TopItem.Index + 1; i < lv.Items.Count; i++ ) {
                Point crc = new Point(0, lv.Items[i].Position.Y + lv.Items[i].Bounds.Height);
                if ( lv.ClientRectangle.Contains(crc) ) {
                    lastVisible = lv.Items[i];
                } else {
                    break;
                }
            }
            return lastVisible;
        }
        void listViewAutoSelect(ListView lv, ListViewItem lvi, bool bDown, bool bSelect, ListViewItem lvo) {
            bool select = bSelect;
            this.m_bListViewAutoSelect = true;
            int n = lvi.Index;
            do {
                if ( bDown ) {
                    n++;
                } else {
                    n--;
                }
                if ( n < 0 ) {
                    break;
                }
                if ( n == lv.Items.Count ) {
                    break;
                }
                if ( MouseButtons != MouseButtons.Left ) {
                    break;
                }
                if ( lvo == null ) {
                    break;
                }
                ListViewItem lvn = lv.Items[n];
                lvn.Selected = select;
                lv.EnsureVisible(lvn.Index);
                Thread.Sleep(20);
                Application.DoEvents();
                if ( n == lvo.Index ) {
                    select = !bSelect;
                    if ( this.m_mms != null ) {
                        this.m_mms.selectionRule = select;
                    }
                }
            } while ( this.m_bListViewAutoSelect );
        }
        private void listViewLeftRight_MouseEnter(object sender, EventArgs e) {
            // unconditionally break autoselect
            this.m_bListViewAutoSelect = false;

            // what listviewitem do we deal with
            if ( MouseButtons == MouseButtons.Left ) {
                ListView lv = (ListView)sender;
                Point pt = lv.PointToClient(new Point(MousePosition.X, MousePosition.Y - 20));            // -20 voodoo number
                ListViewItem item = this.listviewGetItemAt(lv, pt);
                if ( item != null ) {
                    // memorize selection status
                    this.m_bStatusBeforeMouseDown = item.Selected;
                    // listview autoselect when mouse re enters the listview: this is an indicator, that the starting mouse click hit a valid item
                    this.m_bLeftMouseHitItem = true;
                    // prepare select items
                    if ( this.m_mms != null ) {
                        this.m_mms.itemMouseDown = item;
                        this.m_mms.prevMousePos = Cursor.Position;
                        this.m_mms.buttonDownMousePos = this.m_mms.prevMousePos;
                    }
                }
            }
        }
        private void listViewLeftRight_MouseLeave(object sender, EventArgs e) {
            // 20161016: we don't allow auto selection of items, when a dragNdrop op is ongoing
            if ( this.m_draggingFromLv ) {
                return;
            }
            // 20161016 hi/lo rectangles introduced: start autoselect in listview when mouse leaves the listview AND a valid item was hit AND left mouse button is down
            if ( (MouseButtons == MouseButtons.Left) && this.m_bLeftMouseHitItem ) {
                Point cursor = Cursor.Position;
                ListView lv = (ListView)sender;
                Point lvPos = lv.PointToScreen(lv.Location);
                Rectangle hi = new Rectangle(lvPos.X, lvPos.Y - 40, lv.ClientSize.Width, 40);
                Rectangle lo = new Rectangle(lvPos.X, lvPos.Y + lv.ClientSize.Height - 30, lv.ClientSize.Width, 40);  // 30 is the empty space between the last item and the border
                if ( hi.Contains(cursor) ) {
                    this.listViewAutoSelect(lv, lv.TopItem, false, this.m_mms.selectionRule, this.m_mms.itemMouseDownOri);
                }
                if ( lo.Contains(cursor) ) {
                    this.listViewAutoSelect(lv, this.GetLastVisibleItem(lv), true, this.m_mms.selectionRule, this.m_mms.itemMouseDownOri);
                }
            }
            // stop hovering effect on listview when mouse leaves it
            if ( MouseButtons == MouseButtons.None ) {
                ((ListView)sender).Invalidate();
            }
        }

        // ListView.OwnerDraw = true: we need to take care about _DrawItem, _DrawSubItem, _DrawColumnHeader
        private void listViewLeftRight_DrawItem(object sender, DrawListViewItemEventArgs e) {
            // Check if e.Item is selected
            ListView listView = (ListView)sender;
            if ( e.Item.Selected ) {
                // prevents flicker when size changes
                if ( !this.m_bSizing ) {
                    // selected items background color depending on activation status of listview                     
                    if ( listView == this.m_Panel.GetActiveView() ) {
                        e.Graphics.FillRectangle(Brushes.CornflowerBlue, e.Bounds);
                    } else {
                        e.Graphics.FillRectangle(Brushes.Gray, e.Bounds);
                    }
                }
                // 'no details' aka 'list' requires image & text rendering here, otherwise it's done in 'DrawSubItem'
                if ( listView.View != View.Details ) {
                    Rectangle bounds = e.Bounds;
                    Rectangle rectangle = new Rectangle(bounds.X + 1, bounds.Y, 20, 20);
                    e.Graphics.DrawImage(this.imageListLv.Images[e.Item.ImageIndex], rectangle);
                    bounds.X += 20;
                    bounds.Y += 3;
                    e.Graphics.DrawString(e.Item.Text, listView.Font, Brushes.Black, bounds);
                }
            } else {
                // not selected items: Text foreground color of folders in blue and files in black
                if ( !e.Item.Selected && ((e.Item.ImageIndex == 3) || (e.Item.ImageIndex == 0)) ) {
                    e.Item.ForeColor = Color.DarkBlue;
                } else {
                    e.Item.ForeColor = Color.Black;
                }
                if ( listView.View != View.Details ) {
                    e.DrawDefault = true;   // nice feature: in unselected status the current list item is rendered differently when hovering
                }
            }

            //if ( e.Item.Focused ) {
            //    e.Item.ForeColor = Color.Red;
            //}
        }
        private void listViewLeftRight_DrawSubItem(object sender, DrawListViewSubItemEventArgs e) {
            // item playground
            Rectangle bounds = e.Bounds;
            // item text
            string text = e.SubItem.Text;

            // if e.Item is selected we need to apply our own rules
            if ( e.Item.Selected ) {
                // render icon in column 0
                if ( e.ColumnIndex == 0 ) {
                    // draw item image
                    Rectangle rectangle = new Rectangle(bounds.X + 4, e.Bounds.Y, 20, 20);
                    e.Graphics.DrawImage(this.imageListLv.Images[e.Item.ImageIndex], rectangle);
                    bounds.X += 23;
                    bounds.Width -= 20;
                    // shorten item text if needed
                    Size textSize = TextRenderer.MeasureText(text, e.SubItem.Font);
                    int decrease = 1;
                    while ( textSize.Width > bounds.Width ) {
                        int newLength = e.SubItem.Text.Length - (++decrease);
                        if ( newLength <= 0 ) {
                            break;
                        }
                        text = e.SubItem.Text.Substring(0, newLength) + "...";
                        textSize = TextRenderer.MeasureText(text, e.SubItem.Font);
                    }
                } else {
                    // subitems need a slight X position tweak depending on their alignment
                    if ( e.Header.TextAlign == HorizontalAlignment.Right ) {
                        bounds.X -= 2;
                    } else {
                        bounds.X += 3;
                    }
                    // show an image in column 3: percentage of the free HD space
                    if ( (e.ColumnIndex == 3) && (e.SubItem.Text.EndsWith("%")) ) {
                        int ofs = (int)Math.Round(bounds.Width * (1 - float.Parse(e.SubItem.Text.Substring(0, e.SubItem.Text.Length - 1)) / 100));
                        Rectangle rectangle = new Rectangle(bounds.X, bounds.Y + 5, ofs, bounds.Height - 9);
                        e.Graphics.FillRectangle(Brushes.LightPink, rectangle);
                        rectangle = new Rectangle(bounds.X + ofs, bounds.Y + 5, bounds.Width - ofs, bounds.Height - 9);
                        e.Graphics.FillRectangle(Brushes.LightGreen, rectangle);
                        bounds.Width += 6;
                    }
                }

                // Y needs tweaking depending on Fontsize: +4 small, +3 large, +2 extra  
                bounds.Y += 4;
                if ( this.largeToolStripMenuItem.Checked ) {
                    bounds.Y -= 1;
                }
                if ( this.extraToolStripMenuItem.Checked ) {
                    bounds.Y -= 2;
                }

                // stubborn "align" needs separate generation 
                TextFormatFlags align = TextFormatFlags.Left;
                if ( e.Header.TextAlign == HorizontalAlignment.Right ) {
                    align = TextFormatFlags.Right;
                }

                // distinguish between folders (ImageIndex == 0 or 3) and files (all others): show selected folders in white and files in black
                if ( (e.Item.ImageIndex == 3) || (e.Item.ImageIndex == 0) ) {
                    TextRenderer.DrawText(e.Graphics, text, e.SubItem.Font, bounds, Color.White, align);
                } else {
                    TextRenderer.DrawText(e.Graphics, text, e.SubItem.Font, bounds, Color.Black, align);
                }
            } else {
                // in "Computer View" show an image in column 3: percentage graph of the free HD space
                if ( (e.ColumnIndex == 3) && (e.SubItem.Text.EndsWith("%")) ) {
                    int ofs = (int)Math.Round(bounds.Width * (1 - float.Parse(text.Substring(0, text.Length - 1)) / 100));
                    Rectangle rectangle = new Rectangle(bounds.X, bounds.Y + 5, ofs, bounds.Height - 9);
                    e.Graphics.FillRectangle(Brushes.LightPink, rectangle);
                    rectangle = new Rectangle(bounds.X + ofs, bounds.Y + 5, bounds.Width - ofs, bounds.Height - 9);
                    e.Graphics.FillRectangle(Brushes.LightGreen, rectangle);
                    // x pos tweak
                    bounds.X -= 2;
                    bounds.Width += 6;
                    // Y needs tweaking depending on fontsize: +4 small, +3 large, +2 extra  
                    bounds.Y += 4;
                    if ( this.largeToolStripMenuItem.Checked ) {
                        bounds.Y -= 1;
                    }
                    if ( this.extraToolStripMenuItem.Checked ) {
                        bounds.Y -= 2;
                    }
                    TextRenderer.DrawText(e.Graphics, text, e.SubItem.Font, bounds, Color.Black, TextFormatFlags.Right);
                } else {
                    e.DrawDefault = true;
                }
            }

            //if ( e.Item.Focused ) {
            //    TextFormatFlags align = TextFormatFlags.Left;
            //    if ( e.Header.TextAlign == HorizontalAlignment.Right ) {
            //        align = TextFormatFlags.Right;
            //    }
            //    TextRenderer.DrawText(e.Graphics, text, e.SubItem.Font, bounds, Color.Red, align);
            //}

        }
        private void listViewLeftRight_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e) {
            e.Graphics.FillRectangle(Brushes.BlanchedAlmond, e.Bounds);
            e.DrawDefault = true;
        }

        // way faster ListView selection in virtual mode compared to regular mode
        bool m_bFirstInsertAfterMouseUp = true;
        void SelectListViewItems(int status) {
            ListView lv = this.m_Panel.GetActiveView();
            if ( lv.Items.Count == 0 ) {
                return;
            }

            ListViewItem[] lvarr = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide());

            // three selection scenarios: all, nothing, invert
            lv.SelectedIndexChanged -= new System.EventHandler(this.listViewLeftRight_SelectedIndexChanged);
            lv.BeginUpdate();
            switch ( status ) {
                case 2: {  // 20161016: toggle the selection status of the focused item via Insert key --> little bit tricky behaviour with listview
                        ListViewItem item = lv.FocusedItem;
                        if ( (item != null) && (item.Index < lv.Items.Count - 1) ) {
                            if ( this.m_bFirstInsertAfterMouseUp && (lv.SelectedIndices.Count > 1) ) {
                                item.Selected = !item.Selected;
                            } else {
                                lv.FocusedItem = lv.Items[lv.FocusedItem.Index + 1];
                                item = lv.FocusedItem;
                                item.Selected = !item.Selected;
                            }
                            this.m_bFirstInsertAfterMouseUp = false;
                        }
                        break;
                    }
                case 1: {  // 20161016: select according to wildcard mask 
                        lv.SelectedIndices.Clear();   // call  was added: Initially I thought it might be nice to "add" multiple selection filters while selecting, but it turned out being too dangerous.
                        string wildcard = "";
                        InputBox ib = new InputBox();
                        ib.Category = this.m_sLHSfilter;
                        ib.Text = "Select files & folders";
                        ib.Location = new Point(MousePosition.X - 90, MousePosition.Y - 180);
                        ib.ShowDialog();
                        if ( ib.DialogResult != DialogResult.OK ) {
                            lv.EndUpdate();
                            lv.SelectedIndexChanged += new System.EventHandler(this.listViewLeftRight_SelectedIndexChanged);
                            return;
                        } else {
                            wildcard = ib.Category;
                        }
                        Regex regex = GrzTools.FastFileFind.FindFilesPatternToRegex.Convert(wildcard);
                        for ( int i = 0; i < lvarr.Length; i++ ) {
                            if ( regex.IsMatch(lvarr[i].Text) ) {
                                lv.SelectedIndices.Add(i);
                            }
                        }
                        break;
                    }
                case 3: {  // select all
                        for ( int i = 0; i < lvarr.Length; i++ ) {
                            lv.SelectedIndices.Add(i);
                        }
                        break;
                    }
                case 0: {  // select nothing
                        lv.SelectedIndices.Clear();
                        break;
                    }
                case -1: { // invert current selection
                        // get current selection
                        List<int> curSel = new List<int>();
                        foreach ( int ndx in lv.SelectedIndices ) {
                            curSel.Add(ndx);
                        }
                        // loop master ListView ... 
                        for ( int i = 0; i < lvarr.Length; i++ ) {
                            // ... and check against current selection
                            bool bFound = false;
                            foreach ( int s in curSel ) {
                                // at hit remove this entry from the list of selected indices
                                if ( i == s ) {
                                    lv.SelectedIndices.Remove(i);
                                    bFound = true;
                                    break;
                                }
                            }
                            // at no hit add this entry to the list of selected indices
                            if ( !bFound ) {
                                lv.SelectedIndices.Add(i);
                            }
                        }
                        break;
                    }
            }
            lv.EndUpdate();
            lv.SelectedIndexChanged += new System.EventHandler(this.listViewLeftRight_SelectedIndexChanged);
            this.listViewLeftRight_SelectedIndexChanged(lv, null);

            // ensure [..] is NEVER selected, in case it exists AND the selection count is > 1  
            ListViewItem lvi = this.m_Panel.FindListViewArrItemWithText(this.m_Panel.GetActiveSide(), "[..]", 2);
            if ( lvi != null ) {
                if ( lv.SelectedIndices.Count > 1 ) {
                    if ( (int)lvi.Tag == lv.SelectedIndices[0] ) {
                        lv.SelectedIndices.Remove(0);
                    }
                }
            }
            // select [..] in case nothing else is selected
            if ( lv.SelectedIndices.Count == 0 ) {
                if ( lvi != null ) {
                    lvi.Selected = true;
                } else {
                    lv.Items[0].Selected = true;
                }
            }
        }
        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e) {
            this.SelectListViewItems(1);
        }
        private void deselectAllToolStripMenuItem_Click(object sender, EventArgs e) {
            this.SelectListViewItems(0);
        }
        private void invertSelectionToolStripMenuItem_Click(object sender, EventArgs e) {
            this.SelectListViewItems(-1);
        }
        // windows common dialog "open file with ..."
        private void openWithToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.m_Panel.GetActiveView().SelectedIndices.Count == 0 ) {
                return;
            }
            string selection = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[this.m_Panel.GetActiveView().SelectedIndices[0]].Text;
            string folder = this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString();
            string file = @Path.Combine(folder, selection);
            if ( File.Exists(file) ) {
                string args = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll");
                args += ",OpenAs_RunDLL " + file;
                Process.Start("rundll32.exe", args);
            }
        }
        // shortcut on desktop
        private void linkOnDesktopToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.m_Panel.GetActiveView().SelectedIndices.Count == 0 ) {
                return;
            }
            string selection = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[this.m_Panel.GetActiveView().SelectedIndices[0]].Text;
            string file = @Path.Combine(this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), selection);
            if ( this.linkOnDesktopToolStripMenuItem.Text == "Link to Desktop" ) {
                GrzTools.FileTools.CreateShortcut(file, "");
            } else {
                GrzTools.FileTools.CreateShortcut(file, this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString());
            }
        }
        // properties of a file / folder or a selection of them
        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.m_Panel.GetActiveView().SelectedIndices.Count > 1 ) {
                // self made property dlg
                string path = this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString();
                ListView lv = this.m_Panel.GetActiveView();
                ListView tmplv = new ListView();
                foreach ( int ndx in lv.SelectedIndices ) {
                    ListViewItem lvi = (ListViewItem)this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[ndx].Clone();
                    tmplv.Items.Add(lvi);
                }
                PropertyForm frm = new PropertyForm(this, tmplv, path);
                frm.Location = new Point(MousePosition.X - frm.Width / 2, this.m_listViewL.PointToScreen(this.m_listViewL.Location).Y);
                frm.Show();
            } else {
                // explorer standard dlg
                if ( this.m_Panel.GetActiveView().SelectedIndices.Count == 0 ) {
                    return;
                }
                string selection = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[this.m_Panel.GetActiveView().SelectedIndices[0]].Text;
                string file = @Path.Combine(this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), selection);

                if ( file[1] != ':' ) {
                    if ( file == @"Computer\Downloads" ) {
                        @file = @GrzTools.FileTools.TranslateSpecialFolderNames("Downloads");
                    }
                    if ( file == @"Computer\Desktop" ) {
                        @file = @GrzTools.FileTools.TranslateSpecialFolderNames("Desktop");
                    }
                    if ( file == @"Computer\Documents" ) {
                        @file = @GrzTools.FileTools.TranslateSpecialFolderNames("Documents");
                    }
                    if ( file == @"Computer\Shared Folders" ) {
                        Process.Start("fsmgmt.msc");
                        return;
                    }
                    if ( file == @"Computer\Network" ) { //20160320
                        Process.Start("control.exe", "/name Microsoft.NetworkAndSharingCenter");
                        return;
                    }
                }

                GrzTools.FileTools.ShowFileProperties(file);
            }
        }

        /*
        // Have a separate Thread for SHFileOperation (using the original Windows UI), which executes copy/move/delete operations
        // - executing copy/move/delete in MainForm doesn't allow to access the UI, because SHFileOperation is application modal
        // - therefore SHFileOperation is put into a separate thread and our UI keeps responsive:
        //   a) BackGroundWorker always uses MTA mode, which might be confusing: cancel one copy operation shows another one still running 
        //   b) therefore Thread was chosen with STA enabled
        // - an event is raised, when SHFileOperation is done: we use it to reload the listviews manually in case of mapped network drives  
        */
        private void ShfoEventHandler(object source, ShfoWorker.ShfoEventArgs e) {
            try {
                // if there is a message (normally an error), we show it 
                if ( e.message.Length > 0 ) {
                    this.Invoke(new Action(() => { MessageBox.Show(e.message); }));
                }

                this.Invoke(new Action(() => {
                    // 20161016: dragNdrop to a unmonitored folder (either monitor is disabled in cfw, or it's stupid USB-Drive) shall force a refresh  
                    if ( e.bForceRefresh ) {
                        this.LoadListView(this.m_Panel.GetActiveSide(), this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), "*");
                    }
                    // 20160306: in case of mapped network drive we reload manually, because fsw is not always reliable in such case
                    string driveLHS = Path.GetPathRoot(this.m_Panel.button(Side.left).Tag.ToString());
                    if ( NetworkMapping.MappedDriveResolver.isNetworkDrive(driveLHS) ) {
                        this.LoadListView(Side.left, this.m_Panel.button(Side.left).Tag.ToString(), "*");
                    }
                    string driveRHS = Path.GetPathRoot(this.m_Panel.button(Side.right).Tag.ToString());
                    if ( NetworkMapping.MappedDriveResolver.isNetworkDrive(driveRHS) ) {
                        this.LoadListView(Side.right, this.m_Panel.button(Side.right).Tag.ToString(), "*");
                    }
                }));
            } catch ( Exception ) {; } // ShfoWorker might still run when App is closed, then we get an exception the moment the event is raised 
        }
        public class ShfoWorker {
            public delegate void EventHandler(object sender, ShfoEventArgs e);
            public event EventHandler ShfoEvent = delegate { };
            public class ShfoEventArgs : EventArgs {
                public ShfoEventArgs(string message, bool bForceRefresh) {
                    this.message = message;
                    this.bForceRefresh = bForceRefresh;
                }
                public string message;
                public bool bForceRefresh;
            }
            public struct Arguments {
                public IntPtr hOwner;
                public List<string> lSource;
                public List<string> lDestin;
                public GrzTools.ShellFileOperation.FileOperations fAction;
                public string sAction;
                public bool bRenameCollision;
                public bool bForceRefresh;
                public Arguments(IntPtr parent, List<string> src, List<string> dst, GrzTools.ShellFileOperation.FileOperations fo, string act, bool bRenameCollision, bool bForceRefresh) {
                    this.hOwner = parent;
                    this.lSource = src;
                    this.lDestin = dst;
                    this.sAction = act;
                    this.fAction = fo;
                    this.bRenameCollision = bRenameCollision;
                    this.bForceRefresh = bForceRefresh;
                }

            }
            private bool m_bActive = false;
            Arguments arguments;
            public ShfoWorker() {
            }
            public void SetArguments(Arguments args) {
                this.arguments = args;
            }
            public bool IsActive {
                get {
                    return this.m_bActive;
                }
            }
            public void DoWorkShfo() {
                this.m_bActive = true;
                GrzTools.ShellFileOperation fo = new GrzTools.ShellFileOperation();
                if ( this.arguments.bRenameCollision ) {
                    fo.OperationFlags |= GrzTools.ShellFileOperation.ShellFileOperationFlags.FOF_RENAMEONCOLLISION;
                }
                fo.Operation = this.arguments.fAction;
                fo.OwnerWindow = this.arguments.hOwner;
                fo.SourceFiles = this.arguments.lSource.ToArray();
                fo.DestFiles = this.arguments.lDestin.ToArray();
                bool RetVal = fo.DoOperation();
                string msg = "";
                if ( !RetVal ) {
                    msg = this.arguments.sAction + " completed with errors.";
                }
                ShfoEvent(this, new ShfoEventArgs(msg, this.arguments.bForceRefresh));
                this.m_bActive = false;
            }
        }
        ListType GetSelectedListViewStrings(out List<string> src, out List<string> dst, string realDestination, out string wpdFolder, bool bDelete = false) {
            ListType lt = ListType.FileSystem;
            wpdFolder = "";

            src = new List<String>();
            dst = new List<String>();
            ListView lv = this.m_Panel.GetActiveView();
            ListViewItem[] lvarr = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide());
            string pathSrc = this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString();
            if ( pathSrc == "Computer" ) {
                return ListType.Error;
            }

            // the real destination is set, when the user was asked for "what destination" - otherwise destination is taken from the 'passive side' button.Tag
            string pathDst;
            if ( realDestination == null ) {
                // good to FS
                pathDst = this.m_Panel.button(this.m_Panel.GetPassiveSide()).Tag.ToString();
            } else {
                pathDst = realDestination;
            }

            // now check for WPD
            int indexWPDdst = this.getIndexOfWPD(pathDst);
            int indexWPDsrc = this.getIndexOfWPD(pathSrc);
            if ( (indexWPDdst != -1) && (indexWPDsrc != -1) ) {
                if ( (indexWPDdst != indexWPDsrc) ) {
                    MessageBox.Show("File operations between different WPD are not implemented.", "Error");
                    return ListType.Error;
                }
            }

            // collect files and folders from active listview
            foreach ( int ndx in lv.SelectedIndices ) {

                // listvie item
                ListViewItem lvi = lvarr[ndx];

                // "[..]" could be a valid file/folder, therefore we check ImageIndex != 2, which is "Level Up"
                if ( lvi.ImageIndex == 2 ) { // aka "LevelUp"
                    continue;
                }

                // connected htm folder
                string htmFolder = "";

                //
                // source
                //
                string nameSrc = lvi.SubItems[0].Text;
                if ( indexWPDsrc != -1 ) {
                    // src is WPD
                    if ( lvi.SubItems[6].Text.StartsWith("wpd_") ) {
                        string[] arr = lvi.SubItems[6].Text.Split('_');
                        // WPD folders are only allowed when delete is flagged
                        if ( ((lvi.ImageIndex != 0) && (lvi.ImageIndex != 3)) || bDelete ) {
                            // VOODOO: 'original filename' + "?" + 'WPD file ID' + "?" + 'file time' is retuned as string
                            src.Add(nameSrc + "?" + arr[1] + "?" + lvi.SubItems[1].Text);
                        }
                        lt = ListType.WPDsrc;
                        wpdFolder = pathSrc;
                    }
                } else {
                    // src is FS
                    src.Add(Path.Combine(pathSrc, nameSrc));
                    // connected htm folder
                    if ( this.connectHtmWithItsFilesFolderToolStripMenuItem.Checked ) {
                        int pos = nameSrc.LastIndexOf(".htm");                       
                        if ( pos != -1 && pos >= nameSrc.Length - 5 ) {              // get pos regardless whether nameSrc ends with .htm (len-4) or .html (len-5)
                            string searchFolder = nameSrc.Substring(0, pos) + "_*";  // search for directories with wildcard *; actual connected folder name is language dependend: _files or _Dateien
                            foreach ( string directory in System.IO.Directory.GetDirectories(pathSrc, searchFolder) ) {
                                src.Add(directory);                                  // take the 1st matching folder
                                htmFolder = Path.GetFileName(directory);
                                break;
                            }
                        }
                    }
                }

                //
                // destination
                //
                if ( bDelete ) {
                    continue;
                }
                if ( indexWPDdst != -1 ) {
                    // WPD folders are not allowed
                    if ( (lvi.ImageIndex != 0) && (lvi.ImageIndex != 3) ) {
                        // in case of WPD, the destination is ALLWAYS the parent folder ID of the passive WPD side
                        WPD wpd = this.m_Panel.GetWPD(this.m_Panel.GetPassiveSide(), this.m_Panel.GetActiveTabIndex(this.m_Panel.GetPassiveSide()));
                        dst.Add(wpd.currentFolderID);
                    }
                    lt = ListType.WPDdst;
                    wpdFolder = pathDst;
                } else {
                    // regular FS
                    dst.Add(Path.Combine(pathDst, nameSrc));
                    // connected htmFolder only if it exists at all
                    if ( htmFolder.Length > 0 ) {
                        string folder = Path.Combine(pathDst, htmFolder);
                        // add connected htmFolder only if it is not existing
                        if ( dst.FirstOrDefault(x => x == folder) == null ) { 
                            dst.Add(folder);
                        }
                    }
                }

                // sanity check
                if ( src.Count > dst.Count ) {
                    src.RemoveAt(src.Count - 1);
                }
                if ( src.Count < dst.Count ) {
                    dst.RemoveAt(dst.Count - 1);
                }
            }

            // if not in delete mode, a destination "Computer" makes no sense
            if ( (pathDst == "Computer") && !bDelete ) {
                return ListType.Error;
            }

            return lt;
        }

        //
        // copy 
        //
        private void buttonF5_Click(object sender, EventArgs e) {
            if ( this.m_Panel.GetActiveView().SelectedIndices.Count == 0 ) {
                return;
            }
            // Desktop & Shared Folders have drives in the ListView - they are not allowed to copy/move/delete/mkdir/rename
            ListViewItem[] lva = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide());
            foreach ( ListViewItem lvi in lva ) {
                if ( (lvi.Text.Length > 1) && (lvi.Text[1] == ':') ) {
                    return;
                }
            }
            string singleFile = "";
            string description;
            if ( this.m_Panel.GetActiveView().SelectedIndices.Count > 1 ) {
                description = "Copy " + this.m_Panel.GetActiveView().SelectedIndices.Count.ToString() + " files/folders to the destination as shown below.";
            } else {
                ListViewItem clvi = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[this.m_Panel.GetActiveView().SelectedIndices[0]];
                description = "Copy '" + clvi.Text + "' to the destination as shown below.";
                singleFile = clvi.Text;
            }
            string destination = this.m_Panel.button(this.m_Panel.GetPassiveSide()).Tag.ToString();
            GrzTools.MouseHook.Start(this);
            using ( crmdDialog dlg = new crmdDialog("Copy", description, destination, true) ) {
                DialogResult dlgr = dlg.ShowDialog(this);
                GrzTools.MouseHook.Stop();
                if ( DialogResult.OK == dlgr ) {
                    if ( dlg.ReturnDestination != destination ) {
                        if ( (singleFile.Length != 0) && (dlg.ReturnDestination[1] != ':') && (dlg.ReturnDestination[1] != '\\') ) {
                            // the operator obviously overrode the path suggestion with just a filename, therefore we take the source folder and copy the given file there
                            string currPath = this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString();
                            string fullSrc = Path.Combine(currPath, singleFile);
                            string fullDst = Path.Combine(currPath, dlg.ReturnDestination);
                            try {
                                File.Copy(fullSrc, fullDst);
                            } catch ( Exception ) {
                                MessageBox.Show("Error copy '" + fullSrc + "' to '" + fullDst + "'");
                                return;
                            }
                        } else {
                            // here we are sure, destination is a folder
                            DialogResult confirm = MessageBox.Show("The folder '" + dlg.ReturnDestination + "' will be created?", "Note", MessageBoxButtons.OKCancel);
                            if ( confirm == DialogResult.OK ) {
                                DirectoryInfo di;
                                try {
                                    di = System.IO.Directory.CreateDirectory(dlg.ReturnDestination);
                                } catch ( Exception ) {
                                    MessageBox.Show("Could not create '" + dlg.ReturnDestination + "'.", "Error creating directory");
                                    return;
                                }
                                MyButtonEventArgs eva = new MyButtonEventArgs();
                                eva.sDestination = di.FullName;
                                this.copySelectionToolStripMenuItem_Click(null, eva);
                            } else {
                                MessageBox.Show("Copy aborted by Operator.");
                                return;
                            }
                        }
                    } else {
                        this.copySelectionToolStripMenuItem_Click(null, null);
                    }
                }
            }
        }
        public int getIndexOfWPD(string wpdFolder) {
            int wpdIndex = -1;
            for ( int i = 0; i < this.m_WPD.Count; i++ ) {
                if ( wpdFolder.StartsWith(this.m_WPD[i].deviceName) ) {
                    wpdIndex = i;
                    break;
                }
            }
            return wpdIndex;
        }
        private void copySelectionToolStripMenuItem_Click(object sender, EventArgs e) {
            this.m_shfoIsActive = true;
            List<string> source = new List<String>();
            List<string> destin = new List<String>();
            string realDestination = null;
            if ( (sender == null) && (e != null) ) {
                MyButtonEventArgs eva = (MyButtonEventArgs)e;
                realDestination = eva.sDestination;
            }
            string wpdFolder = "";
            ListType lt = this.GetSelectedListViewStrings(out source, out destin, realDestination, out wpdFolder);
            if ( lt == ListType.Error ) {
                return;
            }
            if ( source.Count == 0 ) {
                return;
            }

            // standard FS copy
            if ( lt == ListType.FileSystem ) {
                this.m_shfo.SetArguments(new ShfoWorker.Arguments(this.Handle, source, destin, GrzTools.ShellFileOperation.FileOperations.FO_COPY, "Copy", false, false));
                Thread thread = new Thread(this.m_shfo.DoWorkShfo);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }

            // copy "from WPD to FS" OR "from FS to WPD"
            if ( (lt == ListType.WPDsrc) || (lt == ListType.WPDdst) ) {
                try {
                    this.wpdCopy(source, destin, wpdFolder, lt, false);
                } catch {
                    MessageBox.Show("The portable device is not accessible.", "Portable Device");
                }
            }
        }
        void wpdCopy(List<string> source, List<string> destin, string wpdFolder, ListType lt, bool bDeleteAfterCopy) {
            // obtain WPD from list of memorized WPDs
            int wpdIndex = this.getIndexOfWPD(wpdFolder);
            if ( wpdIndex == -1 ) {
                return;
            }
            // progress
            SimpleProgress sp = new SimpleProgress();
            sp.StartPosition = FormStartPosition.Manual;
            sp.Location = new Point(this.Location.X + (this.Width - sp.Width) / 2, this.Location.Y + 100);
            sp.Text = "Copy Files";
            sp.LabelPercent = "0%";
            sp.ProgressValue = 0;
            sp.Show(this);
            int val = (int)(100f / (double)source.Count);

            // select in listview string
            string select = "?";
            if ( bDeleteAfterCopy ) {
                int index = this.m_Panel.GetActiveView().SelectedIndices[0];
                if ( index > 0 ) {
                    index--;
                }
                select = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[index].Text;
            }

            // device connect
            this.m_WPD[wpdIndex].wpd.Connect();

            // copy loop
            bool SrcAndDestWPD = false;
            bool success = true;
            for ( int i = 0; i < source.Count; i++ ) {

                // progress #1
                sp.LabelText = Path.GetFileName(destin[i]);
                sp.ProgressValue += val / 2;
                sp.Invalidate(true);
                Application.DoEvents();

                // copy WPD to FS
                if ( lt == ListType.WPDsrc ) {
                    string[] arr = source[i].Split('?');
                    if ( !this.m_WPD[wpdIndex].wpd.DownloadFileFromWPD(new PortableDevices.PortableDeviceFile(arr[1], arr[0]), Path.GetDirectoryName(destin[i])) ) {
                        success = false;
                    } else {
                        // date time correction 
                        DateTime ft = (DateTime.ParseExact(arr[2], "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal)).ToUniversalTime();
                        File.SetCreationTime(destin[i], ft);
                        File.SetLastWriteTime(destin[i], ft);
                        // move means delete on WPD after copy
                        if ( bDeleteAfterCopy ) {
                            if ( !this.m_WPD[wpdIndex].wpd.DeleteFile(new PortableDevices.PortableDeviceFile(arr[1], "")) ) {
                                success = false;
                            }
                        }
                    }
                }

                // copy 'WPD or FS' to WPD
                if ( lt == ListType.WPDdst ) {
                    bool singleOp = false;

                    // WPD to WPD
                    if ( source[i].Contains('?') ) {
                        SrcAndDestWPD = true;
                        // source is WPD file too --> file operation takes place inside the same WPD
                        string[] arr = source[i].Split('?');

                        // I didn't get it running, therefore the long and ugly way below
                        //singleOp = m_WPD[wpdIndex].wpd.CopyInsideWPD(new PortableDevices.PortableDeviceFile(arr[1], arr[0]), destin[i]);

                        // download file temporary to app path
                        if ( this.m_WPD[wpdIndex].wpd.DownloadFileFromWPD(new PortableDevices.PortableDeviceFile(arr[1], arr[0]), Application.StartupPath) ) {
                            // date time correction 
                            DateTime ft = (DateTime.ParseExact(arr[2], "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal)).ToUniversalTime();
                            File.SetCreationTime(arr[0], ft);
                            File.SetLastWriteTime(arr[0], ft);
                            // upload temporary file
                            if ( this.m_WPD[wpdIndex].wpd.UploadFileToWPD(arr[0], destin[i]) ) {
                                // delete temporary file in app path
                                try {
                                    File.Delete(arr[0]);
                                    // since we came here, singleOp was so far successful
                                    singleOp = true;
                                } catch {; }
                                // move file?
                                if ( singleOp ) {
                                    if ( bDeleteAfterCopy ) {
                                        if ( !this.m_WPD[wpdIndex].wpd.DeleteFile(new PortableDevices.PortableDeviceFile(arr[1], "")) ) {
                                            singleOp = false;
                                        }
                                    }
                                }
                                // when all is done
                                if ( !singleOp ) {
                                    success = false;
                                }
                            }
                        }

                        // FS to WPD
                    } else {
                        // source is regular FS file
                        singleOp = this.m_WPD[wpdIndex].wpd.UploadFileToWPD(source[i], destin[i]);
                        select = "*";
                        if ( !singleOp ) {
                            success = false;
                        } else {
                            // move means delete on FS
                            if ( bDeleteAfterCopy ) {
                                try {
                                    File.Delete(source[i]);
                                } catch {
                                    success = false;
                                }
                            }
                        }
                    }
                }

                // progress #2
                sp.ProgressValue += val / 2;
                sp.LabelPercent = sp.ProgressValue.ToString() + "%";
                sp.Invalidate(true);
                Application.DoEvents();
            }

            // disconnect WPD 
            this.m_WPD[wpdIndex].wpd.Disconnect();

            // refresh listview
            if ( lt == ListType.WPDdst ) {
                this.LoadListView(this.m_Panel.GetPassiveSide(), this.m_Panel.button(this.m_Panel.GetPassiveSide()).Tag.ToString(), select);
                if ( SrcAndDestWPD ) {
                    this.LoadListView(this.m_Panel.GetActiveSide(), this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), select);
                }
            }
            if ( lt == ListType.WPDsrc ) {
                this.LoadListView(this.m_Panel.GetActiveSide(), this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), select);
            }

            // end progress
            sp.Close();

            // final msg
            if ( !success ) {
                MessageBox.Show("Not all files could be copied or moved.", "Error");
            }
        }
        DateTime DateTaken(Image getImage) {
            int DateTakenValue = 0x9003; //36867;
            if ( !getImage.PropertyIdList.Contains(DateTakenValue) )
                return new DateTime(1900, 1, 1);
            string dateTakenTag = System.Text.Encoding.ASCII.GetString(getImage.GetPropertyItem(DateTakenValue).Value);
            string[] parts = dateTakenTag.Split(':', ' ');
            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            int day = int.Parse(parts[2]);
            int hour = int.Parse(parts[3]);
            int minute = int.Parse(parts[4]);
            int second = int.Parse(parts[5]);
            return new DateTime(year, month, day, hour, minute, second);
        }

        //
        // move 
        //
        private void buttonF6_Click(object sender, EventArgs e) {
            if ( this.m_Panel.GetActiveView().SelectedIndices.Count == 0 ) {
                return;
            }
            // Desktop & Shared Folders have drives in the ListView - they are not allowed to copy/move/delete/mkdir/rename
            ListViewItem[] lva = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide());
            foreach ( ListViewItem lvi in lva ) {
                if ( (lvi.Text.Length > 1) && (lvi.Text[1] == ':') ) {
                    return;
                }
            }

            string description;
            if ( this.m_Panel.GetActiveView().SelectedIndices.Count > 1 ) {
                description = "Move " + this.m_Panel.GetActiveView().SelectedIndices.Count.ToString() + " files/folders to the destination as shown below.";
            } else {
                ListViewItem clvi = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[this.m_Panel.GetActiveView().SelectedIndices[0]];
                description = "Move '" + clvi.Text + "' to the destination as shown below.";
            }
            string destination = this.m_Panel.button(this.m_Panel.GetPassiveSide()).Tag.ToString();
            GrzTools.MouseHook.Start(this);
            using ( crmdDialog dlg = new crmdDialog("Move", description, destination, true) ) {
                DialogResult dlgr = dlg.ShowDialog(this);
                GrzTools.MouseHook.Stop();
                if ( DialogResult.OK == dlgr ) {
                    if ( dlg.ReturnDestination != destination ) {
                        DialogResult confirm = MessageBox.Show("The folder '" + dlg.ReturnDestination + "' will be created?", "Note", MessageBoxButtons.OKCancel);
                        if ( confirm == DialogResult.OK ) {
                            DirectoryInfo di;
                            try {
                                di = System.IO.Directory.CreateDirectory(dlg.ReturnDestination);
                            } catch ( Exception ) {
                                MessageBox.Show("Could not create '" + dlg.ReturnDestination + "'.", "Error creating directory");
                                return;
                            }
                            MyButtonEventArgs eva = new MyButtonEventArgs();
                            eva.sDestination = di.FullName;
                            this.moveSelectionF6ToolStripMenuItem_Click(null, eva);
                        } else {
                            MessageBox.Show("Move aborted by Operator.");
                            return;
                        }
                    } else {
                        this.moveSelectionF6ToolStripMenuItem_Click(null, null);
                    }
                }
            }
        }
        private void moveSelectionF6ToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.m_bBlockListViewActivity ) { // a listview load > 500 items is not finished yet
                return;
            }
            this.m_shfoIsActive = true;
            List<string> source = new List<String>();
            List<string> destin = new List<String>();
            string realDestination = null;
            if ( (sender == null) && (e != null) ) {
                MyButtonEventArgs eva = (MyButtonEventArgs)e;
                realDestination = eva.sDestination;
            }
            // get lists containing sources and destinations
            string wpdFolder = "";
            ListType lt = this.GetSelectedListViewStrings(out source, out destin, realDestination, out wpdFolder);
            if ( source.Count == 0 ) {
                return;
            }

            // 20160821: in case one of the files in source files is active in preview, we unload it from preview
            foreach ( string fn in source ) {
                if ( this.m_LastPreview == fn ) {
                    this.m_LastPreview = "";
                    this.previewCtl.LoadDocument("", "", this.m_Panel.GetActiveView());
                }
            }
            // move on FS
            if ( lt == ListType.FileSystem ) {
                this.m_shfo.SetArguments(new ShfoWorker.Arguments(this.Handle, source, destin, GrzTools.ShellFileOperation.FileOperations.FO_MOVE, "Move", false, false));
                Thread thread = new Thread(this.m_shfo.DoWorkShfo);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
            // copy + delete "from WPD to FS" OR "from FS to WPD"
            if ( (lt == ListType.WPDsrc) || (lt == ListType.WPDdst) ) {
                this.wpdCopy(source, destin, wpdFolder, lt, true);
            }
        }

        //
        // new --> ... (from file menu AND from F7)
        //
        private void buttonF7_Click(object sender, EventArgs e) {
            // prevent creating more then wanetd and stop editing the new name
            if ( this.buttonF9.Text.Contains("Escape") ) {
                this.QuitRename();
                return;
            }

            if ( (ModifierKeys & Keys.Shift) == Keys.Shift ) {
                this.fileToolStripMenuItem_DropDownOpened(null, null);
            } else {
                this.newFolderToolStripMenuItem_Click(null, null);
            }
        }
        // --> ... new folder
        async private void newFolderToolStripMenuItem_Click(object sender, EventArgs e) {
            // Desktop & Shared Folders have drives in the ListView - they are not allowed to copy/move/delete/mkdir/rename
            ListViewItem[] lva = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide());
            foreach ( ListViewItem lvi in lva ) {
                if ( (lvi.Text.Length > 1) && (lvi.Text[1] == ':') ) {
                    return;
                }
            }

            // WPD entry point
            string currentFolder = this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString();
            int wpdIndex = this.getIndexOfWPD(currentFolder);
            if ( wpdIndex != -1 ) {
                SimpleInput dlg = new SimpleInput();
                dlg.Text = "Please provide a folder name";
                dlg.Hint = "";
                dlg.Input = "NewFolder";
                dlg.ShowDialog();
                if ( dlg.DialogResult != DialogResult.OK ) {
                    return;
                }
                this.m_WPD[wpdIndex].wpd.Connect();
                this.m_WPD[wpdIndex].wpd.CreateFolder(dlg.Input, this.m_Panel.GetWPD(this.m_Panel.GetActiveSide(), this.m_Panel.GetActiveTabIndex(this.m_Panel.GetActiveSide())).currentFolderID);
                this.m_WPD[wpdIndex].wpd.Disconnect();
                await this.LoadListView(this.m_Panel.GetActiveSide(), this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), dlg.Input);
                return;
            }

            // generate a new directory and take care if "New Folder" already exists
            string newFolder = Path.Combine(currentFolder, "New Folder");
            int counter = 0;
            while ( System.IO.Directory.Exists(newFolder) ) {
                newFolder = Path.Combine(this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), "New Folder(" + counter++ + ")");
            }
            try {
                System.IO.Directory.CreateDirectory(newFolder);
            } catch ( Exception ) {
                MessageBox.Show("User privileges do not allow to create a folder here.", "Error");
                return;
            }
            // asap stop all file system monitoring activities
            this.timerFileSystemMonitorLeft.Stop();
            this.timerFileSystemMonitorRight.Stop();
            this.m_bFileSystemChangeActionLeft = false;
            this.m_bFileSystemChangeActionRight = false;
            // re load listviews manually
            if ( this.m_Panel.button(Side.left).Tag.ToString() == this.m_Panel.button(Side.right).Tag.ToString() ) {
                await this.LoadListView(Side.left, this.m_Panel.button(Side.left).Tag.ToString(), "");
                await this.LoadListView(Side.right, this.m_Panel.button(Side.right).Tag.ToString(), "");
            } else {
                await this.LoadListView(this.m_Panel.GetActiveSide(), this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), "");
            }
            // select the recently generated directory
            string text = Path.GetFileName(newFolder);
            this.m_Panel.GetActiveView().SelectedIndices.Clear();
            try {
                ListViewItem lvi = this.m_Panel.FindListViewArrItemWithText(this.m_Panel.GetActiveSide(), text, -1);
                lvi.Selected = true;
                this.m_Panel.GetActiveView().SelectedIndices.Add((int)lvi.Tag);
                // rename the recently generated directory
                this.BeginRename();
            } catch ( Exception ) {; }
        }
        // --> ... new file
        async private void fileToolStripMenuItem_DropDownOpened(object sender, EventArgs e) {
            // WPD entry point
            string currentFolder = this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString();
            int wpdIndex = this.getIndexOfWPD(currentFolder);
            if ( wpdIndex != -1 ) {
                MessageBox.Show("Creation of files on WPD is not implemented.", "Sorry");
                return;
            }

            // generate a new file and take care if "New File" already exists
            string newFile = Path.Combine(currentFolder, "NewFile.ext");
            int counter = 0;
            while ( File.Exists(newFile) ) {
                newFile = Path.Combine(currentFolder, "NewFile(" + counter++ + ").ext");
            }
            try {
                FileStream fs = File.Create(newFile);
                fs.Close();
            } catch ( Exception ) {
                MessageBox.Show("User privileges do not allow to create a file here.", "Error");
                return;
            }
            // asap stop all file system monitoring activities
            this.timerFileSystemMonitorLeft.Stop();
            this.timerFileSystemMonitorRight.Stop();
            this.m_bFileSystemChangeActionLeft = false;
            this.m_bFileSystemChangeActionRight = false;
            // re load listviews manually
            if ( this.m_Panel.button(Side.left).Tag.ToString() == this.m_Panel.button(Side.right).Tag.ToString() ) {
                await this.LoadListView(Side.left, this.m_Panel.button(Side.left).Tag.ToString(), "");
                await this.LoadListView(Side.right, this.m_Panel.button(Side.right).Tag.ToString(), "");
            } else {
                await this.LoadListView(this.m_Panel.GetActiveSide(), this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), "");
            }
            // select the recently generated entry
            string fileNameOnly = Path.GetFileName(newFile);
            this.m_Panel.GetActiveView().SelectedIndices.Clear();
            try {
                ListViewItem lvi = this.m_Panel.FindListViewArrItemWithText(this.m_Panel.GetActiveSide(), fileNameOnly, -1);
                this.m_Panel.GetActiveView().EnsureVisible((int)lvi.Tag);
                this.m_Panel.GetActiveView().SelectedIndices.Add((int)lvi.Tag);
                // rename the recently generated file
                this.BeginRename();
            } catch ( Exception ) {; }
        }

        //
        // 20161016: rename a selection of files
        //
        async private void renameSelectionToolStripMenuItem_Click(object sender, EventArgs e) {
            // sanity check skips no selection and [..] 
            ListView lv = this.m_Panel.listview(this.m_Panel.GetActiveSide());
            if ( lv.SelectedIndices.Count == 0 ) {
                return;
            }
            if ( lv.Items[0].Selected ) {
                return;
            }

            // get rename pattern
            SimpleInput dlg = new SimpleInput();
            dlg.Text = "Please provide a rename rule (see 2nd parameter: CMD rename /?)";
            dlg.Hint = "The rename rule may contain characters and wildcards ? and *. Any rename conflict will be simply skipped.\r\rhttp://superuser.com/questions/475874/how-does-the-windows-rename-command-interpret-wildcards";
            dlg.Input = "*.*";
            dlg.ShowDialog();
            if ( dlg.DialogResult != DialogResult.OK ) {
                return;
            }
            string pattern = dlg.Input;

            // get containing folder name
            string folder = this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString();

            // get selected files&folders 
            List<int> lstSel = new List<int>();
            List<string> lst = new List<string>();
            for ( int i = 0; i < lv.SelectedIndices.Count; i++ ) {
                lst.Add(lv.Items[lv.SelectedIndices[i]].Text);
                lstSel.Add(lv.Items[lv.SelectedIndices[i]].Index);
            }

            // if fs watcher would do the job, we cannot keep the current selection 
            this.fileSystemWatcherLeft.EnableRaisingEvents = false;
            this.fileSystemWatcherRight.EnableRaisingEvents = false;

            // apply renaming rule via CMD.EXE
            bool bError = false;
            foreach ( string s in lst ) {
                Process p = new Process();
                p.StartInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe");
                p.StartInfo.Arguments = String.Format("/c ren \"{0}\" \"{1}\"", Path.Combine(folder, s), pattern);
                p.StartInfo.WorkingDirectory = folder;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                while ( !p.HasExited ) {
                    Thread.Sleep(10);
                }
                bError = Convert.ToBoolean(p.ExitCode);
            }

            // list view reload
            await this.LoadListView(this.m_Panel.GetActiveSide(), this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), "?");
            // restore original selection (doesn't work thru LoadListview with '?', because the originally selected files do not exist anymore after renaming)
            foreach ( int i in lstSel ) {
                lv.Items[i].Selected = true;
            }
            // take care about other listview
            if ( this.m_Panel.button(Side.left).Tag.ToString() == this.m_Panel.button(Side.right).Tag.ToString() ) {
                await this.LoadListView(this.m_Panel.GetPassiveSide(), this.m_Panel.button(this.m_Panel.GetPassiveSide()).Tag.ToString(), "?");
            }
            // restart fs watchers
            this.fileSystemWatcherLeft.EnableRaisingEvents = true;
            this.fileSystemWatcherRight.EnableRaisingEvents = true;
            // error?
            if ( bError ) {
                MessageBox.Show("Not all files could be renamed.", "Error");
            }
        }

        //
        // delete: core functions come from Microsoft.VisualBasic.FileIO.FileSystem --> DeleteDirectory & DeleteFile
        //
        public class MyButtonEventArgs : EventArgs {
            public string sDestination { get; set; }
        }
        // two entry points here: for "button F8" itself & from "file menu F8"
        private void buttonF8_Click(object sender, EventArgs e) {
            if ( this.m_Panel.GetActiveView().SelectedIndices.Count == 0 ) {
                return;
            }

            // Desktop & Shared Folders have drives in the ListView - they are not allowed to copy/move/delete/mkdir/rename
            ListViewItem[] lva = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide());
            foreach ( ListViewItem lvi in lva ) {
                if ( (lvi.Text.Length > 1) && (lvi.Text[1] == ':') ) {
                    return;
                }
            }
            string description;
            if ( this.m_Panel.GetActiveView().SelectedIndices.Count > 1 ) {
                description = "Delete " + this.m_Panel.GetActiveView().SelectedIndices.Count.ToString() + " files/folders.";
            } else {
                ListViewItem clvi = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[this.m_Panel.GetActiveView().SelectedIndices[0]];
                string selection = clvi.Text;
                if ( (selection == "[..]") && (clvi.ImageIndex == 2) ) { // aka "LevelUp"
                    return;
                }
                description = "Delete '" + selection + "'.";
            }
            string destination = "";
            GrzTools.MouseHook.Start(this);
            using ( crmdDialog dlg = new crmdDialog("Delete", description, destination, true) ) {
                DialogResult dlgr = dlg.ShowDialog(this);
                GrzTools.MouseHook.Stop();
                if ( DialogResult.OK == dlgr ) {
                    MyButtonEventArgs eva = new MyButtonEventArgs();
                    if ( dlg.ReturnDestination == "::trash::" ) {
                        eva.sDestination = dlg.ReturnDestination;
                    } else {
                        eva.sDestination = "::reallydelete::";
                    }
                    this.deleteSelectionF8ToolStripMenuItem_Click(null, eva);
                }
            }
        }
        // entry point for right click popup menu
        private void deleteSelectionF8ToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.m_bBlockListViewActivity ) { // a listview load > 500 items is not finished yet
                return;
            }
            if ( this.m_Panel.GetActiveView().SelectedIndices.Count == 0 ) {
                return;
            }

            // prepare recycler: PopupMenu ---> sender != null, F8 ---> sender == null 
            MyButtonEventArgs eva = new MyButtonEventArgs();
            if ( sender != null ) {
                eva.sDestination = "::trash::";
            } else {
                eva = (MyButtonEventArgs)e;
            }

            // like in explorer: holding shift key while clicking "delete to trash" in PopupMenu shall truly delete
            if ( (ModifierKeys & Keys.Shift) == Keys.Shift ) {
                DialogResult dr = MessageBox.Show("You really want to delete the selection permanently?", "Attention", MessageBoxButtons.OKCancel);
                if ( dr == DialogResult.Cancel ) {
                    return;
                }
                eva.sDestination = "::reallydelete::";
            }

            // what do we want to delete, essentially we only need "source", "destin" isn't even touched
            List<string> source = new List<String>();
            List<string> destin = new List<String>();
            string wpdFolder = "";
            ListType lt = this.GetSelectedListViewStrings(out source, out destin, null, out wpdFolder, true);
            if ( source.Count == 0 ) {
                return;
            }

            // get first item above & outside of selection -- that's the one we select after deletion is done
            Side side = this.m_Panel.GetActiveSide();
            ListView lv = this.m_Panel.GetActiveView();
            int index = lv.SelectedIndices[0];
            if ( index > 0 ) {
                index--;
            }
            string utext = lv.Items[index].Text;
            if ( index + 2 < lv.Items.Count ) {
                index += 2;
            }
            string ltext = lv.Items[index].Text;
            string text = utext;

            // refine recycle bin operation
            RecycleOption ro = RecycleOption.DeletePermanently;
            if ( eva.sDestination == "::trash::" ) {
                ro = RecycleOption.SendToRecycleBin;
            }

            // check whether drive has a recycle bin at all
            if ( ro == RecycleOption.SendToRecycleBin ) {
                string root = "";
                try {
                    root = Path.GetPathRoot(source[0]);
                } catch ( PathTooLongException ) {
                    MessageBox.Show(source[0], "Path + Filename too long");
                    return;
                }
                if ( !GrzTools.FileTools.DriveHasRecycleBin(root) ) {
                    string txt = Localizer.GetString("notrash");
                    DialogResult dr = MessageBox.Show(txt, "Attention", MessageBoxButtons.OKCancel);
                    if ( dr == DialogResult.Cancel ) {
                        return;
                    }
                    ro = RecycleOption.DeletePermanently;
                }
            }

            // in case the file is shown in preview, we first need to release it from preview
            if ( this.previewCtl.Visible ) {
                this.previewCtl.Clear();
                text = ltext;
            }

            // init progress dlg
            SimpleProgress sp = new SimpleProgress();
            sp.StartPosition = FormStartPosition.Manual;
            sp.Location = new Point(this.Location.X + (this.Width - sp.Width) / 2, this.Location.Y + 100);
            sp.Text = "Delete Progress";
            sp.LabelPercent = "0%";
            sp.ProgressValue = 0;
            sp.ShowCancelButton = true;
            sp.Show(this);

            // there is a need to distinguish between FS und WPD files/folders
            if ( (lt == ListType.FileSystem) || (lt == ListType.WPDdst) ) {
                // stop all file system monitoring activities: delete would otherwise generate a flood of change events
                this.fileSystemWatcherLeft.EnableRaisingEvents = false;
                this.fileSystemWatcherRight.EnableRaisingEvents = false;
                // start backgroundworker for delete: deletion on slow media could be a long lasting thing
                BackgroundWorker bg = new BackgroundWorker();
                bg.WorkerReportsProgress = true;
                bg.DoWork += new DoWorkEventHandler(bg_DoWorkDelete);
                bg.ProgressChanged += new ProgressChangedEventHandler(this.bg_DoWorkDeleteProgressChanged);
                bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.bg_DoWorkDeleteCompleted);
                bg.RunWorkerAsync(new DoWorkDeleteArgs(source, ro, text, sp));
            }
            if ( lt == ListType.WPDsrc ) {
                // obtain WPD from list of memorized WPDs
                int wpdIndex = this.getIndexOfWPD(wpdFolder);
                if ( wpdIndex == -1 ) {
                    sp.Close();
                    return;
                }
                this.m_WPD[wpdIndex].wpd.Connect();
                int i = 1;
                int max = source.Count * 2;
                double pct = 100.0f / Math.Max(1, max);
                bool success = true;
                // execute deletion 
                foreach ( string entry in source ) {
                    // cancel op
                    if ( sp != null && !sp.Visible ) {
                        break;
                    }
                    sp.LabelPercent = ((int)(i * pct)).ToString() + "%";
                    sp.ProgressValue += (int)pct;
                    string[] arr = entry.Split('?');
                    if ( !this.m_WPD[wpdIndex].wpd.DeleteFile(new PortableDevices.PortableDeviceFile(arr[1], "")) ) {
                        success = false;
                    }
                    i++;
                    sp.LabelPercent = ((int)(i * pct)).ToString() + "%";
                    sp.ProgressValue += (int)pct;
                }
                this.m_WPD[wpdIndex].wpd.Disconnect();
                this.LoadListView(side, this.m_Panel.button(side).Tag.ToString(), utext);
                // close progress
                if ( sp != null ) {
                    sp.Close();
                }
                if ( !success ) {
                    MessageBox.Show("Not all items could be deleted", "Delete Error");
                }
            }
        }
        // bgw start arguments structure
        struct DoWorkDeleteArgs {
            public DoWorkDeleteArgs(List<string> source, RecycleOption ro, string selecttext, SimpleProgress sp, bool deleteFailed = false) {
                this.source = source;
                this.ro = ro;
                this.selecttext = selecttext;
                this.sp = sp;
                this.deleteFailed = deleteFailed;
            }
            public List<string> source;
            public RecycleOption ro;
            public string selecttext;
            public SimpleProgress sp;
            public bool deleteFailed;
        }
        // bgw DoWork
        static void bg_DoWorkDelete(object sender, DoWorkEventArgs e) {
            SimpleProgress sp = ((DoWorkDeleteArgs)e.Argument).sp;
            string returnText = ((DoWorkDeleteArgs)e.Argument).selecttext;

            BackgroundWorker worker = (BackgroundWorker)sender;
            List<string> source = ((DoWorkDeleteArgs)e.Argument).source;
            RecycleOption ro = ((DoWorkDeleteArgs)e.Argument).ro;
            bool bDeleteConfirmation = true;
            bool bDeleteFailed = false;
            // progress preset
            int i = 1;
            int max = source.Count * 2;
            double pct = 100.0f / Math.Max(1, max);
            // execute deletion 
            foreach ( string entry in source ) {
                // cancel op
                if ( sp != null && !sp.Visible ) {
                    returnText = "aborted";
                    break;
                }
                // single file delete result
                bDeleteFailed = false;
                // report progress
                DoWorkDeleteArgs dwda = new DoWorkDeleteArgs(null, ro, entry, sp);
                worker.ReportProgress((int)((i++) * pct), dwda);
                // if for whatever reason [..] is selected, it will throw an exception
                try {
                    FileAttributes attr = File.GetAttributes(@entry);
                    // any deletion is tricky 
                    if ( (attr & FileAttributes.Directory) == FileAttributes.Directory ) {
                        try {
                            // deleting folder looks a bit dangerous ... 
                            int delCnt = GrzTools.FastFileFind.FileFolderCount(@entry, "*.*");
                            if ( (ro == RecycleOption.DeletePermanently) && (delCnt > 200) ) {
                                // extra confirm folder deletion containing >200 items
                                if ( bDeleteConfirmation ) {
                                    string message = "Do you want to EXTRA confirm the deletion of folders containing >200 items?";
                                    string caption = "Confirm Large Folder Deletion";
                                    int res = GrzTools.Native.MessageBoxTopYesNo(message, caption); // a normal MessageBox could be overlapped by MainForm
                                    if ( res == 7 ) { // aka NO
                                        bDeleteConfirmation = false;
                                    }
                                }
                                // fully fledged UI
                                FileSystem.DeleteDirectory(entry, bDeleteConfirmation ? UIOption.AllDialogs : UIOption.OnlyErrorDialogs, ro);
                            } else {
                                FileSystem.DeleteDirectory(entry, UIOption.OnlyErrorDialogs, ro);
                            }
                        } catch ( OperationCanceledException ) {
                            bDeleteFailed = true;
                        } catch ( Exception ) {
                            bDeleteFailed = true;
                        }
                    } else {
                        try {
                            FileSystem.DeleteFile(entry, UIOption.OnlyErrorDialogs, ro);
                        } catch ( OperationCanceledException ) {
                            bDeleteFailed = true;
                        } catch ( Exception ) {
                            bDeleteFailed = true;
                        }
                    }
                } catch ( Exception ) {
                    bDeleteFailed = true;
                }

                if ( bDeleteFailed ) {
                    int dr = GrzTools.Native.MessageBoxTopYesNo("Abort current operation?", "Abort"); // a normal MessageBox could be overlapped by MainForm
                    if ( dr == 6 ) {  // aka YES
                        Thread.Sleep(1000);
                        e.Result = new DoWorkDeleteArgs(null, RecycleOption.SendToRecycleBin, entry, sp, true);
                        return;
                    }
                }

                // report progress
                worker.ReportProgress((int)((i++) * pct), new DoWorkDeleteArgs(null, ro, entry + " - deleted", sp));
            }
            // w/o this sleep, Progress doesn't show nicely
            Thread.Sleep(1000);
            // forward the item text to select to "Completed" and more
            e.Result = new DoWorkDeleteArgs(null, ro, returnText, sp);
        }
        // bgw Progress
        private void bg_DoWorkDeleteProgressChanged(object sender, ProgressChangedEventArgs e) {
            SimpleProgress sp = ((DoWorkDeleteArgs)e.UserState).sp;

            sp.LabelText = "";
            sp.LabelText = ((DoWorkDeleteArgs)e.UserState).selecttext;
            sp.LabelPercent = (e.ProgressPercentage.ToString() + "%");
            sp.ProgressValue = e.ProgressPercentage;
        }
        // bgw Completed
        void bg_DoWorkDeleteCompleted(object sender, RunWorkerCompletedEventArgs e) {
            // item text to select
            string text = ((DoWorkDeleteArgs)e.Result).selecttext;
            if ( ((DoWorkDeleteArgs)e.Result).deleteFailed ) {
                text = "?";
            }
            SimpleProgress sp = ((DoWorkDeleteArgs)e.Result).sp;

            // re load listviews manually: filesystemwatcher was turned off during delete due to too many events
            string lft = this.m_Panel.button(Side.left).Tag.ToString();
            string rgt = this.m_Panel.button(Side.right).Tag.ToString();
            if ( lft == rgt ) {
                this.LoadListView(Side.left, lft, text);
                this.LoadListView(Side.right, rgt, text);
            } else {
                // 20161016: left view shows folder having a subfolder AND right view shows exactly this subfolder - if a file inside the subfolder is deleted, the left view needs a refresh 
                if ( lft.StartsWith(rgt) || rgt.StartsWith(lft) ) {
                    this.LoadListView(Side.left, lft, "?");
                    this.LoadListView(Side.right, rgt, "?");
                } else {
                    this.LoadListView(this.m_Panel.GetActiveSide(), this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), text);
                }
            }

            // start all file system monitoring activities
            try {
                if ( System.IO.Directory.Exists(this.fileSystemWatcherLeft.Path) && this.filesystemMonitoringToolStripMenuItem.Checked ) {
                    this.fileSystemWatcherLeft.EnableRaisingEvents = true;
                }
                if ( System.IO.Directory.Exists(this.fileSystemWatcherRight.Path) && this.filesystemMonitoringToolStripMenuItem.Checked ) {
                    this.fileSystemWatcherRight.EnableRaisingEvents = true;
                }
            } catch ( Exception ) {; }

            // select at least something, unless drive is empty
            if ( this.m_listViewL.SelectedIndices.Count == 0 ) {
                if ( this.m_listViewL.Items.Count > 0 ) {
                    this.m_listViewL.Items[0].Selected = true;
                } else {
                    this.m_Panel.RenderListviewLabel(Side.left);
                }
            }
            if ( this.m_listViewR.SelectedIndices.Count == 0 ) {
                if ( this.m_listViewR.Items.Count > 0 ) {
                    this.m_listViewR.Items[0].Selected = true;
                } else {
                    this.m_Panel.RenderListviewLabel(Side.right);
                }
            }

            // close progress
            if ( sp != null ) {
                sp.Close();
            }

            // show error msg
            if ( text == "error" ) {
                MessageBox.Show("Not all files/folders could be deleted.", "Error");
            }
            if ( text == "aborted" ) {
                MessageBox.Show("Not all files/folders were deleted.", "User aborted");
            }
        }

        //
        // send files/folders to ZIP Archive
        //
        bool _bZipBreak = false;
        private void zIPFilesToolStripMenuItem_Click(object sender, EventArgs e) {
            // organize async break via "Cancel Window" --> event is sent to this method in MainThread --> set a global variable, which affects the zipping method
            if ( sender is Button ) {
                if ( ((Button)(sender)).Text == "cancel" ) {
                    this._bZipBreak = true;
                }
                return;
            }

            // 20161016 zip with password
            string pwd = "";
            if ( this.zIPFilesToolStripMenuItem.Text == "Make ZIP with Password" ) {
                SimpleInput sdlg = new SimpleInput();
                sdlg.Hint = "Please type a Password";
                sdlg.Input = "Type Password here ...";
                sdlg.Text = "Type Password into the edit box";
                sdlg.ShowDialog();
                if ( sdlg.DialogResult == DialogResult.OK ) {
                    pwd = sdlg.Input;
                }
                if ( pwd.Length == 0 ) {
                    MessageBox.Show("Since the Password length is Zero, there will be no Password assigned to the ZIP archive.", "Note");
                }
            }

            // Desktop & Shared Folders have drives in the ListView - they are not allowed to copy/move/delete/mkdir/rename/MakeZip
            ListViewItem[] lva = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide());
            foreach ( ListViewItem lvi in lva ) {
                if ( (lvi.Text.Length > 1) && (lvi.Text[1] == ':') ) {
                    this.renameToolStripMenuItem1.Checked = false;
                    return;
                }
            }

            // files & folders to ZIP
            List<string> source = new List<String>();
            List<string> destin = new List<String>();
            string wpdFolder = "";
            ListType lt = this.GetSelectedListViewStrings(out source, out destin, null, out wpdFolder, true); // need to set flag "delete", although we don't delete at all
            if ( lt != ListType.FileSystem ) {
                return;
            }
            if ( source.Count == 0 ) {
                return;
            }

            // zip filename
            string zipfile = Path.Combine(Path.GetDirectoryName(source[0]), Path.GetFileNameWithoutExtension(source[0]) + ".zip");
            if ( File.Exists(zipfile) ) {
                DialogResult dr = MessageBox.Show("File '" + zipfile + "' already exists. Overwrite it?", "Warning", MessageBoxButtons.YesNo);
                if ( dr != DialogResult.Yes ) {
                    return;
                }
            }

            // asynchronously start zipping as single task: with params & return value
            Task<string> t = new Task<string>(() => this.MakeZip(zipfile, source, pwd, ref this._bZipBreak));
            t.Start();

            // show a "Cancel Operation" window
            this._bZipBreak = false;
            CancelDialog dlg = new CancelDialog();
            dlg.WantClose += new EventHandler<EventArgs>(this.zIPFilesToolStripMenuItem_Click);
            dlg.Location = new Point(MousePosition.X - 50, MousePosition.Y + 25);
            dlg.Show();

            // show some lame progress in title bar of main window
            string sBusy = "Generating ZIP file - you better not close this application until this text disappears";
            Size textSize = TextRenderer.MeasureText(sBusy, this.Font);
            Size dotSize = TextRenderer.MeasureText(" .", this.Font);
            this.Text = sBusy;
            int dotCnt = 2 * (this.Width - textSize.Width) / dotSize.Width;
            Stopwatch sw = Stopwatch.StartNew();

            // cooperative wait for end of zipping, aka task t is completed
            do {
                Application.DoEvents();
                if ( sw.ElapsedMilliseconds > 200 ) {
                    this.Text += " .";
                    if ( this.Text.Length - sBusy.Length > dotCnt ) {
                        this.Text = sBusy;
                    }
                    sw = Stopwatch.StartNew();
                }
            } while ( !t.IsCompleted );

            // zipping is done
            sw.Stop();
            dlg.WantClose -= this.zIPFilesToolStripMenuItem_Click;
            dlg.Close();
            this.Text = this.Tag.ToString();
            if ( t.Result.Contains("ERROR:") ) {
                MessageBox.Show(t.Result, "ZIP ERROR");
            } else {
                string zipfileResult = t.Result;
                // success
                GrzTools.AutoMessageBox.Show("Zip file generation finished\n\n" + zipfileResult, "Success", 2000);
                // select generated ZIP
                ListViewItem[] lviarr = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide());
                this.m_Panel.GetActiveView().SelectedIndices.Clear();
                foreach ( ListViewItem lvi in lviarr ) {
                    if ( zipfile.EndsWith(lvi.Text) && (lvi.Index != -1) ) {
                        lvi.Selected = true;
                        this.m_Panel.GetActiveView().EnsureVisible(lvi.Index);     // 20161016 mysterious: lvi.Index could be -1, all other lvi properties are ok 
                        break;
                    }
                }
                // 20160206: if nothing is selected, it's likely filesystemwatcher didn't detect the new file - observed on a mapped network drive (\plenaxis)
                if ( this.m_Panel.GetActiveView().SelectedIndices.Count == 0 ) {
                    this.LoadListView(this.m_Panel.GetActiveSide(), this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), Path.GetFileName(zipfile));
                    if ( this.m_Panel.button(Side.left).Tag.ToString() == this.m_Panel.button(Side.right).Tag.ToString() ) {
                        this.LoadListView(this.m_Panel.GetPassiveSide(), this.m_Panel.button(this.m_Panel.GetPassiveSide()).Tag.ToString(), Path.GetFileName(zipfile));
                    }
                }
            }
        }
        public string MakeZip(string zipfile, List<string> source, string pwd, ref bool bbreak) {
            if ( source.Count == 0 ) {
                return "ERROR: nothing to ZIP";
            }
            string retval = zipfile;
            ZipOutputStream zip;
            try {
                zip = new ZipOutputStream(File.Create(zipfile));
                zip.Password = pwd;
                //                zip.SetLevel(9);
            } catch ( Exception ) {
                return "ERROR: cannot create ZIP";
            }
            foreach ( string entry in source ) {
                FileAttributes attr = File.GetAttributes(@entry);
                try {
                    if ( (attr & FileAttributes.Directory) == FileAttributes.Directory ) {
                        ZipFolder(Path.GetDirectoryName(@entry), @entry, zip, ref bbreak);
                    } else {
                        AddFileToZip(zip, "", @entry, ref bbreak);
                    }
                } catch ( Exception ) {
                    retval = "ERROR: ZIP File Generation " + zipfile;
                }
                if ( bbreak ) {
                    retval = "ERROR: ZIP File Generation aborted by Operator";
                    break;
                }
            }
            zip.Finish();
            zip.Close();

            return retval;
        }
        public static void ZipFolder(string RootFolder, string CurrentFolder, ZipOutputStream zStream, ref bool bbreak) {
            if ( bbreak ) {
                return;
            }
            string[] SubFolders = System.IO.Directory.GetDirectories(CurrentFolder);
            foreach ( string Folder in SubFolders ) {
                ZipFolder(RootFolder, Folder, zStream, ref bbreak);
            }
            string relativePath = CurrentFolder.Substring(RootFolder.Length) + "\\";
            if ( relativePath.Length > 1 ) {
                DirectoryInfo di = new DirectoryInfo(relativePath);
                ZipEntry dirEntry;
                dirEntry = new ZipEntry(ZipEntry.CleanName(relativePath));
                dirEntry.DateTime = di.LastWriteTime;
            }
            foreach ( string file in System.IO.Directory.GetFiles(CurrentFolder) ) {
                if ( bbreak ) {
                    return;
                }
                AddFileToZip(zStream, relativePath, file, ref bbreak);
            }
        }
        private static void AddFileToZip(ZipOutputStream zStream, string relativePath, string file, ref bool bbreak) {
            FileInfo fi = new FileInfo(file);
            byte[] buffer = new byte[4096];
            string fileRelativePath = (relativePath.Length > 1 ? relativePath : string.Empty) + Path.GetFileName(file);
            ZipEntry entry = new ZipEntry(ZipEntry.CleanName(fileRelativePath));    // crucial: ZipEntry.CleanName(..) ensures, win explorer can open & show the zip 
            entry.DateTime = fi.LastWriteTime;
            zStream.PutNextEntry(entry);
            using ( FileStream fs = File.OpenRead(file) ) {
                int sourceBytes;
                do {
                    sourceBytes = fs.Read(buffer, 0, buffer.Length);
                    zStream.Write(buffer, 0, sourceBytes);
                } while ( (sourceBytes > 0) && !bbreak );
            }
        }

        // "Computer" view has its own context menu, one entry allows to rename drives
        private void renameToolStripMenuItem2_Click(object sender, EventArgs e) {
            if ( this.renameToolStripMenuItem2.Checked ) {
                this.BeginRename();
            } else {
                this.ExecuteRename();
            }
        }
        // rename via "FileMenu"
        private void renameToolStripMenuItem1_Click(object sender, EventArgs e) {
            // no selection == no action
            ListView lv = this.m_Panel.GetActiveView();
            if ( lv.SelectedIndices.Count > 1 ) {
                this.renameToolStripMenuItem1.Checked = false;
                return;
            }
            // Desktop & Shared Folders are not allowed to copy/move/delete/mkdir/rename
            Side side = this.m_Panel.GetActiveSide();
            ListViewItem[] lvarr = this.m_Panel.GetListViewArr(side);
            if ( (this.m_Panel.button(side).Tag.ToString() == "Computer") && (lvarr[lv.SelectedIndices[0]].Text[1] != ':') ) {
                this.renameToolStripMenuItem1.Checked = false;
                return;
            }
            // start/stop rename 
            if ( this.renameToolStripMenuItem1.Checked ) {
                this.BeginRename();
            } else {
                this.ExecuteRename();
            }
        }
        // rename via "ListView" ContextMenu - NOTE: this handler will nevvr get called from "Computer" view, because that view has its own context menu
        private void renameToolStripMenuItem_Click(object sender, EventArgs e) {
            // Desktop & Shared Folders have drives in the ListView - they are not allowed to copy/move/delete/mkdir/rename
            ListViewItem[] lva = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide());
            foreach ( ListViewItem lvi in lva ) {
                if ( (lvi.Text.Length > 1) && (lvi.Text[1] == ':') ) {
                    return;
                }
            }

            if ( this.renameToolStripMenuItem.Checked ) {
                this.BeginRename();
            } else {
                this.ExecuteRename();
            }
        }
        private void buttonF9_MouseDown(object sender, MouseEventArgs e) {
            // LostFocus happens prior to F9ButtonDown, therefore F9ButtonDown restarts renaming
            if ( (DateTime.Now - this.m_dtLostFocus).TotalMilliseconds < 100 ) {
                return;
            }
            if ( this.buttonF9.Text == "- Escape -" ) {
                this.QuitRename();
            } else {
                // Desktop & Shared Folders have drives in the ListView - they are not allowed to copy/move/delete/mkdir/rename
                ListViewItem[] lva = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide());
                foreach ( ListViewItem lvi in lva ) {
                    if ( (lvi.Text.Length > 1) && (lvi.Text[1] == ':') ) {
                        return;
                    }
                }
                this.BeginRename();
            }
        }
        void BeginRename() {
            this.renameToolStripMenuItem2.Checked = true;
            this.renameToolStripMenuItem1.Checked = true;
            this.renameToolStripMenuItem.Checked = true;
            ListView lv = this.m_Panel.GetActiveView();
            if ( lv.SelectedIndices.Count == 0 ) {
                this.QuitRename();
                return;
            }
            this.buttonF9.Text = "- Escape -";
            this.m_editbox.Parent = lv;
            ListViewItem lvi = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[lv.SelectedIndices[0]];
            lv.EnsureVisible(lvi.Index);
            if ( (lvi.Text == "[..]") && (lvi.ImageIndex == 2) ) {  // aka "LevelUp"
                this.QuitRename();
                return;
            }
            this.m_hitinfoRename = lv.HitTest(lvi.Position.X, lvi.Position.Y);
            Point pt = this.m_hitinfoRename.SubItem.Bounds.Location;
            string path = this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString();
            if ( path != "Computer" ) {
                // eidt box to rename file or folder
                pt.X += 23;
                pt.Y += 1;
                this.m_editbox.Bounds = new Rectangle(pt, new Size(lv.Columns[0].Width - 23, this.m_hitinfoRename.SubItem.Bounds.Height));
                this.m_editbox.Text = lvi.Text;
                this.m_original = lvi.Text;
            } else {
                // 20160320: allow renaming a volume label
                if ( (lvi.Text.Length == 2) && (lvi.Text[1] == ':') ) {
                    pt.X += 23;
                    pt.Y += 1;
                    this.m_editbox.Bounds = lvi.SubItems[5].Bounds;
                    this.m_editbox.Text = lvi.SubItems[5].Text;
                    this.m_original = lvi.SubItems[5].Text;
                }
            }
            this.m_bStealFocus = false;
            this.m_editbox.Select(0, this.m_editbox.Text.Length);
            this.m_editbox.Show();
            this.m_editbox.Focus();
        }
        void ExecuteRename() {
            // just in case, new name is empty 
            if ( this.m_editbox.Text.Length == 0 ) {
                this.QuitRename();
                return;
            }
            // just in case, nothing was changed 
            if ( this.m_editbox.Text == this.m_original ) {
                this.QuitRename();
                return;
            }
            // m_editbox is connected to a certain listview, if the active listview != m_editbox.parent, we need to give up with the rename because the operator switched to the other listview
            if ( this.m_editbox.Parent != this.m_Panel.GetActiveView() ) {
                this.QuitRename();
                return;
            }
            // give focus back to command line
            this.m_bStealFocus = true;
            this.textBoxCommandLine.Focus();
            string source = "";
            try {
                ListViewItem lvi = this.m_hitinfoRename.Item;
                source = lvi.Text;
                string destination = this.m_editbox.Text;
                string samepath = this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString();
                string fullsrc = Path.Combine(samepath, source);
                string fulldst = Path.Combine(samepath, destination);
                if ( fullsrc == fulldst ) {
                    this.QuitRename();
                    return;
                }
                // stop FS watcher
                this.fileSystemWatcherLeft.EnableRaisingEvents = false;
                this.fileSystemWatcherRight.EnableRaisingEvents = false;
                if ( samepath != "Computer" ) {
                    // deal with folders & files
                    if ( (lvi.ImageIndex == 3) || (lvi.ImageIndex == 0) ) {
                        // rename a folder is actually move to the same path but different foldername
                        System.IO.Directory.Move(fullsrc, fulldst);
                    } else {
                        if ( lvi.ImageIndex != 2 ) {
                            // rename a file is actually move to the same path but different filername
                            File.Move(fullsrc, fulldst);
                        } else {
                            // anything other than folder or file is not handled 
                            this.QuitRename();
                            return;
                        }
                    }
                } else {
                    // 20160320: rename volume labels
                    if ( (lvi.Text.Length == 2) && (lvi.Text[1] == ':') ) {
                        // set volume label
                        DriveInfo di = new DriveInfo(lvi.Text.Substring(0, 1));
                        di.VolumeLabel = destination;
                        // tricky: LoadListView shall keep the renamed drive selected
                        destination = lvi.Text;
                    }
                }
                // start FS watcher
                if ( System.IO.Directory.Exists(this.fileSystemWatcherLeft.Path) && this.filesystemMonitoringToolStripMenuItem.Checked ) {
                    this.fileSystemWatcherLeft.EnableRaisingEvents = true;
                }
                if ( System.IO.Directory.Exists(this.fileSystemWatcherRight.Path) && this.filesystemMonitoringToolStripMenuItem.Checked ) {
                    this.fileSystemWatcherRight.EnableRaisingEvents = true;
                }
                this.m_bFileSystemChangeActionLeft = true;
                this.m_bFileSystemChangeActionRight = true;
                // reload list(s)
                Side sideActive = this.m_Panel.GetActiveSide();
                string sNewItemText = destination;
                this.LoadListView(sideActive, this.m_Panel.button(sideActive).Tag.ToString(), sNewItemText);
                if ( this.m_Panel.button(Side.left).Tag.ToString() == this.m_Panel.button(Side.right).Tag.ToString() ) {
                    this.LoadListView(this.m_Panel.GetPassiveSide(), this.m_Panel.button(sideActive).Tag.ToString(), sNewItemText);
                }
            } catch ( Exception e ) {
                this.QuitRename();
                MessageBox.Show(source + " could not be renamed\n\n" + e.Message + "\n\nYou may retry as Administrator.", "F9 - Name");
                // start FS watcher
                if ( System.IO.Directory.Exists(this.fileSystemWatcherLeft.Path) && this.filesystemMonitoringToolStripMenuItem.Checked ) {
                    this.fileSystemWatcherLeft.EnableRaisingEvents = true;
                }
                if ( System.IO.Directory.Exists(this.fileSystemWatcherRight.Path) && this.filesystemMonitoringToolStripMenuItem.Checked ) {
                    this.fileSystemWatcherRight.EnableRaisingEvents = true;
                }
                this.m_bFileSystemChangeActionLeft = true;
                this.m_bFileSystemChangeActionRight = true;
                return;
            }
            // display new item (file/folder) name
            this.m_hitinfoRename.SubItem.Text = this.m_editbox.Text;
            // status ante bellum
            this.QuitRename();
        }
        void QuitRename() {
            this.renameToolStripMenuItem.Checked = false;
            this.renameToolStripMenuItem1.Checked = false;
            this.renameToolStripMenuItem2.Checked = false;
            this.m_original = "";
            this.m_editbox.Parent = null;
            this.m_editbox.Text = "";
            this.m_editbox.Hide();
            this.buttonF9.Text = "F9 - Name";
            this.m_bKeepFocused = false;
            this.m_bStealFocus = true;
            this.textBoxCommandLine.Focus();
            this.m_bFileSystemChangeActionLeft = true;
            this.m_bFileSystemChangeActionRight = true;
        }
        void OnEditBoxLostFocus(object sender, EventArgs e) {
            // AWKWARD: opening a context menu in "Computer lv" calls out OnEditBoxLostFocus, while "regular lv" doesn't 
            if ( this.m_bKeepFocused ) {
                this.m_bKeepFocused = false;
                return;
            }
            if ( !this.renameToolStripMenuItem.Checked ) {
                return;
            }
            if ( !this.renameToolStripMenuItem1.Checked ) {
                return;
            }
            if ( !this.renameToolStripMenuItem2.Checked ) {
                return;
            }

            // LostFocus happens prior to F9ButtonDown, therefore F9ButtonDown restarts renaming unless we ignore F9ButtonDown for some time
            this.m_dtLostFocus = DateTime.Now;

            // quit rename when is mouse over "F9 Esc=cancel" OR mouse moved somewhere outside m_editbox.Parent, otherwise rename
            // AWKWARD: "regular lv" always lands here prior to left mouse down, while "Computer lv" first lands in "mouse down"
            // the .net way: Control ctl = FindControlAtCursor(this);
            IntPtr hWnd = WindowFromPoint(Control.MousePosition);
            if ( this.buttonF9.ClientRectangle.Contains(this.buttonF9.PointToClient(Control.MousePosition)) || (this.m_editbox.Parent.Handle != hWnd) ) {
                this.QuitRename();
            } else {
                this.ExecuteRename();
            }
        }
        private void OnEditBoxKeyDown(object sender, KeyEventArgs e) {
            // pushing ESC shall not alter the name
            if ( e.KeyCode == Keys.Escape ) {
                this.QuitRename();
            }
            // pushing ENTER shall alter the name
            if ( e.KeyCode == Keys.Enter ) {
                // we fool the editbox's lostfocus event 
                this.m_bKeepFocused = true;
                // Enter == rename
                this.ExecuteRename();
            }
        }
        // "do nothing = Esc" ==> this method used for 2 context menus: "listview" and "computer view"
        private void closeMenuToolStripMenuItem_Click(object sender, EventArgs e) {
            this.m_bRunSize = false;    // stop pending folder sizes 
            this.QuitRename();          // quit a rename  
        }

        /*
                // the .net way: 2x helper for "get control under mouse cursor" --ALTERNATIVE-- win32: via IntPtr hWnd = WindowFromPoint(Control.MousePosition);
                public static Control FindControlAtPoint( Control container, Point pos )
                {
                    Control child;
                    foreach ( Control c in container.Controls ) {
                        if ( c.Visible && c.Bounds.Contains(pos) ) {
                            child = FindControlAtPoint(c, new Point(pos.X - c.Left, pos.Y - c.Top));
                            if ( child == null ) return c;
                            else return child;
                        }
                    }
                    return null;
                }
                public static Control FindControlAtCursor( Form form )
                {
                    Point pos = Cursor.Position;
                    if ( form.Bounds.Contains(pos) )
                        return FindControlAtPoint(form, form.PointToClient(pos));
                    return null;
                }
        */

        // file system monitoring
        /*
        // Monitoring file system changes
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // a) only thru fileSystemWatcher: 
        //    - pro: shows immidiately any change
        //    - con: generates too many events
        // b) only thru timer:
        //    - pro: simple implementation 
        //    - con: has a constant work load, which is not necessary in 99% of the time
        //    - con: first change is shown with timer delay
        //
        // My Concept - combine a) and b) - listen to fileSystemWatcher once after it raised an event, then ignore it for some time 
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Normally (no file system changes at all) the timer is disabled.
        // Any arbitrary file system change event (change, create, delete, rename) starts a timer + allows exactly one immediate list refresh + disables further list refreshes.
        // We allow a file system change event only once per timer tick interval, currently set to 1000ms.         
        // Frequent file system changes are therefore ignored, until timer tick re enables the list refresh.
        // Whenever the timer expires, we need to make a final cleanup. During the last timer interval, we might have ignored a couple of file system changes. 
        */
        private void timerFileSystemMonitorLeft_Tick(object sender, EventArgs e) {
            this.timerFileSystemMonitorLeft.Stop();                                                                 // stop timer

            string signal = this.m_shfoIsActive ? "*" : "?";                                                             // "*" mark new item ... vs. ... "?" keep current selection 
            this.m_shfoIsActive = false;
            this.LoadListView(Side.left, this.m_Panel.button(Side.left).Tag.ToString(), signal);                              // this is the final cleanup, after there was no file system change event anymore
            if ( this.m_Panel.button(Side.left).Tag.ToString() == this.m_Panel.button(Side.right).Tag.ToString() ) {
                this.LoadListView(Side.right, this.m_Panel.button(Side.right).Tag.ToString(), signal);
            }

            this.m_bFileSystemChangeActionLeft = true;                                                                   // enable list refresh
        }
        private void timerFileSystemMonitorRight_Tick(object sender, EventArgs e) {
            this.timerFileSystemMonitorRight.Stop();
            string signal = this.m_shfoIsActive ? "*" : "?";
            this.m_shfoIsActive = false;
            this.LoadListView(Side.right, this.m_Panel.button(Side.right).Tag.ToString(), signal);
            if ( this.m_Panel.button(Side.left).Tag.ToString() == this.m_Panel.button(Side.right).Tag.ToString() ) {
                this.LoadListView(Side.left, this.m_Panel.button(Side.left).Tag.ToString(), signal);
            }
            this.m_bFileSystemChangeActionRight = true;
        }
        async private void fileSystemWatcherLeft_Changed(object sender, FileSystemEventArgs e) {
            if ( this.m_bFileSystemChangeActionLeft ) {
                // negative side effect --> it would overlook when the folder becomes empty OR if an empty folder gets filled 
                //if ( !listsShowFolderSizesToolStripMenuItem.Checked && (e.ChangeType == WatcherChangeTypes.Changed) && System.IO.Directory.Exists(e.FullPath) ) {
                //    // 20161016: ignore subfolder changes when ShowFolderSizes is disabled (default), only changes take place and if it's a directory -- creation/delete is still monitored
                //    return;
                //}
                this.m_bFileSystemChangeActionLeft = false;                                                           // disable list refresh
                string signal = this.m_shfoIsActive ? "*" : "?";                                                      // refresh list: "*" show new item VERSUS "?" keep current selection 
                await this.LoadListView(Side.left, this.m_Panel.button(Side.left).Tag.ToString(), signal);
                if ( this.m_Panel.button(Side.left).Tag.ToString() == this.m_Panel.button(Side.right).Tag.ToString() ) {   // just in case
                    await this.LoadListView(Side.right, this.m_Panel.button(Side.right).Tag.ToString(), signal);
                }
                this.timerFileSystemMonitorLeft.Start();                                                         // start timer to re enable list refresh
            }
        }
        async private void fileSystemWatcherRight_Changed(object sender, FileSystemEventArgs e) {
            if ( this.m_bFileSystemChangeActionRight ) {
                //if ( !listsShowFolderSizesToolStripMenuItem.Checked && (e.ChangeType == WatcherChangeTypes.Changed) && System.IO.Directory.Exists(e.FullPath) ) {
                //    return;
                //}
                this.m_bFileSystemChangeActionRight = false;
                string signal = this.m_shfoIsActive ? "*" : "?";
                await this.LoadListView(Side.right, (this.m_Panel.button(Side.right)).Tag.ToString(), signal);
                if ( this.m_Panel.button(Side.left).Tag.ToString() == this.m_Panel.button(Side.right).Tag.ToString() ) {
                    await this.LoadListView(Side.left, this.m_Panel.button(Side.left).Tag.ToString(), signal);
                }
                this.timerFileSystemMonitorRight.Start();
            }
        }
        private void fileSystemWatcherLeft_Renamed(object sender, RenamedEventArgs e) {
            this.fileSystemWatcherLeft_Changed(sender, new FileSystemEventArgs(WatcherChangeTypes.Renamed, e.FullPath, e.Name));
        }
        private void fileSystemWatcherRight_Renamed(object sender, RenamedEventArgs e) {
            this.fileSystemWatcherRight_Changed(sender, new FileSystemEventArgs(WatcherChangeTypes.Renamed, e.FullPath, e.Name));
        }

        // reset "fit view": we only want to act after a manual column width change by the user ...  
        private void listViewLeftRight_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e) {
            if ( ((DateTime.Now - this.m_dtLastLoad).TotalMilliseconds < 3000) || this.m_bBlockListViewActivity ) {
                return;
            }
            //            this.listViewFitColumnsToolStripMenuItem.Checked = false;
            //            MessageBox.Show("listview autofit disabled");
        }

        // view file 
        private void buttonF3_Click(object sender, EventArgs e) {
            if ( this.m_Panel.GetActiveView().SelectedIndices.Count == 0 ) {
                return;
            }
            if ( this.m_Panel.listType[(int)this.m_Panel.GetActiveSide()] != ListType.FileSystem ) {
                MessageBox.Show("Not yet implemented for WPD.", "Sorry");
                return;
            }
            string thefile = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[this.m_Panel.GetActiveView().SelectedIndices[0]].Text;
            string filename = Path.Combine(this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), thefile);
            if ( !File.Exists(filename) ) {
                return;
            }
            try {
                FileViewer fv = new FileViewer(filename);
                fv.Show();
            } catch ( Exception ) {; }
        }

        // edit file
        private void editToolStripMenuItem1_Click(object sender, EventArgs e) {
            this.buttonF4_Click(null, null);
        }
        private void buttonF4_Click(object sender, EventArgs e) {
            if ( this.m_Panel.GetActiveView().SelectedIndices.Count == 0 ) {
                return;
            }
            if ( this.m_Panel.listType[(int)this.m_Panel.GetActiveSide()] != ListType.FileSystem ) {
                MessageBox.Show("Not yet implemented for WPD.", "Sorry");
                return;
            }
            string thefile = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[this.m_Panel.GetActiveView().SelectedIndices[0]].Text;
            string filename = Path.Combine(this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), thefile);
            string registeredApp = "";

            // all text like files: opened with default text editor 
            Encoding enc;
            if ( GrzTools.FileTools.IsTextFile(out enc, filename, 200) ) {
                registeredApp = GrzTools.FileAssociation.Get(".txt");
                if ( (registeredApp != null) && (registeredApp != "") ) {
                    System.Diagnostics.Process p = new System.Diagnostics.Process();
                    p.StartInfo.FileName = registeredApp;
                    p.StartInfo.Arguments = "\"" + filename + "\"";
                    p.Start();
                }
                return;
            }

            // Images only: since we pushed the EDIT button we rather want to modify the file as to show it
            if ( this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[this.m_Panel.GetActiveView().SelectedIndices[0]].SubItems[3].Text.Contains("image") ) {
                try {
                    ProcessStartInfo startInfo = new ProcessStartInfo(filename);
                    startInfo.Verb = "edit";
                    Process.Start(startInfo);
                } catch ( Exception ) {
                    this.openWithToolStripMenuItem_Click(null, null);
                }
                return;
            }

            // all files other than text & image will be started as argument with their associated app
            string extension = Path.GetExtension(filename);
            registeredApp = GrzTools.FileAssociation.Get(extension);
            if ( (registeredApp != null) && (registeredApp != "") ) {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = registeredApp;
                p.StartInfo.Arguments = "\"" + filename + "\"";
                try {
                    p.Start();
                } catch ( Exception ) {
                    p = new System.Diagnostics.Process();
                    p.StartInfo.FileName = filename;
                    try {
                        p.Start();
                    } catch ( Exception ) {
                        this.openWithToolStripMenuItem_Click(null, null);
                    }
                }
            }
        }

        // print file OR preview print file
        private void printToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.m_Panel.GetActiveView().SelectedIndices.Count == 0 ) {
                return;
            }
            if ( this.m_Panel.listType[(int)this.m_Panel.GetActiveSide()] != ListType.FileSystem ) {
                MessageBox.Show("Not yet implemented for WPD.", "Sorry");
                return;
            }
            // get filename
            string thefile = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[this.m_Panel.GetActiveView().SelectedIndices[0]].Text;
            string fullPath = Path.Combine(this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), thefile);
            // print file
            bool preview = this.printToolStripMenuItem.Text == "Print Preview" ? true : false;
            GrzTools.Print.PrintFile(fullPath, preview);
        }

        // quit cfw
        private void buttonF10_Click(object sender, EventArgs e) {
            this.Close();
        }

        // listview refresh/reload via menu
        private void refreshLeftToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.m_Panel.GetActiveSide() == Side.left ) {
                // reload left
                if ( this.m_Panel.button(Side.left).Tag.ToString() != "Computer" ) {
                    this.LoadListView(Side.left, this.m_Panel.button(Side.left).Tag.ToString(), "");
                } else {
                    this.timerRefeshComputerView_Tick(null, null);
                }
            }
            if ( this.m_Panel.GetActiveSide() == Side.right ) {
                // sync left side according to right path
                this.LoadListView(Side.left, this.m_Panel.button(Side.right).Tag.ToString(), "");
                this.m_Panel.folders.InsertTopFolder(Side.left, this.m_Panel.GetActiveTabIndex(Side.left), this.m_Panel.button(Side.right).Tag.ToString());
                this.m_Panel.RenderListviewLabel(Side.left);
            }
        }
        private void refreshRightToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.m_Panel.GetActiveSide() == Side.left ) {
                // sync right side according to left path 
                this.LoadListView(Side.right, this.m_Panel.button(Side.left).Tag.ToString(), "");
                this.m_Panel.folders.InsertTopFolder(Side.right, this.m_Panel.GetActiveTabIndex(Side.right), this.m_Panel.button(Side.left).Tag.ToString());
                this.m_Panel.RenderListviewLabel(Side.right);
            }
            if ( this.m_Panel.GetActiveSide() == Side.right ) {
                // reload right
                if ( this.m_Panel.button(Side.right).Tag.ToString() != "Computer" ) {
                    this.LoadListView(Side.right, this.m_Panel.button(Side.right).Tag.ToString(), "");
                } else {
                    this.timerRefeshComputerView_Tick(null, null);
                }
            }
        }
        private void listPassiveListActiveToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.m_Panel.GetActiveSide() == Side.left ) {
                this.refreshRightToolStripMenuItem_Click(null, null);
            } else {
                this.refreshLeftToolStripMenuItem_Click(null, null);
            }
        }


        //
        // file time settings
        //
        int m_evtNdx = 0;
        double m_pctStep = 0;
        async private void fileAttributeToolStripMenuItem_Click(object sender, EventArgs e) {
            // no selection
            if ( this.m_Panel.GetActiveView().SelectedIndices.Count == 0 ) {
                return;
            }
            // WPD
            if ( this.m_Panel.listType[(int)this.m_Panel.GetActiveSide()] != ListType.FileSystem ) {
                MessageBox.Show("Not yet implemented for WPD.", "Sorry");
                return;
            }
            // Desktop & Shared Folders have drives in the ListView - they are not allowed to copy/move/delete/mkdir/rename/MakeZip/MD5Sum
            ListViewItem[] lva = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide());
            foreach ( ListViewItem lvi in lva ) {
                if ( (lvi.Text.Length > 1) && (lvi.Text[1] == ':') ) {
                    this.renameToolStripMenuItem1.Checked = false;
                    return;
                }
            }
            // level up cannot be processed
            ListViewItem toplvi = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[this.m_Panel.GetActiveView().SelectedIndices[0]];
            string firstfile = toplvi.Text;
            if ( (firstfile == "[..]") && (toplvi.ImageIndex == 2) ) {  // aka "LevelUp"
                MessageBox.Show("Item " + firstfile + " cannot be processed. You should exclude it from the selection.", "Error");
                return;
            }

            Side side = this.m_Panel.GetActiveSide();
            this.fileSystemWatcherLeft.EnableRaisingEvents = false;
            this.fileSystemWatcherRight.EnableRaisingEvents = false;
            int[] selColl = new int[this.m_Panel.GetActiveView().SelectedIndices.Count];
            this.m_Panel.GetActiveView().SelectedIndices.CopyTo(selColl, 0);
            int fails = 0;
            bool bDirectory = ((toplvi.ImageIndex == 3) || (toplvi.ImageIndex == 0)) ? true : false;
            string path = this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString();
            string full = Path.Combine(path, firstfile);
            AttributesEditor ae = new AttributesEditor(full, bDirectory);
            DialogResult dlr = ae.ShowDialog();
            // closing this dlg with "Yes" means: all selected folders/files shall get the same datetime as the first selected item, which was already changed by the dlg
            if ( (dlr == DialogResult.Yes) || (dlr == DialogResult.Ignore) ) {
                // here we pick datetime from the first item selected, which was already set by the dlg
                DateTime dtCreate;
                DateTime dtWrite;
                DateTime dtAccess;
                // magic file finder class
                GrzTools.FastFileFind fff = new GrzTools.FastFileFind(this);
                // top level folder/file setting
                if ( bDirectory ) {
                    dtCreate = System.IO.Directory.GetCreationTime(full);
                    dtWrite = System.IO.Directory.GetLastWriteTime(full);
                    dtAccess = System.IO.Directory.GetLastAccessTime(full);
                    // "Ignore" stands for recursive datetime to subfolders
                    if ( dlr == DialogResult.Ignore ) {

                        // init progress
                        int foldercount = Math.Max(1, new GrzTools.FastDirectoryEnumerator().GetAllDirectories(full).Count());
                        this.m_pctStep = 100 / foldercount;
                        this.m_evtNdx = 1;
                        SimpleProgress sp = new SimpleProgress();
                        sp.StartPosition = FormStartPosition.Manual;
                        sp.Location = new Point(this.Location.X + (this.Width - sp.Width) / 2, this.Location.Y + 100);
                        sp.Text = "Setting File Times - " + full;
                        sp.LabelPercent = "0%";
                        sp.ProgressValue = 0;
                        sp.Show(this);
                        // set datetime
                        fff.CountSizeEvent += new EventHandler<GrzTools.FastFileFind.CountSizeEventArgs>(this.CountSizeEvent_Received);
                        fff.SetDateTimeFilesFoldersRecursive(full, dtCreate, dtWrite, dtAccess, ref fails);
                        fff.CountSizeEvent -= this.CountSizeEvent_Received;
                        sp.Close();
                    }
                } else {
                    dtCreate = File.GetCreationTime(full);
                    dtWrite = File.GetLastWriteTime(full);
                    dtAccess = File.GetLastAccessTime(full);
                }
                // now we apply the datetime from above to the rest of the selection
                bool bError = false;
                foreach ( int ndx in selColl ) {
                    ListViewItem fn = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[ndx];
                    // skip first selection, because this one is already modified
                    if ( fn.Text == firstfile ) {
                        continue;
                    }
                    full = Path.Combine(path, fn.Text);
                    try {
                        if ( (fn.ImageIndex == 3) || (fn.ImageIndex == 0) ) {
                            // set single folder
                            System.IO.Directory.SetCreationTime(full, dtCreate);
                            System.IO.Directory.SetLastWriteTime(full, dtWrite);
                            System.IO.Directory.SetLastAccessTime(full, dtAccess);
                            // "Ignore" stands for recursive datetime to subfolders
                            if ( dlr == DialogResult.Ignore ) {
                                // init progress
                                int foldercount = Math.Max(1, new GrzTools.FastDirectoryEnumerator().GetAllDirectories(full).Count());
                                this.m_pctStep = 100 / foldercount;
                                this.m_evtNdx = 1;
                                m_sp = new SimpleProgress();
                                m_sp.StartPosition = FormStartPosition.Manual;
                                m_sp.Location = new Point(this.Location.X + (this.Width - m_sp.Width) / 2, this.Location.Y + 100);
                                m_sp.Text = "Setting File Times - " + full;
                                m_sp.LabelPercent = "0%";
                                m_sp.ProgressValue = 0;
                                m_sp.Show(this);
                                // set datetime
                                fff.CountSizeEvent += new EventHandler<GrzTools.FastFileFind.CountSizeEventArgs>(this.CountSizeEvent_Received);
                                fff.SetDateTimeFilesFoldersRecursive(full, dtCreate, dtWrite, dtAccess, ref fails);
                                fff.CountSizeEvent -= this.CountSizeEvent_Received;
                                m_sp.Close();
                            }
                        } else {
                            // set single file
                            File.SetCreationTime(full, dtCreate);
                            File.SetLastWriteTime(full, dtWrite);
                            File.SetLastAccessTime(full, dtAccess);
                        }
                    } catch ( Exception ) {
                        bError = true;
                    }
                }
                // failures & errors
                if ( m_sp != null ) {
                    m_sp.Close();
                }
                if ( fails > 0 ) {
                    bError = true;
                }
                if ( this.m_Panel.button(Side.left).Tag.ToString() == this.m_Panel.button(Side.right).Tag.ToString() ) {
                    await this.LoadListView(Side.left, this.m_Panel.button(Side.left).Tag.ToString(), "*");
                    await this.LoadListView(Side.right, this.m_Panel.button(Side.right).Tag.ToString(), "*");
                } else {
                    await this.LoadListView(side, this.m_Panel.button(side).Tag.ToString(), "*");
                }
                if ( System.IO.Directory.Exists(this.fileSystemWatcherLeft.Path) && this.filesystemMonitoringToolStripMenuItem.Checked ) {
                    this.fileSystemWatcherLeft.EnableRaisingEvents = true;
                }
                if ( System.IO.Directory.Exists(this.fileSystemWatcherRight.Path) && this.filesystemMonitoringToolStripMenuItem.Checked ) {
                    this.fileSystemWatcherRight.EnableRaisingEvents = true;
                }
                if ( bError ) {
                    string text = "Not all file times could be set.\n\nYou may retry as 'Administrator'.";
                    if ( fails > 0 ) {
                        text = fails.ToString() + " file times could not be set.\n\nYou may retry as 'Administrator' and remove r/o status from affected files.";
                    }
                    MessageBox.Show(text, "Error");
                }
            }
            ae.Dispose();
        }
        // all this effort only to show progess: all parallel tasks send messages to this event handler
        void CountSizeEvent_Received(object sender, GrzTools.FastFileFind.CountSizeEventArgs e) {
            if ( e.FolderCount == 1 ) {
                // alive
                int val = m_sp.ProgressValue;
            } else {
                // real progress
                int pct = (int)(this.m_evtNdx++ * this.m_pctStep);
                m_sp.ProgressValue = pct;
                m_sp.LabelText = pct.ToString() + "%";
            }
        }

        // start new instance of cfw
        private void newCfWInstanceToolStripMenuItem_Click(object sender, EventArgs e) {
            string exeName = Application.ExecutablePath; //System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            ProcessStartInfo startInfo = new ProcessStartInfo(exeName);
            try {
                System.Diagnostics.Process.Start(startInfo);
            } catch ( Exception ) {; }
        }

        // generate MD5 sum of a file
        private void mD5SumToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.m_Panel.GetActiveView().SelectedIndices.Count == 0 ) {
                return;
            }
            // WPD
            if ( this.m_Panel.listType[(int)this.m_Panel.GetActiveSide()] != ListType.FileSystem ) {
                MessageBox.Show("Not yet implemented for WPD.", "Sorry");
                return;
            }
            ListViewItem lvi = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[this.m_Panel.GetActiveView().SelectedIndices[0]];
            string selection = lvi.Text;
            if ( (lvi.ImageIndex == 3) || (lvi.ImageIndex == 0) ) {
                GrzTools.AutoMessageBox.Show("There is no MD5 sum computation on a System.IO.Directory.", "Note", 2000);
                return;
            }
            string file = @Path.Combine(this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), selection);
            string md5str = "";
            using ( MD5 md5 = MD5.Create() ) {
                try {
                    using ( FileStream stream = File.OpenRead(file) ) {
                        md5str = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                        System.Windows.Forms.Clipboard.SetText(md5str);
                        MessageBox.Show(md5str + "\n\n\n" + selection + "\n\nMD5 sum is copied to clipboard.", "MD5 Sum");
                    }
                } catch ( Exception ) {
                    MessageBox.Show("Access denied on '" + selection + "'", "Error");
                }
            }
        }
        // generate SHA256 sum of a file
        private void SHA256SumToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.m_Panel.GetActiveView().SelectedIndices.Count == 0 ) {
                return;
            }
            // WPD
            if ( this.m_Panel.listType[(int)this.m_Panel.GetActiveSide()] != ListType.FileSystem ) {
                MessageBox.Show("Not yet implemented for WPD.", "Sorry");
                return;
            }
            ListViewItem lvi = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[this.m_Panel.GetActiveView().SelectedIndices[0]];
            string selection = lvi.Text;
            if ( (lvi.ImageIndex == 3) || (lvi.ImageIndex == 0) ) {
                GrzTools.AutoMessageBox.Show("There is no SHA256 sum computation on a System.IO.Directory.", "Note", 2000);
                return;
            }
            string file = @Path.Combine(this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), selection);

            try {
                string SHA256str = GrzTools.SHA256.GetChecksum(file);
                System.Windows.Forms.Clipboard.SetText(SHA256str);
                MessageBox.Show(SHA256str + "\n\n\n" + selection + "\n\nSHA256 sum is copied to clipboard.", "SHA256 Sum");
            } catch ( Exception ) {
                MessageBox.Show("Access denied on '" + selection + "'", "Error");
            }
        }

        // delete empty directories
        private void deleteEmptyDirectoriesToolStripMenuItem_Click(object sender, EventArgs e) {
            // WPD
            if ( this.m_Panel.listType[(int)this.m_Panel.GetActiveSide()] != ListType.FileSystem ) {
                MessageBox.Show("Not yet implemented for WPD.", "Sorry");
                return;
            }
            string message = "This will delete all empty directories underneath:\n'" + this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString() + "'\n\nYes - Proceed\n\nNo  - Don't Proceed";
            DialogResult dlr = MessageBox.Show(message, "Delete Empty Directories", MessageBoxButtons.YesNo);
            if ( dlr != System.Windows.Forms.DialogResult.Yes ) {
                return;
            }
            int iDeleted = 0;
            bool bError = false;
            string path = this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString();
            foreach ( int ndx in this.m_Panel.GetActiveView().SelectedIndices ) {
                ListViewItem fn = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[ndx];
                string file = "";
                // 3 and 0 are the image indexes for folders, only folder are processed - anything else is skipped 
                if ( (fn.ImageIndex == 3) || (fn.ImageIndex == 0) ) {
                    file = fn.Text;
                } else {
                    continue;
                }
                string full = Path.Combine(path, file);
                bError = !GrzTools.FileTools.DeleteEmptyDirs(full, ref iDeleted);
            }
            if ( bError ) {
                MessageBox.Show("Not all empty directories could be deleted.\n\nYou may retry as 'Administrator'.", "Error");
            } else {
                MessageBox.Show("There were " + iDeleted.ToString() + " empty directories deleted.", "Info");
            }
        }

        // open winmerge and use the two topmost file selections from list left & right
        private void winMergeTwoFilesToolStripMenuItem_Click(object sender, EventArgs e) {
            // find winmerge
            string winmergePath = GrzTools.InstalledPrograms.ProgramPath("winmerge");
            if ( winmergePath.Length == 0 ) {
                GrzTools.AutoMessageBox.Show("WinMerge not found.", "Error", 2000);
                return;
            }
            // WPD
            if ( this.m_Panel.listType[(int)this.m_Panel.GetActiveSide()] != ListType.FileSystem ) {
                MessageBox.Show("Not yet implemented for WPD.", "Sorry");
                return;
            }

            // image indices folder, up, folder empty
            int[] nofile = { 0, 2, 3 };

            // file left
            string file = "";
            foreach ( int ndx in this.m_listViewL.SelectedIndices ) {
                ListViewItem fn = this.m_Panel.GetListViewArr(Side.left)[ndx];
                if ( Array.IndexOf(nofile, fn.ImageIndex) == -1 ) {
                    file = fn.Text;
                    break;
                }
            }
            if ( file.Length == 0 ) {
                MessageBox.Show("No selection on left side.", "Error");
                return;
            }
            string path1 = Path.Combine(this.buttonLeft.Tag.ToString(), file);
            if ( !File.Exists(path1) ) {
                GrzTools.AutoMessageBox.Show("No file selected on left side.", "Error", 1000);
            }

            // file right
            file = "";
            foreach ( int ndx in this.m_listViewR.SelectedIndices ) {
                ListViewItem fn = this.m_Panel.GetListViewArr(Side.right)[ndx];
                if ( Array.IndexOf(nofile, fn.ImageIndex) == -1 ) {
                    file = fn.Text;
                    break;
                }
            }
            if ( file.Length == 0 ) {
                MessageBox.Show("No selection on right side.", "Error");
                return;
            }
            string path2 = Path.Combine(this.buttonRight.Tag.ToString(), file);
            if ( !File.Exists(path2) ) {
                GrzTools.AutoMessageBox.Show("No file selected on right side.", "Error", 1000);
            }

            // start winmerge with two files as parameter
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = winmergePath;
            p.StartInfo.Arguments = " -e -ub " + "\"" + path1 + "\"" + " " + "\"" + path2 + "\"";
            p.Start();
        }

        // files and folders comparison: top level only
        private void topLevelNoSubfoldersToolStripMenuItem_Click(object sender, EventArgs e) {
            // WPD
            if ( this.m_Panel.listType[(int)this.m_Panel.GetActiveSide()] != ListType.FileSystem ) {
                MessageBox.Show("Not yet implemented for WPD.", "Sorry");
                return;
            }

            // remove all selections from the lists
            this.m_listViewL.SelectedIndices.Clear();
            this.m_listViewR.SelectedIndices.Clear();

            // check list left against list right
            ListViewItem[] lviarrLHS = this.m_Panel.GetListViewArr(Side.left);
            ListViewItem[] lviarrRHS = this.m_Panel.GetListViewArr(Side.right);
            for ( int a = 0; a < lviarrLHS.Length; a++ ) {
                ListViewItem lviA = lviarrLHS[a];
                bool bHit = false;
                for ( int b = 0; b < lviarrRHS.Length; b++ ) {
                    ListViewItem lviB = lviarrRHS[b];
                    if ( lviA.SubItems[0].Text == lviB.SubItems[0].Text ) {
                        bHit = true;
                        if ( (lviA.SubItems[1].Text != lviB.SubItems[1].Text) || (lviA.SubItems[2].Text != lviB.SubItems[2].Text) ) {
                            this.m_listViewL.SelectedIndices.Add(a);
                            this.m_listViewR.SelectedIndices.Add(b);
                        }
                    }
                }
                if ( !bHit ) {
                    this.m_listViewL.SelectedIndices.Add(a);
                }
            }
            // then again but vice versa
            for ( int b = 0; b < lviarrRHS.Length; b++ ) {
                ListViewItem lviB = lviarrRHS[b];
                bool bHit = false;
                for ( int a = 0; a < lviarrLHS.Length; a++ ) {
                    ListViewItem lviA = lviarrLHS[a];
                    if ( lviA.SubItems[0].Text == lviB.SubItems[0].Text ) {
                        bHit = true;
                    }
                }
                if ( !bHit ) {
                    this.m_listViewR.SelectedIndices.Add(b);
                }
            }

            // make sure at least one item is selected
            if ( this.m_listViewL.SelectedIndices.Count == 0 ) {
                if ( this.m_listViewL.Items.Count > 0 ) {
                    this.m_listViewL.Items[0].Selected = true;
                }
            }
            if ( this.m_listViewR.SelectedIndices.Count == 0 ) {
                if ( this.m_listViewR.Items.Count > 0 ) {
                    this.m_listViewR.Items[0].Selected = true;
                }
            }
        }
        // full files and folders comparison
        private void includingSubfoldersToolStripMenuItem_Click(object sender, EventArgs e) {
            // we don't allow computer view
            string pathA = this.buttonLeft.Tag.ToString();
            string pathB = this.buttonRight.Tag.ToString();
            if ( (pathA == "Computer") || (pathB == "Computer") ) {
                MessageBox.Show("Both panels lef&right shall show a folder view.", "Error");
                return;
            }
            // WPD
            if ( this.m_Panel.listType[(int)this.m_Panel.GetActiveSide()] != ListType.FileSystem ) {
                MessageBox.Show("Not yet implemented for WPD.", "Sorry");
                return;
            }

            // lists of pathes to compare
            List<string> pathesA = new List<string>();
            List<string> pathesB = new List<string>();

            // listviews
            ListViewItem[] lva = this.m_Panel.GetListViewArr(Side.left);
            ListViewItem[] lvb = this.m_Panel.GetListViewArr(Side.right);

            if ( lva[0].Selected && lvb[0].Selected ) {
                // if both [..] are selected, we fully compare these two pathes
                pathesA.Add(pathA);
                pathesB.Add(pathB);
            } else {
                // selected files are ignored, only selected folders are taken into account 
                // folder(s) left selection
                foreach ( int ndx in this.m_listViewL.SelectedIndices ) {
                    if ( (lva[ndx].ImageIndex == 0) || (lva[ndx].ImageIndex == 3) ) {
                        pathesA.Add(Path.Combine(pathA, lva[ndx].Text));
                    }
                }
                // if there are no folders in the list, we add the top level one
                if ( pathesA.Count == 0 ) {
                    pathesA.Add(pathA);
                }
                // folder(s) right selection
                foreach ( int ndx in this.m_listViewR.SelectedIndices ) {
                    if ( (lvb[ndx].ImageIndex == 0) || (lvb[ndx].ImageIndex == 3) ) {
                        pathesB.Add(Path.Combine(pathB, lvb[ndx].Text));
                    }
                }
                // if there are no folders in the list, we add the top level one
                if ( pathesB.Count == 0 ) {
                    pathesB.Add(pathB);
                }
                // folder count mismatch
                if ( pathesA.Count != pathesB.Count ) {
                    MessageBox.Show("Selected number of folders left/right must match.", "Error");
                    return;
                }
            }

            // execute compare operation
            FileComparer fc = new FileComparer(this, pathesA, pathesB);
            fc.Show();
        }

        // hide nonsense entries from "ListView" context menu depending on circumstances
        private void contextMenuStripListItems_Opening(object sender, CancelEventArgs e) {
            // paranoya
            if ( this.m_Panel.GetActiveView().SelectedIndices.Count == 0 ) {
                return;
            }

            // enable everything
            this.copySelectionToolStripMenuItem.Enabled = true;
            this.moveSelectionF6ToolStripMenuItem.Enabled = true;
            this.deleteSelectionF8ToolStripMenuItem.Enabled = true;
            if ( (ModifierKeys & Keys.Shift) == Keys.Shift ) {
                this.deleteSelectionF8ToolStripMenuItem.Text = "Delete permanently";
                this.deleteSelectionF8ToolStripMenuItem.Image = cfw.Properties.Resources.permanent;
                this.linkOnDesktopToolStripMenuItem.Text = "Link here";
                this.printToolStripMenuItem.Text = "Print Preview";
            } else {
                this.deleteSelectionF8ToolStripMenuItem.Text = "Delete to Trash";
                this.deleteSelectionF8ToolStripMenuItem.Image = cfw.Properties.Resources.trash;
                this.linkOnDesktopToolStripMenuItem.Text = "Link to Desktop";
                this.printToolStripMenuItem.Text = "Print";
            }
            this.copyFilePathToolStripMenuItem.Enabled = true;
            this.cutToolStripMenuItem.Enabled = true;
            this.copyFilePathAsObjectToolStripMenuItem.Enabled = true;
            this.editToolStripMenuItem1.Enabled = true;
            this.openWithToolStripMenuItem.Enabled = true;
            this.runAsAdministratorToolStripMenuItem.Enabled = true;
            this.linkOnDesktopToolStripMenuItem.Enabled = true;
            this.propertiesToolStripMenuItem.Enabled = true;
            this.renameToolStripMenuItem.Enabled = true;
            this.newToolStripMenuItem.Enabled = true;
            this.pasteToolStripMenuItem.Enabled = true;
            if ( !Clipboard.ContainsFileDropList() ) {
                this.pasteToolStripMenuItem.Enabled = false;
            }
            this.printToolStripMenuItem.Enabled = true;

            // disable certain entries depending on the context
            ListViewItem lvi = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[this.m_Panel.GetActiveView().SelectedIndices[0]];
            string selection = lvi.Text;
            string pcmode = this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString();
            if ( (pcmode == "Computer") || (pcmode == "Shared Folders") ) {
                this.newToolStripMenuItem.Enabled = false;
                this.copySelectionToolStripMenuItem.Enabled = false;
                this.cutToolStripMenuItem.Enabled = false;
                this.pasteToolStripMenuItem.Enabled = false;
                this.moveSelectionF6ToolStripMenuItem.Enabled = false;
                this.deleteSelectionF8ToolStripMenuItem.Enabled = false;
                this.copyFilePathAsObjectToolStripMenuItem.Enabled = false;
                this.editToolStripMenuItem1.Enabled = false;
                this.openWithToolStripMenuItem.Enabled = false;
                this.runAsAdministratorToolStripMenuItem.Enabled = false;
                // we disable the rename entry only in case it is not checked, this way we could end a rename process even when the other side of a listview became active
                if ( this.renameToolStripMenuItem.Checked && (this.m_editbox.Parent != this.m_Panel.GetActiveView()) ) {
                    this.QuitRename();
                }
                if ( !this.renameToolStripMenuItem.Checked ) {
                    this.renameToolStripMenuItem.Enabled = false;
                }
            }

            if ( (selection == "[..]") && (lvi.ImageIndex == 2) ) {  // aka "LevelUp"
                this.copySelectionToolStripMenuItem.Enabled = false;
                this.moveSelectionF6ToolStripMenuItem.Enabled = false;
                this.deleteSelectionF8ToolStripMenuItem.Enabled = false;
                this.copyFilePathToolStripMenuItem.Enabled = false;
                this.cutToolStripMenuItem.Enabled = false;
                this.copyFilePathAsObjectToolStripMenuItem.Enabled = false;
                this.editToolStripMenuItem1.Enabled = false;
                this.openWithToolStripMenuItem.Enabled = false;
                this.runAsAdministratorToolStripMenuItem.Enabled = false;
                this.linkOnDesktopToolStripMenuItem.Enabled = false;
                this.propertiesToolStripMenuItem.Enabled = false;
                this.printToolStripMenuItem.Enabled = false;
                // we disable the rename entry only in case it is not checked, this way we could end a rename process even when the other side of a listview became active
                if ( this.renameToolStripMenuItem.Checked && (this.m_editbox.Parent != this.m_Panel.GetActiveView()) ) {
                    this.QuitRename();
                }
                if ( !this.renameToolStripMenuItem.Checked ) {
                    this.renameToolStripMenuItem.Enabled = false;
                }
            }

            if ( (lvi.ImageIndex == 0) || (lvi.ImageIndex == 3) ) { // folder 
                this.printToolStripMenuItem.Enabled = false;
            }

        }
        // copy file/path link to clipboard
        private void copyFilePathToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.m_Panel.GetActiveView().SelectedIndices.Count == 0 ) {
                return;
            }
            StringCollection paths = new StringCollection();
            ListView lv = this.m_Panel.GetActiveView();
            foreach ( int ndx in lv.SelectedIndices ) {
                ListViewItem lvi = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[ndx];
                string file = lvi.Text;
                string fullpath = Path.Combine(this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), file);
                paths.Add(fullpath);
            }

            string finaltext = "";
            for ( int i = 0; i < paths.Count; i++ ) {
                if ( i > 0 ) {
                    finaltext += "\r\n";
                }
                finaltext += paths[i];
            }

            Clipboard.Clear();
            Clipboard.SetText(finaltext);
        }
        // copy file/path object to clipboard
        private void copyFilePathAsObjectToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.m_Panel.GetActiveView().SelectedIndices.Count == 0 ) {
                return;
            }
            // WPD
            if ( this.m_Panel.listType[(int)this.m_Panel.GetActiveSide()] != ListType.FileSystem ) {
                MessageBox.Show("Not yet implemented for WPD.", "Sorry");
                return;
            }
            StringCollection paths = new StringCollection();
            ListView lv = this.m_Panel.GetActiveView();
            foreach ( int ndx in lv.SelectedIndices ) {
                ListViewItem lvi = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[ndx];
                string file = lvi.Text;
                string fullpath = Path.Combine(this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), file);
                if ( System.IO.Directory.Exists(fullpath) || File.Exists(fullpath) ) {
                    paths.Add(fullpath);
                }
            }

            if ( paths.Count > 0 ) {
                Clipboard.Clear();
                Clipboard.SetFileDropList(paths);
            }
        }
        // cut: copy file/path object to clipboard + tell OS about the drop info 
        private void cutToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.m_Panel.GetActiveView().SelectedIndices.Count == 0 ) {
                return;
            }
            // WPD
            if ( this.m_Panel.listType[(int)this.m_Panel.GetActiveSide()] != ListType.FileSystem ) {
                MessageBox.Show("Not yet implemented for WPD.", "Sorry");
                return;
            }
            StringCollection paths = new StringCollection();
            ListView lv = this.m_Panel.GetActiveView();
            foreach ( int ndx in lv.SelectedIndices ) {
                ListViewItem lvi = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[ndx];
                string file = lvi.Text;
                string fullpath = Path.Combine(this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), file);
                if ( System.IO.Directory.Exists(fullpath) || File.Exists(fullpath) ) {
                    paths.Add(fullpath);
                }
            }

            // prepare the drop effect (aka move)
            byte[] moveEffect = new byte[] { 2, 0, 0, 0 };
            MemoryStream dropEffect = new MemoryStream();
            dropEffect.Write(moveEffect, 0, moveEffect.Length);

            // data will carry the info about files to move (something like copy then delete)
            DataObject data = new DataObject();
            data.SetFileDropList(paths);
            data.SetData("Preferred DropEffect", dropEffect);
            Clipboard.Clear();
            Clipboard.SetDataObject(data, true);
        }

        // paste file/path object from clipboard via bgw to keep UI responsive 
        private void pasteToolStripMenuItem_Click(object sender, EventArgs e) {
            // WPD
            if ( this.m_Panel.listType[(int)this.m_Panel.GetActiveSide()] != ListType.FileSystem ) {
                MessageBox.Show("Not yet implemented for WPD.", "Sorry");
                return;
            }
            this.pasteFromClipBoard();
        }
        void pasteFromClipBoard() {
            if ( !Clipboard.ContainsFileDropList() ) {
                return;
            }

            // initial FileOperationStrategy fos is set to FO_COPY
            GrzTools.ShellFileOperation.FileOperations fos = GrzTools.ShellFileOperation.FileOperations.FO_COPY;

            // do we have a drop info (aka move info) stored in the associated data
            DataObject data = (DataObject)Clipboard.GetDataObject();
            MemoryStream obj = (MemoryStream)data.GetData("Preferred DropEffect");
            if ( obj != null ) {
                byte[] moveEffect = new byte[obj.Length];
                obj.Read(moveEffect, 0, (int)obj.Length);
                // fulfilling that check, complies to a FO_MOVE operation
                if ( (moveEffect.Length >= 4) && (moveEffect[0] == 2) && (moveEffect[1] == 0) && (moveEffect[2] == 0) && (moveEffect[3] == 0) ) {
                    fos = GrzTools.ShellFileOperation.FileOperations.FO_MOVE;
                }
            }

            // signal to fs watcher, that it should expect new items - reset happens in timer cleanup 
            this.m_shfoIsActive = true;

            // simple paste from clipboard
            string sDestinationBasePath = this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString();
            List<string> dst = new List<string>();
            List<string> src = new List<string>();
            StringCollection paths = Clipboard.GetFileDropList();
            foreach ( string file in paths ) {
                src.Add(file);
                dst.Add(Path.Combine(sDestinationBasePath, Path.GetFileName(file)));
            }

            // paste from clipboard
            this.m_shfo.SetArguments(new ShfoWorker.Arguments(this.Handle, src, dst, fos, "Paste from Clipboard", true, false));
            Thread thread = new Thread(this.m_shfo.DoWorkShfo);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            // in case of a FO_MOVE operation, we better clear the clipboard - reason: source and dest files lists are compromised after FO_MOVE
            if ( fos == GrzTools.ShellFileOperation.FileOperations.FO_MOVE ) {
                Clipboard.Clear();
            }
        }

        // RS232
        private void rS232ToolStripMenuItem_Click(object sender, EventArgs e) {
            RS232Form frm = new RS232Form();
            frm.Show();
        }

        // GPIO
        private void portIOToolStripMenuItem_Click(object sender, EventArgs e) {
            PortIO pio = new PortIO();
            pio.Show();
        }

        // media change has to be delayed, otherwise I get an error
        string m_sDriveToComputer = "";
        async private void timerMediaChangeDelayedRefresh_Tick(object sender, EventArgs e) {
            // stop timer
            this.timerMediaChangeDelayedRefresh.Stop();

            // update "Computer" view
            if ( this.m_Panel.button(Side.left).Text == "Computer" ) {
                await this.LoadListView(Side.left, "Computer", "");
            }
            if ( this.m_Panel.button(Side.right).Text == "Computer" ) {
                await this.LoadListView(Side.right, "Computer", "");
            }

            // switch to "Computer" view
            if ( this.m_sDriveToComputer.Length > 0 ) {
                if ( this.m_Panel.button(Side.left).Text.StartsWith(this.m_sDriveToComputer) ) {
                    await this.LoadListView(Side.left, "Computer", "");
                    this.m_Panel.folders.InsertTopFolder(Side.left, this.m_Panel.GetActiveTabIndex(Side.left), "Computer");
                }
                if ( this.m_Panel.button(Side.right).Text.StartsWith(this.m_sDriveToComputer) ) {
                    await this.LoadListView(Side.right, "Computer", "");
                    this.m_Panel.folders.InsertTopFolder(Side.right, this.m_Panel.GetActiveTabIndex(Side.right), "Computer");
                }
            }
            this.m_sDriveToComputer = "";
        }
        // public media change event
        public event EventHandler<MediaChangeEventArgs> MediaChangeEvent;
        // event handler receives media change events
        void MediaChangeEvent_Received(object sender, MediaChangeEventArgs e) {
            // very elegant: w/o this Invoke(..) thing, events raised from a separate thread are forbidden to access UI-Thread elements and throw exceptions 
            if ( this.InvokeRequired ) {
                try {
                    this.Invoke(new EventHandler<MediaChangeEventArgs>(this.MediaChangeEvent_Received), sender, e);
                } catch ( Exception ) {; }
                return;
            }

            // two kinds of media change shall reload listview
            if ( (e.MediaChange == MediaChangeEventArgs.Media.arrived) ) {
                // start timer
                this.timerMediaChangeDelayedRefresh.Start();
            }
            if ( (e.MediaChange == MediaChangeEventArgs.Media.removed) ) {
                // start timer
                this.timerMediaChangeDelayedRefresh.Start();
            }
            if ( (e.MediaChange == MediaChangeEventArgs.Media.queryremove) ) {
                // we shall switch from the Drive to Computer view, because the drive is about to remove
                this.m_sDriveToComputer = e.Drive;
                // start timer
                this.timerMediaChangeDelayedRefresh.Start();
            }
        }
        // public definition of the media event arguments class
        public class MediaChangeEventArgs : EventArgs {
            public MediaChangeEventArgs(Media media, string drive = "") {
                this.MediaChange = media;
                this.Drive = drive;
            }
            public enum Media { arrived, removed, queryremove };
            public Media MediaChange { get; set; }
            public string Drive { get; set; }
        }
        // detect media change via WM_DEVICECHANGE
        private const int DBT_DEVICEQUERYREMOVE = 0x8001;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int WM_DEVICECHANGE = 0x0219;
        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        // change system menu to show "about" entry AND modify WndProc to be able to show about
        private void SetupSystemMenu() {
            // get handle to system menu
            int menu = GetSystemMenu(this.Handle.ToInt32(), 0);
            // add a separator
            AppendMenu(menu, 0xA00, 0, null);
            // add an item with a unique ID
            AppendMenu(menu, 0, 1234, "About cfw");
        }
        protected override void WndProc(ref System.Windows.Forms.Message m) {
            // memorize Alt status: no clue why this is needed here too, also implemented in IMessageFilter, PreFilterMessage 
            //            if ( ModifierKeys == Keys.Alt ) {
            //                m_bAltKeyActive = true;
            //            } else {
            //                m_bAltKeyActive = false;
            //            }

            // show About box: WM_SYSCOMMAND is 0x112
            if ( m.Msg == 0x112 ) {
                // check for added menu item ID
                if ( m.WParam.ToInt32() == 1234 ) {
                    // show About box here...
                    About dlg = new About();
                    dlg.ShowDialog();
                    dlg.Dispose();
                }
            }

            // any media change: USB, CD, DVD
            if ( m.Msg == WM_DEVICECHANGE ) {
                if ( (int)m.WParam == DBT_DEVICEQUERYREMOVE ) {
                    // we need to stop the FS watcher als well as the drive detector
                    if ( this.m_driveDetector[0] != null ) {
                        if ( this.fileSystemWatcherLeft.Path.StartsWith(this.m_driveDetector[0].HookedDrive) ) {
                            string drive = this.m_driveDetector[0].HookedDrive;
                            this.m_driveDetector[0].DisableQueryRemove();
                            this.m_driveDetector[0].Dispose();
                            this.m_driveDetector[0] = null;
                            this.fileSystemWatcherLeft.EnableRaisingEvents = false;
                            MediaChangeEvent(this, new MediaChangeEventArgs(MediaChangeEventArgs.Media.queryremove, drive));
                        }
                    }
                    if ( this.m_driveDetector[1] != null ) {
                        if ( this.fileSystemWatcherRight.Path.StartsWith(this.m_driveDetector[1].HookedDrive) ) {
                            string drive = this.m_driveDetector[1].HookedDrive;
                            this.m_driveDetector[1].DisableQueryRemove();
                            this.m_driveDetector[1].Dispose();
                            this.m_driveDetector[1] = null;
                            this.fileSystemWatcherRight.EnableRaisingEvents = false;
                            MediaChangeEvent(this, new MediaChangeEventArgs(MediaChangeEventArgs.Media.queryremove, drive));
                        }
                    }
                }
                if ( (int)m.WParam == DBT_DEVICEARRIVAL ) {
                    // update SelectFileFolder dlg
                    if ( this.m_sff != null ) {
                        if ( this.m_sff.Visible ) {
                            this.m_sff.RefreshRequest("new media");
                        } else {
                            this.m_sffNeedsRefresh = true;
                        }
                    }
                    // fire media change event
                    MediaChangeEvent(this, new MediaChangeEventArgs(MediaChangeEventArgs.Media.arrived));
                }
                if ( (int)m.WParam == DBT_DEVICEREMOVECOMPLETE ) {
                    // update SelectFileFolder dlg
                    if ( this.m_sff != null ) {
                        if ( this.m_sff.Visible ) {
                            this.m_sff.RefreshRequest("media removed");
                        } else {
                            this.m_sffNeedsRefresh = true;
                        }
                    }
                    // fire media change event
                    MediaChangeEvent(this, new MediaChangeEventArgs(MediaChangeEventArgs.Media.removed));
                }
            }

            // base behaviour
            base.WndProc(ref m);
        }

        // go one folder level up
        private void buttonLhsUp_Click(object sender, EventArgs e) {
            this.ListviewOneLevelUp(Side.left);
        }
        private void buttonRhsUp_Click(object sender, EventArgs e) {
            this.ListviewOneLevelUp(Side.right);
        }

        // navigate thru folder history
        private void buttonLhsPrev_Click(object sender, EventArgs e) {
            this.m_bRunSize = false;
            string[] directories = this.m_Panel.button(Side.left).Tag.ToString().Split(Path.DirectorySeparatorChar);
            string selectItem = (directories[directories.Count() - 1].Length > 0) ? directories[directories.Count() - 1] : directories[0];
            string folder = this.m_Panel.folders.GetPreviousFolder(Side.left, this.m_Panel.GetActiveTabIndex(Side.left));
            this.LoadListView(Side.left, folder, selectItem);
        }
        private void buttonLhsNext_Click(object sender, EventArgs e) {
            this.m_bRunSize = false;
            string[] directories = this.m_Panel.button(Side.left).Tag.ToString().Split(Path.DirectorySeparatorChar);
            string selectItem = (directories[directories.Count() - 1].Length > 0) ? directories[directories.Count() - 1] : directories[0];
            string folder = this.m_Panel.folders.GetNextFolder(Side.left, this.m_Panel.GetActiveTabIndex(Side.left));
            this.LoadListView(Side.left, folder, selectItem);
        }
        private void buttonRhsNext_Click(object sender, EventArgs e) {
            this.m_bRunSize = false;
            string[] directories = this.m_Panel.button(Side.right).Tag.ToString().Split(Path.DirectorySeparatorChar);
            string selectItem = (directories[directories.Count() - 1].Length > 0) ? directories[directories.Count() - 1] : directories[0];
            string folder = this.m_Panel.folders.GetNextFolder(Side.right, this.m_Panel.GetActiveTabIndex(Side.right));
            this.LoadListView(Side.right, folder, selectItem);
        }
        private void buttonRhsPrev_Click(object sender, EventArgs e) {
            this.m_bRunSize = false;
            string[] directories = this.m_Panel.button(Side.right).Tag.ToString().Split(Path.DirectorySeparatorChar);
            string selectItem = (directories[directories.Count() - 1].Length > 0) ? directories[directories.Count() - 1] : directories[0];
            string folder = this.m_Panel.folders.GetPreviousFolder(Side.right, this.m_Panel.GetActiveTabIndex(Side.right));
            this.LoadListView(Side.right, folder, selectItem);
        }

        // show folder history list
        private void ShowFolderList(Side side) {
            this.m_bRunSize = false;
            // prepare list content
            List<string> ls = new List<string>(this.m_Panel.folders.GetFolderList(side, this.m_Panel.GetActiveTabIndex(side)).Where(c => c != null));
            ls.Insert(0, "<return>");
            // form with history list is placed at the current mouse position, shifted vertically by 1/2 of an item height
            Rectangle wa = Screen.PrimaryScreen.WorkingArea;
            this.m_sl = new SimpleList(ls, this.m_Panel.folders.GetCurrentIndex(side, this.m_Panel.GetActiveTabIndex(side)));
            this.m_sl.Left = MousePosition.X - this.m_sl.Width / 2;
            if ( this.m_sl.Right > wa.Right ) {
                this.m_sl.Left -= (this.m_sl.Right - wa.Right);
            }
            if ( this.m_sl.Left < wa.Left ) {
                this.m_sl.Left -= (this.m_sl.Left - wa.Left);
            }
            this.m_sl.Top = MousePosition.Y - this.m_sl.GetListItemHeight / 2;
            // show simple list dlg
            this.m_sl.ShowDialog();
            // the list might have changed
            List<string> retList = this.m_sl.GetStringList;
            if ( retList.Count != this.m_Panel.folders.GetFolderList(side, this.m_Panel.GetActiveTabIndex(side)).Count ) {
                this.m_Panel.folders.ClearFolderHistory(side);
                for ( int i = retList.Count - 1; i >= 0; i-- ) {
                    if ( retList[i] != null ) {
                        this.m_Panel.folders.InsertTopFolder(side, this.m_Panel.GetActiveTabIndex(side), retList[i]);
                    }
                }
            }
            // dlg ended with Esc or <return>
            if ( (this.m_sl.ReturnFolder.Length == 0) || (this.m_sl.ReturnFolder == "<return>") ) {
                this.m_sl.Dispose();
                return;
            }
            // now the serious stuff with the new folder
            string folder = this.m_sl.ReturnFolder;
            int index = this.m_sl.ReturnIndex - 1;
            if ( GrzTools.FileTools.PathExists(folder, 500, this.m_WPD) ) {
                // load folder to switch to
                this.LoadListView(side, folder, "");
                // set history list index according to recently set index
                this.m_Panel.folders.SetCurrentIndex(side, this.m_Panel.GetActiveTabIndex(side), index);
            } else {
                // give up
                if ( (folder == "My Computer") || (folder == "Computer") || (folder == @"Computer\Shared Folders") ) {
                    this.LoadListView(side, folder, "");
                    this.m_Panel.folders.SetCurrentIndex(side, this.m_Panel.GetActiveTabIndex(side), index);
                } else {
                    if ( MessageBox.Show(string.Format("The folder\r\r{0}\r\rdoesn't exist on this PC.\rIts entry will be removed from the history list?", folder), "Question", MessageBoxButtons.YesNo) == DialogResult.Yes ) {
                        this.m_Panel.folders.DeleteFolderByIndexFromList(side, this.m_Panel.GetActiveTabIndex(side), index);
                    }
                }
            }
            this.m_sl.Dispose();
        }
        private void buttonRhsUp_MouseEnter(object sender, EventArgs e) {
            this.ShowFolderList(Side.right);
        }
        private void buttonLhsUp_MouseEnter(object sender, EventArgs e) {
            this.ShowFolderList(Side.left);
        }
        private void buttonRhsPrev_MouseDown(object sender, MouseEventArgs e) {
            if ( e.Button == MouseButtons.Right ) {
                this.ShowFolderList(Side.right);
            }
        }
        private void buttonRhsNext_MouseDown(object sender, MouseEventArgs e) {
            if ( e.Button == MouseButtons.Right ) {
                this.ShowFolderList(Side.right);
            }
        }
        private void buttonRhsUp_MouseDown(object sender, MouseEventArgs e) {
            if ( e.Button == MouseButtons.Right ) {
                this.ShowFolderList(Side.right);
            }
        }
        private void buttonLhsUp_MouseDown(object sender, MouseEventArgs e) {
            if ( e.Button == MouseButtons.Right ) {
                this.ShowFolderList(Side.left);
            }
        }
        private void buttonLhsNext_MouseDown(object sender, MouseEventArgs e) {
            if ( e.Button == MouseButtons.Right ) {
                this.ShowFolderList(Side.left);
            }
        }
        private void buttonLhsPrev_MouseDown(object sender, MouseEventArgs e) {
            if ( e.Button == MouseButtons.Right ) {
                this.ShowFolderList(Side.left);
            }
        }

        // helper: get TabPage from TabControl via mouseposition when mouse hovering
        int m_lastActiveTabPageIndex = -1;
        private void tabControlLR_MouseMove(object sender, MouseEventArgs e) {
            int tabIndex = this.GetPageIndexByPoint((TabControl)sender, e.Location);
            if ( this.m_lastActiveTabPageIndex != tabIndex ) {
                this.m_lastActiveTabPageIndex = tabIndex;
                this.m_toolTip.Hide((TabControl)sender);
                this.buttonLeftRight_MouseEnter(sender, e);
            }
        }
        TabPage GetPageByPoint(TabControl tabControl, Point point) {
            for ( int i = 0; i < tabControl.TabPages.Count; i++ ) {
                TabPage page = tabControl.TabPages[i];
                if ( tabControl.GetTabRect(i).Contains(point) ) {
                    this.m_lastActiveTabPageIndex = i;
                    return page;
                }
            }
            return null;
        }
        int GetPageIndexByPoint(TabControl tabControl, Point point) {
            for ( int i = 0; i < tabControl.TabPages.Count; i++ ) {
                TabPage page = tabControl.TabPages[i];
                if ( tabControl.GetTabRect(i).Contains(point) ) {
                    return i;
                }
            }
            return -1;
        }
        // show full path when hovering the top buttons + labelPrompt AND the full path is not visible due to its length
        private void buttonLeftRight_MouseEnter(object sender, EventArgs e) {
            Point pt = ((Control)sender).PointToClient(MousePosition);
            // button & label
            string fullText = ((Control)sender).Tag.ToString();
            string shrtText = ((Control)sender).Text;
            int height = ((Control)sender).Height;
            // tabs
            if ( sender is TabControl ) {
                TabPage tp = this.GetPageByPoint((TabControl)sender, pt);
                if ( tp == null ) {
                    return;
                }
                fullText = tp.Tag.ToString();
                shrtText = tp.Text;
                height = 78;
            }
            // common
            if ( fullText.Length != shrtText.Length ) {
                Size textSize = TextRenderer.MeasureText(fullText, ((Control)sender).Font);
                pt.Y = ((Control)sender).Location.Y - height;
                pt.X -= textSize.Width / 2;
                if ( sender == this.labelPrompt ) {
                    pt.Y = ((Control)sender).Location.Y + height;
                }
                if ( this.PointToScreen(pt).X < 0 ) {
                    pt.X -= this.PointToScreen(pt).X;
                }
                this.m_toolTip.Show(fullText, (Control)sender, pt);
            }
        }
        private void buttonLeftRight_MouseLeave(object sender, EventArgs e) {
            // only make tooltip disappear, if there is no FlyingLabel
            if ( this.m_fldlg.IsDisposed ) {
                this.m_toolTip.Hide((Control)sender);
            }
            this.m_lastActiveTabPageIndex = -1;
        }

        // 20160824: make path under hovering mouse cursor selectable similar to what explorer does
        string _lastSubPath = "";
        bool _bLockFlyingLabel = false;
        Point _eLocation = new Point();
        DateTime _lastActiveFlyingLabel = DateTime.Now;
        private void buttonLeftRight_MouseMove(object sender, MouseEventArgs e) {
            // prevent showing a flying label in case the previous one was recently closed (mouse move debouncing) - otherwise we won't be able to see the right mouse click menu
            if ( (DateTime.Now - this._lastActiveFlyingLabel).Milliseconds < 50 ) {
                return;
            }

            // same position as last time?
            if ( e.Location == this._eLocation ) {
                return;
            }
            this._eLocation = e.Location;

            // locked due to folder change?
            if ( this._bLockFlyingLabel ) {
                return;
            }

            // make sure the call came from a button
            Control ctl = FromHandle(((Control)sender).Handle);
            if ( ctl.GetType() != typeof(Button) ) {
                return;
            }

            // get button text, current mouse position in button, button text length, button width, button text start position in pixels
            string path = ((Control)sender).Text;
            float mousePosX = ((Control)sender).PointToClient(MousePosition).X;
            float textWidth = TextRenderer.MeasureText(path, ((Control)sender).Font).Width;
            float butnWidth = ((Control)sender).Size.Width;
            float textStrtX = (butnWidth - textWidth) / 2;

            // return if mouse is not pointing into text of the button
            if ( (mousePosX < textStrtX) || (mousePosX > textStrtX + textWidth) ) {
                this._lastSubPath = "";
                return;
            }

            // get index of char under mouse cursor --> ndx
            string pathTrail = "";
            int ndx = 0;
            for ( int i = 0; i < path.Length; i++ ) {
                int pos = TextRenderer.MeasureText(path.Substring(0, path.Length - i), ((Control)sender).Font).Width;
                if ( (textStrtX + pos) < mousePosX ) {
                    ndx = path.Length - i + 1;
                    break;
                }
            }
            ndx = Math.Min(path.Length - 1, ndx);
            pathTrail = path.Substring(ndx);

            // get sub path (containing char under cursor) surrounded by \\, could contain ... --> needed to correct 
            string[] splitArr = path.Split('\\');
            string curChar = path.Substring(ndx, 1);
            int posArr = 0;
            int posTest = 0;
            for ( int i = 0; i < splitArr.Length; i++ ) {
                posTest += splitArr[i].Length + 1;
                if ( posTest > ndx ) {
                    posArr = i;
                    break;
                }
            }
            string subPath = splitArr[posArr];
            string tailPath = "";
            for ( int i = posArr + 1; i < splitArr.Length; i++ ) {
                tailPath += splitArr[i] + '\\';
            }
            tailPath = tailPath.TrimEnd('\\');

            // allows "menu right click" to pop up
            if ( this._lastSubPath == subPath ) {
                return;
            }
            this._lastSubPath = subPath;

            // build full path beginning from left until subPath under cursor and remove a trailing '\'
            string fullPath = "";
            for ( int i = 0; i <= posArr; i++ ) {
                fullPath += splitArr[i] + '\\';
            }
            fullPath = fullPath.TrimEnd('\\');

            // get the path preceeding subPath, needed to show subPath exactly overlapping the original button text
            string ahdPath = "";
            string[] splitFullArr = ((Control)sender).Text.Split('\\');
            for ( int i = 0; i < posArr; i++ ) {
                ahdPath += splitFullArr[i] + '\\';
            }
            int aheadLength = TextRenderer.MeasureText(ahdPath, ((Control)sender).Font).Width;

            // correct for ... contained in selected text
            if ( fullPath.Contains("...") ) {
                // did we click left or right or in the middle of the dots?
                int posDots = path.IndexOf(" ... ");
                int pixDots = TextRenderer.MeasureText(path.Substring(0, posDots), ((Control)sender).Font).Width;

                if ( mousePosX < pixDots + textStrtX ) {
                    // we take the left part of the ...
                    posDots = fullPath.IndexOf(" ... ");
                    fullPath = fullPath.Substring(0, posDots);
                    string realPath = ((Control)sender).Tag.ToString();
                    int posReal = realPath.IndexOf('\\', posDots);
                    if ( posReal != -1 ) {
                        fullPath = realPath.Substring(0, posReal);
                        int endPos = fullPath.LastIndexOf('\\');
                        if ( endPos != -1 ) {
                            subPath = fullPath.Substring(Math.Min(endPos + 1, fullPath.Length - 1));
                        }
                    } else {
                        // give up and take the right portion of " ... "
                        fullPath = realPath;
                        int subStartPos = fullPath.LastIndexOf('\\');
                        if ( subStartPos != -1 ) {
                            subPath = fullPath.Substring(Math.Min(subStartPos + 1, fullPath.Length - 1));
                        }
                    }
                } else {
                    fullPath = "";
                    subPath = "";
                    // we take the part right of the ...
                    if ( !pathTrail.Contains("...") ) {
                        // find pathTrail in full path beginning from its tail
                        string realPath = ((Control)sender).Tag.ToString();
                        int pathTrailPos = realPath.LastIndexOf(pathTrail);
                        if ( pathTrailPos != -1 ) {
                            // extend full path toward tail after pathTrail until the next \\ is found
                            int nextSlashPos = realPath.IndexOf('\\', pathTrailPos);
                            if ( nextSlashPos != -1 ) {
                                fullPath = realPath.Substring(0, nextSlashPos);
                                int endPos = fullPath.LastIndexOf('\\');
                                if ( endPos != -1 ) {
                                    subPath = fullPath.Substring(Math.Min(endPos + 1, fullPath.Length - 1));
                                }
                                // find splitArr text in subPath
                                for ( int i = 0; i < splitArr.Length; i++ ) {
                                    string workString = splitArr[i];
                                    if ( workString.Contains("...") ) {
                                        workString = workString.Substring(workString.IndexOf(" ... ") + 5);
                                    }
                                    if ( subPath.Contains(workString) ) {
                                        // delta between workString and splitArr[i]
                                        int delta = subPath.Length - workString.Length;
                                        // adjust aheadLength
                                        int dotspos = splitArr[i].IndexOf(" ... ");
                                        string txt = splitArr[i].Substring(0, Math.Max(0, dotspos - delta));
                                        int ofs = TextRenderer.MeasureText(txt, ((Control)sender).Font).Width;
                                        aheadLength += ofs;
                                        break;
                                    }
                                }
                            } else {
                                // if the is no slash after pathTrail, then full path is ready
                                fullPath = realPath;
                                int subStartPos = fullPath.LastIndexOf('\\');
                                if ( subStartPos != -1 ) {
                                    subPath = fullPath.Substring(Math.Min(subStartPos + 1, fullPath.Length - 1));
                                }
                            }
                        }
                    } else {
                        // mouse is pointing directly into the ' ... ' sequence of a shortened path
                        // TBD: either provide something useful or leave it as it is, because this will only happen in very rare cases, though forget it 
                    }
                }
            }

            // calc label show position relative to button
            Point pt = new Point();
            pt.Y = ((Control)sender).Location.Y + 1;
            pt.X = (int)textStrtX + (aheadLength > 0 ? aheadLength - 8 : -1);

            // show FlyingLabel
            try {
                if ( this.m_fldlg.IsDisposed || (this.m_fldlg == null) ) {
                    this.m_fldlg = new FlyingLabel("empty");
                    this.m_fldlg.ChangeEvent += this.FlyingLabelEvent;
                }
                this.m_fldlg.TailPath = tailPath;
                this.m_fldlg.LabelText = subPath;
                this.m_fldlg.FullPath = fullPath;
                this.m_fldlg.Location = ((Control)sender).PointToScreen(pt);
                this.m_fldlg.Show();
            } catch ( Exception ex ) {
                MessageBox.Show(ex.Message);
            }
        }
        void FlyingLabelEvent(object sender, FlyingLabel.ChangeEventArgs e) {
            string selectItem = "";
            if ( this.m_fldlg != null ) {
                selectItem = this.m_fldlg.TailPath;
                int ndx = selectItem.IndexOf('\\');
                if ( ndx != -1 ) {
                    selectItem = selectItem.Substring(0, ndx);
                }
            }
            this.closeFlyingLabel();

            // since there was a click to the FlyingLabel, we activate the corresponding side 
            Side side = Side.left;
            int mousePosX = this.PointToClient(MousePosition).X;
            if ( mousePosX >= this.Width / 2 ) {
                side = Side.right;
            }
            this.m_Panel.SetActiveSide(side);

            if ( e.fullpath.Length > 0 ) {
                // 20161016 (removed 2nd side): change path of active side according to what was selected on the label (if selection makes sense)
                this._bLockFlyingLabel = true;
                try {
                    if ( this.m_Panel.button(side).Tag.ToString().Contains(e.fullpath) ) {
                        this.LoadListView(side, e.fullpath, selectItem);
                        this.m_Panel.folders.InsertTopFolder(side, this.m_Panel.GetActiveTabIndex(side), e.fullpath);
                    }
                } catch ( Exception ) {; }
                this._bLockFlyingLabel = false;
            }
        }
        private void closeFlyingLabel() {
            if ( !this.m_fldlg.IsDisposed && (this.m_fldlg != null) ) {
                this.m_fldlg.ChangeEvent -= this.FlyingLabelEvent;
                this.m_fldlg.Close();
            }

            this._bLockFlyingLabel = false;
            this._lastSubPath = "";

            this._lastActiveFlyingLabel = DateTime.Now;
        }

        // dialog about what file types shall have a preview
        private void previewToolStripMenuItem_Click(object sender, EventArgs e) {
            PreviewMode pm = new PreviewMode();
            pm.Doc = this.m_bDoc;
            pm.Img = this.m_bImg;
            pm.Zip = this.m_bZip;
            pm.Pdf = this.m_bPdf;
            pm.Htm = this.m_bHtm;
            pm.AsIs = this.m_bAsIs;
            pm.CfwVideo = this.m_bCfwVideo;
            pm.WmpVideo = this.m_bWmpVideo;
            pm.WmpAudio = this.m_bWmpAudio;
            if ( pm.ShowDialog() == System.Windows.Forms.DialogResult.OK ) {
                this.m_bDoc = pm.Doc;
                this.m_bImg = pm.Img;
                this.m_bZip = pm.Zip;
                this.m_bPdf = pm.Pdf;
                this.m_bHtm = pm.Htm;
                this.m_bAsIs = pm.AsIs;
                this.m_bWmpVideo = pm.WmpVideo;
                this.m_bCfwVideo = pm.CfwVideo;
                this.m_bWmpAudio = pm.WmpAudio;
                this.previewCtl.SetPreviewFiles(this.m_bImg, this.m_bDoc, this.m_bPdf, this.m_bHtm, this.m_bZip, this.m_bAsIs, this.m_bCfwVideo, this.m_bWmpAudio, this.m_bWmpVideo);
            }
            pm.Dispose();
        }

        // via FileMenu: assignment of function keys
        private void assignF1F2F11F12ToolStripMenuItem_Click(object sender, EventArgs e) {
            bool bAdm1 = false;
            if ( (this.buttonF1.Image != null) && (this.buttonF1.Image.Tag == this.m_imgAdm.Tag) ) {
                bAdm1 = true;
            }
            bool bAdm2 = false;
            if ( (this.buttonF2.Image != null) && (this.buttonF2.Image.Tag == this.m_imgAdm.Tag) ) {
                bAdm2 = true;
            }
            bool bAdm11 = false;
            if ( (this.buttonF11.Image != null) && (this.buttonF11.Image.Tag == this.m_imgAdm.Tag) ) {
                bAdm11 = true;
            }
            bool bAdm12 = false;
            if ( (this.buttonF12.Image != null) && (this.buttonF12.Image.Tag == this.m_imgAdm.Tag) ) {
                bAdm12 = true;
            }
            using ( AssignFKeys dlg = new AssignFKeys(this.buttonF1.Text, this.buttonF2.Text, this.buttonF11.Text, this.buttonF12.Text,
                                                       (string)this.buttonF1.Tag, (string)this.buttonF2.Tag, (string)this.buttonF11.Tag, (string)this.buttonF12.Tag,
                                                       bAdm1, bAdm2, bAdm11, bAdm12) ) {
                if ( dlg.ShowDialog() == DialogResult.OK ) {
                    Icon tmpIco = null;

                    this.buttonF1.Text = dlg.f1text;
                    this.buttonF1.Tag = dlg.f1prog;
                    tmpIco = GrzTools.RegisteredFileType.ExtractIconFromFile(this.@buttonF1.Tag.ToString(), false);
                    this.buttonF1.Image = dlg.f1admin ? this.m_imgAdm : tmpIco != null ? tmpIco.ToBitmap() : null;
                    this.buttonF2.Text = dlg.f2text;
                    this.buttonF2.Tag = dlg.f2prog;
                    tmpIco = GrzTools.RegisteredFileType.ExtractIconFromFile(this.@buttonF2.Tag.ToString(), false);
                    this.buttonF2.Image = dlg.f2admin ? this.m_imgAdm : tmpIco != null ? tmpIco.ToBitmap() : null;
                    this.buttonF11.Text = dlg.f11text;
                    this.buttonF11.Tag = dlg.f11prog;
                    tmpIco = GrzTools.RegisteredFileType.ExtractIconFromFile(this.@buttonF11.Tag.ToString(), false);
                    this.buttonF11.Image = dlg.f11admin ? this.m_imgAdm : tmpIco != null ? tmpIco.ToBitmap() : null;
                    this.buttonF12.Text = dlg.f12text;
                    this.buttonF12.Tag = dlg.f12prog;
                    tmpIco = GrzTools.RegisteredFileType.ExtractIconFromFile(this.@buttonF12.Tag.ToString(), false);
                    this.buttonF12.Image = dlg.f12admin ? this.m_imgAdm : tmpIco != null ? tmpIco.ToBitmap() : null;
                    GrzTools.IniFile ini = new GrzTools.IniFile(System.Windows.Forms.Application.ExecutablePath + ".ini");
                    ini.IniWriteValue("cfw", "F1txt", this.buttonF1.Text);
                    ini.IniWriteValue("cfw", "F1prg", this.buttonF1.Tag.ToString());
                    ini.IniWriteValue("cfw", "F1adm", this.buttonF1.Image != null ? (this.buttonF1.Image.Tag == this.m_imgAdm.Tag ? this.buttonF1.Image.Tag.ToString() : "") : "");
                    ini.IniWriteValue("cfw", "F2txt", this.buttonF2.Text);
                    ini.IniWriteValue("cfw", "F2prg", this.buttonF2.Tag.ToString());
                    ini.IniWriteValue("cfw", "F2adm", this.buttonF2.Image != null ? (this.buttonF2.Image.Tag == this.m_imgAdm.Tag ? this.buttonF2.Image.Tag.ToString() : "") : "");
                    ini.IniWriteValue("cfw", "F11txt", this.buttonF11.Text);
                    ini.IniWriteValue("cfw", "F11prg", this.buttonF11.Tag.ToString());
                    ini.IniWriteValue("cfw", "F11adm", this.buttonF11.Image != null ? (this.buttonF11.Image.Tag == this.m_imgAdm.Tag ? this.buttonF11.Image.Tag.ToString() : "") : "");
                    ini.IniWriteValue("cfw", "F12txt", this.buttonF12.Text);
                    ini.IniWriteValue("cfw", "F12prg", this.buttonF12.Tag.ToString());
                    ini.IniWriteValue("cfw", "F12adm", this.buttonF12.Image != null ? (this.buttonF12.Image.Tag == this.m_imgAdm.Tag ? this.buttonF12.Image.Tag.ToString() : "") : "");
                }
            }
        }
        // via right click on buttons F1F2F11F12: assignment of function keys
        private void buttonF1F2F11F12_MouseDown(object sender, MouseEventArgs e) {
            if ( e.Button == MouseButtons.Right ) {
                this.assignF1F2F11F12ToolStripMenuItem_Click(null, null);
            }
        }

        // run cmd.exe as Admin and change directory 
        public void runAdminCmdHere(string directory, bool runAsAdmin) {
            Process prc = new Process();
            prc.StartInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe", "/k cd /D " + @directory);
            prc.StartInfo.UseShellExecute = true;
            prc.StartInfo.CreateNoWindow = false;
            if ( runAsAdmin ) {
                prc.StartInfo.Verb = "runas";
            }
            try {
                prc.Start();
            } catch ( Exception ) {; }
        }
        // via FileMenu & ListContextMenu: run cmd.exe as Admin and change directory 
        private void runCmdexeHereToolStripMenuItem_Click(object sender, EventArgs e) {
            string @selection = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[this.m_Panel.GetActiveView().SelectedIndices[0]].Text;
            if ( @selection == "[..]" ) {
                @selection = "";
            }
            string @file = @Path.Combine(this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), @selection);

            if ( file[1] != ':' ) {
                if ( file == @"Computer\Downloads" ) {
                    @file = @GrzTools.FileTools.TranslateSpecialFolderNames("Downloads");
                }
                if ( file == @"Computer\Desktop" ) {
                    @file = @GrzTools.FileTools.TranslateSpecialFolderNames("Desktop");
                }
                if ( file == @"Computer\Documents" ) {
                    @file = @GrzTools.FileTools.TranslateSpecialFolderNames("Documents");
                }
                if ( file == @"Computer\Shared Folders" ) {
                    @file = this.DRVC;
                }
                if ( file == @"Computer\Network" ) {
                    @file = this.DRVC;
                }
            }

            if ( File.Exists(@file) ) {
                @file = Path.GetDirectoryName(@file);
            }

            bool runAsAdmin = false;
            if ( this.runCmdexeHereToolStripMenuItem1.Text.Contains("admin") ) {
                runAsAdmin = true;
            }
            if ( (Control.MouseButtons & MouseButtons.Right) == MouseButtons.Right ) {
                runAsAdmin = false;
            }

            this.runAdminCmdHere(@file, runAsAdmin);
            return;
        }
        private void runCmdexeHereToolStripMenuItem1_MouseDown(object sender, MouseEventArgs e) {
            if ( (Control.MouseButtons & MouseButtons.Right) == MouseButtons.Right ) {
                this.runCmdexeHereToolStripMenuItem_Click(sender, e);
            }
        }

        // function keys: F1 F2 F11 F12
        private void buttonFx_Click(object sender, EventArgs e) {
            Process mProcess = new Process();
            mProcess.StartInfo = new System.Diagnostics.ProcessStartInfo(((Button)sender).Tag.ToString());
            mProcess.StartInfo.Verb = "";
            mProcess.StartInfo.WorkingDirectory = this.@m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString();
            if ( (((Button)sender).Image != null) && (((Button)sender).Image.Tag == this.m_imgAdm.Tag) ) {
                mProcess.StartInfo.Verb = "runas";
                // special case cmd.exe as Admin
                if ( mProcess.StartInfo.FileName.StartsWith("cmd.exe", StringComparison.InvariantCultureIgnoreCase) ) {
                    this.runAdminCmdHere(this.@m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), true);
                    return;
                }
            }
            try {
                mProcess.Start();
            } catch ( Exception ) {; }
        }

        // cfw localization via menu command
        private void setlanguageToolStripMenuItem_Click(object sender, EventArgs e) {
            // the toolstripitem which was clicked on carries in its Tag the selected culture info
            string toolitemtagstr = ((ToolStripMenuItem)sender).Tag.ToString();
            // we iterate all suitems to languageToolStripMenuItem and set the chaeckmark accordingly
            foreach ( ToolStripMenuItem ti in this.languageToolStripMenuItem.DropDownItems ) {
                if ( ti.Tag.ToString().Contains(toolitemtagstr) ) {
                    ti.Checked = true;
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(toolitemtagstr);
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(toolitemtagstr);
                } else {
                    ti.Checked = false;
                }
            }
            // Restart cfw with changed culture setting
            string exeName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            ProcessStartInfo startInfo = new ProcessStartInfo(exeName);
            try {
                System.Diagnostics.Process.Start(startInfo);
                this.Close();
            } catch ( Exception ) {; }
        }

        // distinguish between sorting by filename vs. extension AND force sorting
        private void SortOrderToolStripMenuItem_Click(object sender, EventArgs e) {
            // block resize events
            this.m_dtLastLoad = DateTime.Now;
            // name vs. ext.
            if ( this.colheadLHS_contextMenuStrip.Items[0] == (ToolStripMenuItem)sender ) {
                ((ToolStripMenuItem)this.colheadLHS_contextMenuStrip.Items[0]).Checked = true;
                ((ToolStripMenuItem)this.colheadLHS_contextMenuStrip.Items[1]).Checked = false;
                this.SortListView(Side.left, 0, true);
            } else {
                if ( this.colheadLHS_contextMenuStrip.Items[1] == (ToolStripItem)sender ) {
                    ((ToolStripMenuItem)this.colheadLHS_contextMenuStrip.Items[1]).Checked = true;
                    ((ToolStripMenuItem)this.colheadLHS_contextMenuStrip.Items[0]).Checked = false;
                    this.SortListView(Side.left, 0, true);
                } else {
                    if ( this.colheadRHS_contextMenuStrip.Items[0] == (ToolStripItem)sender ) {
                        ((ToolStripMenuItem)this.colheadRHS_contextMenuStrip.Items[0]).Checked = true;
                        ((ToolStripMenuItem)this.colheadRHS_contextMenuStrip.Items[1]).Checked = false;
                        this.SortListView(Side.right, 0, true);
                    } else {
                        if ( this.colheadRHS_contextMenuStrip.Items[1] == (ToolStripItem)sender ) {
                            ((ToolStripMenuItem)this.colheadRHS_contextMenuStrip.Items[1]).Checked = true;
                            ((ToolStripMenuItem)this.colheadRHS_contextMenuStrip.Items[0]).Checked = false;
                            this.SortListView(Side.right, 0, true);
                        }
                    }
                }
            }
        }

        // network drive mapping dialog
        private void mapNetworkDriveToolStripMenuItem_Click(object sender, EventArgs e) {
            string selText = "";
            if ( this.m_Panel.button(this.m_Panel.GetActiveSide()).Text == "Computer" ) {
                ListView lv = this.m_Panel.GetActiveView();
                if ( lv != null ) {
                    if ( lv.SelectedIndices.Count > 0 ) {
                        if ( (new[] { 5, 9 }).Contains(lv.Items[lv.SelectedIndices[0]].ImageIndex) ) {
                            selText = lv.Items[lv.SelectedIndices[0]].Text;
                        }
                    }
                }
            }

            NetworkMapping nmdlg = new NetworkMapping(selText);
            DialogResult dlr = nmdlg.ShowDialog(this);
            nmdlg.Dispose();
        }

        // run as Admin
        private void runAsAdministratorToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.m_Panel.GetActiveView().SelectedIndices.Count == 0 ) {
                return;
            }
            ListViewItem lvi = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[this.m_Panel.GetActiveView().SelectedIndices[0]];
            string file = lvi.Text;
            if ( (file == "[..]") && (lvi.ImageIndex == 2) ) {  // aka "LevelUp"
                return;
            }
            // selected file
            string path = this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString();
            string full = Path.Combine(path, file);
            // start program and run as admin
            ProcessStartInfo startInfo = new ProcessStartInfo(full);
            startInfo.Verb = "runas";
            try {
                System.Diagnostics.Process.Start(startInfo);
            } catch ( Exception ) {
                // exception is thrown, when selection is not an executable - now we start this like normal 
                startInfo = new ProcessStartInfo(full);
                System.Diagnostics.Process.Start(startInfo);
            }
        }

        // needed for virtual ListView mode
        private void listViewRight_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) {
            ListViewItem[] lviarr = this.m_Panel.GetListViewArr(Side.right);
            if ( (lviarr == null) || (lviarr.Length == 0) ) {
                string[] cols = new string[8];
                e.Item = new ListViewItem(cols, 0);
                return;
            }
            int ndx = Math.Max(0, Math.Min(e.ItemIndex, lviarr.Length - 1));
            e.Item = lviarr[ndx];
        }
        private void listViewLeft_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) {
            ListViewItem[] lviarr = this.m_Panel.GetListViewArr(Side.left);
            if ( (lviarr == null) || (lviarr.Length == 0) ) {
                string[] cols = new string[8];
                e.Item = new ListViewItem(cols, 0);
                return;
            }
            int ndx = Math.Max(0, Math.Min(e.ItemIndex, lviarr.Length - 1));
            e.Item = lviarr[ndx];
        }
        // I accidentally overlooked this function, it is needed for FindItemWithText: therefore I implemented m_Panel.FindListViewArrItemWithText, which does something similar
        private void listViewRight_SearchForVirtualItem(object sender, SearchForVirtualItemEventArgs e) {
        }
        private void listViewLeft_SearchForVirtualItem(object sender, SearchForVirtualItemEventArgs e) {
        }

        // synced ListView scrolling
        private void listsScrollSynchonizedToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.detailsLeftToolStripMenuItem.Checked && this.detailsLeftToolStripMenuItem.Enabled && this.detailsRightToolStripMenuItem.Checked && this.detailsRightToolStripMenuItem.Enabled ) {
                if ( this.listsScrollSynchonizedToolStripMenuItem.Checked ) {
                    this.m_listViewL.Buddy = this.m_listViewR;
                    this.m_listViewR.Buddy = this.m_listViewL;
                } else {
                    this.m_listViewL.Buddy = null;
                    this.m_listViewR.Buddy = null;
                }
            } else {
                this.listsScrollSynchonizedToolStripMenuItem.Checked = false;
                GrzTools.AutoMessageBox.Show("This mode only works, when both lists are set to 'Details'.", "Note", 3000);
            }
        }

        // reload listviews regardless, whether option is enabled or not
        private void computerShowsShareFoldersToolStripMenuItem_Click(object sender, EventArgs e) {
            this.LoadListView(Side.left, this.m_Panel.button(Side.left).Tag.ToString(), "");
            this.LoadListView(Side.right, this.m_Panel.button(Side.right).Tag.ToString(), "");
        }
        private void computerShowsFolderSizesToolStripMenuItem_Click(object sender, EventArgs e) {
            this.LoadListView(Side.left, this.m_Panel.button(Side.left).Tag.ToString(), "");
            this.LoadListView(Side.right, this.m_Panel.button(Side.right).Tag.ToString(), "");
        }

        // maintain folder history affects buttons' visibility
        private void folderHistoryToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.folderHistoryToolStripMenuItem.Checked ) {
                this.m_Panel.folders.MaintainFolderHistory = true;
                this.buttonLhsPrev.Visible = true;
                this.buttonLhsNext.Visible = true;
                this.buttonRhsPrev.Visible = true;
                this.buttonRhsNext.Visible = true;
                this.tableLayoutPanelLhsButtons.SetCellPosition(this.buttonLeft, new TableLayoutPanelCellPosition(3, 0));
                this.tableLayoutPanelLhsButtons.SetColumnSpan(this.buttonLeft, 1);
                this.tableLayoutPanelRhsButtons.SetCellPosition(this.buttonRight, new TableLayoutPanelCellPosition(0, 0));
                this.tableLayoutPanelRhsButtons.SetColumnSpan(this.buttonRight, 1);
                if ( sender != null ) {
                    // we only put something in, if call came from menu - otherwise c: would be added to the list thru startup
                    this.m_Panel.folders.InsertTopFolder(Side.left, this.m_Panel.GetActiveTabIndex(Side.left), this.m_Panel.button(Side.left).Tag.ToString());
                    this.m_Panel.folders.InsertTopFolder(Side.right, this.m_Panel.GetActiveTabIndex(Side.right), this.m_Panel.button(Side.right).Tag.ToString());
                }
            } else {
                this.m_Panel.folders.MaintainFolderHistory = false;
                this.m_Panel.folders.ClearFolderHistory(Side.left);
                this.m_Panel.folders.ClearFolderHistory(Side.right);
                this.tableLayoutPanelLhsButtons.SetCellPosition(this.buttonLeft, new TableLayoutPanelCellPosition(0, 0));
                this.tableLayoutPanelLhsButtons.SetColumnSpan(this.buttonLeft, 3);
                this.tableLayoutPanelRhsButtons.SetCellPosition(this.buttonRight, new TableLayoutPanelCellPosition(0, 0));
                this.tableLayoutPanelRhsButtons.SetColumnSpan(this.buttonRight, 3);
                this.buttonLhsPrev.Visible = false;
                this.buttonLhsNext.Visible = false;
                this.buttonRhsPrev.Visible = false;
                this.buttonRhsNext.Visible = false;
            }
        }

        private void loupeToolStripMenuItem_Click(object sender, EventArgs e) {
            Loupe lp = new Loupe();
            lp.StartPosition = FormStartPosition.Manual;
            lp.Location = new Point(this.Location.X - lp.Width - 5, this.Location.Y + 5);
            lp.Show(this);
        }

        private void autoNetworkScanToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.m_sff != null ) {
                this.m_sff.AutoNetworkScan = this.autoNetworkScanToolStripMenuItem.Checked;
            }
        }

        private void myIPAddressToolStripMenuItem_Click(object sender, EventArgs e) {
            IpEtc dlg = new IpEtc();
            dlg.StartPosition = FormStartPosition.Manual;
            dlg.Location = new Point(this.Location.X + (this.Width - dlg.Width) / 2, this.Location.Y + 100);
            dlg.Show(this);
        }

        private void highlightEmptyFolderToolStripMenuItem_Click(object sender, EventArgs e) {
            this.LoadListView(Side.left, this.m_Panel.button(Side.left).Tag.ToString(), "*");
            this.LoadListView(Side.right, this.m_Panel.button(Side.right).Tag.ToString(), "*");
        }

        // switch both listviews' sides
        private void listsExchangeToolStripMenuItem_Click(object sender, EventArgs e) {
            // memorize right button info
            string pathOldLeftSide = this.m_Panel.button(Side.left).Tag.ToString();

            // load left listview with right button info
            this.LoadListView(Side.left, this.m_Panel.button(Side.right).Tag.ToString(), "");
            this.m_Panel.folders.InsertTopFolder(Side.left, this.m_Panel.GetActiveTabIndex(Side.left), this.m_Panel.button(Side.right).Tag.ToString());

            // load right listview with old left button info
            this.LoadListView(Side.right, pathOldLeftSide, "");
            this.m_Panel.folders.InsertTopFolder(Side.left, this.m_Panel.GetActiveTabIndex(Side.left), pathOldLeftSide);

            // render label
            if ( this.m_Panel.GetActiveSide() == Side.left ) {
                this.m_Panel.RenderListviewLabel(Side.left);
            } else {
                this.m_Panel.RenderListviewLabel(Side.right);
            }
        }

        // "Computer" view context menu behaviour
        private void contextMenuStripComputerView_Opening(object sender, CancelEventArgs e) {
            // in case of "eject" and "format drive" and "rename"
            this.ejectDriveToolStripMenuItem.Enabled = false;
            this.formatDriveToolStripMenuItem1.Enabled = false;
            if ( this.renameToolStripMenuItem2.Checked ) {
                this.renameToolStripMenuItem2.Enabled = true;
            } else {
                this.renameToolStripMenuItem2.Enabled = false;
            }
            // what side was clicked? 
            Side side = this.m_Panel.GetActiveSide();
            if ( this.m_Panel.button(side).Tag.ToString() == "Computer" ) {
                this.ejectDriveToolStripMenuItem.Text = "Eject Drive";
                // is there any selection?
                if ( (this.m_Panel.listview(side)).SelectedIndices.Count == 0 ) {
                    return;
                }
                // no data?
                if ( this.m_Panel.GetListViewArr(side) == null ) {
                    return;
                }
                // What item shall be treated here?
                int pos = this.m_Panel.listview(side).SelectedIndices[0];
                ListViewItem lvi = this.m_Panel.GetListViewArr(side)[pos];
                string selection = lvi.Text;
                // is the selection other than a drive?
                if ( selection[1] != ':' ) {
                    return;
                }
                if ( !(new[] { 4, 5, 6, 7, 9 }).Contains(lvi.ImageIndex) ) {
                    return;
                }
                // allow format & rename drive 
                this.formatDriveToolStripMenuItem1.Enabled = true;
                this.renameToolStripMenuItem2.Enabled = true;

                // allow eject drive only for Network-drives, USB-drives and ROM
                if ( !(new[] { 5, 9, 6, 7 }).Contains(lvi.ImageIndex) ) {
                    return;
                }
                if ( (new[] { 5, 9 }).Contains(lvi.ImageIndex) ) {
                    this.ejectDriveToolStripMenuItem.Text = "Disconnect Network Drive";
                }

                // ROM must be ready
                DriveInfo di = new DriveInfo(selection.Substring(0, 1));
                if ( di.DriveType == DriveType.CDRom ) {
                    if ( !di.IsReady ) {
                        return;
                    }
                }

                // we reach this point only for removable (USB & CD) and network drives
                this.ejectDriveToolStripMenuItem.Enabled = true;
            }
        }

        // file menu toolstrip behaviour
        private void toolStripMenuItem3_DropDownOpening(object sender, EventArgs e) {
            // zIPFilesToolStripMenuItem with or w/o password protection
            // new file OR new folder depending on Modifier
            if ( (ModifierKeys & Keys.Shift) == Keys.Shift ) {
                this.zIPFilesToolStripMenuItem.Text = "Make ZIP with Password";
                this.newFolderToolStripMenuItem.Text = "New File";
                this.newFolderToolStripMenuItem.Image = cfw.Properties.Resources.document;
            } else {
                this.zIPFilesToolStripMenuItem.Text = "Make ZIP";
                this.newFolderToolStripMenuItem.Text = "New Folder";
                this.newFolderToolStripMenuItem.Image = cfw.Properties.Resources.Folder;
            }

            // enable/disable WinMerge mode
            this.winMergeTwoFilesToolStripMenuItem.Enabled = GrzTools.InstalledPrograms.ProgramPath("winmerge").Length > 0;

            // in case of "eject" and "format drive"
            this.ejectUSBDriveToolStripMenuItem.Enabled = false;
            this.formatDriveToolStripMenuItem.Enabled = false;
            // what side was clicked? 
            Side side = this.m_Panel.GetActiveSide();
            if ( this.m_Panel.button(side).Tag.ToString() == "Computer" ) {
                // is there any selection?
                if ( (this.m_Panel.listview(side)).SelectedIndices.Count == 0 ) {
                    return;
                }
                // no data?
                if ( this.m_Panel.GetListViewArr(side) == null ) {
                    return;
                }
                // What item shall be treated here?
                int pos = this.m_Panel.listview(side).SelectedIndices[0];
                ListViewItem lvi = this.m_Panel.GetListViewArr(side)[pos];
                string selection = lvi.Text;
                // is the selection a drive?
                if ( selection[1] != ':' ) {
                    return;
                }
                if ( !(new[] { 4, 5, 6, 7, 9 }).Contains(lvi.ImageIndex) ) {
                    return;
                }
                // allow format drive 
                this.formatDriveToolStripMenuItem.Enabled = true;

                // allow eject drive only for USB-drives
                if ( lvi.ImageIndex != 7 ) {
                    return;
                }
                this.ejectUSBDriveToolStripMenuItem.Enabled = true;
            }
        }
        private void menuStrip1_KeyDown(object sender, KeyEventArgs e) {
            if ( (ModifierKeys & Keys.Shift) == Keys.Shift ) {
                this.menuStrip1.Hide();
                this.menuStrip1.Show();
            }
        }
        private void menuStrip1_KeyPress(object sender, KeyPressEventArgs e) {
        }
        private void menuStrip1_KeyUp(object sender, KeyEventArgs e) {
            if ( (ModifierKeys & Keys.Shift) == Keys.Shift ) {
                this.menuStrip1.Hide();
                this.menuStrip1.Show();
            }
        }

        // sometimes it's nice to simply ignore FS change events
        private void filesystemMonitoringToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.filesystemMonitoringToolStripMenuItem.Checked ) {
                if ( System.IO.Directory.Exists(this.fileSystemWatcherLeft.Path) ) {
                    this.fileSystemWatcherLeft.EnableRaisingEvents = true;
                    this.LoadListView(Side.left, this.m_Panel.button(Side.left).Tag.ToString(), "");
                }
                if ( System.IO.Directory.Exists(this.fileSystemWatcherRight.Path) ) {
                    this.fileSystemWatcherRight.EnableRaisingEvents = true;
                    this.LoadListView(Side.right, this.m_Panel.button(Side.right).Tag.ToString(), "");
                }
            } else {
                this.fileSystemWatcherLeft.EnableRaisingEvents = false;
                this.fileSystemWatcherRight.EnableRaisingEvents = false;
            }
        }

        // formatting a drive via explorer dlg
        private void formatDriveToolStripMenuItem_Click(object sender, EventArgs e) {
            // what side was clicked? 
            Side side = this.m_Panel.GetActiveSide();

            // we let it work only in Computer view
            if ( this.m_Panel.button(side).Tag.ToString() != "Computer" ) {
                return;
            }

            // is there any selection?
            if ( (this.m_Panel.listview(side)).SelectedIndices.Count == 0 ) {
                return;
            }
            // no data?
            if ( this.m_Panel.GetListViewArr(side) == null ) {
                return;
            }
            // What item shall be treated here?
            int pos = this.m_Panel.listview(side).SelectedIndices[0];
            ListViewItem lvi = this.m_Panel.GetListViewArr(side)[pos];
            string selection = lvi.Text;
            // is the selection a drive?
            if ( selection[1] != ':' ) {
                return;
            }
            if ( !(new[] { 4, 5, 6, 7, 9 }).Contains(lvi.ImageIndex) ) {
                return;
            }

            // get the drive number from the drive letter
            DriveInfo drive = new DriveInfo(selection.Substring(0, 2));
            byte[] bytes = Encoding.ASCII.GetBytes(drive.Name.ToCharArray());
            uint driveNumber = Convert.ToUInt32(bytes[0] - Encoding.ASCII.GetBytes(new[] { 'A' })[0]);

            // 20160320: create if needed an exe file ( just containing SHFormatDrive(..) ) on the fly and start it in a separate process, completely independent from cfw - even when cfw closes
            FormatDrive.Go(driveNumber.ToString());
        }

        //
        // a class, which compiles an exe on the fly (if needed) and runs it in a separate process
        //
        class FormatDrive {
            // source code to compile: it's a program, calling the windows explorer's format drive dialog and starting it in a separate process
            // reason: if we would start it from the cfw-thread, it would get killed when cfw closes
            static readonly string m_code =
            "using System;" +
            "using System.Runtime.InteropServices;" +
            "public class cfwformatdrive" +
            "{" +
            "    [DllImport(\"shell32.dll\")]" +
            "    static extern uint SHFormatDrive(IntPtr hwnd, uint drive, uint fmtID, uint options);" +
            "    static void Main(string[] args) {" +
            "        if ( args.Length == 0 ) {" +
            "           Console.WriteLine(\"missing argument\");" +
            "           return;" +
            "        }" +
            "        SHFormatDrive(IntPtr.Zero, uint.Parse(args[0]), (uint)0xFFFF, 0);" +
            "    }" +
            "}";

            // compiles the source code and runs the just generated exe file
            public static void Go(string driveNumber) {
                // exe file name (despite of it's obfuscating name .bin)
                string fn = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "cfwformatdrive.bin");
                // create exe only if necessary
                if ( !File.Exists(fn) ) {
                    CompilerParameters cp = new CompilerParameters();
                    cp.OutputAssembly = fn;
                    cp.GenerateInMemory = false;
                    cp.GenerateExecutable = true;
                    string[] references = { "System.dll" };
                    cp.ReferencedAssemblies.AddRange(references);
                    CSharpCodeProvider provider = new CSharpCodeProvider();
                    CompilerResults compile = provider.CompileAssemblyFromSource(cp, m_code);
                    if ( compile.Errors.HasErrors ) {
                        string text = "Compile error: ";
                        foreach ( CompilerError ce in compile.Errors ) {
                            text += "\r\n" + ce.ToString();
                        }
                        throw new Exception(text);
                    }
                }
                // execute exe: 3x startinfo setting needed to get rid of the cmd-window
                Process prc = new Process();
                prc.StartInfo = new System.Diagnostics.ProcessStartInfo(fn);
                prc.StartInfo.UseShellExecute = false;
                prc.StartInfo.CreateNoWindow = true;
                prc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                prc.StartInfo.Arguments = driveNumber;
                prc.Start();
            }
        }

        //
        // the buttons (left/right) have their own context menu, this method makes initial settings regarding "eject drive"
        //
        private void contextMenuStripButtons_Opening(object sender, CancelEventArgs e) {
            this.ejectDriveToolStripMenuItem1.Text = "Eject Drive";
            // check whether "eject" is applicable
            this.ejectDriveToolStripMenuItem1.Enabled = false;
            string drivename = this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString();
            if ( (drivename.Length > 1) && (drivename[1] == ':') ) {
                DriveInfo di = new DriveInfo(drivename);
                // allow USB eject
                if ( di.DriveType == DriveType.Removable ) {
                    this.ejectDriveToolStripMenuItem1.Enabled = true;
                }
                // allow ROM eject
                if ( di.DriveType == DriveType.CDRom ) {
                    if ( di.IsReady ) {
                        this.ejectDriveToolStripMenuItem1.Enabled = true;
                    }
                }
                // allow Network drive to disconnect
                if ( di.DriveType == DriveType.Network ) {
                    this.ejectDriveToolStripMenuItem1.Text = "Disconnect Network Drive";
                    this.ejectDriveToolStripMenuItem1.Enabled = true;
                }
            }
            // clear & clone tabs
            this.clearTabToolStripMenuItem.Visible = false;
            this.cloneTabToolStripMenuItem.Visible = false;
            this.clearTabToolStripMenuItem.Enabled = false;
            this.cloneTabToolStripMenuItem.Enabled = false;
            ContextMenuStrip menu = sender as ContextMenuStrip;
            Control sourceControl = menu.SourceControl;
            if ( (sourceControl == this.tabControlRight) || (sourceControl == this.tabControlLeft) ) {
                if ( ((TabControl)sourceControl).SelectedIndex == 0 ) {
                    this.clearTabToolStripMenuItem.Visible = true;
                    this.cloneTabToolStripMenuItem.Visible = true;
                    this.cloneTabToolStripMenuItem.Enabled = true;
                } else {
                    this.clearTabToolStripMenuItem.Visible = true;
                    this.cloneTabToolStripMenuItem.Visible = true;
                    this.clearTabToolStripMenuItem.Enabled = true;
                    this.cloneTabToolStripMenuItem.Enabled = true;
                }
            }
        }
        // the buttons (left/right) have their own context menu, one entry allows to eject drives
        private void ejectDriveToolStripMenuItem1_Click(object sender, EventArgs e) {
            // get drive
            Side side = this.m_Panel.GetActiveSide();
            string drivename = this.m_Panel.button(side).Tag.ToString();
            // obtain drive info
            DriveInfo di = new DriveInfo(drivename);
            // eject USB drive
            if ( di.DriveType == DriveType.Removable ) {
                // stop FS watchdog 
                if ( side == Side.left ) {
                    if ( this.m_driveDetector[0] != null ) {
                        this.m_driveDetector[0].DisableQueryRemove();
                        this.m_driveDetector[0].Dispose();
                        this.m_driveDetector[0] = null;
                    }
                    this.fileSystemWatcherLeft.EnableRaisingEvents = false;
                    this.m_Panel.SetFileSystemWatcher(Side.left, drivename, false);
                }
                if ( side == Side.right ) {
                    if ( this.m_driveDetector[1] != null ) {
                        this.m_driveDetector[1].DisableQueryRemove();
                        this.m_driveDetector[1].Dispose();
                        this.m_driveDetector[1] = null;
                    }
                    this.fileSystemWatcherRight.EnableRaisingEvents = false;
                    this.m_Panel.SetFileSystemWatcher(Side.right, drivename, false);
                }
                drivename = drivename.Substring(0, 2);
                GrzTools.UsbEject.VolumeDeviceClass volumes = new GrzTools.UsbEject.VolumeDeviceClass();
                foreach ( GrzTools.UsbEject.Volume vol in volumes.Devices ) {
                    if ( (vol.LogicalDrive != null) && vol.LogicalDrive.Equals(drivename) ) {
                        vol.Eject(false);
                        break;
                    }
                }
                // update SelectFileFolder dlg
                if ( this.m_sff != null ) {
                    if ( this.m_sff.Visible ) {
                        this.m_sff.RefreshRequest("media removed");
                    } else {
                        this.m_sffNeedsRefresh = true;
                    }
                }
                // fire media change event and signal to switch to Computer view
                this.m_sDriveToComputer = drivename;
                MediaChangeEvent(this, new MediaChangeEventArgs(MediaChangeEventArgs.Media.removed));
            }
            // eject ROM
            if ( di.DriveType == DriveType.CDRom ) {
                string driveLetter = drivename.Substring(0, 1);
                if ( di.IsReady ) {
                    string returnString = "";
                    mciSendStringA("open " + driveLetter + ": type CDAudio alias drive" + driveLetter, returnString, 0, 0);
                    mciSendStringA("set drive" + driveLetter + " door open", returnString, 0, 0);
                    mciSendStringA("open", returnString, 0, 0);
                    mciSendStringA("Set CDAudio Door Open", returnString, 0, 0);
                    // update SelectFileFolder dlg
                    if ( this.m_sff != null ) {
                        if ( this.m_sff.Visible ) {
                            this.m_sff.RefreshRequest("media removed");
                        } else {
                            this.m_sffNeedsRefresh = true;
                        }
                    }
                    // fire media change event and signal to switch to Computer view
                    this.m_sDriveToComputer = drivename;
                    MediaChangeEvent(this, new MediaChangeEventArgs(MediaChangeEventArgs.Media.removed));
                }
            }
            // allow Network drive to disconnect
            if ( di.DriveType == DriveType.Network ) {
                string driveToUnMap = di.Name.Substring(0, 1);
                GrzTools.DriveSettings.DisconnectNetworkDrive(driveToUnMap, true, false);
                // update SelectFileFolder dlg
                if ( this.m_sff != null ) {
                    if ( this.m_sff.Visible ) {
                        this.m_sff.RefreshRequest("media removed");
                    } else {
                        this.m_sffNeedsRefresh = true;
                    }
                }
                // fire media change event and signal to switch to Computer view
                this.m_sDriveToComputer = drivename;
                MediaChangeEvent(this, new MediaChangeEventArgs(MediaChangeEventArgs.Media.removed));
            }
        }

        // event handler for "click on eject drive"
        private void ejectUSBDriveToolStripMenuItem_Click(object sender, EventArgs e) {
            // what side was clicked? 
            Side side = this.m_Panel.GetActiveSide();

            // we let it work only in Computer view
            if ( this.m_Panel.button(side).Tag.ToString() != "Computer" ) {
                return;
            }

            // is there any selection?
            if ( (this.m_Panel.listview(side)).SelectedIndices.Count == 0 ) {
                return;
            }
            // no data?
            if ( this.m_Panel.GetListViewArr(side) == null ) {
                return;
            }
            // What item shall be treated here?
            int pos = this.m_Panel.listview(side).SelectedIndices[0];
            ListViewItem lvi = this.m_Panel.GetListViewArr(side)[pos];
            string selection = lvi.Text;
            // is the selection a drive?
            if ( selection[1] != ':' ) {
                return;
            }
            // drive name 
            string eject_drive = lvi.Text.Substring(0, 2);
            // USB drive
            if ( lvi.ImageIndex == 7 ) {
                // finally eject
                GrzTools.UsbEject.VolumeDeviceClass volumes = new GrzTools.UsbEject.VolumeDeviceClass();
                foreach ( GrzTools.UsbEject.Volume vol in volumes.Devices ) {
                    if ( (vol.LogicalDrive != null) && vol.LogicalDrive.Equals(eject_drive) ) {
                        vol.Eject(false);
                        break;
                    }
                }
            }
            // 20160320: open ROM
            if ( lvi.ImageIndex == 6 ) {
                string driveLetter = eject_drive.Substring(0, 1);
                DriveInfo di = new DriveInfo(driveLetter);
                if ( (di.DriveType == DriveType.CDRom) && (di.Name.Contains(eject_drive)) ) {
                    if ( di.IsReady ) {
                        string returnString = "";
                        mciSendStringA("open " + driveLetter + ": type CDAudio alias drive" + driveLetter, returnString, 0, 0);
                        mciSendStringA("set drive" + driveLetter + " door open", returnString, 0, 0);
                        mciSendStringA("open", returnString, 0, 0);
                        mciSendStringA("Set CDAudio Door Open", returnString, 0, 0);
                    }
                }
            }
            // 20161016: disconnect network drive
            if ( (new[] { 5, 9 }).Contains(lvi.ImageIndex) ) {
                string driveLetter = eject_drive.Substring(0, 1);
                GrzTools.DriveSettings.DisconnectNetworkDrive(driveLetter, true, false);
            }
        }
        // Hex Viewer
        private void viewHexModeToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.m_Panel.GetActiveView().SelectedIndices.Count == 0 ) {
                return;
            }
            // WPD
            if ( this.m_Panel.listType[(int)this.m_Panel.GetActiveSide()] != ListType.FileSystem ) {
                MessageBox.Show("Not yet implemented for WPD.", "Sorry");
                return;
            }
            string thefile = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[this.m_Panel.GetActiveView().SelectedIndices[0]].Text;
            string filename = Path.Combine(this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), thefile);
            if ( !File.Exists(filename) ) {
                SelectFolderOrFile sff = new SelectFolderOrFile();
                sff.Text = "Select File";
                sff.DefaultPath = this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString();
                DialogResult dlr = sff.ShowDialog(this);
                if ( dlr != System.Windows.Forms.DialogResult.OK ) {
                    sff.Dispose();
                    return;
                }
                filename = sff.ReturnFile;
                sff.Dispose();
            }
            try {
                HexViewerForm form = new HexViewerForm(filename);
                form.Show();
            } catch ( Exception ex ) {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        // Hex Editor
        private void hexEditorToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.m_Panel.GetActiveView().SelectedIndices.Count == 0 ) {
                return;
            }
            // WPD
            if ( this.m_Panel.listType[(int)this.m_Panel.GetActiveSide()] != ListType.FileSystem ) {
                MessageBox.Show("Not yet implemented for WPD.", "Sorry");
                return;
            }
            string thefile = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[this.m_Panel.GetActiveView().SelectedIndices[0]].Text;
            string filename = Path.Combine(this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), thefile);
            if ( !File.Exists(filename) ) {
                SelectFolderOrFile sff = new SelectFolderOrFile();
                sff.Text = "Select File";
                sff.DefaultPath = this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString();
                DialogResult dlr = sff.ShowDialog(this);
                if ( dlr != System.Windows.Forms.DialogResult.OK ) {
                    sff.Dispose();
                    return;
                }
                filename = sff.ReturnFile;
                sff.Dispose();
            }
            try {
                HexEdit form = new HexEdit(filename);
                form.Show();
            } catch ( Exception ex ) {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        // split container allows to change panel width, here click right on splitter resets any width change
        private void setSplitContainerBar(int position) {
            if ( position == -1 ) {
                this.splitContainer1.SplitterDistance = (this.splitContainer1.Width - this.splitContainer1.SplitterWidth) / 2;
            } else {
                this.splitContainer1.SplitterDistance = Math.Max(position, 200);
            }
            this.m_iSplitterPosition = position;
        }
        private void splitContainer1_MouseDown(object sender, MouseEventArgs e) {
            if ( e.Button == MouseButtons.Right ) {
                this.setSplitContainerBar(-1);
            }
        }
        private void listsCenterSplitterBarToolStripMenuItem_Click(object sender, EventArgs e) {
            this.setSplitContainerBar(-1);
        }
        // resizing the splitter container
        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e) {
            if ( this.m_bInitOngoing ) {
                return;
            }

            // when resizing the splitter container, we need to re adjust the column width of the listviews
            this.listViewFitColumnsToolStripMenuItem_Click(null, null);
            // in case of preview 
            if ( (this.previewCtl != null) && this.previewCtl.Visible ) {
                this.previewCtl.RePaint();
            }
            // render the top buttons containing the pathes
            if ( this.m_Panel == null ) {
                return;
            }
            this.m_Panel.SetButtonText(Side.left, this.m_Panel.button(Side.left).Tag.ToString(), this.m_sLHSfilter);
            this.m_Panel.SetButtonText(Side.right, this.m_Panel.button(Side.right).Tag.ToString(), this.m_sRHSfilter);
            // render lables below the list views
            this.m_Panel.RenderListviewLabel(Side.left);
            this.m_Panel.RenderListviewLabel(Side.right);
            // set splitter container position
            int center = this.splitContainer1.Width / 2;
            bool bHitCenter = false;
            if ( (this.splitContainer1.SplitterDistance > (center - 20)) && (this.splitContainer1.SplitterDistance < (center + 20)) ) {
                bHitCenter = true;
            }
            this.m_iSplitterPosition = bHitCenter ? -1 : this.splitContainer1.SplitterDistance;
        }

        // a table of available shortcuts
        private void shortcutsToolStripMenuItem_Click(object sender, EventArgs e) {
            Form fc = Application.OpenForms["ShowShortcuts"];
            if ( fc == null ) {
                this.m_frm = new ShowShortcuts(this);
            }
            if ( !this.m_frm.Visible ) {
                this.m_frm.Show();
            } else {
                this.m_frm.BringToFront();
            }
        }

        // save URL as windows file in MHT format
        public string ConvertToWindowsFileName(string urlText) {
            List<string> urlParts = new List<string>();
            string rt = "";
            Regex r = new Regex(@"[a-z]+", RegexOptions.IgnoreCase);
            foreach ( Match m in r.Matches(urlText) ) {
                urlParts.Add(m.Value);
            }
            for ( int i = 0; i < urlParts.Count; i++ ) {
                rt = rt + urlParts[i];
                rt = rt + "_";
            }
            return rt;
        }
        private void saveHTMLAsMHTSingleFileToolStripMenuItem_Click(object sender, EventArgs e) {
            SimpleInput ib = new SimpleInput();
            ib.Hint = "The URL below will be saved as MHT single file in the active folder.";
            ib.Input = "paste URL here ...";
            ib.Text = "Paste URL into edit box";
            ib.ShowDialog();
            if ( ib.DialogResult == DialogResult.OK ) {
                string url = ib.Input;
                CDO.Message msg = new CDO.MessageClass();
                CDO.Configuration cfg = new CDO.ConfigurationClass();
                msg.Configuration = cfg;
                try {
                    msg.CreateMHTMLBody(url, CDO.CdoMHTMLFlags.cdoSuppressAll, "", "");
                    string urlFile = this.ConvertToWindowsFileName(url) + ".mht";
                    string outFile = Path.Combine(this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), urlFile); //20161016: was previously Download folder 
                    msg.GetStream().SaveToFile(outFile, ADODB.SaveOptionsEnum.adSaveCreateOverWrite);
                } catch ( Exception ) {
                    MessageBox.Show("Could not open URL.", "Error");
                }
            }
        }

        // place a text to image
        public Bitmap watermarkBitmap(string oriFilename, bool bFilename, bool bTimestamp) {
            Bitmap bmp = new Bitmap(oriFilename);
            if ( !bFilename && !bTimestamp ) {
                return bmp;
            }

            string stampFinal = "";

            if ( bFilename ) {
                stampFinal = Path.GetFileName(oriFilename);
            }
            if ( bTimestamp ) {
                FileInfo fi = new FileInfo(oriFilename);
                stampFinal += " " + fi.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss");
            }

            // watermark text height
            int heightText = bmp.Height / 30;
            // choose font for text
            Font font = new Font("Arial", heightText, FontStyle.Bold, GraphicsUnit.Pixel);
            // choose color
            Color color = Color.Black;
            // location of the watermark text in the image
            Point pt = new Point(5, 5);
            SolidBrush brush = new SolidBrush(color);
            // draw text on white background into image
            try {
                using ( Graphics graphics = Graphics.FromImage(bmp) ) {
                    int lengthText = (int)graphics.MeasureString(stampFinal, font).Width;
                    graphics.FillRectangle(Brushes.White, 0, 0, lengthText + 10, heightText + 10);
                    graphics.DrawString(stampFinal, font, brush, pt);
                }
            } catch {; }
            // return modified image back to original imagefile
            return bmp;
        }
        // determine whether file is of image type
        bool isImage(string file) {
            string ext = Path.GetExtension(file);
            if ( ".jpg.jpeg.JPG.JPEG".IndexOf(ext) != -1 ) {
                return true;
            }
            return false;
        }
        bool hasJpegHeader2(string filename) {
            using ( BinaryReader br = new BinaryReader(File.Open(filename, FileMode.Open)) ) {
                UInt16 soi = br.ReadUInt16();    // Start of Image (SOI) marker (FFD8)
                UInt16 marker = br.ReadUInt16(); // JFIF marker (FFE0) or EXIF marker(FF01)
                return soi == 0xd8ff && (marker & 0xe0ff) == 0xe0ff;
            }
        }
        bool hasJpegHeader(string filename) {
            try {
                // 0000000: ffd8 ffe0 0010 4a46 4946 0001 0101 0048  ......JFIF.....H
                // 0000000: ffd8 ffe1 14f8 4578 6966 0000 4d4d 002a  ......Exif..MM.*    
                using ( BinaryReader br = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read)) ) {
                    UInt16 soi = br.ReadUInt16();        // Start of Image (SOI) marker (FFD8)
                    UInt16 marker = br.ReadUInt16();     // JFIF marker (FFE0) EXIF marker (FFE1)
                    //UInt16 markerSize = br.ReadUInt16(); // size of marker data (incl. marker)
                    //UInt32 four = br.ReadUInt32();       // JFIF 0x4649464a or Exif  0x66697845

                    Boolean isJpeg = soi == 0xd8ff && (marker & 0xe0ff) == 0xe0ff;
                    //Boolean isExif = isJpeg && four == 0x66697845;
                    //Boolean isJfif = isJpeg && four == 0x4649464a;

                    //if ( isJpeg ) {
                    //    if ( isExif )
                    //        Console.WriteLine("EXIF: {0}", filename);
                    //    else if ( isJfif )
                    //        Console.WriteLine("JFIF: {0}", filename);
                    //    else
                    //        Console.WriteLine("JPEG: {0}", filename);
                    //}

                    return isJpeg;
                    //return isJfif;
                    //return isExif;
                }
            } catch {
                return false;
            }
        }
        // convert a sequence of images to video
        async private void imageSequenceToToolStripMenuItem_Click(object sender, EventArgs e) {
            // WPD
            if ( this.m_Panel.listType[(int)this.m_Panel.GetActiveSide()] != ListType.FileSystem ) {
                MessageBox.Show("Not yet implemented for WPD.", "Sorry");
                return;
            }
            // if there is no reasonable selection
            Side side = this.m_Panel.GetActiveSide();
            ListView lv = this.m_Panel.GetActiveView();
            ListViewItem[] lvarr = this.m_Panel.GetListViewArr(side);
            int fileCount = lv.SelectedIndices.Count;
            if ( lv.SelectedIndices.Count < 2 ) {
                if ( MessageBox.Show("Too few selection of JPG images.\r\n\r\nSelect all JPG files?", "Question", MessageBoxButtons.OKCancel) != System.Windows.Forms.DialogResult.OK ) {
                    return;
                }
                lv.SelectedIndexChanged -= new System.EventHandler(this.listViewLeftRight_SelectedIndexChanged);
                lv.BeginUpdate();
                lv.SelectedIndices.Clear();
                string wildcard = "*.jpg";
                Regex regex = GrzTools.FastFileFind.FindFilesPatternToRegex.Convert(wildcard);
                for ( int i = 0; i < lvarr.Length; i++ ) {
                    if ( regex.IsMatch(lvarr[i].Text) ) {
                        lv.SelectedIndices.Add(i);
                    }
                }
                fileCount = lv.SelectedIndices.Count;
                lv.EndUpdate();
                lv.SelectedIndexChanged += new System.EventHandler(this.listViewLeftRight_SelectedIndexChanged);
            }
            fileCount = lv.SelectedIndices.Count;
            if ( lv.SelectedIndices.Count < 2 ) {
                // finally give up
                MessageBox.Show("Too few selection of images", "Note");
                return;
            }

            // create a video output filename
            string folder = this.m_Panel.button(side).Tag.ToString();
            string finalOutFile = Path.GetFileName(folder) + ".avi";
            finalOutFile = Path.Combine(folder, finalOutFile);
            if ( File.Exists(finalOutFile) ) {
                if ( DialogResult.Yes != MessageBox.Show("'" + finalOutFile + "' already exists.\n\nIt will be overwritten.", "Note", MessageBoxButtons.YesNoCancel) ) {
                    return;
                }
            }

            // stop all file system monitoring activities
            this.m_bFileSystemChangeActionLeft = false;
            this.m_bFileSystemChangeActionRight = false;

            // 1st image in sequence determines Size for all other images 
            string file = lvarr[lv.SelectedIndices[0]].Text;
            string fullpath = Path.Combine(folder, file);
            Bitmap image = new Bitmap(fullpath);
            Size size = new Size(image.Width, image.Height);
            image.Dispose();

            // create video writer instance
            char[] arr = "DIVX".ToCharArray();
            Emgu.CV.VideoWriter writerOpenCV = new Emgu.CV.VideoWriter(finalOutFile, Emgu.CV.VideoWriter.Fourcc(arr[0], arr[1], arr[2], arr[3]), 25, size, true);

            // init progress dlg
            SimpleProgress sp = new SimpleProgress();
            sp.StartPosition = FormStartPosition.Manual;
            sp.Location = new Point(this.Location.X + (this.Width - m_sp.Width) / 2, this.Location.Y + 100);
            sp.Text = "Conversion Progress";
            sp.LabelPercent = "0%";
            sp.ProgressValue = 0;
            sp.FrameCount = fileCount;
            sp.Show(this);

            // write bitmaps into video file
            Monitor.Enter(lv);
            int index = 0;
            foreach ( int ndx in lv.SelectedIndices ) {
                Application.DoEvents();
                // the file
                file = lvarr[ndx].Text;
                fullpath = Path.Combine(folder, file);
                // an item must be selected, an existing file and an image (by extension), otherwise the files are simply ignored
                if ( File.Exists(fullpath) && this.isImage(fullpath) ) {
                    // watermark?
                    image = this.watermarkBitmap(fullpath, this.imageFilenameWatermarkToolStripMenuItem.Checked, this.imageTimestampWatermarkToolStripMenuItem.Checked);
                    // resize?
                    if ( (image.Width != size.Width) || (image.Height != size.Height) ) {
                        image = this.resizeBitmap(image, size);
                    }
                    // write image
                    Emgu.CV.Image<Emgu.CV.Structure.Bgr, Byte> imageCV = new Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte>(image);
                    writerOpenCV.Write(imageCV.Mat.Clone());
                    imageCV.Dispose();
                    image.Dispose();
                    // progress
                    sp.UpdateStatus(String.Format("frame = {0}({1})", index, fileCount), index++);
                    Application.DoEvents();
                }
            }
            Monitor.Exit(lv);

            // close video file & progress
            writerOpenCV.Dispose();
            sp.Close();

            // get out here in case the listview tab was changed while the images were processed
            if ( this.m_Panel.button(side).Tag.ToString() != folder ) {
                this.m_bFileSystemChangeActionLeft = true;
                this.m_bFileSystemChangeActionRight = true;
                return;
            }

            // remove selection from items
            lv = this.m_Panel.GetActiveView();
            lv.SelectedIndices.Clear();

            // re load list
            string lookForFile = Path.GetFileName(finalOutFile);
            await this.LoadListView(side, this.m_Panel.button(side).Tag.ToString(), lookForFile);
            if ( this.m_Panel.button(side).Tag.ToString() == this.m_Panel.button(this.m_Panel.GetPassiveSide()).Tag.ToString() ) {
                await this.LoadListView(this.m_Panel.GetPassiveSide(), this.m_Panel.button(this.m_Panel.GetPassiveSide()).Tag.ToString(), lookForFile);
            }

            // 1501 limit not yet processed
            if ( !this.m_bgRunWorkerCompleted ) {
                Stopwatch sw = new Stopwatch();
                do {
                    Application.DoEvents();
                } while ( !this.m_bgRunWorkerCompleted && (sw.ElapsedMilliseconds < 2000) );
            }

            // select just generated video file
            ListViewItem outItem = this.m_Panel.FindListViewArrItemWithText(side, lookForFile, -1);
            if ( (outItem != null) && ((int)outItem.Tag != -1) ) {
                outItem.Selected = true;
            } else {
                MessageBox.Show("Creating video sequence from images failed.", "Error");
            }

            // re enable file system monitoring activities
            this.m_bFileSystemChangeActionLeft = true;
            this.m_bFileSystemChangeActionRight = true;
        }
        Bitmap resizeBitmap(Bitmap srcImage, Size newSize) {
            Bitmap newImage = new Bitmap(newSize.Width, newSize.Height);
            using ( Graphics gr = Graphics.FromImage(newImage) ) {
                gr.Clear(Color.Black);
                // do some math to keep aspect ratio
                double srcMultiplierW = newImage.Width / (double)srcImage.Width;
                double srcMultiplierH = newImage.Height / (double)srcImage.Height;
                double srcMultiplier = Math.Min(srcMultiplierW, srcMultiplierH);
                double w = srcMultiplier * srcImage.Width;
                double h = srcMultiplier * srcImage.Height;
                int x = (newSize.Width - (int)(w + 0.5)) / 2;
                int y = (newSize.Height - (int)(h + 0.5)) / 2;
                // image resize quality criteria
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                // draw image at offset location with new size
                gr.DrawImage(srcImage, new Rectangle(x, y, (int)w, (int)h));
            }
            return newImage;
        }
        private void imageSequenceToToolStripMenuItem_MouseEnter(object sender, EventArgs e) {
            // prevent menu item from close itself: http://stackoverflow.com/questions/13350171/how-to-leave-toolstripmenu-open-after-clicking-an-item
            this.imageSequenceToToolStripMenuItem.DropDown.AutoClose = false;
        }
        private void imageSequenceToToolStripMenuItem_MouseLeave(object sender, EventArgs e) {
            this.imageSequenceToToolStripMenuItem.DropDown.AutoClose = true;
        }

        // hide/show immediate folder sizes
        void removeFolderSizes(Side side) {
            string text = "<SUBDIR>";
            this.m_Panel.maxLen2[(int)side, this.m_Panel.GetActiveTabIndex(side)] = Math.Min(this.m_Panel.maxLen2[(int)side, this.m_Panel.GetActiveTabIndex(side)], text.Length);
            ListViewItem[] arr = this.m_Panel.GetListViewArr(side);
            for ( int i = 0; i < arr.Length; i++ ) {
                if ( (arr[i].ImageIndex == 3) || (arr[i].ImageIndex == 0) ) {
                    arr[i].SubItems[2].Text = text;
                    if ( arr[i].ListView != null ) {
                        arr[i].ListView.RedrawItems(i, i, false);
                    }
                }
            }
            this.listViewFitColumns(side);
        }
        void updateFolderSize(ListViewItem item, int itemNdx, string oriText, string path, Side side) {
            if ( !this.m_bRunSize ) {
                return;
            }
            // get the space consumption of a folder incl. its subfolders
            long size = 0;
            GrzTools.FastFileFind.FileSizes(ref this.m_bRunSize, path, ref size);
            try {
                // since the call comes from another thread, we need to Invoke
                this.Invoke(new Action(() => {
                    if ( item.Text == oriText ) {
                        // size text for folder item is now the real size, instead of <SUBDIR> 
                        string sizeStr = size.ToString("0,0", CultureInfo.InvariantCulture);
                        item.SubItems[2].Text = sizeStr;
                        if ( (item.ListView != null) && (itemNdx < item.ListView.Items.Count) ) {
                            item.ListView.RedrawItems(itemNdx, itemNdx, false);
                            Application.DoEvents();
                        }
                        // if needed, we resize the column width
                        if ( sizeStr.Length > this.m_Panel.maxLen2[(int)side, this.m_Panel.GetActiveTabIndex(side)] ) {
                            this.m_Panel.maxLen2[(int)side, this.m_Panel.GetActiveTabIndex(side)] = sizeStr.Length;
                            this.listViewFitColumns(side);
                            Application.DoEvents();
                        }
                    } else {
                        // if the texts don't match, we very likely switched to another listview
                        this.m_bRunSize = false;
                    }
                }));
            } catch {; }
        }
        private void timerRunSize_Tick(object sender, EventArgs e) {
            if ( this.m_Panel == null ) {
                return;
            }
            bool completed = true;
            for ( int i = 0; i < this.m_lstTasksRunSize.Count; i++ ) {
                if ( this.m_lstTasksRunSize[i] != null ) {
                    if ( !this.m_lstTasksRunSize[i].IsCompleted ) {
                        completed = false;
                        break;
                    }
                } else {
                    this.m_lstTasksRunSize.RemoveAt(i);
                }
            }
            if ( completed ) {
                this.m_lstTasksRunSize.Clear();
                this.timerRunSize.Stop();
                Side side = this.m_Panel.GetActiveSide();
                this.SortListView(side, side == Side.left ? this.m_Panel.LastSortedColumnLhs[this.m_Panel.GetActiveTabIndex(side)] : this.m_Panel.LastSortedColumnRhs[this.m_Panel.GetActiveTabIndex(side)], true);
            }
        }
        void updateFolderSizes(Side side) {
            this.m_bRunSize = true;
            this.m_lstTasksRunSize = new List<Task>();
            ListViewItem[] arr = this.m_Panel.GetListViewArr(side);
            Parallel.For(1, arr.Length, i => {
                if ( (arr[i].ImageIndex == 3) || (arr[i].ImageIndex == 0) ) {
                    string path = Path.Combine(this.m_Panel.button(side).Tag.ToString(), arr[i].Text);
                    int ndx = i;                   // strange: I cannot forward i
                    ListViewItem item = arr[i];    //   -"-    I cannot froward arr[i]
                    string oriText = arr[i].Text;  //   -"-    I cannot froward arr[i].Text
                    Task t = new Task(() => this.updateFolderSize(item, ndx, oriText, path, side));
                    t.Start();
                    this.m_lstTasksRunSize.Add(t);
                }
            });
            // we need to resort if current sort order is "by size == 2": then we start a timer, which checks for completion status of stored tasks
            if ( ((side == Side.left) ? this.m_Panel.LastSortedColumnLhs[this.m_Panel.GetActiveTabIndex(side)] : this.m_Panel.LastSortedColumnRhs[this.m_Panel.GetActiveTabIndex(side)]) == 2 ) {
                this.Invoke(new Action(() => {
                    this.timerRunSize.Start();
                }));
            }
        }
        private void listsShowFolderSizesToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.listsShowFolderSizesToolStripMenuItem.Checked ) {
                this.m_bRunSize = true;
                if ( this.m_Panel.button(Side.left).Tag.ToString() != "Computer" ) {
                    new Task(() => this.updateFolderSizes(Side.left)).Start();
                    //updateFolderSizes(Side.left);
                }
                if ( this.m_Panel.button(Side.right).Tag.ToString() != "Computer" ) {
                    new Task(() => this.updateFolderSizes(Side.right)).Start();
                    //updateFolderSizes(Side.right);
                }
            } else {
                this.m_bRunSize = false;
                if ( this.m_Panel.button(Side.left).Tag.ToString() != "Computer" ) {
                    this.removeFolderSizes(Side.left);
                }
                if ( this.m_Panel.button(Side.right).Tag.ToString() != "Computer" ) {
                    this.removeFolderSizes(Side.right);
                }
            }
        }

        // TURN ON double buffering - only applied to ListView - avoids flickering when ListView in virtual mode AND owner draw
        public static void SetDoubleBuffered(System.Windows.Forms.Control c) {
            //Taxes: Remote Desktop Connection and painting
            //http://blogs.msdn.com/oldnewthing/archive/2006/01/03/508694.aspx
            //            if ( System.Windows.Forms.SystemInformation.TerminalServerSession ) {
            //                return;
            //            }
            System.Reflection.PropertyInfo aProp = typeof(System.Windows.Forms.Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            aProp.SetValue(c, true, null);
        }

        // make new shortcut at location
        private void linkToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.m_Panel.GetActiveView().SelectedIndices.Count == 0 ) {
                return;
            }
            // WPD
            if ( this.m_Panel.listType[(int)this.m_Panel.GetActiveSide()] != ListType.FileSystem ) {
                MessageBox.Show("Not yet implemented for WPD.", "Sorry");
                return;
            }

            string currentFolder = this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString();
            SelectFolderOrFile sff = new SelectFolderOrFile();
            sff.Text = "Select Folder/File or type/paste into the text box below ...";
            sff.DefaultPath = currentFolder;
            if ( !System.IO.Directory.Exists(currentFolder) ) {
                sff.DefaultPath = Path.GetDirectoryName(Application.ExecutablePath);
            }
            string file = "";
            DialogResult dlr = sff.ShowDialog(this);
            if ( dlr == System.Windows.Forms.DialogResult.OK ) {
                file = sff.ReturnPath;
                GrzTools.FileTools.CreateShortcut(file, currentFolder);
            }
            sff.Dispose();
        }

        // 20161016
        private void listViewLeftRight_VirtualItemsSelectionRangeChanged(object sender, ListViewVirtualItemsSelectionRangeChangedEventArgs e) {
            ListView lv = (ListView)sender;
            Side side = this.m_Panel.GetSideFromView(lv);
            this.m_Panel.RenderListviewLabel(side);
        }

        // 20161016
        private void linkCmdexeHereToolStripMenuItem_Click(object sender, EventArgs e) {
            bool admin = this.linkCmdexeHereToolStripMenuItem.Text.Contains("admin") ? true : false;
            if ( (Control.MouseButtons & MouseButtons.Right) == MouseButtons.Right ) {
                admin = false;
            }
            string arg = String.Format("/K cd /d \"{0}\"", this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString());
            GrzTools.FileTools.CreateShortcut("cmd.exe", this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString(), admin, arg);
        }
        private void linkCmdexeHereToolStripMenuItem_MouseDown(object sender, MouseEventArgs e) {
            if ( (Control.MouseButtons & MouseButtons.Right) == MouseButtons.Right ) {
                this.linkCmdexeHereToolStripMenuItem_Click(sender, e);
            }
        }

        private void moreToolStripMenuItem_Click(object sender, EventArgs e) {
            // build a list of FileSystemInfo
            ListView lv = this.m_Panel.GetActiveView();
            List<FileSystemInfo> lfsi = new List<FileSystemInfo>();
            if ( lv.SelectedIndices.Count >= 1 ) {
                for ( int i = 0; i < lv.SelectedIndices.Count; i++ ) {
                    string file = this.m_Panel.GetListViewArr(this.m_Panel.GetActiveSide())[lv.SelectedIndices[i]].Text;
                    string path = this.m_Panel.button(this.m_Panel.GetActiveSide()).Tag.ToString();
                    string fullpath = Path.Combine(path, file);
                    lfsi.Add(new FileInfo(fullpath));
                }
            }
            // open windows' original context menu
            ShellContextMenu scm = new ShellContextMenu();
            Point pt = this.PointToClient(MousePosition);
            scm.Show(this, pt, lfsi.ToArray());
        }

        // changing TABS 
        private void tabControlLeft_SelectedIndexChanged(object sender, EventArgs e) {
            // get set active listview according to selected tab
            switch ( this.tabControlLeft.SelectedIndex ) {
                case 0:
                    this.m_listViewL = this.cfwListViewL0;
                    break;
                case 1:
                    this.m_listViewL = this.cfwListViewL1;
                    break;
                case 2:
                    this.m_listViewL = this.cfwListViewL2;
                    break;
                case 3:
                    this.m_listViewL = this.cfwListViewL3;
                    break;
                case 4:
                    this.m_listViewL = this.cfwListViewL4;
                    break;
            }
            this.m_Panel.SetActiveListView(Side.left, this.m_listViewL, this.tabControlLeft.SelectedIndex);
            // get path matching to seleted tab
            string path = this.m_Panel.GetListPath(Side.left, this.tabControlLeft.SelectedIndex);
            if ( this.m_Panel.IsRefreshListRequired(Side.left, this.tabControlLeft.SelectedIndex) ) {
                this.LoadListView(Side.left, path, "");
                this.m_Panel.folders.InsertTopFolder(Side.left, this.m_Panel.GetActiveTabIndex(Side.left), path);
            } else {
                // if listview is not refreshed, we need to do a couple of things manually, which were normally done by LoadListView(..) 
                this.m_Panel.SetButtonText(Side.left, path, this.m_sLHSfilter);
                this.m_Panel.button(Side.left).Tag = path;
                this.m_Panel.SetListPath(Side.left, this.tabControlLeft.SelectedIndex, path, false);
                this.setTabControlText(Side.left, this.tabControlLeft.SelectedIndex, path);
                this.RenderCommandline(this.m_Panel.button(Side.left).Tag.ToString());
                this.m_Panel.RenderListviewLabel(Side.left);
                this.m_Panel.SetFileSystemWatcher(Side.left, path, this.filesystemMonitoringToolStripMenuItem.Checked);
                if ( this.m_Panel.button(Side.left).Text == "Computer" ) {
                    this.timerRefeshComputerView.Interval = 1;
                    this.timerRefeshComputerView.Start();
                }
            }
        }
        private void tabControlRight_SelectedIndexChanged(object sender, EventArgs e) {
            switch ( this.tabControlRight.SelectedIndex ) {
                case 0:
                    this.m_listViewR = this.cfwListViewR0;
                    break;
                case 1:
                    this.m_listViewR = this.cfwListViewR1;
                    break;
                case 2:
                    this.m_listViewR = this.cfwListViewR2;
                    break;
                case 3:
                    this.m_listViewR = this.cfwListViewR3;
                    break;
                case 4:
                    this.m_listViewR = this.cfwListViewR4;
                    break;
            }
            this.m_Panel.SetActiveListView(Side.right, this.m_listViewR, this.tabControlRight.SelectedIndex);
            string path = this.m_Panel.GetListPath(Side.right, this.tabControlRight.SelectedIndex);
            bool refreshListView = this.m_Panel.IsRefreshListRequired(Side.right, this.tabControlRight.SelectedIndex);
            if ( refreshListView ) {
                this.LoadListView(Side.right, path, "");
                this.m_Panel.folders.InsertTopFolder(Side.right, this.m_Panel.GetActiveTabIndex(Side.right), path);
            } else {
                this.m_Panel.SetButtonText(Side.right, path, this.m_sLHSfilter);
                this.m_Panel.button(Side.right).Tag = path;
                this.m_Panel.SetListPath(Side.right, this.tabControlRight.SelectedIndex, path, false);
                this.setTabControlText(Side.right, this.tabControlRight.SelectedIndex, path);
                this.RenderCommandline(this.m_Panel.button(Side.right).Tag.ToString());
                this.m_Panel.RenderListviewLabel(Side.right);
                this.m_Panel.SetFileSystemWatcher(Side.right, path, this.filesystemMonitoringToolStripMenuItem.Checked);
                if ( this.m_Panel.button(Side.right).Text == "Computer" ) {
                    this.timerRefeshComputerView.Interval = 1;
                    this.timerRefeshComputerView.Start();
                }
            }
        }

        private void resetAllListsToolStripMenuItem_Click(object sender, EventArgs e) {
            // in case of "no tabs": 
            //    the currently shown listviewX needs to go back to its original place in the tabcontrol (the tab where it originally came from when switching to "no tabs")
            //    and listview0 goes to the panel
            if ( !this.listsInTabsToolStripMenuItem.Checked ) {
                int tabNdx = this.m_Panel.GetActiveTabIndex(Side.right);
                this.m_listViewR.BorderStyle = BorderStyle.None;
                this.tableLayoutPanelRight.Controls.Remove(this.m_listViewR);
                this.tabControlRight.TabPages[tabNdx].Controls.Add(this.m_listViewR);
                this.cfwListViewR0.BorderStyle = BorderStyle.FixedSingle;
                this.tableLayoutPanelRight.Controls.Add(this.cfwListViewR0);
                tabNdx = this.m_Panel.GetActiveTabIndex(Side.left);
                this.m_listViewL.BorderStyle = BorderStyle.None;
                this.tableLayoutPanelLeft.Controls.Remove(this.m_listViewL);
                this.tabControlLeft.TabPages[tabNdx].Controls.Add(this.m_listViewL);
                this.cfwListViewL0.BorderStyle = BorderStyle.FixedSingle;
                this.tableLayoutPanelLeft.Controls.Add(this.cfwListViewL0);
            }

            // cleanup backwards to finish at index 0 
            for ( int i = 4; i >= 0; i-- ) {
                this.clearTab(Side.left, i);
                this.clearTab(Side.right, i);
            }

            // load listviews with default values
            this.m_Panel.SetActiveListView(Side.right, this.m_listViewR, 0);
            this.LoadListView(Side.right, this.DRVC, "");
            this.m_Panel.SetActiveListView(Side.left, this.m_listViewL, 0);
            this.LoadListView(Side.left, this.DRVC, "");
        }

        // a class specifically used while dragging (better copying) a tab to another position
        class mvTab {
            public bool active = false;
            public Side side = Side.none;
            public int tabIndex = -1;

            public mvTab(bool active, Side side, int tabIndex) {
                this.active = active;
                this.side = side;
                this.tabIndex = tabIndex;
            }
        }
        mvTab m_mvTab;
        private void tabControlDownLR(object sender, MouseEventArgs e) {
            // right mouse button down shall just select a tab
            if ( e.Button == MouseButtons.Right ) {
                Point ptms = MousePosition;
                Side side = Side.left;
                if ( (TabControl)sender == this.tabControlLeft ) {
                    for ( int i = 0; i < this.tabControlLeft.TabCount; i++ ) {
                        Rectangle rc = this.tabControlLeft.GetTabRect(i);
                        Point pts = this.tabControlLeft.PointToScreen(new Point(rc.X, rc.Y));
                        Rectangle rcs = new Rectangle(pts.X, pts.Y, rc.Width, rc.Height);
                        if ( rcs.Contains(ptms.X, ptms.Y) ) {
                            this.tabControlLeft.SelectedIndex = i;
                        }
                    }
                }
                if ( (TabControl)sender == this.tabControlRight ) {
                    side = Side.right;
                    for ( int i = 0; i < this.tabControlRight.TabCount; i++ ) {
                        Rectangle rc = this.tabControlRight.GetTabRect(i);
                        Point pts = this.tabControlRight.PointToScreen(new Point(rc.X, rc.Y));
                        Rectangle rcs = new Rectangle(pts.X, pts.Y, rc.Width, rc.Height);
                        if ( rcs.Contains(ptms.X, ptms.Y) ) {
                            this.tabControlRight.SelectedIndex = i;
                        }
                    }
                }
                this.m_Panel.SetActiveSide(side);
            }
            // left mouse button down could be the beginning of a tab dragging (better copy) process
            if ( e.Button == MouseButtons.Left ) {
                Point ptms = MousePosition;
                Side side = Side.left;
                if ( (TabControl)sender == this.tabControlLeft ) {
                    for ( int i = 0; i < this.tabControlLeft.TabCount; i++ ) {
                        Rectangle rc = this.tabControlLeft.GetTabRect(i);
                        Point pts = this.tabControlLeft.PointToScreen(new Point(rc.X, rc.Y));
                        Rectangle rcs = new Rectangle(pts.X, pts.Y, rc.Width, rc.Height);
                        if ( rcs.Contains(ptms.X, ptms.Y) ) {
                            this.tabControlLeft.SelectedIndex = i;
                            this.m_mvTab = new mvTab(true, Side.left, i);
                            this.tabControlLeft.Cursor = Cursors.Hand;
                            break;
                        }
                    }
                }
                if ( (TabControl)sender == this.tabControlRight ) {
                    side = Side.right;
                    for ( int i = 0; i < this.tabControlRight.TabCount; i++ ) {
                        Rectangle rc = this.tabControlRight.GetTabRect(i);
                        Point pts = this.tabControlRight.PointToScreen(new Point(rc.X, rc.Y));
                        Rectangle rcs = new Rectangle(pts.X, pts.Y, rc.Width, rc.Height);
                        if ( rcs.Contains(ptms.X, ptms.Y) ) {
                            this.tabControlRight.SelectedIndex = i;
                            this.m_mvTab = new mvTab(true, Side.right, i);
                            this.tabControlRight.Cursor = Cursors.Hand;
                            break;
                        }
                    }
                }
                this.m_Panel.SetActiveSide(side);
            }
        }
        private void tabControlUpLR(object sender, MouseEventArgs e) {
            // left mouse button up will finish a tab move process: sender is always the control, which started the move process
            if ( e.Button == MouseButtons.Left ) {
                // we need to determine the correct side from the mouseposition, where the mouseup did happen 
                Point ptms = MousePosition;
                bool foundSide = false;
                int tabIndex = -1;
                Side side = Side.left;
                for ( int i = 0; i < this.tabControlLeft.TabCount; i++ ) {
                    Rectangle rc = this.tabControlLeft.GetTabRect(i);
                    Point pts = this.tabControlLeft.PointToScreen(new Point(rc.X, rc.Y));
                    Rectangle rcs = new Rectangle(pts.X, pts.Y, rc.Width, rc.Height);
                    if ( rcs.Contains(ptms.X, ptms.Y) ) {
                        tabIndex = i;
                        foundSide = true;
                        break;
                    }
                }
                if ( !foundSide ) {
                    side = Side.right;
                    for ( int i = 0; i < this.tabControlRight.TabCount; i++ ) {
                        Rectangle rc = this.tabControlRight.GetTabRect(i);
                        Point pts = this.tabControlRight.PointToScreen(new Point(rc.X, rc.Y));
                        Rectangle rcs = new Rectangle(pts.X, pts.Y, rc.Width, rc.Height);
                        if ( rcs.Contains(ptms.X, ptms.Y) ) {
                            tabIndex = i;
                            foundSide = true;
                            break;
                        }
                    }
                }
                // if we have found a side
                if ( foundSide ) {
                    // active move && (same side but different tab OR side change)
                    if ( this.m_mvTab.active && (((this.m_mvTab.side == side) && (this.m_mvTab.tabIndex != tabIndex)) || (this.m_mvTab.side != side)) ) {
                        // replace current data with m_mvTab data at side & tabIndex
                        string path = this.m_Panel.button(this.m_mvTab.side).Tag.ToString();
                        this.m_Panel.SetListPath(side, tabIndex, path, true);
                        if ( side == Side.left ) {
                            if ( this.tabControlLeft.SelectedIndex == tabIndex ) {
                                this.tabControlLeft_SelectedIndexChanged(null, null);
                            } else {
                                this.tabControlLeft.SelectedIndex = tabIndex;
                            }
                        }
                        if ( side == Side.right ) {
                            if ( this.tabControlRight.SelectedIndex == tabIndex ) {
                                this.tabControlRight_SelectedIndexChanged(null, null);
                            } else {
                                this.tabControlRight.SelectedIndex = tabIndex;
                            }
                        }
                    }
                }
                // unconditionally set active side
                this.m_Panel.SetActiveSide(side);
                this.tabControlLeft.Cursor = Cursors.Default;
                this.tabControlRight.Cursor = Cursors.Default;
            }
            // unconditionally reset tab move process
            if ( this.m_mvTab != null ) {
                this.m_mvTab.active = false;
            }
        }

        private void clearTabToolStripMenuItem_Click(object sender, EventArgs e) {
            ToolStripItem menuItem = sender as ToolStripItem;
            if ( menuItem != null ) {
                ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                if ( owner != null ) {
                    Control sourceControl = owner.SourceControl;
                    if ( sourceControl == this.tabControlRight ) {
                        int tabIndex = this.m_Panel.GetActiveTabIndex(Side.right);
                        if ( tabIndex == 0 ) {
                            return;
                        }
                        this.clearTab(Side.right, tabIndex);
                    }
                    if ( sourceControl == this.tabControlLeft ) {
                        int tabIndex = this.m_Panel.GetActiveTabIndex(Side.left);
                        if ( tabIndex == 0 ) {
                            return;
                        }
                        this.clearTab(Side.left, tabIndex);
                    }
                }
            }
        }
        private void clearTab(Side side, int tabIndex) {
            this.m_Panel.SetListViewArr(side, tabIndex, null);                                                         // empty virtual listview data array 
            this.m_Panel.SetListPath(side, tabIndex, "", true);                                                        // no connection to any path
            this.setTabControlText(side, tabIndex, "");                                                                // set no text on tab
            if ( side == Side.right ) {
                this.m_lvwColumnSorter[(int)side][this.m_Panel.LastSortedColumnRhs[tabIndex]].Order = SortOrder.Ascending;  // set default sort order
                this.m_Panel.LastSortedColumnRhs[tabIndex] = 0;                                                        // set default sort column 
                this.tabControlRight.SelectedIndex = 0;                                                                // finally switch to tab with index 0
            }
            if ( side == Side.left ) {
                this.m_lvwColumnSorter[(int)side][this.m_Panel.LastSortedColumnRhs[tabIndex]].Order = SortOrder.Ascending;
                this.m_Panel.LastSortedColumnLhs[tabIndex] = 0;
                this.tabControlLeft.SelectedIndex = 0;
            }
        }

        private void listsInTabsToolStripMenuItem_Click(object sender, EventArgs e) {
            this.showListsInTabs(this.listsInTabsToolStripMenuItem.Checked);
        }
        private void showListsInTabs(bool bShowTabs) {
            // make sure to have the correct listview assigned: it is especially important, when switching from "no tabs" to "tabs"  
            switch ( this.tabControlRight.SelectedIndex ) {
                case 0:
                    this.m_listViewR = this.cfwListViewR0;
                    break;
                case 1:
                    this.m_listViewR = this.cfwListViewR1;
                    break;
                case 2:
                    this.m_listViewR = this.cfwListViewR2;
                    break;
                case 3:
                    this.m_listViewR = this.cfwListViewR3;
                    break;
                case 4:
                    this.m_listViewR = this.cfwListViewR4;
                    break;
            }
            switch ( this.tabControlLeft.SelectedIndex ) {
                case 0:
                    this.m_listViewL = this.cfwListViewL0;
                    break;
                case 1:
                    this.m_listViewL = this.cfwListViewL1;
                    break;
                case 2:
                    this.m_listViewL = this.cfwListViewL2;
                    break;
                case 3:
                    this.m_listViewL = this.cfwListViewL3;
                    break;
                case 4:
                    this.m_listViewL = this.cfwListViewL4;
                    break;
            }

            // make sure, we have the same listview selection, as we had before we came here: memorize the current listview selections, they get lost when adding/removing a listview
            int[] selL = new int[this.m_listViewL.SelectedIndices.Count];
            this.m_listViewL.SelectedIndices.CopyTo(selL, 0);
            int[] selR = new int[this.m_listViewR.SelectedIndices.Count];
            this.m_listViewR.SelectedIndices.CopyTo(selR, 0);

            // 'tabs' or 'no tabs' for listviews
            if ( bShowTabs ) {
                this.m_listViewR.BorderStyle = BorderStyle.None;
                this.tableLayoutPanelRight.Controls.Remove(this.m_listViewR);
                this.tableLayoutPanelRight.Controls.Add(this.tabControlRight);
                this.tabControlRight.TabPages[this.tabControlRight.SelectedIndex].Controls.Add(this.m_listViewR);

                this.m_listViewL.BorderStyle = BorderStyle.None;
                this.tableLayoutPanelLeft.Controls.Remove(this.m_listViewL);
                this.tableLayoutPanelLeft.Controls.Add(this.tabControlLeft);
                this.tabControlLeft.TabPages[this.tabControlLeft.SelectedIndex].Controls.Add(this.m_listViewL);
            } else {
                this.m_listViewR.BorderStyle = BorderStyle.FixedSingle;
                this.tableLayoutPanelRight.Controls.Remove(this.tabControlRight);
                this.tableLayoutPanelRight.Controls.Add(this.m_listViewR);

                this.m_listViewL.BorderStyle = BorderStyle.FixedSingle;
                this.tableLayoutPanelLeft.Controls.Remove(this.tabControlLeft);
                this.tableLayoutPanelLeft.Controls.Add(this.m_listViewL);
            }

            // follow a certain sequence 
            if ( this.m_Panel.GetActiveSide() == Side.left ) {
                this.tabControlRight_SelectedIndexChanged(null, null);
                this.tabControlLeft_SelectedIndexChanged(null, null);
            } else {
                this.tabControlLeft_SelectedIndexChanged(null, null);
                this.tabControlRight_SelectedIndexChanged(null, null);
            }

            // make sure, we have the same listview selection, as we had before we came here
            if ( this.m_listViewL.SelectedIndices.Count == 0 ) {
                foreach ( int i in selL ) {
                    if ( i < this.m_listViewL.Items.Count ) {
                        this.m_listViewL.SelectedIndices.Add(i);
                    }
                }
            }
            if ( this.m_listViewR.SelectedIndices.Count == 0 ) {
                foreach ( int i in selR ) {
                    if ( i < this.m_listViewR.Items.Count ) {
                        this.m_listViewR.SelectedIndices.Add(i);
                    }
                }
            }
        }

        private void cloneTabToolStripMenuItem_Click(object sender, EventArgs e) {
            ToolStripItem menuItem = sender as ToolStripItem;
            if ( menuItem == null ) {
                return;
            }
            ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
            if ( owner == null ) {
                return;
            }
            Control sourceControl = owner.SourceControl;
            if ( sourceControl == this.tabControlRight ) {
                int tabIndex = this.m_Panel.GetActiveTabIndex(Side.right);
                int newTabIndex = tabIndex + 1 > 4 ? 0 : tabIndex + 1;
                this.m_Panel.SetListViewArr(Side.right, newTabIndex, this.m_Panel.GetListViewArr(Side.right));
                this.m_Panel.SetListPath(Side.right, newTabIndex, this.m_Panel.GetListPath(Side.right, tabIndex), true);
                this.setTabControlText(Side.right, newTabIndex, this.m_Panel.GetListPath(Side.right, tabIndex));
                this.tabControlRight.SelectedIndex = newTabIndex;
            }
            if ( sourceControl == this.tabControlLeft ) {
                int tabIndex = this.m_Panel.GetActiveTabIndex(Side.left);
                int newTabIndex = tabIndex + 1 > 4 ? 0 : tabIndex + 1;
                this.m_Panel.SetListViewArr(Side.left, newTabIndex, this.m_Panel.GetListViewArr(Side.left));
                this.m_Panel.SetListPath(Side.left, newTabIndex, this.m_Panel.GetListPath(Side.left, tabIndex), true);
                this.setTabControlText(Side.left, newTabIndex, this.m_Panel.GetListPath(Side.left, tabIndex));
                this.tabControlLeft.SelectedIndex = newTabIndex;
            }
        }

        // have an item, which shows folder history from context menu
        private void folderHistoryToolStripMenuItem1_Click(object sender, EventArgs e) {
            Side side = this.m_Panel.GetActiveSide();
            this.ShowFolderList(side);
        }

        // cpu temperature
        private void cPUTemperatureMonitoringToolStripMenuItem_Click(object sender, EventArgs e) {
            GrzTools.IniFile ini = new GrzTools.IniFile(System.Windows.Forms.Application.ExecutablePath + ".ini");
            ini.IniWriteValue("cfw", "temperatureCPU", this.cPUTemperatureMonitoringToolStripMenuItem.Checked.ToString());
            if ( this.cPUTemperatureMonitoringToolStripMenuItem.Checked ) {
                if ( new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) ) {
                    this.m_bRunTemperatureCPU = true;
                    this.m_cpuTemperature = new Task(() => this.cpuTemperature(this.buttonF12, ref this.m_bRunTemperatureCPU));
                    this.m_cpuTemperature.Start();
                } else {
                    MessageBox.Show("CPU temperature monitoring won't work, unless you restart CfW as Administrator.", "Note");
                }
            } else {
                this.m_bRunTemperatureCPU = false;
            }
        }
        public void cpuTemperature(Control ctl, ref bool m_bRunTemperatureCPU) {
            ctl.Tag = ctl.Text;
            UpdateVisitor updateVisitor = new UpdateVisitor();
            Computer computer = new Computer();
            computer.Open();
            computer.CPUEnabled = true;
            do {
                computer.Accept(updateVisitor);
                double maxCpuTemp = 0;
                try {
                    for ( int i = 0; i < computer.Hardware.Length; i++ ) {
                        if ( computer.Hardware[i].HardwareType == HardwareType.CPU ) {
                            for ( int j = 0; j < computer.Hardware[i].Sensors.Length; j++ ) {
                                if ( computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature ) {
                                    maxCpuTemp = Math.Max((double)computer.Hardware[i].Sensors[j].Value, maxCpuTemp);
                                }
                            }
                        }
                    }
                } catch {; }
                this.Invoke(new Action(() => {
                    ctl.Text = String.Format("cpu {0} °C", maxCpuTemp);
                }));
                Thread.Sleep(2000);
            } while ( m_bRunTemperatureCPU );
            computer.Close();
            this.Invoke(new Action(() => {
                ctl.Text = ctl.Tag.ToString();
            }));
        }
        private class UpdateVisitor : IVisitor {
            public void VisitComputer(IComputer computer) {
                computer.Traverse(this);
            }
            public void VisitHardware(IHardware hardware) {
                hardware.Update();
                foreach ( IHardware subHardware in hardware.SubHardware )
                    subHardware.Accept(this);
            }
            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }

        // permanent start as admin or as user
        private void runCfWAlwaysAsAdministratorToolStripMenuItem_Click(object sender, EventArgs e) {
            GrzTools.IniFile ini = new GrzTools.IniFile(System.Windows.Forms.Application.ExecutablePath + ".ini");
            ini.IniWriteValue("cfw", "adminmode", this.runCfWAlwaysAsAdministratorToolStripMenuItem.Checked.ToString());

            if ( this.runCfWAlwaysAsAdministratorToolStripMenuItem.Checked ) {
                if ( !new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) ) {
                    ProcessStartInfo startInfo = new ProcessStartInfo(Application.ExecutablePath);
                    startInfo.Verb = "runas";
                    try {
                        System.Diagnostics.Process.Start(startInfo);
                        this.Close();
                    } catch ( Exception ) {
                        this.runCfWAlwaysAsAdministratorToolStripMenuItem.Checked = false;
                        ini.IniWriteValue("cfw", "adminmode", this.runCfWAlwaysAsAdministratorToolStripMenuItem.Checked.ToString());
                        MessageBox.Show("Could not start 'admin mode'.", "Note");
                    }
                } else {
                    MessageBox.Show("Already running 'admin mode'.");
                }
            } else {
                if ( new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) ) {
                    try {
                        // a tricky way to revert the UAC elevation by injecting the app into explorer.exe
                        ProcessStartInfo startInfo = new ProcessStartInfo("explorer.exe");
                        startInfo.Arguments = Application.ExecutablePath;
                        System.Diagnostics.Process.Start(startInfo);
                        this.Close();
                    } catch ( Exception ) {
                        this.runCfWAlwaysAsAdministratorToolStripMenuItem.Checked = true;
                        ini.IniWriteValue("cfw", "adminmode", this.runCfWAlwaysAsAdministratorToolStripMenuItem.Checked.ToString());
                        MessageBox.Show("Could not start 'user mode'.", "Note");
                    }
                } else {
                    MessageBox.Show("Already running 'user mode'.");
                }
            }
        }

        // restore image filetime from EXIF meta data
        private void restoreToEXIFDataToolStripMenuItem_Click(object sender, EventArgs e) {
            // WPD
            if ( this.m_Panel.listType[(int)this.m_Panel.GetActiveSide()] != ListType.FileSystem ) {
                MessageBox.Show("Not implemented for WPD.", "Sorry");
                return;
            }

            Side side = this.m_Panel.GetActiveSide();
            ListView lv = this.m_Panel.GetActiveView();
            ListViewItem[] lvarr = this.m_Panel.GetListViewArr(side);
            if ( lv.SelectedIndices.Count < 1 ) {
                MessageBox.Show("Too few selection of images", "Note");
                return;
            }
            string folder = this.m_Panel.button(side).Tag.ToString();

            // init progress dlg
            SimpleProgress sp = new SimpleProgress();
            sp.StartPosition = FormStartPosition.Manual;
            sp.Location = new Point(this.Location.X + (this.Width - sp.Width) / 2, this.Location.Y + 100);
            sp.Text = "Progress";
            sp.LabelPercent = "0%";
            sp.ProgressValue = 0;
            sp.Show(this);
            int cnt = 1;
            int max = lv.SelectedIndices.Count;
            double pct = 100.0f / Math.Max(1, max);
            double prg = 0;

            this.m_bFileSystemChangeActionLeft = false;
            this.m_bFileSystemChangeActionRight = false;

            Monitor.Enter(lv);
            foreach ( int ndx in lv.SelectedIndices ) {
                string file = lvarr[ndx].Text;
                string fullpath = Path.Combine(folder, file);
                if ( File.Exists(fullpath) && this.isImage(fullpath) ) {
                    DateTime dateTime = this.getMetaDateTime(fullpath);
                    if ( dateTime > DateTime.MinValue ) {
                        File.SetCreationTime(fullpath, dateTime);
                        File.SetLastWriteTime(fullpath, dateTime);
                        File.SetLastAccessTime(fullpath, dateTime);
                    }
                }
                sp.LabelPercent = ((int)(cnt * pct)).ToString() + "%";
                prg += pct;
                sp.ProgressValue = Math.Min((int)prg, 100);
                cnt++;
                Application.DoEvents();
            }
            Monitor.Exit(lv);

            sp.Close();
            this.m_bFileSystemChangeActionLeft = true;
            this.m_bFileSystemChangeActionRight = true;
            lv.SelectedIndices.Clear();
            this.LoadListView(side, this.m_Panel.button(side).Tag.ToString(), "");
        }
        DateTime getMetaDateTime(string imgFileName) {
            DateTime dateTime = new DateTime();
            IList<MetadataExtractor.Directory> directories = (IList<MetadataExtractor.Directory>)MetadataExtractor.ImageMetadataReader.ReadMetadata(imgFileName);
            if ( directories == null ) {
                return dateTime;
            }

            // print out all metadata
            //foreach ( var directory in directories )
            //    foreach ( var tag in System.IO.Directory.Tags )
            //        Console.WriteLine($"{System.IO.Directory.Name} - {tag.Name} = {tag.Description}");

            // access the date time
            MetadataExtractor.Formats.Exif.ExifSubIfdDirectory subIfdDirectory = directories.OfType<MetadataExtractor.Formats.Exif.ExifSubIfdDirectory>().FirstOrDefault();
            if ( subIfdDirectory == null ) {
                return dateTime;
            }
            try {
                dateTime = subIfdDirectory.GetDateTime(MetadataExtractor.Formats.Exif.ExifDirectoryBase.TagDateTimeOriginal);
            } catch {
                try {
                    dateTime = subIfdDirectory.GetDateTime(MetadataExtractor.Formats.Exif.ExifDirectoryBase.TagDateTimeDigitized);
                } catch {
                    try {
                        dateTime = subIfdDirectory.GetDateTime(MetadataExtractor.Formats.Exif.ExifDirectoryBase.TagDateTime);
                    } catch {
                        ;
                    }
                }
            }
            return dateTime;
        }

        // cfw should be disconnected from any fs watch activity when minimized
        bool _appStateMinimized = false;
        bool _fsWatcherRightStateBeforeMinimize = false;
        bool _fsWatcherLeftStateBeforeMinimize = false;
        private void MainForm_Resize(object sender, EventArgs e) {
            if ( (this.WindowState == FormWindowState.Normal) && this._appStateMinimized ) {
                this._appStateMinimized = false;
                this.fileSystemWatcherRight.EnableRaisingEvents = this._fsWatcherRightStateBeforeMinimize;
                this.fileSystemWatcherLeft.EnableRaisingEvents = this._fsWatcherLeftStateBeforeMinimize;
            }
            if ( this.WindowState == FormWindowState.Minimized ) {
                this._appStateMinimized = true;
                this._fsWatcherRightStateBeforeMinimize = this.fileSystemWatcherRight.EnableRaisingEvents;
                this._fsWatcherLeftStateBeforeMinimize = this.fileSystemWatcherLeft.EnableRaisingEvents;
            }
        }

        private void MainForm_Deactivate(object sender, EventArgs e) {
            // it may happen, that a context menu is still open 
            if ( this.contextMenuStripButtons.Visible ) {
                this.contextMenuStripButtons.Close();
            }
        }

    }

    // we deal with two panels: left and right, each containing button, tabcontrol with listviews, label
    public enum ListType { Error, FileSystem, WPDsrc, WPDdst };
    public enum Side { left, right, both, none };
    public class Panel {
        private readonly MainForm.WPD[,] m_wpd = new MainForm.WPD[2, 5];                          // windows portable devices
        private readonly bool[,] m_bRefreshListRequired = new bool[2, 5];                         // refresh listview flag: there is no need to always reload a listview when switching tabs
        private readonly FileSystemWatcher[,] m_fswb = new FileSystemWatcher[2, 5];                // 2 sides with 5 fsw per side - they are kept in background, after a change event only the refresh listview flag is set
        private readonly FileSystemWatcher[] m_fsw = new FileSystemWatcher[2];                    // 1x per side - active listviews have an active fsw, for these guys the action shall take place immediately 
        private readonly int[] m_ActiveTabIndex = new int[2];                                     // 1x per side
        private readonly List<string>[] m_ListViewPaths = new List<string>[2];                    // 5x per side
        private Side m_ActiveSide = Side.left;
        private Side m_PassiveSide = Side.right;
        private ListView m_ActiveView;
        private readonly Button[] m_button = new Button[2];                                       // 1x per side 
        private readonly List<ListViewItem[]> m_ListViewItemsLhs = new List<ListViewItem[]>();    // 5x left
        private readonly List<ListViewItem[]> m_ListViewItemsRhs = new List<ListViewItem[]>();    // 5x right
        private readonly ListView[] m_listview = new ListView[2];                                 // 1x per side
        private readonly Label[] m_label = new Label[2];                                          // 1x per side
        private static readonly Button[] m_buttonPrev = new Button[2];
        private static readonly Button[] m_buttonNext = new Button[2];
        private readonly string[,] m_symlink = new string[2, 5];                                   // TBD 5x per side 

        public ListType[] listType = new ListType[] { ListType.Error, ListType.Error };
        public int[] LastSortedColumnLhs = new int[5];                                   // 5x per side
        public int[] LastSortedColumnRhs = new int[5];                                   // 5x per side      
        public int[,] maxLen2 = new int[2, 5];                                            // 5x per side 
        public int[,] maxLen3 = new int[2, 5];                                            // 5x per side

        private readonly Form m_parent;

        public Panel(Form parent) {
            this.m_parent = parent;
            this.m_ListViewPaths[0] = new List<string>();
            this.m_ListViewPaths[1] = new List<string>();
            ListViewItem[] dummy = new ListViewItem[0];
            for ( int i = 0; i < 5; i++ ) {
                // 10 portable devices
                this.m_wpd[0, i] = new MainForm.WPD(null, "", "");
                this.m_wpd[1, i] = new MainForm.WPD(null, "", "");
                // 10x pathes
                this.m_ListViewPaths[0].Add("");
                this.m_ListViewPaths[1].Add("");
                // 10x data storage for virtual listviews
                this.m_ListViewItemsLhs.Add(dummy);
                this.m_ListViewItemsRhs.Add(dummy);
                // 10x listview refresh flags
                this.m_bRefreshListRequired[0, i] = true;
                this.m_bRefreshListRequired[1, i] = true;
                // 10x background fsw
                this.m_fswb[0, i] = new FileSystemWatcher();
                this.m_fswb[0, i].EnableRaisingEvents = false;
                this.m_fswb[0, i].Changed += new System.IO.FileSystemEventHandler(this.fswb_Changed);
                this.m_fswb[0, i].Created += new System.IO.FileSystemEventHandler(this.fswb_Changed);
                this.m_fswb[0, i].Deleted += new System.IO.FileSystemEventHandler(this.fswb_Changed);
                this.m_fswb[0, i].Renamed += new System.IO.RenamedEventHandler(this.fswb_Changed);
                this.m_fswb[1, i] = new FileSystemWatcher();
                this.m_fswb[1, i].EnableRaisingEvents = false;
                this.m_fswb[1, i].Changed += new System.IO.FileSystemEventHandler(this.fswb_Changed);
                this.m_fswb[1, i].Created += new System.IO.FileSystemEventHandler(this.fswb_Changed);
                this.m_fswb[1, i].Deleted += new System.IO.FileSystemEventHandler(this.fswb_Changed);
                this.m_fswb[1, i].Renamed += new System.IO.RenamedEventHandler(this.fswb_Changed);
            }
        }

        private void fswb_Changed(object sender, FileSystemEventArgs e) {
            for ( int side = 0; side < 2; side++ ) {
                for ( int tab = 0; tab < 5; tab++ ) {
                    if ( sender == this.m_fswb[side, tab] ) {
                        this.m_bRefreshListRequired[side, tab] = true;
                    }
                }
            }
        }
        public bool IsRefreshListRequired(Side side, int tabIndex) {
            return this.m_bRefreshListRequired[(int)side, tabIndex];
        }

        public void InitPanel(ref Button bLeft,
                               ref Button bRight,
                               ref cfwListView lvLeft,
                               ref cfwListView lvRight,
                               ref Label lLeft,
                               ref Label lRight,
                               ref Button bLhsPrev,
                               ref Button bLhsNext,
                               ref Button bRhsPrev,
                               ref Button bRhsNext) {
            this.m_button[(int)Side.left] = bLeft;
            this.m_button[(int)Side.right] = bRight;
            this.m_listview[(int)Side.left] = lvLeft;
            this.m_listview[(int)Side.right] = lvRight;
            this.m_label[(int)Side.left] = lLeft;
            this.m_label[(int)Side.right] = lRight;
            m_buttonPrev[(int)Side.left] = bLhsPrev;
            m_buttonNext[(int)Side.left] = bLhsNext;
            m_buttonPrev[(int)Side.right] = bRhsPrev;
            m_buttonNext[(int)Side.right] = bRhsNext;
            this.folders = new FolderHistory();
        }
        public MainForm.WPD GetWPD(Side side, int tabIndex) {
            return this.m_wpd[(int)side, tabIndex];
        }
        public void SetWPD(Side side, int tabIndex, MainForm.WPD wpd) {
            this.m_wpd[(int)side, tabIndex] = wpd;
        }
        public string GetActivePath() {
            Side side = this.m_ActiveSide;
            int index = this.m_ActiveTabIndex[(int)side];
            return this.m_ListViewPaths[(int)side][index];
        }
        public int GetActiveTabIndex(Side side) {
            return this.m_ActiveTabIndex[(int)side];
        }
        public string GetListPath(Side side, int index) {
            return this.m_ListViewPaths[(int)side][index];
        }
        public void SetListPath(Side side, int tabIndex, string path, bool refreshListviewOnNextActivation) {
            //TBD GrzTools.FileTools.GetProperDirectoryCapitalization(di);
            // store "path" according to "side", "tabIndex" in a 2:5 array
            this.m_ListViewPaths[(int)side][tabIndex] = path;
            // activate a background fsw per path
            if ( System.IO.Directory.Exists(path) ) {
                try {
                    this.m_fswb[(int)side, tabIndex].Path = path;
                    this.m_fswb[(int)side, tabIndex].Filter = "*";
                    this.m_fswb[(int)side, tabIndex].EnableRaisingEvents = true;
                } catch {
                    this.m_fswb[(int)side, tabIndex].EnableRaisingEvents = false;
                }
            } else {
                this.m_fswb[(int)side, tabIndex].EnableRaisingEvents = false;
            }
            // the flag indicates, whether the listview needs to be reloaded NEXT time this tab is navigated to: needed after clearing a tab
            this.m_bRefreshListRequired[(int)side, tabIndex] = refreshListviewOnNextActivation;
        }
        // only one listview is active determined by: side, listview, tabindex
        public void SetActiveListView(Side side, ListView lv, int tabIndex) {
            this.m_listview[(int)side] = lv;
            this.m_ActiveTabIndex[(int)side] = tabIndex;
            this.SetActiveSide(side);
        }
        public FileSystemWatcher GetFileSystemWatcher(Side side) {
            return this.m_fsw[(int)side];
        }
        public void InitFileSystemWatcher(Side side, FileSystemWatcher fsw) {
            this.m_fsw[(int)side] = fsw;
        }
        public void SetFileSystemWatcher(Side side, string path, bool enable) {
            if ( this.m_fsw[(int)side] == null ) {
                return;
            }
            if ( System.IO.Directory.Exists(path) ) {
                this.m_fsw[(int)side].Path = path;
                this.m_fsw[(int)side].EnableRaisingEvents = enable;
                this.m_fswb[(int)side, this.GetActiveTabIndex(side)].EnableRaisingEvents = enable;
            } else {
                this.m_fsw[(int)side].EnableRaisingEvents = false;
                this.m_fswb[(int)side, this.GetActiveTabIndex(side)].EnableRaisingEvents = false;
            }
        }
        public Side GetSideFromView(ListView lv) {
            return lv == this.m_listview[(int)Side.left] ? Side.left : Side.right;
        }
        public ListViewItem[] GetListViewArr(Side side) {
            int tabIndex = this.m_ActiveTabIndex[(int)side];
            return side == Side.left ? this.m_ListViewItemsLhs[tabIndex] : this.m_ListViewItemsRhs[tabIndex];
        }
        public void SetListViewArr(Side side, int tabIndex, ListViewItem[] arr) {
            if ( arr == null ) {
                arr = new ListViewItem[0];
            }
            // set item's array
            if ( side == Side.left ) {
                this.m_ListViewItemsLhs[tabIndex] = arr;
            }
            if ( side == Side.right ) {
                this.m_ListViewItemsRhs[tabIndex] = arr;
            }
            // a virtual listview always needs to know the length of its items' array 
            this.m_listview[(int)side].VirtualListSize = arr.Length;
        }
        // I overlooked [..] being a legit filename: if an imageindex other than -1 is provided, then we check for it too. Searches for "[..]" will be accompanied with imageindex = 2  
        public ListViewItem FindListViewArrItemWithText(Side side, string text, int imgNdx) {
            ListViewItem lvi = null;
            ListViewItem[] lviarr = this.GetListViewArr(side);
            for ( int ndx = 0; ndx < lviarr.Length; ndx++ ) {
                if ( text == lviarr[ndx].Text ) {
                    if ( imgNdx != -1 ) {
                        if ( lviarr[ndx].ImageIndex == imgNdx ) {
                            lvi = lviarr[ndx];
                            lvi.Tag = ndx;
                            break;
                        }
                    } else {
                        lvi = lviarr[ndx];
                        lvi.Tag = ndx;
                        break;
                    }
                }
            }
            return lvi;
        }
        public ListViewItem ListViewArrItemStartsWithText(Side side, string text, int imgNdx) {
            ListViewItem lvi = null;
            ListViewItem[] lviarr = this.GetListViewArr(side);

            for ( int ndx = 0; ndx < lviarr.Length; ndx++ ) {
                if ( lviarr[ndx].Text.StartsWith(text) ) {
                    if ( imgNdx != -1 ) {
                        if ( lviarr[ndx].ImageIndex == imgNdx ) {
                            lvi = lviarr[ndx];
                            lvi.Tag = ndx;
                            break;
                        }
                    } else {
                        lvi = lviarr[ndx];
                        lvi.Tag = ndx;
                        break;
                    }
                }
            }
            return lvi;
        }// 20160417: auto cmd completion via TAB
        public ListViewItem FindListViewArrItemWithTextPartial(Side side, string text) {
            ListViewItem lvi = null;
            ListViewItem[] lviarr = this.GetListViewArr(side);
            for ( int ndx = 0; ndx < lviarr.Length; ndx++ ) {
                string fullText = lviarr[ndx].Text;
                if ( fullText.StartsWith(text, StringComparison.InvariantCultureIgnoreCase) ) {
                    lvi = lviarr[ndx];
                    lvi.Tag = ndx;
                    break;
                }
            }
            return lvi;
        }
        public string GetSymLink(Side side, int tabIndex) {
            return this.m_symlink[(int)side, tabIndex];
        }
        public void SetSymLink(Side side, int tabIndex, string value) {
            this.m_symlink[(int)side, tabIndex] = value;
        }
        public Button button(Side side) {
            return this.m_button[(int)side];
        }
        public void SetButtonText(Side side, string folder, string filter) {
            string toshow = folder;

            if ( filter != "*.*" ) {
                toshow = "[" + filter + "]   " + toshow;
                this.m_button[(int)side].ForeColor = Color.Red;
            } else {
                this.m_button[(int)side].ForeColor = Color.Black;
            }

            Font font = this.m_button[(int)side].Font;
            Size textSize = TextRenderer.MeasureText(toshow, font);
            int cutter = 3;
            //int possibleLength = m_button[(int)side].Width - 40;
            int possibleLength = (this.m_parent.Width - 180) / 2 - 40;
            if ( possibleLength <= 0 ) {
                return;
            }
            while ( textSize.Width > possibleLength ) {
                int len = Math.Max(toshow.Length / 2 - cutter, 5);
                int ndx = Math.Min(toshow.Length / 2 + cutter, toshow.Length - 5);
                toshow = toshow.Substring(0, len) + " ... " + toshow.Substring(ndx);
                cutter++;
                textSize = TextRenderer.MeasureText(toshow, font);
            }
            this.m_button[(int)side].Text = toshow;
        }
        public ListView listview(Side side) {
            return this.m_listview[(int)side];
        }
        public Label label(Side side) {
            return this.m_label[(int)side];
        }
        public Side GetActiveSide() {
            return this.m_ActiveSide;
        }
        public Side GetPassiveSide() {
            return this.m_PassiveSide;
        }
        public ListView GetActiveView() {
            return this.m_ActiveView;
        }
        public void SetActiveSide(Side side) {
            this.m_ActiveSide = side;
            if ( side == Side.left ) {
                if ( File.Exists(this.m_button[(int)Side.left].Text) ) {
                    this.m_button[(int)Side.left].BackColor = Color.LightPink;
                } else {
                    this.m_button[(int)Side.left].BackColor = Color.AliceBlue;
                }
                this.m_button[(int)Side.right].BackColor = SystemColors.Control;
                this.m_ActiveView = this.m_listview[(int)Side.left];
                this.m_PassiveSide = Side.right;
            }
            if ( side == Side.right ) {
                this.m_button[(int)Side.left].BackColor = SystemColors.Control;
                if ( File.Exists(this.m_button[(int)Side.right].Text) ) {
                    this.m_button[(int)Side.right].BackColor = Color.LightPink;
                } else {
                    this.m_button[(int)Side.right].BackColor = Color.AliceBlue;
                }
                this.m_ActiveView = this.m_listview[(int)Side.right];
                this.m_PassiveSide = Side.left;
            }
            this.m_listview[(int)Side.left].Invalidate();
            this.m_listview[(int)Side.right].Invalidate();
        }
        // populate text label underneath the list views
        public void RenderListviewLabel(Side side) {
            // in case of no selection
            if ( this.m_listview[(int)side].SelectedIndices.Count == 0 ) {
                if ( this.m_listview[(int)side].Items.Count == 0 ) {
                    this.m_label[(int)side].Text = "- empty -";
                }
                return;
            }

            string sNameOfFiles = " files";

            // special case, if we are at "Computer" level
            if ( side == this.GetActiveSide() ) {
                if ( this.button(side).Text == "Computer" ) {
                    ListViewItem[] arr = this.GetListViewArr(side);
                    if ( arr == null ) {
                        return;
                    }
                    sNameOfFiles = " drives";
                }
            }

            bool specFolder = ((this.button(side).Text[1] != ':') && (this.button(side).Text[1] != '\\'));
            if ( (this.m_listview[(int)side].SelectedIndices.Count == 1) || specFolder ) {
                // show either single item text (shortened if needed)
                ListViewItem[] arr = this.GetListViewArr(side);
                if ( (arr == null) || (arr.Length == 0) ) {
                    this.m_label[(int)side].Text = "?";
                    return;
                }
                string text = arr[this.m_listview[(int)side].SelectedIndices[0]].Text;
                Font font = this.m_label[(int)side].Font;
                Size textSize = TextRenderer.MeasureText(text, font);
                int cutter = 0;
                while ( textSize.Width >= this.m_label[(int)side].Width ) {
                    text = arr[this.m_listview[(int)side].SelectedIndices[0]].Text;
                    text = text.Substring(0, text.Length / 2 - cutter) + " ... " + text.Substring(text.Length / 2 + cutter);
                    cutter++;
                    textSize = TextRenderer.MeasureText(text, font);
                }
                this.m_label[(int)side].Text = text;
            } else {
                // show 'multiple selection' properties, aka number selected of files and folders
                ListView lv = this.m_listview[(int)side];
                int ficnt = 0;
                int focnt = 0;
                Int64 fiSizes = 0;
                Int64 foSizes = 0;
                foreach ( int ndx in lv.SelectedIndices ) {
                    // current item
                    ListViewItem lvi = lv.Items[ndx];
                    // just counts
                    if ( (lvi.ImageIndex == 3) || (lvi.ImageIndex == 0) ) {
                        // count
                        focnt++;
                        // add sizes
                        try {
                            long one = 0;
                            long.TryParse(lvi.SubItems[2].Text, NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out one);
                            foSizes += one;
                        } catch ( Exception ) {; }
                    } else {
                        // count
                        ficnt++;
                        // add sizes
                        try {
                            long one = 0;
                            long.TryParse(lvi.SubItems[2].Text, NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out one);
                            fiSizes += one;
                        } catch ( Exception ) {; }
                    }
                }
                string separator = "+ ";
                if ( foSizes == 0 ) {
                    separator = "/ ";
                }
                this.m_label[(int)side].Text = focnt.ToString() + " folders " + separator + ficnt.ToString() + sNameOfFiles + " --> " + GrzTools.StringTools.SizeSuffix(fiSizes + foSizes);
            }
        }

        // https://stackoverflow.com/questions/6115721/how-to-save-restore-serializable-object-to-from-file
        // Write the contents of the variable someClass to a file.
        // WriteToBinaryFile<SomeClass>("C:\someClass.txt", object1);
        // Read the file contents back into a variable.
        // SomeClass object1= ReadFromBinaryFile<SomeClass>("C:\someClass.txt");
        public static void WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false) {
            using ( Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create) ) {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
            }
        }
        public static T ReadFromBinaryFile<T>(string filePath) {
            using ( Stream stream = File.Open(filePath, FileMode.Open) ) {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (T)binaryFormatter.Deserialize(stream);
            }
        }

        public FolderHistory folders = new FolderHistory();
        [Serializable]
        public class FolderHistory {
            private readonly int[,] iCurrentItem = new int[2, 5];
            private readonly List<string>[,] list = new List<string>[2, 5];
            public bool MaintainFolderHistory { get; set; }

            public FolderHistory() {
                for ( int side = 0; side < 2; side++ ) {
                    for ( int tab = 0; tab < 5; tab++ ) {
                        this.list[side, tab] = new List<string>();
                    }
                }
            }
            public void ClearFolderHistory(Side side) {
                for ( int i = 0; i < 5; i++ ) {
                    this.list[(int)side, i].Clear();
                }
            }
            public void InsertTopFolder(Side side, int tabIndex, string folder) {
                // keep folder history yes OR no
                if ( !this.MaintainFolderHistory ) {
                    return;
                }

                // don't add empty folder names
                if ( folder == null ) {
                    return;
                }
                if ( folder.Trim().Length == 0 ) {
                    return;
                }

                // translate special folders
                if ( folder == @"Computer\Desktop" ) {
                    folder = GrzTools.FileTools.TranslateSpecialFolderNames("Desktop");
                }
                if ( folder == @"Computer\Documents" ) {
                    folder = GrzTools.FileTools.TranslateSpecialFolderNames("Documents");
                }
                if ( folder == @"Computer\Downloads" ) {
                    folder = GrzTools.FileTools.TranslateSpecialFolderNames("Downloads");
                }

                // whenever a folder is inserted, that is our top index folder
                this.iCurrentItem[(int)side, tabIndex] = 0;
                // insert folder only if it is not on top of the list
                int count = this.list[(int)side, tabIndex].Count;
                if ( count > 0 ) {
                    if ( this.list[(int)side, tabIndex][0] != folder ) {
                        this.list[(int)side, tabIndex].Insert(0, folder);
                    }
                } else {
                    this.list[(int)side, tabIndex].Insert(0, folder);
                }
                // limit count of history folders 
                if ( count > 40 ) {
                    this.list[(int)side, tabIndex].RemoveAt(40);
                }
                // "back button" availibility
                if ( this.list[(int)side, tabIndex].Count > 0 ) {
                    m_buttonPrev[(int)side].Enabled = true;
                }
            }
            public void SetPrevNextButtonsAvailibility(Side side, int tabIndex) {
                if ( this.iCurrentItem[(int)side, tabIndex] > 0 ) {
                    m_buttonNext[(int)side].Enabled = true;
                } else {
                    m_buttonNext[(int)side].Enabled = false;
                }
                if ( this.iCurrentItem[(int)side, tabIndex] < this.list[(int)side, tabIndex].Count - 1 ) {
                    m_buttonPrev[(int)side].Enabled = true;
                } else {
                    m_buttonPrev[(int)side].Enabled = false;
                }
            }
            public void SetCurrentIndex(Side side, int tabIndex, int listIndex) {
                this.iCurrentItem[(int)side, tabIndex] = Math.Min(Math.Max(listIndex, 0), this.list[(int)side, tabIndex].Count - 1);
                this.SetPrevNextButtonsAvailibility(side, tabIndex);
            }
            public int GetCurrentIndex(Side side, int tabIndex) {
                return this.iCurrentItem[(int)side, tabIndex];
            }
            public void DeleteFolderByIndexFromList(Side side, int tabIndex, int listIndex) {
                this.list[(int)side, tabIndex].RemoveAt(listIndex);
            }
            public string GetPreviousFolder(Side side, int tabIndex) {
                if ( this.list[(int)side, tabIndex].Count == 0 ) {
                    return "";
                }
                if ( this.iCurrentItem[(int)side, tabIndex] < this.list[(int)side, tabIndex].Count - 1 ) {
                    this.iCurrentItem[(int)side, tabIndex]++;
                }
                this.SetPrevNextButtonsAvailibility(side, tabIndex);
                string tmp = this.list[(int)side, tabIndex][this.iCurrentItem[(int)side, tabIndex]];
                return tmp;
            }
            public string GetNextFolder(Side side, int tabIndex) {
                if ( this.list[(int)side, tabIndex].Count == 0 ) {
                    return "";
                }
                if ( this.iCurrentItem[(int)side, tabIndex] > 0 ) {
                    this.iCurrentItem[(int)side, tabIndex]--;
                }
                this.SetPrevNextButtonsAvailibility(side, tabIndex);
                string tmp = this.list[(int)side, tabIndex][this.iCurrentItem[(int)side, tabIndex]];
                return tmp;
            }
            public List<string> GetFolderList(Side side, int tabIndex) {
                return this.list[(int)side, tabIndex];
            }
        }
    }

    // class to simplify the access to resource strings depending on the app's current localization
    public class Localizer {
        readonly ResourceManager manager;
        static readonly Localizer localizer = new Localizer();
        private Localizer() {
            // this file must exist as MyStringResource.resx PLUS it's localized translations (like MyStringResource.de-DE.resx)
            this.manager = new ResourceManager("cfw.MyStringResource", this.GetType().Assembly);
        }
        public static string GetString(string ResourceString) {
            string ret = localizer.manager.GetString(ResourceString, Thread.CurrentThread.CurrentUICulture);
            if ( ret == null )
                throw new Exception(string.Format("The localized string for {0} is not found", ResourceString));
            return ret;
        }
    }

    // subclassed ListView for synced scrolling of both ListView
    public class cfwListView : ListView {
        private const int WM_VSCROLL = 0x115;
        private const int WM_MOUSEWHEEL = 0x20a;
        private const int WM_KEYDOWN = 0x0100;
        private static bool m_scrolling;                                                      // in case buddy tries to scroll us
        public Control Buddy { get; set; }
        protected override void WndProc(ref System.Windows.Forms.Message m) {
            base.WndProc(ref m);

            if ( this.Buddy != null ) {                                                            // Buddy != null in case of synced scrolling 
                // this should cover all events realted to scrolling
                if ( (m.Msg == WM_KEYDOWN || m.Msg == WM_VSCROLL || m.Msg == WM_MOUSEWHEEL) && !m_scrolling ) {
                    m_scrolling = true;
                    if ( this.Items.Count > 0 ) {
                        ListViewItem lvi = this.TopItem;                                      // first visible item in initiating ListView
                        int slvi = this.SelectedIndices[0];                                   // first selected Index in initiating ListView
                        if ( ((ListView)this.Buddy).Items.Count > lvi.Index ) {
                            ((ListView)this.Buddy).TopItem = ((ListView)this.Buddy).Items[lvi.Index];   // set buddy's first visible item
                            if ( m.Msg == WM_KEYDOWN ) {                                      // only when using KeyBoard, sync selection too 
                                if ( ((ListView)this.Buddy).Items.Count > slvi ) {
                                    ((ListView)this.Buddy).SelectedIndices.Clear();
                                    ((ListView)this.Buddy).Items[slvi].Selected = true;
                                }
                            }
                        }
                    }
                    m_scrolling = false;
                }
            }
        }
    }

    public static class ControlExtentions {
        /// <summary>
        /// Turn on or off control double buffering (Dirty hack!)
        /// </summary>
        /// <param name="control">Control to operate</param>
        /// <param name="setting">true to turn on double buffering</param>
        public static void MakeDoubleBuffered(this Control control, bool setting) {
            Type controlType = control.GetType();
            PropertyInfo pi = controlType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(control, setting, null);
        }
    }

    // This class is an implementation of the 'IComparer' interface.
    public class ListViewColumnSorter : IComparer {
        /// <summary>
        /// Specifies the column to be sorted
        /// </summary>
        private int ColumnToSort;
        private bool bSortExtension;
        private string str1;
        private string str2;
        /// <summary>
        /// Specifies the order in which to sort (i.e. 'Ascending').
        /// </summary>
        private SortOrder OrderOfSort;
        /// <summary>
        /// Case insensitive comparer object
        /// </summary>
        private readonly CaseInsensitiveComparer ObjectCompare;

        /// <summary>
        /// Class constructor.  Initializes various elements
        /// </summary>
        public ListViewColumnSorter() {
            // Initialize the column to '0'
            this.ColumnToSort = 0;

            // sort by extension instead of filename
            this.bSortExtension = false;

            // Initialize the sort order to 'none'
            this.OrderOfSort = SortOrder.None;

            // Initialize the CaseInsensitiveComparer object
            this.ObjectCompare = new CaseInsensitiveComparer();
        }

        /// <summary>
        /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
        /// </summary>
        /// <param name="x">First object to be compared</param>
        /// <param name="y">Second object to be compared</param>
        /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
        public int Compare(object x, object y) {
            int compareResult = 0;

            // Cast the objects to be compared to ListViewItem objects
            ListViewItem listviewX = (ListViewItem)x;
            ListViewItem listviewY = (ListViewItem)y;

            // sort by name
            if ( this.ColumnToSort == 0 ) {
                this.str1 = listviewX.SubItems[this.ColumnToSort].Text;
                // in case of folders (listviewX.ImageIndex == 0) we add a preceding "!" to the folder name, that ensures folders being separated after sorting (--> files may start with .git etc)
                if ( (listviewX.ImageIndex == 3) || (listviewX.ImageIndex == 0) ) {
                    this.str1 = "!" + this.str1;
                }
                this.str2 = listviewY.SubItems[this.ColumnToSort].Text;
                if ( (listviewY.ImageIndex == 3) || (listviewY.ImageIndex == 0) ) {
                    this.str2 = "!" + this.str2;
                }
                // take care about extension only on files not on folders
                if ( this.bSortExtension ) {
                    if ( (listviewX.ImageIndex != 3) && (listviewX.ImageIndex != 0) ) {
                        this.str1 = Path.GetExtension(this.str1) + "_" + Path.GetFileNameWithoutExtension(this.str1);
                    }
                    if ( (listviewY.ImageIndex != 3) && (listviewY.ImageIndex != 0) ) {
                        this.str2 = Path.GetExtension(this.str2) + "_" + Path.GetFileNameWithoutExtension(this.str2);
                    }
                }
                // exec comparison
                compareResult = this.ObjectCompare.Compare(this.str1, this.str2);
            }
            // by type
            if ( this.ColumnToSort == 3 ) {
                this.str1 = listviewX.SubItems[this.ColumnToSort].Text;
                this.str2 = listviewY.SubItems[this.ColumnToSort].Text;
                compareResult = this.ObjectCompare.Compare(this.str1, this.str2);
            }
            // by date
            if ( this.ColumnToSort == 1 ) {
                try {
                    this.str1 = listviewX.SubItems[7].Text;
                    this.str2 = listviewY.SubItems[7].Text;

                    //str1 = listviewX.SubItems[ColumnToSort].Text;
                    //str2 = listviewY.SubItems[ColumnToSort].Text;
                    //str1 = str1.Length < 10 ? "00.00.0000 00:00:00.000" : str1;
                    //str2 = str2.Length < 10 ? "00.00.0000 00:00:00.000" : str2;
                    //str1 = str1.Substring(6, 4) + str1.Substring(3, 2) + str1.Substring(0, 2) + str1.Substring(11, 2) + str1.Substring(14, 2) + str1.Substring(17, 2) + str1.Substring(20, 3);
                    //str2 = str2.Substring(6, 4) + str2.Substring(3, 2) + str2.Substring(0, 2) + str2.Substring(11, 2) + str2.Substring(14, 2) + str2.Substring(17, 2) + str2.Substring(20, 3);

                    compareResult = this.ObjectCompare.Compare(ulong.Parse(this.str1), ulong.Parse(this.str2));
                } catch ( Exception ) {
                    return 0;
                }
            }
            // by size: -1 on folders makes sure, they are valued less than 0 byte files 
            if ( this.ColumnToSort == 2 ) {
                try {
                    this.str1 = listviewX.SubItems[this.ColumnToSort].Text.Replace(",", "");
                    this.str2 = listviewY.SubItems[this.ColumnToSort].Text.Replace(",", "");
                    this.str1 = this.str1[0] == '<' ? "-1" : this.str1;
                    this.str2 = this.str2[0] == '<' ? "-1" : this.str2;
                    compareResult = this.ObjectCompare.Compare(Int64.Parse(this.str1), Int64.Parse(this.str2));
                } catch ( Exception ) {
                    return 0;
                }
            }

            // Calculate correct return value based on object comparison
            if ( this.OrderOfSort == SortOrder.Ascending ) {
                // Ascending sort is selected, return normal result of compare operation
                return compareResult;
            } else if ( this.OrderOfSort == SortOrder.Descending ) {
                // Descending sort is selected, return negative result of compare operation
                return (-compareResult);
            } else {
                // Return '0' to indicate they are equal
                return 0;
            }
        }

        /// <summary>
        /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
        /// </summary>
        public int SortColumn {
            set {
                this.ColumnToSort = value;
            }
            get {
                return this.ColumnToSort;
            }
        }

        /// <summary>
        /// Gets or sets the extension sorting flag
        /// </summary>
        public bool SortExtension {
            set {
                this.bSortExtension = value;
            }
            get {
                return this.bSortExtension;
            }
        }

        /// <summary>
        /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
        /// </summary>
        public SortOrder Order {
            set {
                this.OrderOfSort = value;
            }
            get {
                return this.OrderOfSort;
            }
        }
    }
}
