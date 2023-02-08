// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VAC_T.Models;

namespace VAC_T.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<VAC_TUser> _userManager;
        private readonly SignInManager<VAC_TUser> _signInManager;

        public IndexModel(
            UserManager<VAC_TUser> userManager,
            SignInManager<VAC_TUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }
        public string Id { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            

            public string Name { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [DataType(DataType.EmailAddress)]
            public string Email { get; set; }

            [DataType(DataType.Date)]
            public DateTime BirthDate { get; set; }

            public string Address { get; set; }

            public string Postcode { get; set; }

            public string Residence { get; set; }

            [Display(Name = "Profile picture")]
            public string ProfilePicture { get; set; }

            public string Motivation { get; set; }

            public string CV { get; set; }

        }

        private async Task LoadAsync(VAC_TUser user)
        {
            var id = await _userManager.GetUserIdAsync(user);
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var name = user.Name;
            var address = user.Address;
            var profilePicture = user.ProfilePicture;
            var motivation = user.Motivation;
            var cV = user.CV;
            var postcode = user.Postcode;
            var residence = user.Residence;
            var birthDate = user.BirthDate;
            Username = userName;
            Id = id;

            Input = new InputModel
            {
                Name = name,
                Email= email,
                PhoneNumber = phoneNumber,
                Address = address,
                ProfilePicture = profilePicture,
                Motivation = motivation,
                CV = cV,
                Postcode = postcode,
                Residence = residence,
                BirthDate = (DateTime)birthDate

            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            var email = await _userManager.GetEmailAsync(user);
            if (Input.Email != email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, Input.Email);
                // way around to confirm the email.
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
                if (!setEmailResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set email.";
                    return RedirectToPage();
                }
            }

            if (Input.Name != user.Name)
            {
                user.Name = Input.Name;
                await _userManager.UpdateAsync(user);
            }

            if (Input.BirthDate != user.BirthDate)
            {
                user.BirthDate = Input.BirthDate;
                await _userManager.UpdateAsync(user);
            }

            if (Input.Address != user.Address)
            {
                user.Address = Input.Address;
                await _userManager.UpdateAsync(user);
            }

            if (Input.Postcode != user.Postcode)
            {
                user.Postcode = Input.Postcode;
                await _userManager.UpdateAsync(user);
            }

            if (Input.Residence != user.Residence)
            {
                user.Residence = Input.Residence;
                await _userManager.UpdateAsync(user);
            }


            if (Input.ProfilePicture!= null)
            {
                user.ProfilePicture = Input.ProfilePicture;
                await _userManager.UpdateAsync(user);
            }

            if (Input.Motivation!= null)
            {
                user.Motivation= Input.Motivation;
                await _userManager.UpdateAsync(user);
            }

            if (Input.CV!= null)
            {
                user.CV = Input.CV;
                await _userManager.UpdateAsync(user);
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
