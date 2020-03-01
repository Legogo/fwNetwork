using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

abstract public class NwkServer : NwkSystemBase
{
  static public NwkServer nwkServer;

  int port = 9999;
  int maxConnections = 10;

  protected NwkSendWrapper sendServer;

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
    CreateServer(); //auto create
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

    sendServer = new NwkSendWrapper();

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
    log("OnClientConnected ; sending ping pong transaction");

    NwkMessage outgoingMessage = new NwkMessage();
    outgoingMessage.setSender("0");
    outgoingMessage.setupNwkType(NwkMessageType.CONNECTION_PINGPONG);

    //give message to listener system to plug a callback
    sendServer.sendServerToClientTransaction(outgoingMessage, clientConnectionMessage.conn.connectionId, delegate (NwkMessage clientMsg)
    {
      log("received uid from client " + clientMsg.senderUid);
      addClient(clientMsg.senderUid);

      //broadcast to all
      NwkMessage msg = new NwkMessage();
      msg.setupNwkType(NwkMessageType.CONNECTION);
      msg.setSender("0");

      msg.setupMessage(clientMsg.senderUid); // msg will contain new client uid

      //send new client UID to everybody
      sendServer.broadcastServerToAll(msg, "0");
    });

    log("asking to new client its uid");
    log(outgoingMessage.toString());

  }
  
  void OnClientDisconnected(NetworkMessage netMessage)
  {
    log("OnClientDisconnected");

    broadcastDisconnectionPing();
  }

  virtual protected void onDisconnection(int uid) { }

  void OnMessageReceived(NetworkMessage netMessage)
  {
    //log("OnMessageReceived : "+netMessage.msgType);

    // You can send any object that inherence from MessageBase
    // The client and server can be on different projects, as long as the MyNetworkMessage or the class you are using have the same implementation on both projects
    // The first thing we do is deserialize the message to our custom type

    NwkMessage incomingMessage = null;

    try { incomingMessage = netMessage.ReadMessage<NwkMessage>(); }
    catch { incomingMessage = null; }

    if(incomingMessage == null)
    {
      log("server couldn't read standard NwkMesssage ; passing it on");
      return;
    }


    if(!incomingMessage.silent)
    {
      log("client # " + incomingMessage.senderUid);
      log(incomingMessage.toString());
    }

    //scope is "who" need to treat the message
    if (incomingMessage.messageScope != 0)
    {
      onNewNwkMessage(incomingMessage, netMessage.conn.connectionId);
      return;
    }

    NwkMessageType typ = (NwkMessageType)incomingMessage.messageType;
    switch (typ)
    {
      case NwkMessageType.DISCONNECTION_PONG:
        log("received disconnection pong from " + incomingMessage.senderUid);
        clients[incomingMessage.senderUid].resetTimeout();
        break;
    }

    if (incomingMessage.isTransactionMessage())
    {
      log(listener.getStackCount() + " transaction(s) before solving");

      listener.solveReceivedMessage(incomingMessage); // check for pongs
      log(listener.getStackCount() + " transaction(s) after solving");
      log(listener.toString());
    }

  }

  /// <summary>
  /// how subcontext will solve message
  /// connId = is id of msg sender
  /// </summary>
  abstract protected void onNewNwkMessage(NwkMessage msg, int connId);

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

    sendServer.broadcastServerToAll(msg, "0");
    
    //after deconnection we wait for a signal JIC
    foreach (KeyValuePair<string, NwkClientData> kp in clients)
    {
      clients[kp.Key].startTimeout();
    }
    
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
