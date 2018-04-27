using UnityEngine;
using System.Collections;
using System;

namespace Gj
{
    public interface UISystem
    {
        void UIClick(string key);
    }

    public struct Attr {
        public string name;
        public float speed;
        public Type type;
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

    public struct SkillExtra
    {
        public float intervalTime;
        public TargetRelation targetRelation;
        public ExtraType extraType;
        public float value;
        public string attrubute;
        public HandleType handleType;
        public NumType numType;
    }

    public struct Skill
    {
        public string name;
        public float value;
        public float need;
        public float range;
        public float intervalTime;
        public float readyTime;
        public float castTime;
        public float sustainedTime;
        public Type type;
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
