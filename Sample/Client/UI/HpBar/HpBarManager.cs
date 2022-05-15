using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YeongJ.Inagme;

namespace YeongJ.UI
{
    public class HpBarManager : UISingleton<HpBarManager>
    {
        [SerializeField] HpBar _templateHpBar;
        [SerializeField] HpBar _myHpBar;
        [SerializeField] RectTransform _hpBarRoot;

        private Dictionary<int, HpBar> _hpBars = new Dictionary<int, HpBar>();
        private TeamType _playerTeamType = TeamType.Friendly;
        private int _myObjectId;

        void Update()
        {
            var d_enum = _hpBars.GetEnumerator();
            while(d_enum.MoveNext())
            {
                d_enum.Current.Value.UpdateHpBar();
                d_enum.Current.Value.UpdatePpsition();
                d_enum.Current.Value.UpdateTeamTypeColor();
            }

            _myHpBar.UpdateHpBar();
        }

        public void AddMyHpBar(int objectId)
        {
            var ownerActor = GetActor(objectId);
            if (ownerActor == null)
                return;

            _myObjectId = objectId;
            _myHpBar.SetActor(ownerActor);
            _myHpBar.SetPlayerTeamType(_playerTeamType);
        }

        public void SetPlayerTeamType(TeamType teamType)
        {
            _playerTeamType = teamType;

            var d_enum = _hpBars.GetEnumerator();
            while (d_enum.MoveNext())
            {
                d_enum.Current.Value.SetPlayerTeamType(teamType);
            }
        }

        public void AddHpBar(int objectId)
        {
		    GameObjectType objectType = ObjectManager.GetObjectTypeById(objectId);
            if (objectType == GameObjectType.Skill)
                return;

            RemoveHpBar(objectId);

            var owner = GetActor(objectId);
            if (owner == null)
                return;

            var newHpBar = GameObjectCache.Make(_templateHpBar, _hpBarRoot);
            newHpBar.SetActor(owner);
            _hpBars.Add(objectId, newHpBar);
        }

        public void ChangeHpBar(int objectId, int hp)
        {
            if(_myObjectId == objectId)
            {
                _myHpBar.ChangeHp(hp);
                return;
            }

            if (!_hpBars.ContainsKey(objectId))
            {
                return;
            }

            _hpBars[objectId].ChangeHp(hp);
        }

        public void RemoveHpBar(int objectId)
        {
            if (!_hpBars.ContainsKey(objectId))
                return;

            GameObjectCache.Delete(_hpBars[objectId].transform);
            _hpBars.Remove(objectId);
        }

        BaseActor GetActor(int objectId)
        {
            return Managers.Object.FindById(objectId)?.GetComponent<BaseActor>();
        }
    }
}