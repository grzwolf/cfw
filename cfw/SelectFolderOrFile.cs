using System;
using System.ComponentModel;
using System.IO;                           // Path
using System.Threading;                    // sleep   
using System.Windows.Forms;

namespace cfw {
    public partial class SelectFolderOrFile : Form {
        string m_DefaultPath = "";
        bool m_FolderHistoryMode = false;

        public string ReturnPath { get; set; }
        public string ReturnFile { get; set; }

        public string DefaultPath {
            set {
                this.m_DefaultPath = value;
                this.textBoxOutput.Text = value;
                this.fileSystemTreeView1.DefaultPath = value;
            }
        }

        public SelectFolderOrFile() {
            this.InitializeComponent();
            this.ReturnPath = "";
            this.ReturnFile = "";

            this.fileSystemTreeView1.WantClose += new EventHandler<FileSystemTreeView.WantCloseEventArgs>(this.fileSystemTreeView1_WantClose);
            this.fileSystemTreeView1.SelectionChanged += new EventHandler<FileSystemTreeView.SelectionChangedEventArgs>(this.fileSystemTreeView1_SelectionChanged);
            this.fileSystemTreeView1.NetworkScanProgress += new EventHandler<FileSystemTreeView.NetworkScanEventArgs>(this.fileSystemTreeView1_NetworkScanProgress);

            this.progressBar1.Step = 1;
            this.progressBar1.Value = 0;

            GrzTools.IniFile ini = new GrzTools.IniFile(System.Windows.Forms.Application.ExecutablePath + ".ini");
            for ( int i = 0; i < 5; i++ ) {
                string tmp = ini.IniReadValue("cfw", "selectFolder" + i.ToString(), "");
                if ( tmp.Length > 0 ) {
                    this.comboBoxFolderHistory.Items.Add(tmp);
                }
            }
            if ( this.comboBoxFolderHistory.Items.Count > 0 ) {
                this.comboBoxFolderHistory.SelectedIndex = 0;
            }

            // add left double click behaviour to combobox' textbox
            this.comboBoxFolderHistory.Tag = DateTime.MinValue;
        }

        public bool AutoNetworkScan {
            get {
                return this.fileSystemTreeView1.AutoNetworkScan;
            }
            set {
                this.fileSystemTreeView1.AutoNetworkScan = value;
            }
        }

        // force refresh by parent (after media change) via call thru public method: delayed execution via timer was needed due to strange exceptions
        public void RefreshRequest(string msg) {
            this.timerDelayedRefresh.Start();
        }
        private void timerDelayedRefresh_Tick(object sender, EventArgs e) {
            this.timerDelayedRefresh.Stop();
            this.buttonRefresh_Click(null, null);
        }

        // network scanning (windows\winsxs too) may take a long time
        private void timerShowBreakButton_Tick(object sender, EventArgs e) {
            this.timerShowBreakButton.Stop();
            this.buttonBreak.Visible = true;
        }
        private void fileSystemTreeView1_NetworkScanProgress(object sender, FileSystemTreeView.NetworkScanEventArgs nsea) {
            if ( nsea.Current == 0 ) {
                this.progressBar1.Maximum = (int)nsea.Maximal;
                this.buttonNewFolder.Enabled = false;
                this.buttonRefresh.Enabled = false;
                this.buttonSelect.Enabled = false;
                this.timerShowBreakButton.Start();
                if ( (nsea.Current == 0) && (nsea.Maximal == 0) ) {
                    this.timerShowBreakButton.Stop();
                    this.buttonBreak.Visible = false;
                    this.buttonNewFolder.Enabled = true;
                    this.buttonRefresh.Enabled = true;
                    this.buttonSelect.Enabled = true;
                }
            } else {
                try {
                    this.progressBar1.Value = (int)nsea.Current;
                } catch ( Exception ) {; }
            }
        }

        // fileSystemTreeView sent a message to select&close the dialog, happens when user right clicks an item 
        private void fileSystemTreeView1_WantClose(object sender, FileSystemTreeView.WantCloseEventArgs wcea) {
            this.m_FolderHistoryMode = false;

            if ( !this.buttonSelect.Enabled ) {
                return;
            }

            this.fileSystemTreeView1.Break = true;
            this.ReturnPath = this.fileSystemTreeView1.SelectedPath;
            if ( this.textBoxOutput.Text.StartsWith("My Computer") ) {
                //                ReturnPath = m_DefaultPath;
            }
            this.ReturnFile = this.fileSystemTreeView1.SelectedFile;
            if ( !File.Exists(this.ReturnFile) ) {
                this.ReturnFile = "";
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // current selected path/file was chaged by fileSystemTreeView
        private void fileSystemTreeView1_SelectionChanged(object sender, FileSystemTreeView.SelectionChangedEventArgs scea) {
            this.m_FolderHistoryMode = false;

            this.textBoxOutput.Text = scea.Selection;
            if ( scea.Selection.Length == 0 ) {
                this.textBoxOutput.Text = this.m_DefaultPath;
            }
        }

        // select and close
        private void buttonSelect_Click(object sender, EventArgs e) {
            this.ReturnPath = this.textBoxOutput.Text;
            this.ReturnFile = this.fileSystemTreeView1.SelectedFile;
            if ( this.m_FolderHistoryMode ) {
                if ( !this.comboBoxFolderHistory.Items.Contains(this.textBoxOutput.Text) ) {
                    this.comboBoxFolderHistory.Items.Insert(0, this.textBoxOutput.Text);
                }
            }
        }

        // save folder history to ini
        private void saveFolderHistory() {
            GrzTools.IniFile ini = new GrzTools.IniFile(System.Windows.Forms.Application.ExecutablePath + ".ini");
            for ( int i = 0; i < 5; i++ ) {
                string tmp = "";
                if ( i < this.comboBoxFolderHistory.Items.Count ) {
                    tmp = (string)this.comboBoxFolderHistory.Items[i];
                } else {
                    tmp = null;
                }
                ini.IniWriteValue("cfw", "selectFolder" + i.ToString(), tmp);
            }
        }

        // create a new folder
        private void buttonNewFolder_Click(object sender, EventArgs e) {
            this.m_FolderHistoryMode = false;

            if ( this.textBoxOutput.Text.StartsWith("My Computer") ) {
                return;
            }

            string newFolder = Microsoft.VisualBasic.Interaction.InputBox("Type in the name for the new folder.", "New Folder", "New Folder");
            newFolder = Path.Combine(this.textBoxOutput.Text, newFolder);

            if ( Directory.Exists(newFolder) ) {
                MessageBox.Show("Selected folder name already exists: " + newFolder, "Error");
                return;
            }

            try {
                Directory.CreateDirectory(newFolder);
                string newNodeName = Path.GetFileName(newFolder);
                this.fileSystemTreeView1.Refresh(newNodeName);
            } catch ( Exception ) {
                ;
            }
        }

        // close dialog
        private void buttonCancel_Click(object sender, EventArgs e) {
            this.ReturnPath = "";
            this.ReturnFile = "";
            this.fileSystemTreeView1.Break = true;
            Thread.Sleep(100);
        }
        protected override bool ProcessDialogKey(Keys keyData) {
            // ET shall close dlg and forward current selection
            if ( Form.ModifierKeys == Keys.None && keyData == Keys.Enter ) {
                this.buttonSelect_Click(null, null);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            if ( Form.ModifierKeys == Keys.None && keyData == Keys.Escape ) {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        // break current search processes
        private void buttonBreak_Click(object sender, EventArgs e) {
            this.m_FolderHistoryMode = false;
            this.fileSystemTreeView1.Break = true;
            Thread.Sleep(200);
            this.buttonBreak.Visible = false;
        }

        // refresh treeview and listview
        private void buttonRefresh_Click(object sender, EventArgs e) {
            this.m_FolderHistoryMode = false;
            if ( sender == null ) {
                this.fileSystemTreeView1.Refresh("mediachanged");
            } else {
                this.fileSystemTreeView1.Refresh("");
            }
        }

        private void buttonMapFolder_Click(object sender, EventArgs e) {
            this.m_FolderHistoryMode = false;
            string sNetDrive = "";
            if ( this.textBoxOutput.Text.StartsWith("\\\\") ) {
                sNetDrive = this.textBoxOutput.Text;
            }
            NetworkMapping nmdlg = new NetworkMapping(sNetDrive);
            DialogResult dlr = nmdlg.ShowDialog(this);
            nmdlg.Dispose();
        }

        private void buttonNetwork_Click(object sender, EventArgs e) {
            this.m_FolderHistoryMode = false;
            this.buttonNetwork.Text = "Network";
            this.fileSystemTreeView1.DefaultIP = "";
            IpEtc dlg = new IpEtc();
            dlg.ShowDialog();
            string ip = dlg.ReturnValueIpString;
            if ( ip.Length > 0 ) {
                this.buttonNetwork.Text = ip;
                this.fileSystemTreeView1.DefaultIP = ip;
            }
        }

        // if textbox gets active focus, there's something to memorize for the history
        private void textBoxOutput_Enter(object sender, EventArgs e) {
            this.m_FolderHistoryMode = true;
        }

        // maintain folder history combobox
        private void toolStripMenuItemTakeIt_Click(object sender, EventArgs e) {
            this.useComboboBoxItem();
        }
        private void useComboboBoxItem() {
            if ( this.comboBoxFolderHistory.Items.Count == 0 ) {
                return;
            }
            if ( this.comboBoxFolderHistory.SelectedIndex < 0 ) {
                return;
            }
            this.textBoxOutput.Text = (string)this.comboBoxFolderHistory.Items[this.comboBoxFolderHistory.SelectedIndex];
            this.comboBoxFolderHistory.Items.RemoveAt(this.comboBoxFolderHistory.SelectedIndex);
            this.comboBoxFolderHistory.Items.Insert(0, this.textBoxOutput.Text);
            this.comboBoxFolderHistory.SelectedIndex = 0;
        }
        private void toolStripMenuItemInsert_Click(object sender, EventArgs e) {
            String clp = Clipboard.ContainsText() ? Clipboard.GetText() : "";
            if ( !GrzTools.FileTools.PathExists(clp, 500) ) {
                return;
            }
            this.comboBoxFolderHistory.Text = clp;
            string newItem = this.comboBoxFolderHistory.Text;
            if ( !this.comboBoxFolderHistory.Items.Contains(newItem) ) {
                this.comboBoxFolderHistory.Items.Insert(0, newItem);
                this.comboBoxFolderHistory.SelectedIndex = 0;
            }
        }
        private void toolStripMenuItemDelete_Click(object sender, EventArgs e) {
            if ( this.comboBoxFolderHistory.Items.Count == 0 ) {
                return;
            }
            if ( this.comboBoxFolderHistory.SelectedIndex < 0 ) {
                return;
            }
            this.comboBoxFolderHistory.Items.RemoveAt(this.comboBoxFolderHistory.SelectedIndex);
            this.comboBoxFolderHistory.SelectedIndex = this.comboBoxFolderHistory.Items.Count > 0 ? 0 : -1;
        }
        private void toolStripMenuItemDeleteAll_Click(object sender, EventArgs e) {
            this.comboBoxFolderHistory.Items.Clear();
            this.comboBoxFolderHistory.ResetText();
            this.comboBoxFolderHistory.SelectedIndex = -1;
        }
        // the textbox of a combobox normally has no doublclick event - here we go: set in Constructor --> this.comboBoxFolderHistory.Tag = DateTime.MinValue;
        private void comboBoxFolderHistory_MouseDown(object sender, MouseEventArgs e) {
            if ( e.Button == System.Windows.Forms.MouseButtons.Left ) {
                if ( (DateTime)(this.comboBoxFolderHistory.Tag) > DateTime.MinValue ) {
                    int ms = (DateTime.Now - ((DateTime)(this.comboBoxFolderHistory.Tag))).Milliseconds;
                    if ( ms < 200 ) {
                        // double click event
                        if ( this.comboBoxFolderHistory.Items.Count > 0 ) {
                            this.useComboboBoxItem();
                        }
                    }
                }
                this.comboBoxFolderHistory.Tag = new DateTime(0);
            }
        }
        private void comboBoxFolderHistory_MouseUp(object sender, MouseEventArgs e) {
            if ( e.Button == System.Windows.Forms.MouseButtons.Left ) {
                if ( (DateTime)(this.comboBoxFolderHistory.Tag) == DateTime.MinValue ) {
                    this.comboBoxFolderHistory.Tag = DateTime.Now;
                }
            }
        }
        private void comboBoxFolderHistory_SelectedIndexChanged(object sender, EventArgs e) {
            this.comboBoxFolderHistory.Tag = new DateTime(0);
        }

        private void SelectFolderOrFile_FormClosing(object sender, FormClosingEventArgs e) {
            this.saveFolderHistory();
        }

        private void contextMenuStripComboBoxFolderHistory_Opening(object sender, CancelEventArgs e) {
            if ( this.comboBoxFolderHistory.Items.Count == 0 ) {
                this.toolStripMenuItemDelete.Enabled = false;
                this.toolStripMenuItemDeleteAll.Enabled = false;
                this.toolStripMenuItemTakeIt.Enabled = false;
            } else {
                this.toolStripMenuItemDelete.Enabled = true;
                this.toolStripMenuItemDeleteAll.Enabled = true;
                this.toolStripMenuItemTakeIt.Enabled = true;
            }
            this.toolStripMenuItemInsert.Enabled = true;
            String clp = Clipboard.ContainsText() ? Clipboard.GetText() : "";
            if ( !GrzTools.FileTools.PathExists(clp, 500) ) {
                this.toolStripMenuItemInsert.Enabled = false;
            }
        }

        private void comboBoxFolderHistory_KeyDown(object sender, KeyEventArgs e) {
            if ( e.KeyCode == Keys.Enter ) {
                string newItem = this.comboBoxFolderHistory.Text;
                if ( !this.comboBoxFolderHistory.Items.Contains(newItem) ) {
                    this.comboBoxFolderHistory.Items.Insert(0, newItem);
                    this.comboBoxFolderHistory.SelectedIndex = 0;
                }
                this.useComboboBoxItem();
                this.ReturnPath = this.textBoxOutput.Text;
            }
        }

    }
}
