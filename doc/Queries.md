### Interacting with q

The `qSharp` library supports both synchronous and asynchronous queries.

Synchronous query waits for service response and blocks requesting process until it receives the response. 

Asynchronous query does not block the requesting process and returns immediately without any result. The actual query result can be obtained either by issuing a corresponding request later on, or by setting up a listener to await and react accordingly to received data.

The `qSharp` library provides following API methods in the `QConnection` interface to query the kdb+: 

```csharp
/// Executes a synchronous query against the remote q service.
object Sync(string query, params object[] parameters)

/// Executes an asynchronous query against the remote q service.
void Async(string query, params object[] parameters)

/// Executes a query against the remote q service.
int Query(MessageType msgType, string query, params object[] parameters)
```

where:
* `query` is the definition of the query to be executed,
* `parameters` is a list of additional parameters used when executing given query,
* `msgType` indicates type of the q message to be sent. 

In typical use case, `query` is the name of the function to call and parameters are its parameters. When parameters list is empty the query can be an arbitrary q expression (e.g. `0 +/ til 100`).

In order to retrieve query result (for the `Async()` or `Query()` methods), one has to call:
```csharp
/// Reads next message from the remote q service.
object Receive(bool dataOnly = true, bool raw = false)
```

If the `dataOnly` parameter is set to `true`, only data part of the message is returned. If set to `false`, both data and message meta-information is returned as a wrapped structure `QMessage`.

If the `raw` parameter is set to `false`, message is parsed and transformed to C# object. If set to `true`, message is not parsed and raw array of bytes is returned.

### Asynchronous subscription

The `QCallbackConnection` class extends the `QBasicConnection` class and enables the Event mechanism to publish messages received from remote q service.

One can subscribe to q messages via the following event handler:
```csharp
virtual public event EventHandler<QMessageEvent> DataReceived
```

The `QCallbackConnection` wraps the thread instance which can be used to listen to incoming q messages and pushing these via the event handler. This thread can be start and stopped via `StartListener()` and `StopListener()` methods respectively.

Similarly, errors caught in the listener thread can be subscribed to by the event handler:
```csharp
virtual public event EventHandler<QErrorEvent> ErrorOccured
```