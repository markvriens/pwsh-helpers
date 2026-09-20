using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace Build_JsonValidator.Helpers;
public static class JsonValidator
{
	private static readonly HttpClient HttpClient = new();

	public static async Task<ValidationResult> ValidateFromArgsAsync(string[] args)
	{
		try
		{
			var parseResult = ParseArguments(args);
			if (!parseResult.Success)
			{
				return new ValidationResult(false, parseResult.Errors);
			}

			var jsonPath = parseResult.JsonPath!;
			var schemaSource = parseResult.SchemaPath!;

			if (!File.Exists(jsonPath))
			{
				return new ValidationResult(false, new List<string> { $"JSON file not found: {jsonPath}" });
			}

			var jsonContent = await File.ReadAllTextAsync(jsonPath);

			var schemaLoadResult = await LoadSchemaContentAsync(schemaSource);
			if (!schemaLoadResult.Success)
			{
				return new ValidationResult(false, schemaLoadResult.Errors);
			}

			JToken jsonToken;
			try
			{
				jsonToken = JToken.Parse(jsonContent);
			}
			catch (Exception ex)
			{
				return new ValidationResult(false, new List<string> { $"Invalid JSON document: {ex.Message}" });
			}

			JSchema schema;
			try
			{
				schema = LoadSchema(schemaLoadResult.SchemaUri!, schemaLoadResult.SchemaContent!);
			}
			catch (Exception ex)
			{
				return new ValidationResult(false, new List<string> { $"Invalid JSON schema: {ex.Message}" });
			}

			IList<string> validationErrors = new List<string>();
			var isValid = jsonToken.IsValid(schema, out validationErrors);

			return new ValidationResult(isValid, validationErrors.ToList());
		}
		catch (Exception ex)
		{
			return new ValidationResult(false, new List<string> { $"Unexpected error: {ex.Message}" });
		}
	}

	private static async Task<SchemaLoadResult> LoadSchemaContentAsync(string schemaSource)
	{
		if (TryGetSchemaUri(schemaSource, out var schemaUri))
		{
			if (schemaUri!.IsFile)
			{
				if (!File.Exists(schemaUri.LocalPath))
				{
					return new SchemaLoadResult(false, null, null, new List<string> { $"Schema file not found: {schemaUri.LocalPath}" });
				}

				var fileContent = await File.ReadAllTextAsync(schemaUri.LocalPath);
				return new SchemaLoadResult(true, schemaUri, fileContent, new List<string>());
			}

			if (schemaUri.Scheme == Uri.UriSchemeHttp || schemaUri.Scheme == Uri.UriSchemeHttps)
			{
				try
				{
					var content = await HttpClient.GetStringAsync(schemaUri);
					return new SchemaLoadResult(true, schemaUri, content, new List<string>());
				}
				catch (Exception ex)
				{
					return new SchemaLoadResult(false, null, null, new List<string> { $"Unable to load schema URI: {schemaUri}. {ex.Message}" });
				}
			}
		}

		if (!File.Exists(schemaSource))
		{
			return new SchemaLoadResult(false, null, null, new List<string> { $"Schema file not found: {schemaSource}" });
		}

		var fullPath = Path.GetFullPath(schemaSource);
		var schemaFileUri = new Uri(fullPath);
		var schemaContent = await File.ReadAllTextAsync(fullPath);

		return new SchemaLoadResult(true, schemaFileUri, schemaContent, new List<string>());
	}

	private static bool TryGetSchemaUri(string schemaSource, out Uri? schemaUri)
	{
		schemaUri = null;

		if (!Uri.TryCreate(schemaSource, UriKind.Absolute, out var candidate))
		{
			return false;
		}

		if (candidate.Scheme != Uri.UriSchemeHttp &&
			candidate.Scheme != Uri.UriSchemeHttps &&
			candidate.Scheme != Uri.UriSchemeFile)
		{
			return false;
		}

		schemaUri = candidate;
		return true;
	}

	private static JSchema LoadSchema(Uri schemaUri, string schemaContent)
	{
		var urlResolver = new JSchemaUrlResolver();
		var resolver = new JSchemaPreloadedResolver(urlResolver);

		resolver.Add(schemaUri, schemaContent);

		var settings = new JSchemaReaderSettings
		{
			BaseUri = schemaUri,
			Resolver = resolver,
			ResolveSchemaReferences = true
		};

		return JSchema.Parse(schemaContent, settings);
	}

	public static ArgumentParseResult ParseArguments(string[] args)
	{
		if (args.Length == 2)
		{
			return new ArgumentParseResult(true, args[0], args[1], new List<string>());
		}

		string? jsonPath = null;
		string? schemaPath = null;
		var errors = new List<string>();

		for (var i = 0; i < args.Length; i++)
		{
			switch (args[i])
			{
				case "--json":
				case "-j":
					if (i + 1 >= args.Length)
					{
						errors.Add("Missing value for --json.");
					}
					else
					{
						jsonPath = args[++i];
					}
					break;

				case "--schema":
				case "-s":
					if (i + 1 >= args.Length)
					{
						errors.Add("Missing value for --schema.");
					}
					else
					{
						schemaPath = args[++i];
					}
					break;

				case "--help":
				case "-h":
					errors.Add("Usage: mv-utils <json-file-path> <schema-file-path>");
					errors.Add("   or: mv-utils --json <json-file-path> --schema <schema-file-path>");
					return new ArgumentParseResult(false, null, null, errors);

				default:
					errors.Add($"Unknown argument: {args[i]}");
					break;
			}
		}

		if (string.IsNullOrWhiteSpace(jsonPath))
		{
			errors.Add("JSON file path is required. Use --json <path>.");
		}

		if (string.IsNullOrWhiteSpace(schemaPath))
		{
			errors.Add("Schema file path is required. Use --schema <path>.");
		}

		return errors.Count > 0
			? new ArgumentParseResult(false, null, null, errors)
			: new ArgumentParseResult(true, jsonPath, schemaPath, errors);
	}
}

public sealed record ValidationResult(
	[property: JsonProperty("valid")] bool Valid,
	[property: JsonProperty("errors")] List<string> Errors);

public sealed record ArgumentParseResult(bool Success, string? JsonPath, string? SchemaPath, List<string> Errors);

public sealed record SchemaLoadResult(bool Success, Uri? SchemaUri, string? SchemaContent, List<string> Errors);