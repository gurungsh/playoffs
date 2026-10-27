using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Playoffs.Web.Auth;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (ctx, cfg) =>
        cfg
            .ReadFrom.Configuration(ctx.Configuration)
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
);

builder
    .Services.AddOptions<AuthOptions>()
    .BindConfiguration(AuthOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

// The cookie only holds a key; the ticket with the tokens stays in this cache.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSingleton<ITicketStore, DistributedTicketStore>();
builder
    .Services.AddOptions<CookieAuthenticationOptions>(
        CookieAuthenticationDefaults.AuthenticationScheme
    )
    .Configure<ITicketStore>((options, store) => options.SessionStore = store);

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options => options.AccessDeniedPath = "/Account/AccessDenied")
    .AddOpenIdConnect(options =>
    {
        options.ResponseType = "code";
        options.SaveTokens = true;
        options.MapInboundClaims = false;
        options.TokenValidationParameters.NameClaimType = "preferred_username";
        options.TokenValidationParameters.RoleClaimType = "roles";
    });

builder
    .Services.AddOptions<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme)
    .Configure<IOptions<AuthOptions>>(
        (options, auth) =>
        {
            options.Authority = auth.Value.Authority;
            options.ClientId = auth.Value.ClientId;
            options.ClientSecret = auth.Value.ClientSecret;
            options.RequireHttpsMetadata = auth.Value.RequireHttpsMetadata;
        }
    );

builder
    .Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.Admin, p => p.RequireRole(Policies.Admin))
    .AddPolicy(Policies.Organizer, p => p.RequireRole(Policies.Organizer, Policies.Admin));

builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute())
);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("areas", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapDefaultControllerRoute();

app.Run();

public partial class Program;