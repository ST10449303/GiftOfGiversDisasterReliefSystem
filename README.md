# Gift of the Givers Disaster Relief System

## Overview

The Gift of the Givers Disaster Relief System is an ASP.NET Core MVC web application developed as a group academic project to support the management of disaster relief activities.

The system provides functionality for managing donations, volunteers, relief projects, users and other disaster-relief activities.

## Technologies Used

- C#
- ASP.NET Core MVC
- .NET 8
- Entity Framework Core
- SQL Server / Azure SQL
- ASP.NET Core Identity
- Azure Functions
- Azure App Service
- Azure DevOps
- Azure Repos
- Azure Pipelines
- Azure Artifacts
- NuGet
- Git
- GitHub
- Bootstrap
- Visual Studio 2022

## Key Features

- User authentication and role-based access
- Donor registration and donation processing
- Donation confirmation and tax certificate numbers
- Volunteer management
- Relief project management
- Contact functionality
- Azure Function for tax certificate generation
- Azure SQL database integration
- NuGet helper package integration
- CI/CD pipeline using Azure DevOps

## Azure Function

An Azure HTTP-triggered Function was developed for generating donation tax certificate information.

The function was tested locally using browser requests and Postman before being deployed to Azure.

## DevOps and CI/CD

Azure DevOps was used throughout the development lifecycle.

The project includes:

- Azure Repos for source control
- Feature branches
- Git commits and merges
- Azure Pipelines for automated restore, build and test execution
- Azure Artifacts for hosting a custom NuGet package

## Custom NuGet Package

A reusable helper library named:

`GiftOfTheGivers.Helpers`

was created, packaged as a NuGet package and published to Azure Artifacts.

The package was then installed and used by the main web application.

## My Contributions

My contributions to the project included:

- Developing and integrating the Azure Function
- Implementing donation-related functionality
- Working with Azure SQL
- Creating and integrating the `GiftOfTheGivers.Helpers` NuGet package
- Configuring Azure DevOps repositories
- Working with Git branches and merges
- Creating and maintaining the Azure Pipeline
- Configuring automated build and test execution
- Managing the GitHub repository

## Project Management

This was developed as a group academic project. Different team members contributed to different features of the system.

## Learning Outcomes

This project provided practical experience in:

- Full-stack .NET development
- Database development
- Cloud services
- Azure Functions
- Git and GitHub
- Azure DevOps
- CI/CD
- NuGet package development
- Software collaboration and version control
