﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using ShoppingCart.Data;
using ShoppingCart.Models;

namespace ShoppingCart.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ShoppingCartContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RegisterModel(
            UserManager<User> userManager,
            IUserStore<User> userStore,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ShoppingCartContext context,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public SelectList IdentificationTypes { get; set; }
        public SelectList UserAddressProvinces { get; set; }
        public SelectList UserAddressCities { get; set; }

        public IActionResult OnGetUserAddressCitiesById(int selectedUserAddressProvinceId)
        {
            
            var userAddressCities = new SelectList(_context.UserAddressCities.Where(x => x.UserAddressProvinceId == selectedUserAddressProvinceId).Select(x => new SelectListItem { Value = x.UserAddressCityId.ToString(), Text = x.City }).ToList(), "Value", "Text");

            return new JsonResult(userAddressCities);
            //return new JsonResult(new SelectList(_context.UserAddressCities.Where(x => x.UserAddressProvinceId == selectedUserAddressProvinceId), "UserAddressCityId", "City"));
        }

        public class InputModel
        {
            [Display(Name = "Primer nombre")]
            [Required(ErrorMessage ="Debe ingresar su primer nombre")]
            [StringLength(50, MinimumLength = 2, ErrorMessage = "Debes ingresar como mínimo {2} y máximo {1} caracteres")]
            public string FirstName { get; set; } = null!;

            [Display(Name = "Primer apellido")]
            [Required(ErrorMessage = "Debe ingresar su primer apellido")]
            [StringLength(50, MinimumLength = 2, ErrorMessage = "Debes ingresar como mínimo {2} y máximo {1} caracteres")]
            public string LastName { get; set; } = null!;

            [Display(Name = "Tipo de identificación")]
            [Required(ErrorMessage = "Debe seleccionar su tipo de identificación")]
            public int IdentificationTypeId { get; set; }

            [Display(Name = "Número de identificación (en el caso del pasaporte recuerde ingresar las letras también)")]
            [Required(ErrorMessage = "Debe ingresar su número de identificación")]
            [StringLength(50, MinimumLength = 2, ErrorMessage = "Debes ingresar como mínimo {2} y máximo {1} caracteres")]
            public string Identification { get; set; } = null!;

            [Display(Name = "Número de contacto")]
            [Required(ErrorMessage = "Debe ingresar su número de contacto")]
            [DataType(DataType.PhoneNumber)]
            [StringLength(50, MinimumLength = 6, ErrorMessage = "Debes ingresar como mínimo {2} y máximo {1} caracteres")]
            //[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Número de contacto inválido")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Dirección")]
            [Required(ErrorMessage = "Debe ingresar su dirección")]
            [StringLength(50, MinimumLength = 2, ErrorMessage = "Debes ingresar como mínimo {2} y máximo {1} caracteres")]
            public string Address { get; set; } = null!;

            [Display(Name = "Provincia o departamento")]
            [Required(ErrorMessage = "Debe seleccionar una provincia o departamento")]
            public int UserAddressProvinceId { get; set; }

            [Display(Name = "Ciudad")]
            [Required(ErrorMessage = "Debe seleccionar una ciudad escogiendo primero una provincia o departamento")]
            public int UserAddressCityId { get; set; }

            [Display(Name = "Código postal")]
            [Required(ErrorMessage = "Debe ingresar su código postal")]
            [StringLength(50, MinimumLength = 2, ErrorMessage = "Debes ingresar como mínimo {2} y máximo {1} caracteres")]
            public string PostalCode { get; set; } = null!;

            [Display(Name = "Correo electrónico")]
            [Required(ErrorMessage = "Debe ingresar su correo electrónico")]
            [DataType(DataType.EmailAddress)]
            [EmailAddress(ErrorMessage = "Correo electrónico inválido")]
            public string Email { get; set; }

            [Display(Name = "Contraseña")]
            [Required(ErrorMessage = "Debe ingresar una contraseña")]
            [DataType(DataType.Password)]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$", ErrorMessage = "La contraseña debe ser de mínimo 6 caracteres y debe incluir al menos una letra mayúscula, una letra minúscula, un caracter no alfanúmerico y un número")]
            [StringLength(20, MinimumLength = 6, ErrorMessage = "Debes ingresar como mínimo {2} y máximo {1} caracteres")]
            public string Password { get; set; }

            [Display(Name = "Confirmar contraseña")]
            [DataType(DataType.Password)]
            [Required(ErrorMessage = "Debe confirmar la contraseña")]
            [Compare("Password", ErrorMessage = "La contraseña y la confirmación de contraseña no coinciden")]
            [StringLength(20, MinimumLength = 6)]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            IdentificationTypes = new SelectList(_context.IdentificationTypes, "IdentificationTypeId", "Type");
            UserAddressProvinces = new SelectList(_context.UserAddressProvinces, "UserAddressProvinceId", "Province");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                user.Identification = Input.Identification;
                user.Address = Input.Address;
                user.PostalCode = Input.PostalCode;

                user.IdentificationTypeId = Input.IdentificationTypeId;
                user.UserAddressProvinceId = Input.UserAddressProvinceId;
                user.UserAddressCityId = Input.UserAddressCityId;

                user.CreationDate = DateTime.Now;
                user.ModificationDate = DateTime.Now;

                await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);
                await _userManager.AddToRoleAsync(user, Data.Enums.Roles.Client.ToString());

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private User CreateUser()
        {
            try
            {
                return Activator.CreateInstance<User>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(User)}'. " +
                    $"Ensure that '{nameof(User)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<User>)_userStore;
        }
    }
}