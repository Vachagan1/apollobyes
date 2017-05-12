using System.Web.Mvc;
using MvcAffableBean.Models;
using System.Web.Security;
using System;
using System.Linq;

namespace ProductStore.Controllers
{
    public class AccountsController : Controller, IDisposable
    {
        private ProductStoreContext customerDb = new ProductStoreContext();

        private void MigrateShoppingCart(string userName)
        {
            var cart = ShoppingCart.GetCart(HttpContext);

            cart.MigrateCart(userName);
            Session[ShoppingCart.CartSessionKey] = userName;
        }

        public ActionResult LogOn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var users = customerDb.Set<Customer>().Where(x => x.UserName == model.UserName && x.Password == model.Password).SingleOrDefault();
                if (users != null)
                {
                    MigrateShoppingCart(model.UserName);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }
            return View(model);
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            var cart = ShoppingCart.GetCart(this.HttpContext);
            cart.EmptyCart();

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var users = customerDb.Set<Customer>().Where(x => x.UserName == model.UserName || x.Email == model.Email).ToList();

                if (users != null && !users.Any())
                {
                    MigrateShoppingCart(model.UserName);
                    Customer newUser = new Customer() { UserName = model.UserName, Email = model.Email, Password = model.Password };
                    using (var tr = customerDb.Database.BeginTransaction())
                    {
                        try
                        {
                            customerDb.Dispose();
                            customerDb = new ProductStoreContext();
                            customerDb.Customers.Add(newUser);
                            customerDb.SaveChanges();
                        }
                        catch
                        {
                            tr.Rollback();
                        }
                    }
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "User name already exists. Please enter a different user name.");
                }
            }
            return View(model);
        }

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                bool changePasswordSucceeded = true;
                try
                {
                    var currentUser = customerDb.Set<Customer>().Where(x => x.UserName == model.UserName).SingleOrDefault();
                    if(currentUser != null && currentUser.Password == model.OldPassword)
                    {
                        currentUser.Password = model.NewPassword;
                        customerDb.Dispose();
                        customerDb = new ProductStoreContext();
                        customerDb.Customers.Attach(currentUser);
                        using(var tr = customerDb.Database.BeginTransaction())
                        {
                            try
                            {
                                customerDb.SaveChanges();
                            }
                            catch
                            {
                                tr.Rollback();
                            }
                        }

                    }else
                    {
                        changePasswordSucceeded = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }
            
            return View(model);
        }

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }


    }
}