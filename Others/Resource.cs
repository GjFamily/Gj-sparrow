using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Gj
{
    public static class Resource
    {

        public static Dictionary<string, AudioClip> audioClipMap = new Dictionary<string, AudioClip>();
        public static Dictionary<string, Texture> textureMap = new Dictionary<string, Texture>();

        public static Texture GetTexture(string tag)
        {
            if (!textureMap.ContainsKey(tag))
            {
                textureMap.Add(tag, Resources.Load<Texture>("texture/" + tag));
            }
            return textureMap[tag];
        }

        public static AudioClip GetAudioClip(string tag)
        {
            if (!audioClipMap.ContainsKey(tag))
            {
                audioClipMap.Add(tag, Resources.Load<AudioClip>("audio/" + tag));
            }
            return audioClipMap[tag];
        }

        public static Dictionary<string, string> GetLanguage(string language)
        {
            TextAsset ta = Resources.Load<TextAsset>("language/" + language);
            string text = ta.text;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string[] lines = text.Split('\n');
            foreach (string line in lines)
            {
                if (line == null)
                {
                    continue;
                }
                string[] keyAndValue = line.Split('=');
                dic.Add(keyAndValue[0], keyAndValue[1]);
            }

            return dic;
        }
    }
}