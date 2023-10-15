#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

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
                Debug.Log("Registering table: " + table.Name + " with id: " + table.TableId + " and schema: " + table.ValueSchema);
                ds.RegisterTable(table.TableId, table.Name, table.ValueSchema);
            }
        }
    }
}
