using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NwkSystemBase : MonoBehaviour
{
  // The id we use to identify our messages and register the handler
  protected short messageID = 1000;

  protected NwkMessageListener listener;

  NwkUiView nwkUiView;

  protected Dictionary<string, NwkClientData> clients = new Dictionary<string, NwkClientData>();

  virtual protected void Awake()
  {
    listener = NwkMessageListener.getListener();
    if (listener == null) listener = gameObject.AddComponent<NwkMessageListener>();

  }
  
  IEnumerator Start()
  {
    yield return null;
    yield return null;

    EngineLoader.loadScenes(new string[] { "network-view", "resource-camera" });

    Debug.Log("waiting for nwk ui view");
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
