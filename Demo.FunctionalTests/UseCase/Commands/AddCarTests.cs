using Xunit.Abstractions;

namespace Demo.FunctionalTests.UseCase.Commands;

[Collection(nameof(ServiceCollectionFixture))]
public class AddCarTests
{
    private readonly DemoService.DemoServiceClient _client;

    public AddCarTests(ServiceFixture service, ITestOutputHelper output)
    {
        service.OutputHelper = output;
        _client = service.GrpcClient;
    }

    [Fact]
    public async Task CanAddCar()
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