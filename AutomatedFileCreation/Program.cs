using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

        Console.Write("Enter solution path (or press Enter for current directory): ");
        string solutionPath = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(solutionPath))
            solutionPath = Directory.GetCurrentDirectory();

        // Ask for methods to include
        var methodsToInclude = GetMethodsToInclude();

        try
        {
            GenerateFiles(entityName, solutionPath, methodsToInclude);
            UpdateDependencyConfigurator(entityName, solutionPath);
            Console.WriteLine($"\nAll files generated successfully for entity: {entityName}");
            Console.WriteLine("DependencyConfigurator updated with new dependencies!");
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

    // NEW METHOD: Update DependencyConfigurator
    static void UpdateDependencyConfigurator(string entityName, string solutionPath)
    {
        string dependencyConfiguratorPath = Path.Combine(solutionPath, "Audree.DMS.API", "DependencyConfigurations", "DependencyConfigurator.cs");

        if (!File.Exists(dependencyConfiguratorPath))
        {
            Console.WriteLine($"Warning: DependencyConfigurator.cs not found at {dependencyConfiguratorPath}");
            return;
        }

        string fileContent = File.ReadAllText(dependencyConfiguratorPath);

        // Check if dependencies already exist
        if (fileContent.Contains($"I{entityName}Repository") || fileContent.Contains($"I{entityName}Business"))
        {
            Console.WriteLine($"Dependencies for {entityName} already exist in DependencyConfigurator.cs");
            return;
        }

        // Add repository dependency
        string repositoryDependency = $"         services.AddTransient<I{entityName}Repository, {entityName}Repository>();";
        fileContent = AddDependencyToMethod(fileContent, "InjectRepositoryDependencies", repositoryDependency);

        // Add business dependency
        string businessDependency = $"         services.AddTransient<I{entityName}Business, {entityName}Business>();";
        fileContent = AddDependencyToMethod(fileContent, "InjectBusinessDependencies", businessDependency);

        // Write back to file
        File.WriteAllText(dependencyConfiguratorPath, fileContent);
        Console.WriteLine($"Updated: {dependencyConfiguratorPath}");
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
            string updatedMethod = methodBody + dependency + Environment.NewLine + "     " + closingBrace;

            fileContent = fileContent.Replace(match.Value, updatedMethod);
        }
        else
        {
            Console.WriteLine($"Warning: Could not find method {methodName} in DependencyConfigurator.cs");
        }

        return fileContent;
    }
}