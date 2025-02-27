using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlibabaCloud.OSS.V2.Internal {
    internal class ExecuteStack : IDisposable {

        public delegate IExecuteMiddleware CreateMiddleware(IExecuteMiddleware next);

        private readonly object _lock = new object();

        private readonly Transport.HttpTransport? _handler;

        //private Tuple<CreateMiddleware, string>[] _stack = List<>;
        private IList<Tuple<CreateMiddleware, string>> _stack = new List<Tuple<CreateMiddleware, string>> { };

        private IExecuteMiddleware? _cached;

        public ExecuteStack(Transport.HttpTransport? handler) {
            _handler = handler;
        }

        public void Push(CreateMiddleware create, string name) {
            _stack.Add(new Tuple<CreateMiddleware, string>(create, name));
            _cached = null;
        }

        public IExecuteMiddleware Resolve() {
            if (_cached == null) {
                lock (_lock) {
                    if (_cached == null) {
                        if (_handler == null) {
                            throw new Exception("HttpTransport is null");
                        }
                        IExecuteMiddleware prev = new TransportExecuteMiddleware(_handler);
                        //foreach (var stack in _stack.Reverse()) {
                        //    prev = stack.Item1(prev);
                        //}
                        for (var i = _stack.Count - 1; i >= 0; i--) {
                            prev = _stack[i].Item1(prev);
                        }
                        _cached = prev;
                    }
                }
            }
            return _cached;
        }

        public Task<ResponseMessage> ExecuteAsync(RequestMessage request, ExecuteContext context) {
            var handler = Resolve();
            return handler.ExecuteAsync(request, context);
        }

        public void Dispose() {
            _handler?.Dispose();
            _cached = null;
            _stack = new List<Tuple<CreateMiddleware, string>> { };
        }
    }
}
