using Api.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace Api.Tests.Application.Behaviors;

public sealed class LoggingBehaviorTests
{
    [Test]
    public async Task LoggingBehavior_logs_success_with_elapsed_time()
    {
        var logger = new TestLogger<LoggingBehavior<TestRequest, string>>();
        var behavior = new LoggingBehavior<TestRequest, string>(logger);

        var response = await behavior.Handle(
            new TestRequest(),
            _ => Task.FromResult("ok"),
            CancellationToken.None
        );

        response.ShouldBe("ok");
        logger.Messages.Count.ShouldBe(2);
        logger.Messages[0].ShouldContain("Handling TestRequest");
        logger.Messages[1].ShouldContain("Handled TestRequest in");
        logger.Messages[1].ShouldContain("ms");
    }

    [Test]
    public async Task LoggingBehavior_logs_failure_with_elapsed_time()
    {
        var logger = new TestLogger<LoggingBehavior<TestRequest, string>>();
        var behavior = new LoggingBehavior<TestRequest, string>(logger);
        var exception = new InvalidOperationException("boom");

        var thrown = await Should.ThrowAsync<InvalidOperationException>(() =>
            behavior.Handle(
                new TestRequest(),
                _ => Task.FromException<string>(exception),
                CancellationToken.None
            )
        );

        thrown.ShouldBe(exception);
        logger.Messages.Count.ShouldBe(2);
        logger.Messages[0].ShouldContain("Handling TestRequest");
        logger.Messages[1].ShouldContain("Failed TestRequest after");
        logger.Messages[1].ShouldContain("ms");
    }

    private sealed record TestRequest : IRequest<string>;

    private sealed class TestLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        )
        {
            Messages.Add(formatter(state, exception));
        }

        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new();

            public void Dispose() { }
        }
    }
}
