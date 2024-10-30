using RedisSqlite.Data;
using RedisSqlite.Models;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;

namespace RedisSqlite.Services 
{
    public class CarService : ICarService
    {
        private readonly ApiDbContext _dbContext;
        private readonly IDatabase _db;
        public CarService(ApiDbContext dbContext, IConnectionMultiplexer connection)
        {
            _dbContext = dbContext;
            _db = connection.GetDatabase();
        }

        public async Task<IEnumerable<Car>> GetAllCarsAsync()
        {
            var redisData = _db.HashGetAll("cardb");
            if (redisData.Length == 0)
            {
                // Retrieve all cars from the database
                var data = await _dbContext.Cars.ToListAsync();
                
                // Store each car in Redis hash
                var hashEntries = data.Select(car => 
                    new HashEntry(car.Id, JsonSerializer.Serialize(car))
                ).ToArray();

                // Set the hash entries in Redis
                _db.HashSet("carall", hashEntries);

                // Set expiration for the hash key
                _db.KeyExpire("carall", TimeSpan.FromMinutes(1));

                return data; // Return the list from the database
            }

            
            // If data exists in Redis, deserialize and return it
            return redisData.Select(entry => 
                JsonSerializer.Deserialize<Car>(entry.Value)
            ).ToList();
           
        }

        public async Task<Car> GetCarByIdAsync(int id)
        {
            var redisData = _db.HashGetAll("cardb");
            return await _dbContext.Cars.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Car> AddCarAsync(Car car)
        {
            _dbContext.Cars.Add(car);
            _db.HashSet("cardb", new HashEntry[]
            {
                new HashEntry(car.Id, JsonSerializer.Serialize(car))
            });

            // Set expiration for the hash key
            _db.KeyExpire("cardb", TimeSpan.FromMinutes(2));

            await _dbContext.SaveChangesAsync();
            return car;
        }

        public async Task<Car?> UpdateCarAsync(int id, Car car)
        {
            
            var existingCar = await _dbContext.Cars.FirstOrDefaultAsync(c => c.Id == id);

            if (existingCar == null)
                return default;

            existingCar.Make = car.Make;
            existingCar.Model = car.Model;
            existingCar.Year = car.Year;
            existingCar.Color = car.Color;
            await _dbContext.SaveChangesAsync();
            return existingCar;
        }

        public async Task<Car> DeleteCarAsync(int id)
        {
            var car = await _dbContext.Cars.FirstOrDefaultAsync(c => c.Id == id);
            if (car != null)
            {
                _dbContext.Cars.Remove(car);
                _dbContext.SaveChangesAsync();
                _db.HashDelete("cardb",$"car:{id}");
            }
            return car;
        }

        public async Task<List<Car>> SearchCarsAsync(string keyword)
        {
            return await _dbContext.Cars.Where(c => c.Make.Contains(keyword) || c.Color.Contains(keyword)).ToListAsync();
        }
    }
}