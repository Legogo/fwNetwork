
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

/// <summary>
/// can't serialize short ?
/// https://forum.unity.com/threads/serialize-short-ushort-etc.150952/
/// </summary>

public enum eNwkMessageType
{
  NONE = 0, // nothing specific
  CONNECTION, // a new client is connected (on clients) ; msg contains uid
  DISCONNECTION, // a client tells server that it will disconnect
  CONNECTION_PINGPONG = 3, // server <-> client transaction on new client connection
  SRV_DISCONNECTION_PING, CLT_DISCONNECTION_PONG = 5, // server broadcast ping ; all connected client must answer pong
  ASSIGN_ID = 6,
  PING = 7, PONG = 8, // ping module
  SYNC, SYNC_ONCE // syncables ; only for server to receive and keep track of data (NOT client <-> client)
};

public interface iNwkMessage
{
  short getMessageId();
}

public interface iNwkMessageId : iNwkMessage
{
  NwkMessageModIdCard getIdCard();
  bool isSilent();
  
}

[System.Serializable]
public class NwkMessageModIdCard
{

  public int type;
  public int sender; // nwk uid



  public void setupId(int nwkUid, int type)
  {
    sender = nwkUid; this.type = type;
  }

  public void setMessageType(eNwkMessageType newType) => type = (int)newType;
  public void setMessageType(int newType) => type = newType;
  public int getMessageType() => type;

  public void setMessageSender(int newNwkUid) => sender = newNwkUid;
  public int getMessageSender() => sender;

  public string toString() => sender + "||" + type;
}

[System.Serializable]
public class NwkMessageModBytes
{
  //data to transfert
  public byte[] messageBytes;

  public void setupByteData(object obj) => messageBytes = serializeObject(obj);
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

[System.Serializable]
public class NwkMessageModHeader
{

  //string to transfert
  public string messageHeader = ""; // the actual message

  public void setupHeader(string header) => messageHeader = header;
  public string getHeader() => messageHeader;

}