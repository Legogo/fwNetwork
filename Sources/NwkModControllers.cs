using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// module meant to sync plugged controller info
/// </summary>

public class NwkModControllers : NwkModule, iNwkSync
{
  bool useUniqIds = false;

  //list keep uids of old controllers but track connected state
  List<NwkModClientControllers> _clientsControllers = new List<NwkModClientControllers>();

  protected override void setupModule()
  {
    base.setupModule();

    this.subSync();
  }

  public int getSyncIID() => getData().idCard.syncIID;

  NwkModClientControllers getClientControllers(short nwkUid)
  {
    for (int i = 0; i < _clientsControllers.Count; i++)
    {
      if (_clientsControllers[i].clientNwkUid == nwkUid) return _clientsControllers[i];
    }
    return null;
  }

  /// <summary>
  /// deviceUid is id given by Input system for each controller
  /// </summary>
  public void extControllerPlugged(short deviceNwkUid, int deviceUid)
  {
    NwkModClientControllers cc = getClientControllers(deviceNwkUid);

    bool add = false;

    if(cc == null)
    {
      cc = new NwkModClientControllers() { clientNwkUid = deviceNwkUid };
      _clientsControllers.Add(cc);

      log(getStamp() + " new controller client detected : " + deviceNwkUid+" ; count = "+_clientsControllers.Count);
      add = true;
    }
    else
    {
      //check if existing and solve connected state
      SolverControllerState scs = cc.clientControllers.Select(x => x).Where(x => x.deviceId == deviceUid).FirstOrDefault();
      if (scs == null) add = true;
      else
      {
        if (scs.connected) Debug.LogWarning("controller is marked as already connected ??");
        else
        {
          scs.connected = true;

          log(getStamp() + " knwon controller " + deviceUid + " re-connected");
        }
      }
      
    }

    //need to add new controller
    if(add)
    {
      cc.clientControllers.Add(new SolverControllerState() { deviceId = deviceUid, connected = true });
      log(getStamp() + " new controller detected, uid : " + deviceUid + " (client : " + deviceNwkUid + ") ; count = " + cc.clientControllers.Count);
    }

  }

  public void extControllerUnplugged(short deviceNwkUid, int deviceUid)
  {
    NwkModClientControllers cc = getClientControllers(deviceNwkUid);
    if(cc == null)
    {
      Debug.LogWarning("client " + deviceNwkUid + " unknown ? / "+_clientsControllers.Count);
      return;
    }

    //because controller can't be tracked by uid, need to remove
    if(!useUniqIds)
    {
      cc.removeControllerOfId(deviceUid);
    }
    else
    {
      SolverControllerState control = cc.clientControllers.Select(x => x).Where(x => x.deviceId == deviceUid).FirstOrDefault();
      control.connected = false;
    }


    log(getStamp() + " controller " + deviceUid + " is now set as disconnected");
  }

  void merge(List<NwkModClientControllers> tmpList)
  {
    for (int i = 0; i < _clientsControllers.Count; i++)
    {
      //only others
      if(_clientsControllers[i].clientNwkUid != NwkClient.nwkUid)
      {

        //update controllers of received client data
        bool found = false;
        for (int j = 0; j < tmpList.Count; j++)
        {
          if(_clientsControllers[i].clientNwkUid == tmpList[j].clientNwkUid)
          {
            _clientsControllers[i].clientControllers = tmpList[j].clientControllers;
            found = true;
          }
        }
        
        if(!found)
        {
          Debug.LogWarning("couldn't find " + _clientsControllers[i].clientNwkUid + " into nwk message list");
        }
      }
    }
  }


  public void unpack(object obj)
  {
    List<NwkModClientControllers> nwkList = (List<NwkModClientControllers>)obj;
    Debug.Assert(nwkList != null, "can't unpack object into list of client controllers ?");

    merge(nwkList);
  }

  /// <summary>
  /// sending data to server
  /// </summary>
  public object pack()
  {
    return _clientsControllers;
  }

  NwkSyncableData syncData;
  public NwkSyncableData getData()
  {
    if (syncData == null) syncData = new NwkSyncableData(this, 1f); // mod controller
    return syncData;
  }







  public override void drawGui()
  {
    base.drawGui();

    foreach(NwkModClientControllers cc in _clientsControllers)
    {
      string ids = "";
      for (int j = 0; j < cc.clientControllers.Count; j++)
      {
        if (j != 0) ids += ",";
        ids += "<color=";
        ids += (cc.clientControllers[j].connected) ? "green" : "red";
        ids += ">" + cc.clientControllers[j].deviceId + "</color>";
      }

      GUILayout.Label(cc.clientNwkUid + " ? " + ids);
    }
    
  }

  protected override bool canUpdate()
  {
    return true;
  }

}

[System.Serializable]
public class NwkModClientControllers
{
  public short clientNwkUid = -1;
  public List<SolverControllerState> clientControllers = new List<SolverControllerState>();

  public bool removeControllerOfId(int deviceId)
  {
    int idx = 0;
    while(idx < clientControllers.Count)
    {
      if (clientControllers[idx].deviceId == deviceId) clientControllers.RemoveAt(idx);
      else idx++;
    }

    return clientControllers.Count > 0;
  }
}

[System.Serializable]
public class SolverControllerState
{
  public int deviceId;
  public bool connected; // can't track uniq id of controller : this field is now deprecated/ignored
}
