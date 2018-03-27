using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gj
{
    public class RockerInputHelper : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        private float radius;
        public Image item;
        public float size;
        public float offsetX;
        public float offsetY;
        public bool left;
        public bool hide;
        public string key;
        // Use this for initialization
        void Start()
        {
            radius = size / 2;
            if (hide)
            {
                item.gameObject.SetActive(false);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (hide)
            {
                item.gameObject.SetActive(true);
            }
            Vector2 position = GetPosition(eventData.position.x, eventData.position.y);
            item.rectTransform.localPosition = position;
            StartChange();
            ChangeValue(position.x / radius, position.y / radius);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            item.rectTransform.localPosition = new Vector2(0, 0);
            ChangeValue(0, 0);
            if (hide)
            {
                item.gameObject.SetActive(false);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 position = GetPosition(eventData.position.x, eventData.position.y);
            item.rectTransform.localPosition = position;
            EndChange();
            ChangeValue(position.x / radius, position.y / radius);
        }

        private void StartChange()
        {
            if (left)
            {
                SystemInput.lk = key;
            }
            else
            {
                SystemInput.rk = key;
            }
        }

        private void EndChange()
        {

            if (left)
            {
                SystemInput.lk = null;
            }
            else
            {
                SystemInput.rk = null;
            }
        }

        private void ChangeValue(float x, float y)
        {
            if (left)
            {
                SystemInput.lh = x;
                SystemInput.lv = y;
            }
            else
            {
                SystemInput.rh = x;
                SystemInput.rv = y;
            }
        }

        private float GetPositionX(float x)
        {

            if (left)
            {
                return x - radius - offsetX;
            }
            else
            {
                return x - SimpleTools.width + radius + offsetX;
            }
        }

        private float GetPositionY(float y)
        {
            return y - radius - offsetY;
        }

        private bool IsOut(float x, float y)
        {
            //计算两坐标点之间的距离
            return Vector2.Distance(new Vector2(x, y), new Vector2(0, 0)) > radius;
        }

        private Vector2 GetPosition(float x, float y)
        {
            float targetX = GetPositionX(SimpleTools.GetX(x));
            float targetY = GetPositionY(SimpleTools.GetY(y));
            if (IsOut(targetX, targetY))
            {
                //获取x，y坐标点弧度
                float radians = Mathf.Atan2(targetX, targetY);
                //sin和cos对应的是弧度不是角度
                return new Vector2(Mathf.Sin(radians) * radius, Mathf.Cos(radians) * radius);
            }
            else
            {
                return new Vector2(targetX, targetY);
            }
        }
    }

}