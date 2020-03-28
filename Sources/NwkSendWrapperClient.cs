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
  }

}
