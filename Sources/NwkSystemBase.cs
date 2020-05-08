using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

abstract public class NwkSystemBase : MonoBehaviour
{
  public static NwkSystemBase nwkSys;

  // The id we use to identify our messages and register the handler
  public const short messageID = 1000;

  protected NwkMessageListener listener;

  public bool useUiView = false;
  protected NwkUiView uiView;
  //protected NwkSendWrapper sendWrapper; // wrapper must be generated only when connection is active

  public List<NwkClientData> clientDatas = new List<NwkClientData>();

  bool _localConnectionStatus = false;

  protected NwkTick tick;

  virtual protected void Awake()
  {
    nwkSys = this;

    tick = getModule<NwkTick>();

    //sendWrapper = generateSendWrapper();
    
    listener = NwkMessageListener.getListener();
    if (listener == null) listener = gameObject.AddComponent<NwkMessageListener>();
  }

  //abstract protected NwkSendWrapper generateSendWrapper();

  IEnumerator Start()
  {
    //tbsure make sure engine as not already loaded stuff
    for (int i = 0; i < 10; i++) yield return null;

    //Debug.Log("waiting for engine to be finished");

    IEnumerator specBootSetup = appSpecificBootSetup();
    while (specBootSetup.MoveNext()) yield return null;

    //Debug.Log("waiting for nwk ui view ...");

    if (useUiView)
    {
      Debug.Log("loading debug ui view");

      AsyncOperation async = NwkUnityTools.loadScene("network-view");

      if (async != null)
      {
        while (!async.isDone) yield return null;
      }

      uiView = GameObject.FindObjectOfType<NwkUiView>();
      uiView.setLabel(GetType().ToString());

      //Debug.Log(uiView);
    }

    setup();
  }

  /// <summary>
  /// permet dans un projet de rajouter des choses pour bloquer le lancement
  /// </summary>
  /// <returns></returns>
  virtual protected IEnumerator appSpecificBootSetup()
  {

    /*
    //wait for resource engine ?
    while (EngineManager.isLoading()) yield return null;
    yield return null;
    EngineLoader.loadScenes(new string[] { "network-view", "resource-camera" });
    */

    yield return null;
  }
  
  virtual protected void setup()
  { }

  void Update()
  {
    //update client data info :shrug:
    //le calcul de size / frame
    for (int i = 0; i < clientDatas.Count; i++)
    {
      clientDatas[i].update(this as NwkServer);
    }

    bool _connected = isConnected();
    if (_localConnectionStatus != _connected)
    {
      if (_connected) onStateConnected();
      else onStateDisconnected();

      _localConnectionStatus = _connected;
    }

    updateNetwork();
  }

  /// <summary>
  /// describe how to register an handle
  /// </summary>
  abstract protected void registerHandle(short messageID, NetworkMessageDelegate callback);

  /// <summary>
  /// all handler needed
  /// </summary>
  virtual protected void registerHandlers()
  {
    registerHandle(NwkMessageBasic.MSG_ID_BASIC, unetOnMessageBasic);
    registerHandle(NwkMessageComplexe.MSG_ID_COMPLEXE, unetOnMessageComplexe);
    registerHandle(NwkMessageTransaction.MSG_ID_TRANSACTION, unetOnMessageTransaction);
    registerHandle(NwkMessageFull.MSG_ID_FULL, unetOnMessageFull);

    //registerHandle(NwkMessageCustom.MSG_ID_CUSTOM, unetOnMessageCustom);
  }

  void unetOnMessageBasic(NetworkMessage netMessage) => solveBasic(netMessage.ReadMessage<NwkMessageBasic>(), netMessage.conn.connectionId);
  void unetOnMessageComplexe(NetworkMessage netMessage) => solveComplexe(netMessage.ReadMessage<NwkMessageComplexe>(), netMessage.conn.connectionId);
  void unetOnMessageTransaction(NetworkMessage netMessage) => solveTransaction(netMessage.ReadMessage<NwkMessageTransaction>(), netMessage.conn.connectionId);
  void unetOnMessageFull(NetworkMessage netMessage) => solveFull(netMessage.ReadMessage<NwkMessageFull>(), netMessage.conn.connectionId);
  //void unetOnMessageCustom(NetworkMessage netMessage) => solveCustom(netMessage.ReadMessage<NwkMessageCustom>(), netMessage.conn.connectionId);

  abstract protected void solveBasic(NwkMessageBasic message, int connID);
  abstract protected void solveComplexe(NwkMessageComplexe message, int connID);
  abstract protected void solveTransaction(NwkMessageTransaction message, int connID);
  abstract protected void solveFull(NwkMessageFull message, int connID);
  //abstract protected void solveCustom(NwkMessageCustom message, int connID);

  virtual protected void onStateConnected()
  {
    Debug.Log("<b>"+GetType() + " connected !</b>");

    uiView?.setConnected(true);

    tick.resetTickCount();
  }

  virtual protected void onStateDisconnected()
  {
    if (uiView != null) uiView.setConnected(false);
  }

  virtual protected void updateNetwork()
  {

    if(uiView != null && tick != null)
    {
      uiView.txtTick.text = tick.getTickCount().ToString();
    }
    
  }

  protected NwkClientData addClient(int newUid, int newConnId = -1)
  {
    NwkClientData data = getClientData(newUid);

    if (data == null)
    {
      data = new NwkClientData(newUid);
      data.connId = newConnId;

      clientDatas.Add(data);
    }

    //data.setConnected();

    return data;
  }

  public NwkClientData getClientData(int uid)
  {
    for (int i = 0; i < clientDatas.Count; i++)
    {
      if (clientDatas[i].nwkUid == uid) return clientDatas[i];
    }
    return null;
  }

  public int countConnectedClients()
  {
    return clientDatas.Count;
  }

  public void logError(string ct)
  {
    log("<color=red>" + ct + "</color>");
  }

  public void log(string ct, bool silent = false)
  {
    if (NwkClient.isClient()) ct = NwkClient.nwkUid + " " + ct;

    Debug.Log(ct);

    if (uiView == null)
    {
      return;
    }

    if(!silent) uiView.addLog(ct);
    else uiView.addRaw(ct);
  }

  public T getModule<T>() where T : NwkModule
  {
    T cmp = GetComponentInChildren<T>();
    if (cmp != null) return cmp;
    
    cmp = new GameObject("[" + typeof(T).ToString() + "]").AddComponent<T>();
    cmp.transform.SetParent(transform);

    return cmp;
  }

  NwkModule[] getAllModules()
  {
    List<NwkModule> mods = new List<NwkModule>();
    //mods.AddRange(transform.GetComponents<NwkModule>());
    mods.AddRange(transform.GetComponentsInChildren<NwkModule>());
    return mods.ToArray();
  }

  private void OnGUI()
  {
    NwkModule[] mods = getAllModules();

    //GUILayout.BeginArea
    GUILayout.BeginHorizontal();
    for (int i = 0; i < mods.Length; i++)
    {
      GUILayout.BeginVertical(GUILayout.Width(200f));
      mods[i].drawGui();
      GUILayout.EndVertical();
    }
    GUILayout.EndHorizontal();

  }

  void OnApplicationQuit()
  {
    Debug.LogWarning("<color=red>APPLICATION QUIT CONNECTION KILL</color>");
    disconnect();
  }

  abstract public void disconnect();
  abstract public void connect();
  abstract public bool isConnected(); // server has wrapper to send msg

  static public bool isClient() => NwkClient.nwkClient != null;
  static public bool isServer() => NwkServer.nwkServer != null;

  static public bool isNetwork()
  {
    if (NwkServer.nwkServer != null) return true;
    if (NwkClient.nwkClient != null) return true;
    return false;
  }
}
