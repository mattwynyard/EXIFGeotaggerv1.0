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
        private string mDataPath;
        private string mInPath;
        public string[] mFiles; //array containing absolute paths of photos.
        private string mOutPath;
        private string mLayer;
        private string mColor;
        private Boolean mAllRecords;
        public EXIFGeoTagger mParent;
        private FolderBrowserDialog folderBrowseDialog;
        private OpenFileDialog openFileDialog;
        private string filter;
        public event writeGeoTagDelegate writeGeoTag;
        public delegate void writeGeoTagDelegate(string dbPath, string inPath, string outPath, string layer, string color, Boolean allRecords);

        public GeotagForm()
        {
            InitializeComponent();
            Text = "Geotag";
            filter = "mdb files|*.mdb";
        }

        private void GeotagForm_Load(object sender, EventArgs e)
        {
            this.BringToFront();
            this.TopMost = true;
            btnColor.BackColor = colorDialog1.Color;
            mColor = "ffff8080";
            mLayer = txtLayer.Text;
        }

        private void btnBrowse0_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                //openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = filter;
                openFileDialog.FilterIndex = 2;
                openFileDialog.Title = "Browse Files";
                openFileDialog.RestoreDirectory = true;
                openFileDialog.DefaultExt = "mdb";
                DialogResult result = openFileDialog.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
                {
                    mDataPath = openFileDialog.FileName;
                    txtDataSource.Text = openFileDialog.FileName;
                    BringToFront();
                    TopMost = true;
                }
            }
        }

        private void btnBrowse1_Click(object sender, EventArgs e)
        {
            using (var browseDialog = new FolderBrowserDialog())
            {
                browseDialog.SelectedPath = Directory.GetParent(mDataPath).ToString();
                DialogResult result = browseDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(browseDialog.SelectedPath))
                {
                    mInPath = browseDialog.SelectedPath;
                    txtInputPath.Text = mInPath;
                    BringToFront();
                    TopMost = true;
                }
            }
        }

        private void btnBrowse2_Click(object sender, EventArgs e)
        {
            using(var browseDialog = new FolderBrowserDialog())
            {
                browseDialog.SelectedPath = Directory.GetParent(mDataPath).ToString();
                DialogResult result = browseDialog.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(browseDialog.SelectedPath))
                {  
                    mOutPath = browseDialog.SelectedPath;
                    string root = Directory.GetDirectoryRoot(mOutPath);
                    txtOutputPath.Text = mOutPath;
                }
            }
        }

        private void chkGeoMark_CheckedChanged(object sender, EventArgs e)
        {
            if (ckBoxGeoMark.Checked)
            {
                mAllRecords = true;
            }
            else
            {
                mAllRecords = false;
            }
        }

        private void btnColour_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                btnColor.BackColor = colorDialog1.Color;
                mColor = colorDialog1.Color.Name;
                //mParent.mlayerColourHex = colorDialog1.Color.Name;
                //mParent.mlayerColour = colorDialog1.Color;
            }
        }


        private void btnGeotag_Click(object sender, EventArgs e)
        {
            Close();
            mParent.BringToFront();;
            writeGeoTag(mDataPath, mInPath, mOutPath, mLayer, mColor, mAllRecords);
        }

        private void txtDataSource_TextChanged(object sender, EventArgs e)
        {
            mDataPath = txtDataSource.Text;

        }

        private void TxtLayer_TextChanged(object sender, EventArgs e)
        {
            mLayer = txtLayer.Text;
        }
    }
}
