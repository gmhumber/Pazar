# Pazar Marketplace

Pazar Marketplace is a classifieds website created with ASP.NET, C#, Razor, Entity Framework and MS SQL. The goal of the project was to create a website with a database containing multiple tables, and use Entity Framework to interface between the website and the database. CRUD features were implemented for all database tables, and access to some of these CRUD features depended on a user's access privilages (User vs Admin privilages).

This project allows for 2 user roles: "User" and "Admin". The web interface allows creation of regular user accounts. Manually change the role of any user to "Admin" in the database if desired. 

This project includes test data and user accounts. The user accounts are:


Usr: admin@admin.com </br>
Pass: Test123!

Usr: user@user.com </br>
Pass: Test123!

Usr: test@test.com </br>
Pass: Test123!

The features of this project includes: </br>
- User authentication </br>
- protected areas and public areas of the website </br>
- CSRF protection </br>
- Search feature </br>
- CRUD features for regular users </br>
- Additional CRUD features available to admin users </br>
- CRUD features for images (located in the /ListingImages directory of the project) </br>
