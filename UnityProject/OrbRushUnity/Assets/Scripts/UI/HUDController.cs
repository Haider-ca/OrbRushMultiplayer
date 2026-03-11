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

		private string fallbackScoreText = string.Empty;
		private string fallbackStatusText = string.Empty;
		private GUIStyle scoreStyle;
		private GUIStyle statusStyle;
		private GUIStyle buttonStyle;
		private GUIStyle overlayStyle;
		private GUIStyle modalStyle;
		private GUIStyle modalTitleStyle;
		private GUIStyle modalBodyStyle;

		private void Awake()
		{
			Instance = this;
		}

		public void SetScoreText(string text)
		{
			fallbackScoreText = text ?? string.Empty;

			if (scoreText != null)
				scoreText.text = text;
		}

		public void SetStatusText(string text)
		{
			fallbackStatusText = text ?? string.Empty;

			if (statusText != null)
				statusText.text = text;
		}

		private void OnGUI()
		{
			EnsureStyles();

			if ((scoreText == null || statusText == null) && !string.IsNullOrEmpty(fallbackScoreText))
				GUI.Label(new Rect(24, 24, 520, 300), fallbackScoreText, scoreStyle);

			if (ScoreManager.Instance != null && ScoreManager.Instance.IsRoundEnded())
				DrawRoundEndModal();
		}

		private void EnsureStyles()
		{
			if (scoreStyle == null)
			{
				scoreStyle = new GUIStyle(GUI.skin.box);
				scoreStyle.alignment = TextAnchor.UpperLeft;
				scoreStyle.fontSize = 30;
				scoreStyle.normal.textColor = Color.white;
				scoreStyle.padding = new RectOffset(18, 18, 18, 18);
			}

			if (statusStyle == null)
			{
				statusStyle = new GUIStyle(GUI.skin.box);
				statusStyle.alignment = TextAnchor.MiddleLeft;
				statusStyle.fontSize = 24;
				statusStyle.normal.textColor = Color.white;
				statusStyle.padding = new RectOffset(18, 18, 12, 12);
			}

			if (buttonStyle == null)
			{
				buttonStyle = new GUIStyle(GUI.skin.button);
				buttonStyle.fontSize = 24;
			}

			if (overlayStyle == null)
			{
				Texture2D overlayTexture = new Texture2D(1, 1);
				overlayTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.65f));
				overlayTexture.Apply();

				overlayStyle = new GUIStyle();
				overlayStyle.normal.background = overlayTexture;
			}

			if (modalStyle == null)
			{
				Texture2D modalTexture = new Texture2D(1, 1);
				modalTexture.SetPixel(0, 0, new Color(0.12f, 0.09f, 0.05f, 0.96f));
				modalTexture.Apply();

				modalStyle = new GUIStyle(GUI.skin.box);
				modalStyle.normal.background = modalTexture;
				modalStyle.padding = new RectOffset(28, 28, 24, 24);
			}

			if (modalTitleStyle == null)
			{
				modalTitleStyle = new GUIStyle(GUI.skin.label);
				modalTitleStyle.fontSize = 34;
				modalTitleStyle.fontStyle = FontStyle.Bold;
				modalTitleStyle.alignment = TextAnchor.MiddleCenter;
				modalTitleStyle.normal.textColor = Color.white;
			}

			if (modalBodyStyle == null)
			{
				modalBodyStyle = new GUIStyle(GUI.skin.label);
				modalBodyStyle.fontSize = 24;
				modalBodyStyle.alignment = TextAnchor.UpperLeft;
				modalBodyStyle.normal.textColor = Color.white;
				modalBodyStyle.wordWrap = true;
			}
		}

		private void DrawRoundEndModal()
		{
			float overlayWidth = Screen.width;
			float overlayHeight = Screen.height;
			GUI.Box(new Rect(0, 0, overlayWidth, overlayHeight), GUIContent.none, overlayStyle);

			float modalWidth = 620f;
			float modalHeight = 360f;
			float modalX = (overlayWidth - modalWidth) * 0.5f;
			float modalY = (overlayHeight - modalHeight) * 0.5f;

			GUI.Box(new Rect(modalX, modalY, modalWidth, modalHeight), GUIContent.none, modalStyle);
			GUI.Label(new Rect(modalX + 20f, modalY + 20f, modalWidth - 40f, 48f), "Round Over", modalTitleStyle);

			string winnerText = ScoreManager.Instance.GetWinnerSummary();
			string scoreTextBody = ScoreManager.Instance.GetScoreboardSummary();
			string bodyText = winnerText + "\n\nFinal Scores\n" + scoreTextBody;
			GUI.Label(new Rect(modalX + 36f, modalY + 84f, modalWidth - 72f, 180f), bodyText, modalBodyStyle);

			if (GUI.Button(new Rect(modalX + (modalWidth - 220f) * 0.5f, modalY + 286f, 220f, 52f), "Restart", buttonStyle))
				ScoreManager.Instance.RestartRound();
		}
	}
}
