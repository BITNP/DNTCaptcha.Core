using DNTCaptcha.Core.Contracts;
using DNTCaptcha.Core.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DNTCaptcha.Core
{
    internal sealed class DNTCaptchaMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ICaptchaImageProvider _captchaImageProvider;
        private readonly ICaptchaProtectionProvider _captchaProtectionProvider;
        private readonly ICaptchaStorageProvider _captchaStorageProvider;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly ILogger<DNTCaptchaImageController> _logger;

        private static readonly Dictionary<string, string> DefaultValues = new Dictionary<string, string>()
        {
            ["text"] = null,
            ["rndDate"] = null,
            ["foreColor"] = "#1B0172",
            ["backColor"] = "",
            ["fontSize"] = "12",
            ["fontName"] = "Tahoma"
        };
        private static readonly RouteData EmptyRouteData = new RouteData();
        private static readonly ActionDescriptor EmptyActionDescriptor = new ActionDescriptor();

        public DNTCaptchaMiddleware(
            RequestDelegate next,
            ICaptchaImageProvider captchaImageProvider,
            ICaptchaProtectionProvider captchaProtectionProvider,
            ITempDataProvider tempDataProvider,
            ICaptchaStorageProvider captchaStorageProvider,
            ILogger<DNTCaptchaImageController> logger)
        {
            this._next = next;

            captchaImageProvider.CheckArgumentNull(nameof(captchaImageProvider));
            captchaProtectionProvider.CheckArgumentNull(nameof(captchaProtectionProvider));
            tempDataProvider.CheckArgumentNull(nameof(tempDataProvider));
            captchaStorageProvider.CheckArgumentNull(nameof(captchaStorageProvider));
            captchaStorageProvider.CheckArgumentNull(nameof(logger));

            _captchaImageProvider = captchaImageProvider;
            _captchaProtectionProvider = captchaProtectionProvider;
            _tempDataProvider = tempDataProvider;
            _captchaStorageProvider = captchaStorageProvider;
            _logger = logger;
        }

        public Task Invoke(HttpContext httpContext)
        {
            if (!httpContext.Request.Path.Equals(Constants.SHOW_IMAGE_NAME, StringComparison.CurrentCultureIgnoreCase))
                return _next(httpContext);

            IQueryCollection query = httpContext.Request.Query;
            string text = GetQueryValue(query, "text");
            string rndDate = GetQueryValue(query, "rndDate");
            string foreColor = GetQueryValue(query, "foreColor");
            string backColor = GetQueryValue(query, "backColor");
            string fontSize = GetQueryValue(query, "fontSize");
            string fontName = GetQueryValue(query, "fontName");
            if (!float.TryParse(fontSize, out float fFontSize))
            {
                fFontSize = float.Parse(DefaultValues["fontSize"]);
            }
            return DrawImageAsync(httpContext, text, rndDate, foreColor, backColor, fFontSize, fontName);
        }
        private string GetQueryValue(IQueryCollection query, string key)
        {
            if (query.TryGetValue(key, out var value))
            {
                return value;
            }
            return DefaultValues[key];
        }

        private Task DrawImageAsync(HttpContext httpContext, string text, string rndDate, string foreColor = "#1B0172", string backColor = "", float fontSize = 12, string fontName = "Tahoma")
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                httpContext.Response.StatusCode = 400;
                return Task.CompletedTask;
            }

            var decryptedText = _captchaProtectionProvider.Decrypt(text);
            if (decryptedText == null)
            {
                httpContext.Response.StatusCode = 400;
                return Task.CompletedTask;
            }

            byte[] image;
            try
            {
                image = _captchaImageProvider.DrawCaptcha(decryptedText, foreColor, backColor, fontSize, fontName);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(1001, ex, "DrawCaptcha error.");
                httpContext.Response.StatusCode = 400;
                return Task.CompletedTask;
            }
            FileContentResult result = new FileContentResult(_captchaImageProvider.DrawCaptcha(decryptedText, foreColor, backColor, fontSize, fontName), "image/png");
            HttpResponse response = httpContext.Response;
            response.ContentType = result.ContentType;
            int length = result.FileContents.Length;
            response.ContentLength = length;
            response.StatusCode = 200;
            return response.Body.WriteAsync(result.FileContents, 0, length);
        }
    }
}
