using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

/// <summary>
/// 
/// IP:PORT (default port is 9999)
/// 
/// logs to server on startup
/// CONNECTION_PINGPONG -> will generate it's own uniqId and send it to the server
/// 
/// </summary>

abstract public class NwkClient : NwkSystemBase
{
  public const int defaultPort = 9999;

  static public NwkClient nwkClient;
  
  static public string nwkUid = "-1"; // will be populated ; stays at -1 until it created a connection with server
  static public int nwkConnId = -1;

  /*
  static public int getParsedNwkUid()
  {
    try
    {
      return int.Parse(nwkUid);
    }
    catch(Exception e)
    {
      Debug.Log("can't parse : " + nwkUid);
    }

    return -1;
  }
  */

  public NwkSendWrapperClient sendWrapperClient;
  NetworkClient unetClient;

  protected override void Awake()
  {
    base.Awake();

    nwkClient = this;

    nwkUid = generateUniqNetworkId(); // client is generating its UID

    uiView?.setConnected(false);
  }

  override protected void setup()
  {
    base.setup();

    CreateClient();

    addClient(nwkUid.ToString()); // localy add ref
  }

  void CreateClient()
  {
    Debug.Log("NwkClient creating client");

    var config = new ConnectionConfig();

    // Config the Channels we will use
    config.AddChannel(QosType.ReliableFragmented);
    config.AddChannel(QosType.UnreliableFragmented);

    // Create the client ant attach the configuration
    unetClient = new NetworkClient();
    unetClient.Configure(config, 1);

    // Register the handlers for the different network messages
    RegisterHandlers();

    if (!useLobbySystem())
    {
      log("this client is flagged without lobby, attemping connection ...");
      connectToIpPort(getConnectionIpAddress()); // localhost
    }

  }

  /// <summary>
  /// without a lobby system client will connect on its own
  /// </summary>
  /// <returns></returns>
  abstract protected bool useLobbySystem();

  /// <summary>
  /// might/must be override in children
  /// </summary>
  virtual protected string getConnectionIpAddress() => "localhost";

  public override void connect()
  {
    if (isConnected())
    {
      log("asking for connection but already connected ?");
      return;
    }

    Debug.Assert(unetClient != null);

    connectToIpPort(getConnectionIpAddress());
  }

  /// <summary>
  /// called by lobby
  /// </summary>
  public void connectToIpPort(string ip = "")
  {
    if (ip.Length <= 0) ip = getConnectionIpAddress();

    int port = defaultPort;

    // Connect to the server
    unetClient.Connect(ip, port);

    log(" ... client is connecting to : " + ip + ":" + port);

    sendWrapperClient = new NwkSendWrapperClient(unetClient);
  }

  // Register the handlers for the different message types
  void RegisterHandlers()
  {
    // Unity have different Messages types defined in MsgType
    unetClient.RegisterHandler(messageID, unetOnMessageReceived);
    unetClient.RegisterHandler(MsgType.Connect, unetOnOtherConnected);
    unetClient.RegisterHandler(MsgType.Disconnect, unetOnOtherDisconnected);
  }

  public override void disconnect()
  {
    if (!isConnected())
    {
      log("asking for disconnection but not connected");
      return;
    }

    Debug.Assert(unetClient != null);

    //unetClient.Disconnect();
    //https://answers.unity.com/questions/1281928/unet-handle-client-disconnection.html
    //https://forum.unity.com/threads/how-to-disconnect-clients-properly.393416/
    //NetworkManager.singleton.StopClient();

    log("sending server disconnection message ...");

    //tell server
    NwkMessage msg = NwkMessage.getStandardMessage(nwkUid, NwkMessageType.DISCONNECTION);
    sendWrapperClient.sendClientToServer(msg);

    //Debug.Log(unetClient.isConnected);

    //StopAllCoroutines(); // previous ?
    if (coproDisco != null) StopCoroutine(coproDisco);
    coproDisco = StartCoroutine(processDisconnection());
  }

  Coroutine coproDisco = null;
  IEnumerator processDisconnection()
  {
    float time = 1f;

    log(" ... disconnection in " + time + " seconde(s)");

    yield return new WaitForSeconds(time);

    log(" ... telling unet client to disconnect");

    unetClient.Disconnect();
  }

  protected override void onStateConnected()
  {
    base.onStateConnected();

    //ref THIS client as connected in data
    getClientData(nwkUid).setConnected();

    uiView?.setConnected(true);
  }

  protected override void onStateDisconnected()
  {
    base.onStateDisconnected();

    getClientData(nwkUid).setAsDisconnected();

    uiView?.setConnected(false);
  }

  void unetOnOtherConnected(NetworkMessage message)
  {
    log("Client::OnConnected : " + message.msgType);

    getModule<NwkModPing>();
  }

  /// <summary>
  /// on OTHER CLIENT(s) disconnection
  /// </summary>
  void unetOnOtherDisconnected(NetworkMessage message)
  {
    log("Client::OnDisconnected : " + message.msgType);

    //NwkMessage msg = convertMessage(message);

    //getClientData(nwkUid).setAsDisconnected();
    //NwkUiView nView = qh.gc<NwkUiView>();
    //if (nView != null) nView.onDisconnection();
  }

  // Message received from the server
  void unetOnMessageReceived(NetworkMessage netMessage)
  {
    NwkMessage incMessage = convertMessage(netMessage);

    if (incMessage == null)
    {
      log("client couldn't read standard NwkMesssage");
      return;
    }

    if (!incMessage.silentLogs)
    {
      log("Client::OnMessageReceived");
      log(incMessage.toString());
    }

    NwkMessageScope scope = (NwkMessageScope)incMessage.messageScope;
    switch (scope)
    {
      case NwkMessageScope.BASIC: solveBasicMessage(incMessage); break;
      case NwkMessageScope.MODS: solveModsMessage(incMessage); break;
      case NwkMessageScope.CUSTOM: solveCustomMessage(incMessage); break;
      default: throw new NotImplementedException();
    }

  }

  void solveBasicMessage(NwkMessage incMessage)
  {

    NwkMessageType mtype = (NwkMessageType)incMessage.messageType;

    switch (mtype)
    {
      case NwkMessageType.CONNECTION:


        string broadcastedUid = incMessage.getHeader();

        if (broadcastedUid == nwkUid)
        {
          log("just received server acknowledgement for uid : "+broadcastedUid);

          //server asked, we sent the answer ... connection ready ?
          onNetworkLinkReady();

        }
        else
        {

          log("just received info that client uid : " + broadcastedUid + " just connected");

        }

        break;
      case NwkMessageType.CONNECTION_PINGPONG: // server is asking for uid

        NwkMessage msg = new NwkMessage();
        //msg.clean();

        msg.setSender(nwkUid);
        msg.setupNwkType(NwkMessageType.CONNECTION_PINGPONG); // same type for transaction solving

        string fid = nwkUid + ":" + nwkConnId;
        msg.setupHeader(fid); // give local uid

        msg.token = incMessage.token; // transfert token

        sendWrapperClient.sendClientToServer(msg);

        break;
      case NwkMessageType.PONG:

        //inject pong delta
        getClientData(NwkClient.nwkUid).eventPing(getModule<NwkModPing>().pong());

        break;
      case NwkMessageType.SYNC:

        NwkSyncer.instance.applyMessage(incMessage);

        break;
      case NwkMessageType.NONE:
        break;
      default: throw new NotImplementedException();
    }

  }

  void solveModsMessage(NwkMessage incMessage)
  {
    NwkMessageMods mt = (NwkMessageMods)incMessage.messageType;
    switch (mt)
    {
      case NwkMessageMods.NONE:
        break;
      default: throw new NotImplementedException();
    }

  }

  void solveCustomMessage(NwkMessage incMessage)
  {
    onNwkMessageScopeChange(incMessage);
  }

  NwkMessage convertMessage(NetworkMessage netMessage)
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

    return incomingMessage;
  }

  /// <summary>
  /// when we are sure that server and client knows each other
  /// reaction to a msg from server
  /// </summary>
  virtual protected void onNetworkLinkReady()
  {
    nwkConnId = unetClient.connection.connectionId;

    //update local client connId
    getClientData(nwkUid).connId = nwkConnId;

    log("this client generated network id : " + nwkUid + " ; connId : " + unetClient.connection.connectionId);

    string fullId = nwkUid + ":" + nwkConnId;

    if(uiView != null)
    {
      uiView.setLabel(GetType().ToString() + " " + fullId);
    }
    
    log("network link is ready , solved network fid is : " + fullId);
  }

  abstract protected void onNwkMessageScopeChange(NwkMessage nwkMsg);

  public override bool isConnected()
  {
    if (unetClient == null) return false;
    if (!unetClient.isConnected) return false;
    return nwkUid.Length > 0;
  }




  /// <summary>
  /// uid is stored in ppref for next launch
  /// </summary>
  static public string generateUniqNetworkId()
  {
    string id = PlayerPrefs.GetString("nwkid", "");

    if (id.Length <= 0)
    {
      //id = generateUniqId();
      id = SystemInfo.deviceUniqueIdentifier;

      PlayerPrefs.SetString("nwkid", id);
      PlayerPrefs.Save();
    }

    return id;
  }

  /// <summary>
  /// generic generator
  /// </summary>
  static public string generateUniqId()
  {
    //solve uid
    string newUid = Random.Range(0, 999999).ToString();
    return newUid;
  }

}
