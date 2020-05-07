using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NwkMessageComplexe : MessageBase, iNwkMessageId
{
  public const short MSG_ID_COMPLEXE = MsgType.Highest + 2;
  public short getMessageUnetId() => MSG_ID_COMPLEXE;

  public bool isSilent() => true;

  NwkMessageModIdCard id;
  public NwkMessageModIdCard getIdCard()
  {
    if (id == null) id = new NwkMessageModIdCard();
    return id;
  }

  //need to be mask
  public bool broadcast = false; // to aim other clients (but sender)

}
