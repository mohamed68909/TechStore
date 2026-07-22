# 🛒 TechStore - Modern E-Commerce Platform

[![Framework](https://img.shields.io/badge/ASP.NET%20Core-9.0-512BD4?style=for-the-badge&logo=.net)](https://dotnet.microsoft.com/)
[![Database](https://img.shields.io/badge/SQL%20Server-2022-CC292B?style=for-the-badge&logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=for-the-badge&logo=bootstrap)](https://getbootstrap.com/)
[![Payments](https://img.shields.io/badge/Stripe-Integration-008CDD?style=for-the-badge&logo=stripe)](https://stripe.com)
[![License](https://img.shields.io/badge/License-MIT-green.style=for-the-badge)](LICENSE)

**TechStore** is a feature-rich, high-performance e-commerce web application built with **ASP.NET Core 9.0 MVC**. Designed with enterprise-grade architecture, seamless payment processing, robust MFA security, and a sleek modern user interface.

---

## 🌟 Key Features

### 🛍️ Customer Storefront
- **Responsive Catalog Browsing**: Dynamic product catalog with category filtering, instant search, and detailed product views.
- **Persistent Shopping Cart**: Interactive shopping cart with real-time item quantity management.
- **Secure Multi-step Checkout**: Address validation and seamless order summary before payment.
- **Stripe Payment Gateway**: Integrated credit/debit card payment processing via Stripe Checkout.
- **Order Tracking & History**: Real-time order status tracking (Pending → Approved → Processing → Shipped → Completed) with detailed invoice history.

### 🛡️ Administrative Dashboard
- **Product & Category Management**: Full CRUD operations with multi-image support, stock tracking, and metadata tagging.
- **Order Fulfillment Center**: Complete lifecycle control over customer orders with automated status updates.
- **User & Role Management**: Administrative control over customer accounts, role assignments, and lockouts.
- **Real-time Analytics**: KPI tracking for total revenue, order count, and active customer growth metrics.

### 🔐 Security & Identity
- **ASP.NET Core Identity**: Secure password hashing, cookie authentication, and authorization policies.
- **OTP Verification (MFA)**: Custom 6-digit email OTP system enforcing email verification upon registration.
- **Social Authentication**: One-click OAuth 2.0 login integration with **Google** and **Facebook**.
- **UTC Time Standardization**: Globalized time management using `DateTimeOffset.UtcNow` across all operations.

---

## 🛠️ Technology Stack

| Layer | Technology |
| :--- | :--- |
| **Framework** | ASP.NET Core 9.0 (MVC) |
| **ORM / Data Access** | Entity Framework Core 9.0 |
| **Database** | Microsoft SQL Server |
| **Identity & Security** | ASP.NET Core Identity, Custom OTP Service, OAuth 2.0 |
| **Payment Integration** | Stripe.net SDK |
| **Frontend Technologies** | Razor Views, Bootstrap 5, JavaScript (ES6+), jQuery |
| **UI Libraries** | SweetAlert2, DataTables.net, FontAwesome 6, Google Fonts |

---

## 🏗️ Project Architecture

```
TechStore/
├── TechStore/               # Main MVC Web Application (Views, Controllers, ViewModels)
├── TechStore.Services/      # Business Logic Layer (Order, Cart, Product, Email, Stripe)
├── TechStore.DataAccess/    # Data Access Layer (EF Core DbContext, Repositories, Migrations)
├── TechStore.Entities/      # Domain Entities & DTO Models
└── TechStore.Utilities/     # Shared Utilities, Constants (SD), Helpers
```

---

## 🚀 Getting Started

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB or Express)
- [Stripe Account](https://stripe.com) (for test API keys)

### Installation & Setup

1. **Clone the Repository**
   ```bash
   git clone https://github.com/mohamed68909/TechStore.git
   cd TechStore
   ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure Settings**
   Update `appsettings.json` in the `TechStore` project with your Database Connection String, Stripe Keys, and Email settings:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=TechStoreDb;Trusted_Connection=True;TrustServerCertificate=True;"
     },
     "Stripe": {
       "SecretKey": "sk_test_...",
       "PublishableKey": "pk_test_..."
     }
   }
   ```

4. **Apply Database Migrations**
   ```bash
   dotnet ef database update --project TechStore.DataAccess --startup-project TechStore
   ```

5. **Run the Application**
   ```bash
   dotnet run --project TechStore
   ```

---

## 📄 License
This project is open-source and available under the [MIT License](LICENSE).

Developed with ❤️ by **Mohamed Ashraf**
