using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StarAnise
{
	/// <summary>
	/// OverlayWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class OverlayWindow : Window
	{
		CompositeDisposable DisposeBag = new CompositeDisposable();

        public OverlayWindow()
        {
			ViewModel = new OverlayViewModel().AddTo(DisposeBag);
			DataContext = ViewModel;
            InitializeComponent();

			Observable.Merge(
				ViewModel.SelfSelectors.ObserveRemoveChanged().Select(vm => vm.Number),
				ViewModel.Players.ObserveRemoveChanged().Select(vm => vm.Number)
				)
				.Subscribe(number => Player(number, g => g.Children.Clear()))
				.AddTo(DisposeBag);

			Observable.Merge(
				ViewModel.SelfSelectors.ObserveResetChanged(),
				ViewModel.Players.ObserveResetChanged())
				.Subscribe(_ =>
				{
					foreach (var grid in PlayerViewContainers())
						grid.Children.Clear();
				})
				.AddTo(DisposeBag);

			ViewModel.SelfSelectors
				.ObserveAddChanged()
				.Subscribe(it => Player(it.Number, g => g.Children.Add(new SelfSelectorView(it))))
				.AddTo(DisposeBag);

			ViewModel.Players
				.ObserveAddChanged()
				.OfType<OtherViewModel>()
				.Subscribe(vm => Player(vm.Number, g => g.Children.Add(new OtherView(vm))))
				.AddTo(DisposeBag);

			ViewModel.Players
				.ObserveAddChanged()
				.OfType<SelfViewModel>()
				.Subscribe(vm => Player(vm.Number, g => g.Children.Add(new SelfView(vm))))
				.AddTo(DisposeBag);

			ViewModel.Initialize();
		}

		OverlayViewModel ViewModel { get; }

		IEnumerable<Grid> PlayerViewContainers()
		{
			yield return Player1;
			yield return Player2;
			yield return Player3;
			yield return Player4;
			yield return Player5;
			yield return Player6;
			yield return Player7;
			yield return Player8;
		}

		void Player(PlayerNumber number, Action<Grid> action)
			=> action(number switch
			{
				PlayerNumber.P1 => Player1,
				PlayerNumber.P2 => Player2,
				PlayerNumber.P3 => Player3,
				PlayerNumber.P4 => Player4,
				PlayerNumber.P5 => Player5,
				PlayerNumber.P6 => Player6,
				PlayerNumber.P7 => Player7,
				PlayerNumber.P8 => Player8,
				_ => throw new ArgumentOutOfRangeException()
			});

		protected override void OnClosed(EventArgs e)
		{
			DisposeBag.Dispose();
			base.OnClosed(e);
		}
	}
}
