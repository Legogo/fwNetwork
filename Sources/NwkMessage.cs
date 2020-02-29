using UnityEngine.Networking;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public enum NwkMessageType {
  NONE, // nothing specific
  CONNECTION, // a new client is connected (on clients)
  CONNECTION_PINGPONG, // server <-> client transaction on new client connection
  DISCONNECTION_PING, DISCONNECTION_PONG,
  ASSIGN_ID
};

public class NwkMessage : MessageBase
{
  public short messageId = 1000; // nwk print

  public string senderUid = ""; // uniq id on network
  public int token = -1; // transaction token

  //public int messageType = 0; // NONE
  public NwkMessageType messageType = NwkMessageType.NONE;

  public string message = "";
  
  public NwkMessage setupType(NwkMessageType newType)
  {
    //messageType = (int)newType;
    messageType = newType;
    return this;
  }

  public NwkMessage generateToken()
  {
    token = Random.Range(0, 9999);
    return this;
  }

  public NwkMessage assignToken(NwkMessage originMessage)
  {
    token = originMessage.token;
    return this;
  }

  public bool isSameTransaction(NwkMessage other)
  {
    return other.token == token;
  }

  public NwkMessage sendToServer(string senderUid, NetworkClient client)
  {
    this.senderUid = senderUid;
    client.Send(messageId, this);

    Debug.Log(toString());

    return this;
  }

  public NwkMessage broadcastFromServer()
  {
    this.senderUid = "0";

    NetworkServer.SendToAll(messageId, this);
    
    return this;
  }

  public NwkMessage sendServerClientTransaction(NetworkMessage receiver = null, Action<NwkMessage> onTransactionCompleted = null)
  {
    senderUid = "0";

    if (token < 0) generateToken();

    NetworkServer.SendToClient(receiver.conn.connectionId, messageId, this);
    NwkMessageListener.getListener().add(this, onTransactionCompleted);

    return this;
  }

  public bool isTransactionMessage()
  {
    return token > -1;
  }

  public string toString()
  {
    string ct = "[Msg]("+((NwkMessageType)messageType).ToString() + ")";
    ct += "\n  from : " + senderUid;
    ct += "\n  token : " + token;
    ct += "\n  " + message;
    return ct;
  }
}
