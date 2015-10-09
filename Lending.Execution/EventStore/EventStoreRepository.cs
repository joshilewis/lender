using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EventStore.ClientAPI;
using Lending.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceStack.Text;

namespace Lending.Execution.EventStore
{
    public class EventStoreRepository : IRepository, IDisposable
    {
        public const string EventClrTypeHeader = "EventClrTypeName";
        public const string AggregateClrTypeHeader = "AggregateClrTypeName";
        public const string CommitIdHeader = "CommitId";
        private const int WritePageSize = 500;
        private const int ReadPageSize = 500;


        private readonly IEventStoreConnection connection;

        public EventStoreRepository(IEventStoreConnection connection)
        {
            this.Queue = new ConcurrentQueue<Aggregate>();
            this.connection = connection;
        }

        public ConcurrentQueue<Aggregate> Queue { get; private set; }

        public IEnumerable<Event> GetEventsForAggregate<TAggregate>(Guid id) where TAggregate : Aggregate
        {
            return GetEventsForAggregate<TAggregate>(id, int.MaxValue);
        }

        public IEnumerable<Event> GetEventsForAggregate<TAggregate>(Guid id, int version) where TAggregate : Aggregate
        {
            if (version <= 0)
                throw new InvalidOperationException("Cannot get version <= 0");

            var streamName = GetStreamName(typeof (TAggregate), id);

            var eventsList = new List<Event>();

            var sliceStart = 0; //Ignores $StreamCreated
            StreamEventsSlice currentSlice = connection.ReadStreamEventsForwardAsync(streamName, 0, 10, false).Result;
            do
            {
                var sliceCount = sliceStart + ReadPageSize <= version
                    ? ReadPageSize
                    : version - sliceStart + 1;

                currentSlice = connection.ReadStreamEventsForwardAsync(streamName, sliceStart, sliceCount, false).Result;

                if (currentSlice.Status == SliceReadStatus.StreamNotFound)
                    throw new AggregateNotFoundException(id, typeof (TAggregate));

                if (currentSlice.Status == SliceReadStatus.StreamDeleted)
                    throw new AggregateDeletedException(id, typeof (TAggregate));

                sliceStart = currentSlice.NextEventNumber;
                eventsList.AddRange(
                    currentSlice.Events.Select(
                        evnt => DeserializeEvent(evnt.OriginalEvent.Metadata, evnt.OriginalEvent.Data)).ToList());

            } while (version >= currentSlice.NextEventNumber && !currentSlice.IsEndOfStream);

            return eventsList;
        }

        private static string GetStreamName(Type type, Guid aggregateId)
        {
            return $"{char.ToLower(type.Name[0]) + type.Name.Substring(1)}-{aggregateId}";
        }

        private static Event DeserializeEvent(byte[] metadata, byte[] data)
        {
            var eventClrTypeName = JObject.Parse(Encoding.UTF8.GetString(metadata)).Property(EventClrTypeHeader).Value;
            return (Event)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), Type.GetType((string)eventClrTypeName));
        }

        public void Save(Aggregate aggregate)
        {
            Queue.Enqueue(aggregate);
        }

        public void Dispose()
        {
            Queue = null;
        }

        public void Commit(Guid transactionId)
        {
            foreach (Aggregate aggregate in Queue)
            {
                SaveAggregate(aggregate, transactionId);
            }
            Queue = new ConcurrentQueue<Aggregate>(); //https://social.msdn.microsoft.com/Forums/en-US/accf4254-ee81-4059-9251-619bc6bbeadf/clear-a-concurrentqueue?forum=rx
        }

        private void SaveAggregate(Aggregate aggregate, Guid transactionId)
        {
            //Taken from https://github.com/pgermishuys/getting-started-with-event-store/blob/master/src/GetEventStoreRepository/GetEventStoreRepository.cs
            var commitHeaders = new Dictionary<string, object>
            {
                {CommitIdHeader, transactionId},
                {AggregateClrTypeHeader, aggregate.GetType().AssemblyQualifiedName}
            };

            IList<Event> newEvents = aggregate.GetUncommittedEvents();
            int originalVersion = aggregate.Version - newEvents.Count;
            int expectedVersion = originalVersion; //http://stackoverflow.com/a/20204729
            if (originalVersion == 0)
                expectedVersion = ExpectedVersion.NoStream;
            IEnumerable<EventData> eventsToSave = newEvents.Select(e => ToEventData(Guid.NewGuid(), e, commitHeaders));

            string streamName = GetStreamName(aggregate.GetType(), aggregate.Id);

            if (eventsToSave.Count() < WritePageSize)
            {
                connection.AppendToStreamAsync(streamName, ExpectedVersion.Any, eventsToSave).Wait();
            }
            else
            {
                var transaction = connection.StartTransactionAsync(streamName, expectedVersion).Result;

                var position = 0;
                while (position < eventsToSave.Count())
                {
                    var pageEvents = eventsToSave.Skip(position).Take(WritePageSize);
                    transaction.WriteAsync(pageEvents).Wait();
                    position += WritePageSize;
                }

                transaction.CommitAsync().Wait();
            }

            aggregate.ClearUncommittedEvents();

        }

        private static EventData ToEventData(Guid eventId, object evnt, IDictionary<string, object> headers)
        {
            var serializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(evnt, serializerSettings));

            var eventHeaders = new Dictionary<string, object>(headers)
            {
                {
                    EventClrTypeHeader, evnt.GetType().AssemblyQualifiedName
                }
            };
            var metadata = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventHeaders, serializerSettings));
            var typeName = evnt.GetType().Name;

            return new EventData(eventId, typeName, true, data, metadata);
        }

    }
}