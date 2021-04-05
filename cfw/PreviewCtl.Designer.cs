namespace cfw
{
    partial class PreviewCtl
    {
        private PdfiumViewer.PdfViewer pdfViewer;
        private FileViewCtl.FileView fileView;
        private ZipView zipView;
        private System.Windows.Forms.WebBrowser webBrowser;
        private DocView docView;
        private ImgView imgView;
        private VideoPlayer.VideoPlayerCtl videoPlayerCtl;

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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreviewCtl));
            this.pdfViewer = new PdfiumViewer.PdfViewer();
            this.webBrowser = new System.Windows.Forms.WebBrowser();
            this.axWMP = new AxWMPLib.AxWindowsMediaPlayer();
            this.videoPlayerCtl = new VideoPlayer.VideoPlayerCtl();
            this.imgView = new cfw.ImgView();
            this.docView = new cfw.DocView();
            this.zipView = new cfw.ZipView();
            this.fileView = new FileViewCtl.FileView();
            ((System.ComponentModel.ISupportInitialize)(this.axWMP)).BeginInit();
            this.SuspendLayout();
            // 
            // pdfViewer
            // 
            this.pdfViewer.Location = new System.Drawing.Point(3, 3);
            this.pdfViewer.Name = "pdfViewer";
            this.pdfViewer.Size = new System.Drawing.Size(122, 27);
            this.pdfViewer.TabIndex = 0;
            this.pdfViewer.Visible = false;
            // 
            // webBrowser
            // 
            this.webBrowser.Location = new System.Drawing.Point(5, 71);
            this.webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser.Name = "webBrowser";
            this.webBrowser.Size = new System.Drawing.Size(120, 20);
            this.webBrowser.TabIndex = 5;
            this.webBrowser.Visible = false;
            // 
            // axWMP
            // 
            this.axWMP.Enabled = true;
            this.axWMP.Location = new System.Drawing.Point(131, 101);
            this.axWMP.Name = "axWMP";
            this.axWMP.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axWMP.OcxState")));
            this.axWMP.Size = new System.Drawing.Size(116, 34);
            this.axWMP.TabIndex = 10;
            this.axWMP.Visible = false;
            // 
            // videoPlayerCtl
            // 
            this.videoPlayerCtl.Location = new System.Drawing.Point(3, 101);
            this.videoPlayerCtl.MinimumSize = new System.Drawing.Size(100, 20);
            this.videoPlayerCtl.Name = "videoPlayerCtl";
            this.videoPlayerCtl.Size = new System.Drawing.Size(116, 57);
            this.videoPlayerCtl.TabIndex = 9;
            this.videoPlayerCtl.URL = "";
            this.videoPlayerCtl.Visible = false;
            // 
            // imgView
            // 
            this.imgView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.imgView.Location = new System.Drawing.Point(131, 3);
            this.imgView.Margin = new System.Windows.Forms.Padding(0);
            this.imgView.Name = "imgView";
            this.imgView.Size = new System.Drawing.Size(116, 32);
            this.imgView.TabIndex = 7;
            this.imgView.Visible = false;
            // 
            // docView
            // 
            this.docView.Location = new System.Drawing.Point(131, 70);
            this.docView.Name = "docView";
            this.docView.Size = new System.Drawing.Size(116, 21);
            this.docView.TabIndex = 6;
            this.docView.Visible = false;
            // 
            // zipView
            // 
            this.zipView.Location = new System.Drawing.Point(131, 36);
            this.zipView.Name = "zipView";
            this.zipView.Size = new System.Drawing.Size(116, 27);
            this.zipView.TabIndex = 3;
            this.zipView.Visible = false;
            // 
            // fileView
            // 
            this.fileView.Location = new System.Drawing.Point(3, 36);
            this.fileView.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.fileView.Name = "fileView";
            this.fileView.Size = new System.Drawing.Size(122, 29);
            this.fileView.TabIndex = 2;
            this.fileView.Visible = false;
            // 
            // PreviewCtl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.axWMP);
            this.Controls.Add(this.videoPlayerCtl);
            this.Controls.Add(this.imgView);
            this.Controls.Add(this.docView);
            this.Controls.Add(this.webBrowser);
            this.Controls.Add(this.zipView);
            this.Controls.Add(this.fileView);
            this.Controls.Add(this.pdfViewer);
            this.Name = "PreviewCtl";
            this.Size = new System.Drawing.Size(256, 161);
            ((System.ComponentModel.ISupportInitialize)(this.axWMP)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AxWMPLib.AxWindowsMediaPlayer axWMP;
    }
}
