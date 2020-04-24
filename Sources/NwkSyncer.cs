using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
  public const char MSG_HEADER_SPEARATOR = '-';

  static public NwkSyncer instance;

  List<NwkMessageFull> stack = new List<NwkMessageFull>();

  //all objects that have capacities to sync son data
  List<NwkSyncableData> broadcasters = new List<NwkSyncableData>();

  //all objects that will sync their state at interval
  //should be all ONLY local objects ?
  List<NwkSyncableData> frequencers = new List<NwkSyncableData>();

  //list used by client to find objects concerned by a new message
  //unpack is usually called using this list
  //public List<INwkSyncable> syncs = new List<INwkSyncable>();

  //is syncer waiting before sending a stack of message ?
  bool useFrequency = false;
  float stackTimerFrequency = 0.33f;
  float stackTimer = 0f;

  public DataNwkFactory factoryDb;

  override protected void Awake()
  {
    base.Awake();
    instance = this;

    //DontDestroyOnLoad(gameObject);

    if(factoryDb == null)
    {
      Debug.LogWarning("no factory ? no copy will be possible");
    }
  }

  private void Start()
  {
    if (nwkServer != null)
    {
      GameObject.DestroyImmediate(this);
      //enabled = false;
    }

    Debug.Assert(nwkClient != null, "need client at this point");
  }

  private void Update()
  {
    if (!NwkSystemBase.nwkSys.isConnected())
    {
      return;
    }

    solveStack();
    //Debug.Log("has " + syncs.Count + " sync(s) & "+stack.Count+" msg(s)");

    if (useFrequency) //wait before sending all stored messages
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
    else //send all stored messages whenever there is one
    {
      sendStack();
    }
    
  }

  void solveStack()
  {

    for (int i = 0; i < frequencers.Count; i++)
    {
      //NwkSyncableData data = syncs[i].getData();
      NwkSyncableData data = frequencers[i];

      //check if timer reached target (and is rebooted)
      if (data.updateFreqTimer(Time.deltaTime))
      {
        stack.Add(data.packMessage());
        data.resetFreqTimer();
      }
      //else Debug.Log(data.handle+" => "+data.getRemainingTime());
    }

  }

  void sendStack()
  {
    stackTimer = 0f;

    //nothing to send
    if (stack.Count <= 0) return;

    //Debug.Log("sending " + stack.Count + " msg(s)");

    for (int i = 0; i < stack.Count; i++)
    {
      nwkClient.sendWrapperClient.sendClientToServer(stack[i]);
    }

    stack.Clear();
  }

  public bool hasBroad(INwkSyncable sync)
  {
    for (int i = 0; i < broadcasters.Count; i++)
    {
      if (broadcasters[i].handle == sync)
      {
        return true;
      }
    }
    return false;
  }

  public NwkSyncableData sub(INwkSyncable sync)
  {
    if(hasBroad(sync))
    {
      Debug.LogError("already has that sync ? " + sync);
      return null;
    }

    // at this point the generated IID is not final, it can be overritten just after sub (if copy)
    NwkSyncableData data = sync.getData();

    if (data != null)
    {
      broadcasters.Add(data);
      
      //on sub l'objet que si c'est un objet local
      if(data.idCard.syncNwkClientUID == NwkClient.nwkUid)
      {
        frequencers.Add(data);
      }

      Debug.Log("sub '" + sync + "' to syncer");
    }
    else Debug.LogWarning(sync + " was not sub to syncer ?");

    return data;
  }

  public void unsub(INwkSyncable sync)
  {

    if (!hasBroad(sync))
    {
      Debug.LogError("don't have that sync to unsub");
      return;
    }

    int idx = 0;
    while(idx < broadcasters.Count)
    {
      if (broadcasters[idx].handle == sync) stack.RemoveAt(idx);
      else idx++;
    }
  }




  /// <summary>
  /// bridge when receiving new message
  /// </summary>
  public void applyMessage(NwkMessageFull msg)
  {
    string header = msg.getHeader();
    string[] split = header.Split(MSG_HEADER_SPEARATOR);

    short iid = short.Parse(split[0]);

    NwkSyncableData data = getDataByIID(iid);

    if (data == null)
    {
      Debug.LogWarning("no sync data found for msg " + header);

      short oType = short.Parse(split[1]);

      data = solveUnknownData(msg.getIdCard().getMessageSender(), iid, oType);

      if(data == null)
      {
        log("don't have object " + msg.messageHeader + " sent by : " + msg.getIdCard().getMessageSender());
      }
      
    }

    //must have data here ?
    if (data == null)
    {
      Debug.LogError("no data, must have some here");
      return;
    }
    
    data.unpackMessage(msg); // tell object to treat inc data
  }

  protected NwkSyncableData solveUnknownData(int cUID, int oIID, int oPID)
  {
    GameObject copy = factoryDb.copy(oPID);

    if (copy == null)
    {
      Debug.LogWarning("no copy possible with PID : " + oPID);
      return null;
    }

    copy.name = oIID.ToString();

    Debug.Log(Time.frameCount + " => copy ? " + copy);

    INwkSyncable sync = copy.GetComponent<INwkSyncable>();
    if (sync == null) sync = copy.GetComponentInChildren<INwkSyncable>();

    if(sync == null)
    {
      Debug.LogError("no sync found in copy ?");
      return null;
    }

    //copy object will create it's own data during constructor
    //but won't have owner info yet
    NwkSyncableData data = sync.getData().overrideData(cUID, oIID, oPID);
    
    Debug.Log(Time.frameCount+" => data ? " + data);

    return data;
  }

  NwkSyncableData getDataByIID(short iid)
  {
    NwkSyncableData data = null;

    for (int i = 0; i < broadcasters.Count; i++)
    {
      if (broadcasters[i] == null) Debug.LogError(i + " is null (count ? "+ broadcasters.Count+") ? iid "+iid);
      //if (syncs[i].idCard == null) Debug.LogError("no id card ?");

      if (broadcasters[i].idCard.syncIID == iid)
      {
        data = broadcasters[i];
      }
    }

    return data;
  }

  bool hasBroadOfIID(short syncIID)
  {
    for (int i = 0; i < broadcasters.Count; i++)
    {
      if (broadcasters[i].idCard.syncIID == syncIID) return true;
    }
    return false;
  }



  /// <summary>
  /// basic message for sync
  /// </summary>
  static public NwkMessageFull nwkSyncInject(NwkSyncableData syncData)
  {
    NwkMessageFull msg = new NwkMessageFull();

    msg.getIdCard().setupId(NwkClient.nwkUid, (int)eNwkMessageType.SYNC);

    //header is body of message (not sender uid)
    string header = syncData.idCard.syncIID.ToString();

    header += "-" + syncData.idCard.syncPID;

    msg.setupHeader(header);

    msg.setupMessageData(syncData.handle.pack());

    return msg;
  }

}
