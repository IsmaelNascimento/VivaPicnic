using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Vuforia;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Variables 

    private static GameManager m_Instance;
    public static GameManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindObjectOfType(typeof(GameManager)) as GameManager;
            }

            return m_Instance;
        }
    }

    // Variables Controller App
    public bool IsAdm { get; set; }
    public int CountVumarksMax { get; set; }
    public const int countTipsMax = 8;
    private int countFindVumarks = 0;
    private List<InputField> inputFieldTips = new List<InputField>();
    [SerializeField] private Transform prefabInputFieldTip;
    private List<string> tipsGame = new List<string>();
    [HideInInspector] public bool startGameplay;
    private float timerPlaying = 0;
    private string minuteFinal;
    private string secondFinal;

    // Variables server
    private const string urlLicenseAdm = "https://vivapicnic.blob.core.windows.net/viva-picnic/licenseAdm.txt";
    private const string urlPasswordPlayer = "https://vivapicnic.blob.core.windows.net/viva-picnic/passwordPlayer.txt";
    private const string urlPhotoAward = "https://vivapicnic.blob.core.windows.net/viva-picnic/photoReward.png";
    [HideInInspector] public List<string> urlTips = new List<string>();

    // Variables VuMarks
    private bool findVumark = false;
    private List<bool> findVumarks = new List<bool>();

    #endregion

    #region Methods MonoBehaviour

    private void Start()
    {
        countFindVumarks = 0;
        CountVumarksMax = countTipsMax;
        SyncTipsPlayer();
        ActiveCameraNow(false);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void Update()
    {
        if (startGameplay)
        {
            timerPlaying += Time.deltaTime;
            minuteFinal = Mathf.Floor(timerPlaying / 60).ToString("00");
            secondFinal = (timerPlaying % 60).ToString("00");
            UIManager.Instance.SetTextTime(string.Format("Tempo de jogo {0}:{1}", minuteFinal, secondFinal));
        }
    }

    #endregion

    #region Methods Created for General

    private void SyncTipsPlayer()
    {
        urlTips.Clear();

        // Add url tips
        for (int i = 1; i <= CountVumarksMax; i++)
        {
            var urlTip = string.Format("https://vivapicnic.blob.core.windows.net/viva-picnic/ContentsText/tip{0}.txt", i);
            urlTips.Add(urlTip);
        }

        LoadTipsServer(urlTips);
    }

    private string GetStreamingAssetsPath()
    {
        var filePath = Application.persistentDataPath + "/";
        //print("filePath:: " + filePath);
        return filePath;
    }

    private async void UploadPhotoSaveAsync()
    {
        await BlobStorageManager.Instance.UploadOrUpdateBlockBlobAsync("viva-picnic", GetStreamingAssetsPath(), "photoReward.png", () =>
        {
            print("Upload photo...");
            UIManager.Instance.ActiveScreenWarning(ScreenAnyWarning.TypeWarning.Message, "Enviando foto...", false);
        },
        () =>
        {
            print("Upload photo!");
            UIManager.Instance.DestroyManualScreenWarning();
            UIManager.Instance.ActiveScreenWarning(ScreenAnyWarning.TypeWarning.Message, "Foto enviada com sucesso!", true, 2f);
            UIManager.Instance.ActiveOrDisableScreenTakePhoto(false);
            UIManager.Instance.ActiveOrDisableScreenAdm(true);

        },
        (e) =>
        {
            UIManager.Instance.DestroyManualScreenWarning();
            UIManager.Instance.ActiveScreenWarning(ScreenAnyWarning.TypeWarning.Error, "Erro de internet", true, 2f);
            UIManager.Instance.ActiveOrDisableScreenTakePhoto(false);
            UIManager.Instance.ActiveOrDisableScreenAdm(true);
            // Logs
            print(GetStreamingAssetsPath() + "photoReward.png");
            print(e);
        });
    }

    public void ShowPhotoAward(UnityEngine.UI.Image image)
    {
        StartCoroutine(ShowPhotoAward_Coroutine(image));
    }

    private void RemoveInputFieldsTips()
    {
        foreach (var input in inputFieldTips)
        {
            //inputFieldTips.Remove(input);
            Destroy(input.gameObject);
        }

        inputFieldTips.Clear();
    }

    public void LoadTipsServer(List<string> urlTips)
    {
        StartCoroutine(LoadTip_Coroutine(urlTips));
    }

    private void StartGameplay()
    {
        ActiveCameraNow(true);
        countFindVumarks = 0;
        UIManager.Instance.ActiveOrDisableScreenPlayer(true);
        startGameplay = true;
        timerPlaying = 0;
        UIManager.Instance.ActivePanelTime();
        UIManager.Instance.UpdateCountFindTips(0, CountVumarksMax);
        UIManager.Instance.ActiveButtonTip(false);

        for (int i = 0; i < findVumarks.Count; i++)
            findVumarks[i] = false;
    }

    public void ActiveCameraNow(bool activeNow)
    {
        VuforiaBehaviour.Instance.gameObject.SetActive(activeNow);
    }

    private void SetupCountVumarksMax()
    {
        var countTipsAux = 0;

        foreach (var tip in tipsGame)
        {
            if (!string.IsNullOrEmpty(tip))
                countTipsAux++;
        }

        for (int i = 0; i < (countTipsAux+1); i++)
            findVumarks.Add(false);

        CountVumarksMax = (countTipsAux + 1);
        UIManager.Instance.UpdateCountFindTips(0, CountVumarksMax);

        print("CountVumarksMax:: " + CountVumarksMax);
    }

    #endregion

    #region Methods for VuMarks

    public void OnFindVumark(int idVuMark)
    {
        ActiveCameraNow(false);
        findVumark = true;
        UIManager.Instance.ActiveButtonTip(true);

        if (!IsLastVumark())
        {
            findVumarks[idVuMark - 1] = true;
            countFindVumarks++;
            UIManager.Instance.UpdateCountFindTips(countFindVumarks, CountVumarksMax);
        }

        if (IsLastVumark())
        {
            findVumarks[CountVumarksMax - 1] = true;
            UIManager.Instance.ActiveAward();
            startGameplay = false;
            UIManager.Instance.DisablePanelTime();
            print(string.Format("Time final = {0}:{1}", minuteFinal, secondFinal));
            return;
        }

        ShowFindTip(idVuMark);
    }

    private bool IsLastVumark()
    {
        return findVumarks[CountVumarksMax - 1];
    }

    public void OnLostVumark()
    {
        findVumark = false;
    }

    public void ShowFindTip(int idTip)
    {
        var indexTip = idTip - 1;
        var tip = string.Format("Dica Nº {0}:\n{1}", idTip, tipsGame[indexTip]);
        UIManager.Instance.ActiveFindTip(tip);
    }

    #endregion

    #region Methods Created for UIs

    public void OnButtonProfileSelectedClicked(bool isAdm)
    {
        IsAdm = isAdm;
    }

    public void OnButtonVerifyPasswordProfileClicked()
    {
        StartCoroutine(VerifyPasswordProfile_Coroutine());
    }

    public void OnButtonTakePhotoClicked()
    {
        VuforiaRenderer.Instance.Pause(true);
    }

    public void OnButtonNewTakePhotoClicked()
    {
        ActiveCameraNow(true);
    }

    public void OnButtonSavePhotoClicked()
    {
        StartCoroutine(SavePhoto_Coroutine());
        ActiveCameraNow(false);
    }    

    public async void OnButtonUpdatePasswordPlayerClicked()
    {
        await BlobStorageManager.Instance.UpdateFileTxtBlockBlobAsync("viva-picnic", GetStreamingAssetsPath(), "", "passwordPlayer.txt", UIManager.Instance.inputUpdatePasswordUser.text, () =>
        {
            UIManager.Instance.ActiveScreenWarning(ScreenAnyWarning.TypeWarning.Message, "Atualizando...", false);
        },
        () =>
        {
            UIManager.Instance.DestroyManualScreenWarning();
            UIManager.Instance.ActiveScreenWarning(ScreenAnyWarning.TypeWarning.Message, "Senha atualizada!", true, 2f);
        },
        (e) =>
        {
            UIManager.Instance.ActiveScreenWarning(ScreenAnyWarning.TypeWarning.Error, "Erro de internet", true, 2f);
            print(e);
        });
    }

    public void OnButtonAddTipsClicked()
    {
        for (int i = 0; i < countTipsMax; i++)
        {
            var inputField = Instantiate(prefabInputFieldTip, UIManager.Instance.contentScrollViewInputs);
            inputField.GetComponent<InputField>().text = tipsGame[i];
            //inputField.GetComponentInChildren<Text>().text = "Digite a dica " + (i + 1);
            inputFieldTips.Add(inputField.GetComponent<InputField>());
        }
    }

    public async void OnButtonSaveTipsClicked()
    {
        for (int i = 0; i < inputFieldTips.Count; i++)
        {
            var nameFile = string.Format("tip{0}.txt", (i + 1));
            //print(string.Format("Name FIle: {0} and Content: {1}", nameFile, inputFieldTips[i]));

            await BlobStorageManager.Instance.UpdateFileTxtBlockBlobAsync("viva-picnic", GetStreamingAssetsPath(), "ContentsText/", nameFile, inputFieldTips[i].text, () =>
            {
                UIManager.Instance.ActiveScreenWarning(ScreenAnyWarning.TypeWarning.Message, "Adicionando dica...", false);
            },
            () =>
            {
                UIManager.Instance.DestroyManualScreenWarning();
                UIManager.Instance.ActiveScreenWarning(ScreenAnyWarning.TypeWarning.Message, "Dica adicionada", true, 2f);
            },
            (e) =>
            {
                UIManager.Instance.ActiveScreenWarning(ScreenAnyWarning.TypeWarning.Error, "Erro com conexão", true, 2f);
                print(e);
            });
        }

        RemoveInputFieldsTips();
        SyncTipsPlayer();
    }

    public void OnButtonBackAddTipsClicked()
    {
        RemoveInputFieldsTips();
    }

    public void OnButtonFeedbackClicked()
    {
        string emailToReceiveFeedback = "vivapicnic@3e60.com.br";
        //string subject = MyEscapeURL("Feedback sobre AR Livre");
        string subject = "Feedback sobre AR Livre";
        string body = "";
        Application.OpenURL("mailto:" + emailToReceiveFeedback + "?subject=" + subject + "&body=" + body);
    }

    public void LoadScene(string nameScene)
    {
        UIManager.Instance.ActiveScreenWarning(ScreenAnyWarning.TypeWarning.Warning, "Carregado nova tela...", false);
        SceneManager.LoadScene(nameScene);
    }

    #endregion

    #region Coroutines

    private IEnumerator VerifyPasswordProfile_Coroutine()
    {
        var urlCurrent = IsAdm ? urlLicenseAdm : urlPasswordPlayer;

        UIManager.Instance.ActiveScreenWarning(ScreenAnyWarning.TypeWarning.Message, "Verificando...", false);

        using (UnityWebRequest www = UnityWebRequest.Get(urlCurrent))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                UIManager.Instance.ActiveScreenWarning(ScreenAnyWarning.TypeWarning.Error, "Erro com conexão", true, 2f);
                print(www.error);
            }
            else
            {
                UIManager.Instance.DestroyManualScreenWarning();

                if (www.downloadHandler.text == UIManager.Instance.inputPassword.text)
                {
                    print("Password Valid");
                    UIManager.Instance.ActiveOrDisableScreenChooseUser(false);

                    if (IsAdm)
                        UIManager.Instance.ActiveOrDisableScreenAdm(true);
                    else
                        StartGameplay();
                }
                else
                {
                    print("Password Invalid");
                    print("UIManager.Instance.inputPassword.text:: " + UIManager.Instance.inputPassword.text);
                    print("www.downloadHandler.text:: " + www.downloadHandler.text);
                    UIManager.Instance.ActiveScreenWarning(ScreenAnyWarning.TypeWarning.Error, "Senha invalida", true, 2f);
                }
            }
        }
    }

    private IEnumerator SavePhoto_Coroutine()
    {
        yield return new WaitForEndOfFrame();

        string fileName = "photoReward.png";
        var pathScreenshot = GetStreamingAssetsPath() + fileName;

        if (File.Exists(pathScreenshot))
            File.Delete(pathScreenshot);

#if UNITY_ANDROID || UNITY_IOS 
        ScreenCapture.CaptureScreenshot(fileName);
#endif

#if UNITY_EDITOR
        ScreenCapture.CaptureScreenshot(pathScreenshot);
#endif

        yield return new WaitForEndOfFrame();
        ActiveCameraNow(false);
        print("Wait File.Exists(pathScreenshot) for TRUE");
        yield return new WaitUntil(() => File.Exists(pathScreenshot));
        print("File.Exists(pathScreenshot) is TRUE");
        UploadPhotoSaveAsync();
    }

    private IEnumerator ShowPhotoAward_Coroutine(UnityEngine.UI.Image image)
    {
        UIManager.Instance.ActiveScreenWarning(ScreenAnyWarning.TypeWarning.Message, "Carregando foto do prêmio...", false);

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(urlPhotoAward))
        {
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                UIManager.Instance.ActiveScreenWarning(ScreenAnyWarning.TypeWarning.Error, "Erro de internet...", true, 2f);
                Debug.Log(request.error);
            }
            else
            {
                UIManager.Instance.DestroyManualScreenWarning();
                var texture2D = DownloadHandlerTexture.GetContent(request);
                var newSprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(.5f, .5f));
                image.sprite = newSprite;
            }
        }
    }

    private IEnumerator LoadTip_Coroutine(List<string> urlTips)
    {
        var countTip = urlTips.Count;
        print("Carregando dicas...");

        for (int i = 0; i < countTip; i++)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(urlTips[i]))
            {
                yield return request.SendWebRequest();

                if (request.isNetworkError || request.isHttpError)
                {
                    UIManager.Instance.ActiveScreenWarning(ScreenAnyWarning.TypeWarning.Error, "Houve problemas ao carregar dicas.\nReinicie o aplicativo.", false);
                    print(request.error);
                    print(string.Format("Houve problemas ao recarregar dicas.\nReinicie o aplicativo."));
                }
                else
                {
                    print(string.Format("Dica N{0}: {1}", (i+1), request.downloadHandler.text));
                    tipsGame.Add(request.downloadHandler.text);
                }
            }
        }

        SetupCountVumarksMax();
    }

    #endregion
}