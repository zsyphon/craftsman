﻿namespace Craftsman.Builders;

using Helpers;
using Services;
using static Helpers.ConstMessages;

public class ProgramBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public ProgramBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateWebApiProgram(string srcDirectory, bool useJwtAuth, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiProjectRootClassPath(srcDirectory, $"Program.cs", projectBaseName);
        var fileText = GetWebApiProgramText(classPath.ClassNamespace, srcDirectory, useJwtAuth, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public void CreateAuthServerProgram(string projectDirectory, string authServerProjectName)
    {
        var classPath = ClassPathHelper.WebApiProjectRootClassPath(projectDirectory, $"Program.cs", authServerProjectName);
        var fileText = GetAuthServerProgramText(classPath.ClassNamespace);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetWebApiProgramText(string classNamespace, string srcDirectory, bool useJwtAuth, string projectBaseName)
    {
        var hostExtClassPath = ClassPathHelper.WebApiHostExtensionsClassPath(srcDirectory, $"", projectBaseName);
        var apiAppExtensionsClassPath = ClassPathHelper.WebApiApplicationExtensionsClassPath(srcDirectory, "", projectBaseName);
        var configClassPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, "", projectBaseName);
        
        var appAuth = "";
        var corsName = $"{projectBaseName}CorsPolicy";
        if (useJwtAuth)
        {
            appAuth = $@"

app.UseAuthentication();
app.UseAuthorization();";
        }

        return @$"using Serilog;
using {apiAppExtensionsClassPath.ClassNamespace};
using {hostExtClassPath.ClassNamespace};
using {configClassPath.ClassNamespace};

var builder = WebApplication.CreateBuilder(args);
builder.Host.AddLoggingConfiguration(builder.Environment);

builder.ConfigureServices();
var app = builder.Build();

if (builder.Environment.IsDevelopment())
{{
    app.UseDeveloperExceptionPage();
}}
else
{{
    app.UseExceptionHandler(""/Error"");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}}

// For elevated security, it is recommended to remove this middleware and set your server to only listen on https.
// A slightly less secure option would be to redirect http to 400, 505, etc.
app.UseHttpsRedirection();

app.UseCors(""{corsName}"");

app.UseSerilogRequestLogging();
app.UseRouting();{appAuth}

app.UseEndpoints(endpoints =>
{{
    endpoints.MapHealthChecks(""/api/health"");
    endpoints.MapControllers();
}});

app.UseSwaggerExtension();

try
{{
    Log.Information(""Starting application"");
    await app.RunAsync();
}}
catch (Exception e)
{{
    Log.Error(e, ""The application failed to start correctly"");
    throw;
}}
finally
{{
    Log.Information(""Shutting down application"");
    Log.CloseAndFlush();
}}

// Make the implicit Program class public so the functional test project can access it
public partial class Program {{ }}";
    }


    public static string GetAuthServerProgramText(string classNamespace)
    {
        return @$"{DuendeDisclosure}namespace {classNamespace};

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Serilog.Events;

public class Program
{{
    public async static Task Main(string[] args)
    {{
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override(""Microsoft"", LogEventLevel.Warning)
            .MinimumLevel.Override(""Microsoft.Hosting.Lifetime"", LogEventLevel.Information)
            .MinimumLevel.Override(""System"", LogEventLevel.Warning)
            .MinimumLevel.Override(""Microsoft.AspNetCore.Authentication"", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: ""[{{Timestamp:HH:mm:ss}} {{Level}}] {{SourceContext}}{{NewLine}}{{Message:lj}}{{NewLine}}{{Exception}}{{NewLine}}"", theme: AnsiConsoleTheme.Code)
            .CreateLogger();

        var host = CreateHostBuilder(args).Build();

        try
        {{
            Log.Information(""Starting application"");
            await host.RunAsync();
        }}
        catch (Exception e)
        {{
            Log.Error(e, ""The application failed to start correctly"");
            throw;
        }}
        finally
        {{
            Log.Information(""Shutting down application"");
            Log.CloseAndFlush();
        }}
    }}

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder =>
            {{
                webBuilder.UseStartup<Startup>();
            }});
}}";
    }
}
