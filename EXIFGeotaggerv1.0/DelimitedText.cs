using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EXIFGeotagger
{
    public partial class DelimitedText : Form
    {
        private Color mColor;
        private OpenFileDialog openFileDialog;
        private string mfilePath;
        DataTable dt;
        string header;
        List<string> cbList;

        public event ImportDataDelegate importData;
        public delegate void ImportDataDelegate(DataTable table, string layer, Color color);

        public DelimitedText()
        {
            InitializeComponent();
            rdComma.Checked = true;
            cbList = new List<string>();
            cbID.DropDownStyle = ComboBoxStyle.DropDownList;
            cbXField.DropDownStyle = ComboBoxStyle.DropDownList;
            cbYField.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt;*.csv;*.sif;*.dat)|*.txt;*.csv;*.sif;*.dat|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    mfilePath = openFileDialog.FileName;
                    txtDataSource.Text = mfilePath;
                    string path = mfilePath;
                    StreamReader sr = new StreamReader(path);
                    Regex csvSplit = new Regex("(?:^|,)(\"(?:[^\"])*\"|[^,]*)", RegexOptions.Compiled);
                    header = sr.ReadLine();
                    string curr = null;                  
                    string[] values = header.Split(',');
                    dt = new DataTable();
                    DataRow row;
                    List<string> list = new List<string>();
                    foreach (string value in values)
                    {

                        dt.Columns.Add(new DataColumn(value));
                        cbList.Add(value);
                    }

                    cbXField.DataSource = cbList.ToList();
                    cbYField.DataSource = cbList.ToList();
                    cbID.DataSource = cbList.ToList();
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        foreach (Match match in csvSplit.Matches(line))
                        {
                            curr = match.Value;
                            if (0 == curr.Length)
                            {
                                list.Add("");
                            }
                            else
                            {
                                list.Add(curr.TrimStart(','));
                            }
                        }
                        string[] lineArr = list.ToArray(); 
                        if (lineArr.Length == dt.Columns.Count)
                        {
                            row = dt.NewRow();
                            row.ItemArray = lineArr;
                            list.Clear();
                            dt.Rows.Add(row);
                        }
                    }

                    dataGridView1.DataSource = dt;
                } catch (IOException ex )
                {
                    string title = "IO Exception";
                    string message = ex.Message;
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    
                    DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Error);
                    if (result == DialogResult.Yes)
                    {
                        Close();
                    }
                }
            }
            else
            {
                Close();
            }
        }

        private void btnColour_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                btnColour.BackColor = colorDialog1.Color;
                mColor = colorDialog1.Color;

            }
        }

        private void TxtCustom_TextChanged(object sender, EventArgs e)
        {
            rdComma.Checked = false;
            rdTab.Checked = false;
            rdSpace.Checked = false;
            rdColon.Checked = false;
            rdSemiColon.Checked = false;
        }

        private void rdComma_Click(object sender, EventArgs e)
        {
            txtCustom.Text = null;
            rdComma.Checked = true;
        }

        private void rdSpace_Click(object sender, EventArgs e)
        {
            txtCustom.Text = null;
            rdSpace.Checked = true;
        }

        private void rdTab_Click(object sender, EventArgs e)
        {
            txtCustom.Text = null;
            rdTab.Checked = true;
        }

        private void rdSemiColon_Click(object sender, EventArgs e)
        {
            txtCustom.Text = null;
            rdSemiColon.Checked = true;
        }

        private void rdColon_Click(object sender, EventArgs e)
        {
            txtCustom.Text = null;
            rdColon.Checked = true;
        }

        private void TxtDataSource_TextChanged(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;
        }
    }

    
}
