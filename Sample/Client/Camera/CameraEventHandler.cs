using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraEventHandler : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera _virtualCamera;

    public static CameraEventHandler Instance { get; private set; }

    private CinemachineBasicMultiChannelPerlin _perlin;
    private float _shakeTimer;
    private float _velocity;
    
    private Volume _volume;
    private MotionBlur _motionBlur;
    private float _motionBlurTimer;
    private float _velocityMotionBlur;

    void Awake()
    {
        Instance = this;
        MotionBlur motionBlur = null;

        _perlin = _virtualCamera?.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _volume = GameObject.Find("Global Volume")?.GetComponent<Volume>();
        _volume?.profile?.TryGet<MotionBlur>(out motionBlur);
        _motionBlur = motionBlur;
    }

    public void StartShake(float delay, float intensity, float time)
    {
        _velocity = 0.0f;
        _perlin.m_AmplitudeGain = intensity;
        _shakeTimer = time;
    }

    public void StartMotionBlur(float time, float intensity)
    {
        if (_motionBlur == null)
            return;

        _velocityMotionBlur = 0.0f;
        _motionBlur.intensity.value = intensity;
        _motionBlurTimer = time;
    }

    void Update()
    {
        UpdateShake();
        UpdateMotionBlur();
    }

    void UpdateMotionBlur()
    {
        if (_motionBlurTimer > 0)
        {
            _motionBlurTimer -= Time.deltaTime;
        }
        else
        {
            if (_motionBlur.intensity.value <= 0.0f)
                return;

            _motionBlur.intensity.value = Mathf.SmoothDamp(_motionBlur.intensity.value, target: 0.0f, ref _velocityMotionBlur, smoothTime: 0.1f);
        }
    }

    void UpdateShake()
    {
        if (_shakeTimer > 0)
        {
            _shakeTimer -= Time.deltaTime;
        }
        else
        {
            if (_perlin.m_AmplitudeGain <= 0.0f)
                return;

            _perlin.m_AmplitudeGain = Mathf.SmoothDamp(_perlin.m_AmplitudeGain, target: 0.0f, ref _velocity, smoothTime: 0.1f);
        }
    }
}
