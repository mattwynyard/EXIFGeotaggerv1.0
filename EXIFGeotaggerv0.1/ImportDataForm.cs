using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EXIFGeotaggerv0._1
{
    
    public partial class ImportDataForm : Form
    {
        private String mDBPath;
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
                mParent.mlayerColourHex = colorDialog1.Color.Name;
               


            }
        }

        private void txtLayer_TextChanged(object sender, EventArgs e)
        {
            mParent.mDBPath = mDBPath;
            mParent.mLayer = txtLayer.Text;
        }
    }
}
