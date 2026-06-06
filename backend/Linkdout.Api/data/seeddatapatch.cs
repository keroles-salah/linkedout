using Linkdout.Api.Data;

namespace Linkdout.Api.Data;

public static class SeedDataPatch
{
    public static async Task AddGamification(AppDbContext db)
    {
        if (!db.Users.Any()) return;

        var xpData = new Dictionary<string, (int xp, List<string> badges)>
        {
            ["كيرلس صلاح فخري"] = (350, new() { "🖊️ أول منشور", "✍️ كاتب محتوى", "❤️ محبوب", "💬 متفاعل", "🔗 شبكة قوية", "⚡ نشيط" }),
            ["سارة أحمد"] = (220, new() { "🖊️ أول منشور", "❤️ محبوب", "💬 متفاعل", "⚡ نشيط" }),
            ["محمد علي"] = (180, new() { "🖊️ أول منشور", "❤️ محبوب", "⚡ نشيط" }),
            ["نورا حسن"] = (120, new() { "🖊️ أول منشور", "💬 متفاعل" }),
            ["أحمد محمود"] = (90, new() { "🖊️ أول منشور" }),
            ["ليلى سامي"] = (450, new() { "🖊️ أول منشور", "✍️ كاتب محتوى", "❤️ محبوب", "💬 متفاعل", "🔗 شبكة قوية", "⚡ نشيط", "🔥 ملتهب", "👁️ ملف مميز" }),
            ["عمر خالد"] = (45, new() { "🖊️ أول منشور" }),
            ["فاطمة نبيل"] = (300, new() { "🖊️ أول منشور", "✍️ كاتب محتوى", "❤️ محبوب", "💬 متفاعل", "🔗 شبكة قوية", "⚡ نشيط" }),
        };

        foreach (var (name, (xp, badges)) in xpData)
        {
            var user = db.Users.FirstOrDefault(u => u.FullName == name);
            if (user != null)
            {
                user.XP = xp;
                user.Badges = System.Text.Json.JsonSerializer.Serialize(badges);
            }
        }
        await db.SaveChangesAsync();
    }
}
