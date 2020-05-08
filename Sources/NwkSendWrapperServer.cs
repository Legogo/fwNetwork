using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using UnityEngine;

/// <summary>
/// 
/// SERVER TO CLIENT
/// 
/// </summary>

public class NwkSendWrapperServer : NwkSendWrapper
{

  /// <summary>
  /// SERVER
  /// this function needs to provide the client connection id
  /// </summary>
  public void sendToSpecificClient(iNwkMessageId message, int clientConnectionId)
  {
    NetworkServer.SendToClient(clientConnectionId, message.getMessageUnetId(), message as MessageBase);
  }

  /// <summary>
  /// SERVER
  /// server -> send -> client
  /// </summary>
  public void sendTransaction(NwkMessageTransaction message, int clientConnectionId, Action<NwkMessageTransaction> onTransactionCompleted = null)
  {
    message.getIdCard().setMessageSender(0); // server is sender
    message.generateToken(); // won't change token if already setup, a token for when the answer arrives

    Debug.Assert(message.getMessageUnetId() == NwkMessageTransaction.MSG_ID_TRANSACTION, "trying to send transaction message with a message that is not a transaction message ; "+message.getMessageUnetId()+" vs "+NwkMessageTransaction.MSG_ID_TRANSACTION);
    Debug.Assert(message.getIdCard().getMessageType() >= 0, "message type is not setup ?");
    Debug.Assert(message.token >= 0, "token is not setup");

    NwkSystemBase.nwkSys.log("sending transaction ("+ message.getMessageUnetId()+") token ? " + message.token+" type ? "+ message.getIdCard().getMessageType());

    NetworkServer.SendToClient(clientConnectionId, message.getMessageUnetId(), message);
    NwkMessageListener.getListener().add(message, onTransactionCompleted);
  }

  /// <summary>
  /// SERVER
  /// bridge to broadcast message to everyone
  /// only for server
  /// 
  /// senderUid can be different if trying to forward some data from a client to all other clients
  /// </summary>
  public void broadcastServerToAll(iNwkMessageId message, int senderUid = 0)
  {
    message.getIdCard().setMessageSender(senderUid);
    NetworkServer.SendToAll(message.getMessageUnetId(), message as MessageBase);
  }

}
