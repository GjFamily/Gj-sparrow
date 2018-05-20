using UnityEngine;
using System.Collections;

namespace Gj
{
    public class PopupDamage : MonoBehaviour
    {
        //目标位置    
        private Vector3 mTarget;
        //屏幕坐标    
        private Vector3 mScreen;
        //伤害数值    
        private string value;
        public Font font;
        public Color color;
        public int fontSize;

        //文本宽度    
        private float ContentWidth = 100;
        //文本高度    
        private float ContentHeight = 100;

        //GUI坐标    
        private Vector2 mPoint;

        //销毁时间    
        private float FreeTime = 1F;

        void Start()
        {
            //获取目标位置    
            mTarget = transform.position + new Vector3(0, 10, 0);
            //获取屏幕坐标    
            mScreen = Camera.main.WorldToScreenPoint(mTarget);
            //将屏幕坐标转化为GUI坐标    
            mPoint = new Vector2(mScreen.x, Screen.height - mScreen.y);
            //开启自动销毁线程    
            //StartCoroutine("Free");
            Destroy(gameObject, FreeTime);
        }

        void Update()
        {
            //使文本在垂直方向山产生一个偏移    
            transform.Translate(Vector3.up * 1F * Time.deltaTime);
            //重新计算坐标    
            mTarget = transform.position;
            //获取屏幕坐标    
            mScreen = Camera.main.WorldToScreenPoint(mTarget);
            //将屏幕坐标转化为GUI坐标    
            mPoint = new Vector2(mScreen.x, Screen.height - mScreen.y);
        }

        public void SetValue (string v) {
            value = v;
        }

        void OnGUI()
        {
            //保证目标在摄像机前方    
            if (mScreen.z > 0)
            {
                //内部使用GUI坐标进行绘制    
                GUIStyle style = new GUIStyle();
                style.fontSize = fontSize;
                style.normal.textColor = color;
                style.font = font;
                style.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(mPoint.x - (ContentWidth / 2), mPoint.y - (ContentHeight / 2), ContentWidth, ContentHeight), value, style);
            }
        }

        IEnumerator Free()
        {
            yield return new WaitForSeconds(FreeTime);
            Destroy(this.gameObject);
        }
    }
}