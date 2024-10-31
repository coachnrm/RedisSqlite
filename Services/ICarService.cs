using RedisSqlite.Models;

namespace RedisSqlite.Services
{
    public interface ICarService
    {
        Task<IEnumerable<Car>> GetAllCarsAsync();
        Task<Car?> GetCarByIdAsync(int id);
        Task<Car> AddCarAsync(Car car);
        Task<Car?> UpdateCarAsync(int id, Car car);
        Task<Car> DeleteCarAsync(int id);
        Task<List<Car>> SearchCarsAsync(string keyword);
    }
}