using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Automated (Controller, Business, Repository) Files Generator ");
        Console.WriteLine("==================");

        Console.Write("Enter entity name: ");
        string entityName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(entityName))
        {
            Console.WriteLine("Entity name cannot be empty!");
            return;
        }

        string solutionPath = Directory.GetCurrentDirectory();
        Console.WriteLine($"Using current directory: {solutionPath}");

        // Ask for dependency configurator file name
        Console.Write("Enter the file name where you add dependencies(ex: Program.cs or DependencyConfigurator.cs): ");
        string dependencyFileName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(dependencyFileName))
        {
            Console.WriteLine("Dependency configurator file name cannot be empty!");
            return;
        }

        // Ask for methods to include
        var methodsToInclude = GetMethodsToInclude();

        try
        {
            GenerateFiles(entityName, solutionPath, methodsToInclude);
            UpdateDependencyConfigurator(entityName, solutionPath, dependencyFileName);
            Console.WriteLine($"\nAll files generated successfully for entity: {entityName}");
            Console.WriteLine("Dependencies updated successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    static Dictionary<string, bool> GetMethodsToInclude()
    {
        Console.WriteLine("\nSelect methods to include (y/n):");

        var methods = new Dictionary<string, bool>
        {
            ["GetAll"] = AskForMethod("GetAll"),
            ["GetById"] = AskForMethod("GetById"),
            ["Create"] = AskForMethod("Create"),
            ["Update"] = AskForMethod("Update"),
            ["Enable"] = AskForMethod("Enable"),
            ["Disable"] = AskForMethod("Disable")
        };

        return methods;
    }

    static bool AskForMethod(string methodName)
    {
        Console.Write($"Do you want to add \"{methodName}\" method? (y/n): ");
        string response = Console.ReadLine()?.ToLower();
        return response == "y" || response == "yes";
    }

    static void GenerateFiles(string entityName, string solutionPath, Dictionary<string, bool> methodsToInclude)
    {
        var paths = new
        {
            Api = Path.Combine(solutionPath, "Audree.DMS.API"),
            Business = Path.Combine(solutionPath, "Audree.DMS.API.Business"),
            Repository = Path.Combine(solutionPath, "Audree.DMS.API.Repository"),
            Model = Path.Combine(solutionPath, "Audree.DMS.API.Model")
        };

        // Create directories
        CreateDirectoryIfNotExists(Path.Combine(paths.Api, "Controllers"));
        CreateDirectoryIfNotExists(Path.Combine(paths.Business, "Contracts"));
        CreateDirectoryIfNotExists(Path.Combine(paths.Business, "Implementations"));
        CreateDirectoryIfNotExists(Path.Combine(paths.Repository, "Contracts"));
        CreateDirectoryIfNotExists(Path.Combine(paths.Repository, "Implementations"));
        CreateDirectoryIfNotExists(paths.Model);

        // Generate files
        GenerateController(entityName, paths.Api, methodsToInclude);
        GenerateBusinessInterface(entityName, paths.Business, methodsToInclude);
        GenerateBusinessImplementation(entityName, paths.Business, methodsToInclude);
        GenerateRepositoryInterface(entityName, paths.Repository, methodsToInclude);
        GenerateRepositoryImplementation(entityName, paths.Repository, methodsToInclude);
        GenerateModel(entityName, paths.Model);
    }

    static void CreateDirectoryIfNotExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            Console.WriteLine($"Created directory: {path}");
        }
    }

    static void GenerateController(string entityName, string apiPath, Dictionary<string, bool> methods)
    {
        string methodsContent = "";

        if (methods["GetAll"])
        {
            methodsContent += $@"
            [HttpGet]
            public async Task<IActionResult> GetAll()
            {{
                Response response = new Response
                {{
                    Success = false,
                    Message = string.Empty,
                    Data = null
                }};

                try
                {{
                    var result = await _{entityName.ToLower()}Business.GetAllAsync();
                    response.Success = true;
                    response.Data = result;
                    return Ok(response);
                }}
                catch (Exception ex)
                {{
                    response.Success = false;
                    response.Message = ex.Message;
                    this.logger.LogError(ex, $""Error in {entityName}Controller - GetAll\n{{ex.ToString()}}"");
                    return Ok(response);
                }}
            }}
";
        }

        if (methods["GetById"])
        {
            methodsContent += $@"
            [HttpGet(""GetById"")]
            public async Task<IActionResult> GetById(int id)
            {{
                Response response = new Response
                {{
                    Success = false,
                    Message = string.Empty,
                    Data = null
                }};

                if (id <= 0)
                {{
                    response.Message = ApplicationMessages.InputValuesNull;
                    return Ok(response);
                }}

                try
                {{
                    var result = await _{entityName.ToLower()}Business.GetByIdAsync(id);
                    response.Success = true;
                    response.Data = result;
                    return Ok(response);
                }}
                catch (Exception ex)
                {{
                    response.Success = false;
                    response.Message = ex.Message;
                    this.logger.LogError(ex, $""Error in {entityName}Controller - GetById\n{{ex.ToString()}}"");
                    return Ok(response);
                }}
            }}
";
        }

        if (methods["Create"])
        {
            methodsContent += $@"
            [HttpPost]
            public async Task<IActionResult> Create([FromBody] {entityName} {entityName.ToLower()})
            {{
                Response response = new Response
                {{
                    Success = false,
                    Message = string.Empty,
                    Data = null
                }};

                if ({entityName.ToLower()} == null)
                {{
                    response.Message = ApplicationMessages.InputValuesNull;
                    return Ok(response);
                }}

                try
                {{
                    var result = await _{entityName.ToLower()}Business.CreateAsync({entityName.ToLower()});
                    response.Success = true;
                    response.Data = result;
                    response.Message = ApplicationMessages.CreatedSuccessfully;
                    return Ok(response);
                }}
                catch (Exception ex)
                {{
                    response.Success = false;
                    response.Message = ex.Message;
                    this.logger.LogError(ex, $""Error in {entityName}Controller - Create\n{{ex.ToString()}}"");
                    return Ok(response);
                }}
            }}
";
        }

        if (methods["Update"])
        {
            methodsContent += $@"
            [HttpPut(""Update"")]
            public async Task<IActionResult> Update([FromBody] {entityName} {entityName.ToLower()})
            {{
                Response response = new Response
                {{
                    Success = false,
                    Message = string.Empty,
                    Data = null
                }};

                if ({entityName.ToLower()} == null)
                {{
                    response.Message = ApplicationMessages.InputValuesNull;
                    return Ok(response);
                }}

                try
                {{
                    var result = await _{entityName.ToLower()}Business.UpdateAsync({entityName.ToLower()});
                    response.Success = true;
                    response.Data = result;
                    response.Message = ApplicationMessages.UpdatedSuccessfully;
                    return Ok(response);
                }}
                catch (Exception ex)
                {{
                    response.Success = false;
                    response.Message = ex.Message;
                    this.logger.LogError(ex, $""Error in {entityName}Controller - Update\n{{ex.ToString()}}"");
                    return Ok(response);
                }}
            }}
";
        }

        if (methods["Enable"])
        {
            methodsContent += $@"
            [HttpPut(""Enable"")]
            public async Task<IActionResult> Enable(int id)
            {{
                Response response = new Response
                {{
                    Success = false,
                    Message = string.Empty,
                    Data = null
                }};

                if (id <= 0)
                {{
                    response.Message = ApplicationMessages.InputValuesNull;
                    return Ok(response);
                }}

                try
                {{
                    var result = await _{entityName.ToLower()}Business.EnableAsync(id);
                    response.Success = true;
                    response.Data = result;
                    response.Message = ApplicationMessages.EnabledSuccessfully;
                    return Ok(response);
                }}
                catch (Exception ex)
                {{
                    response.Success = false;
                    response.Message = ex.Message;
                    this.logger.LogError(ex, $""Error in {entityName}Controller - Enable\n{{ex.ToString()}}"");
                    return Ok(response);
                }}
            }}
";
        }

        if (methods["Disable"])
        {
            methodsContent += $@"
            [HttpPut(""Disable"")]
            public async Task<IActionResult> Disable(int id)
            {{
                Response response = new Response
                {{
                    Success = false,
                    Message = string.Empty,
                    Data = null
                }};

                if (id <= 0)
                {{
                    response.Message = ApplicationMessages.InputValuesNull;
                    return Ok(response);
                }}

                try
                {{
                    var result = await _{entityName.ToLower()}Business.DisableAsync(id);
                    response.Success = true;
                    response.Data = result;
                    response.Message = ApplicationMessages.DisabledSuccessfully;
                    return Ok(response);
                }}
                catch (Exception ex)
                {{
                    response.Success = false;
                    response.Message = ex.Message;
                    this.logger.LogError(ex, $""Error in {entityName}Controller - Disable\n{{ex.ToString()}}"");
                    return Ok(response);
                }}
            }}
";
        }

        string content = $@"using Audree.DMS.API.Business.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using Audree.DMS.Common.Models;
using Audree.DMS.Common;
using Audree.DMS.API.Model;
using System.Collections.Generic;
using System.Linq;

namespace Audree.DMS.API.Controllers
{{
    [ApiController]
    [Route(""api/[controller]"")]
    public class {entityName}Controller : ControllerBase
    {{
        private readonly I{entityName}Business _{entityName.ToLower()}Business;
        private readonly ILogger<{entityName}Controller> logger;

        public {entityName}Controller(I{entityName}Business {entityName.ToLower()}Business, ILogger<{entityName}Controller> logger)
        {{
            _{entityName.ToLower()}Business = {entityName.ToLower()}Business;
            this.logger = logger;
        }}
{methodsContent}
    }}
}}";

        string filePath = Path.Combine(apiPath, "Controllers", $"{entityName}Controller.cs");
        File.WriteAllText(filePath, content);
        Console.WriteLine($"Generated: {filePath}");
    }

    static void GenerateBusinessInterface(string entityName, string businessPath, Dictionary<string, bool> methods)
    {
        string methodsContent = "";

        if (methods["GetAll"])
            methodsContent += $"        Task<IEnumerable<{entityName}>> GetAllAsync();\n";

        if (methods["GetById"])
            methodsContent += $"        Task<{entityName}?> GetByIdAsync(int id);\n";

        if (methods["Create"])
            methodsContent += $"        Task<{entityName}> CreateAsync({entityName} {entityName.ToLower()});\n";

        if (methods["Update"])
            methodsContent += $"        Task<{entityName}?> UpdateAsync({entityName} {entityName.ToLower()});\n";

        if (methods["Enable"])
            methodsContent += $"        Task<bool> EnableAsync(int id);\n";

        if (methods["Disable"])
            methodsContent += $"        Task<bool> DisableAsync(int id);\n";

        string content = $@"using Audree.DMS.API.Model;

namespace Audree.DMS.API.Business.Contracts
{{
    public interface I{entityName}Business
    {{
{methodsContent}    }}
}}";

        string filePath = Path.Combine(businessPath, "Contracts", $"I{entityName}Business.cs");
        File.WriteAllText(filePath, content);
        Console.WriteLine($"Generated: {filePath}");
    }

    static void GenerateBusinessImplementation(string entityName, string businessPath, Dictionary<string, bool> methods)
    {
        string methodsContent = "";

        if (methods["GetAll"])
        {
            methodsContent += $@"
        public async Task<IEnumerable<{entityName}>> GetAllAsync()
        {{
            return await _{entityName.ToLower()}Repository.GetAllAsync();
        }}
";
        }

        if (methods["GetById"])
        {
            methodsContent += $@"
        public async Task<{entityName}?> GetByIdAsync(int id)
        {{
            return await _{entityName.ToLower()}Repository.GetByIdAsync(id);
        }}
";
        }

        if (methods["Create"])
        {
            methodsContent += $@"
        public async Task<{entityName}> CreateAsync({entityName} {entityName.ToLower()})
        {{
            return await _{entityName.ToLower()}Repository.CreateAsync({entityName.ToLower()});
        }}
";
        }

        if (methods["Update"])
        {
            methodsContent += $@"
        public async Task<{entityName}?> UpdateAsync({entityName} {entityName.ToLower()})
        {{
            return await _{entityName.ToLower()}Repository.UpdateAsync({entityName.ToLower()});
        }}
";
        }

        if (methods["Enable"])
        {
            methodsContent += $@"
        public async Task<bool> EnableAsync(int id)
        {{
            return await _{entityName.ToLower()}Repository.EnableAsync(id);
        }}
";
        }

        if (methods["Disable"])
        {
            methodsContent += $@"
        public async Task<bool> DisableAsync(int id)
        {{
            return await _{entityName.ToLower()}Repository.DisableAsync(id);
        }}
";
        }

        string content = $@"using Audree.DMS.API.Business.Contracts;
using Audree.DMS.API.Repository.Contracts;
using Audree.DMS.API.Model;

namespace Audree.DMS.API.Business.Implementations
{{
    public class {entityName}Business : I{entityName}Business
    {{
        private readonly I{entityName}Repository _{entityName.ToLower()}Repository;

        public {entityName}Business(I{entityName}Repository {entityName.ToLower()}Repository)
        {{
            _{entityName.ToLower()}Repository = {entityName.ToLower()}Repository;
        }}
{methodsContent}    }}
}}";

        string filePath = Path.Combine(businessPath, "Implementations", $"{entityName}Business.cs");
        File.WriteAllText(filePath, content);
        Console.WriteLine($"Generated: {filePath}");
    }

    static void GenerateRepositoryInterface(string entityName, string repositoryPath, Dictionary<string, bool> methods)
    {
        string methodsContent = "";

        if (methods["GetAll"])
            methodsContent += $"        Task<IEnumerable<{entityName}>> GetAllAsync();\n";

        if (methods["GetById"])
            methodsContent += $"        Task<{entityName}?> GetByIdAsync(int id);\n";

        if (methods["Create"])
            methodsContent += $"        Task<{entityName}> CreateAsync({entityName} {entityName.ToLower()});\n";

        if (methods["Update"])
            methodsContent += $"        Task<{entityName}?> UpdateAsync({entityName} {entityName.ToLower()});\n";

        if (methods["Enable"])
            methodsContent += $"        Task<bool> EnableAsync(int id);\n";

        if (methods["Disable"])
            methodsContent += $"        Task<bool> DisableAsync(int id);\n";

        string content = $@"using Audree.DMS.API.Model;

namespace Audree.DMS.API.Repository.Contracts
{{
    public interface I{entityName}Repository
    {{
{methodsContent}    }}
}}";

        string filePath = Path.Combine(repositoryPath, "Contracts", $"I{entityName}Repository.cs");
        File.WriteAllText(filePath, content);
        Console.WriteLine($"Generated: {filePath}");
    }

    static void GenerateRepositoryImplementation(string entityName, string repositoryPath, Dictionary<string, bool> methods)
    {
        string methodsContent = "";

        if (methods["GetAll"])
        {
            methodsContent += $@"
        public async Task<IEnumerable<{entityName}>> GetAllAsync()
        {{
            // Implementation
            throw new NotImplementedException();
        }}
";
        }

        if (methods["GetById"])
        {
            methodsContent += $@"
        public async Task<{entityName}?> GetByIdAsync(int id)
        {{
            // Implementation
            throw new NotImplementedException();
        }}
";
        }

        if (methods["Create"])
        {
            methodsContent += $@"
        public async Task<{entityName}> CreateAsync({entityName} {entityName.ToLower()})
        {{
            // Implementation
            throw new NotImplementedException();
        }}
";
        }

        if (methods["Update"])
        {
            methodsContent += $@"
        public async Task<{entityName}?> UpdateAsync({entityName} {entityName.ToLower()})
        {{
            // Implementation
            throw new NotImplementedException();
        }}
";
        }

        if (methods["Enable"])
        {
            methodsContent += $@"
        public async Task<bool> EnableAsync(int id)
        {{
            // Implementation
            throw new NotImplementedException();
        }}
";
        }

        if (methods["Disable"])
        {
            methodsContent += $@"
        public async Task<bool> DisableAsync(int id)
        {{
            // Implementation
            throw new NotImplementedException();
        }}
";
        }

        string content = $@"using Audree.DMS.API.Repository.Contracts;
using Audree.DMS.API.Model;
using Dapper;


namespace Audree.DMS.API.Repository.Implementations
{{
    public class {entityName}Repository : I{entityName}Repository
    {{
        private readonly IDMSDbConnection dbConnectionRepository;
        
        public {entityName}Repository(IDMSDbConnection dbConnectionRepository)
        {{
            this.dbConnectionRepository = dbConnectionRepository;
        }}
{methodsContent}    }}
}}";

        string filePath = Path.Combine(repositoryPath, "Implementations", $"{entityName}Repository.cs");
        File.WriteAllText(filePath, content);
        Console.WriteLine($"Generated: {filePath}");
    }

    static void GenerateModel(string entityName, string modelPath)
    {
        string content = $@"namespace Audree.DMS.API.Model
{{
    public class {entityName}
    {{
        public int Id {{ get; set; }}
        public string? Name {{ get; set; }}
        public bool Enable {{ get; set; }}
        public bool Active {{ get; set; }}
        public int PlantId {{ get; set; }}
        public string? Comments {{ get; set; }}
        public int CreatedById {{ get; set; }}
        public DateTime CreatedDate {{ get; set; }}
        public int ModifiedById {{ get; set; }}
        public DateTime ModifiedDate {{ get; set; }}
    }}
}}";

        string filePath = Path.Combine(modelPath, $"{entityName}.cs");
        File.WriteAllText(filePath, content);
        Console.WriteLine($"Generated: {filePath}");
    }

    static void UpdateDependencyConfigurator(string entityName, string solutionPath, string dependencyFileName)
    {
        Console.WriteLine($"\nSearching for {dependencyFileName} in all C# microservice API applications...");

        // Find the dependency configurator file
        var foundFiles = FindDependencyConfiguratorFiles(solutionPath, dependencyFileName);

        if (!foundFiles.Any())
        {
            Console.WriteLine($"Warning: {dependencyFileName} not found in any C# microservice API applications under {solutionPath}");
            return;
        }

        // Process each found file
        foreach (var filePath in foundFiles)
        {
            Console.WriteLine($"Found: {filePath}");
            ProcessDependencyFile(entityName, filePath);
        }
    }

    static List<string> FindDependencyConfiguratorFiles(string rootPath, string fileName)
    {
        var foundFiles = new List<string>();

        try
        {
            // Search recursively for the specified file in all subdirectories
            var files = Directory.GetFiles(rootPath, fileName, SearchOption.AllDirectories);

            foreach (var file in files)
            {
                // Check if the file is in a C# project directory (contains .csproj files or typical API structure)
                var directory = Path.GetDirectoryName(file);
                if (IsApiProject(directory))
                {
                    foundFiles.Add(file);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching for files: {ex.Message}");
        }

        return foundFiles;
    }

    static bool IsApiProject(string directory)
    {
        if (string.IsNullOrEmpty(directory))
            return false;

        // Check current directory and parent directories for signs of a C# API project
        var currentDir = new DirectoryInfo(directory);

        while (currentDir != null)
        {
            // Look for .csproj files
            var csprojFiles = currentDir.GetFiles("*.csproj");
            if (csprojFiles.Any())
            {
                // Check if it's likely an API project by looking for common patterns
                var projectContent = string.Empty;
                try
                {
                    projectContent = File.ReadAllText(csprojFiles.First().FullName);
                    if (projectContent.Contains("Microsoft.AspNetCore") ||
                        projectContent.Contains("Web") ||
                        projectContent.Contains("API") ||
                        currentDir.Name.Contains("API", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                catch
                {
                    // If we can't read the project file, check by directory structure
                    if (currentDir.Name.Contains("API", StringComparison.OrdinalIgnoreCase) ||
                        currentDir.GetDirectories("Controllers").Any() ||
                        currentDir.GetDirectories("Business").Any() ||
                        currentDir.GetDirectories("Repository").Any())
                    {
                        return true;
                    }
                }
            }

            currentDir = currentDir.Parent;
        }

        return false;
    }

    static void ProcessDependencyFile(string entityName, string filePath)
    {
        try
        {
            string fileContent = File.ReadAllText(filePath);

            // Check if dependencies already exist
            if (fileContent.Contains($"I{entityName}Repository") || fileContent.Contains($"I{entityName}Business"))
            {
                Console.WriteLine($"Dependencies for {entityName} already exist in {filePath}");
                return;
            }

            // Repository dependency
            string repositoryDependency = $"            services.AddTransient<I{entityName}Repository, {entityName}Repository>();";

            // Business dependency  
            string businessDependency = $"            services.AddTransient<I{entityName}Business, {entityName}Business>();";

            // Try different approaches based on file type
            bool updated = false;

            // Approach 1: Look for existing InjectRepositoryDependencies and InjectBusinessDependencies methods
            if (fileContent.Contains("InjectRepositoryDependencies") && fileContent.Contains("InjectBusinessDependencies"))
            {
                fileContent = AddDependencyToMethod(fileContent, "InjectRepositoryDependencies", repositoryDependency);
                fileContent = AddDependencyToMethod(fileContent, "InjectBusinessDependencies", businessDependency);
                updated = true;
            }
            // Approach 2: Look for ConfigureServices method (Program.cs style)
            else if (fileContent.Contains("ConfigureServices") || fileContent.Contains("builder.Services"))
            {
                fileContent = AddDependencyToConfigureServices(fileContent, entityName);
                updated = true;
            }
            // Approach 3: Look for any services.AddTransient patterns and add near them
            else if (fileContent.Contains("services.AddTransient"))
            {
                fileContent = AddDependencyNearExistingServices(fileContent, repositoryDependency, businessDependency);
                updated = true;
            }

            if (updated)
            {
                File.WriteAllText(filePath, fileContent);
                Console.WriteLine($"Updated dependencies in: {filePath}");
            }
            else
            {
                Console.WriteLine($"Warning: Could not determine how to add dependencies to {filePath}");
                Console.WriteLine("Please manually add the following dependencies:");
                Console.WriteLine(repositoryDependency);
                Console.WriteLine(businessDependency);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing {filePath}: {ex.Message}");
        }
    }

    static string AddDependencyToMethod(string fileContent, string methodName, string dependency)
    {
        // Pattern to find the method and its closing brace
        string pattern = $@"(public static void {methodName}\(this IServiceCollection services\)\s*\{{[^}}]*)(}})";

        var match = Regex.Match(fileContent, pattern, RegexOptions.Singleline);

        if (match.Success)
        {
            string methodBody = match.Groups[1].Value;
            string closingBrace = match.Groups[2].Value;

            // Add the new dependency before the closing brace
            string updatedMethod = methodBody + dependency + Environment.NewLine + "        " + closingBrace;

            fileContent = fileContent.Replace(match.Value, updatedMethod);
        }
        else
        {
            Console.WriteLine($"Warning: Could not find method {methodName}");
        }

        return fileContent;
    }

    static string AddDependencyToConfigureServices(string fileContent, string entityName)
    {
        // Repository dependency
        string repositoryDependency = $"builder.Services.AddTransient<I{entityName}Repository, {entityName}Repository>();";

        // Business dependency  
        string businessDependency = $"builder.Services.AddTransient<I{entityName}Business, {entityName}Business>();";

        // Look for existing AddTransient calls and add after them
        var lines = fileContent.Split('\n').ToList();

        int insertIndex = -1;
        for (int i = lines.Count - 1; i >= 0; i--)
        {
            if (lines[i].Contains("builder.Services.AddTransient") || lines[i].Contains("services.AddTransient"))
            {
                insertIndex = i + 1;
                break;
            }
        }

        if (insertIndex > 0)
        {
            lines.Insert(insertIndex, repositoryDependency);
            lines.Insert(insertIndex + 1, businessDependency);
            return string.Join('\n', lines);
        }

        return fileContent;
    }

    static string AddDependencyNearExistingServices(string fileContent, string repositoryDependency, string businessDependency)
    {
        var lines = fileContent.Split('\n').ToList();

        int insertIndex = -1;
        for (int i = lines.Count - 1; i >= 0; i--)
        {
            if (lines[i].Contains("services.AddTransient"))
            {
                insertIndex = i + 1;
                break;
            }
        }

        if (insertIndex > 0)
        {
            lines.Insert(insertIndex, repositoryDependency);
            lines.Insert(insertIndex + 1, businessDependency);
            return string.Join('\n', lines);
        }

        return fileContent;
    }
}