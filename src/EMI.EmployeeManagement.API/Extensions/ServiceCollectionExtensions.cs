using AutoMapper;
using EMI.EmployeeManagement.API.Automapper;
using EMI.EmployeeManagement.BLL;
using EMI.EmployeeManagement.BLL.Interfaces;
using EMI.EmployeeManagement.DAL;
using EMI.EmployeeManagement.DAL.Interfaces;
using EMI.EmployeeManagement.DAL.UnitOfWorks;
using EMI.EmployeeManagement.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace EMI.EmployeeManagement.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddServices(this IServiceCollection services, IConfiguration configuration)
        {

            // Register BLL
            services.AddScoped<IEmployeeBLL, EmployeeBLL>();
            services.AddScoped<IPositionHistoryBLL, PositionHistoryBLL>();
            services.AddScoped<IAuthBLL, AuthBLL>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register Repository
            services.AddScoped<IEmployeeDAL, EmployeeDAL>();
            services.AddScoped<IPositionHistoryDAL, PositionHistoryDAL>();
            services.AddScoped<IAuthDAL, AuthDAL>();


            //Adding the DbContext
            services.AddDbContext(configuration);
            services.AddMapper();
            services.AddPolicies();
        }

        //Register AppDbContext here
        private static void AddDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("PostgreSql")));
        }


        private static void AddMapper(this IServiceCollection services)
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfileMasterModule());
            });
            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
        }

        private static void AddPolicies(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("newPolicy", app =>
                {
                    app.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
            });
        }
    }
}