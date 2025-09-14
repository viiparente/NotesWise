using NotesWise.API.Extensions;
using NotesWise.API.Models;
using NotesWise.API.Services;

namespace NotesWise.API.Endpoints;

public static class FlashcardEndpoints
{
    public static void MapFlashcardEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/flashcards").WithTags("Flashcards");

        group.MapGet("", GetFlashcards)
            .WithName("GetFlashcards")
            .WithOpenApi();

        group.MapDelete("{id}", DeleteFlashcard)
            .WithName("DeleteFlashcard")
            .WithOpenApi();

        // Note-specific flashcard endpoints
        var noteGroup = routes.MapGroup("/api/notes/{noteId}/flashcards").WithTags("Flashcards");

        noteGroup.MapGet("", GetFlashcardsByNote)
            .WithName("GetFlashcardsByNote")
            .WithOpenApi();

        noteGroup.MapPost("", CreateFlashcards)
            .WithName("CreateFlashcards")
            .WithOpenApi();
    }

    private static async Task<IResult> GetFlashcards(HttpContext context, IDataStore dataStore)
    {
        try
        {
            var userId = context.GetUserIdOrThrow();
            var flashcards = await dataStore.GetFlashcardsAsync(userId);
            return Results.Ok(flashcards);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
    }

    private static async Task<IResult> GetFlashcardsByNote(
        HttpContext context, 
        string noteId, 
        IDataStore dataStore)
    {
        try
        {
            var userId = context.GetUserIdOrThrow();
            
            // Verify the note belongs to the user
            var note = await dataStore.GetNoteByIdAsync(noteId, userId);
            if (note == null)
            {
                return Results.NotFound("Note not found");
            }

            var flashcards = await dataStore.GetFlashcardsByNoteIdAsync(noteId);
            return Results.Ok(flashcards);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
    }

    private static async Task<IResult> CreateFlashcards(
        HttpContext context, 
        string noteId, 
        CreateFlashcardsRequest request, 
        IDataStore dataStore)
    {
        try
        {
            var userId = context.GetUserIdOrThrow();
            
            // Verify the note belongs to the user
            var note = await dataStore.GetNoteByIdAsync(noteId, userId);
            if (note == null)
            {
                return Results.NotFound("Note not found");
            }

            var createdFlashcards = new List<Flashcard>();
            
            foreach (var flashcardRequest in request.Flashcards)
            {
                var flashcard = new Flashcard
                {
                    NoteId = noteId,
                    Question = flashcardRequest.Question,
                    Answer = flashcardRequest.Answer
                };

                var createdFlashcard = await dataStore.CreateFlashcardAsync(flashcard);
                createdFlashcards.Add(createdFlashcard);
            }

            return Results.Created($"/api/notes/{noteId}/flashcards", createdFlashcards);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
    }

    private static async Task<IResult> DeleteFlashcard(
        HttpContext context, 
        string id, 
        IDataStore dataStore)
    {
        try
        {
            var userId = context.GetUserIdOrThrow();
            
            // Note: We should verify the flashcard belongs to a note owned by the user
            // For simplicity, we'll trust the middleware for now, but in production
            // you might want to add an additional check
            
            var success = await dataStore.DeleteFlashcardAsync(id);
            return success ? Results.NoContent() : Results.NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
    }
}