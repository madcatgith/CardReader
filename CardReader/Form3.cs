using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
namespace CardReader
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private String[] baudrates = new String[] {"300","600","1200","1800","2400","4800","7200","9600"};

        private void Form3_Load(object sender, EventArgs e)
        {
            string[] config = new String[6];
            foreach (string portname in SerialPort.GetPortNames()) {
                comboBox1.Items.Add(portname);
            }
            foreach (string speed in baudrates) {
                comboBox2.Items.Add(speed);
            }
            foreach (string parity in Enum.GetNames(typeof(Parity))) {
                comboBox3.Items.Add(parity);
            }
            foreach (string stopbit in Enum.GetNames(typeof(StopBits))) {
                comboBox4.Items.Add(stopbit);
            }

            config=ReadConf();
            comboBox1.SelectedIndex = comboBox1.FindString(config[0]);
            comboBox2.SelectedIndex = comboBox2.FindString(config[1]);
            comboBox3.SelectedIndex = comboBox3.FindString(config[2]);
            comboBox4.SelectedIndex = comboBox4.FindString(config[3]);
            checkBox1.Checked = Convert.ToBoolean(config[4]);
            checkBox2.Checked = Convert.ToBoolean(config[5]);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!File.Exists("config.ini")) {
                File.Delete("config.ini");
            }
            using (StreamWriter writetext = new StreamWriter("config.ini")) {
                writetext.WriteLine("PORT="+comboBox1.SelectedItem.ToString());
                writetext.WriteLine("BAUD="+comboBox2.SelectedItem.ToString());
                writetext.WriteLine("PARITY="+comboBox3.SelectedItem.ToString());
                writetext.WriteLine("STOPBIT="+comboBox4.SelectedItem.ToString());
                writetext.WriteLine("RTS="+checkBox1.Checked.ToString());
                writetext.WriteLine("DRT="+checkBox2.Checked.ToString());
            }

            Form1 main = this.Owner as Form1;
            if (main != null)
            {
                main.SetConf(main.serialPort1);
            }
        }

        public string[] ReadConf() {
            string[] conf = new String[6];
            string line = "";
            string param = "";
            if (File.Exists("config.ini"))
            {
                using (StreamReader readtext = new StreamReader("config.ini"))
                {
                    for (int i = 0; i < 6; i++) {
                        line = readtext.ReadLine();
                        param = line.Substring(line.IndexOf("=") + 1);
                        conf[i] = param;
                    }
                }
            }
            return conf;
        }

        private void Form3_FormClosed(object sender, FormClosedEventArgs e)
        {
            Form1 main = this.Owner as Form1;
            if (main != null)
            {
                main.SetConf(main.serialPort1);
            }
        }
    }
}
