using System.Collections.Generic;
using SimpleJSON;

namespace Gj
{
    public interface UISystem
    {
        void UIClick(int key);
    }

    public interface TargetEntity
    {
        void Idle();
        void Run();
        void Walk();
        void Talk();
        void Laugh();
        void Dizzy();
        void Escape();
        void Sleep();
        void GetUp();
        void Jump();
        void Attack();
        void AttackRepeat();
        void Cast();
        void Charge();
        void Defense();
        void Hit();
        void Die();
    }

    public static class BASE_ATTR
    {
        public const string CATEGORY = "category";
        public const string IDENTITY = "identity";
        public const string COLLIDER = "collider";
        public const string RADIUS = "radius";
        public const string SIZEX = "sizeX";
        public const string SIZEY = "sizeY";
        public const string SIZEZ = "sizeZ";
        public const string CENTERX = "centerX";
        public const string CENTERY = "centerY";
        public const string CENTERZ = "centerZ";
        public const string TRIGGER = "trigger";
        public const string RIGIDBODY = "rigidbody";
        public const string GRAVITY = "gravity";
        public const string KINEMATIC = "kinematic";
    }

    public class BaseAttr
    {
        public BaseAttr(JSONObject json)
        {
            category = (ObjectCategory)json[BASE_ATTR.CATEGORY].AsInt;
            identity = (ObjectIdentity)json[BASE_ATTR.IDENTITY].AsInt;
            collider = (ObjectCollider)json[BASE_ATTR.COLLIDER].AsInt;
            radius = json[BASE_ATTR.RADIUS].AsFloat;
            sizeX = json[BASE_ATTR.SIZEX].AsFloat;
            sizeY = json[BASE_ATTR.SIZEY].AsFloat;
            sizeZ = json[BASE_ATTR.SIZEZ].AsFloat;
            centerX = json[BASE_ATTR.CENTERX].AsFloat;
            centerY = json[BASE_ATTR.CENTERY].AsFloat;
            centerZ = json[BASE_ATTR.CENTERZ].AsFloat;
            trigger = json[BASE_ATTR.TRIGGER].AsBool;
            rigidbody = json[BASE_ATTR.RIGIDBODY].AsBool;
            gravity = json[BASE_ATTR.GRAVITY].AsBool;
            kinematic = json[BASE_ATTR.KINEMATIC].AsBool;
        }
        public ObjectCategory category;
        public ObjectIdentity identity;
        public ObjectCollider collider;
        public float radius;
        public float sizeX;
        public float sizeY;
        public float sizeZ;
        public float centerX;
        public float centerY;
        public float centerZ;
        public bool trigger;
        public bool rigidbody;
        public bool gravity;
        public bool kinematic;
    }


    public static class OBJECT_ATTR
    {
        public const string NAME = "name";
        public const string SHOW_NAME = "showName";
        public const string BASE = "base";
        public const string ENTITY = "entity";
        public const string SPEED = "speed";
        public const string ROTATE = "rotate";
        public const string HEALTH = "health";
        public const string NUMBER = "number";
        public const string BLOCK = "block";
        public const string MAGIC = "magic";
        public const string ENERGY = "energy";
        public const string SCAN_RADIUS = "scanRadius";
        public const string EXTEND = "extend";
    }

    public class ObjectAttr
    {
        public ObjectAttr(JSONObject json)
        {
            name = json[OBJECT_ATTR.NAME];
            showName = json[OBJECT_ATTR.SHOW_NAME];
            baseAttr = new BaseAttr(json[OBJECT_ATTR.BASE].AsObject);
            entity = json[OBJECT_ATTR.ENTITY];
            speed = json[OBJECT_ATTR.SPEED].AsFloat;
            rotate = json[OBJECT_ATTR.ROTATE].AsFloat;
            auto = false;
            kill = 0;
            star = 0;
            health = json[OBJECT_ATTR.HEALTH].AsFloat;
            healthTotal = json[OBJECT_ATTR.HEALTH].AsFloat;
            number = json[OBJECT_ATTR.NUMBER].AsFloat;
            isHot = false;
            hot = 0;
            wait = 0;
            block = json[OBJECT_ATTR.BLOCK].AsFloat;
            magic = json[OBJECT_ATTR.MAGIC].AsFloat;
            magicTotal = json[OBJECT_ATTR.MAGIC].AsFloat;
            energy = json[OBJECT_ATTR.ENERGY].AsFloat;
            energyTotal = json[OBJECT_ATTR.ENERGY].AsFloat;
            scanRadius = json[OBJECT_ATTR.SCAN_RADIUS].AsFloat;
            extend = json[OBJECT_ATTR.EXTEND].AsObject;
            statusList = new List<Status?>();
        }
        public string name;
        public string showName;
        public BaseAttr baseAttr;
        public string entity;
        public float speed;
        public float rotate;
        public float kill;
        public float star;
        public bool auto;
        public float radio;
        public float health;
        public float healthTotal;
        public float wait;
        public float number;
        public bool isHot;
        public float hot;
        public float block;
        public float magic;
        public float magicTotal;
        public float energy;
        public float energyTotal;
        public float scanRadius;
        public List<Status?> statusList;
        public JSONObject extend;
    }

    public struct Status
    {
        public float time;
        public Skill skill;
    }

    public static class SKILL_EXTRA
    {
        public const string VALUE = "value";
        public const string INTERVAL_TIME = "intervalTime";
        public const string SUSTAINED_TIME = "sustainedTime";
        public const string EXTRA_TYPE = "extraType";
        public const string EXTRA_STATUS = "extraStatus";
        public const string STATUS_VALUE = "statusValue";
    }

    public class SkillExtra
    {
        public SkillExtra(JSONObject json)
        {
            value = json[SKILL_EXTRA.VALUE].AsFloat;
            intervalTime = json[SKILL_EXTRA.INTERVAL_TIME].AsFloat;
            sustainedTime = json[SKILL_EXTRA.SUSTAINED_TIME].AsFloat;
            extraType = (SkillExtraType)json[SKILL_EXTRA.EXTRA_TYPE].AsInt;
            extraStatus = (SkillExtraStatus)json[SKILL_EXTRA.EXTRA_STATUS].AsInt;
            statusValue = json[SKILL_EXTRA.STATUS_VALUE].AsFloat;
        }
        public float value;
        public float intervalTime;
        public float sustainedTime;
        public SkillExtraType extraType;
        public SkillExtraStatus extraStatus;
        public float statusValue;
    }

    public static class SKILL
    {
        public const string NAME = "name";
        public const string ENTITY = "entity";
        public const string VALUE = "value";
        public const string NEED = "need";
        public const string RANG = "rang";
        public const string READY_TIME = "readyTime";
        public const string CAST_TIME = "castTime";
        public const string INTERVAL_TIME = "intervalTime";
        public const string SUSTAINED_TIME = "sustainedTime";
        public const string TARGET_RELATION = "targetRelation";
        public const string TARGET_NUM = "targetNum";
        public const string TARGET_NEED = "targetNeed";
        public const string SKILL_TYPE = "skillType";
        public const string CAST_TYPE = "castType";
        public const string CONSUME = "consume";
        public const string HAS_EXTRA = "hasExtra";
        public const string EXTRA = "extra";
    }


    public class Skill
    {
        public Skill(JSONObject json)
        {
            name = json[SKILL.NAME];
            entity = json[SKILL.ENTITY];
            value = json[SKILL.VALUE].AsFloat;
            need = json[SKILL.NEED].AsFloat;
            range = json[SKILL.RANG].AsFloat;
            readyTime = json[SKILL.READY_TIME].AsFloat;
            castTime = json[SKILL.CAST_TIME].AsFloat;
            intervalTime = json[SKILL.INTERVAL_TIME].AsFloat;
            sustainedTime = json[SKILL.SUSTAINED_TIME].AsFloat;
            targetRelation = (TargetRelation)json[SKILL.TARGET_RELATION].AsInt;
            skillType = (SkillType)json[SKILL.SKILL_TYPE].AsInt;
            castType = (SkillCastType)json[SKILL.CAST_TYPE].AsInt;
            consume = (SKillConsume)json[SKILL.CONSUME].AsInt;
            hasExtra = json[SKILL.HAS_EXTRA].AsBool;
            extra = new SkillExtra(json[SKILL.EXTRA].AsObject);
        }
        public int id;
        public string name;
        public string entity;
        public float value;
        public float need;
        public float range;
        public float intervalTime;
        public float readyTime;
        public float castTime;
        public float sustainedTime;
        public TargetRelation targetRelation;
        public SkillType skillType;
        public SkillCastType castType;
        public SKillConsume consume;
        public bool hasExtra;
        public SkillExtra extra;
    }

    public enum SkillExtraStatus
    {
        Frozen = 0,
        Fire = 1,
        Dizzy = 2,
        Fear = 3,
        Aim = 4
    }

    public enum SkillExtraType
    {
        Injured = 0,
        Cure = 1,
        Status = 2,
        InjuredAndStatus = 3,
        CureAndStatus = 4,
    }

    public enum SkillCastType
    {
        Now = 0,
        Ready = 1,
        Sustained = 2,
        ReadyAndSustained = 3
    }

    public enum SkillType
    {
        Injured = 0,
        Cure = 1
    }

    public enum SKillConsume
    {
        Empty = 0,
        Number = 1,
        Hot = 2,
        Block = 3,
        Energy = 4,
        Magic = 5
    }

    public enum TargetRelation
    {
        Self = 0,
        Partner = 1,
        Enemy = 2
    }

    public enum ObjectCategory
    {
        Target = 0,
        Object = 1
    }

    public enum ObjectIdentity
    {
        Partner = 0,
        Enemy = 1,
        Other = 2
    }

    public enum ObjectControl:byte
    {
        Player = 0,
        OtherPlayer = 1,
        Ai = 2
    }

    public enum ObjectCollider
    {
        Empty = 0,
        Box = 1,
        Sphere = 2
    }
}
