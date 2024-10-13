using Elevator.Data.Context;
using Elevator.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elevator.Data.SeedData
{
    public class ElevatorSeedData
    {
        private readonly ElevatorDbContext _context;

        public ElevatorSeedData(ElevatorDbContext context) 
        {
            _context = context;
        }

        public void SeedElevatorData()
        {
            if (!_context.DbElevators.Any())
            {
                IEnumerable<dbElevator> elevators = new List<dbElevator>()
                {
                    new dbElevator()
                    {
                        CarName = "Elevator Car 1",
                        CurrentFloor = 10
                    },
                    new dbElevator()
                    {
                        CarName = "Elevator Car 2",
                        CurrentFloor = 10
                    },
                    new dbElevator()
                    {
                        CarName = "Elevator Car 3",
                        CurrentFloor = 10
                    },
                    new dbElevator()
                    {
                        CarName = "Elevator Car 4",
                        CurrentFloor = 10
                    },
                };

                _context.DbElevators.AddRange(elevators);
                _context.SaveChanges();
            }
        }
    }
}
