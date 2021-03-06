using System;
using System.Collections.Concurrent;
using System.Linq;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Author:         Ronald Wildenberg
/// Description:    Localization.JsonLocalizer Class Library
/// Url:            https://github.com/rwwilden/AspNet5Localization
/// </summary>
namespace Fluent.Core.Services.Localization
{
    public class JsonStringLocalizerFactory : IStringLocalizerFactory
    {
        private static readonly string[] KnownViewExtensions = new[] { ".cshtml" };
        
        private readonly ConcurrentDictionary<string, JsonStringLocalizer> _localizerCache =
            new ConcurrentDictionary<string, JsonStringLocalizer>();
        
        private readonly IHostingEnvironment _applicationEnvironment;
        private readonly ILogger<JsonStringLocalizerFactory> _logger;
        private readonly IOptions<JsonLocalizationOptions> _options;
        private string _resourcesRelativePath;

        public JsonStringLocalizerFactory(IHostingEnvironment applicationEnvironment,
                                          IOptions<JsonLocalizationOptions> localizationOptions,
                                          ILogger<JsonStringLocalizerFactory> logger)
        {
            if (applicationEnvironment == null)
            {
                throw new ArgumentNullException(nameof(applicationEnvironment));
            }
            if (localizationOptions == null)
            {
                throw new ArgumentNullException(nameof(localizationOptions));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this._applicationEnvironment = applicationEnvironment;
            this._logger = logger;
            this._options = localizationOptions;

            _resourcesRelativePath = localizationOptions.Value.ResourcesPath ?? string.Empty;
            if (!string.IsNullOrEmpty(_resourcesRelativePath))
            {
                _resourcesRelativePath = _resourcesRelativePath
                    .Replace(Path.AltDirectorySeparatorChar, '.')
                    .Replace(Path.DirectorySeparatorChar, '.') + ".";
            }
            
            logger.LogTrace($"Created {nameof(JsonStringLocalizerFactory)} with:{Environment.NewLine}" +
                $"    (application name: {applicationEnvironment.ApplicationName}{Environment.NewLine}" +
                $"    (resources relative path: {_resourcesRelativePath})");
        }

        public IStringLocalizer Create(Type resourceSource)
        {
            if (resourceSource == null)
            {
                throw new ArgumentNullException(nameof(resourceSource));
            }
            
            _logger.LogTrace($"Getting localizer for type {resourceSource}");
            
            var typeInfo = resourceSource.GetTypeInfo();
            var assemblyName = typeInfo.Assembly.GetName();
            var resourceBaseName = string.Empty;

            if (Assembly.GetEntryAssembly().FullName != typeInfo.Assembly.FullName)
            {
                // Different logic if coming from another assembly
                resourceBaseName = string.IsNullOrEmpty(_resourcesRelativePath)
                ? typeInfo.FullName
                : assemblyName.Name + "." + _resourcesRelativePath + "." + typeInfo.Name;
            }
            else
            {
                // Re-root the base name if a resources path is set.
                resourceBaseName = string.IsNullOrEmpty(_resourcesRelativePath)
                ? typeInfo.FullName
                : _applicationEnvironment.ApplicationName + "." + _resourcesRelativePath +
                    LocalizerUtil.TrimPrefix(typeInfo.FullName, _applicationEnvironment.ApplicationName + ".");
            }
            _logger.LogTrace($"Localizer basename: {resourceBaseName}");

            return _localizerCache.GetOrAdd(
                resourceBaseName, new JsonStringLocalizer(resourceBaseName, _applicationEnvironment.ApplicationName, _logger, _options));
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            if (baseName == null)
            {
                throw new ArgumentNullException(nameof(baseName));
            }
            
            _logger.LogTrace($"Getting localizer for baseName {baseName} and location {location}");
            
            location = location ?? _applicationEnvironment.ApplicationName;
            
            // Re-root base name if a resources path is set and strip the cshtml part.
            var resourceBaseName = location + "." + _resourcesRelativePath + LocalizerUtil.TrimPrefix(baseName, location + ".");
            
            var viewExtension = KnownViewExtensions.FirstOrDefault(extension => resourceBaseName.EndsWith(extension));
            if (viewExtension != null)
            {
                resourceBaseName = resourceBaseName.Substring(0, resourceBaseName.Length - viewExtension.Length);
            }
            
            _logger.LogTrace($"Localizer basename: {resourceBaseName}");
            
            return _localizerCache.GetOrAdd(
                resourceBaseName, new JsonStringLocalizer(resourceBaseName, _applicationEnvironment.ApplicationName, _logger, _options));
        }
    }
}