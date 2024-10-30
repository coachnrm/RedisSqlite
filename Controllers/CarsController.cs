using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedisSqlite.Services;
using RedisSqlite.Services.Caching;
using RedisSqlite.Models;

namespace RedisSqlite.Controllers 
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarsController : ControllerBase 
    {
        private readonly ICarService _carService; 
        public CarsController(ICarService carService)
        {
            _carService = carService;
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

        [HttpGet("{id}")]
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