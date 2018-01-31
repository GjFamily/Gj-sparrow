using System;
using System.IO;
using System.Collections;
using UnityEngine;

namespace Gj
{
    public class FileTools
	{

		public static string CreateFolder (string path, string name)
		{
			string FolderPath = path + name;
			if (!Directory.Exists (FolderPath)) {
				Directory.CreateDirectory (FolderPath);
			}
			return FolderPath;
		}

		public static void SaveFile (string path, string info, bool append = false)
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

		public static void SaveFile (string path, byte[] info, bool append = false)
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

		public static ArrayList LoadFile (string path)
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

		public static int AllFileSize (string path)
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
					sum += AllFileSize (di [i].FullName);
				}
			}
			return sum;
		}

		public static int FileSize (string path, string name)
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

		public static void DeleteFile(string path, string name)
		{
			File.Delete(path + name);
		}

		public static void DeleteFolder(string path, string name)
		{
			string FolderPath = path + name;

			if (Directory.Exists(FolderPath))
			{
				Directory.Delete(FolderPath);
			}
		}
	}
}