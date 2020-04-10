﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// bridge entre client <-> server et les INwkSyncable(s)
/// 
/// chaque entité a la liste complète des objets qui se sync
/// - soit le client est a l'origine de l'objet
/// - soit il en attend les informations par le server
/// 
/// </summary>

public class NwkSyncer : NwkMono
{
  static public NwkSyncer instance;

  List<NwkSyncableData> stack = new List<NwkSyncableData>();

  //list used by client to find objects concerned by a new message
  //unpack is usually called using this list
  public List<INwkSyncable> syncs = new List<INwkSyncable>();

  //deprecated
  bool useFrequency = false;
  float stackTimerFrequency = 0.33f;
  float stackTimer = 0f;

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
      GameObject.DestroyImmediate(this);
      //enabled = false;
    }
  }

  private void Update()
  {
    if(!NwkSystemBase.nwkSys.isConnected())
    {
      return;
    }

    for (int i = 0; i < syncs.Count; i++)
    {
      NwkSyncableData data = syncs[i].getData();

      //check if timer reached target (and is rebooted)
      if(data.update(Time.deltaTime))
      {
        stack.Add(data);
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

  public bool hasSync(INwkSyncable sync)
  {
    for (int i = 0; i < instance.syncs.Count; i++)
    {
      if (instance.syncs[i] == sync)
      {
        return true;
      }
    }
    return false;
  }

  public void sub(INwkSyncable sync)
  {
    if(hasSync(sync))
    {
      Debug.LogError("already has that sync ?");
      return;
    }

    //NwkSyncableData data = sync.getData();
    syncs.Add(sync);
  }

  public void unsub(INwkSyncable sync)
  {

    if (!hasSync(sync))
    {
      Debug.LogError("don't have that sync to unsub");
      return;
    }

    int idx = 0;
    while(idx < syncs.Count)
    {
      if (syncs[idx] == sync) syncs.RemoveAt(idx);
      else idx++;
    }
  }




  /// <summary>
  /// bridge when receiving new message
  /// </summary>
  public void applyMessage(NwkMessage msg)
  {
    bool found = false;
    for (int i = 0; i < syncs.Count; i++)
    {
      NwkSyncableData data = syncs[i].getData();

      string header = msg.getHeader();
      string[] tmp = header.Split('-');

      if (data.syncUid == tmp[0])
      {
        data.unpackMessage(msg); // tell object to treat inc data
        found = true;
      }
    }

    if(!found)
    {
      Debug.LogWarning("didn't find object with sync id : " + msg.messageHeader);
    }
  }

  bool hasObjectOfSyncUid(string syncUid)
  {
    for (int i = 0; i < syncs.Count; i++)
    {
      if (syncs[i].getData().syncUid == syncUid) return true;
    }
    return false;
  }



  /// <summary>
  /// basic message for sync
  /// </summary>
  static public NwkMessage nwkSyncInject(NwkSyncableData syncData)
  {
    NwkMessage msg = new NwkMessage();

    msg.setupNwkType(NwkMessageType.SYNC);

    string header = syncData.syncUid+"-"+syncData.handle.GetType();
    msg.setupHeader(header);
    //msg.setupMessage(syncData.syncUid);

    msg.setupMessageData(syncData.handle.pack());

    msg.silentLogs = true;

    return msg;
  }

}
