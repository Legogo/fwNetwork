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

    refreshClientList(new List<NwkClientData>());
  }

  public void setLabel(string newLabel)
  {
    txtLabel.text = newLabel;
  }

  public void refreshClientList(List<NwkClientData> clients)
  {
    string ct = "clients x"+ clients.Count;
    for (int i = 0; i < clients.Count; i++)
    {
      ct += "\n #" + clients[i].uid+" ("+clients[i].state+")";
      if(clients[i].timeout > 0f) ct += "\n  "+clients[i].timeout;
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
  }
}
