using System.Windows.Controls;

namespace StarAnise
{
	/// <summary>
	/// Self.xaml の相互作用ロジック
	/// </summary>
	public partial class SelfView : UserControl
	{
		public SelfView(SelfViewModel viewModel)
		{
			InitializeComponent();
			DataContext = viewModel;
		}
	}

	public class SelfViewModel : PlayerViewModel
	{
		public SelfViewModel(PlayerNumber number) : base(number)
		{
		}
	}
}
