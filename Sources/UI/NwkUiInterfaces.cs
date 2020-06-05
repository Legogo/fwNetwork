using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface iNwkUiTab
{
  string getTabLabel();
  void showTab();
  void hideTab();
  void toggleTab();
}