### Connection objects

Connection to the q process is wrapped by the instances of classes implementing the `QConnection` interface.

The `qSharp` library provides two implementations of the `QConnection` interface:
* `QBasicConnection` - basic connector implementation,
* `QCallbackConnection` - in addition to `QBasicConnection` enables the Event mechanism to publish messages received from q.

The `QBasicConnection` class provides following constructor:
```csharp
QBasicConnection(String host, int port, String username, String password, String encoding)
```

The `QCallbackConnection` class provides equivalent constructor:
```csharp
QCallbackConnection(String host, int port, String username, String password, String encoding)
```

### Managing the remote connection

Note that the connection is not established when the connector instance is created. 
The connection is initialized explicitly by calling the `Open()` method.

In order to terminate the remote connection, one has to invoke the `Close()` method.

The `QConnection` interface provides the `Reset()` method which terminates current connection and opens a new one.