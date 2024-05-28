using AspNet8Identity.Components;
using AspNet8Identity.Components.Account;
using AspNet8Identity.Data;
using AspNet8Identity.Misc;
using AspNet8Identity.Services;
using AspNet8Identity.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddSingleton(Log.Logger);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseSqlite(
        builder.Configuration.GetConnectionString("IdentityDb"),
        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<QrCodeService>();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, EmailSender>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("AuthSettings"));
var authSettings = builder.Configuration.GetSection("AuthSettings").Get<AuthSettings>() ?? throw new InvalidOperationException("AuthSettings are not configured correctly.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddMicrosoftAccount(microsoftOptions =>
{
    microsoftOptions.ClientId = authSettings.Microsoft?.Key!;
    microsoftOptions.ClientSecret = authSettings.Microsoft?.Secret!;
})
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = authSettings.Google?.Key!;
    googleOptions.ClientSecret = authSettings.Google?.Secret!;
})
.AddFacebook(facebookOptions =>
{
    facebookOptions.AppId = authSettings.Facebook?.Key!;
    facebookOptions.AppSecret = authSettings.Facebook?.Secret!;
})
.AddTwitter(twitterOptions =>
{
    twitterOptions.ConsumerKey = authSettings.Twitter?.Key!;
    twitterOptions.ConsumerSecret = authSettings.Twitter?.Secret!;
})
.AddIdentityCookies();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "API", Version = "v1" });
});

builder.Services.AddTransient<DbInitializer>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1"));

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGroup("/auth").MapIdentityApi<ApplicationUser>();

app.MapAdditionalIdentityEndpoints();
app.UseSerilogRequestLogging();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();

        var initializer = services.GetRequiredService<DbInitializer>();
        await initializer.SeedRolesAsync();
        await initializer.SeedAdminAsync();

        Log.Information("Seeding Db successfully");
    }
    catch (Exception ex)
    {
        Log.Error("An error occurred seeding the Db. Exception: {ExceptionMessage}", ex.Message);
    }
}

app.MapGet("/api/test-admin", [Authorize(Roles = Roles.Admin)] () => {
    return Results.Ok("Logged as admin");
});

app.Run();
