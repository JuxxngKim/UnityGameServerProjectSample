using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Google.Protobuf.Protocol;
using YeongJ.Inagme;

namespace YeongJ.UI
{
    public class ChatManager : UISingleton<ChatManager>
    {
        [SerializeField] InputField _inputField;
        [SerializeField] ScrollRect _scrollRect;
        [SerializeField] Text _templateChatText;
        [SerializeField] Text _imeIsSelectedText;
        [SerializeField] RectTransform _contentRoot;
        [SerializeField] RectTransform _bubbleRoot;
        [SerializeField] ChatBubble _templateChatBubble;

        List<Text> _chatList = new List<Text>();
        Dictionary<int, ChatBubble> _chatBubbles = new Dictionary<int, ChatBubble>();

        public static bool InputLock = false;

        public override void InitSingleton()
        {
            base.InitSingleton();

            _inputField.onEndEdit.AddListener(SendChat);
        }

        public void Update()
        {
            if(Input.GetKeyDown(KeyCode.Return) && _inputField.gameObject.activeInHierarchy)
            {
                if(!_inputField.isFocused)
                {
                    _inputField.ActivateInputField();
                    _inputField.Select();
                }
            }

            if (_inputField.gameObject.activeInHierarchy)
            {
                _imeIsSelectedText.text = Input.imeIsSelected ? "°¡" : "a";
            }

            InputLock = _inputField.isFocused && _inputField.gameObject.activeInHierarchy;
        }

        public void SendChat(string text)
        {
            var userChat = text;
            if (userChat == string.Empty)
                return;

            C_Chat chatPacket = new C_Chat();
            chatPacket.Chat = text;
            Managers.Network.Send(chatPacket);
            
            _inputField.text = string.Empty;
            _inputField.ActivateInputField();
            _inputField.Select();
        }

        public void AddChat(int objectId, string userName, string userChat)
        {
            string chat = $"[{System.DateTime.Now.Hour:D2}:{System.DateTime.Now.Minute:D2}] {userName} : {userChat}";

            var newText =  GameObjectCache.Make<Text>(_templateChatText, _contentRoot);
            newText.text = chat;
            _chatList.Add(newText);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_contentRoot);

            if (_chatBubbles.ContainsKey(objectId))
            {
                _chatBubbles[objectId].UpdateData(userChat);
            }
            else
            {
                var baseActor = Managers.Object.FindById(objectId)?.GetComponent<BaseActor>();
                if (baseActor == null)
                    return;

                var newBubble = GameObjectCache.Make<ChatBubble>(_templateChatBubble, _bubbleRoot);
                _chatBubbles.Add(objectId, newBubble);

                _chatBubbles[objectId].SetData(baseActor, userName, userChat, RemoveBubbleChat);
            }

            StartCoroutine(RefreshScrollPoisition());
        }

        System.Collections.IEnumerator RefreshScrollPoisition()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            _scrollRect.verticalNormalizedPosition = 0.0f;
        }

        private void RemoveBubbleChat(int objectId)
        {
            if (_chatBubbles.ContainsKey(objectId))
            {
                GameObjectCache.Delete(_chatBubbles[objectId].transform);
                _chatBubbles.Remove(objectId);
            }
        }
    }
}