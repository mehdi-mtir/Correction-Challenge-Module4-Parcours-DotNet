using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ReservSalles.Api.Data;
using ReservSalles.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services ----------
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

// Toutes les erreurs au format ProblemDetails (tâche A5)
builder.Services.AddProblemDetails();

// EF Core + SQLite (tâche A1)
builder.Services.AddDbContext<ReservSallesContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ReservSalles")));

// JWT : clé, émetteur et audience lus dans la configuration (tâche A3)
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<ServiceJeton>();

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
          ?? throw new InvalidOperationException("Section Jwt absente de la configuration");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;   // garder les noms de claims du jeton ("name", "sub")
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Emetteur,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwt.CleDeSignature(),
            NameClaimType = "name",
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });
builder.Services.AddAuthorization();

// Swagger avec le bouton Authorize
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ReservSalles API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Collez le jeton obtenu par POST /api/auth/login",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ---------- Base et données de départ ----------
using (var scope = app.Services.CreateScope())
{
    var contexte = scope.ServiceProvider.GetRequiredService<ReservSallesContext>();
    DonneesDepart.Initialiser(contexte, DateTime.Today, app.Logger);
}

// ---------- Pipeline ----------
app.UseExceptionHandler();   // exception non gérée → 500 ProblemDetails, sans trace de pile
app.UseStatusCodePages();    // 401 / 403 sans corps → ProblemDetails

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Jamais en développement : la redirection ferait perdre l'en-tête Authorization
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Rend la classe Program visible pour d'éventuels tests d'intégration (WebApplicationFactory)
public partial class Program { }
