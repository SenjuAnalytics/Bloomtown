namespace Bloomtown.Shared.Protocol;

public enum EmotionalBondRequestKind : byte
{
    None = 0,
    Perform = 1,

    /// <summary>Player actively requests a small village favor using social standing.</summary>
    RequestStandingFavor = 2,

    /// <summary>Well-liked player actively calls on Harold or Greta for social influence.</summary>
    RequestSocialInfluence = 3,
}