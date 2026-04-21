# 🚀 TechStore - Modern E-Commerce Platform

TechStore is a premium, high-performance e-commerce solution built with **ASP.NET Core 9.0**. It features a robust **Hybrid Architecture** (MVC + API), state-of-the-art security with **OTP & Social Auth**, and a globalized time system using **UTC standardization**.

---

## ✨ Key Features

### 🖥️ Premium User Experience
- **Split-Screen Auth UI**: Bespoke Login and Register pages with glassmorphism effects and modern CSS animations.
- **Responsive Dashboard**: Advanced administrative interface for managing orders, users, and products.
- **Dynamic Shopping Experience**: Real-time cart updates and seamless Stripe payment integration.

### 🏗️ Advanced Architecture
- **Hybrid System**: Separation of concerns with a dedicated **Web API** layer (`TechStore.Api`) alongside the core **MVC** application.
- **Decoupled Logic**: Clean implementation using **Repository Pattern** and **Service Layer** for maximum reusability.
- **Cross-Platform Ready**: The API layer is ready to serve Mobile (Flutter/React Native) or Single Page Applications (Next.js/React).

### 🔐 Security & Identity
- **Multi-Factor OTP**: Secure account verification via 6-digit OTP codes during registration.
- **Social Integration**: Ready-to-use **Google** and **Facebook** authentication.
- **Role-Based Access**: Granular control for Admin and Customer tiers.

### 🌍 Global Standards
- **UTC Time Sync**: standardized time storage using `DateTimeOffset` (UTC) to ensure accuracy across global servers and clients.
- **Multi-Vendor Potential**: Architected to support multi-store and multi-domain environments.

---

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 9.0 (MVC + Web API)
- **Database**: Entity Framework Core + SQL Server
- **Authentication**: ASP.NET Core Identity + External Providers
- **Payments**: Stripe.net Integration
- **UI/UX**: HTML5, CSS3 (Modern Flex/Grid), Bootstrap 5, JavaScript (jQuery)
- **Design**: Google Fonts (Outfit & Cairo), FontAwesome 6

---

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK
- SQL Server (LocalDB or Express)
- Stripe Account (for payment testing)

### Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/mohamed68909/TechStore.git
   ```

2. **Update Configuration**:
   Add your keys in `appsettings.json`:
   ```json
   "stripe": {
     "Secretkey": "sk_test_...",
     "Publishablekey": "pk_test_..."
   },
   "Authentication": {
     "Google": { "ClientId": "...", "ClientSecret": "..." }
   }
   ```

3. **Apply Migrations**:
   ```bash
   dotnet ef database update --project TechStore.DataAccess --startup-project TechStore
   ```

4. **Run the Application**:
   ```bash
   dotnet run --project TechStore
   ```

---

## 📁 Project Structure

- `TechStore`: Core MVC Web Application (UI & Controllers)
- `TechStore.Api`: Dedicated Web API for external clients
- `TechStore.Services`: Business logic and implementation
- `TechStore.Entities`: Domain models and ViewModels
- `TechStore.DataAccess`: EF Core DB Context and Repository implementations
- `TechStore.Utilatis`: Utility classes and SD (Static Details)

---

## 📄 License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

Developed with ❤️ by **Mohamed Ashraf**
