﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class NwkSendWrapperServer : NwkSendWrapper
{/// <summary>
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
  /// this function needs to provide the client connection id
  /// </summary>
  public void sendServerAnswerToSpecificClient(NwkMessage msg, int clientConnectionId)
  {
    msg.setSender("0");
    NetworkServer.SendToClient(clientConnectionId, msg.messageId, msg);
  }

  [Obsolete("use broadcast ; can't target specific client outside of answering flow")]
  public void sendServerToClientsBut(NwkMessage msg, string filterNwkUid)
  {
    msg.setSender("0"); // server

    List<NwkClientData> clients = NwkServer.nwkServer.clientDatas;
    for (int i = 0; i < clients.Count; i++)
    {
      if(clients[i].nwkUid != filterNwkUid)
      {
        //NetworkServer.SendToClient(clients[i]);
      }
    }
  }

  /// <summary>
  /// SERVER
  /// bridge to broadcast message to everyone
  /// only for server
  /// </summary>
  public void broadcastServerToAll(NwkMessage msg, string senderUid)
  {
    msg.setSender(senderUid);
    NetworkServer.SendToAll(msg.messageId, msg);
  }

}