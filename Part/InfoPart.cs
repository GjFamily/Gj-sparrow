using UnityEngine;
using System.Collections;

namespace Gj
{
    public class InfoPart : BasePart
    {
        private Category category = Category.Empty;
        public float radio = 0;
        public enum Category
        {
            Build,
            Empty,
            Skill,
            Target,
            BuildTarget,
            SkillTarget
        }

        public void SetCategory(Category i)
        {
            category = i;
        }

        public bool IsEmpty()
        {
            return category == Category.Empty;
        }

        public bool IsSkill()
        {
            return category == Category.Skill || category == Category.SkillTarget;
        }

        public bool IsTarget()
        {
            return category == Category.Target || category == Category.BuildTarget || category == Category.SkillTarget;
        }

        public bool IsBuild()
        {
            return category == Category.Build || category == Category.BuildTarget;
        }
    }
}
