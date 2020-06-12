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
        public delegate void writeGeoTagDelegate(string dbPath, string inPath, string outPath, string layer, string color, Boolean allRecords, Boolean zip, string inspector,  DateTime startDate, DateTime endDate);

        private Boolean mMirror;
        private Boolean mGamma;
        private Boolean mContrast;
        private Boolean mZip;
        private int days;
        private DateTime startDate;
        private DateTime endDate;

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
            datePickerStart.Enabled = false;
            datePickerEnd.Enabled = false;
            datePickerStart.MaxDate = DateTime.Now;
            datePickerEnd.MaxDate = DateTime.Now;
            //startDate = datePickerStart.Value;
            datePickerEnd.MinDate = startDate = datePickerStart.Value;
            endDate = datePickerEnd.Value;
            days = (datePickerStart.Value - datePickerEnd.Value).Days;

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
            mParent.BringToFront();
            string inspector;
            try
            {
                inspector = cbInspector.SelectedItem.ToString();

            } catch (NullReferenceException ex)
            {
                inspector = "";
            }

            writeGeoTag(mDataPath, mInPath, mOutPath, mLayer, mColor, mAllRecords, mZip, inspector, startDate, endDate);
        }

        private void txtDataSource_TextChanged(object sender, EventArgs e)
        {
            mDataPath = txtDataSource.Text;

        }

        private void TxtLayer_TextChanged(object sender, EventArgs e)
        {
            mLayer = txtLayer.Text;
        }


        private void CkZip_CheckedStateChanged(object sender, EventArgs e)
        {
            if (ckZip.Checked)
            {
                mZip = true;
            }
            else
            {
                mZip = false;
            }
        }

        private void BtnCorrect_Click(object sender, EventArgs e)
        {
            String photo = null;
            using (var openFileDialog = new OpenFileDialog())
            {
                filter = "jpg files|*.jpg";
                openFileDialog.Filter = filter;
                openFileDialog.FilterIndex = 2;
                openFileDialog.Title = "Browse Files";
                openFileDialog.RestoreDirectory = true;
                openFileDialog.DefaultExt = "jpg";
                DialogResult result = openFileDialog.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
                {
                    photo = openFileDialog.FileName;
                    txtDataSource.Text = openFileDialog.FileName;
                    BringToFront();
                    TopMost = true;
                }
            }
            //CorrectionUtil correct = new CorrectionUtil(photo);
            CorrectionUtil.ClaheCorrection(photo, 0.5);

        }

        private void datePickerStart_ValueChanged(object sender, EventArgs e)
        {
            datePickerEnd.MinDate = startDate = datePickerStart.Value;
            days = (datePickerEnd.Value - datePickerStart.Value).Days;
        }

        private void datePickerEnd_ValueChanged(object sender, EventArgs e)
        {

            endDate = datePickerEnd.Value;
            days = (datePickerEnd.Value - datePickerStart.Value).Days;
        }


    }
}
