using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// senderUID allows receiver to know who sent the message
/// /!\ every properties must be public or flagged as serialized
/// </summary>
[System.Serializable]
public class NwkMessageFull : MessageBase, iNwkMessageId
{
  public const short MSG_ID_FULL = MsgType.Highest + 5;
  public short getMessageId() => MSG_ID_FULL;


  NwkMessageIdCard id;
  public NwkMessageIdCard getIdCard()
  {
    if (id == null) id = new NwkMessageIdCard();
    return id;
  }

  public bool isSilent() => true;




  //string to transfert
  public string messageHeader = ""; // the actual message

  //data to transfert
  public byte[] messageBytes;

  public void setupHeader(string header) => messageHeader = header;
  public string getHeader() => messageHeader;

  public void setupMessageData(object obj) => messageBytes = serializeObject(obj);
  public object getMessage()
  {
    //needed in debug ctx
    if (messageBytes == null) return null;
    return deserializeObject(messageBytes);
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
