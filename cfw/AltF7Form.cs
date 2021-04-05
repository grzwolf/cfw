using GrzTools;
using Microsoft.VisualBasic.FileIO;                 // FileSystem  !! Add Reference --> Reference Manager --> expand Assemblies --> select Framework --> Microsoft.VisualBasic
using Microsoft.Win32;                              // registry
using System;
using System.Collections;                           // ListView sorting
using System.Collections.Generic;
using System.Collections.Specialized;               // StringCollection 
using System.ComponentModel;
using System.Diagnostics;                           // Process 
using System.Drawing;
using System.IO;                                    // Path
using System.Linq;
using System.Runtime.InteropServices;               // DLLImport
using System.Text.RegularExpressions;               // regex 
using System.Threading.Tasks;                       // tasks
using System.Windows.Forms;

namespace cfw {
    public partial class AltF7Form : Form, IMessageFilter {
        struct Fn {
            public Fn(string f, string l) {
                this.filename = f;
                this.line = l;
            }
            public string filename;
            public string line;
        }

        readonly List<Fn> m_fn = new List<Fn>();                                 // list of filenames & linenumbers stored in a parallel to allow jumping/linking to file
        List<string> m_fList;                                           // double use: a) list of file to proceed     b) holds filenames of files when a buzzword was found 
        int m_iRun = 0;                                                 // signal to interrupt processes, 3 state flag: -1=skip current file and continue to run; 0=break; 1=run
        string m_buttonOriTxtRUN;                                       // original RUN button text 
        string m_original;                                              // ori search folders/drives
        readonly string m_buttonTxtBrk = "- break -";                            // RUN button break text  
        string[] m_buzzword;                                            // array with buzzwords
        int m_iLastFoundPos = 0;                                        // last find position in output window
        bool m_bCancelCopy = false;                                     // have a chance to cancle the copy progress
        string m_selectedPath;                                          // the folder to copy to, identical to passive folder from cfw
        string m_sLastStorePath = Application.ExecutablePath;           // last path where a file was stored
        int m_GlobalHitCounter = 0;                                     // hit counter 
        string m_defaultSkippedFolders = "";                            // obvious
        DateTime m_begDate;                                             // two date filters for found files 
        DateTime m_endDate;
        Int64 m_sizeLimit;                                              // size filter for found files 
        bool m_bFileSkipper = false;                                    // in case date/size filter wants to skip a file, this would be an indicator 
        MyListViewColumnSorter[] m_lvcs;                                // list sorter for 5 columns
        string m_sStartFolder = "";                                     // initial start folder
        bool m_bInitIsOver = false;                                     // init is over
        bool m_bFakeMaximumActive = true;                               // initially max of progressBarTotal is unknown, we set it 'faked' to 1000
        bool m_bSkipBin = false;                                        // ref to skip binary file while text search 

        int m_iFolderCountBeforeMaximumIsKnown = 0;                     // progress: inspected folders raise events even before the total amount of folders is known
        int m_iTrueFolderCountBeforeMaximumIsKnown = 0;
        int m_totalFolderCount = 0;
        int m_currentFolderCount = 0;
        string m_somethingToShow = "";

        public AltF7Form(string startpath, string passivepath) {
            this.InitializeComponent();
            this.InitForm(startpath, passivepath);

            this.previewCtl.SetPreviewFiles(true, true, true, true, true, true, true, true, true);
        }

        public void InitForm(string startpath, string passivepath) {
            // start path m_selectedPath = passivepath;
            this.textBoxProcessFile.Text = startpath;
            this.m_sStartFolder = startpath;

            this.checkBoxComputer.Checked = false;
            if ( this.textBoxProcessFile.Text == "Computer" ) {
                this.checkBoxComputer.Checked = true;
                this.checkBoxComputer_CheckedChanged(null, null);
                this.m_sStartFolder = @"C:\";
            }

            // passive path
            this.m_selectedPath = passivepath;

            // show what was shown last time
            this.ShowAdvancedOptions(this.checkBoxAdvancedOptions.Checked);

            // save run button original text
            this.m_buttonOriTxtRUN = this.buttonRun.Text;

            // Set KeyPreview object to true to allow the form to process the key before the control with focus processes it
            this.KeyPreview = true;

            // check buzzwords
            this.textBoxBuzzword_TextChanged();

            // listview sorter init
            this.m_lvcs = new MyListViewColumnSorter[this.listViewOutput.Columns.Count];
            for ( int i = 0; i < this.m_lvcs.Length; i++ ) {
                this.m_lvcs[i] = new MyListViewColumnSorter();
                this.m_lvcs[i].Order = SortOrder.None;
                this.m_lvcs[i].SortColumn = i;
            }

            // preset date range 
            if ( this.radioButtonDateAll.Checked ) {
                this.dateTimePickerStart.Value = this.dateTimePickerStart.MinDate;
                this.dateTimePickerStop.Value = DateTime.Today.AddDays(+1);
            }
            //this.dateTimePickerStart.Value = DateTime.Today;
            //this.dateTimePickerStop.Value = DateTime.Today.AddDays(1);

            // IMessageFilter
            // - also needed: class declaration "public partial class MainForm: Form, IMessageFilter"
            // - also needed: event handler "public bool PreFilterMessage( ref Message m )"
            // - also needed: Application.RemoveMessageFilter(this) when closing this form
            Application.AddMessageFilter(this);

            // fake progressBarTotal.Maximum 
            this.m_bFakeMaximumActive = true;
        }

        // IMessageFilter: intercept messages 
        public bool PreFilterMessage(ref Message m) {
            // 20160320: w/o this context menu from Alt-F7 opens always
            IntPtr fgw = GetForegroundWindow();
            if ( (this.WindowState == FormWindowState.Minimized) || (!this.Visible) || (fgw != this.Handle) ) {
                return false;
            }

            // right mouse down: select current item AND open context menu for listview output
            if ( (m.Msg == 0x204) && this.listViewOutput.Visible ) {
                // set focus
                this.listViewOutput.Focus();
                // open context menu
                this.contextMenuStripListView.Show(MousePosition);
                // prevents resetting the the current listview selection
                return true;
            }
            return false;
        }

        // what file/folder to process?
        private void OnButtonClickGetLocation(object sender, EventArgs e) {
            // self made dialog
            SelectFolderOrFile sff = new SelectFolderOrFile();
            sff.Text = "Select Folder or File";
            sff.DefaultPath = this.textBoxProcessFile.Text;
            if ( !Directory.Exists(this.textBoxProcessFile.Text) ) {
                sff.DefaultPath = this.m_sStartFolder;
            }
            DialogResult dlr = sff.ShowDialog(this);
            if ( dlr == System.Windows.Forms.DialogResult.OK ) {
                this.textBoxProcessFile.Text = sff.ReturnPath;
                if ( this.textBoxProcessFile.Text == "My Computer" ) {
                    this.checkBoxComputer.Checked = true;
                    this.checkBoxComputer_CheckedChanged(null, null);
                }
            }
            sff.Dispose();
        }

        // form closing
        private void AltF7Form_FormClosing(object sender, FormClosingEventArgs e) {
            // prevent too early closing in case something is still busy
            if ( this.m_iRun == 1 ) {
                e.Cancel = true;
                this.m_iRun = 0;
                return;
            }

            // IMessageFilter
            Application.RemoveMessageFilter(this);

            // save ini
            this.SaveINI();
        }
        // INI: write 
        void SaveINI() {
            GrzTools.IniFile ini = new GrzTools.IniFile(System.Windows.Forms.Application.ExecutablePath + ".ini");
            ini.IniWriteValue("cfw", "altF7pattern", this.textBoxFilePattern.Text);
            List<string> buzzlist = new List<string>();
            for ( int i = 0; i < 20; i++ ) {
                if ( i < this.comboBoxBuzzword.Items.Count ) {
                    ini.IniWriteValue("cfw", "altf7buzz" + i.ToString(), this.comboBoxBuzzword.GetItemText(this.comboBoxBuzzword.Items[i]));
                } else {
                    ini.IniWriteValue("cfw", "altf7buzz" + i.ToString(), null);
                }
            }
            ini.IniWriteValue("cfw", "altF7separator", this.textBoxSeparator.Text);
            ini.IniWriteValue("cfw", "altF7folderExcludes", this.textBoxFolderExcludes.Text);
            ini.IniWriteValue("cfw", "altF7folderContains", this.textBoxFolderPattern.Text);
            // skip default folders while search
            if ( this.textBoxFolderExcludes.Text != this.m_defaultSkippedFolders ) {
                this.m_defaultSkippedFolders = this.textBoxFolderExcludes.Text;
            }
            ini.IniWriteValue("cfw", "altF7defaultSkippedFolders", this.m_defaultSkippedFolders);
            ini.IniWriteValue("cfw", "ShowAdvancedOptions", this.checkBoxAdvancedOptions.Checked.ToString());
        }
        // read from INI on load form
        private void AltF7Form_Load(object sender, EventArgs e) {
            // INI: read
            GrzTools.IniFile ini = new GrzTools.IniFile(System.Windows.Forms.Application.ExecutablePath + ".ini");
            this.textBoxFilePattern.Text = ini.IniReadValue("cfw", "altF7pattern", "*.*");
            for ( int i = 0; i < 20; i++ ) {
                string tmp = ini.IniReadValue("cfw", "altf7buzz" + i.ToString(), "");
                if ( tmp.Length > 0 ) {
                    this.comboBoxBuzzword.Items.Add(tmp);
                }
            }
            if ( this.comboBoxBuzzword.Items.Count > 0 ) {
                this.comboBoxBuzzword.SelectedIndex = 0;
            }
            this.textBoxSeparator.Text = ini.IniReadValue("cfw", "altF7separator", ";");
            this.textBoxFolderExcludes.Text = ini.IniReadValue("cfw", "altF7folderExcludes", "");
            this.textBoxFolderPattern.Text = ini.IniReadValue("cfw", "altF7folderContains", "");
            // skip default folders while search
            this.m_defaultSkippedFolders = ini.IniReadValue("cfw", "altF7defaultSkippedFolders", "C:\\Windows;Recycle.Bin");
            if ( this.m_defaultSkippedFolders.Length == 0 ) {
                this.m_defaultSkippedFolders = "C:\\Windows;Recycle.Bin";
            }
            if ( this.textBoxFolderExcludes.Text.Contains(this.m_defaultSkippedFolders) ) {
                this.checkBoxSkip.Checked = true;
            }
            this.checkBoxAdvancedOptions.Checked = bool.Parse(ini.IniReadValue("cfw", "ShowAdvancedOptions", "FALSE"));
            this.ShowAdvancedOptions(this.checkBoxAdvancedOptions.Checked);

            this.m_bInitIsOver = true;
        }

        // output window context menu event handlers
        private void OnContextMenuOpen(object sender, CancelEventArgs e) {
            // 20160320: w/o this context menu from Alt-F7 opens always
            IntPtr fgw = GetForegroundWindow();
            if ( (this.WindowState == FormWindowState.Minimized) || (!this.Visible) || (fgw != this.Handle) ) {
                return;
            }

            // jump to behaviour
            try {
                int linenumber = this.textBoxOutput.GetLineFromCharIndex(this.textBoxOutput.SelectionStart);
                string filename = this.m_fn[linenumber].filename;
                string linendx = this.m_fn[linenumber].line;
                if ( File.Exists(filename) ) {
                    this.contextMenuStripOutput.Items[0].Enabled = true;
                } else {
                    this.contextMenuStripOutput.Items[0].Enabled = false;
                }
                if ( Directory.Exists(Path.GetDirectoryName(filename)) ) {
                    this.contextMenuStripOutput.Items[1].Enabled = true;
                    this.contextMenuStripOutput.Items[2].Enabled = true;
                } else {
                    this.contextMenuStripOutput.Items[1].Enabled = false;
                    this.contextMenuStripOutput.Items[2].Enabled = false;
                }
            } catch ( Exception ) {
                this.contextMenuStripOutput.Items[0].Enabled = false;
                this.contextMenuStripOutput.Items[1].Enabled = false;
                this.contextMenuStripOutput.Items[2].Enabled = false;
            }
        }

        //
        // open the file around the caret position (which is translated into m_fn line) in editor (npp preferred, if it fails than default editor)   
        //
        private static string GetClassesRootKeyDefaultValue(string keyPath) {
            using ( RegistryKey key = Registry.ClassesRoot.OpenSubKey(keyPath) ) {
                if ( key == null ) {
                    return null;
                }
                object defaultValue = key.GetValue(null);
                if ( defaultValue == null ) {
                    return null;
                }
                return defaultValue.ToString();
            }
        }
        // mouse double click shall do the same as context menu item click below
        private void TextBoxOutput_MouseDoubleClick(object sender, MouseEventArgs e) {
            this.ToolStripMenuItemOpenFile_Click(null, null);
        }
        private void ToolStripMenuItemOpenFile_Click(object sender, EventArgs e) {
            if ( this.textBoxOutput.Text.Length == 0 ) {
                return;
            }
            int index = this.textBoxOutput.SelectionStart;
            int linenumber = this.textBoxOutput.GetLineFromCharIndex(index);
            if ( linenumber >= this.m_fn.Count ) {
                return;
            }
            string filename = this.m_fn[linenumber].filename;
            string linendx = this.m_fn[linenumber].line;
            if ( File.Exists(filename) ) {
                string extension = Path.GetExtension(filename);
                string registeredApp = GrzTools.FileAssociation.Get(extension);
                if ( (registeredApp != null) && (registeredApp != "") ) {
                    System.Diagnostics.Process p = new System.Diagnostics.Process();
                    p.StartInfo.FileName = registeredApp;
                    if ( registeredApp.Contains("notepad++") ) {
                        p.StartInfo.Arguments = "\"" + filename + "\"" + " -n" + linendx;
                        try {
                            p.Start();
                        } catch ( Exception ) {
                            GrzTools.AutoMessageBox.Show("Couldn't start program.", "Sorry", 2000);
                        }
                    } else {
                        if ( registeredApp.Contains("PDFXCview") ) {
                            Int16.TryParse(linendx, out short line);
                            int page = line / 40;
                            p.StartInfo.Arguments = String.Format("/A page={0} {1}", page, filename);
                            try {
                                p.Start();
                            } catch ( Exception ) {
                                GrzTools.AutoMessageBox.Show("Couldn't start program.", "Sorry", 2000);
                            }
                        } else {
                            Process mProcess = new Process();
                            mProcess.StartInfo = new System.Diagnostics.ProcessStartInfo(filename);
                            try {
                                mProcess.Start();
                            } catch ( Exception ) {
                                ProcessStartInfo processInfo = new ProcessStartInfo(filename);
                                processInfo.Verb = "openas";
                                processInfo.ErrorDialog = false;
                                try {
                                    Process.Start(processInfo);
                                } catch ( Exception ) {
                                    GrzTools.AutoMessageBox.Show("'" + filename + "' is not linked to an executable application.", "Sorry", 2000);
                                }
                            }
                        }
                    }
                } else {
                    Process mProcess = new Process();
                    mProcess.StartInfo = new System.Diagnostics.ProcessStartInfo(filename);
                    mProcess.Start();
                }
            }
        }
        // return the folder around the caret position, which is translated into m_fn line   
        public event EventHandler<LoadFolderEventArgs> LoadFolderRequest;
        public class LoadFolderEventArgs : EventArgs {
            public LoadFolderEventArgs(string folder) {
                this.Folder = folder;
            }
            public string Folder { get; set; }
        }
        private void ToolStripMenuItemCloseAndJumpToFile_Click(object sender, EventArgs e) {
            int linenumber = this.textBoxOutput.GetLineFromCharIndex(this.textBoxOutput.SelectionStart);
            string filename = this.m_fn[linenumber].filename;
            if ( File.Exists(filename) ) {
                EventHandler<LoadFolderEventArgs> handler = LoadFolderRequest;
                if ( handler != null ) {
                    handler(null, new LoadFolderEventArgs(filename));
                    this.Close();
                }
            }
        }
        // open the folder around the caret position, which is translated into m_fn line   
        private void ToolStripMenuItemJumpToFile_Click(object sender, EventArgs e) {
            try {
                int linenumber = this.textBoxOutput.GetLineFromCharIndex(this.textBoxOutput.SelectionStart);
                string program = Application.ExecutablePath;
                string filename = this.m_fn[linenumber].filename;
                if ( File.Exists(@filename) ) {
                    Process.Start(program, @filename);
                }
            } catch ( Exception ) {; }
        }
        private void ToolStripMenuItemSelectAll_Click(object sender, EventArgs e) {
            this.textBoxOutput.SelectAll();
        }
        private void ToolStripMenuItemCopy_Click(object sender, EventArgs e) {
            this.textBoxOutput.Copy();
        }
        private void ToolStripMenuItemPaste_Click(object sender, EventArgs e) {
            this.textBoxOutput.Paste();
        }
        private void ToolStripMenuItemDelete_Click(object sender, EventArgs e) {
            //
            // get 1st line to delete
            //
            // line number with caret
            int linebeg = this.textBoxOutput.GetLineFromCharIndex(this.textBoxOutput.SelectionStart);
            // index of first character in line
            int fstndx = this.textBoxOutput.GetFirstCharIndexFromLine(linebeg);
            // if selection doesn't start at the very first position, this line has to stay in m_fn
            if ( fstndx != this.textBoxOutput.SelectionStart ) {
                linebeg++;
            }

            //
            // get last full line to delete
            //
            //
            int lineend = this.textBoxOutput.GetLineFromCharIndex(this.textBoxOutput.SelectionStart + this.textBoxOutput.SelectionLength);
            int lstndx = 0;
            int beg = this.textBoxOutput.SelectionStart + this.textBoxOutput.SelectionLength;
            for ( int i = beg; i < this.textBoxOutput.Text.Length - 1; i++ ) {
                if ( this.textBoxOutput.Text.Substring(i, 1) == "\n" ) {
                    lstndx = i;
                    break;
                }
            }
            if ( lstndx != beg ) {
                lineend--;
            }

            // delete from m_fn backwards
            for ( int i = lineend; i >= linebeg; i-- ) {
                this.m_fn.RemoveAt(i);
            }

            // delete selected text in textbox
            this.textBoxOutput.SelectedText = "";

        }

        // clear output window
        private void ToolStripMenuItemClearWindow_Click(object sender, EventArgs e) {
            this.listViewOutput.Clear();
            this.textBoxOutput.Clear();
            this.m_fn.Clear();
        }
        // remove all color from output window
        private void ResetColorsToolStripMenuItem_Click(object sender, EventArgs e) {
            this.textBoxOutput.Select(0, this.textBoxOutput.Text.Length);
            this.textBoxOutput.SelectionColor = SystemColors.WindowText;
            this.textBoxOutput.SelectionBackColor = SystemColors.Window;
            this.textBoxOutput.Select(0, 0);
        }

        // search find-text in output window up 
        private void ButtonUp_Click(object sender, EventArgs e) {
            StringComparison sc = this.checkBoxMatchCase.Checked ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            int ndx = this.textBoxOutput.Text.LastIndexOf(this.textBoxFind.Text, Math.Max(0, this.m_iLastFoundPos - this.textBoxFind.Text.Length), sc);
            if ( ndx != -1 ) {
                this.ColorizeFindString(ndx);
                this.m_iLastFoundPos = ndx;
            } else {
                this.m_iLastFoundPos = this.textBoxOutput.Text.Length;
            }
        }
        // search find-text in output window down
        private void ButtonDown_Click(object sender, EventArgs e) {
            StringComparison sc = this.checkBoxMatchCase.Checked ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            int startIndex = Math.Min(Math.Max(0, this.m_iLastFoundPos + this.textBoxFind.Text.Length), this.textBoxOutput.Text.Length);
            int ndx = this.textBoxOutput.Text.IndexOf(this.textBoxFind.Text, startIndex, sc);
            if ( ndx != -1 ) {
                this.ColorizeFindString(ndx);
                this.m_iLastFoundPos = ndx;
            } else {
                this.m_iLastFoundPos = 0;
            }
        }
        // colorize the find string ONCE in output window 
        public void ColorizeFindString(int ndx) {
            this.textBoxOutput.SelectionBackColor = Color.LightBlue;
            int startLine = this.textBoxOutput.GetLineFromCharIndex(ndx);
            int topIndex = this.textBoxOutput.GetCharIndexFromPosition(new System.Drawing.Point(1, 1));
            int topLine = this.textBoxOutput.GetLineFromCharIndex(topIndex);
            int bottomIndex = this.textBoxOutput.GetCharIndexFromPosition(new System.Drawing.Point(1, this.textBoxOutput.Height - 1));
            int bottomLine = this.textBoxOutput.GetLineFromCharIndex(bottomIndex);
            int numVisibleLines = bottomLine - topLine + 1;
            if ( (startLine > bottomLine) || (startLine < topLine) ) {
                int cix = this.textBoxOutput.GetFirstCharIndexFromLine(Math.Max(0, startLine - numVisibleLines / 3 + 1));
                this.textBoxOutput.Select(cix, 0);
                this.textBoxOutput.ScrollToCaret();
            }
            this.textBoxOutput.Select(ndx, this.textBoxFind.Text.Length);
            this.textBoxOutput.SelectionColor = Color.Yellow;
            this.textBoxOutput.SelectionBackColor = Color.Blue;
        }
        // colorize the find string EVERYWEHRE in output window 
        void ColorizeAllFindText() {
            if ( this.textBoxFind.Text.Length == 0 ) {
                return;
            }
            StringComparison sc = this.checkBoxMatchCase.Checked ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            int start = 0;
            do {
                Application.DoEvents();
                int ndx = this.textBoxOutput.Text.IndexOf(this.textBoxFind.Text, Math.Min(this.textBoxOutput.Text.Length, start), sc);
                if ( ndx != -1 ) {
                    this.textBoxOutput.Select(ndx, this.textBoxFind.Text.Length);
                    this.textBoxOutput.SelectionBackColor = Color.LightBlue;
                    this.textBoxOutput.SelectionColor = Color.Yellow;
                    start = ndx + this.textBoxFind.Text.Length;
                } else {
                    break;
                }
            } while ( true );
        }
        // colorize the buzzwords EVERYWEHRE in output window 
        void ColorizeAllBuzzwords() {
            if ( this.m_buzzword[0].Length == 0 ) {
                return;
            }
            StringComparison sc = this.checkBoxMatchCase.Checked ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            foreach ( string s in this.m_buzzword ) {
                int start = 0;
                do {
                    Application.DoEvents();
                    int ndx = this.textBoxOutput.Text.IndexOf(s, Math.Min(this.textBoxOutput.Text.Length, start), sc);
                    if ( ndx != -1 ) {
                        this.textBoxOutput.Select(ndx, s.Length);
                        this.textBoxOutput.SelectionBackColor = Color.Yellow;
                        this.textBoxOutput.SelectionColor = Color.Red;
                        start = ndx + s.Length;
                    } else {
                        break;
                    }
                } while ( true );
            }
        }
        private void ColorizeBuzzwordsToolStripMenuItem_Click(object sender, EventArgs e) {
            this.ColorizeAllBuzzwords();
        }
        private void TextBoxFind_KeyDown(object sender, KeyEventArgs e) {
            if ( e.KeyCode == Keys.Enter ) {
                this.ColorizeAllFindText();
            }
        }
        private void colorizeFindTextToolStripMenuItem_Click(object sender, EventArgs e) {
            this.ColorizeAllFindText();
        }
        // handle ENTER key pressed in ComboBox: insert new item AND force a buzzword change AND start search
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if ( (this.ActiveControl == this.comboBoxBuzzword) && (keyData == Keys.Return) ) {
                // insert item  
                this.comboBoxBuzzword_Validated(null, null);
                // exec search
                this.OnButtonRunClick(null, null);
                return true;
            } else {
                return base.ProcessCmdKey(ref msg, keyData);
            }
        }
        // indicator that combobox had lost focus, ie. editing is over either thru ET or combo lost focus 
        private void comboBoxBuzzword_Validated(object sender, EventArgs e) {
            // insert the just edited buzzword to the combobox list
            string str = this.comboBoxBuzzword.Text;
            int index = this.comboBoxBuzzword.FindStringExact(str);
            // if modified buzzword is already member of the list, we delete it first, then insert it at the top
            if ( (index < this.comboBoxBuzzword.Items.Count) && (index >= 0) ) {
                this.comboBoxBuzzword.Items.RemoveAt(index);
            }
            // insert buzzword at the top of the list
            this.comboBoxBuzzword.Items.Insert(0, str);
            this.comboBoxBuzzword.SelectedIndex = 0;

            // inform output window that the buzzword was changed
            this.textBoxBuzzword_TextChanged();
        }
        private void textBoxBuzzword_TextChanged() {
            if ( this.comboBoxBuzzword.Items.Count == 0 ) {
                return;
            }

            string buzzword = this.comboBoxBuzzword.SelectedItem.ToString();
            if ( buzzword.Length > 0 ) {
                this.colorizeBuzzwordsToolStripMenuItem.Enabled = true;
                this.m_buzzword = buzzword.Split(new string[] { this.textBoxSeparator.Text }, StringSplitOptions.RemoveEmptyEntries);
                if ( this.m_buzzword.Length == 0 ) {
                    this.m_buzzword = new string[] { "" };
                }
            } else {
                this.colorizeBuzzwordsToolStripMenuItem.Enabled = false;
                this.m_buzzword = new string[] { "" };
            }
        }
        private void textBoxFind_TextChanged(object sender, EventArgs e) {
            StringComparison sc = this.checkBoxMatchCase.Checked ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            int ndx = -1;
            this.Invoke(new Action(() => { ndx = this.textBoxOutput.Text.IndexOf(this.textBoxFind.Text, 0, sc); }));
            if ( ndx == -1 ) {
                this.Invoke(new Action(() => {
                    this.textBoxFind.BackColor = Color.MistyRose;
                    this.colorizeFindTextToolStripMenuItem.Enabled = false;
                }));
            } else {
                this.Invoke(new Action(() => {
                    this.textBoxFind.BackColor = SystemColors.Window;
                    this.colorizeFindTextToolStripMenuItem.Enabled = true;
                }));
            }
        }

        // event handler to indicate progress while file search
        string[] m_separator = null;
        string[] m_includes = null;
        void ChangeEvent_Received(object sender, GrzTools.FastFileFind.ChangeEventArgs e) {
            // very elegant: w/o this Invoke(..) thing, events raised from a separate thread are forbidden to access UI-Thread elements and exceptions are thrown
            if ( this.InvokeRequired ) {
                try {
                    this.Invoke(new EventHandler<GrzTools.FastFileFind.ChangeEventArgs>(this.ChangeEvent_Received), sender, e);
                } catch ( Exception ) {; }
                return;
            }

            // colorize part of a required path
            if ( this.m_separator == null && this.checkBoxPathMustContain.Checked ) {
                this.m_separator = new string[] { this.textBoxSeparator.Text };
                this.m_includes = this.textBoxFolderPattern.Text.Split(this.m_separator, StringSplitOptions.RemoveEmptyEntries);
            }

            // progress bar is updated on each entered folder
            if ( (e.LineText == null) && (e.ProgFold == null) && (e.FileName == null) ) {
                if ( this.m_bFakeMaximumActive ) {
                    // happens until we know the number of folders to process: as long as progressbar Maximum is 0, we just count the events (ie. processed folders) 
                    this.m_iFolderCountBeforeMaximumIsKnown++;
                    // fake progress, it lets the progress bar flicker back and forward to indicate the uncertainty of the progress
                    int adder = 10000;
                    if ( this.m_iFolderCountBeforeMaximumIsKnown % 2 == 0 ) {
                        adder = 20000;
                    }
                    this.progressBarTotal.Maximum = this.m_iFolderCountBeforeMaximumIsKnown + adder;
                    this.progressBarTotal.PerformStep();
                    this.m_currentFolderCount++;
                } else {
                    // happens normally when the number of folders is known: we perform simple steps based on known maximum
                    if ( this.progressBarTotal.Value == this.progressBarTotal.Maximum ) {
                        this.progressBarTotal.Maximum++;
                    }
                    this.progressBarTotal.PerformStep();
                }
            }

            if ( e.FileName == null ) {
                // if there is no filename, only counting folders is in progress
                if ( e.ProgFold != null ) {
                    this.m_somethingToShow = e.ProgFold;
                    if ( this.m_bFakeMaximumActive ) {
                        this.m_iTrueFolderCountBeforeMaximumIsKnown++;
                    }
                }
                // linetext consisting only of "\r\n" is the delemiter to the next file
                if ( e.LineText != null ) {
                    if ( (e.LineText == "\r\n") /*&& !this.checkBoxFilesOnly.Checked*/ ) {
                        // don't add when file was skipped due to data/size filters
                        if ( !this.m_bFileSkipper ) {
                            this.m_fn.Add(new Fn("", ""));
                        }
                    }
                }
            } else {
                //
                // we have LineText AND FileLine AND a hit AND no skip file: here we really found a buzzword
                //
                if ( (e.LineText != null) && (e.FileLine != null) && (e.Hit) && !this.m_bFileSkipper ) {
                    // show found text & line number
                    string output = e.LineText + "\r\n";
                    if ( this.checkBoxOutputAddLine.Checked ) {
                        output = e.FileLine + "\t" + output;
                    }
                    this.textBoxOutput.AppendText(output);
                    this.m_fn.Add(new Fn(e.FileName, e.FileLine.ToString()));
                    this.m_GlobalHitCounter++;
                    // colorize buzzwords
                    int selstartpos = this.textBoxOutput.Text.Length - e.LineText.Length - 1;
                    if ( selstartpos < 0 ) {
                        selstartpos = 0;
                    }
                    foreach ( string s in this.m_buzzword ) {
                        int ndx = e.LineText.IndexOf(s, e.StringCompare);
                        if ( ndx != -1 ) {
                            this.textBoxOutput.Select(selstartpos + ndx, s.Length);
                            this.textBoxOutput.SelectionColor = Color.Red;
                            this.textBoxOutput.SelectionBackColor = Color.Yellow;
                        }
                    }
                } else {
                    //
                    // filename == YES && fileline == NO: we don't search buzzwords 
                    //
                    if ( (e.FileName != null) && (e.FileLine == null) ) {

                        // Does file comply to date settings?
                        this.m_bFileSkipper = false;
                        DateTime dateToCheck = new DateTime();
                        try {
                            dateToCheck = File.GetLastWriteTime(e.FileName);
                            if ( !this.radioButtonDateAll.Checked ) {
                                if ( !((dateToCheck >= this.m_begDate) && (dateToCheck <= this.m_endDate)) ) {
                                    this.m_bFileSkipper = true;
                                    return;
                                }
                            }
                        } catch ( Exception ) {
                        }
                        // Does file comply to size settings?
                        long sizeToCheck = -1;
                        try {
                            sizeToCheck = new System.IO.FileInfo(e.FileName).Length;
                            if ( !this.radioButtonSizeAll.Checked ) {
                                if ( this.radioButtonEqual.Checked ) {
                                    if ( sizeToCheck != this.m_sizeLimit ) {
                                        this.m_bFileSkipper = true;
                                        return;
                                    }
                                } else {
                                    if ( this.radioButtonLarger.Checked ) {
                                        if ( sizeToCheck < this.m_sizeLimit ) {
                                            this.m_bFileSkipper = true;
                                            return;
                                        }
                                    } else {
                                        if ( this.radioButtonSmaller.Checked ) {
                                            if ( sizeToCheck > this.m_sizeLimit ) {
                                                this.m_bFileSkipper = true;
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        } catch ( Exception ) {
                        }

                        // general file error while searching text in file
                        string strError = "";
                        if ( e.error ) {
                            strError = e.LineText;
                        }

                        //
                        // update outputs
                        //
                        int pad = this.numericUpDownSize.Value.ToString().Length;
                        if ( this.radioButtonLarger.Checked ) {
                            pad = 15;
                        }
                        string[] starr = new string[5];
                        try {
                            starr = new string[5] { dateToCheck.ToString("dd.MM.yyyy HH:mm:ss"), sizeToCheck.ToString(), Path.GetFileName(e.FileName), Path.GetDirectoryName(e.FileName), Path.GetExtension(e.FileName) };
                        } catch ( Exception ) {
                            starr = new string[5] { dateToCheck.ToString("dd.MM.yyyy HH:mm:ss"), sizeToCheck.ToString(), e.FileName, "filename > 248", "" };
                        }
                        // listview
                        this.listViewOutput.Items.Add(new ListViewItem(starr));
                        // re arrange
                        if ( this.listViewOutput.Items.Count == 1 ) {
                            try {
                                this.listViewOutput.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                            } catch {; }
                        }
                        // text output
                        if ( this.m_buzzword[0].Length > 0 ) {
                            if ( (this.textBoxOutput.Lines.Count() > 0) ) {
                                this.textBoxOutput.AppendText("\r\n");
                            }
                            this.textBoxOutput.AppendText(strError + starr[0] + " " + starr[1].PadLeft(pad, ' ') + " " + e.FileName + "\r\n");
                        } else {
                            this.textBoxOutput.AppendText(strError + starr[0] + " " + starr[1].PadLeft(pad, ' ') + " " + e.FileName + "\r\n");
                            this.m_somethingToShow = e.FileName;
                        }

                        // colorize the "searched for" part of the filename
                        string fullPath = e.FileName + "\r\n";
                        string colorPart = this.textBoxFilePattern.Text;
                        colorPart = String.Join("", colorPart.Split('*'));  // remove the * to only colorize the "search part", supposed to appear at head and tail 
                        if ( colorPart.Contains('?') ) {
                            string pattern = colorPart.Replace('?', '.');   // replace ? with . which stands for 'any char' in a regex expression
                            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
                            Match match = regex.Match(fullPath);
                            if ( match.Success ) {
                                int lastNdxOutput = fullPath.Length - match.Index;
                                int colorStartPos = this.textBoxOutput.Text.Length - lastNdxOutput + 1;
                                this.textBoxOutput.Select(colorStartPos, match.Length);
                                this.textBoxOutput.SelectionColor = Color.Red;
                            }
                        } else {
                            int lastNdxFullPath = fullPath.LastIndexOf(colorPart, StringComparison.OrdinalIgnoreCase);
                            if ( (lastNdxFullPath != -1) && (lastNdxFullPath < fullPath.Length - 1) ) {
                                int lastNdxOutput = fullPath.Length - lastNdxFullPath;
                                int colorStartPos = this.textBoxOutput.Text.Length - lastNdxOutput + 1;
                                this.textBoxOutput.Select(colorStartPos, colorPart.Length);
                                this.textBoxOutput.SelectionColor = Color.Red;
                            }
                        }

                        // colorize the required part of a path
                        if ( this.checkBoxPathMustContain.Checked ) {
                            foreach ( string s in this.m_includes ) {
                                int start = fullPath.IndexOf(s, StringComparison.OrdinalIgnoreCase);
                                if ( start != -1 ) {
                                    start = this.textBoxOutput.Text.Length - fullPath.Length + start + 1;
                                    this.textBoxOutput.Select(start, s.Length);
                                    this.textBoxOutput.SelectionColor = Color.Red;
                                }
                            }
                        }

                        // filename lists
                        this.m_fn.Add(new Fn(e.FileName, ""));
                        this.m_fList.Add(e.FileName);
                    }
                    //
                    // we have a filename and a fileline: this is file search progress for buzzword
                    //
                    if ( (e.FileName != null) && (e.FileLine != null) ) {
                        // current file in progress is shown once
                        if ( this.labelHHlist.Text != e.FileName ) {
                            if ( this.m_iRun == 1 ) {
                                this.m_somethingToShow = e.FileName;
                                this.labelHHlist.Text = e.FileName;
                                this.progressBarCurrent.Value = 0;
                            }
                        }
                        // line count progress when processing text search in a file
                        if ( e.FileLine.EndsWith("l") ) {
                            // this comes in only once per file, trailing l indicates line count of file 
                            this.timerBigFile.Stop();
                            this.progressBarCurrent.Value = 0;
                            this.buttonBigFileSkip.Visible = false;
                            // what shall be the threshold line count for a big file
                            int iHugeFileLineCountLimit = (int)this.numericUpDownHugeFileLineCountLimit.Value;
                            // current line count sets max of progress bar
                            string sLineCount = e.FileLine.Replace("l", "");
                            this.progressBarCurrent.Maximum = int.Parse(sLineCount);
                            // set user break timer to be able to skip a large file
                            if ( this.progressBarCurrent.Maximum > iHugeFileLineCountLimit ) {
                                // huge files could be skipped after 100ms
                                this.timerBigFile.Interval = 100;
                            } else {
                                // small files could be skipped after 5s
                                this.timerBigFile.Interval = 5000;
                            }
                            this.timerBigFile.Start();
                        } else {
                            // normal line count progress
                            this.progressBarCurrent.Value = int.Parse(e.FileLine);
                            //                            Application.DoEvents();
                        }
                    }
                }
            }
        }
        private void timerBigFile_Tick(object sender, EventArgs e) {
            if ( this.m_iRun == 1 ) {
                if ( this.checkBoxAlwaysSkipLargeFiles.Checked ) {
                    this.buttonBigFileSkip.Visible = false;
                    this.m_iRun = -1;
                } else {
                    this.buttonBigFileSkip.Visible = true;
                }
            }
            this.timerBigFile.Stop();
        }
        private void buttonBigFileSkip_Click(object sender, EventArgs e) {
            this.buttonBigFileSkip.Visible = false;
            if ( this.m_iRun == 0 ) {
                // skip single file only when run was not interrupted
                this.m_iRun = -1;
            }
        }

        //
        // main Program Sequence
        //
        private void OnButtonRunClick(object sender, EventArgs e) {
            // allow termination of running process 
            if ( this.buttonRun.Text == this.m_buttonOriTxtRUN ) {

                // colorize required part of a path
                this.m_separator = null;

                // search files > 248 characters
                if ( this.checkBoxLen248.Checked ) {
                    if ( MessageBox.Show("Checking only for 'files >248 char' ignoring all other search settings.", "Note", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.Cancel ) {
                        return;
                    }
                }

                // alter button text
                this.buttonRun.Text = this.m_buttonTxtBrk;

                // save INI file: in case we have a crash, the user were required to type in all entries again
                this.SaveINI();

                // folders to process
                this.m_original = this.textBoxProcessFile.Text;
                //string[] folders = m_original.Split(';');
                string[] folders = this.m_original.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                // count folders async and don't wait for completion
                this.m_iFolderCountBeforeMaximumIsKnown = 0;
                this.m_iTrueFolderCountBeforeMaximumIsKnown = 0;
                this.m_bFakeMaximumActive = true;
                this.m_totalFolderCount = 0;
                this.m_currentFolderCount = 0;
                this.m_iRun = 1;
                Task tp = new Task(() => this.doFolderCount(folders));
                tp.Start();

                // start file search async as a separate task: Task is better than Thread, App-UI is much more responsive
                Task tf = new Task(() => this.doFiles(folders));
                tf.Start();

                // we update status every 300ms 
                this.timerUpdate.Start();

            } else {
                // stop - aka user break
                this.buttonRun.Text = "- wait finish -";
                this.m_folderCounter.Stop();
                this.m_iRun = 0;
                this.timerUpdate.Stop();
            }
        }

        GrzTools.FolderCounter m_folderCounter;
        void doFolderCount(string[] folders) {
            this.m_folderCounter = new GrzTools.FolderCounter();
            // reset global collector for folder count
            this.m_totalFolderCount = 0;
            // loop parallel all top level folders - loop happens only in Computer view, in all other cases we have just a single folder
            folders.ToList().AsParallel().ForAll(folder => {
                object dirCountLock = new object();
                lock ( dirCountLock ) {
                    this.m_totalFolderCount += this.m_folderCounter.GetFolderCount(folder);
                }
            });
            // now we know the exact maximum
            this.m_bFakeMaximumActive = false;
            this.Invoke(new Action(() => {
                this.progressBarTotal.Maximum = Math.Max(this.m_currentFolderCount, this.m_totalFolderCount);
                this.progressBarTotal.Value = this.m_currentFolderCount;
            }));
        }
        void doFiles(string[] folders) {
            // clear output window or not
            if ( !this.appendToOutputToolStripMenuItem.Checked && (this.m_iRun != 0) ) {
                this.m_iLastFoundPos = 0;
                this.Invoke(new Action(() => {
                    this.textBoxOutput.Clear();
                    this.listViewOutput.Items.Clear();
                }));
                this.m_fn.Clear();
            }

            // generate empty file list
            this.m_fList = new List<String>();

            // buzzword etc. separator 
            if ( this.textBoxSeparator.Text.Length == 0 ) {
                this.Invoke(new Action(() => { this.textBoxSeparator.Text = ";"; }));
            }
            string[] separator = { this.textBoxSeparator.Text };

            // prepare search for buzzwords
            string buzzword = "";
            this.Invoke(new Action(() => {
                if ( this.comboBoxBuzzword.SelectedItem != null ) {
                    buzzword = this.comboBoxBuzzword.SelectedItem.ToString();
                }
            }));
            this.m_buzzword = buzzword.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if ( (this.m_buzzword.Length == 0) || !this.checkBoxSearchText.Checked ) {
                this.m_buzzword = new string[] { "" };
            }

            // string comparison rule
            StringComparison sc = this.checkBoxMatchCase.Checked ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            // folder name must contain
            string[] includes = this.textBoxFolderPattern.Text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if ( !this.checkBoxPathMustContain.Checked ) {
                includes = new string[] { "" };
            }

            // folder name must not contain
            string[] excludes = this.textBoxFolderExcludes.Text.Split(separator, StringSplitOptions.None);
            if ( !this.checkBoxPathMustNotContain.Checked ) {
                excludes = new string[] { "" };
            }

            // we want to process a given start folder
            this.Invoke(new Action(() => { this.labelHHlist.Text = ""; }));

            // skip binaries
            this.m_bSkipBin = this.checkBoxSkipSearchingBinaryFiles.Checked;

            // file date init
            this.m_begDate = DateTime.Today;
            this.m_endDate = DateTime.Today.AddDays(1);
            if ( this.radioButtonLastWeek.Checked ) {
                this.m_begDate = DateTime.Today.AddDays(-7);
            }
            if ( this.radioButtonLastMonth.Checked ) {
                this.m_begDate = DateTime.Today.AddMonths(-1);
            }
            if ( this.radioButtonRange.Checked ) {
                this.m_begDate = this.dateTimePickerStart.Value.Date;
                this.m_endDate = this.dateTimePickerStop.Value.Date;
                if ( this.m_begDate == this.m_endDate ) {
                    this.m_endDate = this.m_endDate.AddDays(1);
                }
            }
            // date reversed?
            if ( this.m_begDate > this.m_endDate ) {
                DateTime tmp = this.m_begDate;
                this.m_begDate = this.m_endDate;
                this.m_endDate = tmp;
            }

            // file size init
            try {
                this.m_sizeLimit = (Int64)this.numericUpDownSize.Value;
            } catch ( Exception ) {
                this.Invoke(new Action(() => { this.numericUpDownSize.Value = 1000; }));
                this.m_sizeLimit = 1000;
            }

            // !!! live refresh is much faster when clicking once into the textbox as to scroll the caret
            if ( this.checkBoxLiveRefresh.Checked ) {
                this.Invoke(new Action(() => { this.DoMouseClick(this.textBoxOutput); }));
            } else {
                this.Invoke(new Action(() => { this.textBoxOutput.ScrollToCaret(); }));
            }

            // disable things, which cannot alter during processing
            this.Invoke(new Action(() => {
                this.textBoxProcessFile.Enabled = false;
                this.panelDate.Enabled = false;
                this.panelSize.Enabled = false;
            }));

            // generate a file search object & register an event handler to show progress etc.
            GrzTools.FastFileFind fff = new GrzTools.FastFileFind(this);
            fff.ChangeEvent += new EventHandler<GrzTools.FastFileFind.ChangeEventArgs>(this.ChangeEvent_Received);
            // multiple file masks are allowed
            string[] filemasks = this.textBoxFilePattern.Text.Split(';');
            if ( filemasks[0] == "" ) {
                filemasks[0] = "*";
                this.Invoke(new Action(() => { this.textBoxFilePattern.Text = filemasks[0]; }));
            }
            // if filemask is solitair AND has no wildcards AND exact search is not wanted THEN auto add trailing and preceding wildcards *  
            if ( filemasks.Length == 1 ) {
                if ( !filemasks[0].Contains("*") && !filemasks[0].Contains("?") ) {
                    if ( !this.checkBoxExact.Checked ) {
                        filemasks[0] = "*" + filemasks[0] + "*";
                        this.Invoke(new Action(() => { this.textBoxFilePattern.Text = filemasks[0]; }));
                    }
                }
            }
            // sequentially loop until all requested folders are processed
            int ndxFolder = 0;
            do {
                Application.DoEvents();
                if ( ndxFolder < folders.Length ) {
                    string folder = folders[ndxFolder];
                    if ( folder.Length > 0 ) {
                        if ( !folder.EndsWith("\\") ) {
                            folder += '\\';
                        }
                        string txt = "processing " + folder + (folders.Length > 1 ? " out of " + this.m_original : "");
                        this.Invoke(new Action(() => { this.textBoxProcessFile.Text = txt; }));
                        // loop multiple file masks
                        foreach ( string filemask in filemasks ) {
                            Application.DoEvents();
                            // the main search call: process one single folder at a time: find files in it according to the specs like 'buzzword', 'mask', 'includes', 'excludes'
                            fff.FindFilesTextHelper(ref this.m_iRun, ref this.m_bSkipBin, folder, filemask, sc, this.m_buzzword, includes, excludes, this.checkBoxLen248.Checked);
                            if ( this.m_iRun == 0 ) {
                                break;
                            }
                        }
                    }
                    ndxFolder++;
                }
            } while ( (ndxFolder < folders.Length) && (this.m_iRun != 0) );

            // finale
            this.Invoke(new Action(() => {
                this.timerUpdate.Stop();
                this.timerUpdate_Tick(null, null);
                this.timerBigFile.Stop();
                this.buttonBigFileSkip.Visible = false;
                this.listViewOutput.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                this.textBoxProcessFile.Enabled = true;
                this.panelDate.Enabled = true;
                this.panelSize.Enabled = true;
                this.textBoxProcessFile.Text = this.m_original;
                this.progressBarCurrent.Value = 0;
                this.buttonRun.Text = this.m_buttonOriTxtRUN;
                this.progressBarTotal.Value = this.progressBarTotal.Maximum;
                this.labelHHlist.Text = this.progressBarTotal.Value.ToString() + " folders searched / " + this.m_fList.Count.ToString() + " files found / " + this.m_GlobalHitCounter.ToString() + " matches";
            }));
            fff.ChangeEvent -= new EventHandler<GrzTools.FastFileFind.ChangeEventArgs>(this.ChangeEvent_Received);

            // update the result's text searchbox color
            this.textBoxFind_TextChanged(null, null);

            // updates
            if ( this.m_iRun == 1 ) {
                GrzTools.AutoMessageBox.Show("All done.", "File Search", 2000);
            } else {
                this.Invoke(new Action(() => { this.Text = "File Search - interrupted"; }));
            }
            this.m_iRun = 0;
        }

        // 300ms timer updates the currently processed file/folder, m_somethingToShow comes from ChangeEvent_Received - but updating right there costs too much cpu due to high update frequency
        private void timerUpdate_Tick(object sender, EventArgs e) {
            this.Invoke(new Action(() => {
                this.labelHHlist.Text = this.m_somethingToShow;
                if ( this.m_bFakeMaximumActive ) {
                    int whatToShow = Math.Max(this.m_iTrueFolderCountBeforeMaximumIsKnown, this.m_totalFolderCount);
                    this.Text = "File Search - calculating folder count " + whatToShow.ToString();
                } else {
                    this.Text = "File Search - current folder # " + this.progressBarTotal.Value.ToString() + " (" + this.progressBarTotal.Maximum.ToString() + ")";
                }
            }));
        }

        private void appendToOutputToolStripMenuItem_Click(object sender, EventArgs e) {
            this.appendToOutputToolStripMenuItem.Checked = !this.appendToOutputToolStripMenuItem.Checked;
            this.checkBoxKeepOutput.Checked = this.appendToOutputToolStripMenuItem.Checked;
        }
        private void checkBoxKeepOutput_MouseDown(object sender, MouseEventArgs e) {
            this.checkBoxKeepOutput.Checked = !this.checkBoxKeepOutput.Checked;
            this.appendToOutputToolStripMenuItem.Checked = this.checkBoxKeepOutput.Checked;
        }

        private void saveOutputToolStripMenuItem_Click(object sender, EventArgs e) {
            SaveFileDialog dlg = new SaveFileDialog {
                InitialDirectory = Path.GetDirectoryName(this.m_sLastStorePath),
                Title = "Select file to save output data",
                Filter = "All Files (*.*)|*.*",
                FilterIndex = 1
            };
            if ( dlg.ShowDialog() == DialogResult.OK ) {
                this.m_sLastStorePath = Path.GetDirectoryName(dlg.FileName);
                // save file
                File.WriteAllText(dlg.FileName, this.textBoxOutput.Text);
                // start this file
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.RedirectStandardOutput = false;
                p.StartInfo.FileName = dlg.FileName;
                p.StartInfo.UseShellExecute = true;
                p.Start();
            }
            dlg.Dispose();
        }

        // distinguish between folder vs. PC search
        private void checkBoxComputer_CheckedChanged(object sender, EventArgs e) {
            if ( this.checkBoxComputer.Checked ) {
                this.textBoxProcessFile.Text = "";
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                foreach ( DriveInfo d in allDrives ) {
                    if ( d.DriveType == DriveType.Fixed ) {
                        this.textBoxProcessFile.Text += d.Name + ";";
                    }
                }
            } else {
                this.textBoxProcessFile.Text = this.m_sStartFolder;
            }
        }

        private void textBoxFilePattern_KeyDown(object sender, KeyEventArgs e) {
            if ( e.KeyCode == Keys.Enter ) {
                this.OnButtonRunClick(null, null);
            }
        }

        private void checkBoxSkip_CheckedChanged(object sender, EventArgs e) {
            if ( this.checkBoxSkip.Checked ) {
                if ( !this.textBoxFolderExcludes.Text.Contains(this.m_defaultSkippedFolders) ) {
                    if ( (this.textBoxFolderExcludes.Text.EndsWith(";")) || (this.textBoxFolderExcludes.Text.Length == 0) ) {
                        this.textBoxFolderExcludes.Text += this.m_defaultSkippedFolders;
                    } else {
                        this.textBoxFolderExcludes.Text += ";" + this.m_defaultSkippedFolders;
                    }
                }
            } else {
                if ( this.textBoxFolderExcludes.Text.Length > 0 ) {
                    this.textBoxFolderExcludes.Text = this.textBoxFolderExcludes.Text.Replace(this.m_defaultSkippedFolders, "");
                }
            }
        }

        // hide/show textoutput vs. filelist
        bool m_bSearchVisible = false;
        private void checkBoxFilesOnly_CheckedChanged(object sender, EventArgs e) {
            if ( this.checkBoxFilesOnly.Checked ) { // Listview
                this.m_bSearchVisible = this.findToolStripMenuItem.Checked;
                this.findToolStripMenuItem_Show(false);
                // preview Checkbox immer zeigen
                this.tableLayoutPanel2.SetColumnSpan(this.labelHHlist, 9);
                this.tableLayoutPanel2.SetColumnSpan(this.progressBarTotal, 9);
                this.tableLayoutPanel2.SetColumnSpan(this.progressBarCurrent, 9);
                this.checkBoxPreview.Visible = true;
                // preview control in Abhängigkeit von preview Checkbog zeigen
                this.tableLayoutPanel2.SetRow(this.previewCtl, 11);
                this.tableLayoutPanel2.SetRowSpan(this.previewCtl, 2);
                this.tableLayoutPanel2.SetColumn(this.previewCtl, 7);
                this.tableLayoutPanel2.SetColumnSpan(this.previewCtl, 3);
                this.previewCtl.Visible = this.checkBoxPreview.Checked;
                // listview Größe in Abhängigkeit von checkbox preview
                this.textBoxOutput.Visible = false;
                this.tableLayoutPanel2.SetRow(this.listViewOutput, 11);
                this.tableLayoutPanel2.SetColumn(this.listViewOutput, 0);
                this.tableLayoutPanel2.SetColumnSpan(this.listViewOutput, this.checkBoxPreview.Checked ? 7 : 10);
                this.tableLayoutPanel2.SetRowSpan(this.listViewOutput, 2);
                this.listViewOutput.Visible = true;
                this.listViewOutput.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            } else {
                // preview Dinge nie anzeigen
                this.checkBoxPreview.Visible = false;
                this.previewCtl.Visible = false;
                // alle übrigen Controls in max. Größe anzeigen
                this.tableLayoutPanel2.SetColumnSpan(this.labelHHlist, 10);
                this.tableLayoutPanel2.SetColumnSpan(this.progressBarTotal, 10);
                this.tableLayoutPanel2.SetColumnSpan(this.progressBarCurrent, 10);
                this.listViewOutput.Visible = false;
                this.tableLayoutPanel2.SetRow(this.textBoxOutput, 11);
                this.tableLayoutPanel2.SetColumn(this.textBoxOutput, 0);
                this.tableLayoutPanel2.SetColumnSpan(this.textBoxOutput, 10);
                this.tableLayoutPanel2.SetRowSpan(this.textBoxOutput, 2);
                this.textBoxOutput.Visible = true;
                this.findToolStripMenuItem_Show(this.m_bSearchVisible);
            }
        }

        private void checkBoxPreview_CheckedChanged(object sender, EventArgs e) {
            this.ShowAdvancedOptions(this.checkBoxAdvancedOptions.Checked);
        }

        // ADVANCED checkBox
        private void checkBoxAdvancedOptions_CheckedChanged(object sender, EventArgs e) {
            this.ShowAdvancedOptions(((CheckBox)sender).Checked);
        }
        private void ShowAdvancedOptions(bool bShow) {
            if ( bShow ) { // advanced options ON

                this.checkBoxKeepOutput.Visible = true;
                this.checkBoxLiveRefresh.Visible = true;
                this.panelDate.Visible = true;
                this.panelSize.Visible = true;
                this.panelTextOptions.Visible = true;
                this.panelSeparator.Visible = true;


                if ( this.checkBoxFilesOnly.Checked ) { // Listview mode
                    // immer Anzeige der Preview Checkbox bei FileView 
                    this.tableLayoutPanel2.SetColumnSpan(this.labelHHlist, 9);
                    this.tableLayoutPanel2.SetColumnSpan(this.progressBarTotal, 9);
                    this.tableLayoutPanel2.SetColumnSpan(this.progressBarCurrent, 9);
                    this.checkBoxPreview.Visible = true;
                    // Anzeigegröße der Listview in Abhängigkeit von Preview CheckBox
                    this.tableLayoutPanel2.SetColumn(this.listViewOutput, 0);
                    this.tableLayoutPanel2.SetColumnSpan(this.listViewOutput, this.checkBoxPreview.Checked ? 7 : 10);
                    this.tableLayoutPanel2.SetRowSpan(this.listViewOutput, 2);
                    this.tableLayoutPanel2.SetRow(this.listViewOutput, 11);
                    // Visibility des Preview Controls in Abhängigkeit von Preview CheckBox 
                    this.tableLayoutPanel2.SetRow(this.previewCtl, 11);
                    this.tableLayoutPanel2.SetRowSpan(this.previewCtl, 2);
                    this.tableLayoutPanel2.SetColumn(this.previewCtl, 7);
                    this.tableLayoutPanel2.SetColumnSpan(this.previewCtl, 3);
                    this.previewCtl.Visible = this.checkBoxPreview.Checked;
                } else { // Text mode
                    // Preview ist immer aus
                    this.previewCtl.Visible = false;
                    this.checkBoxPreview.Visible = false;
                    // Anzeigen auf Maximum
                    this.tableLayoutPanel2.SetColumnSpan(this.labelHHlist, 10);
                    this.tableLayoutPanel2.SetColumnSpan(this.progressBarTotal, 10);
                    this.tableLayoutPanel2.SetColumnSpan(this.progressBarCurrent, 10);
                    this.tableLayoutPanel2.SetColumn(this.textBoxOutput, 0);
                    this.tableLayoutPanel2.SetColumnSpan(this.textBoxOutput, 10);
                    this.tableLayoutPanel2.SetRowSpan(this.textBoxOutput, 2);
                    this.tableLayoutPanel2.SetRow(this.textBoxOutput, 11);
                }

            } else {  // advanced options OFF

                this.checkBoxKeepOutput.Visible = false;
                this.checkBoxLiveRefresh.Visible = false;
                this.panelDate.Visible = false;
                this.panelSize.Visible = false;
                this.panelTextOptions.Visible = false;
                this.panelSeparator.Visible = false;

                if ( this.checkBoxFilesOnly.Checked ) {  // FILE VIEW
                    // preview Control in Abhängigkeit von preview Checkbox
                    this.tableLayoutPanel2.SetRow(this.previewCtl, 11);
                    this.tableLayoutPanel2.SetRowSpan(this.previewCtl, 2);
                    this.tableLayoutPanel2.SetColumn(this.previewCtl, 7);
                    this.tableLayoutPanel2.SetColumnSpan(this.previewCtl, 3);
                    this.previewCtl.Visible = this.checkBoxPreview.Checked;
                    // Listview Größe in Abhängigkeit von preview Checkbox
                    this.tableLayoutPanel2.SetColumn(this.listViewOutput, 0);
                    this.tableLayoutPanel2.SetColumnSpan(this.listViewOutput, this.checkBoxPreview.Checked ? 7 : 10);
                    this.tableLayoutPanel2.SetRowSpan(this.listViewOutput, 1);
                    this.tableLayoutPanel2.SetRow(this.listViewOutput, 8);
                    this.tableLayoutPanel2.SetRowSpan(this.listViewOutput, 5);
                    // preview Checkbox immer zeigen
                    this.tableLayoutPanel2.SetColumnSpan(this.labelHHlist, 9);
                    this.tableLayoutPanel2.SetColumnSpan(this.progressBarTotal, 9);
                    this.tableLayoutPanel2.SetColumnSpan(this.progressBarCurrent, 9);
                    this.checkBoxPreview.Visible = true;

                } else { // TEXT VIEW
                    // preview Control und preview Checkbox immer verbergen
                    this.previewCtl.Visible = false;
                    this.checkBoxPreview.Visible = false;
                    // alle übrigen Controls auf max. Größe erweiteren
                    this.tableLayoutPanel2.SetColumnSpan(this.labelHHlist, 10);
                    this.tableLayoutPanel2.SetColumnSpan(this.progressBarTotal, 10);
                    this.tableLayoutPanel2.SetColumnSpan(this.progressBarCurrent, 10);
                    this.tableLayoutPanel2.SetColumn(this.textBoxOutput, 0);
                    this.tableLayoutPanel2.SetColumnSpan(this.textBoxOutput, 10);
                    this.tableLayoutPanel2.SetRowSpan(this.textBoxOutput, 1);
                    this.tableLayoutPanel2.SetRow(this.textBoxOutput, 8);
                    this.tableLayoutPanel2.SetRowSpan(this.textBoxOutput, 5);
                }
            }

            this.findToolStripMenuItem_Show(this.findToolStripMenuItem.Checked);
        }

        // text search or not
        private void checkBoxSearchText_Click(object sender, EventArgs e) {
            if ( this.checkBoxSearchText.Checked ) {
                this.progressBarCurrent.Visible = true;
            } else {
                this.progressBarCurrent.Visible = false;
            }
        }

        // open/close input controls to search in the output window
        private void buttonX_Click(object sender, EventArgs e) {
            this.findToolStripMenuItem_Click(null, null);
        }
        private void AltF7Form_KeyDown(object sender, KeyEventArgs e) {
            // Ctrl-F is supposed to open the "search in output controls"
            if ( (e.Modifiers == Keys.Control) && (e.KeyCode == Keys.F) && (!this.checkBoxFilesOnly.Checked) ) {
                this.findToolStripMenuItem_Click(null, null);
                this.textBoxFind_TextChanged(null, null);
            }
        }
        private void findToolStripMenuItem_Click(object sender, EventArgs e) {
            this.findToolStripMenuItem_Show(!this.findToolStripMenuItem.Checked);
        }
        private void findToolStripMenuItem_Show(bool bShow) {
            if ( !bShow ) {
                // hide find controls
                this.findToolStripMenuItem.Checked = false;
                this.textBoxFind.Visible = false;
                this.buttonDown.Visible = false;
                this.buttonUp.Visible = false;
                this.buttonX.Visible = false;
                if ( this.checkBoxAdvancedOptions.Checked ) {
                    this.tableLayoutPanel2.SetRowSpan(this.textBoxOutput, 2);
                } else {
                    this.tableLayoutPanel2.SetRowSpan(this.textBoxOutput, 5);
                }
            } else {
                // show find controls
                this.findToolStripMenuItem.Checked = true;
                if ( this.checkBoxAdvancedOptions.Checked ) {
                    this.tableLayoutPanel2.SetRowSpan(this.textBoxOutput, 1);
                } else {
                    this.tableLayoutPanel2.SetRowSpan(this.textBoxOutput, 4);
                }
                this.textBoxFind.Visible = true;
                this.textBoxFind.Focus();
                this.buttonDown.Visible = true;
                this.buttonUp.Visible = true;
                this.buttonX.Visible = true;
            }
        }

        // sort listview
        private readonly string _strAscending = "  \x25b3";
        private readonly string _strDscending = "  \x25bd";
        private void listViewOutput_ColumnClick(object sender, ColumnClickEventArgs e) {
            this.SortListView((ListView)sender, e.Column);
        }
        void SortListView(ListView lvView, int iSortColumn) {
            // save current selection of listview items in a list of strings
            List<string> selectionList = new List<string>();
            foreach ( ListViewItem lvi in lvView.SelectedItems ) {
                selectionList.Add(lvi.Text);
            }

            // connect current listview with sorter class
            lvView.ListViewItemSorter = this.m_lvcs[iSortColumn];

            // reverse the sort direction for this column
            if ( this.m_lvcs[iSortColumn].Order == SortOrder.Ascending ) {
                this.m_lvcs[iSortColumn].Order = SortOrder.Descending;
            } else {
                this.m_lvcs[iSortColumn].Order = SortOrder.Ascending;
            }

            // show "sort order sign" in column text
            for ( int i = 0; i < lvView.Columns.Count; i++ ) {
                lvView.Columns[i].Text = (string)lvView.Columns[i].Tag;
            }
            lvView.Columns[iSortColumn].Text = (string)lvView.Columns[iSortColumn].Tag + (this.m_lvcs[iSortColumn].Order == SortOrder.Ascending ? this._strAscending : this._strDscending);

            // Perform the sort with these new sort options
            lvView.Sort();

            // disconnect sorter class
            lvView.ListViewItemSorter = null;

            // restore original selection
            foreach ( string s in selectionList ) {
                ListViewItem lvi = lvView.FindItemWithText(s);
                if ( lvi != null ) {
                    lvi.Selected = true;
                }
            }
        }
        // This class is an implementation of the 'IComparer' interface.
        public class MyListViewColumnSorter : IComparer {
            /// <summary>
            /// Specifies the column to be sorted
            /// </summary>
            private int ColumnToSort;
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
            public MyListViewColumnSorter() {
                // Initialize the column to '0'
                this.ColumnToSort = 0;

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
                this.str1 = listviewX.SubItems[this.ColumnToSort].Text;
                this.str2 = listviewY.SubItems[this.ColumnToSort].Text;

                string strd1 = listviewX.SubItems[0].Text;
                string strd2 = listviewY.SubItems[0].Text;
                strd1 = strd1.Substring(6, 4) + strd1.Substring(3, 2) + strd1.Substring(0, 2) + strd1.Substring(11, 2) + strd1.Substring(14, 2) + strd1.Substring(17, 2);
                strd2 = strd2.Substring(6, 4) + strd2.Substring(3, 2) + strd2.Substring(0, 2) + strd2.Substring(11, 2) + strd2.Substring(14, 2) + strd2.Substring(17, 2);

                // sort by date
                if ( this.ColumnToSort == 0 ) {
                    try {
                        compareResult = this.ObjectCompare.Compare(ulong.Parse(strd1), ulong.Parse(strd2));
                    } catch ( Exception ) {
                        return 0;
                    }
                }
                // by size
                if ( this.ColumnToSort == 1 ) {
                    try {
                        compareResult = this.ObjectCompare.Compare(Int64.Parse(this.str1), Int64.Parse(this.str2));
                    } catch ( Exception ) {
                        return 0;
                    }
                }
                // by file
                if ( this.ColumnToSort == 2 ) {
                    compareResult = String.Compare(this.str1 + strd1, this.str2 + strd2);
                }
                // by path
                if ( this.ColumnToSort == 3 ) {
                    compareResult = String.Compare(this.str1 + strd1, this.str2 + strd2);
                }
                // by extension
                if ( this.ColumnToSort == 4 ) {
                    compareResult = String.Compare(this.str1 + strd1, this.str2 + strd2);
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

        // OPEN FILE
        private void StartFile(string filename) {
            if ( File.Exists(filename) ) {
                string extension = Path.GetExtension(filename);
                string registeredApp = GrzTools.FileAssociation.Get(extension);
                if ( (registeredApp != null) && (registeredApp != "") ) {
                    System.Diagnostics.Process p = new System.Diagnostics.Process();
                    p.StartInfo.FileName = registeredApp;
                    Process mProcess = new Process();
                    mProcess.StartInfo = new System.Diagnostics.ProcessStartInfo(filename);
                    try {
                        mProcess.Start();
                    } catch ( Exception ) {
                        ProcessStartInfo processInfo = new ProcessStartInfo(filename);
                        processInfo.Verb = "openas";
                        processInfo.ErrorDialog = false;
                        try {
                            Process.Start(processInfo);
                        } catch ( Exception ) {
                            GrzTools.AutoMessageBox.Show("'" + filename + "' is not linked to an executable application.", "Sorry", 2000);
                        }
                    }
                }
            }
        }
        private void listViewOutput_DoubleClick(object sender, EventArgs e) {
            ListView lv = (ListView)sender;
            Point pt = lv.PointToClient(Cursor.Position);
            ListViewItem lvi = lv.GetItemAt(pt.X, pt.Y);
            string filename = Path.Combine(lvi.SubItems[3].Text, lvi.SubItems[2].Text);
            this.StartFile(filename);
        }
        private void openFileToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.listViewOutput.SelectedItems.Count == 0 ) {
                return;
            }
            ListViewItem lvi = this.listViewOutput.SelectedItems[0];
            string filename = Path.Combine(lvi.SubItems[3].Text, lvi.SubItems[2].Text);
            this.StartFile(filename);
        }

        // WINMERGE
        private void winmergePairOfFilesToolStripMenuItem_Click(object sender, EventArgs e) {
            // at minimum we need two selected items
            if ( this.listViewOutput.SelectedItems.Count < 2 ) {
                return;
            }

            // file 1
            ListViewItem lvi = this.listViewOutput.SelectedItems[0];
            string file1 = Path.Combine(lvi.SubItems[3].Text, lvi.SubItems[2].Text);

            // file 2
            lvi = this.listViewOutput.SelectedItems[1];
            string file2 = Path.Combine(lvi.SubItems[3].Text, lvi.SubItems[2].Text);

            // find winmerge
            string winmergePath = GrzTools.InstalledPrograms.ProgramPath("winmerge");
            if ( winmergePath.Length == 0 ) {
                return;
            }

            // start winmerge with two files as parameter
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = winmergePath;
            p.StartInfo.Arguments = " -e -ub " + "\"" + file1 + "\"" + " " + "\"" + file2 + "\"";
            p.Start();
        }
        // when the filelist context menu opens, we check whether "winmerge" could be enabled
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();
        private void contextMenuStripListView_Opening(object sender, CancelEventArgs e) {
            // 20160320: w/o this context menu from Alt-F7 opens always
            IntPtr fgw = GetForegroundWindow();
            if ( (this.WindowState == FormWindowState.Minimized) || (!this.Visible) || (fgw != this.Handle) ) {
                return;
            }
            this.contextMenuStripListView.Items[0].Enabled = this.listViewOutput.SelectedItems.Count == 1 ? true : false;
            this.contextMenuStripListView.Items[1].Enabled = this.listViewOutput.SelectedItems.Count == 1 ? true : false;
            this.contextMenuStripListView.Items[2].Enabled = this.listViewOutput.SelectedItems.Count == 1 ? true : false;
            bool wm1 = GrzTools.InstalledPrograms.ProgramPath("winmerge").Length > 0 ? true : false;
            bool wm2 = this.listViewOutput.SelectedItems.Count == 2 ? true : false;
            this.contextMenuStripListView.Items[4].Enabled = wm1 & wm2;
            this.contextMenuStripListView.Items[7].Enabled = this.listViewOutput.SelectedItems.Count > 0 ? true : false;
            ;
            this.contextMenuStripListView.Items[8].Enabled = this.listViewOutput.SelectedItems.Count > 0 ? true : false;
            ;
            this.contextMenuStripListView.Items[9].Enabled = this.listViewOutput.SelectedItems.Count > 0 ? true : false;
            ;
        }

        // in case we preset today/lastweek/lastmonth, then we reflect this timespan in range
        private void radioButtonToday_CheckedChanged(object sender, EventArgs e) {
            if ( this.radioButtonToday.Checked ) {
                this.dateTimePickerStart.Value = DateTime.Today;
                this.dateTimePickerStop.Value = DateTime.Today.AddDays(+1);
            }
        }
        private void radioButtonLastWeek_CheckedChanged(object sender, EventArgs e) {
            if ( this.radioButtonLastWeek.Checked ) {
                this.dateTimePickerStop.Value = DateTime.Today.AddDays(+1);
                this.dateTimePickerStart.Value = DateTime.Today.AddDays(-7);
            }
        }
        private void radioButtonLastMonth_CheckedChanged(object sender, EventArgs e) {
            if ( this.radioButtonLastMonth.Checked ) {
                this.dateTimePickerStop.Value = DateTime.Today.AddDays(+1);
                this.dateTimePickerStart.Value = DateTime.Today.AddMonths(-1);
            }
        }
        private void radioButtonDateAll_CheckedChanged(object sender, EventArgs e) {
            if ( this.radioButtonDateAll.Checked ) {
                this.dateTimePickerStart.Value = this.dateTimePickerStart.MinDate;
                this.dateTimePickerStop.Value = DateTime.Today.AddDays(+1);
            }
        }
        private void radioButtonRange_CheckedChanged(object sender, EventArgs e) {
            if ( this.radioButtonRange.Checked && this.dateTimePickerStart.Value == this.dateTimePickerStart.MinDate ) {
                this.dateTimePickerStart.Value = DateTime.Today;
                this.dateTimePickerStop.Value = DateTime.Today.AddDays(+1);
            }
        }

        // even when search is ongoing, it's nice to have the list resized
        private void AltF7Form_ResizeEnd(object sender, EventArgs e) {
            this.listViewOutput.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        // delete selected files
        private void deleteSelectedFilesToolStripMenuItem_Click(object sender, EventArgs e) {
            // refine recycle bin operation
            RecycleOption ro = RecycleOption.SendToRecycleBin;

            // list of files to delete
            List<string> source = new List<String>();
            foreach ( ListViewItem lv in this.listViewOutput.SelectedItems ) {
                source.Add(Path.Combine(lv.SubItems[3].Text, lv.SubItems[2].Text));
            }
            if ( source.Count == 0 ) {
                return;
            }

            // check whether drive has a recycle bin at all
            if ( ro == RecycleOption.SendToRecycleBin ) {
                string root = Path.GetPathRoot(source[0]);
                if ( !GrzTools.FileTools.DriveHasRecycleBin(root) ) {
                    string txt = Localizer.GetString("notrash");
                    DialogResult dr = MessageBox.Show(txt, "Attention", MessageBoxButtons.OKCancel);
                    if ( dr == DialogResult.Cancel ) {
                        return;
                    }
                }
            }

            // execute deletion
            this.previewCtl.LoadDocument("", "", null);
            foreach ( string entry in source ) {
                try {
                    FileAttributes attr = File.GetAttributes(@entry);
                    if ( (attr & FileAttributes.Directory) == FileAttributes.Directory ) {
                        try {
                            FileSystem.DeleteDirectory(entry, UIOption.OnlyErrorDialogs, ro);
                        } catch ( OperationCanceledException ) {; }
                    } else {
                        try {
                            FileSystem.DeleteFile(entry, UIOption.OnlyErrorDialogs, ro);
                        } catch ( OperationCanceledException ) {; }
                    }
                } catch ( Exception ) {; }
            }
        }

        // 20170702: copy selection from listview output to windows' clipboard as text
        private void copyListItemsToClipBoardToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.listViewOutput.SelectedItems.Count == 0 ) {
                DialogResult dr = MessageBox.Show("Select all items?", "Selection to few", MessageBoxButtons.YesNo);
                if ( dr != System.Windows.Forms.DialogResult.Yes ) {
                    return;
                }
                this.listViewOutput.BeginUpdate();
                this.listViewOutput.Items.OfType<ListViewItem>().ToList().ForEach(item => item.Selected = true);
                this.listViewOutput.EndUpdate();
            }

            // list of files to copy as text
            string text = "";
            foreach ( ListViewItem lvi in this.listViewOutput.SelectedItems ) {
                text += String.Format("{2}\t{3}\t{0}\t{1}\t{4}\r\n", lvi.SubItems[0].Text, lvi.SubItems[1].Text, lvi.SubItems[2].Text, lvi.SubItems[3].Text, lvi.SubItems[4].Text);
            }
            Clipboard.Clear();
            Clipboard.SetText(text);
        }

        // 20160320: copy selected files from listview output to windows' clipboard
        private void copySelectedFilesToClipBoardToolStripMenuItem_Click(object sender, EventArgs e) {
            // list of files to copy
            StringCollection paths = new StringCollection();
            foreach ( ListViewItem lvi in this.listViewOutput.SelectedItems ) {
                string file = Path.Combine(lvi.SubItems[3].Text, lvi.SubItems[2].Text);
                if ( File.Exists(file) ) {
                    paths.Add(file);
                }
            }
            if ( paths.Count > 0 ) {
                Clipboard.Clear();
                Clipboard.SetFileDropList(paths);
            }
        }

        // file copy from listview output
        private void copySelectedFilesToolStripMenuItem_Click(object sender, EventArgs e) {
            this.CopySelectedFilesFromListViewOutput();
        }
        private void CopyProgressFormClosed(object sender, EventArgs e) {
            this.m_bCancelCopy = true;
        }
        private void CopySelectedFilesFromListViewOutput() {
            // select destination folder
            SelectFolderOrFile sff = new SelectFolderOrFile();
            sff.Text = "Select Destination Folder (or File)";
            sff.DefaultPath = this.m_selectedPath;
            DialogResult dlr = sff.ShowDialog(this);
            if ( dlr != System.Windows.Forms.DialogResult.OK ) {
                return;
            }
            this.m_selectedPath = sff.ReturnPath;
            sff.Dispose();

            // list of files to copy
            List<string> source = new List<String>();
            foreach ( ListViewItem lv in this.listViewOutput.SelectedItems ) {
                source.Add(Path.Combine(lv.SubItems[3].Text, lv.SubItems[2].Text));
            }

            // non modal progress dialog 
            this.m_bCancelCopy = false;
            CopyProgressForm dlg = new CopyProgressForm();
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.Closed += new EventHandler(this.CopyProgressFormClosed);
            dlg.progressBar1.Minimum = 0;
            dlg.progressBar1.Maximum = source.Count;
            dlg.progressBar1.Value = 0;
            dlg.progressBar1.Step = 1;
            dlg.Show();

            // progress vars
            int count = Math.Max(1, source.Count);
            int proc = 0;
            double res;

            // don't ask me again flags (index 0 => always ask: 0=yes 1=no    index 1 => what to do: 0=skip 1=override 2=keep  )
            int[] iOver = { 0, 0 };

            // go thru list and copy previously selected filenames as files 
            int iErrorCount = 0;
            foreach ( string s in source ) {
                // cooperation
                Application.DoEvents();

                // progress
                dlg.label1.Text = s;
                res = (proc++ * 100) / count;
                dlg.label2.Text = ((int)res).ToString() + " %";

                // file copy using streams with progress
                FileInfo info = FileSystem.GetFileInfo(s);
                dlg.progressBar2.Minimum = 0;
                dlg.progressBar2.Maximum = (int)(info.Length / 1024);
                dlg.progressBar2.Value = 0;
                dlg.progressBar2.Step = 1;
                string fileDest = Path.Combine(this.m_selectedPath, Path.GetFileName(s));
                bool bSuccess = false;
                // destination file exists ?
                if ( File.Exists(fileDest) ) {
                    // always behaviour (at least once 
                    if ( iOver[0] == 0 ) {  // always == false --> ask file by file
                        using ( AgainMessage askdlg = new AgainMessage() ) {
                            askdlg.StartPosition = FormStartPosition.Manual;
                            askdlg.Location = new Point(dlg.Location.X - 10, dlg.Location.Y - askdlg.Height + 5);
                            askdlg.Text = "Conflict to resolve with file " + Path.GetFileName(s);
                            askdlg.textBoxMessage.Text = Path.GetFileName(s) + "\r\n\r\nThis file already exists in destination folder " + this.m_selectedPath + ".\r\n\r\nWhat should happen next?";
                            askdlg.textBoxMessage.Select(0, 0);
                            DialogResult askres = askdlg.ShowDialog(this);
                            iOver[0] = askdlg.checkBoxAgain.Checked ? 1 : 0;
                            if ( askres == System.Windows.Forms.DialogResult.Abort ) {
                                break;        // dama was answered with 'cancel all' 
                            }
                            if ( askres == System.Windows.Forms.DialogResult.Yes ) {
                                iOver[1] = 1; // dama was answered with 'overwrite'
                                bSuccess = FileTools.FileCopyEx(s, fileDest, dlg.progressBar2);
                            }
                            if ( askres == System.Windows.Forms.DialogResult.No ) {
                                iOver[1] = 0; // dama was answered with 'skip'
                                bSuccess = true;
                            }
                            if ( askres == System.Windows.Forms.DialogResult.Ignore ) {
                                iOver[1] = 2; // dama was answered with 'keep'
                                int i = 0;
                                string fileDestNew;
                                do {
                                    fileDestNew = Path.Combine(Path.GetDirectoryName(fileDest), Path.GetFileNameWithoutExtension(fileDest)) + "(" + i++.ToString() + ")" + Path.GetExtension(fileDest);
                                } while ( File.Exists(fileDestNew) );
                                fileDest = fileDestNew;
                                bSuccess = FileTools.FileCopyEx(s, fileDest, dlg.progressBar2);
                            }
                        }
                    } else {
                        // always = true --> auto copy, but only if always applies to Yes or Ignore  
                        if ( iOver[1] == 2 ) {
                            int i = 0;
                            string fileDestNew;
                            do {
                                fileDestNew = Path.Combine(Path.GetDirectoryName(fileDest), Path.GetFileNameWithoutExtension(fileDest)) + "(" + i++.ToString() + ")" + Path.GetExtension(fileDest);
                            } while ( File.Exists(fileDestNew) );
                            fileDest = fileDestNew;
                            bSuccess = FileTools.FileCopyEx(s, fileDest, dlg.progressBar2);
                        }
                        if ( iOver[1] == 1 ) {
                            bSuccess = FileTools.FileCopyEx(s, fileDest, dlg.progressBar2);
                        }
                    }
                } else {
                    // simple file copy
                    bSuccess = FileTools.FileCopyEx(s, fileDest, dlg.progressBar2);
                }

                // try to keep original file dates
                DateTime dtCreate = File.GetCreationTime(s);
                DateTime dtWrite = File.GetLastWriteTime(s);
                DateTime dtAccess = File.GetLastAccessTime(s);
                try {
                    File.SetCreationTime(fileDest, dtCreate);
                    File.SetLastWriteTime(fileDest, dtWrite);
                    File.SetLastAccessTime(fileDest, dtAccess);
                } catch ( Exception ) {
                    ;
                }

                // error
                if ( !bSuccess ) {
                    iErrorCount++;
                }

                // progress on file count
                dlg.progressBar1.PerformStep();

                // cancel thru progress dlg
                if ( this.m_bCancelCopy )
                    break;
            }

            // error
            if ( iErrorCount > 0 ) {
                MessageBox.Show("There were " + iErrorCount.ToString() + " ERRORs while copying files.", "Error");
            }

            // end progress
            dlg.Close();
        }

        // open containing folder in a new instance of cfw
        private void jumpToFileToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.listViewOutput.SelectedItems.Count == 0 ) {
                return;
            }
            try {
                string program = Application.ExecutablePath;
                ListViewItem lvi = this.listViewOutput.SelectedItems[0];
                string filename = Path.Combine(lvi.SubItems[3].Text, lvi.SubItems[2].Text);
                if ( File.Exists(filename) ) {
                    Process.Start(program, filename);
                }
            } catch ( Exception ) {; }
        }

        // return the selected folder to cfw
        private void closeAndJumpToFileToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.listViewOutput.SelectedItems.Count == 0 ) {
                return;
            }
            ListViewItem lvi = this.listViewOutput.SelectedItems[0];
            string filename = "";
            try {
                filename = Path.Combine(lvi.SubItems[3].Text, lvi.SubItems[2].Text);
            } catch {
                return;
            }
            if ( File.Exists(filename) ) {
                EventHandler<LoadFolderEventArgs> handler = LoadFolderRequest;
                if ( handler != null ) {
                    handler(null, new LoadFolderEventArgs(filename));
                    this.Close();
                }
            }
        }

        private void comboBoxBuzzword_TextChanged(object sender, EventArgs e) {
            if ( !this.m_bInitIsOver ) {
                return;
            }

            this.checkBoxSearchText.Checked = true;
            this.checkBoxSearchText_Click(null, null);
        }

        private void checkBoxSkipSearchingBinaryFiles_CheckedChanged(object sender, EventArgs e) {
            this.m_bSkipBin = this.checkBoxSkipSearchingBinaryFiles.Checked;
        }

        private void textBoxHugeFileLineCountLimit_TextChanged(object sender, EventArgs e) {
            int num = (int)this.numericUpDownHugeFileLineCountLimit.Value;
        }

        // live refresh is much faster when clicking once into the textbox as to scroll the caret
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_MOVE = 0x0001;
        public void DoMouseClick(Control ctl) {
            Point curPos = Cursor.Position;
            Point pt = ctl.PointToScreen(new Point(ctl.Location.X + 5, ctl.Location.Y + 5));
            Cursor.Position = pt;
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            Cursor.Position = curPos;
        }
        private void checkBoxLiveRefresh_CheckedChanged(object sender, EventArgs e) {
            if ( this.checkBoxLiveRefresh.Checked ) {
                this.DoMouseClick(this.textBoxOutput);
            } else {
                this.textBoxOutput.ScrollToCaret();
            }
        }

        private void textBoxOutput_MouseDown(object sender, MouseEventArgs e) {
            this.checkBoxLiveRefresh.CheckedChanged -= new System.EventHandler(this.checkBoxLiveRefresh_CheckedChanged);
            if ( this.checkBoxLiveRefresh.Checked ) {
                // I don't get it to work, the logic is ok
                //                this.checkBoxLiveRefresh.Checked = false;
                //                this.textBoxOutput.ScrollToCaret();
            } else {
                this.checkBoxLiveRefresh.Checked = true;
                this.DoMouseClick(this.textBoxOutput);
            }
            this.checkBoxLiveRefresh.CheckedChanged += new System.EventHandler(this.checkBoxLiveRefresh_CheckedChanged);
        }

        string m_lastFileName = "";
        Boolean m_lstViewHasFocus = false;
        private void listViewOutput_SelectedIndexChanged(object sender, EventArgs e) {
            if ( this.listViewOutput.SelectedItems.Count == 0 ) {
                return;
            }
            ListViewItem lvi = this.listViewOutput.SelectedItems[0];
            string filename = "";
            try {
                filename = Path.Combine(lvi.SubItems[3].Text, lvi.SubItems[2].Text);
            } catch {
                return;
            }
            if ( this.m_lastFileName == filename ) {
                return;
            }
            this.previewCtl.LoadDocument("", "", null);
            try {
                // file association check
                FileVersionInfo wmpInfo = FileVersionInfo.GetVersionInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Windows Media Player", "wmplayer.exe"));
                string ext = Path.GetExtension(filename);
                string typename = "";
                if ( ".avi.AVI.wmv.WMV.mp4.MP4.mov.MOV".Contains(ext) && (wmpInfo.FileName.Length > 0) ) {
                    typename = "WMP";
                }
                this.m_lastFileName = filename;
                this.previewCtl.LoadDocument(filename, typename, null);
            } catch {; }
            if ( !this.m_lstViewHasFocus ) {
                this.listViewOutput.Focus();
            }
        }

        private void previewCtl_Leave(object sender, EventArgs e) {
            this.m_lstViewHasFocus = false;
        }

        private void previewCtl_Enter(object sender, EventArgs e) {
            //            m_lstViewHasFocus = true;
        }

        // checkbox EXACT shall force for real exact filename: if user types a wildcard, an EXACT setting would not fly anymore, so reset it 
        private void textBoxFilePattern_TextChanged(object sender, EventArgs e) {
            string text = this.textBoxFilePattern.Text;
            if ( text.Contains("*") || text.Contains("?") ) {
                this.checkBoxExact.Checked = false;
            }
        }
        // if user forces EXACT search, strip wildcards from filename pattern
        private void checkBoxExact_CheckedChanged(object sender, EventArgs e) {
            if ( this.checkBoxExact.Checked ) {
                string text = this.textBoxFilePattern.Text.Replace("*", "");
                text = text.Replace("?", "");
                this.textBoxFilePattern.Text = text;
            }
        }

    }
}
