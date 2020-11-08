using System;
using System.Collections.Generic;
using System.Linq;

namespace StarAnise
{
	public static class Player
	{
		public static readonly IReadOnlyList<PlayerNumber> AllNumbers 
			= Enum.GetValues(typeof(PlayerNumber)).Cast<PlayerNumber>().ToArray();
	}

	public enum PlayerNumber
	{
		P1 = 1, P2, P3, P4, P5, P6, P7, P8
	}

	public readonly struct Round : IEquatable<Round>
	{
		public static Round First(PlayerNumber matchedOther)
			=> new Round(1, matchedOther, Array.Empty<PlayerNumber>());

		Round(int number, PlayerNumber matchedOther, IEnumerable<PlayerNumber> defeateds)
		{
			Number = number;
			MatchedOther = matchedOther;
			Defeateds = defeateds.ToArray();
		}

		public int Number { get; }
		public PlayerNumber MatchedOther { get; }
		public IReadOnlyList<PlayerNumber> Defeateds { get; }
		public bool HasDefeateds => Defeateds.Any();

		public Round NextMatch(PlayerNumber matchedOther)
			=> new Round(Number + 1, matchedOther, Array.Empty<PlayerNumber>());

		public Round Defeated(PlayerNumber defeated)
			=> new Round(Number, MatchedOther, Defeateds.Append(defeated).ToArray());

		public bool Equals(Round other)
			=> Number == other.Number && MatchedOther == other.MatchedOther && Defeateds.SequenceEqual(other.Defeateds);

		public override bool Equals(object obj) => obj is Round other && Equals(other);

		public override int GetHashCode()
			=> (Number.GetHashCode(), MatchedOther.GetHashCode(), Defeateds.Select(it => it.GetHashCode()).Aggregate((a, b) => a ^ b)).GetHashCode();

		public static bool operator ==(Round left, Round right) => left.Equals(right);

		public static bool operator !=(Round left, Round right) => !(left == right);
	}
}
