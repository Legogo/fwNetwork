using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class NwkUiLogs
{
  Text txtField;
  List<string> logs = new List<string>();
  StringBuilder sbuilder;

  int countMax = 50;

  public NwkUiLogs(Text field, string defaultContent = "")
  {
    sbuilder = new StringBuilder();

    txtField = field;
    txtField.text = defaultContent;

    logs.Clear();
  }

  public void addLog(string ct)
  {
    //line
    string header = Time.frameCount + " | ";
    string line = header + ct;
    logs.Add(line);

    //build
    sbuilder.Clear();

    int count = Mathf.Min(countMax, logs.Count);

    sbuilder.Append(count);

    for (int i = logs.Count - 1; i >= logs.Count - count; i--)
    {
      sbuilder.Append("\n" + logs[i]);
    }

    //display
    txtField.text = sbuilder.ToString();

    //Debug.Log(txtField.text);
  }

}
