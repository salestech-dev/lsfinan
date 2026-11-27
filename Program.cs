using System.Globalization;
using lsfinan.Data;
using lsfinan.Infrastructure;
using lsfinan.Interfaces;
using lsfinan.Models;
using lsfinan.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Resend;

var builder = WebApplication.CreateBuilder(args);

var resendApiKey = Environment.GetEnvironmentVariable("RESEND_APITOKEN");

if (string.IsNullOrWhiteSpace(resendApiKey))
{
    throw new InvalidOperationException("A chave da API do Resend não foi configurada.");
}

builder.Services.AddOptions();
builder.Services.AddHttpClient<IResend, ResendClient>();
builder.Services.Configure<ResendClientOptions>(options =>
{
    options.ApiToken = resendApiKey;
});

builder.Services.AddScoped<IEmailService, EmailService>();

// Sistema em português do Brasil: R$, datas dd/MM/yyyy e vírgula decimal
// iguais em qualquer servidor.
var ptBr = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = ptBr;
CultureInfo.DefaultThreadCurrentUICulture = ptBr;

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // valores em dinheiro aceitam "100.50" e "100,50"
    options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
});

var connectionString =
    Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<Usuario, IdentityRole<int>>(opt =>
    {
        opt.Password.RequireDigit = false;
        opt.Password.RequireLowercase = false;
        opt.Password.RequireNonAlphanumeric = false;
        opt.Password.RequireUppercase = false;
        opt.Password.RequiredLength = 6;
        opt.SignIn.RequireConfirmedEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>();

// Sem isso, quem não está logado era mandado para /Account/Login (que não existe).
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Usuario/Login";
    options.AccessDeniedPath = "/Usuario/Login";
});

builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IEntradaService, EntradasService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IContaService, ContaService>();
builder.Services.AddScoped<ISaidaService, SaidaService>();
builder.Services.AddScoped<IContaPagarService, ContaPagarService>();
builder.Services.AddScoped<ICadastroPendenteService, CadastroPendenteService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
