using BeFit.Data;
using BeFit.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Data Source=befit.db";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    var path = context.Request.Path.ToString();
    var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;

    if ((path == "/exercises.html" || path == "/workouts.html" || path == "/statistics.html") && !isAuthenticated)
    {
        context.Response.Redirect("/login.html");
        return;
    }

    if ((path == "/login.html" || path == "/register.html") && isAuthenticated)
    {
        context.Response.Redirect("/");
        return;
    }

    await next();
});

app.MapGet("/api/exercise-types", async (ApplicationDbContext context) => {
    var exerciseTypes = await context.ExerciseTypes.ToListAsync();
    return Results.Ok(exerciseTypes);
});

app.MapPost("/api/exercise-types", async (ApplicationDbContext context, HttpContext httpContext, UserManager<AppUser> userManager, ExerciseType exerciseType) => {
    var user = await userManager.GetUserAsync(httpContext.User);
    if (user == null) return Results.Unauthorized();

    var isAdmin = await userManager.IsInRoleAsync(user, "Admin");
    if (!isAdmin) return Results.Unauthorized();

    context.ExerciseTypes.Add(exerciseType);
    await context.SaveChangesAsync();
    return Results.Ok(exerciseType);
});

app.MapPut("/api/exercise-types/{id}", async (ApplicationDbContext context, HttpContext httpContext, UserManager<AppUser> userManager, int id, ExerciseType updatedExerciseType) => {
    var user = await userManager.GetUserAsync(httpContext.User);
    if (user == null) return Results.Unauthorized();

    var isAdmin = await userManager.IsInRoleAsync(user, "Admin");
    if (!isAdmin) return Results.Unauthorized();

    var exerciseType = await context.ExerciseTypes.FindAsync(id);
    if (exerciseType == null) return Results.NotFound();

    exerciseType.Name = updatedExerciseType.Name;
    exerciseType.Description = updatedExerciseType.Description;
    exerciseType.MuscleGroup = updatedExerciseType.MuscleGroup;

    await context.SaveChangesAsync();
    return Results.Ok(exerciseType);
});

app.MapDelete("/api/exercise-types/{id}", async (ApplicationDbContext context, HttpContext httpContext, UserManager<AppUser> userManager, int id) => {
    var user = await userManager.GetUserAsync(httpContext.User);
    if (user == null) return Results.Unauthorized();

    var isAdmin = await userManager.IsInRoleAsync(user, "Admin");
    if (!isAdmin) return Results.Unauthorized();

    var exerciseType = await context.ExerciseTypes.FindAsync(id);
    if (exerciseType == null) return Results.NotFound();

    context.ExerciseTypes.Remove(exerciseType);
    await context.SaveChangesAsync();
    return Results.Ok(new { message = "Exercise type deleted" });
});

app.MapGet("/api/workouts", async (ApplicationDbContext context, UserManager<AppUser> userManager, HttpContext httpContext) => {
    var user = await userManager.GetUserAsync(httpContext.User);
    if (user == null) return Results.Unauthorized();

    var workouts = await context.Workouts.Where(w => w.UserId == user.Id).ToListAsync();
    return Results.Ok(workouts);
});

app.MapPost("/api/workouts", async (ApplicationDbContext context, UserManager<AppUser> userManager, HttpContext httpContext, Workout workout) => {
    var user = await userManager.GetUserAsync(httpContext.User);
    if (user == null) return Results.Unauthorized();

    workout.UserId = user.Id;
    context.Workouts.Add(workout);
    await context.SaveChangesAsync();
    return Results.Ok(workout);
});

app.MapPut("/api/workouts/{id}", async (ApplicationDbContext context, UserManager<AppUser> userManager, HttpContext httpContext, int id, Workout updatedWorkout) => {
    var user = await userManager.GetUserAsync(httpContext.User);
    if (user == null) return Results.Unauthorized();

    var workout = await context.Workouts.FirstOrDefaultAsync(w => w.Id == id && w.UserId == user.Id);
    if (workout == null) return Results.NotFound();

    workout.StartDate = updatedWorkout.StartDate;
    workout.EndDate = updatedWorkout.EndDate;
    workout.Notes = updatedWorkout.Notes;

    await context.SaveChangesAsync();
    return Results.Ok(workout);
});

app.MapDelete("/api/workouts/{id}", async (ApplicationDbContext context, UserManager<AppUser> userManager, HttpContext httpContext, int id) => {
    var user = await userManager.GetUserAsync(httpContext.User);
    if (user == null) return Results.Unauthorized();

    var workout = await context.Workouts.FirstOrDefaultAsync(w => w.Id == id && w.UserId == user.Id);
    if (workout == null) return Results.NotFound();

    context.Workouts.Remove(workout);
    await context.SaveChangesAsync();
    return Results.Ok(new { message = "Workout deleted" });
});

app.MapGet("/api/performed-exercises", async (ApplicationDbContext context, UserManager<AppUser> userManager, HttpContext httpContext) => {
    var user = await userManager.GetUserAsync(httpContext.User);
    if (user == null) return Results.Unauthorized();

    var performedExercises = await context.PerformedExercises
        .Where(pe => context.Workouts.Any(w => w.Id == pe.WorkoutId && w.UserId == user.Id))
        .ToListAsync();
    return Results.Ok(performedExercises);
});

app.MapPost("/api/performed-exercises", async (ApplicationDbContext context, UserManager<AppUser> userManager, HttpContext httpContext, PerformedExercise performedExercise) => {
    var user = await userManager.GetUserAsync(httpContext.User);
    if (user == null) return Results.Unauthorized();

    var workout = await context.Workouts.FirstOrDefaultAsync(w => w.Id == performedExercise.WorkoutId && w.UserId == user.Id);
    if (workout == null) return Results.BadRequest(new { error = "Workout not found" });

    context.PerformedExercises.Add(performedExercise);
    await context.SaveChangesAsync();
    return Results.Ok(performedExercise);
});

app.MapDelete("/api/performed-exercises/{id}", async (ApplicationDbContext context, UserManager<AppUser> userManager, HttpContext httpContext, int id) => {
    var user = await userManager.GetUserAsync(httpContext.User);
    if (user == null) return Results.Unauthorized();

    var performedExercise = await context.PerformedExercises
        .FirstOrDefaultAsync(pe => pe.Id == id && context.Workouts.Any(w => w.Id == pe.WorkoutId && w.UserId == user.Id));

    if (performedExercise == null) return Results.NotFound();

    context.PerformedExercises.Remove(performedExercise);
    await context.SaveChangesAsync();
    return Results.Ok(new { message = "Exercise deleted from workout" });
});

app.MapGet("/api/statistics", async (ApplicationDbContext context, UserManager<AppUser> userManager, HttpContext httpContext) => {
    var user = await userManager.GetUserAsync(httpContext.User);
    if (user == null) return Results.Unauthorized();

    var fourWeeksAgo = DateTime.Now.AddDays(-28);

    var userWorkouts = await context.Workouts
        .Where(w => w.UserId == user.Id && w.StartDate >= fourWeeksAgo)
        .Select(w => w.Id)
        .ToListAsync();

    var performedExercises = await context.PerformedExercises
        .Where(pe => userWorkouts.Contains(pe.WorkoutId))
        .ToListAsync();

    var exerciseTypes = await context.ExerciseTypes.ToListAsync();

    var stats = performedExercises
        .GroupBy(pe => pe.ExerciseTypeId)
        .Select(g => {
            var exerciseType = exerciseTypes.FirstOrDefault(et => et.Id == g.Key);
            return new
            {
                ExerciseType = exerciseType?.Name ?? "Unknown",
                TimesPerformed = g.Count(),
                TotalRepetitions = g.Sum(pe => pe.Sets * pe.Reps),
                AverageWeight = g.Average(pe => pe.Weight),
                MaxWeight = g.Max(pe => pe.Weight)
            };
        })
        .ToList();

    return Results.Ok(stats);
});

app.MapGet("/logout", async (SignInManager<AppUser> signInManager, HttpContext context) => {
    await signInManager.SignOutAsync();
    context.Response.Redirect("/login.html");
    return;
});

app.MapPost("/api/auth/login", async (HttpContext context, SignInManager<AppUser> signInManager) => {
    var form = await context.Request.ReadFormAsync();
    var email = form["email"].ToString();
    var password = form["password"].ToString();

    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        return Results.BadRequest(new { success = false, error = "Email and password are required" });

    var result = await signInManager.PasswordSignInAsync(email, password, false, false);

    if (result.Succeeded)
        return Results.Ok(new { success = true });

    return Results.BadRequest(new { success = false, error = "Invalid login attempt" });
});

app.MapPost("/api/auth/register", async (HttpContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager) => {
    var form = await context.Request.ReadFormAsync();
    var email = form["email"].ToString();
    var firstName = form["firstName"].ToString();
    var lastName = form["lastName"].ToString();
    var password = form["password"].ToString();

    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        return Results.BadRequest(new { success = false, error = "Email and password are required" });

    var user = new AppUser
    {
        UserName = email,
        Email = email,
        FirstName = firstName ?? "",
        LastName = lastName ?? ""
    };

    var result = await userManager.CreateAsync(user, password);

    if (result.Succeeded)
    {
        await signInManager.SignInAsync(user, false);
        return Results.Ok(new { success = true });
    }

    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
    return Results.BadRequest(new { success = false, error = errors });
});

app.MapGet("/api/auth/current", async (UserManager<AppUser> userManager, HttpContext context) => {
    var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;

    if (isAuthenticated)
    {
        var user = await userManager.GetUserAsync(context.User);
        if (user != null)
        {
            return Results.Ok(new
            {
                isAuthenticated = true,
                user = new
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                }
            });
        }
    }
    return Results.Ok(new { isAuthenticated = false });
});

app.MapGet("/api/auth/is-admin", async (UserManager<AppUser> userManager, HttpContext httpContext) => {
    var user = await userManager.GetUserAsync(httpContext.User);
    if (user == null) return Results.Ok(new { isAdmin = false });

    var isAdmin = await userManager.IsInRoleAsync(user, "Admin");
    return Results.Ok(new { isAdmin = isAdmin });
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        await context.Database.EnsureCreatedAsync();

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
            Console.WriteLine("✅ Admin role created");
        }

        var adminEmail = builder.Configuration["AdminCredentials:Email"]
                        ?? Environment.GetEnvironmentVariable("BEFIT_ADMIN_EMAIL")
                        ?? "admin@befit.com";

        var adminPassword = builder.Configuration["AdminCredentials:Password"]
                           ?? Environment.GetEnvironmentVariable("BEFIT_ADMIN_PASSWORD")
                           ?? "Admin123!";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "BeFit"
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine($"✅ Admin user created: {adminEmail}");
            }
            else
            {
                Console.WriteLine($"❌ Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        if (!context.ExerciseTypes.Any())
        {
            context.ExerciseTypes.AddRange(
                new ExerciseType { Name = "Wyciskanie sztangi leżąc", MuscleGroup = "Klatka piersiowa", Description = "Podstawowe ćwiczenie na klatkę piersiową" },
                new ExerciseType { Name = "Przysiady ze sztangą", MuscleGroup = "Nogi", Description = "Ćwiczenie na mięśnie nóg i pośladków" },
                new ExerciseType { Name = "Martwy ciąg", MuscleGroup = "Plecy", Description = "Ćwiczenie na mięśnie pleców i nóg" },
                new ExerciseType { Name = "Podciąganie na drążku", MuscleGroup = "Plecy", Description = "Ćwiczenie na mięśnie grzbietu" },
                new ExerciseType { Name = "Wyciskanie żołnierskie", MuscleGroup = "Barki", Description = "Ćwiczenie na mięśnie naramienne" },
                new ExerciseType { Name = "Uginanie ramion ze sztangą", MuscleGroup = "Biceps", Description = "Ćwiczenie na mięśnie dwugłowe ramion" },
                new ExerciseType { Name = "Pompki", MuscleGroup = "Klatka piersiowa", Description = "Podstawowe ćwiczenie z wykorzystaniem masy ciała" }
            );
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Sample exercise types created");
        }

        Console.WriteLine("✅ Database ready!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database initialization error: {ex.Message}");
    }
}

app.Run();