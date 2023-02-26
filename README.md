# ECS Replicator for .NET

Serialize and Deserialize atomic structural changes for components.

Uses [blitser-dotnet](https://github.com/piot/blitser-dotnet) for component serialization.

Not production ready. Please do not use yet.

## Usage

### Component Sender

```csharp
public SnapshotSender(IDataSender world, IEntityContainerWithDetectChanges worldWithChanges, ILog log);
```

Create a SnapshotSender object and pass in your implementation of a Blitser `IDataSender` and a Ecs Replicator `IEntityContainerWithDetectChanges`. as well as a logger (see [Clog](https://github.com/piot/clog-dotnet)).

#### On Connection

When a client connection is established, call `CreateSyncer` on the `IComponentSender` with a unique connection id:

```csharp
public ISyncerForClient CreateSyncer(ConnectionId id);
```

#### Every snapshot

Everytime it is time to take a snapshot of the components, call `IComponentSender:CreateSnapshot`. That creates a internal record of the component changes at that time. The changes are queried using a call to `IEntityContainerWithDetectChanges:EntitiesThatHasChanged`.

```csharp
public interface IEntityContainerWithDetectChanges
{
    public AllEntitiesChangesThisTick EntitiesThatHasChanged(ILog log);
}
```

#### Sending changes to a client

Call `ISnapshotSender:SendSnapshotTo` with the syncer in question. It will always try to replicate the last snapshot that was stored using `ISnapshotSender:CreateSnapshot()`.

```csharp
public ReadOnlySpan<byte> SendSnapshotTo(ISyncerForClient connection);
```

#### Snapshot Receive status from client

```csharp
public interface ISyncerForClient
{
    public void ReceiveStatusFromReceiver(ReadOnlySpan<byte> octets);
    public void ReceiveStatusFromReceiver(IOctetReader reader);
    public void ReceiveStatusFromReceiver(TickId tickId, uint droppedCount);
}
```

Call `ReceiveStatusFromReceiver` when the octets for status has been received. That will influence what is sent on the next call to `SendSnapshotTo` method.

### Component Receiver

Create a SnapshotReceiver object. Pass in the Blitser `IDataReceiver` and Blitser `IEventReceiver` that should receive the changes.

```csharp
SnapshotReceiver(IDataReceiver clientWorld, IEventReceiver eventReceiver, ILog log);
```

After that, use the `ISnapshotReceiver` interface:

```csharp
public interface ISnapshotReceiver
{
    public SnapshotPack Receive(ReadOnlySpan<byte> completePayload);
    public void WriteStatusHeader(IOctetWriter octetWriter);
    public void Process(SnapshotPack snapshotPack);
}
```

When the snapshot octets are received, it should be passed in to `ISnapshotReceiver:Receive()`.

The returned snapshot pack contains the information about the snapshot as well as the octets that should be deserialized.

As soon as possible after receiving, it should notify the remote about what it has been received successfully. So call `WriteStatusHeader` on the next outgoing packet.

When the receiver deems that it is a good time to apply the changes, call `Process` to apply the changes to the components.
