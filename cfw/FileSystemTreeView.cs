using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;                             // - " - 
using System.Drawing;
using System.IO;                                      // Path, Files 
using System.Linq;
using System.Net;                                     // - " -
using System.Net.NetworkInformation;                  // enum PCs in local network via ping
using System.Net.Sockets;                             // - " -
using System.Reflection;
using System.Runtime.InteropServices;                 // DLLImport
using System.Runtime.Serialization.Formatters.Binary; // - " -
using System.Text;
using System.Threading;                               // - " -    
using System.Threading.Tasks;                         // tasks
using System.Windows.Forms;

namespace cfw {
    public partial class FileSystemTreeView : System.Windows.Forms.UserControl {
        private readonly GrzTools.FastFileFind m_fff = new GrzTools.FastFileFind(null);
        private static bool m_run = true;
        static bool m_bDoNotDisturb = false;
        private string m_DefaultFile = "";
        private string m_DefaultPath = "";
        private bool m_bNetworkOnly = false;
        readonly List<host> m_HostList = new List<host>();
        List<host> m_HostListNew = new List<host>();
        bool m_bAutoNetworkScan = false;
        BackgroundWorker bg = null;
        readonly ToolTip m_toolTip;                             // a general tooltip


        public FileSystemTreeView() {
            this.InitializeComponent();

            // populate TreeView initially with something
            TreeNode rootNode = new TreeNode("- init -", 4, 4);
            this.treeView1.Nodes.Add(rootNode);

            // auto network scan initially 2s
            if ( this.m_bAutoNetworkScan ) {
                this.timerRefresh.Interval = 2000;
                this.timerRefresh.Start();
            }

            // tooltip 
            this.components = new System.ComponentModel.Container();
            this.m_toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.m_toolTip.OwnerDraw = true;
            this.m_toolTip.BackColor = System.Drawing.Color.White;
            this.m_toolTip.Draw += new DrawToolTipEventHandler(this.toolTip_Draw);
        }

        // ToolTip background color change
        void toolTip_Draw(object sender, DrawToolTipEventArgs e) {
            e.DrawBackground();
            e.DrawBorder();
            e.DrawText();
        }

        // auto network refresh
        private void timerRefresh_Tick(object sender, EventArgs e) {
            this.timerRefresh.Stop();
            // compare IP count in current list with a new list
            this.RefreshIPs();
            if ( this.m_HostListNew == null ) {
                this.timerRefresh.Start();
                return;
            }
            if ( this.m_HostList == null ) {
                this.timerRefresh.Start();
                return;
            }
            // silent update in case of non matching IP lists in the network
            var firstNotSecond = this.m_HostList.Except(this.m_HostListNew).ToList();
            var secondNotFirst = this.m_HostListNew.Except(this.m_HostList).ToList();
            if ( firstNotSecond.Any() || secondNotFirst.Any() ) {
                if ( this.bg != null ) {
                    this.bg.CancelAsync();
                    this.bg.Dispose();
                    this.bg = null;
                }
                // bgw will be destroyed when cfw closes, could be cancelled by disabling "network refresh" and doesn't have the nasty Application.DoEvents()
                this.bg = new BackgroundWorker();
                this.bg.WorkerSupportsCancellation = true;
                this.bg.DoWork += new DoWorkEventHandler(this.SilentNetworkRefresh);
                this.bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.bg_RunWorkerCompleted);
                this.bg.RunWorkerAsync();
            } else {
                if ( this.m_bAutoNetworkScan ) {
                    this.timerRefresh.Interval = 300000;
                    this.timerRefresh.Start();
                }
            }
        }
        void bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if ( this.bg != null ) {
                this.bg.Dispose();
                this.bg = null;
            }
            // auto network scan every 5min
            if ( this.m_bAutoNetworkScan ) {
                this.timerRefresh.Interval = 300000;
                this.timerRefresh.Start();
            }
        }
        public bool AutoNetworkScan {
            get {
                return this.m_bAutoNetworkScan;
            }
            set {
                this.m_bAutoNetworkScan = value;
                if ( this.m_bAutoNetworkScan ) {
                    this.timerRefresh.Interval = 2000;
                    this.timerRefresh.Start();
                } else {
                    this.timerRefresh.Stop();
                    if ( this.bg != null ) {
                        if ( this.bg.IsBusy ) {
                            this.bg.CancelAsync();
                        }
                        this.bg.Dispose();
                        this.bg = null;
                    }
                }
            }
        }

        // highlight default folder&file, in case they are set
        private void FileSystemTreeView_Load(object sender, EventArgs e) {
            if ( this.treeView1.SelectedNode != null ) {
                this.treeView1.SelectedNode.EnsureVisible();
            }
            if ( this.m_DefaultFile.Length > 0 ) {
                foreach ( ListViewItem lv in this.listView1.Items ) {
                    if ( lv.Text == this.m_DefaultFile ) {
                        lv.Selected = true;
                        this.listView1.SelectedItems[0].EnsureVisible();
                        this.listView1.Focus();
                        break;
                    }
                }
            }

            this.PopulateDriveList();
            this.treeView1.Nodes[0].Expand();
        }

        // public variables & methods
        public bool Break = false;
        public string SelectedPath = "";
        public string SelectedFile = "";
        private string m_sIP = "";
        public string DefaultIP {
            get {
                return this.m_sIP;
            }
            set {
                this.m_sIP = value;
            }
        }
        public string DefaultPath {
            set {
                // get drive from path
                this.m_DefaultPath = value;

                // get root node
                TreeNode sn = this.treeView1.Nodes[0];

                if ( this.m_DefaultPath.Length > 0 ) {
                    string defaultDrive = Path.GetPathRoot(this.m_DefaultPath);
                    this.m_DefaultFile = Path.GetFileName(this.m_DefaultPath);
                    if ( this.m_DefaultPath == "Network" ) {
                        this.m_bNetworkOnly = true;
                    } else {
                        this.m_bNetworkOnly = false;
                    }

                    // find drive and select but don't expand it yet
                    bool run = !this.Break;
                    TreeNode curNode = null;
                    foreach ( TreeNode tn in sn.Nodes ) {
                        // check all nodes and set next level
                        this.PopulateDirectoryNextLevel(ref run, tn);
                        // find matching drive
                        if ( defaultDrive == tn.Text + "\\" ) {
                            this.treeView1.SelectedNode = tn;
                            curNode = tn;
                        }
                    }

                    // navigate to defaultPath, select & expand it, if its length is >3: this way we can control, that a path is shown expanded, while a drive is not  
                    if ( (curNode != null) && (this.m_DefaultPath.Length > 3) ) {
                        // split path to array of foldernames
                        string[] splitArr = this.m_DefaultPath.Split('\\');
                        // search foldernames in tree; we start with index 1, because above we already took care of the drive name
                        for ( int i = 1; i < splitArr.Length; i++ ) {
                            // first populate next level of current node and expand it
                            run = !this.Break;
                            this.PopulateDirectoryNextLevel(ref run, curNode);
                            curNode.Expand();
                            // search current folder name in the just generated population of nodes
                            foreach ( TreeNode n in curNode.Nodes ) {
                                // check all neighbour nodes and set next level
                                this.PopulateDirectoryNextLevel(ref run, n);
                                // when currentfolder name matches with a node, we select it
                                if ( n.Text == splitArr[i] ) {
                                    curNode = n;
                                    this.treeView1.SelectedNode = n;
                                }
                            }
                        }
                    }
                }

                // unconditionally select "My Computer"
                this.treeView1.SelectedNode = this.treeView1.Nodes[0];

                // make sure, root node is expanded
                if ( !sn.IsExpanded ) {
                    sn.Expand();
                }
            }
        }
        public void Refresh(string newNodeName) {
            // global breaker
            this.Break = false;

            //
            // media changed
            //
            if ( newNodeName == "mediachanged" ) {
                this.treeView1.SelectedNode = this.treeView1.Nodes[0];
                newNodeName = "";
            }

            //
            // re read 'My Computer'
            //
            try {
                if ( this.treeView1.SelectedNode.Text == "My Computer" ) {
                    //                    this.treeView1.SelectedNode.Nodes.Clear();
                    this.PopulateDriveList();
                    this.treeView1.Nodes[0].Expand();

                    // ??
                    if ( this.m_DefaultPath.Length > 0 ) {
                        this.DefaultPath = this.m_DefaultPath;
                    }

                    return;
                }
            } catch ( Exception ) {
                return;
            }

            //
            // re read 'Network'
            //
            if ( this.treeView1.SelectedNode.FullPath.StartsWith("My Computer\\Network") ) {
                this.treeView1.SelectedNode.Collapse();
                if ( this.treeView1.SelectedNode.FullPath.EndsWith("My Computer\\Network") ) {
                    // network root
                    this.treeView1.SelectedNode.Nodes.Clear();
                    this.treeView1.SelectedNode.Nodes.Add("-empty-");
                    this.treeView1.SelectedNode.Collapse();
                    string filename = Path.Combine(Application.StartupPath, "cfwnetscan.bin");
                    File.Delete(filename);
                } else {
                    string[] split = this.treeView1.SelectedNode.FullPath.Split('\\');
                    if ( split.Length == 3 ) {
                        // if workstation is root == 3x string inside split array: gets populated with shares
                        this.treeView1.SelectedNode.Nodes.Clear();
                        SHAREINFO[] si = this.EnumNetShares(this.treeView1.SelectedNode.Text);
                        for ( int r = 0; r < si.Length; r++ ) {
                            Application.DoEvents();
                            string sharename = si[r].shi.shi2_netname;
                            uint type = si[r].shi.shi2_type;
                            string path = si[r].shi.shi2_path;
                            if ( type == 0 ) {
                                TreeNode subTn = this.treeView1.SelectedNode.Nodes.Add(sharename + " (" + path + ")");  // node.Text:    sharename (sharepath)
                                subTn.Tag = sharename;                                                                  // node.Tag:     sharename 
                                subTn.Checked = false;                                                                  // node.Checked: indicator, whether the node was already read - if so, it's much faster when expanding
                                subTn.ImageIndex = 6;
                                bool run = !this.Break;
                                this.PopulateDirectoryNextLevel(ref run, subTn);
                                subTn.Checked = true;                                                                   // node.Checked: indicator, whether the node was already read - if so, it's much faster when expanding
                            }
                        }
                    } else {
                        // a real shared folder or a subfolder
                        this.treeView1.SelectedNode.Checked = false;
                    }
                    // save the just updated network tree to disk
                    TreeNode tn = this.treeView1.SelectedNode;
                    do {
                        tn = tn.Parent;
                    } while ( tn.Text != "Network" );
                    this.SaveTree(tn);
                }
                // expand takes care about the next level directories
                this.treeView1.SelectedNode.Expand();

                return;
            }

            //
            // re read regular drives and folders
            //
            this.treeView1.BeginUpdate();
            this.listView1.BeginUpdate();
            this.Cursor = Cursors.WaitCursor;
            // collaps and delete subnodes and reset "CheckedState" of the selected node in treeview
            this.treeView1.SelectedNode.Collapse();
            this.treeView1.SelectedNode.Nodes.Clear();
            this.treeView1.SelectedNode.Checked = false;   // abuse: .Checked as indicator, whether the node was already read - if so, it's much faster when expanding
            // re read nodes for treeview
            bool brun = !this.Break;
            // read "next" level directories below the currently selected node
            this.PopulateDirectoryNextLevel(ref brun, this.treeView1.SelectedNode);
            // treeView1_BeforeExpand takes care about the "over next" level by itself
            this.treeView1.SelectedNode.Expand();
            // IF SO: select recently generated new folder == newNodeName
            if ( newNodeName.Length > 0 ) {
                foreach ( TreeNode tn in this.treeView1.SelectedNode.Nodes ) {
                    if ( tn.Text == newNodeName ) {
                        this.treeView1.SelectedNode = tn;
                        break;
                    }
                }
            }
            // delete files & folders in listview
            this.listView1.Items.Clear();
            // re read files & folders for listview
            this.PopulateFiles(this.treeView1.SelectedNode, false);
            this.treeView1.EndUpdate();
            this.listView1.EndUpdate();
            this.Cursor = Cursors.Default;
        }

        // child control wants to inform parent about progress while scanning network:  public event handler
        public event EventHandler<NetworkScanEventArgs> NetworkScanProgress;
        public class NetworkScanEventArgs : EventArgs {
            public NetworkScanEventArgs(long current, long maximal) {
                this.Current = current;
                this.Maximal = maximal;
            }
            public long Current;
            public long Maximal;
        }

        // child control wants to inform parent about a changed selection: public event handler
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;
        public class SelectionChangedEventArgs : EventArgs {
            public SelectionChangedEventArgs(string selection) {
                this.Selection = selection;
            }
            public string Selection { get; set; }
        }

        // child control wants to close the parent application: public event handler and the sources behind this event
        public event EventHandler<WantCloseEventArgs> WantClose;
        public class WantCloseEventArgs : EventArgs {
            public WantCloseEventArgs(string selection) {
                this.Selection = selection;
            }
            public string Selection { get; set; }
        }
        private void treeView1_MouseClick(object sender, MouseEventArgs e) {
            TreeNode selectedNode = this.treeView1.HitTest(e.Location).Node;
            if ( selectedNode == null ) {
                this.m_toolTip.Hide((Control)sender);
                return;
            }

            // 'mouse left' shall show ip in a tooltip
            if ( e.Button == System.Windows.Forms.MouseButtons.Left ) {
                if ( selectedNode.Tag == null ) {
                    this.m_toolTip.Hide((Control)sender);
                    return;
                }
                // get node ip
                string ip = selectedNode.Tag.ToString();
                // if ip is already shown, then return
                if ( selectedNode.Text.StartsWith(ip) ) {
                    Clipboard.SetText(ip);
                    this.m_toolTip.Hide((Control)sender);
                    return;
                }
                // write to clipboard
                Clipboard.SetText(ip);
                // show tooltip with ip info
                Size textSize = TextRenderer.MeasureText(ip, ((Control)sender).Font);
                Point pt = ((Control)sender).PointToClient(MousePosition);
                pt.X += textSize.Width / 2;
                pt.Y -= textSize.Height / 2;
                this.m_toolTip.Show(ip, (Control)sender, pt);
            }

            // mouse right == select & return
            if ( e.Button == System.Windows.Forms.MouseButtons.Right ) {
                if ( selectedNode != null ) {
                    this.treeView1.SelectedNode = selectedNode;
                    string ip = (string)selectedNode.Tag;
                    this.SelectedPath = getFullPath(selectedNode.FullPath, ip);
                    bool selectedPathExists = true;
                    if ( selectedNode.ImageIndex == 6 ) {  // mapped but perhaps not accessible network drive
                        if ( (ip != null) && (ip.Length > 0) ) {
                            if ( !GrzTools.Network.PingIpOk(ip) ) {
                                selectedPathExists = false;
                            }
                        } else {
                            if ( (this.SelectedPath.Length > 1) && !GrzTools.Network.PingNetDriveOk(this.SelectedPath/*.Substring(0, 2)*/) ) {
                                selectedPathExists = false;
                            } else {
                                this.SelectedPath = selectedNode.Text; // user clicked on 'Network'
                                if ( !GrzTools.FileTools.PathExists(this.SelectedPath, 500) ) {
                                    selectedPathExists = false;
                                }
                            }
                        }
                    }
                    if ( selectedPathExists ) {
                        EventHandler<WantCloseEventArgs> handler = WantClose;
                        if ( handler != null ) {
                            handler(sender, new WantCloseEventArgs(this.SelectedPath));
                        }
                    } else {
                        MessageBox.Show(String.Format("Cannot switch to '{0}'.", this.SelectedPath), "Note");
                    }
                }
            }
        }
        private void treeView1_MouseLeave(object sender, EventArgs e) {
            this.m_toolTip.Hide((Control)sender);
            return;
        }
        private void treeView1_NodeMouseHover(object sender, TreeNodeMouseHoverEventArgs e) {
            TreeNode selectedNode = e.Node;
            if ( selectedNode == null ) {
                this.m_toolTip.Hide((Control)sender);
                return;
            }
            if ( selectedNode.Tag == null ) {
                this.m_toolTip.Hide((Control)sender);
                return;
            }
            this.m_toolTip.Hide((Control)sender);
        }
        private void listView1_MouseClick(object sender, MouseEventArgs e) {
            if ( e.Button == System.Windows.Forms.MouseButtons.Right ) {
                ListViewItem selectedItem = this.listView1.HitTest(e.Location).Item;
                if ( selectedItem != null ) {
                    selectedItem.Selected = true;
                    string selTxt = selectedItem.Text;
                    if ( (selTxt.Length == 2) && (selTxt[1] == ':') ) {
                        this.SelectedPath = selTxt;
                    } else {
                        this.SelectedPath = getFullPath(this.treeView1.SelectedNode.FullPath, (string)this.treeView1.SelectedNode.Tag);
                        if ( selectedItem.ImageIndex == 0 ) { // only folder
                            if ( this.SelectedPath[this.SelectedPath.Length - 1] != '\\' ) {
                                this.SelectedPath += "\\";
                            }
                            this.SelectedPath = Path.Combine(this.SelectedPath, selectedItem.Text);
                        }
                    }
                    if ( selectedItem.ImageIndex == 9 ) { // mapped but not accessible network share
                        this.SelectedPath = "";
                    }
                }
                EventHandler<WantCloseEventArgs> handler = WantClose;
                if ( handler != null ) {
                    handler(sender, new WantCloseEventArgs(this.SelectedPath));
                }
            }
        }

        // detect Windows' Download Folder
        public static readonly Guid guid_Downloads = new Guid("374DE290-123F-4565-9164-39C4925E467B");
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out string pszPath);

        // return listview items with drive info
        private List<ListViewItem> GetDriveInfo() {
            List<ListViewItem> retList = new List<ListViewItem>();
            string[] strarr = new string[7] { "", "", "", "", "", "", "" };
            ListViewItem lvi;

            this.Cursor = Cursors.WaitCursor;

            // supposed to be much faster AND more reliable than the WMI crap 
            foreach ( DriveInfo drive in DriveInfo.GetDrives() ) {
                strarr = new string[7] { "", "", "", "", "", "", "" };
                int imageindex = 2;         // aka "LevelUp"
                strarr[0] = drive.Name.Substring(0, drive.Name.Length - 1);

                // 20160312: GetDrives() --> drive.IsReady will hang in case a network drive is mapped but not accessible
                if ( drive.DriveType == DriveType.Network ) {
                    if ( !GrzTools.Network.PingNetDriveOk(drive.Name/*.Substring(0, drive.Name.Length-1)*/) ) {
                        strarr[0] = drive.Name.Substring(0, drive.Name.Length - 1);
                        imageindex = 9;
                        lvi = new ListViewItem(strarr, imageindex);
                        retList.Add(lvi);
                        continue;
                    }
                }

                if ( drive.IsReady ) {
                    strarr[1] = GrzTools.StringTools.SizeSuffix(drive.TotalSize);
                    strarr[2] = GrzTools.StringTools.SizeSuffix(drive.TotalFreeSpace);
                    strarr[3] = Math.Round(100f * drive.TotalFreeSpace / drive.TotalSize).ToString() + "%";
                    strarr[4] = drive.VolumeLabel;
                    strarr[5] = drive.DriveFormat.ToString();
                    strarr[6] = GrzTools.clsDiskInfoEx.GetFirstPhysicalDriveString(strarr[0]);
                }
                if ( drive.DriveType == DriveType.Removable ) {
                    imageindex = 8;
                }
                if ( drive.DriveType == DriveType.Network ) {
                    imageindex = 6;
                    strarr[6] = GrzTools.Network.LocalToUNC(strarr[0]);
                }
                if ( drive.DriveType == DriveType.CDRom ) {
                    imageindex = 7;
                }
                lvi = new ListViewItem(strarr, imageindex);
                retList.Add(lvi);
            }

            // wait is over
            this.Cursor = Cursors.Default;

            return retList;
        }

        // 20160417: open dlg showing m_DefaultPath 
        void SelectPathInTreeView(string defaultPath) {
            // get drive from path
            if ( defaultPath.Length == 0 ) {
                return;
            }
            string defaultDrive = Path.GetPathRoot(defaultPath);

            // TreeView root node
            TreeNode sn = this.treeView1.Nodes[0];

            // find drive and select it but don't expand it yet
            bool run = !this.Break;
            TreeNode curNode = null;
            foreach ( TreeNode tn in sn.Nodes ) {
                // check all nodes and set next level
                this.PopulateDirectoryNextLevel(ref run, tn);
                // find matching drive
                if ( defaultDrive.StartsWith(tn.Text, StringComparison.InvariantCultureIgnoreCase) ) {
                    this.treeView1.SelectedNode = tn;
                    curNode = tn;
                }
            }

            // navigate to defaultPath, select & expand it, if its length is >3: this way we can control, that a path is shown expanded, while a drive is not  
            if ( (curNode != null) && (defaultPath.Length > 3) ) {
                // split path to array of foldernames
                string[] splitArr = defaultPath.Split('\\');
                // search foldernames in tree; we start with index 1, because above we already took care of the drive name
                for ( int i = 1; i < splitArr.Length; i++ ) {
                    // first populate next level of current node and expand it
                    run = !this.Break;
                    this.PopulateDirectoryNextLevel(ref run, curNode);
                    curNode.Expand();
                    // search current folder name in the just generated population of nodes
                    foreach ( TreeNode n in curNode.Nodes ) {
                        // check all neighbour nodes and set next level
                        this.PopulateDirectoryNextLevel(ref run, n);
                        // when currentfolder name matches with a node, we select it
                        if ( n.Text == splitArr[i] ) {
                            curNode = n;
                            this.treeView1.SelectedNode = n;
                        }
                    }
                }
            }
        }

        // populate TreeView with a drive list & some special folders
        private void PopulateDriveList() {
            int imageIndex = 0;
            int selectIndex = 0;

            this.Cursor = Cursors.WaitCursor;

            //clear TreeView
            this.treeView1.Nodes.Clear();

            // root
            TreeNode rootNode = new TreeNode("My Computer", 4, 4);
            this.treeView1.Nodes.Add(rootNode);
            if ( !rootNode.IsExpanded ) {
                rootNode.Expand();
            }

            // special handling in case we only want to select a network node
            TreeNode newNode;
            if ( this.m_bNetworkOnly ) {
                newNode = new TreeNode("Network", 6, 6);
                int pos = rootNode.Nodes.Add(newNode);
                rootNode.Nodes[pos].Nodes.Add("-empty-");
                this.Cursor = Cursors.Default;
                return;
            }

            // Desktop
            newNode = new TreeNode("Desktop", 5, 5);
            rootNode.Nodes.Add(newNode);

            // Documents
            newNode = new TreeNode("Documents", 0, 3);
            rootNode.Nodes.Add(newNode);

            // Downloads
            newNode = new TreeNode("Downloads", 0, 3);
            rootNode.Nodes.Add(newNode);

            // all drives
            foreach ( DriveInfo drive in DriveInfo.GetDrives() ) {
                switch ( drive.DriveType ) {
                    case DriveType.CDRom:
                        imageIndex = 7;
                        selectIndex = 7;
                        break;
                    case DriveType.Fixed:
                        imageIndex = 2;
                        selectIndex = 2;
                        break;
                    case DriveType.Removable:
                        imageIndex = 8;
                        selectIndex = 8;
                        break;
                    case DriveType.Network:
                        imageIndex = 6;
                        if ( !GrzTools.Network.PingNetDriveOk(drive.Name/*.Substring(0, 2)*/) ) {
                            imageIndex = 9;
                        }
                        selectIndex = 6;
                        break;
                    default:
                        imageIndex = 0;
                        selectIndex = 3;
                        break;
                }
                //create new drive node
                newNode = new TreeNode(drive.Name.Substring(0, drive.Name.Length - 1), imageIndex, selectIndex);
                //add new node for drive
                rootNode.Nodes.Add(newNode);
            }

            // add a fake (=empty) Network node with a more faked subnode
            newNode = new TreeNode("Network", 6, 6);
            int last = rootNode.Nodes.Add(newNode);
            rootNode.Nodes[last].Nodes.Add("-empty-");


            // wait is over
            this.Cursor = Cursors.Default;

            // 20160417: change TreeView to m_DefaultPath 
            this.SelectPathInTreeView(this.m_DefaultPath);
            // 20160417: reset m_DefaultPath to "", it only makes sense at startup
            this.m_DefaultPath = "";
        }

        // helper: obtain host's own IP address
        string MyIp() {
            string output = "";
            NetworkInterface[] ni = NetworkInterface.GetAllNetworkInterfaces();
            foreach ( NetworkInterface item in ni ) {
                if ( (item.OperationalStatus == OperationalStatus.Up) && ((item.NetworkInterfaceType == NetworkInterfaceType.Ethernet) || (item.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)) ) {
                    IPInterfaceProperties adapterProperties = item.GetIPProperties();
                    // we take the ip only if it has a gateway
                    if ( adapterProperties.GatewayAddresses.FirstOrDefault() != null ) {
                        foreach ( UnicastIPAddressInformation ip in adapterProperties.UnicastAddresses ) {
                            if ( ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ) {
                                output = ip.Address.ToString();
                                break;
                            }
                        }
                    }
                }
                if ( output != "" ) {
                    break;
                }
            }
            return this.m_sIP.Length > 0 ? this.m_sIP : output;
        }
        // helper: obtain hosts in a network
        struct host {
            public host(string ip, string host) {
                this.hostip = ip;
                this.hostname = host;
            }
            public string hostip;
            public string hostname;
        }
        static CountdownEvent _countdown;
        static int _upCount = 0;
        static readonly object _lockObj = new object();
        void SearchActiveIpAddresses(string ip, bool bSilent) {
            this.m_HostList.Clear();

            //
            // get IP adresses async, async handler generates m_HostList containing IP Addresses 
            //
            EventHandler<SelectionChangedEventArgs> handler = SelectionChanged;
            if ( (handler != null) && !bSilent ) {
                handler(null, new SelectionChangedEventArgs("Collecting IP Addresses"));
            }
            _countdown = new CountdownEvent(1);
            string ipBase = ip.Substring(0, ip.LastIndexOf(".") + 1);  // sample: "10.0.1." 
            for ( int i = 1; i < 255; i++ ) {
                _countdown.AddCount();
                string currIp = ipBase + i.ToString();
                Ping p = new Ping();
                p.PingCompleted += new PingCompletedEventHandler(this.p_PingCompleted);
                p.SendAsync(currIp, 100, currIp);
            }
            _countdown.Signal();
            _countdown.Wait(2000);
            do {
                Application.DoEvents();
            } while ( _countdown.CurrentCount > 0 );

            //
            // fastest way to resolve ip addresses to netbios names (works in DNS environments too, because DNS sets Netbios names)
            //
            if ( (handler != null) && !bSilent ) {
                handler(null, new SelectionChangedEventArgs("Resolving IP-Addresses to Hostnames"));
            }
            EventHandler<NetworkScanEventArgs> phandler = NetworkScanProgress;
            if ( (handler != null) && !bSilent ) {
                phandler(null, new NetworkScanEventArgs(0, this.m_HostList.Count));
            }
            // start parallel tasks as much IP Addresses were found previously 
            List<Task<host>> tasks = new List<Task<host>>();
//            Parallel.For(1, m_HostList.Count, i => {
            for ( int i = 0; i < this.m_HostList.Count; i++ ) {
                try {
                    try {

                        // http://stackoverflow.com/questions/18029881/how-to-pass-multiple-parameter-in-task
                        //Task<host> t = new Task<host>( () => NetbiosName(m_HostList[i].hostip) );

                        // !! lambda notation is tricky !! 
                        Task<host> t = new Task<host>(n => NetbiosName(n.ToString()), this.m_HostList[i].hostip);

                        t.Start();
                        tasks.Add(t);
                    } catch ( Exception ) {; }
                } catch ( NullReferenceException ) {
                    ;
                }
            }
            // loop running tasks until all tasks finished or break was signalled
            int currCount = 1;
            do {
                for ( int i = 0; i < tasks.Count; i++ ) {
                    Application.DoEvents();
                    if ( (tasks[i] != null) && tasks[i].IsCompleted ) {
                        // find IP in m_HostList via predicate
                        int ndx = this.m_HostList.FindIndex(o => o.hostip == tasks[i].Result.hostip);
                        // update m_HostList: now we have IP Address and Hostname 
                        this.m_HostList[ndx] = new host(tasks[i].Result.hostip, tasks[i].Result.hostname);
                        tasks.RemoveAt(i);
                        // progress
                        if ( phandler != null ) {
                            phandler(null, new NetworkScanEventArgs(currCount++, 0));
                        }
                    }
                }
            } while ( (tasks.Count > 0) && !this.Break );
            if ( (handler != null) && !bSilent ) {
                phandler(null, new NetworkScanEventArgs(0, 0));
            }
        }
        // async ping handler generates m_HostList containing IP Addresses
        void p_PingCompleted(object sender, PingCompletedEventArgs e) {
            try {
                if ( e.Reply != null && e.Reply.Status == IPStatus.Success ) {
                    lock ( _lockObj ) {
                        string ip = (string)e.UserState;
                        this.m_HostList.Add(new host(ip, ip));
                        _upCount++;
                    }
                }
                _countdown.Signal();
            } catch ( Exception ) {; }
        }
        // NetBios Machine Name Resolver: the following byte stream contains the necessary message to request a NetBios name from a machine
        static readonly byte[] NameRequest = new byte[]{
            0x80, 0x94, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x43, 0x4b, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41,
            0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x00, 0x00, 0x21, 0x00, 0x01
        };
        static host NetbiosName(string ipaddress) {
            host hst = new host(ipaddress, ipaddress);
            IPAddress ipa = IPAddress.Parse(ipaddress);
            byte[] receiveBuffer = new byte[1024];
            Socket requestSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            requestSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 100);
            try {
                EndPoint remoteEndpoint = new IPEndPoint(ipa, 137);
                IPEndPoint originEndpoint = new IPEndPoint(IPAddress.Any, 0);
                requestSocket.Bind(originEndpoint);
                requestSocket.SendTo(NameRequest, remoteEndpoint);
                int receivedByteCount = requestSocket.ReceiveFrom(receiveBuffer, ref remoteEndpoint);
                if ( receivedByteCount >= 90 ) {
                    Encoding enc = new ASCIIEncoding();
                    string deviceName = enc.GetString(receiveBuffer, 57, 16);
                    deviceName = deviceName.Replace('\0', ' ');
                    deviceName = deviceName.Trim();
                    hst.hostname = deviceName;
                    string networkName = enc.GetString(receiveBuffer, 75, 16).Trim();
                }
            } catch ( SocketException ) {
                ;
            }
            return hst;
        }

        // silent refresh depending on availibility of IPs in HostList vs. AltHostList
        public void RefreshIPs() {
            if ( this.m_HostListNew == null ) {
                return;
            }

            string myIP = this.MyIp();
            if ( myIP.Length < 7 ) {
                this.m_HostListNew.Clear();
                this.m_HostListNew = null;
                return;
            }

            this.m_HostListNew.Clear();

            _countdown = new CountdownEvent(1);
            string ipBase = myIP.Substring(0, myIP.LastIndexOf(".") + 1);  // sample: "10.0.1." 
            for ( int i = 1; i < 255; i++ ) {
                _countdown.AddCount();
                string currIp = ipBase + i.ToString();
                Ping p = new Ping();
                p.PingCompleted += new PingCompletedEventHandler(this.p_RefreshPingCompleted);
                p.SendAsync(currIp, 100, currIp);
            }
            _countdown.Signal();
            do {
                Application.DoEvents();
            } while ( _countdown.CurrentCount > 0 );
        }
        void p_RefreshPingCompleted(object sender, PingCompletedEventArgs e) {
            string ip = (string)e.UserState;
            if ( e.Reply != null && e.Reply.Status == IPStatus.Success ) {
                this.m_HostListNew.Add(new host(ip, ip));
                lock ( _lockObj ) {
                    _upCount++;
                }
            }
            _countdown.Signal();
        }

        // silent network refresh is a bgw, which starts a number of tasks in parallel
        void SilentNetworkRefresh(object sender, DoWorkEventArgs e) {
            TreeNode tn = new TreeNode();

            // obtain my own IP as a base for the search in the local network
            string ip = this.MyIp();
            if ( ip.Length < 7 ) {
                return;
            }

            //
            // builds "List<host> m_HostList" of found IP Addresses + Hostnames in the searched network
            // 
            this.SearchActiveIpAddresses(ip, true);
            if ( (this.bg == null) || this.bg.CancellationPending ) {
                e.Cancel = true;
                return;
            }

            // remove all existing network nodes
            tn.Nodes.Clear();

            //
            // get network shares of all Hostnames
            //
            // ask with parallel tasks for shared folders at living ip addresses
            List<Task<String[]>> tasks = new List<Task<String[]>>();
            foreach ( host h in this.m_HostList ) {
                Task<String[]> t = new Task<String[]>(n => this.GetDirectoriesInNetworkLocation(n.ToString()), h.hostip);
                t.Start();
                tasks.Add(t);
            }
            // wait for completion of the tasks: needs some precaution to avoid messing things up (disable node) but allows Cancel & Progress
            List<TreeNode> tnl = new List<TreeNode>();
            do {
                for ( int i = 0; i < tasks.Count; i++ ) {
                    // since this method is a bgw, we don't need this anymore, it was needed when called from the form's thread 
                    // Application.DoEvents();
                    if ( tasks[i].IsCompleted ) {
                        // get returned data from thread
                        String[] si = tasks[i].Result;
                        // add a network workstation node, first find its literal machine name
                        string ipadd = si[0];
                        host hst = this.m_HostList.Find(o => o.hostip == ipadd);
                        TreeNode wsn = new TreeNode(hst.hostname, 6, 6);
                        wsn.Tag = ipadd;
                        tnl.Add(wsn);
                        // add to workstation node subnodes (if any) with the network shares
                        int entries = si.Length;
                        string sharename = si[0];
                        if ( sharename.Length > 0 ) {
                            for ( int r = 1; r < entries; r++ ) {
                                Application.DoEvents();
                                sharename = si[r];
                                TreeNode subTn = tnl[tnl.Count - 1].Nodes.Add(sharename); 
                                subTn.Tag = sharename;                                                                }
                        }
                        // remove the just completed task from the tasklist
                        tasks.RemoveAt(i);
                    }

                    if ( (this.bg == null) || this.bg.CancellationPending ) {
                        break;
                    }
                }
            } while ( (tasks.Count > 0) && (this.bg != null) && !this.bg.CancellationPending );

            if ( (this.bg == null) || this.bg.CancellationPending ) {
                e.Cancel = true;
                return;
            }

            // sort the list of nodes: Lambda expression with nameless delegate instead of a separate IComparer class
            tnl.Sort((x, y) => string.Compare(x.Text, y.Text));
            // AddRange had allowed us sorting and is faster
            tn.Nodes.AddRange(tnl.ToArray());

            // if there are no network nodes, then we add a fake one: otherwise 'before expand' would never get called
            if ( tn.Nodes.Count == 0 ) {
                tn.Nodes.Add("-empty-");
            }

            // since it was a silent update/refresh, we save the new tree 
            this.SaveTree(tn);
        }
        //
        // add all network nodes
        //
        void PopulateNetwork(TreeNode tn) {
            // obtain my own IP as a base for the search in the local network
            string ip = this.MyIp();
            if ( ip.Length < 7 ) {
                MessageBox.Show("Cannot obtain valid IP address.\r\nPerhaps, Network is down?", "Network Error");
                return;
            }

            // remove all existing network nodes
            tn.Nodes.Clear();

            // builds "List<host> m_HostList" of found IP Addresses + Hostnames in the searched network
            this.SearchActiveIpAddresses(ip, false);
            if ( this.Break ) {
                return;
            }

            // reset progress bar and text
            EventHandler<NetworkScanEventArgs> phandler = NetworkScanProgress;
            if ( phandler != null ) {
                phandler(null, new NetworkScanEventArgs(0, this.m_HostList.Count));
            }

            //
            // get network shares of all Hostnames
            //
            EventHandler<SelectionChangedEventArgs> handler = SelectionChanged;
            if ( handler != null ) {
                handler(null, new SelectionChangedEventArgs("Get Network Shares from Hostnames"));
            }
            // ask in parallel tasks for shared folders at living ip addresses
            List<Task<String[]>> tasks = new List<Task<String[]>>();
            foreach ( host h in this.m_HostList ) {
                Task<String[]> t = new Task<String[]>(n => GetDirectoriesInNetworkLocation(n.ToString()), h.hostip);
                t.Start();
                tasks.Add(t);
            }
            // wait for completion of the tasks: needs some precaution to avoid messing things up (disable node) but allows Cancel & Progress
            List<TreeNode> tnl = new List<TreeNode>();
            int currCount = 1;
            do {
                for ( int i = 0; i < tasks.Count; i++ ) {
                    Application.DoEvents();
                    if ( tasks[i].IsCompleted ) {
                        // get returned data from thread
                        String[] si = tasks[i].Result;
                        // add a network workstation node, first find its literal machine name
                        string ipadd = si[0];
                        host hst = this.m_HostList.Find(o => o.hostip == ipadd);
                        TreeNode wsn = new TreeNode(hst.hostname, 6, 6);
                        wsn.Tag = ipadd;
                        tnl.Add(wsn);
                        // add to workstation node subnodes (if any) with the network shares
                        int entries = si.Length;
                        for ( int r = 1; r < entries; r++ ) {
                            Application.DoEvents();
                            string sharename = si[r];
                            TreeNode subTn = tnl[tnl.Count - 1].Nodes.Add(sharename);
                            subTn.Tag = sharename;
                        }
                        // show what we have so far
                        tn.Nodes.Add(wsn);
                        if ( !tn.IsExpanded ) {
                            tn.Expand();
                        }
                        tn.Nodes[tn.Nodes.Count - 1].EnsureVisible();
                        // show progress whenever a task is completed
                        if ( phandler != null ) {
                            phandler(null, new NetworkScanEventArgs(currCount++, this.m_HostList.Count));
                        }
                        // remove the just completed task from the tasklist
                        tasks.RemoveAt(i);
                    }
                    if ( this.Break ) {
                        break;
                    }
                }
            } while ( (tasks.Count > 0) && !this.Break );

            // sort the list of nodes: Lambda expression with nameless delegate instead of a separate IComparer class
            tnl.Sort((x, y) => string.Compare(x.Text, y.Text));
            // clear previously unsorted view
            tn.Nodes.Clear();
            // AddRange had allowed us sorting and is faster
            tn.Nodes.AddRange(tnl.ToArray());
            tn.Nodes[0].EnsureVisible();

            // if there are no network nodes, then we add a fake one: otherwise 'before expand' would never get called
            if ( tn.Nodes.Count == 0 ) {
                tn.Nodes.Add("-empty-");
            }
            // reset progress bar & text
            if ( phandler != null ) {
                phandler(null, new NetworkScanEventArgs(0, 0));
            }
            if ( handler != null ) {
                handler(null, new SelectionChangedEventArgs(this.SelectedPath));
            }
        }
        // NetShareEnum shall obtain network shares, works with Windows and Linux because it's actually SMB/Samba
        [DllImport("Netapi32.dll", SetLastError = true)]
        static extern int NetApiBufferFree(IntPtr Buffer);
        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int NetShareEnum(StringBuilder ServerName, int level, ref IntPtr bufPtr, uint prefmaxlen, ref int entriesread, ref int totalentries, ref int resume_handle);
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHARE_INFO_2 {
            public string shi2_netname;
            public uint shi2_type;
            public string shi2_remark;
            public uint shi2_permissions;
            public uint shi2_max_uses;
            public uint shi2_current_uses;
            public string shi2_path;
            public string shi2_passwd;
            public SHARE_INFO_2(string sharename, uint sharetype, string remark, uint permissions, uint max_uses, uint current_uses, string path, string pwd) {
                this.shi2_netname = sharename;
                this.shi2_type = sharetype;
                this.shi2_remark = remark;
                this.shi2_permissions = permissions;
                this.shi2_max_uses = max_uses;
                this.shi2_current_uses = current_uses;
                this.shi2_path = path;
                this.shi2_passwd = pwd;
            }
            public override string ToString() {
                return this.shi2_netname;
            }
        }
        public struct SHAREINFO {
            public SHARE_INFO_2 shi;
            public string ipaddress;
        }
        const uint MAX_PREFERRED_LENGTH = 0xFFFFFFFF;
        const int NERR_Success = 0;
        private enum NetError : uint {
            NERR_Success = 0,
            NERR_BASE = 2100,
            NERR_UnknownDevDir = (NERR_BASE + 16),
            NERR_DuplicateShare = (NERR_BASE + 18),
            NERR_BufTooSmall = (NERR_BASE + 23),
        }
        private enum SHARE_TYPE : uint {
            STYPE_DISKTREE = 0,
            STYPE_PRINTQ = 1,
            STYPE_DEVICE = 2,
            STYPE_IPC = 3,
            STYPE_SPECIAL = 0x80000000,
        }

        // exec cmd.exe with net view seems to be the fastest and most reliable solution (WMI is slow, NetApi32.NetShareEnum skips folders)
        private String[] GetDirectoriesInNetworkLocation(string Server) {
            // a hidden cmd window with redirections
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cmd.Start();
            cmd.StandardInput.WriteLine("net view " + Server);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            string output = cmd.StandardOutput.ReadToEnd();
            cmd.WaitForExit();
            cmd.Close();
            // retval collects the shares of a network device
            List<string> retval = new List<string>();
            // only process, if there are shares at all
            if ( output.Contains("Disk") ) {
                // take care, the path is allowed to contain "--", something we later search for
                string CurrentExecutePath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                output = output.Replace(CurrentExecutePath, "");
                // get to know the end of the ----- sequence
                int firstindex = output.LastIndexOf("--") + 2;
                // the index of the last share
                int lastindex = output.LastIndexOf("Disk");
                // useful info
                output = output.Substring(firstindex, lastindex - firstindex);
                // all share names are followed by a " Disk", remove them
                output = output.Replace("Disk", string.Empty);
                // finally split & trim
                retval = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList();
            }
            // at least return the network device
            retval.Insert(0, Server);
            return retval.ToArray();
        }
        public SHAREINFO[] EnumNetShares(string Server) {
            List<SHAREINFO> ShareInfos = new List<SHAREINFO>();
            int entriesread = 0;
            int totalentries = 0;
            int resume_handle = 0;
            int nStructSize = Marshal.SizeOf(typeof(SHARE_INFO_2));
            IntPtr bufPtr = IntPtr.Zero;
            StringBuilder server = new StringBuilder(Server);
            int ret = NetShareEnum(server, 2, ref bufPtr, MAX_PREFERRED_LENGTH, ref entriesread, ref totalentries, ref resume_handle);
            if ( ret == NERR_Success ) {
                IntPtr currentPtr = bufPtr;
                for ( int i = 0; i < entriesread; i++ ) {
                    SHARE_INFO_2 shi2 = (SHARE_INFO_2)Marshal.PtrToStructure(currentPtr, typeof(SHARE_INFO_2));
                    SHAREINFO shnfo = new SHAREINFO();
                    shnfo.shi = shi2;
                    shnfo.ipaddress = Server;
                    ShareInfos.Add(shnfo);
                    //Remember: 64-bit systems have 64-bit pointers. Using ToInt32 will cause an ArithmeticOverflow exception. Use ToInt64 if you need to.
                    if ( IntPtr.Size == 8 ) {
                        currentPtr = new IntPtr(currentPtr.ToInt64() + nStructSize);
                    } else {
                        currentPtr = new IntPtr(currentPtr.ToInt32() + nStructSize);
                    }
                }
                NetApiBufferFree(bufPtr);
            } else {
                SHARE_INFO_2 shi2 = new SHARE_INFO_2("", 10, "", 0, 0, 0, "", "");
                SHAREINFO shnfo = new SHAREINFO();
                shnfo.shi = shi2;
                shnfo.ipaddress = Server;
                ShareInfos.Add(shnfo);
            }
            return ShareInfos.ToArray();
        }

        // save & load of Network tree node
        void SaveTree(TreeNode node) {
            string filename = Path.Combine(Application.StartupPath, "cfwnetscan.bin");
            foreach ( TreeNode tn in node.Nodes ) {
                tn.ImageIndex = 6;
            }
            try {
                using ( Stream file = File.Open(filename, FileMode.Create) ) {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(file, node.Nodes.Cast<TreeNode>().ToList());
                }
            } catch ( Exception ) {; }
        }
        void LoadTree(TreeNode node) {
            string filename = Path.Combine(Application.StartupPath, "cfwnetscan.bin");
            if ( !File.Exists(filename) ) {
                return;
            }
            using ( Stream file = File.Open(filename, FileMode.Open) ) {
                BinaryFormatter bf = new BinaryFormatter();
                object obj = bf.Deserialize(file);
                TreeNode[] nodeList = (obj as IEnumerable<TreeNode>).ToArray();
                node.Nodes.Clear();
                foreach ( TreeNode tn in nodeList ) {
                    tn.ImageIndex = 6;
                }
                node.Nodes.AddRange(nodeList);
            }
        }

        // before a treeview node gets expanded, we want to populate its content
        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e) {
            // don't mess around
            if ( m_bDoNotDisturb ) {
                return;
            }
            m_bDoNotDisturb = true;

            // global breaker connected to the break button in Dialog
            this.Break = false;

            // get current selection from tree
            TreeNode nodeCurrent = e.Node;
            this.SelectedPath = getFullPath(nodeCurrent.FullPath, (string)nodeCurrent.Tag);

            //
            // "Network" at the end of 'SelectedPath' only happens in case, we expand the root "Network" node
            //
            if ( nodeCurrent.FullPath.EndsWith("Network") ) {
                // don't allow to to play with expand in case "before expand" is already running: applies only to network node expanding
                if ( SystemColors.GrayText == e.Node.ForeColor ) {
                    e.Cancel = true;
                    m_bDoNotDisturb = false;
                    return;
                }
                // load existing network scan from file
                this.LoadTree(nodeCurrent);
                // anytime there is something other than "-empty-" underneath Network AND networkautorefresh is off, then we want to ask for refresh as an option
                if ( nodeCurrent.Nodes[0].Text != "-empty-" ) {
                    // show simple Yes dialog
                    using ( SimpleYes sy = new SimpleYes() ) {
                        sy.Left = MousePosition.X - sy.Width / 2;
                        Point pt = this.PointToScreen(new Point(nodeCurrent.Bounds.X, nodeCurrent.Bounds.Y - 3));
                        sy.Top = pt.Y;
                        // move mouse into Yes dialog window: mouse leaves Yes dialog == Cancel dialog  
                        this.Cursor = new Cursor(Cursor.Current.Handle);
                        Cursor.Position = new Point(MousePosition.X, sy.Top + nodeCurrent.Bounds.Height / 2);
                        DialogResult dr = sy.ShowDialog();
                        if ( dr != DialogResult.Yes ) {
                            // this decision keeps/reuses the previously stored/loaded network tree
                            m_bDoNotDisturb = false;
                            return;
                        }
                    }
                }
                // refresh network information
                e.Node.ForeColor = SystemColors.GrayText;
                this.Break = false;
                this.PopulateNetwork(nodeCurrent);
                this.treeView1.SelectedNode = nodeCurrent;
                e.Node.ForeColor = SystemColors.WindowText;
                this.SaveTree(nodeCurrent);
                m_bDoNotDisturb = false;
                return;
            }

            //
            // load directories (regardless if local fs or network share) at one level deeper: this way treeview shows expandable folders AND/OR nonexpandable folders
            //
            if ( nodeCurrent.Checked ) {     // we abuse the node's (invisible) checked state: it is used as an indicator, whether the node was already read 
                m_bDoNotDisturb = false;
                return;
            }
            // organize progress
            EventHandler<NetworkScanEventArgs> phandler = NetworkScanProgress;
            if ( phandler != null ) {
                phandler(null, new NetworkScanEventArgs(0, nodeCurrent.Nodes.Count));
            }
            Cursor.Current = Cursors.WaitCursor;
            this.treeView1.BeginUpdate();
            m_run = !this.Break;

            /*
                        // sequential fill next level directories: VERY slow on ..\windows\winsxs  
                        foreach ( TreeNode tn in nodeCurrent.Nodes ) {
                            Application.DoEvents();
                            Cursor.Current = Cursors.WaitCursor;
                            PopulateDirectoryNextLevel(ref m_run, tn);
                            if ( phandler != null ) {
                                phandler(null, new NetworkScanEventArgs(++counter, 0));
                            }
                            if ( Break ) {
                                break;
                            }
                        }
            */
            //
            // get next level directories in parallel tasks  
            //
            //List<Task> tasks = new List<Task>();
            // start as many tasks as needed to check the directories in parallel 
            for ( int i = 0; i < nodeCurrent.Nodes.Count; i++ ) {
                TreeNode tn = nodeCurrent.Nodes[i];
                string path = getFullPath(tn.FullPath, (string)tn.Tag);
                if ( path != "" ) {
                    // run task in parallel thread, which might take a while - especially when a network share is mapped but not accessible
                    Task t = new Task(() => this.PopulateDirectoryNextLevel(ref m_run, tn));
                    t.Start();
                    //        tasks.Add(t);
                }
            }
            // cooperative loop until tasks are finished
            //do {
            //    Cursor.Current = Cursors.WaitCursor;
            //    Application.DoEvents();
            //    // loop tasks for completion
            //    for ( int i=0; i<tasks.Count; i++ ) {
            //        if ( tasks[i].IsCompleted ) {
            //            // remove finished task from tasklist
            //            tasks.RemoveAt(i);
            //            // progress
            //            if ( phandler != null ) {
            //                phandler(null, new NetworkScanEventArgs(++counter, 0));
            //                Cursor.Current = Cursors.WaitCursor;
            //                Application.DoEvents();
            //            }
            //        }
            //        if ( Break ) {
            //            break;
            //        }
            //    }
            //} while ( (tasks.Count>0) && !Break );

            // abuse of invisible node checked state: it is used as an indicator, whether the node was already read 
            nodeCurrent.Checked = true;

            // done with progress
            this.treeView1.EndUpdate();
            Cursor.Current = Cursors.Default;
            if ( phandler != null ) {
                phandler(null, new NetworkScanEventArgs(0, 0));
            }

            // an expanded node is a selected node too --> this node selection indirectly calls "treeView1_BeforeSelect"
            this.treeView1.SelectedNode = nodeCurrent;

            // all done
            m_bDoNotDisturb = false;
        }

        protected void PopulateDirectoryNextLevel(ref bool run, TreeNode nodeCurrent) {
            // avoids double read, if node is already populated: happens only at first start of dialog
            if ( nodeCurrent.Nodes.Count > 0 ) {
                return;
            }

            // 20161016: a non accesible network drive would slow down Directory.Exists(path) by cxa. 20s
            if ( nodeCurrent.ImageIndex == 9 ) {
                return;
            }

            // path 
            string path = "";
            try {
                path = getFullPath(nodeCurrent.FullPath, (string)nodeCurrent.Tag);

                // 20160312 - Network: skip not pingable network drives, aka a drive is mapped but not accessible 
                if ( nodeCurrent.ImageIndex == 6 ) {
                    string ip = (string)nodeCurrent.Tag;
                    if ( (ip != null) && (ip.Length > 0) ) {
                        if ( !GrzTools.Network.PingIpOk(ip) ) {
                            return;
                        }
                    } else {
                        if ( (path.Length > 1) && !GrzTools.Network.PingNetDriveOk(path/*.Substring(0, 2)*/) ) {
                            return;
                        }
                    }
                }

                // ROM: if we don't check existence, it may happen that not accessible ROM complains about non existing media
                if ( (nodeCurrent.ImageIndex == 7) && !Directory.Exists(path) ) {
                    return;
                }
            } catch ( Exception ) {
                return;
            }

            // get string array with next level directories: this way we are always 1 level ahead of what the tree shows
            List<string> stringDirectories = this.m_fff.FindFolders(ref run, path);
            TreeNode[] arr = new TreeNode[stringDirectories.Count];
            // loop thru found directories
            for ( int i = 0; i < stringDirectories.Count; i++ ) {
                string stringDir = stringDirectories[i];
                // create node for directory: Text = literal share name / Tag = real name of the share
                TreeNode nodeDir;
                nodeDir = new TreeNode(stringDir, 0, 3);
                nodeDir.Tag = (string)nodeCurrent.Tag;
                arr[i] = nodeDir;
                if ( !run ) {
                    break;
                }
            }
            // sort next level items
            Array.Sort(arr, (a, b) => String.Compare(a.Text, b.Text));
            // AddRange is much faster as the straight forward and simple Add 
            if ( this.InvokeRequired ) {
                this.Invoke(new Action(() => { nodeCurrent.Nodes.AddRange(arr); }));
            } else {
                try {
                    nodeCurrent.Nodes.AddRange(arr);
                } catch ( Exception ) {
                    this.Invoke(new Action(() => { nodeCurrent.Nodes.AddRange(arr); }));
                }
            }
        }

        // before a treeview node gets selected, we want to populate the listview content with folders & files
        private void treeView1_BeforeSelect(object sender, TreeViewCancelEventArgs e) {
            // don't allow to play with expand in case "before expand" is already running: this case is flagged via different ForeColor property
            if ( SystemColors.GrayText == e.Node.ForeColor ) {
                e.Cancel = true;
                return;
            }
            this.Break = false;
            // get current selected drive or folder
            TreeNode nodeCurrent = e.Node;
            // update SelectedPath accordingly
            this.SelectedPath = getFullPath(nodeCurrent.FullPath, (string)nodeCurrent.Tag);
            EventHandler<SelectionChangedEventArgs> handler = SelectionChanged;
            if ( handler != null ) {
                handler(sender, new SelectionChangedEventArgs(this.SelectedPath));
            }
            // ignore top level network devices having no shares from further processing (it sometimes lags)
            if ( nodeCurrent.FullPath.Count(f => f == '\\') == 2 && nodeCurrent.FirstNode == null ) {
                return;
            }
            // populate right hand side window with folders & files
            this.PopulateFiles(nodeCurrent, true);
        }
        protected void PopulateFiles(TreeNode nodeCurrent, bool noFlicker) {
            if ( (nodeCurrent == this.treeView1.SelectedNode) && noFlicker ) {
                return;
            }
            if ( (this.listView1.Tag == nodeCurrent) && noFlicker ) {
                return;
            }
            if ( nodeCurrent.ImageIndex == 9 ) {
                return;
            }
            this.listView1.Tag = nodeCurrent;
            string ip = (string)nodeCurrent.Tag;
            string fullpath = getFullPath(nodeCurrent.FullPath, ip);
            if ( nodeCurrent.ImageIndex == 6 ) { // network drive mapped but perhaps not accessible
                if ( (ip != null) && (ip.Length > 0) ) {
                    if ( !GrzTools.Network.PingIpOk(ip) ) {
                        this.listView1.Items.Clear();
                        return;
                    }
                } else {
                    if ( (fullpath.Length > 1) && !GrzTools.Network.PingNetDriveOk(fullpath/*.Substring(0, 2)*/) ) {
                        this.listView1.Items.Clear();
                        return;
                    }
                }
            }
            if ( fullpath.Length == 0 ) { // click on "Network" shows otherwise the content of windows' current folder
                this.listView1.Items.Clear();
                this.listView1.Columns[0].Text = "Name";
                this.listView1.Columns[1].Text = "Date";
                this.listView1.Columns[1].TextAlign = HorizontalAlignment.Left;
                this.listView1.Columns[2].Text = "Size";
                this.listView1.Columns[2].TextAlign = HorizontalAlignment.Left;
                this.listView1.Columns[3].Text = "";
                this.listView1.Columns[4].Text = "";
                this.listView1.Columns[5].Text = "";
                this.listView1.Columns[6].Text = "";
                this.listView1.Columns[3].Width = 0;
                this.listView1.Columns[4].Width = 0;
                this.listView1.Columns[5].Width = 0;
                this.listView1.Columns[6].Width = 0;
                return;
            }
            Cursor.Current = Cursors.WaitCursor;
            this.listView1.BeginUpdate();
            this.listView1.Items.Clear();
            if ( fullpath != "My Computer" ) {
                // we simply show all files and folders underneath the selected fullpath
                this.listView1.Columns[0].Text = "Name";
                this.listView1.Columns[1].Text = "Date";
                this.listView1.Columns[1].TextAlign = HorizontalAlignment.Left;
                this.listView1.Columns[2].Text = "Size";
                this.listView1.Columns[2].TextAlign = HorizontalAlignment.Left;
                this.listView1.Columns[3].Text = "";
                this.listView1.Columns[4].Text = "";
                this.listView1.Columns[5].Text = "";
                this.listView1.Columns[6].Text = "";
                // ROM: if we don't check existence, it may happen that not accessible ROM complains about a non existing media
                bool bSort = true;
                if ( (nodeCurrent.ImageIndex == 7) && !Directory.Exists(fullpath) ) {
                    bSort = false;
                }
                if ( bSort ) {
                    ListViewItem[] lvarr = this.m_fff.FindFoldersFiles(fullpath).ToArray();
                    Array.Sort(lvarr, (a, b) => String.Compare(a.Tag.ToString(), b.Tag.ToString()));  // sort by Tag!!!
                    this.listView1.Items.AddRange(lvarr);
                }
                this.listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                this.listView1.Columns[3].Width = 0;
                this.listView1.Columns[4].Width = 0;
                this.listView1.Columns[5].Width = 0;
                this.listView1.Columns[6].Width = 0;
            } else {
                // in "My Computer" we show all local drives + total size + free space + percentage
                this.listView1.Columns[0].Text = "Drive";
                this.listView1.Columns[1].Text = "Size";
                this.listView1.Columns[1].TextAlign = HorizontalAlignment.Right;
                this.listView1.Columns[2].Text = "Free";
                this.listView1.Columns[2].TextAlign = HorizontalAlignment.Right;
                this.listView1.Columns[3].Text = "Free %";
                this.listView1.Columns[3].TextAlign = HorizontalAlignment.Right;
                this.listView1.Columns[4].Text = "Location";
                this.listView1.Columns[5].Text = "FS";
                this.listView1.Columns[6].Text = "Name";
                ListViewItem[] lvarr = this.GetDriveInfo().ToArray();
                this.listView1.Items.AddRange(lvarr);
                this.listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
            this.listView1.EndUpdate();
            Cursor.Current = Cursors.Default;
        }

        // listview selection causes an update of SelectedPath
        private void listView1_SelectedIndexChanged(object sender, EventArgs e) {
            if ( this.listView1.SelectedItems.Count > 0 ) {
                // update SelectedPath accordingly
                string basePath = getFullPath(this.treeView1.SelectedNode.FullPath, (string)this.treeView1.SelectedNode.Tag);
                // 20160312: skip mapped but not accessible network shares 
                if ( this.treeView1.SelectedNode.ImageIndex == 9 ) {
                    this.SelectedPath = "";
                    this.SelectedFile = "";
                    return;
                }
                // 20160312: skip mapped but not accessible network shares 
                if ( this.listView1.SelectedItems[0].ImageIndex == 9 ) {
                    this.SelectedPath = "";
                    this.SelectedFile = "";
                    return;
                }
                // there are cases, when a trailing \\ is missing: happens if one tries to Path.Combine a drive F: and a path
                if ( basePath.Length == 2 ) {
                    if ( (basePath[1] == ':') ) {
                        basePath += "\\";
                    }
                }
                string path = Path.Combine(basePath, this.listView1.SelectedItems[0].Text);
                if ( Directory.Exists(path) ) {
                    this.SelectedPath = path;
                    this.SelectedFile = "";
                } else {
                    this.SelectedPath = Path.GetDirectoryName(path);
                    this.SelectedFile = path;
                }
                // inform subscriber about update of SelectedPath
                EventHandler<SelectionChangedEventArgs> handler = SelectionChanged;
                if ( (handler != null) && (this.SelectedPath != null) ) {
                    handler(sender, new SelectionChangedEventArgs(this.SelectedPath));
                }
            }
        }
        // listview item double click shall act like to expand a treeeview node
        private void listView1_DoubleClick(object sender, EventArgs e) {
            // don't allow to to play here in case "before expand" is already running
            if ( m_bDoNotDisturb ) {
                return;
            }

            ListViewItem lvi = this.listView1.SelectedItems[0];

            // 20160414: if a file was double clicked, we change to the containing folder
            string filePath = "";
            string fileName = "";
            if ( lvi != null ) {
                lvi.Selected = true;
                string selTxt = lvi.Text;
                if ( (selTxt.Length == 2) && (selTxt[1] == ':') ) {
                    fileName = selTxt;
                } else {
                    filePath = getFullPath(this.treeView1.SelectedNode.FullPath, (string)this.treeView1.SelectedNode.Tag);
                    fileName = Path.Combine(filePath, selTxt);
                }
            }
            if ( File.Exists(fileName) ) {
                this.SelectedPath = Path.GetDirectoryName(fileName);
                EventHandler<WantCloseEventArgs> handler = WantClose;
                if ( handler != null ) {
                    handler(sender, new WantCloseEventArgs(this.SelectedPath));
                }
                return;
            }

            // a folder was double clicked
            TreeNode sn = this.treeView1.SelectedNode;
            sn.Expand();
            TreeNode nn = null;
            foreach ( TreeNode tn in sn.Nodes ) {
                if ( lvi.Text == tn.Text ) {
                    nn = tn;
                    break;
                }
            }
            if ( nn != null ) {
                try {
                    nn.Expand();
                    this.treeView1.SelectedNode = nn;
                } catch ( Exception ) {; }
            }
        }

        // translate literal folder names into OS understandable folders
        protected static string getFullPath(string stringPath, string tag) {
            string stringParse = "";

            // remove My Computer from path.
            stringParse = stringPath.Replace("My Computer\\", "");

            // replace Desktop with its real path
            if ( stringParse.StartsWith("Desktop") ) {
                stringParse = stringParse.Replace("Desktop", Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            }

            // replace Documents with its real path
            if ( stringParse.StartsWith("Documents") ) {
                stringParse = stringParse.Replace("Documents", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            }

            // replace Downloads with its real path
            if ( stringParse.StartsWith("Downloads") ) {
                string downloads;
                try {
                    SHGetKnownFolderPath(guid_Downloads, 0, IntPtr.Zero, out downloads);
                    downloads = downloads.ReplaceAt(0, Char.ToUpper(downloads[0]));
                } catch ( Exception ) {
                    string pathUserDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    downloads = Path.Combine(pathUserDocs, "Downloads");
                }
                stringParse = stringParse.Replace("Downloads", downloads);
            }

            // a) simple remove Network node only   b) leave hostname and subsequent shares/subfolders untouched
            if ( stringParse.StartsWith("Network") ) {
                stringParse = stringParse.Replace("Network", "");
                // we completely remove the sharepath from node.Text: format 'workstation\\sharename (sharepath)', sample '\\WS\egal (C:\egal)'
                string[] split = stringParse.Split('\\');
                if ( split.Length > 1 ) {
                    stringParse = "";
                    for ( int i = 0; i < split.Length; i++ ) {
                        // if we find the pattern 'tag (C:' we replace it with the tag [remember: tag is the sharename]
                        if ( split[i].StartsWith(tag) && split[i].EndsWith(":") ) {
                            stringParse += "\\" + tag;
                            // now we search the end of the share path indicated by ")", all i in between we skip
                            for ( int j = i; j < split.Length; j++ ) {
                                if ( split[j].EndsWith(")") ) {
                                    i = j;
                                    break;
                                }
                            }
                        } else {
                            stringParse += "\\" + split[i];
                        }
                    }
                }
            }

            return stringParse;
        }

        // apply >=Win7 themes
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        public extern static int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);
        class TreeView : System.Windows.Forms.TreeView {
            protected override void OnHandleCreated(EventArgs e) {
                base.OnHandleCreated(e);

                if ( !this.DesignMode && Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 6 ) {
                    SetWindowTheme(this.Handle, "explorer", null);
                }
            }
        }
        class ListView : System.Windows.Forms.ListView {
            protected override void OnHandleCreated(EventArgs e) {
                base.OnHandleCreated(e);

                if ( !this.DesignMode && Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 6 ) {
                    SetWindowTheme(this.Handle, "explorer", null);
                }
            }
        }

        [DllImport("winmm.dll", EntryPoint = "mciSendString")]
        public static extern int mciSendStringA(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);
        private void treeView1_DoubleClick(object sender, EventArgs e) {
            // what treenode was clicked on
            TreeNode sn = this.treeView1.SelectedNode;

            // reduce number of hits here: there's nothing to do, if selected treenode is already expandable (like normal drives)
            if ( sn.Nodes.Count > 0 ) {
                return;
            }

            // ask if CD tray shall get opened
            string driveName = sn.Text;
            string driveLetter = driveName.Substring(0, 1);
            DriveInfo di = null;
            try {
                di = new DriveInfo(driveLetter);
            } catch {
                return;
            }

            if ( (di.DriveType == DriveType.CDRom) && (di.Name.Contains(driveName)) ) {
                if ( !di.IsReady ) {
                    // CD not ready
                    CDOpenClose cddlg = new CDOpenClose();
                    DialogResult dr = cddlg.ShowDialog();
                    if ( dr == DialogResult.Yes ) {
                        // CD open & exit
                        mciSendStringA("open " + driveLetter + ": type CDAudio alias drive" + driveLetter, null, 0, 0);
                        mciSendStringA("set drive" + driveLetter + " door open", null, 0, 0);
                        return;
                    }
                    if ( dr == DialogResult.No ) {
                        // CD close & exit
                        mciSendStringA("open " + driveLetter + ": type CDaudio alias drive" + driveLetter, null, 0, 0);
                        mciSendStringA("set drive" + driveLetter + " door closed", null, 0, 0);
                        return;
                    }
                    if ( dr == DialogResult.Cancel ) {
                        // just exit
                        return;
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
                                return;
                            }
                        } else {
                            return;
                        }
                    }
                }
            }

            // if there is nothing to expand anymore, so we could return the selected item and close the dialog 
            TreeNode selectedNode = sn;
            if ( selectedNode != null ) {
                this.treeView1.SelectedNode = selectedNode;
                this.SelectedPath = getFullPath(selectedNode.FullPath, (string)selectedNode.Tag);
            }
            EventHandler<WantCloseEventArgs> handler = WantClose;
            if ( handler != null ) {
                handler(sender, new WantCloseEventArgs(this.SelectedPath));
            }
        }

    }

    // string extension method
    public static class Extensions {
        public static string ReplaceAt(this string input, int index, char newChar) {
            if ( input == null ) {
                throw new ArgumentNullException("input");
            }
            char[] chars = input.ToCharArray();
            chars[index] = newChar;
            return new string(chars);
        }
    }

}
