using AllianceGamesSdk.Matchmaking.Models;
using Chromia;
using Chromia.Encoding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Buffer = Chromia.Buffer;

#if ENABLE_IL2CPP
using Newtonsoft.Json.Utilities;
using UnityEngine;
#endif

namespace AllianceGamesSdk.Matchmaking
{
    public static class MatchmakingServiceFactory
    {
#if ENABLE_IL2CPP
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void EnsureAotTypes()
        {
            AotHelper.EnsureType<GetMatchmakingTicketStatusResult>();
            AotHelper.EnsureType<TransactionReceipt>();
        }
#endif

        public static IMatchmakingService Get(
            ChromiaClient chromiaClient
        )
        {
            return new MatchmakingService(chromiaClient);
        }

        public static async Task<IMatchmakingService> Get(
            Common.AllianceGamesBlockchain.Target target
        )
        {
            var chromiaClient = await Common.AllianceGamesBlockchain.Get(target);
            return new MatchmakingService(chromiaClient);
        }
    }

    public interface IMatchmakingService
    {
        Task<TransactionReceipt> CreateMatchmakingTicket(
            CreateMatchmakingTicketRequest request,
            CancellationToken ct
        );

        Task<string> GetMatchmakingTicket(
            GetMatchmakingTicketRequest request,
            CancellationToken ct
        );

        Task<GetMatchmakingTicketStatusResult> GetMatchmakingTicketStatus(
            GetMatchmakingTicketStatusRequest request,
            CancellationToken ct
        );

        Task<GetConnectionDetailsResponse> GetConnectionDetails(
            GetConnectionDetailsRequest request,
            CancellationToken ct
        );

        Task<int> GetAmountTicketsInQueue(
            GetAmountTicketsInQueueRequest request,
            CancellationToken ct
        );

        Task<TransactionReceipt> CancelMatchmakingTicket(
            CancelMatchmakingTicketRequest request,
            CancellationToken ct
        );

        Task<TransactionReceipt> CancelAllMatchmakingTicketsForPlayer(
            CancelAllMatchmakingTicketRequests request,
            CancellationToken ct
        );
    }

    public enum MatchmakingTicketState
    {
        Open,
        WaitingForServer,
        Matched,
        Closed,
        Canceled
    }

    public class MatchmakingService : IMatchmakingService
    {
        private readonly ChromiaClient chromiaClient;

        public MatchmakingService(
            ChromiaClient chromiaClient
        )
        {
            this.chromiaClient = chromiaClient;
        }

        public async Task<TransactionReceipt> CreateMatchmakingTicket(
            CreateMatchmakingTicketRequest request,
            CancellationToken ct
        )
        {
            return await chromiaClient.SendUniqueTransaction(
                new Operation("ag.IMatchmaking.create_ticket", request), ct: ct);
        }

        public async Task<string> GetMatchmakingTicket(
            GetMatchmakingTicketRequest request,
            CancellationToken ct
        )
        {
            return await chromiaClient.Query<string>(
                "ag.IMatchmaking.get_ticket_id",
                ("par", new object[] { request.Identifier, request.Duid, request.QueueName })
            );
        }

        public async Task<int> GetAmountTicketsInQueue(
            GetAmountTicketsInQueueRequest request,
            CancellationToken ct
        )
        {
            return await chromiaClient.Query<int>(
                "ag.IMatchmaking.get_amount_tickets_in_queue",
                ("par", new object[] { request.Duid, request.QueueName })
            );
        }

        public async Task<GetMatchmakingTicketStatusResult> GetMatchmakingTicketStatus(
            GetMatchmakingTicketStatusRequest request,
            CancellationToken ct
        )
        {
            return await chromiaClient.Query<GetMatchmakingTicketStatusResult>(
                "ag.IMatchmaking.get_ticket_status",
                ct,
                ("par", new object[] { request.TicketId })
            );
        }

        public async Task<GetConnectionDetailsResponse> GetConnectionDetails(
            GetConnectionDetailsRequest request,
            CancellationToken ct
        )
        {
            return await chromiaClient.Query<GetConnectionDetailsResponse>(
                "ag.ISession.get_connection_details",
                ct,
                ("session_id", request.SessionId)
            );
        }

        public async Task<TransactionReceipt> CancelMatchmakingTicket(
            CancelMatchmakingTicketRequest request,
            CancellationToken ct
        )
        {
            return await chromiaClient.SendUniqueTransaction(
                new Operation("ag.IMatchmaking.cancel_ticket", request), ct: ct);
        }

        public async Task<TransactionReceipt> CancelAllMatchmakingTicketsForPlayer(
            CancelAllMatchmakingTicketRequests request,
            CancellationToken ct
        )
        {
            return await chromiaClient.SendUniqueTransaction(
                new Operation("ag.IMatchmaking.cancel_all_tickets", request), ct: ct);
        }

        public static async Task<string> GetDuid(
            ChromiaClient chromiaClient,
            string displayName,
            CancellationToken ct
        )
        {
            return await chromiaClient.Query<string>(
                "ag.IDappProvider.get_uid",
                ct,
                ("display_name", displayName)
            );
        }
    }

    namespace Models
    {
        [PostchainSerializable]
        public class CreateMatchmakingTicketRequest
        {
            [PostchainProperty("identifier", 0)]
            public Buffer Identifier;
            [PostchainProperty("network_signer", 1)]
            public Buffer NetworkSigner;
            [PostchainProperty("duid", 2)]
            public string Duid;
            [PostchainProperty("queue_name", 3)]
            public string QueueName;
            [PostchainProperty("match_data", 4)]
            public string MatchData = "[]";
            [PostchainProperty("attributes", 5)]
            public Dictionary<string, object> Attributes = new Dictionary<string, object>();

            [JsonConstructor]
            public CreateMatchmakingTicketRequest() { }
        }

        [PostchainSerializable]
        public class GetMatchmakingTicketRequest
        {
            [PostchainProperty("identifier", 0)]
            public Buffer Identifier;
            [PostchainProperty("duid", 1)]
            public string Duid;
            [PostchainProperty("queue_name", 2)]
            public string QueueName;

            [JsonConstructor]
            public GetMatchmakingTicketRequest() { }
        }

        [PostchainSerializable]
        public class GetAmountTicketsInQueueRequest
        {
            [PostchainProperty("duid", 0)]
            public string Duid;
            [PostchainProperty("queue_name", 1)]
            public string QueueName;

            [JsonConstructor]
            public GetAmountTicketsInQueueRequest() { }
        }

        [PostchainSerializable]
        public class GetMatchmakingTicketStatusRequest
        {
            [PostchainProperty("ticket_id", 0)]
            public string TicketId;

            [JsonConstructor]
            public GetMatchmakingTicketStatusRequest() { }
        }

        [PostchainSerializable]
        public class GetMatchmakingTicketStatusResult
        {
            [PostchainProperty("ticket_id", 0)]
            public string TicketId;
            [PostchainProperty("queue_name", 1)]
            public string QueueName;
            [PostchainProperty("created_at", 2)]
            public long CreatedAtTimestamp;
            public DateTime CreatedAt => DateTimeOffset.FromUnixTimeMilliseconds(CreatedAtTimestamp).DateTime;
            [PostchainProperty("status", 3)]
            public MatchmakingTicketState Status;
            [PostchainProperty("give_up_after_seconds", 4)]
            public int GiveUpAfterSeconds;
            [PostchainProperty("identifier", 5)]
            public Buffer Identifier;
            [PostchainProperty("session_id", 6)]
            public string SessionId;

            [JsonConstructor]
            public GetMatchmakingTicketStatusResult() { }
        }

        [PostchainSerializable]
        public class GetConnectionDetailsRequest
        {
            [PostchainProperty("session_id", 0)]
            public string SessionId;

            [JsonConstructor]
            public GetConnectionDetailsRequest() { }
        }

        [PostchainSerializable]
        public class GetConnectionDetailsResponse
        {
            [PostchainProperty("url", 0)]
            public string CoordinatorUrl;
            [PostchainProperty("address", 1)]
            public Buffer CoordinatorPubkey;

            [JsonConstructor]
            public GetConnectionDetailsResponse() { }
        }

        [PostchainSerializable]
        public class CancelMatchmakingTicketRequest
        {
            [PostchainProperty("identifier", 0)]
            public Buffer Identifier;
            [PostchainProperty("ticket_id", 1)]
            public string TicketId;

            [JsonConstructor]
            public CancelMatchmakingTicketRequest() { }
        }

        [PostchainSerializable]
        public class CancelAllMatchmakingTicketRequests
        {
            [PostchainProperty("identifier", 0)]
            public Buffer Identifier;
            [PostchainProperty("duid", 1)]
            public string Duid;

            [JsonConstructor]
            public CancelAllMatchmakingTicketRequests() { }
        }

        public enum TicketStatus
        {
            [EnumMember(Value = "open")]
            open,
            [EnumMember(Value = "canceled")]
            canceled,
            [EnumMember(Value = "matched")]
            matched
        }
    }
}