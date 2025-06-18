using Microsoft.Data.Sqlite;
using NorthwindBlazor.Models;

namespace NorthwindBlazor.Data;

public class DataManager
{
    private readonly string _connectionString;

    public DataManager(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("NorthwindConnection") ?? throw new ArgumentNullException("Connection string not found");
    }

    private SqliteConnection GetConnection()
    {
        return new SqliteConnection(_connectionString);
    }

    #region Category Methods

    public async Task<ReturnListType<Category>> GetAllCategoriesAsync()
    {
        var result = new ReturnListType<Category>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                SELECT c.CategoryID, c.CategoryName, c.Description, c.Picture
                FROM Categories c
                ORDER BY c.CategoryName";
            
            using var command = new SqliteCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                var category = new Category
                {
                    CategoryId = reader.GetInt32(0), // CategoryID
                    CategoryName = reader.IsDBNull(1) ? null : reader.GetString(1), // CategoryName
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2), // Description
                    Picture = reader.IsDBNull(3) ? null : (byte[])reader[3] // Picture
                };
                
                result.Data.Add(category);
            }
            
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add(ex.Message);
        }
        
        return result;
    }

    public async Task<ReturnType<Category>> GetCategoryByIdAsync(int categoryId)
    {
        var result = new ReturnType<Category>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                SELECT c.CategoryID, c.CategoryName, c.Description, c.Picture
                FROM Categories c
                WHERE c.CategoryID = @CategoryID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@CategoryID", categoryId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                result.Data = new Category
                {
                    CategoryId = reader.GetInt32(0), // CategoryID
                    CategoryName = reader.IsDBNull(1) ? null : reader.GetString(1), // CategoryName
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2), // Description
                    Picture = reader.IsDBNull(3) ? null : (byte[])reader[3] // Picture
                };
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"Category with ID {categoryId} not found");
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add(ex.Message);
        }
        
        return result;
    }

    public async Task<ReturnType<Category>> AddCategoryAsync(Category category)
    {
        var result = new ReturnType<Category>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                INSERT INTO Categories (CategoryName, Description, Picture)
                VALUES (@CategoryName, @Description, @Picture);
                SELECT last_insert_rowid();";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@CategoryName", category.CategoryName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Description", category.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Picture", category.Picture ?? (object)DBNull.Value);
            
            var newId = Convert.ToInt32(await command.ExecuteScalarAsync());
            category.CategoryId = newId;
            
            result.Data = category;
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add(ex.Message);
        }
        
        return result;
    }

    public async Task<ReturnType<Category>> UpdateCategoryAsync(Category category)
    {
        var result = new ReturnType<Category>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                UPDATE Categories 
                SET CategoryName = @CategoryName, Description = @Description, Picture = @Picture
                WHERE CategoryID = @CategoryID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@CategoryID", category.CategoryId);
            command.Parameters.AddWithValue("@CategoryName", category.CategoryName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Description", category.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Picture", category.Picture ?? (object)DBNull.Value);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            
            if (rowsAffected > 0)
            {
                result.Data = category;
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"Category with ID {category.CategoryId} not found");
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add(ex.Message);
        }
        
        return result;
    }

    public async Task<ReturnType<bool>> DeleteCategoryAsync(Category category)
    {
        return await DeleteCategoryByIdAsync(category.CategoryId);
    }

    public async Task<ReturnType<bool>> DeleteCategoryByIdAsync(int categoryId)
    {
        var result = new ReturnType<bool>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "DELETE FROM Categories WHERE CategoryID = @CategoryID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@CategoryID", categoryId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            
            if (rowsAffected > 0)
            {
                result.Data = true;
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"Category with ID {categoryId} not found");
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add(ex.Message);
        }
        
        return result;
    }

    #endregion

    #region Customer Methods

    public async Task<ReturnListType<Customer>> GetAllCustomersAsync()
    {
        var result = new ReturnListType<Customer>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                SELECT c.CustomerID, c.CompanyName, c.ContactName, c.ContactTitle, 
                       c.Address, c.City, c.Region, c.PostalCode, c.Country, c.Phone, c.Fax
                FROM Customers c
                ORDER BY c.CompanyName";
            
            using var command = new SqliteCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                var customer = new Customer
                {
                    CustomerId = reader.GetString(0), // CustomerID
                    CompanyName = reader.IsDBNull(1) ? null : reader.GetString(1), // CompanyName
                    ContactName = reader.IsDBNull(2) ? null : reader.GetString(2), // ContactName
                    ContactTitle = reader.IsDBNull(3) ? null : reader.GetString(3), // ContactTitle
                    Address = reader.IsDBNull(4) ? null : reader.GetString(4), // Address
                    City = reader.IsDBNull(5) ? null : reader.GetString(5), // City
                    Region = reader.IsDBNull(6) ? null : reader.GetString(6), // Region
                    PostalCode = reader.IsDBNull(7) ? null : reader.GetString(7), // PostalCode
                    Country = reader.IsDBNull(8) ? null : reader.GetString(8), // Country
                    Phone = reader.IsDBNull(9) ? null : reader.GetString(9), // Phone
                    Fax = reader.IsDBNull(10) ? null : reader.GetString(10) // Fax
                };
                
                result.Data.Add(customer);
            }
            
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add(ex.Message);
        }
        
        return result;
    }

    public async Task<ReturnType<Customer>> GetCustomerByIdAsync(string customerId)
    {
        var result = new ReturnType<Customer>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            // Get customer with orders
            const string sql = @"
                SELECT c.CustomerID, c.CompanyName, c.ContactName, c.ContactTitle, 
                       c.Address, c.City, c.Region, c.PostalCode, c.Country, c.Phone, c.Fax,
                       o.OrderID, o.EmployeeID, o.OrderDate, o.RequiredDate, o.ShippedDate,
                       o.ShipVia, o.Freight, o.ShipName, o.ShipAddress, o.ShipCity,
                       o.ShipRegion, o.ShipPostalCode, o.ShipCountry
                FROM Customers c
                LEFT JOIN Orders o ON c.CustomerID = o.CustomerID
                WHERE c.CustomerID = @CustomerID
                ORDER BY o.OrderDate DESC";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@CustomerID", customerId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            Customer? customer = null;
            var orders = new List<Order>();
            
            while (await reader.ReadAsync())
            {
                if (customer == null)
                {
                    customer = new Customer
                    {
                        CustomerId = reader.GetString(0), // CustomerID
                        CompanyName = reader.IsDBNull(1) ? null : reader.GetString(1), // CompanyName
                        ContactName = reader.IsDBNull(2) ? null : reader.GetString(2), // ContactName
                        ContactTitle = reader.IsDBNull(3) ? null : reader.GetString(3), // ContactTitle
                        Address = reader.IsDBNull(4) ? null : reader.GetString(4), // Address
                        City = reader.IsDBNull(5) ? null : reader.GetString(5), // City
                        Region = reader.IsDBNull(6) ? null : reader.GetString(6), // Region
                        PostalCode = reader.IsDBNull(7) ? null : reader.GetString(7), // PostalCode
                        Country = reader.IsDBNull(8) ? null : reader.GetString(8), // Country
                        Phone = reader.IsDBNull(9) ? null : reader.GetString(9), // Phone
                        Fax = reader.IsDBNull(10) ? null : reader.GetString(10) // Fax
                    };
                }
                
                // Add order if exists
                if (!reader.IsDBNull(11)) // OrderID
                {
                    var order = new Order
                    {
                        OrderId = reader.GetInt32(11), // OrderID
                        CustomerId = customerId,
                        Customer = customer,
                        EmployeeId = reader.IsDBNull(12) ? null : reader.GetInt32(12), // EmployeeID
                        OrderDate = reader.IsDBNull(13) ? null : reader.GetDateTime(13), // OrderDate
                        RequiredDate = reader.IsDBNull(14) ? null : reader.GetDateTime(14), // RequiredDate
                        ShippedDate = reader.IsDBNull(15) ? null : reader.GetDateTime(15), // ShippedDate
                        ShipVia = reader.IsDBNull(16) ? null : reader.GetInt32(16), // ShipVia
                        Freight = reader.IsDBNull(17) ? null : reader.GetInt32(17), // Freight
                        ShipName = reader.IsDBNull(18) ? null : reader.GetString(18), // ShipName
                        ShipAddress = reader.IsDBNull(19) ? null : reader.GetString(19), // ShipAddress
                        ShipCity = reader.IsDBNull(20) ? null : reader.GetString(20), // ShipCity
                        ShipRegion = reader.IsDBNull(21) ? null : reader.GetString(21), // ShipRegion
                        ShipPostalCode = reader.IsDBNull(22) ? null : reader.GetString(22), // ShipPostalCode
                        ShipCountry = reader.IsDBNull(23) ? null : reader.GetString(23) // ShipCountry
                    };
                    orders.Add(order);
                }
            }
            
            if (customer != null)
            {
                customer.Orders = orders;
                result.Data = customer;
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"Customer with ID {customerId} not found");
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add(ex.Message);
        }
        
        return result;
    }

    public async Task<ReturnType<Customer>> AddCustomerAsync(Customer customer)
    {
        var result = new ReturnType<Customer>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                INSERT INTO Customers (CustomerID, CompanyName, ContactName, ContactTitle, Address, 
                                     City, Region, PostalCode, Country, Phone, Fax)
                VALUES (@CustomerID, @CompanyName, @ContactName, @ContactTitle, @Address,
                        @City, @Region, @PostalCode, @Country, @Phone, @Fax)";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@CustomerID", customer.CustomerId);
            command.Parameters.AddWithValue("@CompanyName", customer.CompanyName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ContactName", customer.ContactName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ContactTitle", customer.ContactTitle ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Address", customer.Address ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@City", customer.City ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Region", customer.Region ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PostalCode", customer.PostalCode ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Country", customer.Country ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Phone", customer.Phone ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Fax", customer.Fax ?? (object)DBNull.Value);
            
            await command.ExecuteNonQueryAsync();
            
            result.Data = customer;
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add(ex.Message);
        }
        
        return result;
    }

    public async Task<ReturnType<Customer>> UpdateCustomerAsync(Customer customer)
    {
        var result = new ReturnType<Customer>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                UPDATE Customers 
                SET CompanyName = @CompanyName, ContactName = @ContactName, ContactTitle = @ContactTitle,
                    Address = @Address, City = @City, Region = @Region, PostalCode = @PostalCode,
                    Country = @Country, Phone = @Phone, Fax = @Fax
                WHERE CustomerID = @CustomerID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@CustomerID", customer.CustomerId);
            command.Parameters.AddWithValue("@CompanyName", customer.CompanyName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ContactName", customer.ContactName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ContactTitle", customer.ContactTitle ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Address", customer.Address ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@City", customer.City ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Region", customer.Region ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PostalCode", customer.PostalCode ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Country", customer.Country ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Phone", customer.Phone ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Fax", customer.Fax ?? (object)DBNull.Value);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            
            if (rowsAffected > 0)
            {
                result.Data = customer;
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"Customer with ID {customer.CustomerId} not found");
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add(ex.Message);
        }
        
        return result;
    }

    public async Task<ReturnType<bool>> DeleteCustomerAsync(Customer customer)
    {
        return await DeleteCustomerByIdAsync(customer.CustomerId);
    }

    public async Task<ReturnType<bool>> DeleteCustomerByIdAsync(string customerId)
    {
        var result = new ReturnType<bool>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "DELETE FROM Customers WHERE CustomerID = @CustomerID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@CustomerID", customerId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            
            if (rowsAffected > 0)
            {
                result.Data = true;
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"Customer with ID {customerId} not found");
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add(ex.Message);
        }
        
        return result;
    }

    #endregion

    #region Product Methods

    public async Task<ReturnListType<Product>> GetAllProductsAsync()
    {
        var result = new ReturnListType<Product>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                SELECT p.ProductID, p.ProductName, p.SupplierID, p.CategoryID, p.QuantityPerUnit,
                       p.UnitPrice, p.UnitsInStock, p.UnitsOnOrder, p.ReorderLevel, p.Discontinued,
                       c.CategoryName, c.Description as CategoryDescription, c.Picture,
                       s.CompanyName as SupplierName, s.ContactName as SupplierContact
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID
                ORDER BY p.ProductName";
            
            using var command = new SqliteCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                var product = new Product
                {
                    ProductId = reader.GetInt32(0), // ProductID
                    ProductName = reader.GetString(1), // ProductName
                    SupplierId = reader.IsDBNull(2) ? null : reader.GetInt32(2), // SupplierID
                    CategoryId = reader.IsDBNull(3) ? null : reader.GetInt32(3), // CategoryID
                    QuantityPerUnit = reader.IsDBNull(4) ? null : reader.GetString(4), // QuantityPerUnit
                    UnitPrice = reader.IsDBNull(5) ? null : reader.GetDouble(5), // UnitPrice
                    UnitsInStock = reader.IsDBNull(6) ? null : reader.GetInt32(6), // UnitsInStock
                    UnitsOnOrder = reader.IsDBNull(7) ? null : reader.GetInt32(7), // UnitsOnOrder
                    ReorderLevel = reader.IsDBNull(8) ? null : reader.GetInt32(8), // ReorderLevel
                    Discontinued = reader.GetString(9) // Discontinued
                };

                // Populate Category if exists
                if (!reader.IsDBNull(3)) // CategoryID exists
                {
                    product.Category = new Category
                    {
                        CategoryId = reader.GetInt32(3), // CategoryID
                        CategoryName = reader.IsDBNull(10) ? null : reader.GetString(10), // CategoryName
                        Description = reader.IsDBNull(11) ? null : reader.GetString(11), // CategoryDescription
                        Picture = reader.IsDBNull(12) ? null : (byte[])reader[12] // Picture
                    };
                }

                // Populate Supplier if exists
                if (!reader.IsDBNull(2)) // SupplierID exists
                {
                    product.Supplier = new Supplier
                    {
                        SupplierId = reader.GetInt32(2), // SupplierID
                        CompanyName = reader.IsDBNull(13) ? null : reader.GetString(13), // SupplierName
                        ContactName = reader.IsDBNull(14) ? null : reader.GetString(14) // SupplierContact
                    };
                }
                
                result.Data.Add(product);
            }
            
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add(ex.Message);
        }
        
        return result;
    }

    public async Task<ReturnType<Product>> GetProductByIdAsync(int productId)
    {
        var result = new ReturnType<Product>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                SELECT p.ProductID, p.ProductName, p.SupplierID, p.CategoryID, p.QuantityPerUnit,
                       p.UnitPrice, p.UnitsInStock, p.UnitsOnOrder, p.ReorderLevel, p.Discontinued,
                       c.CategoryName, c.Description as CategoryDescription, c.Picture,
                       s.CompanyName as SupplierName, s.ContactName as SupplierContact, s.ContactTitle, 
                       s.Address, s.City, s.Region, s.PostalCode, s.Country, s.Phone, s.Fax, s.HomePage
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID
                WHERE p.ProductID = @ProductID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@ProductID", productId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                result.Data = new Product
                {
                    ProductId = reader.GetInt32(0), // ProductID
                    ProductName = reader.GetString(1), // ProductName
                    SupplierId = reader.IsDBNull(2) ? null : reader.GetInt32(2), // SupplierID
                    CategoryId = reader.IsDBNull(3) ? null : reader.GetInt32(3), // CategoryID
                    QuantityPerUnit = reader.IsDBNull(4) ? null : reader.GetString(4), // QuantityPerUnit
                    UnitPrice = reader.IsDBNull(5) ? null : reader.GetDouble(5), // UnitPrice
                    UnitsInStock = reader.IsDBNull(6) ? null : reader.GetInt32(6), // UnitsInStock
                    UnitsOnOrder = reader.IsDBNull(7) ? null : reader.GetInt32(7), // UnitsOnOrder
                    ReorderLevel = reader.IsDBNull(8) ? null : reader.GetInt32(8), // ReorderLevel
                    Discontinued = reader.GetString(9) // Discontinued
                };

                // Populate Category if exists
                if (!reader.IsDBNull(3)) // CategoryID exists
                {
                    result.Data.Category = new Category
                    {
                        CategoryId = reader.GetInt32(3), // CategoryID
                        CategoryName = reader.IsDBNull(10) ? null : reader.GetString(10), // CategoryName
                        Description = reader.IsDBNull(11) ? null : reader.GetString(11), // CategoryDescription
                        Picture = reader.IsDBNull(12) ? null : (byte[])reader[12] // Picture
                    };
                }

                // Populate Supplier if exists
                if (!reader.IsDBNull(2)) // SupplierID exists
                {
                    result.Data.Supplier = new Supplier
                    {
                        SupplierId = reader.GetInt32(2), // SupplierID
                        CompanyName = reader.IsDBNull(13) ? null : reader.GetString(13), // SupplierName
                        ContactName = reader.IsDBNull(14) ? null : reader.GetString(14), // SupplierContact
                        ContactTitle = reader.IsDBNull(15) ? null : reader.GetString(15), // ContactTitle
                        Address = reader.IsDBNull(16) ? null : reader.GetString(16), // Address
                        City = reader.IsDBNull(17) ? null : reader.GetString(17), // City
                        Region = reader.IsDBNull(18) ? null : reader.GetString(18), // Region
                        PostalCode = reader.IsDBNull(19) ? null : reader.GetString(19), // PostalCode
                        Country = reader.IsDBNull(20) ? null : reader.GetString(20), // Country
                        Phone = reader.IsDBNull(21) ? null : reader.GetString(21), // Phone
                        Fax = reader.IsDBNull(22) ? null : reader.GetString(22), // Fax
                        HomePage = reader.IsDBNull(23) ? null : reader.GetString(23) // HomePage
                    };
                }

                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"Product with ID {productId} not found");
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add(ex.Message);
        }
        
        return result;
    }

    public async Task<ReturnType<Product>> AddProductAsync(Product product)
    {
        var result = new ReturnType<Product>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                INSERT INTO Products (ProductName, SupplierID, CategoryID, QuantityPerUnit, UnitPrice,
                                    UnitsInStock, UnitsOnOrder, ReorderLevel, Discontinued)
                VALUES (@ProductName, @SupplierID, @CategoryID, @QuantityPerUnit, @UnitPrice,
                        @UnitsInStock, @UnitsOnOrder, @ReorderLevel, @Discontinued);
                SELECT last_insert_rowid();";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@ProductName", product.ProductName);
            command.Parameters.AddWithValue("@SupplierID", product.SupplierId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CategoryID", product.CategoryId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@QuantityPerUnit", product.QuantityPerUnit ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@UnitPrice", product.UnitPrice ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@UnitsInStock", product.UnitsInStock ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@UnitsOnOrder", product.UnitsOnOrder ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ReorderLevel", product.ReorderLevel ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Discontinued", product.Discontinued);
            
            var newId = Convert.ToInt32(await command.ExecuteScalarAsync());
            product.ProductId = newId;
            
            result.Data = product;
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add(ex.Message);
        }
        
        return result;
    }

    public async Task<ReturnType<Product>> UpdateProductAsync(Product product)
    {
        var result = new ReturnType<Product>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                UPDATE Products 
                SET ProductName = @ProductName, SupplierID = @SupplierID, CategoryID = @CategoryID,
                    QuantityPerUnit = @QuantityPerUnit, UnitPrice = @UnitPrice, UnitsInStock = @UnitsInStock,
                    UnitsOnOrder = @UnitsOnOrder, ReorderLevel = @ReorderLevel, Discontinued = @Discontinued
                WHERE ProductID = @ProductID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@ProductID", product.ProductId);
            command.Parameters.AddWithValue("@ProductName", product.ProductName);
            command.Parameters.AddWithValue("@SupplierID", product.SupplierId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CategoryID", product.CategoryId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@QuantityPerUnit", product.QuantityPerUnit ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@UnitPrice", product.UnitPrice ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@UnitsInStock", product.UnitsInStock ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@UnitsOnOrder", product.UnitsOnOrder ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ReorderLevel", product.ReorderLevel ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Discontinued", product.Discontinued);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            
            if (rowsAffected > 0)
            {
                result.Data = product;
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"Product with ID {product.ProductId} not found");
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add(ex.Message);
        }
        
        return result;
    }

    public async Task<ReturnType<bool>> DeleteProductAsync(Product product)
    {
        return await DeleteProductByIdAsync(product.ProductId);
    }

    public async Task<ReturnType<bool>> DeleteProductByIdAsync(int productId)
    {
        var result = new ReturnType<bool>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "DELETE FROM Products WHERE ProductID = @ProductID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@ProductID", productId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            
            if (rowsAffected > 0)
            {
                result.Data = true;
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"Product with ID {productId} not found");
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add(ex.Message);
        }
        
        return result;
    }

    #endregion
}