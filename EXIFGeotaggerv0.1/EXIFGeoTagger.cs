using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.ComponentModel;
using System.Data;

using System.Windows.Forms;

namespace EXIFGeotaggerv0._1
{
    public partial class EXIFGeoTagger : Form
    {
        String mDBPath;
        Dictionary<string, Record> mRecordDict;
        string[] mFiles; //array containing absolute paths of photos.

        string folderPath = "C:\\androidapp\\Thumbnails";

        public EXIFGeoTagger()
        {
            InitializeComponent();
            mRecordDict = new Dictionary<string, Record>();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "mdb files|*.mdb";
            openFileDialog.FilterIndex = 2;
            openFileDialog.Title = "Browse Files";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.DefaultExt = "mdb";
            openFileDialog.ShowDialog();

            //mRecords = new ArrayList();

            this.txtFilePath.Text = openFileDialog.FileName;
            mDBPath = openFileDialog.FileName;

            
            mFiles = Directory.GetFiles(folderPath);
            txtConsole.AppendText("Building file dictionary..." + Environment.NewLine);
            foreach (string file in mFiles)
            {
                string path = Path.GetFileNameWithoutExtension(file); //filename without extension of photo
                Record r = new Record(path);
                mRecordDict.Add(path, r);
                txtConsole.AppendText(mRecordDict.Count + " added" + Environment.NewLine);
                txtConsole.Clear();
            }
            txtConsole.AppendText("Exctracted " + mFiles.Length + " files" + Environment.NewLine);
            txtConsole.AppendText("Built dictionary..." + Environment.NewLine);
            txtConsole.AppendText(mRecordDict.Count + " keys added");

            string connectionString = string.Format("Provider={0}; Data Source={1}; Jet OLEDB:Engine Type={2}",
                                "Microsoft.Jet.OLEDB.4.0", mDBPath, 5);

            OleDbConnection connection = new OleDbConnection(connectionString);
            string connectionStr = connection.ConnectionString;

            string strSQL = "SELECT * FROM PhotoList;";

            OleDbCommand command = new OleDbCommand(strSQL, connection);
            // Open the connection and execute the select command.  
            try
            {
                // Open connecton  
                connection.Open();
                String[] photoPath = new String[mFiles.Length];
   
                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    int i = 0;
                    while (reader.Read())
                    {
                        Object[] row = new Object[reader.FieldCount];
                        reader.GetValues(row);
                        String photo = (string)row[1];
                        txtConsole.AppendText(photo + Environment.NewLine);
                        buildDictionary(i, photo, row);
                        i++;
                    }
                    //foreach (KeyValuePair<String, Record> kvp in mRecordDict)
                    //{
                    //    //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                    //    txtConsole.AppendText("Key: " + kvp.Key + " Value: " + kvp.Value + Environment.NewLine);
                    //}
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private void buildDictionary(int i, String photo, Object[] row)
        {
            try
            {
                Record r = mRecordDict[photo];
                r.Latitude = (double)row[2];
                r.Longitude = (double)row[3];
                r.Altitude = (double)row[4];
                r.Bearing = Convert.ToDouble(row[5]);
                r.Velocity = Convert.ToDouble(row[6]);
                r.Satellites = Convert.ToInt32(row[7]);
                r.PDop = Convert.ToDouble(row[8]);
                r.Inspector = Convert.ToString(row[9]);
                r.TimeStamp = Convert.ToDateTime(row[11]);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        private void btnGeotag_Click(object sender, EventArgs e)
        {
            foreach (string filePath in mFiles)
            {
                Image image = new Bitmap(filePath);
                //int height = theImage.Height;
                //int width = theImage.Width;
                //PropertyItem[] propItems = theImage.PropertyItems;
                PropertyItem propItemLat = image.GetPropertyItem(0x0002);
                PropertyItem propItemLatRef = image.GetPropertyItem(0x0003);
                Record r = mRecordDict[Path.GetFileNameWithoutExtension(filePath)];
                double latitude = r.Latitude;

                //if (latitude < 0)
                //{
                //    propItemLatRef.Value = 
                //}
                int[] values = r.setEXIFCoordinate("latitude");

                txtConsole.AppendText("Deg: " + values[0] + " Min: " + values[2] + " Sec: " + (double)values[4] / values[5] + Environment.NewLine);
                byte[] byteArray = new byte[24];
                int offset = 0;
                foreach (var value in values)
                {
                    BitConverter.GetBytes(value).CopyTo(byteArray, offset);
                    offset += 4;
                }
                //propItemLat.Len = byteArray.Length;
                propItemLat.Type = 5; //write bytes
                propItemLat.Value = byteArray; //write bytes
                image.SetPropertyItem(propItemLat);
                image.Save(filePath);
  
                //double longitude = r.Longitude;
                //values = r.setEXIFCoordinate("longitude");
                int degrees = BitConverter.ToInt32(propItemLat.Value, 0);
                txtConsole.AppendText(degrees + Environment.NewLine);

            }
        }
    }
}
