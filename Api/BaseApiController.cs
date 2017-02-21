using System;
using Fluent.Core.Services.Context;
using Microsoft.AspNetCore.Mvc;

namespace Fluent.Core.Api
{
    public class BaseApiController : Controller
    {
        public BaseApiController(IFluentContext fluent, CommonResponses common)
        {
            if (fluent == null)
            {
                throw new ArgumentNullException(nameof(fluent));
            }
            if (common == null)
            {
                throw new ArgumentNullException(nameof(common));
            }

            Fluent = fluent;
            CommonResponses = common;
        }

        public IFluentContext Fluent { get; private set; }

        public CommonResponses CommonResponses { get; private set; }
    }
}