// AdminNavPages.cs
using Microsoft.AspNetCore.Mvc.Rendering;

public static class AdminNavPages
{
    public static string Index1 => "Index1";
    public static string Users => "Users";
    public static string Settings => "Settings";
    public static string Questionnaires => "Questionnaires";

    public static string? Index1NavClass(ViewContext viewContext) => PageNavClass(viewContext, Index1);
    public static string? UsersNavClass(ViewContext viewContext) => PageNavClass(viewContext, Users);
    public static string? SettingsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Settings);
    public static string? QuestionnairesNavClass(ViewContext viewContext) => PageNavClass(viewContext, Questionnaires);

    private static string? PageNavClass(ViewContext viewContext, string page)
    {
        var activePage = viewContext.ViewData["ActivePage"] as string
            ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
        return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
    }
}