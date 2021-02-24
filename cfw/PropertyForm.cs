using System;
using System.Collections.Generic;
using System.Diagnostics;                   // stopwatch  
using System.Drawing;
using System.Globalization;                 // CultureInfo.InvariantCulture
using System.IO;                            // Path
using System.Linq;
using System.Threading.Tasks;               // tasks
using System.Windows.Forms;

namespace cfw {
    public partial class PropertyForm : Form {
        readonly ListView m_selection;
        readonly string m_path;
        readonly Form m_parent;

        bool m_run = false;

        long m_FolderCount = 0;
        long m_FileCount = 0;
        long m_ExactSize = 0;
        string m_curpath = "";

        Stopwatch m_sw;

        public PropertyForm(Form parent, ListView selection, string path) {
            this.InitializeComponent();
            this.m_parent = parent;
            this.m_selection = selection;
            this.m_path = path;
            this.labelSelection.Text = this.m_selection.Items[0].Text + " ... " + this.m_selection.Items[this.m_selection.Items.Count - 1].Text;
            this.timerDelayedStart.Start();
            this.timerRefresh.Start();
        }

        void DoWork() {
            this.buttonOkBreak.Text = "- break -";
            this.buttonOkBreak.BackColor = Color.FromArgb(255, 192, 192);
            this.m_run = true;
            this.m_sw = Stopwatch.StartNew();

            // empty HD space & total HD space via unmanaged Win32 function
            ulong spaceleft = 0;
            ulong spacetotal = 0;
            if ( this.m_path != "Computer" ) {
                GrzTools.FileTools.DriveFreeBytes(Path.GetPathRoot(this.m_path), out spaceleft, out spacetotal);
                this.labelSpaceLeft.Text = GrzTools.StringTools.SizeSuffix((Int64)spaceleft);
                this.labelSpaceTotal.Text = GrzTools.StringTools.SizeSuffix((Int64)spacetotal);
            } else {
                // 20160320: in case of "Computer" we were required to go thru all selected drives 
                ulong spaceleftall = 0;
                ulong spacetotalall = 0;
                foreach ( ListViewItem lvi in this.m_selection.Items ) {
                    if ( (lvi.ImageIndex != 9) && (lvi.Text.Length == 2) && (lvi.Text.Length > 1) && (lvi.Text[1] == ':') ) {
                        GrzTools.FileTools.DriveFreeBytes(lvi.Text, out spaceleft, out spacetotal);
                        spaceleftall += spaceleft;
                        spacetotalall += spacetotal;
                    }
                }
                this.labelSpaceLeft.Text = GrzTools.StringTools.SizeSuffix((Int64)spaceleftall);
                this.labelSpaceTotal.Text = GrzTools.StringTools.SizeSuffix((Int64)spacetotalall);
            }

            Cursor.Current = Cursors.WaitCursor;
            // global collector for folders, files, size
            GrzTools.FastFileFind.FocFicSiz ffs = new GrzTools.FastFileFind.FocFicSiz(0, 0, 0);
            // start as many selected folders parallel tasks to count files&folders
            List<Task> tasks = new List<Task>();
            List<GrzTools.FastFileFind> lfff = new List<GrzTools.FastFileFind>();
            foreach ( ListViewItem lvi in this.m_selection.Items ) {
                // skip nonsense AND not connected network drives
                string selection = lvi.Text;
                if ( ((selection[1] == '.') && (lvi.ImageIndex == 2)) || (lvi.ImageIndex == 9) ) {
                    continue;
                }
                // get full path
                string file = @Path.Combine(this.m_path, selection);
                // 20160319 drives are new: distinguish between folders/drives and top level files
                if ( (new[] { 0, 3, 4, 5, 6, 7 }).Contains(lvi.ImageIndex) ) {
                    // GrzTools.FastFileFind.FileCountSize only counts folders underneath, not the top level folder itself
                    ffs.foc += 1;
                    this.m_FolderCount += 1;
                    // execute fastest ever file search in parallel tasks
                    GrzTools.FastFileFind fff = new GrzTools.FastFileFind(this);
                    fff.CountSizeEvent += new EventHandler<GrzTools.FastFileFind.CountSizeEventArgs>(this.CountSizeEvent_Received);
                    Task t = new Task(() => fff.FileCountSize(ref ffs, ref this.m_run, file, "*.*"));
                    t.Start();
                    tasks.Add(t);
                    // add finder instance to list of finders
                    lfff.Add(fff);
                } else {
                    // files only
                    ffs.fic += 1;
                    try {
                        ffs.siz += new System.IO.FileInfo(file).Length;
                    } catch ( Exception ) {
                    } finally {
                    }
                }
            }

            // cooperative loop until tasks are finished
            do {
                // loop tasks for completion
                for ( int i = 0; i < tasks.Count; i++ ) {
                    Application.DoEvents();
                    if ( tasks[i].IsCompleted ) {
                        // remove task from list of tasks
                        tasks.RemoveAt(i);
                        // remove current finder from list of finders
                        lfff[i].CountSizeEvent -= this.CountSizeEvent_Received;
                        lfff.RemoveAt(i);
                    }
                }
            } while ( (tasks.Count > 0) && this.m_run );

            // finally set values according to ffs: for yet unknown reason the summing in the eventhandler occasionally fails slightly
            this.m_FolderCount = ffs.foc;
            this.m_FileCount = ffs.fic;
            this.m_ExactSize = ffs.siz;
            this.timerRefresh_Tick(null, null);

            // the end
            this.m_sw.Stop();
            this.buttonOkBreak.Text = "Ok";
            this.buttonOkBreak.BackColor = SystemColors.Control;
            Cursor.Current = Cursors.Default;
        }

        // all this effort only to show progess: all parallel tasks send messages to this event handler
        void CountSizeEvent_Received(object sender, GrzTools.FastFileFind.CountSizeEventArgs e) {
            this.m_FolderCount += e.FolderCount;
            this.m_FileCount += e.FileCount;
            this.m_ExactSize += e.Size;
            this.m_curpath = e.Path;
        }

        private void buttonOkBreak_Click(object sender, EventArgs e) {
            if ( this.buttonOkBreak.Text == "Ok" ) {
                this.Close();
                this.m_parent.WindowState = FormWindowState.Normal;
            }
            if ( this.buttonOkBreak.Text != "Ok" ) {
                this.m_run = false;
            }
        }

        private void timerDelayedStart_Tick(object sender, EventArgs e) {
            this.timerDelayedStart.Stop();
            this.DoWork();
            this.timerRefresh.Stop();
            if ( this.m_run ) {
                this.labelSearchTime.Text = "Search took " + (this.m_sw.ElapsedMilliseconds / 1000).ToString() + "s";
                this.labelCurrent.Text = "done";
            } else {
                this.labelSearchTime.Text = "Search was interrupted after " + (this.m_sw.ElapsedMilliseconds / 1000).ToString() + "s";
            }
        }

        static readonly CultureInfo ci = new CultureInfo("de-DE");
        private void timerRefresh_Tick(object sender, EventArgs e) {
            this.labelFolderCount.Text = string.Format(ci, "{0:#,#}", this.m_FolderCount);
            this.labelFileCount.Text = string.Format(ci, "{0:#,#}", this.m_FileCount);
            this.labelTotalSize.Text = GrzTools.StringTools.SizeSuffix(this.m_ExactSize);
            this.labelExactSize.Text = string.Format(ci, "{0:#,#} bytes", this.m_ExactSize);
            this.labelSearchTime.Text = "Searching " + (this.m_sw.ElapsedMilliseconds / 1000).ToString() + "s";
            this.labelCurrent.Text = this.m_curpath;
        }

        private void PropertyForm_FormClosing(object sender, FormClosingEventArgs e) {
            this.m_run = false;
        }
    }
}
