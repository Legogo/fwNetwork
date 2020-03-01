using System;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

/// <summary>
/// logs to server on startup
/// CONNECTION_PINGPONG -> will generate it's own uniqId and send it to the server
/// 
/// </summary>

abstract public class NwkClient : NwkSystemBase
{
  static public NwkClient nwkClient;
  static public string nwkUid = "-1"; // will be populated ; stays at -1 until it created a connection with server

  static public int getParsedNwkUid() => int.Parse(nwkUid);

  int port = 9999;
  string ip = "localhost";
  
  // The network client
  public NetworkClient client;

  public NwkSendWrapper sendClient;

  protected override void Awake()
  {
    base.Awake();
    nwkClient = this;
  }

  override protected void setup()
  {
    CreateClient();
  }

  void CreateClient()
  {
    var config = new ConnectionConfig();

    // Config the Channels we will use
    config.AddChannel(QosType.ReliableFragmented);
    config.AddChannel(QosType.UnreliableFragmented);

    // Create the client ant attach the configuration
    client = new NetworkClient();
    client.Configure(config, 1);

    // Register the handlers for the different network messages
    RegisterHandlers();

    // Connect to the server
    client.Connect(ip, port);

    sendClient = new NwkSendWrapper();
  }

  // Register the handlers for the different message types
  void RegisterHandlers()
  {
    // Unity have different Messages types defined in MsgType
    client.RegisterHandler(messageID, OnMessageReceived);
    client.RegisterHandler(MsgType.Connect, OnConnected);
    client.RegisterHandler(MsgType.Disconnect, OnDisconnected);
  }

  void OnConnected(NetworkMessage message)
  {
    log("Client::OnConnected : " + message.msgType);
  }

  void OnDisconnected(NetworkMessage message)
  {
    log("Client::OnDisconnected : " + message.msgType);
  }

  // Message received from the server
  void OnMessageReceived(NetworkMessage netMessage)
  {
    // You can send any object that inherence from MessageBase
    // The client and server can be on different projects, as long as the MyNetworkMessage or the class you are using have the same implementation on both projects
    // The first thing we do is deserialize the message to our custom type

    NwkMessage objectMessage = null;
    try
    {
      objectMessage = netMessage.ReadMessage<NwkMessage>();
    }
    catch
    {
      objectMessage = null;
    }

    if(objectMessage == null)
    {
      log("client couldn't read standard NwkMesssage");
      return;
    }

    log("Client::OnMessageReceived");
    log(objectMessage.toString());

    if(objectMessage.messageScope != 0)
    {
      onNwkMessageScopeChange(objectMessage);
      return;
    }
    
    NwkMessageType mtype = (NwkMessageType)objectMessage.messageType;
    switch (mtype)
    {
      case NwkMessageType.CONNECTION_PINGPONG: // server is asking for a pong

        nwkUid = generateUniqNetworkId();

        addClient(nwkUid.ToString());

        log("this client generated network id : " + nwkUid);

        NwkMessage msg = new NwkMessage();
        msg.setSender(nwkUid);
        msg.setupNwkType(NwkMessageType.CONNECTION_PINGPONG);
        msg.setupMessage(nwkUid); // give local uid

        msg.token = objectMessage.token; // transfert token

        sendClient.sendClientToServer(msg);

        onNetworkLinkReady();
        break;
    }

  }

  /// <summary>
  /// when we are sure that server and client knows each other
  /// </summary>
  abstract protected void onNetworkLinkReady();
  abstract protected void onNwkMessageScopeChange(NwkMessage nwkMsg);

  static public string generateUniqNetworkId()
  {
    //solve uid
    string newUid = "c" + Random.Range(0, 999999);
    return newUid;
  }
}
