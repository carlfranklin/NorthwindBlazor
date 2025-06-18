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

    #region Supplier Methods

    public async Task<ReturnListType<Supplier>> GetAllSuppliersAsync()
    {
        var result = new ReturnListType<Supplier>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                SELECT SupplierID, CompanyName, ContactName, ContactTitle, Address, City, Region, 
                       PostalCode, Country, Phone, Fax, HomePage
                FROM Suppliers ORDER BY CompanyName";
            
            using var command = new SqliteCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                result.Data.Add(new Supplier
                {
                    SupplierId = reader.GetInt32(0),
                    CompanyName = reader.GetString(1),
                    ContactName = reader.IsDBNull(2) ? null : reader.GetString(2),
                    ContactTitle = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Address = reader.IsDBNull(4) ? null : reader.GetString(4),
                    City = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Region = reader.IsDBNull(6) ? null : reader.GetString(6),
                    PostalCode = reader.IsDBNull(7) ? null : reader.GetString(7),
                    Country = reader.IsDBNull(8) ? null : reader.GetString(8),
                    Phone = reader.IsDBNull(9) ? null : reader.GetString(9),
                    Fax = reader.IsDBNull(10) ? null : reader.GetString(10),
                    HomePage = reader.IsDBNull(11) ? null : reader.GetString(11)
                });
            }
            
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add(ex.Message);
        }
        
        return result;
    }

    public async Task<ReturnType<Supplier>> GetSupplierByIdAsync(int supplierId)
    {
        var result = new ReturnType<Supplier>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                SELECT s.SupplierID, s.CompanyName, s.ContactName, s.ContactTitle, s.Address, s.City, s.Region, 
                       s.PostalCode, s.Country, s.Phone, s.Fax, s.HomePage,
                       p.ProductID, p.ProductName
                FROM Suppliers s
                LEFT JOIN Products p ON s.SupplierID = p.SupplierID
                WHERE s.SupplierID = @SupplierID
                ORDER BY p.ProductName";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@SupplierID", supplierId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            Supplier? supplier = null;
            var products = new List<Product>();
            
            while (await reader.ReadAsync())
            {
                if (supplier == null)
                {
                    supplier = new Supplier
                    {
                        SupplierId = reader.GetInt32(0),
                        CompanyName = reader.GetString(1),
                        ContactName = reader.IsDBNull(2) ? null : reader.GetString(2),
                        ContactTitle = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Address = reader.IsDBNull(4) ? null : reader.GetString(4),
                        City = reader.IsDBNull(5) ? null : reader.GetString(5),
                        Region = reader.IsDBNull(6) ? null : reader.GetString(6),
                        PostalCode = reader.IsDBNull(7) ? null : reader.GetString(7),
                        Country = reader.IsDBNull(8) ? null : reader.GetString(8),
                        Phone = reader.IsDBNull(9) ? null : reader.GetString(9),
                        Fax = reader.IsDBNull(10) ? null : reader.GetString(10),
                        HomePage = reader.IsDBNull(11) ? null : reader.GetString(11)
                    };
                }
                
                if (!reader.IsDBNull(12)) // ProductID
                {
                    products.Add(new Product
                    {
                        ProductId = reader.GetInt32(12),
                        ProductName = reader.GetString(13),
                        SupplierId = supplierId,
                        Supplier = supplier
                    });
                }
            }
            
            if (supplier != null)
            {
                supplier.Products = products;
                result.Data = supplier;
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"Supplier with ID {supplierId} not found");
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add(ex.Message);
        }
        
        return result;
    }

    public async Task<ReturnType<Supplier>> AddSupplierAsync(Supplier supplier)
    {
        var result = new ReturnType<Supplier>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                INSERT INTO Suppliers (CompanyName, ContactName, ContactTitle, Address, City, Region,
                                     PostalCode, Country, Phone, Fax, HomePage)
                VALUES (@CompanyName, @ContactName, @ContactTitle, @Address, @City, @Region,
                        @PostalCode, @Country, @Phone, @Fax, @HomePage);
                SELECT last_insert_rowid();";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@CompanyName", supplier.CompanyName);
            command.Parameters.AddWithValue("@ContactName", supplier.ContactName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ContactTitle", supplier.ContactTitle ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Address", supplier.Address ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@City", supplier.City ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Region", supplier.Region ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PostalCode", supplier.PostalCode ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Country", supplier.Country ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Phone", supplier.Phone ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Fax", supplier.Fax ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@HomePage", supplier.HomePage ?? (object)DBNull.Value);
            
            var newId = Convert.ToInt32(await command.ExecuteScalarAsync());
            supplier.SupplierId = newId;
            
            result.Data = supplier;
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add(ex.Message);
        }
        
        return result;
    }

    public async Task<ReturnType<Supplier>> UpdateSupplierAsync(Supplier supplier)
    {
        var result = new ReturnType<Supplier>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                UPDATE Suppliers 
                SET CompanyName = @CompanyName, ContactName = @ContactName, ContactTitle = @ContactTitle,
                    Address = @Address, City = @City, Region = @Region, PostalCode = @PostalCode,
                    Country = @Country, Phone = @Phone, Fax = @Fax, HomePage = @HomePage
                WHERE SupplierID = @SupplierID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@SupplierID", supplier.SupplierId);
            command.Parameters.AddWithValue("@CompanyName", supplier.CompanyName);
            command.Parameters.AddWithValue("@ContactName", supplier.ContactName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ContactTitle", supplier.ContactTitle ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Address", supplier.Address ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@City", supplier.City ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Region", supplier.Region ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PostalCode", supplier.PostalCode ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Country", supplier.Country ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Phone", supplier.Phone ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Fax", supplier.Fax ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@HomePage", supplier.HomePage ?? (object)DBNull.Value);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            
            if (rowsAffected > 0)
            {
                result.Data = supplier;
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"Supplier with ID {supplier.SupplierId} not found");
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add(ex.Message);
        }
        
        return result;
    }

    public async Task<ReturnType<bool>> DeleteSupplierAsync(Supplier supplier)
    {
        return await DeleteSupplierByIdAsync(supplier.SupplierId);
    }

    public async Task<ReturnType<bool>> DeleteSupplierByIdAsync(int supplierId)
    {
        var result = new ReturnType<bool>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "DELETE FROM Suppliers WHERE SupplierID = @SupplierID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@SupplierID", supplierId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            
            if (rowsAffected > 0)
            {
                result.Data = true;
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"Supplier with ID {supplierId} not found");
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add(ex.Message);
        }
        
        return result;
    }

    #endregion

    #region OrderDetail Methods

    public async Task<ReturnListType<OrderDetail>> GetAllOrderDetailsAsync()
    {
        var result = new ReturnListType<OrderDetail>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                SELECT od.OrderID, od.ProductID, od.UnitPrice, od.Quantity, od.Discount,
                       p.ProductName, o.OrderDate
                FROM [Order Details] od
                LEFT JOIN Products p ON od.ProductID = p.ProductID
                LEFT JOIN Orders o ON od.OrderID = o.OrderID
                ORDER BY o.OrderDate DESC, p.ProductName";
            
            using var command = new SqliteCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = reader.GetInt32(0),
                    ProductId = reader.GetInt32(1),
                    UnitPrice = reader.GetDouble(2),
                    Quantity = reader.GetInt32(3),
                    Discount = reader.GetDouble(4)
                };

                if (!reader.IsDBNull(5))
                {
                    orderDetail.Product = new Product
                    {
                        ProductId = reader.GetInt32(1),
                        ProductName = reader.GetString(5)
                    };
                }

                if (!reader.IsDBNull(6))
                {
                    orderDetail.Order = new Order
                    {
                        OrderId = reader.GetInt32(0),
                        OrderDate = reader.GetDateTime(6)
                    };
                }

                result.Data.Add(orderDetail);
            }
            
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add(ex.Message);
        }
        
        return result;
    }

    public async Task<ReturnType<OrderDetail>> GetOrderDetailByIdAsync(int orderId, int productId)
    {
        var result = new ReturnType<OrderDetail>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                SELECT od.OrderID, od.ProductID, od.UnitPrice, od.Quantity, od.Discount,
                       p.ProductName, p.CategoryID, p.SupplierID,
                       o.OrderDate, o.CustomerID
                FROM [Order Details] od
                LEFT JOIN Products p ON od.ProductID = p.ProductID
                LEFT JOIN Orders o ON od.OrderID = o.OrderID
                WHERE od.OrderID = @OrderID AND od.ProductID = @ProductID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@OrderID", orderId);
            command.Parameters.AddWithValue("@ProductID", productId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                result.Data = new OrderDetail
                {
                    OrderId = reader.GetInt32(0),
                    ProductId = reader.GetInt32(1),
                    UnitPrice = reader.GetDouble(2),
                    Quantity = reader.GetInt32(3),
                    Discount = reader.GetDouble(4)
                };

                if (!reader.IsDBNull(5))
                {
                    result.Data.Product = new Product
                    {
                        ProductId = reader.GetInt32(1),
                        ProductName = reader.GetString(5),
                        CategoryId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                        SupplierId = reader.IsDBNull(7) ? null : reader.GetInt32(7)
                    };
                }

                if (!reader.IsDBNull(8))
                {
                    result.Data.Order = new Order
                    {
                        OrderId = reader.GetInt32(0),
                        OrderDate = reader.GetDateTime(8),
                        CustomerId = reader.IsDBNull(9) ? null : reader.GetString(9)
                    };
                }

                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"OrderDetail with OrderID {orderId} and ProductID {productId} not found");
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add(ex.Message);
        }
        
        return result;
    }

    public async Task<ReturnType<OrderDetail>> AddOrderDetailAsync(OrderDetail orderDetail)
    {
        var result = new ReturnType<OrderDetail>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                INSERT INTO [Order Details] (OrderID, ProductID, UnitPrice, Quantity, Discount)
                VALUES (@OrderID, @ProductID, @UnitPrice, @Quantity, @Discount)";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@OrderID", orderDetail.OrderId);
            command.Parameters.AddWithValue("@ProductID", orderDetail.ProductId);
            command.Parameters.AddWithValue("@UnitPrice", orderDetail.UnitPrice);
            command.Parameters.AddWithValue("@Quantity", orderDetail.Quantity);
            command.Parameters.AddWithValue("@Discount", orderDetail.Discount);
            
            await command.ExecuteNonQueryAsync();
            
            result.Data = orderDetail;
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add(ex.Message);
        }
        
        return result;
    }

    public async Task<ReturnType<OrderDetail>> UpdateOrderDetailAsync(OrderDetail orderDetail)
    {
        var result = new ReturnType<OrderDetail>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                UPDATE [Order Details] 
                SET UnitPrice = @UnitPrice, Quantity = @Quantity, Discount = @Discount
                WHERE OrderID = @OrderID AND ProductID = @ProductID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@OrderID", orderDetail.OrderId);
            command.Parameters.AddWithValue("@ProductID", orderDetail.ProductId);
            command.Parameters.AddWithValue("@UnitPrice", orderDetail.UnitPrice);
            command.Parameters.AddWithValue("@Quantity", orderDetail.Quantity);
            command.Parameters.AddWithValue("@Discount", orderDetail.Discount);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            
            if (rowsAffected > 0)
            {
                result.Data = orderDetail;
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"OrderDetail with OrderID {orderDetail.OrderId} and ProductID {orderDetail.ProductId} not found");
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add(ex.Message);
        }
        
        return result;
    }

    public async Task<ReturnType<bool>> DeleteOrderDetailAsync(OrderDetail orderDetail)
    {
        return await DeleteOrderDetailByIdAsync(orderDetail.OrderId, orderDetail.ProductId);
    }

    public async Task<ReturnType<bool>> DeleteOrderDetailByIdAsync(int orderId, int productId)
    {
        var result = new ReturnType<bool>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "DELETE FROM [Order Details] WHERE OrderID = @OrderID AND ProductID = @ProductID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@OrderID", orderId);
            command.Parameters.AddWithValue("@ProductID", productId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            
            if (rowsAffected > 0)
            {
                result.Data = true;
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"OrderDetail with OrderID {orderId} and ProductID {productId} not found");
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessages.Add(ex.Message);
        }
        
        return result;
    }

    #endregion

    #region Simple CRUD Methods for Remaining Models

    // Employee CRUD
    public async Task<ReturnListType<Employee>> GetAllEmployeesAsync()
    {
        var result = new ReturnListType<Employee>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                SELECT EmployeeID, LastName, FirstName, Title, TitleOfCourtesy, BirthDate, HireDate,
                       Address, City, Region, PostalCode, Country, HomePhone, Extension, Photo, Notes, ReportsTo, PhotoPath
                FROM Employees ORDER BY LastName, FirstName";
            
            using var command = new SqliteCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                result.Data.Add(new Employee
                {
                    EmployeeId = reader.GetInt32(0),
                    LastName = reader.IsDBNull(1) ? null : reader.GetString(1),
                    FirstName = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Title = reader.IsDBNull(3) ? null : reader.GetString(3),
                    TitleOfCourtesy = reader.IsDBNull(4) ? null : reader.GetString(4),
                    BirthDate = reader.IsDBNull(5) ? null : DateOnly.FromDateTime(reader.GetDateTime(5)),
                    HireDate = reader.IsDBNull(6) ? null : DateOnly.FromDateTime(reader.GetDateTime(6)),
                    Address = reader.IsDBNull(7) ? null : reader.GetString(7),
                    City = reader.IsDBNull(8) ? null : reader.GetString(8),
                    Region = reader.IsDBNull(9) ? null : reader.GetString(9),
                    PostalCode = reader.IsDBNull(10) ? null : reader.GetString(10),
                    Country = reader.IsDBNull(11) ? null : reader.GetString(11),
                    HomePhone = reader.IsDBNull(12) ? null : reader.GetString(12),
                    Extension = reader.IsDBNull(13) ? null : reader.GetString(13),
                    Photo = reader.IsDBNull(14) ? null : (byte[])reader[14],
                    Notes = reader.IsDBNull(15) ? null : reader.GetString(15),
                    ReportsTo = reader.IsDBNull(16) ? null : reader.GetInt32(16),
                    PhotoPath = reader.IsDBNull(17) ? null : reader.GetString(17)
                });
            }
            
            result.Success = true;
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    public async Task<ReturnType<Employee>> GetEmployeeByIdAsync(int employeeId)
    {
        var result = new ReturnType<Employee>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                SELECT EmployeeID, LastName, FirstName, Title, TitleOfCourtesy, BirthDate, HireDate,
                       Address, City, Region, PostalCode, Country, HomePhone, Extension, Photo, Notes, ReportsTo, PhotoPath
                FROM Employees WHERE EmployeeID = @EmployeeID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@EmployeeID", employeeId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                result.Data = new Employee
                {
                    EmployeeId = reader.GetInt32(0),
                    LastName = reader.IsDBNull(1) ? null : reader.GetString(1),
                    FirstName = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Title = reader.IsDBNull(3) ? null : reader.GetString(3),
                    TitleOfCourtesy = reader.IsDBNull(4) ? null : reader.GetString(4),
                    BirthDate = reader.IsDBNull(5) ? null : DateOnly.FromDateTime(reader.GetDateTime(5)),
                    HireDate = reader.IsDBNull(6) ? null : DateOnly.FromDateTime(reader.GetDateTime(6)),
                    Address = reader.IsDBNull(7) ? null : reader.GetString(7),
                    City = reader.IsDBNull(8) ? null : reader.GetString(8),
                    Region = reader.IsDBNull(9) ? null : reader.GetString(9),
                    PostalCode = reader.IsDBNull(10) ? null : reader.GetString(10),
                    Country = reader.IsDBNull(11) ? null : reader.GetString(11),
                    HomePhone = reader.IsDBNull(12) ? null : reader.GetString(12),
                    Extension = reader.IsDBNull(13) ? null : reader.GetString(13),
                    Photo = reader.IsDBNull(14) ? null : (byte[])reader[14],
                    Notes = reader.IsDBNull(15) ? null : reader.GetString(15),
                    ReportsTo = reader.IsDBNull(16) ? null : reader.GetInt32(16),
                    PhotoPath = reader.IsDBNull(17) ? null : reader.GetString(17)
                };
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"Employee with ID {employeeId} not found");
            }
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    public async Task<ReturnType<Employee>> AddEmployeeAsync(Employee employee)
    {
        var result = new ReturnType<Employee>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                INSERT INTO Employees (LastName, FirstName, Title, TitleOfCourtesy, BirthDate, HireDate,
                                     Address, City, Region, PostalCode, Country, HomePhone, Extension,
                                     Photo, Notes, ReportsTo, PhotoPath)
                VALUES (@LastName, @FirstName, @Title, @TitleOfCourtesy, @BirthDate, @HireDate,
                        @Address, @City, @Region, @PostalCode, @Country, @HomePhone, @Extension,
                        @Photo, @Notes, @ReportsTo, @PhotoPath);
                SELECT last_insert_rowid();";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@LastName", employee.LastName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@FirstName", employee.FirstName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Title", employee.Title ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@TitleOfCourtesy", employee.TitleOfCourtesy ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@BirthDate", employee.BirthDate?.ToDateTime(TimeOnly.MinValue) ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@HireDate", employee.HireDate?.ToDateTime(TimeOnly.MinValue) ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Address", employee.Address ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@City", employee.City ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Region", employee.Region ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PostalCode", employee.PostalCode ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Country", employee.Country ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@HomePhone", employee.HomePhone ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Extension", employee.Extension ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Photo", employee.Photo ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Notes", employee.Notes ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ReportsTo", employee.ReportsTo ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PhotoPath", employee.PhotoPath ?? (object)DBNull.Value);
            
            var newId = Convert.ToInt32(await command.ExecuteScalarAsync());
            employee.EmployeeId = newId;
            result.Data = employee;
            result.Success = true;
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    public async Task<ReturnType<Employee>> UpdateEmployeeAsync(Employee employee)
    {
        var result = new ReturnType<Employee>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                UPDATE Employees 
                SET LastName = @LastName, FirstName = @FirstName, Title = @Title, TitleOfCourtesy = @TitleOfCourtesy,
                    BirthDate = @BirthDate, HireDate = @HireDate, Address = @Address, City = @City,
                    Region = @Region, PostalCode = @PostalCode, Country = @Country, HomePhone = @HomePhone,
                    Extension = @Extension, Photo = @Photo, Notes = @Notes, ReportsTo = @ReportsTo,
                    PhotoPath = @PhotoPath
                WHERE EmployeeID = @EmployeeID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@EmployeeID", employee.EmployeeId);
            command.Parameters.AddWithValue("@LastName", employee.LastName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@FirstName", employee.FirstName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Title", employee.Title ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@TitleOfCourtesy", employee.TitleOfCourtesy ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@BirthDate", employee.BirthDate?.ToDateTime(TimeOnly.MinValue) ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@HireDate", employee.HireDate?.ToDateTime(TimeOnly.MinValue) ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Address", employee.Address ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@City", employee.City ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Region", employee.Region ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PostalCode", employee.PostalCode ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Country", employee.Country ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@HomePhone", employee.HomePhone ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Extension", employee.Extension ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Photo", employee.Photo ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Notes", employee.Notes ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ReportsTo", employee.ReportsTo ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PhotoPath", employee.PhotoPath ?? (object)DBNull.Value);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            
            if (rowsAffected > 0)
            {
                result.Data = employee;
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"Employee with ID {employee.EmployeeId} not found");
            }
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    public async Task<ReturnType<bool>> DeleteEmployeeAsync(Employee employee) => await DeleteEmployeeByIdAsync(employee.EmployeeId);

    public async Task<ReturnType<bool>> DeleteEmployeeByIdAsync(int employeeId)
    {
        var result = new ReturnType<bool>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "DELETE FROM Employees WHERE EmployeeID = @EmployeeID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@EmployeeID", employeeId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            
            if (rowsAffected > 0)
            {
                result.Data = true;
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"Employee with ID {employeeId} not found");
            }
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    // Order CRUD (simplified)
    public async Task<ReturnListType<Order>> GetAllOrdersAsync()
    {
        var result = new ReturnListType<Order>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                SELECT OrderID, CustomerID, EmployeeID, OrderDate, RequiredDate, ShippedDate,
                       ShipVia, Freight, ShipName, ShipAddress, ShipCity, ShipRegion,
                       ShipPostalCode, ShipCountry
                FROM Orders ORDER BY OrderDate DESC";
            
            using var command = new SqliteCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                result.Data.Add(new Order
                {
                    OrderId = reader.GetInt32(0),
                    CustomerId = reader.IsDBNull(1) ? null : reader.GetString(1),
                    EmployeeId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    OrderDate = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                    RequiredDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                    ShippedDate = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                    ShipVia = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                    Freight = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                    ShipName = reader.IsDBNull(8) ? null : reader.GetString(8),
                    ShipAddress = reader.IsDBNull(9) ? null : reader.GetString(9),
                    ShipCity = reader.IsDBNull(10) ? null : reader.GetString(10),
                    ShipRegion = reader.IsDBNull(11) ? null : reader.GetString(11),
                    ShipPostalCode = reader.IsDBNull(12) ? null : reader.GetString(12),
                    ShipCountry = reader.IsDBNull(13) ? null : reader.GetString(13)
                });
            }
            
            result.Success = true;
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    public async Task<ReturnType<Order>> GetOrderByIdAsync(int orderId)
    {
        var result = new ReturnType<Order>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                SELECT OrderID, CustomerID, EmployeeID, OrderDate, RequiredDate, ShippedDate,
                       ShipVia, Freight, ShipName, ShipAddress, ShipCity, ShipRegion,
                       ShipPostalCode, ShipCountry
                FROM Orders WHERE OrderID = @OrderID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@OrderID", orderId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                result.Data = new Order
                {
                    OrderId = reader.GetInt32(0),
                    CustomerId = reader.IsDBNull(1) ? null : reader.GetString(1),
                    EmployeeId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    OrderDate = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                    RequiredDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                    ShippedDate = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                    ShipVia = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                    Freight = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                    ShipName = reader.IsDBNull(8) ? null : reader.GetString(8),
                    ShipAddress = reader.IsDBNull(9) ? null : reader.GetString(9),
                    ShipCity = reader.IsDBNull(10) ? null : reader.GetString(10),
                    ShipRegion = reader.IsDBNull(11) ? null : reader.GetString(11),
                    ShipPostalCode = reader.IsDBNull(12) ? null : reader.GetString(12),
                    ShipCountry = reader.IsDBNull(13) ? null : reader.GetString(13)
                };
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"Order with ID {orderId} not found");
            }
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    public async Task<ReturnType<Order>> AddOrderAsync(Order order)
    {
        var result = new ReturnType<Order>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                INSERT INTO Orders (CustomerID, EmployeeID, OrderDate, RequiredDate, ShippedDate,
                                  ShipVia, Freight, ShipName, ShipAddress, ShipCity, ShipRegion,
                                  ShipPostalCode, ShipCountry)
                VALUES (@CustomerID, @EmployeeID, @OrderDate, @RequiredDate, @ShippedDate,
                        @ShipVia, @Freight, @ShipName, @ShipAddress, @ShipCity, @ShipRegion,
                        @ShipPostalCode, @ShipCountry);
                SELECT last_insert_rowid();";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@CustomerID", order.CustomerId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@EmployeeID", order.EmployeeId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@OrderDate", order.OrderDate ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@RequiredDate", order.RequiredDate ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ShippedDate", order.ShippedDate ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ShipVia", order.ShipVia ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Freight", order.Freight ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ShipName", order.ShipName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ShipAddress", order.ShipAddress ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ShipCity", order.ShipCity ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ShipRegion", order.ShipRegion ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ShipPostalCode", order.ShipPostalCode ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ShipCountry", order.ShipCountry ?? (object)DBNull.Value);
            
            var newId = Convert.ToInt32(await command.ExecuteScalarAsync());
            order.OrderId = newId;
            result.Data = order;
            result.Success = true;
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    public async Task<ReturnType<Order>> UpdateOrderAsync(Order order)
    {
        var result = new ReturnType<Order>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = @"
                UPDATE Orders 
                SET CustomerID = @CustomerID, EmployeeID = @EmployeeID, OrderDate = @OrderDate,
                    RequiredDate = @RequiredDate, ShippedDate = @ShippedDate, ShipVia = @ShipVia,
                    Freight = @Freight, ShipName = @ShipName, ShipAddress = @ShipAddress,
                    ShipCity = @ShipCity, ShipRegion = @ShipRegion, ShipPostalCode = @ShipPostalCode,
                    ShipCountry = @ShipCountry
                WHERE OrderID = @OrderID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@OrderID", order.OrderId);
            command.Parameters.AddWithValue("@CustomerID", order.CustomerId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@EmployeeID", order.EmployeeId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@OrderDate", order.OrderDate ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@RequiredDate", order.RequiredDate ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ShippedDate", order.ShippedDate ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ShipVia", order.ShipVia ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Freight", order.Freight ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ShipName", order.ShipName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ShipAddress", order.ShipAddress ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ShipCity", order.ShipCity ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ShipRegion", order.ShipRegion ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ShipPostalCode", order.ShipPostalCode ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ShipCountry", order.ShipCountry ?? (object)DBNull.Value);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            
            if (rowsAffected > 0)
            {
                result.Data = order;
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"Order with ID {order.OrderId} not found");
            }
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    public async Task<ReturnType<bool>> DeleteOrderAsync(Order order) => await DeleteOrderByIdAsync(order.OrderId);

    public async Task<ReturnType<bool>> DeleteOrderByIdAsync(int orderId)
    {
        var result = new ReturnType<bool>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "DELETE FROM Orders WHERE OrderID = @OrderID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@OrderID", orderId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            
            if (rowsAffected > 0)
            {
                result.Data = true;
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"Order with ID {orderId} not found");
            }
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    // Remaining simple models - Shipper, Region, Territory, CustomerDemographic
    public async Task<ReturnListType<Shipper>> GetAllShippersAsync()
    {
        var result = new ReturnListType<Shipper>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "SELECT ShipperID, CompanyName, Phone FROM Shippers ORDER BY CompanyName";
            
            using var command = new SqliteCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                result.Data.Add(new Shipper
                {
                    ShipperId = reader.GetInt32(0),
                    CompanyName = reader.GetString(1),
                    Phone = reader.IsDBNull(2) ? null : reader.GetString(2)
                });
            }
            
            result.Success = true;
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    public async Task<ReturnType<Shipper>> GetShipperByIdAsync(int shipperId)
    {
        var result = new ReturnType<Shipper>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "SELECT ShipperID, CompanyName, Phone FROM Shippers WHERE ShipperID = @ShipperID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@ShipperID", shipperId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                result.Data = new Shipper
                {
                    ShipperId = reader.GetInt32(0),
                    CompanyName = reader.GetString(1),
                    Phone = reader.IsDBNull(2) ? null : reader.GetString(2)
                };
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"Shipper with ID {shipperId} not found");
            }
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    public async Task<ReturnType<Shipper>> AddShipperAsync(Shipper shipper)
    {
        var result = new ReturnType<Shipper>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "INSERT INTO Shippers (CompanyName, Phone) VALUES (@CompanyName, @Phone); SELECT last_insert_rowid();";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@CompanyName", shipper.CompanyName);
            command.Parameters.AddWithValue("@Phone", shipper.Phone ?? (object)DBNull.Value);
            
            var newId = Convert.ToInt32(await command.ExecuteScalarAsync());
            shipper.ShipperId = newId;
            result.Data = shipper;
            result.Success = true;
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    public async Task<ReturnType<Shipper>> UpdateShipperAsync(Shipper shipper)
    {
        var result = new ReturnType<Shipper>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "UPDATE Shippers SET CompanyName = @CompanyName, Phone = @Phone WHERE ShipperID = @ShipperID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@ShipperID", shipper.ShipperId);
            command.Parameters.AddWithValue("@CompanyName", shipper.CompanyName);
            command.Parameters.AddWithValue("@Phone", shipper.Phone ?? (object)DBNull.Value);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            
            if (rowsAffected > 0)
            {
                result.Data = shipper;
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"Shipper with ID {shipper.ShipperId} not found");
            }
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    public async Task<ReturnType<bool>> DeleteShipperAsync(Shipper shipper) => await DeleteShipperByIdAsync(shipper.ShipperId);

    public async Task<ReturnType<bool>> DeleteShipperByIdAsync(int shipperId)
    {
        var result = new ReturnType<bool>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "DELETE FROM Shippers WHERE ShipperID = @ShipperID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@ShipperID", shipperId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            result.Data = rowsAffected > 0;
            result.Success = true;
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    // Region CRUD methods - simplified implementation
    public async Task<ReturnListType<Region>> GetAllRegionsAsync()
    {
        var result = new ReturnListType<Region>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "SELECT RegionID, RegionDescription FROM Region ORDER BY RegionDescription";
            
            using var command = new SqliteCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                result.Data.Add(new Region
                {
                    RegionId = reader.GetInt32(0),
                    RegionDescription = reader.GetString(1)
                });
            }
            
            result.Success = true;
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    public async Task<ReturnType<Region>> GetRegionByIdAsync(int regionId)
    {
        var result = new ReturnType<Region>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "SELECT RegionID, RegionDescription FROM Region WHERE RegionID = @RegionID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@RegionID", regionId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                result.Data = new Region
                {
                    RegionId = reader.GetInt32(0),
                    RegionDescription = reader.GetString(1)
                };
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"Region with ID {regionId} not found");
            }
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    public async Task<ReturnType<Region>> AddRegionAsync(Region region)
    {
        var result = new ReturnType<Region>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "INSERT INTO Region (RegionID, RegionDescription) VALUES (@RegionID, @RegionDescription)";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@RegionID", region.RegionId);
            command.Parameters.AddWithValue("@RegionDescription", region.RegionDescription);
            
            await command.ExecuteNonQueryAsync();
            result.Data = region;
            result.Success = true;
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    public async Task<ReturnType<Region>> UpdateRegionAsync(Region region)
    {
        var result = new ReturnType<Region>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "UPDATE Region SET RegionDescription = @RegionDescription WHERE RegionID = @RegionID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@RegionID", region.RegionId);
            command.Parameters.AddWithValue("@RegionDescription", region.RegionDescription);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            
            if (rowsAffected > 0)
            {
                result.Data = region;
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"Region with ID {region.RegionId} not found");
            }
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    public async Task<ReturnType<bool>> DeleteRegionAsync(Region region) => await DeleteRegionByIdAsync(region.RegionId);

    public async Task<ReturnType<bool>> DeleteRegionByIdAsync(int regionId)
    {
        var result = new ReturnType<bool>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "DELETE FROM Region WHERE RegionID = @RegionID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@RegionID", regionId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            result.Data = rowsAffected > 0;
            result.Success = true;
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    // Territory CRUD methods
    public async Task<ReturnListType<Territory>> GetAllTerritoriesAsync()
    {
        var result = new ReturnListType<Territory>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "SELECT TerritoryID, TerritoryDescription, RegionID FROM Territories ORDER BY TerritoryDescription";
            
            using var command = new SqliteCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                result.Data.Add(new Territory
                {
                    TerritoryId = reader.GetString(0),
                    TerritoryDescription = reader.GetString(1),
                    RegionId = reader.GetInt32(2)
                });
            }
            
            result.Success = true;
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    public async Task<ReturnType<Territory>> GetTerritoryByIdAsync(string territoryId)
    {
        var result = new ReturnType<Territory>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "SELECT TerritoryID, TerritoryDescription, RegionID FROM Territories WHERE TerritoryID = @TerritoryID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@TerritoryID", territoryId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                result.Data = new Territory
                {
                    TerritoryId = reader.GetString(0),
                    TerritoryDescription = reader.GetString(1),
                    RegionId = reader.GetInt32(2)
                };
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"Territory with ID {territoryId} not found");
            }
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    public async Task<ReturnType<Territory>> AddTerritoryAsync(Territory territory)
    {
        var result = new ReturnType<Territory>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "INSERT INTO Territories (TerritoryID, TerritoryDescription, RegionID) VALUES (@TerritoryID, @TerritoryDescription, @RegionID)";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@TerritoryID", territory.TerritoryId);
            command.Parameters.AddWithValue("@TerritoryDescription", territory.TerritoryDescription);
            command.Parameters.AddWithValue("@RegionID", territory.RegionId);
            
            await command.ExecuteNonQueryAsync();
            result.Data = territory;
            result.Success = true;
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    public async Task<ReturnType<Territory>> UpdateTerritoryAsync(Territory territory)
    {
        var result = new ReturnType<Territory>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "UPDATE Territories SET TerritoryDescription = @TerritoryDescription, RegionID = @RegionID WHERE TerritoryID = @TerritoryID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@TerritoryID", territory.TerritoryId);
            command.Parameters.AddWithValue("@TerritoryDescription", territory.TerritoryDescription);
            command.Parameters.AddWithValue("@RegionID", territory.RegionId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            
            if (rowsAffected > 0)
            {
                result.Data = territory;
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"Territory with ID {territory.TerritoryId} not found");
            }
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    public async Task<ReturnType<bool>> DeleteTerritoryAsync(Territory territory) => await DeleteTerritoryByIdAsync(territory.TerritoryId);

    public async Task<ReturnType<bool>> DeleteTerritoryByIdAsync(string territoryId)
    {
        var result = new ReturnType<bool>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "DELETE FROM Territories WHERE TerritoryID = @TerritoryID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@TerritoryID", territoryId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            result.Data = rowsAffected > 0;
            result.Success = true;
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    // CustomerDemographic CRUD methods
    public async Task<ReturnListType<CustomerDemographic>> GetAllCustomerDemographicsAsync()
    {
        var result = new ReturnListType<CustomerDemographic>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "SELECT CustomerTypeID, CustomerDesc FROM CustomerDemographics ORDER BY CustomerTypeID";
            
            using var command = new SqliteCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                result.Data.Add(new CustomerDemographic
                {
                    CustomerTypeId = reader.GetString(0),
                    CustomerDesc = reader.IsDBNull(1) ? null : reader.GetString(1)
                });
            }
            
            result.Success = true;
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    public async Task<ReturnType<CustomerDemographic>> GetCustomerDemographicByIdAsync(string customerTypeId)
    {
        var result = new ReturnType<CustomerDemographic>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "SELECT CustomerTypeID, CustomerDesc FROM CustomerDemographics WHERE CustomerTypeID = @CustomerTypeID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@CustomerTypeID", customerTypeId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                result.Data = new CustomerDemographic
                {
                    CustomerTypeId = reader.GetString(0),
                    CustomerDesc = reader.IsDBNull(1) ? null : reader.GetString(1)
                };
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"CustomerDemographic with ID {customerTypeId} not found");
            }
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    public async Task<ReturnType<CustomerDemographic>> AddCustomerDemographicAsync(CustomerDemographic customerDemographic)
    {
        var result = new ReturnType<CustomerDemographic>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "INSERT INTO CustomerDemographics (CustomerTypeID, CustomerDesc) VALUES (@CustomerTypeID, @CustomerDesc)";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@CustomerTypeID", customerDemographic.CustomerTypeId);
            command.Parameters.AddWithValue("@CustomerDesc", customerDemographic.CustomerDesc ?? (object)DBNull.Value);
            
            await command.ExecuteNonQueryAsync();
            result.Data = customerDemographic;
            result.Success = true;
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    public async Task<ReturnType<CustomerDemographic>> UpdateCustomerDemographicAsync(CustomerDemographic customerDemographic)
    {
        var result = new ReturnType<CustomerDemographic>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "UPDATE CustomerDemographics SET CustomerDesc = @CustomerDesc WHERE CustomerTypeID = @CustomerTypeID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@CustomerTypeID", customerDemographic.CustomerTypeId);
            command.Parameters.AddWithValue("@CustomerDesc", customerDemographic.CustomerDesc ?? (object)DBNull.Value);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            
            if (rowsAffected > 0)
            {
                result.Data = customerDemographic;
                result.Success = true;
            }
            else
            {
                result.ErrorMessages.Add($"CustomerDemographic with ID {customerDemographic.CustomerTypeId} not found");
            }
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    public async Task<ReturnType<bool>> DeleteCustomerDemographicAsync(CustomerDemographic customerDemographic) => await DeleteCustomerDemographicByIdAsync(customerDemographic.CustomerTypeId);

    public async Task<ReturnType<bool>> DeleteCustomerDemographicByIdAsync(string customerTypeId)
    {
        var result = new ReturnType<bool>();
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            const string sql = "DELETE FROM CustomerDemographics WHERE CustomerTypeID = @CustomerTypeID";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@CustomerTypeID", customerTypeId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            result.Data = rowsAffected > 0;
            result.Success = true;
        }
        catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }
        return result;
    }

    #endregion
}