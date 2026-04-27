using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ToDoDbContext>();
//הגדרת מדיניות ה-CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// עוזר למערכת לסרוק ולמצוא את כל ה-Routes שלנו
builder.Services.AddEndpointsApiExplorer();
// מייצר את התיעוד עצמו
builder.Services.AddSwaggerGen();

var app = builder.Build();
//הפעלת ה-CORS באפליקציה
app.UseCors("AllowAll");

// הפעלת ממשק המשתמש
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapGet("/", () => "Hello World!");


//Routes Mapping
//שליפת כל המשימות- GET
app.MapGet("/items", async (ToDoDbContext db) =>
{
    var items = await db.Items.ToListAsync();
    return Results.Ok(items);
});

//הוספת משימה חדשה- POST
app.MapPost("/items", async (Item newItem, ToDoDbContext db) =>
{
    db.Items.Add(newItem);
    await db.SaveChangesAsync();

    // מחזיר סטטוס 201 (Created) עם המשימה שנוצרה
    return Results.Created($"/items/{newItem.Id}", newItem);
});

//עדכון משימה- PUT
app.MapPut("/items/{id}", async (int id, Item updateItem, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);



    if (item is null)
    {
        return Results.NotFound(); // מחזיר 404 אם המשימה לא קיימת
    }

    // עדכון השדות
    // בדיקה האם השם שקיבלנו אינו ריק
    if (!string.IsNullOrEmpty(updateItem.Name))
    {
        // רק אם יש שם חדש, נעדכן אותו
        item.Name = updateItem.Name;
    }

    // את סטטוס ההשלמה נעדכן בכל מקרה
    item.IsComplete = updateItem.IsComplete;

    await db.SaveChangesAsync();
    return Results.NoContent(); // מחזיר סטטוס 204 (הצלחה ללא תוכן חוזר)
});

//מחיקת משימה- DELETE
app.MapDelete("/items/{id}", async (int id, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);

    if (item is null)
    {
        return Results.NotFound();
    }

    db.Items.Remove(item);
    await db.SaveChangesAsync();

    return Results.Ok(item); // מחזיר את המשימה שנמחקה
});

app.Run();
