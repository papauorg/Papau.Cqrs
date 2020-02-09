using System;

namespace bikechallenge.Domain.Support
{
    public class SystemClock : IClock
    {
        DateTimeOffset IClock.GetUtcNow()
        {
            return DateTime.UtcNow;
        }
    }
}