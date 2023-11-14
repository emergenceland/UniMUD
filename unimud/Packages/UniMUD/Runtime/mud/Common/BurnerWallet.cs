using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using UnityEngine;
using Nethereum.Web3.Accounts;

namespace mud
{
    public static partial class Common
    {
        public static string GeneratePrivateKey()
        {
            var ecKey = EthECKey.GenerateKey();
            return ecKey.GetPrivateKeyAsBytes().ToHex();
        }

        public static string GetBurnerPrivateKey()
        {
            {
                var savedBurnerWallet = PlayerPrefs.GetString("burnerWallet");
                if (!string.IsNullOrWhiteSpace(savedBurnerWallet)) {
                    return savedBurnerWallet;
                }

                return GeneratePrivateKey();
           
            }
        }

        public static string GetBurnerAddress() {
            return PlayerPrefs.GetString("burnerAddress");
        }

        public static Account CreateAndSaveAccount(string newPrivateKey, int chainID) {  
            Account newAccount = new Account(newPrivateKey, chainID);
            PlayerPrefs.SetString("burnerWallet", newPrivateKey);
            PlayerPrefs.SetString("burnerAddress", newAccount.Address);
            PlayerPrefs.Save();
            return newAccount;
        }


    }
}
