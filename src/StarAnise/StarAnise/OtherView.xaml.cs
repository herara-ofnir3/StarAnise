using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StarAnise
{
	/// <summary>
	/// Other.xaml の相互作用ロジック
	/// </summary>
	public partial class OtherView : UserControl
	{
		public OtherView(OtherViewModel viewModel)
		{
			InitializeComponent();
			DataContext = ViewModel = viewModel;
		}

		public OtherViewModel ViewModel { get; }
	}

	public class OtherViewModel : PlayerViewModel
	{
		public OtherViewModel(PlayerNumber number, OverlayViewModel overlay) : base(number)
		{
			IsNextMatchCandidate = overlay.State.InBattle()
				.Select(it => it.NextMatchCandidates().Contains(number))
				.ToReactiveProperty()
				.AddTo(DisposeBag);

			IsDefeated = overlay.State.InBattle()
				.Select(it => it.IsDefeated(number))
				.ToReadOnlyReactiveProperty()
				.AddTo(DisposeBag);

			IsAlived = IsDefeated
				.Select(it => !it)
				.ToReadOnlyReactiveProperty()
				.AddTo(DisposeBag);

			BackgroundColor = Observable.CombineLatest(IsNextMatchCandidate, IsDefeated, (a, b) => (isNextMatchCandidate: a, isDefeated: b))
				.Select(it =>
				{
					if (it.isNextMatchCandidate)
						return new SolidColorBrush(Colors.Orange);

					if (it.isDefeated)
						return new SolidColorBrush(Colors.DarkGray);

					return new SolidColorBrush(Colors.Transparent);
				})
				.ToReadOnlyReactiveProperty()
				.AddTo(DisposeBag);

			MatchedCommand = IsNextMatchCandidate
				.ToReactiveCommand()
				.AddTo(DisposeBag);

			DefeatedCommand = overlay.State.InBattle()
				.Select(it => it.LastRound.HasValue && !it.IsDefeated(number))
				.ToReactiveCommand()
				.AddTo(DisposeBag);

			MatchedCommand
				.Subscribe(_ => overlay.Matched(number))
				.AddTo(DisposeBag);

			DefeatedCommand
				.Subscribe(_ => overlay.Defeated(number))
				.AddTo(DisposeBag);
		}

		public IReadOnlyReactiveProperty<bool> IsDefeated { get; }
		public IReadOnlyReactiveProperty<bool> IsAlived { get; }
		public IReadOnlyReactiveProperty<bool> IsNextMatchCandidate { get; }
		public IReadOnlyReactiveProperty<SolidColorBrush> BackgroundColor { get; }

		public ReactiveCommand MatchedCommand { get; }
		public ReactiveCommand DefeatedCommand { get; }
	}
}
