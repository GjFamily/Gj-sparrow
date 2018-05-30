using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace Gj
{
    public static class AI
    {
        public const string MODEL_LIST = "list";
        public const string MODEL_RADIUS = "radius";
        public const string BEHAVIOR_LIST = "list";
        public const string BEHAVIOR_KEY = "key";
        public const string BEHAVIOR_TIME = "time";
        public const string PREREQUISITES = "list";
        public const string PREREQUISITE_TYPE = "type";
        public const string PREREQUISITE_CATEGORY = "category";
        public const string PREREQUISITE_VALUE = "value";
    }

    public class AiBrain
    {
        public AiBrain(JSONObject json)
        {
            models = new List<AiModel>();
            foreach (JSONObject model in json[AI.MODEL_LIST].AsArray)
            {
                models.Add(new AiModel(model));
            }
        }

        public List<AiModel> models;
        private AiModelKey modelKey = AiModelKey.Standby;

        public void ChangeModel(AiModelKey aiModelKey)
        {
            modelKey = aiModelKey;
        }

        public AiModel GetModel()
        {
            return models[(int)modelKey];
        }

        public AiBehavior CheckBehavior(AiStatus aiStatus)
        {
            AiBehavior? aiBehavior = null;
            List<AiBehavior> aiBehaviors = GetModel().behaviors;
            foreach (AiBehavior behavior in aiBehaviors)
            {
                if (aiBehavior != null) break;
                foreach (List<AiPrerequisite> prerequisites in behavior.prerequisites)
                {
                    bool check = true;
                    foreach (AiPrerequisite prerequisite in prerequisites)
                    {
                        if (!aiStatus.Check(prerequisite))
                        {
                            check = false;
                            break;
                        }
                    }
                    if (check)
                    {
                        aiBehavior = behavior;
                        break;
                    }
                }
            }
            if (aiBehavior == null)
            {
                aiBehavior = aiBehaviors[aiBehaviors.Count - 1];
            }
            return aiBehavior.Value;
        }

        public AiCommand FormatCommand(AiBehavior aiBehavior) {
            byte type = aiBehavior.key;
            byte category = 0;
            float vale = 0;
            return new AiCommand(type, category, vale);
        }
    }

    public struct AiModel
    {
        public AiModel(JSONObject json)
        {
            behaviors = new List<AiBehavior>();
            foreach (JSONObject behavior in json[AI.BEHAVIOR_LIST].AsArray)
            {
                behaviors.Add(new AiBehavior(behavior));
            }
            radius = json[AI.MODEL_RADIUS];
        }
        public List<AiBehavior> behaviors;
        public float radius;
    }

    public struct AiCommand
    {
        public AiCommand(byte t, byte c, float v)
        {
            type = t;
            category = c;
            value = v;
        }
        public byte type;
        public byte category;
        public float value;
    }

    public struct AiBehavior
    {
        public AiBehavior(JSONObject json)
        {
            key = (byte)json[AI.BEHAVIOR_KEY].AsInt;
            time = json[AI.BEHAVIOR_TIME].AsFloat;
            prerequisites = new List<List<AiPrerequisite>>();
            foreach (JSONArray array in json[AI.PREREQUISITES].AsArray)
            {
                List<AiPrerequisite> _prerequisites = new List<AiPrerequisite>();
                foreach (JSONObject prerequisite in array)
                {
                    _prerequisites.Add(new AiPrerequisite(prerequisite));
                }
                prerequisites.Add(_prerequisites);
            }
        }
        public byte key;
        public float time;
        public List<List<AiPrerequisite>> prerequisites;
    }

    public struct AiPrerequisite
    {
        public AiPrerequisite(JSONObject json)
        {
            type = (byte)json[AI.PREREQUISITE_TYPE].AsInt;
            category = (byte)json[AI.PREREQUISITE_CATEGORY].AsInt;
            value = json[AI.PREREQUISITE_VALUE].AsFloat;
        }
        public byte type;
        public byte category;
        public float value;
    }

    public class AiStatus
    {
        public AiStatus(ObjectAttr objectAttr)
        {
            attr = objectAttr;
        }

        public ObjectAttr attr;
        public GameObject target;
        public GameObject nearestTarget;
        public Dictionary<string, float> skillTime;
        public List<GameObject> teammate;
        public List<GameObject> enemy;
        public List<GameObject> partner;
        public List<GameObject> safe;
        public List<GameObject> attack;
        public List<GameObject> beAttack;
        public float lastHitTime;
        public AiBehavior? lastBehavior;
        public float lastBehaviorTime;

        public bool Check(AiPrerequisite prerequisite)
        {
            switch (prerequisite.type)
            {
                default:
                    break;
            }
            return false;
        }

        public bool IsFree() {
            if (lastBehavior != null) {
                if (Time.time > lastBehavior.Value.time + lastBehaviorTime) {
                    return true;
                }
            }
            return true;
        }
    }

    public enum AiModelKey
    {
        Standby = 0,
        Attack = 1,
        Defense = 2,
        Escape = 3
    }
}