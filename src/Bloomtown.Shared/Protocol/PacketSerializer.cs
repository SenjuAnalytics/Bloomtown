using System.Buffers.Binary;
using System.Text;
using Bloomtown.Shared.Crafting;
using Bloomtown.Shared.Housing;
using Bloomtown.Shared.Community;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Leadership;
using Bloomtown.Shared.Milestone;

namespace Bloomtown.Shared.Protocol;

public static class PacketSerializer
{
    public const int PlayerInputPacketSize = 18;

    public static int WritePlayerInput(Span<byte> buffer, PlayerInput input)
    {
        buffer[0] = (byte)PacketType.PlayerInput;
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[1..5], input.Seq);
        BinaryPrimitivesCompat.WriteSingleLittleEndian(buffer[5..9], input.MoveDirX);
        BinaryPrimitivesCompat.WriteSingleLittleEndian(buffer[9..13], input.MoveDirY);
        BinaryPrimitivesCompat.WriteSingleLittleEndian(buffer[13..17], input.LookYaw);
        buffer[17] = (byte)(input.JumpPressed ? 1 : 0);
        return PlayerInputPacketSize;
    }

    public static PlayerInput ReadPlayerInput(ReadOnlySpan<byte> buffer)
    {
        return new PlayerInput
        {
            Seq = BinaryPrimitives.ReadUInt32LittleEndian(buffer[1..5]),
            MoveDirX = BinaryPrimitivesCompat.ReadSingleLittleEndian(buffer[5..9]),
            MoveDirY = BinaryPrimitivesCompat.ReadSingleLittleEndian(buffer[9..13]),
            LookYaw = BinaryPrimitivesCompat.ReadSingleLittleEndian(buffer[13..17]),
            JumpPressed = buffer.Length > 17 && buffer[17] == 1,
        };
    }

    public static int WriteEntityDelta(Span<byte> buffer, EntityDelta delta)
    {
        buffer[0] = (byte)PacketType.EntityDelta;
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[1..5], delta.EntityId);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[5..9], delta.Seq);
        BinaryPrimitivesCompat.WriteSingleLittleEndian(buffer[9..13], delta.PositionX);
        BinaryPrimitivesCompat.WriteSingleLittleEndian(buffer[13..17], delta.PositionY);
        BinaryPrimitivesCompat.WriteSingleLittleEndian(buffer[17..21], delta.PositionZ);
        BinaryPrimitivesCompat.WriteSingleLittleEndian(buffer[21..25], delta.RotationYaw);
        return 25;
    }

    public static EntityDelta ReadEntityDelta(ReadOnlySpan<byte> buffer)
    {
        return new EntityDelta
        {
            EntityId = BinaryPrimitives.ReadUInt32LittleEndian(buffer[1..5]),
            Seq = BinaryPrimitives.ReadUInt32LittleEndian(buffer[5..9]),
            PositionX = BinaryPrimitivesCompat.ReadSingleLittleEndian(buffer[9..13]),
            PositionY = BinaryPrimitivesCompat.ReadSingleLittleEndian(buffer[13..17]),
            PositionZ = BinaryPrimitivesCompat.ReadSingleLittleEndian(buffer[17..21]),
            RotationYaw = BinaryPrimitivesCompat.ReadSingleLittleEndian(buffer[21..25]),
        };
    }

    public static int WriteConnectAccept(Span<byte> buffer, ConnectAccept accept)
    {
        buffer[0] = (byte)PacketType.ConnectAccept;
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[1..5], accept.EntityId);
        BinaryPrimitivesCompat.WriteSingleLittleEndian(buffer[5..9], accept.SpawnX);
        BinaryPrimitivesCompat.WriteSingleLittleEndian(buffer[9..13], accept.SpawnY);
        BinaryPrimitivesCompat.WriteSingleLittleEndian(buffer[13..17], accept.SpawnZ);
        return 17;
    }

    public static ConnectAccept ReadConnectAccept(ReadOnlySpan<byte> buffer)
    {
        return new ConnectAccept
        {
            EntityId = BinaryPrimitives.ReadUInt32LittleEndian(buffer[1..5]),
            SpawnX = BinaryPrimitivesCompat.ReadSingleLittleEndian(buffer[5..9]),
            SpawnY = BinaryPrimitivesCompat.ReadSingleLittleEndian(buffer[9..13]),
            SpawnZ = BinaryPrimitivesCompat.ReadSingleLittleEndian(buffer[13..17]),
        };
    }

    public const int NpcInteractionRequestPacketSize = 6;
    public const int NpcInteractionResponseHeaderSize = 10;
    public const int MaxInteractionMessageBytes = 200;

    public static int WriteNpcInteractionRequest(Span<byte> buffer, NpcInteractionRequest request)
    {
        buffer[0] = (byte)PacketType.NpcInteractionRequest;
        buffer[1] = (byte)request.Kind;
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[2..6], request.TargetNpcEntityId);
        return NpcInteractionRequestPacketSize;
    }

    public static NpcInteractionRequest ReadNpcInteractionRequest(ReadOnlySpan<byte> buffer)
    {
        return new NpcInteractionRequest(
            (NpcInteractionKind)buffer[1],
            BinaryPrimitives.ReadUInt32LittleEndian(buffer[2..6]));
    }

    public static int WriteNpcInteractionResponse(Span<byte> buffer, NpcInteractionResponse response)
    {
        buffer[0] = (byte)PacketType.NpcInteractionResponse;
        buffer[1] = (byte)(response.Success ? 1 : 0);
        buffer[2] = (byte)response.Kind;
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[3..7], response.NpcEntityId);
        buffer[7] = (byte)response.FailureReason;

        var messageBytes = Encoding.UTF8.GetBytes(response.Message);
        if (messageBytes.Length > MaxInteractionMessageBytes)
            messageBytes = messageBytes[..MaxInteractionMessageBytes];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[8..10], (ushort)messageBytes.Length);
        messageBytes.CopyTo(buffer[10..]);

        return NpcInteractionResponseHeaderSize + messageBytes.Length;
    }

    public static NpcInteractionResponse ReadNpcInteractionResponse(ReadOnlySpan<byte> buffer)
    {
        var success = buffer[1] == 1;
        var kind = (NpcInteractionKind)buffer[2];
        var npcEntityId = BinaryPrimitives.ReadUInt32LittleEndian(buffer[3..7]);
        var failureReason = (NpcInteractionFailureReason)buffer[7];
        var messageLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[8..10]);
        var message = Encoding.UTF8.GetString(buffer[10..(10 + messageLength)]);

        return new NpcInteractionResponse(success, kind, npcEntityId, failureReason, message);
    }

    public const int EconomyRequestPacketSize = 8;
    public const int EconomyResponseHeaderSize = 10;
    public const int MaxEconomyMessageBytes = 300;

    public static int WriteEconomyRequest(Span<byte> buffer, EconomyRequest request)
    {
        buffer[0] = (byte)PacketType.EconomyRequest;
        buffer[1] = (byte)request.Kind;
        buffer[2] = (byte)request.ItemType;
        buffer[3] = request.Quantity;
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[4..8], request.NpcEntityId);
        return EconomyRequestPacketSize;
    }

    public static EconomyRequest ReadEconomyRequest(ReadOnlySpan<byte> buffer)
    {
        return new EconomyRequest(
            (EconomyRequestKind)buffer[1],
            (ItemType)buffer[2],
            buffer[3],
            BinaryPrimitives.ReadUInt32LittleEndian(buffer[4..8]));
    }

    public static int WriteEconomyResponse(Span<byte> buffer, EconomyResponse response)
    {
        buffer[0] = (byte)PacketType.EconomyResponse;
        buffer[1] = (byte)(response.Success ? 1 : 0);
        buffer[2] = (byte)response.Kind;
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[3..7], response.NpcEntityId);
        buffer[7] = (byte)response.FailureReason;

        var messageBytes = Encoding.UTF8.GetBytes(response.Message);
        if (messageBytes.Length > MaxEconomyMessageBytes)
            messageBytes = messageBytes[..MaxEconomyMessageBytes];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[8..10], (ushort)messageBytes.Length);
        messageBytes.CopyTo(buffer[10..]);

        return EconomyResponseHeaderSize + messageBytes.Length;
    }

    public static EconomyResponse ReadEconomyResponse(ReadOnlySpan<byte> buffer)
    {
        var success = buffer[1] == 1;
        var kind = (EconomyRequestKind)buffer[2];
        var npcEntityId = BinaryPrimitives.ReadUInt32LittleEndian(buffer[3..7]);
        var failureReason = (EconomyFailureReason)buffer[7];
        var messageLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[8..10]);
        var message = Encoding.UTF8.GetString(buffer[10..(10 + messageLength)]);

        return new EconomyResponse(success, kind, npcEntityId, failureReason, message);
    }

    public const int GatheringRequestPacketSize = 3;
    public const int GatheringResponseHeaderSize = 14;
    public const int MaxGatheringMessageBytes = 250;

    public static int WriteGatheringRequest(Span<byte> buffer, GatheringRequest request)
    {
        buffer[0] = (byte)PacketType.GatheringRequest;
        buffer[1] = (byte)request.ResourceType;
        buffer[2] = 0;
        return GatheringRequestPacketSize;
    }

    public static GatheringRequest ReadGatheringRequest(ReadOnlySpan<byte> buffer)
    {
        return new GatheringRequest((ItemType)buffer[1]);
    }

    public static int WriteGatheringResponse(Span<byte> buffer, GatheringResponse response)
    {
        buffer[0] = (byte)PacketType.GatheringResponse;
        buffer[1] = (byte)response.Kind;
        buffer[2] = (byte)response.ResourceType;
        buffer[3] = (byte)response.FailureReason;
        BinaryPrimitives.WriteInt32LittleEndian(buffer[4..8], response.Quantity);
        BinaryPrimitives.WriteInt32LittleEndian(buffer[8..12], response.NodeId);

        var messageBytes = Encoding.UTF8.GetBytes(response.Message);
        if (messageBytes.Length > MaxGatheringMessageBytes)
            messageBytes = messageBytes[..MaxGatheringMessageBytes];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[12..14], (ushort)messageBytes.Length);
        messageBytes.CopyTo(buffer[14..]);

        // FIX: Sebelumnya return GatheringResponseHeaderSize + 2 + messageBytes.Length (kelebihan 2 byte).
        // Header 14 byte sudah termasuk 2 byte ushort panjang pesan di [12..14],
        // jadi return yang benar adalah HeaderSize + messageBytes.Length saja.
        return GatheringResponseHeaderSize + messageBytes.Length;
    }

    public static GatheringResponse ReadGatheringResponse(ReadOnlySpan<byte> buffer)
    {
        var kind = (GatheringResponseKind)buffer[1];
        var resourceType = (ItemType)buffer[2];
        var failureReason = (GatheringFailureReason)buffer[3];
        var quantity = BinaryPrimitives.ReadInt32LittleEndian(buffer[4..8]);
        var nodeId = BinaryPrimitives.ReadInt32LittleEndian(buffer[8..12]);
        var messageLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[12..14]);
        var message = Encoding.UTF8.GetString(buffer[14..(14 + messageLength)]);

        return new GatheringResponse(kind, resourceType, quantity, nodeId, failureReason, message);
    }

    public const int ClientQueryRequestPacketSize = 3;
    public const int ClientQueryResponseHeaderSize = 5;
    public const int MaxClientQueryMessageBytes = 1200;

    public static int WriteClientQueryRequest(Span<byte> buffer, ClientQueryRequest request)
    {
        buffer[0] = (byte)PacketType.ClientQueryRequest;
        buffer[1] = (byte)request.Kind;
        buffer[2] = 0;
        return ClientQueryRequestPacketSize;
    }

    public static ClientQueryRequest ReadClientQueryRequest(ReadOnlySpan<byte> buffer)
    {
        return new ClientQueryRequest((ClientQueryKind)buffer[1]);
    }

    public static int WriteClientQueryResponse(Span<byte> buffer, ClientQueryResponse response)
    {
        buffer[0] = (byte)PacketType.ClientQueryResponse;
        buffer[1] = (byte)response.Kind;
        buffer[2] = (byte)(response.Success ? 1 : 0);

        var messageBytes = Encoding.UTF8.GetBytes(response.Message);
        if (messageBytes.Length > MaxClientQueryMessageBytes)
            messageBytes = messageBytes[..MaxClientQueryMessageBytes];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[3..5], (ushort)messageBytes.Length);
        messageBytes.CopyTo(buffer[5..]);

        return ClientQueryResponseHeaderSize + messageBytes.Length;
    }

    public static ClientQueryResponse ReadClientQueryResponse(ReadOnlySpan<byte> buffer)
    {
        var kind = (ClientQueryKind)buffer[1];
        var success = buffer[2] == 1;
        var messageLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[3..5]);
        var message = Encoding.UTF8.GetString(buffer[5..(5 + messageLength)]);

        return new ClientQueryResponse(kind, success, message);
    }

    public const int ChestRequestPacketSize = 5;
    public const int ChestResponseHeaderSize = 6;
    public const int MaxChestMessageBytes = 300;

    public static int WriteChestRequest(Span<byte> buffer, ChestRequest request)
    {
        buffer[0] = (byte)PacketType.ChestRequest;
        buffer[1] = (byte)request.Kind;
        buffer[2] = (byte)request.ItemType;
        buffer[3] = request.Quantity;
        buffer[4] = 0;
        return ChestRequestPacketSize;
    }

    public static ChestRequest ReadChestRequest(ReadOnlySpan<byte> buffer)
    {
        return new ChestRequest(
            (ChestRequestKind)buffer[1],
            (ItemType)buffer[2],
            buffer[3]);
    }

    public static int WriteChestResponse(Span<byte> buffer, ChestResponse response)
    {
        buffer[0] = (byte)PacketType.ChestResponse;
        buffer[1] = (byte)(response.Success ? 1 : 0);
        buffer[2] = (byte)response.Kind;
        buffer[3] = (byte)response.FailureReason;

        var messageBytes = Encoding.UTF8.GetBytes(response.Message);
        if (messageBytes.Length > MaxChestMessageBytes)
            messageBytes = messageBytes[..MaxChestMessageBytes];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[4..6], (ushort)messageBytes.Length);
        messageBytes.CopyTo(buffer[6..]);

        return ChestResponseHeaderSize + messageBytes.Length;
    }

    public static ChestResponse ReadChestResponse(ReadOnlySpan<byte> buffer)
    {
        var success = buffer[1] == 1;
        var kind = (ChestRequestKind)buffer[2];
        var failureReason = (ChestFailureReason)buffer[3];
        var messageLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[4..6]);
        var message = Encoding.UTF8.GetString(buffer[6..(6 + messageLength)]);

        return new ChestResponse(success, kind, failureReason, message);
    }

    public const int CommunityProjectRequestPacketSize = 6;
    public const int CommunityProjectResponseHeaderSize = 6;
    public const int MaxCommunityProjectMessageBytes = 1200;

    public static int WriteCommunityProjectRequest(Span<byte> buffer, CommunityProjectRequest request)
    {
        buffer[0] = (byte)PacketType.CommunityProjectRequest;
        buffer[1] = (byte)request.Kind;
        buffer[2] = request.ProjectId;
        buffer[3] = (byte)request.ItemType;
        buffer[4] = request.Quantity;
        buffer[5] = 0;
        return CommunityProjectRequestPacketSize;
    }

    public static CommunityProjectRequest ReadCommunityProjectRequest(ReadOnlySpan<byte> buffer)
    {
        return new CommunityProjectRequest(
            (CommunityProjectRequestKind)buffer[1],
            buffer[2],
            (ItemType)buffer[3],
            buffer[4]);
    }

    public static int WriteCommunityProjectResponse(Span<byte> buffer, CommunityProjectResponse response)
    {
        buffer[0] = (byte)PacketType.CommunityProjectResponse;
        buffer[1] = (byte)(response.Success ? 1 : 0);
        buffer[2] = (byte)response.Kind;
        buffer[3] = (byte)response.FailureReason;

        var messageBytes = Encoding.UTF8.GetBytes(response.Message);
        if (messageBytes.Length > MaxCommunityProjectMessageBytes)
            messageBytes = messageBytes[..MaxCommunityProjectMessageBytes];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[4..6], (ushort)messageBytes.Length);
        messageBytes.CopyTo(buffer[6..]);

        return CommunityProjectResponseHeaderSize + messageBytes.Length;
    }

    public static CommunityProjectResponse ReadCommunityProjectResponse(ReadOnlySpan<byte> buffer)
    {
        var success = buffer[1] == 1;
        var kind = (CommunityProjectRequestKind)buffer[2];
        var failureReason = (CommunityProjectFailureReason)buffer[3];
        var messageLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[4..6]);
        var message = Encoding.UTF8.GetString(buffer[6..(6 + messageLength)]);

        return new CommunityProjectResponse(success, kind, failureReason, message);
    }

    public const int MilestoneRequestPacketSize = 5;
    public const int MilestoneResponseHeaderSize = 6;
    public const int MilestoneNotificationHeaderSize = 3;
    public const int MaxMilestoneMessageBytes = 400;

    public static int WriteMilestoneRequest(Span<byte> buffer, MilestoneRequest request)
    {
        buffer[0] = (byte)PacketType.MilestoneRequest;
        buffer[1] = (byte)request.Kind;
        buffer[2] = (byte)request.Interaction;
        buffer[3] = 0;
        buffer[4] = 0;
        return MilestoneRequestPacketSize;
    }

    public static MilestoneRequest ReadMilestoneRequest(ReadOnlySpan<byte> buffer)
    {
        return new MilestoneRequest(
            (MilestoneRequestKind)buffer[1],
            (MilestoneInteractionKind)buffer[2]);
    }

    public static int WriteMilestoneResponse(Span<byte> buffer, MilestoneResponse response)
    {
        buffer[0] = (byte)PacketType.MilestoneResponse;
        buffer[1] = (byte)(response.Success ? 1 : 0);
        buffer[2] = (byte)response.Kind;
        buffer[3] = (byte)response.FailureReason;

        var messageBytes = Encoding.UTF8.GetBytes(response.Message);
        if (messageBytes.Length > MaxMilestoneMessageBytes)
            messageBytes = messageBytes[..MaxMilestoneMessageBytes];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[4..6], (ushort)messageBytes.Length);
        messageBytes.CopyTo(buffer[6..]);

        return MilestoneResponseHeaderSize + messageBytes.Length;
    }

    public static MilestoneResponse ReadMilestoneResponse(ReadOnlySpan<byte> buffer)
    {
        var success = buffer[1] == 1;
        var kind = (MilestoneRequestKind)buffer[2];
        var failureReason = (MilestoneFailureReason)buffer[3];
        var messageLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[4..6]);
        var message = Encoding.UTF8.GetString(buffer[6..(6 + messageLength)]);

        return new MilestoneResponse(success, kind, failureReason, message);
    }

    public static int WriteMilestoneNotification(Span<byte> buffer, string message)
    {
        buffer[0] = (byte)PacketType.MilestoneNotification;

        var messageBytes = Encoding.UTF8.GetBytes(message);
        if (messageBytes.Length > MaxMilestoneMessageBytes)
            messageBytes = messageBytes[..MaxMilestoneMessageBytes];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[1..3], (ushort)messageBytes.Length);
        messageBytes.CopyTo(buffer[3..]);

        return MilestoneNotificationHeaderSize + messageBytes.Length;
    }

    public static string ReadMilestoneNotification(ReadOnlySpan<byte> buffer)
    {
        var messageLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[1..3]);
        return Encoding.UTF8.GetString(buffer[3..(3 + messageLength)]);
    }

    public const int ProjectProposalRequestHeaderSize = 8;
    public const int ProjectProposalRequestTailSize = 5;
    public const int MaxProjectProposalNameBytes = 80;
    public const int ProjectProposalResponseHeaderSize = 6;
    public const int ProjectProposalNotificationHeaderSize = 3;
    public const int MaxProjectProposalMessageBytes = 1200;

    public static int WriteProjectProposalRequest(Span<byte> buffer, ProjectProposalRequest request)
    {
        buffer[0] = (byte)PacketType.ProjectProposalRequest;
        buffer[1] = (byte)request.Kind;

        var nameBytes = Encoding.UTF8.GetBytes(request.ProjectName);
        if (nameBytes.Length > MaxProjectProposalNameBytes)
            nameBytes = nameBytes[..MaxProjectProposalNameBytes];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[2..4], (ushort)nameBytes.Length);
        nameBytes.CopyTo(buffer[4..]);
        var offset = 4 + nameBytes.Length;

        buffer[offset] = request.WoodQuantity;
        buffer[offset + 1] = request.StoneQuantity;
        buffer[offset + 2] = request.AppleQuantity;
        buffer[offset + 3] = request.ToolQuantity;
        offset += 4;

        BinaryPrimitives.WriteInt32LittleEndian(buffer[offset..], request.ProposalId);
        buffer[offset + 4] = (byte)request.VoteChoice;

        return ProjectProposalRequestHeaderSize + nameBytes.Length + ProjectProposalRequestTailSize;
    }

    public static ProjectProposalRequest ReadProjectProposalRequest(ReadOnlySpan<byte> buffer)
    {
        var kind = (ProjectProposalRequestKind)buffer[1];
        var nameLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[2..4]);
        var projectName = Encoding.UTF8.GetString(buffer[4..(4 + nameLength)]);
        var offset = 4 + nameLength;

        var wood = buffer[offset];
        var stone = buffer[offset + 1];
        var apple = buffer[offset + 2];
        var tool = buffer[offset + 3];
        offset += 4;

        var proposalId = 0;
        var voteChoice = ProjectVoteChoice.None;
        if (buffer.Length >= offset + ProjectProposalRequestTailSize)
        {
            proposalId = BinaryPrimitives.ReadInt32LittleEndian(buffer[offset..]);
            voteChoice = (ProjectVoteChoice)buffer[offset + 4];
        }

        return new ProjectProposalRequest(kind, projectName, wood, stone, apple, tool, proposalId, voteChoice);
    }

    public static int WriteProjectProposalResponse(Span<byte> buffer, ProjectProposalResponse response)
    {
        buffer[0] = (byte)PacketType.ProjectProposalResponse;
        buffer[1] = (byte)(response.Success ? 1 : 0);
        buffer[2] = (byte)response.Kind;
        buffer[3] = (byte)response.FailureReason;

        var messageBytes = Encoding.UTF8.GetBytes(response.Message);
        if (messageBytes.Length > MaxProjectProposalMessageBytes)
            messageBytes = messageBytes[..MaxProjectProposalMessageBytes];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[4..6], (ushort)messageBytes.Length);
        messageBytes.CopyTo(buffer[6..]);

        return ProjectProposalResponseHeaderSize + messageBytes.Length;
    }

    public static ProjectProposalResponse ReadProjectProposalResponse(ReadOnlySpan<byte> buffer)
    {
        var success = buffer[1] == 1;
        var kind = (ProjectProposalRequestKind)buffer[2];
        var failureReason = (ProjectProposalFailureReason)buffer[3];
        var messageLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[4..6]);
        var message = Encoding.UTF8.GetString(buffer[6..(6 + messageLength)]);

        return new ProjectProposalResponse(success, kind, failureReason, message);
    }

    public static int WriteProjectProposalNotification(Span<byte> buffer, string message)
    {
        buffer[0] = (byte)PacketType.ProjectProposalNotification;

        var messageBytes = Encoding.UTF8.GetBytes(message);
        if (messageBytes.Length > MaxProjectProposalMessageBytes)
            messageBytes = messageBytes[..MaxProjectProposalMessageBytes];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[1..3], (ushort)messageBytes.Length);
        messageBytes.CopyTo(buffer[3..]);

        return ProjectProposalNotificationHeaderSize + messageBytes.Length;
    }

    public static string ReadProjectProposalNotification(ReadOnlySpan<byte> buffer)
    {
        var messageLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[1..3]);
        return Encoding.UTF8.GetString(buffer[3..(3 + messageLength)]);
    }

    public const int VillagePositionRequestPacketSize = 9;
    public const int VillagePositionResponseHeaderSize = 6;
    public const int VillagePositionNotificationHeaderSize = 3;
    public const int MaxVillagePositionMessageBytes = 2000;

    public static int WriteVillagePositionRequest(Span<byte> buffer, VillagePositionRequest request)
    {
        buffer[0] = (byte)PacketType.VillagePositionRequest;
        buffer[1] = (byte)request.Kind;
        buffer[2] = (byte)request.Position;
        buffer[3] = (byte)request.VoteChoice;
        BinaryPrimitives.WriteInt32LittleEndian(buffer[4..8], request.ProposalId);
        buffer[8] = 0;
        return VillagePositionRequestPacketSize;
    }

    public static VillagePositionRequest ReadVillagePositionRequest(ReadOnlySpan<byte> buffer)
    {
        return new VillagePositionRequest(
            (VillagePositionRequestKind)buffer[1],
            (VillagePosition)buffer[2],
            (ProjectVoteChoice)buffer[3],
            BinaryPrimitives.ReadInt32LittleEndian(buffer[4..8]));
    }

    public static int WriteVillagePositionResponse(Span<byte> buffer, VillagePositionResponse response)
    {
        buffer[0] = (byte)PacketType.VillagePositionResponse;
        buffer[1] = (byte)(response.Success ? 1 : 0);
        buffer[2] = (byte)response.Kind;
        buffer[3] = (byte)response.FailureReason;

        var messageBytes = Encoding.UTF8.GetBytes(response.Message);
        if (messageBytes.Length > MaxVillagePositionMessageBytes)
            messageBytes = messageBytes[..MaxVillagePositionMessageBytes];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[4..6], (ushort)messageBytes.Length);
        messageBytes.CopyTo(buffer[6..]);

        return VillagePositionResponseHeaderSize + messageBytes.Length;
    }

    public static VillagePositionResponse ReadVillagePositionResponse(ReadOnlySpan<byte> buffer)
    {
        var success = buffer[1] == 1;
        var kind = (VillagePositionRequestKind)buffer[2];
        var failureReason = (VillagePositionFailureReason)buffer[3];
        var messageLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[4..6]);
        var message = Encoding.UTF8.GetString(buffer[6..(6 + messageLength)]);

        return new VillagePositionResponse(success, kind, failureReason, message);
    }

    public static int WriteVillagePositionNotification(Span<byte> buffer, string message)
    {
        buffer[0] = (byte)PacketType.VillagePositionNotification;

        var messageBytes = Encoding.UTF8.GetBytes(message);
        if (messageBytes.Length > MaxVillagePositionMessageBytes)
            messageBytes = messageBytes[..MaxVillagePositionMessageBytes];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[1..3], (ushort)messageBytes.Length);
        messageBytes.CopyTo(buffer[3..]);

        return VillagePositionNotificationHeaderSize + messageBytes.Length;
    }

    public static string ReadVillagePositionNotification(ReadOnlySpan<byte> buffer)
    {
        var messageLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[1..3]);
        return Encoding.UTF8.GetString(buffer[3..(3 + messageLength)]);
    }

    public const int HomeRequestPacketSize = 6;
    public const int HomeResponseHeaderSize = 6;
    public const int MaxHomeMessageBytes = 300;

    public static int WriteHomeRequest(Span<byte> buffer, HomeRequest request)
    {
        buffer[0] = (byte)PacketType.HomeRequest;
        buffer[1] = (byte)request.Kind;
        buffer[2] = (byte)request.ItemType;
        buffer[3] = request.Quantity;
        buffer[4] = (byte)request.FurnitureType;
        buffer[5] = (byte)request.ActivityType;
        return HomeRequestPacketSize;
    }

    public static HomeRequest ReadHomeRequest(ReadOnlySpan<byte> buffer)
    {
        return new HomeRequest(
            (HomeRequestKind)buffer[1],
            (ItemType)buffer[2],
            buffer[3],
            (FurnitureType)buffer[4],
            (HomeActivityType)buffer[5]);
    }

    public static int WriteHomeResponse(Span<byte> buffer, HomeResponse response)
    {
        buffer[0] = (byte)PacketType.HomeResponse;
        buffer[1] = (byte)(response.Success ? 1 : 0);
        buffer[2] = (byte)response.Kind;
        buffer[3] = (byte)response.FailureReason;

        var messageBytes = Encoding.UTF8.GetBytes(response.Message);
        if (messageBytes.Length > MaxHomeMessageBytes)
            messageBytes = messageBytes[..MaxHomeMessageBytes];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[4..6], (ushort)messageBytes.Length);
        messageBytes.CopyTo(buffer[6..]);

        return HomeResponseHeaderSize + messageBytes.Length;
    }

    public static HomeResponse ReadHomeResponse(ReadOnlySpan<byte> buffer)
    {
        var success = buffer[1] == 1;
        var kind = (HomeRequestKind)buffer[2];
        var failureReason = (HomeFailureReason)buffer[3];
        var messageLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[4..6]);
        var message = Encoding.UTF8.GetString(buffer[6..(6 + messageLength)]);

        return new HomeResponse(success, kind, failureReason, message);
    }

    public const int CraftingRequestPacketSize = 4;
    public const int CraftingResponseHeaderSize = 8;
    public const int MaxCraftingMessageBytes = 300;

    public static int WriteCraftingRequest(Span<byte> buffer, CraftingRequest request)
    {
        buffer[0] = (byte)PacketType.CraftingRequest;
        buffer[1] = (byte)request.RecipeId;
        buffer[2] = request.Quantity;
        buffer[3] = 0;
        return CraftingRequestPacketSize;
    }

    public static CraftingRequest ReadCraftingRequest(ReadOnlySpan<byte> buffer)
    {
        return new CraftingRequest((CraftingRecipeId)buffer[1], buffer[2]);
    }

    public static int WriteCraftingResponse(Span<byte> buffer, CraftingResponse response)
    {
        buffer[0] = (byte)PacketType.CraftingResponse;
        buffer[1] = (byte)(response.Success ? 1 : 0);
        buffer[2] = (byte)response.RecipeId;
        buffer[3] = response.Quantity;
        buffer[4] = (byte)response.FailureReason;
        buffer[5] = 0;

        var messageBytes = Encoding.UTF8.GetBytes(response.Message);
        if (messageBytes.Length > MaxCraftingMessageBytes)
            messageBytes = messageBytes[..MaxCraftingMessageBytes];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[6..8], (ushort)messageBytes.Length);
        messageBytes.CopyTo(buffer[8..]);

        return CraftingResponseHeaderSize + messageBytes.Length;
    }

    public static CraftingResponse ReadCraftingResponse(ReadOnlySpan<byte> buffer)
    {
        var success = buffer[1] == 1;
        var recipeId = (CraftingRecipeId)buffer[2];
        var quantity = buffer[3];
        var failureReason = (CraftingFailureReason)buffer[4];
        var messageLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[6..8]);
        var message = Encoding.UTF8.GetString(buffer[8..(8 + messageLength)]);

        return new CraftingResponse(success, recipeId, quantity, failureReason, message);
    }

    public const int GiftRequestPacketSize = 8;
    public const int GiftResponseHeaderSize = 19;
    public const int MaxGiftDialogueBytes = 250;
    public const int MaxGiftMessageBytes = 200;

    public static int WriteGiftRequest(Span<byte> buffer, GiftRequest request)
    {
        buffer[0] = (byte)PacketType.GiftRequest;
        buffer[1] = (byte)request.ItemType;
        buffer[2] = request.Quantity;
        buffer[3] = 0;
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[4..8], request.NpcEntityId);
        return GiftRequestPacketSize;
    }

    public static GiftRequest ReadGiftRequest(ReadOnlySpan<byte> buffer)
    {
        return new GiftRequest(
            (ItemType)buffer[1],
            buffer[2],
            BinaryPrimitives.ReadUInt32LittleEndian(buffer[4..8]));
    }

    public static int WriteGiftResponse(Span<byte> buffer, GiftResponse response)
    {
        buffer[0] = (byte)PacketType.GiftResponse;
        buffer[1] = (byte)(response.Success ? 1 : 0);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[2..6], response.NpcEntityId);
        buffer[6] = (byte)response.ItemType;
        buffer[7] = response.Quantity;
        BinaryPrimitives.WriteInt32LittleEndian(buffer[8..12], response.AffinityGained);
        BinaryPrimitives.WriteInt32LittleEndian(buffer[12..16], response.NewAffinity);
        buffer[16] = (byte)response.FailureReason;

        var dialogueBytes = Encoding.UTF8.GetBytes(response.NpcDialogue);
        if (dialogueBytes.Length > MaxGiftDialogueBytes)
            dialogueBytes = dialogueBytes[..MaxGiftDialogueBytes];

        var messageBytes = Encoding.UTF8.GetBytes(response.Message);
        if (messageBytes.Length > MaxGiftMessageBytes)
            messageBytes = messageBytes[..MaxGiftMessageBytes];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[17..19], (ushort)dialogueBytes.Length);
        dialogueBytes.CopyTo(buffer[19..]);

        var messageOffset = 19 + dialogueBytes.Length;
        BinaryPrimitives.WriteUInt16LittleEndian(buffer[messageOffset..(messageOffset + 2)], (ushort)messageBytes.Length);
        messageBytes.CopyTo(buffer[(messageOffset + 2)..]);

        return messageOffset + 2 + messageBytes.Length;
    }

    public static GiftResponse ReadGiftResponse(ReadOnlySpan<byte> buffer)
    {
        var success = buffer[1] == 1;
        var npcEntityId = BinaryPrimitives.ReadUInt32LittleEndian(buffer[2..6]);
        var itemType = (ItemType)buffer[6];
        var quantity = buffer[7];
        var affinityGained = BinaryPrimitives.ReadInt32LittleEndian(buffer[8..12]);
        var newAffinity = BinaryPrimitives.ReadInt32LittleEndian(buffer[12..16]);
        var failureReason = (GiftFailureReason)buffer[16];
        var dialogueLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[17..19]);
        var dialogue = Encoding.UTF8.GetString(buffer[19..(19 + dialogueLength)]);
        var messageOffset = 19 + dialogueLength;
        var messageLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[messageOffset..(messageOffset + 2)]);
        var message = Encoding.UTF8.GetString(buffer[(messageOffset + 2)..(messageOffset + 2 + messageLength)]);

        return new GiftResponse(success, npcEntityId, itemType, quantity, affinityGained, newAffinity,
            failureReason, dialogue, message);
    }

    public const int NpcAmbientCommentHeaderSize = 7;
    public const int MaxNpcAmbientCommentBytes = 300;

    public static int WriteNpcAmbientComment(Span<byte> buffer, uint npcEntityId, string message)
    {
        buffer[0] = (byte)PacketType.NpcAmbientComment;
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[1..5], npcEntityId);

        var messageBytes = Encoding.UTF8.GetBytes(message);
        if (messageBytes.Length > MaxNpcAmbientCommentBytes)
            messageBytes = messageBytes[..MaxNpcAmbientCommentBytes];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[5..7], (ushort)messageBytes.Length);
        messageBytes.CopyTo(buffer[7..]);

        return NpcAmbientCommentHeaderSize + messageBytes.Length;
    }

    public static (uint NpcEntityId, string Message) ReadNpcAmbientComment(ReadOnlySpan<byte> buffer)
    {
        var npcEntityId = BinaryPrimitives.ReadUInt32LittleEndian(buffer[1..5]);
        var messageLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[5..7]);
        var message = Encoding.UTF8.GetString(buffer[7..(7 + messageLength)]);
        return (npcEntityId, message);
    }

    public const int VillageAreaRequestPacketSize = 5;
    public const int VillageAreaResponseHeaderSize = 6;
    public const int VillageAreaNotificationHeaderSize = 3;
    public const int MaxVillageAreaMessageBytes = 1200;

    public static int WriteVillageAreaRequest(Span<byte> buffer, VillageAreaRequest request)
    {
        buffer[0] = (byte)PacketType.VillageAreaRequest;
        buffer[1] = (byte)request.Kind;
        buffer[2] = (byte)request.Interaction;
        buffer[3] = 0;
        buffer[4] = 0;
        return VillageAreaRequestPacketSize;
    }

    public static VillageAreaRequest ReadVillageAreaRequest(ReadOnlySpan<byte> buffer)
    {
        return new VillageAreaRequest(
            (VillageAreaRequestKind)buffer[1],
            (Village.VillageAreaInteractionKind)buffer[2]);
    }

    public static int WriteVillageAreaResponse(Span<byte> buffer, VillageAreaResponse response)
    {
        buffer[0] = (byte)PacketType.VillageAreaResponse;
        buffer[1] = (byte)(response.Success ? 1 : 0);
        buffer[2] = (byte)response.Kind;
        buffer[3] = (byte)response.FailureReason;

        var messageBytes = Encoding.UTF8.GetBytes(response.Message);
        if (messageBytes.Length > MaxVillageAreaMessageBytes)
            messageBytes = messageBytes[..MaxVillageAreaMessageBytes];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[4..6], (ushort)messageBytes.Length);
        messageBytes.CopyTo(buffer[6..]);

        return VillageAreaResponseHeaderSize + messageBytes.Length;
    }

    public static VillageAreaResponse ReadVillageAreaResponse(ReadOnlySpan<byte> buffer)
    {
        var success = buffer[1] == 1;
        var kind = (VillageAreaRequestKind)buffer[2];
        var failureReason = (VillageAreaFailureReason)buffer[3];
        var messageLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[4..6]);
        var message = Encoding.UTF8.GetString(buffer[6..(6 + messageLength)]);

        return new VillageAreaResponse(success, kind, failureReason, message);
    }

    public static int WriteVillageAreaNotification(Span<byte> buffer, string message)
    {
        buffer[0] = (byte)PacketType.VillageAreaNotification;

        var messageBytes = Encoding.UTF8.GetBytes(message);
        if (messageBytes.Length > MaxVillageAreaMessageBytes)
            messageBytes = messageBytes[..MaxVillageAreaMessageBytes];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[1..3], (ushort)messageBytes.Length);
        messageBytes.CopyTo(buffer[3..]);

        return VillageAreaNotificationHeaderSize + messageBytes.Length;
    }

    public static string ReadVillageAreaNotification(ReadOnlySpan<byte> buffer)
    {
        var messageLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[1..3]);
        return Encoding.UTF8.GetString(buffer[3..(3 + messageLength)]);
    }

    public const int PersonalRoutineRequestPacketSize = 5;
    public const int PersonalRoutineResponseHeaderSize = 6;
    public const int MaxPersonalRoutineMessageBytes = 1200;

    public static int WritePersonalRoutineRequest(Span<byte> buffer, PersonalRoutineRequest request)
    {
        buffer[0] = (byte)PacketType.PersonalRoutineRequest;
        buffer[1] = (byte)request.Kind;
        buffer[2] = (byte)request.Routine;
        buffer[3] = 0;
        buffer[4] = 0;
        return PersonalRoutineRequestPacketSize;
    }

    public static PersonalRoutineRequest ReadPersonalRoutineRequest(ReadOnlySpan<byte> buffer)
    {
        return new PersonalRoutineRequest(
            (PersonalRoutineRequestKind)buffer[1],
            (Routines.PersonalRoutineKind)buffer[2]);
    }

    public static int WritePersonalRoutineResponse(Span<byte> buffer, PersonalRoutineResponse response)
    {
        buffer[0] = (byte)PacketType.PersonalRoutineResponse;
        buffer[1] = (byte)(response.Success ? 1 : 0);
        buffer[2] = (byte)response.Kind;
        buffer[3] = (byte)response.FailureReason;

        var messageBytes = Encoding.UTF8.GetBytes(response.Message);
        if (messageBytes.Length > MaxPersonalRoutineMessageBytes)
            messageBytes = messageBytes[..MaxPersonalRoutineMessageBytes];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[4..6], (ushort)messageBytes.Length);
        messageBytes.CopyTo(buffer[6..]);

        return PersonalRoutineResponseHeaderSize + messageBytes.Length;
    }

    public static PersonalRoutineResponse ReadPersonalRoutineResponse(ReadOnlySpan<byte> buffer)
    {
        var success = buffer[1] == 1;
        var kind = (PersonalRoutineRequestKind)buffer[2];
        var failureReason = (PersonalRoutineFailureReason)buffer[3];
        var messageLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[4..6]);
        var message = Encoding.UTF8.GetString(buffer[6..(6 + messageLength)]);

        return new PersonalRoutineResponse(success, kind, failureReason, message);
    }

    public const int CommunityActivityRequestPacketSize = 5;
    public const int CommunityActivityResponseHeaderSize = 6;
    public const int MaxCommunityActivityMessageBytes = 1200;

    public static int WriteCommunityActivityRequest(Span<byte> buffer, CommunityActivityRequest request)
    {
        buffer[0] = (byte)PacketType.CommunityActivityRequest;
        buffer[1] = (byte)request.Kind;
        buffer[2] = (byte)request.Activity;
        buffer[3] = 0;
        buffer[4] = 0;
        return CommunityActivityRequestPacketSize;
    }

    public static CommunityActivityRequest ReadCommunityActivityRequest(ReadOnlySpan<byte> buffer)
    {
        return new CommunityActivityRequest(
            (CommunityActivityRequestKind)buffer[1],
            (Community.CommunityActivityKind)buffer[2]);
    }

    public static int WriteCommunityActivityResponse(Span<byte> buffer, CommunityActivityResponse response)
    {
        buffer[0] = (byte)PacketType.CommunityActivityResponse;
        buffer[1] = (byte)(response.Success ? 1 : 0);
        buffer[2] = (byte)response.Kind;
        buffer[3] = (byte)response.FailureReason;

        var messageBytes = Encoding.UTF8.GetBytes(response.Message);
        if (messageBytes.Length > MaxCommunityActivityMessageBytes)
            messageBytes = messageBytes[..MaxCommunityActivityMessageBytes];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[4..6], (ushort)messageBytes.Length);
        messageBytes.CopyTo(buffer[6..]);

        return CommunityActivityResponseHeaderSize + messageBytes.Length;
    }

    public static CommunityActivityResponse ReadCommunityActivityResponse(ReadOnlySpan<byte> buffer)
    {
        var success = buffer[1] == 1;
        var kind = (CommunityActivityRequestKind)buffer[2];
        var failureReason = (CommunityActivityFailureReason)buffer[3];
        var messageLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[4..6]);
        var message = Encoding.UTF8.GetString(buffer[6..(6 + messageLength)]);

        return new CommunityActivityResponse(success, kind, failureReason, message);
    }

    public const int DailyVillageActivityRequestPacketSize = 5;
    public const int DailyVillageActivityResponseHeaderSize = 6;
    public const int MaxDailyVillageActivityMessageBytes = 1200;

    public static int WriteDailyVillageActivityRequest(Span<byte> buffer, DailyVillageActivityRequest request)
    {
        buffer[0] = (byte)PacketType.DailyVillageActivityRequest;
        buffer[1] = (byte)request.Kind;
        buffer[2] = (byte)request.Activity;
        buffer[3] = 0;
        buffer[4] = 0;
        return DailyVillageActivityRequestPacketSize;
    }

    public static DailyVillageActivityRequest ReadDailyVillageActivityRequest(ReadOnlySpan<byte> buffer)
    {
        return new DailyVillageActivityRequest(
            (DailyVillageActivityRequestKind)buffer[1],
            (Village.DailyVillageActivityKind)buffer[2]);
    }

    public static int WriteDailyVillageActivityResponse(Span<byte> buffer, DailyVillageActivityResponse response)
    {
        buffer[0] = (byte)PacketType.DailyVillageActivityResponse;
        buffer[1] = (byte)(response.Success ? 1 : 0);
        buffer[2] = (byte)response.Kind;
        buffer[3] = (byte)response.FailureReason;

        var messageBytes = Encoding.UTF8.GetBytes(response.Message);
        if (messageBytes.Length > MaxDailyVillageActivityMessageBytes)
            messageBytes = messageBytes[..MaxDailyVillageActivityMessageBytes];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[4..6], (ushort)messageBytes.Length);
        messageBytes.CopyTo(buffer[6..]);

        return DailyVillageActivityResponseHeaderSize + messageBytes.Length;
    }

    public static DailyVillageActivityResponse ReadDailyVillageActivityResponse(ReadOnlySpan<byte> buffer)
    {
        var success = buffer[1] == 1;
        var kind = (DailyVillageActivityRequestKind)buffer[2];
        var failureReason = (DailyVillageActivityFailureReason)buffer[3];
        var messageLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[4..6]);
        var message = Encoding.UTF8.GetString(buffer[6..(6 + messageLength)]);

        return new DailyVillageActivityResponse(success, kind, failureReason, message);
    }

    public const int DailyRhythmRequestPacketSize = 5;
    public const int DailyRhythmResponseHeaderSize = 6;
    public const int MaxDailyRhythmMessageBytes = 1400;

    public static int WriteDailyRhythmRequest(Span<byte> buffer, DailyRhythmRequest request)
    {
        buffer[0] = (byte)PacketType.DailyRhythmRequest;
        buffer[1] = (byte)request.Kind;
        buffer[2] = 0;
        buffer[3] = 0;
        buffer[4] = 0;
        return DailyRhythmRequestPacketSize;
    }

    public static DailyRhythmRequest ReadDailyRhythmRequest(ReadOnlySpan<byte> buffer)
    {
        return new DailyRhythmRequest((DailyRhythmRequestKind)buffer[1]);
    }

    public static int WriteDailyRhythmResponse(Span<byte> buffer, DailyRhythmResponse response)
    {
        buffer[0] = (byte)PacketType.DailyRhythmResponse;
        buffer[1] = (byte)(response.Success ? 1 : 0);
        buffer[2] = (byte)response.Kind;
        buffer[3] = (byte)response.FailureReason;

        var messageBytes = Encoding.UTF8.GetBytes(response.Message);
        if (messageBytes.Length > MaxDailyRhythmMessageBytes)
            messageBytes = messageBytes[..MaxDailyRhythmMessageBytes];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[4..6], (ushort)messageBytes.Length);
        messageBytes.CopyTo(buffer[6..]);

        return DailyRhythmResponseHeaderSize + messageBytes.Length;
    }

    public static DailyRhythmResponse ReadDailyRhythmResponse(ReadOnlySpan<byte> buffer)
    {
        var success = buffer[1] == 1;
        var kind = (DailyRhythmRequestKind)buffer[2];
        var failureReason = (DailyRhythmFailureReason)buffer[3];
        var messageLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[4..6]);
        var message = Encoding.UTF8.GetString(buffer[6..(6 + messageLength)]);

        return new DailyRhythmResponse(success, kind, failureReason, message);
    }

    public const int LegacyFocusRequestPacketSize = 5;
    public const int LegacyFocusResponseHeaderSize = 6;
    public const int MaxLegacyFocusMessageBytes = 1200;

    public static int WriteLegacyFocusRequest(Span<byte> buffer, LegacyFocusRequest request)
    {
        buffer[0] = (byte)PacketType.LegacyFocusRequest;
        buffer[1] = (byte)request.Kind;
        buffer[2] = (byte)request.Path;
        buffer[3] = 0;
        buffer[4] = 0;
        return LegacyFocusRequestPacketSize;
    }

    public static LegacyFocusRequest ReadLegacyFocusRequest(ReadOnlySpan<byte> buffer)
    {
        return new LegacyFocusRequest(
            (LegacyFocusRequestKind)buffer[1],
            (Goals.LegacyArchetype)buffer[2]);
    }

    public static int WriteLegacyFocusResponse(Span<byte> buffer, LegacyFocusResponse response)
    {
        buffer[0] = (byte)PacketType.LegacyFocusResponse;
        buffer[1] = (byte)(response.Success ? 1 : 0);
        buffer[2] = (byte)response.Kind;
        buffer[3] = (byte)response.FailureReason;

        var messageBytes = Encoding.UTF8.GetBytes(response.Message);
        if (messageBytes.Length > MaxLegacyFocusMessageBytes)
            messageBytes = messageBytes[..MaxLegacyFocusMessageBytes];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[4..6], (ushort)messageBytes.Length);
        messageBytes.CopyTo(buffer[6..]);

        return LegacyFocusResponseHeaderSize + messageBytes.Length;
    }

    public static LegacyFocusResponse ReadLegacyFocusResponse(ReadOnlySpan<byte> buffer)
    {
        var success = buffer[1] == 1;
        var kind = (LegacyFocusRequestKind)buffer[2];
        var failureReason = (LegacyFocusFailureReason)buffer[3];
        var messageLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[4..6]);
        var message = Encoding.UTF8.GetString(buffer[6..(6 + messageLength)]);

        return new LegacyFocusResponse(success, kind, failureReason, message);
    }

    public const int EmotionalBondRequestPacketSize = 8;
    public const int EmotionalBondResponseHeaderSize = 7;
    public const int MaxEmotionalBondMessageBytes = 1200;

    public static int WriteEmotionalBondRequest(Span<byte> buffer, EmotionalBondRequest request)
    {
        buffer[0] = (byte)PacketType.EmotionalBondRequest;
        buffer[1] = (byte)request.Kind;
        buffer[2] = (byte)request.Action;
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[3..7], request.TargetNpcEntityId);
        buffer[7] = 0;
        return EmotionalBondRequestPacketSize;
    }

    public static EmotionalBondRequest ReadEmotionalBondRequest(ReadOnlySpan<byte> buffer)
    {
        return new EmotionalBondRequest(
            (EmotionalBondRequestKind)buffer[1],
            (EmotionalBondActionKind)buffer[2],
            BinaryPrimitives.ReadUInt32LittleEndian(buffer[3..7]));
    }

    public static int WriteEmotionalBondResponse(Span<byte> buffer, EmotionalBondResponse response)
    {
        buffer[0] = (byte)PacketType.EmotionalBondResponse;
        buffer[1] = (byte)(response.Success ? 1 : 0);
        buffer[2] = (byte)response.Kind;
        buffer[3] = (byte)response.Action;
        buffer[4] = (byte)response.FailureReason;

        var messageBytes = Encoding.UTF8.GetBytes(response.Message);
        if (messageBytes.Length > MaxEmotionalBondMessageBytes)
            messageBytes = messageBytes[..MaxEmotionalBondMessageBytes];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[5..7], (ushort)messageBytes.Length);
        messageBytes.CopyTo(buffer[7..]);

        return EmotionalBondResponseHeaderSize + messageBytes.Length;
    }

    public static EmotionalBondResponse ReadEmotionalBondResponse(ReadOnlySpan<byte> buffer)
    {
        var success = buffer[1] == 1;
        var kind = (EmotionalBondRequestKind)buffer[2];
        var action = (EmotionalBondActionKind)buffer[3];
        var failureReason = (EmotionalBondFailureReason)buffer[4];
        var messageLength = BinaryPrimitives.ReadUInt16LittleEndian(buffer[5..7]);
        var message = Encoding.UTF8.GetString(buffer[7..(7 + messageLength)]);

        return new EmotionalBondResponse(success, kind, action, failureReason, message);
    }
}
