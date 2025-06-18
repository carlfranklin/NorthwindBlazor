# BlazorNorthwind - Development Instructions

## Project Overview
This is a Blazor Web application that lets the user manage the Northwind database with forms-over-data.

## Technology Stack
- **Framework**: ASP.NET Core 9 Blazor Web App with global server interactivity. 
- **Database**: ADO.NET against a local SQLite database: northwind.db
- Do not downgrade to .NET 8. Continue using .NET 9.

## ADO.NET Policies
- Use the NorthwindConnection connection string in the appsettings.json file for all database connections.
- Use standard ADO.NET connections and commands with using statements.
- Check for null values in both incoming models and database results.
- Use try/catch exception handling around all database operations.

## Blazor EditForm Policies
- Use the EditForm component for all forms.
- Include validation attributes in the model classes for data annotations.
- Use the ValidationSummary component to display validation errors.
- Use the bootstrap grid system with bootstrap control styles. Make it look good!
- Wrap all select elements in div with the "list-group" class, but change the style of the selected div background color to a light gray.
- Always use the Virtualize component for all lists.
- Never show ID fields in the UI. Use the ID as a hidden field in the EditForm only.
- If an error is returned from a data manager, notify the user in an unobtrusive manner. Give the user the option to view the exception message(s).