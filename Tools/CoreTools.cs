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
            if (info == null) return false;
            return info.IsTarget();
        }

        public static Info GetInfo(GameObject obj)
        {
            return obj.GetComponent<Info>();
        }

        public static GameObject GetMaster(GameObject obj)
        {
            Info info = GetInfo(obj);
            if (info != null && info.master != null)
            {
                return info.master;
            }
            else
            {
                return obj;
            }
        }

        public static Info GetMasterInfo(GameObject obj)
        {
            return GetInfo(GetMaster(obj));
        }

        public static bool AllowTarget(Skill skill, GameObject master, GameObject target)
        {
            if (skill.targetRelation == TargetRelation.Self)
            {
                return master == target;
            }
            else
            {
                Info info = GetInfo(master);
                if (info == null) return false;
                if (skill.targetRelation == TargetRelation.Partner)
                {
                    return info.IsPartner(target);
                }
                else if (skill.targetRelation == TargetRelation.Enemy)
                {
                    return info.IsEnemy(target);
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool AllowRange(Skill skill, GameObject master, GameObject target)
        {
            return AllowRange(skill, master, target.transform);
        }

        public static bool AllowRange(Skill skill, GameObject master, Transform transform)
        {
            if (skill.range <= 0) return true;
            return Vector3.Distance(master.transform.position, transform.position) <= skill.range;
        }

        public static bool IsEnv(GameObject obj)
        {
            return obj.tag == "Env";
        }

        public static bool AllowCollision(GameObject obj)
        {
            if (IsEnv(obj))
            {
                return true;
            }
            else
            {
                Info info = GetInfo(obj);
                if (info != null && info.HaveBody())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}