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

  protected NwkSendWrapperServer sendWrapper;

  protected override void Awake()
  {
    base.Awake();

    nwkServer = this;

    // Usually the server doesn't need to draw anything on the screen
    Application.runInBackground = true;

    Screen.SetResolution(1280, 720, false);

    Debug.LogWarning("================== <b>SERVER</b> ==================");
  }

  override protected void setup()
  {
    CreateServer(); //auto create
  }

  public override void connect()
  {
    int listenPort = NetworkServer.listenPort; // config ?
    if (listenPort == port)
    {
      log("already listening to port " + port + " ; asking for server creation but already created");
      return;
    }

    CreateServer();
  }

  void CreateServer()
  {
    // Register handlers for the types of messages we can receive
    RegisterHandlers();

    ConnectionConfig config = new ConnectionConfig();
    // There are different types of channels you can use, check the official documentation
    config.AddChannel(QosType.ReliableFragmented);
    config.AddChannel(QosType.UnreliableFragmented);

    var ht = new HostTopology(config, maxConnections);

    bool started = true;

    if (!NetworkServer.Configure(ht))
    {
      log("No server created, error on the configuration definition");
      started = false;
    }

    if (started)
    {
      // Start listening on the defined port
      if (NetworkServer.Listen(port)) log("Server created, listening on port: " + port);
      else
      {
        log("No server created, could not listen to the port: " + port);
        started = false;
      }

    }

    if (!started)
    {
      log("server flagged as not started ?");
      return;
    }

    onServerReady();
  }

  virtual protected void onServerReady()
  {
    log("server ready, generating send wrapper");

    sendWrapper = new NwkSendWrapperServer();

    //nwkUiView.setLabel(GetType().ToString());
  }

  public override void disconnect()
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

    //server prepare message to ask for uid of newly connected client
    //to send only to new client server will use connectionId stored within origin conn message
    NwkMessage outgoingMessage = new NwkMessage();
    outgoingMessage.setSender("0");
    outgoingMessage.setupNwkType(NwkMessageType.CONNECTION_PINGPONG);

    //give message to listener system to plug a callback
    sendWrapper.sendServerToClientTransaction(outgoingMessage, clientConnectionMessage.conn.connectionId, delegate (NwkMessage clientMsg)
    {
      // --- CALLBACK TRANSACTION

      string fid = clientMsg.getHeader();
      log("received uid from client : " + clientMsg.senderUid + " ; fid : " + fid);

      NwkClientData data = addClient(clientMsg.senderUid, clientConnectionMessage.conn.connectionId); // server ref new client in list
      data.setConnected(); // mark as connected

      //broadcast to all
      NwkMessage msg = new NwkMessage();
      msg.setupNwkType(NwkMessageType.CONNECTION);
      msg.setSender("0");

      msg.setupHeader(clientMsg.senderUid); // msg will contain new client uid

      //send new client UID to everybody
      sendWrapper.broadcastServerToAll(msg, "0");

      // ---

    });

    log("asking to new client its uid");
    log(outgoingMessage.toString());

    //NwkUiView nView = qh.gc<NwkUiView>();
    //if (nView != null) nView.onConnection();
  }

  void OnClientDisconnected(NetworkMessage netMessage)
  {
    log("OnClientDisconnected");

    //NwkUiView nView = qh.gc<NwkUiView>();
    //if (nView != null) nView.onDisconnection();

    //getClientData()

    broadcastDisconnectionPing();
  }

  protected override void updateNetwork()
  {
    base.updateNetwork();

    //check for stuff in clients

    float dlt;
    for (int i = 0; i < clientDatas.Count; i++)
    {
      if(clientDatas[i].updateTimeout(Time.realtimeSinceStartup))
      {
        log(clientDatas[i].nwkUid + " timeout !");
      }
    }
  }

  void OnMessageReceived(NetworkMessage netMessage)
  {
    //log("OnMessageReceived : "+netMessage.msgType);

    // You can send any object that inherence from MessageBase
    // The client and server can be on different projects, as long as the MyNetworkMessage or the class you are using have the same implementation on both projects
    // The first thing we do is deserialize the message to our custom type

    NwkMessage incomingMessage = null;

    try { incomingMessage = netMessage.ReadMessage<NwkMessage>(); }
    catch { incomingMessage = null; }

    if (incomingMessage == null)
    {
      log("server couldn't read standard NwkMesssage ; passing it on");
      return;
    }

    //solve size info
    if (incomingMessage.messageBytes.Length > 0)
    {
      int msgSize = incomingMessage.messageBytes.Length * 4;
      if (msgSize > 0)
      {
        if (incomingMessage.senderUid.Length > 0)
        {
          getClientData(incomingMessage.senderUid).msgSizes.Add(msgSize);
        }
      }
    }

    //scope is "who" need to treat the message
    //scope of 0 is default integration
    //scope != 0 -> pass on the message to whoever is capable of solving it
    NwkMessageScope scope = (NwkMessageScope)incomingMessage.messageScope;

    switch (scope)
    {
      case NwkMessageScope.BASIC:
        solveBasicScope(incomingMessage, netMessage.conn.connectionId);
        solveTransaction(incomingMessage);
        break;
      case NwkMessageScope.MODS:
        break;
      case NwkMessageScope.CUSTOM:
        onNewNwkMessage(incomingMessage, netMessage.conn.connectionId);
        break;
      default: throw new NotImplementedException(scope.ToString());
    }

  }

  void solveBasicScope(NwkMessage msg, int senderConnectionId)
  {
    log("client # " + msg.senderUid, msg.silentLogs);
    //log(msg.toString(), msg.silentLogs);

    //typ must be nulled (using none) to stop propagation

    NwkMessageType typ = (NwkMessageType)msg.messageType;
    switch (typ)
    {
      case NwkMessageType.CLT_DISCONNECTION_PONG:

        log("received disconnection pong from " + msg.senderUid, msg.silentLogs);

        //getClientData(msg.senderUid).resetTimeout();

        break;
      case NwkMessageType.PING: // client sent ping

        if (!msg.silentLogs) log("received ping from " + msg.senderUid, msg.silentLogs);

        //ref timestamp to solve timeout
        pingMessage(msg.senderUid);

        // re-use message :shrug:
        msg.clean();

        msg.silentLogs = true;
        msg.setupNwkType(NwkMessageType.PONG);
        sendWrapper.sendServerAnswerToSpecificClient(msg, senderConnectionId);

        break;
      case NwkMessageType.DISCONNECTION:

        getClientData(msg.senderUid).setAsDisconnected();

        //msg.clean();

        break;
      case NwkMessageType.SYNC:

        //send new data to everybody
        //also specify sender to be able to filter on the other end
        sendWrapper.broadcastServerToAll(msg, msg.senderUid);

        //sendWrapper.sendServerToSpecificClient

        break;
      case NwkMessageType.NONE: break;
      case NwkMessageType.CONNECTION_PINGPONG:
        //must implem for transaction of that type
        break;
      default: throw new NotImplementedException(typ.ToString());
    }

  }

  void pingMessage(string senderUid)
  {
    getClientData(senderUid).eventPing(Time.realtimeSinceStartup);
  }

  void solveTransaction(NwkMessage msg)
  {

    if (msg.isTransactionMessage())
    {
      log(listener.getStackCount() + " transaction(s) before solving");

      listener.solveReceivedMessage(msg); // check for pongs
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
    log("server -> broadcasting disconnection ping (clients " + clientDatas.Count + ")");

    //error
    if (clientDatas.Count == 0)
    {
      log("disconnection event but no clients recorded ?");
      return;
    }

    //send a disconnection transaction to everyone
    //server will start timeout-ing all clients
    //and will stop timeout-ing everyclients that answers
    NwkMessage msg = new NwkMessage();
    msg.setSender("0");
    msg.setupNwkType(NwkMessageType.SRV_DISCONNECTION_PING);

    sendWrapper.broadcastServerToAll(msg, "0");

    //after deconnection we wait for a signal JIC
    //for (int i = 0; i < clientDatas.Count; i++) clientDatas[i].startTimeout();
  }

  /// <summary>
  /// remove disconnected clients from list
  /// </summary>
  void cleanClientList()
  {
    List<string> keys = new List<string>();
    for (int i = 0; i < clientDatas.Count; i++)
    {
      if (clientDatas[i].isDisconnected())
      {
        log("client " + clientDatas[i].nwkUid + " has timed out, removing it from clients list");
        keys.Add(clientDatas[i].nwkUid);
      }
    }

    int idx = 0;
    while (idx < clientDatas.Count)
    {
      bool found = false;
      for (int i = 0; i < keys.Count; i++)
      {
        if (clientDatas[idx].nwkUid == keys[i])
        {
          clientDatas.RemoveAt(idx);
          found = true;
        }
      }
      if (!found) idx++;
    }
  }

  public override bool isConnected() => sendWrapper != null;

}
