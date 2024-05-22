using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Mirror.Examples.Chat;
using System.Collections.Generic;
using System.Collections;

public class ChattingUI : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] Text Text_ChatHistory;
    [SerializeField] Scrollbar ScrollBar_Chat;
    [SerializeField] InputField Input_ChatMsg;
    [SerializeField] Button Btn_Send;

    internal static string _localPlayerName;

    // 서버온리 - 연결된 플레이어들 이름
    internal static readonly Dictionary<NetworkConnectionToClient, string> _connectedNameDic = new Dictionary<NetworkConnectionToClient, string>();

    public override void OnStartServer()
    {
        _connectedNameDic.Clear();
    }

    public override void OnStartClient()
    {
        Text_ChatHistory.text = string.Empty;
    }

    [Command(requiresAuthority = false)]
    void CommandSendMsg(string msg, NetworkConnectionToClient sender = null)
    {
        if (!_connectedNameDic.ContainsKey(sender))
        {
            var player = sender.identity.GetComponent<Player>();
            var playerName = player.playerName;
            _connectedNameDic.Add(sender, playerName);
        }

        if (!string.IsNullOrWhiteSpace(msg))
        {
            var senderName = _connectedNameDic[sender];
            OnRecvMessage(senderName, msg.Trim());
        }
    }

    [ClientRpc]
    void OnRecvMessage(string senderName, string msg)
    {
        string formatedMsg = (senderName == _localPlayerName) ?
            $"<color=red>{senderName}:</color> {msg}" :
            $"<color=blue>{senderName}:</color> {msg}";

        AppendMessage(formatedMsg);
    }

    // ===================== [UI] =================================
    void AppendMessage(string msg)
    {
        StartCoroutine(AppendAndScroll(msg));
    }

    IEnumerator AppendAndScroll(string msg)
    {
        Text_ChatHistory.text += msg + "\n";

        yield return null;
        yield return null;

        ScrollBar_Chat.value = 0;
    }

    // ============================================================

    public void OnClick_SendMsg()
    {
        var currentChatMsg = Input_ChatMsg.text; 
        if (!string.IsNullOrWhiteSpace(currentChatMsg))
        {
            CommandSendMsg(currentChatMsg.Trim());
        }
    }
}
