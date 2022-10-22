using Xunit.Abstractions;

namespace Demo.FunctionalTests.UseCase.Queries;

[Collection(nameof(ServiceCollectionFixture))]
public class GetCarTests
{
    private readonly DemoService.DemoServiceClient _client;

    public GetCarTests(ServiceFixture service, ITestOutputHelper output)
    {
        service.OutputHelper = output;
        _client = service.GrpcClient;
    }

    [Fact]
    public async Task CanGetCar()
    {
        // arrange
        var id = Guid.NewGuid().ToString();

        // act
        await _client.AddCarAsync(new AddCarRequest {Id = id});
        var result = await _client.GetCarAsync(new GetCarRequest {Id = id});

        // assert
        result.Id.Should().Be(id);
        result.Registration.Should().BeEmpty();
    }
}