using CourseLibrary.API.DbContexts;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(setupAction =>
{
    setupAction.ReturnHttpNotAcceptable = true;
    //setupAction.OutputFormatters.Add(
    //    new XmlDataContractSerializerOutputFormatter());
})
.AddNewtonsoftJson(setupAction=>
{
    setupAction.SerializerSettings.ContractResolver =
        new CamelCasePropertyNamesContractResolver();

})
.AddXmlDataContractSerializerFormatters()
.ConfigureApiBehaviorOptions(setupAction =>
{
    setupAction.InvalidModelStateResponseFactory = context =>
    {
        // create a problem details object
        var problemDetailsFactory = context.HttpContext.RequestServices
            .GetRequiredService<ProblemDetailsFactory>();
        var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(
                            context.HttpContext,
                            context.ModelState);
        // add additional info not added by default
        problemDetails.Detail = "See the errors field for details.";
        problemDetails.Instance = context.HttpContext.Request.Path;
        // find out which status code to use
        var actionExecutingContext =
              context as Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext;

        // if there are modelstate errors & all keys were correctly
        // found/parsed we're dealing with validation errors
        //
        // if the context couldn't be cast to an ActionExecutingContext
        // because it's a ControllerContext, we're dealing with an issue 
        // that happened after the initial input was correctly parsed.  
        // This happens, for example, when manually validating an object inside
        // of a controller action.  That means that by then all keys
        // WERE correctly found and parsed.  In that case, we're
        // thus also dealing with a validation error.
        if (context.ModelState.ErrorCount > 0 &&
            (context is ControllerContext ||
             actionExecutingContext?.ActionArguments.Count == context.ActionDescriptor.Parameters.Count))
        {
            problemDetails.Type = "https://courselibrary.com/modelvalidationproblem";
            problemDetails.Status = StatusCodes.Status422UnprocessableEntity;
            problemDetails.Title = "One or more validation errors occurred.";

            return new UnprocessableEntityObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" }
            };
        }
        // if one of the keys wasn't correctly found / couldn't be parsed
        // we're dealing with null/unparsable input
        problemDetails.Status = StatusCodes.Status400BadRequest;
        problemDetails.Title = "One or more errors on input occurred.";
        return new BadRequestObjectResult(problemDetails)
        {
            ContentTypes = { "application/problem+json" }
        };

    };
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<ICourseLibraryRepository, CourseLibraryRepository>();

builder.Services.AddDbContext<CourseLibraryContext>(options =>
{
    options.UseSqlServer(
        @"Server=.;Database=CourseLibraryDB;Trusted_Connection=True;");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler(appBuilder =>
    {
        appBuilder.Run(async context =>
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
        });
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
