﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gj
{
    public class InputControlHelper : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        private float radius;
        public Image item;
        public float size;
        public float offsetX;
        public float offsetY;
        public bool left;
        public bool refresh = true;
        // Use this for initialization
        void Start()
        {
            radius = size / 2;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Vector2 position = GetPosition(eventData.position.x, eventData.position.y);
            item.rectTransform.localPosition = position;
            ChangeValue(position.x / radius, position.y / radius);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (refresh) {
                item.rectTransform.localPosition = new Vector2(0, 0);
            }
            ChangeValue(0, 0);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 position = GetPosition(eventData.position.x, eventData.position.y);
            item.rectTransform.localPosition = position;
            ChangeValue(position.x / radius, position.y / radius);
        }

        private void ChangeValue(float x, float y) {
            if (left) {
                SystemInput.lh = x / radius;
                SystemInput.lv = y / radius;
            } else {
                SystemInput.rh = x / radius;
                SystemInput.rv = y / radius;
            }
        }

        private float GetPositionX(float x)
        {
            
            if (left) {
                return x - radius - offsetX;
            } else {
                return x - Tools.width + radius + offsetX;
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
            float targetX = GetPositionX(Tools.GetX(x));
            float targetY = GetPositionY(Tools.GetY(y));
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