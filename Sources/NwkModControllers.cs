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

public class NwkModControllers : NwkModule, iNwkSync
{
  SolverControllers sControl;

  protected override void setupModule()
  {
    base.setupModule();

    sControl = qh.gc<SolverControllers>();
    Debug.Assert(sControl);

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
  public object pack()
  {
    return sControl.clients;
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

    for (int i = 0; i < sControl.clients.Count; i++)
    {
      List<SolverControllerState> states = sControl.clients[i].states;

      string ids = "";
      for (int j = 0; j < states.Count; j++)
      {
        if (j != 0) ids += ",";
        ids += states[j].deviceId;
      }

      GUILayout.Label(sControl.clients[i].nwkUID+ " ? " + ids);
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
