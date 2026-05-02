using GenericScreenGenFactoryLib;
using GenericScreenGenImplementationsLib;
using GenericScreenGenInterfacesLib;
using GenericScreenGenUtilsLib;
using Microsoft.AspNetCore.Cors.Infrastructure;
using MyDataStoreProviders;

namespace GenericScreenGenApp
{
	/// <summary>
	/// Application entry point for the Generic Screen Generator web host.
	/// </summary>
	public static class Program
	{
		private const string RECORD_ID_FIELD_NAME = "__record-id";

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
			objBuilder.Services.AddSingleton<ILayoutPolicy, CFlowLayoutPolicy>();
			objBuilder.Services.AddSingleton<ILayoutPolicy, CTabularLayoutPolicy>();
			objBuilder.Services.AddSingleton<ILayoutPolicy, CRecordDetailLayoutPolicy>();
			objBuilder.Services.AddSingleton<ILayoutPolicyRegistry, CLayoutPolicyRegistry>();
			objBuilder.Services.AddSingleton<IFieldTypeRegistry>(delegate
			{
				return CreateFieldTypeRegistry(objBuilder.Environment.ContentRootPath);
			});
			objBuilder.Services.AddSingleton<IScreenConfigProvider>(sp =>
			{
				ILayoutPolicyRegistry itfLayoutPolicyRegistry = sp.GetRequiredService<ILayoutPolicyRegistry>();
				IFieldTypeRegistry itfFieldTypeRegistry = sp.GetRequiredService<IFieldTypeRegistry>();
				return CreateScreenConfigProvider(objFactory, itfLayoutPolicyRegistry, itfFieldTypeRegistry);
			});
			objBuilder.Services.AddSingleton<IScreenSchemaValidator>(delegate { return CreateScreenSchemaValidator(objFactory); });
			objBuilder.Services.AddSingleton<IScreenRenderModelFactory>(delegate { return CreateScreenRenderModelFactory(objFactory); });
			objBuilder.Services.AddSingleton<IDataStore>(delegate
			{
				string strDataStoreFolderPath = Path.Combine(objBuilder.Environment.ContentRootPath, "DataStore");
				return new CJsonDataStore(strDataStoreFolderPath);
			});

			WebApplication objApp = objBuilder.Build();
			objApp.UseCors("ClientAppCorsPolicy");

			objApp.MapPost("/api/screens/refresh", delegate (IScreenConfigProvider itfScreenConfigProvider)
			{
				if (!itfScreenConfigProvider.TryReloadScreens(out string strReloadError))
				{
					return Results.Problem(strReloadError);
				}

				if (!itfScreenConfigProvider.TryGetAvailableScreens(out IReadOnlyList<string> lstScreenFileNames, out string strListError))
				{
					return Results.Problem(strListError);
				}

				return Results.Ok(new
				{
					refreshedScreenCount = lstScreenFileNames.Count,
					refreshedAtUtc = DateTime.UtcNow
				});
			});

			objApp.MapGet("/api/screens", delegate (IScreenConfigProvider itfScreenConfigProvider)
			{
				if (!itfScreenConfigProvider.TryGetAvailableScreens(out IReadOnlyList<string> lstScreenFileNames, out string strError))
				{
					return Results.Problem(strError);
				}

				List<object> lstResponse = new List<object>();

				foreach (string strScreenFileName in lstScreenFileNames)
				{
					if (!itfScreenConfigProvider.TryGetScreenDefinition(strScreenFileName, out IScreenDefinition? itfScreenDefinition, out string strLookupError) || itfScreenDefinition is null)
					{
						return Results.Problem(strLookupError);
					}

					lstResponse.Add(new
					{
						screenId = itfScreenDefinition.ScreenId,
						fileName = strScreenFileName,
						displayName = itfScreenDefinition.DisplayName
					});
				}

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

			objApp.MapGet("/api/data/{strScreenFileName}/{strRecordId}", delegate (
				string strScreenFileName,
				string strRecordId,
				IDataStore itfDataStore)
			{
				if (!itfDataStore.TryLoadRows(strScreenFileName, out IReadOnlyList<IReadOnlyDictionary<string, string>> lstRows, out string strLoadError))
				{
					return Results.Problem(strLoadError);
				}

				IReadOnlyDictionary<string, string>? itfMatchingRow = FindRowByRecordId(lstRows, strRecordId);
				if (itfMatchingRow is null)
				{
					return Results.NotFound(new { error = $"Record '{strRecordId}' was not found for screen '{strScreenFileName}'." });
				}

				Dictionary<string, string> dictResponseData = itfMatchingRow
					.Where(objKvp => !string.Equals(objKvp.Key, RECORD_ID_FIELD_NAME, StringComparison.OrdinalIgnoreCase))
					.ToDictionary(objKvp => objKvp.Key, objKvp => objKvp.Value ?? string.Empty, StringComparer.OrdinalIgnoreCase);

				return Results.Ok(new
				{
					recordId = strRecordId,
					data = dictResponseData
				});
			});

			objApp.MapPut("/api/data/{strScreenFileName}/{strRecordId}", delegate (
				string strScreenFileName,
				string strRecordId,
				CRecordDataUpsertRequest? objRequest,
				IDataStore itfDataStore)
			{
				if (objRequest?.Data is null)
				{
					return Results.BadRequest(new { error = "Request body must contain a data object." });
				}

				if (!itfDataStore.TryLoadRows(strScreenFileName, out IReadOnlyList<IReadOnlyDictionary<string, string>> lstRows, out string strLoadError))
				{
					return Results.Problem(strLoadError);
				}

				Dictionary<string, string> dictRecordData = objRequest.Data
					.Where(objKvp => !string.IsNullOrWhiteSpace(objKvp.Key))
					.ToDictionary(objKvp => objKvp.Key, objKvp => objKvp.Value ?? string.Empty, StringComparer.OrdinalIgnoreCase);
				dictRecordData[RECORD_ID_FIELD_NAME] = strRecordId;

				List<IReadOnlyDictionary<string, string>> lstWritableRows = lstRows
					.Select(itfRow => (IReadOnlyDictionary<string, string>)itfRow.ToDictionary(objKvp => objKvp.Key, objKvp => objKvp.Value ?? string.Empty, StringComparer.OrdinalIgnoreCase))
					.ToList();

				int iExistingIndex = FindRowIndexByRecordId(lstWritableRows, strRecordId);
				if (iExistingIndex >= 0)
				{
					lstWritableRows[iExistingIndex] = dictRecordData;
				}
				else
				{
					lstWritableRows.Add(dictRecordData);
				}

				if (!itfDataStore.TrySaveRows(strScreenFileName, lstWritableRows, out string strSaveError))
				{
					return Results.Problem(strSaveError);
				}

				return Results.Ok(new
				{
					screenFileName = strScreenFileName,
					recordId = strRecordId,
					savedAtUtc = DateTime.UtcNow
				});
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

		private static IScreenConfigProvider CreateScreenConfigProvider(CGenericScreenGenFactory objFactory, ILayoutPolicyRegistry itfLayoutPolicyRegistry, IFieldTypeRegistry itfFieldTypeRegistry)
		{
			if (!objFactory.TryCreateScreenConfigProvider(itfLayoutPolicyRegistry, itfFieldTypeRegistry, out IScreenConfigProvider? itfScreenConfigProvider, out string strProviderError) || itfScreenConfigProvider is null)
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

		private static IFieldTypeRegistry CreateFieldTypeRegistry(string strContentRootPath)
		{
			CFieldTypeRegistry objFieldTypeRegistry = new CFieldTypeRegistry();

			if (!objFieldTypeRegistry.Init(strContentRootPath, out string strError))
			{
				throw new InvalidOperationException(strError);
			}

			return objFieldTypeRegistry;
		}

		private static IReadOnlyDictionary<string, string>? FindRowByRecordId(IReadOnlyList<IReadOnlyDictionary<string, string>> lstRows, string strRecordId)
		{
			foreach (IReadOnlyDictionary<string, string> itfRow in lstRows)
			{
				if (itfRow.TryGetValue(RECORD_ID_FIELD_NAME, out string? strExistingRecordId) &&
					string.Equals(strExistingRecordId, strRecordId, StringComparison.OrdinalIgnoreCase))
				{
					return itfRow;
				}
			}

			return null;
		}

		private static int FindRowIndexByRecordId(IReadOnlyList<IReadOnlyDictionary<string, string>> lstRows, string strRecordId)
		{
			for (int iIndex = 0; iIndex < lstRows.Count; iIndex++)
			{
				IReadOnlyDictionary<string, string> itfRow = lstRows[iIndex];
				if (itfRow.TryGetValue(RECORD_ID_FIELD_NAME, out string? strExistingRecordId) &&
					string.Equals(strExistingRecordId, strRecordId, StringComparison.OrdinalIgnoreCase))
				{
					return iIndex;
				}
			}

			return -1;
		}

		private sealed class CRecordDataUpsertRequest
		{
			public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		}
	}
}
