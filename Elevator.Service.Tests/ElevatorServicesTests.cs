using Elevator.Contracts;
using Elevator.Data.Context;
using Elevator.Data.Repository.Interface;
using Elevator.Data.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Elevator.Data.SeedData;
using Elevator.Data.Entities;
using Elevator.Dto;
using Elevator.Dto.Enums;
using Azure.Core;

namespace Elevator.Service.Tests
{
    public class ElevatorServicesTests
    {
        private IElevatorServices _ElevatorServices;
        private IRepository<dbElevator> _DbElevators;

        public ElevatorServicesTests()
        {
            TestSetup();
        }

        private void TestSetup()
        {
            var services = new ServiceCollection();
            services.AddDbContext<ElevatorDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            services.AddTransient<ElevatorSeedData>();

            services.AddTransient<IElevatorServices, ElevatorServices>();
            services.AddScoped(typeof(IRepository<>), typeof(DataRepository<>));
            services.AddTransient<IContext, ElevatorDbContext>();

            var serviceProvider = services.BuildServiceProvider();

            _ElevatorServices = serviceProvider.GetService<IElevatorServices>();
            _DbElevators = serviceProvider.GetService<IRepository<dbElevator>>();

            FeedElevatorData();
        }

        private void FeedElevatorData()
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

            foreach (var item in elevators)
            {
                _DbElevators.Insert(item);
            }
        }

        [Fact]
        public void GetElevators_GetDataStored()
        {
            var elevators = _ElevatorServices.GetElevators();

            Assert.Equal(4, elevators.Count());
            Assert.NotNull(elevators.Where(e => e.CarName == "Elevator Car 1"));
            Assert.NotNull(elevators.Where(e => e.CarName == "Elevator Car 2"));
            Assert.NotNull(elevators.Where(e => e.CarName == "Elevator Car 3"));
            Assert.NotNull(elevators.Where(e => e.CarName == "Elevator Car 4"));
        }

        [Fact]
        public void GetElevatorById_GetValidElevatorData_ReturnElevatorData()
        {
            var elevators = _ElevatorServices.GetElevators();

            var elevator1 = elevators.Where(e => e.CarName == "Elevator Car 1").FirstOrDefault();

            var elevatorData = _ElevatorServices.GetElevatorById(elevator1.CarId.Value);

            Assert.NotNull(elevatorData);
            Assert.Equal(elevator1.CarId, elevatorData.CarId);
        }

        [Fact]
        public void GetElevatorById_GetInvalidElevatorData_ReturnNull()
        {

            var elevatorData = _ElevatorServices.GetElevatorById(99999);

            Assert.Null(elevatorData);
        }

        [Fact]
        public void ResetElevatorFloor_ResetElevator1_ReturnElevatorRequestData()
        {
            var elevator = _ElevatorServices.GetElevatorById(1);

            Assert.NotNull(elevator);

            var request = new ElevatorRequestDto()
            {
                CarId = elevator.CarId.Value,
                RequestedDirection = (int)ElevatorDirectionEnum.DOWN,
                RequestedFromFloor = 5,
                RequestedFloors = [ 4 ]
            };

            _ElevatorServices.MoveElevator(request, elevator, true);

            elevator = _ElevatorServices.GetElevatorById(1);

            //Elevator current floor last stop
            Assert.Equal(4, elevator.CurrentFloor);

            _ElevatorServices.ResetElevatorFloor(elevator.CarId.Value);

            elevator = _ElevatorServices.GetElevatorById(1);

            Assert.Equal(10, elevator.CurrentFloor);

        }

        [Fact]
        public void ResetElevatorFloor_ResetInvalidElevator_ReturnNull()
        {
            var result = _ElevatorServices.ResetElevatorFloor(99999);

            Assert.Null(result);
        }

        [Fact]
        public void QueueElevatorRequest_RequestForElevator2_ReturnMessage()
        {
            var elevator = _ElevatorServices.GetElevatorById(2);

            Assert.NotNull(elevator);

            var elevatorRequest = new ElevatorRequestDto()
            {
                CarId = elevator.CarId.Value,
                RequestedDirection = (int)ElevatorDirectionEnum.DOWN,
                RequestedFromFloor = 5,
                RequestedFloors = [4]
            };

            var request = _ElevatorServices.QueueElevatorRequest(elevatorRequest);

            Assert.Equal("Success: Queue Added to : " + elevator.CarName, request);
        }

        [Fact]
        public void QueueElevatorRequest_ResetInvalidElevator_ReturnErrorMessage()
        {
            var elevatorRequest = new ElevatorRequestDto()
            {
                CarId = 99999,
                RequestedDirection = (int)ElevatorDirectionEnum.DOWN,
                RequestedFromFloor = 5,
                RequestedFloors = [4]
            };

            var result = _ElevatorServices.QueueElevatorRequest(elevatorRequest);

            Assert.Equal("Elevator does not exists - cannot make requests", result);
        }

        [Fact]
        public void QueueElevatorRequest_RequestedFromFloor_InvalidRequestAbove10thFloor_ReturnErrorMessage()
        {
            var elevator = _ElevatorServices.GetElevatorById(2);

            Assert.NotNull(elevator);

            var elevatorRequest = new ElevatorRequestDto()
            {
                CarId = elevator.CarId.Value,
                RequestedDirection = (int)ElevatorDirectionEnum.DOWN,
                RequestedFromFloor = 11,
                RequestedFloors = [4]
            };

            var result = _ElevatorServices.QueueElevatorRequest(elevatorRequest);

            Assert.Equal("Request Invalid Floors - cannot exceed 10th floor", result);
        }

        [Fact]
        public void QueueElevatorRequest_RequestedFromFloor_InvalidRequestBelow1stFloor_ReturnErrorMessage()
        {
            var elevator = _ElevatorServices.GetElevatorById(2);

            Assert.NotNull(elevator);

            var elevatorRequest = new ElevatorRequestDto()
            {
                CarId = elevator.CarId.Value,
                RequestedDirection = (int)ElevatorDirectionEnum.DOWN,
                RequestedFromFloor = 0,
                RequestedFloors = [4]
            };

            var result = _ElevatorServices.QueueElevatorRequest(elevatorRequest);

            Assert.Equal("Request Invalid Floors - does not have floors below 1st floor", result);
        }

        [Fact]
        public void QueueElevatorRequest_RequestedFloors_InvalidRequestAbove10thFloor_ReturnErrorMessage()
        {
            var elevator = _ElevatorServices.GetElevatorById(2);

            Assert.NotNull(elevator);

            var elevatorRequest = new ElevatorRequestDto()
            {
                CarId = elevator.CarId.Value,
                RequestedDirection = (int)ElevatorDirectionEnum.DOWN,
                RequestedFromFloor = 5,
                RequestedFloors = [4,11]
            };

            var result = _ElevatorServices.QueueElevatorRequest(elevatorRequest);

            Assert.Equal("Request Invalid Floors - cannot exceed 10th floor", result);
        }

        [Fact]
        public void QueueElevatorRequest_RequestedFloors_InvalidRequestBelow1stFloor_ReturnErrorMessage()
        {
            var elevator = _ElevatorServices.GetElevatorById(2);

            Assert.NotNull(elevator);

            var elevatorRequest = new ElevatorRequestDto()
            {
                CarId = elevator.CarId.Value,
                RequestedDirection = (int)ElevatorDirectionEnum.DOWN,
                RequestedFromFloor = 5,
                RequestedFloors = [4,0]
            };

            var result = _ElevatorServices.QueueElevatorRequest(elevatorRequest);

            Assert.Equal("Request Invalid Floors - does not have floors below 1st floor", result);
        }

        [Fact]
        public void QueueElevatorRequest_Direction_InvalidRequestDirection_ReturnErrorMessage()
        {
            var elevator = _ElevatorServices.GetElevatorById(2);

            Assert.NotNull(elevator);

            var elevatorRequest = new ElevatorRequestDto()
            {
                CarId = elevator.CarId.Value,
                RequestedDirection = 9,
                RequestedFromFloor = 5,
                RequestedFloors = [4]
            };

            var result = _ElevatorServices.QueueElevatorRequest(elevatorRequest);

            Assert.Equal("Request Elevator Direction - Up(1) and Down(2) are the only options", result);
        }

        [Fact]
        public void MoveElevator_Elevator3_ReturnElevatorUpdatedData()
        {
            var elevator = _ElevatorServices.GetElevatorById(3);

            Assert.NotNull(elevator);

            var request = new ElevatorRequestDto()
            {
                CarId = elevator.CarId.Value,
                RequestedDirection = (int)ElevatorDirectionEnum.DOWN,
                RequestedFromFloor = 5,
                RequestedFloors = [4]
            };

            var result = _ElevatorServices.MoveElevator(request, elevator, true);

            elevator = _ElevatorServices.GetElevatorById(3);

            //Elevator current floor last stop
            Assert.Equal(4, elevator.CurrentFloor);
            Assert.Equal("Done: " + elevator.CarName + " is now Stopped", result);
        }

        [Fact]
        public void MoveElevator_RequestedFromFloor_InvalidRequestAbove10thFloor_ReturnErrorMessage()
        {
            var elevator = _ElevatorServices.GetElevatorById(4);

            Assert.NotNull(elevator);

            var request = new ElevatorRequestDto()
            {
                CarId = elevator.CarId.Value,
                RequestedDirection = (int)ElevatorDirectionEnum.DOWN,
                RequestedFromFloor = 11,
                RequestedFloors = [4]
            };

            var result = _ElevatorServices.MoveElevator(request, elevator);

            elevator = _ElevatorServices.GetElevatorById(3);

            //Elevator current floor last stop
            Assert.Equal(10, elevator.CurrentFloor);
            Assert.Equal("Request Invalid Floors - cannot exceed 10th floor", result);
        }

        [Fact]
        public void MoveElevator_RequestedFromFloor_InvalidRequestBelow1stFloor_ReturnErrorMessage()
        {
            var elevator = _ElevatorServices.GetElevatorById(4);

            Assert.NotNull(elevator);

            var request = new ElevatorRequestDto()
            {
                CarId = elevator.CarId.Value,
                RequestedDirection = (int)ElevatorDirectionEnum.DOWN,
                RequestedFromFloor = 0,
                RequestedFloors = [4]
            };

            var result = _ElevatorServices.MoveElevator(request, elevator);

            elevator = _ElevatorServices.GetElevatorById(3);

            //Elevator current floor last stop
            Assert.Equal(10, elevator.CurrentFloor);
            Assert.Equal("Request Invalid Floors - does not have floors below 1st floor", result);
        }

        [Fact]
        public void MoveElevator_RequestedFloors_InvalidRequestAbove10thFloor_ReturnErrorMessage()
        {
            var elevator = _ElevatorServices.GetElevatorById(4);

            Assert.NotNull(elevator);

            var request = new ElevatorRequestDto()
            {
                CarId = elevator.CarId.Value,
                RequestedDirection = (int)ElevatorDirectionEnum.DOWN,
                RequestedFromFloor = 5,
                RequestedFloors = [4,11]
            };

            var result = _ElevatorServices.MoveElevator(request, elevator);

            elevator = _ElevatorServices.GetElevatorById(3);

            //Elevator current floor last stop
            Assert.Equal(10, elevator.CurrentFloor);
            Assert.Equal("Request Invalid Floors - cannot exceed 10th floor", result);
        }

        [Fact]
        public void MoveElevator_RequestedFloors_InvalidRequestBelow1stFloor_ReturnErrorMessage()
        {
            var elevator = _ElevatorServices.GetElevatorById(4);

            Assert.NotNull(elevator);

            var request = new ElevatorRequestDto()
            {
                CarId = elevator.CarId.Value,
                RequestedDirection = (int)ElevatorDirectionEnum.DOWN,
                RequestedFromFloor = 5,
                RequestedFloors = [4,0]
            };

            var result = _ElevatorServices.MoveElevator(request, elevator);

            elevator = _ElevatorServices.GetElevatorById(3);

            //Elevator current floor last stop
            Assert.Equal(10, elevator.CurrentFloor);
            Assert.Equal("Request Invalid Floors - does not have floors below 1st floor", result);
        }

        [Fact]
        public void MoveElevator_Direction_InvalidRequestDirection_ReturnErrorMessage()
        {
            var elevator = _ElevatorServices.GetElevatorById(4);

            Assert.NotNull(elevator);

            var request = new ElevatorRequestDto()
            {
                CarId = elevator.CarId.Value,
                RequestedDirection = 9,
                RequestedFromFloor = 5,
                RequestedFloors = [4]
            };

            var result = _ElevatorServices.MoveElevator(request, elevator, true);

            elevator = _ElevatorServices.GetElevatorById(3);

            //Elevator current floor last stop
            Assert.Equal(10, elevator.CurrentFloor);
            Assert.Equal("Request Elevator Direction - Up(1) and Down(2) are the only options", result);
        }
    }
}