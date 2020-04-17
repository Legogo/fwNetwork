using UnityEngine.Networking;

public class NwkSendWrapperClient : NwkSendWrapper
{
  NetworkClient unetClient;

  public NwkSendWrapperClient(NetworkClient unetClient)
  {
    this.unetClient = unetClient;
  }

  /// <summary>
  /// CLIENT
  /// </summary>
  /// <param name="msg"></param>
  public void sendClientToServer(NwkMessage msg)
  {
    msg.senderUid = NwkClient.nwkUid; // assign client id before sending
    unetClient.Send(msg.messageId, msg);
    //unetClient.SendUnreliable
    //NwkSystemBase.nwkSys.log("sent message of type : " + msg.messageType);
  }

  public void sendClientToClients(NwkMessage msg)
  {
    msg.senderUid = NwkClient.nwkUid; // assign client id before sending
    msg.broadcast = true;

    unetClient.Send(msg.messageId, msg);

    NwkSystemBase.nwkSys.log("sent message of type : " + msg.messageType);
  }
}
