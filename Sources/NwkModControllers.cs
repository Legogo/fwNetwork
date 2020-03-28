using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// module meant to sync plugged controller info
/// </summary>

public class NwkModControllers : NwkModuleClient, INwkSyncable
{
  //list keep uids of old controllers but track connected state
  List<NwkModControllerState> _controllers = new List<NwkModControllerState>();

  /// <summary>
  /// deviceUid is id given by Input system for each controller
  /// </summary>
  public void extControllerPlugged(string deviceUid)
  {
    int idx = getControllerIndexByUid(deviceUid);
    
    if(idx < 0)
    {
      _controllers.Add(new NwkModControllerState() { controllerUid = deviceUid, connected = true });
      log(getStamp() + " new controller detected, uid : " + deviceUid + " ; new controller count = " + _controllers.Count);
    }
    else
    {
      log(getStamp() + " knwon controller " + deviceUid + " re-connected");
      NwkModControllerState cs = _controllers[idx];
      cs.connected = true;
      _controllers[idx] = cs;
    }

  }

  public void extControllerUnPlugged(string deviceUid)
  {

    int idx = getControllerIndexByUid(deviceUid);
    Debug.Assert(idx < 0, "deconnection of unknown controller ?");
    if (idx < 0) return;

    NwkModControllerState cs = _controllers[idx];
    cs.connected = false;
    _controllers[idx] = cs;

    log(getStamp() + " controller " + deviceUid + " disconnected");

  }

  protected int getControllerIndexByUid(string uid)
  {
    for (int i = 0; i < _controllers.Count; i++)
    {
      if (_controllers[i].controllerUid == uid) return i;
    }
    return -1;
  }


  /// <summary>
  /// don't need to receive data
  /// </summary>
  public void unpack(object obj) { }

  /// <summary>
  /// sending data to server
  /// </summary>
  public object pack()
  {
    NwkModControllerData data = new NwkModControllerData();

    data.nwkUid = NwkClient.getParsedNwkUid();
    data.controllers = _controllers.ToArray();

    return data;
  }

  NwkSyncableData syncData;
  public NwkSyncableData getData()
  {
    if (syncData == null) syncData = new NwkSyncableData(this, 1f);
    return syncData;
  }
}

[System.Serializable]
public struct NwkModControllerData
{
  public int nwkUid;
  public NwkModControllerState[] controllers;
}

[System.Serializable]
public struct NwkModControllerState
{
  public string controllerUid;
  public bool connected;
}
