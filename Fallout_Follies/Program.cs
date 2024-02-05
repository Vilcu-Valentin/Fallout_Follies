using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
});

var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettingsSection);
var jwtSettings = jwtSettingsSection.Get<JwtSettings>();
var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key), 
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        RoleClaimType = ClaimTypes.Role 
    };
});


builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JWTToken_Auth_API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp",
        builder => builder.WithOrigins("http://localhost:4200") // Angular's default port
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

var app = builder.Build();

app.UseCors("AllowWebApp");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await EnsureRolesAsync(roleManager);

        await SeedData(userManager, roleManager, context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during the seeding process.");
    }
}

app.Run();


static async Task SeedData(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
{
    // Seed Users
    await SeedUsers(userManager);

    // Seed Products
    await SeedProducts(context);
}

async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
{
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
    if (!await roleManager.RoleExistsAsync("User"))
    {
        await roleManager.CreateAsync(new IdentityRole("User"));
    }
}

static async Task SeedUsers(UserManager<ApplicationUser> userManager)
{
    if (!userManager.Users.Any())
    {
        var adminUser = new ApplicationUser
        {
            UserName = "admin@gmail.com",
            Email = "admin@gmail.com",
            FirstName = "Admin",
            LastName = "Admin"
        };

        await userManager.CreateAsync(adminUser, "AdminPassword123!");
        await userManager.AddToRoleAsync(adminUser, "Admin");

        var testUser1 = new ApplicationUser
        {
            UserName = "testuser1@gmail.com",
            Email = "testuser1@gmail.com",
            FirstName = "Test1",
            LastName = "User"
        };

        await userManager.CreateAsync(testUser1, "TestUserPassword123!");
        await userManager.AddToRoleAsync(testUser1, "User");

        var testUser2 = new ApplicationUser
        {
            UserName = "testuser2@gmail.com",
            Email = "testuser2@gmail.com",
            FirstName = "Test2",
            LastName = "User"
        };

        await userManager.CreateAsync(testUser2, "TestUserPassword123!");
        await userManager.AddToRoleAsync(testUser2, "User");
    }
}

static async Task SeedProducts(ApplicationDbContext context)
{
    if (!context.Products.Any())
    {
        context.Products.AddRange(
            new Product
            {
                Name = "Atomic Annihilator",
                Description = "For those who want to really make an impact...",
                Price = 10000.00M,
                ImageUrl = "/assets/images/atomic_annihilator.png",
                Yield = 50000,
                Specs = "Proprietary fusion ignition system..."
            },
            new Product
            {
                Name = "Fission Fiesta",
                Description = "Looking for a way to spice up your outdoor party? Look no further than our Fission Fiesta. With a rainbow of radioactive colors, this bomb will light up the sky and keep the party going all night long.",
                Price = 2000.00M,
                ImageUrl = "/assets/images/fission_fiesta.png",
                Yield = 100,
                Specs = "Advanced fission technology for maximum boom potential; Ergonomic handle for comfortable carrying..."
            },
            new Product
            {
                Name = "Fusion Frenzy",
                Description = "It's like a party in a bomb, with all the bells and whistles you'd expect from a good nuclear explosion. Get ready to feel the heat!",
                Price = 1000.00M,
                ImageUrl = "/assets/images/fusion_frenzy.png",
                Yield = 50,
                Specs = "Advanced fusion technology for maximum boom potential; Explosively stylish design..."
            },
            new Product
            {
                Name = "Nuke-A-Loo",
                Description = "This bomb is like a dance party, except it will level entire cities in its wake. So put on your dancing shoes and run for cover!",
                Price = 4500.00M,
                ImageUrl = "/assets/images/nuke_a_loo.png",
                Yield = 1200,
                Specs = "Carbon-fiber reinforced casing for durability; Customizable yield settings..."
            },
            new Product
            {
                Name = "Plutonium Pizzazz",
                Description = "This bomb has style and flair, with a little bit of extra radioactive punch. It's perfect for when you want to make a statement and leave an impact.",
                Price = 400.00M,
                ImageUrl = "/assets/images/plutonium_pizzazz.png",
                Yield = 25,
                Specs = "Contains 100% explosive energy; Guaranteed to leave a lasting impression..."
            },
            new Product
            {
                Name = "Radioactive Razzle Dazzle",
                Description = "This bomb creates a dazzling light show that will leave you in awe. But beware, the after-effects may be less than desirable.",
                Price = 3500.00M,
                ImageUrl = "/assets/images/radioactive_razzle.png",
                Yield = 500,
                Specs = "Bomb-licious design; Guaranteed to leave your enemies glowing with envy..."
            },
            new Product
            {
                Name = "Isotope Ice Cream",
                Description = "Chill out with this refreshing treat. It's the perfect way to beat the heat on a hot summer day. Warning: do not eat.",
                Price = 125.00M,
                ImageUrl = "/assets/images/isotope_ice_cream.png",
                Yield = 10,
                Specs = "It may look small, but packs a serious punch; Warning: May cause sudden urge to yell 'Boom!'..."
            },
            new Product
            {
                Name = "Gamma Gallop",
                Description = "Unleash this bad boy on your enemies and watch as they run for the hills. This bomb packs a serious punch and is sure to leave a lasting impression.",
                Price = 750.00M,
                ImageUrl = "/assets/images/gamma_gallop.png",
                Yield = 40,
                Specs = "For those times when diplomacy just isn't enough; Because sometimes you just need a nuclear explosion..."
            },
            new Product
            {
                Name = "Meltdown Muffin",
                Description = "Guaranteed to give you a warm, toasty feeling inside. It's the perfect pick-me-up on a cold, dark day. Its explosive power is enough to warm up even the coldest of hearts, and leave a toasty feeling that lasts for miles around.",
                Price = 8000.00M,
                ImageUrl = "/assets/images/meltdown_muffin.png",
                Yield = 5000,
                Specs = "Just like grandma used to make, except with a lot more radiation; Unleash the power of a thousand suns..."
            },
            new Product
            {
                Name = "Uranium Unicorn",
                Description = "This magical bomb will transport you to a world of pure, radioactive delight. Don't let its cute exterior fool you – it packs a powerful punch!",
                Price = 1250.00M,
                ImageUrl = "/assets/images/uranium_unicorn.png",
                Yield = 85,
                Specs = "For those days when a simple bomb just won't cut it; Guaranteed to leave an impact that will last for centuries..."
            }
        );

        await context.SaveChangesAsync();
    }
}