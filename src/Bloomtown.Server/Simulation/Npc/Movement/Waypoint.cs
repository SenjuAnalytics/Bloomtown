namespace Bloomtown.Server.Simulation.Npc.Movement;

/// <summary>One world-space point on an NPC patrol path (XZ plane).</summary>
public readonly record struct Waypoint(float X, float Z);