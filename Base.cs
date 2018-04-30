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

    public static class TARGETATTR {
        public const string NAME = "name";
        public const string SHOW_NAME = "showName";
        public const string SPEED = "speed";
        public const string ROTATE = "rotate";
        public const string RADIO = "radio";
        public const string HEALTH = "health";
        public const string NUMBER = "number";
        public const string BLOCK = "block";
        public const string MAGIC = "magic";
        public const string ENERGY = "energy";
        public const string SCAN_RADIUS = "scanRadius";
        public const string EXTEND = "extend";
    }

    public class TargetAttr
    {
        public TargetAttr(JSONObject json) {
            name = json[TARGETATTR.NAME];
            showName = json[TARGETATTR.SHOW_NAME];
            speed = json[TARGETATTR.SPEED].AsFloat;
            rotate = json[TARGETATTR.ROTATE].AsFloat;
            auto = false;
            kill = 0;
            star = 0;
            radio = json[TARGETATTR.RADIO].AsFloat;
            health = json[TARGETATTR.HEALTH].AsFloat;
            healthTotal = json[TARGETATTR.HEALTH].AsFloat;
            number = json[TARGETATTR.NUMBER].AsFloat;
            isHot = false;
            hot = 0;
            wait = 0;
            block = json[TARGETATTR.BLOCK].AsFloat;
            magic = json[TARGETATTR.MAGIC].AsFloat;
            magicTotal = json[TARGETATTR.MAGIC].AsFloat;
            energy = json[TARGETATTR.ENERGY].AsFloat;
            energyTotal = json[TARGETATTR.ENERGY].AsFloat;
            scanRadius = json[TARGETATTR.SCAN_RADIUS].AsFloat;
            extend = json[TARGETATTR.EXTEND].AsObject;
        }
        public string name;
        public string showName;
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
        public SkillExtra (JSONObject json) {
            value = json[SKILLEXTRA.VALUE].AsFloat;
            intervalTime = json[SKILLEXTRA.INTERVAL_TIME].AsFloat;
            extraType = (ExtraType) json[SKILLEXTRA.EXTRA_TYPE].AsInt;
            handleType = (HandleType) json[SKILLEXTRA.HANDLE_TYPE].AsInt;
        }
        public float value;
        public float intervalTime;
        public ExtraType extraType;
        public HandleType handleType;
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
        public const string NEED_TYPE = "needType";
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
            targetNum = (TargetNum)json[SKILL.TARGET_NUM].AsInt;
            targetNeed = (TargetNeed)json[SKILL.TARGET_NEED].AsInt;
            skillType = (SkillType)json[SKILL.SKILL_TYPE].AsInt;
            castType = (CastType)json[SKILL.CAST_TYPE].AsInt;
            needType = (NeedType)json[SKILL.NEED_TYPE].AsInt;
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
        public TargetNum targetNum;
        public TargetNeed targetNeed;
        public SkillType skillType;
        public CastType castType;
        public NeedType needType;
        public SkillExtra extra;
    }

    public enum HandleType
    {
        Add = 0,
        Subtract = 1,
        Multiply = 2,
        Divide = 3
    }

    public enum ExtraType
    {
        Cast = 0,
        Attribute = 1,
        Special = 2
    }

    public enum CastType
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
        Prop = 2,
        Attr = 3
    }

    public enum TargetNeed
    {
        Target = 0,
        Region = 1,
        None = 2
    }

    public enum TargetNum
    {
        One = 0,
        Some = 1
    }

    public enum NeedType
    {
        Number = 0,
        Hot = 1,
        Block = 2,
        Energy = 3,
        Magic = 4,
        Empty = 5
    }

    public enum TargetRelation
    {
        Self = 0,
        Partner = 1,
        Enemy = 2
    }
}
