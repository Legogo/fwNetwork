using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NwkMessageTransaction : MessageBase, iNwkMessageId
{
  public const short MSG_ID_TRANSACTION = MsgType.Highest + 4;
  public short getMessageId() => MSG_ID_TRANSACTION;

  public NwkMessageModIdCard id;
  public NwkMessageModIdCard getIdCard()
  {
    if (id == null) id = new NwkMessageModIdCard();
    return id;
  }

  public bool isSilent() => false;




  //transaction
  public short token = -1; // transaction token (not needed for one way transaction) ; ONLY WORKS for scope of 0

  public void generateToken()
  {
    if (token > -1) return;
    token = (short)Random.Range(0, 9999);

    //if (messageScope > 0) Debug.LogError("can't use transaction for scopes != 0");
  }

  public bool isSameTransaction(NwkMessageTransaction other) => other.token == token;

}
