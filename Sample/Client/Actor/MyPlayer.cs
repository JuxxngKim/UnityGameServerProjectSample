using Cinemachine;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;
using YeongJ.UI;

namespace YeongJ.Inagme
{
    public class MyPlayer : Player
    {
        [SerializeField] CinemachineVirtualCamera _virtualCamera;
        [SerializeField] GameObject _makerEffect;

        float _latency;
        float _inputCheckTime = 0.0f;

        const float _inputDelay = 0.1f;
        const float _camDistanceMin = 2.6f;
        const float _camDistanceMax = 14.0f;
        const float _camAngleMin = 15.0f;
        const float _camAngleMax = 45.0f;

        CinemachineFramingTransposer _transposer;
        ObjModel _level;

        public CinemachineFramingTransposer Transposer
        {
            get
            {
                if (_transposer == null)
                    _transposer = _virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();

                return _transposer;
            }
        }

        public void InitMap(ObjModel level)
        {
            _level = level;
        }

        public void SetLatency(float latency)
        {
            _latency = latency;
        }

        public override void Init(int Id)
        {
            base.Init(Id);

            _inputHandle = UpdateKeyInput;
            _inputCheckTime = _inputDelay;
        }

        public void UpdateKeyInput()
        {
            UpdateMouseScroll();
            UpdateMoveInupt();
            UpdateSkillInput();
        }

        void UpdateSkillInput()
        {
            if (ChatManager.InputLock)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                    return;

                SendSkillPacket(skillId: 1);
                return;
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                SendDancePacket();
                return;
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                SendSkillPacket(skillId: 2, isCliektSpawn: true);
                return;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                SendSkillPacket(skillId: 5, isCliektSpawn: true);
                return;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                SendSkillPacket(skillId: 3, isCliektSpawn: true);
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                SendTeleportSkillPacekt();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                SendSkillPacket(skillId: 4, isCliektSpawn: true);
                return;
            }
        }

        void UpdateMoveInupt()
        {
            if (Input.GetMouseButtonDown(1))
            {
                SendMovePacket();
                _inputCheckTime = _inputDelay;
                return;
            }

            if (_inputCheckTime > 0.0f)
            {
                _inputCheckTime -= Time.deltaTime;
                return;
            }

            _inputCheckTime = _inputDelay;
            if (Input.GetMouseButton(1))
            {
                SendMovePacket(makeMaker: false);
            }
        }

        void UpdateMouseScroll()
        {
            if (Transposer == null)
                return;

            float scroll = -Input.mouseScrollDelta.y * 3.0f;
            var distance = Transposer.m_CameraDistance + scroll;
            Transposer.m_CameraDistance = Mathf.Clamp(distance, _camDistanceMin, _camDistanceMax);

            float angleVelocity = 0.0f;
            var angles = _virtualCamera.transform.eulerAngles;

            if (Transposer.m_CameraDistance <= _camDistanceMin)
            {
                angles.x = Mathf.SmoothDamp(angles.x, _camAngleMin, ref angleVelocity, 0.04f);
                _virtualCamera.transform.eulerAngles = angles;
            }
            else
            {
                angles.x = Mathf.SmoothDamp(angles.x, _camAngleMax, ref angleVelocity, 0.04f);
                _virtualCamera.transform.eulerAngles = angles;
            }
        }

        void SendMovePacket(bool makeMaker = true)
        {
            if (ServerPosInfo.State == ActorState.Attack)
                return;

            var clickResult = GetClickPosition();
            if (!clickResult.result)
            {
                return;
            }

            if (makeMaker)
            {
                MakeMakerEffect(clickResult.position);
            }

            C_Move movePacket = new C_Move();
            movePacket.PosInfo = ServerPosInfo.Clone();
            movePacket.PosInfo.Position = clickResult.position.ToFloat3();
            Managers.Network.Send(movePacket);
        }

        void SendDancePacket()
        {
            if (ServerDir != Vector3.zero)
                return;

            C_Dance dancePacket = new C_Dance();
            dancePacket.DanceId = 1;

            Managers.Network.Send(dancePacket);
        }

        void SendSkillPacket(int skillId = 0, bool isCliektSpawn = false)
        {
            if (ServerPosInfo.State == ActorState.Attack)
                return;

            var clickResult = GetClickPosition();
            if (!clickResult.result)
                return;

            var skillDir = clickResult.position - this.transform.position;
            skillDir.Normalize();

            var spawnPosition = isCliektSpawn ? clickResult.position.ToFloat3() : transform.position.ToFloat3();

            var skillPacket = new C_Skill();
            skillPacket.Info = new SkillInfo();
            skillPacket.Info.SkillId = skillId;
            skillPacket.Info.SpawnPosition = spawnPosition;
            skillPacket.Info.SkillDirection = skillDir.ToFloat3();

            Managers.Network.Send(skillPacket);
        }

        void SendTeleportSkillPacekt()
        {
            if (ServerPosInfo.State == ActorState.Attack)
                return;

            var clickResult = GetClickPosition();
            if (!clickResult.result)
                return;

            var currentPosition = transform.position;
            currentPosition.y = 0.0f;

            var skillDir = clickResult.position - currentPosition;
            skillDir.y = 0.0f;
            skillDir.Normalize();

            float teleportLength = 4.0f;

            for (int length = (int)teleportLength; length > 0; --length)
            {
                var targetPosition = currentPosition + (skillDir * (float)length);
                bool isVaild = _level.IsVaildPosition(targetPosition);
                if(isVaild)
                {
                    var skillPacket = new C_Skill();
                    skillPacket.Info = new SkillInfo();
                    skillPacket.Info.SkillId = -1;
                    skillPacket.Info.SpawnPosition = targetPosition.ToFloat3();
                    skillPacket.Info.SkillDirection = skillDir.ToFloat3();
                    Managers.Network.Send(skillPacket);
                    break;
                }
            }
        }

        (bool result, Vector3 position) GetClickPosition()
        {
            var layerMask = LayerMask.NameToLayer("Ground");

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, Mathf.Infinity, ~layerMask))
            {
                return (true, hit.point);
            }

            return (false, Vector3.zero);
        }

        void MakeMakerEffect(Vector3 spawnPosition)
        {
            var makerEffect = GameObjectCache.Make(_makerEffect.transform, this.transform.parent);
            makerEffect.transform.position = spawnPosition;
            GameObjectCache.DeleteDelayed(makerEffect, delayTime: 1.0f);
        }

        protected override void StartTeleport(float teleportTime)
        {
            base.StartTeleport(teleportTime);
            CameraEventHandler.Instance.StartMotionBlur(time: teleportTime, intensity: 5.0f);
        }

        public override void OnDead()
        {
            base.OnDead();

            _inputHandle = null;
        }

        public override void OnResurrection()
        {
            _inputHandle = UpdateKeyInput;
            _inputCheckTime = _inputDelay;
        }
    }
}