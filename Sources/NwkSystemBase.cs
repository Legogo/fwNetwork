using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

abstract public class NwkSystemBase : MonoBehaviour
{
  public static NwkSystemBase nwkSys;

  // The id we use to identify our messages and register the handler
  protected short messageID = 1000;

  protected NwkMessageListener listener;

  public bool useUiView = false;
  protected NwkUiView uiView;
  //protected NwkSendWrapper sendWrapper; // wrapper must be generated only when connection is active

  public List<NwkClientData> clientDatas = new List<NwkClientData>();

  bool _localConnectionStatus = false;


  virtual protected void Awake()
  {
    nwkSys = this;

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

    if(useUiView)
    {
      Debug.Log("loading debug ui view");

      AsyncOperation async = NwkUnityTools.loadScene("network-view");
      
      if(async != null)
      {
        while (!async.isDone) yield return null;
      }
      
      uiView = GameObject.FindObjectOfType<NwkUiView>();
      uiView.setLabel(GetType().ToString());

      //Debug.Log(uiView);
    }

    yield return null;

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

  virtual protected void onStateConnected()
  {
    if (uiView != null) uiView.setConnected(true);
  }

  virtual protected void onStateDisconnected()
  {
    if (uiView != null) uiView.setConnected(false);
  }

  virtual protected void updateNetwork()
  { }

  protected NwkClientData addClient(string newUid, int newConnId = -1)
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

  public NwkClientData getClientData(string uid)
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

  public void log(string ct, bool silent = false)
  {
    if (uiView == null) return;

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
  abstract public bool isConnected();

  static public bool isClient() => NwkClient.nwkClient != null;
  static public bool isServer() => NwkServer.nwkServer != null;

  static public bool isNetwork()
  {
    if (NwkServer.nwkServer != null) return true;
    if (NwkClient.nwkClient != null) return true;
    return false;
  }
}
