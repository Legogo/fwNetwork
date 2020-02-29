using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;

public class NwkServer : NwkSystemBase
{
  int port = 9999;
  int maxConnections = 10;

  protected override void Awake()
  {
    base.Awake();

    // Usually the server doesn't need to draw anything on the screen
    Application.runInBackground = true;

    Screen.SetResolution(1280, 720, false);
  }

  override protected void setup()
  {
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
  }

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

  void OnClientConnected(NetworkMessage netMessage)
  {
    // Do stuff when a client connects to this server

    log("OnClientConnected");

    //solving new uid for client
    //connection_solveNewClient(netMessage);
    connection_askForUid(netMessage);

    // Send a thank you message to the client that just connected
    //RaiderNwkMessage messageContainer = new RaiderNwkMessage();
    //messageContainer.message = "Thanks for joining!";

    // This sends a message to a specific client, using the connectionId
    //NetworkServer.SendToClient(netMessage.conn.connectionId, messageID, messageContainer);

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
    log("OnMessageReceived");

    // You can send any object that inherence from MessageBase
    // The client and server can be on different projects, as long as the MyNetworkMessage or the class you are using have the same implementation on both projects
    // The first thing we do is deserialize the message to our custom type

    NwkMessage objectMessage = netMessage.ReadMessage<NwkMessage>();
    log(objectMessage.toString());

    //do stuff with message

    if(objectMessage.messageType == NwkMessageType.DISCONNECTION_PONG)
    {
      log("received disconnection pong from " + objectMessage.senderUid);
      clients[objectMessage.senderUid].resetTimeout();
    }

    if (objectMessage.isTransactionMessage())
    {
      log(listener.getStackCount() + " transaction(s) before solving");

      listener.solveReceivedMessage(objectMessage); // check for pongs
      log(listener.getStackCount()+ " transaction(s) after solving");
      log(listener.toString());
    }

  }

  void connection_askForUid(NetworkMessage connectedClient)
  {
    
    NwkMessage outgoingMessage = new NwkMessage();
    outgoingMessage.setupType(NwkMessageType.CONNECTION_PINGPONG);
    outgoingMessage.sendServerClientTransaction(connectedClient, delegate (NwkMessage clientMsg)
    {
      log("received uid from client " + clientMsg.senderUid);
      addClient(clientMsg.senderUid);

      //broadcast to all
      NwkMessage msg = new NwkMessage();
      msg.setupType(NwkMessageType.CONNECTION);
      msg.message = clientMsg.senderUid;
      msg.broadcastFromServer();
    });

    log("asking to new client its uid");
    log(outgoingMessage.toString());
  }

  void connection_solveNewClient(NetworkMessage clientMsg)
  {
    //solve uid
    string newUid = "c"+Random.Range(0, 999999);
    
    //send message to that client
    NwkMessage outgoingMessage = new NwkMessage();

    outgoingMessage.setupType(NwkMessageType.ASSIGN_ID);
    outgoingMessage.message = newUid;

    log("solveNewClient | -> | sending uid : " + newUid);

    addClient(newUid);

    outgoingMessage.generateToken().sendServerClientTransaction(clientMsg, delegate (NwkMessage pongMsg)
    {
      log("solveNewClient | <- | client #"+ newUid + " acknowledged uid");

      // Send a message to all the clients connected
      NwkMessage msg = new NwkMessage();
      msg.setupType(NwkMessageType.CONNECTION);
      msg.message = newUid;

      // Broadcast a message a to everyone connected
      msg.broadcastFromServer();
    });
    
  }

  void broadcastDisconnectionPing()
  {
    log("server -> broadcasting disconnection ping (clients "+clients.Count+")");

    if(clients.Count == 0)
    {
      log("disconnection event but no clients recorded ?");
      return;
    }

    NwkMessage msg = null;

    msg = new NwkMessage();
    msg.setupType(NwkMessageType.DISCONNECTION_PING).broadcastFromServer();

    foreach(KeyValuePair<string, NwkClientData> kp in clients)
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
