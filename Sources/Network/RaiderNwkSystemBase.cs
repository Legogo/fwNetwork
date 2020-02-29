using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RaiderNwkSystemBase : MonoBehaviour
{
  // The id we use to identify our messages and register the handler
  protected short messageID = 1000;

  protected RaiderNwkMessageListener listener;

  NwkUiView nwkUiView;

  protected Dictionary<string, NwkClientData> clients = new Dictionary<string, NwkClientData>();

  virtual protected void Awake()
  {
    listener = RaiderNwkMessageListener.getListener();
    if (listener == null) listener = gameObject.AddComponent<RaiderNwkMessageListener>();

    EngineLoader.loadScenes(new string[] { "network-view", "resource-camera" });
  }
  
  IEnumerator Start()
  {
    
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

    clients.Add(newUid, data);

    refreshClientList();
  }

  protected void refreshClientList()
  {
    List<NwkClientData> list = new List<NwkClientData>();
    foreach(KeyValuePair<string, NwkClientData> kp in clients)
    {
      list.Add(kp.Value);
    }
    nwkUiView.refreshClientList(list);
  }

  protected void log(string ct)
  {
    nwkUiView.addLog(ct);
  }
  
}
