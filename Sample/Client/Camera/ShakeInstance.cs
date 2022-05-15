using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YeongJ.Inagme
{
    public class ShakeInstance : MonoBehaviour
    {
        [SerializeField] float _shakeDelay;
        [SerializeField] float _shakeIntensity;

        float _remainTime;

        void Start()
        {
            _remainTime = _shakeDelay <= 0.0f ? 0.1f : _shakeDelay;
        }

        void Update()
        {
            if (_remainTime <= 0.0f)
                return;

            _remainTime -= Time.deltaTime;
            if (_remainTime > 0.0f)
                return;

            var diff = Camera.main.transform.position - transform.position;
            if (diff.magnitude > 30.0f)
                return;

            CameraEventHandler.Instance.StartShake(_shakeDelay, _shakeIntensity, time: 0.5f);
            CameraEventHandler.Instance.StartMotionBlur(time: 0.5f, _shakeIntensity);
        }
    }
}