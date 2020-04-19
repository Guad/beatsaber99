using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaber99Client.UI
{
    public class DynamicText : MonoBehaviour
    {
        private Canvas _canvas;
        private Vector3 _basePosition = new Vector3(0f, 4f, 5f);
        private float _scale = 2.0f;
        private float _width = 200f;

        private float _fontSize = 12f;
        private Color _fontColor = Color.white;

        public TextMeshProUGUI text;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos">X: left-right, Y: up-down, Z: front-back</param>
        /// <param name="fontSize"></param>
        /// <returns></returns>
        public static DynamicText Create(Vector3 pos, float fontSize)
        {
            var t = new GameObject().AddComponent<DynamicText>();

            t._fontSize = fontSize;
            t._basePosition = pos;

            if (t.text != null)
            {
                t.text.fontSize = fontSize;
                t._canvas.transform.position = pos;
            }

            return t;
        }

        public void Delete()
        {
            Destroy(gameObject);
        }

        public void SetPosition(Vector3 pos)
        {
            if (_canvas != null && _canvas.transform != null)
                _canvas.transform.position = pos;
        }

        void Awake()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            var collider = gameObject.AddComponent<MeshCollider>();
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 100;
            _canvas.GetComponent<RectTransform>().localScale = new Vector3(0.012f * _scale, 0.012f * _scale, 0.012f * _scale);

            var textGameobject = new GameObject().AddComponent<TextMeshProUGUI>();
            text = textGameobject.GetComponent<TextMeshProUGUI>();

            text.rectTransform.SetParent(gameObject.transform, false);
            text.rectTransform.localPosition = new Vector3(0, 0, 0);
            text.rectTransform.localRotation = new Quaternion(0, 0, 0, 0);
            text.rectTransform.pivot = new Vector2(0, 0);
            text.rectTransform.sizeDelta = new Vector2(_width, 1);
            text.enableWordWrapping = false;
            text.richText = true;
            text.fontSize = _fontSize;
            text.overflowMode = TextOverflowModes.Overflow;
            text.alignment = TextAlignmentOptions.Left;
            text.color = _fontColor;
            text.enabled = true;
            text.text = "";

            _canvas.transform.position = _basePosition;
            _canvas.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }
}