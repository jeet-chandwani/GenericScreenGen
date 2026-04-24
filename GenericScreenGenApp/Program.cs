using GenericScreenGenFactoryLib;
using GenericScreenGenImplementationsLib;
using GenericScreenGenInterfacesLib;
using GenericScreenGenUtilsLib;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace GenericScreenGenApp
{
	/// <summary>
	/// Application entry point for the Generic Screen Generator web host.
	/// </summary>
	public static class Program
	{
		/// <summary>
		/// Application entry point.
		/// </summary>
		/// <param name="arrArgs">Command-line arguments.</param>
		/// <returns>Zero when startup succeeds.</returns>
		public static int Main(string[] arrArgs)
		{
			WebApplicationBuilder objBuilder = WebApplication.CreateBuilder(arrArgs);
			CGenericScreenGenFactory objFactory = CreateFactory(objBuilder.Environment.ContentRootPath);
			string[] arrAllowedCorsOrigins = objBuilder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

			objBuilder.Services.AddCors(delegate (CorsOptions objCorsOptions)
			{
				objCorsOptions.AddPolicy("ClientAppCorsPolicy", delegate (CorsPolicyBuilder objCorsPolicyBuilder)
				{
					if (arrAllowedCorsOrigins.Length == 0 || arrAllowedCorsOrigins.Any(strOrigin => strOrigin == "*"))
					{
						objCorsPolicyBuilder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
						return;
					}

					objCorsPolicyBuilder.WithOrigins(arrAllowedCorsOrigins).AllowAnyHeader().AllowAnyMethod();
				});
			});

			objBuilder.Services.AddSingleton<ILayoutPolicy, CPerLineLayoutPolicy>();
			objBuilder.Services.AddSingleton<ILayoutPolicyRegistry, CLayoutPolicyRegistry>();
			objBuilder.Services.AddSingleton<IScreenConfigProvider>(sp =>
			{
				ILayoutPolicyRegistry itfRegistry = sp.GetRequiredService<ILayoutPolicyRegistry>();
				return CreateScreenConfigProvider(objFactory, itfRegistry);
			});
			objBuilder.Services.AddSingleton<IScreenSchemaValidator>(delegate { return CreateScreenSchemaValidator(objFactory); });
			objBuilder.Services.AddSingleton<IScreenRenderModelFactory>(delegate { return CreateScreenRenderModelFactory(objFactory); });

			WebApplication objApp = objBuilder.Build();
			objApp.UseCors("ClientAppCorsPolicy");

			objApp.MapGet("/api/screens", delegate (IScreenConfigProvider itfScreenConfigProvider)
			{
				if (!itfScreenConfigProvider.TryGetAvailableScreens(out IReadOnlyList<string> lstScreenFileNames, out string strError))
				{
					return Results.Problem(strError);
				}

				List<object> lstResponse = lstScreenFileNames
					.Select(strScreenFileName => new
					{
						fileName = strScreenFileName,
						displayName = CScreenNameUtility.GetDisplayNameFromFileName(strScreenFileName)
					})
					.Cast<object>()
					.ToList();

				return Results.Ok(lstResponse);
			});

			objApp.MapGet("/api/screens/{strScreenFileName}", delegate (string strScreenFileName, IScreenConfigProvider itfScreenConfigProvider)
			{
				if (!itfScreenConfigProvider.TryGetScreenDefinition(strScreenFileName, out IScreenDefinition? itfScreenDefinition, out string strError) || itfScreenDefinition is null)
				{
					return Results.NotFound(new { error = strError });
				}

				return Results.Ok(itfScreenDefinition);
			});

			objApp.MapGet("/api/screens/{strScreenFileName}/render", delegate (
				string strScreenFileName,
				IScreenConfigProvider itfScreenConfigProvider,
				IScreenRenderModelFactory itfScreenRenderModelFactory)
			{
				if (!itfScreenConfigProvider.TryGetScreenDefinition(strScreenFileName, out IScreenDefinition? itfScreenDefinition, out string strLookupError) || itfScreenDefinition is null)
				{
					return Results.NotFound(new { error = strLookupError });
				}

				if (!itfScreenRenderModelFactory.TryCreateRenderModel(itfScreenDefinition, out IScreenRenderModel? itfScreenRenderModel, out string strRenderError) || itfScreenRenderModel is null)
				{
					return Results.Problem(strRenderError);
				}

				return Results.Ok(itfScreenRenderModel);
			});

			objApp.MapGet("/api/screens/validation", delegate (IScreenConfigProvider itfScreenConfigProvider, IScreenSchemaValidator itfScreenSchemaValidator)
			{
				if (!itfScreenConfigProvider.TryGetAvailableScreens(out IReadOnlyList<string> lstScreenFileNames, out string strListError))
				{
					return Results.Problem(strListError);
				}

				string strScreenFolderPath = Path.Combine(objBuilder.Environment.ContentRootPath, CScreenGeneratorConstants.SCREEN_FOLDER_NAME);
				List<IScreenValidationResult> lstValidationResults = new List<IScreenValidationResult>();

				foreach (string strScreenFileName in lstScreenFileNames)
				{
					string strScreenFilePath = Path.Combine(strScreenFolderPath, strScreenFileName);

					if (!itfScreenSchemaValidator.TryValidateScreen(strScreenFilePath, out IScreenValidationResult? itfValidationResult, out string strValidationError) || itfValidationResult is null)
					{
						return Results.Problem(strValidationError);
					}

					lstValidationResults.Add(itfValidationResult);
				}

				return Results.Ok(lstValidationResults);
			});

			objApp.MapGet("/api/schema", delegate ()
			{
				string strSchemaFilePath = Path.Combine(objBuilder.Environment.ContentRootPath, "Schemas", "ScreenConfigSchema.json");
				return Results.File(strSchemaFilePath, "application/schema+json");
			});

			objApp.Run();
			return 0;
		}

		private static CGenericScreenGenFactory CreateFactory(string strContentRootPath)
		{
			string strScreenFolderPath = Path.Combine(strContentRootPath, CScreenGeneratorConstants.SCREEN_FOLDER_NAME);
			CGenericScreenGenFactory objFactory = new CGenericScreenGenFactory();

			if (!objFactory.Init(strScreenFolderPath, out string strFactoryError))
			{
				throw new InvalidOperationException(strFactoryError);
			}

			return objFactory;
		}

		private static IScreenConfigProvider CreateScreenConfigProvider(CGenericScreenGenFactory objFactory, ILayoutPolicyRegistry itfLayoutPolicyRegistry)
		{
			if (!objFactory.TryCreateScreenConfigProvider(itfLayoutPolicyRegistry, out IScreenConfigProvider? itfScreenConfigProvider, out string strProviderError) || itfScreenConfigProvider is null)
			{
				throw new InvalidOperationException(strProviderError);
			}

			return itfScreenConfigProvider;
		}

		private static IScreenSchemaValidator CreateScreenSchemaValidator(CGenericScreenGenFactory objFactory)
		{
			if (!objFactory.TryCreateScreenSchemaValidator(out IScreenSchemaValidator? itfScreenSchemaValidator, out string strValidatorError) || itfScreenSchemaValidator is null)
			{
				throw new InvalidOperationException(strValidatorError);
			}

			return itfScreenSchemaValidator;
		}

		private static IScreenRenderModelFactory CreateScreenRenderModelFactory(CGenericScreenGenFactory objFactory)
		{
			if (!objFactory.TryCreateScreenRenderModelFactory(out IScreenRenderModelFactory? itfScreenRenderModelFactory, out string strFactoryError) || itfScreenRenderModelFactory is null)
			{
				throw new InvalidOperationException(strFactoryError);
			}

			return itfScreenRenderModelFactory;
		}
	}
}
