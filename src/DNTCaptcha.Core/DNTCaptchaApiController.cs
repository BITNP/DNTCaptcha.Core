using System;
using DNTCaptcha.Core.Contracts;
using DNTCaptcha.Core.Providers;
using Microsoft.AspNetCore.Mvc;

namespace DNTCaptcha.Core
{
    /// <summary>
    /// DNTCaptcha Api
    /// </summary>
    public class DNTCaptchaApiController : Controller
    {
        private readonly ICaptchaProtectionProvider _captchaProtectionProvider;
        private readonly ICaptchaStorageProvider _captchaStorageProvider;
        private readonly IHumanReadableIntegerProvider _humanReadableIntegerProvider;
        private readonly IRandomNumberProvider _randomNumberProvider;

        /// <summary>
        /// DNTCaptcha TagHelper
        /// </summary>
        public DNTCaptchaApiController(
            ICaptchaProtectionProvider captchaProtectionProvider,
            IRandomNumberProvider randomNumberProvider,
            IHumanReadableIntegerProvider humanReadableIntegerProvider,
            ICaptchaStorageProvider captchaStorageProvider)
        {
            captchaProtectionProvider.CheckArgumentNull(nameof(captchaProtectionProvider));
            randomNumberProvider.CheckArgumentNull(nameof(randomNumberProvider));
            humanReadableIntegerProvider.CheckArgumentNull(nameof(humanReadableIntegerProvider));
            captchaStorageProvider.CheckArgumentNull(nameof(captchaStorageProvider));

            _captchaProtectionProvider = captchaProtectionProvider;
            _randomNumberProvider = randomNumberProvider;
            _humanReadableIntegerProvider = humanReadableIntegerProvider;
            _captchaStorageProvider = captchaStorageProvider;
        }

        /// <summary>
        /// Creates DNTCaptcha
        /// </summary>
        /// <param name="captchaAttributes">captcha attributes</param>
        /// <returns></returns>
        [HttpPost]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true, Duration = 0)]
        public IActionResult CreateDNTCaptcha([FromBody]DNTCaptchaTagHelperHtmlAttributes captchaAttributes)
        {
            var number = _randomNumberProvider.Next(captchaAttributes.Min, captchaAttributes.Max);
            var randomText = _humanReadableIntegerProvider.NumberToText(number, captchaAttributes.Language);
            var encryptedText = _captchaProtectionProvider.Encrypt(randomText);
            var captchaImageUrl = getCaptchaImageUrl(captchaAttributes, encryptedText);
            var captchaDivId = $"dntCaptcha{Guid.NewGuid().ToString("N")}{_randomNumberProvider.Next(captchaAttributes.Min, captchaAttributes.Max)}";
            var cookieToken = $".{captchaDivId}";
            var hiddenInputToken = _captchaProtectionProvider.Encrypt(cookieToken);

            _captchaStorageProvider.Add(HttpContext, cookieToken, randomText);

            return Json(new
            {
                dntCaptchaImgUrl = captchaImageUrl,
                dntCaptchaId = captchaDivId,
                dntCaptchaTextValue = encryptedText,
                dntCaptchaTokenValue = hiddenInputToken
            });
        }

        private string getCaptchaImageUrl(DNTCaptchaTagHelperHtmlAttributes captchaAttributes, string encryptedText)
        {
            return "";
            //var actionUrl = Url.Action(action: nameof(DNTCaptchaImageController.Show),
            //   controller: nameof(DNTCaptchaImageController).Replace("Controller", string.Empty),
            //   values:
            //   new
            //   {
            //       text = encryptedText,
            //       rndDate = DateTime.Now.Ticks,
            //       foreColor = captchaAttributes.ForeColor,
            //       backColor = captchaAttributes.BackColor,
            //       fontSize = captchaAttributes.FontSize,
            //       fontName = captchaAttributes.FontName,
            //       area = ""
            //   },
            //   protocol: HttpContext.Request.Scheme);
            //return actionUrl;
        }
    }
}