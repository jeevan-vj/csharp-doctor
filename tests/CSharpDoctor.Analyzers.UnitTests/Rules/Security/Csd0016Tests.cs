using CSharpDoctor.Analyzers.Rules.Observability;
using CSharpDoctor.Analyzers.Rules.Performance;
using CSharpDoctor.Analyzers.Rules.Security;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Security;

public sealed class Csd0016Tests
{
    [Fact]
    public async Task ShouldReport_WhenFileReadUsesDynamicPath()
    {
        var source = """
            class C {
                void M(string file) {
                    System.IO.File.ReadAllText(file);
                }
            }
            """;

        await AnalyzerTest<Csd0016PathTraversalAnalyzer>.RunAsync(source, "CSD0016");
    }

    [Fact]
    public async Task ShouldNotReport_WhenPathIsLiteral()
    {
        var source = """
            class C {
                void M() {
                    System.IO.File.ReadAllText("/tmp/safe.txt");
                }
            }
            """;

        await AnalyzerTest<Csd0016PathTraversalAnalyzer>.RunAsync(source);
    }
}

public sealed class Csd0017Tests
{
    [Fact]
    public async Task ShouldReport_WhenProcessStartUsesDynamicArguments()
    {
        var source = """
            class Process { public static void Start(string fileName, string arguments) { } }
            class C {
                void M(string input) {
                    Process.Start("tool.exe", input);
                }
            }
            """;

        await AnalyzerTest<Csd0017CommandInjectionAnalyzer>.RunAsync(source, "CSD0017");
    }

    [Fact]
    public async Task ShouldNotReport_WhenArgumentsAreLiteral()
    {
        var source = """
            class Process { public static void Start(string fileName, string arguments) { } }
            class C {
                void M() {
                    Process.Start("tool.exe", "--help");
                }
            }
            """;

        await AnalyzerTest<Csd0017CommandInjectionAnalyzer>.RunAsync(source);
    }
}

public sealed class Csd0018Tests
{
    [Fact]
    public async Task ShouldReport_WhenHttpClientUsesDynamicUrl()
    {
        var source = """
            class HttpClient {
                public System.Threading.Tasks.Task GetAsync(string url) => null!;
            }
            class C {
                async System.Threading.Tasks.Task M(string url) {
                    var client = new HttpClient();
                    await client.GetAsync(url);
                }
            }
            """;

        await AnalyzerTest<Csd0018SsrfAnalyzer>.RunAsync(source, "CSD0018");
    }

    [Fact]
    public async Task ShouldNotReport_WhenUrlIsLiteral()
    {
        var source = """
            class HttpClient {
                public System.Threading.Tasks.Task GetAsync(string url) => null!;
            }
            class C {
                async System.Threading.Tasks.Task M() {
                    var client = new HttpClient();
                    await client.GetAsync("https://example.com");
                }
            }
            """;

        await AnalyzerTest<Csd0018SsrfAnalyzer>.RunAsync(source);
    }
}

public sealed class Csd0019Tests
{
    [Fact]
    public async Task ShouldReport_WhenRedirectUsesDynamicUrl()
    {
        var source = """
            class Controller {
                public object Redirect(string url) => url;
            }
            class C : Controller {
                object M(string returnUrl) => Redirect(returnUrl);
            }
            """;

        await AnalyzerTest<Csd0019OpenRedirectAnalyzer>.RunAsync(source, "CSD0019");
    }

    [Fact]
    public async Task ShouldNotReport_WhenRedirectIsLiteral()
    {
        var source = """
            class Controller {
                public object Redirect(string url) => url;
            }
            class C : Controller {
                object M() => Redirect("/home");
            }
            """;

        await AnalyzerTest<Csd0019OpenRedirectAnalyzer>.RunAsync(source);
    }
}

public sealed class Csde0008Tests
{
    [Fact]
    public async Task ShouldReport_WhenSaveChangesInsideForeach()
    {
        var source = """
            class DbContext {
                public int SaveChanges() => 0;
            }
            class C {
                void M(DbContext db, int[] ids) {
                    foreach (var id in ids) {
                        db.SaveChanges();
                    }
                }
            }
            """;

        await AnalyzerTest<Csde0008SaveChangesInLoopAnalyzer>.RunAsync(source, "CSDE0008");
    }

    [Fact]
    public async Task ShouldNotReport_WhenSaveChangesOutsideLoop()
    {
        var source = """
            class DbContext {
                public int SaveChanges() => 0;
            }
            class C {
                void M(DbContext db) {
                    db.SaveChanges();
                }
            }
            """;

        await AnalyzerTest<Csde0008SaveChangesInLoopAnalyzer>.RunAsync(source);
    }

    [Fact]
    public async Task ShouldReport_WhenSaveChangesAsyncInsideWhile()
    {
        var source = """
            class DbContext {
                public System.Threading.Tasks.Task<int> SaveChangesAsync() => null!;
            }
            class C {
                async System.Threading.Tasks.Task M(DbContext db) {
                    while (true) {
                        await db.SaveChangesAsync();
                    }
                }
            }
            """;

        await AnalyzerTest<Csde0008SaveChangesInLoopAnalyzer>.RunAsync(source, "CSDE0008");
    }
}

public sealed class Csd0501Tests
{
    [Fact]
    public async Task ShouldReport_WhenLoggingPasswordProperty()
    {
        var source = """
            interface ILogger<T> {
                void LogInformation(string message, params object[] args);
            }
            class C {
                void M(ILogger<C> logger, string password) {
                    logger.LogInformation("login {Password}", password);
                }
            }
            """;

        await AnalyzerTest<Csd0501SensitiveDataInLogsAnalyzer>.RunAsync(source, "CSD0501");
    }

    [Fact]
    public async Task ShouldNotReport_WhenLoggingRedactedValue()
    {
        var source = """
            interface ILogger<T> {
                void LogInformation(string message, params object[] args);
            }
            class C {
                void M(ILogger<C> logger, string redactedToken) {
                    logger.LogInformation("token {RedactedToken}", redactedToken);
                }
            }
            """;

        await AnalyzerTest<Csd0501SensitiveDataInLogsAnalyzer>.RunAsync(source);
    }
}

public sealed class Csd0502Tests
{
    [Fact]
    public async Task ShouldReport_WhenExceptionPassedAsTemplateArgument()
    {
        var source = """
            interface ILogger<T> {
                void LogError(System.Exception ex, string message, params object[] args);
                void LogError(string message, params object[] args);
            }
            class C {
                void M(ILogger<C> logger) {
                    try { }
                    catch (System.Exception ex) {
                        logger.LogError("failed {Ex}", ex);
                    }
                }
            }
            """;

        await AnalyzerTest<Csd0502MissingExceptionParameterAnalyzer>.RunAsync(source, "CSD0502");
    }

    [Fact]
    public async Task ShouldNotReport_WhenExceptionIsFirstParameter()
    {
        var source = """
            interface ILogger<T> {
                void LogError(System.Exception ex, string message, params object[] args);
            }
            class C {
                void M(ILogger<C> logger) {
                    try { }
                    catch (System.Exception ex) {
                        logger.LogError(ex, "failed");
                    }
                }
            }
            """;

        await AnalyzerTest<Csd0502MissingExceptionParameterAnalyzer>.RunAsync(source);
    }
}

public sealed class Csd0503Tests
{
    [Fact]
    public async Task ShouldReport_WhenLoggingRequestHeaders()
    {
        var source = """
            interface ILogger<T> { void LogInformation(string message, params object[] args); }
            class HttpRequest { public object Headers { get; set; } = null!; }
            class C {
                void M(ILogger<C> logger, HttpRequest request) {
                    logger.LogInformation("headers {Headers}", request.Headers);
                }
            }
            """;

        await AnalyzerTest<Csd0503RawRequestLoggingAnalyzer>.RunAsync(source, "CSD0503");
    }

    [Fact]
    public async Task ShouldNotReport_WhenLoggingSafeValue()
    {
        var source = """
            interface ILogger<T> { void LogInformation(string message, params object[] args); }
            class C {
                void M(ILogger<C> logger, string userId) {
                    logger.LogInformation("user {UserId}", userId);
                }
            }
            """;

        await AnalyzerTest<Csd0503RawRequestLoggingAnalyzer>.RunAsync(source);
    }
}
