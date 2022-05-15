using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using YeongJ.Inagme;

namespace YeongJ.UI
{
    public class ChatBubble : MonoBehaviour
    {
        [SerializeField] RectTransform _bubbleRoot;
        [SerializeField] Text _nameText;
        [SerializeField] Text _chatText;
        [SerializeField] float _remainTime;

        int _objectId;
        Vector3 _currentVelocity = Vector3.zero;
        
        BaseActor _baseActor;
        UnityAction<int> _completeCallback;
        
        public void SetData(BaseActor baseActor, string userName, string chatText, UnityAction<int> completeCallback)
        {
            _baseActor = baseActor;
            _nameText.text = userName;
            _chatText.text = chatText;
            _remainTime = 3.0f;
            _completeCallback = completeCallback;

            _objectId = baseActor?.Id ?? 0;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_bubbleRoot);
            
            this.transform.position = Camera.main.WorldToScreenPoint(_baseActor.UIRoot.transform.position);
        }

        public void UpdateData(string chatText)
        {
            _remainTime = 3.0f;
            _chatText.text = chatText;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_bubbleRoot);
        }

        void Update()
        {
            UpdateChat();

            _remainTime -= Time.deltaTime;
            if(_remainTime <= 0f)
            {
                _completeCallback?.Invoke(_objectId);
                _completeCallback = null;
            }
        }

        void UpdateChat()
        {
            if (_baseActor == null)
                return;

            var velocity = Vector3.zero;

            var targetPosition = Camera.main.WorldToScreenPoint(_baseActor.UIRoot.transform.position);
            this.transform.position = Vector3.SmoothDamp(this.transform.position, targetPosition, ref velocity, 0.01f);
        }
    }
}