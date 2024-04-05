using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using Microsoft.EntityFrameworkCore;


//PostgreSQL
namespace BSTOServer.Databases
{
    public class PostgreSqlDb : DbContext
	{
		private readonly string _connectionString;
		public DbSet<PlaneFaultData> FaultsTable => Set<PlaneFaultData>();
		public PostgreSqlDb(string connectionString)
		{
			if (string.IsNullOrWhiteSpace(connectionString))
			{
				throw new ArgumentException($"\"{nameof(connectionString)}\" не может быть пустым или содержать только пробел.", nameof(connectionString));
			}
			_connectionString = connectionString;
			Database.EnsureCreated();
		}
		protected override void OnModelCreating(ModelBuilder modelBuilder) { }
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseNpgsql(_connectionString);
		}

		//CRUD Methods
		public async Task<IReadOnlyList<PlaneFaultData>> ReadAllFailuresAsync()
		{
			return await FaultsTable.ToListAsync();
		}
		public async Task AddPlaneFault(PlaneFaultData fault)
		{
			if (fault is null)
			{
				throw new ArgumentNullException(nameof(fault));
			}

			await FaultsTable.AddAsync(fault);
			await this.SaveChangesAsync();
		}
		public async Task<PlaneFaultData?> ReadPlaneFaultByIdAsync(Guid Id)
		{
			if (Id == Guid.Empty) { throw new ArgumentOutOfRangeException(nameof(Id)); }

			var planeFault = await FaultsTable.FirstOrDefaultAsync(f => f.Id == Id);
			return planeFault;
		}
		public async Task<bool> UpdatePlaneFaultAsync(PlaneFaultData fault)
		{
			if (fault is null)
			{
				throw new ArgumentNullException(nameof(fault));
			}

			var oldFault = await ReadPlaneFaultByIdAsync(fault.Id);
			if (oldFault is null) { return false; }

			oldFault.FaultCode = fault.FaultCode;
			oldFault.FaultMessage = fault.FaultMessage;
			oldFault.PlaneIdentificator = fault.PlaneIdentificator;
			oldFault.FromPlace = fault.FromPlace;
			oldFault.ToPlace = fault.ToPlace;
			oldFault.RecordUTCDate = fault.RecordUTCDate;

			await this.SaveChangesAsync();
			return true;
		}
		public async Task<bool> RemoveByGuidPlaneFaultAsync(Guid Id)
		{
			if (Id == Guid.Empty) { throw new ArgumentOutOfRangeException(nameof(Id)); }

			var fault = await ReadPlaneFaultByIdAsync(Id);
			if (fault is null) { return false; }

			FaultsTable.Remove(fault);
			await this.SaveChangesAsync();
			return true;
		}

		public async Task<bool> CheckData(PlaneFaultData newFault)
		{
			var obj = await FaultsTable.FirstOrDefaultAsync(f => f.PlaneIdentificator == newFault.PlaneIdentificator
															&& f.FromPlace == newFault.FromPlace && f.ToPlace == newFault.ToPlace
															&& f.FaultMessage == newFault.FaultMessage
															&& DateTime.UtcNow - f.RecordUTCDate < TimeSpan.FromMinutes(30));
			//такое объект не найден (проверку прошел)
			if (obj is null) return true;
			return false;
        }
	}
}
