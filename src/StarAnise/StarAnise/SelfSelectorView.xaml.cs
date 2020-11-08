using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Controls;

namespace StarAnise
{
	/// <summary>
	/// SelfSelector.xaml の相互作用ロジック
	/// </summary>
	public partial class SelfSelectorView : UserControl
	{
		public SelfSelectorView(SelfSelectorViewModel viewModel)
		{
			InitializeComponent();
			DataContext = viewModel;
		}
	}

	public class SelfSelectorViewModel : ViewModel
	{
		public SelfSelectorViewModel(PlayerNumber number, OverlayViewModel overlay)
		{
			Number = number;
			SelectCommand = new ReactiveCommand();
			SelectCommand
				.Subscribe(_ => overlay.Ready(number))
				.AddTo(DisposeBag);
		}

		public PlayerNumber Number { get; }

		public ReactiveCommand SelectCommand { get; }
	}
}
