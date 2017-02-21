using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Author:         Ronald Wildenberg
/// Description:    Localization.JsonLocalizer Class Library
/// Url:            https://github.com/rwwilden/AspNet5Localization
/// </summary>
namespace Fluent.Core.Services.Localization
{
    public class JsonStringLocalizer : IStringLocalizer
    {
        private readonly ConcurrentDictionary<string, Lazy<JObject>> _resourceObjectCache = new ConcurrentDictionary<string, Lazy<JObject>>();
        private readonly string _baseName;
        private readonly string _applicationName;
        private readonly ILogger _logger;
        private readonly IOptions<JsonLocalizationOptions> _options;
        private readonly IEnumerable<string> _resourceFileLocations;

        public JsonStringLocalizer(string baseName, string applicationName, ILogger logger, IOptions<JsonLocalizationOptions> options)
        {
            if (baseName == null)
            {
                throw new ArgumentNullException(nameof(baseName));
            }
            if (applicationName == null)
            {
                throw new ArgumentNullException(nameof(applicationName));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this._baseName = baseName;
            this._applicationName = applicationName;
            this._logger = logger;
            this._options = options;

            // Get a list of possible resource file locations.
            _resourceFileLocations = LocalizerUtil.ExpandPaths(baseName, applicationName).ToList();
            foreach (var resFileLocation in _resourceFileLocations)
            {
                logger.LogTrace($"Resource file location base path: {resFileLocation}");
            }
        }

        public virtual LocalizedString this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var value = GetLocalizedString(name, CultureInfo.CurrentUICulture);
                return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
            }
        }

        public virtual LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var format = GetLocalizedString(name, CultureInfo.CurrentUICulture);
                var value = string.Format(format ?? name, arguments);
                return new LocalizedString(name, value, resourceNotFound: format == null);
            }
        }

        public virtual IEnumerable<LocalizedString> GetAllStrings(bool includeAncestorCultures) => GetAllStrings(includeAncestorCultures, CultureInfo.CurrentUICulture);

        protected IEnumerable<LocalizedString> GetAllStrings(bool includeAncestorCultures, CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }
            throw new NotImplementedException();
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            if (culture == null)
            {
                return new JsonStringLocalizer(_baseName, _applicationName, _logger, _options);
            }
            throw new NotImplementedException();
        }

        protected string GetLocalizedString(string name, CultureInfo culture)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            // Attempt to get resource with the given name from the resource object. if not found, try parent
            // resource object until parent begets himself.
            var currentCulture = CultureInfo.CurrentCulture;
            CultureInfo previousCulture = null;
            do
            {
                var resourceObject = GetResourceObject(currentCulture);
                if (resourceObject == null)
                {
                    _logger.LogInformation($"No resource file found or error occurred for base name {_baseName}, culture {currentCulture} and key '{name}'");
                }
                else
                {
                    JToken value;
                    if (resourceObject.TryGetValue(name, out value))
                    {
                        var localizedString = value.ToString();
                        return localizedString;
                    }
                }

                // Consult parent culture.
                previousCulture = currentCulture;
                currentCulture = currentCulture.Parent;
                _logger.LogTrace($"Switching to parent culture {currentCulture} for key '{name}'.");
            }
            while (previousCulture != currentCulture);

            _logger.LogInformation($"Could not find key '{name}' in resource file for base name {_baseName} and culture {CultureInfo.CurrentCulture}");
            return null;
        }

        private JObject GetResourceObject(CultureInfo currentCulture)
        {
            if (currentCulture == null)
            {
                throw new ArgumentNullException(nameof(currentCulture));
            }

            _logger.LogTrace($"Attempt to get resource object for culture {currentCulture}");
            var cultureSuffix = "." + currentCulture.Name;
            cultureSuffix = cultureSuffix == "." ? "" : cultureSuffix;

            var assembly = Assembly.GetEntryAssembly();
            var isAssembly = false;
            Stream assemblyResourceStream = null;

            var lazyJObjectGetter = new Lazy<JObject>(() =>
            {
                // First attempt to find a resource file location that exists.
                string resourcePath = null;
                foreach (var resourceFileLocation in _resourceFileLocations)
                {
                    resourcePath = resourceFileLocation + cultureSuffix + ".json";
                    if (File.Exists(resourcePath))
                    {
                        _logger.LogInformation($"Resource file location {resourcePath} found");
                        isAssembly = false;
                        break;
                    }
                    else if ((assemblyResourceStream = GetResourceObjectFromAssembly(resourcePath)) != null)
                    {
                        _logger.LogInformation($"Resource assembly location {resourcePath} found");
                        isAssembly = true;
                        break;
                    }
                    else
                    {
                        _logger.LogTrace($"Resource file location {resourcePath} does not exist");
                        resourcePath = null;
                    }
                }
                if (resourcePath == null)
                {
                    _logger.LogTrace($"No resource file found for suffix {cultureSuffix}");
                    return null;
                }

                // Found a resource file path: attempt to parse it into a JObject.
                try
                {
                    if (!isAssembly)
                    {
                        var resourceFileStream =
                        new FileStream(resourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
                        using (resourceFileStream)
                        {
                            var resourceReader =
                                new JsonTextReader(new StreamReader(resourceFileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true));
                            using (resourceReader)
                            {
                                return JObject.Load(resourceReader);
                            }
                        }
                    }
                    else
                    {
                        var resourceReader =
                                new JsonTextReader(new StreamReader(assemblyResourceStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true));
                            using (resourceReader)
                            {
                                return JObject.Load(resourceReader);
                            }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error occurred attempting to read JSON resource file {resourcePath}: {e}");
                    return null;
                }

            }, LazyThreadSafetyMode.ExecutionAndPublication);

            lazyJObjectGetter = _resourceObjectCache.GetOrAdd(cultureSuffix, lazyJObjectGetter);
            var resourceObject = lazyJObjectGetter.Value;
            return resourceObject;
        }

        private Stream GetResourceObjectFromAssembly(string resourcePath)
        {
            foreach (var assembly in Assembly.GetEntryAssembly().GetReferencedAssemblies()
                                             .Where(a => Regex.IsMatch(a.Name, _options.Value.AllowedAssembliesRegularExpression,
                                                                       RegexOptions.IgnoreCase) ))
            {
                _logger.LogTrace($"Searching reference assembly {assembly.Name} for {resourcePath}");
                var loaded = Assembly.Load(assembly);
                Stream resourceStream = null;
                if (loaded != null && (resourceStream = loaded.GetManifestResourceStream(resourcePath)) != null)
                {
                    _logger.LogInformation($"Found {resourcePath} in assembly {assembly.Name}");
                    return resourceStream;
                }
            }
            return null;
        }

        private string[] GetCultureSuffixes(CultureInfo currentCulture)
        {
            // Get culture suffixes (e.g.: { "nl-NL.", "nl.", "" }).
            string[] cultureSuffixes;
            if (currentCulture == null)
            {
                cultureSuffixes = new[] { "" };
            }
            else
            {
                if (currentCulture.IsNeutralCulture)
                {
                    cultureSuffixes = new[] { currentCulture.Name + ".", "" };
                }
                else
                {
                    cultureSuffixes = new[] { currentCulture.Name + ".", currentCulture.Parent.Name + ".", "" };
                }
            }

            var cultureSuffixesLogString = string.Join(", ", cultureSuffixes);
            _logger.LogTrace($"Using culture suffixes {cultureSuffixesLogString}");
            return cultureSuffixes;
        }
    }
}