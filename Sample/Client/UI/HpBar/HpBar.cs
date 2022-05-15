using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YeongJ.Inagme;

public class HpBar : MonoBehaviour
{
    [SerializeField] GameObject _root;
    [SerializeField] Slider _hpSlider;
    [SerializeField] Slider _bgSlider;
    [SerializeField] Text _levelText;
    [SerializeField] Text _nameText;
    [SerializeField] Image _gagueImage;
    [SerializeField] Color _playerTeamTypeColor;
    [SerializeField] Color _opponentTeamTypeColor;

    public BaseActor Owner { get; private set; }

    private TeamType _playerTeamType = TeamType.Friendly;

    public void SetActor(BaseActor owner)
    {
        Owner = owner;

        if(_nameText != null)
            _nameText.text = Owner.name;

        if (_levelText != null)
            _levelText.text = $"Lv.{Owner.Stat.Level}";

        ChangeHp(Owner.Stat.Hp);
    }

    public void ChangeHp(int hp)
    {
        float ratio = hp <= 0.0f ? 0.0f : (float)hp / (float)Owner.Stat.MaxHp;
        _hpSlider.value = ratio;
    }

    public void SetPlayerTeamType(TeamType teamType)
    {
        _playerTeamType = teamType;
    }

    public void UpdateHpBar()
    {
        if (Owner == null)
            return;

        if (_hpSlider.value == _bgSlider.value)
            return;

        var sliderVelocity = 0.0f;
        _bgSlider.value = Mathf.SmoothDamp(_bgSlider.value, _hpSlider.value, ref sliderVelocity, 0.05f);
    }

    public void UpdateTeamTypeColor()
    {
        if (_gagueImage == null)
            return;

        if (Owner.TeamType == _playerTeamType && Owner.TeamType == TeamType.Friendly && _playerTeamType == TeamType.Friendly)
        {
            _gagueImage.color = _playerTeamTypeColor;
        }
        else
        {
            _gagueImage.color = _opponentTeamTypeColor;
        }
    }

    public void UpdatePpsition()
    {
        if (Owner == null)
            return;

        _root.gameObject.SetActive(false);

        var diff = Owner.UIRoot.transform.position - Camera.main.transform.position;
        diff.y = 0.0f;

        if (diff.z < 0.0f)
            return;
        if (diff.magnitude > 30.0f)
            return;

        var velocity = Vector3.zero;
        var targetPosition = Camera.main.WorldToScreenPoint(Owner.UIRoot.transform.position);
        this.transform.position = Vector3.SmoothDamp(this.transform.position, targetPosition, ref velocity, 0.005f);
        _root.gameObject.SetActive(true);
    }
}
