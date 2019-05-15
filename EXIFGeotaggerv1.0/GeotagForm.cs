using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EXIFGeotagger //v0._1
{
    public partial class GeotagForm : Form
    {
        private String inPath;
        private String outPath;
        public EXIFGeoTagger mParent;
        private FolderBrowserDialog folderBrowseDialog;
        public event writeGeoTagDelegate writeGeoTag;
        public delegate void writeGeoTagDelegate(string filePath, string layer, string color);

        public GeotagForm()
        {
            InitializeComponent();
        }

        private void GeotagForm_Load(object sender, EventArgs e)
        {
            this.BringToFront();
            this.TopMost = true;
        }

        private void btnBrowse1_Click(object sender, EventArgs e)
        {
            using (var browseDialog = new FolderBrowserDialog())
            {
                DialogResult result = browseDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(browseDialog.SelectedPath))
                {
                    //mParent.mFiles = Directory.GetFiles(browseDialog.SelectedPath);
                    inPath = browseDialog.SelectedPath;
                    txtInputPath.Text = inPath;
                    MessageBox.Show("Files found: " + Directory.GetFiles(inPath).Length.ToString(), "Message");
                    this.BringToFront();
                    this.TopMost = true;
                }
            }
        }

        private void btnBrowse2_Click(object sender, EventArgs e)
        {
            using(var browseDialog = new FolderBrowserDialog())
            {
                DialogResult result = browseDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(browseDialog.SelectedPath))
                {
                    
                    outPath = browseDialog.SelectedPath;
                    txtOutputPath.Text = outPath;
                }
            }
        }

        private void btnGeotag_Click(object sender, EventArgs e)
        {
            mParent.outFolder = outPath;
            mParent.mFiles = Directory.GetFiles(inPath);
            Close();
            mParent.BringToFront();
            //mParent.TopMost = true;
            mParent.startWorker(sender, e);
        }

        private void Label3_Click(object sender, EventArgs e)
        {

        }

        private void Label3_Click_1(object sender, EventArgs e)
        {

        }
    }
}
