namespace cfw
{
    partial class FileViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileViewer));
            this.fileView1 = new FileViewCtl.FileView();
            this.SuspendLayout();
            // 
            // fileView1
            // 
            resources.ApplyResources(this.fileView1, "fileView1");
            this.fileView1.Name = "fileView1";
            // 
            // FileViewer
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.fileView1);
            this.Name = "FileViewer";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FileViewer_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private FileViewCtl.FileView fileView1;

    }
}