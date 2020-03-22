using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NwkSystemBase : MonoBehaviour
{
  public static NwkSystemBase nwkSys;

  // The id we use to identify our messages and register the handler
  protected short messageID = 1000;

  protected NwkMessageListener listener;

  NwkUiView nwkUiView;

  public List<NwkClientData> clientDatas = new List<NwkClientData>();

  virtual protected void Awake()
  {
    nwkSys = this;

    listener = NwkMessageListener.getListener();
    if (listener == null) listener = gameObject.AddComponent<NwkMessageListener>();
  }
  
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

  public void log(string ct)
  {
    nwkUiView.addLog(ct);
  }

  public T getModule<T>() where T : NwkModule
  {
    T cmp = GetComponent<T>();
    if (cmp != null) return cmp;
    return gameObject.AddComponent<T>();
  }

  static public bool isClient() => NwkClient.nwkClient != null;
  static public bool isServer() => NwkServer.nwkServer != null;

  static public bool isNetwork()
  {
    if (NwkServer.nwkServer != null) return true;
    if (NwkClient.nwkClient != null) return true;
    return false;
  }
}
