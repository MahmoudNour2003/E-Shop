# 🛒 E-Shop: Modern ASP.NET Core E-Commerce Platform

E-Shop is a full-featured, secure, and production-ready e-commerce web application built using the modern **ASP.NET Core MVC** framework, **Entity Framework Core**, and **Microsoft SQL Server**. It follows industry-standard design patterns to separate data access, business logic, and user interface.

---

## ✨ Key Features

### 👤 Customer Experience
* **Dynamic Product Catalog**: Browse products with ease. Includes built-in pagination (12 items per page), full-text search (matching by product name or SKU), and category-based filtering.
* **Smart Catalog Sorting**: Sort products by:
  * Name (A-Z)
  * Price (Low to High / High to Low)
  * Newest arrivals
* **Product Details View**: View detailed product specifications along with automatic recommendations for related products within the same category.
* **Session-Based Shopping Cart**: Seamlessly add items to the cart, update item quantities, remove specific items, or clear the cart. Session states are stored securely on the server.
* **Smooth Checkout & Validation**: Checkout with a pre-saved address or add a new one. The checkout process automatically validates real-time stock levels, decrements inventory atomically, creates unique order numbers, and clears the cart upon completion.
* **Customer Profile Dashboard**: Allows users to manage their personal details, store multiple shipping addresses (with custom default shipping flags), and browse detailed order history with real-time status updates.

### 🛡️ Administrator Panel
* **Product Management (CRUD)**: Create, read, update, and disable products. Supports editing SKU, pricing, stock levels, active status, and binds items to product categories.
* **Category Tree Management (CRUD)**: Manage hierarchical categories. Supports creating child categories mapped to parent categories, ensuring neat organization.
* **Order Management**: Monitor and track all customer orders, inspect line items (quantity, unit price, and total line price), and update order status through processing stages.

---

## 🛠️ Architecture & Design Patterns

The codebase is built around clean architecture principles and patterns:
1. **Model-View-Controller (MVC)**: Segregates frontend views (`.cshtml` Razor templates) from backend routing logic (`Controllers`) and view-specific structures (`ViewModels`).
2. **Repository Pattern**: Abstract data queries via generic `IEntityRepo<T>` and concrete `EntityRepo<T>` implementations, promoting testability.
3. **Unit of Work Pattern**: Bundles multiple repository modifications into single database transactions (`IUnitOfWork` & `UnitOfWork`), preserving database integrity.
4. **ASP.NET Core Identity**: Standard authentication middleware managing registration, logins, hashing passwords, enforcing security guidelines (e.g., minimum 8 characters, digit, lowercase, uppercase requirements), and mapping roles (`Admin` vs. `Customer`).
5. **Relational Database Design**: Programmed using EF Core Fluent API, specifying:
   - Primary and Foreign Key relationships.
   - Precise decimal mapping for currency configurations (`precision: 18, 2`).
   - Unique constraints on critical indexes (Name, SKU, and Order Number).
   - Referential action policies (`Cascade` delete for user addresses, `Restrict` delete for products inside category / orders).

---

## 💻 Tech Stack

* **Backend Framework**: .NET Core (C#) / ASP.NET Core MVC
* **ORM (Object-Relational Mapper)**: Entity Framework Core
* **Database**: Microsoft SQL Server
* **Authentication**: ASP.NET Core Identity
* **Frontend UI**: HTML5, Razor Views (`.cshtml`), Bootstrap, CSS3, jQuery

---

## 📂 Project Structure

```text
├── DB/                      # Class Library (Data Access Layer)
│   ├── Data/
│   │   └── DBContext.cs     # Entity Framework DB Context, Fluent API, Role seeding
│   ├── Migrations/          # EF Core migrations history files
│   ├── Repo/                # Repository & Unit of Work implementation files
│   ├── Address.cs           # EF Address Entity
│   ├── Category.cs          # EF Category Entity
│   ├── Product.cs           # EF Product Entity
│   ├── Order.cs             # EF Order Entity
│   ├── Order_Item.cs        # EF Order Line Item Entity
│   └── APP_USER.cs          # Extended Identity User Entity
│
└── E-Shop/                  # Web Presentation Layer (MVC App)
    ├── Controllers/         # MVC Controllers handling requests (Catalog, Account, Cart, etc.)
    ├── Models/              # ViewModels (VM) representing clean UI-bound data
    ├── Views/               # Razor View HTML pages grouped by controller
    ├── wwwroot/             # Client-side static assets (Bootstrap, custom JS, CSS)
    ├── appsettings.json     # Configuration file (contains connection strings)
    ├── Program.cs           # App entry point, Services registry, Middleware pipeline
    └── E-Shop.csproj        # Web project configuration XML
```

---

## 🚀 Getting Started & Local Setup

### Prerequisites
* Ensure you have the **.NET SDK** installed.
* Ensure **Microsoft SQL Server** (or SQL Server Express / LocalDB) is installed and running on your machine.

### Installation Instructions

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/MahmoudNour2003/E-Shop.git
   cd E-Shop
   ```

2. **Configure Connection Strings**:
   Open [appsettings.json](file:///C:/Users/mahmoud1/Documents/antigravity/keen-hypatia/E-Shop/E-Shop/appsettings.json) and verify the SQL Server connection string under `ConnectionStrings.DefaultConnection`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Data Source=.;Database=E-Shop;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;"
   }
   ```
   * *Adjust `Data Source` server name (e.g. `(localdb)\MSSQLLocalDB` or `.\SQLEXPRESS`) depending on your local instance.*

3. **Apply Database Migrations**:
   Run the following EF Core migration commands inside the root folder to construct the database schema and seed standard Identity Roles (`Admin` and `Customer`):
   ```bash
   dotnet ef database update --project DB --startup-project E-Shop
   ```
   *(Note: Make sure you have the EF Core tools installed: `dotnet tool install --global dotnet-ef`)*

4. **Launch the Application**:
   Startup the MVC web application using:
   ```bash
   dotnet run --project E-Shop
   ```
   Open your browser and navigate to `https://localhost:7082` (or the HTTP address outputted in the console log).
