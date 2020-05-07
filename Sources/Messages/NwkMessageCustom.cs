using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// inherit but do not use as is
/// </summary>
[System.Obsolete("old deprecated logic, don't use custom, create your own")]
public class NwkMessageCustom : MessageBase, iNwkMessageId
{
  public const short MSG_ID_CUSTOM = MsgType.Highest + 3;
  public short getMessageUnetId() => MSG_ID_CUSTOM;

  public NwkMessageModIdCard id;

  public NwkMessageCustom():base()
  {
    id = new NwkMessageModIdCard();
  }
  
  public NwkMessageModIdCard getIdCard() => id;
  public bool isSilent() => true;
}
