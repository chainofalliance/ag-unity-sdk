using Chromia;
using System.Linq;
using System.Threading.Tasks;

namespace AllianceGamesSdk.Common
{
    public class AllianceGamesBlockchain
    {
        public enum Target
        {
            Local,
            Devnet,
            Testnet,
            Mainnet
        }

        public struct Config
        {
            public string[] NodeUrls;
            public string Brid;
            public int ChainId;

            public static Config Get(Target target)
            {
                return target switch
                {
                    Target.Local => new() { ChainId = 0, NodeUrls = new[] { "http://localhost:7740/" } },
                    Target.Devnet => new() { ChainId = 1, NodeUrls = new[] { "http://localhost:7740/" } },
                    Target.Testnet => new()
                    {
                        Brid = "4FC7F780620D35B0BAE620DA69DC1476AA676AE7F11A640C65D88127EFFAA08B",
                        NodeUrls = new[]
                        {
                            "https://node1.testnet.chromia.com:7740/",
                            "https://node2.testnet.chromia.com:7740/",
                            "https://node3.testnet.chromia.com:7740/"
                        }
                    },
                    _ => throw new System.NotImplementedException()
                };
            }
        }

        public static async Task<ChromiaClient> Get(Target target)
        {
            var config = Config.Get(target);
            if (!string.IsNullOrEmpty(config.Brid))
            {
                return await ChromiaClient.Create(config.NodeUrls.ToList(), Buffer.From(config.Brid));
            }
            else
            {
                return await ChromiaClient.Create(config.NodeUrls.ToList(), config.ChainId);
            }
        }
    }
}