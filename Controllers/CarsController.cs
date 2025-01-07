using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedisSqlite.Services;
using RedisSqlite.Services.Caching;
using RedisSqlite.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace RedisSqlite.Controllers 
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarsController : ControllerBase 
    {
        private readonly ICarService _carService; 
        private readonly IDatabase _database;
        public CarsController(ICarService carService, IConnectionMultiplexer connectionMultiplexer)
        {
            _carService = carService;
            _database = connectionMultiplexer.GetDatabase();
        }       

        [HttpGet("caraddredis/{id}")]
        public ActionResult<Car> GetCarFromAddDataToRedis(int id)
        {         
            var key = $"caradd:{id}";
            var carEntries = _database.HashGetAll(key);

            if (carEntries.Length == 0)
            {
                return NotFound(); // Return 404 if no data found
            }

            // Assuming each field in the hash corresponds to a property of the Car class
            var car = new Car
            {
                Id = id, // Set the ID from the URL parameter
                Make = carEntries.FirstOrDefault(x => x.Name == "Make").Value,
                Model = carEntries.FirstOrDefault(x => x.Name == "Model").Value,
                Year = int.TryParse(carEntries.FirstOrDefault(x => x.Name == "Year").Value, out var year) ? year : 0
            };

            return Ok(car); // Return the car object
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCars()
        {
            var cars = await _carService.GetAllCarsAsync();
            if (cars is not null)
            {
                return Ok(cars);
            }

            return Ok(cars);
        }

         [HttpGet("checkdb")]
        public async Task<IActionResult> GetAllCarsCheckDB()
        {
            var cars = await _carService.GetAllCarsEqualDbAsync();
            if (cars is not null)
            {
                return Ok(cars);
            }

            return Ok(cars);
        }

        [HttpGet("carid/{id}")]
        public async Task<IActionResult> GetCarById(int id)
        {
            var car = await _carService.GetCarByIdAsync(id);
            if (car == null)
            {
                return NotFound();
            }
            return Ok(car);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCar([FromBody] Car car)
        {
            var createdCar = await _carService.AddCarAsync(car);
            return CreatedAtAction(nameof(GetCarById), new { id = createdCar.Id }, createdCar);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCar(int id, [FromBody] Car car)
        {
            var updatedCar = await _carService.UpdateCarAsync(id, car);
            if (updatedCar == null)
            {
                return NotFound();
            }
            return Ok(updatedCar);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var deletedCar = await _carService.DeleteCarAsync(id);
            if (deletedCar == null)
            {
                return NotFound();
            }
            return Ok(deletedCar);
        }

        [HttpGet("{word}")]
        public async Task<IActionResult> SearchCar(string word)
        {
            var searchCar = await _carService.SearchCarsAsync(word);
            if (searchCar == null)
            {
                return NotFound();
            }
            return Ok(searchCar);
        }
    }
}