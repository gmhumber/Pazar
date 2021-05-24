using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pazar.Models;
using Pazar.Models.Viewmodels;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Web.Script.Serialization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.IO;



namespace Pazar.Controllers
{
    public class PazarViewController : Controller
    {

        // A large portion of the code in this project can be attributed to the Varsity project by Christine Bittle, retrieved in Februrary 2021
        // Use an instance of HttpClient class to interface with our APIs and JavaScriptSerializer to parse JSON data passed between the view and API controllers.

        private JavaScriptSerializer jss = new JavaScriptSerializer();
        private static readonly HttpClient client;

        static PazarViewController()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false
            };
            client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://localhost:44383/api/");
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // Code below was retained for further investigation; likely part of a security mechanism by the API controller to restict access to authenticated clients. 
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ACCESS_TOKEN);

        }


        /// <summary>
        /// The List method obtains a collection of all current listings through a call to an API controller, then passes the returned data to the View for display.
        /// </summary>
        /// <returns>A view with all ad listings.</returns>
        // GET: PazarView/List
        [AllowAnonymous]
        public ActionResult List()
        {

            string url = "pazarapi/getallads";
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                IEnumerable<AdDTO> AdsList = response.Content.ReadAsAsync<IEnumerable<AdDTO>>().Result;
                return View(AdsList);
            }
            else
            {
                return View("Error");
            }
        }

        /// <summary>
        /// The Search method takes the search parameter as an input string, then performs a search aginst the current listings by making a call to an API controller. The search results are returned as a collection which is then passed to the View for display.
        /// </summary>
        /// <param name="queryParameter">A string representing the search parameter.</param>
        /// <returns>A view with all of the search results.</returns>
        // GET: PazarView/Search
        [AllowAnonymous]
        public ActionResult Search(string queryParameter = null)
        {

            if (queryParameter.Length == 0 || queryParameter == null)
            {
                return Redirect("List");
            } 
            else
            {
                string url = $"pazarapi/search/{queryParameter}";
                HttpResponseMessage response = client.GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                {
                    IEnumerable<AdDTO> AdsList = response.Content.ReadAsAsync<IEnumerable<AdDTO>>().Result;
                    return View(AdsList);
                }
                else
                {
                    return View("Error");
                }
            }
            

        }

        /// <summary>
        /// The GET Create method obtains data of all available lising categories and types by making calls to the API controllers, then passes the returned data to the View so that a form can be rendered that will take in information necessary to create a new ad lising. Access to this method is restricted.
        /// </summary>
        /// <returns>A view with a form for creating a new ad listing.</returns>
        // GET: PazarView/Create
        [Authorize(Roles = "User, Admin")]
        public ActionResult Create()
        {
            AdCrudModel ViewModel = new AdCrudModel();

            string url = "pazarapi/getallcategories";
            HttpResponseMessage response = client.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {
                ViewModel.allCatagories = response.Content.ReadAsAsync<IEnumerable<CategoryDTO>>().Result;

                url = "pazarapi/getalltypes";
                response = client.GetAsync(url).Result;

                ViewModel.allTypes = response.Content.ReadAsAsync<IEnumerable<TypeDTO>>().Result;

                return View(ViewModel);

            }
            else 
            {
                return View("Error");
            }
        }


        /// <summary>
        /// The POST Create method takes in data from the request body using the POST method necessary to create a new ad listing. Additional data such as the requesting user's ID and the timestamp are added, then the entire set of data is bundled in a AdModel object and passed to the API controller to create the new ad listing. Access to this method is restricted.
        /// </summary>
        /// <param name="NewAd">An AdModel object containing the received form data.</param>
        /// <returns>Redirect to a view listing all of the logged in user's ad listings.</returns>
        // POST: PazarView/Create
        [HttpPost]
        [Authorize(Roles = "User, Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AdModel NewAd)
        {
            // Assign outstanding values to the model
            NewAd.ApplicationUserId = User.Identity.GetUserId();
            NewAd.Timestamp = DateTime.Now;

            // Stringify JSON data and send the create request to the API controller using POST method
            string url = "pazarapi/addad";
            HttpContent content = new StringContent(jss.Serialize(NewAd));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = client.PostAsync(url, content).Result;

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("ShowUserAds");
            }
            else
            {
                return View("Error");
            }
        }

        /// <summary>
        /// The ShowUserAds method obtains all ad listings that a logged-in user has control over by making a request to an API controller method with the requesting user's particulars. Data is returned as a collection which is then passed to the View for display. Access to this method is restricted.
        /// </summary>
        /// <returns>A view showing all of the listings that the logged in user can modify.</returns>
        // GET: PazarView/ShowUserAds
        [Authorize(Roles = "User, Admin")]
        public ActionResult ShowUserAds()
        {
            // Get the logged in user's ID and role (NOTE: must investigate a more elegant way to obtain the user's roles)
            string userId = User.Identity.GetUserId();
            bool userIsAdmin = User.IsInRole("Admin");
            string userRole = userIsAdmin ? "Admin" : "User";

            // Pass the data for user authenication to the API controller, along with the request to obtain the listings that the user can modify
            string url = $"pazarapi/getuserads/{userId}/{userRole}";
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                IEnumerable<AdDTO> UserAdsList = response.Content.ReadAsAsync<IEnumerable<AdDTO>>().Result;
                
                return View(UserAdsList);
            }
            else
            {
                return View("Error");
            }
        }


        /// <summary>
        /// The DeleteConfirm method receives an ID number representing an ad listing to be deleted. This method obtains the ad listing's particulars from an API controller method then returns the data to teh View for display. The purpose is to allow the user to confirm the listing to be deleted. Access to this method is restricted.
        /// </summary>
        /// <param name="id">ID number of the ad listing to be deleted</param>
        /// <returns>A view showing the listing entry to be deleted to prompt the user for confirmation to delete</returns>
        // GET: PazarView/DeleteCofirm/5
        [Authorize(Roles = "User, Admin")]
        public ActionResult DeleteConfirm(int id)
        {
            string url = $"pazarapi/getonead/{id}";
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
             
                AdDTO OneAd = response.Content.ReadAsAsync<AdDTO>().Result;

                string userId = User.Identity.GetUserId();
                bool userIsAdmin = User.IsInRole("Admin");
                string userRole = userIsAdmin ? "Admin" : "User";

                if (userId != OneAd.ApplicationUserId && userRole != "Admin")
                {
                    return View("Error");
                }
                else
                {
                    return View(OneAd);
                }
            }
            else
            {
                return View("Error");
            }
        }

        /// <summary>
        /// The Delete method receives an ID number representing an ad listing to be deleted. The ID is passed to an API controller method and the ad listing which deletes the listing. Access to this method is restricted.
        /// </summary>
        /// <param name="id">ID number of the ad listing to be deleted</param>
        /// <returns>If deletion was successful, return a view showing the logged in user's current listings</returns>
        // POST: PazarView/Delete/5
        [HttpPost]
        [Authorize(Roles = "User, Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            string userId = User.Identity.GetUserId();
            bool userIsAdmin = User.IsInRole("Admin");
            string userRole = userIsAdmin ? "Admin" : "User";

            string url = $"pazarapi/deletead/{id}/{userId}/{userRole}";
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                
                return RedirectToAction("ShowUserAds");
            }
            else
            {
                return View("Error");
            }
        }


        /// <summary>
        /// The Edit method receives an ID number representing an ad listing to be edited. This method obtains the ad listing's particulars, all currently available listing types and categories from API controller methods then returns the data to the View for display. The purpose is to display the current listing and allow the user to confirm the listing properties to be edited. Access to this method is restricted.
        /// </summary>
        /// <param name="id">ID number of the ad listing to be edited</param>
        /// <returns>A form prompting the user for changes to be made to a listing</returns>
        // GET: PazarView/Edit/5
        [Authorize(Roles = "User, Admin")]
        public ActionResult Edit(int id)
        {
            // Get logged in user ID and confirm whether user has Admin privilages
            string userId = User.Identity.GetUserId();
            bool userIsAdmin = User.IsInRole("Admin");
            string userRole = userIsAdmin ? "Admin" : "User";

            AdCrudModel ViewModel = new AdCrudModel();

            AdDTO EditAd = new AdDTO();

            // Make async requests to gather database data that will be used to populate the HTML form
            string urlGetAllCategories = "pazarapi/getallcategories";
            HttpResponseMessage responseGetAllCategories = client.GetAsync(urlGetAllCategories).Result;

            string urlGetAllTypes = "pazarapi/getalltypes";
            HttpResponseMessage responseGetAllTypes = client.GetAsync(urlGetAllTypes).Result;

            string urlGetEditAd = $"pazarapi/getonead/{id}";
            HttpResponseMessage responseGetEditAd = client.GetAsync(urlGetEditAd).Result;

            // If all async requests are successful, then bind the returned values with the ViewModel object and pass it to the View, otherwise return the error page
            if (responseGetAllCategories.IsSuccessStatusCode && responseGetAllTypes.IsSuccessStatusCode && responseGetEditAd.IsSuccessStatusCode)
            {
                ViewModel.allCatagories = responseGetAllCategories.Content.ReadAsAsync<IEnumerable<CategoryDTO>>().Result;

                ViewModel.allTypes = responseGetAllTypes.Content.ReadAsAsync<IEnumerable<TypeDTO>>().Result;

                EditAd = responseGetEditAd.Content.ReadAsAsync<AdDTO>().Result;

                if (userId != EditAd.ApplicationUserId && userRole != "Admin")
                {
                    return View("Error");
                }
                else
                {
                    ViewBag.EditAd = EditAd;
                    return View(ViewModel);
                }

            }
            else
            {
                return View("Error");
            }

        }

        /// <summary>
        /// The Edit method receives an ID number representing an ad listing to be edited and an AdModel object representing the listing properties to be changed. The AdModel object is passed to an API controller method which effects the changes in the database. Access to this method is restricted.
        /// </summary>
        /// <param name="adId">ID number of the ad listing to be edited</param>
        /// <param name="EditedAd">AdModel object containing the user's requested changes to the ad listing.</param>
        /// <returns>If edit action was successful, then return a view containing the logged in user's existing listings</returns>
        // POST: PazarView/Edit/5
        [HttpPost]
        [Authorize(Roles = "User, Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int adId, AdModel EditedAd, HttpPostedFileBase ListingPic)
        {
            if (!ModelState.IsValid) 
            {
                return View("Error");
            }
            
            // Get ID and role of the logged in user, then pass it to the API controller for authentication purposes
            string userId = User.Identity.GetUserId();
            bool userIsAdmin = User.IsInRole("Admin");
            string userRole = userIsAdmin ? "Admin" : "User";

            // Assign the current datetime value to the Model
            EditedAd.Timestamp = DateTime.Now;

            // Pass the Model to the API controller as a JSON string to be used for updating the relevant database entry
            string url = $"pazarapi/updatead/{adId}/{userId}/{userRole}";
            HttpContent content = new StringContent(jss.Serialize(EditedAd));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = client.PostAsync(url, content).Result;

            if (!response.IsSuccessStatusCode)
            {
                return View("Error");
            }
            else 
            {
                try
                {
                    // Send image-related data to the API controller for insertion into the database
                    url = $"pazarapi/insertimagetoad/{adId}/{userId}/{userRole}";
                    //Debug.WriteLine("Received player picture " + PlayerPic.FileName);
                    MultipartFormDataContent requestcontent = new MultipartFormDataContent();
                    HttpContent imageContent = new StreamContent(ListingPic.InputStream);
                    requestcontent.Add(imageContent, "ListingPic", ListingPic.FileName);
                    response = client.PostAsync(url, requestcontent).Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        return View("Error");
                    }
                    else
                    {
                        return RedirectToAction("ShowUserAds");
                    }
                }
                catch
                {
                    Debug.WriteLine("No image received as part of the user's update request.");
                    return RedirectToAction("ShowUserAds");
                }

                    
            }

        }

        /// <summary>
        /// The AdminShowEditCategories method obtains all available ad lisitng categories from an API controller method then returns the data to the View for display. The purpose is to allow the admin to see the categories and edit them if needed. Access to this method is restricted.
        /// </summary>
        /// <returns>A view containing all listing categories</returns>
        // GET: PazarView/AdminShowCategories
        [Authorize(Roles = "Admin")]
        public ActionResult AdminShowEditCategories()
        {
            AdCrudModel ViewModel = new AdCrudModel();

            string url = "pazarapi/getallcategories";
            HttpResponseMessage response = client.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {
                ViewModel.allCatagories = response.Content.ReadAsAsync<IEnumerable<CategoryDTO>>().Result;

                return View(ViewModel);

            }
            else
            {
                return View("Error");
            }
        }


        /// <summary>
        /// The AdminShowEditTypes method obtains all available ad lisitng types from an API controller method then returns the data to the View for display. The purpose is to allow the admin to see the types and edit them if needed. Access to this method is restricted.
        /// </summary>
        /// <returns>A view containing all listing types</returns>
        // GET: PazarView/AdminShowEditTypes
        [Authorize(Roles = "Admin")]
        public ActionResult AdminShowEditTypes()
        {
            AdCrudModel ViewModel = new AdCrudModel();

            string url = "pazarapi/getalltypes";
            HttpResponseMessage response = client.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {
                ViewModel.allTypes = response.Content.ReadAsAsync<IEnumerable<TypeDTO>>().Result;

                return View(ViewModel);
            }
            else
            {
                return View("Error");
            }
        }

        /// <summary>
        /// The AdminEditCategory method receives a CategoryModel object as the input which represents the ad listing catagory to be edited. That object is passed to an API controller method as JSON data using a POST method which then effects the changes. Access to this method is restricted.
        /// </summary>
        /// <param name="EditedCategory">CategoryModel object which represents the ad listing catagory to be edited. The data is received using the POST method as part of the request body.</param>
        /// <returns>A view of the admin user's dashboard</returns>
        // POST: PazarView/AdminEditCategory
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult AdminEditCategory(CategoryModel EditedCategory)
        {
            if (!ModelState.IsValid)
            {
                return View("Error");
            }

            string url = "pazarapi/updatecategory";
            HttpContent content = new StringContent(jss.Serialize(EditedCategory));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = client.PostAsync(url, content).Result;

            if (!response.IsSuccessStatusCode)
            {
                return View("Error");
            }
            else
            {
                return RedirectToAction("Index", "Manage");
            }

        }


        /// <summary>
        /// The AdminCreateCategory method generates a View for an admin to create new ad listing categories. Access to this method is restricted.
        /// </summary>
        /// <returns>A form for an admin user to create new listing categories</returns>
        // GET: PazarView/AdminCreateCategory
        [Authorize(Roles = "Admin")]
        public ActionResult AdminCreateCategory()
        {
            return View();
        }


        /// <summary>
        /// The AdminCreateCategory method receives a CategoryModel object as the input which represents the ad listing catagory to be created. That object is passed to an API controller method as JSON data using a POST method which then effects the changes. Access to this method is restricted.
        /// </summary>
        /// <param name="newCategory">CategoryModel object which represents the ad listing catagory to be created. Recevied as POST data from the request body.</param>
        /// <returns>If a listing category was successfully added, then return a view of all the listing categories</returns>
        // POST: PazarView/AdminCreateCategory
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult AdminCreateCategory(CategoryModel newCategory)
        {

            string url = "pazarapi/addcategory";
            HttpContent content = new StringContent(jss.Serialize(newCategory));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = client.PostAsync(url, content).Result;

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminShowEditCategories");
            }
            else
            {
                return View("Error");
            }
        }

        /// <summary>
        /// The AdminEditType method receives a TypeModel object as the input which represents the ad listing type to be edited. That object is passed to an API controller method as JSON data using a POST method which then effects the changes. Access to this method is restricted.
        /// </summary>
        /// <param name="EditedType">TypeModel object which represents the ad listing type to be edited. The data is received using the POST method as part of the request body.</param> 
        /// <returns>Redirect to the admin user's dashboard if a new lisitng type was successfully edited</returns>
        // POST: PazarView/AdminEditTypes
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult AdminEditTypes(TypeModel EditedType)
        {
            if (!ModelState.IsValid)
            {
                return View("Error");
            }

            string url = "pazarapi/updatetype";
            HttpContent content = new StringContent(jss.Serialize(EditedType));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = client.PostAsync(url, content).Result;

            if (!response.IsSuccessStatusCode)
            {
                return View("Error");
            }
            else
            {
                return RedirectToAction("Index", "Manage");
            }

        }


        /// <summary>
        /// The AdminCreateType method generates a View for an admin to create new ad listing types. Access to this method is restricted.
        /// </summary>
        /// <returns>A view with a form for an admin user to create a new listing type</returns>
        // GET: PazarView/AdminCreateType
        [Authorize(Roles = "Admin")]
        public ActionResult AdminCreateType()
        {
            return View();
        }


        /// <summary>
        /// The AdminCreateType method receives a TypeModel object as the input which represents the ad listing type to be created. That object is passed to an API controller method as JSON data using a POST method which then effects the changes. Access to this method is restricted.
        /// </summary>
        /// <param name="newType">TypeModel object which represents the ad listing type to be created. Recevied as POST data from the request body.</param>
        /// <returns>If a new lisitng type was successfully created, then redirect user to a view of all current listing types</returns>
        // POST: PazarView/AdminCreateType
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult AdminCreateType(TypeModel newType)
        {

            string url = "pazarapi/addtype";
            HttpContent content = new StringContent(jss.Serialize(newType));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = client.PostAsync(url, content).Result;

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminShowEditTypes");
            }
            else
            {
                return View("Error");
            }
        }


        /// <summary>
        /// This AdminDeleteTypes method is used to delete a listing type from the database. It accepts as input the ID of the type to be deleted then passes that data to the API controller method which deletes the entry.
        /// </summary>
        /// <param name="TypeId">ID of the listing type to be deleted, received via the POST method.</param>
        /// <returns>If a listing type was successfully deleted, then return a view listing all then current listing types</returns>
        // POST: PazarView/AdminDeleteTypes
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        [Route("pazarview/admindeletetypes")]
        public ActionResult AdminDeleteTypes(int TypeId)
        {

            string url = $"pazarapi/deletetype/{TypeId}";
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {

                return RedirectToAction("AdminShowEditTypes");
            }
            else
            {
                return View("Error");
            }
        }

        /// <summary>
        /// This AdminDeleteCategory method is used to delete a listing category from the database. It accepts as input the ID of the category to be deleted then passes that data to the API controller method which deletes the entry.
        /// </summary>
        /// <param name="CategoryId">ID of the listing category to be deleted, received via the POST method.</param>
        /// <returns>If a listing category was successfully deleted, then return a view containing all of the then current listing categories</returns>
        // POST: PazarView/AdminDeleteCategory
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        [Route("pazarview/admindeletecategory")]
        public ActionResult AdminDeleteCategory(int CategoryId)
        {

            string url = $"pazarapi/deletecategory/{CategoryId}";
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {

                return RedirectToAction("AdminShowEditCategories");
            }
            else
            {
                return View("Error");
            }
        }


        /// <summary>
        /// The Details method takes in an integer representing the Id of an ad lisitng, and then obtains the data for that listing by making a call to the API controllers. It then returns a View containing that data.
        /// </summary>
        /// <param name="id">Id of an ad listing.</param>
        /// <returns>A View containing the details of an ad listing.</returns>
        /// <returns>A view with the details of an ad listing and image if applicable</returns>
        // GET: PazarView/Details/5
        public ActionResult Details(int id)
        {
            string url = $"pazarapi/getonead/{id}";
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {

                AdDTO OneAd = response.Content.ReadAsAsync<AdDTO>().Result;

                string imageFileName = Path.GetFileName(OneAd.ImagePath);
                //Debug.WriteLine(imageFileName);
                //Debug.WriteLine(imageFileName.GetType());
                //Debug.WriteLine(typeof(string));
                //Debug.WriteLine(System.IO.File.Exists(OneAd.ImagePath));


                // Evaluate the data type of imageFileName. If it fails, then no image path was stored in the db and therefore we infer that no image is available for that listing.
                try
                {
                    imageFileName.GetType();
                }
                catch
                {
                    imageFileName = "no_image_available";
                }
                
                ViewBag.imageFileName = imageFileName;
                return View(OneAd);

            }
            else
            {
                return View("Error");
            }

        }


    }
}
