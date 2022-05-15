using Google.Protobuf.Protocol;
using UnityEngine;

namespace YeongJ.Inagme
{
    public class BaseActor : MonoBehaviour
    {
        [SerializeField] protected GameObject _model;
        [SerializeField] protected GameObject _uIRoot;
        [SerializeField] protected GameObject _actorRoot;
        [SerializeField] protected Animator _animator;
        [SerializeField] protected float _groundedRayDistance = 30f;

        [Header("Effect")]
        [SerializeField] protected GameObject _hitEffect;

        public int Id { get; private set; }
        public StatInfo Stat { get { return _stat; } set { _stat = value; } }
        public PositionInfo ServerPosInfo { get { return _serverPosInfo; } set { _serverPosInfo = value; } }
        public GameObject UIRoot => _uIRoot;
        public GameObject ActorRoot => _actorRoot ?? null;
        public GameObject HitEffect => _hitEffect ?? null;
        public TeamType TeamType => _teamType;

        protected float _positionLerpTime;
        protected float _currentPositionLerpTime;
        protected Vector3 _currentVelocity;

        protected StatInfo _stat;
        protected PositionInfo _serverPosInfo;
        protected PositionInfo _prevServerPosInfo;
        protected float _currentAnimatorVelocity;

        protected TeamType _teamType;

        protected delegate void InputHandle();
        protected delegate void CommandHandle();

        protected InputHandle _inputHandle = null;
        protected CommandHandle _commandHandle = null;


        public Vector3 ServerDir
        {
            get
            {
                if (_serverPosInfo.Direction == null)
                    _serverPosInfo.Direction = Vector3.zero.ToFloat3();

                return _serverPosInfo.Direction.ToVector3();
            }
            set
            {
                if (_serverPosInfo.Direction == null)
                    _serverPosInfo.Direction = Vector3.zero.ToFloat3();

                _serverPosInfo.Direction = value.ToFloat3();
            }
        }

        public Vector3 ServerPos
        {
            get
            {
                if (_serverPosInfo.Position == null)
                    _serverPosInfo.Position = Vector3.zero.ToFloat3();

                return _serverPosInfo.Position.ToVector3();
            }
            set
            {
                if (_serverPosInfo.Position == null)
                    _serverPosInfo.Position = Vector3.zero.ToFloat3();

                _serverPosInfo.Position = value.ToFloat3();
            }
        }

        public Vector3 PrevServerPos
        {
            get
            {
                if (_prevServerPosInfo == null)
                    _prevServerPosInfo = _serverPosInfo.Clone();

                if (_prevServerPosInfo.Position == null)
                    _prevServerPosInfo.Position = ServerPos.ToFloat3();

                return _prevServerPosInfo.Position.ToVector3();
            }
        }



        public Vector3 ServerLookDir
        {
            get
            {
                if (_serverPosInfo.LookDirection == null)
                    _serverPosInfo.LookDirection = Vector3.down.ToFloat3();

                return _serverPosInfo.LookDirection.ToVector3();
            }
            set
            {
                _serverPosInfo.LookDirection = value.ToFloat3();
            }
        }

        public virtual void Init(int Id)
        {
            this.Id = Id;
            _commandHandle = UpdateCommandIdleMove;
        }

        public virtual void SetServerPos(PositionInfo posInfo)
        {
            _prevServerPosInfo = _serverPosInfo;
            _serverPosInfo = posInfo;
            _currentPositionLerpTime = _positionLerpTime = Const.FrameTime + Managers.Network.Latency + Const.MoveLerpDelay;
        }

        public virtual void SetTeamType(TeamType teamType)
        {
            _teamType = teamType;
        }

        public virtual void Remove() { }

        public void Update()
        {
            UpdateActor();
        }

        public void UpdateActor()
        {
            ProcessInput();
            ProcessCommand();
        }

        public virtual void ProcessCommand()
        {
            if (_commandHandle == null)
                return;

            _commandHandle();
        }

        public virtual void ProcessInput()
        {
            if (_inputHandle == null)
                return;

            _inputHandle();
        }

        protected virtual void UpdateCommandIdleMove()
        {
            UpdateMove();
            UpdateRotation();
        }

        public virtual void SyncPos()
        {
            transform.position = ServerPos;
            UpdateHeight();
            UpdateRotation(isLerp: false);
        }

        protected virtual void UpdateMove()
        {
            var currentPosition = this.transform.position;
            currentPosition.y = 0.0f;
            Vector3 newPositon;

            if (ServerDir != Vector3.zero)
            {
                float speed = Time.deltaTime * Mathf.Max(1.0f, (Stat.Speed - Stat.Speed * 0.1f));
                newPositon = currentPosition + ServerDir * speed;
                _currentAnimatorVelocity = 0.2f;
            }
            else
            {
                newPositon = Vector3.SmoothDamp(currentPosition, ServerPos, ref _currentVelocity, 0.05f);
                _currentAnimatorVelocity = Mathf.Clamp01(_currentAnimatorVelocity - Time.deltaTime);
            }

            transform.position = newPositon;
            UpdateHeight();


            if (_animator != null)
            {
                _animator.SetFloat("Velocity", _currentAnimatorVelocity);
            }
        }

        protected virtual void UpdateHeight()
        {
            var layerMask = LayerMask.NameToLayer("Ground");

            RaycastHit hit;
            Ray ray = new Ray(transform.position + Vector3.up * _groundedRayDistance, -Vector3.up);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~layerMask))
            {
                var currentPosition = this.transform.position;
                currentPosition.y = hit.point.y;
                this.transform.position = currentPosition;
            }
        }

        protected virtual void UpdateRotation(bool isLerp = true)
        {
            if (_model == null)
                return;

            if (isLerp)
            {
                _model.transform.rotation = Quaternion.Lerp(_model.transform.rotation, Quaternion.LookRotation(ServerLookDir), Time.deltaTime * 10f);
            }
            else
            {
                _model.transform.rotation = Quaternion.LookRotation(ServerLookDir);
            }
        }

        public virtual void OnHit()
        {
            _animator.SetTrigger(Const.TriggerHit);
        }

        public virtual void OnDead()
        {
            _animator.SetTrigger(Const.TriggerDeath);
        }

        public virtual void OnResurrection() { }

        public virtual void OnDance() { }
        public virtual void UseSkill(SkillInfo skillInfo) { }
    }
}