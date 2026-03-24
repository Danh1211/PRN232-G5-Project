using Microsoft.EntityFrameworkCore;
using PRN232_BE;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using PRN232_BE.BackgroundJobs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CloneEbayDb1Context>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("MyDB")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHostedService<AutoReleaseOrdersService>();
// 2. C?u hình JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = builder.Configuration["Jwt:Issuer"], // L?y t? appsettings.json
			ValidAudience = builder.Configuration["Jwt:Audience"], // L?y t? appsettings.json
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])) // L?y t? appsettings.json
		};
	});

// 3. C?u hình Swagger ?? h? tr? nh?p JWT Token
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ebay Clone API", Version = "v1" });

	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Description = "JWT Authorization header using the Bearer scheme. Hãy nh?p Token c?a b?n vào ô bên d??i.\r\n\r\nVí d?: 'eyJhbGciOiJIUzI1NiIs...'",
		Name = "Authorization",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.Http,
		Scheme = "bearer"
	});

	c.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			new string[] {}
		}
	});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 4. KÍCH HO?T AUTHENTICATION (B?t bu?c ph?i n?m TR??C UseAuthorization)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();