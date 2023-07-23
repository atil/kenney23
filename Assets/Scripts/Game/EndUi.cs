using JamKit;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game
{
    public class EndUi : UiBase
    {
        [SerializeField] private Button _playButton;

        void Start()
        {
            Camera.backgroundColor = Globals.EndSceneCameraBackgroundColor;
            PlayerPrefs.SetInt("kenney.currentLevel", 0);

            Sfx.Instance.Play("kenney");

            FadeIn();
        }

        public void OnClickedPlayButton()
        {
            Sfx.Instance.Play("ButtonClick");

            _playButton.interactable = false;
            FadeOut(null, () => SceneManager.LoadScene("Game"));
        }
    }
}