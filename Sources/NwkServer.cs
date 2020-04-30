using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;

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

    //tick.resetTickCount();

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
    registerHandlers();

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

  protected override void registerHandle(short messageID, NetworkMessageDelegate callback)
  {
    NetworkServer.RegisterHandler(messageID, callback);
  }

  protected override void registerHandlers()
  {
    base.registerHandlers();

    // Unity have different Messages types defined in MsgType
    NetworkServer.RegisterHandler(MsgType.Connect, OnClientConnected);
    NetworkServer.RegisterHandler(MsgType.Disconnect, OnClientDisconnected);

  }

  void OnClientConnected(NetworkMessage clientConnectionMessage)
  {
    log("OnClientConnected ; sending ping pong transaction");

    //server prepare message to ask for uid of newly connected client
    //to send only to new client server will use connectionId stored within origin conn message
    NwkMessageTransaction transMessage = new NwkMessageTransaction();
    transMessage.getIdCard().setupId(0, (int)eNwkMessageType.CONNECTION_PINGPONG);

    //give message to listener system to plug a callback
    sendWrapper.sendTransaction(transMessage, clientConnectionMessage.conn.connectionId, delegate (NwkMessageTransaction waybackMessage)
    {
      // --- CALLBACK TRANSACTION

      log("received uid from client : " + waybackMessage.getIdCard().getMessageSender()+" , token ? "+waybackMessage.token);
      
      NwkClientData data = addClient(waybackMessage.getIdCard().getMessageSender(), clientConnectionMessage.conn.connectionId); // server ref new client in list
      data.setConnected(); // mark as connected

      //broadcast to all
      NwkMessageFull msg = new NwkMessageFull();
      msg.getIdCard().setupId(0, (int)eNwkMessageType.CONNECTION);

      msg.header.setupHeader(waybackMessage.getIdCard().getMessageSender().ToString()); // msg will contain new client uid

      //send new client UID to everybody
      sendWrapper.broadcastServerToAll(msg, 0);

      // ---

    });

    //log("asking to new client its uid");

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

  virtual protected void onTick(int tick)
  { }

  protected override void updateNetwork()
  {
    base.updateNetwork();

    //check for stuff in clients

    //float dlt;
    for (int i = 0; i < clientDatas.Count; i++)
    {
      if(clientDatas[i].updateTimeout(Time.realtimeSinceStartup))
      {
        log(clientDatas[i].nwkUid + " timeout !");
      }
    }


  }

  void solveMessageSize(NwkMessageFull msg)
  {
    if (msg == null) return;

    //solve size info
    if (msg.bytes.messageBytes.Length > 0)
    {
      int msgSize = msg.bytes.messageBytes.Length * 4;
      if (msgSize > 0)
      {
        int nUid = msg.getIdCard().getMessageSender();
        //incoming message has sender ?
        if (nUid > 0)
        {
          getClientData(nUid).msgSizes.Add(msgSize);
        }
      }
    }

  }

  protected override void solveBasic(NwkMessageBasic message, int connID)
  {
    log("client # " + message.getIdCard().getMessageSender() + " (" + message.ToString() + ")", message.isSilent());
    
    NwkMessageBasic bMessage;

    eNwkMessageType mtype = (eNwkMessageType)message.getIdCard().getMessageType();
    switch (mtype)
    {
      case eNwkMessageType.PING: // client sent ping

        //ref timestamp to solve timeout
        pingMessage(message.getIdCard().getMessageSender());

        //setup pong message
        bMessage = new NwkMessageBasic();
        bMessage.getIdCard().setMessageType(eNwkMessageType.PONG);

        //Send pong message
        sendWrapper.sendToSpecificClient(bMessage, connID);

        break;
      case eNwkMessageType.DISCONNECTION:

        getClientData(message.getIdCard().getMessageSender()).setAsDisconnected();

        //msg.clean();

        break;
      case eNwkMessageType.TICK:

        //send tick data

        NwkMessageFull mf = new NwkMessageFull();
        mf.getIdCard().setupId(0, (int)eNwkMessageType.TICK);
        mf.bytes.setByteData(getModule<NwkTick>().data);

        sendWrapper.sendToSpecificClient(mf, connID);

        break;
      case eNwkMessageType.NONE: break;
      default: throw new NotImplementedException("base ; not implem " + mtype);
    }
  }

  protected override void solveComplexe(NwkMessageComplexe message, int connID)
  {
    throw new NotImplementedException();
  }

  protected override void solveFull(NwkMessageFull message, int connID)
  {
    eNwkMessageType mtype = (eNwkMessageType)message.getIdCard().getMessageType();
    switch (mtype)
    {
      case eNwkMessageType.SYNC:

        //send new data to everybody
        //also specify sender to be able to filter on the other end
        sendWrapper.broadcastServerToAll(message, message.getIdCard().getMessageSender());
        break;
      case eNwkMessageType.NONE: break;
      default: throw new NotImplementedException("full ; not implem " + mtype);
    }
  }

  protected override void solveTransaction(NwkMessageTransaction message, int connID)
  {
    if (message == null) return;
    log(listener.getStackCount() + " transaction(s) before solving");

    listener.solveReceivedMessage(message); // check for pongs
    log(listener.getStackCount() + " transaction(s) after solving");
    log(listener.toString());
  }

  void pingMessage(int senderUid)
  {
    getClientData(senderUid).eventPing(Time.realtimeSinceStartup);
  }

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
    NwkMessageBasic msg = new NwkMessageBasic();
    msg.getIdCard().setupId(0, (int)eNwkMessageType.SRV_DISCONNECTION_PING);

    sendWrapper.broadcastServerToAll(msg, 0);

    //after deconnection we wait for a signal JIC
    //for (int i = 0; i < clientDatas.Count; i++) clientDatas[i].startTimeout();
  }

  /// <summary>
  /// remove disconnected clients from list
  /// </summary>
  void cleanClientList()
  {
    List<int> keys = new List<int>();
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
