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
            var redisData = _db.HashGetAll("carall");
            //var redisData = _db.HashGetAll("cardb"); if get from post in Redis
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

        public async Task<Car?> GetCarByIdAsync(int id)
        {
            Car DataFromDb =  await _dbContext.Cars.FirstOrDefaultAsync(x => x.Id == id);

            if (DataFromDb != null)
            {
                // Serialize the car object and store it in Redis
                _db.HashSet($"car{DataFromDb.Id}", new HashEntry[]
                {
                    new HashEntry(DataFromDb.Id, JsonSerializer.Serialize(DataFromDb))
                });

                // Set expiration for the hash key
                _db.KeyExpire($"car{DataFromDb.Id}", TimeSpan.FromMinutes(1));
            }       

            return DataFromDb;
        }

        public async Task<Car> AddCarAsync(Car car)
        {
            try
            {
                _dbContext.Cars.Add(car);
                await _dbContext.SaveChangesAsync();

                var serializedCar = JsonSerializer.Serialize(car);
                // Serialize the car object and store it in Redis
                await _db.HashSetAsync($"caradd:{car.Id}", new HashEntry[] 
                {
                    new HashEntry(car.Id, serializedCar)
                });

                // Set expiration for the hash key
                await _db.KeyExpireAsync($"caradd:{car.Id}", TimeSpan.FromMinutes(1));

                return car;
            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., log it)
                throw; // or return an appropriate result
            }
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

            _db.HashSet($"carupdate{id}", new HashEntry[]
            {
                new HashEntry(car.Id, JsonSerializer.Serialize(existingCar))
            });

            // Set expiration for the hash key
            _db.KeyExpire($"carupdate{id}", TimeSpan.FromMinutes(1));

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
                //_db.HashDelete("cardb",$"car:{id}");
            }
            return car;
        }

        public async Task<List<Car>> SearchCarsAsync(string keyword)
        {
            return await _dbContext.Cars.Where(c => c.Make.Contains(keyword) || c.Color.Contains(keyword)).ToListAsync();
        }

        public async Task<IEnumerable<Car>> GetAllCarsEqualDbAsync()
        {
            // Get all cars from Redis
            var redisData = _db.HashGetAll("carall");

            if (redisData.Length == 0)
            {
                // No data in Redis, fetch from database
                return await LoadDataFromDatabaseAsync();
            }

            // Deserialize Redis data
            var redisCars = redisData
                .Select(entry => JsonSerializer.Deserialize<Car>(entry.Value))
                .ToList();

            // Get data from the database
            var databaseCars = await _dbContext.Cars.ToListAsync();

            // Compare Redis data and database data
            var redisSet = new HashSet<int>(redisCars.Select(car => car.Id));
            var databaseSet = new HashSet<int>(databaseCars.Select(car => car.Id));

            if (!redisSet.SetEquals(databaseSet))
            {
                // Data mismatch, reload from database
                return await ReloadDataFromDatabaseAsync(databaseCars);
            }

            // If data matches, return data from Redis
            return redisCars;
        }

        private async Task<IEnumerable<Car>> LoadDataFromDatabaseAsync()
        {
            var data = await _dbContext.Cars.ToListAsync();

            // Store each car in Redis hash
            var hashEntries = data.Select(car =>
                new HashEntry(car.Id, JsonSerializer.Serialize(car))
            ).ToArray();

            // Set the hash entries in Redis
            _db.HashSet("carall", hashEntries);

            // Set expiration for the hash key
            _db.KeyExpire("carall", TimeSpan.FromMinutes(1));

            return data;
        }

        private async Task<IEnumerable<Car>> ReloadDataFromDatabaseAsync(IEnumerable<Car> databaseCars)
        {
            // Remove the existing Redis data
            _db.KeyDelete("carall");

            // Reload data into Redis
            var hashEntries = databaseCars.Select(car =>
                new HashEntry(car.Id, JsonSerializer.Serialize(car))
            ).ToArray();

            // Set the new hash entries in Redis
            _db.HashSet("carall", hashEntries);

            // Set expiration for the hash key
            _db.KeyExpire("carall", TimeSpan.FromMinutes(1));

            return databaseCars;
        }
    }
}