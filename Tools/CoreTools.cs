using System;
using System.Reflection;
using System.Text;
using System.Collections;
using System.Security.Cryptography;
using Random = UnityEngine.Random;
using UnityEngine;

namespace Gj
{
    public static class CoreTools
    {
        public static bool IsTarget (GameObject obj) {
            Info info = GetInfo(obj);
            return info.IsTarget();
        }

        public static Info GetInfo(GameObject obj)
        {
            return obj.GetComponent<Info>();
        }

        public static float GetAttribute(GameObject obj, string key)
        {
            Info info = GetInfo(obj);
            if (info != null)
            {
                return info.GetAttribute(key);
            }
            return 0;
        }

        public static void SetAttribute(GameObject obj, string key, float value)
        {
            Info info = GetInfo(obj);
            if (info != null)
            {
                info.SetAttribute(key, value);
            }
        }

        public static GameObject GetMaster(GameObject obj)
        {
            if (obj.GetComponent<BeLongPart>() != null)
            {
                return obj.GetComponent<BeLongPart>().GetMaster();
            }
            else
            {
                return obj;
            }
        }

        public static void BindPart(Component c, GameObject t)
        {
            foreach (object attributes in c.GetType().GetCustomAttributes(typeof(RequirePart), true))
            {
                RequirePart requirePart = attributes as RequirePart;
                if (null != requirePart)
                {
                    if (t.GetComponent(requirePart.part) == null)
                    {
                        t.AddComponent(requirePart.part);
                    }
                }
            }
        }

        public static void AddFeature(Component c, GameObject t)
        {
            foreach (FieldInfo fieldInfo in c.GetType().GetFields())
            {
                if (fieldInfo.FieldType == typeof(GameObject))
                {
                    GameObject obj = fieldInfo.GetValue(c) as GameObject;
                    foreach (object attributes in fieldInfo.GetCustomAttributes(typeof(AddFeature), false))
                    {
                        AddFeature addFeature = attributes as AddFeature;
                        if (null != addFeature)
                        {
                            if (t.GetComponent(addFeature.feature) == null)
                            {
                                if (addFeature.prefab)
                                {
                                    obj = ModelTools.Create(obj, t);
                                }
                                BaseFeature baseFeature = t.AddComponent(addFeature.feature) as BaseFeature;

                                if (obj != null)
                                {
                                    baseFeature.Feature = obj;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void BindFeature(Component c, GameObject t)
        {
            foreach (object attributes in c.GetType().GetCustomAttributes(typeof(RequireFeature), true))
            {
                RequireFeature requireFeature = attributes as RequireFeature;
                if (null != requireFeature)
                {
                    if (t.GetComponent(requireFeature.feature) == null)
                    {
                        GameObject obj = ModelTools.Create(null, t);

                        BaseFeature baseFeature = t.AddComponent(requireFeature.feature) as BaseFeature;

                        if (obj != null)
                        {
                            obj.name = requireFeature.feature.Name;
                            baseFeature.Feature = obj;
                        }
                    }
                }
            }
        }

        public static AllowSync AllowSync(Component c, GameObject t)
        {
            foreach (object attributes in c.GetType().GetCustomAttributes(typeof(AllowSync), true))
            {
                AllowSync allowSync = attributes as AllowSync;
                if (null != allowSync)
                {
                    allowSync.Register(c, t);
                    return allowSync;
                }
            }
            return null;
        }

        public static void InfoSync(Component c, GameObject t)
        {
            foreach (FieldInfo fieldInfo in c.GetType().GetFields())
            {
                if (fieldInfo.FieldType == typeof(GameObject))
                {
                    GameObject obj = fieldInfo.GetValue(c) as GameObject;
                    foreach (object attributes in fieldInfo.GetCustomAttributes(typeof(InfoSync), false))
                    {
                        InfoSync infoSync = attributes as InfoSync;
                        if (null != infoSync)
                        {
                            infoSync.Register(c, t, obj);
                        }
                    }
                }
            }
        }
    }
}