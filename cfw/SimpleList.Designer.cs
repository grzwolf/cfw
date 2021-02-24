namespace cfw
{
    partial class SimpleList
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if ( disposing && (components != null) ) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.listBox = new System.Windows.Forms.ListBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.escToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeItemFromListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyLinkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBox
            // 
            this.listBox.BackColor = System.Drawing.SystemColors.Window;
            this.listBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox.FormattingEnabled = true;
            this.listBox.Location = new System.Drawing.Point(0, 0);
            this.listBox.Name = "listBox";
            this.listBox.Size = new System.Drawing.Size(284, 262);
            this.listBox.TabIndex = 0;
            this.listBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listBox_MouseClick);
            this.listBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBox_KeyDown);
            this.listBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listBox_MouseDown);
            this.listBox.MouseLeave += new System.EventHandler(this.listBox_MouseLeave);
            this.listBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.listBox_MouseMove);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.escToolStripMenuItem,
            this.copyLinkToolStripMenuItem,
            this.toolStripSeparator1,
            this.removeItemFromListToolStripMenuItem,
            this.clearListToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(192, 120);
            // 
            // escToolStripMenuItem
            // 
            this.escToolStripMenuItem.Name = "escToolStripMenuItem";
            this.escToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.escToolStripMenuItem.Text = "Esc";
            this.escToolStripMenuItem.Click += new System.EventHandler(this.escToolStripMenuItem_Click);
            // 
            // removeItemFromListToolStripMenuItem
            // 
            this.removeItemFromListToolStripMenuItem.Name = "removeItemFromListToolStripMenuItem";
            this.removeItemFromListToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.removeItemFromListToolStripMenuItem.Text = "Remove item from list";
            this.removeItemFromListToolStripMenuItem.Click += new System.EventHandler(this.removeItemFromListToolStripMenuItem_Click);
            // 
            // copyLinkToolStripMenuItem
            // 
            this.copyLinkToolStripMenuItem.Name = "copyLinkToolStripMenuItem";
            this.copyLinkToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.copyLinkToolStripMenuItem.Text = "Copy link";
            this.copyLinkToolStripMenuItem.Click += new System.EventHandler(this.copyLinkToolStripMenuItem_Click);
            // 
            // clearListToolStripMenuItem
            // 
            this.clearListToolStripMenuItem.Name = "clearListToolStripMenuItem";
            this.clearListToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.clearListToolStripMenuItem.Text = "Clear list";
            this.clearListToolStripMenuItem.Click += new System.EventHandler(this.clearListToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(188, 6);
            // 
            // SimpleList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.ControlBox = false;
            this.Controls.Add(this.listBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Name = "SimpleList";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SimpleList_KeyDown);
            this.MouseLeave += new System.EventHandler(this.SimpleList_MouseLeave);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBox;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem removeItemFromListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem escToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyLinkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearListToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    }
}