﻿using NLog;
using System.Linq;
using NWConsole.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NLog.LayoutRenderers;

// See https://aka.ms/new-console-template for more information
string path = Directory.GetCurrentDirectory() + "\\nlog.config";

// create instance of Logger
var logger = LogManager.Setup().LoadConfigurationFromFile(path).GetCurrentClassLogger();
logger.Info("Program started");

try
{
    var db = new NWContext();
    string choice;
    do
    {
        Console.WriteLine("1) Display Categories");
        Console.WriteLine("2) Add Category");
        Console.WriteLine("3) Edit Category");
        Console.WriteLine("4) Delete Category");
        Console.WriteLine("5) Display Category and related products");
        Console.WriteLine("6) Display all Categories and their related products");
        Console.WriteLine("7) Add Product");
        Console.WriteLine("8) Edit Product");
        Console.WriteLine("9) Delete Product");
        Console.WriteLine("10) Display all records in the Products table");
        Console.WriteLine("11) Display a specific Product");
        Console.WriteLine("12) Display Orders");
        Console.WriteLine("\"q\" to quit");
        choice = Console.ReadLine();
        Console.Clear();
        logger.Info($"Option {choice} selected");

        if (choice == "1") //1) Display Categories
        {
            var query = db.Categories.OrderBy(p => p.CategoryName);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{query.Count()} records returned");
            Console.ForegroundColor = ConsoleColor.White;

            GetCategory(db, logger, false);
        }
        else if (choice == "2") //2) Add Category
        {
            Category category = new Category();
            Console.Write("Enter Category Name:");
            category.CategoryName = Console.ReadLine();
            Console.Write("Enter the Category Description:");
            category.Description = Console.ReadLine();
            ValidationContext context = new ValidationContext(category, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(category, context, results, true);
            if (isValid)
            {
                // check for unique name
                if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                }
                else
                {
                    logger.Info("Validation passed");
                    
                    // save category to db
                    db.AddCategorie(category);
                    logger.Info($"Category add: {category.CategoryName}");
                }
            }
            else
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
            }
        }
        else if (choice == "3") //3) Edit Category
        {
            Console.WriteLine("Choose the Category to edit: ");
            Category category = GetCategory(db, logger, true);

            if(category != null)
            {
                // edit
                Console.Write("Enter Category Name:");
                category.CategoryName = Console.ReadLine();
                Console.Write("Enter the Category Description:");
                category.Description = Console.ReadLine();

                db.Update(category);
                db.SaveChanges();
                logger.Info($"Category edit: {category.CategoryName}");
            }
            else
                logger.Error("It is a wrong choice.");
        }
        else if (choice == "4") //4) Delete Category
        {
            Console.WriteLine("Choose the Category to delete: ");
            Category category = GetCategory(db, logger, true);

            if(category != null)
            {
                // Delete
                db.DeleteCategorie(category);
                logger.Info($"Category Delete: {category.CategoryName}");
            }
            else
                logger.Error("It is a wrong choice.");

        }
        else if (choice == "5") //5) Display Category and related products
        {
            Console.Clear();
            Console.WriteLine("Select the category whose products you want to display:");
            Category category = GetCategory(db, logger, true);
            Console.Clear();
            Category fullcategory = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == category.CategoryId);
            Console.WriteLine($"{fullcategory.CategoryName} - {fullcategory.Description}");
            GetProduct(fullcategory, logger, false);
        }
        else if (choice == "6") //6) Display all Categories and their related products
        {
            var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName}");
                GetProduct(item, logger, false);
            }
        }
        else if (choice == "7") //7) Add Product
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Choose the Category to add Product: ");
                Category category = GetCategory(db, logger, true);

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Select Category: {category.CategoryName}");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Product product = new Product();
                Console.Write("Enter Product Name:");
                product.ProductName = Console.ReadLine();
                product.CategoryId = category.CategoryId;
                product.Category = category;
                Console.Write("Enter the Product Quantity per Unit:");
                product.QuantityPerUnit = Console.ReadLine();
                Console.Write("Enter the Product Unit Price:");
                product.UnitPrice = Convert.ToDecimal(Console.ReadLine());
                Console.Write("Enter the Product Units in Stock:");
                product.UnitsInStock = Convert.ToInt16(Console.ReadLine());
                Console.ForegroundColor = ConsoleColor.White;

                ValidationContext context = new ValidationContext(category, null, null);
                List<ValidationResult> results = new List<ValidationResult>();

                var isValid = Validator.TryValidateObject(category, context, results, true);
                if (isValid)
                {
                    // check for unique name
                    if (db.Products.Any(p => p.ProductName == product.ProductName))
                    {
                        // generate validation error
                        isValid = false;
                        logger.Error("Already exists a same Product name");
                        results.Add(new ValidationResult("Name exists", new string[] { "ProductName" }));
                    }
                    else
                    {
                        logger.Info("Validation passed");
                        
                        // save category to db
                        db.AddProduct(product, category);
                        logger.Info($"{category.CategoryName} - Product add: {product.ProductName}");
                    }
                }
                else
                {
                    foreach (var result in results)
                    {
                        logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                logger.Error($"Add Product Error: {ex.Message}");
            }
        }
        else if (choice == "8") //8) Edit Product
        {
            try
            {           
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Choose the Category to Edit Product: ");
                Category category = GetCategory(db, logger, true);

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Select Category: {category.CategoryName}");
                Console.ForegroundColor = ConsoleColor.Yellow;

                if(category != null && category.Products.Count > 0)
                {
                    // edit Product
                    Product pp = GetProduct(category, logger, true);

                    Console.Write("Enter Product Name:");
                    pp.ProductName = Console.ReadLine();
                    Console.Write("Enter the Product Quantity per Unit:");
                    pp.QuantityPerUnit = Console.ReadLine();
                    Console.Write("Enter the Product Unit Price:");
                    pp.UnitPrice = Convert.ToDecimal(Console.ReadLine());
                    Console.Write("Enter the Product Units in Stock:");
                    pp.UnitsInStock = Convert.ToInt16(Console.ReadLine());

                    db.Update(category);
                    db.SaveChanges();
                    logger.Info($"Product edit: {pp.ProductName}");
                }
                else
                {
                    if(category.Products.Count == 0)
                        logger.Error("No Products.");
                    else 
                        logger.Error("It is a wrong choice.");
                }
                        
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (System.Exception ex)
            {
                logger.Error($"Edit Product Error: {ex.Message}");
            }
        }
        else if (choice == "9") //9) Delete Product
        {
            try
            {
                var products = db.Products.OrderBy(p => p.ProductId);
                Console.ForegroundColor = ConsoleColor.Blue;
                foreach (var item in products)
                {
                    Console.WriteLine($"{item.ProductId}) {item.ProductName} - Category ID: {item.CategoryId}");
                }
                Console.ForegroundColor = ConsoleColor.White;

                Console.Write($"Choose the Product to delete: ");
                Product pp = products.FirstOrDefault(c => c.ProductId == Convert.ToInt32(Console.ReadLine()));

                db.DeleteProduct(pp);
                logger.Info($"Procuct Delete: {pp.ProductName}");
            }
            catch (System.Exception ex)
            {
                logger.Error($"Delete Product error: {ex.Message}");
            }

            /*
            Console.WriteLine("Choose the Category to Product delete: ");
            Category category = GetCategory(db, logger, true);

            if(category != null)
            {
                Product product = GetProduct(category, logger, true);

                // Delete Procuct
                db.DeleteProduct(product);
                logger.Info($"Procuct Delete: {category.CategoryName} - {product.ProductName}");
            }
            else
                logger.Error("It is a wrong choice.");
            */
        }
        else if (choice == "10") //10) Display all records in the Products table
        {
            try
            {
                var products = db.Products.OrderBy(p => p.ProductName);
                Console.ForegroundColor = ConsoleColor.Blue;
                foreach (var item in products)
                {
                    Console.WriteLine($"{item.ProductName} - {item.CategoryId}");
                }
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (System.Exception ex)
            {
                logger.Error($"Display all records in the Products table error: {ex.Message}");
            }
        }
        else if (choice == "11") //11) Display a specific Product
        {
            try
            {        
                var products = db.Products.OrderBy(p => p.ProductId);
                Console.ForegroundColor = ConsoleColor.Blue;
                foreach (var item in products)
                {
                    Console.WriteLine($"{item.ProductId}) {item.ProductName} - Category ID: {item.CategoryId}");
                }
                Console.ForegroundColor = ConsoleColor.White;

                Console.Write($"Choose the Product to display: ");
                Product pp = products.FirstOrDefault(c => c.ProductId == Convert.ToInt32(Console.ReadLine()));

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{pp.ProductName}: {pp.CategoryId} \n\tQuantity Per Unit: {pp.QuantityPerUnit}\n\tUnit Price: ${pp.UnitPrice}\n\tUnit in Stock: {pp.UnitsInStock}");
                Console.ForegroundColor = ConsoleColor.White;

            }
            catch (System.Exception ex)
            {
                logger.Error($"Display a apecific product error: {ex.Message}");
            }
        }
        else if (choice == "12") //12) Display Orders
        {
            try
            {        
                var orders = db.Orders.OrderBy(o => o.OrderId);
                Console.ForegroundColor = ConsoleColor.Cyan;
                foreach (var item in orders)
                {
                    Console.WriteLine($"{item.OrderId}) {item.ShipName} -  Required Date: {item.RequiredDate} Shipped Date: {item.ShippedDate}");
                }
                Console.ForegroundColor = ConsoleColor.White;

                Console.Write($"Choose the Order: ");
                OrderDetail oo = db.OrderDetails.FirstOrDefault(c => c.OrderId == Convert.ToInt32(Console.ReadLine()));
                Product pp = db.Products.FirstOrDefault(p => p.ProductId == oo.ProductId);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{oo.OrderId}: {pp.ProductName} \n\tQuantity: {oo.Quantity}\n\tDiscount: ${oo.Discount}");
                Console.ForegroundColor = ConsoleColor.White;

            }
            catch (System.Exception ex)
            {
                logger.Error($"Display Orders error: {ex.Message}");
            }
        }
        Console.WriteLine();

    } while (choice.ToLower() != "q");
}
catch (Exception ex)
{
    logger.Error(ex.Message);
}

logger.Info("Program ended");


static Category GetCategory(NWContext db, Logger logger, bool getCategory)
{
    Console.ForegroundColor = ConsoleColor.Magenta;

    if(!getCategory)
    {
        var query = db.Categories.OrderBy(c => c.CategoryName);

        foreach (var item in query)
        {
            Console.WriteLine($"{item.CategoryName} - {item.Description}");
        }
    }
    else
    {
        var query = db.Categories.OrderBy(c => c.CategoryId);

        foreach (var item in query)
            Console.WriteLine($"{item.CategoryId}) {item.CategoryName} - {item.Description}");

        Console.ForegroundColor = ConsoleColor.Green;

        if(int.TryParse(Console.ReadLine(), out int CategoryID))
        {
            Category category = db.Categories.FirstOrDefault(c => c.CategoryId == CategoryID);
            if(category != null)
            {
                Console.ForegroundColor = ConsoleColor.White;
                return category;
            }
        }
        logger.Error("Invalid Category Id");
    }

    Console.ForegroundColor = ConsoleColor.White;
    return null;
}

static Product GetProduct(Category category, Logger logger, bool returnProduct)
{
    try
    {
        Console.ForegroundColor = ConsoleColor.Blue;

        if(!returnProduct)
        {
            foreach (Product p in category.Products)
            {
                Console.WriteLine($"\t{p.ProductName}");
            }
        }
        else
        {
            foreach (Product p in category.Products)
            {
                Console.WriteLine($"{p.ProductId}) {p.ProductName}");
            }

            if(int.TryParse(Console.ReadLine(), out int ProductId))
            {
                Product pp = category.Products.FirstOrDefault(c => c.ProductId == ProductId);
                if(pp != null)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    return pp;
                }
            }
        }
        Console.ForegroundColor = ConsoleColor.White;

    }
    catch (Exception ex)
    {
        logger.Error($"print Product error: {ex.Message}");
    }

    return null;
}

/*
static void OrganizedCategory(NWContext db, Logger logger)
{
    try
    {
        int i = 1;
        foreach (var item in db.Categories)
        {
            item.CategoryId = i;
            i++;
        }
    
    }
    catch (System.Exception ex)
    {
        logger.Error($"Organized Category ID error: {ex.Message}");
        throw;
    }
}*/
