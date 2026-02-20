# 🏗️ GeoBuild - Project Monitoring System

A comprehensive project monitoring and disbursement tracking system that digitizes the manual process of site visit reporting, geo-tagging photos, and facilitating communication between Relationship Managers (RMs) and Quantity Surveyors (QSs).


## Table of Contents
- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [API Documentation](#api-documentation)
- [Database Schema](#database-schema)
- [Frontend Development](#frontend-development)
- [Project Structure](#project-structure)
- [Contributing](#contributing)
- [License](#license)

## Overview

GeoBuild replaces the manual, paper-based process of construction loan monitoring with a digital workflow that ensures:

- **Authenticity**: Geo-tagged photos verify site visits
- **Efficiency**: Real-time notifications and task assignment
- **Transparency**: Complete audit trail of all actions
- **Standardization**: Consistent report formats
- **Integration**: Connects with core banking system (IBPS)

##  Features

### For Relationship Managers (RMs)
- **IBPS Lookup**: Auto-populate facility details
- **Geo-tagged Photos**: GPS-verified site photos
- **Digital Call Report**: Match the existing Word template
- **Document Upload**: Attach certificates and valuations
- **My Reports**: Track all submitted reports

### For Quantity Surveyors (QSs)
- **Pending Pool**: View all reports needing review
- **Task Locking**: Prevent double-review conflicts
- **Comment System**: Threaded discussions
- **Approval Workflow**: Approve with custom amount
- **Return to RM**: Send back with feedback

### System Features
- **JWT Authentication**: Secure role-based access
- **Notifications**: In-app and email alerts
- **PDF Generation**: Branded approval reports
- **Mobile-Friendly**: Works on phones and tablets
- **Audit Trail**: Complete action history

## Tech Stack

### Backend
- **Framework**: ASP.NET Core 8.0
- **Database**: MySQL 8.0 with Entity Framework Core
- **Authentication**: JWT Bearer + BCrypt
- **Documentation**: Swagger/OpenAPI
- **Email**: SMTP integration ready

### Frontend
- **Framework**: React 18 with TypeScript
- **Build Tool**: Vite
- **Styling**: Tailwind CSS
- **State Management**: React Context + Hooks
- **HTTP Client**: Axios
- **Forms**: React Hook Form

## Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [MySQL 8.0](https://www.mysql.com/downloads/)
- [Node.js 18+](https://nodejs.org/)
- [Git](https://git-scm.com/)

### Backend Setup

Clone the repository
git clone https://github.com/yourusername/geobuild.git
cd geobuild/geoback

# Restore packages
dotnet restore

### Update database connection in appsettings.json
# "DefaultConnection": "Server=localhost;Port=3306;Database=geobuild;User=root;Password=yourpassword;"

### Create and apply migrations
dotnet ef migrations add InitialCreate
dotnet ef database update

### Run the backend
dotnet run