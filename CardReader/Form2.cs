using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;

namespace CardReader
{
    public partial class Form2 : Form
    {
        public static string cardkey = "";
        public Form1 form;

        public Form2()
        {
            InitializeComponent();
            
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            serialPort1.Open();
            Debug.WriteLine("opened");
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {

                byte[] buffer = new byte[serialPort1.BytesToRead];
                serialPort1.Read(buffer, 0, serialPort1.BytesToRead);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < buffer.Length; i++)
                {
                    sb.AppendFormat("{0:X2}", buffer[i]);
                }

                //if ((sb.ToString().Equals("00")) && (!cardkey.Equals("")))
                cardkey += sb.ToString();
                if (cardkey.Length==26)
                {
                    ReadNumber(cardkey);
                    cardkey = "";
                    serialPort1.Close();
                }

                Debug.WriteLine(sb.ToString());

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void ReadNumber(string key="") {
            Debug.WriteLine(key);
            string cleankey = "";
            if ((key.Substring(0, 2)).Equals("23"))
            {
                key = key.Substring(2, 21);
                Debug.WriteLine(key);
                for (int i = 1; i < key.Length; i++)
                {
                    if ((i % 2) != 0)
                    {
                        cleankey += key[i];
                    }
                }
                    textBox1.Invoke(new Action(() => textBox1.Text=cleankey));
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            form.SqlDataAddCard(textBox2.Text.ToString(),textBox1.Text.ToString());
            GetCards();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int rowindex = dataGridView1.SelectedRows[0].Index;
            int id = Int32.Parse(dataGridView1.Rows[rowindex].Cells[0].Value.ToString());
            DeleteItem(id);

        }

        public void DeleteItem(int id,string db_name= "cardscheme.db3") {
            SQLiteConnection conn = new SQLiteConnection("Data source=" + db_name);
            conn.Open();
            SQLiteCommand command = new SQLiteCommand("DELETE FROM 'cards' WHERE id="+id+";", conn);
            command.ExecuteNonQuery();
            conn.Close();
            GetCards();
        }

        public void GetCards() {
            dataGridView1.Rows.Clear();
            string[,] table = form.SqlDataSelectAll();
            Debug.WriteLine(table.GetLength(0));
            for (int i = 0; i < table.GetLength(0); i++)
            {
                if (!(table[i,0]==null))
                {
                    if (!table[i, 0].Equals(""))
                    {
                        dataGridView1.Rows.Add(new object[] {
                            table[i,0],
                            table[i,1],
                            table[i,2]
                        });
                    }
                }
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            Form1 main = new Form1();
            main.SetConf(serialPort1);
            form = new Form1();
            //dataGridView1.ColumnCount = 3;
            GetCards();
        }
    }
}
