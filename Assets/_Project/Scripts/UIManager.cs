using UnityEngine;
using UnityEngine.UI;
using static ScreenAnyWarning;

public class UIManager : MonoBehaviour
{
    #region Variables

    private static UIManager m_Instance;
    public static UIManager Instance
    {
        get
        {
            if(m_Instance == null)
            {
                m_Instance = FindObjectOfType(typeof(UIManager)) as UIManager;
            }

            return m_Instance;
        }
    }

    [Header("Inputfields")]
    public InputField inputPassword;
    public InputField inputUpdatePasswordUser;

    [Header("Screens")]
    [SerializeField] private Transform canvasMain;
    [SerializeField] private GameObject screenAdm;
    [SerializeField] private GameObject screenPlayer;
    [SerializeField] private GameObject screenWarning;
    [SerializeField] private GameObject screenChooseUser;
    [SerializeField] private GameObject screenTakePhoto;
    [SerializeField] private GameObject screenFindTip;
    [SerializeField] private GameObject screenAward;
    [SerializeField] private GameObject panelTime;

    [Header("Popups")]
    [SerializeField] private GameObject popupScreenChooseUser;

    [Header("Texts")]
    [SerializeField] private Text txtTip;
    [SerializeField] private Text txtCountFindTips;
    [SerializeField] private Text txtTime;

    [Header("Buttons")]
    [SerializeField] private GameObject btnTipGameplay;

    [Header("Others")]
    public Transform contentScrollViewInputs;

    #endregion

    #region Methods MonoBehavipour

    private void Start()
    {

    }

    #endregion

    #region Methods 

    public Transform GetCanvasMain()
    {
        return canvasMain;
    }

    public void ActiveOrDisableScreenAdm(bool active)
    {
        screenAdm.SetActive(active);
    }

    public void ActiveOrDisableScreenPlayer(bool active)
    {
        screenPlayer.SetActive(active);
    }

    public void ActiveOrDisableScreenChooseUser(bool active)
    {
        screenChooseUser.SetActive(active);
    }

    public void ActiveOrDisableScreenTakePhoto(bool active)
    {
        screenTakePhoto.SetActive(active);
    }

    /// <summary>
    /// Ativa tela de avisos. Tempo obrigatorio somente se parametro destroyAutomatic é verdadeiro.
    /// </summary>
    /// <param name="typeWarning"></param>
    /// <param name="textBodyWarning"></param>
    /// <param name="destroyAutomatic"></param>
    /// <param name="timeWarning"></param>
    public void ActiveScreenWarning(TypeWarning typeWarning, string textBodyWarning, bool destroyAutomatic, float timeWarning = 0)
    {
        var goWarning = Instantiate(screenWarning, canvasMain);
        goWarning.SetActive(false);
        goWarning.GetComponent<ScreenAnyWarning>().SetupWarning(typeWarning, textBodyWarning, destroyAutomatic, timeWarning);
        goWarning.SetActive(true);
    }

    public void DestroyManualScreenWarning()
    {
        var screenWarning = FindObjectOfType(typeof(ScreenAnyWarning)) as ScreenAnyWarning;

        if (screenWarning != null)
            Destroy(screenWarning.gameObject);
    }

    public void ActiveFindTip(string textTip)
    {
        txtTip.text = textTip;
        screenFindTip.SetActive(true);
    }

    public void UpdateCountFindTips(int countFindTips, int countTipsMax)
    {
        txtCountFindTips.text = string.Format("{0}/{1}", countFindTips, countTipsMax);
    }

    public void ActiveAward()
    {
        screenAward.SetActive(true);
    }

    public void SetTextTime(string value)
    {
        txtTime.text = value;
    }

    public void DisablePanelTime()
    {
        panelTime.SetActive(false);
    }

    public void ActivePanelTime()
    {
        panelTime.SetActive(true);
    }

    public void ActiveButtonTip(bool active)
    {
        btnTipGameplay.SetActive(active);
    }

    #endregion
}
