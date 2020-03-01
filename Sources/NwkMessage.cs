using UnityEngine.Networking;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public enum NwkMessageType {
  NONE, // nothing specific
  CONNECTION, // a new client is connected (on clients) ; msg contains uid
  CONNECTION_PINGPONG, // server <-> client transaction on new client connection
  DISCONNECTION_PING, DISCONNECTION_PONG,
  ASSIGN_ID
};

/// <summary>
/// senderUID allows receiver to know who sent the message
/// </summary>

public class NwkMessage : MessageBase
{
  public short messageId = 1000; // nwk print (:shrug:)

  public string senderUid = "0"; // uniq id on network, server is 0
  
  public NwkMessageType nwkMsgType = NwkMessageType.NONE;

  public int token = -1; // transaction token (not needed for one way transaction)
  
  public string message = ""; // the actual message
  
  public void setSender(string senderUid)
  {
    this.senderUid = senderUid;
  }

  public void setupNwkType(NwkMessageType typ)
  {
    nwkMsgType = typ;
  }

  public void setupMessage(string msg)
  {
    message = msg;
  }

  public void generateToken()
  {
    if (token > -1) return;
    token = Random.Range(0, 9999);
  }

  /// <summary>
  /// if message as a token, it's a transaction
  /// </summary>
  /// <returns></returns>
  public bool isTransactionMessage() => token > -1;

  public bool isSameTransaction(NwkMessage other) => other.token == token;

  public string toString()
  {
    string ct = "[Msg]("+((NwkMessageType)nwkMsgType).ToString() + ")";
    ct += "\n  from : " + senderUid;
    ct += "\n  token : " + token;
    ct += "\n  " + message;
    return ct;
  }
}
