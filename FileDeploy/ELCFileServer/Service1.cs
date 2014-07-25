using FileLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.IO;

namespace ELCFileServer
{
    public partial class Service1 : ServiceBase
    {

        private void Log(string message)
        {
            //string path = "FileServerLog.log";
            //string log = string.Format("{0}:{1}", DateTime.Now, message);
            //if (!File.Exists(path))
            //    File.Create(path);
            //StreamWriter sw = null;
            //try
            //{
            //    sw = File.AppendText(path);

            //    sw.WriteLine(log);
            //    sw.Flush();
            //}
            //catch
            //{
            //}
            //finally
            //{
            //    if (sw != null)
            //        sw.Close(); ;
            //}
            //try
            //{
            //    //TestSql.SqlHelper.ExecuteNonQuery("data Source=172.16.210.145;Initial Catalog=CRM;user id=sa;pwd=arvato", CommandType.Text, string.Format("INSERT INTO serverlog(message,servername) SELECT '{0}','ELCFileServer'", message));
            //}
            //catch (Exception ex)
            //{
            //}
        }


        public Service1()
        {
            InitializeComponent();

            try
            {
                Log("Server Init!");
                int port = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["port"]);
                Log("端口："+port.ToString());
                ChannelServices.RegisterChannel(new TcpChannel(port), false);
                RemotingConfiguration.RegisterWellKnownServiceType(typeof(FileLibrary.FileServer), "fileService", WellKnownObjectMode.Singleton); 
                //MakeDBFile.StartMonitor(strConn, intervalTime); 

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        protected override void OnStart(string[] args)
        {
            Log("Server Start!");
            fw = new System.IO.FileSystemWatcher();
            fw.Path = BuildFileCache.BasePath;
            fw.IncludeSubdirectories = true;
            fw.EnableRaisingEvents = true;
            fw.Deleted += new System.IO.FileSystemEventHandler(fw_Deleted);
            fw.Created += new System.IO.FileSystemEventHandler(fw_Created);
            fw.Changed += fw_Changed;
            ////启动即加载
            BuildFileCache.fileCache.Clear();
            BuildFileCache.fileDirectory.Clear();
            Console.WriteLine("清除文件完毕!");
            Console.WriteLine("加载所有文件中...");
            Log("加载所有文件中");
            BuildFileCache.BuildAll();
            Log("文件数：" + BuildFileCache.fileDirectory.Count.ToString() + "所有文件大小:" + BuildFileCache.totalsize.ToString() + " ,  压缩后:" + BuildFileCache.compresssize.ToString() + ", 压缩比:" + Convert.ToDouble(BuildFileCache.compresssize) / Convert.ToDouble(BuildFileCache.totalsize));
        }

        protected override void OnStop()
        {
            Log("Server stop!");
            fw.Deleted -= new System.IO.FileSystemEventHandler(fw_Deleted);
            fw.Created -= new System.IO.FileSystemEventHandler(fw_Created);
            fw.Changed -= fw_Changed;

        }


        static System.IO.FileSystemWatcher fw = null;



        void fw_Created(object sender, System.IO.FileSystemEventArgs e)
        {
            Thread.Sleep(5000);
            Log(System.DateTime.Now.ToString(" MM/dd hh:mm:ss ") + ":  从缓存中新增 " + e.FullPath);
            BuildFile(e);
        }

        private void BuildFile(System.IO.FileSystemEventArgs e)
        {
            try
            {
                BuildFileCache.BuildFile(e.FullPath);
            }
            catch
            {
                Thread.Sleep(10000);
                try
                {
                    BuildFileCache.BuildFile(e.FullPath);
                }
                catch
                {
                }
            }
        }
        void fw_Deleted(object sender, System.IO.FileSystemEventArgs e)
        { 
            Log(System.DateTime.Now.ToString(" MM/dd hh:mm:ss ") + ":  从缓存中删除 " + e.FullPath);
            DelFromCache(e);
        }

        private static void DelFromCache(System.IO.FileSystemEventArgs e)
        {
            if (BuildFileCache.fileCache.Contains(e.Name)) BuildFileCache.fileCache.Remove(e.Name);
            if (BuildFileCache.fileDirectory.Contains(e.Name)) BuildFileCache.fileDirectory.Remove(e.Name);
        }
        void fw_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            Thread.Sleep(5000); 
            Log(System.DateTime.Now.ToString(" MM/dd hh:mm:ss ") + ":  从缓存中修改 " + e.FullPath);  
            DelFromCache(e);
            BuildFile(e); 
        }
    }
}
