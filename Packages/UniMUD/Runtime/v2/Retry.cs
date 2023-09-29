using System;
using Cysharp.Threading.Tasks;

namespace v2
{
    
    public class RetryConfig
    {
        public int RetryCount { get; set; } = 2;
        public Func<RetryConfig, UniTask<bool>> ShouldRetry { get; set; } = config => UniTask.FromResult(true);
        public int Count { get; set; }
        public Exception Error { get; set; }
        private int defaultDelay = 100;
        private Func<RetryConfig, int> delayFunc = null;

        public void SetDelay(int delay) => defaultDelay = delay;
        public void SetDelay(Func<RetryConfig, int> delayFunction) => delayFunc = delayFunction;
        public int GetDelay(RetryConfig config) => delayFunc != null ? delayFunc(config) : defaultDelay;
    } 
}
