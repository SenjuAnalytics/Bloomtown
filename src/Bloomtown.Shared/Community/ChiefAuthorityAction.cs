namespace Bloomtown.Shared.Community;

/// <summary>
/// Special Chief actions logged for daily limits and auditing.
/// </summary>
public enum ChiefAuthorityAction : byte
{
    DirectApprove = 1,
    Veto = 2,
}