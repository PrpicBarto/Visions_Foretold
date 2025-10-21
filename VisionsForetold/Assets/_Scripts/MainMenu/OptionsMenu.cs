using System;
using UnityEngine;
using UnityEngine.UI;
public class OptionsMenu : MonoBehaviour
{
    public Slider volumeSlider;

    private void Start()
    {
        if (PlayerPrefs.HasKey("Volume"))
        {
            volumeSlider.value = PlayerPrefs.GetFloat("Volume");
        }
        else
        {
            PlayerPrefs.SetFloat("Volume", 0.75f);
        }
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    private void SetVolume(float value)
    {
        if (AudioManager.instance == null)
        {
            AudioManager.instance.SetVolume(value);
        }
    }

    public void CloseOptionsMenu()
    {
        gameObject.SetActive(false);
    }
}
