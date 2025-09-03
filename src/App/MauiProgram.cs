using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ExamEngine;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>();

        // Logging
        builder.Logging.AddDebug();

        // DI registrations (placeholders as singletons)
        builder.Services
            .AddSingleton<IDatabaseManager, DatabaseManager>()
            .AddSingleton<IQuestionRepository, QuestionRepository>()
            .AddSingleton<IAttemptRepository, AttemptRepository>()
            .AddSingleton<IProgressService, ProgressService>()
            .AddSingleton<ContentPackService>();

        return builder.Build();
    }
}

// Placeholder service contracts and implementations for DI
public interface IDatabaseManager { }
public sealed class DatabaseManager : IDatabaseManager { }

public interface IQuestionRepository { }
public sealed class QuestionRepository : IQuestionRepository { }

public interface IAttemptRepository { }
public sealed class AttemptRepository : IAttemptRepository { }

public interface IProgressService { }
public sealed class ProgressService : IProgressService { }

public sealed class ContentPackService { }
