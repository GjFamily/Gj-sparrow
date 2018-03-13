using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Gj
{
	public class ModelTools
	{
		public static GameObject Create (GameObject prefab)
		{
			if (prefab != null) {
                GameObject obj = GameObject.Instantiate(prefab);
                obj.name = prefab.name;
                Debug.Log(obj.name);
                return obj;
			} else {
				return new GameObject ();
			}
		}

		public static GameObject Create (GameObject prefab, GameObject parent)
		{

			GameObject obj = Create (prefab);
			obj.transform.SetParent (parent.transform, false);
			return obj;
		}

		public static GameObject Create (GameObject prefab, Canvas parent)
		{
			GameObject obj = Create (prefab);
			obj.transform.SetParent (parent.transform, false);
			return obj;
		}

		public static void AddTriggersListener (GameObject obj, EventTriggerType eventID, UnityAction<BaseEventData> action)
		{
			EventTrigger trigger = obj.GetComponent<EventTrigger> ();
			if (trigger == null) {
				trigger = obj.AddComponent<EventTrigger> ();
			}

			if (trigger.triggers.Count == 0) {
				trigger.triggers = new List<EventTrigger.Entry> ();
			}

			UnityAction<BaseEventData> callback = new UnityAction<BaseEventData> (action);
			EventTrigger.Entry entry = new EventTrigger.Entry ();
			entry.eventID = eventID;
			entry.callback.AddListener (callback);
			trigger.triggers.Add (entry);
		}

		public enum BlendMode
		{
			Opaque,
			Cutout,
			Fade,
			Transparent
		}

		public static Material GetMaterial (GameObject obj)
		{
			return obj.GetComponent<Renderer> ().material;
		}

		public static void ChangeColor (GameObject obj, Color color)
		{
			Material material = GetMaterial (obj);
			material.color = color;
		}

		public static void ChangeAlpha (GameObject obj, float alpha)
		{
			Material material = GetMaterial (obj);
			Color color = material.color;
			color.a = alpha;
			material.color = color;
		}

		public static void ChangeRenderMode (GameObject obj, BlendMode blendMode)
		{
			Material material = GetMaterial (obj);
			switch (blendMode) {
			case BlendMode.Opaque:
				material.SetInt ("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt ("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				material.SetInt ("_ZWrite", 1);
				material.DisableKeyword ("_ALPHATEST_ON");
				material.DisableKeyword ("_ALPHABLEND_ON");
				material.DisableKeyword ("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = -1;
				break;
			case BlendMode.Cutout:
				material.SetInt ("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt ("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				material.SetInt ("_ZWrite", 1);
				material.EnableKeyword ("_ALPHATEST_ON");
				material.DisableKeyword ("_ALPHABLEND_ON");
				material.DisableKeyword ("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = 2450;
				break;
			case BlendMode.Fade:
				material.SetInt ("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				material.SetInt ("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				material.SetInt ("_ZWrite", 0);
				material.DisableKeyword ("_ALPHATEST_ON");
				material.EnableKeyword ("_ALPHABLEND_ON");
				material.DisableKeyword ("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = 3000;
				break;
			case BlendMode.Transparent:
				material.SetInt ("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt ("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				material.SetInt ("_ZWrite", 0);
				material.DisableKeyword ("_ALPHATEST_ON");
				material.DisableKeyword ("_ALPHABLEND_ON");
				material.EnableKeyword ("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = 3000;
				break;
			}
		}

		public static float ComputeRotaion (float value)
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