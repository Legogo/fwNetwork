using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NwkSyncer : NwkMono
{
  static protected NwkSyncer instance;

  //list used by client to find objects concerned by a new message
  //unpack is usually called using this list
  public List<NwkSyncableData> syncs = new List<NwkSyncableData>();

  bool useFrequency = false;
  float stackTimerFrequency = 0.33f;
  float stackTimer = 0f;

  List<NwkSyncableData> stack = new List<NwkSyncableData>();

  override protected void Awake()
  {
    base.Awake();
    instance = this;

    //DontDestroyOnLoad(gameObject);
  }

  private void Start()
  {
    if (nwkServer != null)
    {
      enabled = false;
    }
  }

  private void Update()
  {

    for (int i = 0; i < syncs.Count; i++)
    {
      //check if timer reached target (and is rebooted)
      if(syncs[i].update(Time.deltaTime))
      {
        stack.Add(syncs[i]);
      }
    }

    if(useFrequency)
    {
      if (stackTimer < stackTimerFrequency)
      {
        stackTimer += Time.deltaTime;
        if (stackTimer >= stackTimerFrequency)
        {
          sendStack();
        }
      }
    }
    else
    {
      sendStack();
    }
    
  }

  void sendStack()
  {
    stackTimer = 0f;

    //nothing to send
    if (stack.Count <= 0) return;

    Debug.Log("sending " + stack.Count + " msg(s)");

    for (int i = 0; i < stack.Count; i++)
    {
      nwkClient.sendWrapperClient.sendClientToServer(stack[i].packMessage());
    }

    stack.Clear();
  }

  public bool hasSync(NwkSyncableData sync)
  {
    for (int i = 0; i < instance.syncs.Count; i++)
    {
      if (instance.syncs[i].handle == sync)
      {
        return true;
      }
    }
    return false;
  }

  public void sub(NwkSyncableData sync)
  {
    
    if(hasSync(sync))
    {
      Debug.LogError("already has that sync ?");
      return;
    }

    syncs.Add(sync);
  }

  public void unsub(NwkSyncableData sync)
  {
    if(!hasSync(sync))
    {
      Debug.LogError("don't have that sync to unsub");
      return;
    }

    int idx = 0;
    while(idx < syncs.Count)
    {
      if (syncs[idx].handle == sync) syncs.RemoveAt(idx);
      else idx++;
    }
  }

  /// <summary>
  /// bridge when receiving new message
  /// </summary>
  static public void apply(string syncId, NwkMessage msg)
  {
    bool found = false;
    for (int i = 0; i < instance.syncs.Count; i++)
    {
      if (instance.syncs[i].syncUid == syncId)
      {
        instance.syncs[i].unpackMessage(msg); // tell object to treat inc data
        found = true;
      }
    }

    if(!found)
    {
      Debug.LogWarning("didn't find object with sync id : " + syncId);
    }
  }

  /// <summary>
  /// basic message for sync
  /// </summary>
  static public NwkMessage nwkSyncInject(NwkSyncableData syncData)
  {
    NwkMessage msg = new NwkMessage();

    msg.setScope(1);
    msg.setupMessage(syncData.syncUid);
    msg.setupNwkType((int)RobbersNwkMessageType.sync);
    msg.setupMessageData(syncData.handle.pack());
    msg.silentLogs = true;

    return msg;
  }

}
