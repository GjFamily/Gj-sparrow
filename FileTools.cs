using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Gj
{
	public class FileTools
	{

		public static string createFolder (string path, string name)
		{
			string FolderPath = path + name;
			if (!Directory.Exists (FolderPath)) {
				Directory.CreateDirectory (FolderPath);
			}
			return FolderPath;
		}

		public static void saveFile (string path, string info, bool append = false)
		{
			StreamWriter sw;
			FileInfo t = new FileInfo (path);
			if (!t.Exists) {
				sw = t.CreateText ();
			} else {
				if (append) {
					sw = t.AppendText ();
				} else {
					return;
				}
			}
			sw.WriteLine (info);
			sw.Close ();
			sw.Dispose ();
		}

		public static void saveFile (string path, byte[] info, bool append = false)
		{
			StreamWriter sw;
			FileInfo t = new FileInfo (path);
			if (!t.Exists) {
				sw = t.CreateText ();
			} else {
				if (append) {
					sw = t.AppendText ();
				} else {
					return;
				}
			}
			sw.Write (info);
			sw.Close ();
			sw.Dispose ();
		}

		public static ArrayList loadFile (string path)
		{
			StreamReader sr = null;
			try {
				sr = File.OpenText (path);
			} catch (Exception e) {
				Debug.LogException (e);
				return null;
			}
			string line;
			ArrayList arrlist = new ArrayList ();
			while ((line = sr.ReadLine ()) != null) {
				arrlist.Add (line);
			}
			sr.Close ();
			sr.Dispose ();
			return arrlist;
		}

		public static int allFileSize (string path)
		{
			int sum = 0;
			if (!Directory.Exists (path)) {
				return 0;
			}

			DirectoryInfo dti = new DirectoryInfo (path);

			FileInfo[] fi = dti.GetFiles ();

			foreach (FileInfo f in fi) {

				sum += Convert.ToInt32 (f.Length / 1024);
			}

			DirectoryInfo[] di = dti.GetDirectories ();

			if (di.Length > 0) {
				for (int i = 0; i < di.Length; i++) {
					sum += allFileSize (di [i].FullName);
				}
			}
			return sum;
		}

		public static int fileSize (string path, string name)
		{
			int sum = 0;
			if (!Directory.Exists (path)) {
				return 0;
			} else {
				FileInfo Files = new FileInfo (@path + name);
				sum += Convert.ToInt32 (Files.Length / 1024);
			}
			return sum;
		}

		public static void deleteFile(string path, string name)
		{
			File.Delete(path + name);
		}

		public static void deleteFolder(string path, string name)
		{
			string FolderPath = path + name;

			if (Directory.Exists(FolderPath))
			{
				Directory.Delete(FolderPath);
			}
		}
	}
}