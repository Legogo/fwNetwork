using UnityEngine.Networking;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public enum RaiderNwkMessageType {
  NONE, // nothing specific
  CONNECTION, // a new client is connected (on clients)
  CONNECTION_PINGPONG, // server <-> client transaction on new client connection
  DISCONNECTION_PING, DISCONNECTION_PONG,
  ASSIGN_ID
};

public class RaiderNwkMessage : MessageBase
{
  public short messageId = 1000; // nwk print

  public string senderUid = ""; // uniq id on network
  public int token = -1; // transaction token

  //public int messageType = 0; // NONE
  public RaiderNwkMessageType messageType = RaiderNwkMessageType.NONE;

  public string message = "";
  
  public RaiderNwkMessage setupType(RaiderNwkMessageType newType)
  {
    //messageType = (int)newType;
    messageType = newType;
    return this;
  }

  public RaiderNwkMessage generateToken()
  {
    token = Random.Range(0, 9999);
    return this;
  }

  public RaiderNwkMessage assignToken(RaiderNwkMessage originMessage)
  {
    token = originMessage.token;
    return this;
  }

  public bool isSameTransaction(RaiderNwkMessage other)
  {
    return other.token == token;
  }

  public RaiderNwkMessage sendToServer(string senderUid, NetworkClient client)
  {
    this.senderUid = senderUid;
    client.Send(messageId, this);

    Debug.Log(toString());

    return this;
  }

  public RaiderNwkMessage broadcastFromServer()
  {
    this.senderUid = "0";

    NetworkServer.SendToAll(messageId, this);
    
    return this;
  }

  public RaiderNwkMessage sendServerClientTransaction(NetworkMessage receiver = null, Action<RaiderNwkMessage> onTransactionCompleted = null)
  {
    senderUid = "0";

    if (token < 0) generateToken();

    NetworkServer.SendToClient(receiver.conn.connectionId, messageId, this);
    RaiderNwkMessageListener.getListener().add(this, onTransactionCompleted);

    return this;
  }

  public bool isTransactionMessage()
  {
    return token > -1;
  }

  public string toString()
  {
    string ct = "[Msg]("+((RaiderNwkMessageType)messageType).ToString() + ")";
    ct += "\n  from : " + senderUid;
    ct += "\n  token : " + token;
    ct += "\n  " + message;
    return ct;
  }
}
