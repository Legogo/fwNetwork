using UnityEngine.Networking;

[System.Serializable]
public class NwkMessageBasic : MessageBase, iNwkMessageId
{
  public const short MSG_ID_BASIC = MsgType.Highest + 1;
  public short getMessageId() => MSG_ID_BASIC;

  public NwkMessageIdCard id;
  public NwkMessageIdCard getIdCard()
  {
    if (id == null) id = new NwkMessageIdCard();
    return id;
  }

  public bool isSilent()
  {
    if (id.type == (int)eNwkMessageType.PING) return true;
    return false;
  }

}
