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
        private GUIStyle _statusPanelStyle;
        private GUIStyle _announcementStyle;
        private string _announcementText = string.Empty;
        private float _announcementUntil;

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

        public void ShowAnnouncement(string text, float durationSeconds)
        {
            _announcementText = text ?? string.Empty;
            _announcementUntil = Time.time + Mathf.Max(0.1f, durationSeconds);
        }

        private void OnGUI()
        {
            EnsureStyles();

            if ((!scoreText || !statusText) && !string.IsNullOrEmpty(_fallbackScoreText))
            {
                GUI.Box(new Rect(18, 18, 300, 134), GUIContent.none, _statusPanelStyle);
                GUI.Label(new Rect(30, 28, 272, 120), _fallbackScoreText, _scoreStyle);
            }

            if ((!scoreText || !statusText) && !string.IsNullOrEmpty(_fallbackStatusText))
                GUI.Label(new Rect(24, Screen.height - 76, 460, 48), _fallbackStatusText, _statusStyle);

            if (Time.time < _announcementUntil && !string.IsNullOrEmpty(_announcementText))
                GUI.Label(new Rect(Screen.width * 0.5f - 220f, 96f, 440f, 48f), _announcementText, _announcementStyle);

            if (ScoreManager.Instance && ScoreManager.Instance.IsRoundEnded())
                DrawRoundEndModal();
        }

        private void EnsureStyles()
        {
            if (_scoreStyle == null)
            {
                _scoreStyle = new GUIStyle();
                _scoreStyle.alignment = TextAnchor.UpperLeft;
                _scoreStyle.fontSize = 18;
                _scoreStyle.normal.textColor = new Color(0.98f, 1f, 0.94f);
                _scoreStyle.normal.background = null;
                _scoreStyle.padding = new RectOffset(8, 8, 8, 8);
            }

            if (_statusStyle == null)
            {
                _statusStyle = new GUIStyle(GUI.skin.label);
                _statusStyle.alignment = TextAnchor.MiddleLeft;
                _statusStyle.fontSize = 22;
                _statusStyle.fontStyle = FontStyle.Bold;
                _statusStyle.normal.textColor = new Color(1f, 0.92f, 0.56f);
                _statusStyle.padding = new RectOffset(14, 14, 10, 10);
            }

            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button);
                _buttonStyle.fontSize = 24;
                _buttonStyle.fontStyle = FontStyle.Bold;
            }

            if (_announcementStyle == null)
            {
                _announcementStyle = new GUIStyle(GUI.skin.box);
                _announcementStyle.alignment = TextAnchor.MiddleCenter;
                _announcementStyle.fontSize = 22;
                _announcementStyle.fontStyle = FontStyle.Bold;
                _announcementStyle.normal.textColor = new Color(0.96f, 1f, 0.94f);

                Texture2D announcementTexture = new Texture2D(1, 1);
                announcementTexture.SetPixel(0, 0, new Color(0.16f, 0.34f, 0.24f, 0.9f));
                announcementTexture.Apply();
                _announcementStyle.normal.background = announcementTexture;
                _announcementStyle.padding = new RectOffset(18, 18, 10, 10);
            }

            if (_overlayStyle == null)
            {
                Texture2D overlayTexture = new Texture2D(1, 1);
                overlayTexture.SetPixel(0, 0, new Color(0.04f, 0.08f, 0.05f, 0.48f));
                overlayTexture.Apply();

                _overlayStyle = new GUIStyle();
                _overlayStyle.normal.background = overlayTexture;
            }

            if (_statusPanelStyle == null)
            {
                Texture2D statusPanelTexture = new Texture2D(1, 1);
                statusPanelTexture.SetPixel(0, 0, new Color(0.2f, 0.34f, 0.24f, 0.78f));
                statusPanelTexture.Apply();

                _statusPanelStyle = new GUIStyle(GUI.skin.box);
                _statusPanelStyle.normal.background = statusPanelTexture;
                _statusPanelStyle.border = new RectOffset(8, 8, 8, 8);
            }

            if (_modalStyle == null)
            {
                Texture2D modalTexture = new Texture2D(1, 1);
                modalTexture.SetPixel(0, 0, new Color(0.22f, 0.32f, 0.24f, 0.95f));
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
                _modalTitleStyle.normal.textColor = new Color(1f, 0.97f, 0.8f);
            }

            if (_modalBodyStyle == null)
            {
                _modalBodyStyle = new GUIStyle(GUI.skin.label);
                _modalBodyStyle.fontSize = 24;
                _modalBodyStyle.alignment = TextAnchor.UpperLeft;
                _modalBodyStyle.normal.textColor = new Color(0.96f, 1f, 0.95f);
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
