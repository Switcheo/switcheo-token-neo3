

using Neo;
using Neo.SmartContract;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using Neo.SmartContract.Framework.Attributes;
using System;
using System.ComponentModel;
using System.Numerics;

namespace SwitcheoToken
{
    [DisplayName("Switcheo Token")]
    [ManifestExtra("Author", "Switcheo Labs")]
    [ManifestExtra("Email", "engineering@switcheo.network")]
    [ManifestExtra("Description", "This is the NEP-17 contract for Switcheo Token on Neo 3.0")]
    [SupportedStandards("NEP-17")]
    [ContractPermission("*")]
    public class SwitcheoToken : SmartContract
    {
        #region Notifications

        [DisplayName("Transfer")]
        public static event Action<UInt160, UInt160, BigInteger> OnTransfer;

        [DisplayName("Notify")]
        public static event Action<string, object, object> Notify;

        #endregion

        //initial operator
        [InitialValue("NYxb4fSZVKAz8YsgaPK2WkT3KcAE9b3Vag", ContractParameterType.Hash160)]
        private static readonly UInt160 Owner = default;
        private static readonly byte[] SupplyKey = "supply".ToByteArray();
        private static readonly byte[] OwnerKey = "owner".ToByteArray();
        private static readonly byte[] PendingOwnerKey = "pendingOwner".ToByteArray();
        private static readonly byte[] BalancePrefix = new byte[] { 0x01, 0x01 };
        private static readonly byte[] ContractPrefix = new byte[] { 0x01, 0x02 };
        private static readonly byte[] BridgePrefix = new byte[] { 0x01, 0x03 };

        public static readonly StorageMap BalanceMap = new StorageMap(Storage.CurrentContext, BalancePrefix);
        public static readonly StorageMap ContractMap = new StorageMap(Storage.CurrentContext, ContractPrefix);
        public static readonly StorageMap BridgeMap = new StorageMap(Storage.CurrentContext, BridgePrefix);

        public static void _deploy(object data, bool update)
        {
            if (update) return;
            ContractMap.Put(OwnerKey, Owner);
        }

        public static UInt160 GetOwner() => (UInt160)ContractMap.Get(OwnerKey);

        public static UInt160 GetPendingOwner() => (UInt160)ContractMap.Get(PendingOwnerKey);

        // When this contract address is included in the transaction signature,
        // this method will be triggered as a VerificationTrigger to verify that the signature is correct.
        // For example, this method needs to be called when withdrawing token from the contract.
        public static bool Verify() => IsOwner();

        #region Nep-17 Methods

        public static string Symbol() => "SWTH";

        public static byte Decimals() => 8;

        public static BigInteger BalanceOf(UInt160 address) => (BigInteger)BalanceMap.Get(address);

        public static BigInteger TotalSupply() => (BigInteger)ContractMap.Get(SupplyKey);

        public static bool Transfer(UInt160 from, UInt160 to, BigInteger amount, object data = null)
        {
            Assert(!from.IsValid, "The from argument is invalid.");
            Assert(to is null || !to.IsValid, "The to argument is invalid.");
            Assert(amount >= 0, "The amount argument must be a positive number.");
            Assert(Runtime.CheckWitness(from) || from.Equals(Runtime.CallingScriptHash), "No authorization.");

            if (amount == 0 || from == to) {
              PostTransfer(from, to, amount, data);
              return true;
            }

            if (IsBridge(from)) {
              Mint(from, amount);
            }

            IncreaseBalance(from, -amount);
            IncreaseBalance(to, +amount);

            PostTransfer(from, to, amount, data);

            if (IsBridge(to)) {
              Burn(to, amount);
            }

            return true;
        }

        #endregion

        private static bool IsOwner() => Runtime.CheckWitness(GetOwner());

        private static bool IsPendingOwner() => Runtime.CheckWitness(GetPendingOwner());

        private static bool IsBridge(UInt160 key) => (string)BridgeMap.Get(key) == "1";

        private static void IncreaseSupply(BigInteger value)
        {
          var supply = TotalSupply() + value;
          Assert(supply >= 0, "Negative supply.");
          ContractMap.Put(SupplyKey, supply);
        }

        private static void IncreaseBalance(UInt160 key, BigInteger value)
        {
          var balance = BalanceOf(key) + value;
          Assert(balance >= 0, "Insufficient balance.");
          if (balance == 0)
            BalanceMap.Delete(key);
          else
            BalanceMap.Put(key, balance);
        }

        private static void PostTransfer(UInt160 from, UInt160 to, BigInteger amount, object data)
        {
            OnTransfer(from, to, amount);
            if (to is not null && ContractManagement.GetContract(to) is not null)
                Contract.Call(to, "onNEP17Payment", CallFlags.All, from, amount, data);
        }

        private static void Mint(UInt160 address, BigInteger amount)
        {
            Assert(address.IsValid, "The address is invalid.");
            Assert(IsBridge(address), "No authorization.");

            IncreaseSupply(amount);
            IncreaseBalance(address, amount);

            OnTransfer(null, address, amount);
        }

        private static void Burn(UInt160 address, BigInteger amount)
        {
            Assert(address.IsValid, "The address is invalid.");
            Assert(IsBridge(address), "No authorization.");

            IncreaseSupply(-amount);
            IncreaseBalance(address, -amount);

            OnTransfer(address, null, amount);
        }

        #region Operator Methods

        public static bool AddBridge(UInt160 address)
        {
            Assert(address.IsValid, "The address is invalid.");
            Assert(IsOwner(), "No authorization.");

            BridgeMap.Put(address, "1");
            return true;
        }

        public static bool RemoveBridge(UInt160 address)
        {
            Assert(address.IsValid, "The address is invalid.");
            Assert(IsOwner(), "No authorization.");
            Assert(IsBridge(address), "The address is not a bridge.");

            BridgeMap.Delete(address);
            return true;
        }

        public static bool InitiateOwnershipTransfer(UInt160 newOwner)
        {
            Assert(newOwner.IsValid, "The new owner address is invalid.");
            Assert(IsOwner(), "No authorization.");

            ContractMap.Put(PendingOwnerKey, newOwner);
            return true;
        }

        public static bool AcceptOwnershipTransfer()
        {
            Assert(IsPendingOwner(), "No authorization.");

            ContractMap.Put(OwnerKey, GetPendingOwner());
            return true;
        }

        public static void Update(ByteString nefFile, string manifest)
        {
            Assert(IsOwner(), "No authorization.");

            ContractManagement.Update(nefFile, manifest);
        }

        #endregion

        private static void Assert(bool condition, string msg, object result = null, string errorType = "Error")
        {
            if (!condition)
            {
                Notify(errorType, result, msg);
                throw new InvalidOperationException(msg);
            }
        }
    }
}