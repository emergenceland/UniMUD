#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using mud.IStore.ContractDefinition;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace mud
{
    public static partial class Common
    {
        public static IObservable<T> AsyncEnumerableToObservable<T>(IAsyncEnumerable<T> source)
        {
            return Observable.Create<T>(observer =>
            {
                var _ = IterateAsync(observer, source);
                return Disposable.Empty;
            }, true);
        }

        private static async UniTask IterateAsync<T>(IObserver<T> observer, IAsyncEnumerable<T> source)
        {
            try
            {
                await foreach (var item in source)
                {
                    observer.OnNext(item);
                }

                observer.OnCompleted();
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }
        }

        public enum PadDirection
        {
            Left,
            Right
        }

        public static string Pad(string input, PadDirection direction, int size)
        {
            int paddingSize = size * 2 - input.Length;
            if (paddingSize <= 0) return input;

            string padding = new string('0', paddingSize);
            return direction == PadDirection.Left ? padding + input : input + padding;
        }

        public static List<string> FormatGetRecordResult(string input)
        {
            if (input.All(c => c == '\u0000')) return new List<string> { string.Empty };

            if (input.Length == 0) return new List<string> { string.Empty };

            var parts = Regex.Split(input, @"[\x00-\x1F]|[\uFFFD]|[\s@`]")
                .Where(part => !string.IsNullOrWhiteSpace(part)).ToList();

            return parts.Count > 0 ? parts : new List<string> { string.Empty };
        }

        public static Dictionary<string, object> CreateProperty(
            params (string Key, object Value)[] keyValuePairs
        )
        {
            return keyValuePairs.ToDictionary(
                keyValuePair => keyValuePair.Key,
                keyValuePair => keyValuePair.Value
            );
        }

        public static void LoadConfig(Dictionary<string, ProtocolParser.Table> tables, RxDatastore ds)
        {
            foreach (var (_, table) in tables)
            {
                ds.RegisterTable(table.TableId, table.Name, table.ValueSchema);
            }
        }

        public static async UniTask GetRequestAsync(string uri)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                await webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError("Error: " + webRequest.error);
                }
                else
                {
                    Debug.Log("Received: " + webRequest.downloadHandler.text);
                }
            }
        }

        public static async UniTask<string?> GetRequestAsyncString(string uri)
        {
            try
            {
                using var webRequest = UnityWebRequest.Get(uri);
                await webRequest.SendWebRequest();

                if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Error: {webRequest.error}");
                    return null;
                }

                return webRequest.downloadHandler.text;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Request to {uri} failed with exception: {ex.Message}");
                return null;
            }
        } 

        public static MudLog? FilterLogToSnapshotLog(FilterLog log, RxDatastore ds)
        {
            if (log.IsLogForEvent(new StoreSetRecordEventDTO().GetEventABI().Sha3Signature))
            {
                var decoded = Event<StoreSetRecordEventDTO>.DecodeEvent(log).Event;

                return new MudLog
                {
                    eventName = "Store_SetRecord",
                    address = log.Address,
                    args = new MudLogArgs
                    {
                        tableId = BytesToHex(decoded.TableId),
                        keyTuple = decoded.KeyTuple.Select(key => BytesToHex(key)).ToArray(),
                        staticData = BytesToHex(decoded.StaticData),
                        encodedLengths = BytesToHex(decoded.EncodedLengths),
                        dynamicData = BytesToHex(decoded.DynamicData)
                    },
                };
            }

            if (log.IsLogForEvent(new StoreSpliceStaticDataEventDTO().GetEventABI().Sha3Signature))
            {
                var decoded = Event<StoreSpliceStaticDataEventDTO>.DecodeEvent(log).Event;
                var start = (int)decoded.Start;
                var tableId = BytesToHex(decoded.TableId);
                var table = ds.TryGetTableById(tableId);
                if (table == null) return null;
                var entity = ConcatHex(decoded.KeyTuple.Select(b => BytesToHex(b)).ToArray());
                var previousValue = table.GetValue(entity);
                object previousStaticData = null;
                previousValue?.RawValue?.TryGetValue("__staticData", out previousStaticData);
                string previousStaticResult = previousStaticData as string ?? "0x";

                object previousEncodedLengths = null;
                object previousDynamicData = null;

                previousValue?.RawValue?.TryGetValue("__encodedLengths", out previousEncodedLengths);
                previousValue?.RawValue?.TryGetValue("__staticData", out previousDynamicData);

                var data = BytesToHex(decoded.Data);
                var staticData = SpliceHex(previousStaticResult, start, Size(data), data);
                return new MudLog
                {
                    eventName = "Store_SpliceStatic",
                    address = log.Address,
                    args = new MudLogArgs
                    {
                        tableId = BytesToHex(decoded.TableId),
                        keyTuple = decoded.KeyTuple.Select(key => BytesToHex(key)).ToArray(),
                        staticData = staticData,
                        encodedLengths = previousEncodedLengths as string ?? "0x",
                        dynamicData = previousDynamicData as string ?? "0x"
                    },
                };
            }

            if (log.IsLogForEvent(new StoreSpliceDynamicDataEventDTO().GetEventABI().Sha3Signature))
            {
                var decoded = Event<StoreSpliceDynamicDataEventDTO>.DecodeEvent(log).Event;
                var start = (int)decoded.Start;
                var deleteCount = decoded.DeleteCount;
                var tableId = BytesToHex(decoded.TableId);
                var table = ds.TryGetTableById(tableId);
                if (table == null) return null;
                var entity = ConcatHex(decoded.KeyTuple.Select(b => BytesToHex(b)).ToArray());
                var previousValue = table.GetValue(entity);

                object previousStaticData = null;
                previousValue?.RawValue?.TryGetValue("__staticData", out previousStaticData);

                object previousDynamicData = null;
                previousValue?.RawValue?.TryGetValue("__dynamicValue", out previousDynamicData);

                var data = BytesToHex(decoded.Data);

                var dynamicData =
                    SpliceHex((string)previousDynamicData ?? "0x", start, (int)deleteCount, data);
                return new MudLog
                {
                    eventName = "Store_SpliceDynamic",
                    address = log.Address,
                    args = new MudLogArgs
                    {
                        tableId = BytesToHex(decoded.TableId),
                        keyTuple = decoded.KeyTuple.Select(key => BytesToHex(key)).ToArray(),
                        staticData = previousStaticData as string ?? "0x",
                        encodedLengths = BytesToHex(decoded.EncodedLengths),
                        dynamicData = dynamicData
                    },
                };
            }

            if (log.IsLogForEvent(new StoreDeleteRecordEventDTO().GetEventABI().Sha3Signature))
            {
                var decoded = Event<StoreDeleteRecordEventDTO>.DecodeEvent(log).Event;
                return new MudLog
                {
                    eventName = "Store_DeleteRecord",
                    address = log.Address,
                    args = new MudLogArgs
                    {
                        tableId = BytesToHex(decoded.TableId),
                        keyTuple = decoded.KeyTuple.Select(key => BytesToHex(key)).ToArray(),
                        staticData = null,
                        encodedLengths = null,
                        dynamicData = null
                    },
                };
            }

            throw new Exception("Unknown log type");
        }
    }
}
