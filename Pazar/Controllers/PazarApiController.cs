using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Pazar.Models;
using System.Diagnostics;
using System.IO;
using System.Web;


namespace Pazar.Controllers
{
    public class PazarApiController : ApiController
    {

        // A large portion of the code in this project can be attributed to the Varsity project by Christine Bittle, retrieved in Februrary 2021

        // Create a new database context for interfacing with the db
        private ApplicationDbContext db = new ApplicationDbContext();


        /// <summary>
        /// The GetAllAds method accesses the database to obtain a collection of all current ad listings and return the collection.
        /// </summary>
        /// <returns>List of ad listing objects containing all current ad listings</returns>
        // GET: api/PazarApi/GetAllAds
        [ResponseType(typeof(IEnumerable<AdModel>))]
        public IHttpActionResult GetAllAds()
        {
            List<AdModel> AllAdsList = db.Ads.ToList();
            List<AdDTO> AllAdsDTOList = new List<AdDTO> { };

            foreach (var element in AllAdsList)
            {
                AdDTO NewAd = new AdDTO
                {
                    AdId = element.AdId,
                    ApplicationUserId = element.ApplicationUserId,
                    CategoryId = element.CategoryId,
                    Category = db.Categories.Find(element.CategoryId).Category,
                    TypeId = element.TypeId,
                    Type = db.Types.Find(element.TypeId).Type,
                    Title = element.Title,
                    Description = element.Description,
                    Price = element.Price,
                    Timestamp = element.Timestamp,
                    Location = element.Location
                };
                AllAdsDTOList.Add(NewAd);
            }
            return Ok(AllAdsDTOList);
        }


        /// <summary>
        /// The Search method performs a query against all ad listings in the database to find entities that match with the query parameter that is recieved. The resulting data forms a collection which is then returned.
        /// </summary>
        /// <param name="queryParameter">Search parameters received as part of the query string.</param>
        /// <returns>A list of ad listing objects that matches the search query</returns>
        // GET: api/PazarApi/Search
        [HttpGet]
        [ResponseType(typeof(IEnumerable<AdModel>))]
        [Route("api/pazarapi/search/{queryParameter}")]
        public IHttpActionResult Search(string queryParameter)
        {
            List<AdModel> SearchedAdsList = db.Ads
                                       .Where(oneAd => 
                                        oneAd.Title.Contains(queryParameter) || 
                                        oneAd.Description.Contains(queryParameter))
                                       .ToList();

            List<AdDTO> SearchedAdsDTOList = new List<AdDTO> { };

            foreach (var element in SearchedAdsList)
            {
                AdDTO NewAd = new AdDTO
                {
                    AdId = element.AdId,
                    ApplicationUserId = element.ApplicationUserId,
                    CategoryId = element.CategoryId,
                    Category = db.Categories.Find(element.CategoryId).Category,
                    TypeId = element.TypeId,
                    Type = db.Types.Find(element.TypeId).Type,
                    Title = element.Title,
                    Description = element.Description,
                    Price = element.Price,
                    Timestamp = element.Timestamp,
                    Location = element.Location
                };
                SearchedAdsDTOList.Add(NewAd);
            }
            return Ok(SearchedAdsDTOList);
        }


        /// <summary>
        /// The GetAllCategories method gets all current ad listing categories from the database and reutrn them as a List collection.
        /// </summary>
        /// <returns>A list of all current listing categories</returns>
        // GET: api/PazarApi/GetAllCategories
        [ResponseType(typeof(IEnumerable<CategoryDTO>))]
        public IHttpActionResult GetAllCategories()
        {
            List<CategoryModel> AllCategoriesList = db.Categories.ToList();
            List<CategoryDTO> AllCategoriesDTOList = new List<CategoryDTO> { };

            foreach (var element in AllCategoriesList)
            {
                CategoryDTO NewCategory = new CategoryDTO
                {
                    CategoryId = element.CategoryId,
                    Category = element.Category
                };

                AllCategoriesDTOList.Add(NewCategory);
            }
            return Ok(AllCategoriesDTOList);
        }


        /// <summary>
        /// The GetAllTypes method gets all current ad listing types from the database and reutrn them as a List collection.
        /// </summary>
        /// <returns>A list of all listing types</returns>
        // GET: api/PazarApi/GetAllTypes
        [ResponseType(typeof(IEnumerable<TypeDTO>))]
        public IHttpActionResult GetAllTypes()
        {
            List<TypeModel> AllTypesList = db.Types.ToList();
            List<TypeDTO> AllTypesDTOList = new List<TypeDTO> { };

            foreach (var element in AllTypesList)
            {
                TypeDTO NewType = new TypeDTO
                {
                    TypeId = element.TypeId,
                    Type = element.Type
                };

                AllTypesDTOList.Add(NewType);
            }
            return Ok(AllTypesDTOList);
        }


        /// <summary>
        /// The AddAd method accepts an AdModel object as an input via the POST method which represents a new ad listing to be added to the database.
        /// </summary>
        /// <param name="NewAd">AdModel object which represents a new ad listing to be added to the database</param>
        /// <returns>200 status code if ad listing was successfully added</returns>
        // POST: api/PazarApi/AddAd
        [HttpPost]
        public IHttpActionResult AddAd([FromBody] AdModel NewAd)
        {
            //Debug.WriteLine(NewAd.Title);
            //Will Validate according to data annotations specified on model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Ads.Add(NewAd);
            db.SaveChanges();

            return Ok();
        }


        /// <summary>
        /// The GetUserAds method retrieves the ad listings that a user has control over and then returns it as a List collection.
        /// </summary>
        /// <param name="userId">ID of the requesting user.</param>
        /// <param name="userRole">User role of the requesting user.</param>
        /// <returns>A list of ad lisitng objects controlled by a user, or 404 if the request is denied</returns>
        // GET: api/PazarApi/GetUserAds
        [ResponseType(typeof(IEnumerable<AdModel>))]
        [Route("api/pazarapi/getuserads/{userId}/{userRole}")]
        public IHttpActionResult GetUserAds(string userId, string userRole)
        {

            List<AdModel> UserAdsList;

            // Admins are entited to view all ad listings; regular users are only permitted to review their own listings and no others.
            if (userRole == "User")
            {
                UserAdsList = db.Ads
                    .Where(a => a.ApplicationUserId == userId)
                    .ToList();
            } 
            else if (userRole == "Admin")
            {
                UserAdsList = db.Ads.ToList();
            }
            else
            {
                return NotFound();
            }

            // Use a DTO to trasnfer the data back to the View controller.
            List<AdDTO> UserAdsDTOList = new List<AdDTO> { };

            foreach (var element in UserAdsList)
            {
                AdDTO UserAd = new AdDTO
                {
                    AdId = element.AdId,
                    ApplicationUserId = element.ApplicationUserId,
                    CategoryId = element.CategoryId,
                    Category = db.Categories.Find(element.CategoryId).Category,
                    TypeId = element.TypeId,
                    Type = db.Types.Find(element.TypeId).Type,
                    Title = element.Title,
                    Description = element.Description,
                    Price = element.Price,
                    Timestamp = element.Timestamp,
                    Location = element.Location
                };
                UserAdsDTOList.Add(UserAd);
            }
            return Ok(UserAdsDTOList);
        }


        /// <summary>
        /// The GetOneAd method retrieves a single ad listing from the database based on the listing's ID number. The ad listing is returned.
        /// </summary>
        /// <param name="id">ID number of the ad listing to be retrived from the database.</param>
        /// <returns>A single ad listing object</returns>
        // GET: api/PazarApi/GetOneAd/5
        [ResponseType(typeof(AdDTO))]
        public IHttpActionResult GetOneAd(int id)
        {
            AdModel OneAd = new AdModel();
                
            OneAd = db.Ads.Find(id);

            AdDTO OneAdDTO = new AdDTO();

            OneAdDTO.AdId = OneAd.AdId;
            OneAdDTO.ApplicationUserId = OneAd.ApplicationUserId;
            OneAdDTO.CategoryId = OneAd.CategoryId;
            OneAdDTO.Category = db.Categories.Find(OneAd.CategoryId).Category;
            OneAdDTO.TypeId = OneAd.TypeId;
            OneAdDTO.Type = db.Types.Find(OneAd.TypeId).Type;
            OneAdDTO.Title = OneAd.Title;
            OneAdDTO.Description = OneAd.Description;
            OneAdDTO.Price = OneAd.Price;
            OneAdDTO.Timestamp = OneAd.Timestamp;
            OneAdDTO.Location = OneAd.Location;
            OneAdDTO.ImagePath = OneAd.ImagePath;

            return Ok(OneAdDTO);

        }


        /// <summary>
        /// The DeleteAd method deletes an ad listing from the database.
        /// </summary>
        /// <param name="adId">ID number of the listing to be deleted.</param>
        /// <param name="userId">User ID of the requesting user.</param>
        /// <param name="userRole">User role of the requesting user.</param>
        /// <returns>Status code 200 if delete request is fulfilled, otherwise a 404</returns>
        // GET: api/PazarApi/DeleteAd/{adId}/{userId}/{userRole}
        [HttpGet]
        [ResponseType(typeof(void))]
        [Route("api/pazarapi/deletead/{adId}/{userId}/{userRole}")]
        public IHttpActionResult DeleteAd(int adId, string userId, string userRole)
        {
            AdModel OneAd;

            OneAd = db.Ads.Find(adId);

            // Check that the ad listing exists in the database
            if (OneAd == null)
            {
                return NotFound();
            }

            // Admins can delete any ad listing; a regular user can only delete their own listings.
            if (OneAd.ApplicationUserId != userId && userRole != "Admin")
            {
                return BadRequest();
            }

            string imageFileName = Path.GetFileName(OneAd.ImagePath);
            string imagePath = HttpContext.Current.Server.MapPath($"~/ListingImages/{imageFileName}");

            // Delete the listing's image, if it exists.
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }

            db.Ads.Remove(OneAd);
            db.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// The UpdateAd method accepts an AdModel object with the ad listing data to be updated. The method then updates the entry for that ad listing in the database based on the received data.
        /// </summary>
        /// <param name="adId">ID of the ad listing to be updated.</param>
        /// <param name="userId">ID of the requesting user.</param>
        /// <param name="userRole">User role of the requesting user.</param>
        /// <param name="updatedAd">AdModel object containing the ad listing data to be updated</param>
        /// <returns>Status code 200 if update request is fulfilled, otherwise a 404 or 500 error</returns>
        // POST: api/pazarapi/updatead/{adId}/{userId}/{userRole}
        [HttpPost]
        [Route("api/pazarapi/updatead/{adId}/{userId}/{userRole}")]
        public IHttpActionResult UpdateAd(int adId, string userId, string userRole, [FromBody] AdModel updatedAd)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            AdModel existingAd = db.Ads.Find(adId);

            if (!AdExists(adId))
            {
                return NotFound();
            }

            //Extra check to ensure that the correct ad listing is being updated.
            if (existingAd.AdId != adId || existingAd.AdId != updatedAd.AdId)
            {
                return BadRequest();
            }

            //Admins can update any ad listing; regular users can only update their own listings.
            if (existingAd.ApplicationUserId != userId && userRole != "Admin")
            {
                return BadRequest();
            }

            existingAd.CategoryId = updatedAd.CategoryId;
            existingAd.TypeId = updatedAd.TypeId;
            existingAd.Title = updatedAd.Title;
            existingAd.Description = updatedAd.Description;
            existingAd.Price = updatedAd.Price;
            existingAd.Timestamp = updatedAd.Timestamp;
            existingAd.Location = updatedAd.Location;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(HttpStatusCode.InternalServerError);
            }

            return StatusCode(HttpStatusCode.NoContent);

        }



        /// <summary>
        /// The InsertImageToAd method accepts a binary image file and saves it to the disk. It then stores the file path of the image to the database with the listing's other data.
        /// </summary>
        /// <param name="adId">Listing's Id</param>
        /// <param name="userId">Requesting User's Id</param>
        /// <param name="userRole">Role of the requesting user</param>
        /// <returns>200 status code if successful, and a 404 or 500 error otherwise</returns>
        // POST: api/pazarapi/updatead/{adId}/{userId}/{userRole}
        [HttpPost]
        [Route("api/pazarapi/insertimagetoad/{adId}/{userId}/{userRole}")]
        public IHttpActionResult InsertImageToAd(int adId, string userId, string userRole)
        {
            // Retrieve the existing ad listing record from the DB
            AdModel existingAd = db.Ads.Find(adId);

            // Check that the ad listing exists
            if (!AdExists(adId))
            {
                return NotFound();
            }

            //Admins can update any ad listing; regular users can only update their own listings.
            if (existingAd.ApplicationUserId != userId && userRole != "Admin")
            {
                return BadRequest();
            }

            if (Request.Content.IsMimeMultipartContent())
            {
                Debug.WriteLine("Received multipart form data.");

                int numfiles = HttpContext.Current.Request.Files.Count;
                Debug.WriteLine("Files Received: " + numfiles);

                //Check if a file is posted
                if (numfiles == 1 && HttpContext.Current.Request.Files[0] != null)
                {
                    var newImage = HttpContext.Current.Request.Files[0];
                    //Check if the file is empty
                    if (newImage.ContentLength > 0)
                    {
                        var valtypes = new[] { "jpeg", "jpg", "png", "gif" };
                        var extension = Path.GetExtension(newImage.FileName).Substring(1);
                        //Check the extension of the file
                        if (valtypes.Contains(extension))
                        {
                            try
                            {
                                //Set image's filename to the ad listing's Id
                                string newImageFileName = $"{adId}.{extension}";

                                //Get a direct file path to ~/Content/Players/{id}.{extension}
                                string fullImagePath = Path.Combine(HttpContext.Current.Server.MapPath("~/ListingImages/"), newImageFileName);

                                //Save the image file to disk
                                newImage.SaveAs(fullImagePath);

                                //Store the image file path in the database
                                existingAd.ImagePath = fullImagePath;

                                db.SaveChanges();

                            }
                            catch
                            {
                                return StatusCode(HttpStatusCode.InternalServerError);
                            }
                        }
                    }

                }
            }

            return Ok();
        }





        /// <summary>
        /// The UpdateCategory method updates a listing category currently in the database.
        /// </summary>
        /// <param name="editedCategory">CategoryModel object that contains the category data to be updated in the database.</param>
        /// <returns>204 status code if successful, othereise a 404 or 500 error</returns>
        // POST: api/pazarapi/updatecategory
        [HttpPost]
        public IHttpActionResult UpdateCategory([FromBody] CategoryModel editedCategory)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            CategoryModel existingCategory  = db.Categories.Find(editedCategory.CategoryId);

            if (!CategoryExists(editedCategory.CategoryId))
            {
                return NotFound();
            }

            existingCategory.Category = editedCategory.Category;


            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(HttpStatusCode.InternalServerError);
            }

            return StatusCode(HttpStatusCode.NoContent);

        }

        /// <summary>
        /// The AddCategory method adds a new listing category to the database.
        /// </summary>
        /// <param name="newCategory">CategoryModel object that contains the new category data to be created in the database.</param>
        /// <returns>204 status code if successful, otherwise a 400 error</returns>
        // POST: api/PazarApi/AddCategory
        [HttpPost]
        public IHttpActionResult AddCategory([FromBody] CategoryModel newCategory)
        {

            //Will Validate according to data annotations specified on model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Categories.Add(newCategory);
            db.SaveChanges();

            return Ok();
        }


        /// <summary>
        /// The UpdateType method updates a listing type currently in the database.
        /// </summary>
        /// <param name="editedType">CategoryModel object that contains the type data to be updated in the database.</param>
        /// <returns>204 status code if successful, othereise a 400, 404 or 500 error</returns>
        // POST: api/pazarapi/updatetype
        [HttpPost]
        public IHttpActionResult UpdateType([FromBody] TypeModel editedType)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TypeModel existingType = db.Types.Find(editedType.TypeId);

            if (!TypeExists(editedType.TypeId))
            {
                return NotFound();
            }

            existingType.Type = editedType.Type;


            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(HttpStatusCode.InternalServerError);
            }

            return StatusCode(HttpStatusCode.NoContent);

        }

        /// <summary>
        /// The AddType method adds a new listing category to the database.
        /// </summary>
        /// <param name="newType">TypeModel object that contains the new type data to be added in the database.</param>
        /// <returns>200 status code if successful, othereise a 400 error</returns>
        // POST: api/PazarApi/AddType
        [HttpPost]
        public IHttpActionResult AddType([FromBody] TypeModel newType)
        {

            //Will Validate according to data annotations specified on model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Types.Add(newType);
            db.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// The DeleteType method deletes a listing type from the database.
        /// </summary>
        /// <param name="id">ID of the type to be deleted</param>
        /// <returns>200 status code if successful, othereise a 404 error</returns>
        // GET: api/PazarApi/DeleteType/{id}
        [HttpGet]
        [ResponseType(typeof(void))]
        [Route("api/pazarapi/deletetype/{id}")]
        public IHttpActionResult DeleteType(int id)
        {
            TypeModel deleteType;

            deleteType = db.Types.Find(id);

            // Check that the type exists in the database
            if (deleteType == null)
            {
                return NotFound();
            }

            db.Types.Remove(deleteType);
            db.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// The DeleteCategory method deletes a listing category from the database.
        /// </summary>
        /// <param name="id">ID of the category to be deleted</param>
        /// <returns>200 status code if successful, othereise a 404 error</returns>
        // GET: api/PazarApi/DeleteCategory/{id}
        [HttpGet]
        [ResponseType(typeof(void))]
        [Route("api/pazarapi/deletecategory/{id}")]
        public IHttpActionResult DeleteCategory(int id)
        {
            CategoryModel deleteCategory;

            deleteCategory = db.Categories.Find(id);

            // Check that the type exists in the database
            if (deleteCategory == null)
            {
                return NotFound();
            }

            db.Categories.Remove(deleteCategory);
            db.SaveChanges();

            return Ok();
        }




        //// POST: api/PazarApi
        //[ResponseType(typeof(AdModel))]
        //public IHttpActionResult PostAdModel(AdModel adModel)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    db.Ads.Add(adModel);
        //    db.SaveChanges();

        //    return CreatedAtRoute("DefaultApi", new { id = adModel.AdId }, adModel);
        //}



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        //Methods to check that an ad listing, catgory or type already existings in the database.

        private bool AdExists(int id)
        {
            return db.Ads.Count(e => e.AdId == id) > 0;
        }

        private bool CategoryExists(int id)
        {
            return db.Categories.Count(e => e.CategoryId == id) > 0;
        }

        private bool TypeExists(int id)
        {
            return db.Types.Count(e => e.TypeId == id) > 0;
        }


    }
}