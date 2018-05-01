using UnityEngine;
using System.Collections;
using Gj;

[RequireComponent(typeof(SkillPart))]
public class PowerPart : BasePart
{
    public void Init () {
        GetComponent<SkillPart>().SetPower(IsEnoughConsume, Consume);
    }

    private bool IsEnoughConsume(Skill skill)
    {
        bool result = true;
        switch (skill.consume)
        {
            case SKillConsume.Hot:
                if (Info.attr.isHot) {
                    result = false;
                }
                break;
            case SKillConsume.Block:
                if (Info.attr.block < skill.need) {
                    result = false;
                }
                break;
            case SKillConsume.Number:
                if (Info.attr.number < skill.need)
                {
                    result = false;
                }
                break;
            default:
                break;
        }
        return result;
    }

    private void Consume(Skill skill)
    {
        switch (skill.consume)
        {
            case SKillConsume.Hot:
                float hot = Info.attr.hot;
                hot += skill.need;
                if (hot >= 100)
                {
                    Info.attr.isHot = true;
                    hot = 100;
                }
                Info.attr.hot = hot;
                break;
            case SKillConsume.Block:
                Info.attr.block -= skill.need;
                break;
            case SKillConsume.Number:
                Info.attr.number -= skill.need;
                break;
            default:
                break;
        }
    }
}
