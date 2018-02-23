using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using Gj.Galaxy.Logic;
using System;

namespace Gj.Galaxy.Utils{
    public static class Extensions
    {
        public static Dictionary<MethodInfo, ParameterInfo[]> ParametersOfMethods = new Dictionary<MethodInfo, ParameterInfo[]>();
        public static ParameterInfo[] GetCachedParemeters(this MethodInfo mo)
        {
            ParameterInfo[] result;
            bool cached = ParametersOfMethods.TryGetValue(mo, out result);

            if (!cached)
            {
                result = mo.GetParameters();
                ParametersOfMethods[mo] = result;
            }

            return result;
        }

        public static NetworkEntity[] GetEntitysInChildren(this UnityEngine.GameObject go)
        {
            return go.GetComponentsInChildren<NetworkEntity>(true) as NetworkEntity[];
        }

        public static NetworkEntity GetEntity(this UnityEngine.GameObject go)
        {
            return go.GetComponent<NetworkEntity>() as NetworkEntity;
        }

        /// <summary>compares the squared magnitude of target - second to given float value</summary>
        public static bool AlmostEquals(this Vector3 target, Vector3 second, float sqrMagnitudePrecision)
        {
            return (target - second).sqrMagnitude < sqrMagnitudePrecision;  // TODO: inline vector methods to optimize?
        }

        public static bool AlmostEquals(this Vector2 target, Vector2 second, float sqrMagnitudePrecision)
        {
            return (target - second).sqrMagnitude < sqrMagnitudePrecision;  // TODO: inline vector methods to optimize?
        }

        public static bool AlmostEquals(this Quaternion target, Quaternion second, float maxAngle)
        {
            return Quaternion.Angle(target, second) < maxAngle;
        }

        public static bool AlmostEquals(this float target, float second, float floatDiff)
        {
            return Mathf.Abs(target - second) < floatDiff;
        }

        public static void Merge(this IDictionary target, IDictionary addHash)
        {
            if (addHash == null || target.Equals(addHash))
            {
                return;
            }

            foreach (object key in addHash.Keys)
            {
                target[key] = addHash[key];
            }
        }

        public static void MergeStringKeys(this IDictionary target, IDictionary addHash)
        {
            if (addHash == null || target.Equals(addHash))
            {
                return;
            }

            foreach (object key in addHash.Keys)
            {
                // only merge keys of type string
                if (key is string)
                {
                    target[key] = addHash[key];
                }
            }
        }

        public static string ToStringFull(this object[] data)
        {
            if (data == null) return "null";

            string[] sb = new string[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                object o = data[i];
                sb[i] = (o != null) ? o.ToString() : "null";
            }

            return string.Join(", ", sb);
        }

        public static string ToStringFull(this IDictionary data)
        {
            if (data == null) return "null";

            string[] sb = new string[data.Count];
            for (int i = 0; i < data.Count; i++)
            {
                object o = data[i];
                sb[i] = (o != null) ? o.ToString() : "null";
            }

            return string.Join(", ", sb);
        }

        public static void StripKeysWithNullValues(this IDictionary original)
        {
            object[] keys = new object[original.Count];
            int i = 0;
            foreach (object k in original.Keys)
            {
                keys[i++] = k;
            }

            for (int index = 0; index < keys.Length; index++)
            {
                var key = keys[index];
                if (original[key] == null)
                {
                    original.Remove(key);
                }
            }
        }

        public static bool Contains(this int[] target, int nr)
        {
            if (target == null)
            {
                return false;
            }

            for (int index = 0; index < target.Length; index++)
            {
                if (target[index] == nr)
                {
                    return true;
                }
            }

            return false;
        }

    }
    public static class ReflectClass{
        public static bool CheckTypeMatch(ParameterInfo[] methodParameters, Type[] callParameterTypes)
        {
            if (methodParameters.Length < callParameterTypes.Length)
            {
                return false;
            }

            for (int index = 0; index < callParameterTypes.Length; index++)
            {
#if NETFX_CORE
            TypeInfo methodParamTI = methodParameters[index].ParameterType.GetTypeInfo();
            TypeInfo callParamTI = callParameterTypes[index].GetTypeInfo();

            if (callParameterTypes[index] != null && !methodParamTI.IsAssignableFrom(callParamTI) && !(callParamTI.IsEnum && System.Enum.GetUnderlyingType(methodParamTI.AsType()).GetTypeInfo().IsAssignableFrom(callParamTI)))
            {
                return false;
            }
#else
                Type type = methodParameters[index].ParameterType;
                if (callParameterTypes[index] != null && !type.IsAssignableFrom(callParameterTypes[index]) && !(type.IsEnum && System.Enum.GetUnderlyingType(type).IsAssignableFrom(callParameterTypes[index])))
                {
                    return false;
                }
#endif
            }

            return true;
        }

        public static bool GetMethod(MonoBehaviour monob, string methodType, out MethodInfo mi)
        {
            mi = null;

            if (monob == null || string.IsNullOrEmpty(methodType))
            {
                return false;
            }

            var t = monob.GetType();
            //var methods = new List<MethodInfo>();
            var methods = t.GetMethods();
            //methods.InsertRange(0, t.GetMethods());
            for (int index = 0; index < methods.Length; index++)
            {
                MethodInfo methodInfo = methods[index];
                if (methodInfo.Name.Equals(methodType))
                {
                    mi = methodInfo;
                    return true;
                }
            }

            return false;
        }
    }

    public static class GameObjectExtensions
    {
        public static bool GetActive(this GameObject target)
        {
            return target.activeInHierarchy;
        }
    }

}
