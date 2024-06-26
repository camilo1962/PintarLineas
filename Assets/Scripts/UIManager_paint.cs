﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace LinePaint
{
    public class UIManager_paint : MonoBehaviour
    {
        [SerializeField] private Text totalDiamonds, diamondsEarned, levelText;
        [SerializeField] private GameObject mainMenu, levelCompleteMenu, extraBtnHolder, sountBtnOff;
        [SerializeField] private Button settingsBtn, nextButton, soundBtn, vbrationBtn, retryBtn;

        public Text LevelText { get => levelText; }
        public Text TotalDiamonds { get => totalDiamonds; }
        public GameObject panel_loading;

        private void Start()
        {
            sountBtnOff.SetActive(PlayerPrefs.GetInt("SoundOn", 1) == 0 ? true : false);
            AudioListener.volume = PlayerPrefs.GetInt("SoundOn", 1);

            settingsBtn.onClick.AddListener(() => OnClick(settingsBtn));
            nextButton.onClick.AddListener(() => OnClick(nextButton));
            soundBtn.onClick.AddListener(() => OnClick(soundBtn));
            retryBtn.onClick.AddListener(() => OnClick(retryBtn));
        }

        private void OnClick(Button btn)
        {
            SoundManager_paint.Instance.PlayFx(FxType.Button);
            switch (btn.name)
            {
                case "SettingsBtn":
                    extraBtnHolder.SetActive(!extraBtnHolder.activeInHierarchy);
                    break;
                case "NextBtn":
                case "RetryBtn":
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    break;
                case "SoundBtn":
                    PlayerPrefs.SetInt("SoundOn", PlayerPrefs.GetInt("SoundOn", 1) == 0 ? 1 : 0);
                    sountBtnOff.SetActive(PlayerPrefs.GetInt("SoundOn", 1) == 0 ? true : false);
                    AudioListener.volume = PlayerPrefs.GetInt("SoundOn", 1);
                    break;
            }
        }

        public void LevelCompleted()
        {
            mainMenu.SetActive(false);
            levelCompleteMenu.SetActive(true);
            totalDiamonds.text = "" + GameManager_paint.totalDiamonds;
           // AdManager.instance.show_ads_ingames();
        }
        public void click_menu()
        {
            panel_loading.SetActive(true); 
            SceneManager.LoadSceneAsync(0 , LoadSceneMode.Single); 
        }

    }
}