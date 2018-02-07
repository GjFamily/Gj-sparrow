using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Gj
{
    public class Localization
    {
        //单例模式    
        private static Localization _instance;

        public static Localization GetInstance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Localization();
                }

                return _instance;
            }
        }

        private const string chinese = "Chinese";
        private const string english = "English";

        //选择自已需要的本地语言    
        public string language = chinese;


        private Dictionary<string, string> dic = new Dictionary<string, string>();
        /// <summary>    
        /// 读取配置文件，将文件信息保存到字典里    
        /// </summary>    
        public Localization()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseTraditional:
                case SystemLanguage.ChineseSimplified:
                    language = chinese;
                    break;
                default:
                    language = english;
                    break;
            }
            dic = Resource.GetLanguage(language);
        }

        /// <summary>    
        /// 获取value    
        /// </summary>    
        /// <param name="key"></param>    
        /// <returns></returns>    
        public string GetValue(string key)
        {
            if (dic.ContainsKey(key) == false)
            {
                return null;
            }
            string value = null;
            dic.TryGetValue(key, out value);
            return value;
        }
    }
}
