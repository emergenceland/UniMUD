namespace mud
{
    public class NetworkTypes
    {
        public enum NetworkType {Local,Testnet,Mainnet}
        public class LocalDeploy
        {
            public string worldAddress;
            public int blockNumber;
        }
    }
}
