using DNTCaptcha.Core.Contracts;
using DNTCaptcha.Core.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DNTCaptcha.Core
{
    /// <summary>
    /// Constants.
    /// </summary>
    internal static class Constants
    {
        public const string SHOW_IMAGE_NAME = "/Captcha/Image";
    }
}