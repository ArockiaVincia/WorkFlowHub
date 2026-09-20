using EmployeeWorkFlowHub.Common;
using EmployeeWorkFlowHub.Models;
using EmployeeWorkFlowHub.Repository.Interfaces;
using EmployeeWorkFlowHub.Service.Interfaces;

namespace EmployeeWorkFlowHub.Service.Services
{
    /// <summary>
    /// Implements ITaskService for business logic and data orchestration.
    /// Enforces role-based business rules and returns ResultArgs.
    /// </summary>
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;

        public TaskService(ITaskRepository taskRepository, IProjectRepository projectRepository)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
        }

        public async Task<ResultArgs> GetAllAsync(string userRole, int currentEmployeeId)
        {
            var result = new ResultArgs();

            if (userRole.Equals(CommonVariable.RoleName.Manager, StringComparison.OrdinalIgnoreCase) ||
                userRole.Equals(CommonVariable.RoleName.Admin, StringComparison.OrdinalIgnoreCase))
            {
                var tasks = await _taskRepository.GetAllAsync();
                result.StatusCode = 200;
                result.StatusMessage = "Tasks retrieved successfully.";
                result.ResultData = tasks;
                return result;
            }
            else if (userRole.Contains("Lead", StringComparison.OrdinalIgnoreCase))
            {
                var tasks = await _taskRepository.GetByProjectLeadAsync(currentEmployeeId);
                result.StatusCode = 200;
                result.StatusMessage = "Tasks retrieved successfully.";
                result.ResultData = tasks;
                return result;
            }
            else if (userRole.Contains("QC", StringComparison.OrdinalIgnoreCase) ||
                     userRole.Contains("Quality", StringComparison.OrdinalIgnoreCase))
            {
                var tasks = await _taskRepository.GetForQCAsync();
                result.StatusCode = 200;
                result.StatusMessage = "QC tasks retrieved successfully.";
                result.ResultData = tasks;
                return result;
            }
            else if (userRole.Contains("Developer", StringComparison.OrdinalIgnoreCase) ||
                     userRole.Contains("Member", StringComparison.OrdinalIgnoreCase))
            {
                var tasks = await _taskRepository.GetByEmployeeAsync(currentEmployeeId);
                result.StatusCode = 200;
                result.StatusMessage = "Tasks retrieved successfully.";
                result.ResultData = tasks;
                return result;
            }

            result.StatusCode = 403;
            result.StatusMessage = "You do not have permission to view tasks.";
            return result;
        }

        public async Task<ResultArgs> GetByIdAsync(int id, string userRole, int currentEmployeeId)
        {
            var result = new ResultArgs();
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
            {
                result.StatusCode = 404;
                result.StatusMessage = $"Task with ID {id} was not found.";
                return result;
            }

            // Role access check
            if (userRole.Contains("Developer", StringComparison.OrdinalIgnoreCase) || userRole.Contains("Member", StringComparison.OrdinalIgnoreCase))
            {
                if (task.EmployeeId != currentEmployeeId)
                {
                    result.StatusCode = 403;
                    result.StatusMessage = "You can only view tasks assigned to you.";
                    return result;
                }
            }
            else if (userRole.Contains("Lead", StringComparison.OrdinalIgnoreCase))
            {
                if (task.ProjectId.HasValue)
                {
                    var proj = await _projectRepository.GetByIdAsync(task.ProjectId.Value);
                    if (proj != null && proj.ProjectManagerId != currentEmployeeId)
                    {
                        result.StatusCode = 403;
                        result.StatusMessage = "You can only view tasks in projects you lead.";
                        return result;
                    }
                }
            }

            result.StatusCode = 200;
            result.StatusMessage = "Task retrieved successfully.";
            result.ResultData = task;
            return result;
        }

        public async Task<ResultArgs> CreateAsync(TaskItem task, string userRole, int currentEmployeeId)
        {
            if (userRole.Contains("Developer", StringComparison.OrdinalIgnoreCase) ||
                userRole.Contains("Member", StringComparison.OrdinalIgnoreCase) ||
                userRole.Contains("QC", StringComparison.OrdinalIgnoreCase) ||
                userRole.Contains("Quality", StringComparison.OrdinalIgnoreCase))
            {
                return new ResultArgs { StatusCode = 403, StatusMessage = "Only Managers and Team Leads can create tasks." };
            }

            if (string.IsNullOrWhiteSpace(task.Title))
            {
                return new ResultArgs { StatusCode = 400, StatusMessage = "Task title is required." };
            }
            if (task.EmployeeId <= 0)
            {
                return new ResultArgs { StatusCode = 400, StatusMessage = "Assigned employee is required." };
            }

            if (userRole.Contains("Lead", StringComparison.OrdinalIgnoreCase) && task.ProjectId.HasValue)
            {
                var proj = await _projectRepository.GetByIdAsync(task.ProjectId.Value);
                if (proj != null && proj.ProjectManagerId != currentEmployeeId)
                {
                    return new ResultArgs { StatusCode = 403, StatusMessage = "Team Leads can only create tasks for projects they lead." };
                }
            }

            return await _taskRepository.InsertAsync(task);
        }

        public async Task<ResultArgs> UpdateAsync(TaskItem task, string userRole, int currentEmployeeId)
        {
            if (task.Id <= 0)
            {
                return new ResultArgs { StatusCode = 400, StatusMessage = "Valid task ID is required." };
            }

            var existing = await _taskRepository.GetByIdAsync(task.Id);
            if (existing == null)
            {
                return new ResultArgs { StatusCode = 404, StatusMessage = $"Task with ID {task.Id} was not found." };
            }

            // Developer / Team Member can only update status
            if (userRole.Contains("Developer", StringComparison.OrdinalIgnoreCase) || userRole.Contains("Member", StringComparison.OrdinalIgnoreCase))
            {
                if (existing.EmployeeId != currentEmployeeId)
                {
                    return new ResultArgs { StatusCode = 403, StatusMessage = "You can only update tasks assigned to you." };
                }

                existing.Status = task.Status;
                return await _taskRepository.UpdateAsync(existing);
            }

            // QC role can update status for QC stages
            if (userRole.Contains("QC", StringComparison.OrdinalIgnoreCase) || userRole.Contains("Quality", StringComparison.OrdinalIgnoreCase))
            {
                existing.Status = task.Status;
                return await _taskRepository.UpdateAsync(existing);
            }

            // Team Lead can update tasks in projects they lead
            if (userRole.Contains("Lead", StringComparison.OrdinalIgnoreCase))
            {
                if (existing.ProjectId.HasValue)
                {
                    var proj = await _projectRepository.GetByIdAsync(existing.ProjectId.Value);
                    if (proj != null && proj.ProjectManagerId != currentEmployeeId)
                    {
                        return new ResultArgs { StatusCode = 403, StatusMessage = "Team Leads can only update tasks in projects they lead." };
                    }
                }
            }

            return await _taskRepository.UpdateAsync(task);
        }

        public async Task<ResultArgs> UpdateStatusAsync(int id, string status, string userRole, int currentEmployeeId)
        {
            if (id <= 0)
            {
                return new ResultArgs { StatusCode = 400, StatusMessage = "Valid task ID is required." };
            }
            if (string.IsNullOrWhiteSpace(status))
            {
                return new ResultArgs { StatusCode = 400, StatusMessage = "Status is required." };
            }

            var existing = await _taskRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return new ResultArgs { StatusCode = 404, StatusMessage = $"Task with ID {id} was not found." };
            }

            if (userRole.Contains("Developer", StringComparison.OrdinalIgnoreCase) || userRole.Contains("Member", StringComparison.OrdinalIgnoreCase))
            {
                if (existing.EmployeeId != currentEmployeeId)
                {
                    return new ResultArgs { StatusCode = 403, StatusMessage = "You can only update tasks assigned to you." };
                }
            }

            existing.Status = status.Trim();
            return await _taskRepository.UpdateAsync(existing);
        }

        public async Task<ResultArgs> DeleteAsync(int id, string userRole, int currentEmployeeId)
        {
            var existing = await _taskRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return new ResultArgs { StatusCode = 404, StatusMessage = $"Task with ID {id} was not found." };
            }

            if (userRole.Equals(CommonVariable.RoleName.Manager, StringComparison.OrdinalIgnoreCase) ||
                userRole.Equals(CommonVariable.RoleName.Admin, StringComparison.OrdinalIgnoreCase))
            {
                return await _taskRepository.DeleteAsync(id);
            }
            else if (userRole.Contains("Lead", StringComparison.OrdinalIgnoreCase))
            {
                if (existing.ProjectId.HasValue)
                {
                    var proj = await _projectRepository.GetByIdAsync(existing.ProjectId.Value);
                    if (proj != null && proj.ProjectManagerId == currentEmployeeId)
                    {
                        return await _taskRepository.DeleteAsync(id);
                    }
                }
                return new ResultArgs { StatusCode = 403, StatusMessage = "Team Leads can only delete tasks belonging to projects they lead." };
            }

            return new ResultArgs { StatusCode = 403, StatusMessage = "You do not have permission to delete tasks." };
        }
    }
}
