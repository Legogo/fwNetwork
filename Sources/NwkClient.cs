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
  static public bool isConnected() => getParsedNwkUid() > -1;

  static public int getParsedNwkUid()
  {
    return int.Parse(nwkUid);
  }

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
    Debug.Log("NwkClient creating client");

    var config = new ConnectionConfig();

    // Config the Channels we will use
    config.AddChannel(QosType.ReliableFragmented);
    config.AddChannel(QosType.UnreliableFragmented);

    // Create the client ant attach the configuration
    client = new NetworkClient();
    client.Configure(config, 1);

    // Register the handlers for the different network messages
    RegisterHandlers();

    if(!useLobbySystem())
    {
      log("this client is flagged without lobby, attemping connection ...");
      connectToIpPort(); // localhost
    }
  }

  /// <summary>
  /// without a lobby system client will connect on its own
  /// </summary>
  /// <returns></returns>
  abstract protected bool useLobbySystem();

  /// <summary>
  /// called by lobby
  /// </summary>
  public void connectToIpPort(string ip = "localhost", int port = 9999)
  {
    // Connect to the server
    client.Connect(ip, port);

    log(" ... client is connecting to : "+ip+":"+port);

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

    getModule<NwkModPing>();
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

    NwkMessage incomingMessage = null;
    try
    {
      incomingMessage = netMessage.ReadMessage<NwkMessage>();
    }
    catch
    {
      incomingMessage = null;
    }

    if(incomingMessage == null)
    {
      log("client couldn't read standard NwkMesssage");
      return;
    }

    if(!incomingMessage.silentLogs)
    {
      log("Client::OnMessageReceived");
      log(incomingMessage.toString());
    }

    if (incomingMessage.messageScope != 0)
    {
      onNwkMessageScopeChange(incomingMessage);
      return;
    }
    
    NwkMessageType mtype = (NwkMessageType)incomingMessage.messageType;
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

        msg.token = incomingMessage.token; // transfert token

        sendClient.sendClientToServer(msg);

        onNetworkLinkReady();
        break;
      case NwkMessageType.PONG:

        getClientData(nwkUid).ping = getModule<NwkModPing>().pong();

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
    string newUid = Random.Range(0, 999999).ToString();
    return newUid;
  }
}
