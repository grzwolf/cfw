using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;                                    // file stream 
using System.Runtime.InteropServices;               // DLLImport

namespace FileViewCtl
{
    public partial class FileView: UserControl
    {
        string m_sFileName = "";
        Form m_Parent = null;

        bool m_bBinaryFile = false;
        List<string> m_list = new List<string>();
        int m_iCurrentBlockEnd = 0;
        int m_iCurrentBlockBeg = 0;
        int m_iMaxFilePosition = 0;
        long m_iFileLength = 0;
        int m_iLastFoundPos = 0;
        DateTime m_dtLastUpdate = DateTime.Now;
        List<long> m_lSeekPosD = new List<long>();
        long m_lSeekPosU = -1;
        long m_lReadPos = 0;


//        Form form = new Form();

        public FileView()
        {
            InitializeComponent();

            // hide find controls normally
            this.textBoxFind.Visible = false;
            this.buttonDown.Visible = false;
            this.buttonUp.Visible = false;
            this.buttonX.Visible = false;
            this.tableLayoutPanel1.SetRowSpan(this.richTextBox, 2);

            // some extra events
            this.richTextBox.ScrolledToBottom += rtfScrolledBottom_ScrolledToBottom;
            this.richTextBox.ScrolledToTop += rtfScrolledBottom_ScrolledToTop;
            this.richTextBox.Scrolled += rtfScrolled;

/*
            form.StartPosition = FormStartPosition.CenterParent;
            form.ControlBox = false;
            form.Size = new System.Drawing.Size(500, 20);
            form.Text = "";
            form.Show(this);
*/ 
        }

        public void Clear( Form parent )
        {
//            form.Text = "";

            m_Parent = parent;
            this.richTextBox.Clear();
            this.richTextBox.Text = "<nothing>";

            this.vScrollBar.Visible = false;
            this.tableLayoutPanel1.SetColumnSpan(this.richTextBox, 5);
            this.richTextBox.ScrollBars = RichTextBoxScrollBars.Both;
            this.richTextBox.WordWrap = true;

            m_list.Clear();
        }

        public bool LoadDocumentFirstPage( string filename )
        {
            m_sFileName = filename;
            this.richTextBox.Clear();
            m_list.Clear();

            var filestream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var file = new StreamReader(filestream, Encoding.Default, true, 1000);
            int iBreaker = 0;
            string lineOfText;
            while ( ((lineOfText = file.ReadLine()) != null) && (iBreaker++ < 2000) ) {
                for ( int j = 0; j < lineOfText.Length; j++ ) {
                    // remove zeros
                    if ( lineOfText[j] == '\0' ) {
                        lineOfText.Remove(j, 1);
                    }
                    // remove ctrl chars
                    if ( char.IsControl(lineOfText[j]) ) {
                        lineOfText.Replace(lineOfText[j], '.');
                    }
                }
                m_list.Add(lineOfText);
            }
            this.richTextBox.Lines = m_list.ToArray();
            this.richTextBox.Select(1, 0);
            this.richTextBox.ScrollToCaret();

            return true;
        }

        public bool LoadDocument( Form parent, string filename )
        {
            try {
                Encoding enc;
                m_sFileName = filename;
                m_Parent = parent;
                this.richTextBox.Clear();
                m_list.Clear();
                m_iCurrentBlockEnd = 0;
                m_iCurrentBlockBeg = 0;
                m_iMaxFilePosition = 0;
                m_iFileLength = 0;
                m_iLastFoundPos = 0;
                m_lSeekPosD.Clear();
                m_lSeekPosD.Add(0);
                m_lSeekPosU = -1;
                m_lReadPos = 0;
                m_bBinaryFile = false;
                DateTime m_dtLastUpdate = DateTime.Now;
                if ( !IsTextFile(out enc, filename, 100) ) {

                    // disable original vert. Scroller of textbox, setup a separate vert. Scroller
                    this.tableLayoutPanel1.SetColumnSpan(this.richTextBox, 4);
                    this.vScrollBar.Visible = true;
                    this.vScrollBar.Value = 0;
                    this.richTextBox.ScrollBars = RichTextBoxScrollBars.Horizontal;
                    this.richTextBox.WordWrap = false;

                    //
                    // binary files as text
                    //
                    m_bBinaryFile = true;
                    FileInfo f = new FileInfo(m_sFileName);
                    m_iFileLength = f.Length;
                    int chrPerLine = (int)(1.24f * this.richTextBox.ClientSize.Width / this.richTextBox.Font.Size);
                    this.richTextBox.Text = ReadBinaryLines(m_sFileName, 1000, chrPerLine, 0, 0, out m_lReadPos).ToString();
                    this.vScrollBar.Maximum = this.richTextBox.GetLineFromCharIndex(this.richTextBox.Text.Length-1);

                } else {

                    //
                    // all files considered being text files
                    //

                    this.vScrollBar.Visible = false;
                    this.tableLayoutPanel1.SetColumnSpan(this.richTextBox, 5);
                    this.richTextBox.ScrollBars = RichTextBoxScrollBars.Both;
                    this.richTextBox.WordWrap = true;

                    FileStream fs = new FileStream(m_sFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    List<string> tmplst = new List<string>();
                    using ( var sr = new StreamReader(fs) ) {
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
                    m_iMaxFilePosition = this.richTextBox.Lines.Length;
                }
                UpdateCaretPos();
                return true;
            } catch ( Exception ) {
                return false;
            }
        }
        StringBuilder ReadBinaryLines( string filename, long NumberOfLines, int chrPerLine, long FromPos, long StopPos, out long FilePos )
        {
            StringBuilder retlist = new StringBuilder();
            var filestream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            filestream.Seek(FromPos, 0);
            var file = new StreamReader(filestream, Encoding.Default, true, 10000);
            string lineOfText;
            long iBreaker = NumberOfLines > 0 ? NumberOfLines : 1;
            while ( ((lineOfText = file.ReadLine()) != null) && (iBreaker > 0) ) {
                lineOfText = lineOfText.Replace('\0', '.');
                lineOfText = lineOfText.Replace('\t', ' ');
                lineOfText = lineOfText.Replace('\v', ' ');
                lineOfText = lineOfText.Replace('\b', ' ');
                lineOfText = lineOfText.Replace('\f', ' ');
                for ( int n=chrPerLine; n<lineOfText.Length; n+=chrPerLine ) {
                    lineOfText = lineOfText.Insert(Math.Min(n, lineOfText.Length), "\r");
                }
                lineOfText += "\r";
                retlist.Append(lineOfText);
                if ( NumberOfLines > 0 ) {
                    iBreaker--;
                }
                if ( StopPos > 0 ) {
                    if ( filestream.Position >= StopPos ) {
                        break;
                    }
                }
            }
            FilePos = filestream.Position;
            return retlist;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool LockWindowUpdate( IntPtr hWndLock );
        bool bReload = false;
        int m_lastcl = -1;
        private void vScrollBar_Scroll( object sender, ScrollEventArgs e )
        {
            if ( !m_bBinaryFile ) {
                return;
            }

            // cast event info
            int newPos =  e.NewValue;
            int oldPos =  e.OldValue;
            ScrollOrientation so = e.ScrollOrientation;
            ScrollEventType st = e.Type;

            // debug
//            form.Text = "o:" + oldPos.ToString() + " n:" + newPos.ToString() + " sel:" + this.richTextBox.SelectionStart.ToString() + " seek:" + m_lSeekPos.Count.ToString() + " v:" + vScrollBar.Value.ToString() + " m:" + vScrollBar.Maximum.ToString();

            int lpp = (richTextBox.ClientSize.Height / richTextBox.Font.Height) - 1;
            int chrPerLine = (int)(1.24f * this.richTextBox.ClientSize.Width / this.richTextBox.Font.Size);

            // unteres Ende der richTextBox erreicht
//            if ( (newPos + lpp > this.vScrollBar.Maximum) && (m_lReadPos < m_iFileLength) && !bReload && (st == ScrollEventType.EndScroll) ) {
            int cl = this.richTextBox.GetLineFromCharIndex(this.richTextBox.SelectionStart);
            if ( (cl == m_lastcl) && (newPos + lpp >= this.vScrollBar.Maximum) && (m_lReadPos < m_iFileLength) && !bReload && (st == ScrollEventType.EndScroll) ) {
//            if ( (cl == m_lastcl) && (this.richTextBox.SelectionStart >= this.richTextBox.Text.Length-1) && (newPos + lpp >= this.vScrollBar.Maximum) && (m_lReadPos < m_iFileLength) && !bReload && (st == ScrollEventType.EndScroll) ) {

                // nachladen: die letzte m_lReadPos wird hier zur neuen SeekPos
                LockWindowUpdate(this.richTextBox.Handle);
                this.richTextBox.Clear();
                bReload = false;
                this.richTextBox.Text = ReadBinaryLines(m_sFileName, 1000, chrPerLine, m_lReadPos, 0, out m_lReadPos).ToString();
                m_lSeekPosD.Add(m_lReadPos);
                // set scroller max and value AND return the altered e.NewValue
                this.vScrollBar.Value = 0;
                this.vScrollBar.Maximum = this.richTextBox.GetLineFromCharIndex(this.richTextBox.Text.Length-1);
                e.NewValue = 0;
                // Caret versetzen (via Scroller geht es nicht)
                this.richTextBox.SelectionStart = 0;
                this.richTextBox.SelectionLength = 0;
                this.richTextBox.ScrollToCaret();
                LockWindowUpdate(IntPtr.Zero);

//                form.Text = "down o:" + oldPos.ToString() + " n:" + newPos.ToString() + " sel:" + this.richTextBox.SelectionStart.ToString() + " seek:" + m_lSeekPos.Count.ToString() + " v:" + vScrollBar.Value.ToString() + " m:" + vScrollBar.Maximum.ToString();
                return;
            }

            // oberes Ende erreicht, wenn dauerhaft von unten gekommen wurde
            if ( (cl == 0) && (cl == m_lastcl) && (m_lSeekPosU != -1) && !bReload && (st == ScrollEventType.EndScroll) ) {
//            if ( (this.richTextBox.SelectionStart == 0) && (m_lSeekPosU != -1) && !bReload && (st == ScrollEventType.EndScroll) ) {
                // nachladen: die letzte SeekPos löschen und dann wieder die letzte SeekPos nehmen
                LockWindowUpdate(this.richTextBox.Handle);
                this.richTextBox.Clear();
                bReload = false;
                this.richTextBox.Text = ReadBinaryLines(m_sFileName, -1, chrPerLine, m_lSeekPosU-150000, m_lSeekPosU, out m_lReadPos).ToString();
                m_lSeekPosU = Math.Max(0, m_lSeekPosU-150000); 
                // 
                bReload = false;
                // Caret versetzen (via Scroller geht es nicht)
                this.richTextBox.SelectionStart = this.richTextBox.Text.Length - 1;
                this.richTextBox.SelectionLength = 0;
                this.richTextBox.ScrollToCaret();
                LockWindowUpdate(IntPtr.Zero);
                // set scroller max and value AND return the altered e.NewValue
                this.vScrollBar.Maximum = this.richTextBox.GetLineFromCharIndex(this.richTextBox.Text.Length-1); // alt war: this.richTextBox.Lines.Length;
                this.vScrollBar.Value = this.vScrollBar.Maximum; // -lpp;
                e.NewValue = this.vScrollBar.Value;

//                form.Text = "up o:" + oldPos.ToString() + " n:" + newPos.ToString() + " sel:" + this.richTextBox.SelectionStart.ToString() + " seek:" + m_lSeekPos.Count.ToString() + " v:" + vScrollBar.Value.ToString() + " m:" + vScrollBar.Maximum.ToString();
                return;
            }

            // oberes Ende erreicht, wenn vorher nach unten gegangen aber dort gewendet wurde 
            if ( (cl == 0) && (cl == m_lastcl) && (m_lSeekPosD.Count > 1) && !bReload && (st == ScrollEventType.EndScroll) ) {
//            if ( (this.richTextBox.SelectionStart == 0) && (m_lSeekPosD.Count > 1) && !bReload && (st == ScrollEventType.EndScroll) ) {
                // nachladen: die letzte SeekPos löschen und dann wieder die letzte SeekPos nehmen
                LockWindowUpdate(this.richTextBox.Handle);
                this.richTextBox.Clear();
                bReload = false;
                m_lSeekPosD.RemoveAt(m_lSeekPosD.Count-1);
                this.richTextBox.Text = ReadBinaryLines(m_sFileName, 1000, chrPerLine, m_lSeekPosD[m_lSeekPosD.Count-1], 0, out m_lReadPos).ToString();
                // 
                bReload = false;
                // Caret versetzen (via Scroller geht es nicht)
                this.richTextBox.SelectionStart = this.richTextBox.Text.Length - 1;
                this.richTextBox.SelectionLength = 0;
                this.richTextBox.ScrollToCaret();
                LockWindowUpdate(IntPtr.Zero);
                // set scroller max and value AND return the altered e.NewValue
                this.vScrollBar.Maximum = this.richTextBox.GetLineFromCharIndex(this.richTextBox.Text.Length-1); // alt war: this.richTextBox.Lines.Length;
                this.vScrollBar.Value = this.vScrollBar.Maximum; // -lpp;
                e.NewValue = this.vScrollBar.Value;

//                form.Text = "up o:" + oldPos.ToString() + " n:" + newPos.ToString() + " sel:" + this.richTextBox.SelectionStart.ToString() + " seek:" + m_lSeekPos.Count.ToString() + " v:" + vScrollBar.Value.ToString() + " m:" + vScrollBar.Maximum.ToString();
                return;
            }
            m_lastcl = cl;

            // move textbox
            if ( (newPos != oldPos) && !bReload ) {
                LockWindowUpdate(this.richTextBox.Handle);
                this.richTextBox.SelectionStart = this.richTextBox.GetFirstCharIndexFromLine(newPos);
                this.richTextBox.SelectionLength = 0;
                // avoids flickering on LineDown when the end of the text is visible
                int lastChar = richTextBox.GetCharIndexFromPosition(new Point(richTextBox.ClientSize.Width-1, richTextBox.ClientSize.Height-1));
                if ( lastChar != this.richTextBox.Text.Length-1 ) {
                    this.richTextBox.ScrollToCaret();
                }
                this.richTextBox.Focus();
                LockWindowUpdate(IntPtr.Zero);
//                form.Text = "normal o:" + oldPos.ToString() + " n:" + newPos.ToString() + " sel:" + this.richTextBox.SelectionStart.ToString() + " seek:" + m_lSeekPos.Count.ToString() + " v:" + vScrollBar.Value.ToString() + " m:" + vScrollBar.Maximum.ToString();
                return;
            }

//            form.Text = "nothing o:" + oldPos.ToString() + " n:" + newPos.ToString() + " sel:" + this.richTextBox.SelectionStart.ToString() + " seek:" + m_lSeekPos.Count.ToString() + " v:" + vScrollBar.Value.ToString() + " m:" + vScrollBar.Maximum.ToString();
            bReload = false;
        }

        private void rtfScrolled( object sender, RTFScrolledBottom.WheelEventArgs e )
        {
            if ( !m_bBinaryFile ) {
                return;
            }
            int lpp = (richTextBox.ClientSize.Height / richTextBox.Font.Height) - 1;
            if ( e.WheelEvent == "WheelDown" ) {
                vScrollBar_Scroll(this.richTextBox, new ScrollEventArgs(ScrollEventType.EndScroll, this.vScrollBar.Value, Math.Min(this.vScrollBar.Maximum, this.vScrollBar.Value + lpp)));
                this.vScrollBar.Value = Math.Min(this.vScrollBar.Maximum, this.vScrollBar.Value + lpp);
            }
            if ( e.WheelEvent == "WheelUp" ) {
                vScrollBar_Scroll(this.richTextBox, new ScrollEventArgs(ScrollEventType.EndScroll, this.vScrollBar.Value, Math.Max(0, this.vScrollBar.Value - lpp)));
                this.vScrollBar.Value = Math.Max(0, this.vScrollBar.Value - lpp);
            }
        }

        private void rtfScrolledBottom_ScrolledToBottom( object sender, EventArgs e )
        {
            if ( !m_bBinaryFile ) {
                return;
            }
        }
        private void rtfScrolledBottom_ScrolledToTop( object sender, EventArgs e )
        {
            if ( !m_bBinaryFile ) {
                return;
            }
        }

        private static int EM_LINEINDEX = 0xbb;
        [DllImport("user32.dll")]
        extern static int SendMessage( IntPtr hwnd, int message, int wparam, int lparam );
        private void UpdateCaretPos()
        {
            if ( m_Parent == null ) {
                return;
            }
            if ( m_bBinaryFile ) {
                m_Parent.Text = Path.GetFileName(m_sFileName) + " --  " + (this.richTextBox.GetLineFromCharIndex(this.richTextBox.SelectionStart) + m_iCurrentBlockBeg).ToString() + " (" + m_iMaxFilePosition.ToString() + ")";
            } else {
                int line, col, index;
                if ( this.richTextBox.SelectionLength > 0 ) {
                    index = this.richTextBox.SelectionStart + this.richTextBox.SelectionLength;
                } else {
                    index = this.richTextBox.SelectionStart;
                }
                line = this.richTextBox.GetLineFromCharIndex(index);
                col = index - SendMessage(this.richTextBox.Handle, EM_LINEINDEX, -1, 0);
                m_Parent.Text = Path.GetFileName(m_sFileName) + " --  " +  (++line).ToString() + ", " + (++col).ToString();
            }
        }
        private void richTextBox_KeyDown( object sender, KeyEventArgs e )
        {
            UpdateCaretPos();

            int lpp = (richTextBox.ClientSize.Height / richTextBox.Font.Height) - 1;
            int chrPerLine = (int)(1.24f * this.richTextBox.ClientSize.Width / this.richTextBox.Font.Size);

            if ( e.KeyCode == Keys.PageDown ) {
                vScrollBar_Scroll(this.richTextBox, new ScrollEventArgs(ScrollEventType.EndScroll, this.vScrollBar.Value, Math.Min(this.vScrollBar.Maximum, this.vScrollBar.Value + lpp)));
                this.vScrollBar.Value = Math.Min(this.vScrollBar.Maximum, this.vScrollBar.Value + lpp);
            }
            if ( e.KeyCode == Keys.PageUp ) {
                vScrollBar_Scroll(this.richTextBox, new ScrollEventArgs(ScrollEventType.EndScroll, this.vScrollBar.Value, Math.Max(0, this.vScrollBar.Value - lpp)));
                this.vScrollBar.Value = Math.Max(0, this.vScrollBar.Value - lpp);
            }
            if ( e.KeyCode == Keys.Down ) {
                vScrollBar_Scroll(this.richTextBox, new ScrollEventArgs(ScrollEventType.EndScroll, this.vScrollBar.Value, Math.Min(this.vScrollBar.Maximum, this.vScrollBar.Value + 1)));
                this.vScrollBar.Value = Math.Min(this.vScrollBar.Maximum, this.vScrollBar.Value + 1);
            }
            if ( e.KeyCode == Keys.Up ) {
                vScrollBar_Scroll(this.richTextBox, new ScrollEventArgs(ScrollEventType.EndScroll, this.vScrollBar.Value, Math.Max(0, this.vScrollBar.Value - 1)));
                this.vScrollBar.Value = Math.Max(0, this.vScrollBar.Value - 1);
            }
            if ( (e.KeyCode == Keys.Home) && ((e.Modifiers & Keys.Control) == Keys.Control) ) {
                LockWindowUpdate(this.richTextBox.Handle);
                m_lSeekPosU = -1;
                m_lSeekPosD.Clear();
                m_lSeekPosD.Add(0);
                this.richTextBox.Text = ReadBinaryLines(m_sFileName, 1000, chrPerLine, 0, 0, out m_lReadPos).ToString();
                m_lSeekPosD.Add(m_lReadPos);
                this.vScrollBar.Value = 0;
                this.vScrollBar.Maximum = this.richTextBox.GetLineFromCharIndex(this.richTextBox.Text.Length-1);
                this.richTextBox.SelectionStart = 0;
                this.richTextBox.SelectionLength = 0;
                this.richTextBox.ScrollToCaret();
                LockWindowUpdate(IntPtr.Zero);
            }
            if ( (e.KeyCode == Keys.End) && ((e.Modifiers & Keys.Control) == Keys.Control) ) {
                LockWindowUpdate(this.richTextBox.Handle);
                m_lSeekPosD.Clear();
                this.richTextBox.Text = ReadBinaryLines(m_sFileName, -1, chrPerLine, m_iFileLength - 150000, 0, out m_lReadPos).ToString();
                m_lSeekPosU = m_iFileLength - 150000;
                this.vScrollBar.Maximum = this.richTextBox.GetLineFromCharIndex(this.richTextBox.Text.Length-1);
                this.vScrollBar.Value = this.vScrollBar.Maximum;
                this.richTextBox.SelectionStart = this.richTextBox.Text.Length-1; // this.richTextBox.GetFirstCharIndexFromLine(this.richTextBox.Lines.Length-1);
                this.richTextBox.SelectionLength = 0;
                this.richTextBox.ScrollToCaret();
                LockWindowUpdate(IntPtr.Zero);
            }
        }

        private void richTextBox_KeyUp( object sender, KeyEventArgs e )
        {
            UpdateCaretPos();
        }
        private void richTextBox_MouseDown( object sender, MouseEventArgs e )
        {
            UpdateCaretPos();
        }
        private void wordWrapToolStripMenuItem_Click( object sender, EventArgs e )
        {
            this.richTextBox.WordWrap = this.wordWrapToolStripMenuItem.Checked;
        }
        public void ToggleFind()
        {
            this.findToolStripMenuItem.Checked = !this.findToolStripMenuItem.Checked;
            findToolStripMenuItem_Click(null, null);
        }
        private void findToolStripMenuItem_Click( object sender, EventArgs e )
        {
            if ( this.findToolStripMenuItem.Checked ) {
                // show find controls
                this.tableLayoutPanel1.SetRowSpan(this.richTextBox, 1);
                this.textBoxFind.Visible = true;
                this.textBoxFind.Focus();
                this.buttonDown.Visible = true;
                this.buttonUp.Visible = true;
                this.buttonX.Visible = true;
            } else {
                // hide find controls normally
                this.textBoxFind.Visible = false;
                this.buttonDown.Visible = false;
                this.buttonUp.Visible = false;
                this.buttonX.Visible = false;
                this.tableLayoutPanel1.SetRowSpan(this.richTextBox, 2);
            }
        }
        // search find-text in output window up 
        private void buttonUp_Click( object sender, EventArgs e )
        {
            StringComparison sc = StringComparison.OrdinalIgnoreCase;
            int ndx = this.richTextBox.Text.LastIndexOf(this.textBoxFind.Text, Math.Max(0, this.m_iLastFoundPos - this.textBoxFind.Text.Length), sc);
            if ( ndx != -1 ) {
                ColorizeFindString(ndx);
                this.m_iLastFoundPos = ndx;
            } else {
                this.m_iLastFoundPos = this.richTextBox.Text.Length;
            }
        }
        // search find-text in output window down
        private void buttonDown_Click( object sender, EventArgs e )
        {
            StringComparison sc = StringComparison.OrdinalIgnoreCase;
            int ndx = this.richTextBox.Text.IndexOf(this.textBoxFind.Text, this.m_iLastFoundPos + this.textBoxFind.Text.Length, sc);
            if ( ndx != -1 ) {
                ColorizeFindString(ndx);
                this.m_iLastFoundPos = ndx;
            } else {
                this.m_iLastFoundPos = 0;
            }
        }
        // colorize the find string ONCE in output window 
        public void ColorizeFindString( int ndx )
        {
            this.richTextBox.SelectionBackColor = Color.Yellow;
            this.richTextBox.SelectionColor = Color.Red;
            int startLine = this.richTextBox.GetLineFromCharIndex(ndx);
            int topIndex = this.richTextBox.GetCharIndexFromPosition(new System.Drawing.Point(1, 1));
            int topLine = this.richTextBox.GetLineFromCharIndex(topIndex);
            int bottomIndex = this.richTextBox.GetCharIndexFromPosition(new System.Drawing.Point(1, this.richTextBox.Height - 1));
            int bottomLine = this.richTextBox.GetLineFromCharIndex(bottomIndex);
            int numVisibleLines = bottomLine - topLine + 1;
            if ( (startLine > bottomLine) || (startLine < topLine) ) {
                int cix = this.richTextBox.GetFirstCharIndexFromLine(Math.Max(0, startLine - numVisibleLines/3 +1));
                this.richTextBox.Select(cix, 0);
                this.richTextBox.ScrollToCaret();
            }
            this.richTextBox.Select(ndx, this.textBoxFind.Text.Length);
            this.richTextBox.SelectionColor = Color.Yellow;
            this.richTextBox.SelectionBackColor = Color.Blue;
        }
        // colorize the find string EVERYWEHRE in output window 
        void ColorizeAllFindText()
        {
            if ( this.textBoxFind.Text.Length == 0 ) {
                return;
            }
            StringComparison sc = StringComparison.OrdinalIgnoreCase;
            int start = 0;
            do {
                Application.DoEvents();
                int ndx = this.richTextBox.Text.IndexOf(this.textBoxFind.Text, Math.Min(this.richTextBox.Text.Length, start), sc);
                if ( ndx != -1 ) {
                    this.richTextBox.Select(ndx, this.textBoxFind.Text.Length);
                    this.richTextBox.SelectionBackColor = Color.Yellow;
                    this.richTextBox.SelectionColor = Color.Red;
                    start = ndx + this.textBoxFind.Text.Length;
                } else {
                    break;
                }
            } while ( true );
        }
        private void textBoxFind_KeyDown( object sender, KeyEventArgs e )
        {
            if ( e.KeyCode == Keys.Enter ) {
                ColorizeAllFindText();
            }
        }
        private void colorizeFindTextToolStripMenuItem_Click( object sender, EventArgs e )
        {
            ColorizeAllFindText();
        }
        private void textBoxFind_TextChanged( object sender, EventArgs e )
        {
            StringComparison sc = StringComparison.OrdinalIgnoreCase;
            int ndx = this.richTextBox.Text.IndexOf(this.textBoxFind.Text, 0, sc);
            if ( ndx == -1 ) {
                this.textBoxFind.BackColor = Color.MistyRose;
                this.colorizeFindTextToolStripMenuItem.Enabled = false;
            } else {
                this.textBoxFind.BackColor = SystemColors.Window;
                this.colorizeFindTextToolStripMenuItem.Enabled = true;
            }
        }
        private void colorizeFindTextToolStripMenuItem_Click_1( object sender, EventArgs e )
        {
            ColorizeAllFindText();
        }
        // remove all color from output window
        private void resetColorsToolStripMenuItem_Click( object sender, EventArgs e )
        {
            this.richTextBox.Select(0, this.richTextBox.Text.Length);
            this.richTextBox.SelectionColor = SystemColors.WindowText;
            this.richTextBox.SelectionBackColor = SystemColors.Control;
            this.richTextBox.Select(0, 0);
        }
        // close input controls to search in the output window
        private void buttonX_Click( object sender, EventArgs e )
        {
            this.findToolStripMenuItem.Checked = false;
            findToolStripMenuItem_Click(null, null);
        }
        private void FileViewer_KeyDown( object sender, KeyEventArgs e )
        {
            if ( (e.Modifiers == Keys.Control) && (e.KeyCode == Keys.F) ) {
                this.findToolStripMenuItem.Checked = !this.findToolStripMenuItem.Checked;
                findToolStripMenuItem_Click(null, null);
            }
        }
        // copy selection to clipboard
        private void copyToolStripMenuItem_Click( object sender, EventArgs e )
        {
            this.richTextBox.Copy();
        }

        public static bool IsTextFile( out Encoding encoding, string fileName, int windowSize )
        {
            FileStream fileStream = null;
            try {
                //                fileStream = File.OpenRead(fileName);
                fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            } catch ( Exception ) {
                encoding = Encoding.Default;
                return false;
            }

            var rawData = new byte[windowSize];
            var text = new char[windowSize];
            var isText = true;

            // Read raw bytes
            var rawLength = fileStream.Read(rawData, 0, rawData.Length);
            fileStream.Seek(0, SeekOrigin.Begin);

            // Detect encoding correctly (from Rick Strahl's blog)
            // http://www.west-wind.com/weblog/posts/2007/Nov/28/Detecting-Text-Encoding-for-StreamReader
            if ( rawData[0] == 0xef && rawData[1] == 0xbb && rawData[2] == 0xbf ) {
                encoding = Encoding.UTF8;
            } else if ( rawData[0] == 0xff && rawData[1] == 0xfe ) {
                encoding = Encoding.Unicode;
            } else if ( rawData[0] == 0 && rawData[1] == 0 && rawData[2] == 0xfe && rawData[3] == 0xff ) {
                encoding = Encoding.UTF32;
            } else if ( rawData[0] == 0x2b && rawData[1] == 0x2f && rawData[2] == 0x76 ) {
                encoding = Encoding.UTF7;
            } else if ( rawData[0] == 0xfe && rawData[1] == 0xff ) {
                encoding = Encoding.BigEndianUnicode; // utf-16be
            } else {
                encoding = Encoding.Default;
            }

            // Read text and detect the encoding
            using ( var streamReader = new StreamReader(fileStream) ) {
                streamReader.Read(text, 0, text.Length);
            }

            // if a potential string contains more than 5x '\0', it's rather unlike being text
            if ( encoding == Encoding.Default ) {
                int count = text.Count(f => f == '\0');
                if ( count > 5 ) {
                    return false;
                }
            }

            using ( var memoryStream = new MemoryStream() ) {
                using ( var streamWriter = new StreamWriter(memoryStream, encoding) ) {
                    // Write the text to a buffer
                    streamWriter.Write(text);
                    streamWriter.Flush();

                    // Get the buffer from the memory stream for comparision
                    var memoryBuffer = memoryStream.GetBuffer();

                    // Compare only bytes read
                    for ( var i = 0; i < rawLength && isText; i++ ) {
                        isText = rawData[i] == memoryBuffer[i];
                    }
                }
            }
            return isText;
        }

        public class RTFScrolledBottom: RichTextBox
        {
            public class WheelEventArgs: EventArgs
            {
                public WheelEventArgs( string wheelevent )
                {
                    WheelEvent = wheelevent;
                }
                public string WheelEvent { get; set; }
            }

            public event EventHandler ScrolledToBottom;
            public event EventHandler ScrolledToTop;
            public event EventHandler<WheelEventArgs> Scrolled;

            private const int WM_VSCROLL = 0x115;
            private const int WM_MOUSEWHEEL = 0x20A;
            private const int WM_USER = 0x400;
            private const int SB_VERT = 1;
            private const int EM_SETSCROLLPOS = WM_USER + 222;
            private const int EM_GETSCROLLPOS = WM_USER + 221;

            [DllImport("user32.dll")]
            private static extern bool GetScrollRange( IntPtr hWnd, int nBar, out int lpMinPos, out int lpMaxPos );

            [DllImport("user32.dll")]
            private static extern IntPtr SendMessage( IntPtr hWnd, Int32 wMsg, Int32 wParam, ref Point lParam );

            // 32 bit scrolling of pane slider
            // http://stackoverflow.com/questions/1380104/cc-setscrollpos-user32-dll
            [DllImport("user32.dll")]
            static extern int SetScrollPos( IntPtr hWnd, int nBar, int nPos, bool bRedraw );
            [DllImport("User32.dll")]
            private extern static int GetScrollPos( IntPtr hWnd, int nBar );
            private enum ScrollBarType: uint { SbHorz = 0, SbVert = 1, SbCtl = 2, SbBoth = 3 }

            public void SetVerticalScrollPos( int pos )
            {
                SetScrollPos(this.Handle, 0x1, pos, true);
            }
            public int GetVerticalScrollPos()
            {
                int n = GetScrollPos(this.Handle, (int)ScrollBarType.SbVert);
                if ( n > this.ClientSize.Height ) {
                    n += this.ClientSize.Height;
                }

                return n;
            }
            public bool IsAtMaxScroll()
            {
                int minScroll;
                int maxScroll;
                GetScrollRange(this.Handle, SB_VERT, out minScroll, out maxScroll);
                Point rtfPoint = Point.Empty;
                SendMessage(this.Handle, EM_GETSCROLLPOS, 0, ref rtfPoint);

                return (rtfPoint.Y + this.ClientSize.Height >= maxScroll);
            }
            public bool IsAtMinScroll()
            {
                int minScroll;
                int maxScroll;
                GetScrollRange(this.Handle, SB_VERT, out minScroll, out maxScroll);
                Point rtfPoint = Point.Empty;
                SendMessage(this.Handle, EM_GETSCROLLPOS, 0, ref rtfPoint);

                return (rtfPoint.Y <= minScroll);
            }

            public int GetMaxScrollPosV()
            {
                int minScroll;
                int maxScroll;
                GetScrollRange(this.Handle, SB_VERT, out minScroll, out maxScroll);
                return maxScroll;
            }
            protected virtual void OnScrolled( WheelEventArgs e )
            {
                if ( Scrolled != null ) {
                    Scrolled(this, e);
                }
            }
            protected virtual void OnScrolledToBottom( EventArgs e )
            {
                if ( ScrolledToBottom != null )
                    ScrolledToBottom(this, e);
            }
            protected virtual void OnScrolledToTop( EventArgs e )
            {
                if ( ScrolledToTop != null )
                    ScrolledToTop(this, e);
            }
            protected override void OnKeyUp( KeyEventArgs e )
            {
/*
                if ( IsAtMaxScroll() )
                    OnScrolledToBottom(EventArgs.Empty);
                if ( IsAtMinScroll() )
                    OnScrolledToTop(EventArgs.Empty);
*/
                base.OnKeyUp(e);
            }
            protected override void WndProc( ref Message m )
            {
                if ( m.Msg == WM_MOUSEWHEEL ) {
                    if ( (long)m.WParam < 0 ) {
                        // scroll down
                        OnScrolled(new WheelEventArgs("WheelDown"));
                    } else {
                        // scroll up
                        OnScrolled(new WheelEventArgs("WheelUp"));
                    }
                }

                base.WndProc(ref m);
            }
        }

        private void richTextBox_SizeChanged( object sender, EventArgs e )
        {
            this.vScrollBar.LargeChange = (richTextBox.ClientSize.Height / richTextBox.Font.Height) - 1;
        }
    }

    public class VerticalProgressBar: ProgressBar
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= 0x04;
                return cp;
            }
        }
    }

    //Must add using System.Runtime.InteropServices;
    //We can define some extension method for this purpose
    public static class RichTextBoxExtension
    {
        [DllImport("user32")]
        private static extern int GetScrollInfo( IntPtr hwnd, int nBar, ref SCROLLINFO scrollInfo );
        public struct SCROLLINFO
        {
            public int cbSize;
            public int fMask;
            public int min;
            public int max;
            public int nPage;
            public int nPos;
            public int nTrackPos;
        }
        public static SCROLLINFO GetScrollInfoV( this RichTextBox rtb )
        {
            SCROLLINFO scrollInfo = new SCROLLINFO();
            scrollInfo.cbSize = Marshal.SizeOf(scrollInfo);
            //SIF_RANGE = 0x1, SIF_TRACKPOS = 0x10,  SIF_PAGE= 0x2
            scrollInfo.fMask = 0x10 | 0x1 | 0x2;
            GetScrollInfo(rtb.Handle, 1, ref scrollInfo); //nBar = 1 -> VScrollbar
            return scrollInfo;
        }
        public static bool ReachedBottom( this RichTextBox rtb )
        {
            SCROLLINFO scrollInfo = new SCROLLINFO();
            scrollInfo.cbSize = Marshal.SizeOf(scrollInfo);
            //SIF_RANGE = 0x1, SIF_TRACKPOS = 0x10,  SIF_PAGE= 0x2
            scrollInfo.fMask = 0x10 | 0x1 | 0x2;
            GetScrollInfo(rtb.Handle, 1, ref scrollInfo); //nBar = 1 -> VScrollbar
            return scrollInfo.max == scrollInfo.nTrackPos + scrollInfo.nPage;
        }
    }
}
