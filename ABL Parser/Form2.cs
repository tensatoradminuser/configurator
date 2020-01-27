using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ABL_Parser
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            try
            {
                // Show the dialog and get result.
                DialogResult result = openFileDialog1.ShowDialog();
                string filename = string.Empty;

                if (result == DialogResult.OK) // Test result.
                {
                    filename = openFileDialog1.FileName;
                }

                // Load CSV and display in grid.
                StreamReader sr = new StreamReader(filename);
                string[] headers = sr.ReadLine().Split(',');
                DataTable dt = new DataTable();
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }
                while (!sr.EndOfStream)
                {
                    string[] rows = Regex.Split(sr.ReadLine(), ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        dr[i] = rows[i];
                    }
                    dt.Rows.Add(dr);
                }

                this.dataGridView1.Visible = true;
                dataGridView1.AutoGenerateColumns = true;
                dataGridView1.DataSource = dt;

                // Show columns and types.
                List<int> maximumLengthForColumns =
                    Enumerable.Range(0, dt.Columns.Count)
                    .Select(col => dt.AsEnumerable()
                    .Select(row => row[col]).OfType<string>()
                    .Max(val => val.Length)).ToList();

                foreach (DataColumn column in dt.Columns)
                {
                    Debug.WriteLine(column.ColumnName + "\t" + column.DataType.ToString());
                    string tabs = column.ColumnName.Length < 10 ? "\t\t" : "\t";
                    textBox1.Text = textBox1.Text + Environment.NewLine + column.ColumnName + tabs + column.DataType.ToString() + " (" + maximumLengthForColumns[column.Ordinal].ToString() + " )";
                }

                this.Text = filename;
                textBox1.Text = textBox1.Text + Environment.NewLine + Environment.NewLine + filename;
            }
            catch
            {

            }




        }
    }
}
