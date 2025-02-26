using System.Threading.Tasks;


namespace AlibabaCloud.OSS.v2.Internal {
    internal interface IExecuteMiddleware {
        public Task<ResponseMessage> ExecuteAsync(RequestMessage request, ExecuteContext context);
    }
}
