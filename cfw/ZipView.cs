using ICSharpCode.SharpZipLib.Zip;          // zip
using System.Collections.Generic;
using System.Windows.Forms;

namespace cfw {
    public partial class ZipView : UserControl {
        public ZipView() {
            this.InitializeComponent();
        }

        public void ClearView() {
            this.richTextBox.Clear();
        }

        public void LoadZip(string file) {
            this.richTextBox.Clear();
            List<string> list = new List<string>();
            using ( ZipFile zf = new ZipFile(file) ) {
                foreach ( ZipEntry ze in zf ) {
                    if ( ze.IsFile ) {
                        //string[] items = { "", "", "" };
                        //items[0] = ze.Name;
                        //items[1] = ze.DateTime.ToString("dd.MM.yyyy HH:mm:ss");
                        //items[2] = ze.Size.ToString("0,0", CultureInfo.InvariantCulture);
                        list.Add(ze.Name);
                    }
                }
            }
            this.richTextBox.Lines = list.ToArray();
            this.richTextBox.Select(1, 0);
            this.richTextBox.ScrollToCaret();
        }
    }
}
