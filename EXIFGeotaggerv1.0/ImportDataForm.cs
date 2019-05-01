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
        private String mDBPath;
        //public getDBPathCallback getDBPathCallback;
        //public getLayerCallback getLayerCallback;
        //public getLayerColor getLayerColor;
        //public getLayerColorHex getLayerColorHex;
        public EXIFGeoTagger mParent;
        OpenFileDialog openFileDialog;

        public ImportDataForm()
        {
            InitializeComponent();
              
        }

        private void ImportDataForm_Load(object sender, EventArgs e)
        {
            this.BringToFront();
            this.TopMost = true;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "mdb files|*.mdb";
            openFileDialog.FilterIndex = 2;
            openFileDialog.Title = "Browse Files";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.DefaultExt = "mdb";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                
                mDBPath = openFileDialog.FileName;
                
                txtDBName.Text = mDBPath;
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
            mParent.importAccessData(sender, e);
            
        }

        private void btnColour_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                btnColour.BackColor = colorDialog1.Color;
                mParent.mLayerColorHex = colorDialog1.Color.Name;
                //getLayerColorHex(colorDialog1.Color.Name);
                //getLayerColor(colorDialog1.Color);
                mParent.mLayerColor = colorDialog1.Color;
            }
        }

        private void txtLayer_TextChanged(object sender, EventArgs e)
        {
            mParent.mDBPath = mDBPath;
            //getDBPathCallback(mDBPath);
            mParent.mLayer = txtLayer.Text;
            //getLayerCallback(txtLayer.Text);
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
