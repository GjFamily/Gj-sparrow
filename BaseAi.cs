using System.Collections.Generic;
using SimpleJSON;

namespace Gj
{
    public static class AI
    {
        public const string MODEL_LIST = "list";
        public const string BEHAVIOR_KEY = "key";
        public const string PREREQUISITES = "list";
        public const string PREREQUISITE_TYPE = "type";
        public const string PREREQUISITE_CATEGORY = "category";
        public const string PREREQUISITE_VALUE = "value";
    }


    public class Ai
    {
        public Ai(JSONObject json)
        {
            models = new List<AiModel>();
            foreach (JSONArray array in json[AI.MODEL_LIST].AsArray)
            {
                models.Add(new AiModel(array));
            }
        }
        public List<AiModel> models;
        private AiModelKey modelKey = AiModelKey.Standby;
        public void ChangeModel(AiModelKey aiModelKey)
        {
            modelKey = aiModelKey;
        }
        private AiModel GetModel()
        {
            return models[(int)modelKey];
        }

        public byte CheckBehavior(AiStatus status)
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
                        if (!status.Check(prerequisite))
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
            return aiBehavior.Value.key;
        }
    }

    public struct AiModel
    {
        public AiModel(JSONArray array)
        {
            behaviors = new List<AiBehavior>();
            foreach (JSONObject json in array)
            {
                behaviors.Add(new AiBehavior(json));
            }
        }
        public List<AiBehavior> behaviors;
    }

    public struct AiBehavior
    {
        public AiBehavior(JSONObject json)
        {
            key = (byte)json[AI.BEHAVIOR_KEY].AsInt;
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
        public ObjectAttr attr;
        public bool Check(AiPrerequisite prerequisite)
        {
            return false;
        }
    }

    public enum AiModelKey
    {
        Standby = 0,
        Attack = 1
    }
}