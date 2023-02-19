using System.IO;
using System.Threading.Tasks;

using FluentAssertions;

using NSubstitute;

using Papau.Cqrs.Domain;
using Papau.Cqrs.LiteDb.Domain.Aggregates;

using Xunit;

namespace Papau.Cqrs.Tests.Data.LiteDbRepository;

public class LiteDbAggregateRepositoryTests
{
    readonly LiteDbAggregateRepository<LiteDbTestAggregate> _repo;
    readonly LiteDB.LiteDatabase _liteDb;

    public LiteDbAggregateRepositoryTests()
    {
        var eventPublisher = Substitute.For<IEventPublisher>();
        var stream = new MemoryStream();
        _liteDb = new LiteDB.LiteDatabase(stream);

        _repo = new LiteDbAggregateRepository<LiteDbTestAggregate>(eventPublisher, _liteDb);
    }

    [Fact]
    public async Task Can_Write_Aggregate_To_Store_And_Read_It_Back_Out()
    {
        var sample = new LiteDbTestAggregate();
        sample.AddSampleEvent();

        await _repo.Save(sample).ConfigureAwait(false);

        var collection = _liteDb.GetCollection("AllEvents");
        var events = collection.FindAll();
        events.Should().HaveCount(1);

        var readSample = await _repo.GetById(sample.Id).ConfigureAwait(false);
        readSample.Id.Should().Be(sample.Id);
        readSample.SampleProperty.Should().Be(sample.SampleProperty);
    }
}