using System.Collections.Generic;
using System.Threading;

namespace AlibabaCloud.OSS.V2.Paginator
{
    /// <summary>
    /// Interface for operation paginators
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPaginator<out T>
    {
        IEnumerable<T> IterPage();
        IAsyncEnumerable<T> IterPageAsync(CancellationToken cancellationToken = default);
    }
}
