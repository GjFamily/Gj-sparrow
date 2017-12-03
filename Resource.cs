using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Gj
{
	public class Resource
	{

		public static Dictionary<string, AudioClip> audioClipMap = new Dictionary<string, AudioClip> ();
		public static Dictionary<string, Texture> textureMap = new Dictionary<string, Texture> ();

		public static Texture getTexture (string tag)
		{
			if (!textureMap.ContainsKey (tag)) {
				textureMap.Add (tag, Resources.Load<Texture> ("texture/" + tag));
			}
			return textureMap [tag];
		}

		public static AudioClip getAudioClip (string tag)
		{
			if (!audioClipMap.ContainsKey (tag)) {
				audioClipMap.Add (tag, Resources.Load<AudioClip> ("audio/" + tag));
			}
			return audioClipMap [tag];
		}
	}
}