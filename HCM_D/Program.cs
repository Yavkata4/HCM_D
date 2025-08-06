using HCM_D.Data;
using HCM_D.Services;
using HCM_D.Middleware;
using HCM_D.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Reflection;
using System.Text.Json;

namespace HCM_D
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Register custom services
            builder.Services.AddScoped<EmployeeNumberService>();
            
            builder.Services.AddRazorPages();

            // Add API Controllers
            builder.Services.AddControllers();

            // Add Health Checks
            builder.Services.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("database")
                .AddCheck<EmployeeDataHealthCheck>("employee_data")
                .AddDbContextCheck<ApplicationDbContext>("efcore");

            // Add Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ElevateHR Solutions API",
                    Version = "v1.0",
                    Description = "A comprehensive Human Capital Management (HCM) API for managing employees, departments, and salary information. This API provides secure, role-based access to HR data with comprehensive CRUD operations.",
                    Contact = new OpenApiContact
                    {
                        Name = "ElevateHR Support",
                        Email = "support@elevatehr.com",
                        Url = new Uri("https://elevatehr.com")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    }
                });

                // Include XML comments for better documentation
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                // Add security definition for authentication
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });

                // Add response examples
                c.EnableAnnotations();
            });

            // Add comprehensive logging
            builder.Services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
                
                if (builder.Environment.IsDevelopment())
                {
                    logging.SetMinimumLevel(LogLevel.Debug);
                }
                else
                {
                    logging.SetMinimumLevel(LogLevel.Information);
                }
            });

            // Add CORS for API access
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("ApiPolicy", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // Authorization policies
            builder.Services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeFolder("/Employees");
                options.Conventions.AuthorizeFolder("/Departments");
                options.Conventions.AuthorizeFolder("/SalaryHistories");
                options.Conventions.AuthorizeFolder("/CompanyGrowth");
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
                
                // Enable Swagger in development
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ElevateHR Solutions API v1.0");
                    c.RoutePrefix = "swagger";
                    c.DocumentTitle = "ElevateHR Solutions API Documentation";
                    c.DefaultModelsExpandDepth(-1);
                    c.DisplayRequestDuration();
                    c.EnableFilter();
                    c.EnableDeepLinking();
                    c.DefaultModelExpandDepth(2);
                });
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // Add custom middleware
            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseMiddleware<GlobalExceptionMiddleware>();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors("ApiPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            // Map health checks
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var response = new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(x => new
                        {
                            name = x.Key,
                            status = x.Value.Status.ToString(),
                            description = x.Value.Description,
                            data = x.Value.Data
                        }),
                        duration = report.TotalDuration
                    };
                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                }
            });

            app.MapRazorPages();
            app.MapControllers();

            // Initialize database and seed data
            using (var scope = app.Services.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                try
                {
                    logger.LogInformation("Starting database initialization...");
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    await DbInitializer.SeedRolesAndAdminAsync(app);
                    DbInitializer.SeedEmployees(context);
                    logger.LogInformation("Database initialization completed successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while initializing the database.");
                    throw;
                }
            }

            app.Logger.LogInformation("ElevateHR Solutions application started successfully");
            app.Logger.LogInformation("API Documentation available at: /swagger");
            app.Logger.LogInformation("Health Checks available at: /health");
            
            app.Run();
        }
    }
}
