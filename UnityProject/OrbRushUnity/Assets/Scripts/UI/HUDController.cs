using UnityEngine;
using UnityEngine.UI;

namespace OrbRush.UI
{
	// Author: UI Team
	// Responsibility: Update score and status text
	public class HUDController : MonoBehaviour
	{
		public static HUDController Instance;

		public Text scoreText;
		public Text statusText;

		private void Awake()
		{
			Instance = this;
		}

		public void SetScoreText(string text)
		{
			if (scoreText != null)
				scoreText.text = text;
		}

		public void SetStatusText(string text)
		{
			if (statusText != null)
				statusText.text = text;
		}
	}
}