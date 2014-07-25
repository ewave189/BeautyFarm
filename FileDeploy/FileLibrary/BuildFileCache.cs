using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;

namespace FileLibrary
{
    public class BuildFileCache
    {
        /// <summary>
        /// 放程序的基本路径
        /// </summary>
        public static string BasePath = AppDomain.CurrentDomain.BaseDirectory + "AppDomain";
        public static Hashtable fileCache = new Hashtable();
        public static Hashtable fileDirectory = new Hashtable();
        public static List<string> log = new List<string>();
        public static List<string> Errors = new List<string>();

        public static void ShowFileInfo()
        {
            System.Collections.IDictionaryEnumerator g = fileDirectory.GetEnumerator();
            while (g.MoveNext())
            {
                Console.WriteLine(g.Key.ToString() + "  " + g.Value.ToString());
            }
            Console.WriteLine("Total Files :" + fileCache.Count.ToString());
        }

        public static void ShowLog()
        {
            foreach (string l in log)
            {
                Console.WriteLine(l);
            }
        }

        public static void BuildAll()
        {
            if (!Directory.Exists(BasePath)) { Console.WriteLine(" 没有找到路径 : " + BasePath); return; }
       
            List<string> temp = new List<string>();
            System.Collections.IDictionaryEnumerator g = fileDirectory.GetEnumerator();
            while (g.MoveNext())
            {
                FileStructure f = g.Value as FileStructure;
                if (System.IO.File.Exists(BasePath + f.path))
                {
                    temp.Add(g.Key.ToString());
                }
            }
            foreach (string k in temp)
            {
                Console.WriteLine("no found this file and remove from cache:" + k);
                fileCache.Remove(k);
                fileDirectory.Remove(k);
            }

            foreach (String _File in Directory.GetFiles(BasePath, "*.*", SearchOption.AllDirectories))
            {
                FileInfo fileInfo = new FileInfo(_File);
                if (fileDirectory.Contains(fileInfo.Name))
                {
                    if (fileDirectory[fileInfo.Name].ToString() == fileInfo.LastWriteTime.ToString())
                    {
                        continue;
                    }
                    else
                    {
                        fileDirectory.Remove(fileInfo.Name);
                        fileCache.Remove(fileInfo.Name);
                    }
                }
                FileStructure fs = new FileStructure();
                Console.Write(System.DateTime.Now.ToString(" MM/dd hh:mm:ss ") + ": 压缩到缓存  " + fileInfo.Name);
                fs.path = fileInfo.Directory.FullName.Substring(BasePath.Length);
                fs.stream = CompressionFile(_File);
                fileCache.Add(fileInfo.Name, fs);
                fileDirectory.Add(fileInfo.Name, fileInfo.LastWriteTime.ToString());
            }
            Console.WriteLine("文件数：" + fileDirectory.Count.ToString() + "所有文件大小:" + totalsize.ToString() + " ,  压缩后:" + compresssize.ToString() + ", 压缩比:" + Convert.ToDouble(compresssize) / Convert.ToDouble(totalsize));
        }

        public static void BuildFile(string fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            if (!fileInfo.Exists) return;
            FileStructure fs = new FileStructure();
            Console.WriteLine(System.DateTime.Now.ToString(" MM/dd hh:mm:ss ") + ":  压缩到缓存 " + fileInfo.Name);
            fs.path = fileInfo.Directory.Name.Remove(0, 9);
            fs.stream = CompressionFile(fileName);
            if (fileCache.Contains(fileInfo.Name)) fileCache.Remove(fileInfo.Name);
            fileCache.Add(fileInfo.Name, fs);
            if (fileDirectory.Contains(fileInfo.Name)) fileDirectory.Remove(fileInfo.Name);
            fileDirectory.Add(fileInfo.Name, fileInfo.LastWriteTime.ToString());
        }

       public    static int totalsize = 0;
       public static int compresssize = 0;

        static void CheckFileShare(string FileName)
        {
            try
            {
                new System.IO.FileInfo(FileName);
            }
            catch (System.IO.IOException e)
            {
                Console.Error.WriteLine(e.Message);
                System.Threading.Thread.Sleep(1000);
                CheckFileShare(FileName);
            }
        }

        public static byte[] CompressionFile(string FileName)
        {
            CheckFileShare(FileName);
            MemoryStream fileStream = new MemoryStream();
            byte[] getchr = System.IO.File.ReadAllBytes(FileName);
            totalsize += getchr.Length;
            Console.Write(" size:" + getchr.Length);
            ZipOutputStream output = new ZipOutputStream(fileStream);
            output.SetLevel(8);
            ZipEntry entry = new ZipEntry(FileName);
            Crc32 crc = new Crc32();
            crc.Reset();
            crc.Update(getchr);
            entry.Crc = crc.Value;
            entry.DateTime = DateTime.Now;
            entry.Size = getchr.Length;
            output.PutNextEntry(entry);
            output.Write(getchr, 0, getchr.Length);
            output.Finish();
            output.Close();
            byte[] array = fileStream.ToArray();
            compresssize += array.Length;
            Console.Write(" / " + array.Length);
            Console.WriteLine();
            return array;
        }
    }
}
