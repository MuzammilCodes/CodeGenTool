# CodeGenTool

> ⚠️ **Prerequisite Project Structure**

Before using this tool, ensure your microservice project follows the naming convention:

The project names **must end with** one of the following layers:
- `API`
- `Business`
- `Repository`
- `Model`

For example:
- `Contoso.UserManagement.API`
- `Contoso.UserManagement.Business`
- `Contoso.UserManagement.Repository`
- `Contoso.UserManagement.Model`

The tool depends on this naming convention to properly locate and inject the generated files into the correct projects.

---

## 📦 What is CodeGenTool?

**CodeGenTool** is a .NET command-line utility that automates the creation of boilerplate code for a microservices-based API architecture.

By entering an entity name, the tool generates a fully structured set of C# files, including:

- API Controllers
- Business layer interfaces and implementations
- Repository layer interfaces and implementations
- Corresponding Model class

It also updates the dependency injection configuration automatically (e.g., in `Program.cs` or `DependencyConfigurator.cs`).

---

## ⚙️ Customization Options

Users can selectively include common methods such as:

- `GetAll`
- `Create`
- `Update`
- `Enable`
- ...and more

This makes the scaffolding process **flexible**, **customizable**, and **fast**, aligning with best practices in clean architecture for backend development.
