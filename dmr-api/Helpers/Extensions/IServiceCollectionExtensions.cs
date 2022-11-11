using AutoMapper;
using DMR_API._Repositories.Interface;
using DMR_API._Repositories.Repositories;
using DMR_API._Services.Interface;
using DMR_API._Services.Services;
using DMR_API.Data;
using DMR_API.Helpers.AutoMapper;
using DMR_API.SchedulerHelper;
using DMR_API.SchedulerHelper.Jobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;

namespace DMR_API.Helpers.Extensions
{
    public static class IServiceCollectionExtensions
    {

        public static IServiceCollection AddDatabaseExtention(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure DbContext with Scoped lifetime 
            var appsettings = configuration.GetSection("Appsettings").Get<Appsettings>();
            services.AddSingleton(appsettings);
            services.AddDbContext<DataContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            services.AddDbContext<IoTContext>(options => options.UseMySQL(configuration.GetConnectionString("IoTConnection")));

            services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));
            services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));
            services.AddSingleton<IMongoDbSettings>(serviceProvider =>
            serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value);
          
            return services;
        }

        public static IServiceCollection AddRepositoriesExtention(this IServiceCollection services)
        {
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IGlueIngredientRepository, GlueIngredientRepository>();
            services.AddScoped<IGlueRepository, GlueRepository>();
            services.AddScoped<IIngredientRepository, IngredientRepository>();
            services.AddScoped<IMakeGlueRepository, MakeGlueRepository>();
            services.AddScoped<IModelNameRepository, ModelNameRepository>();
            services.AddScoped<IUserDetailRepository, UserDetailRepository>();
            services.AddScoped<IPlanRepository, PlanRepository>();
            services.AddScoped<IPlanDetailRepository, PlanDetailRepository>();
            services.AddScoped<ILineRepository, LineRepository>();
            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<IProcessRepository, ProcessRepository>();
            services.AddScoped<IArtProcessRepository, ArtProcessRepository>();
            services.AddScoped<IProcessRepository, ProcessRepository>();

            services.AddScoped<IArticleNoRepository, ArticleNoRepository>();
            services.AddScoped<IBuildingRepository, BuildingRepository>();
            services.AddScoped<IBuildingUserRepository, BuildingUserRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IModelNoRepository, ModelNoRepository>();
            services.AddScoped<IKindRepository, KindRepository>();
            services.AddScoped<IPartRepository, PartRepository>();
            services.AddScoped<IMaterialRepository, MaterialRepository>();
            services.AddScoped<IModelNoRepository, ModelNoRepository>();
            services.AddScoped<IModelNoRepository, ModelNoRepository>();
            services.AddScoped<IBPFCEstablishRepository, BPFCEstablishRepository>();
            services.AddScoped<IMixingInfoRepository, MixingInfoRepository>();
            services.AddScoped<IMixingInfoDetailRepository, MixingInfoDetailRepository>();
            services.AddScoped<IMixingRepository, MixingRepository>();
            services.AddScoped<IIngredientInfoRepository, IngredientInfoRepository>();
            services.AddScoped<IIngredientInfoReportRepository, IngredientInfoReportRepository>();
            services.AddScoped<IBPFCHistoryRepository, BPFCHistoryRepository>();
            services.AddScoped<ISettingRepository, SettingRepository>();
            services.AddScoped<IStirRepository, StirRepository>();
            services.AddScoped<IAbnormalRepository, AbnormalRepository>();
            services.AddScoped<IRawDataRepository, RawDataRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IScaleMachineRepository, ScaleMachineRepository>();
            services.AddScoped<IDispatchRepository, DispatchRepository>();
            services.AddScoped<IDispatchListRepository, DispatchListRepository>();
            services.AddScoped<IGlueTypeRepository, GlueTypeRepository>();
            services.AddScoped<IToDoListRepository, ToDoListRepository>();
            services.AddScoped<IGlueNameRepository, GlueNameRepository>();
            services.AddScoped<IStationRepository, StationRepository>();
            services.AddScoped<ILunchTimeRepository, LunchTimeRepository>();
            services.AddScoped<IMailingRepository, MailingRepository>();
            services.AddScoped<IPeriodRepository, PeriodRepository>();
            services.AddScoped<IPeriodRepository, PeriodRepository>();
            services.AddScoped<IDispatchListDetailRepository, DispatchListDetailRepository>();
            services.AddScoped<ISubpackageRepository, SubpackageRepository>();
            services.AddScoped<IShakeRepository, ShakeRepository>();
            services.AddScoped<IPeriodDispatchRepository, PeriodDispatchRepository>();
            services.AddScoped<IPeriodMixingRepository, PeriodMixingRepository>();
            services.AddScoped<IKindTypeRepository, KindTypeRepository>();
            services.AddScoped<IBuildingTypeRepository, BuildingTypeRepository>();
            services.AddScoped<ISubpackageCapacityRepository, SubpackageCapacityRepository>();

            services.AddScoped<IActionRepository, ActionRepository>();
            services.AddScoped<IFunctionSystemRepository, FunctionSystemRepository>();
            services.AddScoped<IActionInFunctionSystemRepository, ActionInFunctionSystemRepository>();
            services.AddScoped<IFunctionSystemRepository, FunctionSystemRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IModuleRepository, ModuleRepository>();
            services.AddScoped<IVersionRepository, VersionRepository>();
            services.AddScoped<IStirRawDataRepository, StirRawDataRepository>();

            return services;
        }

        public static IServiceCollection AddServicesExtention(this IServiceCollection services)
        {
            services.AddScoped<IMixingService, MixingService>();
            services.AddScoped<IGlueIngredientService, GlueIngredientService>();
            services.AddScoped<IGlueService, GlueService>();
            services.AddScoped<IMakeGlueService, MakeGlueService>();
            services.AddScoped<IIngredientService, IngredientService>();
            services.AddScoped<IModelNameService, ModelNameService>();
            services.AddScoped<IUserDetailService, UserDetailService>();
            services.AddScoped<IPlanService, PlanService>();
            services.AddScoped<ILineService, LineService>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<IArticleNoService, ArticleNoService>();
            services.AddScoped<IBuildingService, BuildingService>();
            services.AddScoped<IBuildingUserService, BuildingUserService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IBPFCEstablishService, BPFCEstablishService>();
            services.AddScoped<IModelNoService, ModelNoService>();
            services.AddScoped<IArtProcessService, ArtProcessService>();
            services.AddScoped<IProcessService, ProcessService>();
            services.AddScoped<IKindService, KindService>();
            services.AddScoped<IPartService, PartService>();
            services.AddScoped<IMaterialService, MaterialService>();
            services.AddScoped<IMixingInfoService, MixingInfoService>();
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<IAbnormalService, AbnormalService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserRoleService, UserRoleService>();
            services.AddScoped<IScaleMachineService, ScaleMachineService>();

            services.AddScoped<IDispatchService, DispatchService>();

            services.AddScoped<IToDoListService, ToDoListService>(); 

            services.AddScoped<IGlueTypeService, GlueTypeService>();
            services.AddScoped<IShakeService, ShakeService>();

            services.AddScoped<IStirService, StirService>();
            services.AddScoped<IMailingService, MailingService>();
            services.AddScoped<IBuildingLunchTimeService, BuildingLunchTimeService>();
            services.AddScoped<IBottomFactoryService, BottomFactoryService>();
            services.AddScoped<ILunchTimeService, LunchTimeService>();
            services.AddScoped<ISubpackageCapacityService, SubpackageCapacityService>();

            services.AddScoped<IStationService, StationService>(); //  duy trì trạng thái trong một request
            services.AddScoped<IPermissionService, PermissionService>(); //  duy trì trạng thái trong một request
            //extension
            services.AddScoped<IMailExtension, MailExtension>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IJWTService, JWTService>();
            services.AddScoped<IVersionService, VersionService>();
            services.AddScoped<IStirRawDataService, StirRawDataService>();


            //Không bao giờ inject Scoped & Transient service vào Singleton service
            //Không bao giờ inject Transient Service vào Scope Service

            return services;
        }

        public static IServiceCollection AddShedulerExtention(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddQuartz(async q =>
            {
                q.SchedulerId = "dmr-api";

                // Thuc thi luc 6:00, lap lai 1 tieng 1 lan
                var option = new ReloadTodoJob();
                await new SchedulerBase<ReloadTodoJob>(configuration).Start(1, IntervalUnit.Hour, 6, 00);

                // Thuc thi luc 6:00 đên 21 gio la ngung lap lai 30 phut 1 lan
                var startAt = TimeSpan.FromHours(6);
                var endAt = TimeSpan.FromHours(21);
                var repeatMins = 30;
                await new SchedulerBase<ReloadDispatchJob>(configuration).Start(repeatMins, startAt, endAt);
            });

            // ASP.NET Core hosting
            services.AddQuartzServer(options =>
            {
                // when shutting down we want jobs to complete gracefully
                options.WaitForJobsToComplete = true;
            });
            return services;
        }


        public static IServiceCollection AddHttpClientExtention(this IServiceCollection services, IConfiguration configuration)
        {
            var appsettings = configuration.GetSection("Appsettings").Get<Appsettings>();
            services.AddHttpClient("default", client =>
            {
                client.BaseAddress = new Uri(appsettings.API_AUTH_URL);

                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            return services;
        }

        public static IServiceCollection AddAuthenticationWithSwaggerExtention(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // which means that the authentication requires HTTPS for the metadata address or authority.
                options.SaveToken = true; // which saves the JWT access token in the current HttpContext
                                          // we can retrieve it using the method await HttpContext.GetTokenAsync(“Bearer”, “access_token”)
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII
                        .GetBytes(configuration.GetSection("AppSettings:Token").Value)),
                    ValidateIssuer = false,
                    ValidateAudience = false

                    // ClockSkew = TimeSpan.FromMinutes(1) //  which gives an allowance time for the token expiration validation
                };
                //options.Events = new JwtBearerEvents
                //{
                //    OnMessageReceived = context =>
                //    {
                //        var accessToken = context.Request.Query["access_token"];
                //        var token = context.Request.Headers.FirstOrDefault(x => x.Key == "Authorization");
                //        // If the request is for our hub...
                //        var path = context.HttpContext.Request.Path;
                //        if (!string.IsNullOrEmpty(accessToken) &&
                //            (path.StartsWithSegments("/ec-hub")))
                //        {
                //            // Read the token out of the query string
                //            context.Token = accessToken;
                //        }
                //        return Task.CompletedTask;
                //    }
                //};
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Digital mixing room system", Version = "2.1.0" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
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

            });

            return services;
        }
        public static IServiceCollection AddAutoMapperExtention(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Startup));
            services.AddScoped<IMapper>(sp =>
            {
                return new Mapper(AutoMapperConfig.RegisterMappings());
            });
            services.AddSingleton(AutoMapperConfig.RegisterMappings());
            return services;
        }

        public static IServiceCollection AddCorsExtension(this IServiceCollection services, IConfiguration Configuration)
        {
            var appsettings = Configuration.GetSection("Appsettings").Get<Appsettings>();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.WithOrigins(appsettings.CorsPolicy
                    ) //register for client
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            return services;
        }

        public static IServiceCollection AddSpaExtention(this IServiceCollection services)
        {
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = @"wwwroot/ClientApp";
            });
            return services;
        }

        public static IServiceCollection AddRedisCacheExtention(this IServiceCollection services)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "localhost";
                options.InstanceName = "IoT";
            });
            return services;
        }
    }
}
