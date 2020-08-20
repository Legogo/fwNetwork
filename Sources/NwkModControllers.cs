using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

/// <summary>
/// module meant to sync plugged controller info
/// 
/// TODO
/// all data must be contained in nwk fwp
/// no dep out of nwk fwp
/// 
/// </summary>

abstract public class NwkModControllers : NwkModule, iNwkSync
{
  protected override void setupModule()
  {
    base.setupModule();

    this.subSync();
  }

  public int getSyncIID() => getData().idCard.syncIID;

  void merge(List<SolverClientControllersWrapper> wrappers)
  {
    throw new NotImplementedException();
  }

  public void unpack(object obj)
  {
    List<SolverClientControllersWrapper> wrappers = (List<SolverClientControllersWrapper>)obj;
    Debug.Assert(wrappers != null, "can't unpack object into list of client controllers ?");
    merge(wrappers);
  }

  /// <summary>
  /// sending data to server
  /// </summary>
  public object pack() => getClients();

  /// <summary>
  /// returns a list of solved clients in context
  /// </summary>
  abstract protected List<SolverClientControllersWrapper> getClients();

  NwkSyncableData syncData;
  public NwkSyncableData getData()
  {
    if (syncData == null) syncData = new NwkSyncableData(this, 1f); // mod controller
    return syncData;
  }







  public override void drawGui()
  {
    base.drawGui();

    List<SolverClientControllersWrapper> clients = getClients();

    for (int i = 0; i < clients.Count; i++)
    {
      List<SolverControllerState> states = clients[i].states;

      string ids = "";
      for (int j = 0; j < states.Count; j++)
      {
        if (j != 0) ids += ",";
        ids += states[j].deviceId;
      }

      GUILayout.Label(clients[i].nwkUID+ " ? " + ids);
    }
    
  }

  protected override bool canUpdate()
  {
    return true;
  }

}

[System.Serializable]
public class SolverClientControllersWrapper
{
  public short nwkUID;
  public List<SolverControllerState> states;
}

[System.Serializable]
public class SolverControllerState
{
  public int deviceId = -1;
  public short brainUID = -1;
}
