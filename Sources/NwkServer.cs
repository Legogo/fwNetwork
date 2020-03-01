using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class NwkServer : NwkSystemBase
{
  static public NwkServer nwkServer;

  int port = 9999;
  int maxConnections = 10;

  protected override void Awake()
  {
    base.Awake();

    nwkServer = this;

    // Usually the server doesn't need to draw anything on the screen
    Application.runInBackground = true;

    Screen.SetResolution(1280, 720, false);
  }

  override protected void setup()
  {
    //auto create
    CreateServer();
  }

  void CreateServer()
  {
    // Register handlers for the types of messages we can receive
    RegisterHandlers();

    var config = new ConnectionConfig();
    // There are different types of channels you can use, check the official documentation
    config.AddChannel(QosType.ReliableFragmented);
    config.AddChannel(QosType.UnreliableFragmented);

    var ht = new HostTopology(config, maxConnections);

    if (!NetworkServer.Configure(ht))
    {
      log("No server created, error on the configuration definition");
      return;
    }
    else
    {
      // Start listening on the defined port
      if (NetworkServer.Listen(port)) log("Server created, listening on port: " + port);
      else log("No server created, could not listen to the port: " + port);
    }

    onServerReady();
  }

  virtual protected void onServerReady()
  { }

  void OnApplicationQuit()
  {
    NetworkServer.Shutdown();
  }

  private void RegisterHandlers()
  {
    // Unity have different Messages types defined in MsgType
    NetworkServer.RegisterHandler(MsgType.Connect, OnClientConnected);
    NetworkServer.RegisterHandler(MsgType.Disconnect, OnClientDisconnected);

    // Our message use his own message type.
    NetworkServer.RegisterHandler(messageID, OnMessageReceived);
  }

  private void RegisterHandler(short t, NetworkMessageDelegate handler)
  {
    NetworkServer.RegisterHandler(t, handler);
  }



  void OnClientConnected(NetworkMessage clientConnectionMessage)
  {
    log("OnClientConnected");

    //solving new uid for client
    connection_askForUid(clientConnectionMessage.conn.connectionId);

  }
  
  void OnClientDisconnected(NetworkMessage netMessage)
  {
    log("OnClientDisconnected");

    //todo
    //ping pong avec tt le monde pour savoir qui a deco ...

    broadcastDisconnectionPing();
  }

  void OnMessageReceived(NetworkMessage netMessage)
  {
    log("OnMessageReceived : "+netMessage.msgType);

    // You can send any object that inherence from MessageBase
    // The client and server can be on different projects, as long as the MyNetworkMessage or the class you are using have the same implementation on both projects
    // The first thing we do is deserialize the message to our custom type

    NwkMessage objectMessage = null;

    try { objectMessage = netMessage.ReadMessage<NwkMessage>(); }
    catch { objectMessage = null; }

    if(objectMessage != null)
    {
      log(objectMessage.toString());

      switch (objectMessage.nwkMsgType)
      {
        case NwkMessageType.DISCONNECTION_PONG:
          log("received disconnection pong from " + objectMessage.senderUid);
          clients[objectMessage.senderUid].resetTimeout();
          break;
        default:
          break;
      }

      if (objectMessage.isTransactionMessage())
      {
        log(listener.getStackCount() + " transaction(s) before solving");

        listener.solveReceivedMessage(objectMessage); // check for pongs
        log(listener.getStackCount() + " transaction(s) after solving");
        log(listener.toString());
      }
    }
    else
    {
      log("server couldn't read standard NwkMesssage ; passing it on");
      onNewNwkMessage(netMessage);
    }
    
  }

  virtual protected void onNewNwkMessage(NetworkMessage msg)
  { }

  void connection_askForUid(int connId)
  {
    NwkMessage outgoingMessage = new NwkMessage();
    outgoingMessage.setSender("0");
    outgoingMessage.setupNwkType(NwkMessageType.CONNECTION_PINGPONG);

    //give message to listener system to plug a callback
    sendServerToClientTransaction(outgoingMessage, connId, delegate (NwkMessage clientMsg)
    {
      log("received uid from client " + clientMsg.senderUid);
      addClient(clientMsg.senderUid);

      //broadcast to all
      NwkMessage msg = new NwkMessage();
      msg.setupNwkType(NwkMessageType.CONNECTION);
      msg.setSender("0");

      msg.setupMessage(clientMsg.senderUid); // msg will contain new client uid

      //send new client UID to everybody
      broadcastFromServer(msg);
    });

    log("asking to new client its uid");
    log(outgoingMessage.toString());
  }

  void broadcastDisconnectionPing()
  {
    log("server -> broadcasting disconnection ping (clients "+clients.Count+")");

    //error
    if(clients.Count == 0)
    {
      log("disconnection event but no clients recorded ?");
      return;
    }

    //send a disconnection transaction to everyone
    //server will start timeout-ing all clients
    //and will stop timeout-ing everyclients that answers
    NwkMessage msg = new NwkMessage();
    msg.setSender("0");
    msg.setupNwkType(NwkMessageType.DISCONNECTION_PING);

    broadcastFromServer(msg);
    
    //after deconnection we wait for a signal JIC
    foreach (KeyValuePair<string, NwkClientData> kp in clients)
    {
      clients[kp.Key].startTimeout();
    }
    
  }


  /// <summary>
  /// server -> send -> client
  /// </summary>
  public void sendServerToClientTransaction(NwkMessage msg, int clientConnectionId, Action<NwkMessage> onTransactionCompleted = null)
  {
    msg.senderUid = "0";
    msg.generateToken(); // a token for when the answer arrives

    NetworkServer.SendToClient(clientConnectionId, msg.messageId, msg);
    NwkMessageListener.getListener().add(msg, onTransactionCompleted);
  }

  public void sendToSpecificClient(NwkMessage msg, int clientConnectionId)
  {
    msg.senderUid = "0";
    NetworkServer.SendToClient(clientConnectionId, msg.messageId, msg);
  }

  /// <summary>
  /// bridge to broadcast message to everyone
  /// only for server
  /// </summary>
  public void broadcastFromServer(NwkMessage msg)
  {
    msg.senderUid = "0";
    NetworkServer.SendToAll(msg.messageId, msg);
  }



  private void Update()
  {
    updateTimeout();
  }

  void updateTimeout()
  {
    bool somethingChanged = false;
    foreach (KeyValuePair<string, NwkClientData> kp in clients)
    {
      float time = clients[kp.Key].timeout;
      if(time > 0f)
      {
        float next = time - Time.deltaTime;

        //log info on countdown int change
        if(Mathf.FloorToInt(time) != Mathf.FloorToInt(next))
        {
          log("client " + kp.Key + " is timing out " + next);
          somethingChanged = true;
        }

        //clients[kp.Key].timeout = next;
        kp.Value.timeout = next;

        if(next < 0f && time > 0f)
        {
          kp.Value.setAsDisconnected();
          somethingChanged = true;
        }
      }
    }

    if (somethingChanged) checkClientList();
  }

  void checkClientList()
  {
    List<string> keys = new List<string>();
    foreach (KeyValuePair<string, NwkClientData> kp in clients)
    {
      if (kp.Value.isDisconnected())
      {
        log("client " + kp.Key + " has timed out, removing it from clients list");
        keys.Add(kp.Key);
      }
    }

    for (int i = 0; i < keys.Count; i++)
    {
      clients.Remove(keys[i]);
    }

    refreshClientList();
  }
}
