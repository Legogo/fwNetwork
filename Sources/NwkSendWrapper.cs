using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class NwkSendWrapper
{
  /// <summary>
  /// SERVER
  /// server -> send -> client
  /// </summary>
  public void sendServerToClientTransaction(NwkMessage msg, int clientConnectionId, Action<NwkMessage> onTransactionCompleted = null)
  {
    msg.setSender("0");
    msg.generateToken(); // a token for when the answer arrives

    NetworkServer.SendToClient(clientConnectionId, msg.messageId, msg);
    NwkMessageListener.getListener().add(msg, onTransactionCompleted);
  }

  /// <summary>
  /// SERVER
  /// </summary>
  /// <param name="msg"></param>
  /// <param name="clientConnectionId"></param>
  public void sendServerToSpecificClient(NwkMessage msg, int clientConnectionId)
  {
    msg.setSender("0");
    NetworkServer.SendToClient(clientConnectionId, msg.messageId, msg);
  }

  /// <summary>
  /// SERVER
  /// bridge to broadcast message to everyone
  /// only for server
  /// </summary>
  public void broadcastServerToAll(NwkMessage msg)
  {
    msg.setSender("0");
    NetworkServer.SendToAll(msg.messageId, msg);
  }

  /// <summary>
  /// CLIENT
  /// </summary>
  /// <param name="msg"></param>
  public void sendClientToServer(NwkMessage msg)
  {
    msg.senderUid = NwkClient.nwkUid; // assign client id before sending
    NwkClient.nwkClient.client.Send(msg.messageId, msg);
  }

}
