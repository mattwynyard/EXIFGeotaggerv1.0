using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EXIFGeotagger //v0._1
{
    
    public partial class ImportDataForm : Form
    {
        private String mfilePath;
        public EXIFGeoTagger mParent;
        OpenFileDialog openFileDialog;
        String fileType;
        String filter;

        public ImportDataForm(string fileType)
        {
            InitializeComponent();
            this.fileType = fileType;
            if (fileType.Equals("access"))
            {
                this.Text = "Import Access Database";
                filter = "mdb files|*.mdb";
            }
            else if (fileType.Equals("dat"))
            {
                this.Text = "Open Data File";
                filter = "exf files|*.exf";
            }
        }

            private void ImportDataForm_Load(object sender, EventArgs e)
        {
            this.BringToFront();
            this.TopMost = true;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (fileType.Equals("access")) {
                openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = filter;
                openFileDialog.FilterIndex = 2;
                openFileDialog.Title = "Browse Files";
                openFileDialog.RestoreDirectory = true;
                openFileDialog.DefaultExt = "mdb";
            } else if (fileType.Equals("exf"))
            {
                
                openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = filter;
                openFileDialog.FilterIndex = 2;
                openFileDialog.Title = "Browse Files";
                openFileDialog.RestoreDirectory = true;
                openFileDialog.DefaultExt = "exf";
            }

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {

                mfilePath = openFileDialog.FileName;
                txtDBName.Text = mfilePath;
            }
            else
            {
                Close();
                mParent.BringToFront();
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            Close();
            mParent.BringToFront();
            if (fileType.Equals("access"))
            {
                mParent.importAccessData(sender, e);
            }
            else if (fileType.Equals("exf"))
            {
                mParent.deSerializeData(mfilePath);
            }
        }

        private void btnColour_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                btnColour.BackColor = colorDialog1.Color;
                mParent.mlayerColourHex = colorDialog1.Color.Name;
                mParent.mlayerColour = colorDialog1.Color;
            }
        }

        private void txtLayer_TextChanged(object sender, EventArgs e)
        {
            mParent.mDBPath = mfilePath;
            mParent.mLayer = txtLayer.Text;
        }

        private void ckBoxGeomark_CheckedChanged(object sender, EventArgs e)
        {
            if (ckBoxGeomark.Checked)
            {
                this.mParent.allRecords = true;
            } else
            {
                this.mParent.allRecords = false;
            }
        }
    }
}
