using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Diagnostics;
using ICSharpCode.SharpZipLib.Zip;
using FileLibrary;
using System.Configuration;

namespace ELCDeploy
{
    public partial class Form1 : Form
    {
        String BasePath = AppDomain.CurrentDomain.BaseDirectory; 
        string versionfile = AppDomain.CurrentDomain.BaseDirectory + "versionfile.xml";
        System.Collections.Hashtable localVersion = new System.Collections.Hashtable();
        string ownerIp = "";
        public Form1()
        {
            try
            { 
                InitializeComponent();

                SetDePloyEnable(false);

                ownerIp = System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName()).AddressList[0].ToString();
                if (System.IO.File.Exists(versionfile))
                {
                    System.Data.DataSet ds = new DataSet();
                    ds.ReadXml(versionfile);
                    if (ds.Tables.Count == 0)
                    {
                        return;
                    }
                    foreach (DataRow eRow in ds.Tables[0].Rows)
                    {
                        localVersion.Add(eRow[0].ToString(), eRow[1].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                FileServer serverObj = (FileServer)Activator.GetObject(
                    typeof(FileServer), "Tcp://" + ip + ":" + port + "/fileService");
                System.Collections.Hashtable directory = serverObj.GetfileDirectory();
                backgroundWorker.ReportProgress(directory.Count, "Total file is :" + directory.Count);
                System.Collections.IDictionaryEnumerator g = directory.GetEnumerator();
                int i = 1;
                while (g.MoveNext())
                {
                    backgroundWorker.ReportProgress(i, "(" + i + "/" + directory.Count + ")" + g.Key.ToString());
                    i++;
                    if (g.Key.ToString().StartsWith("WindowsFormsApplication1"))
                    {
                        Console.WriteLine("");
                    }
                    if (localVersion.Contains(g.Key) && localVersion[g.Key].Equals(g.Value))
                    {
                        continue;
                    }
                    try
                    {
                        FileStructure b = serverObj.DownloadFile(g.Key.ToString(), ownerIp);
                        string path = BasePath + b.path + "\\";
                        if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
                        writeFile(path + g.Key.ToString(), b.stream);
                        //RegisterCom(g.Key.ToString());
                        localVersion.Remove(g.Key);
                        localVersion.Add(g.Key, g.Value);
                    }
                    catch
                    { }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void writeFile(string fileName, byte[] stream)
        {
            ZipInputStream zis = new ZipInputStream(new MemoryStream(stream, 0, stream.Length));
            ZipEntry theEntry;
            while ((theEntry = zis.GetNextEntry()) != null)
            {
                if (fileName != String.Empty)
                {
                    FileStream streamWriter = File.Create(fileName);
                    int size = 2048;
                    if (stream.Length < 2048)
                        size = stream.Length;
                    byte[] data = new byte[size];
                    while (true)
                    {
                        size = zis.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            streamWriter.Write(data, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }

                    streamWriter.Close();
                }
            }
            zis.Close();
        }



        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            label1.Text = e.UserState.ToString();
            label3.Text = "安装路径:" + BasePath;
            if (e.ProgressPercentage == -1)
                return;
            if (progressBar1.Value == 0)
            {
                progressBar1.Maximum = e.ProgressPercentage;
                progressBar1.Value = 1;
            }
            else
                progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            System.Data.DataSet ds = new DataSet();
            System.Data.DataTable dt = new DataTable();
            dt.Columns.Add("Key");
            dt.Columns.Add("Value");
            System.Collections.IDictionaryEnumerator g = localVersion.GetEnumerator();
            while (g.MoveNext())
            {
                DataRow eRow = dt.NewRow();
                eRow[0] = g.Key.ToString();
                eRow[1] = g.Value.ToString();
                dt.Rows.Add(eRow);
            }
            ds.Tables.Add(dt);
            ds.WriteXml(versionfile);

            //startCRM(); 
            SetDePloyEnable(true );
        }

        private void SetDePloyEnable(bool enable)
        {
            btnHQClient.Enabled = enable;
            btnShopClient.Enabled = enable;
            btnShopServer.Enabled = enable;
        }



       

        bool TelnetApp(string IP, int port)
        {
          //  return true;
            IPAddress addr = IPAddress.Parse(IP);
            System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
            try
            {
                socket.Connect(addr, port);
            }
            catch
            {
                socket.Close();
                return false;
            }
            if (socket.Connected)
            {
                socket.Disconnect(false);
                socket.Close();
                return true;
            }
            else
            {
                socket.Close();
                return false;
            }
        }
        string ip;
        int port;
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                this.WindowState = FormWindowState.Minimized;
                ip = System.Configuration.ConfigurationSettings.AppSettings["Server"];
                port = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["Port"]);
                if (!TelnetApp(ip, port))
                {
                    SetDePloyEnable(true);
                }
                else
                {
                    this.WindowState = FormWindowState.Normal;
                    CheckLocalFile();
                    backgroundWorker.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// 解密
        /// </summary> 
        public string decode(string str)
        {
            string dtext = "";

            for (int i = 0; i < str.Length; i++)
            {
                dtext = dtext + (char)(str[i] - 10 + 1 * 2);
            }
            return dtext;
        }

        private void CheckLocalFile()
        {
            label1.Text = "Check local file environment ... ";
            progressBar1.Maximum = localVersion.Count;
            int i = 0;
            System.Collections.IDictionaryEnumerator g = localVersion.GetEnumerator();
            List<string> temp = new List<string>();
            while (g.MoveNext())
            {
                progressBar1.Value = i;
                i++;
                string[] s = System.IO.Directory.GetFiles(BasePath, g.Key.ToString(), SearchOption.AllDirectories);
                if (s.Length == 0)
                {
                    temp.Add(g.Key.ToString());

                }
            }
            foreach (string s in temp)
            {
                localVersion[s] = System.DateTime.MinValue.ToString();
            }
            progressBar1.Value = 0;
        }


        private void startCRM(string AppName)
        {
            this.DialogResult = DialogResult.No;
            string install_folder = BasePath;

            FileInfo fi = new FileInfo(install_folder + ConfigurationManager.AppSettings[AppName]);
            if (fi.Exists)
            {
                Process.Start(install_folder + ConfigurationManager.AppSettings[AppName]);
            
            }
            // this.Close();
        }
        private void btnShopClient_Click(object sender, EventArgs e)
        {
            startCRM("ShopClient");
            this.Close();
        }

        private void btnShopServer_Click(object sender, EventArgs e)
        {
            startCRM("ShopServer");
        }

        private void btnHQClient_Click(object sender, EventArgs e)
        {
            startCRM("HQClient");
            this.Close();
        }
    }

}