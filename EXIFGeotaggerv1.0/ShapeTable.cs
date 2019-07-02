using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EXIFGeotagger
{
    public partial class ShapeTable : Form
    {
        public ShapeTable(DataTable table)
        {
            InitializeComponent();
            dataGridView1.DataSource = table;
        }

        private void ShapeTable_Load(object sender, EventArgs e)
        {

        }
    }
}
