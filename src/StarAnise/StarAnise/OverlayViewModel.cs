using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace StarAnise
{
	public class OverlayViewModel : ViewModel
	{
		public ReactiveProperty<StateType> State { get; } = new ReactiveProperty<StateType>(new StateType.Standby(null));
		public ReactiveCollection<SelfSelectorViewModel> SelfSelectors { get; } = new ReactiveCollection<SelfSelectorViewModel>();
		public ReactiveCollection<PlayerViewModel> Players { get; } = new ReactiveCollection<PlayerViewModel>();
		public ReactiveCommand ResetCommand { get; }

		public OverlayViewModel()
		{
			ResetCommand = State.Select(it => !it.IsStandby).ToReactiveCommand();
			ResetCommand
				.Subscribe(_ =>
				{
					if (State.Value is StateType.InBattle inBattle)
						State.Value = inBattle.Reset();
				})
				.AddTo(DisposeBag);
		}

		public void Initialize()
		{
			State.Standby()
				.Subscribe(_ =>
				{
					foreach (var player in Players)
						player.Dispose();
					Players.Clear();

					foreach (var number in Player.AllNumbers)
						SelfSelectors.Add(new SelfSelectorViewModel(number, this).AddTo(DisposeBag));
				})
				.AddTo(DisposeBag);

			State.InBattle()
				.Where(it => it.Prev.IsStandby)
				.Subscribe(inBattle =>
				{
					foreach (var selfSelector in SelfSelectors)
						selfSelector.Dispose();
					SelfSelectors.Clear();

					foreach (var number in Player.AllNumbers)
					{
						if (number == inBattle.Self)
							Players.Add(new SelfViewModel(number).AddTo(DisposeBag));
						else
							Players.Add(new OtherViewModel(number, this).AddTo(DisposeBag));
					}
				})
				.AddTo(DisposeBag);
		}

		public void Ready(PlayerNumber self)
		{
			if (State.Value is StateType.Standby standby)
				State.Value = standby.Ready(self);
		}

		public void Matched(PlayerNumber matchedOther)
		{
			if (State.Value is StateType.InBattle inBattle)
				State.Value = inBattle.NextRound(matchedOther);
		}

		public void Defeated(PlayerNumber defeated)
		{
			if (State.Value is StateType.InBattle inBattle)
				State.Value = inBattle.Defeated(defeated);
		}

		public class StateType
		{
			public bool IsStandby => this is Standby;
			public bool IsInBattle => this is InBattle;

			public class Standby : StateType 
			{
				public Standby(StateType prev) => Prev = prev;

				public StateType Prev { get; }

				public InBattle Ready(PlayerNumber self)
					=> new InBattle(this, self, Array.Empty<Round>());
			}

			public class InBattle : StateType
			{
				public InBattle(StateType prev, PlayerNumber self, IEnumerable<Round> rounds)
				{
					Prev = prev;
					Self = self;
					Rounds = rounds.OrderBy(it => it.Number).ToArray();
					Others = Player.AllNumbers.Where(it => it != Self).ToArray();
				}

				public StateType Prev { get; }
				public PlayerNumber Self { get; }
				public IReadOnlyList<Round> Rounds { get; }
				public Round? LastRound => Rounds.Select(it => (Round?)it).LastOrDefault();

				public IReadOnlyList<PlayerNumber> Others;

				public IReadOnlyList<PlayerNumber> DefeatedOthers()
					=> Rounds.SelectMany(it => it.Defeateds).ToArray();

				public bool IsDefeated(PlayerNumber number)
					=> DefeatedOthers().Contains(number);

				public IReadOnlyList<PlayerNumber> AlivedOthers()
					=> Others.Except(DefeatedOthers()).ToArray();

				public InBattle NextRound(PlayerNumber matchedOther)
				{
					var nextRound = LastRound?.NextMatch(matchedOther) ?? Round.First(matchedOther);
					return new InBattle(this, Self, Rounds.Append(nextRound));
				}

				public InBattle Defeated(PlayerNumber defeated)
				{
					if (!LastRound.HasValue)
						throw new InvalidOperationException();

					return new InBattle(this, Self, Rounds.Take(Rounds.Count - 1).Append(LastRound.Value.Defeated(defeated)));
				}

				public Standby Reset() => new Standby(this);

				public IEnumerable<PlayerNumber> NextMatchCandidates()
				{
					var lastDefeatedRound = Rounds
						.OrderByDescending(it => it.Number)
						.Select(it => (Round?)it)
						.FirstOrDefault(it => it.Value.HasDefeateds);

					var targetRounds = Rounds.SkipWhile(it => it.Number <= (lastDefeatedRound?.Number ?? 0));
					var alivedOthers = AlivedOthers();
					var recentRoundCount = 4 < alivedOthers.Count ? 4 : alivedOthers.Count - 1;
					var recentRounds = targetRounds.OrderByDescending(it => it.Number).Take(recentRoundCount);
					return alivedOthers.Except(recentRounds.Select(it => it.MatchedOther));
				}
			}
		}
	}

	public static class OverlayViewModelHelper
	{
		public static IObservable<OverlayViewModel.StateType.Standby> Standby(this IObservable<OverlayViewModel.StateType> observable)
			=> observable.OfType<OverlayViewModel.StateType.Standby>();

		public static IObservable<OverlayViewModel.StateType.InBattle> InBattle(this IObservable<OverlayViewModel.StateType> observable)
			=> observable.OfType<OverlayViewModel.StateType.InBattle>();
	}
}
