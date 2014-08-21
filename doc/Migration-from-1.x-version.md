This page describes the most important differences between `qSharp` versions: `1.x` and `2.x`.

### Class hierarchy

Class hierarchy has been remodeled to enable end-user to subclass in more convenient manner.

* `QConnection` is now interface and can be implemented in 3rd party extensions. E.g.: custom load balancing implementation class may implement `QConnection` and enclose multiple instances of `QBasicConnection` to perform balancing.
* Basic connectivity is now implemented in the `QBasicConnection` class. This class provides minimal, complete set of API methods to interact with remote q processes.
* Functionality for the asynchronous connectivity and listening in the wrapped thread is now refactored to the `QCallbackConnection` class.

### QReader API

`QReader` no longer provides get accessors: 
* `DataSize()`
* `Endianess()`
* `MessageSize()`
* `MessageType()`
* `Compressed()`

As a replacement, `QReader.Read(…)` method returns instances of `QMessage` struct. The `QMessage` instance provides access to both data payload and meta information like: size of the message, size of data, message type, endianess and compression flag.

One have to use lower level API of the `QConnection` interface to access these information:
* `object Receive(bool dataOnly = true, bool raw = false)`
* `int Query(MessageType msgType, string query, params object[] parameters)`

### QWriter API

`QWriter.Write(…)` method now returns number of written bytes (i.e.: size of the message). Accessors: `MessageSize()` and `DataSize()` have been removed.

Please note that header in IPC protocol has fixed length of 8 bytes and thus size of data payload is equal to `MessageSize – 8`.

### Failover

Generic failover functionality (`QFailoverConnection` class) has been removed. There are no plans to reintroduce this feature at the moment.

### Samples

Please refer to [samples section](Usage-examples.md) for up-to-date usage examples.