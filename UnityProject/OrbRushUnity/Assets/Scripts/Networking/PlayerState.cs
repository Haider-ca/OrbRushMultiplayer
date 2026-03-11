namespace OrbRush.Networking
{
	[System.Serializable]
	public class PlayerState
	{
		public string messageType;
		public int playerId;
		public float x;
		public float y;
		public float z;
		public int score;
		public long sequence;
		public int winnerId;
	}
}