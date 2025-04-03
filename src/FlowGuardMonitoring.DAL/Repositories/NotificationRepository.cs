using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlowGuardMonitoring.DAL.Data;
using FlowGuardMonitoring.DAL.Models;
using FlowGuardMonitoring.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FlowGuardMonitoring.DAL.Repositories
{
    public class NotificationRepository : IRepository<Notification>
    {
        private readonly FlowGuardMonitoringContext context;

        public NotificationRepository(FlowGuardMonitoringContext context)
        {
            this.context = context;
        }

        public async Task<List<Notification>> GetAllAsync()
        {
            return await this.context.Notifications.ToListAsync();
        }

        public async Task<List<Notification>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string sortColumn,
            string sortDirection,
            string searchValue,
            string userId,
            int? id = null)
        {
            var query = this.context.Notifications.AsQueryable();

            if (string.IsNullOrEmpty(userId))
            {
                return new List<Notification>();
            }

            query = query.Where(n => n.UserId == userId);
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(n => n.Title.Contains(searchValue) || n.Description.Contains(searchValue));
            }

            switch (sortColumn.ToLower())
            {
                case "message":
                    query = sortDirection == "asc" ? query.OrderBy(n => n.Description) : query.OrderByDescending(n => n.Description);
                    break;
                case "type":
                    query = sortDirection == "asc" ? query.OrderBy(n => n.Type) : query.OrderByDescending(n => n.Type);
                    break;
                default:
                    query = sortDirection == "asc" ? query.OrderBy(n => n.Date) : query.OrderByDescending(n => n.Date);
                    break;
            }

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(int id)
        {
            return await this.context.Notifications.FindAsync(id);
        }

        public async Task AddAsync(Notification entity)
        {
            await this.context.Notifications.AddAsync(entity);
            await this.context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Notification entity)
        {
            this.context.Notifications.Update(entity);
            await this.context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await this.GetByIdAsync(id);
            if (entity != null)
            {
                this.context.Notifications.Remove(entity);
                await this.context.SaveChangesAsync();
            }
        }

        public int GetCount(string userId, string searchValue = "")
        {
            var query = this.context.Notifications.AsQueryable();

            if (string.IsNullOrEmpty(userId))
            {
                return 0;
            }

            if (!string.IsNullOrEmpty(searchValue))
            {
                return query.Count(n =>
                    (n.Title.Contains(searchValue) || n.Description.Contains(searchValue))
                    && n.UserId == userId);
            }

            return query.Count(n => n.UserId == userId);
        }

        public int GetUnreadCount(string userId)
        {
            var query = this.context.Notifications.AsQueryable();
            if (userId.IsNullOrEmpty())
            {
                return 0;
            }

            return query.Count(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkAsRead(int notificationId)
        {
            var notification = await this.GetByIdAsync(notificationId);
            if (notification == null)
            {
                return;
            }

            notification.IsRead = true;
            await this.context.SaveChangesAsync();
        }
    }
}
