using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public enum NwkMessageType
{
  NONE, // nothing specific
  CONNECTION, // a new client is connected (on clients) ; msg contains uid
  DISCONNECTION, // a client tells server that it will disconnect
  CONNECTION_PINGPONG, // server <-> client transaction on new client connection
  SRV_DISCONNECTION_PING, CLT_DISCONNECTION_PONG, // server broadcast ping ; all connected client must answer pong
  ASSIGN_ID,
  PING,PONG, // ping module
  SYNC, SYNC_ONCE // syncables ; only for server to receive and keep track of data (NOT client <-> client)
};

public enum NwkMessageMods
{
  NONE
  //CONTROLLER_UPDATE // mod controller send/receive data
};

public enum NwkMessageScope
{
  BASIC,
  MODS,
  CUSTOM
}

/// <summary>
/// senderUID allows receiver to know who sent the message
/// /!\ every properties must be public or flagged as serialized
/// </summary>
public class NwkMessage : MessageBase
{
  public short messageId = 1000; // nwk print (:shrug:)
  
  public string senderUid = ""; // uniq id on network, server is 0
  public int messageScope = 0; // 0 is basic msg ; 1 mods ; 2 custom ; all above is a specific way to discriminate
  public int messageType = 0;
  
  public string messageHeader = ""; // the actual message
  public byte[] messageBytes;

  public int token = -1; // transaction token (not needed for one way transaction) ; ONLY WORKS for scope of 0
  public bool silentLogs = false; // no logs

  public bool broadcast = false; // to aim other clients (but sender)

  public NwkMessage clean()
  {
    senderUid = "-1";
    messageScope = 0;
    messageType = 0;
    messageHeader = "";
    messageBytes = null;
    token = -1;
    silentLogs = false;
    broadcast = false;

    return this;
  }

  public void setSender(string senderUid) => this.senderUid = senderUid;

  /// <summary>
  /// it's better to set scope through a type setup method
  /// </summary>
  //public void setScope(int newScope) => messageScope = newScope;

  public void setupNwkType(NwkMessageType typ) => setupNwkScopedType(NwkMessageScope.BASIC, (int)typ);
  public void setupNwkType(NwkMessageMods typ) => setupNwkScopedType(NwkMessageScope.MODS, (int)typ);
  public void setupNwkCustomType(int typ) => setupNwkScopedType(NwkMessageScope.CUSTOM, typ);
  
  public void setupNwkScopedType(NwkMessageScope scope, int typ)
  {
    messageScope = (int)scope;
    messageType = typ;
  }

  public void setupHeader(string header) => messageHeader = header;
  public string getHeader() => messageHeader;

  public void setupMessageData(object obj) => messageBytes = serializeObject(obj);
  public object getMessage()
  {
    //needed in debug ctx
    if (messageBytes == null) return null;
    return deserializeObject(messageBytes);
  }
  
  public bool cmpMessageType(NwkMessageType typ) => messageType == (int)typ;

  public void generateToken()
  {
    if (token > -1) return;
    token = Random.Range(0, 9999);

    if (messageScope > 0) Debug.LogError("can't use transaction for scopes != 0");
  }

  /// <summary>
  /// if message as a token, it's a transaction
  /// </summary>
  /// <returns></returns>
  public bool isTransactionMessage() => token > -1;

  public bool isSameTransaction(NwkMessage other) => other.token == token;

  public string toString()
  {
    string ct = "["+GetType()+"] (scope ? "+messageScope+" , type ? " + messageType + ")";

    ct += "\n  from : " + senderUid;
    
    if(token > -1) ct += "\n  token : " + token;
    if(messageHeader.Length > 0) ct += "\n  message : " + messageHeader;

    return ct;
  }

  public static byte[] serializeObject(object obj)
  {
    MemoryStream stream = new MemoryStream();
    BinaryFormatter bf = new BinaryFormatter();

    bf.Serialize(stream, obj);

    return stream.GetBuffer();
  }

  public static object deserializeObject(byte[] buffer)
  {
    MemoryStream stream = new MemoryStream(buffer);
    BinaryFormatter bf = new BinaryFormatter();

    return bf.Deserialize(stream);
  }

  static public NwkMessage getStandardMessage(string sender, NwkMessageType msgType)
  {
    NwkMessage msg = new NwkMessage();
    msg.setSender(sender);
    msg.setupNwkType(msgType);
    msg.silentLogs = true;
    return msg;
  }

}
