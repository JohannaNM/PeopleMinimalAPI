
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.OpenApi.Models;
using PeopleMinimalAPI.Data;
using PeopleMinimalAPI.Models;

namespace PeopleMinimalAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

           

            /////////////////////////////////////////////////////////////////////////
            ////////////                    People                    //////////////
            ///////////////////////////////////////////////////////////////////////

            // Return all people
            app.MapGet("/people", async (ApplicationDbContext context) =>
            {
                var people = await context.People
                .Include(person => person.Interests.Where(interest => interest.FkPersonId == 1))
                .ToListAsync();
                
                if (people == null || !people.Any())
                {
                    return Results.NotFound("Couldn't find any people");
                }
                return Results.Ok(people);
            });

            //Create a person
            app.MapPost("/people", async (Person person, ApplicationDbContext context) =>
            {
                context.People.Add(person);
                await context.SaveChangesAsync();
                return Results.Created($"/people/{person.PersonId}", person);
            });

            // Get an person by Id
            app.MapGet("/people/{id:int}", async (int id, ApplicationDbContext context) =>
            {
                var person = await context.People
                .FindAsync(id);
                
                
                if (person == null)
                {
                    return Results.NotFound("Person not found");
                }
                return Results.Ok(person);
            });

            //Edit a person
            app.MapPut("/people/{id:int}", async (int id, Person updatedPerson, ApplicationDbContext context) =>
            {
                var person = await context.People.FindAsync(id);
                if (person == null)
                {
                    return Results.NotFound("Person not found");
                }
                person.Name = updatedPerson.Name;
                person.PhoneNumber = updatedPerson.PhoneNumber;
           
                await context.SaveChangesAsync();
                return Results.Ok(person);

            });

            //Delete a person
            app.MapDelete("/people/{id:int}", async (int id, ApplicationDbContext context) =>
            {
                var person = await context.People.FindAsync(id);
                if (person == null)
                {
                    return Results.NotFound("Person not found");
                }
                context.People.Remove(person);
                await context.SaveChangesAsync();
                return Results.Ok($"Person with id: {id} is deleted");
            });


            /////////////////////////////////////////////////////////////////////////
            ////////////                  Interests                   //////////////
            ///////////////////////////////////////////////////////////////////////

            // Get all interests
            app.MapGet("/interests", async (ApplicationDbContext context) =>
            {
                var interests = await context.Interests.Include(p => p.Person).ToListAsync();
                if (interests == null || !interests.Any())
                {
                    return Results.NotFound("No interests in database");
                }
                return Results.Ok(interests);
            });

            //Add a interest
            app.MapPost("/interests", async (Interest interest, ApplicationDbContext context) =>
            {
                context.Interests.Add(interest);
                await context.SaveChangesAsync();
                return Results.Created($"/interests/{interest.InterestId}", interest);
            });

            // Get a interest by id
            app.MapGet("/interests/{id:int}", async (int id, ApplicationDbContext context) =>
            {
                var interest = await context.Interests.FindAsync(id);
                if (interest == null)
                {
                    return Results.NotFound("Interest not found");
                }
                return Results.Ok(interest);
            });

            //Update a interest
            app.MapPut("/interests/{id:int}", async (int id, Interest updatedInterest, ApplicationDbContext context) =>
            {
                var interest = await context.Interests.FindAsync(id);
                if (interest == null)
                {
                    return Results.NotFound("Interest not found");
                }
                interest.Title = updatedInterest.Title;
                interest.Description = updatedInterest.Description;
                interest.FkPersonId = updatedInterest.FkPersonId;
                
                await context.SaveChangesAsync();
                return Results.Ok(interest);

            });

            //Delete interest
            app.MapDelete("/interests/{id:int}", async (int id, ApplicationDbContext context) =>
            {
                var interest = await context.Interests.FindAsync(id);
                if (interest == null)
                {
                    return Results.NotFound("Interest not found");
                }
                context.Interests.Remove(interest);
                await context.SaveChangesAsync();
                return Results.Ok($"Interest with id: {id} is deleted");
            });

            /////////////////////////////////////////////////////////////////////////
            ////////////                    Links                     //////////////
            ///////////////////////////////////////////////////////////////////////

            // Get all links
            app.MapGet("/links", async (ApplicationDbContext context) =>
            {
                var link = await context.Links
                .Include(i => i.Interest)
                .ThenInclude(i => i.Person)
                .ToListAsync();
                if (link == null || !link.Any())
                {
                    return Results.NotFound("No links in database");
                }
                return Results.Ok(link);
            });

            //Add a link
            app.MapPost("/links", async (Link link, ApplicationDbContext context) =>
            {
                context.Links.Add(link);
                await context.SaveChangesAsync();
                return Results.Created($"/links/{link.LinkId}", link);
            });

            // Get a link by id
            app.MapGet("/links/{id:int}", async (int id, ApplicationDbContext context) =>
            {
                var link = await context.Links.FindAsync(id);
                if (link == null)
                {
                    return Results.NotFound("Link not found");
                }
                return Results.Ok(link);
            });

            //Update a link
            app.MapPut("/links/{id:int}", async (int id, Link updatedLink, ApplicationDbContext context) =>
            {
                var link = await context.Links.FindAsync(id);
                if (link == null)
                {
                    return Results.NotFound("Link not found");
                }
                link.Url = updatedLink.Url;
                link.FkInterestId = updatedLink.FkInterestId;

                await context.SaveChangesAsync();
                return Results.Ok(link);

            });

            //Delete a link
            app.MapDelete("/links/{id:int}", async (int id, ApplicationDbContext context) =>
            {
                var link = await context.Links.FindAsync(id);
                if (link == null)
                {
                    return Results.NotFound("Link not found");
                }
                context.Links.Remove(link);
                await context.SaveChangesAsync();
                return Results.Ok($"Link with id: {id} is deleted");
            });

            /////////////////////////////////////////////////////////////////////////
            ////////////                  Connected                   //////////////
            ///////////////////////////////////////////////////////////////////////


            app.MapGet("/peopleWithInterestsAndLinks",  (ApplicationDbContext context) =>
            {
                var results =  Task.Run(() =>
                {
                    return from person in context.People
                           select new
                           {
                               person.PersonId,
                               person.Name,
                               person.PhoneNumber,
                               Interest = (from interest in context.Interests
                                           where interest.FkPersonId == person.PersonId
                                           select new
                                           {
                                               interest.InterestId,
                                               interest.Title,
                                               interest.Description,
                                               Link = (from link in context.Links
                                                       where link.FkInterestId == interest.InterestId
                                                       select new
                                                       {
                                                           link.LinkId,
                                                           link.Url
                                                       }).ToList()
                                           }).ToList()
                           };
                });

                if (results == null)
                {
                    return Results.NotFound("Couldn't find any people");
                }

                return Results.Ok(results);

            });


            // Get an person by Id
            app.MapGet("/personWithInterests/{id:int}", (int id, ApplicationDbContext context) =>
            {

                var people = Task.Run(() =>
                {
                    return from person in context.People
                           where person.PersonId == id
                           select new
                           {
                               person.PersonId,
                               person.Name,
                               Interest = (from interest in context.Interests
                                           where interest.FkPersonId == person.PersonId
                                           select new
                                           {
                                               interest.InterestId,
                                               interest.Title,
                                               interest.Description
                                           }).ToList()

                           };

                });

                if (people == null)
                {
                    return Results.NotFound("Person not found");
                }
                return Results.Ok(people);
            });

            app.MapGet("/personWithLinks/{id:int}", (int id, ApplicationDbContext context) =>
            {

                var people = Task.Run(() =>
                {
                    return from person in context.People
                           where person.PersonId == id
                           select new
                           {
                               person.Name,
                               Interest = (from interest in context.Interests
                                           where interest.FkPersonId == person.PersonId
                                           select new
                                           {
                                               interest.InterestId,
                                               Link = (from link in context.Links
                                                       where link.FkInterestId == interest.InterestId
                                                       select new
                                                       {
                                                           link.LinkId,
                                                           link.Url
                                                       }).ToList()
                                           }).ToList()
                           };

                });

                if (people == null)
                {
                    return Results.NotFound("Person not found");
                }
                return Results.Ok(people);
            });

         

            app.Run();


        }
    }
}
