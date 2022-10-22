using Xunit.Abstractions;

namespace Demo.FunctionalTests.UseCase.Commands;

[Collection(nameof(ServiceCollectionFixture))]
public class RegisterCarTests
{
    private readonly DemoService.DemoServiceClient _client;

    public RegisterCarTests(ServiceFixture service, ITestOutputHelper output)
    {
        service.OutputHelper = output;
        _client = service.GrpcClient;
    }

    [Fact]
    public async Task CanRegisterCar()
    {
        // arrange
        var id = Guid.NewGuid().ToString();
        var registration = Guid.NewGuid().ToString()[..6];

        // act
        await _client.AddCarAsync(new AddCarRequest {Id = id});
        await _client.RegisterCarAsync(new RegisterCarRequest {Id = id, Registration=registration});
        var result = await _client.GetCarByRegistrationAsync(new GetCarByRegistrationRequest {Registration=registration});

        // assert
        result.Id.Should().Be(id);
        result.Registration.Should().Be(registration);
    }
}