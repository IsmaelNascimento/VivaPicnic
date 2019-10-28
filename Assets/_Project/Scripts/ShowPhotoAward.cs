using UnityEngine;
using UnityEngine.UI;

public class ShowPhotoAward : MonoBehaviour
{
    private void OnEnable()
    {
        GameManager.Instance.ShowPhotoAward(GetComponent<Image>());
    }
}