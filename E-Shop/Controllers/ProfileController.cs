using DB;
using DB.Repo;
using E_Shop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Shop.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<APP_USER> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public ProfileController(UserManager<APP_USER> userManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? editAddressId = null)
        {
            var user = await GetCurrentUserAsync(includeAddresses: true);
            if (user == null)
            {
                return Unauthorized();
            }

            return View(MapProfileVM(user, editAddressId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileVM model)
        {
            var user = await GetCurrentUserAsync(includeAddresses: true);
            if (user == null)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                var invalidModel = MapProfileVM(user);
                invalidModel.FullName = model.FullName;
                invalidModel.Email = model.Email;
                invalidModel.PhoneNumber = model.PhoneNumber;
                return View("Index", invalidModel);
            }

            user.FullName = model.FullName.Trim();
            user.PhoneNumber = model.PhoneNumber;

            var newEmail = model.Email.Trim();
            if (!string.Equals(user.Email, newEmail, StringComparison.OrdinalIgnoreCase))
            {
                var emailResult = await _userManager.SetEmailAsync(user, newEmail);
                if (!emailResult.Succeeded)
                {
                    AddIdentityErrors(emailResult);
                    return View("Index", MapProfileVM(user));
                }

                var userNameResult = await _userManager.SetUserNameAsync(user, newEmail);
                if (!userNameResult.Succeeded)
                {
                    AddIdentityErrors(userNameResult);
                    return View("Index", MapProfileVM(user));
                }
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                AddIdentityErrors(updateResult);
                return View("Index", MapProfileVM(user));
            }

            TempData["ProfileMessage"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAddress([Bind(Prefix = "NewAddress")] ProfileAddressInputVM newAddress)
        {
            var user = await GetCurrentUserAsync(includeAddresses: true);
            if (user == null)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                var vm = MapProfileVM(user);
                vm.NewAddress = newAddress;
                return View("Index", vm);
            }

            if (newAddress.IsDefault)
            {
                foreach (var existingAddress in user.Addresses)
                {
                    existingAddress.IsDefault = false;
                }
            }

            user.Addresses.Add(new Address
            {
                UserId = user.Id,
                Country = newAddress.Country.Trim(),
                City = newAddress.City.Trim(),
                Street = newAddress.Street.Trim(),
                Zip = newAddress.Zip.Trim(),
                IsDefault = newAddress.IsDefault || !user.Addresses.Any()
            });

            await _unitOfWork.CompleteAsync();
            TempData["ProfileMessage"] = "Address added successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAddress([Bind(Prefix = "EditAddress")] ProfileAddressInputVM editAddress)
        {
            var user = await GetCurrentUserAsync(includeAddresses: true);
            if (user == null)
            {
                return Unauthorized();
            }

            if (!editAddress.AddressId.HasValue)
            {
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                var vm = MapProfileVM(user, editAddress.AddressId);
                vm.EditAddress = editAddress;
                vm.EditAddressId = editAddress.AddressId;
                return View("Index", vm);
            }

            var address = user.Addresses.FirstOrDefault(a => a.AddressId == editAddress.AddressId.Value);
            if (address == null)
            {
                return NotFound();
            }

            if (editAddress.IsDefault)
            {
                foreach (var existingAddress in user.Addresses)
                {
                    existingAddress.IsDefault = existingAddress.AddressId == address.AddressId;
                }
            }

            address.Country = editAddress.Country.Trim();
            address.City = editAddress.City.Trim();
            address.Street = editAddress.Street.Trim();
            address.Zip = editAddress.Zip.Trim();

            if (editAddress.IsDefault)
            {
                address.IsDefault = true;
            }

            await _unitOfWork.CompleteAsync();
            TempData["ProfileMessage"] = "Address updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAddress(int addressId)
        {
            var user = await GetCurrentUserAsync(includeAddresses: true);
            if (user == null)
            {
                return Unauthorized();
            }

            var address = user.Addresses.FirstOrDefault(a => a.AddressId == addressId);
            if (address == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var wasDefault = address.IsDefault;
            _unitOfWork.Addresses.Remove(address);
            await _unitOfWork.CompleteAsync();

            if (wasDefault)
            {
                var userAddresses = await _unitOfWork.Addresses.FindAsync(a => a.UserId == user.Id);
                var nextAddress = userAddresses.OrderBy(a => a.AddressId).FirstOrDefault();

                if (nextAddress != null)
                {
                    nextAddress.IsDefault = true;
                    _unitOfWork.Addresses.Update(nextAddress);
                    await _unitOfWork.CompleteAsync();
                }
            }

            TempData["ProfileMessage"] = "Address deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDefaultAddress(int addressId)
        {
            var user = await GetCurrentUserAsync(includeAddresses: true);
            if (user == null)
            {
                return Unauthorized();
            }

            var address = user.Addresses.FirstOrDefault(a => a.AddressId == addressId);
            if (address == null)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var existingAddress in user.Addresses)
            {
                existingAddress.IsDefault = existingAddress.AddressId == addressId;
            }

            await _unitOfWork.CompleteAsync();
            TempData["ProfileMessage"] = "Default address updated.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<APP_USER?> GetCurrentUserAsync(bool includeAddresses)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }

            if (!includeAddresses)
            {
                return await _userManager.FindByIdAsync(userId);
            }

            return await _userManager.Users
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        private static ProfileVM MapProfileVM(APP_USER user, int? editAddressId = null)
        {
            var vm = new ProfileVM
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                EditAddressId = editAddressId,
                Addresses = user.Addresses
                    .OrderByDescending(a => a.IsDefault)
                    .ThenBy(a => a.AddressId)
                    .Select(a => new ProfileAddressVM
                    {
                        AddressId = a.AddressId,
                        Country = a.Country,
                        City = a.City,
                        Street = a.Street,
                        Zip = a.Zip,
                        IsDefault = a.IsDefault
                    })
                    .ToList()
            };

            if (editAddressId.HasValue)
            {
                var address = user.Addresses.FirstOrDefault(a => a.AddressId == editAddressId.Value);
                if (address != null)
                {
                    vm.EditAddress = new ProfileAddressInputVM
                    {
                        AddressId = address.AddressId,
                        Country = address.Country,
                        City = address.City,
                        Street = address.Street,
                        Zip = address.Zip,
                        IsDefault = address.IsDefault
                    };
                }
            }

            return vm;
        }

        private void AddIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
