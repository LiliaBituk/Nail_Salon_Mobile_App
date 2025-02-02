//using Business_Logic;
//using SQLite;

//public class Database
//{
//    private readonly SQLiteAsyncConnection _database;

//    public Database(string dbPath)
//    {
//        _database = new SQLiteAsyncConnection(dbPath);
//        _database.CreateTableAsync<Customer>().Wait();
//        _database.CreateTableAsync<Service>().Wait();
//        _database.CreateTableAsync<VisitLogs>().Wait(); 
//    }

//    public Task<int> SaveCustomerAsync(Customer customer)
//    {
//        return _database.InsertAsync(customer);
//    }

//    public Task<int> SaveVisitLogAsync(VisitLogs visitLog)
//    {
//        return _database.InsertAsync(visitLog);
//    }

//    public Task<int> SaveServicesAsync(Service service)
//    {
//        return _database.InsertAsync(service);
//    }

//    public Task<List<Customer>> GetCustomersAsync()
//    {
//        return _database.Table<Customer>().ToListAsync();
//    }

//    public Task<List<VisitLogs>> GetVisitLogsAsync()
//    {
//        return _database.Table<VisitLogs>().ToListAsync();
//    }

//    public Task<List<Service>> GetServicesAsync()
//    {
//        return _database.Table<Service>().ToListAsync();
//    }

//    public async Task<Customer> GetCustomerByDetailsAsync(string fullName, decimal phoneNumber)
//    {
//        return await _database.Table<Customer>()
//            .FirstOrDefaultAsync(c => c.CustomerFullName == fullName && c.CustomerPhoneNumber == phoneNumber);
//    }

//    public Task<List<T>> GetAllAsync<T>() where T : new()
//    {
//        return _database.Table<T>().ToListAsync();
//    }
//}

using Business_Logic;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Database
{
    private readonly SQLiteAsyncConnection _database;

    public Database(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
        _database.CreateTableAsync<Customer>().Wait();
        _database.CreateTableAsync<Service>().Wait();
        _database.CreateTableAsync<VisitLogs>().Wait();
    }

    public Task<int> SaveCustomerAsync(Customer customer)
    {
        return _database.InsertAsync(customer);
    }

    public Task<int> SaveServiceAsync(Service service)
    {
        return _database.InsertAsync(service);
    }

    public Task<int> SaveVisitLogAsync(VisitLogs visitLog)
    {
        return _database.InsertAsync(visitLog);
    }

    public Task<List<Service>> GetServicesAsync()
    {
        return _database.Table<Service>().ToListAsync();
    }

    public Task<Customer> GetCustomerByIdAsync(int id)
    {
        return _database.Table<Customer>().FirstOrDefaultAsync(c => c.Id == id);
    }

    public Task<Service> GetServiceByIdAsync(int id)
    {
        return _database.Table<Service>().FirstOrDefaultAsync(s => s.Id == id);
    }

    public Task<Customer> GetCustomerByDetailsAsync(string fullName, decimal phoneNumber)
    {
        return _database.Table<Customer>()
            .FirstOrDefaultAsync(c => c.CustomerFullName == fullName && c.CustomerPhoneNumber == phoneNumber);
    }
}