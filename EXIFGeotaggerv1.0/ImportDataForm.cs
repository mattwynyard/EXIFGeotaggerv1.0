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
        public event layerVariablesDelegate layerVariables;
        public delegate void layerVariablesDelegate(string filePath, string layer, string color);

        private String mfilePath;
        public EXIFGeoTagger mParent;
        private OpenFileDialog openFileDialog;
        private FolderBrowserDialog browseFolderDialog;
        private string fileType;
        private string filter;
        private string mlayerColourHex;
        private string mLayer;

        public ImportDataForm(string fileType)
        {
            InitializeComponent();
            this.fileType = fileType;
            if (fileType.Equals("access"))
            {
                this.Text = "Import Access Database";
                filter = "mdb files|*.mdb";
            }
            else if (fileType.Equals("exf"))
            {
                this.Text = "Open Data File";
                filter = "exf files|*.exf";
            } else if (fileType.Equals("photos"))
            {
                this.Text = "Import photo data";
                filter = "jpg files|*.jpg";
            }
        }

            private void ImportDataForm_Load(object sender, EventArgs e)
        {
            this.BringToFront();
            this.TopMost = true;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (fileType.Equals("access"))
            {
                openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = filter;
                openFileDialog.FilterIndex = 2;
                openFileDialog.Title = "Browse Files";
                openFileDialog.RestoreDirectory = true;
                openFileDialog.DefaultExt = "mdb";
            }
            else if (fileType.Equals("exf"))
            {
                openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = filter;
                openFileDialog.FilterIndex = 2;
                openFileDialog.Title = "Browse Files";
                openFileDialog.RestoreDirectory = true;
                openFileDialog.DefaultExt = "exf";
            }
            else if (fileType.Equals("photos"))
            {
                browseFolderDialog = new FolderBrowserDialog();
                if (browseFolderDialog.ShowDialog() == DialogResult.OK)
                {
                    mfilePath = browseFolderDialog.SelectedPath;
                    txtDBName.Text = mfilePath;
                    
                } else
                {
                    Close();
                    mParent.BringToFront();
                }
            }
            if (openFileDialog != null)
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {

                    mfilePath = openFileDialog.FileName;
                    txtDBName.Text = mfilePath;
                }
                else
                {
                    Close();
                }
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            Close();
            mParent.BringToFront();
            if (fileType.Equals("access"))
            {
                //mParent.importAccessData(sender, e);
                mParent.startWorker(sender, e);
            }
            else if (fileType.Equals("exf"))
            {
                mParent.deSerializeData(mfilePath);
            }
            else if (fileType.Equals("photos"))
            {
                if (this.layerVariables != null)
                {
                    this.layerVariables(mfilePath, mLayer, mlayerColourHex);
                }
            }


        }

        private void btnColour_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                btnColour.BackColor = colorDialog1.Color;
                mlayerColourHex = colorDialog1.Color.Name;
                mParent.mlayerColourHex = colorDialog1.Color.Name;
                mParent.mlayerColour = colorDialog1.Color;
            }
        }

        private void txtLayer_TextChanged(object sender, EventArgs e)
        {
            mParent.mDBPath = mfilePath;
            mParent.mLayer = txtLayer.Text;
            mLayer = txtLayer.Text;
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
