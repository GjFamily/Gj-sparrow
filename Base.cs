using UnityEngine;
using System.Collections;
using System;
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

    public static class BASEATTR
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
            category = (ObjectCategory)json[BASEATTR.CATEGORY].AsInt;
            identity = (ObjectIdentity)json[BASEATTR.IDENTITY].AsInt;
            collider = (ObjectCollider)json[BASEATTR.COLLIDER].AsInt;
            radius = json[BASEATTR.RADIUS].AsFloat;
            sizeX = json[BASEATTR.SIZEX].AsFloat;
            sizeY = json[BASEATTR.SIZEY].AsFloat;
            sizeZ = json[BASEATTR.SIZEZ].AsFloat;
            centerX = json[BASEATTR.CENTERX].AsFloat;
            centerY = json[BASEATTR.CENTERY].AsFloat;
            centerZ = json[BASEATTR.CENTERZ].AsFloat;
            trigger = json[BASEATTR.TRIGGER].AsBool;
            rigidbody = json[BASEATTR.RIGIDBODY].AsBool;
            gravity = json[BASEATTR.GRAVITY].AsBool;
            kinematic = json[BASEATTR.KINEMATIC].AsBool;
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


    public static class OBJECTATTR
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
            name = json[OBJECTATTR.NAME];
            showName = json[OBJECTATTR.SHOW_NAME];
            baseAttr = new BaseAttr(json[OBJECTATTR.BASE].AsObject);
            entity = json[OBJECTATTR.ENTITY];
            speed = json[OBJECTATTR.SPEED].AsFloat;
            rotate = json[OBJECTATTR.ROTATE].AsFloat;
            auto = false;
            kill = 0;
            star = 0;
            health = json[OBJECTATTR.HEALTH].AsFloat;
            healthTotal = json[OBJECTATTR.HEALTH].AsFloat;
            number = json[OBJECTATTR.NUMBER].AsFloat;
            isHot = false;
            hot = 0;
            wait = 0;
            block = json[OBJECTATTR.BLOCK].AsFloat;
            magic = json[OBJECTATTR.MAGIC].AsFloat;
            magicTotal = json[OBJECTATTR.MAGIC].AsFloat;
            energy = json[OBJECTATTR.ENERGY].AsFloat;
            energyTotal = json[OBJECTATTR.ENERGY].AsFloat;
            scanRadius = json[OBJECTATTR.SCAN_RADIUS].AsFloat;
            extend = json[OBJECTATTR.EXTEND].AsObject;
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
        public JSONObject extend;
    }

    public static class SKILLEXTRA
    {
        public const string VALUE = "value";
        public const string INTERVAL_TIME = "intervalTime";
        public const string EXTRA_TYPE = "extraType";
        public const string HANDLE_TYPE = "handleType";
    }

    public class SkillExtra
    {
        public SkillExtra(JSONObject json)
        {
            value = json[SKILLEXTRA.VALUE].AsFloat;
            intervalTime = json[SKILLEXTRA.INTERVAL_TIME].AsFloat;
            extraType = (SkillExtraType)json[SKILLEXTRA.EXTRA_TYPE].AsInt;
            handleType = (SkillExtraHandleType)json[SKILLEXTRA.HANDLE_TYPE].AsInt;
        }
        public float value;
        public float intervalTime;
        public SkillExtraType extraType;
        public SkillExtraHandleType handleType;
    }

    public static class SKILL
    {
        public const string NAME = "name";
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
    }


    public class Skill
    {
        public Skill(JSONObject json)
        {
            name = json[SKILL.NAME];
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
        }
        public string name;
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
        public SkillExtra extra;
    }

    public enum SkillExtraHandleType
    {
        Add = 0,
        Subtract = 1,
        Multiply = 2,
        Divide = 3
    }

    public enum SkillExtraType
    {
        Cast = 0,
        Attribute = 1,
        Special = 2
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
        Attack = 0,
        Defense = 1,
        Plus = 2,
        Minus = 3
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
        Player,
        OtherPlayer,
        Ai
    }

    public enum ObjectCollider
    {
        Empty = 0,
        Box = 1,
        Sphere = 2
    }
}
