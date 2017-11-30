﻿using System;
using System.Text;
using System.Collections;
using System.Security.Cryptography;
using Random = UnityEngine.Random;

namespace Gj
{
	public class Tools
	{
		public static string generateStr (int Length)
		{
			char[] chars = {
				'0',
				'1',
				'2',
				'3',
				'4',
				'5',
				'6',
				'7',
				'8',
				'9',
				'a',
				'b',
				'c',
				'd',
				'e',
				'f',
				'g',
				'h',
				'i',
				'j',
				'k',
				'l',
				'm',
				'n',
				'o',
				'p',
				'q',
				'r',
				's',
				't',
				'u',
				'v',
				'w',
				'x',
				'y',
				'z',
				'A',
				'B',
				'C',
				'D',
				'E',
				'F',
				'G',
				'H',
				'I',
				'J',
				'K',
				'L',
				'M',
				'N',
				'O',
				'P',
				'Q',
				'R',
				'S',
				'T',
				'U',
				'V',
				'W',
				'X',
				'Y',
				'Z'
			};

			System.Text.StringBuilder newRandom = new System.Text.StringBuilder (62);
			for (int i = 0; i < Length; i++) {
				newRandom.Append (chars [Random.Range (0, 62)]);
			}
			return newRandom.ToString ();
		}

		public static string generateStr (string[] chars, int Length)
		{
			int count = chars.Length;
			System.Text.StringBuilder newRandom = new System.Text.StringBuilder (count);

			for (int i = 0; i < Length; i++) {
				newRandom.Append (chars [Random.Range (0, count)]);
			}
			return newRandom.ToString ();
		}

		public static string GenerateNum (int Length)
		{
			char[] chars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

			System.Text.StringBuilder newRandom = new System.Text.StringBuilder (10);
			for (int i = 0; i < Length; i++) {
				newRandom.Append (chars [Random.Range (0, 10)]);
			}
			return newRandom.ToString ();
		}

		public static string[] stringToList (string str)
		{
			return str.Split ('*');
		}

		public static string md5 (string str)
		{
			string pwd = "";
			MD5 md5 = MD5.Create ();
			byte[] s = md5.ComputeHash (Encoding.UTF8.GetBytes (str));
			for (int i = 0; i < s.Length; i++) {
				pwd = pwd + s [i].ToString ();
			}
			return pwd;
		}

		public static float computeRotaion (float value)
		{
			if (value > 360) {
				return value - 360;
			} else if (value < 0) {
				return value + 360;
			} else {
				return value;
			}
		}
	}
}