// Modified by SignalFx

using System.Collections.Generic;

namespace Datadog.Trace.TestHelpers
{
    public static class HeadersCollectionTestHelpers
    {
        public static IEnumerable<object[]> GetInvalidIds()
        {
            yield return new object[] { "0" };
            yield return new object[] { "-1" };
            yield return new object[] { "id" };
        }

        public static IEnumerable<object[]> GetInvalidSamplingPriorities()
        {
            yield return new object[] { "-2" };
            yield return new object[] { "3" };
            yield return new object[] { "sampling.priority" };
        }
    }
}
