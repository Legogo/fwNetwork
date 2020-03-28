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

  NwkUiView nwkUiView;
  //protected NwkSendWrapper sendWrapper; // wrapper must be generated only when connection is active

  public List<NwkClientData> clientDatas = new List<NwkClientData>();

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

    //wait for resource engine ?
    while (EngineManager.isLoading()) yield return null;

    yield return null;

    EngineLoader.loadScenes(new string[] { "network-view", "resource-camera" });

    //Debug.Log("waiting for nwk ui view ...");

    while (nwkUiView == null)
    {
      nwkUiView = GameObject.FindObjectOfType<NwkUiView>();
      yield return null;
    }

    nwkUiView.setLabel(GetType().ToString());

    yield return null;

    setup();
  }

  virtual protected void setup()
  { }

  private void Update()
  {
    for (int i = 0; i < clientDatas.Count; i++)
    {
      clientDatas[i].update(this as NwkServer);
    }

    updateNetwork();
  }

  virtual protected void updateNetwork()
  { }

  protected void addClient(string newUid)
  {
    NwkClientData data = new NwkClientData();
    data.uid = newUid;
    
    clientDatas.Add(data);
  }

  public NwkClientData getClientData(string uid)
  {
    for (int i = 0; i < clientDatas.Count; i++)
    {
      if (clientDatas[i].uid == uid) return clientDatas[i];
    }
    return null;
  }

  public int countConnectedClients()
  {
    return clientDatas.Count;
  }

  public void log(string ct, bool silent = false)
  {
    if(!silent) nwkUiView.addLog(ct);
    else nwkUiView.addRaw(ct);
  }

  public T getModule<T>() where T : NwkModule
  {
    T cmp = GetComponentInChildren<T>();
    if (cmp != null) return cmp;
    
    cmp = new GameObject("[" + typeof(T).ToString() + "]").AddComponent<T>();
    cmp.transform.SetParent(transform);

    return cmp;
  }

  void OnApplicationQuit()
  {
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
