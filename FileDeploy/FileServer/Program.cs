using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Threading;
using System.Runtime.Remoting.Channels.Tcp;
using FileLibrary;

namespace FileServer
{
    class Program
    {
        static System.IO.FileSystemWatcher fw = null;
        static void Main(string[] args)
        {
            try
            {
                int port = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["port"]);

                ChannelServices.RegisterChannel(new TcpChannel(port), false);
                RemotingConfiguration.RegisterWellKnownServiceType(typeof(FileLibrary.FileServer), "fileService", WellKnownObjectMode.Singleton);

                //MakeDBFile.StartMonitor(strConn, intervalTime);

                fw = new System.IO.FileSystemWatcher();
                fw.Path = BuildFileCache.BasePath;
                fw.IncludeSubdirectories = true;
                fw.EnableRaisingEvents = true;
                fw.Deleted += new System.IO.FileSystemEventHandler(fw_Deleted);
                fw.Created += new System.IO.FileSystemEventHandler(fw_Created);
                fw.Changed+=fw_Changed;
            
                //启动即加载
                BuildFileCache.fileCache.Clear();
                BuildFileCache.fileDirectory.Clear();
                Console.WriteLine("清除文件完毕!");
                Console.WriteLine("加载所有文件中...");
                BuildFileCache.BuildAll();
                while (true)
                {
                    Console.WriteLine("...");
                    Console.WriteLine("请选择以下一项操作：");
                    Console.WriteLine("1.加载所有文件。");
                    Console.WriteLine("2.显示访问日志。");
                    Console.WriteLine("3.显示文件信息。");
                    Console.WriteLine("4.清除屏幕。");
                    Console.WriteLine("5.清除日志。");
                    Console.WriteLine("6.清除文件。");
                    Console.WriteLine("7.退出");
                    Console.Write("请选择以上一项操作 ：");
                    string control = Console.ReadLine();
                    switch (control)
                    {
                        case "1":
                            Console.WriteLine("加载所有文件中...");
                            BuildFileCache.BuildAll();
                            break;
                        case "2":
                            BuildFileCache.ShowLog();
                            break;
                        case "3":
                            BuildFileCache.ShowFileInfo();
                            break;
                        case "4":
                            Console.Clear();
                            break;
                        case "5":
                            BuildFileCache.log.Clear();
                            Console.WriteLine("清除日志完毕!");
                            break;
                        case "6":
                            BuildFileCache.fileCache.Clear();
                            BuildFileCache.fileDirectory.Clear();
                            Console.WriteLine("清除文件完毕!");
                            break;
                        case "7":
                            Console.Write("是否确认退出系统？ Y - 退出 N - 继续 :");
                            string c = Console.ReadLine();
                            switch (c)
                            {
                                case "Y":
                                    Console.WriteLine("系统退出中...");
                                    System.Threading.Thread.Sleep(5000);
                                    return;
                                default:
                                    break;
                            }
                            break;
                        default:
                            Console.WriteLine("'" + control + "' 不是内部或外部命令，也不是可运行的程序或批处理文件。");
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }

        }

        static void fw_Created(object sender, System.IO.FileSystemEventArgs e)
        {
            Thread.Sleep(5000);
            BuildFile(e);
        }

        private static void BuildFile(System.IO.FileSystemEventArgs e)
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
        public static void fw_Deleted(object sender, System.IO.FileSystemEventArgs e)
        {
            Console.WriteLine(System.DateTime.Now.ToString(" MM/dd hh:mm:ss ") + ":  从缓存中删除 " + e.FullPath);
            if (BuildFileCache.fileCache.Contains(e.Name)) BuildFileCache.fileCache.Remove(e.Name);
            if (BuildFileCache.fileDirectory.Contains(e.Name)) BuildFileCache.fileDirectory.Remove(e.Name);
        }
        public static void fw_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            Thread.Sleep(5000);
            ////fw.EnableRaisingEvents = false;
            fw_Deleted(sender, e);
            BuildFile(e);
            //fw.EnableRaisingEvents = true;
        }
    }
}
