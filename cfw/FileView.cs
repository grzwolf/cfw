using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;                                           // file stream 
using System.Runtime.InteropServices;                      // DLLImport
using System.Text;
using System.Windows.Forms;

namespace FileViewCtl {
    public partial class FileView : UserControl {
        string m_sFileName = "";
        Form m_Parent = null;
        bool m_bBinaryFile = false;
        long m_iFileLength = 0;
        int m_iLastFoundPos = 0;
        DateTime m_dtLastUpdate = DateTime.Now;
        long m_lReadPos = 0;
        long m_lFromPos = 0;
        int m_NumOfBytes = 150000;
        int m_iFposBeg = 0;
        int m_iFposEnd = 0;
        bool m_bFindStop = false;
        readonly ToolTip m_toolTip;



        //        Form form = new Form();

        public FileView() {
            this.InitializeComponent();

            // hide find controls normally
            this.textBoxFind.Visible = false;
            this.buttonDown.Visible = false;
            this.buttonUp.Visible = false;
            this.buttonX.Visible = false;
            this.tableLayoutPanel1.SetRowSpan(this.richTextBox, 2);

            // some extra events
            this.richTextBox.Scrolled += this.rtfScrolled;

            // tooltip
            this.components = new System.ComponentModel.Container();
            this.m_toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.m_toolTip.OwnerDraw = true;
            this.m_toolTip.Draw += new DrawToolTipEventHandler(this.toolTip_Draw);
        }

        // ToolTip 
        void toolTip_Draw(object sender, DrawToolTipEventArgs e) {
            e.DrawBackground();
            e.DrawBorder();
            e.DrawText();
        }

        // 20161016 close view via parent F3
        public void CloseView() {
            this.m_sFileName = "";
            this.m_Parent = null;
            this.m_bBinaryFile = false;
            this.m_iFileLength = 0;
            this.m_iLastFoundPos = 0;
            this.m_dtLastUpdate = DateTime.Now;
            this.m_lReadPos = 0;
            this.m_lFromPos = 0;
            this.m_NumOfBytes = 150000;
            this.m_iFposBeg = 0;
            this.m_iFposEnd = 0;
            this.m_bFindStop = false;

            this.m_toolTip.Draw -= this.toolTip_Draw;
            this.richTextBox.Scrolled -= this.rtfScrolled;
        }

        public void Clear(Form parent) {
            this.m_Parent = parent;
            this.richTextBox.Clear();
            this.richTextBox.Text = "<nothing>";

            this.vScrollBar.Visible = false;
            this.panelFilePos.Visible = false;
            this.tableLayoutPanel1.SetColumnSpan(this.richTextBox, 6);
            this.richTextBox.ScrollBars = RichTextBoxScrollBars.Both;
            this.richTextBox.WordWrap = true;
        }

        public bool LoadDocument(Form parent, string filename, ListView focus) {
            try {
                this.findToolStripMenuItem_Click(null, null);
                Encoding enc;
                this.m_sFileName = filename;
                this.m_Parent = parent;
                this.richTextBox.Clear();
                this.m_iFileLength = 0;
                this.m_iLastFoundPos = 0;
                this.m_lReadPos = 0;
                this.m_lFromPos = 0;
                this.m_bBinaryFile = false;
                DateTime m_dtLastUpdate = DateTime.Now;
                if ( !GrzTools.FileTools.IsTextFile(out enc, filename, 100) ) {

                    // disable original vert. Scroller of textbox, setup a separate vert. Scroller and a global indicator for the file sector shown
                    this.tableLayoutPanel1.SetColumnSpan(this.richTextBox, 4);
                    this.panelFilePos.Visible = true;
                    this.vScrollBar.Visible = true;
                    this.vScrollBar.Value = 0;
                    this.richTextBox.ScrollBars = RichTextBoxScrollBars.Horizontal;
                    this.richTextBox.WordWrap = true;
                    // binary files as text
                    this.m_bBinaryFile = true;
                    FileInfo f = new FileInfo(this.m_sFileName);
                    this.m_iFileLength = f.Length;
                    // load file to textbox
                    this.richTextBox.Text = this.ReadBinaryLines(this.m_sFileName, this.m_lFromPos, this.m_NumOfBytes, out this.m_lReadPos).ToString();
                    this.ColorizeAllFindText();
                    // adjust global indicator
                    this.SetGlobalFilePos();
                    // set vScroller
                    this.vScrollBar.Maximum = this.richTextBox.GetLineFromCharIndex(this.richTextBox.Text.Length - 1);
                    // perhaps we don't need scroller and global indicator
                    int lpp = (this.richTextBox.ClientSize.Height / this.richTextBox.Font.Height) - 1;
                    if ( lpp >= this.vScrollBar.Maximum ) {
                        this.vScrollBar.Visible = false;
                        this.tableLayoutPanel1.SetColumnSpan(this.richTextBox, 5);
                    }

                } else {

                    //
                    // all files considered being text files
                    //

                    this.panelFilePos.Visible = false;
                    this.vScrollBar.Visible = false;
                    this.tableLayoutPanel1.SetColumnSpan(this.richTextBox, 6);
                    this.richTextBox.ScrollBars = RichTextBoxScrollBars.Both;
                    this.richTextBox.WordWrap = true;

                    FileStream fs = new FileStream(this.m_sFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    List<string> tmplst = new List<string>();
                    using ( StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default) ) {
                        string line = "";
                        while ( (line = sr.ReadLine()) != null ) {
                            tmplst.Add(line);
                        }
                    }
                    // replace all zeros: there are even text files containing zeros (SBCS)
                    for ( int i = 0; i < tmplst.Count; i++ ) {
                        tmplst[i] = tmplst[i].Replace('\0', '.');
                    }
                    this.richTextBox.Lines = tmplst.ToArray();
                    this.ColorizeAllFindText();
                    this.m_iFileLength = this.richTextBox.Lines.Length;
                }
                this.UpdateCaretPos();
                if ( focus != null ) {
                    focus.Focus();
                }
                return true;
            } catch ( Exception ) {
                return false;
            }
        }

        // reading from a binary: treat file like text files via replacing impeding chars
        StringBuilder ReadBinaryLines(string filename, long FromPos, int offset, out long FilePos) {
            // read beginning from position
            FromPos = Math.Max(0, FromPos);
            // stop reading at position
            long StopPos = FromPos + offset;
            // stringbuilder
            StringBuilder retlist = new StringBuilder();
            // filestream
            FileStream filestream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            // set initial read position
            filestream.Seek(FromPos, 0);
            // file
            StreamReader file = new StreamReader(filestream, Encoding.Default);
            // read buffer
            char[] lineOfText = new char[4096];
            // number of read chars
            int ret = 0;
            do {
                // read sort of a line, though it's rather a number of chars
                ret = file.Read(lineOfText, 0, lineOfText.Length);
                // modify read string: replace control chars with '.', w/o this the read string length would not correspond to the total file length
                for ( int i = 0; i < ret; i++ ) {
                    if ( char.IsControl(lineOfText[i]) ) {
                        lineOfText[i] = '.';
                    }
                }
                // append modified read string
                retlist.Append(lineOfText, 0, ret);
                // stop reading if requested via argument
                if ( filestream.Position >= StopPos ) {
                    break;
                }
            } while ( ret > 0 ); // eof = -1
            // give back the latest read position
            FilePos = filestream.Position;
            // return read lines 
            return retlist;
        }

        // search binary and return a stringbuilder on success
        bool SearchBinary(string filename, string search, bool bSearchDirectionDown, long FromPos, int offset, out long FilePos, out StringBuilder sb, out int SearchFoundPos) {
            // read beginning from position
            FromPos = Math.Max(0, FromPos);
            // stop reading at position
            long StopPos = FromPos + offset;
            // stringbuilder
            StringBuilder retlist = new StringBuilder();
            // filestream
            FileStream filestream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            // set initial read position
            filestream.Seek(FromPos, 0);
            // file
            StreamReader file = new StreamReader(filestream, Encoding.Default);
            // read buffer
            char[] lineOfText = new char[4096];
            // number of read chars
            int ret = 0;
            do {
                // read sort of a line, though it's rather a number of chars
                ret = file.Read(lineOfText, 0, lineOfText.Length);
                // modify rwad string: replace control chars with '.', w/o this the read string length would not correspond to the total file length
                for ( int i = 0; i < ret; i++ ) {
                    if ( char.IsControl(lineOfText[i]) ) {
                        lineOfText[i] = '.';
                    }
                }
                // append modified read string
                retlist.Append(lineOfText, 0, ret);
                // stop reading if requested via argument
                if ( filestream.Position >= StopPos ) {
                    break;
                }
            } while ( ret > 0 );  // eof = -1
            // give back the latest read position
            FilePos = filestream.Position;
            //
            if ( bSearchDirectionDown ) {
                SearchFoundPos = retlist.ToString().IndexOf(search, StringComparison.InvariantCultureIgnoreCase);
            } else {
                SearchFoundPos = retlist.ToString().LastIndexOf(search, StringComparison.InvariantCultureIgnoreCase);
            }
            if ( SearchFoundPos != -1 ) {
                // return read lines 
                sb = retlist;
                return true;
            } else {
                sb = null;
                return false;
            }
        }


        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool LockWindowUpdate(IntPtr hWndLock);
        int m_lastcl = -1;
        private void vScrollBar_Scroll(object sender, ScrollEventArgs e) {
            if ( !this.m_bBinaryFile ) {
                return;
            }

            // cast event info
            int newPos = e.NewValue;
            int oldPos = e.OldValue;
            ScrollOrientation so = e.ScrollOrientation;
            ScrollEventType st = e.Type;

            int lpp = (this.richTextBox.ClientSize.Height / this.richTextBox.Font.Height) - 1;

            // unteres Ende der richTextBox erreicht
            int cl = this.richTextBox.GetLineFromCharIndex(this.richTextBox.SelectionStart);
            if ( (cl == this.m_lastcl) && (newPos + lpp >= this.vScrollBar.Maximum) && (this.m_lReadPos < this.m_iFileLength) ) {

                // nachladen: die letzte m_lReadPos wird hier zur neuen m_lFromPos
                LockWindowUpdate(this.richTextBox.Handle);
                this.richTextBox.Clear();

                this.m_lFromPos = this.m_lReadPos;
                this.richTextBox.Text = this.ReadBinaryLines(this.m_sFileName, this.m_lFromPos, this.m_NumOfBytes, out this.m_lReadPos).ToString();
                this.ColorizeAllFindText();
                this.SetGlobalFilePos();

                // set scroller max and value AND return the altered e.NewValue
                this.vScrollBar.Value = 0;
                this.vScrollBar.Maximum = this.richTextBox.GetLineFromCharIndex(this.richTextBox.Text.Length - 1);
                e.NewValue = 0;

                // Caret versetzen (via Scroller geht es nicht)
                this.richTextBox.SelectionStart = 0;
                this.richTextBox.SelectionLength = 0;
                this.richTextBox.ScrollToCaret();
                LockWindowUpdate(IntPtr.Zero);
                return;
            }

            // oberes Ende erreicht
            if ( (cl == 0) && (cl == this.m_lastcl) && (newPos == 0) && (this.m_lFromPos > 0) ) {
                LockWindowUpdate(this.richTextBox.Handle);
                this.richTextBox.Clear();

                // scroll back to begin of file AND don't read too many chars after last known red position
                int tmpNumOfBytes = this.m_NumOfBytes;
                if ( this.m_lFromPos - this.m_NumOfBytes < 0 ) {
                    tmpNumOfBytes = (int)this.m_lFromPos;
                    this.m_lFromPos = 0;
                } else {
                    // normal case far away from begin of file
                    this.m_lFromPos -= this.m_NumOfBytes;
                }

                this.richTextBox.Text = this.ReadBinaryLines(this.m_sFileName, this.m_lFromPos, tmpNumOfBytes, out this.m_lReadPos).ToString();
                this.ColorizeAllFindText();
                this.SetGlobalFilePos();

                // Caret versetzen (via Scroller geht es nicht)
                this.richTextBox.SelectionStart = this.richTextBox.Text.Length - 1;
                this.richTextBox.SelectionLength = 0;
                this.richTextBox.ScrollToCaret();
                LockWindowUpdate(IntPtr.Zero);

                // set scroller max and value AND return the altered e.NewValue
                this.vScrollBar.Maximum = this.richTextBox.GetLineFromCharIndex(this.richTextBox.Text.Length - 1);
                this.vScrollBar.Value = this.vScrollBar.Maximum; // -lpp;
                e.NewValue = this.vScrollBar.Value;
                return;
            }

            this.m_lastcl = cl;

            // move textbox
            if ( newPos != oldPos ) {
                LockWindowUpdate(this.richTextBox.Handle);
                this.richTextBox.SelectionStart = Math.Max(0, this.richTextBox.GetFirstCharIndexFromLine(newPos));
                this.richTextBox.SelectionLength = 0;
                // avoids flickering on LineDown when the end of the text is visible
                int lastChar = this.richTextBox.GetCharIndexFromPosition(new Point(this.richTextBox.ClientSize.Width - 1, this.richTextBox.ClientSize.Height - 1));
                if ( lastChar != this.richTextBox.Text.Length - 1 ) {
                    this.richTextBox.ScrollToCaret();
                }
                if ( !this.findToolStripMenuItem.Checked ) {
                    this.richTextBox.Focus();
                }
                LockWindowUpdate(IntPtr.Zero);
                if ( st == ScrollEventType.ThumbTrack ) {
                    Application.DoEvents();
                }
                this.UpdateCaretPos();
                return;
            }

        }

        private void rtfScrolled(object sender, RTFScrolledBottom.WheelEventArgs e) {
            if ( !this.m_bBinaryFile ) {
                return;
            }
            int lpp = (this.richTextBox.ClientSize.Height / this.richTextBox.Font.Height) - 1;
            if ( e.WheelEvent == "WheelDown" ) {
                this.vScrollBar_Scroll(this.richTextBox, new ScrollEventArgs(ScrollEventType.EndScroll, this.vScrollBar.Value, Math.Min(this.vScrollBar.Maximum, this.vScrollBar.Value + lpp)));
                this.vScrollBar.Value = Math.Min(this.vScrollBar.Maximum, this.vScrollBar.Value + lpp);
            }
            if ( e.WheelEvent == "WheelUp" ) {
                this.vScrollBar_Scroll(this.richTextBox, new ScrollEventArgs(ScrollEventType.EndScroll, this.vScrollBar.Value, Math.Max(0, this.vScrollBar.Value - lpp)));
                this.vScrollBar.Value = Math.Max(0, this.vScrollBar.Value - lpp);
            }
        }

        private static readonly int EM_LINEINDEX = 0xbb;
        [DllImport("user32.dll")]
        extern static int SendMessage(IntPtr hwnd, int message, int wparam, int lparam);
        private void UpdateCaretPos() {
            if ( this.m_Parent == null ) {
                if ( this.m_bBinaryFile ) {
                    int position = this.richTextBox.SelectionStart;
                    this.labelStatus.ForeColor = Color.Black;
                    if ( (position + this.m_lFromPos == this.m_iFileLength) || (position + this.m_lFromPos == 0) ) {
                        this.labelStatus.ForeColor = Color.Red;
                    }
                    this.labelStatus.Text = ((position + this.m_lFromPos) / (double)this.m_iFileLength).ToString("0.000%") + "  --  " + (position + this.m_lFromPos).ToString("##,#0") + " (" + this.m_iFileLength.ToString("##,#") + ")";
                }
                return;
            }
            try {
                if ( this.m_bBinaryFile ) {
                    this.m_Parent.Text = Path.GetFileName(this.m_sFileName) + " --  " + (this.richTextBox.SelectionStart + this.m_lFromPos).ToString("##,#0") + " (" + this.m_iFileLength.ToString("##,#") + ")";
                } else {
                    int line, col, index;
                    if ( this.richTextBox.SelectionLength > 0 ) {
                        index = this.richTextBox.SelectionStart + this.richTextBox.SelectionLength;
                    } else {
                        index = this.richTextBox.SelectionStart;
                    }
                    line = this.richTextBox.GetLineFromCharIndex(index);
                    col = index - SendMessage(this.richTextBox.Handle, EM_LINEINDEX, -1, 0);
                    this.m_Parent.Text = Path.GetFileName(this.m_sFileName) + " --  " + (++line).ToString() + ", " + (++col).ToString();
                }
            } catch {; }
        }
        private void richTextBox_KeyDown(object sender, KeyEventArgs e) {
            int lpp = (this.richTextBox.ClientSize.Height / this.richTextBox.Font.Height) - 1;

            if ( (e.Modifiers == Keys.Control) && (e.KeyCode == Keys.F) ) {
                this.toggleFind();
            }

            if ( (e.Modifiers == Keys.None) && (e.KeyCode == Keys.F3) ) {
                this.searchForward();
            }
            if ( (e.Modifiers == Keys.Shift) && (e.KeyCode == Keys.F3) ) {
                this.searchBckward();
            }

            if ( e.KeyCode == Keys.PageDown ) {
                this.vScrollBar_Scroll(this.richTextBox, new ScrollEventArgs(ScrollEventType.EndScroll, this.vScrollBar.Value, Math.Min(this.vScrollBar.Maximum, this.vScrollBar.Value + lpp)));
                this.vScrollBar.Value = Math.Min(this.vScrollBar.Maximum, this.vScrollBar.Value + lpp);
            }
            if ( e.KeyCode == Keys.PageUp ) {
                this.vScrollBar_Scroll(this.richTextBox, new ScrollEventArgs(ScrollEventType.EndScroll, this.vScrollBar.Value, Math.Max(0, this.vScrollBar.Value - lpp)));
                this.vScrollBar.Value = Math.Max(0, this.vScrollBar.Value - lpp);
            }
            if ( e.KeyCode == Keys.Down ) {
                this.vScrollBar_Scroll(this.richTextBox, new ScrollEventArgs(ScrollEventType.EndScroll, this.vScrollBar.Value, Math.Min(this.vScrollBar.Maximum, this.vScrollBar.Value + 1)));
                this.vScrollBar.Value = Math.Min(this.vScrollBar.Maximum, this.vScrollBar.Value + 1);
            }
            if ( e.KeyCode == Keys.Up ) {
                this.vScrollBar_Scroll(this.richTextBox, new ScrollEventArgs(ScrollEventType.EndScroll, this.vScrollBar.Value, Math.Max(0, this.vScrollBar.Value - 1)));
                this.vScrollBar.Value = Math.Max(0, this.vScrollBar.Value - 1);
            }
            if ( (e.KeyCode == Keys.Home) && ((e.Modifiers & Keys.Control) == Keys.Control) ) {
                LockWindowUpdate(this.richTextBox.Handle);
                this.m_lFromPos = 0;
                this.richTextBox.Text = this.ReadBinaryLines(this.m_sFileName, this.m_lFromPos, this.m_NumOfBytes, out this.m_lReadPos).ToString();
                this.ColorizeAllFindText();
                this.SetGlobalFilePos();

                this.vScrollBar.Value = 0;
                this.vScrollBar.Maximum = this.richTextBox.GetLineFromCharIndex(this.richTextBox.Text.Length - 1);
                this.richTextBox.SelectionStart = 0;
                this.richTextBox.SelectionLength = 0;
                this.richTextBox.ScrollToCaret();
                LockWindowUpdate(IntPtr.Zero);
            }
            if ( (e.KeyCode == Keys.End) && ((e.Modifiers & Keys.Control) == Keys.Control) ) {
                LockWindowUpdate(this.richTextBox.Handle);
                this.m_lFromPos = Math.Max(0, this.m_iFileLength - this.m_NumOfBytes);
                this.richTextBox.Text = this.ReadBinaryLines(this.m_sFileName, this.m_lFromPos, this.m_NumOfBytes, out this.m_lReadPos).ToString();
                this.ColorizeAllFindText();
                this.SetGlobalFilePos();

                this.vScrollBar.Maximum = this.richTextBox.GetLineFromCharIndex(this.richTextBox.Text.Length - 1);
                this.vScrollBar.Value = this.vScrollBar.Maximum;
                this.richTextBox.SelectionStart = this.richTextBox.Text.Length;
                this.richTextBox.SelectionLength = 0;
                this.richTextBox.ScrollToCaret();
                LockWindowUpdate(IntPtr.Zero);
            }

            this.UpdateCaretPos();
        }

        void SetGlobalFilePos() {
            this.m_iFposBeg = (int)Math.Min(this.panelFilePos.ClientSize.Height - 10, (this.m_lFromPos * this.panelFilePos.ClientSize.Height / (double)this.m_iFileLength));
            this.m_iFposEnd = (int)Math.Max(10, (double)(((this.m_lReadPos - this.m_lFromPos) * this.panelFilePos.ClientSize.Height) / (double)this.m_iFileLength));
            this.panelFilePos.Invalidate();
        }

        private void richTextBox_KeyUp(object sender, KeyEventArgs e) {
            this.UpdateCaretPos();
        }
        private void richTextBox_MouseDown(object sender, MouseEventArgs e) {
            if ( e.Button == System.Windows.Forms.MouseButtons.Left ) {
                this.UpdateCaretPos();
                this.richTextBox.Focus();
            }
        }
        private void richTextBox_MouseMove(object sender, MouseEventArgs e) {
            if ( e.Button == System.Windows.Forms.MouseButtons.Left ) {
                this.UpdateCaretPos();
            }
        }
        private void richTextBox_MouseUp(object sender, MouseEventArgs e) {
            if ( e.Button == System.Windows.Forms.MouseButtons.Left ) {
                this.UpdateCaretPos();
            }
        }
        private void wordWrapToolStripMenuItem_Click(object sender, EventArgs e) {
            this.richTextBox.WordWrap = this.wordWrapToolStripMenuItem.Checked;
        }
        public void toggleFind() {
            this.findToolStripMenuItem.Checked = !this.findToolStripMenuItem.Checked;
            this.findToolStripMenuItem_Click(null, null);
        }
        private void findToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.findToolStripMenuItem.Checked ) {
                // show find controls
                this.tableLayoutPanel1.SetRowSpan(this.richTextBox, 1);
                this.tableLayoutPanel1.SetRowSpan(this.vScrollBar, 1);
                this.tableLayoutPanel1.SetRowSpan(this.panelFilePos, 1);
                this.tableLayoutPanel1.PerformLayout();
                this.textBoxFind.Visible = true;
                this.textBoxFind.Focus();
                this.buttonDown.Enabled = true;
                this.buttonUp.Enabled = true;
                this.buttonDown.Visible = true;
                this.buttonUp.Visible = true;
                this.buttonX.Visible = true;
                this.labelStatus.Visible = true;
            } else {
                // hide find controls normally
                this.textBoxFind.Visible = false;
                this.buttonDown.Visible = false;
                this.buttonUp.Visible = false;
                this.buttonX.Visible = false;
                this.labelStatus.Visible = false;
                this.tableLayoutPanel1.SetRowSpan(this.richTextBox, 2);
                this.tableLayoutPanel1.SetRowSpan(this.vScrollBar, 2);
                this.tableLayoutPanel1.SetRowSpan(this.panelFilePos, 2);
                this.tableLayoutPanel1.PerformLayout();
                this.richTextBox.Focus();
            }
            this.labelStatus.Text = "";
            this.Invalidate(true);
            if ( this.m_bBinaryFile ) {
                this.SetGlobalFilePos();
            }
        }

        // 20161016: ext. search up & down
        private void buttonUp_Click(object sender, EventArgs e) {
            this.searchBckward();
        }
        private void buttonDown_Click(object sender, EventArgs e) {
            this.searchForward();
        }
        // find stop
        private void findStopToolStripMenuItem_Click(object sender, EventArgs e) {
            this.m_bFindStop = true;
        }
        // search find-text in output window up 
        private void searchBckward() {
            if ( !this.findToolStripMenuItem.Checked ) {
                return;
            }
            if ( this.richTextBox.Text.Length == 0 ) {
                return;
            }
            if ( this.buttonUp.Text == "-stop-" ) {
                this.m_bFindStop = true;
                return;
            }

            this.findStopToolStripMenuItem.Enabled = true;
            this.m_bFindStop = false;
            StringComparison sc = StringComparison.OrdinalIgnoreCase;
            int ndx = this.richTextBox.Text.LastIndexOf(this.textBoxFind.Text, Math.Max(0, this.m_iLastFoundPos - this.textBoxFind.Text.Length), sc);
            if ( ndx != -1 ) {
                // found in richtextbox
                this.ColorizeFindString(ndx);
                this.m_iLastFoundPos = ndx;
                if ( this.m_bBinaryFile ) {
                    this.vScrollBar.Value = Math.Min(this.vScrollBar.Maximum, this.richTextBox.GetLineFromCharIndex(this.m_iLastFoundPos));
                }
                this.UpdateCaretPos();
            } else {
                // not found in richtextbox
                if ( this.m_lFromPos > 0 ) {
                    this.buttonDown.Enabled = false;
                    this.buttonUp.Text = "-stop-";
                    bool bFireAgain = false;
                    bool bFound = false;
                    while ( (this.m_lFromPos > 0) && this.buttonUp.Visible && !this.m_bFindStop ) {
                        Application.DoEvents();
                        // check previous chunk of file
                        int tmpNumOfBytes = this.m_NumOfBytes;
                        if ( this.m_lFromPos - this.m_NumOfBytes < 0 ) {
                            tmpNumOfBytes = (int)this.m_lFromPos;
                            this.m_lFromPos = 0;
                        } else {
                            this.m_lFromPos -= this.m_NumOfBytes;
                        }
                        int SearchFoundPos = 0;
                        StringBuilder sb = new StringBuilder();
                        bFound = this.SearchBinary(this.m_sFileName, this.textBoxFind.Text, false, this.m_lFromPos, this.m_NumOfBytes, out this.m_lReadPos, out sb, out SearchFoundPos);
                        if ( bFound ) {
                            LockWindowUpdate(this.richTextBox.Handle);
                            this.richTextBox.Text = sb.ToString();
                            this.richTextBox.SelectionStart = SearchFoundPos;
                            this.richTextBox.SelectionLength = 0;
                            this.richTextBox.ScrollToCaret();
                            LockWindowUpdate(IntPtr.Zero);
                            Application.DoEvents();
                            this.ColorizeAllFindText();
                            this.ColorizeFindString(SearchFoundPos);
                            this.vScrollBar.Maximum = this.richTextBox.GetLineFromCharIndex(this.richTextBox.Text.Length - 1);
                            this.vScrollBar.Value = this.richTextBox.GetLineFromCharIndex(SearchFoundPos);
                            this.m_iLastFoundPos = SearchFoundPos;
                            bFireAgain = false;
                            this.UpdateCaretPos();
                            break;
                        } else {
                            bFireAgain = true;
                        }
                        this.SetGlobalFilePos();
                        this.UpdateCaretPos();
                    }
                    this.buttonDown.Enabled = true;
                    this.buttonUp.Text = "up";

                    // essential
                    Application.DoEvents();
                    Application.DoEvents();
                    Application.DoEvents();
                    if ( this.buttonUp.Visible && bFireAgain && (this.m_lFromPos > 0) && !this.m_bFindStop ) {
                        // fire event again
                        this.buttonUp.PerformClick();
                    }

                    if ( !this.buttonUp.Visible && !bFound || this.m_bFindStop ) {
                        LockWindowUpdate(this.richTextBox.Handle);
                        this.richTextBox.Text = this.ReadBinaryLines(this.m_sFileName, this.m_lFromPos, this.m_NumOfBytes, out this.m_lReadPos).ToString();
                        this.ColorizeAllFindText();
                        this.SetGlobalFilePos();
                        this.vScrollBar.Value = 0;
                        this.vScrollBar.Maximum = this.richTextBox.GetLineFromCharIndex(this.richTextBox.Text.Length - 1);
                        this.richTextBox.SelectionStart = 0;
                        this.richTextBox.SelectionLength = 0;
                        this.m_iLastFoundPos = 0;
                        this.richTextBox.ScrollToCaret();
                        LockWindowUpdate(IntPtr.Zero);
                        this.UpdateCaretPos();
                    }

                    if ( (this.m_lFromPos == 0) && !bFound ) {
                        this.richTextBox_KeyDown(null, new KeyEventArgs(Keys.Control | Keys.Home));
                        this.m_iLastFoundPos = 0;
                        this.UpdateCaretPos();
                    }
                } else {
                    this.richTextBox_KeyDown(null, new KeyEventArgs(Keys.Control | Keys.Home));
                    this.m_iLastFoundPos = 0;
                    this.UpdateCaretPos();
                }
            }
            this.findStopToolStripMenuItem.Enabled = false;

            // 20161016
            if ( this.findToolStripMenuItem.Checked ) {
                this.textBoxFind.Focus();
            }
        }
        // search find-text in output window down
        private void searchForward() {
            if ( !this.findToolStripMenuItem.Checked ) {
                return;
            }
            if ( this.richTextBox.Text.Length == 0 ) {
                return;
            }
            if ( this.buttonDown.Text == "-stop-" ) {
                this.m_bFindStop = true;
                return;
            }

            this.findStopToolStripMenuItem.Enabled = true;
            this.m_bFindStop = false;
            StringComparison sc = StringComparison.OrdinalIgnoreCase;
            int ndx = -1;
            if ( this.m_iLastFoundPos + this.textBoxFind.Text.Length < this.richTextBox.Text.Length ) {
                ndx = this.richTextBox.Text.IndexOf(this.textBoxFind.Text, this.m_iLastFoundPos + this.textBoxFind.Text.Length, sc);
            }
            if ( ndx != -1 ) {
                // search text found within richtextbox
                this.ColorizeFindString(ndx);
                this.m_iLastFoundPos = ndx;
                if ( this.m_bBinaryFile ) {
                    this.vScrollBar.Value = Math.Min(this.vScrollBar.Maximum, this.richTextBox.GetLineFromCharIndex(this.m_iLastFoundPos));
                }
                this.UpdateCaretPos();
            } else {
                // search text not found within richtextbox, but more file chunks are available
                if ( this.m_lReadPos < this.m_iFileLength ) {
                    bool bFound = false;
                    bool bFireAgain = false;
                    this.buttonDown.Text = "-stop-";
                    this.buttonUp.Enabled = false;
                    // loop file until search text was found OR file end was reached
                    while ( (this.m_lReadPos < this.m_iFileLength) && this.buttonDown.Visible && !this.m_bFindStop ) {
                        Application.DoEvents();
                        // check next chunk of file
                        this.m_lFromPos = this.m_lReadPos;
                        int SearchFoundPos = 0;
                        StringBuilder sb = new StringBuilder();
                        bFound = this.SearchBinary(this.m_sFileName, this.textBoxFind.Text, true, this.m_lFromPos, this.m_NumOfBytes, out this.m_lReadPos, out sb, out SearchFoundPos);
                        if ( bFound ) {
                            LockWindowUpdate(this.richTextBox.Handle);
                            this.richTextBox.Text = sb.ToString();
                            this.vScrollBar.Maximum = this.richTextBox.GetLineFromCharIndex(this.richTextBox.Text.Length - 1);
                            this.vScrollBar.Value = this.richTextBox.GetLineFromCharIndex(SearchFoundPos);
                            this.richTextBox.SelectionStart = SearchFoundPos;
                            this.richTextBox.SelectionLength = 0;
                            this.richTextBox.ScrollToCaret();
                            LockWindowUpdate(IntPtr.Zero);
                            Application.DoEvents();
                            this.ColorizeAllFindText();
                            this.ColorizeFindString(SearchFoundPos);
                            this.m_iLastFoundPos = SearchFoundPos;
                            bFireAgain = false;
                            break;
                        } else {
                            bFireAgain = true;
                        }
                        this.SetGlobalFilePos();
                        this.UpdateCaretPos();
                    }
                    this.buttonDown.Text = "down";
                    this.buttonUp.Enabled = true;

                    Application.DoEvents();
                    Application.DoEvents();
                    Application.DoEvents();

                    // search again in next chunk
                    if ( this.buttonDown.Visible && bFireAgain && (this.m_lReadPos < this.m_iFileLength) && !this.m_bFindStop ) {
                        this.buttonDown.PerformClick();
                    }

                    // after search break or text not found, show current chunk
                    if ( (!this.buttonDown.Visible && !bFound) || this.m_bFindStop ) {
                        LockWindowUpdate(this.richTextBox.Handle);
                        this.richTextBox.Text = this.ReadBinaryLines(this.m_sFileName, this.m_lFromPos, this.m_NumOfBytes, out this.m_lReadPos).ToString();
                        this.ColorizeAllFindText();
                        this.SetGlobalFilePos();
                        this.vScrollBar.Value = 0;
                        this.vScrollBar.Maximum = this.richTextBox.GetLineFromCharIndex(this.richTextBox.Text.Length - 1);
                        this.m_iLastFoundPos = 0;
                        this.richTextBox.SelectionStart = 0;
                        this.richTextBox.SelectionLength = 0;
                        this.richTextBox.ScrollToCaret();
                        LockWindowUpdate(IntPtr.Zero);
                        this.UpdateCaretPos();
                    }

                    // reached file end AND search text was not found: force jump to file end
                    if ( (this.m_lReadPos == this.m_iFileLength) && !bFound ) {
                        this.richTextBox_KeyDown(null, new KeyEventArgs(Keys.Control | Keys.End));
                        this.richTextBox.SelectionStart = this.richTextBox.Text.Length;
                        this.m_iLastFoundPos = this.richTextBox.Text.Length;
                        this.UpdateCaretPos();
                    }
                } else {
                    // reached file end: force jump to file end
                    this.richTextBox_KeyDown(null, new KeyEventArgs(Keys.Control | Keys.End));
                    this.richTextBox.SelectionStart = this.richTextBox.Text.Length;
                    this.m_iLastFoundPos = this.richTextBox.Text.Length;
                    this.UpdateCaretPos();
                }
            }
            this.findStopToolStripMenuItem.Enabled = false;

            // 20161016
            if ( this.findToolStripMenuItem.Checked ) {
                this.textBoxFind.Focus();
            }
        }
        // colorize the find string ONCE in output window 
        public void ColorizeFindString(int ndx) {
            this.richTextBox.SelectionBackColor = Color.Yellow;
            this.richTextBox.SelectionColor = Color.Red;
            int startLine = this.richTextBox.GetLineFromCharIndex(ndx);
            int topIndex = this.richTextBox.GetCharIndexFromPosition(new System.Drawing.Point(1, 1));
            int topLine = this.richTextBox.GetLineFromCharIndex(topIndex);
            int bottomIndex = this.richTextBox.GetCharIndexFromPosition(new System.Drawing.Point(1, this.richTextBox.Height - 1));
            int bottomLine = this.richTextBox.GetLineFromCharIndex(bottomIndex);
            int numVisibleLines = bottomLine - topLine + 1;
            if ( (startLine > bottomLine) || (startLine < topLine) ) {
                int cix = this.richTextBox.GetFirstCharIndexFromLine(Math.Max(0, startLine - numVisibleLines / 3 + 1));
                this.richTextBox.Select(cix, 0);
                this.richTextBox.ScrollToCaret();
            }
            this.richTextBox.Select(ndx, this.textBoxFind.Text.Length);
            this.richTextBox.SelectionColor = Color.Yellow;
            this.richTextBox.SelectionBackColor = Color.Blue;
        }
        // colorize the find string EVERYWEHRE in output window 
        void ColorizeAllFindText() {
            if ( !this.textBoxFind.Visible || !this.colorizeFindTextToolStripMenuItem.Checked || (this.textBoxFind.Text.Length == 0) ) {
                this.textBoxFind.BackColor = SystemColors.Window;
                return;
            }

            int memstartpos = this.richTextBox.SelectionStart;

            bool bSomethingFound = false;
            StringComparison sc = StringComparison.OrdinalIgnoreCase;
            int start = 0;
            do {
                Application.DoEvents();
                int ndx = this.richTextBox.Text.IndexOf(this.textBoxFind.Text, Math.Min(this.richTextBox.Text.Length, start), sc);
                if ( ndx != -1 ) {
                    bSomethingFound = true;
                    this.richTextBox.Select(ndx, this.textBoxFind.Text.Length);
                    this.richTextBox.SelectionBackColor = Color.Yellow;
                    this.richTextBox.SelectionColor = Color.Red;
                    start = ndx + this.textBoxFind.Text.Length;
                } else {
                    break;
                }
            } while ( true );

            this.textBoxFind.BackColor = SystemColors.Window;
            if ( !bSomethingFound ) {
                this.textBoxFind.BackColor = Color.MistyRose;
            }

            this.richTextBox.SelectionStart = memstartpos;
            this.richTextBox.SelectionLength = 0;
        }
        private void textBoxFind_KeyDown(object sender, KeyEventArgs e) {
            if ( e.KeyCode == Keys.Enter ) {
                this.ColorizeAllFindText();
                this.searchForward();
            }
            // 20161016: forward most of the navigation related keys to richTextBox
            if ( (e.KeyCode != Keys.Up) && (e.KeyCode != Keys.Down) && (e.KeyCode != Keys.Left) && (e.KeyCode != Keys.Right) ) {
                this.richTextBox_KeyDown(sender, e);
            }
        }
        private void textBoxFind_TextChanged(object sender, EventArgs e) {
            if ( this.textBoxFind.Text.Length == 0 ) {
                return;
            }

            // allow to jump to a certain file position
            if ( this.m_bBinaryFile ) {
                double n = 0f;
                //             last character in search string is the % sign          AND              everything prior to the % sign is an integer number  
                if ( (this.textBoxFind.Text[this.textBoxFind.Text.Length - 1] == '%') && double.TryParse(this.textBoxFind.Text.Substring(0, this.textBoxFind.Text.Length - 1), out n) && (n >= 0) && (n <= 100) ) {
                    DialogResult dr = MessageBox.Show("Jump to the file position " + this.textBoxFind.Text + " ?", "Question", MessageBoxButtons.OKCancel);
                    if ( dr == DialogResult.OK ) {
                        this.m_lFromPos = (long)((n / 100f) * this.m_iFileLength);
                        this.richTextBox.Text = this.ReadBinaryLines(this.m_sFileName, this.m_lFromPos, this.m_NumOfBytes, out this.m_lReadPos).ToString();
                        this.textBoxFind.Text = "";
                        this.ColorizeAllFindText();
                        this.SetGlobalFilePos();
                        this.vScrollBar.Value = 0;
                        this.vScrollBar.Maximum = this.richTextBox.GetLineFromCharIndex(this.richTextBox.Text.Length - 1);
                        if ( !this.findToolStripMenuItem.Checked ) {
                            this.richTextBox.Focus();
                        }
                        Point pt = new Point(this.richTextBox.Bounds.X + this.richTextBox.Width / 2, this.richTextBox.Bounds.Y + this.richTextBox.Height / 2);
                        Cursor.Position = this.richTextBox.PointToScreen(pt);
                        return;
                    }
                }
            }

            // show search text field in pink color if search text could not be found in richtextbox 
            StringComparison sc = StringComparison.OrdinalIgnoreCase;
            int ndx = this.richTextBox.Text.IndexOf(this.textBoxFind.Text, 0, sc);
            if ( ndx == -1 ) {
                this.textBoxFind.BackColor = Color.MistyRose;
            } else {
                this.textBoxFind.BackColor = SystemColors.Window;
            }
        }
        private void colorizeFindTextToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.colorizeFindTextToolStripMenuItem.Checked ) {
                this.ColorizeAllFindText();
            } else {
                this.resetColorsToolStripMenuItem_Click(null, null);
            }
        }
        // remove all color from output window
        private void resetColorsToolStripMenuItem_Click(object sender, EventArgs e) {
            this.richTextBox.Select(0, this.richTextBox.Text.Length);
            this.richTextBox.SelectionColor = SystemColors.WindowText;
            this.richTextBox.SelectionBackColor = SystemColors.Control;
            this.richTextBox.Select(0, 0);
        }
        // close input controls to search in the output window
        private void buttonX_Click(object sender, EventArgs e) {
            this.findToolStripMenuItem.Checked = false;
            this.findToolStripMenuItem_Click(null, null);
        }
        // copy selection to clipboard
        private void copyToolStripMenuItem_Click(object sender, EventArgs e) {
            this.richTextBox.Copy();
        }

        public class RTFScrolledBottom : RichTextBox {
            public class WheelEventArgs : EventArgs {
                public WheelEventArgs(string wheelevent) {
                    this.WheelEvent = wheelevent;
                }
                public string WheelEvent { get; set; }
            }

            public event EventHandler<WheelEventArgs> Scrolled;

            private const int WM_VSCROLL = 0x115;
            private const int WM_MOUSEWHEEL = 0x20A;
            private const int WM_USER = 0x400;
            private const int SB_VERT = 1;
            private const int EM_SETSCROLLPOS = WM_USER + 222;
            private const int EM_GETSCROLLPOS = WM_USER + 221;

            [DllImport("user32.dll")]
            private static extern bool GetScrollRange(IntPtr hWnd, int nBar, out int lpMinPos, out int lpMaxPos);

            [DllImport("user32.dll")]
            private static extern IntPtr SendMessage(IntPtr hWnd, Int32 wMsg, Int32 wParam, ref Point lParam);

            // 32 bit scrolling of pane slider
            // http://stackoverflow.com/questions/1380104/cc-setscrollpos-user32-dll
            [DllImport("user32.dll")]
            static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);
            [DllImport("User32.dll")]
            private extern static int GetScrollPos(IntPtr hWnd, int nBar);
            private enum ScrollBarType : uint { SbHorz = 0, SbVert = 1, SbCtl = 2, SbBoth = 3 }

            public void SetVerticalScrollPos(int pos) {
                SetScrollPos(this.Handle, 0x1, pos, true);
            }
            public int GetVerticalScrollPos() {
                int n = GetScrollPos(this.Handle, (int)ScrollBarType.SbVert);
                if ( n > this.ClientSize.Height ) {
                    n += this.ClientSize.Height;
                }

                return n;
            }
            public bool IsAtMaxScroll() {
                int minScroll;
                int maxScroll;
                GetScrollRange(this.Handle, SB_VERT, out minScroll, out maxScroll);
                Point rtfPoint = Point.Empty;
                SendMessage(this.Handle, EM_GETSCROLLPOS, 0, ref rtfPoint);

                return (rtfPoint.Y + this.ClientSize.Height >= maxScroll);
            }
            public bool IsAtMinScroll() {
                int minScroll;
                int maxScroll;
                GetScrollRange(this.Handle, SB_VERT, out minScroll, out maxScroll);
                Point rtfPoint = Point.Empty;
                SendMessage(this.Handle, EM_GETSCROLLPOS, 0, ref rtfPoint);

                return (rtfPoint.Y <= minScroll);
            }

            public int GetMaxScrollPosV() {
                int minScroll;
                int maxScroll;
                GetScrollRange(this.Handle, SB_VERT, out minScroll, out maxScroll);
                return maxScroll;
            }
            protected virtual void OnScrolled(WheelEventArgs e) {
                if ( Scrolled != null ) {
                    Scrolled(this, e);
                }
            }
            protected override void WndProc(ref Message m) {
                if ( m.Msg == WM_MOUSEWHEEL ) {
                    if ( (long)m.WParam < 0 ) {
                        // scroll down
                        this.OnScrolled(new WheelEventArgs("WheelDown"));
                    } else {
                        // scroll up
                        this.OnScrolled(new WheelEventArgs("WheelUp"));
                    }
                }

                base.WndProc(ref m);
            }
        }

        private void richTextBox_SizeChanged(object sender, EventArgs e) {
            this.vScrollBar.LargeChange = (this.richTextBox.ClientSize.Height / this.richTextBox.Font.Height) - 1;
        }

        // Anzeige der globalen Dateiposition
        private void panelFilePos_Paint(object sender, PaintEventArgs e) {
            e.Graphics.FillRectangle(Brushes.Green, 2, this.m_iFposBeg, 4, this.m_iFposEnd);
        }

        private void textBoxFind_MouseHover(object sender, EventArgs e) {
            if ( this.m_bBinaryFile ) {
                if ( this.m_toolTip.Active ) {
                    Point pt = this.PointToScreen(this.textBoxFind.Location);
                    pt.X += 20;
                    pt.Y += this.textBoxFind.Height + 10;
                    pt = this.textBoxFind.PointToClient(pt);
                    this.m_toolTip.Show("type search text OR a % value into the textbox to jump to this file position", this.textBoxFind, pt.X, pt.Y);
                }
            }
        }
        private void textBoxFind_MouseLeave(object sender, EventArgs e) {
            this.m_toolTip.Hide(this.textBoxFind);
        }

        // organize printing of a text snippet
        private void printSelectionToolStripMenuItem_Click(object sender, EventArgs e) {
            string text = this.richTextBox.SelectedText;
            if ( text.Length == 0 ) {
                return;
            }
            GrzTools.Print.PrintText(text, true);
        }
        private void contextMenuStrip_Opening(object sender, CancelEventArgs e) {
            this.printSelectionToolStripMenuItem.Enabled = this.richTextBox.SelectedText.Length > 0 ? true : false;
        }

    }

    // unused, but interesting
    public class VerticalProgressBar : ProgressBar {
        protected override CreateParams CreateParams {
            get {
                CreateParams cp = base.CreateParams;
                cp.Style |= 0x04;
                return cp;
            }
        }
    }

    //Must add using System.Runtime.InteropServices;
    //We can define some extension method for this purpose
    public static class RichTextBoxExtension {
        [DllImport("user32")]
        private static extern int GetScrollInfo(IntPtr hwnd, int nBar, ref SCROLLINFO scrollInfo);
        public struct SCROLLINFO {
            public int cbSize;
            public int fMask;
            public int min;
            public int max;
            public int nPage;
            public int nPos;
            public int nTrackPos;
        }
        public static SCROLLINFO GetScrollInfoV(this RichTextBox rtb) {
            SCROLLINFO scrollInfo = new SCROLLINFO();
            scrollInfo.cbSize = Marshal.SizeOf(scrollInfo);
            //SIF_RANGE = 0x1, SIF_TRACKPOS = 0x10,  SIF_PAGE= 0x2
            scrollInfo.fMask = 0x10 | 0x1 | 0x2;
            GetScrollInfo(rtb.Handle, 1, ref scrollInfo); //nBar = 1 -> VScrollbar
            return scrollInfo;
        }
        public static bool ReachedBottom(this RichTextBox rtb) {
            SCROLLINFO scrollInfo = new SCROLLINFO();
            scrollInfo.cbSize = Marshal.SizeOf(scrollInfo);
            //SIF_RANGE = 0x1, SIF_TRACKPOS = 0x10,  SIF_PAGE= 0x2
            scrollInfo.fMask = 0x10 | 0x1 | 0x2;
            GetScrollInfo(rtb.Handle, 1, ref scrollInfo); //nBar = 1 -> VScrollbar
            return scrollInfo.max == scrollInfo.nTrackPos + scrollInfo.nPage;
        }
    }
}
