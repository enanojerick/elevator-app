using Elevator.Data.Entities;
using Elevator.Data.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Elevator.Data.Context
{
    public class ElevatorDbContext : DbContext, IContext
    {

        public ElevatorDbContext(DbContextOptions<ElevatorDbContext> options) : base(options)
        {

        }

        public virtual DbSet<dbElevator> DbElevators { get; set; }
        public virtual DbSet<dbElevatorProgress> DbElevatorProgress { get; set; }
        public virtual DbSet<dbElevatorRequest> DbElevatorsRequest { get; set; }

    }
}
