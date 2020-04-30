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
  
  static public short nwkUid = -1; // will be populated ; stays at -1 until it created a connection with server
  static public int nwkConnId = -1;

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

    addClient(nwkUid); // localy add ref
  }

  void CreateClient()
  {
    Debug.Log(GetType()+" creating client ... ");

    var config = new ConnectionConfig();

    // Config the Channels we will use
    config.AddChannel(QosType.ReliableFragmented);
    config.AddChannel(QosType.UnreliableFragmented);

    // Create the client ant attach the configuration
    unetClient = new NetworkClient();
    unetClient.Configure(config, 12);

    // Register the handlers for the different network messages
    registerHandlers();

    if (!useLobbySystem())
    {
      log("this client is flagged without lobby, attemping connection ...");
      connectToIpPort(getConnectionIpAddress()); // localhost
    }
    else
    {
      Debug.LogWarning("using lobby system, app must implem it's own way to connect to server");
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

  protected override void registerHandle(short messageID, NetworkMessageDelegate callback)
  {
    unetClient.RegisterHandler(messageID, callback);
  }

  protected override void registerHandlers()
  {
    base.registerHandlers();

    unetClient.RegisterHandler(MsgType.Connect, unetConnected);
    unetClient.RegisterHandler(MsgType.Disconnect, unetDisconnected);
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

    log("this client is <b>sending disconnection message</b> to server ...");

    //tell server
    NwkMessageComplexe msg = new NwkMessageComplexe();
    msg.getIdCard().setupId(nwkUid, (int)eNwkMessageType.DISCONNECTION);

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

    log("this client is processing disconnection");
    log("  L disconnection in " + time + " secondes");

    //laisser le temps au server de recevoir le message ? :shrug:
    yield return new WaitForSeconds(time);

    log("  L telling sysClient to disconnect");

    unetClient.Disconnect();
  }

  protected override void onStateConnected()
  {
    base.onStateConnected(); // update view

    //ref THIS client as connected in data
    getClientData(nwkUid).setConnected();

  }

  protected override void onStateDisconnected()
  {
    base.onStateDisconnected();

    getClientData(nwkUid).setAsDisconnected();

    uiView?.setConnected(false);
  }

  void unetConnected(NetworkMessage message)
  {
    log(GetType()+ " unetConnected");

    getModule<NwkModPing>();
  }

  /// <summary>
  /// on OTHER CLIENT(s) disconnection
  /// </summary>
  void unetDisconnected(NetworkMessage message)
  {
    log(GetType()+ " unetDisconnected");

    //NwkMessage msg = convertMessage(message);

    //getClientData(nwkUid).setAsDisconnected();
    //NwkUiView nView = qh.gc<NwkUiView>();
    //if (nView != null) nView.onDisconnection();
  }

  override protected void solveTransaction(NwkMessageTransaction message, int connID)
  {
    log("solving transaction : " + message.getIdCard().toString()+" => "+message.token);

    eNwkMessageType mtype = (eNwkMessageType)message.getIdCard().getMessageType();
    switch (mtype)
    {
      case eNwkMessageType.CONNECTION_PINGPONG: // received msg from server that is asking for uid
        solvePingPong(message);
        break;
      case eNwkMessageType.NONE: break;
      default: throw new NotImplementedException("transaction not implem " + mtype);
    }
  }

  override protected void solveFull(NwkMessageFull message, int connID)
  {
    eNwkMessageType mtype = (eNwkMessageType)message.getIdCard().getMessageType();
    switch (mtype)
    {
      case eNwkMessageType.CONNECTION:

        int broadcastedUid = int.Parse(message.header.getHeader());

        if (broadcastedUid == nwkUid)
        {
          log("just received server acknowledgement for uid : " + broadcastedUid);

          //server asked, we sent the answer ... connection ready ?
          onNetworkLinkReady();

        }
        else
        {
          log("just received info that client uid : " + broadcastedUid + " just connected");
        }

        break;
      case eNwkMessageType.SYNC:

        //if match then it's mine, dont do anything with it
        //si y a modif d'un objet local par un autre client il faut passer par un msg type : SYNC_ONCE !

        Debug.Log(Time.frameCount + " | " + message.getIdCard().getMessageSender() + " vs " + nwkUid);

        if (message.getIdCard().getMessageSender() != NwkClient.nwkUid)
        {
          Debug.Log("  applied");
          NwkSyncer.instance.applyMessage(message);
        }

        break;

      case eNwkMessageType.TICK:

        float pingToMs = NwkClient.nwkClient.getModule<NwkModPing>().getRawPing();

        TickData td = (TickData)message.bytes.getObjectFromByteData();
        getModule<NwkTick>().setupTick(td.tickRate, td.tick, td.tickRateTimer, pingToMs);

        break;

      case eNwkMessageType.NONE: break;
      default: throw new NotImplementedException("full not implem " + mtype);
    }
  }

  override protected void solveComplexe(NwkMessageComplexe message, int connID)
  {
    eNwkMessageType mtype = (eNwkMessageType)message.getIdCard().getMessageType();
    switch (mtype)
    {
      case eNwkMessageType.NONE: break;
      default: throw new NotImplementedException("complexe not implem " + mtype);
    }
  }

  override protected void solveBasic(NwkMessageBasic message, int connID)
  {
    eNwkMessageType mtype = (eNwkMessageType)message.getIdCard().getMessageType();

    switch (mtype)
    {
      
      case eNwkMessageType.PONG: // received a pong, do something with it

        //inject pong delta
        getClientData(NwkClient.nwkUid).eventPing(getModule<NwkModPing>().pong());

        break;

      case eNwkMessageType.NONE: break;
      default: throw new NotImplementedException("basic not implem "+mtype);
    }

  }

  void solvePingPong(NwkMessageTransaction message)
  {
    
    NwkMessageTransaction trans = new NwkMessageTransaction();
    //msg.clean();

    NwkMessageTransaction original = message as NwkMessageTransaction;

    trans.getIdCard().setupId(nwkUid, (int)eNwkMessageType.CONNECTION_PINGPONG); // same type for transaction solving

    trans.token = original.token; // transfert token

    log("server asked for pong, processing ; token : "+trans.token);

    sendWrapperClient.sendClientToServer(trans);
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

  public override bool isConnected()
  {
    if (unetClient == null) return false;
    if (!unetClient.isConnected) return false;
    return nwkUid > 0;
  }




  /// <summary>
  /// uid is stored in ppref for next launch
  /// </summary>
  static public short generateUniqNetworkId()
  {
    short id = (short)PlayerPrefs.GetInt("nwkid", -1);

    if (id < 0)
    {
      id = generateUniqId();
      //id = SystemInfo.deviceUniqueIdentifier;

      PlayerPrefs.SetInt("nwkid", id);
      PlayerPrefs.Save();
    }

    return id;
  }

  static public short generateUniqId()
  {
    return (short)Random.Range(0, 9999);
  }

  /// <summary>
  /// generic generator
  /// </summary>
  static public string generateUniqStringId()
  {
    //solve uid
    string newUid = Random.Range(0, 999999).ToString();
    return newUid;
  }

}
