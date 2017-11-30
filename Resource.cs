using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Gj
{
	public class Resource
	{

		public Dictionary<string, AudioClip> audioClipMap = new Dictionary<string, AudioClip> ();
		public Dictionary<string, Texture> textureMap = new Dictionary<string, Texture> ();

		public Texture getTexture (string tag)
		{
			if (!textureMap.ContainsKey (tag)) {
				textureMap.Add (tag, Resources.Load<Texture> ("texture/" + tag));
			}
			return textureMap [tag];
		}

		public AudioClip getAudioClip (string tag)
		{
			if (!audioClipMap.ContainsKey (tag)) {
				audioClipMap.Add (tag, Resources.Load<AudioClip> ("audio/" + tag));
			}
			return audioClipMap [tag];
		}
	}
}