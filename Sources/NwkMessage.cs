using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public enum NwkMessageType {
  NONE, // nothing specific
  CONNECTION, // a new client is connected (on clients) ; msg contains uid
  CONNECTION_PINGPONG, // server <-> client transaction on new client connection
  DISCONNECTION_PING, DISCONNECTION_PONG,
  ASSIGN_ID,
  PING,PONG
};

/// <summary>
/// senderUID allows receiver to know who sent the message
/// /!\ every properties must be public or flagged as serialized
/// </summary>
public class NwkMessage : MessageBase
{
  public short messageId = 1000; // nwk print (:shrug:)
  
  public string senderUid = "-1"; // uniq id on network, server is 0
  public int messageScope = 0; // 0 is basic msg ; all above is a specific way to discriminate
  public int messageType = 0;
  
  public string messageHeader = ""; // the actual message
  public byte[] messageBytes;

  public int token = -1; // transaction token (not needed for one way transaction) ; ONLY WORKS for scope of 0
  public bool silentLogs = false; // no logs

  public NwkMessage clean()
  {
    senderUid = "-1";
    messageScope = 0;
    messageType = 0;
    messageHeader = "";
    messageBytes = null;
    token = -1;
    silentLogs = false;
    return this;
  }

  public void setSender(string senderUid) => this.senderUid = senderUid;
  public void setScope(int newScope) => messageScope = newScope;
  public void setupNwkType(NwkMessageType typ) => messageType = (int)typ;
  public void setupNwkType(int typ) => messageType = typ;
  
  public void setupMessage(string header) => messageHeader = header;
  public void setupMessageData(object obj) => messageBytes = serializeObject(obj);
  
  public object getMessage() => deserializeObject(messageBytes);
  public string getHeader() => messageHeader;

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

}
