using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using NorthwindBlazor.Models;
using Microsoft.Extensions.Configuration;

namespace NorthwindBlazor.Data;

public class DataManager
{
    private readonly string _connectionString;

    public DataManager(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("NorthwindConnection") 
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string 'NorthwindConnection' not found");
    }

    #region Helper Methods

    private SqliteConnection GetConnection()
    {
        return new SqliteConnection(_connectionString);
    }

    private ReturnType<T> HandleError<T>(Exception ex, string operation)
    {
        return new ReturnType<T>(false, default(T), new List<string> { $"Error in {operation}: {ex.Message}" });
    }

    private ReturnListType<T> HandleListError<T>(Exception ex, string operation)
    {
        return new ReturnListType<T>(false, new List<T>(), new List<string> { $"Error in {operation}: {ex.Message}" });
    }

    #endregion

    #region Category Methods

    public ReturnListType<Category> GetAllCategories()
    {
        try
        {
            var categories = new List<Category>();
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT CategoryID, CategoryName, Description, Picture 
                FROM Categories";
            
            using var command = new SqliteCommand(sql, connection);
            using var reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                categories.Add(new Category
                {
                    CategoryId = reader.GetInt32("CategoryID"),
                    CategoryName = reader.IsDBNull("CategoryName") ? null : reader.GetString("CategoryName"),
                    Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                    Picture = reader.IsDBNull("Picture") ? null : (byte[])reader["Picture"]
                });
            }
            
            return new ReturnListType<Category>(true, categories);
        }
        catch (Exception ex)
        {
            return HandleListError<Category>(ex, "GetAllCategories");
        }
    }

    public ReturnType<Category> GetCategoryById(int categoryId)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT c.CategoryID, c.CategoryName, c.Description, c.Picture
                FROM Categories c
                WHERE c.CategoryID = @categoryId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@categoryId", categoryId);
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                var category = new Category
                {
                    CategoryId = reader.GetInt32("CategoryID"),
                    CategoryName = reader.IsDBNull("CategoryName") ? null : reader.GetString("CategoryName"),
                    Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                    Picture = reader.IsDBNull("Picture") ? null : (byte[])reader["Picture"]
                };
                
                // Load Products for this Category
                reader.Close();
                const string productsSql = @"
                    SELECT p.ProductID, p.ProductName, p.SupplierID, p.CategoryID, p.QuantityPerUnit, 
                           p.UnitPrice, p.UnitsInStock, p.UnitsOnOrder, p.ReorderLevel, p.Discontinued
                    FROM Products p
                    WHERE p.CategoryID = @categoryId";
                
                using var productsCommand = new SqliteCommand(productsSql, connection);
                productsCommand.Parameters.AddWithValue("@categoryId", categoryId);
                using var productsReader = productsCommand.ExecuteReader();
                
                var products = new List<Product>();
                while (productsReader.Read())
                {
                    products.Add(new Product
                    {
                        ProductId = productsReader.GetInt32("ProductID"),
                        ProductName = productsReader.GetString("ProductName"),
                        SupplierId = productsReader.IsDBNull("SupplierID") ? null : productsReader.GetInt32("SupplierID"),
                        CategoryId = productsReader.IsDBNull("CategoryID") ? null : productsReader.GetInt32("CategoryID"),
                        QuantityPerUnit = productsReader.IsDBNull("QuantityPerUnit") ? null : productsReader.GetString("QuantityPerUnit"),
                        UnitPrice = productsReader.IsDBNull("UnitPrice") ? null : productsReader.GetDouble("UnitPrice"),
                        UnitsInStock = productsReader.IsDBNull("UnitsInStock") ? null : productsReader.GetInt32("UnitsInStock"),
                        UnitsOnOrder = productsReader.IsDBNull("UnitsOnOrder") ? null : productsReader.GetInt32("UnitsOnOrder"),
                        ReorderLevel = productsReader.IsDBNull("ReorderLevel") ? null : productsReader.GetInt32("ReorderLevel"),
                        Discontinued = productsReader.GetString("Discontinued")
                    });
                }
                category.Products = products;
                
                return new ReturnType<Category>(true, category);
            }
            
            return new ReturnType<Category>(false, null, new List<string> { "Category not found" });
        }
        catch (Exception ex)
        {
            return HandleError<Category>(ex, "GetCategoryById");
        }
    }

    public ReturnType<Category> UpdateCategory(Category category)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                UPDATE Categories 
                SET CategoryName = @categoryName, Description = @description, Picture = @picture
                WHERE CategoryID = @categoryId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@categoryId", category.CategoryId);
            command.Parameters.AddWithValue("@categoryName", category.CategoryName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@description", category.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@picture", category.Picture ?? (object)DBNull.Value);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<Category>(true, category);
            }
            
            return new ReturnType<Category>(false, null, new List<string> { "Category not found or no changes made" });
        }
        catch (Exception ex)
        {
            return HandleError<Category>(ex, "UpdateCategory");
        }
    }

    public ReturnType<Category> DeleteCategory(Category category)
    {
        return DeleteCategoryById(category.CategoryId);
    }

    public ReturnType<Category> DeleteCategoryById(int categoryId)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = "DELETE FROM Categories WHERE CategoryID = @categoryId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@categoryId", categoryId);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<Category>(true, null);
            }
            
            return new ReturnType<Category>(false, null, new List<string> { "Category not found" });
        }
        catch (Exception ex)
        {
            return HandleError<Category>(ex, "DeleteCategoryById");
        }
    }

    public ReturnType<Category> AddCategory(Category category)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                INSERT INTO Categories (CategoryName, Description, Picture)
                VALUES (@categoryName, @description, @picture);
                SELECT last_insert_rowid();";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@categoryName", category.CategoryName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@description", category.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@picture", category.Picture ?? (object)DBNull.Value);
            
            var newId = Convert.ToInt32(command.ExecuteScalar());
            category.CategoryId = newId;
            
            return new ReturnType<Category>(true, category);
        }
        catch (Exception ex)
        {
            return HandleError<Category>(ex, "AddCategory");
        }
    }

    #endregion

    #region Customer Methods

    public ReturnListType<Customer> GetAllCustomers()
    {
        try
        {
            var customers = new List<Customer>();
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT CustomerID, CompanyName, ContactName, ContactTitle, Address, 
                       City, Region, PostalCode, Country, Phone, Fax 
                FROM Customers";
            
            using var command = new SqliteCommand(sql, connection);
            using var reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                customers.Add(new Customer
                {
                    CustomerId = reader.GetString("CustomerID"),
                    CompanyName = reader.IsDBNull("CompanyName") ? null : reader.GetString("CompanyName"),
                    ContactName = reader.IsDBNull("ContactName") ? null : reader.GetString("ContactName"),
                    ContactTitle = reader.IsDBNull("ContactTitle") ? null : reader.GetString("ContactTitle"),
                    Address = reader.IsDBNull("Address") ? null : reader.GetString("Address"),
                    City = reader.IsDBNull("City") ? null : reader.GetString("City"),
                    Region = reader.IsDBNull("Region") ? null : reader.GetString("Region"),
                    PostalCode = reader.IsDBNull("PostalCode") ? null : reader.GetString("PostalCode"),
                    Country = reader.IsDBNull("Country") ? null : reader.GetString("Country"),
                    Phone = reader.IsDBNull("Phone") ? null : reader.GetString("Phone"),
                    Fax = reader.IsDBNull("Fax") ? null : reader.GetString("Fax")
                });
            }
            
            return new ReturnListType<Customer>(true, customers);
        }
        catch (Exception ex)
        {
            return HandleListError<Customer>(ex, "GetAllCustomers");
        }
    }

    public ReturnType<Customer> GetCustomerById(string customerId)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT CustomerID, CompanyName, ContactName, ContactTitle, Address, 
                       City, Region, PostalCode, Country, Phone, Fax
                FROM Customers
                WHERE CustomerID = @customerId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@customerId", customerId);
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                var customer = new Customer
                {
                    CustomerId = reader.GetString("CustomerID"),
                    CompanyName = reader.IsDBNull("CompanyName") ? null : reader.GetString("CompanyName"),
                    ContactName = reader.IsDBNull("ContactName") ? null : reader.GetString("ContactName"),
                    ContactTitle = reader.IsDBNull("ContactTitle") ? null : reader.GetString("ContactTitle"),
                    Address = reader.IsDBNull("Address") ? null : reader.GetString("Address"),
                    City = reader.IsDBNull("City") ? null : reader.GetString("City"),
                    Region = reader.IsDBNull("Region") ? null : reader.GetString("Region"),
                    PostalCode = reader.IsDBNull("PostalCode") ? null : reader.GetString("PostalCode"),
                    Country = reader.IsDBNull("Country") ? null : reader.GetString("Country"),
                    Phone = reader.IsDBNull("Phone") ? null : reader.GetString("Phone"),
                    Fax = reader.IsDBNull("Fax") ? null : reader.GetString("Fax")
                };
                
                // Load Orders for this Customer
                reader.Close();
                const string ordersSql = @"
                    SELECT o.OrderID, o.CustomerID, o.EmployeeID, o.OrderDate, o.RequiredDate, 
                           o.ShippedDate, o.ShipVia, o.Freight, o.ShipName, o.ShipAddress, 
                           o.ShipCity, o.ShipRegion, o.ShipPostalCode, o.ShipCountry
                    FROM Orders o
                    WHERE o.CustomerID = @customerId";
                
                using var ordersCommand = new SqliteCommand(ordersSql, connection);
                ordersCommand.Parameters.AddWithValue("@customerId", customerId);
                using var ordersReader = ordersCommand.ExecuteReader();
                
                var orders = new List<Order>();
                while (ordersReader.Read())
                {
                    orders.Add(new Order
                    {
                        OrderId = ordersReader.GetInt32("OrderID"),
                        CustomerId = ordersReader.IsDBNull("CustomerID") ? null : ordersReader.GetString("CustomerID"),
                        EmployeeId = ordersReader.IsDBNull("EmployeeID") ? null : ordersReader.GetInt32("EmployeeID"),
                        OrderDate = ordersReader.IsDBNull("OrderDate") ? null : ordersReader.GetDateTime("OrderDate"),
                        RequiredDate = ordersReader.IsDBNull("RequiredDate") ? null : ordersReader.GetDateTime("RequiredDate"),
                        ShippedDate = ordersReader.IsDBNull("ShippedDate") ? null : ordersReader.GetDateTime("ShippedDate"),
                        ShipVia = ordersReader.IsDBNull("ShipVia") ? null : ordersReader.GetInt32("ShipVia"),
                        Freight = ordersReader.IsDBNull("Freight") ? null : ordersReader.GetInt32("Freight"),
                        ShipName = ordersReader.IsDBNull("ShipName") ? null : ordersReader.GetString("ShipName"),
                        ShipAddress = ordersReader.IsDBNull("ShipAddress") ? null : ordersReader.GetString("ShipAddress"),
                        ShipCity = ordersReader.IsDBNull("ShipCity") ? null : ordersReader.GetString("ShipCity"),
                        ShipRegion = ordersReader.IsDBNull("ShipRegion") ? null : ordersReader.GetString("ShipRegion"),
                        ShipPostalCode = ordersReader.IsDBNull("ShipPostalCode") ? null : ordersReader.GetString("ShipPostalCode"),
                        ShipCountry = ordersReader.IsDBNull("ShipCountry") ? null : ordersReader.GetString("ShipCountry")
                    });
                }
                customer.Orders = orders;
                
                return new ReturnType<Customer>(true, customer);
            }
            
            return new ReturnType<Customer>(false, null, new List<string> { "Customer not found" });
        }
        catch (Exception ex)
        {
            return HandleError<Customer>(ex, "GetCustomerById");
        }
    }

    public ReturnType<Customer> UpdateCustomer(Customer customer)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                UPDATE Customers 
                SET CompanyName = @companyName, ContactName = @contactName, ContactTitle = @contactTitle,
                    Address = @address, City = @city, Region = @region, PostalCode = @postalCode,
                    Country = @country, Phone = @phone, Fax = @fax
                WHERE CustomerID = @customerId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@customerId", customer.CustomerId);
            command.Parameters.AddWithValue("@companyName", customer.CompanyName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@contactName", customer.ContactName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@contactTitle", customer.ContactTitle ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@address", customer.Address ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@city", customer.City ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@region", customer.Region ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@postalCode", customer.PostalCode ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@country", customer.Country ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@phone", customer.Phone ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@fax", customer.Fax ?? (object)DBNull.Value);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<Customer>(true, customer);
            }
            
            return new ReturnType<Customer>(false, null, new List<string> { "Customer not found or no changes made" });
        }
        catch (Exception ex)
        {
            return HandleError<Customer>(ex, "UpdateCustomer");
        }
    }

    public ReturnType<Customer> DeleteCustomer(Customer customer)
    {
        return DeleteCustomerById(customer.CustomerId);
    }

    public ReturnType<Customer> DeleteCustomerById(string customerId)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = "DELETE FROM Customers WHERE CustomerID = @customerId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@customerId", customerId);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<Customer>(true, null);
            }
            
            return new ReturnType<Customer>(false, null, new List<string> { "Customer not found" });
        }
        catch (Exception ex)
        {
            return HandleError<Customer>(ex, "DeleteCustomerById");
        }
    }

    public ReturnType<Customer> AddCustomer(Customer customer)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                INSERT INTO Customers (CustomerID, CompanyName, ContactName, ContactTitle, Address, 
                                     City, Region, PostalCode, Country, Phone, Fax)
                VALUES (@customerId, @companyName, @contactName, @contactTitle, @address, 
                        @city, @region, @postalCode, @country, @phone, @fax)";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@customerId", customer.CustomerId);
            command.Parameters.AddWithValue("@companyName", customer.CompanyName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@contactName", customer.ContactName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@contactTitle", customer.ContactTitle ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@address", customer.Address ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@city", customer.City ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@region", customer.Region ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@postalCode", customer.PostalCode ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@country", customer.Country ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@phone", customer.Phone ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@fax", customer.Fax ?? (object)DBNull.Value);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<Customer>(true, customer);
            }
            
            return new ReturnType<Customer>(false, null, new List<string> { "Failed to add customer" });
        }
        catch (Exception ex)
        {
            return HandleError<Customer>(ex, "AddCustomer");
        }
    }

    #endregion

    #region Product Methods

    public ReturnListType<Product> GetAllProducts()
    {
        try
        {
            var products = new List<Product>();
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT p.ProductID, p.ProductName, p.SupplierID, p.CategoryID, p.QuantityPerUnit, 
                       p.UnitPrice, p.UnitsInStock, p.UnitsOnOrder, p.ReorderLevel, p.Discontinued,
                       c.CategoryName, c.Description as CategoryDescription, c.Picture,
                       s.CompanyName as SupplierCompanyName, s.ContactName as SupplierContactName
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID";
            
            using var command = new SqliteCommand(sql, connection);
            using var reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                var product = new Product
                {
                    ProductId = reader.GetInt32("ProductID"),
                    ProductName = reader.GetString("ProductName"),
                    SupplierId = reader.IsDBNull("SupplierID") ? null : reader.GetInt32("SupplierID"),
                    CategoryId = reader.IsDBNull("CategoryID") ? null : reader.GetInt32("CategoryID"),
                    QuantityPerUnit = reader.IsDBNull("QuantityPerUnit") ? null : reader.GetString("QuantityPerUnit"),
                    UnitPrice = reader.IsDBNull("UnitPrice") ? null : reader.GetDouble("UnitPrice"),
                    UnitsInStock = reader.IsDBNull("UnitsInStock") ? null : reader.GetInt32("UnitsInStock"),
                    UnitsOnOrder = reader.IsDBNull("UnitsOnOrder") ? null : reader.GetInt32("UnitsOnOrder"),
                    ReorderLevel = reader.IsDBNull("ReorderLevel") ? null : reader.GetInt32("ReorderLevel"),
                    Discontinued = reader.GetString("Discontinued")
                };

                // Set Category if available
                if (!reader.IsDBNull("CategoryID"))
                {
                    product.Category = new Category
                    {
                        CategoryId = reader.GetInt32("CategoryID"),
                        CategoryName = reader.IsDBNull("CategoryName") ? null : reader.GetString("CategoryName"),
                        Description = reader.IsDBNull("CategoryDescription") ? null : reader.GetString("CategoryDescription"),
                        Picture = reader.IsDBNull("Picture") ? null : (byte[])reader["Picture"]
                    };
                }

                // Set Supplier if available
                if (!reader.IsDBNull("SupplierID"))
                {
                    product.Supplier = new Supplier
                    {
                        SupplierId = reader.GetInt32("SupplierID"),
                        CompanyName = reader.IsDBNull("SupplierCompanyName") ? "" : reader.GetString("SupplierCompanyName"),
                        ContactName = reader.IsDBNull("SupplierContactName") ? null : reader.GetString("SupplierContactName")
                    };
                }

                products.Add(product);
            }
            
            return new ReturnListType<Product>(true, products);
        }
        catch (Exception ex)
        {
            return HandleListError<Product>(ex, "GetAllProducts");
        }
    }

    public ReturnType<Product> GetProductById(int productId)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT p.ProductID, p.ProductName, p.SupplierID, p.CategoryID, p.QuantityPerUnit, 
                       p.UnitPrice, p.UnitsInStock, p.UnitsOnOrder, p.ReorderLevel, p.Discontinued,
                       c.CategoryName, c.Description as CategoryDescription, c.Picture,
                       s.CompanyName as SupplierCompanyName, s.ContactName as SupplierContactName,
                       s.ContactTitle as SupplierContactTitle, s.Address as SupplierAddress,
                       s.City as SupplierCity, s.Region as SupplierRegion, s.PostalCode as SupplierPostalCode,
                       s.Country as SupplierCountry, s.Phone as SupplierPhone, s.Fax as SupplierFax,
                       s.HomePage as SupplierHomePage
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID
                WHERE p.ProductID = @productId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@productId", productId);
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                var product = new Product
                {
                    ProductId = reader.GetInt32("ProductID"),
                    ProductName = reader.GetString("ProductName"),
                    SupplierId = reader.IsDBNull("SupplierID") ? null : reader.GetInt32("SupplierID"),
                    CategoryId = reader.IsDBNull("CategoryID") ? null : reader.GetInt32("CategoryID"),
                    QuantityPerUnit = reader.IsDBNull("QuantityPerUnit") ? null : reader.GetString("QuantityPerUnit"),
                    UnitPrice = reader.IsDBNull("UnitPrice") ? null : reader.GetDouble("UnitPrice"),
                    UnitsInStock = reader.IsDBNull("UnitsInStock") ? null : reader.GetInt32("UnitsInStock"),
                    UnitsOnOrder = reader.IsDBNull("UnitsOnOrder") ? null : reader.GetInt32("UnitsOnOrder"),
                    ReorderLevel = reader.IsDBNull("ReorderLevel") ? null : reader.GetInt32("ReorderLevel"),
                    Discontinued = reader.GetString("Discontinued")
                };

                // Set Category if available
                if (!reader.IsDBNull("CategoryID"))
                {
                    product.Category = new Category
                    {
                        CategoryId = reader.GetInt32("CategoryID"),
                        CategoryName = reader.IsDBNull("CategoryName") ? null : reader.GetString("CategoryName"),
                        Description = reader.IsDBNull("CategoryDescription") ? null : reader.GetString("CategoryDescription"),
                        Picture = reader.IsDBNull("Picture") ? null : (byte[])reader["Picture"]
                    };
                }

                // Set Supplier if available
                if (!reader.IsDBNull("SupplierID"))
                {
                    product.Supplier = new Supplier
                    {
                        SupplierId = reader.GetInt32("SupplierID"),
                        CompanyName = reader.IsDBNull("SupplierCompanyName") ? "" : reader.GetString("SupplierCompanyName"),
                        ContactName = reader.IsDBNull("SupplierContactName") ? null : reader.GetString("SupplierContactName"),
                        ContactTitle = reader.IsDBNull("SupplierContactTitle") ? null : reader.GetString("SupplierContactTitle"),
                        Address = reader.IsDBNull("SupplierAddress") ? null : reader.GetString("SupplierAddress"),
                        City = reader.IsDBNull("SupplierCity") ? null : reader.GetString("SupplierCity"),
                        Region = reader.IsDBNull("SupplierRegion") ? null : reader.GetString("SupplierRegion"),
                        PostalCode = reader.IsDBNull("SupplierPostalCode") ? null : reader.GetString("SupplierPostalCode"),
                        Country = reader.IsDBNull("SupplierCountry") ? null : reader.GetString("SupplierCountry"),
                        Phone = reader.IsDBNull("SupplierPhone") ? null : reader.GetString("SupplierPhone"),
                        Fax = reader.IsDBNull("SupplierFax") ? null : reader.GetString("SupplierFax"),
                        HomePage = reader.IsDBNull("SupplierHomePage") ? null : reader.GetString("SupplierHomePage")
                    };
                }
                
                return new ReturnType<Product>(true, product);
            }
            
            return new ReturnType<Product>(false, null, new List<string> { "Product not found" });
        }
        catch (Exception ex)
        {
            return HandleError<Product>(ex, "GetProductById");
        }
    }

    public ReturnType<Product> UpdateProduct(Product product)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                UPDATE Products 
                SET ProductName = @productName, SupplierID = @supplierId, CategoryID = @categoryId,
                    QuantityPerUnit = @quantityPerUnit, UnitPrice = @unitPrice, UnitsInStock = @unitsInStock,
                    UnitsOnOrder = @unitsOnOrder, ReorderLevel = @reorderLevel, Discontinued = @discontinued
                WHERE ProductID = @productId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@productId", product.ProductId);
            command.Parameters.AddWithValue("@productName", product.ProductName);
            command.Parameters.AddWithValue("@supplierId", product.SupplierId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@categoryId", product.CategoryId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@quantityPerUnit", product.QuantityPerUnit ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@unitPrice", product.UnitPrice ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@unitsInStock", product.UnitsInStock ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@unitsOnOrder", product.UnitsOnOrder ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@reorderLevel", product.ReorderLevel ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@discontinued", product.Discontinued);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<Product>(true, product);
            }
            
            return new ReturnType<Product>(false, null, new List<string> { "Product not found or no changes made" });
        }
        catch (Exception ex)
        {
            return HandleError<Product>(ex, "UpdateProduct");
        }
    }

    public ReturnType<Product> DeleteProduct(Product product)
    {
        return DeleteProductById(product.ProductId);
    }

    public ReturnType<Product> DeleteProductById(int productId)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = "DELETE FROM Products WHERE ProductID = @productId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@productId", productId);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<Product>(true, null);
            }
            
            return new ReturnType<Product>(false, null, new List<string> { "Product not found" });
        }
        catch (Exception ex)
        {
            return HandleError<Product>(ex, "DeleteProductById");
        }
    }

    public ReturnType<Product> AddProduct(Product product)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                INSERT INTO Products (ProductName, SupplierID, CategoryID, QuantityPerUnit, UnitPrice, 
                                    UnitsInStock, UnitsOnOrder, ReorderLevel, Discontinued)
                VALUES (@productName, @supplierId, @categoryId, @quantityPerUnit, @unitPrice, 
                        @unitsInStock, @unitsOnOrder, @reorderLevel, @discontinued);
                SELECT last_insert_rowid();";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@productName", product.ProductName);
            command.Parameters.AddWithValue("@supplierId", product.SupplierId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@categoryId", product.CategoryId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@quantityPerUnit", product.QuantityPerUnit ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@unitPrice", product.UnitPrice ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@unitsInStock", product.UnitsInStock ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@unitsOnOrder", product.UnitsOnOrder ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@reorderLevel", product.ReorderLevel ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@discontinued", product.Discontinued);
            
            var newId = Convert.ToInt32(command.ExecuteScalar());
            product.ProductId = newId;
            
            return new ReturnType<Product>(true, product);
        }
        catch (Exception ex)
        {
            return HandleError<Product>(ex, "AddProduct");
        }
    }

    #endregion

    #region Order Methods

    public ReturnListType<Order> GetAllOrders()
    {
        try
        {
            var orders = new List<Order>();
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT o.OrderID, o.CustomerID, o.EmployeeID, o.OrderDate, o.RequiredDate, 
                       o.ShippedDate, o.ShipVia, o.Freight, o.ShipName, o.ShipAddress, 
                       o.ShipCity, o.ShipRegion, o.ShipPostalCode, o.ShipCountry,
                       c.CompanyName as CustomerCompanyName,
                       e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName,
                       s.CompanyName as ShipperCompanyName
                FROM Orders o
                LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
                LEFT JOIN Shippers s ON o.ShipVia = s.ShipperID";
            
            using var command = new SqliteCommand(sql, connection);
            using var reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                var order = new Order
                {
                    OrderId = reader.GetInt32("OrderID"),
                    CustomerId = reader.IsDBNull("CustomerID") ? null : reader.GetString("CustomerID"),
                    EmployeeId = reader.IsDBNull("EmployeeID") ? null : reader.GetInt32("EmployeeID"),
                    OrderDate = reader.IsDBNull("OrderDate") ? null : reader.GetDateTime("OrderDate"),
                    RequiredDate = reader.IsDBNull("RequiredDate") ? null : reader.GetDateTime("RequiredDate"),
                    ShippedDate = reader.IsDBNull("ShippedDate") ? null : reader.GetDateTime("ShippedDate"),
                    ShipVia = reader.IsDBNull("ShipVia") ? null : reader.GetInt32("ShipVia"),
                    Freight = reader.IsDBNull("Freight") ? null : reader.GetInt32("Freight"),
                    ShipName = reader.IsDBNull("ShipName") ? null : reader.GetString("ShipName"),
                    ShipAddress = reader.IsDBNull("ShipAddress") ? null : reader.GetString("ShipAddress"),
                    ShipCity = reader.IsDBNull("ShipCity") ? null : reader.GetString("ShipCity"),
                    ShipRegion = reader.IsDBNull("ShipRegion") ? null : reader.GetString("ShipRegion"),
                    ShipPostalCode = reader.IsDBNull("ShipPostalCode") ? null : reader.GetString("ShipPostalCode"),
                    ShipCountry = reader.IsDBNull("ShipCountry") ? null : reader.GetString("ShipCountry")
                };

                // Set Customer if available
                if (!reader.IsDBNull("CustomerID"))
                {
                    order.Customer = new Customer
                    {
                        CustomerId = reader.GetString("CustomerID"),
                        CompanyName = reader.IsDBNull("CustomerCompanyName") ? null : reader.GetString("CustomerCompanyName")
                    };
                }

                // Set Employee if available
                if (!reader.IsDBNull("EmployeeID"))
                {
                    order.Employee = new Employee
                    {
                        EmployeeId = reader.GetInt32("EmployeeID"),
                        FirstName = reader.IsDBNull("EmployeeFirstName") ? null : reader.GetString("EmployeeFirstName"),
                        LastName = reader.IsDBNull("EmployeeLastName") ? null : reader.GetString("EmployeeLastName")
                    };
                }

                // Set Shipper if available
                if (!reader.IsDBNull("ShipVia"))
                {
                    order.ShipViaNavigation = new Shipper
                    {
                        ShipperId = reader.GetInt32("ShipVia"),
                        CompanyName = reader.IsDBNull("ShipperCompanyName") ? "" : reader.GetString("ShipperCompanyName")
                    };
                }

                orders.Add(order);
            }
            
            return new ReturnListType<Order>(true, orders);
        }
        catch (Exception ex)
        {
            return HandleListError<Order>(ex, "GetAllOrders");
        }
    }

    public ReturnType<Order> GetOrderById(int orderId)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT o.OrderID, o.CustomerID, o.EmployeeID, o.OrderDate, o.RequiredDate, 
                       o.ShippedDate, o.ShipVia, o.Freight, o.ShipName, o.ShipAddress, 
                       o.ShipCity, o.ShipRegion, o.ShipPostalCode, o.ShipCountry,
                       c.CompanyName as CustomerCompanyName, c.ContactName as CustomerContactName,
                       e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName,
                       s.CompanyName as ShipperCompanyName, s.Phone as ShipperPhone
                FROM Orders o
                LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
                LEFT JOIN Shippers s ON o.ShipVia = s.ShipperID
                WHERE o.OrderID = @orderId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@orderId", orderId);
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                var order = new Order
                {
                    OrderId = reader.GetInt32("OrderID"),
                    CustomerId = reader.IsDBNull("CustomerID") ? null : reader.GetString("CustomerID"),
                    EmployeeId = reader.IsDBNull("EmployeeID") ? null : reader.GetInt32("EmployeeID"),
                    OrderDate = reader.IsDBNull("OrderDate") ? null : reader.GetDateTime("OrderDate"),
                    RequiredDate = reader.IsDBNull("RequiredDate") ? null : reader.GetDateTime("RequiredDate"),
                    ShippedDate = reader.IsDBNull("ShippedDate") ? null : reader.GetDateTime("ShippedDate"),
                    ShipVia = reader.IsDBNull("ShipVia") ? null : reader.GetInt32("ShipVia"),
                    Freight = reader.IsDBNull("Freight") ? null : reader.GetInt32("Freight"),
                    ShipName = reader.IsDBNull("ShipName") ? null : reader.GetString("ShipName"),
                    ShipAddress = reader.IsDBNull("ShipAddress") ? null : reader.GetString("ShipAddress"),
                    ShipCity = reader.IsDBNull("ShipCity") ? null : reader.GetString("ShipCity"),
                    ShipRegion = reader.IsDBNull("ShipRegion") ? null : reader.GetString("ShipRegion"),
                    ShipPostalCode = reader.IsDBNull("ShipPostalCode") ? null : reader.GetString("ShipPostalCode"),
                    ShipCountry = reader.IsDBNull("ShipCountry") ? null : reader.GetString("ShipCountry")
                };

                // Set Customer if available
                if (!reader.IsDBNull("CustomerID"))
                {
                    order.Customer = new Customer
                    {
                        CustomerId = reader.GetString("CustomerID"),
                        CompanyName = reader.IsDBNull("CustomerCompanyName") ? null : reader.GetString("CustomerCompanyName"),
                        ContactName = reader.IsDBNull("CustomerContactName") ? null : reader.GetString("CustomerContactName")
                    };
                }

                // Set Employee if available
                if (!reader.IsDBNull("EmployeeID"))
                {
                    order.Employee = new Employee
                    {
                        EmployeeId = reader.GetInt32("EmployeeID"),
                        FirstName = reader.IsDBNull("EmployeeFirstName") ? null : reader.GetString("EmployeeFirstName"),
                        LastName = reader.IsDBNull("EmployeeLastName") ? null : reader.GetString("EmployeeLastName")
                    };
                }

                // Set Shipper if available
                if (!reader.IsDBNull("ShipVia"))
                {
                    order.ShipViaNavigation = new Shipper
                    {
                        ShipperId = reader.GetInt32("ShipVia"),
                        CompanyName = reader.IsDBNull("ShipperCompanyName") ? "" : reader.GetString("ShipperCompanyName"),
                        Phone = reader.IsDBNull("ShipperPhone") ? null : reader.GetString("ShipperPhone")
                    };
                }

                // Load OrderDetails for this Order
                reader.Close();
                const string orderDetailsSql = @"
                    SELECT od.OrderID, od.ProductID, od.UnitPrice, od.Quantity, od.Discount,
                           p.ProductName
                    FROM [Order Details] od
                    LEFT JOIN Products p ON od.ProductID = p.ProductID
                    WHERE od.OrderID = @orderId";
                
                using var orderDetailsCommand = new SqliteCommand(orderDetailsSql, connection);
                orderDetailsCommand.Parameters.AddWithValue("@orderId", orderId);
                using var orderDetailsReader = orderDetailsCommand.ExecuteReader();
                
                var orderDetails = new List<OrderDetail>();
                while (orderDetailsReader.Read())
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = orderDetailsReader.GetInt32("OrderID"),
                        ProductId = orderDetailsReader.GetInt32("ProductID"),
                        UnitPrice = orderDetailsReader.GetDouble("UnitPrice"),
                        Quantity = orderDetailsReader.GetInt32("Quantity"),
                        Discount = orderDetailsReader.GetDouble("Discount")
                    };

                    if (!orderDetailsReader.IsDBNull("ProductName"))
                    {
                        orderDetail.Product = new Product
                        {
                            ProductId = orderDetailsReader.GetInt32("ProductID"),
                            ProductName = orderDetailsReader.GetString("ProductName")
                        };
                    }

                    orderDetails.Add(orderDetail);
                }
                order.OrderDetails = orderDetails;
                
                return new ReturnType<Order>(true, order);
            }
            
            return new ReturnType<Order>(false, null, new List<string> { "Order not found" });
        }
        catch (Exception ex)
        {
            return HandleError<Order>(ex, "GetOrderById");
        }
    }

    public ReturnType<Order> UpdateOrder(Order order)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                UPDATE Orders 
                SET CustomerID = @customerId, EmployeeID = @employeeId, OrderDate = @orderDate,
                    RequiredDate = @requiredDate, ShippedDate = @shippedDate, ShipVia = @shipVia,
                    Freight = @freight, ShipName = @shipName, ShipAddress = @shipAddress,
                    ShipCity = @shipCity, ShipRegion = @shipRegion, ShipPostalCode = @shipPostalCode,
                    ShipCountry = @shipCountry
                WHERE OrderID = @orderId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@orderId", order.OrderId);
            command.Parameters.AddWithValue("@customerId", order.CustomerId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@employeeId", order.EmployeeId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@orderDate", order.OrderDate ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@requiredDate", order.RequiredDate ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@shippedDate", order.ShippedDate ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@shipVia", order.ShipVia ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@freight", order.Freight ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@shipName", order.ShipName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@shipAddress", order.ShipAddress ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@shipCity", order.ShipCity ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@shipRegion", order.ShipRegion ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@shipPostalCode", order.ShipPostalCode ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@shipCountry", order.ShipCountry ?? (object)DBNull.Value);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<Order>(true, order);
            }
            
            return new ReturnType<Order>(false, null, new List<string> { "Order not found or no changes made" });
        }
        catch (Exception ex)
        {
            return HandleError<Order>(ex, "UpdateOrder");
        }
    }

    public ReturnType<Order> DeleteOrder(Order order)
    {
        return DeleteOrderById(order.OrderId);
    }

    public ReturnType<Order> DeleteOrderById(int orderId)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = "DELETE FROM Orders WHERE OrderID = @orderId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@orderId", orderId);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<Order>(true, null);
            }
            
            return new ReturnType<Order>(false, null, new List<string> { "Order not found" });
        }
        catch (Exception ex)
        {
            return HandleError<Order>(ex, "DeleteOrderById");
        }
    }

    public ReturnType<Order> AddOrder(Order order)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                INSERT INTO Orders (CustomerID, EmployeeID, OrderDate, RequiredDate, ShippedDate, 
                                  ShipVia, Freight, ShipName, ShipAddress, ShipCity, ShipRegion, 
                                  ShipPostalCode, ShipCountry)
                VALUES (@customerId, @employeeId, @orderDate, @requiredDate, @shippedDate, 
                        @shipVia, @freight, @shipName, @shipAddress, @shipCity, @shipRegion, 
                        @shipPostalCode, @shipCountry);
                SELECT last_insert_rowid();";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@customerId", order.CustomerId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@employeeId", order.EmployeeId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@orderDate", order.OrderDate ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@requiredDate", order.RequiredDate ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@shippedDate", order.ShippedDate ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@shipVia", order.ShipVia ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@freight", order.Freight ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@shipName", order.ShipName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@shipAddress", order.ShipAddress ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@shipCity", order.ShipCity ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@shipRegion", order.ShipRegion ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@shipPostalCode", order.ShipPostalCode ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@shipCountry", order.ShipCountry ?? (object)DBNull.Value);
            
            var newId = Convert.ToInt32(command.ExecuteScalar());
            order.OrderId = newId;
            
            return new ReturnType<Order>(true, order);
        }
        catch (Exception ex)
        {
            return HandleError<Order>(ex, "AddOrder");
        }
    }

    #endregion

    #region OrderDetail Methods

    public ReturnListType<OrderDetail> GetAllOrderDetails()
    {
        try
        {
            var orderDetails = new List<OrderDetail>();
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT od.OrderID, od.ProductID, od.UnitPrice, od.Quantity, od.Discount,
                       p.ProductName, p.Discontinued as ProductDiscontinued,
                       o.OrderDate, o.CustomerID
                FROM [Order Details] od
                LEFT JOIN Products p ON od.ProductID = p.ProductID
                LEFT JOIN Orders o ON od.OrderID = o.OrderID";
            
            using var command = new SqliteCommand(sql, connection);
            using var reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = reader.GetInt32("OrderID"),
                    ProductId = reader.GetInt32("ProductID"),
                    UnitPrice = reader.GetDouble("UnitPrice"),
                    Quantity = reader.GetInt32("Quantity"),
                    Discount = reader.GetDouble("Discount")
                };

                // Set Product if available
                if (!reader.IsDBNull("ProductName"))
                {
                    orderDetail.Product = new Product
                    {
                        ProductId = reader.GetInt32("ProductID"),
                        ProductName = reader.GetString("ProductName"),
                        Discontinued = reader.IsDBNull("ProductDiscontinued") ? "" : reader.GetString("ProductDiscontinued")
                    };
                }

                // Set Order if available
                if (!reader.IsDBNull("OrderDate"))
                {
                    orderDetail.Order = new Order
                    {
                        OrderId = reader.GetInt32("OrderID"),
                        OrderDate = reader.IsDBNull("OrderDate") ? null : reader.GetDateTime("OrderDate"),
                        CustomerId = reader.IsDBNull("CustomerID") ? null : reader.GetString("CustomerID")
                    };
                }

                orderDetails.Add(orderDetail);
            }
            
            return new ReturnListType<OrderDetail>(true, orderDetails);
        }
        catch (Exception ex)
        {
            return HandleListError<OrderDetail>(ex, "GetAllOrderDetails");
        }
    }

    public ReturnType<OrderDetail> GetOrderDetailById(int orderId, int productId)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT od.OrderID, od.ProductID, od.UnitPrice, od.Quantity, od.Discount,
                       p.ProductName, p.QuantityPerUnit, p.Discontinued as ProductDiscontinued,
                       o.OrderDate, o.CustomerID, o.ShipName
                FROM [Order Details] od
                LEFT JOIN Products p ON od.ProductID = p.ProductID
                LEFT JOIN Orders o ON od.OrderID = o.OrderID
                WHERE od.OrderID = @orderId AND od.ProductID = @productId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@orderId", orderId);
            command.Parameters.AddWithValue("@productId", productId);
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = reader.GetInt32("OrderID"),
                    ProductId = reader.GetInt32("ProductID"),
                    UnitPrice = reader.GetDouble("UnitPrice"),
                    Quantity = reader.GetInt32("Quantity"),
                    Discount = reader.GetDouble("Discount")
                };

                // Set Product if available
                if (!reader.IsDBNull("ProductName"))
                {
                    orderDetail.Product = new Product
                    {
                        ProductId = reader.GetInt32("ProductID"),
                        ProductName = reader.GetString("ProductName"),
                        QuantityPerUnit = reader.IsDBNull("QuantityPerUnit") ? null : reader.GetString("QuantityPerUnit"),
                        Discontinued = reader.IsDBNull("ProductDiscontinued") ? "" : reader.GetString("ProductDiscontinued")
                    };
                }

                // Set Order if available
                if (!reader.IsDBNull("OrderDate"))
                {
                    orderDetail.Order = new Order
                    {
                        OrderId = reader.GetInt32("OrderID"),
                        OrderDate = reader.IsDBNull("OrderDate") ? null : reader.GetDateTime("OrderDate"),
                        CustomerId = reader.IsDBNull("CustomerID") ? null : reader.GetString("CustomerID"),
                        ShipName = reader.IsDBNull("ShipName") ? null : reader.GetString("ShipName")
                    };
                }
                
                return new ReturnType<OrderDetail>(true, orderDetail);
            }
            
            return new ReturnType<OrderDetail>(false, null, new List<string> { "OrderDetail not found" });
        }
        catch (Exception ex)
        {
            return HandleError<OrderDetail>(ex, "GetOrderDetailById");
        }
    }

    public ReturnType<OrderDetail> UpdateOrderDetail(OrderDetail orderDetail)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                UPDATE [Order Details] 
                SET UnitPrice = @unitPrice, Quantity = @quantity, Discount = @discount
                WHERE OrderID = @orderId AND ProductID = @productId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@orderId", orderDetail.OrderId);
            command.Parameters.AddWithValue("@productId", orderDetail.ProductId);
            command.Parameters.AddWithValue("@unitPrice", orderDetail.UnitPrice);
            command.Parameters.AddWithValue("@quantity", orderDetail.Quantity);
            command.Parameters.AddWithValue("@discount", orderDetail.Discount);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<OrderDetail>(true, orderDetail);
            }
            
            return new ReturnType<OrderDetail>(false, null, new List<string> { "OrderDetail not found or no changes made" });
        }
        catch (Exception ex)
        {
            return HandleError<OrderDetail>(ex, "UpdateOrderDetail");
        }
    }

    public ReturnType<OrderDetail> DeleteOrderDetail(OrderDetail orderDetail)
    {
        return DeleteOrderDetailById(orderDetail.OrderId, orderDetail.ProductId);
    }

    public ReturnType<OrderDetail> DeleteOrderDetailById(int orderId, int productId)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = "DELETE FROM [Order Details] WHERE OrderID = @orderId AND ProductID = @productId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@orderId", orderId);
            command.Parameters.AddWithValue("@productId", productId);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<OrderDetail>(true, null);
            }
            
            return new ReturnType<OrderDetail>(false, null, new List<string> { "OrderDetail not found" });
        }
        catch (Exception ex)
        {
            return HandleError<OrderDetail>(ex, "DeleteOrderDetailById");
        }
    }

    public ReturnType<OrderDetail> AddOrderDetail(OrderDetail orderDetail)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                INSERT INTO [Order Details] (OrderID, ProductID, UnitPrice, Quantity, Discount)
                VALUES (@orderId, @productId, @unitPrice, @quantity, @discount)";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@orderId", orderDetail.OrderId);
            command.Parameters.AddWithValue("@productId", orderDetail.ProductId);
            command.Parameters.AddWithValue("@unitPrice", orderDetail.UnitPrice);
            command.Parameters.AddWithValue("@quantity", orderDetail.Quantity);
            command.Parameters.AddWithValue("@discount", orderDetail.Discount);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<OrderDetail>(true, orderDetail);
            }
            
            return new ReturnType<OrderDetail>(false, null, new List<string> { "Failed to add OrderDetail" });
        }
        catch (Exception ex)
        {
            return HandleError<OrderDetail>(ex, "AddOrderDetail");
        }
    }

    #endregion

    #region Employee Methods

    public ReturnListType<Employee> GetAllEmployees()
    {
        try
        {
            var employees = new List<Employee>();
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT e.EmployeeID, e.LastName, e.FirstName, e.Title, e.TitleOfCourtesy, 
                       e.BirthDate, e.HireDate, e.Address, e.City, e.Region, e.PostalCode, 
                       e.Country, e.HomePhone, e.Extension, e.Photo, e.Notes, e.ReportsTo, e.PhotoPath,
                       m.FirstName as ManagerFirstName, m.LastName as ManagerLastName
                FROM Employees e
                LEFT JOIN Employees m ON e.ReportsTo = m.EmployeeID";
            
            using var command = new SqliteCommand(sql, connection);
            using var reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                var employee = new Employee
                {
                    EmployeeId = reader.GetInt32("EmployeeID"),
                    LastName = reader.IsDBNull("LastName") ? null : reader.GetString("LastName"),
                    FirstName = reader.IsDBNull("FirstName") ? null : reader.GetString("FirstName"),
                    Title = reader.IsDBNull("Title") ? null : reader.GetString("Title"),
                    TitleOfCourtesy = reader.IsDBNull("TitleOfCourtesy") ? null : reader.GetString("TitleOfCourtesy"),
                    BirthDate = reader.IsDBNull("BirthDate") ? null : DateOnly.FromDateTime(reader.GetDateTime("BirthDate")),
                    HireDate = reader.IsDBNull("HireDate") ? null : DateOnly.FromDateTime(reader.GetDateTime("HireDate")),
                    Address = reader.IsDBNull("Address") ? null : reader.GetString("Address"),
                    City = reader.IsDBNull("City") ? null : reader.GetString("City"),
                    Region = reader.IsDBNull("Region") ? null : reader.GetString("Region"),
                    PostalCode = reader.IsDBNull("PostalCode") ? null : reader.GetString("PostalCode"),
                    Country = reader.IsDBNull("Country") ? null : reader.GetString("Country"),
                    HomePhone = reader.IsDBNull("HomePhone") ? null : reader.GetString("HomePhone"),
                    Extension = reader.IsDBNull("Extension") ? null : reader.GetString("Extension"),
                    Photo = reader.IsDBNull("Photo") ? null : (byte[])reader["Photo"],
                    Notes = reader.IsDBNull("Notes") ? null : reader.GetString("Notes"),
                    ReportsTo = reader.IsDBNull("ReportsTo") ? null : reader.GetInt32("ReportsTo"),
                    PhotoPath = reader.IsDBNull("PhotoPath") ? null : reader.GetString("PhotoPath")
                };

                // Set ReportsTo navigation if available
                if (!reader.IsDBNull("ReportsTo"))
                {
                    employee.ReportsToNavigation = new Employee
                    {
                        EmployeeId = reader.GetInt32("ReportsTo"),
                        FirstName = reader.IsDBNull("ManagerFirstName") ? null : reader.GetString("ManagerFirstName"),
                        LastName = reader.IsDBNull("ManagerLastName") ? null : reader.GetString("ManagerLastName")
                    };
                }

                employees.Add(employee);
            }
            
            return new ReturnListType<Employee>(true, employees);
        }
        catch (Exception ex)
        {
            return HandleListError<Employee>(ex, "GetAllEmployees");
        }
    }

    public ReturnType<Employee> GetEmployeeById(int employeeId)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT e.EmployeeID, e.LastName, e.FirstName, e.Title, e.TitleOfCourtesy, 
                       e.BirthDate, e.HireDate, e.Address, e.City, e.Region, e.PostalCode, 
                       e.Country, e.HomePhone, e.Extension, e.Photo, e.Notes, e.ReportsTo, e.PhotoPath,
                       m.FirstName as ManagerFirstName, m.LastName as ManagerLastName
                FROM Employees e
                LEFT JOIN Employees m ON e.ReportsTo = m.EmployeeID
                WHERE e.EmployeeID = @employeeId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@employeeId", employeeId);
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                var employee = new Employee
                {
                    EmployeeId = reader.GetInt32("EmployeeID"),
                    LastName = reader.IsDBNull("LastName") ? null : reader.GetString("LastName"),
                    FirstName = reader.IsDBNull("FirstName") ? null : reader.GetString("FirstName"),
                    Title = reader.IsDBNull("Title") ? null : reader.GetString("Title"),
                    TitleOfCourtesy = reader.IsDBNull("TitleOfCourtesy") ? null : reader.GetString("TitleOfCourtesy"),
                    BirthDate = reader.IsDBNull("BirthDate") ? null : DateOnly.FromDateTime(reader.GetDateTime("BirthDate")),
                    HireDate = reader.IsDBNull("HireDate") ? null : DateOnly.FromDateTime(reader.GetDateTime("HireDate")),
                    Address = reader.IsDBNull("Address") ? null : reader.GetString("Address"),
                    City = reader.IsDBNull("City") ? null : reader.GetString("City"),
                    Region = reader.IsDBNull("Region") ? null : reader.GetString("Region"),
                    PostalCode = reader.IsDBNull("PostalCode") ? null : reader.GetString("PostalCode"),
                    Country = reader.IsDBNull("Country") ? null : reader.GetString("Country"),
                    HomePhone = reader.IsDBNull("HomePhone") ? null : reader.GetString("HomePhone"),
                    Extension = reader.IsDBNull("Extension") ? null : reader.GetString("Extension"),
                    Photo = reader.IsDBNull("Photo") ? null : (byte[])reader["Photo"],
                    Notes = reader.IsDBNull("Notes") ? null : reader.GetString("Notes"),
                    ReportsTo = reader.IsDBNull("ReportsTo") ? null : reader.GetInt32("ReportsTo"),
                    PhotoPath = reader.IsDBNull("PhotoPath") ? null : reader.GetString("PhotoPath")
                };

                // Set ReportsTo navigation if available
                if (!reader.IsDBNull("ReportsTo"))
                {
                    employee.ReportsToNavigation = new Employee
                    {
                        EmployeeId = reader.GetInt32("ReportsTo"),
                        FirstName = reader.IsDBNull("ManagerFirstName") ? null : reader.GetString("ManagerFirstName"),
                        LastName = reader.IsDBNull("ManagerLastName") ? null : reader.GetString("ManagerLastName")
                    };
                }
                
                return new ReturnType<Employee>(true, employee);
            }
            
            return new ReturnType<Employee>(false, null, new List<string> { "Employee not found" });
        }
        catch (Exception ex)
        {
            return HandleError<Employee>(ex, "GetEmployeeById");
        }
    }

    public ReturnType<Employee> UpdateEmployee(Employee employee)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                UPDATE Employees 
                SET LastName = @lastName, FirstName = @firstName, Title = @title, TitleOfCourtesy = @titleOfCourtesy,
                    BirthDate = @birthDate, HireDate = @hireDate, Address = @address, City = @city,
                    Region = @region, PostalCode = @postalCode, Country = @country, HomePhone = @homePhone,
                    Extension = @extension, Photo = @photo, Notes = @notes, ReportsTo = @reportsTo, PhotoPath = @photoPath
                WHERE EmployeeID = @employeeId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@employeeId", employee.EmployeeId);
            command.Parameters.AddWithValue("@lastName", employee.LastName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@firstName", employee.FirstName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@title", employee.Title ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@titleOfCourtesy", employee.TitleOfCourtesy ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@birthDate", employee.BirthDate?.ToDateTime(TimeOnly.MinValue) ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@hireDate", employee.HireDate?.ToDateTime(TimeOnly.MinValue) ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@address", employee.Address ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@city", employee.City ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@region", employee.Region ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@postalCode", employee.PostalCode ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@country", employee.Country ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@homePhone", employee.HomePhone ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@extension", employee.Extension ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@photo", employee.Photo ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@notes", employee.Notes ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@reportsTo", employee.ReportsTo ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@photoPath", employee.PhotoPath ?? (object)DBNull.Value);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<Employee>(true, employee);
            }
            
            return new ReturnType<Employee>(false, null, new List<string> { "Employee not found or no changes made" });
        }
        catch (Exception ex)
        {
            return HandleError<Employee>(ex, "UpdateEmployee");
        }
    }

    public ReturnType<Employee> DeleteEmployee(Employee employee)
    {
        return DeleteEmployeeById(employee.EmployeeId);
    }

    public ReturnType<Employee> DeleteEmployeeById(int employeeId)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = "DELETE FROM Employees WHERE EmployeeID = @employeeId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@employeeId", employeeId);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<Employee>(true, null);
            }
            
            return new ReturnType<Employee>(false, null, new List<string> { "Employee not found" });
        }
        catch (Exception ex)
        {
            return HandleError<Employee>(ex, "DeleteEmployeeById");
        }
    }

    public ReturnType<Employee> AddEmployee(Employee employee)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                INSERT INTO Employees (LastName, FirstName, Title, TitleOfCourtesy, BirthDate, HireDate, 
                                     Address, City, Region, PostalCode, Country, HomePhone, Extension, 
                                     Photo, Notes, ReportsTo, PhotoPath)
                VALUES (@lastName, @firstName, @title, @titleOfCourtesy, @birthDate, @hireDate, 
                        @address, @city, @region, @postalCode, @country, @homePhone, @extension, 
                        @photo, @notes, @reportsTo, @photoPath);
                SELECT last_insert_rowid();";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@lastName", employee.LastName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@firstName", employee.FirstName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@title", employee.Title ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@titleOfCourtesy", employee.TitleOfCourtesy ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@birthDate", employee.BirthDate?.ToDateTime(TimeOnly.MinValue) ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@hireDate", employee.HireDate?.ToDateTime(TimeOnly.MinValue) ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@address", employee.Address ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@city", employee.City ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@region", employee.Region ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@postalCode", employee.PostalCode ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@country", employee.Country ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@homePhone", employee.HomePhone ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@extension", employee.Extension ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@photo", employee.Photo ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@notes", employee.Notes ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@reportsTo", employee.ReportsTo ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@photoPath", employee.PhotoPath ?? (object)DBNull.Value);
            
            var newId = Convert.ToInt32(command.ExecuteScalar());
            employee.EmployeeId = newId;
            
            return new ReturnType<Employee>(true, employee);
        }
        catch (Exception ex)
        {
            return HandleError<Employee>(ex, "AddEmployee");
        }
    }

    #endregion

    #region Supplier Methods

    public ReturnListType<Supplier> GetAllSuppliers()
    {
        try
        {
            var suppliers = new List<Supplier>();
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT SupplierID, CompanyName, ContactName, ContactTitle, Address, 
                       City, Region, PostalCode, Country, Phone, Fax, HomePage 
                FROM Suppliers";
            
            using var command = new SqliteCommand(sql, connection);
            using var reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                suppliers.Add(new Supplier
                {
                    SupplierId = reader.GetInt32("SupplierID"),
                    CompanyName = reader.GetString("CompanyName"),
                    ContactName = reader.IsDBNull("ContactName") ? null : reader.GetString("ContactName"),
                    ContactTitle = reader.IsDBNull("ContactTitle") ? null : reader.GetString("ContactTitle"),
                    Address = reader.IsDBNull("Address") ? null : reader.GetString("Address"),
                    City = reader.IsDBNull("City") ? null : reader.GetString("City"),
                    Region = reader.IsDBNull("Region") ? null : reader.GetString("Region"),
                    PostalCode = reader.IsDBNull("PostalCode") ? null : reader.GetString("PostalCode"),
                    Country = reader.IsDBNull("Country") ? null : reader.GetString("Country"),
                    Phone = reader.IsDBNull("Phone") ? null : reader.GetString("Phone"),
                    Fax = reader.IsDBNull("Fax") ? null : reader.GetString("Fax"),
                    HomePage = reader.IsDBNull("HomePage") ? null : reader.GetString("HomePage")
                });
            }
            
            return new ReturnListType<Supplier>(true, suppliers);
        }
        catch (Exception ex)
        {
            return HandleListError<Supplier>(ex, "GetAllSuppliers");
        }
    }

    public ReturnType<Supplier> GetSupplierById(int supplierId)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT SupplierID, CompanyName, ContactName, ContactTitle, Address, 
                       City, Region, PostalCode, Country, Phone, Fax, HomePage
                FROM Suppliers
                WHERE SupplierID = @supplierId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@supplierId", supplierId);
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                var supplier = new Supplier
                {
                    SupplierId = reader.GetInt32("SupplierID"),
                    CompanyName = reader.GetString("CompanyName"),
                    ContactName = reader.IsDBNull("ContactName") ? null : reader.GetString("ContactName"),
                    ContactTitle = reader.IsDBNull("ContactTitle") ? null : reader.GetString("ContactTitle"),
                    Address = reader.IsDBNull("Address") ? null : reader.GetString("Address"),
                    City = reader.IsDBNull("City") ? null : reader.GetString("City"),
                    Region = reader.IsDBNull("Region") ? null : reader.GetString("Region"),
                    PostalCode = reader.IsDBNull("PostalCode") ? null : reader.GetString("PostalCode"),
                    Country = reader.IsDBNull("Country") ? null : reader.GetString("Country"),
                    Phone = reader.IsDBNull("Phone") ? null : reader.GetString("Phone"),
                    Fax = reader.IsDBNull("Fax") ? null : reader.GetString("Fax"),
                    HomePage = reader.IsDBNull("HomePage") ? null : reader.GetString("HomePage")
                };
                
                // Load Products for this Supplier
                reader.Close();
                const string productsSql = @"
                    SELECT ProductID, ProductName, CategoryID, QuantityPerUnit, UnitPrice, 
                           UnitsInStock, UnitsOnOrder, ReorderLevel, Discontinued
                    FROM Products
                    WHERE SupplierID = @supplierId";
                
                using var productsCommand = new SqliteCommand(productsSql, connection);
                productsCommand.Parameters.AddWithValue("@supplierId", supplierId);
                using var productsReader = productsCommand.ExecuteReader();
                
                var products = new List<Product>();
                while (productsReader.Read())
                {
                    products.Add(new Product
                    {
                        ProductId = productsReader.GetInt32("ProductID"),
                        ProductName = productsReader.GetString("ProductName"),
                        CategoryId = productsReader.IsDBNull("CategoryID") ? null : productsReader.GetInt32("CategoryID"),
                        QuantityPerUnit = productsReader.IsDBNull("QuantityPerUnit") ? null : productsReader.GetString("QuantityPerUnit"),
                        UnitPrice = productsReader.IsDBNull("UnitPrice") ? null : productsReader.GetDouble("UnitPrice"),
                        UnitsInStock = productsReader.IsDBNull("UnitsInStock") ? null : productsReader.GetInt32("UnitsInStock"),
                        UnitsOnOrder = productsReader.IsDBNull("UnitsOnOrder") ? null : productsReader.GetInt32("UnitsOnOrder"),
                        ReorderLevel = productsReader.IsDBNull("ReorderLevel") ? null : productsReader.GetInt32("ReorderLevel"),
                        Discontinued = productsReader.GetString("Discontinued")
                    });
                }
                supplier.Products = products;
                
                return new ReturnType<Supplier>(true, supplier);
            }
            
            return new ReturnType<Supplier>(false, null, new List<string> { "Supplier not found" });
        }
        catch (Exception ex)
        {
            return HandleError<Supplier>(ex, "GetSupplierById");
        }
    }

    public ReturnType<Supplier> UpdateSupplier(Supplier supplier)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                UPDATE Suppliers 
                SET CompanyName = @companyName, ContactName = @contactName, ContactTitle = @contactTitle,
                    Address = @address, City = @city, Region = @region, PostalCode = @postalCode,
                    Country = @country, Phone = @phone, Fax = @fax, HomePage = @homePage
                WHERE SupplierID = @supplierId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@supplierId", supplier.SupplierId);
            command.Parameters.AddWithValue("@companyName", supplier.CompanyName);
            command.Parameters.AddWithValue("@contactName", supplier.ContactName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@contactTitle", supplier.ContactTitle ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@address", supplier.Address ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@city", supplier.City ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@region", supplier.Region ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@postalCode", supplier.PostalCode ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@country", supplier.Country ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@phone", supplier.Phone ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@fax", supplier.Fax ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@homePage", supplier.HomePage ?? (object)DBNull.Value);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<Supplier>(true, supplier);
            }
            
            return new ReturnType<Supplier>(false, null, new List<string> { "Supplier not found or no changes made" });
        }
        catch (Exception ex)
        {
            return HandleError<Supplier>(ex, "UpdateSupplier");
        }
    }

    public ReturnType<Supplier> DeleteSupplier(Supplier supplier)
    {
        return DeleteSupplierById(supplier.SupplierId);
    }

    public ReturnType<Supplier> DeleteSupplierById(int supplierId)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = "DELETE FROM Suppliers WHERE SupplierID = @supplierId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@supplierId", supplierId);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<Supplier>(true, null);
            }
            
            return new ReturnType<Supplier>(false, null, new List<string> { "Supplier not found" });
        }
        catch (Exception ex)
        {
            return HandleError<Supplier>(ex, "DeleteSupplierById");
        }
    }

    public ReturnType<Supplier> AddSupplier(Supplier supplier)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                INSERT INTO Suppliers (CompanyName, ContactName, ContactTitle, Address, City, 
                                     Region, PostalCode, Country, Phone, Fax, HomePage)
                VALUES (@companyName, @contactName, @contactTitle, @address, @city, 
                        @region, @postalCode, @country, @phone, @fax, @homePage);
                SELECT last_insert_rowid();";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@companyName", supplier.CompanyName);
            command.Parameters.AddWithValue("@contactName", supplier.ContactName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@contactTitle", supplier.ContactTitle ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@address", supplier.Address ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@city", supplier.City ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@region", supplier.Region ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@postalCode", supplier.PostalCode ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@country", supplier.Country ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@phone", supplier.Phone ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@fax", supplier.Fax ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@homePage", supplier.HomePage ?? (object)DBNull.Value);
            
            var newId = Convert.ToInt32(command.ExecuteScalar());
            supplier.SupplierId = newId;
            
            return new ReturnType<Supplier>(true, supplier);
        }
        catch (Exception ex)
        {
            return HandleError<Supplier>(ex, "AddSupplier");
        }
    }

    #endregion

    #region Shipper Methods

    public ReturnListType<Shipper> GetAllShippers()
    {
        try
        {
            var shippers = new List<Shipper>();
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT ShipperID, CompanyName, Phone 
                FROM Shippers";
            
            using var command = new SqliteCommand(sql, connection);
            using var reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                shippers.Add(new Shipper
                {
                    ShipperId = reader.GetInt32("ShipperID"),
                    CompanyName = reader.GetString("CompanyName"),
                    Phone = reader.IsDBNull("Phone") ? null : reader.GetString("Phone")
                });
            }
            
            return new ReturnListType<Shipper>(true, shippers);
        }
        catch (Exception ex)
        {
            return HandleListError<Shipper>(ex, "GetAllShippers");
        }
    }

    public ReturnType<Shipper> GetShipperById(int shipperId)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT ShipperID, CompanyName, Phone
                FROM Shippers
                WHERE ShipperID = @shipperId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@shipperId", shipperId);
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                var shipper = new Shipper
                {
                    ShipperId = reader.GetInt32("ShipperID"),
                    CompanyName = reader.GetString("CompanyName"),
                    Phone = reader.IsDBNull("Phone") ? null : reader.GetString("Phone")
                };
                
                // Load Orders for this Shipper
                reader.Close();
                const string ordersSql = @"
                    SELECT OrderID, CustomerID, EmployeeID, OrderDate, RequiredDate, 
                           ShippedDate, Freight, ShipName, ShipAddress, ShipCity
                    FROM Orders
                    WHERE ShipVia = @shipperId";
                
                using var ordersCommand = new SqliteCommand(ordersSql, connection);
                ordersCommand.Parameters.AddWithValue("@shipperId", shipperId);
                using var ordersReader = ordersCommand.ExecuteReader();
                
                var orders = new List<Order>();
                while (ordersReader.Read())
                {
                    orders.Add(new Order
                    {
                        OrderId = ordersReader.GetInt32("OrderID"),
                        CustomerId = ordersReader.IsDBNull("CustomerID") ? null : ordersReader.GetString("CustomerID"),
                        EmployeeId = ordersReader.IsDBNull("EmployeeID") ? null : ordersReader.GetInt32("EmployeeID"),
                        OrderDate = ordersReader.IsDBNull("OrderDate") ? null : ordersReader.GetDateTime("OrderDate"),
                        RequiredDate = ordersReader.IsDBNull("RequiredDate") ? null : ordersReader.GetDateTime("RequiredDate"),
                        ShippedDate = ordersReader.IsDBNull("ShippedDate") ? null : ordersReader.GetDateTime("ShippedDate"),
                        Freight = ordersReader.IsDBNull("Freight") ? null : ordersReader.GetInt32("Freight"),
                        ShipName = ordersReader.IsDBNull("ShipName") ? null : ordersReader.GetString("ShipName"),
                        ShipAddress = ordersReader.IsDBNull("ShipAddress") ? null : ordersReader.GetString("ShipAddress"),
                        ShipCity = ordersReader.IsDBNull("ShipCity") ? null : ordersReader.GetString("ShipCity")
                    });
                }
                shipper.Orders = orders;
                
                return new ReturnType<Shipper>(true, shipper);
            }
            
            return new ReturnType<Shipper>(false, null, new List<string> { "Shipper not found" });
        }
        catch (Exception ex)
        {
            return HandleError<Shipper>(ex, "GetShipperById");
        }
    }

    public ReturnType<Shipper> UpdateShipper(Shipper shipper)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                UPDATE Shippers 
                SET CompanyName = @companyName, Phone = @phone
                WHERE ShipperID = @shipperId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@shipperId", shipper.ShipperId);
            command.Parameters.AddWithValue("@companyName", shipper.CompanyName);
            command.Parameters.AddWithValue("@phone", shipper.Phone ?? (object)DBNull.Value);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<Shipper>(true, shipper);
            }
            
            return new ReturnType<Shipper>(false, null, new List<string> { "Shipper not found or no changes made" });
        }
        catch (Exception ex)
        {
            return HandleError<Shipper>(ex, "UpdateShipper");
        }
    }

    public ReturnType<Shipper> DeleteShipper(Shipper shipper)
    {
        return DeleteShipperById(shipper.ShipperId);
    }

    public ReturnType<Shipper> DeleteShipperById(int shipperId)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = "DELETE FROM Shippers WHERE ShipperID = @shipperId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@shipperId", shipperId);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<Shipper>(true, null);
            }
            
            return new ReturnType<Shipper>(false, null, new List<string> { "Shipper not found" });
        }
        catch (Exception ex)
        {
            return HandleError<Shipper>(ex, "DeleteShipperById");
        }
    }

    public ReturnType<Shipper> AddShipper(Shipper shipper)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                INSERT INTO Shippers (CompanyName, Phone)
                VALUES (@companyName, @phone);
                SELECT last_insert_rowid();";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@companyName", shipper.CompanyName);
            command.Parameters.AddWithValue("@phone", shipper.Phone ?? (object)DBNull.Value);
            
            var newId = Convert.ToInt32(command.ExecuteScalar());
            shipper.ShipperId = newId;
            
            return new ReturnType<Shipper>(true, shipper);
        }
        catch (Exception ex)
        {
            return HandleError<Shipper>(ex, "AddShipper");
        }
    }

    #endregion

    #region Region Methods

    public ReturnListType<Region> GetAllRegions()
    {
        try
        {
            var regions = new List<Region>();
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT RegionID, RegionDescription 
                FROM Region";
            
            using var command = new SqliteCommand(sql, connection);
            using var reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                regions.Add(new Region
                {
                    RegionId = reader.GetInt32("RegionID"),
                    RegionDescription = reader.GetString("RegionDescription")
                });
            }
            
            return new ReturnListType<Region>(true, regions);
        }
        catch (Exception ex)
        {
            return HandleListError<Region>(ex, "GetAllRegions");
        }
    }

    public ReturnType<Region> GetRegionById(int regionId)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT RegionID, RegionDescription
                FROM Region
                WHERE RegionID = @regionId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@regionId", regionId);
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                var region = new Region
                {
                    RegionId = reader.GetInt32("RegionID"),
                    RegionDescription = reader.GetString("RegionDescription")
                };
                
                // Load Territories for this Region
                reader.Close();
                const string territoriesSql = @"
                    SELECT TerritoryID, TerritoryDescription, RegionID
                    FROM Territories
                    WHERE RegionID = @regionId";
                
                using var territoriesCommand = new SqliteCommand(territoriesSql, connection);
                territoriesCommand.Parameters.AddWithValue("@regionId", regionId);
                using var territoriesReader = territoriesCommand.ExecuteReader();
                
                var territories = new List<Territory>();
                while (territoriesReader.Read())
                {
                    territories.Add(new Territory
                    {
                        TerritoryId = territoriesReader.GetString("TerritoryID"),
                        TerritoryDescription = territoriesReader.GetString("TerritoryDescription"),
                        RegionId = territoriesReader.GetInt32("RegionID")
                    });
                }
                region.Territories = territories;
                
                return new ReturnType<Region>(true, region);
            }
            
            return new ReturnType<Region>(false, null, new List<string> { "Region not found" });
        }
        catch (Exception ex)
        {
            return HandleError<Region>(ex, "GetRegionById");
        }
    }

    public ReturnType<Region> UpdateRegion(Region region)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                UPDATE Region 
                SET RegionDescription = @regionDescription
                WHERE RegionID = @regionId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@regionId", region.RegionId);
            command.Parameters.AddWithValue("@regionDescription", region.RegionDescription);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<Region>(true, region);
            }
            
            return new ReturnType<Region>(false, null, new List<string> { "Region not found or no changes made" });
        }
        catch (Exception ex)
        {
            return HandleError<Region>(ex, "UpdateRegion");
        }
    }

    public ReturnType<Region> DeleteRegion(Region region)
    {
        return DeleteRegionById(region.RegionId);
    }

    public ReturnType<Region> DeleteRegionById(int regionId)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = "DELETE FROM Region WHERE RegionID = @regionId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@regionId", regionId);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<Region>(true, null);
            }
            
            return new ReturnType<Region>(false, null, new List<string> { "Region not found" });
        }
        catch (Exception ex)
        {
            return HandleError<Region>(ex, "DeleteRegionById");
        }
    }

    public ReturnType<Region> AddRegion(Region region)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                INSERT INTO Region (RegionDescription)
                VALUES (@regionDescription);
                SELECT last_insert_rowid();";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@regionDescription", region.RegionDescription);
            
            var newId = Convert.ToInt32(command.ExecuteScalar());
            region.RegionId = newId;
            
            return new ReturnType<Region>(true, region);
        }
        catch (Exception ex)
        {
            return HandleError<Region>(ex, "AddRegion");
        }
    }

    #endregion

    #region Territory Methods

    public ReturnListType<Territory> GetAllTerritories()
    {
        try
        {
            var territories = new List<Territory>();
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT t.TerritoryID, t.TerritoryDescription, t.RegionID,
                       r.RegionDescription
                FROM Territories t
                LEFT JOIN Region r ON t.RegionID = r.RegionID";
            
            using var command = new SqliteCommand(sql, connection);
            using var reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                var territory = new Territory
                {
                    TerritoryId = reader.GetString("TerritoryID"),
                    TerritoryDescription = reader.GetString("TerritoryDescription"),
                    RegionId = reader.GetInt32("RegionID")
                };

                // Set Region if available
                if (!reader.IsDBNull("RegionDescription"))
                {
                    territory.Region = new Region
                    {
                        RegionId = reader.GetInt32("RegionID"),
                        RegionDescription = reader.GetString("RegionDescription")
                    };
                }

                territories.Add(territory);
            }
            
            return new ReturnListType<Territory>(true, territories);
        }
        catch (Exception ex)
        {
            return HandleListError<Territory>(ex, "GetAllTerritories");
        }
    }

    public ReturnType<Territory> GetTerritoryById(string territoryId)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT t.TerritoryID, t.TerritoryDescription, t.RegionID,
                       r.RegionDescription
                FROM Territories t
                LEFT JOIN Region r ON t.RegionID = r.RegionID
                WHERE t.TerritoryID = @territoryId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@territoryId", territoryId);
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                var territory = new Territory
                {
                    TerritoryId = reader.GetString("TerritoryID"),
                    TerritoryDescription = reader.GetString("TerritoryDescription"),
                    RegionId = reader.GetInt32("RegionID")
                };

                // Set Region if available
                if (!reader.IsDBNull("RegionDescription"))
                {
                    territory.Region = new Region
                    {
                        RegionId = reader.GetInt32("RegionID"),
                        RegionDescription = reader.GetString("RegionDescription")
                    };
                }
                
                return new ReturnType<Territory>(true, territory);
            }
            
            return new ReturnType<Territory>(false, null, new List<string> { "Territory not found" });
        }
        catch (Exception ex)
        {
            return HandleError<Territory>(ex, "GetTerritoryById");
        }
    }

    public ReturnType<Territory> UpdateTerritory(Territory territory)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                UPDATE Territories 
                SET TerritoryDescription = @territoryDescription, RegionID = @regionId
                WHERE TerritoryID = @territoryId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@territoryId", territory.TerritoryId);
            command.Parameters.AddWithValue("@territoryDescription", territory.TerritoryDescription);
            command.Parameters.AddWithValue("@regionId", territory.RegionId);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<Territory>(true, territory);
            }
            
            return new ReturnType<Territory>(false, null, new List<string> { "Territory not found or no changes made" });
        }
        catch (Exception ex)
        {
            return HandleError<Territory>(ex, "UpdateTerritory");
        }
    }

    public ReturnType<Territory> DeleteTerritory(Territory territory)
    {
        return DeleteTerritoryById(territory.TerritoryId);
    }

    public ReturnType<Territory> DeleteTerritoryById(string territoryId)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = "DELETE FROM Territories WHERE TerritoryID = @territoryId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@territoryId", territoryId);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<Territory>(true, null);
            }
            
            return new ReturnType<Territory>(false, null, new List<string> { "Territory not found" });
        }
        catch (Exception ex)
        {
            return HandleError<Territory>(ex, "DeleteTerritoryById");
        }
    }

    public ReturnType<Territory> AddTerritory(Territory territory)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                INSERT INTO Territories (TerritoryID, TerritoryDescription, RegionID)
                VALUES (@territoryId, @territoryDescription, @regionId)";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@territoryId", territory.TerritoryId);
            command.Parameters.AddWithValue("@territoryDescription", territory.TerritoryDescription);
            command.Parameters.AddWithValue("@regionId", territory.RegionId);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<Territory>(true, territory);
            }
            
            return new ReturnType<Territory>(false, null, new List<string> { "Failed to add Territory" });
        }
        catch (Exception ex)
        {
            return HandleError<Territory>(ex, "AddTerritory");
        }
    }

    #endregion

    #region CustomerDemographic Methods

    public ReturnListType<CustomerDemographic> GetAllCustomerDemographics()
    {
        try
        {
            var customerDemographics = new List<CustomerDemographic>();
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT CustomerTypeID, CustomerDesc 
                FROM CustomerDemographics";
            
            using var command = new SqliteCommand(sql, connection);
            using var reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                customerDemographics.Add(new CustomerDemographic
                {
                    CustomerTypeId = reader.GetString("CustomerTypeID"),
                    CustomerDesc = reader.IsDBNull("CustomerDesc") ? null : reader.GetString("CustomerDesc")
                });
            }
            
            return new ReturnListType<CustomerDemographic>(true, customerDemographics);
        }
        catch (Exception ex)
        {
            return HandleListError<CustomerDemographic>(ex, "GetAllCustomerDemographics");
        }
    }

    public ReturnType<CustomerDemographic> GetCustomerDemographicById(string customerTypeId)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT CustomerTypeID, CustomerDesc
                FROM CustomerDemographics
                WHERE CustomerTypeID = @customerTypeId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@customerTypeId", customerTypeId);
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                var customerDemographic = new CustomerDemographic
                {
                    CustomerTypeId = reader.GetString("CustomerTypeID"),
                    CustomerDesc = reader.IsDBNull("CustomerDesc") ? null : reader.GetString("CustomerDesc")
                };
                
                return new ReturnType<CustomerDemographic>(true, customerDemographic);
            }
            
            return new ReturnType<CustomerDemographic>(false, null, new List<string> { "CustomerDemographic not found" });
        }
        catch (Exception ex)
        {
            return HandleError<CustomerDemographic>(ex, "GetCustomerDemographicById");
        }
    }

    public ReturnType<CustomerDemographic> UpdateCustomerDemographic(CustomerDemographic customerDemographic)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                UPDATE CustomerDemographics 
                SET CustomerDesc = @customerDesc
                WHERE CustomerTypeID = @customerTypeId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@customerTypeId", customerDemographic.CustomerTypeId);
            command.Parameters.AddWithValue("@customerDesc", customerDemographic.CustomerDesc ?? (object)DBNull.Value);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<CustomerDemographic>(true, customerDemographic);
            }
            
            return new ReturnType<CustomerDemographic>(false, null, new List<string> { "CustomerDemographic not found or no changes made" });
        }
        catch (Exception ex)
        {
            return HandleError<CustomerDemographic>(ex, "UpdateCustomerDemographic");
        }
    }

    public ReturnType<CustomerDemographic> DeleteCustomerDemographic(CustomerDemographic customerDemographic)
    {
        return DeleteCustomerDemographicById(customerDemographic.CustomerTypeId);
    }

    public ReturnType<CustomerDemographic> DeleteCustomerDemographicById(string customerTypeId)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = "DELETE FROM CustomerDemographics WHERE CustomerTypeID = @customerTypeId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@customerTypeId", customerTypeId);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<CustomerDemographic>(true, null);
            }
            
            return new ReturnType<CustomerDemographic>(false, null, new List<string> { "CustomerDemographic not found" });
        }
        catch (Exception ex)
        {
            return HandleError<CustomerDemographic>(ex, "DeleteCustomerDemographicById");
        }
    }

    public ReturnType<CustomerDemographic> AddCustomerDemographic(CustomerDemographic customerDemographic)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                INSERT INTO CustomerDemographics (CustomerTypeID, CustomerDesc)
                VALUES (@customerTypeId, @customerDesc)";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@customerTypeId", customerDemographic.CustomerTypeId);
            command.Parameters.AddWithValue("@customerDesc", customerDemographic.CustomerDesc ?? (object)DBNull.Value);
            
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                return new ReturnType<CustomerDemographic>(true, customerDemographic);
            }
            
            return new ReturnType<CustomerDemographic>(false, null, new List<string> { "Failed to add CustomerDemographic" });
        }
        catch (Exception ex)
        {
            return HandleError<CustomerDemographic>(ex, "AddCustomerDemographic");
        }
    }

    #endregion

    #region View Models - Read Only Methods

    // Note: View models typically only support Get operations as they represent database views or computed results

    #region Invoice Methods

    public ReturnListType<Invoice> GetAllInvoices()
    {
        try
        {
            var invoices = new List<Invoice>();
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT ShipName, ShipAddress, ShipCity, ShipRegion, ShipPostalCode, ShipCountry,
                       CustomerID, CustomerName, Address, City, Region, PostalCode, Country,
                       Salesperson, OrderID, OrderDate, RequiredDate, ShippedDate, ShipperName,
                       ProductID, ProductName, UnitPrice, Quantity, Discount, ExtendedPrice, Freight
                FROM Invoices";
            
            using var command = new SqliteCommand(sql, connection);
            using var reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                invoices.Add(new Invoice
                {
                    ShipName = reader.IsDBNull("ShipName") ? null : reader.GetString("ShipName"),
                    ShipAddress = reader.IsDBNull("ShipAddress") ? null : reader.GetString("ShipAddress"),
                    ShipCity = reader.IsDBNull("ShipCity") ? null : reader.GetString("ShipCity"),
                    ShipRegion = reader.IsDBNull("ShipRegion") ? null : reader.GetString("ShipRegion"),
                    ShipPostalCode = reader.IsDBNull("ShipPostalCode") ? null : reader.GetString("ShipPostalCode"),
                    ShipCountry = reader.IsDBNull("ShipCountry") ? null : reader.GetString("ShipCountry"),
                    CustomerId = reader.IsDBNull("CustomerID") ? null : reader.GetString("CustomerID"),
                    CustomerName = reader.IsDBNull("CustomerName") ? null : reader.GetString("CustomerName"),
                    Address = reader.IsDBNull("Address") ? null : reader.GetString("Address"),
                    City = reader.IsDBNull("City") ? null : reader.GetString("City"),
                    Region = reader.IsDBNull("Region") ? null : reader.GetString("Region"),
                    PostalCode = reader.IsDBNull("PostalCode") ? null : reader.GetString("PostalCode"),
                    Country = reader.IsDBNull("Country") ? null : reader.GetString("Country"),
                    Salesperson = reader.IsDBNull("Salesperson") ? null : reader.GetInt32("Salesperson"),
                    OrderId = reader.IsDBNull("OrderID") ? null : reader.GetInt32("OrderID"),
                    OrderDate = reader.IsDBNull("OrderDate") ? null : reader.GetDateTime("OrderDate"),
                    RequiredDate = reader.IsDBNull("RequiredDate") ? null : reader.GetDateTime("RequiredDate"),
                    ShippedDate = reader.IsDBNull("ShippedDate") ? null : reader.GetDateTime("ShippedDate"),
                    ShipperName = reader.IsDBNull("ShipperName") ? null : reader.GetString("ShipperName"),
                    ProductId = reader.IsDBNull("ProductID") ? null : reader.GetInt32("ProductID"),
                    ProductName = reader.IsDBNull("ProductName") ? null : reader.GetString("ProductName"),
                    UnitPrice = reader.IsDBNull("UnitPrice") ? null : reader.GetDouble("UnitPrice"),
                    Quantity = reader.IsDBNull("Quantity") ? null : reader.GetInt32("Quantity"),
                    Discount = reader.IsDBNull("Discount") ? null : reader.GetDouble("Discount"),
                    ExtendedPrice = reader.IsDBNull("ExtendedPrice") ? null : reader.GetDouble("ExtendedPrice"),
                    Freight = reader.IsDBNull("Freight") ? null : reader.GetDouble("Freight")
                });
            }
            
            return new ReturnListType<Invoice>(true, invoices);
        }
        catch (Exception ex)
        {
            return HandleListError<Invoice>(ex, "GetAllInvoices");
        }
    }

    public ReturnType<Invoice> GetInvoiceById(int orderId, int productId)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT ShipName, ShipAddress, ShipCity, ShipRegion, ShipPostalCode, ShipCountry,
                       CustomerID, CustomerName, Address, City, Region, PostalCode, Country,
                       Salesperson, OrderID, OrderDate, RequiredDate, ShippedDate, ShipperName,
                       ProductID, ProductName, UnitPrice, Quantity, Discount, ExtendedPrice, Freight
                FROM Invoices
                WHERE OrderID = @orderId AND ProductID = @productId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@orderId", orderId);
            command.Parameters.AddWithValue("@productId", productId);
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                var invoice = new Invoice
                {
                    ShipName = reader.IsDBNull("ShipName") ? null : reader.GetString("ShipName"),
                    ShipAddress = reader.IsDBNull("ShipAddress") ? null : reader.GetString("ShipAddress"),
                    ShipCity = reader.IsDBNull("ShipCity") ? null : reader.GetString("ShipCity"),
                    ShipRegion = reader.IsDBNull("ShipRegion") ? null : reader.GetString("ShipRegion"),
                    ShipPostalCode = reader.IsDBNull("ShipPostalCode") ? null : reader.GetString("ShipPostalCode"),
                    ShipCountry = reader.IsDBNull("ShipCountry") ? null : reader.GetString("ShipCountry"),
                    CustomerId = reader.IsDBNull("CustomerID") ? null : reader.GetString("CustomerID"),
                    CustomerName = reader.IsDBNull("CustomerName") ? null : reader.GetString("CustomerName"),
                    Address = reader.IsDBNull("Address") ? null : reader.GetString("Address"),
                    City = reader.IsDBNull("City") ? null : reader.GetString("City"),
                    Region = reader.IsDBNull("Region") ? null : reader.GetString("Region"),
                    PostalCode = reader.IsDBNull("PostalCode") ? null : reader.GetString("PostalCode"),
                    Country = reader.IsDBNull("Country") ? null : reader.GetString("Country"),
                    Salesperson = reader.IsDBNull("Salesperson") ? null : reader.GetInt32("Salesperson"),
                    OrderId = reader.IsDBNull("OrderID") ? null : reader.GetInt32("OrderID"),
                    OrderDate = reader.IsDBNull("OrderDate") ? null : reader.GetDateTime("OrderDate"),
                    RequiredDate = reader.IsDBNull("RequiredDate") ? null : reader.GetDateTime("RequiredDate"),
                    ShippedDate = reader.IsDBNull("ShippedDate") ? null : reader.GetDateTime("ShippedDate"),
                    ShipperName = reader.IsDBNull("ShipperName") ? null : reader.GetString("ShipperName"),
                    ProductId = reader.IsDBNull("ProductID") ? null : reader.GetInt32("ProductID"),
                    ProductName = reader.IsDBNull("ProductName") ? null : reader.GetString("ProductName"),
                    UnitPrice = reader.IsDBNull("UnitPrice") ? null : reader.GetDouble("UnitPrice"),
                    Quantity = reader.IsDBNull("Quantity") ? null : reader.GetInt32("Quantity"),
                    Discount = reader.IsDBNull("Discount") ? null : reader.GetDouble("Discount"),
                    ExtendedPrice = reader.IsDBNull("ExtendedPrice") ? null : reader.GetDouble("ExtendedPrice"),
                    Freight = reader.IsDBNull("Freight") ? null : reader.GetDouble("Freight")
                };
                
                return new ReturnType<Invoice>(true, invoice);
            }
            
            return new ReturnType<Invoice>(false, null, new List<string> { "Invoice not found" });
        }
        catch (Exception ex)
        {
            return HandleError<Invoice>(ex, "GetInvoiceById");
        }
    }

    // Invoice is typically a view - Update/Delete/Add operations are not supported
    public ReturnType<Invoice> UpdateInvoice(Invoice invoice)
    {
        return new ReturnType<Invoice>(false, null, new List<string> { "Update operations not supported for Invoice view" });
    }

    public ReturnType<Invoice> DeleteInvoice(Invoice invoice)
    {
        return new ReturnType<Invoice>(false, null, new List<string> { "Delete operations not supported for Invoice view" });
    }

    public ReturnType<Invoice> DeleteInvoiceById(int orderId, int productId)
    {
        return new ReturnType<Invoice>(false, null, new List<string> { "Delete operations not supported for Invoice view" });
    }

    public ReturnType<Invoice> AddInvoice(Invoice invoice)
    {
        return new ReturnType<Invoice>(false, null, new List<string> { "Add operations not supported for Invoice view" });
    }

    #endregion

    #region AlphabeticalListOfProduct Methods

    public ReturnListType<AlphabeticalListOfProduct> GetAllAlphabeticalListOfProducts()
    {
        try
        {
            var products = new List<AlphabeticalListOfProduct>();
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT ProductID, ProductName, SupplierID, CategoryID, QuantityPerUnit,
                       UnitPrice, UnitsInStock, UnitsOnOrder, ReorderLevel, Discontinued, CategoryName
                FROM [Alphabetical list of products]";
            
            using var command = new SqliteCommand(sql, connection);
            using var reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                products.Add(new AlphabeticalListOfProduct
                {
                    ProductId = reader.IsDBNull("ProductID") ? null : reader.GetInt32("ProductID"),
                    ProductName = reader.IsDBNull("ProductName") ? null : reader.GetString("ProductName"),
                    SupplierId = reader.IsDBNull("SupplierID") ? null : reader.GetInt32("SupplierID"),
                    CategoryId = reader.IsDBNull("CategoryID") ? null : reader.GetInt32("CategoryID"),
                    QuantityPerUnit = reader.IsDBNull("QuantityPerUnit") ? null : reader.GetString("QuantityPerUnit"),
                    UnitPrice = reader.IsDBNull("UnitPrice") ? null : reader.GetDouble("UnitPrice"),
                    UnitsInStock = reader.IsDBNull("UnitsInStock") ? null : reader.GetInt32("UnitsInStock"),
                    UnitsOnOrder = reader.IsDBNull("UnitsOnOrder") ? null : reader.GetInt32("UnitsOnOrder"),
                    ReorderLevel = reader.IsDBNull("ReorderLevel") ? null : reader.GetInt32("ReorderLevel"),
                    Discontinued = reader.IsDBNull("Discontinued") ? null : reader.GetString("Discontinued"),
                    CategoryName = reader.IsDBNull("CategoryName") ? null : reader.GetString("CategoryName")
                });
            }
            
            return new ReturnListType<AlphabeticalListOfProduct>(true, products);
        }
        catch (Exception ex)
        {
            return HandleListError<AlphabeticalListOfProduct>(ex, "GetAllAlphabeticalListOfProducts");
        }
    }

    public ReturnType<AlphabeticalListOfProduct> GetAlphabeticalListOfProductById(int productId)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            
            const string sql = @"
                SELECT ProductID, ProductName, SupplierID, CategoryID, QuantityPerUnit,
                       UnitPrice, UnitsInStock, UnitsOnOrder, ReorderLevel, Discontinued, CategoryName
                FROM [Alphabetical list of products]
                WHERE ProductID = @productId";
            
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@productId", productId);
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                var product = new AlphabeticalListOfProduct
                {
                    ProductId = reader.IsDBNull("ProductID") ? null : reader.GetInt32("ProductID"),
                    ProductName = reader.IsDBNull("ProductName") ? null : reader.GetString("ProductName"),
                    SupplierId = reader.IsDBNull("SupplierID") ? null : reader.GetInt32("SupplierID"),
                    CategoryId = reader.IsDBNull("CategoryID") ? null : reader.GetInt32("CategoryID"),
                    QuantityPerUnit = reader.IsDBNull("QuantityPerUnit") ? null : reader.GetString("QuantityPerUnit"),
                    UnitPrice = reader.IsDBNull("UnitPrice") ? null : reader.GetDouble("UnitPrice"),
                    UnitsInStock = reader.IsDBNull("UnitsInStock") ? null : reader.GetInt32("UnitsInStock"),
                    UnitsOnOrder = reader.IsDBNull("UnitsOnOrder") ? null : reader.GetInt32("UnitsOnOrder"),
                    ReorderLevel = reader.IsDBNull("ReorderLevel") ? null : reader.GetInt32("ReorderLevel"),
                    Discontinued = reader.IsDBNull("Discontinued") ? null : reader.GetString("Discontinued"),
                    CategoryName = reader.IsDBNull("CategoryName") ? null : reader.GetString("CategoryName")
                };
                
                return new ReturnType<AlphabeticalListOfProduct>(true, product);
            }
            
            return new ReturnType<AlphabeticalListOfProduct>(false, null, new List<string> { "Product not found" });
        }
        catch (Exception ex)
        {
            return HandleError<AlphabeticalListOfProduct>(ex, "GetAlphabeticalListOfProductById");
        }
    }

    // View operations - Update/Delete/Add not supported
    public ReturnType<AlphabeticalListOfProduct> UpdateAlphabeticalListOfProduct(AlphabeticalListOfProduct alphabeticalListOfProduct)
    {
        return new ReturnType<AlphabeticalListOfProduct>(false, null, new List<string> { "Update operations not supported for view" });
    }

    public ReturnType<AlphabeticalListOfProduct> DeleteAlphabeticalListOfProduct(AlphabeticalListOfProduct alphabeticalListOfProduct)
    {
        return new ReturnType<AlphabeticalListOfProduct>(false, null, new List<string> { "Delete operations not supported for view" });
    }

    public ReturnType<AlphabeticalListOfProduct> DeleteAlphabeticalListOfProductById(int productId)
    {
        return new ReturnType<AlphabeticalListOfProduct>(false, null, new List<string> { "Delete operations not supported for view" });
    }

    public ReturnType<AlphabeticalListOfProduct> AddAlphabeticalListOfProduct(AlphabeticalListOfProduct alphabeticalListOfProduct)
    {
        return new ReturnType<AlphabeticalListOfProduct>(false, null, new List<string> { "Add operations not supported for view" });
    }

    #endregion

    #endregion
}