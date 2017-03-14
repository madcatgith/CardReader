using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.IO.Ports;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;

namespace CardReader
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
        }

        private void Form4_Load(object sender, EventArgs e)
        {
            FillInTable();
            FillOutTable();
        }

        private void FillInTable(string db_name = "cardscheme.db3") {
            dataGridView1.Rows.Clear();
            SQLiteConnection conn = new SQLiteConnection(string.Format("Data Source={0};", db_name));
            conn.Open();
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM 'income'", conn);
            SQLiteDataReader reader = command.ExecuteReader();
            int i = 0;
            foreach (DbDataRecord record in reader)
            {   
                dataGridView1.Rows.Add(new object[] {
                        record["name"].ToString(),
                        record["datetime"].ToString()
                    });
                i++;
            }
            reader.Close();
            conn.Close();
            
        }

        private void FillOutTable(string db_name = "cardscheme.db3")
        {
            dataGridView2.Rows.Clear();
            SQLiteConnection conn = new SQLiteConnection(string.Format("Data Source={0};", db_name));
            conn.Open();
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM 'outcome'", conn);
            SQLiteDataReader reader = command.ExecuteReader();
            int i = 0;
            foreach (DbDataRecord record in reader)
            {
                dataGridView2.Rows.Add(new object[] {
                        record["name"].ToString(),
                        record["datetime"].ToString()
                    });
                i++;
            }
            reader.Close();
            conn.Close();
            
        }

    }
}
