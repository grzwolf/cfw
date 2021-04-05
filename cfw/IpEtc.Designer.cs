namespace cfw
{
    partial class IpEtc
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
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyIPAddressToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copySubnetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyGatewayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyMACAddressToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.button1 = new System.Windows.Forms.Button();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader2,
            this.columnHeader3});
            this.listView1.ContextMenuStrip = this.contextMenuStrip1;
            this.listView1.Dock = System.Windows.Forms.DockStyle.Top;
            this.listView1.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listView1.FullRowSelect = true;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(794, 171);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Adapter";
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "IP-Address";
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Subnet";
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Gateway";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "MAC-Address";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Service";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyIPAddressToolStripMenuItem,
            this.copySubnetToolStripMenuItem,
            this.copyGatewayToolStripMenuItem,
            this.copyMACAddressToolStripMenuItem,
            this.copyRowToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(180, 114);
            // 
            // copyIPAddressToolStripMenuItem
            // 
            this.copyIPAddressToolStripMenuItem.Name = "copyIPAddressToolStripMenuItem";
            this.copyIPAddressToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.copyIPAddressToolStripMenuItem.Text = "Copy IP-Address";
            this.copyIPAddressToolStripMenuItem.Click += new System.EventHandler(this.copyIPAddressToolStripMenuItem_Click);
            // 
            // copySubnetToolStripMenuItem
            // 
            this.copySubnetToolStripMenuItem.Name = "copySubnetToolStripMenuItem";
            this.copySubnetToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.copySubnetToolStripMenuItem.Text = "Copy Subnet";
            this.copySubnetToolStripMenuItem.Click += new System.EventHandler(this.copySubnetToolStripMenuItem_Click);
            // 
            // copyGatewayToolStripMenuItem
            // 
            this.copyGatewayToolStripMenuItem.Name = "copyGatewayToolStripMenuItem";
            this.copyGatewayToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.copyGatewayToolStripMenuItem.Text = "Copy Gateway";
            this.copyGatewayToolStripMenuItem.Click += new System.EventHandler(this.copyGatewayToolStripMenuItem_Click);
            // 
            // copyMACAddressToolStripMenuItem
            // 
            this.copyMACAddressToolStripMenuItem.Name = "copyMACAddressToolStripMenuItem";
            this.copyMACAddressToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.copyMACAddressToolStripMenuItem.Text = "Copy MAC-Address";
            this.copyMACAddressToolStripMenuItem.Click += new System.EventHandler(this.copyMACAddressToolStripMenuItem_Click);
            // 
            // copyRowToolStripMenuItem
            // 
            this.copyRowToolStripMenuItem.Name = "copyRowToolStripMenuItem";
            this.copyRowToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.copyRowToolStripMenuItem.Text = "Copy Full Row";
            this.copyRowToolStripMenuItem.Click += new System.EventHandler(this.copyRowToolStripMenuItem_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(360, 200);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 32);
            this.button1.TabIndex = 2;
            this.button1.Text = "Close";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // IpEtc
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(794, 262);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.listView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(810, 296);
            this.Name = "IpEtc";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "My IP";
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem copyIPAddressToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copySubnetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyGatewayToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyRowToolStripMenuItem;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ToolStripMenuItem copyMACAddressToolStripMenuItem;
    }
}