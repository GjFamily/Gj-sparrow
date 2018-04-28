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
        switch (skill.needType)
        {
            case NeedType.Hot:
                if (Info.attr.isHot) {
                    result = false;
                }
                break;
            case NeedType.Block:
                if (Info.attr.block < skill.need) {
                    result = false;
                }
                break;
            case NeedType.Number:
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
        switch (skill.needType)
        {
            case NeedType.Hot:
                float hot = Info.attr.hot;
                hot += skill.need;
                if (hot >= 100)
                {
                    Info.attr.isHot = true;
                    hot = 100;
                }
                Info.attr.hot = hot;
                break;
            case NeedType.Block:
                Info.attr.block -= skill.need;
                break;
            case NeedType.Number:
                Info.attr.number -= skill.need;
                break;
            default:
                break;
        }
    }
}
