using System;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

/// <summary>
/// logs to server on startup
/// CONNECTION_PINGPONG -> will generate it's own uniqId and send it to the server
/// 
/// </summary>

public class RaiderNwkClient : RaiderNwkSystemBase
{
  static public string nwkUid = "";

  int port = 9999;
  string ip = "localhost";
  
  // The network client
  public NetworkClient client;
  
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
    // Do stuff when connected to the server

    //RaiderNwkMessage messageContainer = new RaiderNwkMessage();
    //messageContainer.message = "Hello server!";

    // Say hi to the server when connected
    //client.Send(messageID, messageContainer);

    log("Client::OnConnected : " + message.msgType);
  }

  void OnDisconnected(NetworkMessage message)
  {
    // Do stuff when disconnected to the server
  }

  // Message received from the server
  void OnMessageReceived(NetworkMessage netMessage)
  {
    // You can send any object that inherence from MessageBase
    // The client and server can be on different projects, as long as the MyNetworkMessage or the class you are using have the same implementation on both projects
    // The first thing we do is deserialize the message to our custom type
    RaiderNwkMessage objectMessage = netMessage.ReadMessage<RaiderNwkMessage>();

    log("Client::OnMessageReceived");
    log(objectMessage.toString());

    RaiderNwkMessage msg = null;
    switch (objectMessage.messageType)
    {
      case RaiderNwkMessageType.CONNECTION_PINGPONG:

        nwkUid = generateUniqNetworkId();

        addClient(nwkUid.ToString());

        log("generating network id : "+nwkUid);

        msg = new RaiderNwkMessage().setupType(RaiderNwkMessageType.CONNECTION_PINGPONG);
        msg.assignToken(objectMessage);
        msg.message = nwkUid;
        msg.sendToServer(nwkUid, client);
        
        break;
      case RaiderNwkMessageType.ASSIGN_ID: // deprecated

        nwkUid = objectMessage.message; // record uid

        log("me (#"+nwkUid+") sending -> server ? pong");

        new RaiderNwkMessage().assignToken(objectMessage).sendToServer(nwkUid, client); // pong

        addClient(nwkUid.ToString());

        break;
    }
    
  }

  static public string generateUniqNetworkId()
  {
    //solve uid
    string newUid = "c" + Random.Range(0, 999999);
    return newUid;
  }
}
