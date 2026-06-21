using Bloomtown.Shared.Routines;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Tests;

public sealed class PersonalRoutineConfigTests
{
    [Fact]
    public void All_DefinesThreePersonalRoutines()
    {
        Assert.Equal(3, PersonalRoutineConfig.All.Count);
    }

    [Fact]
    public void TryGet_MapsCommands()
    {
        Assert.True(PersonalRoutineConfig.TryGet(PersonalRoutineKind.MorningStretch, out var stretch));
        Assert.Equal("morning stretch", stretch.CommandHint);

        Assert.True(PersonalRoutineConfig.TryGet(PersonalRoutineKind.EveningWindDown, out var windDown));
        Assert.Equal("evening wind down", windDown.CommandHint);

        Assert.True(PersonalRoutineConfig.TryGet(PersonalRoutineKind.SitAndReflect, out var reflect));
        Assert.Equal("sit and reflect", reflect.CommandHint);
    }

    [Fact]
    public void CalculateEffects_GivesBonusDuringIdealPhase()
    {
        var stretch = PersonalRoutineConfig.All.First(r => r.Kind == PersonalRoutineKind.MorningStretch);

        var ideal = PersonalRoutineConfig.CalculateEffects(stretch, GameTimeOfDay.Morning);
        var offPhase = PersonalRoutineConfig.CalculateEffects(stretch, GameTimeOfDay.Evening);

        Assert.True(ideal.IdealPhase);
        Assert.False(offPhase.IdealPhase);
        Assert.True(ideal.MoodGain > offPhase.MoodGain);
        Assert.True(ideal.FatigueReduction > offPhase.FatigueReduction);
    }

    [Fact]
    public void CalculateEffects_SitAndReflectFavorsMoodInAfternoon()
    {
        var reflect = PersonalRoutineConfig.All.First(r => r.Kind == PersonalRoutineKind.SitAndReflect);

        var afternoon = PersonalRoutineConfig.CalculateEffects(reflect, GameTimeOfDay.Afternoon);
        var night = PersonalRoutineConfig.CalculateEffects(reflect, GameTimeOfDay.Night);

        Assert.True(afternoon.IdealPhase);
        Assert.False(night.IdealPhase);
        Assert.Equal(8f, afternoon.MoodGain);
        Assert.Equal(6f * PersonalRoutineConfig.OffPhaseMultiplier, night.MoodGain);
    }

    [Theory]
    [InlineData(PersonalRoutineKind.MorningStretch)]
    [InlineData(PersonalRoutineKind.EveningWindDown)]
    [InlineData(PersonalRoutineKind.SitAndReflect)]
    public void GetCooldown_ReturnsPositiveDuration(PersonalRoutineKind kind)
    {
        Assert.True(PersonalRoutineConfig.GetCooldown(kind) > TimeSpan.Zero);
    }

    [Fact]
    public void FormatPersonalRhythmStatus_IncludesCurrentPhase()
    {
        var status = PersonalRoutineConfig.FormatPersonalRhythmStatus(GameTimeOfDay.Morning);
        Assert.Contains("Morning", status);
        Assert.StartsWith("Personal rhythm", status);
    }

    [Fact]
    public void PickFlavorText_ReturnsNonEmptyLine()
    {
        var routine = PersonalRoutineConfig.All[0];
        var flavor = PersonalRoutineConfig.PickFlavorText(routine, idealPhase: true, variationSeed: 0);
        Assert.False(string.IsNullOrWhiteSpace(flavor));
    }

    [Theory]
    [InlineData(5, GameTimeOfDay.Morning)]
    [InlineData(14, GameTimeOfDay.Afternoon)]
    public void GameTimeHelper_MatchesVillageLifeBands(int hour, GameTimeOfDay expected)
    {
        Assert.Equal(expected, GameTimeHelper.GetTimeOfDay(hour));
    }
}