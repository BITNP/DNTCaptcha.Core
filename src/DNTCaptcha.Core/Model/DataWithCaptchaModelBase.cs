using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using DNTCaptcha.Core.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace DNTCaptcha.Core.Model
{

    /// <summary>
    /// Validation Attribute for Captcha
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ValidateCaptchaAttribute : ValidationAttribute
    {
        /// <summary>
        /// Validation Invoker
        /// </summary>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (validationContext.ObjectInstance == null)
                return new ValidationResult("The object reference is not set to an instance of object");

            if (!(validationContext.ObjectInstance is DataWithCaptchaModelBase modelBase))
                return new ValidationResult($"The model is not a derived type of {typeof(DataWithCaptchaModelBase)}");

            HttpContext httpContext = validationContext.GetService<IHttpContextAccessor>().HttpContext;
            IDNTCaptchaValidatorService validator = validationContext.GetService<IDNTCaptchaValidatorService>();
            string text = modelBase.CaptchaText;
            string token = modelBase.CaptchaToken;
            string input = modelBase.CaptchaInput;
            DNTCaptchaValidatorResult result = validator.Validate(httpContext, text, input, token, Language.English, "", "");
            if (result.IsValid)
            {
                modelBase.IsCaptchaValid = true;
                return ValidationResult.Success;
            }
            return new ValidationResult("Captcha error");
        }
    }

    /// <summary>
    /// Model with captcha
    /// </summary>
    public abstract class DataWithCaptchaModelBase
    {
        /// <summary>
        /// Token
        /// </summary>
        public string CaptchaToken { get; set; }

        /// <summary>
        /// Text
        /// </summary>
        public string CaptchaText { get; set; }

        /// <summary>
        /// User inputed captcha
        /// </summary>
        [ValidateCaptcha]
        public string CaptchaInput { get; set; }

        /// <summary>
        /// Is captcha valid
        /// </summary>
        public bool IsCaptchaValid { get; internal set; }
    }
}
