using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AnimeCharacters.Data.Services
{
    public class RateLimiter : IDisposable
    {
        private readonly object _lock = new object();
        private readonly Queue<DateTime> _requestTimes = new();
        private int _maxRequests;
        private readonly TimeSpan _timeWindow;
        private readonly TimeSpan _baseBackoffDelay;
        private DateTime? _backoffUntil;
        private int _consecutiveFailures = 0;
        private int _remainingRequests;
        private DateTime _rateLimitWindowReset = DateTime.UtcNow;

        public RateLimiter(int maxRequests = 60, TimeSpan? timeWindow = null, TimeSpan? baseBackoffDelay = null)
        {
            _maxRequests = maxRequests;
            _remainingRequests = maxRequests;
            _timeWindow = timeWindow ?? TimeSpan.FromMinutes(1);
            _baseBackoffDelay = baseBackoffDelay ?? TimeSpan.FromSeconds(1);
        }

        public async Task<bool> TryAcquireAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;

                // Check if we're in backoff period
                if (_backoffUntil.HasValue && now < _backoffUntil.Value)
                {
                    return false;
                }

                // Check header-based rate limiting first
                if (IsRateLimitedByHeaders())
                {
                    return false;
                }

                // Remove old requests outside the time window
                while (_requestTimes.Count > 0 && now - _requestTimes.Peek() > _timeWindow)
                {
                    _requestTimes.Dequeue();
                }

                // Check if we can make a request based on our local tracking
                if (_requestTimes.Count >= _maxRequests)
                {
                    return false;
                }

                // Record this request
                _requestTimes.Enqueue(now);
                
                // Decrement remaining requests from headers if we have that info
                if (_remainingRequests > 0)
                {
                    _remainingRequests--;
                }
                
                return true;
            }
        }

        public async Task WaitForAvailabilityAsync(CancellationToken cancellationToken = default)
        {
            while (!await TryAcquireAsync(cancellationToken))
            {
                var delay = GetNextAvailableDelay();
                await Task.Delay(delay, cancellationToken);
            }
        }

        public void RecordSuccess()
        {
            lock (_lock)
            {
                _consecutiveFailures = 0;
                _backoffUntil = null;
            }
        }

        public void RecordFailure(bool isRateLimit = false, bool isCorsError = false)
        {
            lock (_lock)
            {
                _consecutiveFailures++;
                
                var backoffMultiplier = Math.Min(Math.Pow(2, _consecutiveFailures - 1), 32); // Max 32x backoff
                var backoffDelay = TimeSpan.FromMilliseconds(_baseBackoffDelay.TotalMilliseconds * backoffMultiplier);
                
                // For rate limit errors or CORS errors (treat CORS as rate limit), use longer backoff
                if (isRateLimit || isCorsError)
                {
                    backoffDelay = TimeSpan.FromMinutes(Math.Min(backoffMultiplier * 2, 60));
                    
                    // For CORS errors, assume we hit rate limits and reset remaining to 0
                    if (isCorsError)
                    {
                        _remainingRequests = 0;
                        _rateLimitWindowReset = DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)); // Assume 1-minute reset
                    }
                }

                _backoffUntil = DateTime.UtcNow.Add(backoffDelay);
            }
        }

        public void UpdateFromHeaders(Dictionary<string, string> headers)
        {
            lock (_lock)
            {
                if (headers.TryGetValue("X-Ratelimit-Limit", out var limitStr) &&
                    int.TryParse(limitStr, out var limit))
                {
                    _maxRequests = limit;
                }

                if (headers.TryGetValue("X-Ratelimit-Remaining", out var remainingStr) &&
                    int.TryParse(remainingStr, out var remaining))
                {
                    _remainingRequests = remaining;
                }

                if (headers.TryGetValue("X-Ratelimit-Reset", out var resetStr))
                {
                    // Try to parse as Unix timestamp or ISO date
                    if (long.TryParse(resetStr, out var unixTimestamp))
                    {
                        _rateLimitWindowReset = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime;
                    }
                    else if (DateTime.TryParse(resetStr, out var resetDate))
                    {
                        _rateLimitWindowReset = resetDate.ToUniversalTime();
                    }
                }
            }
        }

        public bool IsRateLimitedByHeaders()
        {
            lock (_lock)
            {
                return _remainingRequests <= 0 && DateTime.UtcNow < _rateLimitWindowReset;
            }
        }

        public RateLimitStatus GetStatus()
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                
                // Clean up old requests
                while (_requestTimes.Count > 0 && now - _requestTimes.Peek() > _timeWindow)
                {
                    _requestTimes.Dequeue();
                }

                var windowStart = _requestTimes.Count > 0 ? _requestTimes.Peek() : now;
                var backoffDelay = _backoffUntil.HasValue && _backoffUntil.Value > now 
                    ? _backoffUntil.Value - now 
                    : (TimeSpan?)null;

                return new RateLimitStatus
                {
                    RequestsInCurrentWindow = _requestTimes.Count,
                    MaxRequestsPerWindow = _maxRequests,
                    WindowDuration = _timeWindow,
                    WindowStartTime = windowStart,
                    BackoffDelay = backoffDelay,
                    IsRateLimited = _requestTimes.Count >= _maxRequests || _backoffUntil.HasValue && _backoffUntil.Value > now
                };
            }
        }

        private TimeSpan GetNextAvailableDelay()
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;

                // If in backoff, wait until backoff expires
                if (_backoffUntil.HasValue && _backoffUntil.Value > now)
                {
                    return _backoffUntil.Value - now;
                }

                // If rate limited, wait until oldest request expires
                if (_requestTimes.Count >= _maxRequests)
                {
                    var oldestRequest = _requestTimes.Peek();
                    var waitTime = _timeWindow - (now - oldestRequest);
                    return waitTime.TotalMilliseconds > 0 ? waitTime : TimeSpan.FromMilliseconds(100);
                }

                return TimeSpan.FromMilliseconds(100); // Small delay for polling
            }
        }

        public void Dispose()
        {
            // RateLimiter doesn't have unmanaged resources to dispose
            // This is here to satisfy the IDisposable interface used in PrioritySyncService
        }
    }
}