using Microsoft.VisualBasic.FileIO;         // delete to trash bin & alternative IO
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;                   // process & stopwatch
using System.Drawing;
using System.Globalization;                 // CultureInfo.InvariantCulture
using System.IO;                            // Path
using System.Linq;
using System.Runtime.InteropServices;       // DLLImport
using System.Threading;                     // tasks
using System.Windows.Forms;

namespace cfw {
    public partial class FileComparer : Form, IMessageFilter {
        readonly MainForm m_Parent = null;
        bool m_bRun = false;
        readonly string m_buttonRefreshText = "";
        readonly List<string> headersA;
        readonly List<string> headersB;

        public FileComparer(MainForm parent, List<string> headersA, List<string> headersB) {
            this.InitializeComponent();
            this.m_buttonRefreshText = this.buttonRefresh.Text;
            this.m_Parent = parent;

            this.headersA = headersA;
            this.headersB = headersB;

            this.listView1.Columns[0].Text = "#";
            this.listView1.Columns[2].Text = "status";
            if ( this.headersA.Count == 1 ) {
                this.listView1.Columns[1].Text = this.headersA[0];
                this.listView1.Columns[3].Text = this.headersB[0];
            } else {
                this.listView1.Columns[1].Text = "CfW left panel selection";
                this.listView1.Columns[3].Text = "CfW right panel selection";
            }

            //          autostart via timer: I don't like it anymore
            //            timerFirstStart.Start();

            // IMessageFilter
            // - also needed: class declaration "public partial class MainForm: Form, IMessageFilter"
            // - also needed: event handler "public bool PreFilterMessage( ref Message m )"
            // - also needed: Application.RemoveMessageFilter(this) when closing this form
            Application.AddMessageFilter(this);
        }

        // timer handler starts folder comparison
        private void timerFirstStart_Tick(object sender, EventArgs e) {
            this.timerFirstStart.Stop();
            this.buttonRefresh_Click(null, null);
        }

        // IMessageFilter: intercept messages 
        public bool PreFilterMessage(ref Message m) {
            // ListView right mouse down: the ONLY way to keep the current selectionm AND to open a context menu
            if ( (m.Msg == 0x204) && this.listView1.Visible && (this.WindowState != FormWindowState.Minimized) ) {
                // set focus
                this.listView1.Focus();
                // open context menu
                this.contextMenuStrip.Show(MousePosition);
                // prevents resetting the the current listview selection
                return true;
            }
            return false;
        }

        // ListView behaviour
        bool m_bSelectRule;                                     // select: that is the rule how to select mouse moved items
        bool m_bStatusBeforeMouseDown;                          // select: selection status of item befor mouse went down 
        Point m_ptMove = new Point(0, 0);                       // select: mouse position after fake 4 pixel mouse move   
        int[] m_sic = new int[1];
        private void listView1_MouseDown(object sender, MouseEventArgs e) {
            if ( (e.Button == MouseButtons.Left) && ((Control.ModifierKeys & Keys.Shift) == 0) && ((Control.ModifierKeys & Keys.Control) == 0) ) {
                ListView lv = (ListView)sender;

                Point mousePos = lv.PointToClient(Control.MousePosition);
                ListViewHitTestInfo hitTest = lv.HitTest(mousePos);
                if ( hitTest == null ) {
                    return;
                }
                if ( hitTest.SubItem == null ) {
                    return;
                }
                int columnIndex = hitTest.Item.SubItems.IndexOf(hitTest.SubItem);
                if ( columnIndex == 2 ) {
                    string copyDir = "";
                    // click item with folder: all folder items (but files only) with alter its copy direction together with the folder
                    if ( hitTest.Item.BackColor == Color.CadetBlue ) {
                        // affect the folder item
                        if ( (hitTest.Item.SubItems[2].Text == "") || (hitTest.Item.SubItems[2].Text == "<?>") ) {
                            copyDir = "--->";
                        } else {
                            if ( hitTest.Item.SubItems[2].Text == "--->" ) {
                                copyDir = "<---";
                            } else {
                                if ( hitTest.Item.SubItems[2].Text == "<---" ) {
                                    copyDir = "XXX";
                                } else {
                                    if ( hitTest.Item.SubItems[2].Text == "XXX" ) {
                                        copyDir = "<?>";
                                    }
                                }
                            }
                        }
                        // affect the folder's files 
                        int startIndex = Math.Min(hitTest.Item.Index + 1, lv.Items.Count);
                        hitTest.Item.SubItems[2].Text = copyDir;
                        for ( int i = startIndex; i < lv.Items.Count; i++ ) {
                            if ( lv.Items[i].BackColor != Color.CadetBlue ) {
                                if ( lv.Items[i].SubItems[2].Text != "" ) {
                                    lv.Items[i].SubItems[2].Text = copyDir;
                                }
                            } else {
                                break;
                            }
                        }
                    } else {
                        // everytime a file was hit, we reset the folder the file belongs to = ""
                        int startIndex = Math.Max(hitTest.Item.Index - 1, 0);
                        for ( int i = startIndex; i >= 0; i-- ) {
                            if ( lv.Items[i].BackColor == Color.CadetBlue ) {
                                lv.Items[i].SubItems[2].Text = "";
                                break;
                            }
                        }
                        // then we alter the file itself
                        if ( hitTest.Item.SubItems[2].Text == "<?>" ) {
                            hitTest.Item.SubItems[2].Text = "--->";
                            return;
                        }
                        if ( hitTest.Item.SubItems[2].Text == "--->" ) {
                            hitTest.Item.SubItems[2].Text = "<---";
                            return;
                        }
                        if ( hitTest.Item.SubItems[2].Text == "<---" ) {
                            hitTest.Item.SubItems[2].Text = "XXX";
                            return;
                        }
                        if ( hitTest.Item.SubItems[2].Text == "XXX" ) {
                            hitTest.Item.SubItems[2].Text = "<?>";
                            return;
                        }
                    }
                }

                ListViewItem item = lv.GetItemAt(e.X, e.Y);
                if ( item != null ) {
                    this.m_bStatusBeforeMouseDown = item.Selected;
                }

                // memorize current selection: we restore this selection at "Mouse Up" in case selection was meanwhile deselected 
                this.m_sic = new int[lv.SelectedIndices.Count];
                lv.SelectedIndices.CopyTo(this.m_sic, 0);

                if ( item == null ) {
                    GrzTools.DrawingControl.ResumeDrawing(lv);
                    return;
                }

                // setup the selection status rule based on the selection status of the hovered (ie. current) item
                this.m_bSelectRule = true;
                if ( lv.SelectedItems.Count != 1 ) {  // if we only have 1 selected item, we take over this status which is always true  
                    this.m_bSelectRule = !item.Selected;
                }

                // this is essential to allow a 'non connected multi selection', otherwise ANY mouse down resets any selection 
                item.Selected = true;
            }
        }
        ListViewItem _lastMouseMoveItem = null;
        private void listView1_MouseMove(object sender, MouseEventArgs e) {
            // select / deselect items when mouse moves based on the selection rule established in 'mouse down' 
            if ( e.Button == MouseButtons.Left ) {

                // 
                ListView lv = (ListView)sender;
                ListViewItem item = lv.GetItemAt(e.X, e.Y);
                if ( item == null ) {
                    return;
                }

                // we want to apply the selection rule
                item.Selected = this.m_bSelectRule;

                // ensure there is always at least one selected item
                if ( lv.SelectedItems.Count == 0 ) {
                    item.Selected = true;
                }
            }
            // 20160824 magic: select all --> restore last status --> hovering selected items made them disappear  !!! therefore we force a single redraw when hovering a selected item !!!
            if ( e.Button == MouseButtons.None ) {
                ListView lv = (ListView)sender;
                ListViewItem item = lv.GetItemAt(e.X, e.Y);
                if ( item == null ) {
                    return;
                }
                // using _lastMouseMoveItem prevents item from flickering while hover
                if ( item.Selected && (item != this._lastMouseMoveItem) ) {
                    this._lastMouseMoveItem = item;
                    this.listView1.Invalidate(item.Bounds, false);
                    this.listView1.Update();
                }
            }
        }
        private void listView1_MouseUp(object sender, MouseEventArgs e) {
            ListView lv = (ListView)sender;
            ListViewItem item = lv.GetItemAt(e.X, e.Y);
            if ( item == null ) {
                if ( lv.Items.Count == 0 ) {
                    return;
                }
                if ( lv.SelectedItems.Count == 0 ) {
                    // make sure, we have the same selection at "Mouse Up" as it had been at "Mouse Down" 
                    foreach ( int i in this.m_sic ) {
                        lv.Items[i].Selected = true;
                    }
                }
                return;
            }

            // ensure there is always at least one selected item
            if ( lv.SelectedItems.Count == 0 ) {
                item.Selected = true;
            }
            return;
        }

        // insert / add a listview item
        void InsertListViewRecordAtPos(ListView lv, int iInsertPos, string col1, string col2, string col3, string col4, bool bHeadline, bool bAlterBackColor) {
            ListViewItem lvi;
            string[] strarr;
            strarr = new string[4] { col1, col2, col3, col4 };
            lvi = new ListViewItem(strarr);
            // 20160130: save original copy direction in .Tag
            lvi.SubItems[2].Tag = lvi.SubItems[2].Text;
            if ( bHeadline ) {
                lvi.BackColor = Color.CadetBlue;
            }
            if ( lv.Items.Count > 0 ) {
                Color prevColor = lv.Items[lv.Items.Count - 1].BackColor;
                if ( !bHeadline ) {
                    if ( bAlterBackColor ) {
                        if ( prevColor == SystemColors.Window ) {
                            lvi.BackColor = Color.Beige;
                        }
                    } else {
                        lvi.BackColor = prevColor;
                    }
                }
            }
            lv.Items.Insert(iInsertPos, lvi);
        }
        void MakeListViewRecord(ListView lv, string col1, string col2, string col3, string col4, bool bHeadline, bool bAlterBackColor) {
            ListViewItem lvi;
            string[] strarr;
            strarr = new string[4] { col1, col2, col3, col4 };
            lvi = new ListViewItem(strarr);
            // 20160130: save original copy direction in .Tag
            lvi.SubItems[2].Tag = lvi.SubItems[2].Text;
            if ( bHeadline ) {
                lvi.BackColor = Color.CadetBlue;
            }
            if ( lv.Items.Count > 0 ) {
                Color prevColor = lv.Items[lv.Items.Count - 1].BackColor;
                if ( !bHeadline ) {
                    if ( bAlterBackColor ) {
                        if ( prevColor == SystemColors.Window ) {
                            lvi.BackColor = Color.Beige;
                        }
                    } else {
                        lvi.BackColor = prevColor;
                    }
                }
            }
            lv.Items.Add(lvi);
            lv.Items[lv.Items.Count - 1].EnsureVisible();
        }

        // main work 
        bool CompareFolders(ListView lv, string pathA, string pathB, bool bSubDirectories) {
            List<string> errorList = new List<string>();

            this.checkBoxXXX.Checked = false;
            this.checkBoxEoNA.Checked = false;
            this.checkBoxToLeft.Checked = false;
            this.checkBoxToRight.Checked = false;

            // no compare rule == no comparison
            if ( !this.checkBox1by1.Checked && !this.checkBoxLength.Checked && !this.checkBoxLastWrite.Checked && !this.checkBoxExistence.Checked ) {
                return true;
            }

            // reset all controls & vars
            this.textBoxA.Text = "";
            this.textBoxB.Text = "";
            int iFilesA = 0;
            int iFilesAnoB = 0;
            //            int iFoldersAnoB = 0;
            int iFilesB = 0;
            int iFilesBnoA = 0;
            //int iFoldersBnoA = 0;
            int iFilesABmismatch = 0;

            // counter for differences 
            int iDiffCount = 0;

            // placeholder for the name of the directory in B
            string sFolderB = "n/a";

            // live update or not
            this.listView1.Invalidate(true);
            Application.DoEvents();
            if ( this.checkBoxLiveView.Checked ) {
                this.listView1.EndUpdate();
            } else {
                this.listView1.BeginUpdate();
            }

            // get list of folders in A and B
            List<FileInfo> listA;
            List<FileInfo> listB;
            DirectoryInfo dirA = new DirectoryInfo(pathA);
            DirectoryInfo dirB = new DirectoryInfo(pathB);
            List<string> listFoldersA = new List<string>();
            List<string> listFoldersB = new List<string>();

            // "top level only" shall ignore all deep down subfolders: useful because it's more sophisticated as a simple list compare in MainForm
            if ( this.checkBoxTopLevel.Checked ) {
                listFoldersA = new GrzTools.FastDirectoryEnumerator().GetDirectories(pathA, "*.*").ToList();
                listFoldersB = new GrzTools.FastDirectoryEnumerator().GetDirectories(pathB, "*.*").ToList();
            } else {
                listFoldersA = new GrzTools.FastDirectoryEnumerator().GetAllDirectories(pathA).ToList();
                listFoldersB = new GrzTools.FastDirectoryEnumerator().GetAllDirectories(pathB).ToList();
            }
            listFoldersA.Insert(0, pathA);
            listFoldersB.Insert(0, pathB);
            this.progressBar1.Value = 0;

            // ONLY progress maximum: will be shown based on folder counts in A & B
            this.progressBar1.Maximum = listFoldersA.Count() + listFoldersB.Count;

            //
            // outmost loop over all directories in A
            //
            foreach ( string folderA in listFoldersA ) {

                // path too long
                if ( folderA.Length > 247 ) {
                    errorList.Add(folderA);
                    continue;
                }

                this.progressBar1.PerformStep();
                Application.DoEvents();
                if ( !this.m_bRun ) {
                    return false;
                }
                this.Text = "checking left to right --->";

                // file mask is the most important input
                string filemask = "";
                try {
                    filemask = this.comboBoxWildCard.SelectedItem.ToString();
                } catch ( Exception ) {; }

                // flag to show a foldername as headline for later if needed (aka: a file deviates or is missing)
                bool bShowFolderAB = true;

                // build an A filelist based on current folder A
                listA = new List<FileInfo>();
                if ( (!this.checkBoxTopLevel.Checked) || (folderA == listFoldersA[0]) ) {
                    listA = GrzTools.FastDirectoryEnumerator.GetFiles(folderA, filemask).ToList();
                }

                // check whether folderA exists in B and memorize that info (as foldername or n/a) for later use
                string sCheckFolderB = pathB + folderA.Substring(pathA.Length);
                if ( Directory.Exists(sCheckFolderB) ) {
                    sFolderB = sCheckFolderB;
                } else {
                    if ( this.checkBox1by1.Checked ) {
                        this.MakeListViewRecord(lv, "", folderA, "", "n/a", true, false);
                    }
                }

                //
                // 2nd level inner loop across all A files of the given A folder and check whether they have counterparts in B
                //
                foreach ( FileInfo fileA in listA ) {
                    // path too long
                    if ( fileA.FullName.Length > 259 ) {
                        errorList.Add(fileA.FullName);
                        continue;
                    }
                    if ( !this.m_bRun ) {
                        return false;
                    }
                    this.textBoxA.BackColor = SystemColors.Control;
                    this.textBoxA.Text = fileA.FullName;
                    Application.DoEvents();
                    iFilesA++;
                    // take fullpath from file A but pathA + add it to pathB and check existence of that very file
                    string sCheckFileB = pathB + fileA.FullName.Substring(pathA.Length);
                    if ( File.Exists(sCheckFileB) ) {
                        this.textBoxB.BackColor = SystemColors.Control;
                        this.textBoxB.Text = sCheckFileB;
                        FileInfo fileB = new FileInfo(sCheckFileB);
                        bool bFileMisMatch = false;
                        if ( (fileA.Length != fileB.Length) && this.checkBoxLength.Checked ) {
                            bFileMisMatch = true;
                        }
                        if ( (fileA.LastWriteTime != fileB.LastWriteTime) && this.checkBoxLastWrite.Checked ) {
                            if ( this.checkBoxGrant5Sec.Checked ) {
                                TimeSpan ts = new TimeSpan(fileA.LastWriteTime.Ticks - fileB.LastWriteTime.Ticks);
                                if ( Math.Abs(ts.Ticks) > (TimeSpan.TicksPerSecond * 5) ) {
                                    bFileMisMatch = true;
                                }
                            } else {
                                bFileMisMatch = true;
                            }
                        }
                        // SHOW: file A deviates from file B
                        if ( bFileMisMatch || this.checkBox1by1.Checked ) {
                            // files are different by default
                            string cmpSign = "<!>";
                            // generate a folder item (if needed) in listview
                            if ( bShowFolderAB ) {
                                bShowFolderAB = false;
                                this.MakeListViewRecord(lv, "", folderA, "", sFolderB, true, false);
                            }
                            iFilesABmismatch++;
                            // generate a filename item in listview
                            this.MakeListViewRecord(lv, (++iDiffCount).ToString(), fileA.Name, cmpSign, fileB.Name, false, true);
                            // file details
                            string fileDetailA = fileA.LastWriteTime.ToString("dd.MM.yyyy  HH:mm:ss") + "  " + fileA.Length.ToString("0,0", CultureInfo.InvariantCulture);
                            string fileDetailB = fileB.LastWriteTime.ToString("dd.MM.yyyy  HH:mm:ss") + "  " + fileB.Length.ToString("0,0", CultureInfo.InvariantCulture);
                            // override comparison status, if file details are identical
                            if ( fileDetailA == fileDetailB ) {
                                lv.Items[lv.Items.Count - 1].SubItems[2].Text = "==";
                                lv.Items[lv.Items.Count - 1].SubItems[2].Tag = lv.Items[lv.Items.Count - 1].SubItems[2].Text;
                            } else {
                                // at 1:1 we don't care about anything
                                if ( this.checkBox1by1.Checked ) {
                                    lv.Items[lv.Items.Count - 1].UseItemStyleForSubItems = false;
                                    lv.Items[lv.Items.Count - 1].SubItems[1].BackColor = lv.Items[lv.Items.Count - 1].SubItems[0].BackColor;
                                    lv.Items[lv.Items.Count - 1].SubItems[2].BackColor = Color.Red;
                                    lv.Items[lv.Items.Count - 1].SubItems[3].BackColor = lv.Items[lv.Items.Count - 1].SubItems[0].BackColor;
                                }
                                // 20160130: time comparision at mismatching files: if a file is newer, we suggest to copy it to the other side
                                if ( this.checkBoxLastWrite.Checked ) {
                                    if ( (fileA.LastWriteTime > fileB.LastWriteTime) ) {
                                        lv.Items[lv.Items.Count - 1].SubItems[2].Text = "--->";
                                    } else {
                                        lv.Items[lv.Items.Count - 1].SubItems[2].Text = "<---";
                                    }
                                    lv.Items[lv.Items.Count - 1].SubItems[2].Tag = lv.Items[lv.Items.Count - 1].SubItems[2].Text;
                                }
                            }
                            // generate file details item in listview
                            this.MakeListViewRecord(lv, "", fileDetailA, "", fileDetailB, false, false);
                        }

                    } else {
                        if ( this.checkBoxExistence.Checked || this.checkBox1by1.Checked ) {
                            this.textBoxB.BackColor = Color.Red;
                            this.textBoxB.Text = sCheckFileB;
                            // SHOW: file A is missing in B
                            if ( bShowFolderAB ) {
                                bShowFolderAB = false;
                                this.MakeListViewRecord(lv, "", folderA, "", sFolderB, true, false);
                            }
                            iFilesAnoB++;
                            this.MakeListViewRecord(lv, (++iDiffCount).ToString(), fileA.Name, "--->", "n/a", false, true);
                            string fileDetailA = fileA.LastWriteTime.ToString("dd.MM.yyyy  HH:mm:ss") + "  " + fileA.Length.ToString("0,0", CultureInfo.InvariantCulture);
                            this.MakeListViewRecord(lv, "", fileDetailA, "", "", false, false);
                        }
                    }
                }
            }


            //
            // outmost loop over all directories in B
            //
            foreach ( string folderB in listFoldersB ) {

                // path too long
                if ( folderB.Length > 247 ) {
                    errorList.Add(folderB);
                    continue;
                }

                this.progressBar1.PerformStep();
                Application.DoEvents();
                if ( !this.m_bRun ) {
                    return false;
                }
                this.Text = "<--- checking right to left";

                // file mask is the most important input
                string filemask = "";
                try {
                    filemask = this.comboBoxWildCard.SelectedItem.ToString();
                } catch ( Exception ) {; }

                // flag to show a foldername as headline for later if needed (aka: a file deviates or is missing)
                bool bShowFolderAB = true;

                // build an B filelist based on current folder B
                listB = new List<FileInfo>();
                if ( (!this.checkBoxTopLevel.Checked) || (folderB == listFoldersB[0]) ) {
                    listB = GrzTools.FastDirectoryEnumerator.GetFiles(folderB, filemask).ToList();
                }

                // check whether folderB exists in A and memorize that info (as foldername or n/a) for later use
                string sCheckFolderA = pathA + folderB.Substring(pathB.Length);
                if ( !Directory.Exists(sCheckFolderA) ) {
                    if ( this.checkBox1by1.Checked ) {
                        this.MakeListViewRecord(lv, "", "n/a", "", folderB, true, false);
                    }
                }

                //
                // 2nd level inner loop across all B files of the given B folder and check whether they have counterparts in A
                //
                // we may need an insert position right under the folder name item
                int iInsertPos = this.FindSubItem3(lv, folderB);
                foreach ( FileInfo fileB in listB ) {

                    // path too long
                    if ( fileB.FullName.Length > 259 ) {
                        errorList.Add(fileB.FullName);
                        continue;
                    }

                    if ( !this.m_bRun ) {
                        return false;
                    }
                    this.textBoxB.BackColor = SystemColors.Control;
                    this.textBoxB.Text = fileB.FullName;
                    Application.DoEvents();
                    iFilesB++;
                    // take fullpath from file A but pathA + add it to pathB and check existence of that very file
                    string sCheckFileA = pathA + fileB.FullName.Substring(pathB.Length);
                    if ( !File.Exists(sCheckFileA) ) {
                        // we only care about files B, which not exist in A - because already existing files were checked in the previous outer loop
                        if ( this.checkBoxExistence.Checked || this.checkBox1by1.Checked ) {
                            this.textBoxA.BackColor = Color.Red;
                            this.textBoxA.Text = sCheckFileA;
                            // SHOW: file B is missing in A
                            if ( bShowFolderAB ) {
                                bShowFolderAB = false;
                                // that's the case, when a folder B does not exist in A
                                if ( iInsertPos == -1 ) {
                                    this.MakeListViewRecord(lv, "", sCheckFolderA, "", folderB, true, false);
                                }
                            }
                            iFilesBnoA++;
                            if ( iInsertPos != -1 ) {
                                this.InsertListViewRecordAtPos(lv, iInsertPos + 1, (++iDiffCount).ToString(), "n/a", "<---", fileB.Name, false, true);
                            } else {
                                this.MakeListViewRecord(lv, (++iDiffCount).ToString(), "n/a", "<---", fileB.Name, false, true);
                            }
                            string fileDetailB = fileB.LastWriteTime.ToString("dd.MM.yyyy  HH:mm:ss") + "  " + fileB.Length.ToString("0,0", CultureInfo.InvariantCulture);
                            if ( iInsertPos != -1 ) {
                                this.InsertListViewRecordAtPos(lv, iInsertPos + 2, "", "", "", fileDetailB, false, false);
                            } else {
                                this.MakeListViewRecord(lv, "", "", "", fileDetailB, false, false);
                            }
                        }
                    }
                }
            }

            // re numbering because of InsertListViewRecordAtPos
            int pos = 1;
            int cnt = 1;
            for ( int i = 0; i < lv.Items.Count; i++ ) {
                if ( lv.Items[i].BackColor != Color.CadetBlue ) {
                    if ( pos % 2 != 0 ) {
                        lv.Items[i].Text = cnt.ToString();
                        cnt++;
                    }
                    pos++;
                }
            }

            // unconditional  refresh
            if ( lv.Items.Count > 0 ) {
                lv.EnsureVisible(0);
            }
            for ( int i = 0; i < 20; i++ ) {
                this.listView1.EndUpdate();
            }
            this.listView1.Invalidate(true);

            // just progress
            this.progressBar1.Value = this.progressBar1.Maximum;
            // resize columns
            this.FileComparer_Resize(null, null);
            // results
            this.textBoxA.BackColor = SystemColors.Control;
            this.textBoxA.Text = "Files:\r\n~~~~\r\nlhs total:\t" + iFilesA.ToString() + "\r\nnot in rhs:\t" + iFilesAnoB.ToString();
            this.textBoxB.BackColor = SystemColors.Control;
            this.textBoxB.Text = "Files:\r\n~~~~\r\nrhs total:\t" + iFilesB.ToString() + "\r\nnot in lhs:\t" + iFilesBnoA.ToString();
            // reset progress
            this.progressBar1.Value = 0;
            // failures
            if ( errorList.Count > 0 ) {
                string failures = "";
                foreach ( string s in errorList ) {
                    failures += s + "\r\r\r\n";
                }
                MessageBox.Show(failures, "Note: the following foldernames are too long to process");
            }
            // get out
            return true;
        }
        // helper returns listview index of a given text found in subitem 3: needed to get the index position of a folder item, which allows to insert items at the right place
        int FindSubItem3(ListView lv, string text) {
            int ret = -1;
            for ( int i = 0; i < lv.Items.Count; i++ ) {
                if ( lv.Items[i].SubItems[3].Text == text ) {
                    ret = i;
                    break;
                }
            }
            return ret;
        }

        // main button
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool Wow64RevertWow64FsRedirection(IntPtr ptr);
        private void buttonRefresh_Click(object sender, EventArgs e) {
            if ( this.buttonRefresh.Text == this.m_buttonRefreshText ) {
                this.buttonRefresh.Text = "- break -";
                Application.DoEvents();
                Cursor.Current = Cursors.WaitCursor;

                string currentFormText = this.Text;

                this.listView1.Items.Clear();

                this.m_bRun = true;
                // Disable redirection
                IntPtr wow64Value = IntPtr.Zero;
                try {
                    Wow64DisableWow64FsRedirection(ref wow64Value);
                } catch ( Exception ex ) {
                    MessageBox.Show(ex.Message);
                }
                try {
                    bool bRetval = true;
                    for ( int i = 0; i < this.headersA.Count; i++ ) {
                        string headerA = this.headersA[i];
                        string headerB = this.headersB[i];
                        bRetval = this.CompareFolders(this.listView1, headerA, headerB, true);
                    }
                    if ( !bRetval ) {
                        GrzTools.AutoMessageBox.Show("Interrupted by User", "File Comparer", 2000);
                    } else {
                        if ( this.listView1.Items.Count == 0 ) {
                            MessageBox.Show("According to the selected comparison rules, both sides are identical.", "Finished");
                        }
                    }
                } catch ( Exception ex ) {
                    MessageBox.Show(ex.Message, "comparison failed");
                    // enable redirection
                    try {
                        Wow64RevertWow64FsRedirection(wow64Value);
                    } catch ( Exception exc ) {
                        MessageBox.Show(exc.Message);
                    }
                }
                // enable redirection
                try {
                    Wow64RevertWow64FsRedirection(wow64Value);
                } catch ( Exception ex ) {
                    MessageBox.Show(ex.Message);
                }
                this.m_bRun = false;
                this.buttonRefresh.Text = this.m_buttonRefreshText;
                this.Text = currentFormText;

                Cursor.Current = Cursors.Default;
            } else {
                this.m_bRun = false;
                this.buttonRefresh.Text = this.m_buttonRefreshText;
            }
        }

        private void FileComparer_FormClosing(object sender, FormClosingEventArgs e) {
            // stop comparing
            this.m_bRun = false;
            this.buttonRefresh.Text = this.m_buttonRefreshText;
            // save file masks to INI
            GrzTools.IniFile ini = new GrzTools.IniFile(System.Windows.Forms.Application.ExecutablePath + ".ini");
            List<string> buzzlist = new List<string>();
            for ( int i = 0; i < 5; i++ ) {
                if ( i < this.comboBoxWildCard.Items.Count ) {
                    ini.IniWriteValue("cfw", "filecomparer" + i.ToString(), this.comboBoxWildCard.GetItemText(this.comboBoxWildCard.Items[i]));
                } else {
                    ini.IniWriteValue("cfw", "filecomparer" + i.ToString(), null);
                }
            }
            // save file mask index to INI
            ini.IniWriteValue("cfw", "filecomparerNdx", this.comboBoxWildCard.SelectedIndex.ToString());
            // INI: start position & window size
            ini.IniWriteValue("cfw", "filecomparerStartPositionX", this.Location.X.ToString());
            ini.IniWriteValue("cfw", "filecomparerStartPositionY", this.Location.Y.ToString());
            ini.IniWriteValue("cfw", "filecomparerWidth", this.Size.Width.ToString());
            ini.IniWriteValue("cfw", "filecomparerHeight", this.Size.Height.ToString());

            // IMessageFilter
            Application.RemoveMessageFilter(this);
        }

        // fit columns
        private void FileComparer_Resize(object sender, EventArgs e) {
            this.listView1.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
            this.listView1.Columns[0].Width = Math.Max(40, this.listView1.Columns[0].Width);
            this.listView1.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);
            int iCol1Width = (this.ClientRectangle.Width - 140) / 2;
            this.listView1.Columns[1].Width = Math.Max(iCol1Width, this.listView1.Columns[1].Width);
            this.listView1.AutoResizeColumn(2, ColumnHeaderAutoResizeStyle.ColumnContent);
            this.listView1.Columns[2].Width = Math.Max(100, this.listView1.Columns[2].Width);
            this.listView1.AutoResizeColumn(3, ColumnHeaderAutoResizeStyle.ColumnContent);
            this.listView1.Columns[this.listView1.Columns.Count - 1].Width = -2;  // trick from MSDN: http://msdn.microsoft.com/en-us/library/system.windows.forms.columnheader.width.aspx
            // restore selection status
            this.SelectItemsAccordingToStatus();
        }

        // OwnerDraw = true: we need to take care about _DrawItem, _DrawSubItem, _DrawColumnHeader
        private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e) {
            if ( e.Item.Selected ) {
                // keep selection always visible, even when focus got lost
                e.Graphics.FillRectangle(Brushes.CornflowerBlue, e.Bounds);
                e.DrawText();
            } else {
                if ( e.Item.SubItems[2].Text != e.Item.SubItems[2].Tag.ToString() ) {
                    TextRenderer.DrawText(e.Graphics, e.Item.SubItems[2].Text, e.Item.SubItems[2].Font, e.Item.SubItems[2].Bounds, Color.Red, TextFormatFlags.HorizontalCenter);
                } else {
                    e.DrawDefault = true;
                }
            }
        }
        private void listView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e) {
            if ( e.Item.Selected ) {
                // keep selection always visible, even when focus got lost
                if ( e.ColumnIndex != 0 ) {
                    // if anything was changed from original (aka column 2), we show it in red
                    if ( e.ColumnIndex == 2 ) {
                        // if anything was changed from original, we show it in red
                        if ( e.SubItem.Tag.ToString() != e.SubItem.Text ) {
                            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.SubItem.Font, e.Bounds, Color.Red, TextFormatFlags.HorizontalCenter);
                        } else {
                            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.SubItem.Font, e.Bounds, e.SubItem.ForeColor, TextFormatFlags.HorizontalCenter);
                        }
                    } else {
                        TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.SubItem.Font, e.Bounds, e.SubItem.ForeColor, TextFormatFlags.Default);
                    }
                }
            } else {
                if ( e.ColumnIndex == 2 ) {
                    // if anything was changed from original, we show it in red
                    if ( e.SubItem.Tag.ToString() != e.SubItem.Text ) {
                        TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.SubItem.Font, e.Bounds, Color.Red, TextFormatFlags.HorizontalCenter);
                    } else {
                        TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.SubItem.Font, e.Bounds, e.SubItem.ForeColor, TextFormatFlags.HorizontalCenter);
                    }
                } else {
                    e.DrawDefault = true;
                }
            }
        }
        private void listView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e) {
            e.Graphics.FillRectangle(Brushes.BlanchedAlmond, e.Bounds);
            e.DrawText();
        }

        // get MainForm back when this form closes
        private void FileComparer_FormClosed(object sender, FormClosedEventArgs e) {
            this.m_Parent.WindowState = FormWindowState.Normal;
        }

        private void toolStripMenuItem_winmerge_Click(object sender, EventArgs e) {
            this.StartWinMerge();
        }

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e) {
            if ( this.listView1.SelectedItems.Count == 0 ) {
                return;
            }
            if ( this.listView1.SelectedItems[0].BackColor == Color.CadetBlue ) {
                this.toolStripMenuItem_winmerge.Enabled = false;
                this.toolStripMenuItem_LeftEditor.Enabled = false;
                this.toolStripMenuItem_RightFile.Enabled = false;
                this.bothFilesOpenEditorToolStripMenuItem.Enabled = false;
            } else {
                // sanity check if files exist
                int iIndex = this.listView1.SelectedItems[0].Index;
                if ( iIndex == 0 ) {
                    return;
                }
                int num = -1;
                try {
                    num = Convert.ToInt32(this.listView1.Items[iIndex].SubItems[0].Text);
                } catch ( Exception ) {; }
                if ( num == -1 ) {
                    try {
                        num = Convert.ToInt32(this.listView1.Items[--iIndex].SubItems[0].Text);
                    } catch ( Exception ) {; }
                }
                if ( num == -1 ) {
                    return;
                }
                string file1 = this.listView1.Items[iIndex].SubItems[1].Text;
                string base1 = "";
                int iFolderIndex = -1;
                for ( int i = iIndex; i >= 0; i-- ) {
                    base1 = this.listView1.Items[i].SubItems[1].Text;
                    if ( Directory.Exists(base1) ) {
                        iFolderIndex = i;
                        break;
                    }
                }
                string full1 = Path.Combine(base1, file1);
                if ( !File.Exists(full1) ) {
                    return;
                }
                string file2 = this.listView1.Items[iIndex].SubItems[3].Text;
                string base2 = this.listView1.Items[iFolderIndex].SubItems[3].Text;
                string full2 = Path.Combine(base2, file2);
                if ( !File.Exists(full2) ) {
                    return;
                }
                // check if winmerge exists
                this.toolStripMenuItem_LeftEditor.Enabled = true;
                this.toolStripMenuItem_RightFile.Enabled = true;
                this.bothFilesOpenEditorToolStripMenuItem.Enabled = true;
                string winmergePath = GrzTools.InstalledPrograms.ProgramPath("winmerge");
                if ( winmergePath.Length > 0 ) {
                    this.toolStripMenuItem_winmerge.Enabled = true;
                } else {
                    this.toolStripMenuItem_winmerge.Enabled = false;
                }
            }
        }

        // winmerge and editor options
        void StartWinMerge() {
            if ( this.listView1.SelectedItems.Count == 0 ) {
                GrzTools.AutoMessageBox.Show("Nothing selected.", "Error", 2000);
                return;
            }

            // full filename 1
            int iIndex = this.listView1.SelectedItems[0].Index;
            if ( iIndex == 0 ) {
                GrzTools.AutoMessageBox.Show("Select a pair of files, not directories.", "Error", 2000);
                return;
            }
            int num = -1;
            try {
                num = Convert.ToInt32(this.listView1.Items[iIndex].SubItems[0].Text);
            } catch ( Exception ) {; }
            if ( num == -1 ) {
                try {
                    num = Convert.ToInt32(this.listView1.Items[--iIndex].SubItems[0].Text);
                } catch ( Exception ) {; }
            }
            if ( num == -1 ) {
                GrzTools.AutoMessageBox.Show("Select a pair of files, not directories.", "Error", 2000);
                return;
            }
            string file1 = this.listView1.Items[iIndex].SubItems[1].Text;
            string base1 = "";
            int iFolderIndex = -1;
            for ( int i = iIndex; i >= 0; i-- ) {
                base1 = this.listView1.Items[i].SubItems[1].Text;
                if ( Directory.Exists(base1) ) {
                    iFolderIndex = i;
                    break;
                }
            }
            string full1 = Path.Combine(base1, file1);
            if ( !File.Exists(full1) ) {
                GrzTools.AutoMessageBox.Show("File '" + full1 + "' not existing.", "Error", 2000);
                return;
            }

            // full filename 2
            string file2 = this.listView1.Items[iIndex].SubItems[3].Text;
            string base2 = this.listView1.Items[iFolderIndex].SubItems[3].Text;
            string full2 = Path.Combine(base2, file2);
            if ( !File.Exists(full2) ) {
                GrzTools.AutoMessageBox.Show("File '" + full2 + "' not existing.", "Error", 2000);
                return;
            }

            // find winmerge
            string winmergePath = GrzTools.InstalledPrograms.ProgramPath("winmerge");
            if ( winmergePath.Length == 0 ) {
                return;
            }

            // start winmerge with two files as parameter
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = winmergePath;
            p.StartInfo.Arguments = " -e -ub " + "\"" + full1 + "\"" + " " + "\"" + full2 + "\"";
            p.Start();
        }
        private void toolStripMenuItem_RightFile_Click(object sender, EventArgs e) {
            string filepath = this.GetSelectedFileName(3);
            this.OpenEditor(filepath);
        }
        private void toolStripMenuItem_LeftEditor_Click(object sender, EventArgs e) {
            string filepath = this.GetSelectedFileName(1);
            this.OpenEditor(filepath);
        }
        private void bothFilesOpenEditorToolStripMenuItem_Click(object sender, EventArgs e) {
            this.toolStripMenuItem_LeftEditor_Click(null, null);
            this.toolStripMenuItem_RightFile_Click(null, null);
        }
        string GetSelectedFileName(int column) {
            if ( this.listView1.SelectedItems.Count == 0 ) {
                GrzTools.AutoMessageBox.Show("Nothing selected.", "Error", 2000);
                return "";
            }

            // full filename
            int iIndex = this.listView1.SelectedItems[0].Index;
            if ( iIndex == 0 ) {
                GrzTools.AutoMessageBox.Show("Select a file, not directory.", "Error", 2000);
                return "";
            }
            int num = -1;
            try {
                num = Convert.ToInt32(this.listView1.Items[iIndex].SubItems[0].Text);
            } catch ( Exception ) {; }
            if ( num == -1 ) {
                try {
                    num = Convert.ToInt32(this.listView1.Items[--iIndex].SubItems[0].Text);
                } catch ( Exception ) {; }
            }
            if ( num == -1 ) {
                GrzTools.AutoMessageBox.Show("Select a file, not directory.", "Error", 2000);
                return "";
            }
            string file1 = this.listView1.Items[iIndex].SubItems[column].Text;
            string base1 = "";
            int iFolderIndex = -1;
            for ( int i = iIndex; i >= 0; i-- ) {
                base1 = this.listView1.Items[i].SubItems[column].Text;
                if ( Directory.Exists(base1) ) {
                    iFolderIndex = i;
                    break;
                }
            }
            string full1 = Path.Combine(base1, file1);
            if ( !File.Exists(full1) ) {
                GrzTools.AutoMessageBox.Show("File '" + full1 + "' not existing.", "Error", 2000);
                return "";
            }

            return full1;
        }
        void OpenEditor(string fullfilename) {
            Process mProcess = new Process();
            mProcess.StartInfo = new System.Diagnostics.ProcessStartInfo(fullfilename);
            try {
                mProcess.Start();
            } catch ( Exception ) {
                ProcessStartInfo processInfo = new ProcessStartInfo(fullfilename);
                processInfo.Verb = "openas";
                processInfo.ErrorDialog = false;
                try {
                    Process.Start(processInfo);
                } catch ( Exception ) {
                    MessageBox.Show("'" + fullfilename + "' is not linked to an executable application.", "Error");
                }
            }
        }

        // we grant a +/-5s time delta: useful for copies on USB drives 
        private void checkBoxLastWrite_CheckedChanged(object sender, EventArgs e) {
            if ( this.checkBoxLastWrite.Checked ) {
                this.checkBoxGrant5Sec.Enabled = true;
            } else {
                this.checkBoxGrant5Sec.Checked = false;
                this.checkBoxGrant5Sec.Enabled = false;
            }
        }

        // headline
        private void checkBox1by1_CheckedChanged(object sender, EventArgs e) {
            if ( this.checkBox1by1.Checked ) {
                this.Text = "File Comparer - 'showing all files and folders'";
                this.checkBoxExistence.Checked = false;
                this.checkBoxExistence.Enabled = false;
                this.checkBoxLastWrite.Checked = false;
                this.checkBoxLastWrite.Enabled = false;
                this.checkBoxLength.Checked = false;
                this.checkBoxLength.Enabled = false;
            } else {
                this.Text = "File Comparer - 'showing not matching files'";
                this.checkBoxExistence.Enabled = true;
                this.checkBoxLastWrite.Enabled = true;
                this.checkBoxLength.Enabled = true;
                this.checkBoxExistence.Checked = true;
                this.checkBoxLastWrite.Checked = true;
                this.checkBoxLength.Checked = true;
            }
        }

        // StartWinMerge();
        private void listView1_DoubleClick(object sender, EventArgs e) {
            ListView lv = (ListView)sender;
            Point mousePos = lv.PointToClient(Control.MousePosition);
            ListViewHitTestInfo hitTest = lv.HitTest(mousePos);
            int columnIndex = hitTest.Item.SubItems.IndexOf(hitTest.SubItem);
            if ( columnIndex != 2 ) {
                this.StartWinMerge();
            }
        }

        // intercept combo box ENTER key
        private void comboBoxWildCard_KeyDown(object sender, KeyEventArgs e) {
            if ( e.KeyCode == Keys.Enter ) {
                // insert item  
                this.comboBoxWildCard_Validated(null, null);
                // exec file comparison
                this.buttonRefresh_Click(null, null);
            }
        }

        // intercept combo box losing focus 
        private void comboBoxWildCard_Validated(object sender, EventArgs e) {
            // insert the just edited buzzword to the combobox list
            string str = this.comboBoxWildCard.Text;
            if ( str.Length == 0 ) {
                this.comboBoxWildCard.Text = "*.*";
                str = this.comboBoxWildCard.Text;
            }
            int index = this.comboBoxWildCard.FindStringExact(str);
            if ( (index < this.comboBoxWildCard.Items.Count) && (index >= 0) ) {
                this.comboBoxWildCard.SelectedIndex = index;
            } else {
                this.comboBoxWildCard.Items.Insert(0, str);
                this.comboBoxWildCard.SelectedIndex = 0;
            }
        }

        // populate combo box
        private void FileComparer_Load(object sender, EventArgs e) {
            // INI: read previous file masks
            GrzTools.IniFile ini = new GrzTools.IniFile(System.Windows.Forms.Application.ExecutablePath + ".ini");
            for ( int i = 0; i < 5; i++ ) {
                string tmp = ini.IniReadValue("cfw", "filecomparer" + i.ToString(), "");
                if ( tmp.Length > 0 ) {
                    this.comboBoxWildCard.Items.Add(tmp);
                }
            }
            // INI: read last file mask index 
            int index = int.Parse(ini.IniReadValue("cfw", "filecomparerNdx", "0"));
            if ( this.comboBoxWildCard.Items.Count == 0 ) {
                this.comboBoxWildCard.Items.Add("*.*");
                index = 0;
            }
            this.comboBoxWildCard.SelectedIndex = index;
            // INI: start position & window size
            string strX = ini.IniReadValue("cfw", "filecomparerStartPositionX", "0");
            string strY = ini.IniReadValue("cfw", "filecomparerStartPositionY", "0");
            Rectangle rc = Screen.FromControl(this).Bounds;
            this.Location = new Point(Math.Min(rc.Width / 2, Math.Max(int.Parse(strX), 0)), Math.Min(rc.Height / 2, Math.Max(int.Parse(strY), 0)));
            strX = ini.IniReadValue("cfw", "filecomparerWidth", "800");
            strY = ini.IniReadValue("cfw", "filecomparerHeight", "600");
            this.Size = new Size(Math.Max(int.Parse(strX), 800), Math.Max(int.Parse(strY), 600));
        }

        // copy all selected items to clipboard
        private void copySelectedTextToolStripMenuItem_Click(object sender, EventArgs e) {
            string text = "";
            if ( this.listView1.SelectedIndices.Count > 0 ) {
                foreach ( int ndx in this.listView1.SelectedIndices ) {
                    ListViewItem lvi = this.listView1.Items[ndx];
                    foreach ( ListViewItem.ListViewSubItem listViewSubItem in lvi.SubItems ) {
                        text += (string.Format("{0}\t", listViewSubItem.Text));
                    }
                    text += "\r\n";
                }
            }
            Clipboard.SetText(text);
        }

        // 20160130: change file status
        private void toolStripMenuItemCopyToLeft_Click(object sender, EventArgs e) {
            for ( int i = 0; i < this.listView1.SelectedIndices.Count; i++ ) {
                ListViewItem lvi = this.listView1.Items[this.listView1.SelectedIndices[i]];
                if ( lvi.SubItems[2].Text.Length > 0 ) {
                    lvi.SubItems[2].Text = "<---";
                }
            }
        }
        private void toolStripMenuItemCopyToRight_Click(object sender, EventArgs e) {
            for ( int i = 0; i < this.listView1.SelectedIndices.Count; i++ ) {
                ListViewItem lvi = this.listView1.Items[this.listView1.SelectedIndices[i]];
                if ( lvi.SubItems[2].Text.Length > 0 ) {
                    lvi.SubItems[2].Text = "--->";
                }
            }
        }
        private void toolStripMenuItemDelete_Click(object sender, EventArgs e) {
            for ( int i = 0; i < this.listView1.SelectedIndices.Count; i++ ) {
                ListViewItem lvi = this.listView1.Items[this.listView1.SelectedIndices[i]];
                if ( lvi.SubItems[2].Text.Length > 0 ) {
                    lvi.SubItems[2].Text = "XXX";
                }
            }
        }
        private void toolStripMenuItemIdle_Click(object sender, EventArgs e) {
            for ( int i = 0; i < this.listView1.SelectedIndices.Count; i++ ) {
                ListViewItem lvi = this.listView1.Items[this.listView1.SelectedIndices[i]];
                if ( lvi.SubItems[2].Text.Length > 0 ) {
                    lvi.SubItems[2].Text = "<?>";
                }
            }
        }
        private void resetStatusOfAllSelectedFilesToolStripMenuItem_Click(object sender, EventArgs e) {
            for ( int i = 0; i < this.listView1.SelectedIndices.Count; i++ ) {
                ListViewItem lvi = this.listView1.Items[this.listView1.SelectedIndices[i]];
                lvi.SubItems[2].Text = lvi.SubItems[2].Tag.ToString();
            }
        }

        private void resetStatusOfAllFilesToolStripMenuItem_Click(object sender, EventArgs e) {
            for ( int i = 0; i < this.listView1.Items.Count; i++ ) {
                this.listView1.Items[i].SubItems[2].Text = this.listView1.Items[i].SubItems[2].Tag.ToString();
            }
        }

        // 20160130: change list item selection status
        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e) {
            this.listView1.SelectedIndices.Clear();
            for ( int ndx = 0; ndx < this.listView1.Items.Count; ndx++ ) {
                this.listView1.SelectedIndices.Add(ndx);
            }
        }
        private void selectNothingToolStripMenuItem_Click(object sender, EventArgs e) {
            this.listView1.SelectedIndices.Clear();
        }
        private void invertSelectionToolStripMenuItem_Click(object sender, EventArgs e) {
            for ( int ndx = 0; ndx < this.listView1.Items.Count; ndx++ ) {
                this.listView1.Items[ndx].Selected = !this.listView1.Items[ndx].Selected;
            }
        }

        // 20160130: select files according to item status
        private void checkBoxEoNA_CheckedChanged(object sender, EventArgs e) {
            this.SelectItemsAccordingToStatus();
        }
        private void checkBoxToLeft_CheckedChanged(object sender, EventArgs e) {
            this.SelectItemsAccordingToStatus();
        }
        private void checkBoxToRight_CheckedChanged(object sender, EventArgs e) {
            this.SelectItemsAccordingToStatus();
        }
        // select files additively: 
        // a) "n/a" == all "n/a" files    
        // b) <dir> + !"n/a" == only <dir>    
        // c) <dir> + "n/a" == <dir> + matching "n/a" 
        void SelectItemsAccordingToStatus() {
            int toLHS = 0;
            int toRHS = 0;

            this.checkBoxXXX.CheckedChanged -= this.checkBoxXXX_CheckedChanged;
            this.checkBoxXXX.Checked = false;
            this.checkBoxXXX.CheckedChanged += this.checkBoxXXX_CheckedChanged;
            this.listView1.SelectedIndices.Clear();
            for ( int ndx = 0; ndx < this.listView1.Items.Count; ndx++ ) {

                if ( !this.checkBoxToLeft.Checked && !this.checkBoxToRight.Checked ) {
                    //
                    // no "left right" required, simply select all "n/a"
                    //
                    // all missing files on lhs
                    if ( this.checkBoxEoNA.Checked && (this.listView1.Items[ndx].SubItems[2].Text == "<---") && (this.listView1.Items[ndx].SubItems[1].Text == "n/a") ) {
                        this.listView1.Items[ndx].Selected = true;
                        toLHS++;
                    }
                    // all missing files on rhs
                    if ( this.checkBoxEoNA.Checked && (this.listView1.Items[ndx].SubItems[2].Text == "--->") && (this.listView1.Items[ndx].SubItems[3].Text == "n/a") ) {
                        this.listView1.Items[ndx].Selected = true;
                        toRHS++;
                    }
                }

                if ( this.checkBoxEoNA.Checked ) {
                    //
                    // take "n/a" files into account AND select them matching to their copy direction
                    //
                    // copy to left
                    if ( this.checkBoxToLeft.Checked && (this.listView1.Items[ndx].SubItems[2].Text == "<---") && (this.listView1.Items[ndx].SubItems[1].Text == "n/a") && (this.listView1.Items[ndx].SubItems[3].Text != "n/a") ) {
                        this.listView1.Items[ndx].Selected = true;
                        toLHS++;
                    }
                    // copy to right
                    if ( this.checkBoxToRight.Checked && (this.listView1.Items[ndx].SubItems[2].Text == "--->") && (this.listView1.Items[ndx].SubItems[3].Text == "n/a") && (this.listView1.Items[ndx].SubItems[1].Text != "n/a") ) {
                        this.listView1.Items[ndx].Selected = true;
                        toRHS++;
                    }
                }

                //
                // normal selection, only files with either ---> OR <---
                //
                // copy to left
                if ( this.checkBoxToLeft.Checked && (this.listView1.Items[ndx].SubItems[2].Text == "<---") && (this.listView1.Items[ndx].SubItems[1].Text != "n/a") && (this.listView1.Items[ndx].SubItems[3].Text != "n/a") ) {
                    this.listView1.Items[ndx].Selected = true;
                    toLHS++;
                }
                // copy to right
                if ( this.checkBoxToRight.Checked && (this.listView1.Items[ndx].SubItems[2].Text == "--->") && (this.listView1.Items[ndx].SubItems[3].Text != "n/a") && (this.listView1.Items[ndx].SubItems[1].Text != "n/a") ) {
                    this.listView1.Items[ndx].Selected = true;
                    toRHS++;
                }
            }
            this.listView1.Focus();

            // results
            this.textBoxA.BackColor = SystemColors.Control;
            this.textBoxA.Text = "Copy Files:\r\n~~~~~~~\r\nto RHS:\t" + toRHS.ToString();
            this.textBoxB.BackColor = SystemColors.Control;
            this.textBoxB.Text = "Copy Files:\r\n~~~~~~~\r\nto LHS:\t" + toLHS.ToString();
        }

        // 20160130: select files to delete
        private void checkBoxXXX_CheckedChanged(object sender, EventArgs e) {
            this.checkBoxToLeft.CheckedChanged -= this.checkBoxToLeft_CheckedChanged;
            this.checkBoxToLeft.Checked = false;
            this.checkBoxToLeft.CheckedChanged += this.checkBoxToLeft_CheckedChanged;
            this.checkBoxToRight.CheckedChanged -= this.checkBoxToRight_CheckedChanged;
            this.checkBoxToRight.Checked = false;
            this.checkBoxToRight.CheckedChanged += this.checkBoxToRight_CheckedChanged;
            this.checkBoxEoNA.CheckedChanged -= this.checkBoxEoNA_CheckedChanged;
            this.checkBoxEoNA.Checked = false;
            this.checkBoxEoNA.CheckedChanged += this.checkBoxEoNA_CheckedChanged;
            this.listView1.SelectedIndices.Clear();
            if ( this.checkBoxXXX.Checked ) {
                for ( int ndx = 0; ndx < this.listView1.Items.Count; ndx++ ) {
                    if ( this.listView1.Items[ndx].SubItems[2].Text == "XXX" ) {
                        this.listView1.Items[ndx].Selected = true;
                    }
                }
            }
            this.listView1.Focus();
        }

        private void buttonCopy_Click(object sender, EventArgs e) {
            this.listView1.Focus();

            // build 2 lists containing the files to copy
            int cntLHS2RHS = 0;
            int cntRHS2LHS = 0;
            string pathLHS = "";
            string pathRHS = "";
            List<string> source = new List<String>();
            List<string> destin = new List<String>();

            // loop listview
            for ( int ndx = 0; ndx < this.listView1.Items.Count; ndx++ ) {
                // a folder item was hit
                if ( this.listView1.Items[ndx].BackColor == Color.CadetBlue ) {
                    // both folders exist
                    pathLHS = this.listView1.Items[ndx].SubItems[1].Text;
                    pathRHS = this.listView1.Items[ndx].SubItems[3].Text;
                    // one folder may contain "n/a" - but never both folders
                    if ( pathLHS == "n/a" ) {
                        int delta = this.listView1.Items[ndx].SubItems[3].Text.Length - this.listView1.Columns[3].Text.Length - 1;
                        string sLastFolderAndFile = this.listView1.Items[ndx].SubItems[3].Text.Substring(this.listView1.Items[ndx].SubItems[3].Text.Length - delta);
                        pathLHS = Path.Combine(this.listView1.Columns[1].Text, sLastFolderAndFile);
                    }
                    if ( pathRHS == "n/a" ) {
                        int delta = this.listView1.Items[ndx].SubItems[1].Text.Length - this.listView1.Columns[1].Text.Length - 1;
                        string sLastFolderAndFile = this.listView1.Items[ndx].SubItems[1].Text.Substring(this.listView1.Items[ndx].SubItems[1].Text.Length - delta);
                        pathRHS = Path.Combine(this.listView1.Columns[3].Text, sLastFolderAndFile);
                    }
                }
                // a "to copy item" must be: selected AND of neutral back color AND with a substantial copy strategy
                if ( (this.listView1.Items[ndx].Selected) && (this.listView1.Items[ndx].BackColor != Color.CadetBlue) && (this.listView1.Items[ndx].SubItems[2].Text.Length > 0) ) {
                    // copy to lhs
                    if ( this.listView1.Items[ndx].SubItems[2].Text == "<---" ) {
                        source.Add(Path.Combine(pathRHS, this.listView1.Items[ndx].SubItems[3].Text));
                        destin.Add(Path.Combine(pathLHS, Path.GetFileName(this.listView1.Items[ndx].SubItems[3].Text)));
                        cntRHS2LHS++;
                    }
                    // copy to rhs
                    if ( this.listView1.Items[ndx].SubItems[2].Text == "--->" ) {
                        source.Add(Path.Combine(pathLHS, this.listView1.Items[ndx].SubItems[1].Text));
                        destin.Add(Path.Combine(pathRHS, Path.GetFileName(this.listView1.Items[ndx].SubItems[1].Text)));
                        cntLHS2RHS++;
                    }
                }
            }

            // for the sake of mind
            string text = cntLHS2RHS.ToString() + "\tfiles from LHS to RHS and\r\n\r\n" + cntRHS2LHS.ToString() + "\tfiles from RHS to LHS\r\n\r\nwill be copied.\r\nAny source file will overwrite its counterpart in the destination.\r\n\r\nProceed?";
            DialogResult dr = MessageBox.Show(text, "File Copy", MessageBoxButtons.OKCancel);
            if ( dr != System.Windows.Forms.DialogResult.OK ) {
                return;
            }

            // execute file copy
            MainForm.ShfoWorker shfo = new MainForm.ShfoWorker();
            shfo.ShfoEvent += new MainForm.ShfoWorker.EventHandler(this.shfoEventHandler);
            shfo.SetArguments(new MainForm.ShfoWorker.Arguments(this.Handle, source, destin, GrzTools.ShellFileOperation.FileOperations.FO_COPY, "Copy", false, false));
            Thread thread = new Thread(shfo.DoWorkShfo);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
        private void shfoEventHandler(object source, MainForm.ShfoWorker.ShfoEventArgs e) {
            try {
                // if there is a message (normally error), we show it 
                if ( e.message.Length > 0 ) {
                    this.Invoke(new Action(() => { MessageBox.Show(e.message); }));
                } else {
                    this.Invoke(new Action(() => { GrzTools.AutoMessageBox.Show("Successfully finished.", "File Copy", 2000); }));
                }
            } catch ( Exception ) {; } // ShfoWorker might still run when App is closed, then we get an exception the moment the event is raised 
        }

        // 20161016: activate delete
        private void buttonDelete_Click(object sender, EventArgs e) {
            // loop listview and build a list of files to delete
            List<string> deleteFiles = new List<string>();
            string pathLHS = "";
            string pathRHS = "";
            string pathLhsTrash = "";
            string pathRhsTrash = "";
            for ( int ndx = 0; ndx < this.listView1.Items.Count; ndx++ ) {
                // a folder item was hit
                if ( this.listView1.Items[ndx].BackColor == Color.CadetBlue ) {
                    // get folders from both sides
                    pathLHS = this.listView1.Items[ndx].SubItems[1].Text;
                    pathRHS = this.listView1.Items[ndx].SubItems[3].Text;
                    // memorize a real folder from each side for later trash bin check
                    if ( !pathLHS.Contains("n/a") ) {
                        pathLhsTrash = pathLHS;
                    }
                    if ( !pathRHS.Contains("n/a") ) {
                        pathRhsTrash = pathRHS;
                    }
                }
                // an item must be: selected AND of neutral back color AND with a delete strategy
                if ( (this.listView1.Items[ndx].Selected) && (this.listView1.Items[ndx].BackColor != Color.CadetBlue) && (this.listView1.Items[ndx].SubItems[2].Text.Length > 0) ) {
                    // collect file to delete in lhs and rhs 
                    if ( this.listView1.Items[ndx].SubItems[2].Text == "XXX" ) {
                        string fileLhs = Path.Combine(pathLHS, this.listView1.Items[ndx].SubItems[1].Text);
                        if ( !fileLhs.Contains("n/a") ) {
                            deleteFiles.Add(fileLhs);
                        }
                        string fileRhs = Path.Combine(pathRHS, this.listView1.Items[ndx].SubItems[3].Text);
                        if ( !fileRhs.Contains("n/a") ) {
                            deleteFiles.Add(fileRhs);
                        }
                    }
                }
            }

            // check status of trash bin
            string rootLhs = Path.GetPathRoot(pathLhsTrash);
            string rootRhs = Path.GetPathRoot(pathRhsTrash);
            if ( !GrzTools.FileTools.DriveHasRecycleBin(rootLhs) || !GrzTools.FileTools.DriveHasRecycleBin(rootRhs) ) {
                string txt = Localizer.GetString("notrash");
                DialogResult dr = MessageBox.Show(txt, "Attention", MessageBoxButtons.OKCancel);
                if ( dr == DialogResult.Cancel ) {
                    return;
                }
            }

            // safety belt
            if ( MessageBox.Show("You are about to delete the selected files to the trash bin?", "Question", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes ) {
                return;
            }

            // delete
            string returnText = "Done";
            foreach ( string file in deleteFiles ) {
                try {
                    FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                } catch ( OperationCanceledException ) {
                    returnText = "Deleting files was interrupted by operator.";
                } catch ( Exception ) {
                    returnText = "Error, not all files could be deleted.";
                }
            }
            MessageBox.Show(returnText);
        }

        private void checkBoxLiveView_CheckedChanged(object sender, EventArgs e) {
            if ( this.checkBoxLiveView.Checked ) {
                for ( int i = 0; i < 20; i++ ) {
                    this.listView1.EndUpdate();
                }
                this.listView1.Invalidate(true);
            } else {
                this.listView1.BeginUpdate();
            }
        }

    }
}
