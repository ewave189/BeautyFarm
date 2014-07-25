using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace FileLibrary
{
    public class FileServer : MarshalByRefObject
    {
        public Hashtable GetfileDirectory()
        {
            return BuildFileCache.fileDirectory;
        }

        public FileStructure DownloadFile(string fileName, string Ip)
        {
            if (!BuildFileCache.fileDirectory.Contains(fileName)) return null;
            FileStructure fs = BuildFileCache.fileCache[fileName] as FileStructure;
            BuildFileCache.log.Add(System.DateTime.Now.ToString(" MM/dd hh:mm:ss ") + ":  " + Ip + " 下载 " + fileName);
            return fs;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }

    [Serializable]
    public class FileStructure
    {
        public string path;
        public byte[] stream;
    }
}
