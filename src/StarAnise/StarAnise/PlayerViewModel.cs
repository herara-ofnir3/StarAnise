namespace StarAnise
{
	public class PlayerViewModel : ViewModel
	{
		public PlayerViewModel(PlayerNumber number)
		{
			Number = number;
		}

		public PlayerNumber Number { get; }
	}
}
