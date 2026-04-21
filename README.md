# 🚀 TechStore - Modern E-Commerce Platform

TechStore is a premium, high-performance e-commerce solution built with **ASP.NET Core 9.0**. It features a robust **Hybrid Architecture** (MVC + API), state-of-the-art security with **OTP & Social Auth**, and a globalized time system using **UTC standardization**.

---

## ✨ Key Features & Modules

### 🛠️ Administrative Dashboard (Admin)
- **Category Management**: Full CRUD operations for organizing products with descriptive metadata.
- **Product Management**: Advanced catalog management including image uploads, pricing, and category mapping.
- **Order Processing**: End-to-end workflow management (Pending -> Approved -> Processing -> Shipped -> Cancelled/Refunded).
- **User Management**: Administrative control over user roles, account locking, and profile monitoring.
- **Real-time Metrics**: KPI tracking for total sales, order volume, and customer growth.

### 🛒 Customer Experience (Storefront)
- **Intuitive Shopping**: Responsive product browsing with detailed views and category filtering.
- **Persistent Cart**: Secure shopping cart functionality with live quantity adjustments.
- **Seamless Checkout**: Streamlined multi-step checkout process with physical address validation.
- **Order Tracking**: Personal order history with real-time status updates and shipping tracking numbers.

---

## 🏗️ Technical Architecture

### Hybrid Architecture
The project is split into a **Monolithic MVC** core for SEO-friendly server-side rendering and a **Decoupled Web API** for modern client consumption.
- **TechStore (MVC)**: Handles the main web interface and admin dashboard.
- **TechStore.Api**: Provides JSON endpoints for potential Mobile or SPA integrations.

### API Endpoint Summary
| Controller | Endpoint | Method | Description |
|------------|----------|--------|-------------|
| Categories | `/api/categories` | GET | List all active categories |
| Categories | `/api/categories/{id}` | GET | Get category details |
| Products | `/api/products` | GET | List all products with category info |
| Products | `/api/products/{id}` | GET | Get specific product details |

---

## 🔐 Security & Identity Implementation

### OTP Verification (MFA)
- Implemented a custom **OTP System** that sends a 6-digit verification code to the user's email upon registration.
- Prevents account activation until the code is verified, ensuring 100% valid user emails.
- **OTP Expiry**: Codes are valid for a 10-minute window, synchronized via UTC.

### Social Authentication
- Fully integrated with **Google** and **Facebook** OAuth 2.0.
- Automatic account provisioning for social users with verified email status.

---

## 💳 Payment Gateway Integration

- **Stripe Integration**: Uses the official Stripe.net library for secure, PCI-compliant payment processing.
- **Session Management**: Implements Stripe Checkout Sessions for a hosted, secure payment experience.
- **Post-Payment Logic**: Automatic order status transition and payment intent tracking upon successful transaction.

---

## 🛠️ Technology Stack

- **Backend**: ASP.NET Core 9.0, Entity Framework Core 9.0
- **Database**: MS SQL Server (Relational storage with UTC DateTimeOffset)
- **Security**: ASP.NET Core Identity, JWT (Ready), OTP Verification
- **Frontend MVC**: Razor Pages, Bootstrap 5, jQuery, SweetAlert2, DataTables.net
- **UI Libraries**: FontAwesome 6, Google Fonts (Outfit & Cairo), CSS3 Glassmorphism

---

## 🚀 Installation & Setup

1. **Clone & Restore**:
   ```bash
   git clone https://github.com/mohamed68909/TechStore.git
   dotnet restore
   ```

2. **Database Setup**:
   Update `ConnectionStrings` in `appsettings.json`, then run:
   ```bash
   dotnet ef database update --project TechStore.DataAccess --startup-project TechStore
   ```

3. **External Services**:
   Configure `Stripe` and `Authentication` keys in the secrets manager or `appsettings.json`.

---

## 📁 Project Structure

- `TechStore`: Main Web UI & MVC Logic.
- `TechStore.Api`: RESTful API Layer.
- `TechStore.Services`: Core Business logic (Order, Cart, Product services).
- `TechStore.DataAccess`: Repository implementations and DB Configuration.
- `TechStore.Entities`: Domain Entities and DTOs.
- `TechStore.Utilatis`: Static constants, SD, and helper classes.

---

## 📄 License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

Developed with ❤️ by **Mohamed Ashraf**
