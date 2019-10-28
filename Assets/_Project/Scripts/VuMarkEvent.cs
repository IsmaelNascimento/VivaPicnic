using System.Collections;
using UnityEngine;
using Vuforia;

public class VuMarkEvent : MonoBehaviour
{
    #region PRIVATE_MEMBER_VARIABLES

    VuMarkManager m_VuMarkManager;
    VuMarkTarget m_CurrentVuMark;

    #endregion // PRIVATE_MEMBER_VARIABLES


    #region UNITY_MONOBEHAVIOUR_METHODS

    void Start()
    {
        // register callbacks to VuMark Manager
        m_VuMarkManager = TrackerManager.Instance.GetStateManager().GetVuMarkManager();
        m_VuMarkManager.RegisterVuMarkDetectedCallback(OnVuMarkDetected);
        m_VuMarkManager.RegisterVuMarkLostCallback(OnVuMarkLost);
    }

    void OnDestroy()
    {
        // unregister callbacks from VuMark Manager
        m_VuMarkManager.UnregisterVuMarkDetectedCallback(OnVuMarkDetected);
        m_VuMarkManager.UnregisterVuMarkLostCallback(OnVuMarkLost);
    }

    #endregion // UNITY_MONOBEHAVIOUR_METHODS

    #region PUBLIC_METHODS

    /// <summary>
    /// This method will be called whenever a new VuMark is detected
    /// </summary>
    public void OnVuMarkDetected(VuMarkTarget target)
    {
        var idVumark = int.Parse(GetVuMarkId(target));
        GameManager.Instance.OnFindVumark(idVumark);
        m_CurrentVuMark = target;
        Debug.Log("New VuMark with ID:: " + GetVuMarkId(target));
    }

    /// <summary>
    /// This method will be called whenever a tracked VuMark is lost
    /// </summary>
    public void OnVuMarkLost(VuMarkTarget target)
    {
        m_CurrentVuMark = null;
        GameManager.Instance.OnLostVumark();
        Debug.Log("Lost VuMark with ID:: " + GetVuMarkId(target));
    }

    #endregion // PUBLIC_METHODS

    #region PRIVATE_METHODS

    string GetVuMarkId(VuMarkTarget vumark)
    {
        switch (vumark.InstanceId.DataType)
        {
            case InstanceIdType.BYTES:
                return vumark.InstanceId.HexStringValue;
            case InstanceIdType.STRING:
                return vumark.InstanceId.StringValue;
            case InstanceIdType.NUMERIC:
                return vumark.InstanceId.NumericValue.ToString();
        }

        return string.Empty;
    }

    #endregion // PRIVATE_METHODS
}
