using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NwkUiView : MonoBehaviour
{
  public Text txtLabel;
  public Text txtClients;
  public Button btnConnect;
  public Image stConnection;

  [Header("logs list")]
  public Text txtRaw;
  public Text txtLogs;

  NwkUiViewLogs raws;
  NwkUiViewLogs logs;

  private void Awake()
  {
    txtLabel.text = "";
    txtClients.text = "";

    raws = new NwkUiViewLogs(txtRaw);
    logs = new NwkUiViewLogs(txtLogs);

    btnConnect.gameObject.SetActive(false);
  }

  public void setLabel(string newLabel)
  {
    txtLabel.text = newLabel;
  }

  void Update()
  {
    bool _server = NwkSystemBase.isServer();

    List<NwkClientData> datas = NwkSystemBase.nwkSys.clientDatas;

    string ct = "clients x"+ datas.Count;

    for (int i = 0; i < datas.Count; i++)
    {
      //datas[i].update(); // update size timer

      //STATE
      ct += "\n #" + datas[i].nwkUid+"\t("+ datas[i].state+")";

      if (datas[i].isDisconnected()) continue;

      //PING

      // ping display (client = ping ; server = timeout)
      ct += "\n ping " + datas[i].getPingDelta();

      //SIZE

      if(_server) ct += "\n size " + datas[i].sizeSeconds;
    }

    txtClients.text = ct;
  }

  public void addRaw(string ct)
  {
    raws.addLog(ct);
  }

  public void addLog(string ct)
  {
    addRaw(ct);

    logs.addLog(ct);

    Debug.Log("<color=orange>nwk</color> "+ct);
  }

  public void setConnected(bool connected)
  {
    stConnection.color = connected ? Color.green : Color.red;
    
    if (!btnConnect.gameObject.activeSelf) btnConnect.gameObject.SetActive(true);
    btnConnect.GetComponentInChildren<Text>().text = connected ? "Disconnect" : "Connect";
  }

  public void onConnectButtonPressed()
  {
    if (!NwkClient.isClient()) return;

    bool clientConnection = NwkClient.nwkClient.isConnected();
    NwkClient.nwkClient.log("clicked : "+btnConnect.GetComponentInChildren<Text>().text+" / is connected ? "+clientConnection);

    if (clientConnection)
    {
      NwkClient.nwkClient.disconnect();
    }
    else
    {
      NwkClient.nwkClient.connectToIpPort(); // ui view button connect
    }
  }

}
