using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// senderUID allows receiver to know who sent the message
/// /!\ every properties must be public or flagged as serialized
/// </summary>
[System.Serializable]
public class NwkMessageFull : MessageBase, iNwkMessageId
{
  public const short MSG_ID_FULL = MsgType.Highest + 5;
  public short getMessageUnetId() => MSG_ID_FULL;

  public NwkMessageModHeader header;
  public NwkMessageModBytes bytes;
  public NwkMessageModIdCard id;

  public NwkMessageFull():base()
  {
    id = new NwkMessageModIdCard();
    header = new NwkMessageModHeader();
    bytes = new NwkMessageModBytes();
  }

  public NwkMessageModIdCard getIdCard() => id;
  public bool isSilent() => true;

}
