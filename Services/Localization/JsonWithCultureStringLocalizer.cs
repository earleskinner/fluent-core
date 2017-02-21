using System;
using System.Collections.Generic;
using System.Globalization;
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
    public class JsonWithCultureStringLocalizer : JsonStringLocalizer
    {
        private CultureInfo _culture;

        public JsonWithCultureStringLocalizer(string baseName, string applicationName, CultureInfo culture, ILogger logger, IOptions<JsonLocalizationOptions> localizationOptions)
            : base(baseName, applicationName, logger, localizationOptions)
        {
            if (baseName == null)
            {
                throw new ArgumentNullException(nameof(baseName));
            }
            if (applicationName == null)
            {
                throw new ArgumentNullException(nameof(applicationName));
            }
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            if (localizationOptions == null)
            {
                throw new ArgumentNullException(nameof(localizationOptions));
            }
            
            this._culture = culture;
        }
        
        public override LocalizedString this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }
                
                var value = GetLocalizedString(name, _culture);
                return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
            }
        }
        
        public override LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }
                
                var format = GetLocalizedString(name, _culture);
                var value = string.Format(format ?? name, arguments);
                return new LocalizedString(name, value, resourceNotFound: format == null);
            }
        }
        
        public override IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => GetAllStrings(includeParentCultures, _culture);
    }
}