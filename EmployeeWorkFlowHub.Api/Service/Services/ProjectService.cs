using EmployeeWorkFlowHub.Common;
using EmployeeWorkFlowHub.Models;
using EmployeeWorkFlowHub.Repository.Interfaces;
using EmployeeWorkFlowHub.Service.Interfaces;

namespace EmployeeWorkFlowHub.Service.Services
{
    /// <summary>
    /// Implements IProjectService for business logic and data orchestration.
    /// Enforces role-based business rules and returns ResultArgs.
    /// </summary>
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectService(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<ResultArgs> GetAllAsync(string userRole, int currentEmployeeId)
        {
            var result = new ResultArgs();

            if (userRole.Equals(CommonVariable.RoleName.Manager, StringComparison.OrdinalIgnoreCase) ||
                userRole.Equals(CommonVariable.RoleName.Admin, StringComparison.OrdinalIgnoreCase))
            {
                var projects = await _projectRepository.GetAllAsync();
                result.StatusCode = 200;
                result.StatusMessage = "Projects retrieved successfully.";
                result.ResultData = projects;
                return result;
            }
            else if (userRole.Contains("Lead", StringComparison.OrdinalIgnoreCase))
            {
                var projects = await _projectRepository.GetByManagerIdAsync(currentEmployeeId);
                result.StatusCode = 200;
                result.StatusMessage = "Projects retrieved successfully.";
                result.ResultData = projects;
                return result;
            }

            result.StatusCode = 403;
            result.StatusMessage = "You do not have permission to view projects.";
            return result;
        }

        public async Task<ResultArgs> GetByIdAsync(int id, string userRole, int currentEmployeeId)
        {
            var result = new ResultArgs();
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                result.StatusCode = 404;
                result.StatusMessage = $"Project with ID {id} was not found.";
                return result;
            }

            if (userRole.Contains("Lead", StringComparison.OrdinalIgnoreCase) && project.ProjectManagerId != currentEmployeeId)
            {
                result.StatusCode = 403;
                result.StatusMessage = "You can only view projects you manage.";
                return result;
            }

            result.StatusCode = 200;
            result.StatusMessage = "Project retrieved successfully.";
            result.ResultData = project;
            return result;
        }

        public async Task<ResultArgs> CreateAsync(Project project, string userRole)
        {
            if (!userRole.Equals(CommonVariable.RoleName.Manager, StringComparison.OrdinalIgnoreCase) &&
                !userRole.Equals(CommonVariable.RoleName.Admin, StringComparison.OrdinalIgnoreCase))
            {
                return new ResultArgs { StatusCode = 403, StatusMessage = "Only Managers can create projects." };
            }

            if (string.IsNullOrWhiteSpace(project.Name))
            {
                return new ResultArgs { StatusCode = 400, StatusMessage = "Project name is required." };
            }
            if (project.DepartmentId <= 0)
            {
                return new ResultArgs { StatusCode = 400, StatusMessage = "Valid department is required." };
            }
            if (project.StartDate.HasValue && project.EndDate.HasValue && project.EndDate.Value < project.StartDate.Value)
            {
                return new ResultArgs { StatusCode = 400, StatusMessage = "End Date cannot be earlier than Start Date." };
            }

            return await _projectRepository.InsertAsync(project);
        }

        public async Task<ResultArgs> UpdateAsync(Project project, string userRole, int currentEmployeeId)
        {
            if (project.Id <= 0)
            {
                return new ResultArgs { StatusCode = 400, StatusMessage = "Valid project ID is required." };
            }
            if (string.IsNullOrWhiteSpace(project.Name))
            {
                return new ResultArgs { StatusCode = 400, StatusMessage = "Project name is required." };
            }
            if (project.StartDate.HasValue && project.EndDate.HasValue && project.EndDate.Value < project.StartDate.Value)
            {
                return new ResultArgs { StatusCode = 400, StatusMessage = "End Date cannot be earlier than Start Date." };
            }

            var existing = await _projectRepository.GetByIdAsync(project.Id);
            if (existing == null)
            {
                return new ResultArgs { StatusCode = 404, StatusMessage = $"Project with ID {project.Id} was not found." };
            }

            if (userRole.Contains("Lead", StringComparison.OrdinalIgnoreCase) && existing.ProjectManagerId != currentEmployeeId)
            {
                return new ResultArgs { StatusCode = 403, StatusMessage = "Team Leads can only update projects they manage." };
            }

            return await _projectRepository.UpdateAsync(project);
        }

        public async Task<ResultArgs> DeleteAsync(int id, string userRole)
        {
            if (!userRole.Equals(CommonVariable.RoleName.Manager, StringComparison.OrdinalIgnoreCase) &&
                !userRole.Equals(CommonVariable.RoleName.Admin, StringComparison.OrdinalIgnoreCase))
            {
                return new ResultArgs { StatusCode = 403, StatusMessage = "Only Managers can delete projects." };
            }

            var existing = await _projectRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return new ResultArgs { StatusCode = 404, StatusMessage = $"Project with ID {id} was not found." };
            }

            return await _projectRepository.DeleteAsync(id);
        }
    }
}
