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
    
 /// <summary>
 /// Form used for data import. Obtains user input for the file or folder path, layer name and the color of markers
 /// 
 /// </summary> 
    public partial class ImportDataForm : Form
    {
        public event ImportDataDelegate importData;
        public delegate void ImportDataDelegate(string filePath, string layer, Color color);

        public event UpdateDataDelegate updateData;
        public delegate void UpdateDataDelegate(string filePath, string layer, Color color, Boolean remote);

        private string mfilePath;
        public EXIFGeoTagger mParent;
        private OpenFileDialog openFileDialog;
        private FolderBrowserDialog browseFolderDialog;
        private string fileType;
        private string filter;
        private Color mColor;
        private string mLayer;

        public ImportDataForm(string fileType)
        {
            this.fileType = fileType;
            intialize();
        }

        public ImportDataForm(Form sender, string fileType)
        {
            mParent = sender as EXIFGeoTagger;
            this.fileType = fileType;
            intialize();
        }

        private void intialize()
        {
            InitializeComponent();
            
            if (fileType.Equals("access"))
            {
                Text = "Import Access Database";
                filter = "MS Access (*.mdb *.accdb)|*.mdb;*.accdb";
                ckUploaded.Visible = false;
            }
            else if (fileType.Equals("exf"))
            {
                Text = "Open Data File";
                filter = "exf files|*.exf";
                ckUploaded.Visible = true;
            }
            else if (fileType.Equals("photos"))
            {
                Text = "Import photo data";
                filter = "jpg files|*.jpg";
                ckUploaded.Visible = false;
            }
            else if (fileType.Equals("shape"))
            {
                Text = "Import shapefile";
                filter = "ESRI shapefiles|*.shp";
                ckUploaded.Visible = false;
            }
        }

            /// <summary>
            /// Bring form to the top
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void ImportDataForm_Load(object sender, EventArgs e)
        {
            BringToFront();
            TopMost = true;
        }

        /// <summary>
        /// Sets up the file/folder dialog depending on the file type requested for import
        /// </summary>
        /// <param name="sender"> the browse button</param>
        /// <param name="e"></param>
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (fileType.Equals("access"))
            {
                openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = filter;
                openFileDialog.FilterIndex = 2;
                openFileDialog.Title = "Browse Files";
                openFileDialog.RestoreDirectory = true;
                openFileDialog.DefaultExt = "mdb | accdb";
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
            else if (fileType.Equals("shape"))
            {
                openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = filter;
                openFileDialog.FilterIndex = 2;
                openFileDialog.Title = "Browse Files";
                openFileDialog.RestoreDirectory = true;
                openFileDialog.DefaultExt = "shp";
            }

            //TODO temp hack to handle folder dialog vs file dialog
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
            Boolean remote = false;
            if (ckUploaded.Checked)
            {
                remote = true;
            }
            if (fileType.Equals("exf"))
            {
                updateData(mfilePath, mLayer, mColor, remote);
            }
            else
            {
                importData(mfilePath, mLayer, mColor);
            }
        }

        private void btnColour_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                btnColour.BackColor = colorDialog1.Color;
                mColor = colorDialog1.Color;
                //mParent.mlayerColourHex = colorDialog1.Color.Name;
                //mParent.mlayerColour = colorDialog1.Color;
            }
        }

        private void txtLayer_TextChanged(object sender, EventArgs e)
        {
            //mParent.mDBPath = mfilePath;
            //mParent.mLayer = txtLayer.Text;
            mLayer = txtLayer.Text;
        }

        //private void ckBoxGeomark_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (ckBoxGeomark.Checked)
        //    {
        //        mParent.allRecords = true;
        //    } else
        //    {
        //        mParent.allRecords = false;
        //    }
        //}
    }
}
