using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NwkMessageCustom : MessageBase, iNwkMessageId
{
  public const short MSG_ID_CUSTOM = MsgType.Highest + 3;
  public short getMessageId() => MSG_ID_CUSTOM;

  NwkMessageIdCard id;
  public NwkMessageIdCard getIdCard()
  {
    if (id == null) id = new NwkMessageIdCard();
    return id;
  }

  public bool isSilent() => true;
}
