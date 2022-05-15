using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YeongJ.Inagme;

namespace YeongJ.UI
{
    public class OthersUI : UISingleton<OthersUI>
    {
        [SerializeField] Button _teamTypeChangeButton;
        [SerializeField] Image _teamTypeIcon;

        public void Awake()
        {
            _teamTypeChangeButton.onClick.AddListener(OnClickTeamTypeButton);
        }

        public void RefreshTeamType(TeamType teamType)
        {
            var alpha = _teamTypeIcon.color.a;
            var newColor = teamType == TeamType.Friendly ? Color.white : Color.red;
            newColor.a = alpha;

            _teamTypeIcon.color = newColor;
        }

        private void OnClickTeamTypeButton()
        {
            Managers.Network.Send(new C_ChangeTeam());
        }
    }

}