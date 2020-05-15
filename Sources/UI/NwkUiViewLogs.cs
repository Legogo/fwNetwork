using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NwkUiViewLogs : NwkUiView
{
  public Text txtLabel;
  public Text txtClients;
  public Button btnConnect;
  public Image stConnection;

  public Text txtTick;

  [Header("logs list")]
  public Text txtRaw;
  public Text txtLogs;

  NwkUiLogs raws;
  NwkUiLogs logs;

  protected override void build()
  {
    base.build();
    txtLabel.text = "~Type~";
    txtClients.text = "";

    raws = new NwkUiLogs(txtRaw);
    logs = new NwkUiLogs(txtLogs);

    btnConnect.gameObject.SetActive(false);
  }

  protected override void setup()
  {
    base.setup();

    setConnected(false);
  }

  public override string getTabLabel() => "logs";

  public void setLabel(string newLabel)
  {
    txtLabel.text = newLabel;
  }

  bool canUpdate()
  {
    if (NwkSystemBase.nwkSys == null) return false;
    return true;
  }


  void Update()
  {
    //can update if nwksys is present
    bool allowUpdate = canUpdate();

    //kill visual if can't update
    float targetAlpha = allowUpdate ? 1f : 0f;
    if (groupData.alpha != targetAlpha) groupData.alpha = targetAlpha;

    if (!allowUpdate) return;

    bool _server = NwkSystemBase.isServer();

    List<NwkClientData> datas = NwkSystemBase.nwkSys.clientDatas;

    string ct = "clients x" + datas.Count;

    for (int i = 0; i < datas.Count; i++)
    {
      //datas[i].update(); // update size timer

      //STATE
      ct += "\n #" + datas[i].nwkUid + "\t(" + datas[i].state + ")";

      if (datas[i].isDisconnected()) continue;

      //PING

      // ping display (client = ping ; server = timeout)
      ct += "\n ping " + datas[i].getPingDelta();

      //SIZE

      if (_server) ct += "\n size " + datas[i].sizeSeconds;
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

    //Debug.Log("<color=orange>nwk</color> "+ct);
  }

  public void setConnected(bool connected)
  {
    stConnection.color = connected ? Color.green : Color.red;

    if (!btnConnect.gameObject.activeSelf) btnConnect.gameObject.SetActive(true);

    Text label = btnConnect.GetComponentInChildren<Text>();
    if (label != null)
    {
      if (NwkSystemBase.isServer()) label.text = connected ? "Shutdown" : "Boot";
      else label.text = connected ? "Disconnect" : "Connect";
    }

    show();
  }

  public void onConnectButtonPressed()
  {
    if (NwkSystemBase.isServer())
    {
      NwkServer.nwkServer.disconnect();
    }

    if (!NwkClient.isClient())
    {
      Debug.LogWarning("not on client ? can't react to that button");
      return;
    }

    bool clientConnection = NwkClient.nwkClient.isConnected();
    NwkClient.nwkClient.log("clicked : " + btnConnect.GetComponentInChildren<Text>().text + " / is connected ? " + clientConnection);

    if (clientConnection)
    {
      NwkClient.nwkClient.disconnect();
    }
    else
    {
      NwkClient.nwkClient.connectToIpPort(); // ui view button connect
    }
  }

  public void setupBoot(string label, bool connected)
  {
    setLabel(label);
    setConnected(true);
  }

}
