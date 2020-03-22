using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NwkUiView : MonoBehaviour
{
  public Text txtLabel;
  public Text txtLogs;
  public Text txtClients;

  private void Awake()
  {
    txtLabel.text = "";
    txtLogs.text = "";
    txtClients.text = "";

  }

  public void setLabel(string newLabel)
  {
    txtLabel.text = newLabel;
  }

  private void Update()
  {
    refreshClientList();
  }

  void refreshClientList()
  {
    List<NwkClientData> datas = NwkSystemBase.nwkSys.clientDatas;

    string ct = "clients x"+ datas.Count;

    for (int i = 0; i < datas.Count; i++)
    {
      ct += "\n #" + datas[i].uid+"\t"+ datas[i].state+")";
      if(datas[i].timeout > 0f) ct += "\n  "+ datas[i].timeout;

      float ping = datas[i].ping; // default is client ping
      if (NwkSystemBase.isServer()) ping = NwkModPing.getMilliSec(Time.realtimeSinceStartup - ping); // server is last seen time
      
      ct += "\n ping " + ping;
    }

    txtClients.text = ct;
  }

  public void addLog(string ct)
  {
    Debug.Log(ct);

    string header = "\n"+Time.frameCount + " | ";
    ct = header + ct;

    //add on top
    ct += txtLogs.text;
    txtLogs.text = ct;

    //add on bottom
    //txtLogs.text += ct;
  }
}
