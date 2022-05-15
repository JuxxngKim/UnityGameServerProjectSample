using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YeongJ.Inagme;

namespace YeongJ.UI
{
    public class DamageFont : MonoBehaviour
    {
        [SerializeField] Text _damageText;

        private float _lifeTime;
        private float _remainTime;
        private Vector3 _worldPosition;

        public void Init(Vector3 worldPosition, int damage, float lifeTime)
        {
            _worldPosition = worldPosition;
            _damageText.text = damage.ToString();
            _remainTime = _lifeTime = lifeTime;

            transform.position = Camera.main.WorldToScreenPoint(_worldPosition);
        }

        void Update()
        {
            _remainTime -= Time.deltaTime;
            float ratio = _remainTime <= 0.0f ? 0.0f : Mathf.Clamp01(_remainTime / _lifeTime);
            var newColor = _damageText.color;
            newColor.a = ratio;

            _damageText.color = newColor;

            transform.position = Camera.main.WorldToScreenPoint(_worldPosition);
        }
    }
}
