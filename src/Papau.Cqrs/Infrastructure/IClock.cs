using System;

namespace bikechallenge.Domain.Support
{
    public interface IClock
    {
        DateTimeOffset GetUtcNow();
    }
}