using UnityEngine;

abstract public class NwkUiView : MonoBehaviour
{
  Canvas _canvas;
  public CanvasGroup groupData;

  private void Awake()
  {
    build();
  }

  private void Start()
  {
    setup();
  }

  virtual protected void build()
  {
    _canvas = GetComponent<Canvas>();
    hide();
  }

  virtual protected void setup()
  {
    NwkUiTabs tabs = GameObject.FindObjectOfType<NwkUiTabs>();
    NwkUiView[] views = GameObject.FindObjectsOfType<NwkUiView>();
    if(views.Length>0 && tabs == null)
    {
      NwkUiTabs.loadView("tabs");
    }
    else
    {
      tabs.refreshTabs();
    }
    
  }

  private void OnDestroy()
  {
    destroy();
  }

  virtual protected void destroy()
  {
    gameObject.SetActive(false);

    if(Application.isPlaying)
    {
      //force refresh tabs
      NwkUiTabs views = GameObject.FindObjectOfType<NwkUiTabs>();
      if (views != null) views.refreshTabs();
    }

  }

  public void toggleVisible()
  {
    if (_canvas.enabled) hide();
    else show();

    //Debug.Log(name+" => "+_canvas.enabled);
  }

  public void show()
  {
    _canvas.enabled = true;
  }

  public void hide()
  {
    _canvas.enabled = false;
  }

  virtual public string getTabLabel() => GetType().ToString();
}
