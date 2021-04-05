using MsgReader;                              // handle Outlook msg files
using PdfiumViewer;                           // pdf  
using System;
using System.IO;                              // Path
using System.Runtime.ExceptionServices;
using System.Windows.Forms;

// !!! If the MSO stuff is not present on a PC, opening preview throws exceptions. Therefore I wrap all MSO calls with try/catch only there, where I really need it.
//using Microsoft.Office.Core;                  // msoTriStat - add ref via COM Tab: Microsoft Office 15.0 Object Library 
//using Microsoft.Office.Interop.Excel;         // add reference + browse: C:\Windows\assembly\GAC_MSIL\Microsoft.Office.Interop.Excel\15.0.0.0__71e9bce111e9429c
//using Microsoft.Office.Interop.PowerPoint;    // add reference + browse: 15
//using Visio = Microsoft.Office.Interop.Visio; // add reference + browse: old visio 
//using Microsoft.Office.Interop.MSProject;


namespace cfw {
    public partial class PreviewCtl : UserControl {
        bool b_img = false;
        bool b_doc = false;
        bool b_pdf = false;
        bool b_htm = false;
        bool b_zip = false;
        bool b_asis = false;
        bool b_cfw = false;
        bool b_wmp = false;
        bool b_mp3 = false;

        public PreviewCtl() {
            this.videoPlayerCtl = new VideoPlayer.VideoPlayerCtl();
            this.InitializeComponent();

            this.imgView.Dock = DockStyle.Fill;
            this.pdfViewer.Dock = DockStyle.Fill;
            this.fileView.Dock = DockStyle.Fill;
            this.zipView.Dock = DockStyle.Fill;
            this.webBrowser.Dock = DockStyle.Fill;
            this.docView.Dock = DockStyle.Fill;
            this.videoPlayerCtl.Dock = DockStyle.Fill;
        }

        public void SetPreviewFiles(bool img, bool doc, bool pdf, bool htm, bool zip, bool asis, bool cfw, bool mp3, bool wmp) {
            this.b_img = img;
            this.b_doc = doc;
            this.b_pdf = pdf;
            this.b_htm = htm;
            this.b_zip = zip;
            this.b_asis = asis;
            this.b_cfw = cfw;
            this.b_wmp = wmp;
            this.b_mp3 = mp3;
        }

        public void RePaint() {
            if ( this.imgView.Visible ) {
                this.imgView.RePaint();
            }
        }

        [HandleProcessCorruptedStateExceptions]
        public bool LoadDocument(string filename, string typename, ListView parent) {
            this.Clear();

            if ( !File.Exists(filename) ) {
                return false;
            }

            // all windows media play formats
            if ( typename == "WMP" ) {
                this.pdfViewer.Visible = false;
                this.fileView.Visible = false;
                this.zipView.Visible = false;
                this.imgView.Visible = false;
                this.docView.Visible = false;
                this.webBrowser.Visible = false;
                if ( this.b_cfw ) {
                    this.axWMP.Visible = false;
                    this.videoPlayerCtl.URL = filename;
                    this.videoPlayerCtl.Location = new System.Drawing.Point(this.ClientRectangle.X, this.ClientRectangle.Y);
                    this.videoPlayerCtl.Size = this.Size;
                    this.videoPlayerCtl.Visible = true;
                    return true;
                }
                if ( this.b_wmp ) {
                    this.videoPlayerCtl.Visible = false;
                    this.axWMP.URL = filename;
                    this.axWMP.Visible = true;
                    this.axWMP.Dock = DockStyle.Fill;
                    return true;
                }
            }

            // in case something goes wrong, we allow to fallback to b_asis and show the binary content
            bool asisTmp = this.b_asis;

            // ext drives most of the things
            string ext = System.IO.Path.GetExtension(filename).ToLower();

            // all music
            if ( (".mp3.wav.flac".IndexOf(ext) != -1) && this.b_mp3 ) {
                try {
                    this.pdfViewer.Visible = false;
                    this.fileView.Visible = false;
                    this.zipView.Visible = false;
                    this.imgView.Visible = false;
                    this.webBrowser.Visible = false;
                    this.videoPlayerCtl.Visible = false;
                    this.docView.Visible = false;
                    this.axWMP.Visible = true;
                    this.axWMP.Dock = DockStyle.Fill;
                    this.axWMP.URL = filename;
                    asisTmp = false;
                } catch ( System.Exception ) {
                    asisTmp = true;
                }
            }

            // initially I only found a way to convert Word to html (which is done here), other MSO Files conversations were found later and appear in the HTML section
            if ( (".rtf.doc.docx".IndexOf(ext) != -1) && this.b_doc ) {

                try {
                    this.pdfViewer.Visible = false;
                    this.fileView.Visible = false;
                    this.zipView.Visible = false;
                    this.imgView.Visible = false;
                    this.webBrowser.Visible = false;
                    this.videoPlayerCtl.Visible = false;
                    this.axWMP.Visible = false;
                    this.docView.LoadDocument(filename);
                    this.docView.Visible = true;
                    asisTmp = false;
                } catch ( System.Exception ) {
                    asisTmp = true;
                }

            } else {

                // images
                if ( (".ico.bmp.tif.jpg.jpeg.wmf.gif.png.exif.emf.tiff".IndexOf(ext) != -1) && this.b_img ) {

                    try {
                        this.pdfViewer.Visible = false;
                        this.fileView.Visible = false;
                        this.zipView.Visible = false;
                        this.webBrowser.Visible = false;
                        this.docView.Visible = false;
                        this.videoPlayerCtl.Visible = false;
                        this.axWMP.Visible = false;
                        this.imgView.Visible = true;
                        this.imgView.LoadDocument(filename, parent);
                        asisTmp = false;
                    } catch ( System.Exception ) {
                        asisTmp = true;
                    }

                } else {

                    // PDF and PowerPoint
                    if ( (".pdf.ppt.pptx.pptm.odp".IndexOf(ext) != -1) && (this.b_pdf || this.b_doc) ) {

                        // powerpoint formats via powerpoint interop converted to pdf
                        if ( (".ppt.pptx.pptm.odp".IndexOf(ext) != -1) && this.b_doc ) {
                            string outpath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "temp.pdf");
                            if ( System.IO.File.Exists(outpath) ) {
                                System.IO.File.Delete(outpath);
                            }
                            try {
                                Microsoft.Office.Interop.PowerPoint.Application app = new Microsoft.Office.Interop.PowerPoint.Application();
                                Microsoft.Office.Interop.PowerPoint.Presentations pres = app.Presentations;
                                Microsoft.Office.Interop.PowerPoint.Presentation file = pres.Open(filename, Microsoft.Office.Core.MsoTriState.msoTrue, Microsoft.Office.Core.MsoTriState.msoTrue, Microsoft.Office.Core.MsoTriState.msoFalse);
                                file.SaveCopyAs(outpath, Microsoft.Office.Interop.PowerPoint.PpSaveAsFileType.ppSaveAsPDF, Microsoft.Office.Core.MsoTriState.msoTrue);
                                app.Quit();
                                filename = outpath;
                                asisTmp = false;
                            } catch ( Exception ) {
                                asisTmp = true;
                            }
                        }

                        // show pdf
                        try {
                            this.pdfViewer.ZoomMode = PdfViewerZoomMode.FitWidth;
                            PdfDocument pdoc = PdfDocument.Load(filename);
                            this.pdfViewer.Document = pdoc;
                            this.fileView.Visible = false;
                            this.zipView.Visible = false;
                            this.imgView.Visible = false;
                            this.webBrowser.Visible = false;
                            this.docView.Visible = false;
                            this.videoPlayerCtl.Visible = false;
                            this.axWMP.Visible = false;
                            this.pdfViewer.Visible = true;
                            asisTmp = false;
                        } catch ( Exception ) {
                            asisTmp = true;
                        }

                    } else {

                        // ZIP-Viewer simply shows the content of zip file
                        if ( (".zip".IndexOf(ext) != -1) && this.b_zip ) {
                            try {
                                this.zipView.LoadZip(filename);
                                this.pdfViewer.Visible = false;
                                this.fileView.Visible = false;
                                this.imgView.Visible = false;
                                this.webBrowser.Visible = false;
                                this.docView.Visible = false;
                                this.videoPlayerCtl.Visible = false;
                                this.axWMP.Visible = false;
                                this.zipView.Visible = true;
                                asisTmp = false;
                            } catch ( Exception ) {
                                asisTmp = true;
                            }

                        } else {

                            // collector for everything, which could be converted into HTML
                            if ( (".htm.html.msg.eml.mht.xls.xlsx.xlsb.xlsm.vsd.vsdx.vsdm"/*.mpp.mpx"*/.IndexOf(ext) != -1) && (this.b_htm || this.b_doc) ) {

                                // visio via visio interop to html
                                if ( (".vsd.vsdx.vsdm".IndexOf(ext) != -1) && this.b_doc ) {
                                    string outpath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "temp.html");
                                    if ( System.IO.File.Exists(outpath) ) {
                                        System.IO.File.Delete(outpath);
                                    }

                                    asisTmp = true;
                                    try {
                                        Microsoft.Office.Interop.Visio.IVInvisibleApp visio = null;
                                        visio = new Microsoft.Office.Interop.Visio.InvisibleApp();
                                        Microsoft.Office.Interop.Visio.SaveAsWeb.VisSaveAsWeb saveAsWeb = (Microsoft.Office.Interop.Visio.SaveAsWeb.VisSaveAsWeb)visio.Application.SaveAsWebObject;
                                        saveAsWeb.AttachToVisioDoc(visio.Documents.OpenEx(filename, (short)Microsoft.Office.Interop.Visio.VisOpenSaveArgs.visOpenRO));
                                        Microsoft.Office.Interop.Visio.SaveAsWeb.VisWebPageSettings webPageSettings = (Microsoft.Office.Interop.Visio.SaveAsWeb.VisWebPageSettings)saveAsWeb.WebPageSettings;

                                        webPageSettings.TargetPath = outpath;
                                        webPageSettings.PageTitle = outpath;
                                        webPageSettings.DispScreenRes = Microsoft.Office.Interop.Visio.SaveAsWeb.VISWEB_DISP_RES.res768x1024;
                                        webPageSettings.QuietMode = 1;
                                        webPageSettings.SilentMode = 1;
                                        webPageSettings.NavBar = 1;
                                        webPageSettings.PanAndZoom = 1;
                                        webPageSettings.Search = 1;
                                        webPageSettings.OpenBrowser = 0;
                                        webPageSettings.PropControl = 0;

                                        saveAsWeb.CreatePages();

                                        this.webBrowser.Navigate(outpath);
                                        asisTmp = false;

                                    } catch ( Exception ) {
                                        asisTmp = true;
                                    }

                                }

                                // MS Project formats via project interop to xls: 
                                // - not working with 2003 Interops on a system running MS-Project 2003 (--> 2003 converter is shown)
                                // - not working with 2007 Interops on a system running MS-Project 2003 (--> no html support in 2003)
                                //if ((".mpp.mpx".IndexOf(ext) != -1) && b_doc) {
                                //    string outpath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "temp.xls");
                                //    if (System.IO.File.Exists(outpath)) {
                                //        System.IO.File.Delete(outpath);
                                //    }

                                //    asisTmp = true;
                                //    try {
                                //        var app = new Microsoft.Office.Interop.MSProject.Application();
                                //        app.Visible = false;
                                //        var proj = app.ActiveProject;
                                //        app.FileOpenx(filename, true, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.MSProject.PjPoolOpen.pjPoolReadOnly, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                                //        //app.FileOpenEx(filename, true, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.MSProject.PjPoolOpen.pjPoolReadOnly, Type.Missing, Type.Missing, Type.Missing, Type.Missing); 
                                //        // save MS-Project as Excel file (MS-Project interopt has no HTML exporter) and let Excel viewer do the job
                                //        proj.SaveAs(outpath, PjFileFormat.pjXLS, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                                //        app.Quit(Microsoft.Office.Interop.MSProject.PjSaveType.pjDoNotSave);
                                //        // set temporary Excel filename as "real" filename --> let Excel viewer do the work
                                //        filename = outpath;
                                //        // adjust extension --> let Excel viewer do the work 
                                //        ext = System.IO.Path.GetExtension(filename).ToLower();
                                //        asisTmp = false;
                                //    } catch (System.Reflection.TargetInvocationException x) {
                                //        asisTmp = true;
                                //    }
                                //}

                                // excel formats via excel interop to html
                                if ( (".xls.xlsx.xlsb.xlsm".IndexOf(ext) != -1) && this.b_doc ) {
                                    string outpath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "temp.html");
                                    if ( System.IO.File.Exists(outpath) ) {
                                        System.IO.File.Delete(outpath);
                                    }
                                    this.Cursor = Cursors.WaitCursor;
                                    try {
                                        // http://www.codeproject.com/Articles/507068/Microsoft-Interop-API-to-convert-the-doc-docx-dot
                                        Microsoft.Office.Interop.Excel.Application excel = null;
                                        Microsoft.Office.Interop.Excel.Workbook xls = null;
                                        excel = new Microsoft.Office.Interop.Excel.Application();
                                        object missing = Type.Missing;
                                        object trueObject = true;
                                        excel.Visible = false;
                                        excel.DisplayAlerts = false;
                                        xls = excel.Workbooks.Open(filename, missing, trueObject, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing);
                                        object format = Microsoft.Office.Interop.Excel.XlFileFormat.xlHtml;
                                        System.Collections.IEnumerator wsEnumerator = excel.ActiveWorkbook.Worksheets.GetEnumerator();
                                        while ( wsEnumerator.MoveNext() ) {
                                            this.Cursor = Cursors.WaitCursor;
                                            Microsoft.Office.Interop.Excel.Workbook wsCurrent = xls;
                                            wsCurrent.SaveAs(outpath, format, missing, missing, missing, missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange, missing, missing, missing, missing, missing);
                                        }
                                        excel.Quit();
                                        this.webBrowser.Navigate(outpath);
                                        asisTmp = false;
                                    } catch ( Exception cex ) {
                                        GrzTools.AutoMessageBox.Show(cex.Message, "Exception", 2000);
                                        asisTmp = true;
                                    }
                                    Cursor.Current = Cursors.Default;
                                }

                                // msg files are converted via MsgReader to html
                                if ( ".msg".IndexOf(ext) != -1 ) {
                                    try {
                                        string body = "";
                                        Reader msgReader = new Reader();
                                        using ( StreamReader streamReader = new StreamReader(filename) ) {
                                            body = msgReader.ExtractMsgEmailBody(streamReader.BaseStream, true, "text/html; charset=utf-8");
                                        }
                                        this.webBrowser.DocumentText = body;
                                        asisTmp = false;
                                    } catch ( Exception ) {
                                        asisTmp = true;
                                    }
                                }

                                // html-style formatted files
                                if ( (".htm.html.eml.mht".IndexOf(ext) != -1) && this.b_htm ) {
                                    this.webBrowser.Url = new Uri(filename);
                                    asisTmp = false;
                                }
                                if ( !asisTmp ) {
                                    this.pdfViewer.Visible = false;
                                    this.fileView.Visible = false;
                                    this.zipView.Visible = false;
                                    this.imgView.Visible = false;
                                    this.docView.Visible = false;
                                    this.axWMP.Visible = false;
                                    this.videoPlayerCtl.Visible = false;
                                    this.webBrowser.Visible = true;
                                }

                            }
                        }
                    }
                }
            }

            // standard file view
            if ( asisTmp ) {
                this.fileView.LoadDocument(null, filename, parent);
                this.pdfViewer.Visible = false;
                this.zipView.Visible = false;
                this.imgView.Visible = false;
                this.webBrowser.Visible = false;
                this.docView.Visible = false;
                this.videoPlayerCtl.Visible = false;
                this.axWMP.Visible = false;
                this.fileView.Visible = true;
            }

            return true;
        }

        public void Clear() {
            try {
                this.axWMP.URL = "dummy.mp3";
            } catch ( System.Exception ) {; }
            try {
                this.videoPlayerCtl.URL = "dummy.mp3";
            } catch ( System.Exception ) {; }
            try {
                this.imgView.Clear();
            } catch ( System.Exception ) {; }
            if ( this.pdfViewer.Document != null ) {
                try {
                    this.pdfViewer.Document.Dispose();
                } catch ( System.Exception ) {; }
            }
            try {
                this.webBrowser.Url = new Uri("about:blank");
            } catch ( System.Exception ) {; }
            try {
                this.docView.Clear();
            } catch ( System.Exception ) {; }
            this.zipView.ClearView();

            this.fileView.Clear(null);
            this.pdfViewer.Visible = false;
            this.zipView.Visible = false;
            this.imgView.Visible = false;
            this.webBrowser.Visible = false;
            this.docView.Visible = false;
            this.videoPlayerCtl.Visible = false;
            this.axWMP.Visible = false;
            this.fileView.Visible = true;
        }
    }
}
