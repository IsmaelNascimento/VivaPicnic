using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenAnyWarning : MonoBehaviour
{
    #region Variables

    //private static ScreenAnyWarning m_Instance;
    //public static ScreenAnyWarning Instance
    //{
    //    get
    //    {
    //        if(m_Instance == null)
    //        {
    //            m_Instance = FindObjectOfType(typeof(ScreenAnyWarning)) as ScreenAnyWarning;
    //        }

    //        return m_Instance;
    //    }
    //}

    public enum TypeWarning
    {
        Error = 0, 
        Warning = 1,
        Message = 2
    }

    private TypeWarning typeWarning;
    //public string txtTitleWarning;
    private string txtBodyWarning;
    private float timeWarning;
    private bool destroyAutomatic;

    [SerializeField] private Image bgMessage;
    [SerializeField] private Color colorBgWarningError;
    [SerializeField] private Color colorBgWarningWarning;
    [SerializeField] private Color colorBgWarningMessage;
    [SerializeField] private Text textTitleWarning;
    [SerializeField] private Text textBodyWarning;

    #endregion

    #region Methods MonoBehaviour

    private void OnEnable()
    {
        switch (typeWarning)
        {
            case TypeWarning.Error:
                bgMessage.color = colorBgWarningError;
                textTitleWarning.text = "Erro";
                break;
            case TypeWarning.Warning:
                bgMessage.color = colorBgWarningWarning;
                textTitleWarning.text = "Aviso";
                break;
            case TypeWarning.Message:
                bgMessage.color = colorBgWarningMessage;
                textTitleWarning.text = "Mensagem";
                break;
        }

        
        textBodyWarning.text = txtBodyWarning;

        if(destroyAutomatic)
            StartCoroutine(DisableWarning());
    }

    #endregion

    #region Methods Created 

    public void SetupWarning(TypeWarning typeWarning, string textBodyWarning, bool destroyAutomatic, float timeWarning = 0)
    {
        this.typeWarning = typeWarning;
        txtBodyWarning = textBodyWarning;
        this.timeWarning = timeWarning;
        this.destroyAutomatic = destroyAutomatic;
    }

    #endregion

    #region Coroutines

    private IEnumerator DisableWarning()
    {
        yield return new WaitForSeconds(timeWarning);
        Destroy(gameObject);
    }

    #endregion



}