using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// class qui permet de récup de l'info si je fais parti d'une hierarchie avec des iSyncable
/// </summary>

public interface INwkSyncListener
{

  /// <summary>
  /// when I'm created by a sync event
  /// tout les iNwk enfant vont bubble l'info
  /// </summary>
  void evtINwkScopeChange(iNwkPack sync, bool isLocal);

}
