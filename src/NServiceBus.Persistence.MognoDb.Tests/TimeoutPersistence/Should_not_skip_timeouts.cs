using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Persistence.MognoDb.Tests.SubscriptionPersistence;
using NServiceBus.Persistence.MongoDB.Timeout;
using NServiceBus.Support;
using NServiceBus.Timeout.Core;
using NUnit.Framework;

namespace NServiceBus.Persistence.MognoDb.Tests.TimeoutPersistence
{
    [TestFixture]
    public class Should_not_skip_timeouts : MongoFixture
    {
        [TestCase]
        public void Never_ever()
        {
            var db = Guid.NewGuid().ToString();
            

                var persister = new TimeoutPersister(Database)
                {
                    EndpointName = Guid.NewGuid().ToString(),
                };

                var startSlice = DateTime.UtcNow.AddYears(-10);
                
            // avoid cleanup from running during the test by making it register as being run
                
                DateTime nextTimeToRunQuery;
                Assert.AreEqual(0, persister.GetNextChunk(startSlice, out nextTimeToRunQuery).Count());

                var expected = new List<Tuple<string, DateTime>>();
                var lastTimeout = DateTime.UtcNow;
                var finishedAdding = false;

                new Thread(() =>
                {
                    var sagaId = Guid.NewGuid();
                    for (var i = 0; i < 100; i++)
                    {
                        var td = new TimeoutData
                        {
                            SagaId = sagaId,
                            Destination = new Address("queue", "machine"),
                            Time = DateTime.UtcNow.AddSeconds(RandomProvider.GetThreadRandom().Next(1, 20)),
                            OwningTimeoutManager = persister.EndpointName,
                        };
                        persister.Add(td);
                        expected.Add(new Tuple<string, DateTime>(td.Id, td.Time));
                        lastTimeout = (td.Time > lastTimeout) ? td.Time : lastTimeout;
                    }
                    finishedAdding = true;
                    Trace.WriteLine("*** Finished adding ***");
                }).Start();

                // Mimic the behavior of the TimeoutPersister coordinator
                var found = 0;
                TimeoutData tmptd;
                while (!finishedAdding || startSlice == lastTimeout)
                {
                    DateTime nextRetrieval;
                    var timeoutDatas = persister.GetNextChunk(startSlice, out nextRetrieval);
                    foreach (var timeoutData in timeoutDatas)
                    {
                        if (startSlice < timeoutData.Item2)
                        {
                            startSlice = timeoutData.Item2;
                        }

                        Assert.IsTrue(persister.TryRemove(timeoutData.Item1, out tmptd));
                        found++;
                    }
                }

               

                // If the persister reports stale results have been seen at one point during its normal operation,
                // we need to perform manual cleaup.
                while (true)
                {
                    var chunkToCleanup = persister.GetNextChunk(DateTime.UtcNow.AddDays(1), out nextTimeToRunQuery).ToArray();
                    if (chunkToCleanup.Length == 0) break;

                    found += chunkToCleanup.Length;
                    foreach (var tuple in chunkToCleanup)
                    {
                        Assert.IsTrue(persister.TryRemove(tuple.Item1, out tmptd));
                    }
                }

               
                Assert.AreEqual(0, Database.GetCollection<TimeoutEntity>("timeouts").Count());
                

                Assert.AreEqual(expected.Count, found);
            
        }
        /*
        [TestCase]
        public void Should_not_skip_timeouts_also_with_multiple_clients_adding_timeouts()
        {
            var db = Guid.NewGuid().ToString();
            using (var documentStore = new DocumentStore
            {
                Url = "http://localhost:8081",
                DefaultDatabase = db,
            }.Initialize())
            {
                new TimeoutsIndex().Execute(documentStore);

                var persister = new TimeoutPersister
                {
                    DocumentStore = documentStore,
                    EndpointName = "foo",
                    TriggerCleanupEvery = TimeSpan.FromDays(1), // Make sure cleanup doesn't run automatically
                };

                var startSlice = DateTime.UtcNow.AddYears(-10);
                // avoid cleanup from running during the test by making it register as being run
                Assert.AreEqual(0, persister.GetCleanupChunk(startSlice).Count());

                const int insertsPerThread = 1000;
                var expected = 0;
                var lastExpectedTimeout = DateTime.UtcNow;
                var finishedAdding1 = false;
                var finishedAdding2 = false;

                new Thread(() =>
                {
                    var sagaId = Guid.NewGuid();
                    for (var i = 0; i < insertsPerThread; i++)
                    {
                        var td = new TimeoutData
                        {
                            SagaId = sagaId,
                            Destination = new Address("queue", "machine"),
                            Time = DateTime.UtcNow.AddSeconds(RandomProvider.GetThreadRandom().Next(1, 20)),
                            OwningTimeoutManager = string.Empty,
                        };
                        persister.Add(td);
                        Interlocked.Increment(ref expected);
                        lastExpectedTimeout = (td.Time > lastExpectedTimeout) ? td.Time : lastExpectedTimeout;
                    }
                    finishedAdding1 = true;
                    Console.WriteLine("*** Finished adding ***");
                }).Start();

                new Thread(() =>
                {
                    using (var store = new DocumentStore
                    {
                        Url = "http://localhost:8081",
                        DefaultDatabase = db,
                    }.Initialize())
                    {
                        var persister2 = new TimeoutPersister
                        {
                            DocumentStore = store,
                            EndpointName = "bar",
                        };

                        var sagaId = Guid.NewGuid();
                        for (var i = 0; i < insertsPerThread; i++)
                        {
                            var td = new TimeoutData
                            {
                                SagaId = sagaId,
                                Destination = new Address("queue", "machine"),
                                Time = DateTime.UtcNow.AddSeconds(RandomProvider.GetThreadRandom().Next(1, 20)),
                                OwningTimeoutManager = string.Empty,
                            };
                            persister2.Add(td);
                            Interlocked.Increment(ref expected);
                            lastExpectedTimeout = (td.Time > lastExpectedTimeout) ? td.Time : lastExpectedTimeout;
                        }
                    }
                    finishedAdding2 = true;
                    Console.WriteLine("*** Finished adding via a second client connection ***");
                }).Start();

                // Mimic the behavior of the TimeoutPersister coordinator
                var found = 0;
                TimeoutData tmptd;
                while (!finishedAdding1 || !finishedAdding2 || startSlice < lastExpectedTimeout)
                {
                    DateTime nextRetrieval;
                    var timeoutDatas = persister.GetNextChunk(startSlice, out nextRetrieval);
                    foreach (var timeoutData in timeoutDatas)
                    {
                        if (startSlice < timeoutData.Item2)
                        {
                            startSlice = timeoutData.Item2;
                        }

                        Assert.IsTrue(persister.TryRemove(timeoutData.Item1, out tmptd));
                        found++;
                    }
                }

                WaitForIndexing(documentStore);

                // If the persister reports stale results have been seen at one point during its normal operation,
                // we need to perform manual cleaup.
                while (true)
                {
                    var chunkToCleanup = persister.GetCleanupChunk(DateTime.UtcNow.AddDays(1)).ToArray();
                    Console.WriteLine("Cleanup: got a chunk of size " + chunkToCleanup.Length);
                    if (chunkToCleanup.Length == 0) break;

                    found += chunkToCleanup.Length;
                    foreach (var tuple in chunkToCleanup)
                    {
                        Assert.IsTrue(persister.TryRemove(tuple.Item1, out tmptd));
                    }

                    WaitForIndexing(documentStore);
                }

                using (var session = documentStore.OpenSession())
                {
                    var results = session.Query<TimeoutData>().ToList();
                    Assert.AreEqual(0, results.Count);
                }

                Assert.AreEqual(expected, found);
            }
        }
        */
        public static class RandomProvider
        {
            private static int seed = Environment.TickCount;

            private static ThreadLocal<Random> randomWrapper = new ThreadLocal<Random>(() =>
                new Random(Interlocked.Increment(ref seed))
            );

            public static Random GetThreadRandom()
            {
                return randomWrapper.Value;
            }
        }
    }
}
