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
		public static GameObject create (GameObject prefab)
		{
			if (prefab != null) {
				return GameObject.Instantiate (prefab);
			} else {
				return new GameObject ();
			}
		}

		public static GameObject create (GameObject prefab, GameObject parent)
		{

			GameObject obj = create (prefab);
			obj.transform.SetParent (parent.transform, false);
			return obj;
		}

		public static GameObject create (GameObject prefab, Canvas parent)
		{
			GameObject obj = create (prefab);
			obj.transform.SetParent (parent.transform, false);
			return obj;
		}

		public static void addTriggersListener (GameObject obj, EventTriggerType eventID, UnityAction<BaseEventData> action)
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

		public static Material getMaterial (GameObject obj)
		{
			return obj.GetComponent<Renderer> ().material;
		}

		public static void changeColor (GameObject obj, Color color)
		{
			Material material = getMaterial (obj);
			material.color = color;
		}

		public static void changeAlpha (GameObject obj, float alpha)
		{
			Material material = getMaterial (obj);
			Color color = material.color;
			color.a = alpha;
			material.color = color;
		}

		public static void changeRenderMode (GameObject obj, BlendMode blendMode)
		{
			Material material = getMaterial (obj);
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