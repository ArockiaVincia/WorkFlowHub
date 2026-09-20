using Microsoft.Extensions.DependencyInjection;
using EmployeeWorkFlowHub.Repository.Interfaces;
using EmployeeWorkFlowHub.Repository.Repository;
using EmployeeWorkFlowHub.Service.Interfaces;
using EmployeeWorkFlowHub.Service.Services;
using EmployeeWorkFlowHub.Services;

namespace EmployeeWorkFlowHub
{
    /// <summary>
    /// Dependency Injection setup extensions.
    /// Registers Repositories, Services, and Utilities.
    /// </summary>
    public static class ServiceExtension
    {
        public static IServiceCollection AddDIServicesSetup(this IServiceCollection services)
        {
            // Repository Layer
            services.AddTransient<IAuthenticateRepository, AuthenticateRepository>();
            services.AddTransient<IDepartmentRepository, DepartmentRepository>();
            services.AddTransient<IEmployeeRepository, EmployeeRepository>();
            services.AddTransient<IProjectRepository, ProjectRepository>();
            services.AddTransient<ITaskRepository, TaskRepository>();
            services.AddTransient<ILookupRepository, LookupRepository>();

            // Service Layer
            services.AddTransient<IAuthenticateService, AuthenticateService>();
            services.AddTransient<IDepartmentService, DepartmentService>();
            services.AddTransient<IEmployeeService, EmployeeService>();
            services.AddTransient<IProjectService, ProjectService>();
            services.AddTransient<ITaskService, TaskService>();
            services.AddTransient<ILookupService, LookupService>();

            // Authentication & Token Services
            services.AddTransient<ITokenService, TokenService>();

            return services;
        }
    }
}
