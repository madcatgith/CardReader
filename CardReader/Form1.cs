using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Media;
using System.Threading;
using System.IO;
using System.IO.Ports;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using MySql.Data.MySqlClient;

namespace CardReader
{
    public partial class Form1 : Form
    {
        public static string cardkey = "";
        public static string mysql_server = "176.111.58.218";
        public static string dbmysql_username = "scheduler";
        public static string dbmysql_name = "schedule";
        public static string dbmysql_port = "3306";
        public static string dbmysql_password = "NGNscheduler746";
        public static string connStr = "Server=" + mysql_server + ";user=" + dbmysql_username + ";database=" + dbmysql_name + ";port=" + dbmysql_port + ";password=" + dbmysql_password + ";";
        private static bool startread = false;

        public Form1()
        {
            InitializeComponent();         
        }

        private void button1_Click(object sender, EventArgs e)
        {
            startRead();
        }

        private string ByteToHex(byte[] comByte)
        {
            StringBuilder builder = new StringBuilder(comByte.Length * 3);
            foreach (byte data in comByte)
            {
                builder.Append(Convert.ToString(data, 16).PadLeft(2, '0').PadRight(3, ' '));
            }
            return builder.ToString().ToUpper();
        }

        private void startRead() {
            cardkey = "";
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
            }
            try
            {
                serialPort1.Open();
                button2.Enabled = true;
                button1.Enabled = false;
                Debug.WriteLine("opened");
                if (serialPort1.IsOpen)
                {
                    OnStartBeep();
                    pictureBox1.ImageLocation = "icons/connected.png";
                    notifyIcon1.Text = "Читаем приходы";
                }
                else {
                    notifyIcon1.Text = "Считываение отключено";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                pictureBox1.ImageLocation = "icons/disconnected.png";
                notifyIcon1.Text = "Считываение отключено";
            }
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                int answerlength = 26;
                //int answerlength = 24;
                byte[] buffer = new byte[serialPort1.BytesToRead];
                serialPort1.Read(buffer, 0, serialPort1.BytesToRead);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < buffer.Length; i++)
                {
                    sb.AppendFormat("{0:X2}", buffer[i]);
                }


                if (sb.ToString().Substring(0,2).Equals("23")) { startread = true; }
                //if ((sb.ToString().Equals("00")) && (!cardkey.Equals("")))
                if (startread)
                {
                    cardkey += sb.ToString();
                }
                else {
                    cardkey = "";
                }
                if (sb.ToString().Substring(0, 2).Equals("0D")) { startread = false; }

                //Debug.WriteLine(sb.ToString());
                if (cardkey.Length==answerlength)
                {
                    showCnumber(cardkey);
                    cardkey = "";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();
                }
                button1.Enabled = true;
                button2.Enabled = false;
                Debug.WriteLine("close");
                notifyIcon1.Text = "Считываение отключено";
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
            }
        }

        public void showCnumber(string key = "")
        {
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
                //Debug.WriteLine(cleankey);
                /*if (listBox1.FindString(cleankey) == -1)
                {*/
                    string[] user = SelectUserByCard(cleankey);
                if (user[0]!=null)
                {
                    CameToWork(Int32.Parse(user[0]), user[1]);
                    listBox1.Invoke(new Action(() => listBox1.Items.Add(cleankey)));
                    successBeep();
                }
                else {
                    Debug.WriteLine("No such card");
                    ErrorBeep();
                }
                /*}*/
            }
        }

        public void successBeep() {
            Thread beep = new Thread(()=>greenbeep(0.5f));
            beep.Start();

        }

        public void OnStartBeep() {
            Thread onstartbeep = new Thread(startbeep);
            onstartbeep.Start();
        }

        public void ErrorBeep() {
            Thread errorbeep = new Thread(errorbeepsent);
            errorbeep.Start();
        }

        public void errorbeepsent() {
            byte[] com = new byte[] { 0x49, 0x20, 0x04 };
            serialPort1.Write(com, 0, 3);
            Thread.Sleep(4000);
            com = new byte[] { 0x49, 0x00, 0x00 };
            serialPort1.Write(com, 0, 3);
            Thread.Sleep(0);
        }

        public void greenbeep(float timeout) {
            timeout = timeout * 1000;
            Thread.Sleep((int)timeout);
            byte[] com = new byte[] { 0x49, 0x00, 0x81 };
            serialPort1.Write(com, 0, 3);
            Thread.Sleep((int)timeout);
            com = new byte[] { 0x49, 0x00, 0x00 };
            serialPort1.Write(com, 0, 3);
            Thread.Sleep((int)timeout);
            com = new byte[] { 0x49, 0x00, 0x81 };
            serialPort1.Write(com, 0, 3);
            Thread.Sleep((int)timeout);
            com = new byte[] { 0x49, 0x00, 0x00 };
            serialPort1.Write(com, 0, 3);
            Thread.Sleep(0);
        }

        public void startbeep() {
            try
            {
                byte[] com = new byte[] { 0x49, 0x00, 0x81 };
                serialPort1.Write(com, 0, 3);
                Thread.Sleep(200);
                com = new byte[] { 0x49, 0x00, 0x00 };
                serialPort1.Write(com, 0, 3);
                Thread.Sleep(100);
                com = new byte[] { 0x49, 0x00, 0x81 };
                serialPort1.Write(com, 0, 3);
                Thread.Sleep(200);
                com = new byte[] { 0x49, 0x00, 0x00 };
                serialPort1.Write(com, 0, 3);
                Thread.Sleep(100);
                com = new byte[] { 0x49, 0x00, 0x81 };
                serialPort1.Write(com, 0, 3);
                Thread.Sleep(200);
                com = new byte[] { 0x49, 0x00, 0x00 };
                serialPort1.Write(com, 0, 3);
                Thread.Sleep(0);
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
            }
        }

        public void nullsate() {
            byte[] com = new byte[] { 0x49, 0x00, 0x00 };
            serialPort1.Write(com, 0, 3);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            /*byte[] com = new byte[] { 0x49, 0x00, 0x00 };
            serialPort1.Write(com, 0, 3);
            successBeep();*/
            pictureBox2.ImageLocation = "icons/dbsync.png";
            TransferCardsData();
            TransferIncomeData();
            TransferOutcomeData();
            pictureBox2.ImageLocation = "icons/dbok.png";
            lastsyncdate();

        }

        private void button4_Click(object sender, EventArgs e)
        {
            successBeep();
            //serialPort1.BaseStream.Flush();
            //successBeep();
            //CreateIncomeDB();
            //Debug.WriteLine(serialPort1.PortName.ToString());
        }
        

        /*Database*/
        private void CreateSqlDB(string db_name="cardscheme.db3") {
            SQLiteConnection.CreateFile(db_name);
            SQLiteFactory factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite");
            using (SQLiteConnection connection = (SQLiteConnection)factory.CreateConnection()) {
                connection.ConnectionString = "Data source = " + db_name;
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection)) {
                    command.CommandText = @"CREATE TABLE [cards] (
                                    [id] integer PRIMARY KEY AUTOINCREMENT NOT NULL,
                                    [name] char(100) NOT NULL,
                                    [cardcode] char(100) NOT NULL
                                    );";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public void CreateIncomeDB(string db_name = "cardscheme.db3") {
            SQLiteConnection conn = new SQLiteConnection("Data source=" + db_name);
            conn.Open();
            SQLiteCommand command = new SQLiteCommand(@"DROP TABLE IF EXISTS [income];", conn);
            command.ExecuteNonQuery();
            command = new SQLiteCommand(@"DROP TABLE IF EXISTS [outcome];", conn);
            command.ExecuteNonQuery();
            SQLiteCommand com = new SQLiteCommand(@"CREATE TABLE [income] (
                [id] integer PRIMARY KEY AUTOINCREMENT NOT NULL,
                [user_id] integer NOT NULL,
                [name] char(100) NOT NULL,
                [datetime] DATETIME DEFAULT (datetime('now','localtime'))
            );", conn);
            com.ExecuteNonQuery();
            SQLiteCommand com2 = new SQLiteCommand(@"CREATE TABLE [outcome] (
                [id] integer PRIMARY KEY AUTOINCREMENT NOT NULL,
                [user_id] integer NOT NULL,
                [name] char(100) NOT NULL,
                [datetime] DATETIME DEFAULT (datetime('now','localtime'))
            );",conn);
            com2.ExecuteNonQuery();
            conn.Close();
        }

        public void CameToWork(int user_id, string name, string db_name = "cardscheme.db3") {
            if (InWorkBase(user_id))
            {
                SQLiteConnection conn = new SQLiteConnection("Data source=" + db_name);
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("INSERT INTO 'income' ('user_id','name') VALUES ('" + user_id + "','" + name + "');", conn);
                command.ExecuteNonQuery();
                conn.Close();
                LeaveWork(user_id, name);
            }
            else {
                Debug.WriteLine("Already came");
                LeaveWork(user_id,name);

            }
        }

        public void LeaveWork(int user_id, string name, string db_name = "cardscheme.db3") {
            SQLiteConnection conn = new SQLiteConnection("Data source=" + db_name);
            conn.Open();
            if (OutWorkBase(user_id))
            {
                SQLiteCommand command = new SQLiteCommand("INSERT INTO 'outcome' ('user_id','name') VALUES ('" + user_id + "','" + name + "');", conn);
                command.ExecuteNonQuery();
            }
            else {
                SQLiteCommand command = new SQLiteCommand("UPDATE 'outcome' SET datetime=(datetime('now','localtime')) WHERE user_id=" + user_id + ";", conn);
                command.ExecuteNonQuery();
            }
            
            conn.Close();
        }

        public void SqlDataAddCard(string name, string card, string db_name = "cardscheme.db3") {
            if (CardInBase(card))
            {
                SQLiteConnection conn = new SQLiteConnection("Data source=" + db_name);
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("INSERT INTO 'cards' ('name','cardcode') VALUES ('" + name + "','" + card + "');", conn);
                command.ExecuteNonQuery();
                conn.Close();
            }
            else {
                Debug.WriteLine("Already in base");
                MessageBox.Show("Карта уже в базе");
            }
        }

        public string[,] SqlDataSelectAll(string table_name="cards",string db_name = "cardscheme.db3") {
            string[,] table = new string[20,3];
            SQLiteConnection conn = new SQLiteConnection(string.Format("Data Source={0};", db_name));
            conn.Open();
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM '"+table_name+"'",conn);
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                int i = 0;
                foreach (DbDataRecord record in reader)
                {
                    Debug.WriteLine(record["name"].ToString() + " " + record["cardcode"].ToString());
                    table[i, 0] = record["id"].ToString();
                    table[i, 1] = record["name"].ToString();
                    table[i, 2] = record["cardcode"].ToString();
                    i++;
                }
                conn.Close();
                return table;
            }
        }

        public void TransferCardsData(string table_name = "cards", string db_name= "cardscheme.db3") {
            try
            {
                MySqlConnection mysql_conn = new MySqlConnection(connStr);
                mysql_conn.Open();
                string mysql_query = "";
                SQLiteConnection conn = new SQLiteConnection(string.Format("Data Source={0};", db_name));
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("SELECT * FROM '" + table_name + "'", conn);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    int i = 0;
                    foreach (DbDataRecord record in reader)
                    {
                        mysql_query = "INSERT IGNORE INTO `cards` (`id`,`name`,`cardcode`) VALUES (" + Int32.Parse(record["id"].ToString()) + ",'" + record["name"].ToString() + "','" + record["cardcode"].ToString() + "');";
                        MySqlScript script = new MySqlScript(mysql_conn, mysql_query);
                        script.Execute();
                        i++;
                    }
                    conn.Close();
                    mysql_conn.Close();
                }
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
                pictureBox2.ImageLocation = "icons/dbdisc.png";
            }
        }

        public void TransferIncomeData(string table_name = "income", string db_name = "cardscheme.db3")
        {
            try
            {
                bool inlist = false;
                MySqlConnection mysql_conn = new MySqlConnection(connStr);
                mysql_conn.Open();
                SQLiteConnection conn = new SQLiteConnection(string.Format("Data Source={0};", db_name));
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("SELECT * FROM '" + table_name + "'", conn);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    int i = 0;
                    foreach (DbDataRecord record in reader)
                    {
                        string mysql_query = "SELECT COUNT(*) AS `number` FROM  `income` WHERE  `date` = DATE(STR_TO_DATE('" + record["datetime"].ToString() + "', '%d.%m.%Y %T')) AND  `user_id` = " + record["user_id"].ToString() + " LIMIT 1;";
                        MySqlCommand q = new MySqlCommand(mysql_query, mysql_conn);
                        MySqlDataReader mysqldata;
                        mysqldata = q.ExecuteReader();
                        while (mysqldata.Read())
                        {
                            if (mysqldata.GetInt32(0) > 0)
                            {
                                inlist = true;
                            }
                        }
                        mysqldata.Close();
                        if (!inlist)
                        {
                            mysql_query = "INSERT IGNORE INTO `income` (`user_id`,`name`,`date`,`time`) VALUES (" + Int32.Parse(record["user_id"].ToString()) + ",'" + record["name"].ToString() + "',DATE(STR_TO_DATE('" + record["datetime"].ToString() + "','%d.%m.%Y %T')),TIME(STR_TO_DATE('" + record["datetime"].ToString() + "','%d.%m.%Y %T')));";
                            MySqlScript script = new MySqlScript(mysql_conn, mysql_query);
                            script.Execute();
                            i++;
                        }
                        inlist = false;
                    }

                    Debug.WriteLine("Added to list :" + i);
                    conn.Close();
                    mysql_conn.Close();
                }
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
                pictureBox2.ImageLocation = "icons/dbdisc.png";
            }
        }

        public void TransferOutcomeData(string table_name = "outcome", string db_name = "cardscheme.db3")
        {
            try
            {
                bool inlist = false;
                MySqlConnection mysql_conn = new MySqlConnection(connStr);
                mysql_conn.Open();
                SQLiteConnection conn = new SQLiteConnection(string.Format("Data Source={0};", db_name));
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("SELECT * FROM '" + table_name + "'", conn);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    int i = 0;
                    foreach (DbDataRecord record in reader)
                    {
                        string mysql_query = "SELECT COUNT(*) AS `number` FROM  `outcome` WHERE  `date` = DATE(STR_TO_DATE('" + record["datetime"].ToString() + "', '%d.%m.%Y %T')) AND  `user_id` = " + record["user_id"].ToString() + " LIMIT 1;";
                        MySqlCommand q = new MySqlCommand(mysql_query, mysql_conn);
                        MySqlDataReader mysqldata;
                        mysqldata = q.ExecuteReader();
                        while (mysqldata.Read())
                        {
                            if (mysqldata.GetInt32(0) > 0)
                            {
                                inlist = true;
                            }
                        }
                        mysqldata.Close();
                        if (!inlist)
                        {
                            mysql_query = "INSERT IGNORE INTO `outcome` (`user_id`,`name`,`date`,`time`) VALUES (" + Int32.Parse(record["user_id"].ToString()) + ",'" + record["name"].ToString() + "',DATE(STR_TO_DATE('" + record["datetime"].ToString() + "','%d.%m.%Y %T')),TIME(STR_TO_DATE('" + record["datetime"].ToString() + "','%d.%m.%Y %T')));";
                            MySqlScript script = new MySqlScript(mysql_conn, mysql_query);
                            script.Execute();
                            i++;
                        }
                        else
                        {
                            mysql_query = "UPDATE `outcome` SET `time`=TIME(STR_TO_DATE('" + record["datetime"].ToString() + "','%d.%m.%Y %T')) WHERE `user_id`=" + record["user_id"] + " AND `date`=DATE(STR_TO_DATE('" + record["datetime"].ToString() + "', '%d.%m.%Y %T'));";
                            MySqlScript script = new MySqlScript(mysql_conn, mysql_query);
                            script.Execute();
                        }
                        inlist = false;
                    }

                    Debug.WriteLine("Added to list :" + i);
                    conn.Close();
                    mysql_conn.Close();
                }
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
                pictureBox2.ImageLocation = "icons/dbdisc.png";
            }
        }

        public string[] SelectUserByCard(string card, string db_name = "cardscheme.db3") {
            Debug.WriteLine("Select user");
            string[] row = new string[3]; 
            SQLiteConnection conn = new SQLiteConnection(string.Format("Data Source={0};", db_name));
            conn.Open();
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM 'cards' WHERE cardcode LIKE '" + card + "' LIMIT 1", conn);
            using (SQLiteDataReader reader = command.ExecuteReader()) { 
                foreach (DbDataRecord record in reader)
                {
                    row[0] = record["id"].ToString();
                    row[1] = record["name"].ToString();
                    row[2] = record["cardcode"].ToString();
                }
            conn.Close();
            return row;
            }
        }

        public bool CardInBase(string card, string db_name="cardscheme.db3") {
            int cnt;
            SQLiteConnection conn = new SQLiteConnection(string.Format("Data Source={0};", db_name));
            conn.Open();
            SQLiteCommand command = new SQLiteCommand("SELECT COUNT(*) AS 'number' FROM 'cards' WHERE cardcode LIKE '"+card+"'", conn);
            SQLiteDataReader reader = command.ExecuteReader();
            foreach (DbDataRecord record in reader)
            {
                Debug.WriteLine(record["number"].ToString());
                Int32.TryParse(record["number"].ToString(),out cnt);
                if (cnt > 0) {
                    reader.Close();
                    return false;
                }
            }
            reader.Close();
            return true;
        }

        public bool InWorkBase(int user_id, string db_name = "cardscheme.db3")
        {
            int cnt;
            SQLiteConnection conn = new SQLiteConnection(string.Format("Data Source={0};", db_name));
            conn.Open();
            SQLiteCommand command = new SQLiteCommand("SELECT COUNT(*) AS 'number' FROM 'income' WHERE user_id=" + user_id+" AND DATE(datetime)=DATE('now')", conn);
            SQLiteDataReader reader = command.ExecuteReader();
            foreach (DbDataRecord record in reader)
            {
                Debug.WriteLine(record["number"].ToString());
                Int32.TryParse(record["number"].ToString(), out cnt);
                if (cnt > 0)
                {
                    reader.Close();
                    return false;
                }
            }
            reader.Close();
            return true;
        }

        public bool OutWorkBase(int user_id, string db_name = "cardscheme.db3") {
            int cnt;
            SQLiteConnection conn = new SQLiteConnection(string.Format("Data Source={0};", db_name));
            conn.Open();
            SQLiteCommand command = new SQLiteCommand("SELECT COUNT(*) AS 'number' FROM 'outcome' WHERE user_id=" + user_id + " AND DATE(datetime)=DATE('now')", conn);
            SQLiteDataReader reader = command.ExecuteReader();
            foreach (DbDataRecord record in reader)
            {
                Debug.WriteLine(record["number"].ToString());
                Int32.TryParse(record["number"].ToString(), out cnt);
                if (cnt > 0)
                {
                    reader.Close();
                    return false;
                }
            }
            reader.Close();
            return true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen) {
                serialPort1.Close();
                button1.Enabled = true;
                button2.Enabled = false;
            }
            Form2 form=new Form2();
            form.Show();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
                button1.Enabled = true;
                button2.Enabled = false;
            }
            Form3 form_conf = new Form3();
            form_conf.Owner = this;
            form_conf.Show();
        }

        public bool SetConf(SerialPort sport) {
            if (File.Exists("config.ini"))
            {
                string line = "";
                string param = "";
                try
                {
                    using (StreamReader readtext = new StreamReader("config.ini"))
                    {
                        line = readtext.ReadLine();
                        param = line.Substring(line.IndexOf("=") + 1);
                        sport.PortName = param;
                        line = readtext.ReadLine();
                        param = line.Substring(line.IndexOf("=") + 1);
                        sport.BaudRate = int.Parse(param);
                        line = readtext.ReadLine();
                        param = line.Substring(line.IndexOf("=") + 1);
                        sport.Parity = (Parity)Enum.Parse(typeof(Parity),(string)param);
                        line = readtext.ReadLine();
                        param = line.Substring(line.IndexOf("=") + 1);
                        sport.StopBits = (StopBits)Enum.Parse(typeof(StopBits), (string)param);
                        line = readtext.ReadLine();
                        param = line.Substring(line.IndexOf("=") + 1);
                        sport.RtsEnable = Convert.ToBoolean(param);
                        line = readtext.ReadLine();
                        param = line.Substring(line.IndexOf("=") + 1);
                        sport.DtrEnable = Convert.ToBoolean(param);
                    }
                    
                    return true;
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex);
                    return false;
                }
                
            }
            else {
                return false;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            serialPort1.Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Form4 form4 = new Form4();
            form4.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button2.Enabled = false;
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
            }
            if (!File.Exists("cardscheme.db3"))
            {
                CreateSqlDB();
            }
            if (!SetConf(serialPort1))
            {
                serialPort1.PortName = "COM1";
                serialPort1.Parity = Parity.None;
                serialPort1.StopBits = StopBits.One;
                serialPort1.BaudRate = 2400;
            }
            else
            {
                Debug.WriteLine("config loaded from file");
            }
            pictureBox1.ImageLocation = "icons/disconnected.png";
            pictureBox2.ImageLocation = "icons/dbdisc.png";

            bool cleartable = false;
            if (File.Exists("sync.log")) {
                string date="";
                DateTime today = DateTime.Today;
                using (StreamReader readtext = new StreamReader("sync.log")) {
                    date = readtext.ReadLine().Trim();
                    date = date.Substring(date.IndexOf("=")+1);
                    if (!today.ToString("d").Equals(date)) {
                        cleartable = true;
                    }
                    else
                    {
                        cleartable = false;
                    }
                    Debug.WriteLine(cleartable);
                }
                if (cleartable) {
                    pictureBox2.ImageLocation = "icons/dbsync.png";
                    TransferIncomeData();
                    TransferOutcomeData();
                    pictureBox2.ImageLocation = "icons/dbok.png";
                    CreateIncomeDB();
                    lastsyncdate();
                }
            }
            startRead();
            timer1.Start();
            this.Hide();
            //SqlDataSelectAll();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                pictureBox2.ImageLocation = "icons/dbsync.png";
                TransferIncomeData();
                TransferOutcomeData();
                pictureBox2.ImageLocation = "icons/dbok.png";
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
                pictureBox2.ImageLocation = "icons/dbdisc.png";

            }
        }

        public void lastsyncdate() {
            if (!File.Exists("sync.log"))
            {
                File.Delete("sync.log");
            }
            using (StreamWriter writetext = new StreamWriter("sync.log"))
            {
                DateTime today = DateTime.Today;
                writetext.WriteLine("DATE=" + today.ToString("d"));
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }
    }
}
