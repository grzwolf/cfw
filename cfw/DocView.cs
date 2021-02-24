using Microsoft.Office.Interop.Word;
using System;
using System.IO;
using System.Windows.Forms;

namespace cfw {
    public partial class DocView : UserControl {
        private readonly System.Windows.Forms.WebBrowser webBrowser1;

        delegate void ConvertDocumentDelegate(string fileName);

        public DocView() {
            this.InitializeComponent();

            // Create the webBrowser control on the UserControl. 
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();

            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(532, 514);
            this.webBrowser1.TabIndex = 0;

            this.Controls.Add(this.webBrowser1);

            // set up an event handler to delete our temp file when we're done with it. 
            this.webBrowser1.DocumentCompleted += this.webBrowser1_DocumentCompleted;
        }

        string tempFileName = null;

        public void Clear() {
            this.webBrowser1.Url = new Uri("about:blank");
        }

        public void LoadDocument(string fileName) {
            // Call ConvertDocument asynchronously. 
            ConvertDocumentDelegate del = new ConvertDocumentDelegate(this.ConvertDocument);

            // Call DocumentConversionComplete when the method has completed. 
            del.BeginInvoke(fileName, this.DocumentConversionComplete, null);
        }

        void ConvertDocument(string fileName) {
            object m = System.Reflection.Missing.Value;
            object oldFileName = fileName;
            object readOnly = false;
            ApplicationClass ac = null;

            if ( !File.Exists((string)@oldFileName) ) {
                return;
            }

            try {
                // First, create a new Microsoft.Office.Interop.Word.ApplicationClass.
                ac = new ApplicationClass();

                // Now we open the document.
                Document doc = ac.Documents.Open(ref oldFileName,
                                                  ref m,
                                                  ref readOnly,
                                                  ref m,
                                                  ref m,
                                                  ref m,
                                                  ref m,
                                                  ref m,
                                                  ref m,
                                                  ref m,
                                                  ref m,
                                                  ref m,
                                                  ref m,
                                                  ref m,
                                                  ref m);

                // Create a temp file to save the HTML file to. 
                this.tempFileName = this.GetTempFile("html");

                // Cast these items to object.  The methods we're calling 
                // only take object types in their method parameters. 
                object newFileName = this.tempFileName;

                // We will be saving this file as HTML format. 
                object fileType = WdSaveFormat.wdFormatHTML;

                // Save the file. 
                doc.SaveAs(ref newFileName, ref fileType,
                            ref m, ref m, ref m, ref m, ref m, ref m, ref m,
                            ref m, ref m, ref m, ref m, ref m, ref m, ref m);
            } catch ( System.Runtime.InteropServices.COMException ) {; } finally {
                // Make sure we close the application class. 
                if ( ac != null )
                    ac.Quit(ref readOnly, ref m, ref m);
            }
        }

        void DocumentConversionComplete(IAsyncResult result) {
            // navigate to our temp file. 
            this.webBrowser1.Navigate(this.tempFileName);
        }

        readonly string _randomname = Guid.NewGuid().ToString().Substring(0, 8);
        void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e) {
            if ( this.tempFileName != null ) {
                if ( this.tempFileName != string.Empty ) {
                    // delete the temp file we created. 
                    try {
                        //File.Delete(tempFileName);
                        string path = System.Windows.Forms.Application.StartupPath;
                        path = Path.Combine(path, this._randomname);
                        Directory.Delete(path, true);
                    } catch ( Exception ) {; }

                    // set the tempFileName to an empty string. 
                    this.tempFileName = string.Empty;
                }
            }
        }
        string GetTempFile(string extension) {
            // Uses the Combine, GetTempPath, ChangeExtension, 
            // and GetRandomFile methods of Path to 
            // create a temp file of the extension we're looking for. 
            string path = System.Windows.Forms.Application.StartupPath;
            path = Path.Combine(path, this._randomname);
            Directory.CreateDirectory(path);
            string file = Path.GetRandomFileName();
            string fext = Path.ChangeExtension(file, extension);
            string full = Path.Combine(path, fext);
            return full;
        }
    }
}