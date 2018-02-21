using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Gj
{
    public class TextUIHelper : MonoBehaviour
    {
        public string key = " ";
        void Start()
        {
            GetComponent<Text>().text = Localization.GetInstance.GetValue(key);
        }
    }
}
