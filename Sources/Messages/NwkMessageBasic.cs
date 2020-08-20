using UnityEngine.Networking;
using UnityEngine;

[System.Serializable]
public class NwkMessageBasic : MessageBase, iNwkMessageId
{
  public const short MSG_ID_BASIC = MsgType.Highest + 1;
  public short getMessageUnetId() => MSG_ID_BASIC;

  public NwkMessageModIdCard id;
  public NwkMessageModIdCard getIdCard()
  {
    if (id == null) id = new NwkMessageModIdCard();
    return id;
  }

  public bool isSilent()
  {
    if (id.type == (int)eNwkMessageType.PING) return true;
    return false;
  }

}
