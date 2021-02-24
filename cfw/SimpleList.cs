using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace cfw {
    public partial class SimpleList : Form {
        public int GetListItemHeight {
            get {
                return this.listBox.Font.Height + 2;
            }
        }
        public string ReturnFolder { get; set; }
        public int ReturnIndex { get; set; }
        public List<string> GetStringList {
            get {
                this.listBox.Items.RemoveAt(0);
                return this.listBox.Items.Cast<String>().ToList();
            }
        }

        public SimpleList(List<string> list, int iSelectedIndex) {
            this.InitializeComponent();
            this.listBox.Items.AddRange(list.ToArray());
            if ( this.listBox.Items.Count > 1 ) {
                this.listBox.SelectedIndex = iSelectedIndex + 1;
            } else {
                this.listBox.SelectedIndex = 0;
            }
            this.AutoFit();
            this.ReturnFolder = "";
            this.ReturnIndex = -1;
        }

        // select item & close dialog
        private void listBox_MouseClick(object sender, MouseEventArgs e) {
            this.ReturnFolder = this.listBox.SelectedItem.ToString();
            this.ReturnIndex = this.listBox.SelectedIndex;
            this.Close();
        }

        // autofit dialog and its listbox 
        private void AutoFit() {
            if ( this.listBox.Items.Count == 0 ) {
                return;
            }
            int width = 0;
            using ( Graphics g = this.listBox.CreateGraphics() ) {
                for ( int i = 0; i < this.listBox.Items.Count; i++ ) {
                    int itemWidth = Convert.ToInt32(g.MeasureString(Convert.ToString(this.listBox.Items[i]) + ".", this.listBox.Font).Width);  // w/o the ".", itemWidth is a few pixels too small 
                    width = Math.Max(width, itemWidth);
                }
            }
            this.Width = width;
            int spacing = this.listBox.Font.FontFamily.GetLineSpacing(FontStyle.Regular);
            float lineSpacingPixel = this.listBox.Font.GetHeight() * spacing / this.listBox.Font.FontFamily.GetEmHeight(FontStyle.Regular);
            this.Height = this.listBox.Items.Count * (int)Math.Round(lineSpacingPixel) + 2; // funny: having 3 items in the list, I need to add 2 to .Height - otherwise a vertical scrollbar appears
        }

        // mouse hovering shall highlight the underlying listbox item, regardless what item is actually selected 
        private int m_iIndex = 0;
        private void listBox_MouseMove(object sender, MouseEventArgs e) {
            // check if we have a valid item
            Point point = this.listBox.PointToClient(Cursor.Position);
            int index = this.listBox.IndexFromPoint(point);
            if ( index < 0 ) {
                return;
            }

            // avoid flickering
            if ( index == this.m_iIndex ) {
                return;
            }
            this.m_iIndex = index;

            // highlight current item
            using ( Graphics g = this.listBox.CreateGraphics() ) {
                // remove all highlighting
                for ( int i = 0; i < this.listBox.Items.Count; i++ ) {
                    Rectangle ira = this.listBox.GetItemRectangle(i);
                    ira.Width -= 1;
                    g.DrawRectangle(new Pen(Color.White, 1), ira);
                }
                // highlight the previously found valid item
                Rectangle ra = this.listBox.GetItemRectangle(index);
                ra.Width -= 1;
                g.DrawRectangle(new Pen(Color.Black, 1), ra);
            }
        }

        // keyboard navigation
        private void SimpleList_KeyDown(object sender, KeyEventArgs e) {
            if ( e.KeyCode == Keys.Escape ) {
                this.ReturnFolder = "";
                this.ReturnIndex = -1;
                this.Close();
            }
            if ( e.KeyCode == Keys.Enter ) {
                this.ReturnFolder = this.listBox.SelectedItem.ToString();
                this.ReturnIndex = this.listBox.SelectedIndex;
                this.Close();
            }
        }

        // whenever mouse leaves the playground, it's treated as Cancel/Esc 
        private void SimpleList_MouseLeave(object sender, EventArgs e) {
            if ( this.ReturnFolder != null ) {
                if ( this.ReturnFolder.Length == 0 ) {
                    this.ReturnFolder = "";
                    this.ReturnIndex = -1;
                    this.Close();
                }
            }
        }
        private void listBox_MouseLeave(object sender, EventArgs e) {
            if ( (this.ReturnFolder != null) && (!this.contextMenuStrip1.Visible) ) {
                if ( this.ReturnFolder.Length == 0 ) {
                    this.ReturnFolder = "";
                    this.ReturnIndex = -1;
                    this.Close();
                }
            }
        }

        private void listBox_MouseDown(object sender, MouseEventArgs e) {
            if ( e.Button == MouseButtons.Right ) {

                // check if we have a valid item
                Point point = this.listBox.PointToClient(Cursor.Position);
                int index = this.listBox.IndexFromPoint(point);
                if ( index < 0 ) {
                    return;
                }
                // select item
                this.listBox.SelectedIndex = index;
                // show context menu
                Point showPos = Cursor.Position;
                showPos.Offset(5, 0);
                this.contextMenuStrip1.Show(showPos);
            }
        }

        private void escToolStripMenuItem_Click(object sender, EventArgs e) {
            this.contextMenuStrip1.Close();
        }

        private void DeleteSelectedItemFromList() {
            if ( this.listBox.SelectedIndex != 0 ) {
                int oldNdx = this.listBox.SelectedIndex;
                this.listBox.Items.RemoveAt(oldNdx);
                this.listBox.SelectedIndex = Math.Max(0, Math.Min(this.listBox.Items.Count - 1, oldNdx));
            }
        }
        private void removeItemFromListToolStripMenuItem_Click(object sender, EventArgs e) {
            this.DeleteSelectedItemFromList();
        }
        private void listBox_KeyDown(object sender, KeyEventArgs e) {
            if ( e.KeyCode == Keys.Delete ) {
                this.DeleteSelectedItemFromList();
            }
        }

        private void copyLinkToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.listBox.SelectedIndex != 0 ) {
                Clipboard.Clear();
                Clipboard.SetText(this.listBox.SelectedItem.ToString());
            }
        }

        private void clearListToolStripMenuItem_Click(object sender, EventArgs e) {
            this.listBox.Items.Clear();
            this.listBox.Items.Insert(0, "<return>");
            this.listBox.SelectedIndex = 0;
        }
    }
}
