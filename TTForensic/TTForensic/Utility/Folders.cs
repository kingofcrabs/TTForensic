﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TTForensic.Utility
{
    class Folders
    {
        static public string GetExeFolder()
        {
            string s = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return s + "\\";
        }
        static public string GetLastRunInfoFile()
        {
            return GetExeFolder() + "lastRunInfo.xml";
        }

        static public void WriteResult(bool bok)
        {
            string file = GetOutputFolder() + "result.txt";
            File.WriteAllText(file, bok.ToString());
        }

        static public string GetExeParentFolder()
        {
            string s = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            int index = s.LastIndexOf("\\");
            return s.Substring(0, index) + "\\";
        }

        public static string GetOutputFolder()
        {
            string sOutputFolder = GetExeParentFolder() + "Output\\";

            if (!Directory.Exists(sOutputFolder))
            {
                Directory.CreateDirectory(sOutputFolder);
            }
            return sOutputFolder;
        }

        static public string GetDataFolder()
        {
            string sDataFolder = GetExeParentFolder() + "Data\\";
            if (!Directory.Exists(sDataFolder))
                Directory.CreateDirectory(sDataFolder);
            return sDataFolder;
        }

        public static string GetImageFolder()
        {
            return GetExeParentFolder() + "Images\\";
        }

        internal static string GetProtocolDefinitionXml()
        {
            return GetDataFolder() + "protocol1.xml";
        }

        internal static void WriteVariable(string file, string s)
        {
            string filePath = GetOutputFolder() + file + ".txt";
            File.WriteAllText(filePath, s);
        }

        internal static void WriteRunInfo(string info)
        {
            string filePath = GetOutputFolder() + "history.txt";
            string timeStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            File.AppendAllText(filePath, timeStr + "  " + info + "\r\n");
        }

        internal static string GetBackupFolder()
        {
            string backupFolder = GetExeParentFolder() + "backup\\";
            if (!Directory.Exists(backupFolder))
                Directory.CreateDirectory(backupFolder);
            backupFolder += string.Format(DateTime.Now.ToString("yyMMddHHmmss"));
            if (!Directory.Exists(backupFolder))
                Directory.CreateDirectory(backupFolder);
            return backupFolder;
        }

        internal static void Backup()
        {
            string sBackup = GetBackupFolder();
            string soutPut = GetOutputFolder();
            CopyFolder(soutPut, sBackup);
        }

        public static void CopyFolder(string strFromPath, string strToPath)
        {
            //如果源文件夹不存在，则创建
            if (!Directory.Exists(strFromPath))
            {
                Directory.CreateDirectory(strFromPath);
            }
            //取得要拷贝的文件夹名
            string strFolderName = strFromPath.Substring(strFromPath.LastIndexOf("\\") +
               1, strFromPath.Length - strFromPath.LastIndexOf("\\") - 1);
            //如果目标文件夹中没有源文件夹则在目标文件夹中创建源文件夹
            if (!Directory.Exists(strToPath + "\\" + strFolderName))
            {
                Directory.CreateDirectory(strToPath + "\\" + strFolderName);
            }
            //创建数组保存源文件夹下的文件名
            string[] strFiles = Directory.GetFiles(strFromPath);
            //循环拷贝文件
            for (int i = 0; i < strFiles.Length; i++)
            {
                //取得拷贝的文件名，只取文件名，地址截掉。
                string strFileName = strFiles[i].Substring(strFiles[i].LastIndexOf("\\") + 1, strFiles[i].Length - strFiles[i].LastIndexOf("\\") - 1);
                //开始拷贝文件,true表示覆盖同名文件
                File.Copy(strFiles[i], strToPath + "\\" + strFolderName + "\\" + strFileName, true);
            }
            //创建DirectoryInfo实例
            DirectoryInfo dirInfo = new DirectoryInfo(strFromPath);
            //取得源文件夹下的所有子文件夹名称
            DirectoryInfo[] ZiPath = dirInfo.GetDirectories();
            for (int j = 0; j < ZiPath.Length; j++)
            {
                //获取所有子文件夹名
                string strZiPath = strFromPath + "\\" + ZiPath[j].ToString();
                //把得到的子文件夹当成新的源文件夹，从头开始新一轮的拷贝
                CopyFolder(strZiPath, strToPath + "\\" + strFolderName);
            }
        }
    }
}
