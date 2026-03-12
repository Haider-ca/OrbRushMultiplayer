using UnityEngine;
using UnityEngine.UI;
using OrbRush.GameLogic;

namespace OrbRush.UI
{
    // Author: UI Team
    // Responsibility: Update score and status text
    public class HUDController : MonoBehaviour
    {
        public static HUDController Instance;

        public Text scoreText;
        public Text statusText;

        private string _fallbackScoreText = string.Empty;
        private string _fallbackStatusText = string.Empty;
        private GUIStyle _scoreStyle;
        private GUIStyle _statusStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _overlayStyle;
        private GUIStyle _modalStyle;
        private GUIStyle _modalTitleStyle;
        private GUIStyle _modalBodyStyle;

        private void Awake()
        {
            Instance = this;
        }

        public void SetScoreText(string text)
        {
            _fallbackScoreText = text ?? string.Empty;

            if (scoreText)
                scoreText.text = text;
        }

        public void SetStatusText(string text)
        {
            _fallbackStatusText = text ?? string.Empty;

            if (statusText)
                statusText.text = text;
        }

        private void OnGUI()
        {
            EnsureStyles();

            if ((!scoreText || !statusText) && !string.IsNullOrEmpty(_fallbackScoreText))
                GUI.Label(new Rect(12, 12, 320, 200), _fallbackScoreText, _scoreStyle);

            if (ScoreManager.Instance && ScoreManager.Instance.IsRoundEnded())
                DrawRoundEndModal();
        }

        private void EnsureStyles()
        {
            if (_scoreStyle == null)
            {
                _scoreStyle = new GUIStyle();
                _scoreStyle.alignment = TextAnchor.UpperLeft;
                _scoreStyle.fontSize = 16;
                _scoreStyle.normal.textColor = Color.white;
                _scoreStyle.normal.background = null;
                _scoreStyle.padding = new RectOffset(10, 10, 10, 10);
            }

            if (_statusStyle == null)
            {
                _statusStyle = new GUIStyle(GUI.skin.box);
                _statusStyle.alignment = TextAnchor.MiddleLeft;
                _statusStyle.fontSize = 24;
                _statusStyle.normal.textColor = Color.white;
                _statusStyle.padding = new RectOffset(18, 18, 12, 12);
            }

            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button);
                _buttonStyle.fontSize = 24;
            }

            if (_overlayStyle == null)
            {
                Texture2D overlayTexture = new Texture2D(1, 1);
                overlayTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.65f));
                overlayTexture.Apply();

                _overlayStyle = new GUIStyle();
                _overlayStyle.normal.background = overlayTexture;
            }

            if (_modalStyle == null)
            {
                Texture2D modalTexture = new Texture2D(1, 1);
                modalTexture.SetPixel(0, 0, new Color(0.12f, 0.09f, 0.05f, 0.96f));
                modalTexture.Apply();

                _modalStyle = new GUIStyle(GUI.skin.box);
                _modalStyle.normal.background = modalTexture;
                _modalStyle.padding = new RectOffset(28, 28, 24, 24);
            }

            if (_modalTitleStyle == null)
            {
                _modalTitleStyle = new GUIStyle(GUI.skin.label);
                _modalTitleStyle.fontSize = 34;
                _modalTitleStyle.fontStyle = FontStyle.Bold;
                _modalTitleStyle.alignment = TextAnchor.MiddleCenter;
                _modalTitleStyle.normal.textColor = Color.white;
            }

            if (_modalBodyStyle == null)
            {
                _modalBodyStyle = new GUIStyle(GUI.skin.label);
                _modalBodyStyle.fontSize = 24;
                _modalBodyStyle.alignment = TextAnchor.UpperLeft;
                _modalBodyStyle.normal.textColor = Color.white;
                _modalBodyStyle.wordWrap = true;
            }
        }

        private void DrawRoundEndModal()
        {
            float overlayWidth = Screen.width;
            float overlayHeight = Screen.height;
            GUI.Box(new Rect(0, 0, overlayWidth, overlayHeight), GUIContent.none, _overlayStyle);

            float modalWidth = 620f;
            float modalHeight = 360f;
            float modalX = (overlayWidth - modalWidth) * 0.5f;
            float modalY = (overlayHeight - modalHeight) * 0.5f;

            GUI.Box(new Rect(modalX, modalY, modalWidth, modalHeight), GUIContent.none, _modalStyle);
            GUI.Label(new Rect(modalX + 20f, modalY + 20f, modalWidth - 40f, 48f), "Round Over", _modalTitleStyle);

            string winnerText = ScoreManager.Instance.GetWinnerSummary();
            string scoreTextBody = ScoreManager.Instance.GetScoreboardSummary();
            string bodyText = winnerText + "\n\nFinal Scores\n" + scoreTextBody;
            GUI.Label(new Rect(modalX + 36f, modalY + 84f, modalWidth - 72f, 180f), bodyText, _modalBodyStyle);

            if (GUI.Button(new Rect(modalX + (modalWidth - 220f) * 0.5f, modalY + 286f, 220f, 52f), "Restart", _buttonStyle))
                ScoreManager.Instance.RestartRound();
        }
    }
}