using UnityEngine;
using System.Collections;
using System;
using SimpleJSON;

namespace Gj
{
    public interface UISystem
    {
        void UIClick(string key);
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

    public class TargetAttr
    {
        public TargetAttr(JSONObject json) {
            name = json["name"];
            showName = json["showName"];
            speed = json["speed"].AsFloat;
            rotate = json["rotate"].AsFloat;
            auto = false;
            kill = 0;
            star = 0;
            radio = json["radio"].AsFloat;
            health = json["health"].AsFloat;
            healthTotal = json["health"].AsFloat;
            number = json["number"].AsFloat;
            isHot = false;
            hot = 0;
            wait = 0;
            block = json["block"].AsFloat;
            magic = json["magic"].AsFloat;
            magicTotal = json["magic"].AsFloat;
            energy = json["energy"].AsFloat;
            energyTotal = json["energy"].AsFloat;
            scanRadius = json["scanRadius"].AsFloat;
            extend = json["extend"].AsObject;
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

    public class SkillExtra
    {
        public SkillExtra (JSONObject json) {
            value = json["value"].AsFloat;
            intervalTime = json["intervalTime"].AsFloat;
            targetRelation = (TargetRelation) json["targetRelation"].AsInt;
            extraType = (ExtraType) json["extraType"].AsInt;
            handleType = (HandleType) json["handleType"].AsInt;
            numType = (NumType) json["numType"].AsInt;
        }
        public float value;
        public float intervalTime;
        public TargetRelation targetRelation;
        public ExtraType extraType;
        public HandleType handleType;
        public NumType numType;
    }

    public class Skill
    {
        public Skill(JSONObject json)
        {
            name = json["name"];
            value = json["value"].AsFloat;
            need = json["need"].AsFloat;
            range = json["rang"].AsFloat;
            readyTime = json["readyTime"].AsFloat;
            castTime = json["castTime"].AsFloat;
            intervalTime = json["intervalTime"].AsFloat;
            sustainedTime = json["sustainedTime"].AsFloat;
            targetRelation = (TargetRelation)json["targetRelation"].AsInt;
            targetNum = (TargetNum)json["targetNum"].AsInt;
            targetNeed = (TargetNeed)json["targetNeed"].AsInt;
            skillType = (SkillType)json["skillType"].AsInt;
            castType = (CastType)json["castType"].AsInt;
            needType = (NeedType)json["needType"].AsInt;
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

    public enum NumType
    {
        Only = 0,
        TargetOnly = 1,
        None = 2
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
